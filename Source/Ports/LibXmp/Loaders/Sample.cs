/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Loader;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Loaders
{
	/// <summary>
	/// 
	/// </summary>
	internal static partial class Sample
	{
		/// <summary>
		/// From the Audio File Formats (version 2.5)
		/// Submitted-by: Guido van Rossum [guido@cwi.nl]
		/// Last-modified: 27-Aug-1992
		///
		/// The Acorn Archimedes uses a variation on U-LAW with the bit order
		/// reversed and the sign bit in bit 0. Being a 'minority' architecture,
		/// Arc owners are quite adept at converting sound/image formats from
		/// other machines, and it is unlikely that you'll ever encounter sound in
		/// one of the Arc's own formats (there are several)
		/// </summary>
		private static readonly int8[] vdic_Table =
		[
			/*   0 */	  0,   0,   0,   0,   0,   0,   0,   0,
			/*   8 */	  0,   0,   0,   0,   0,   0,   0,   0,
			/*  16 */	  0,   0,   0,   0,   0,   0,   0,   0,
			/*  24 */	  1,   1,   1,   1,   1,   1,   1,   1,
			/*  32 */	  1,   1,   1,   1,   2,   2,   2,   2,
			/*  40 */	  2,   2,   2,   2,   3,   3,   3,   3,
			/*  48 */	  3,   3,   4,   4,   4,   4,   5,   5,
			/*  56 */	  5,   5,   6,   6,   6,   6,   7,   7,
			/*  64 */	  7,   8,   8,   9,   9,  10,  10,  11,
			/*  72 */	 11,  12,  12,  13,  13,  14,  14,  15,
			/*  80 */	 15,  16,  17,  18,  19,  20,  21,  22,
			/*  88 */	 23,  24,  25,  26,  27,  28,  29,  30,
			/*  96 */	 31,  33,  34,  36,  38,  40,  42,  44,
			/* 104 */	 46,  48,  50,  52,  54,  56,  58,  60,
			/* 112 */	 62,  65,  68,  72,  77,  80,  84,  91,
			/* 120 */	 95,  98, 103, 109, 114, 120, 126, 127
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int LibXmp_Load_Sample(Module_Data m, Hio f, Sample_Flag flags, Xmp_Sample xxs, Span<uint8> buffer, int sampleNumber)
		{
			Hio s = f.GetSampleHio(sampleNumber, xxs.Len);

			c_int r = LibXmp_Load_Sample(m, s, flags, xxs, buffer);

			s.Hio_Close();

			return r;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int LibXmp_Load_Sample(Module_Data m, Hio f, Sample_Flag flags, Xmp_Sample xxs, Span<uint8> buffer)
		{
			c_int channels = 1;

			// Adlib FM patches
			if ((flags & Sample_Flag.Adlib) != 0)
				return 0;

			// Empty or invalid samples
			if (xxs.Len <= 0)
				return 0;

			// Skip sample loading
			// FIXME: Fails for ADPCM samples
			//
			// Sanity check: Skip huge samples (likely corrupt module)
			if ((xxs.Len > Constants.Max_Sample_Size) || ((m != null) && ((m.SmpCtl & Xmp_SmpCtl_Flag.Skip) != 0)))
			{
				if ((~flags & Sample_Flag.NoLoad) != 0)
					f.Hio_Seek(xxs.Len, SeekOrigin.Current);

				return 0;
			}

			// If this sample starts at or after EOF, skip it entirely
			if ((~flags & Sample_Flag.NoLoad) != 0)
			{
				if (f == null)
					return 0;

				c_long file_Pos = f.Hio_Tell();
				c_long file_Len = f.Hio_Size();

				if (file_Pos >= file_Len)
					return 0;

				// If this sample goes past EOF, truncate it
				if (((file_Pos + xxs.Len) > file_Len) && ((~flags & Sample_Flag.Adpcm) != 0))
					xxs.Len = file_Len - file_Pos;
			}

			// Loop parameters sanity check
			if (xxs.Lps < 0)
				xxs.Lps = 0;

			if (xxs.Lpe > xxs.Len)
				xxs.Lpe = xxs.Len;

			if ((xxs.Lps >= xxs.Len) || (xxs.Lps >= xxs.Lpe))
			{
				xxs.Lps = xxs.Lpe = 0;
				xxs.Flg &= ~(Xmp_Sample_Flag.Loop | Xmp_Sample_Flag.Loop_BiDir);
			}

			// Patches with samples
			// Allocate extra sample for interpolation
			c_int byteLen = xxs.Len;
			c_int frameLen = 1;
			c_int extraLen = 4;

			// Disable bidirectional loop flag if sample is not looped
			if ((xxs.Flg & Xmp_Sample_Flag.Loop_BiDir) != 0)
			{
				if ((~xxs.Flg & Xmp_Sample_Flag.Loop) != 0)
					xxs.Flg &= ~Xmp_Sample_Flag.Loop_BiDir;
			}

			if ((xxs.Flg & Xmp_Sample_Flag.SLoop_BiDir) != 0)
			{
				if ((~xxs.Flg & Xmp_Sample_Flag.SLoop) != 0)
					xxs.Flg &= ~Xmp_Sample_Flag.SLoop_BiDir;
			}

			if ((xxs.Flg & Xmp_Sample_Flag._16Bit) != 0)
			{
				byteLen *= 2;
				extraLen *= 2;
				frameLen *= 2;
			}

			if ((xxs.Flg & Xmp_Sample_Flag.Stereo) != 0)
			{
				byteLen *= 2;
				extraLen *= 2;
				frameLen *= 2;
				channels = 2;
			}

			// Add guard bytes before the buffer for higher order interpolation
			xxs.Data = new byte[byteLen + extraLen + 4];
			if (xxs.Data == null)
				goto Err;

			MemoryMarshal.Cast<byte, uint32>(xxs.Data)[0] = 0;
			xxs.DataOffset = 4;

			byte[] dest = xxs.Data;
			int destOffset = xxs.DataOffset;

			// If this is a non-interleaved stereo sample, most conversions need
			// to occur in an intermediate buffer prior to interleaving. Most
			// formats supporting stereo samples use non-interleaved stereo
			if (((xxs.Flg & Xmp_Sample_Flag.Stereo) != 0) && ((~flags & Sample_Flag.Interleaved) != 0))
			{
				byte[] tmp = new byte[byteLen];
				if (tmp == null)
					goto Err2;

				dest = tmp;
				destOffset = 0;
			}

			if ((flags & Sample_Flag.NoLoad) != 0)
				buffer.Slice(0, byteLen).CopyTo(dest.AsSpan(destOffset));
			else
			{
				if ((flags & Sample_Flag.Adpcm) != 0)
				{
					c_int x2 = (byteLen + 1) >> 1;
					sbyte[] table = new sbyte[16];

					if (f.Hio_Read(table, 1, 16) != 16)
						goto Err2;

					if (f.Hio_Read(dest.AsSpan(destOffset + x2), 1, (size_t)x2) != (size_t)x2)
						goto Err2;

					Adpcm4_Decoder(dest, destOffset + x2, dest, destOffset, table, byteLen);
				}
				else
				{
					c_int x = (c_int)f.Hio_Read(dest.AsSpan(destOffset), 1, (size_t)byteLen);
					if (x != byteLen)
						Array.Clear(dest, destOffset + x, byteLen - x);
				}
			}

			if ((flags & Sample_Flag._7Bit) != 0)
				Convert_7Bit_To_8Bit(dest, destOffset, xxs.Len * channels);

			// Fix endianism if needed
			if ((xxs.Flg & Xmp_Sample_Flag._16Bit) != 0)
			{
				if (!BitConverter.IsLittleEndian)
				{
					if ((~flags & Sample_Flag.BigEnd) != 0)
						Convert_Endian(dest, destOffset, xxs.Len);
				}
				else
				{
					if ((flags & Sample_Flag.BigEnd) != 0)
						Convert_Endian(dest, destOffset, xxs.Len);
				}
			}

			// Convert delta samples
			if ((flags & Sample_Flag.Diff) != 0)
				Convert_Delta(dest, destOffset, xxs.Len, (xxs.Flg & Xmp_Sample_Flag._16Bit) != 0, channels);
			else if ((flags & Sample_Flag._8BDiff) != 0)
			{
				c_int len = xxs.Len;

				if ((xxs.Flg & Xmp_Sample_Flag._16Bit) != 0)
					len *= 2;

				Convert_Delta(dest, destOffset, len, false, channels);
			}

			// Convert samples to signed
			if ((flags & Sample_Flag.Uns) != 0)
				Convert_Signal(dest, destOffset, xxs.Len * channels, (xxs.Flg & Xmp_Sample_Flag._16Bit) != 0);

			if ((flags & Sample_Flag.Vidc) != 0)
				Convert_Vidc_To_Linear(dest, destOffset, xxs.Len * channels);

			// Done converting individual samples; convert to interleaved
			if (((xxs.Flg & Xmp_Sample_Flag.Stereo) != 0) && ((~flags & Sample_Flag.Interleaved) != 0))
				Convert_Stereo_Interleaved(xxs.Data, xxs.DataOffset, dest, destOffset, xxs.Len, (xxs.Flg & Xmp_Sample_Flag._16Bit) != 0);

			// Check for full loop samples
			if ((flags & Sample_Flag.FullRep) != 0)
			{
				if ((xxs.Lps == 0) && (xxs.Len > xxs.Lpe))
					xxs.Flg |= Xmp_Sample_Flag.Loop_Full;
			}

			// Add extra samples at end
			for (c_int i = 0; i < extraLen; i++)
				xxs.Data[xxs.DataOffset + byteLen + i] = xxs.Data[xxs.DataOffset + byteLen - frameLen + i];

			// Add extra samples at start
			for (c_int i = -1; i >= -4; i--)
				xxs.Data[xxs.DataOffset + i] = xxs.Data[xxs.DataOffset + frameLen + i];

			return 0;

			Err2:
			LibXmp_Free_Sample(xxs);

			Err:
			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void LibXmp_Free_Sample(Xmp_Sample s)
		{
			if (s.Data != null)
				s.Data = null;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Convert 7 bit samples to 8 bit
		/// </summary>
		/********************************************************************/
		private static void Convert_7Bit_To_8Bit(uint8[] p, int off, c_int l)
		{
			for (; l-- != 0; off++)
				p[off] <<= 1;
		}



		/********************************************************************/
		/// <summary>
		/// Convert Archimedes VIDC samples to linear
		/// </summary>
		/********************************************************************/
		private static void Convert_Vidc_To_Linear(uint8[] p, int off, int l)
		{
			for (c_int i = 0; i < l; i++)
			{
				uint8 x = p[off + i];
				p[off + i] = (uint8)vdic_Table[x >> 1];

				if ((x & 0x01) != 0)
					p[off + i] = (uint8)((int8)p[off + i] * -1);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Adpcm4_Decoder(uint8[] inP, int inp_Off, uint8[] outP, int out_Off, sbyte[] tab, c_int len)
		{
			sbyte delta = 0;

			len = (len + 1) / 2;

			for (c_int i = 0; i < len; i++)
			{
				uint8 b0 = inP[inp_Off];
				uint8 b1 = (uint8)(inP[inp_Off++] >> 4);
				delta += tab[b0 & 0x0f];
				outP[out_Off++] = (uint8)delta;
				delta += tab[b1 & 0x0f];
				outP[out_Off++] = (uint8)delta;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Convert differential to absolute sample data
		/// </summary>
		/********************************************************************/
		private static void Convert_Delta(uint8[] p, int off, c_int frames, bool is_16Bit, c_int channels)
		{
			if (is_16Bit)
			{
				Span<uint16> w = MemoryMarshal.Cast<uint8, uint16>(p);
				off /= 2;

				for (c_int chn = 0; chn < channels; chn++)
				{
					uint16 absVal = 0;

					for (c_int i = 0; i < frames; i++)
					{
						absVal = (uint16)(w[off] + absVal);
						w[off++] = absVal;
					}
				}
			}
			else
			{
				for (c_int chn = 0; chn < channels; chn++)
				{
					uint16 absVal = 0;

					for (c_int i = 0; i < frames; i++)
					{
						absVal = (uint16)(p[off] + absVal);
						p[off++] = (uint8)absVal;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Convert signed to unsigned sample data
		/// </summary>
		/********************************************************************/
		private static void Convert_Signal(uint8[] p, int off, c_int l, bool r)
		{
			if (r)
			{
				Span<uint16> w = MemoryMarshal.Cast<uint8, uint16>(p);
				off /= 2;

				for (; l-- != 0;)
					w[off++] += 0x8000;
			}
			else
			{
				for (; l-- != 0;)
					p[off++] += 0x80;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Convert little-endian 16 bit samples to big-endian
		/// </summary>
		/********************************************************************/
		private static void Convert_Endian(uint8[] p, int off, c_int l)
		{
			for (c_int i = 0; i < l; i++)
			{
				uint8 b = p[off];
				p[off] = p[off + 1];
				p[off + 1] = b;

				off += 2;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Convert non-interleaved stereo to interleaved stereo.
		/// Due to tracker quirks this should be done after delta decoding,
		/// etc.
		/// </summary>
		/********************************************************************/
		private static void Convert_Stereo_Interleaved(uint8[] __out, int out_Off, uint8[] _in, int in_Off, c_int frames, bool is_16Bit)
		{
			if (is_16Bit)
			{
				Span<int16> in_L = MemoryMarshal.Cast<uint8, int16>(_in.AsSpan(in_Off));
				Span<int16> in_R = in_L.Slice(frames);
				Span<int16> _out = MemoryMarshal.Cast<uint8, int16>(__out);
				out_Off /= 2;

				for (c_int i = 0; i < frames; i++)
				{
					_out[out_Off++] = in_L[i];
					_out[out_Off++] = in_R[i];
				}				
			}
			else
			{
				Span<uint8> in_L = _in.AsSpan(in_Off);
				Span<uint8> in_R = in_L.Slice(frames);

				for (c_int i = 0; i < frames; i++)
				{
					__out[out_Off++] = in_L[i];
					__out[out_Off++] = in_R[i];
				}				
			}
		}
		#endregion
	}
}
