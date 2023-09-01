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
		private static readonly int8[] vdic_Table = new int8[128]
		{
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
		};

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
			c_int extraLen = 4;

			// Disable birectional loop flag if sample is not looped
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
			}

			// Add guard bytes before the buffer for higher order interpolation
			xxs.Data = new byte[byteLen + extraLen + 4];
			if (xxs.Data == null)
				goto Err;

			MemoryMarshal.Cast<byte, uint32>(xxs.Data)[0] = 0;
			xxs.DataOffset = 4;

			if ((flags & Sample_Flag.NoLoad) != 0)
				buffer.Slice(0, byteLen).CopyTo(xxs.Data.AsSpan(xxs.DataOffset));
			else
			{
				if ((flags & Sample_Flag.Adpcm) != 0)
				{
					c_int x2 = (byteLen + 1) >> 1;
					sbyte[] table = new sbyte[16];

					if (f.Hio_Read(table, 1, 16) != 16)
						goto Err2;

					if (f.Hio_Read(xxs.Data.AsSpan(xxs.DataOffset + x2), 1, (size_t)x2) != (size_t)x2)
						goto Err2;

					Adpcm4_Decoder(xxs.Data, xxs.DataOffset + x2, xxs.Data, xxs.DataOffset, table, byteLen);
				}
				else
				{
					c_int x = (c_int)f.Hio_Read(xxs.Data.AsSpan(xxs.DataOffset), 1, (size_t)byteLen);
					if (x != byteLen)
						Array.Clear(xxs.Data, xxs.DataOffset + x, byteLen - x);
				}
			}

			if ((flags & Sample_Flag._7Bit) != 0)
				Convert_7Bit_To_8Bit(xxs.Data, xxs.DataOffset, xxs.Len);

			// Fix endianism if needed
			if ((xxs.Flg & Xmp_Sample_Flag._16Bit) != 0)
			{
				if (!BitConverter.IsLittleEndian)
				{
					if ((~flags & Sample_Flag.BigEnd) != 0)
						Convert_Endian(xxs.Data, xxs.DataOffset, xxs.Len);
				}
				else
				{
					if ((flags & Sample_Flag.BigEnd) != 0)
						Convert_Endian(xxs.Data, xxs.DataOffset, xxs.Len);
				}
			}

			// Convert delta samples
			if ((flags & Sample_Flag.Diff) != 0)
				Convert_Delta(xxs.Data, xxs.DataOffset, xxs.Len, (xxs.Flg & Xmp_Sample_Flag._16Bit) != 0);
			else if ((flags & Sample_Flag._8BDiff) != 0)
			{
				c_int len = xxs.Len;

				if ((xxs.Flg & Xmp_Sample_Flag._16Bit) != 0)
					len *= 2;

				Convert_Delta(xxs.Data, xxs.DataOffset, len, false);
			}

			// Convert samples to signed
			if ((flags & Sample_Flag.Uns) != 0)
				Convert_Signal(xxs.Data, xxs.DataOffset, xxs.Len, (xxs.Flg & Xmp_Sample_Flag._16Bit) != 0);

			if ((flags & Sample_Flag.Vidc) != 0)
				Convert_Vidc_To_Linear(xxs.Data, xxs.DataOffset, xxs.Len);

			// Check for full loop samples
			if ((flags & Sample_Flag.FullRep) != 0)
			{
				if ((xxs.Lps == 0) && (xxs.Len > xxs.Lpe))
					xxs.Flg |= Xmp_Sample_Flag.Loop_Full;
			}

			// Add extra samples at end
			if ((xxs.Flg & Xmp_Sample_Flag._16Bit) != 0)
			{
				for (c_int i = 0; i < 8; i++)
					xxs.Data[xxs.DataOffset + byteLen + i] = xxs.Data[xxs.DataOffset + byteLen - 2 + i];
			}
			else
			{
				for (c_int i = 0; i < 4; i++)
					xxs.Data[xxs.DataOffset + byteLen + i] = xxs.Data[xxs.DataOffset + byteLen - 1 + i];
			}

			// Add extra samples at start
			if ((xxs.Flg & Xmp_Sample_Flag._16Bit) != 0)
			{
				xxs.Data[xxs.DataOffset - 2] = xxs.Data[xxs.DataOffset];
				xxs.Data[xxs.DataOffset - 1] = xxs.Data[xxs.DataOffset + 1];
			}
			else
				xxs.Data[xxs.DataOffset - 1] = xxs.Data[xxs.DataOffset];

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
		private static void Convert_Delta(uint8[] p, int off, c_int l, bool r)
		{
			uint16 absVal = 0;

			if (r)
			{
				Span<uint16> w = MemoryMarshal.Cast<uint8, uint16>(p);
				off /= 2;

				for (; l-- != 0;)
				{
					absVal = (uint16)(w[off] + absVal);
					w[off++] = absVal;
				}
			}
			else
			{
				for (; l-- != 0;)
				{
					absVal = (uint16)(p[off] + absVal);
					p[off++] = (uint8)absVal;
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
		#endregion
	}
}
