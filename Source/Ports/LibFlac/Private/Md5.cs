/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Ports.LibFlac.Share;

namespace Polycode.NostalgicPlayer.Ports.LibFlac.Private
{
	/// <summary>
	/// MD5 implementation
	/// </summary>
	internal class Md5
	{
		[StructLayout(LayoutKind.Explicit)]
		private struct Flac__MultiByte
		{
			[FieldOffset(0)] public Flac__byte[] P8;
			[FieldOffset(0)] public Flac__int16[] P16;
			[FieldOffset(0)] public Flac__int32[] P32;
		}

		private class Flac__Md5Context
		{
			public readonly Flac__uint32[] In = new Flac__uint32[16];
			public readonly Flac__uint32[] Buf = new Flac__uint32[4];
			public readonly Flac__uint32[] Bytes = new Flac__uint32[2];
			public Flac__MultiByte Internal_Buf = new Flac__MultiByte();
			public size_t Capacity;
		}

		private readonly Flac__Md5Context ctx = new Flac__Md5Context();

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Md5()
		{
			Flac__Md5Init();
		}



		/********************************************************************/
		/// <summary>
		/// Start MD5 accumulation. Set bit count to 0 and buffer to
		/// mysterious initialization constants
		/// </summary>
		/********************************************************************/
		public void Flac__Md5Init()
		{
			ctx.Buf[0] = 0x67452301;
			ctx.Buf[1] = 0xefcdab89;
			ctx.Buf[2] = 0x98badcfe;
			ctx.Buf[3] = 0x10325476;

			ctx.Bytes[0] = 0;
			ctx.Bytes[1] = 0;

			ctx.Internal_Buf.P8 = null;
			ctx.Capacity = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Update context to reflect the concatenation of another buffer
		/// full of bytes
		/// </summary>
		/********************************************************************/
		public void Flac__Md5Update(Flac__byte[] buf, uint32_t len)
		{
			Span<Flac__byte> inByte = MemoryMarshal.Cast<Flac__uint32, Flac__byte>(ctx.In);
			Span<Flac__byte> spanBuf;

			// Update byte count
			Flac__uint32 t = ctx.Bytes[0];
			if ((ctx.Bytes[0] = t + len) < t)
				ctx.Bytes[1]++;		// Carry from low to high

			t = 64 - (t & 0x3f);	// Space available in @in (at least 1)
			if (t > len)
			{
				spanBuf = new Span<byte>(buf, 0, (int)len);
				spanBuf.CopyTo(inByte.Slice(64 - (int)t));
				return;
			}

			// First chunk is an odd size
			spanBuf = new Span<byte>(buf, 0, (int)t);
			spanBuf.CopyTo(inByte.Slice(64 - (int)t));
			ByteSwapX16(ctx.In);

			Flac__Md5Transform(ctx.Buf, ctx.In);
			int offset = (int)t;
			len -= t;

			// Process data in 64-byte chunks
			while (len >= 64)
			{
				spanBuf = new Span<byte>(buf, offset, 64);
				spanBuf.CopyTo(inByte);
				ByteSwapX16(ctx.In);
				Flac__Md5Transform(ctx.Buf, ctx.In);
				offset += 64;
				len -= 64;
			}

			// Handle the remaining bytes of data
			spanBuf = new Span<byte>(buf, offset, (int)len);
			spanBuf.CopyTo(inByte);
		}



		/********************************************************************/
		/// <summary>
		/// Final wrapup - pad to 64-byte boundary with the bit pattern
		/// 1 0* (64-bit count of bits processed, MSB-first)
		/// </summary>
		/********************************************************************/
		public Flac__byte[] Flac__Md5Final()
		{
			Span<Flac__byte> inByte = MemoryMarshal.Cast<Flac__uint32, Flac__byte>(ctx.In);

			int count = (int)ctx.Bytes[0] & 0x3f;	// Number of bytes in @in
			int p = count;

			// Set the first char of padding to 0x80. There is always room
			inByte[p++] = 0x80;

			// Bytes of padding needed to make 56 bytes (-8..55)
			count = 56 - 1 - count;

			if (count < 0)		// Padding forces an extra block
			{
				inByte.Slice(p, count + 8).Clear();
				ByteSwapX16(ctx.In);
				Flac__Md5Transform(ctx.Buf, ctx.In);
				p = 0;
				count = 56;
			}

			inByte.Slice(p, count).Clear();
			ByteSwap(ctx.In, 14);

			// Append length in bits and transform
			ctx.In[14] = ctx.Bytes[0] << 3;
			ctx.In[15] = ctx.Bytes[1] << 3 | ctx.Bytes[0] >> 29;
			Flac__Md5Transform(ctx.Buf, ctx.In);

			ByteSwap(ctx.Buf, 4);
			Flac__byte[] digests = MemoryMarshal.Cast<Flac__uint32, Flac__byte>(ctx.Buf).ToArray();

			if (ctx.Internal_Buf.P8 != null)
			{
				ctx.Internal_Buf.P8 = null;
				ctx.Capacity = 0;
			}

			Array.Clear(ctx.In);
			Array.Clear(ctx.Buf);
			Array.Clear(ctx.Bytes);

			return digests;
		}



		/********************************************************************/
		/// <summary>
		/// Convert the incoming audio signal to a byte stream and MD5Update
		/// it
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__Md5Accumulate(Flac__int32[][] signal, uint32_t channels, uint32_t samples, uint32_t bytes_Per_Sample)
		{
			size_t bytes_Needed = channels * samples * bytes_Per_Sample;

			// Overflow check
			if (channels > (size_t.MaxValue / bytes_Per_Sample))
				return false;

			if (channels * bytes_Per_Sample > (size_t.MaxValue / samples))
				return false;

			if (ctx.Capacity < bytes_Needed)
			{
				if (ctx.Internal_Buf.P8 != null)
					Array.Resize(ref ctx.Internal_Buf.P8, (int)bytes_Needed);
				else
					ctx.Internal_Buf.P8 = new Flac__byte[bytes_Needed];

				ctx.Capacity = bytes_Needed;
			}

			Format_Input(ref ctx.Internal_Buf, signal, channels, samples, bytes_Per_Sample);

			Flac__Md5Update(ctx.Internal_Buf.P8, (uint32_t)bytes_Needed);

			return true;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ByteSwap(Flac__uint32[] buf, uint32_t words)
		{
			if (!BitConverter.IsLittleEndian)
				throw new NotSupportedException("Big endian mode not supported at the moment");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ByteSwapX16(Flac__uint32[] buf)
		{
			if (!BitConverter.IsLittleEndian)
				throw new NotSupportedException("Big endian mode not supported at the moment");
		}



		/********************************************************************/
		/// <summary>
		/// The core of the MD5 algorithm, this alters an existing MD5 hash
		/// to reflect the addition of 16 longwords of new data. MD5Update
		/// blocks the data and converts bytes into longwords for this
		/// routine
		/// </summary>
		/********************************************************************/
		private void Flac__Md5Transform(Flac__uint32[] buf, Flac__uint32[] @in)
		{
			Flac__uint32 a = buf[0];
			Flac__uint32 b = buf[1];
			Flac__uint32 c = buf[2];
			Flac__uint32 d = buf[3];

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			void Md5Step(Func<Flac__uint32, Flac__uint32, Flac__uint32, Flac__uint32> f, ref Flac__uint32 w, Flac__uint32 x, Flac__uint32 y, Flac__uint32 z, Flac__uint32 @in, Flac__int32 s)
			{
				w += f(x, y, z) + @in;
				w = (w << s | w >> (32 - s)) + x;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			Flac__uint32 F1(Flac__uint32 x, Flac__uint32 y, Flac__uint32 z) => z ^ (x & (y ^ z));

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			Flac__uint32 F2(Flac__uint32 x, Flac__uint32 y, Flac__uint32 z) => F1(z, x, y);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			Flac__uint32 F3(Flac__uint32 x, Flac__uint32 y, Flac__uint32 z) => x ^ y ^ z;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			Flac__uint32 F4(Flac__uint32 x, Flac__uint32 y, Flac__uint32 z) => y ^ (x | ~z);

			Md5Step(F1, ref a, b, c, d, @in[0] + 0xd76aa478, 7);
			Md5Step(F1, ref d, a, b, c, @in[1] + 0xe8c7b756, 12);
			Md5Step(F1, ref c, d, a, b, @in[2] + 0x242070db, 17);
			Md5Step(F1, ref b, c, d, a, @in[3] + 0xc1bdceee, 22);
			Md5Step(F1, ref a, b, c, d, @in[4] + 0xf57c0faf, 7);
			Md5Step(F1, ref d, a, b, c, @in[5] + 0x4787c62a, 12);
			Md5Step(F1, ref c, d, a, b, @in[6] + 0xa8304613, 17);
			Md5Step(F1, ref b, c, d, a, @in[7] + 0xfd469501, 22);
			Md5Step(F1, ref a, b, c, d, @in[8] + 0x698098d8, 7);
			Md5Step(F1, ref d, a, b, c, @in[9] + 0x8b44f7af, 12);
			Md5Step(F1, ref c, d, a, b, @in[10] + 0xffff5bb1, 17);
			Md5Step(F1, ref b, c, d, a, @in[11] + 0x895cd7be, 22);
			Md5Step(F1, ref a, b, c, d, @in[12] + 0x6b901122, 7);
			Md5Step(F1, ref d, a, b, c, @in[13] + 0xfd987193, 12);
			Md5Step(F1, ref c, d, a, b, @in[14] + 0xa679438e, 17);
			Md5Step(F1, ref b, c, d, a, @in[15] + 0x49b40821, 22);

			Md5Step(F2, ref a, b, c, d, @in[1] + 0xf61e2562, 5);
			Md5Step(F2, ref d, a, b, c, @in[6] + 0xc040b340, 9);
			Md5Step(F2, ref c, d, a, b, @in[11] + 0x265e5a51, 14);
			Md5Step(F2, ref b, c, d, a, @in[0] + 0xe9b6c7aa, 20);
			Md5Step(F2, ref a, b, c, d, @in[5] + 0xd62f105d, 5);
			Md5Step(F2, ref d, a, b, c, @in[10] + 0x02441453, 9);
			Md5Step(F2, ref c, d, a, b, @in[15] + 0xd8a1e681, 14);
			Md5Step(F2, ref b, c, d, a, @in[4] + 0xe7d3fbc8, 20);
			Md5Step(F2, ref a, b, c, d, @in[9] + 0x21e1cde6, 5);
			Md5Step(F2, ref d, a, b, c, @in[14] + 0xc33707d6, 9);
			Md5Step(F2, ref c, d, a, b, @in[3] + 0xf4d50d87, 14);
			Md5Step(F2, ref b, c, d, a, @in[8] + 0x455a14ed, 20);
			Md5Step(F2, ref a, b, c, d, @in[13] + 0xa9e3e905, 5);
			Md5Step(F2, ref d, a, b, c, @in[2] + 0xfcefa3f8, 9);
			Md5Step(F2, ref c, d, a, b, @in[7] + 0x676f02d9, 14);
			Md5Step(F2, ref b, c, d, a, @in[12] + 0x8d2a4c8a, 20);

			Md5Step(F3, ref a, b, c, d, @in[5] + 0xfffa3942, 4);
			Md5Step(F3, ref d, a, b, c, @in[8] + 0x8771f681, 11);
			Md5Step(F3, ref c, d, a, b, @in[11] + 0x6d9d6122, 16);
			Md5Step(F3, ref b, c, d, a, @in[14] + 0xfde5380c, 23);
			Md5Step(F3, ref a, b, c, d, @in[1] + 0xa4beea44, 4);
			Md5Step(F3, ref d, a, b, c, @in[4] + 0x4bdecfa9, 11);
			Md5Step(F3, ref c, d, a, b, @in[7] + 0xf6bb4b60, 16);
			Md5Step(F3, ref b, c, d, a, @in[10] + 0xbebfbc70, 23);
			Md5Step(F3, ref a, b, c, d, @in[13] + 0x289b7ec6, 4);
			Md5Step(F3, ref d, a, b, c, @in[0] + 0xeaa127fa, 11);
			Md5Step(F3, ref c, d, a, b, @in[3] + 0xd4ef3085, 16);
			Md5Step(F3, ref b, c, d, a, @in[6] + 0x04881d05, 23);
			Md5Step(F3, ref a, b, c, d, @in[9] + 0xd9d4d039, 4);
			Md5Step(F3, ref d, a, b, c, @in[12] + 0xe6db99e5, 11);
			Md5Step(F3, ref c, d, a, b, @in[15] + 0x1fa27cf8, 16);
			Md5Step(F3, ref b, c, d, a, @in[2] + 0xc4ac5665, 23);

			Md5Step(F4, ref a, b, c, d, @in[0] + 0xf4292244, 6);
			Md5Step(F4, ref d, a, b, c, @in[7] + 0x432aff97, 10);
			Md5Step(F4, ref c, d, a, b, @in[14] + 0xab9423a7, 15);
			Md5Step(F4, ref b, c, d, a, @in[5] + 0xfc93a039, 21);
			Md5Step(F4, ref a, b, c, d, @in[12] + 0x655b59c3, 6);
			Md5Step(F4, ref d, a, b, c, @in[3] + 0x8f0ccc92, 10);
			Md5Step(F4, ref c, d, a, b, @in[10] + 0xffeff47d, 15);
			Md5Step(F4, ref b, c, d, a, @in[1] + 0x85845dd1, 21);
			Md5Step(F4, ref a, b, c, d, @in[8] + 0x6fa87e4f, 6);
			Md5Step(F4, ref d, a, b, c, @in[15] + 0xfe2ce6e0, 10);
			Md5Step(F4, ref c, d, a, b, @in[6] + 0xa3014314, 15);
			Md5Step(F4, ref b, c, d, a, @in[13] + 0x4e0811a1, 21);
			Md5Step(F4, ref a, b, c, d, @in[4] + 0xf7537e82, 6);
			Md5Step(F4, ref d, a, b, c, @in[11] + 0xbd3af235, 10);
			Md5Step(F4, ref c, d, a, b, @in[2] + 0x2ad7d2bb, 15);
			Md5Step(F4, ref b, c, d, a, @in[9] + 0xeb86d391, 21);

			buf[0] += a;
			buf[1] += b;
			buf[2] += c;
			buf[3] += d;
		}



		/********************************************************************/
		/// <summary>
		/// Convert the incoming audio signal to a byte stream
		/// </summary>
		/********************************************************************/
		private void Format_Input(ref Flac__MultiByte mBuf, Flac__int32[][] signal, uint32_t channels, uint32_t samples, uint32_t bytes_Per_Sample)
		{
			Flac__byte[] buf = mBuf.P8;
			Flac__int16[] buf16 = mBuf.P16;
			Flac__int32[] buf32 = mBuf.P32;
			int32_t offset = 0;

			// Storage in the output buffer, buf, is little endian

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			uint32_t Bytes_Channel_Selector(uint32_t bytes, uint32_t channels) => bytes * 100 + channels;

			Dictionary<uint32_t, Action> switchLookup = new Dictionary<uint32_t, Action>
			{
				// First do the most commonly used combinations
				//
				// One byte per sample
				{ Bytes_Channel_Selector(1, 1), () =>
					{
						for (uint32_t sample = 0; sample < samples; sample++)
							buf[offset++] = (Flac__byte)signal[0][sample];
					}
				},
				{ Bytes_Channel_Selector(1, 2), () =>
					{
						for (uint32_t sample = 0; sample < samples; sample++)
						{
							buf[offset++] = (Flac__byte)signal[0][sample];
							buf[offset++] = (Flac__byte)signal[1][sample];
						}
					}
				},
				{ Bytes_Channel_Selector(1, 4), () =>
					{
						for (uint32_t sample = 0; sample < samples; sample++)
						{
							buf[offset++] = (Flac__byte)signal[0][sample];
							buf[offset++] = (Flac__byte)signal[1][sample];
							buf[offset++] = (Flac__byte)signal[2][sample];
							buf[offset++] = (Flac__byte)signal[3][sample];
						}
					}
				},
				{ Bytes_Channel_Selector(1, 6), () =>
					{
						for (uint32_t sample = 0; sample < samples; sample++)
						{
							buf[offset++] = (Flac__byte)signal[0][sample];
							buf[offset++] = (Flac__byte)signal[1][sample];
							buf[offset++] = (Flac__byte)signal[2][sample];
							buf[offset++] = (Flac__byte)signal[3][sample];
							buf[offset++] = (Flac__byte)signal[4][sample];
							buf[offset++] = (Flac__byte)signal[5][sample];
						}
					}
				},
				{ Bytes_Channel_Selector(1, 8), () =>
					{
						for (uint32_t sample = 0; sample < samples; sample++)
						{
							buf[offset++] = (Flac__byte)signal[0][sample];
							buf[offset++] = (Flac__byte)signal[1][sample];
							buf[offset++] = (Flac__byte)signal[2][sample];
							buf[offset++] = (Flac__byte)signal[3][sample];
							buf[offset++] = (Flac__byte)signal[4][sample];
							buf[offset++] = (Flac__byte)signal[5][sample];
							buf[offset++] = (Flac__byte)signal[6][sample];
							buf[offset++] = (Flac__byte)signal[7][sample];
						}
					}
				},
				// Two bytes per sample
				{ Bytes_Channel_Selector(2, 1), () =>
					{
						for (uint32_t sample = 0; sample < samples; sample++)
							buf16[offset++] = EndSwap.H2LE_16((Flac__int16)signal[0][sample]);
					}
				},
				{ Bytes_Channel_Selector(2, 2), () =>
					{
						for (uint32_t sample = 0; sample < samples; sample++)
						{
							buf16[offset++] = EndSwap.H2LE_16((Flac__int16)signal[0][sample]);
							buf16[offset++] = EndSwap.H2LE_16((Flac__int16)signal[1][sample]);
						}
					}
				},
				{ Bytes_Channel_Selector(2, 4), () =>
					{
						for (uint32_t sample = 0; sample < samples; sample++)
						{
							buf16[offset++] = EndSwap.H2LE_16((Flac__int16)signal[0][sample]);
							buf16[offset++] = EndSwap.H2LE_16((Flac__int16)signal[1][sample]);
							buf16[offset++] = EndSwap.H2LE_16((Flac__int16)signal[2][sample]);
							buf16[offset++] = EndSwap.H2LE_16((Flac__int16)signal[3][sample]);
						}
					}
				},
				{ Bytes_Channel_Selector(2, 6), () =>
					{
						for (uint32_t sample = 0; sample < samples; sample++)
						{
							buf16[offset++] = EndSwap.H2LE_16((Flac__int16)signal[0][sample]);
							buf16[offset++] = EndSwap.H2LE_16((Flac__int16)signal[1][sample]);
							buf16[offset++] = EndSwap.H2LE_16((Flac__int16)signal[2][sample]);
							buf16[offset++] = EndSwap.H2LE_16((Flac__int16)signal[3][sample]);
							buf16[offset++] = EndSwap.H2LE_16((Flac__int16)signal[4][sample]);
							buf16[offset++] = EndSwap.H2LE_16((Flac__int16)signal[5][sample]);
						}
					}
				},
				{ Bytes_Channel_Selector(2, 8), () =>
					{
						for (uint32_t sample = 0; sample < samples; sample++)
						{
							buf16[offset++] = EndSwap.H2LE_16((Flac__int16)signal[0][sample]);
							buf16[offset++] = EndSwap.H2LE_16((Flac__int16)signal[1][sample]);
							buf16[offset++] = EndSwap.H2LE_16((Flac__int16)signal[2][sample]);
							buf16[offset++] = EndSwap.H2LE_16((Flac__int16)signal[3][sample]);
							buf16[offset++] = EndSwap.H2LE_16((Flac__int16)signal[4][sample]);
							buf16[offset++] = EndSwap.H2LE_16((Flac__int16)signal[5][sample]);
							buf16[offset++] = EndSwap.H2LE_16((Flac__int16)signal[6][sample]);
							buf16[offset++] = EndSwap.H2LE_16((Flac__int16)signal[7][sample]);
						}
					}
				},
				// Three bytes per sample
				{ Bytes_Channel_Selector(3, 1), () =>
					{
						for (uint32_t sample = 0; sample < samples; sample++)
						{
							Flac__int32 a_Word = signal[0][sample];

							buf[offset++] = (Flac__byte)a_Word;
							a_Word >>= 8;
							buf[offset++] = (Flac__byte)a_Word;
							a_Word >>= 8;
							buf[offset++] = (Flac__byte)a_Word;
						}
					}
				},
				{ Bytes_Channel_Selector(3, 2), () =>
					{
						for (uint32_t sample = 0; sample < samples; sample++)
						{
							Flac__int32 a_Word = signal[0][sample];

							buf[offset++] = (Flac__byte)a_Word;
							a_Word >>= 8;
							buf[offset++] = (Flac__byte)a_Word;
							a_Word >>= 8;
							buf[offset++] = (Flac__byte)a_Word;

							a_Word = signal[1][sample];

							buf[offset++] = (Flac__byte)a_Word;
							a_Word >>= 8;
							buf[offset++] = (Flac__byte)a_Word;
							a_Word >>= 8;
							buf[offset++] = (Flac__byte)a_Word;
						}
					}
				},
				// Four bytes per sample
				{ Bytes_Channel_Selector(4, 1), () =>
					{
						for (uint32_t sample = 0; sample < samples; sample++)
							buf32[offset++] = EndSwap.H2LE_32(signal[0][sample]);
					}
				},
				{ Bytes_Channel_Selector(4, 2), () =>
					{
						for (uint32_t sample = 0; sample < samples; sample++)
						{
							buf32[offset++] = EndSwap.H2LE_32(signal[0][sample]);
							buf32[offset++] = EndSwap.H2LE_32(signal[1][sample]);
						}
					}
				},
				{ Bytes_Channel_Selector(4, 4), () =>
					{
						for (uint32_t sample = 0; sample < samples; sample++)
						{
							buf32[offset++] = EndSwap.H2LE_32(signal[0][sample]);
							buf32[offset++] = EndSwap.H2LE_32(signal[1][sample]);
							buf32[offset++] = EndSwap.H2LE_32(signal[2][sample]);
							buf32[offset++] = EndSwap.H2LE_32(signal[3][sample]);
						}
					}
				},
				{ Bytes_Channel_Selector(4, 6), () =>
					{
						for (uint32_t sample = 0; sample < samples; sample++)
						{
							buf32[offset++] = EndSwap.H2LE_32(signal[0][sample]);
							buf32[offset++] = EndSwap.H2LE_32(signal[1][sample]);
							buf32[offset++] = EndSwap.H2LE_32(signal[2][sample]);
							buf32[offset++] = EndSwap.H2LE_32(signal[3][sample]);
							buf32[offset++] = EndSwap.H2LE_32(signal[4][sample]);
							buf32[offset++] = EndSwap.H2LE_32(signal[5][sample]);
						}
					}
				},
				{ Bytes_Channel_Selector(4, 8), () =>
					{
						for (uint32_t sample = 0; sample < samples; sample++)
						{
							buf32[offset++] = EndSwap.H2LE_32(signal[0][sample]);
							buf32[offset++] = EndSwap.H2LE_32(signal[1][sample]);
							buf32[offset++] = EndSwap.H2LE_32(signal[2][sample]);
							buf32[offset++] = EndSwap.H2LE_32(signal[3][sample]);
							buf32[offset++] = EndSwap.H2LE_32(signal[4][sample]);
							buf32[offset++] = EndSwap.H2LE_32(signal[5][sample]);
							buf32[offset++] = EndSwap.H2LE_32(signal[6][sample]);
							buf32[offset++] = EndSwap.H2LE_32(signal[7][sample]);
						}
					}
				},
				// General version
				{ 1, () =>
					{
						for (uint32_t sample = 0; sample < samples; sample++)
						{
							for (uint32_t channel = 0; channel < channels; channel++)
								buf[offset++] = (Flac__byte)signal[channel][sample];
						}
					}
				},
				{ 2, () =>
					{
						for (uint32_t sample = 0; sample < samples; sample++)
						{
							for (uint32_t channel = 0; channel < channels; channel++)
								buf16[offset++] = EndSwap.H2LE_16((Flac__int16)signal[channel][sample]);
						}
					}
				},
				{ 3, () =>
					{
						for (uint32_t sample = 0; sample < samples; sample++)
						{
							for (uint32_t channel = 0; channel < channels; channel++)
							{
								Flac__int32 a_Word = signal[channel][sample];

								buf[offset++] = (Flac__byte)a_Word;
								a_Word >>= 8;
								buf[offset++] = (Flac__byte)a_Word;
								a_Word >>= 8;
								buf[offset++] = (Flac__byte)a_Word;
							}
						}
					}
				},
				{ 4, () =>
					{
						for (uint32_t sample = 0; sample < samples; sample++)
						{
							for (uint32_t channel = 0; channel < channels; channel++)
								buf32[offset++] = EndSwap.H2LE_32(signal[channel][sample]);
						}
					}
				}
			};

			if (switchLookup.TryGetValue(Bytes_Channel_Selector(bytes_Per_Sample, channels), out Action action))
			{
				action();
				return;
			}

			if (switchLookup.TryGetValue(bytes_Per_Sample, out action))
				action();
		}
		#endregion
	}
}
