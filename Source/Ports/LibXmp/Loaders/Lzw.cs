/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.CKit;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Lzw;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Loaders
{
	/// <summary>
	/// Simple LZW decoder for Digital Symphony.
	/// This does not handle the hacks required for ARC or UnShrink
	/// </summary>
	internal static class Lzw
	{
		private const c_int Lzw_No_Code = 0xffff;
		private const c_int Lzw_Code_Clear = 256;
		private const c_int Lzw_Code_Sym_Eof = 257;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int LibXmp_Read_Lzw(CPointer<uint8> dest, size_t dest_Len, size_t max_Read_Len, c_int flags, Hio f)
		{
			CPointer<uint8> start = dest;
			CPointer<uint8> pos = start;
			size_t left = dest_Len;
			c_int code;

			BitStream bs = Bs_Init(max_Read_Len);
			Lzw_Tree lzw = Lzw_Init_Tree(flags);
			if (lzw == null)
				return -1;

			while (left > 0)
			{
				code = Bs_Read(bs, f, (c_int)lzw.Bits);
				if (code < 0)
					break;

				if (code == Lzw_Code_Clear)
				{
					Lzw_Clear(lzw);
					continue;
				}
				else if (((flags & Lzw_Flag.SymQuirks) != 0) && (code == Lzw_Code_Sym_Eof))
					break;

				lzw.New_Inc = false;

				c_int result = Lzw_Decode(lzw, (uint16)code, ref pos, ref left);
				if (result != 0)
					break;
			}

			if (left > 0)
				CMemory.MemSet<uint8>(pos, 0, (c_int)left);
			else if ((flags & Lzw_Flag.SymQuirks) != 0)
			{
				// Digital Symphony - read final EOF code
				if (lzw.New_Inc)
				{
					// If the final code prior to EOF should have increased
					// the bitwidth, read the EOF with the old bitwidth
					// instead of the new one.
					// 
					// This anomaly exists in FULLEFFECT, NARCOSIS and
					// NEWDANCE. In NEWDANCE (libxmp's test file for this),
					// it occurs specifically in the LZW-compressed sequence.
					// https://github.com/libxmp/libxmp/issues/347
					lzw.Bits--;
				}

				code = Bs_Read(bs, f, (c_int)lzw.Bits);
			}

			if ((flags & Lzw_Flag.SymQuirks) != 0)
			{
				// Digital Symphony LZW compressed stream size is 4 aligned
				size_t num_Read = bs.Num_Read;

				while ((num_Read & 3) != 0)
				{
					f.Hio_Read8();
					num_Read++;
				}
			}

			Lzw_Free(lzw);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Decode Digital Symphony sigma-delta compressed samples.
		/// This isn't really LZW but it uses the same bitstream and
		/// alignment hacks.
		///
		/// Based on the sigma-delta unpacker from OpenMPT by Saga Musix
		/// </summary>
		/********************************************************************/
		public static c_int LibXmp_Read_Sigma_Delta(CPointer<uint8> dest, size_t dest_Len, size_t max_Read_Len, Hio f)
		{
			CPointer<uint8> pos = dest;
			CPointer<uint8> end = pos + dest_Len;
			c_int runLength = 0;
			c_int bits = 8;

			if (dest_Len == 0)
				return 0;

			BitStream bs = Bs_Init(max_Read_Len);

			// DOESN'T count towards alignment
			c_int max_RunLength = f.Hio_Read8();

			// DOES count
			uint8 accumulator = (uint8)Bs_Read(bs, f, bits);
			pos[0, 1] = accumulator;

			while (pos < end)
			{
				c_int value = Bs_Read(bs, f, bits);

				if (value < 0)
					return -1;

				// Expand bitwidth
				if (value == 0)
				{
					if (bits >= 9)
						return -1;

					bits++;
					runLength = 0;
					continue;
				}

				if ((value & 1) != 0)
					accumulator -= (uint8)(value >> 1);
				else
					accumulator += (uint8)(value >> 1);

				pos[0, 1] = accumulator;

				// High bit set resets the run length
				if ((value >> (bits - 1)) != 0)
				{
					runLength = 0;
					continue;
				}

				// Reduce bitwidth
				if (++runLength >= max_RunLength)
				{
					if (bits > 1)
						bits--;

					runLength = 0;
				}
			}

			// Digital Symphony aligns bitstreams to lengths of 4
			if ((bs.Num_Read & 3) != 0)
			{
				size_t total = bs.Num_Read;

				while ((total & 3) != 0)
				{
					f.Hio_Read8();
					total++;
				}
			}

			return 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static BitStream Bs_Init(size_t max_Read)
		{
			BitStream bs = new BitStream
			{
				Buf = 0,
				Num_Read = 0,
				Max_Read = max_Read,
				Bits = 0
			};

			return bs;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Bs_Read(BitStream bs, Hio f, c_int bits)
		{
			if (bs.Bits < bits)
			{
				while (bs.Bits < bits)
				{
					if (bs.Num_Read >= bs.Max_Read)
						return -1;

					uint8 byt = f.Hio_Read8();
					bs.Buf |= (uint32)(byt << bs.Bits);
					bs.Bits += 8;
					bs.Num_Read++;
				}

				if (f.Hio_Error() != 0)
					return -1;
			}

			c_int ret = (c_int)(bs.Buf & ((1 << bits) - 1));
			bs.Buf >>= bits;
			bs.Bits -= bits;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Lzw_Tree Lzw_Init_Tree(c_int flags)
		{
			uint maxBits = (uint)Lzw_Flag.MaxBits(flags);

			Lzw_Tree lzw = new Lzw_Tree();
			lzw.Bits = 9;

			if ((maxBits < lzw.Bits) || (maxBits > 16))
				return null;

			lzw.DefaultLength = 258;	// 256 chars + clear + EOF
			lzw.MaxLength = 1U << (int)lzw.Bits;
			lzw.AllocLength = 1U << (int)maxBits;

			lzw.Codes = CMemory.CAllocObj<Lzw_Code>((c_int)lzw.AllocLength);
			if (lzw.Codes == null)
				return null;

			lzw.Length = lzw.DefaultLength;
			lzw.Previous_Code = Lzw_No_Code;
			lzw.New_Inc = false;
			lzw.Flags = flags;
			lzw.Previous_First_Char = 0;

			for (uint i = 0; i < 256; i++)
			{
				lzw.Codes[i].Length = 1;
				lzw.Codes[i].Value = (uint8)i;
				lzw.Codes[i].Prev = Lzw_No_Code;
			}

			return lzw;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Lzw_Free(Lzw_Tree lzw)
		{
			CMemory.Free(lzw.Codes);
			lzw.Codes.SetToNull();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Lzw_Add(Lzw_Tree lzw)
		{
			if (lzw.Length >= lzw.AllocLength)
				return;

			Lzw_Code current = lzw.Codes[lzw.Length++];

			// Increase bitwidth if the NEXT code would be maxlength
			if ((lzw.Length >= lzw.MaxLength) && (lzw.Length < lzw.AllocLength))
			{
				lzw.MaxLength <<= 1;
				lzw.Bits++;
				lzw.New_Inc = true;
			}

			current.Prev = (uint16)lzw.Previous_Code;
			current.Value = lzw.Previous_First_Char;

			// NOTE: when the length cache deadcode below is enabled, this may
			// intentionally be set to or overflow to 0, in which case the length
			// will be computed as-needed by iterating the tree
			uint16 prev_Length = lzw.Codes[lzw.Previous_Code].Length;
			current.Length = (uint16)(prev_Length != 0 ? prev_Length + 1 : 0);
		}



		/********************************************************************/
		/// <summary>
		/// Reset the LZW tree length
		/// </summary>
		/********************************************************************/
		private static void Lzw_Clear(Lzw_Tree lzw)
		{
			lzw.Bits = 9;
			lzw.MaxLength = (1U << (int)lzw.Bits);
			lzw.Length = lzw.DefaultLength;
			lzw.Previous_Code = Lzw_No_Code;

			// TNE: Uncommented code removed
		}



		/********************************************************************/
		/// <summary>
		/// Get the length of an LZW code, or compute it if it isn't
		/// currently stored. This happens when one or mode codes in the
		/// sequence are marked for reuse
		/// </summary>
		/********************************************************************/
		private static uint16 Lzw_Get_Length(Lzw_Tree lzw, Lzw_Code c)
		{
			// TNE: Uncommented code removed

			return c.Length;
		}



		/********************************************************************/
		/// <summary>
		/// Output an LZW code
		/// </summary>
		/********************************************************************/
		private static c_int Lzw_Output(Lzw_Tree lzw, uint16 code, ref CPointer<uint8> _pos, ref size_t left)
		{
			CPointer<uint8> pos = _pos;

			CPointer<Lzw_Code> codes = lzw.Codes;
			Lzw_Code current = codes[code];
			uint length = Lzw_Get_Length(lzw, current);

			if ((length == 0) || (length > left))
				return -1;

			for (uint i = length - 1; i > 0; i--)
			{
				pos[i] = current.Value;
				code = current.Prev;
				current = codes[code];
			}

			pos[0] = (uint8)code;
			_pos += length;
			left -= length;

			lzw.Previous_First_Char = (uint8)code;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Decode an LZW code and create the next code from known data
		/// </summary>
		/********************************************************************/
		private static c_int Lzw_Decode(Lzw_Tree lzw, uint16 code, ref CPointer<uint8> _pos, ref size_t left)
		{
			bool kwkwk = false;

			// Digital Symphony LZW never seems to reference cleared codes,
			// which allows some assumptions to be made (like never clearing the
			// cached code lengths). If this decoder needs to support those, the
			// cached length handling deadcode above needs to be uncommented
			if (code > lzw.Length)
				return -1;

			// This is a special case--the current code is the previous code with the
			// first character of the previous code appended, and needs to be added
			// before the output occurs (instead of after)
			if (code == lzw.Length)
			{
				if (lzw.Previous_Code == Lzw_No_Code)
					return -1;

				Lzw_Add(lzw);
				lzw.Previous_Code = code;
				kwkwk = true;
			}

			// Otherwise, output first, and then add a new code, which is the previous
			// code with the first character of the current code appended
			c_int result = Lzw_Output(lzw, code, ref _pos, ref left);
			if ((result == 0) && !kwkwk)
			{
				if (lzw.Previous_Code != Lzw_No_Code)
					Lzw_Add(lzw);

				lzw.Previous_Code = code;
			}

			return result;
		}
		#endregion
	}
}
