/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;
using Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Opus
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Opus_Pcm_Soft_Clip(Pointer<c_float> _x, c_int N, c_int C, Pointer<c_float> declip_mem)
		{
			if ((C < 1) || (N < 1) || _x.IsNull || declip_mem.IsNull)
				return;

			// First thing: saturate everything to +/- 2 which is the highest level our
			// non-linearity can handle. At the point where the signal reaches +/-2,
			// the derivative will be zero anyway, so this doesn't introduce any
			// discontinuity in the derivative
			for (c_int i = 0; i < (N * C); i++)
				_x[i] = Arch.MAX16(-2.0f, Arch.MIN16(2.0f, _x[i]));

			for (c_int c = 0; c < C; c++)
			{
				Pointer<c_float> x = _x + c;
				c_float a = declip_mem[c];

				// Continue applying the non-linearity from the previous frame to avoid
				// any discontinuity
				for (c_int i = 0; i < N; i++)
				{
					if ((x[i * C] * a) >= 0)
						break;

					x[i * C] = x[i * C] + a * x[i * C] * x[i * C];
				}

				c_int curr = 0;
				c_float x0 = x[0];

				while (true)
				{
					c_int i;

					for (i = curr; i < N; i++)
					{
						if ((x[i * C] > 1) || ((x[i * C]) < -1))
							break;
					}

					if (i == N)
					{
						a = 0;
						break;
					}

					c_int peakPos = i;
					c_int start = i, end = i;
					c_float maxVal = Arch.ABS16(x[i * C]);

					// Look for first zero crossing before clipping
					while ((start > 0) && ((x[i * C] * x[(start - 1) * C]) >= 0))
						start--;

					// Look for first zero crossing after clipping
					while ((end < N) && ((x[i * C] * x[end * C]) >= 0))
					{
						// Look for other peaks until the next zero-crossing
						if (Arch.ABS16(x[end * C]) > maxVal)
						{
							maxVal = Arch.ABS16(x[end * C]);
							peakPos = end;
						}

						end++;
					}

					// Detect the special case where we clip before the first zero crossing
					bool special = (start == 0) && ((x[i * C] * x[0]) >= 0);

					// Compute a such that maxval + a*maxval^2 = 1
					a = (maxVal - 1) / (maxVal * maxVal);

					// Slightly boost "a" by 2^-22. This is just enough to ensure -ffast-math
					// does not cause output values larger than +/-1, but small enough not
					// to matter even for 24-bit output
					a += a * 2.4e-7f;

					if (x[i * C] > 0)
						a = -a;

					// Apply soft clipping
					for (i = start; i < end; i++)
						x[i * C] = x[i * C] + a * x[i * C] * x[i * C];

					if (special && (peakPos >= 2))
					{
						// Add a linear ramp from the first sample to the signal peak.
						// This avoids a discontinuity at the beginning of the frame
						c_float offset = x0 - x[0];
						c_float delta = offset / peakPos;

						for (i = curr; i < peakPos; i++)
						{
							offset -= delta;
							x[i * C] += offset;
							x[i * C] = Arch.MAX16(-1.0f, Arch.MIN16(1.0f, x[i * C]));
						}
					}

					curr = end;

					if (curr == N)
						break;
				}

				declip_mem[c] = a;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Parse_Size(Pointer<byte> data, opus_int32 len, out opus_int16 size)
		{
			if (len < 1)
			{
				size = -1;
				return -1;
			}
			else if (data[0] < 252)
			{
				size = data[0];
				return 1;
			}
			else if (len < 2)
			{
				size = -1;
				return -1;
			}
			else
			{
				size = (opus_int16)(4 * data[1] + data[0]);
				return 2;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Opus_Packet_Get_Samples_Per_Frame(Pointer<byte> data, opus_int32 Fs)
		{
			c_int audiosize;

			if ((data[0] & 0x80) != 0)
			{
				audiosize = ((data[0] >> 3) & 0x3);
				audiosize = (Fs << audiosize) / 400;
			}
			else if ((data[0] & 0x60) == 0x60)
				audiosize = (data[0] & 0x08) != 0 ? Fs / 50 : Fs / 100;
			else
			{
				audiosize = ((data[0] >> 3) & 0x3);
				if (audiosize == 3)
					audiosize = Fs * 60 / 1000;
				else
					audiosize = (Fs << audiosize) / 100;
			}

			return audiosize;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Opus_Packet_Parse_Impl(Pointer<byte> data, opus_int32 len, bool self_delimited, out byte out_toc, Pointer<Pointer<byte>> frames, Pointer<opus_int16> size, out c_int payload_offset, out opus_int32 packet_offset, out Pointer<byte> padding, out opus_int32 padding_len)
		{
			out_toc = 0;
			payload_offset = 0;
			packet_offset = 0;
			padding = null;
			padding_len = 0;

			opus_int32 pad = 0;
			Pointer<byte> data0 = data;

			if (size.IsNull || (len < 0))
				return (c_int)OpusError.Bad_Arg;

			if (len == 0)
				return (c_int)OpusError.Invalid_Packet;

			c_int framesize = Opus_Packet_Get_Samples_Per_Frame(data, 48000);

			bool cbr = false;
			byte toc = data[0, 1];
			len--;
			opus_int32 last_size = len;

			c_int bytes;
			c_int count;

			switch (toc & 0x3)
			{
				// One frame
				case 0:
				{
					count = 1;
					break;
				}

				// Two CBR frames
				case 1:
				{
					count = 2;
					cbr = true;

					if (!self_delimited)
					{
						if ((len & 0x1) != 0)
							return (c_int)OpusError.Invalid_Packet;

						last_size = len / 2;

						// If last_size doesn't fit in size[0], we'll catch it later
						size[0] = (opus_int16)last_size;
					}
					break;
				}

				// Two VBR frames
				case 2:
				{
					count = 2;
					bytes = Parse_Size(data, len, out opus_int16 _o);
					size[0] = _o;
					len -= bytes;

					if ((size[0] < 0) || (size[0] > len))
						return (c_int)OpusError.Invalid_Packet;

					data += bytes;
					last_size = len - size[0];
					break;
				}

				// Multiple CBR/VBR frames (from 0 to 120 ms)
				default:
				{
					if (len < 1)
						return (c_int)OpusError.Invalid_Packet;

					// Number of frames encoded in bits 0 to 5
					byte ch = data[0, 1];
					count = ch & 0x3f;

					if ((count <= 0) || ((framesize * count) > 5760))
						return (c_int)OpusError.Invalid_Packet;

					len--;

					// Padding flag is bit 6
					if ((ch & 0x40) != 0)
					{
						c_int p;

						do
						{
							if (len <= 0)
								return (c_int)OpusError.Invalid_Packet;

							p = data[0, 1];
							len--;

							c_int tmp = p == 255 ? 254 : p;
							len -= tmp;
							pad += tmp;
						}
						while (p == 255);
					}

					if (len < 0)
						return (c_int)OpusError.Invalid_Packet;

					// VBR flag is bit 7
					cbr = !((ch & 0x80) != 0);
					if (!cbr)
					{
						// VBR case
						last_size = len;

						for (c_int i = 0; i < (count - 1); i++)
						{
							bytes = Parse_Size(data, len, out opus_int16 _o);
							size[i] = _o;
							len -= bytes;

							if ((size[i] < 0) || (size[i] > len))
								return (c_int)OpusError.Invalid_Packet;

							data += bytes;
							last_size -= bytes + size[i];
						}

						if (last_size < 0)
							return (c_int)OpusError.Invalid_Packet;
					}
					else if (!self_delimited)
					{
						// CBR case
						last_size = len / count;

						if ((last_size * count) != len)
							return (c_int)OpusError.Invalid_Packet;

						for (c_int i = 0; i < (count - 1); i++)
							size[i] = (opus_int16)last_size;
					}
					break;
				}
			}

			// Self-delimited framing has an extra size for the last frame
			if (self_delimited)
			{
				bytes = Parse_Size(data, len, out opus_int16 _o);
				size[count - 1] = _o;
				len -= bytes;

				if ((size[count - 1] < 0) || (size[count - 1] > len))
					return (c_int)OpusError.Invalid_Packet;

				data += bytes;

				// For CBR packets, apply the size to all the frames
				if (cbr)
				{
					if ((size[count - 1] * count) > len)
						return (c_int)OpusError.Invalid_Packet;

					for (c_int i = 0; i < (count - 1); i++)
						size[i] = size[count - 1];
				}
				else if ((bytes + size[count - 1])> last_size)
					return (c_int)OpusError.Invalid_Packet;
			}
			else
			{
				// Because it's not encoded explicitly, it's possible the size of the
				// last packet (or all the packets, for the CBR case) is larger than
				// 1275. Reject them here
				if (last_size > 1275)
					return (c_int)OpusError.Invalid_Packet;

				size[count - 1] = (opus_int16)last_size;
			}

			payload_offset = data - data0;

			for (c_int i = 0; i < count; i++)
			{
				if (frames.IsNotNull)
					frames[i] = data;

				data += size[i];
			}

			padding = data;
			padding_len = pad;

			packet_offset = pad + (data - data0);

			out_toc = toc;

			return count;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Opus_Packet_Parse(Pointer<byte> data, opus_int32 len, out byte out_toc, Pointer<Pointer<byte>> frames, Pointer<opus_int16> size, out c_int payload_offset)
		{
			return Opus_Packet_Parse_Impl(data, len, false, out out_toc, frames, size, out payload_offset, out _, out _, out _);
		}
	}
}
