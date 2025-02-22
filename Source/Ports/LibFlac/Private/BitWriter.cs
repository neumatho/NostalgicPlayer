/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers;
using Polycode.NostalgicPlayer.Ports.LibFlac.Share;
using bwWord = System.UInt64;
using Flac__bwTemp = System.UInt64;

namespace Polycode.NostalgicPlayer.Ports.LibFlac.Private
{
	/// <summary>
	/// 
	/// </summary>
	internal class BitWriter
	{
		// The default capacity here doesn't matter too much. The buffer always grows
		// to hold whatever is written to it. Usually the encoder will stop adding at
		// a frame or metadata block, then write that out and clear the buffer for the
		// next one
		private const uint32_t Flac__BitWriter_Default_Capacity = 32768 / sizeof(bwWord);	// Size in words

		/// <summary>
		/// When growing, increment with 1/4th at a time
		/// </summary>
		private const int32_t Flac__BitWriter_Default_Grow_Fraction = 2;   // Means grow by >> 2 (1/4th) of current size

		private class Flac__BitWriter
		{
			public bwWord[] Buffer;
			public bwWord Accum;			// Accumulator; bits are right-justified; when full, accum is appended to buffer
			public uint32_t Capacity;		// Capacity of buffer in words
			public uint32_t Words;			// # of completed words in buffer
			public uint32_t Bits;			// # of used bits in accum
		}

		private Flac__BitWriter bw;

		#region Construction, deletion, initialization, etc methods
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private BitWriter()
		{
			bw = new Flac__BitWriter();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static BitWriter Flac__BitWriter_New()
		{
			return new BitWriter();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Flac__BitWriter_Delete()
		{
			Debug.Assert(bw != null);

			Flac__BitWriter_Free();
			bw = null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__BitWriter_Init()
		{
			Debug.Assert(bw != null);

			bw.Words = bw.Bits = 0;
			bw.Capacity = Flac__BitWriter_Default_Capacity;
			bw.Buffer = new bwWord[bw.Capacity];

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Flac__BitWriter_Free()
		{
			Debug.Assert(bw != null);

			bw.Buffer = null;
			bw.Capacity = 0;
			bw.Words = bw.Bits = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Flac__BitWriter_Clear()
		{
			bw.Words = bw.Bits = 0;
		}
		#endregion

		#region CRC methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__BitWriter_Get_Write_Crc16(out Flac__uint16 crc)
		{
			Debug.Assert((bw.Bits & 7) == 0);	// Assert that we're byte-aligned

			if (!Flac__BitWriter_Get_Buffer(out Span<byte> buffer, out size_t bytes))
			{
				crc = 0;
				return false;
			}

			crc = Crc.Flac__Crc16(buffer, (uint32_t)bytes);
			Flac__BitWriter_Release_Buffer();

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__BitWriter_Get_Write_Crc8(out Flac__byte crc)
		{
			Debug.Assert((bw.Bits & 7) == 0);	// Assert that we're byte-aligned

			if (!Flac__BitWriter_Get_Buffer(out Span<byte> buffer, out size_t bytes))
			{
				crc = 0;
				return false;
			}

			crc = Crc.Flac__Crc8(buffer, (uint32_t)bytes);
			Flac__BitWriter_Release_Buffer();

			return true;
		}
		#endregion

		#region Info methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__BitWriter_Is_Byte_Aligned()
		{
			return (bw.Bits & 7) == 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint32_t Flac__BitWriter_Get_Input_Bits_Unconsumed()
		{
			return (bw.Words * Constants.Flac__Bits_Per_Word) + bw.Bits;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__BitWriter_Get_Buffer(out Span<Flac__byte> buffer, out size_t bytes)
		{
			Debug.Assert((bw.Bits & 7) == 0);

			buffer = null;
			bytes = 0;

			// Double protection
			if ((bw.Bits & 7) != 0)
				return false;

			// If we have bits in the accumulator we have to flush those to the buffer first
			if (bw.Bits != 0)
			{
				Debug.Assert(bw.Words <= bw.Capacity);

				if ((bw.Words == bw.Capacity) && !BitWriter_Grow(Constants.Flac__Bits_Per_Word))
					return false;

				// Append bits as complete word to buffer, but don't change bw.Accum or bw.Bits
				bw.Buffer[bw.Words] = Swap_Be_Word_To_Host(bw.Accum << (int)(Constants.Flac__Bits_Per_Word - bw.Bits));
			}

			// Now we can just return what we have
			buffer = MemoryMarshal.Cast<bwWord, Flac__byte>(bw.Buffer);
			bytes = (Constants.Flac__Bytes_Per_Word * bw.Words) + (bw.Bits >> 3);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Flac__BitWriter_Release_Buffer()
		{
			// Nothing to do. In the future, strict checking of a 'writer-is-in-
			// get-mode' flag could be added everywhere and then cleared here
		}
		#endregion

		#region Write methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Flac__bool Flac__BitWriter_Write_Zeroes(uint32_t bits)
		{
			Debug.Assert(bw != null);
			Debug.Assert(bw.Buffer != null);

			if (bits == 0)
				return true;

			// Slightly pessimistic size check but faster than "<= bw.Words + (bw.Bits+bits+FLAC__BITS_PER_WORD-1)/FLAC__BITS_PER_WORD"
			if ((bw.Capacity <= (bw.Words + bits)) && !BitWriter_Grow(bits))
				return false;

			// First part gets to word alignment
			if (bw.Bits != 0)
			{
				uint32_t n = Math.Min(Constants.Flac__Bits_Per_Word - bw.Bits, bits);
				bw.Accum <<= (int)n;
				bits -= n;
				bw.Bits += n;

				if (bw.Bits == Constants.Flac__Bits_Per_Word)
				{
					bw.Buffer[bw.Words++] = Swap_Be_Word_To_Host(bw.Accum);
					bw.Bits = 0;
				}
				else
					return true;
			}

			// Do whole words
			while (bits >= Constants.Flac__Bits_Per_Word)
			{
				bw.Buffer[bw.Words++] = 0;
				bits -= Constants.Flac__Bits_Per_Word;
			}

			// Do any leftovers
			if (bits > 0)
			{
				bw.Accum = 0;
				bw.Bits = bits;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Flac__bool Flac__BitWriter_Write_Raw_UInt32(Flac__uint32 val, uint32_t bits)
		{
			// Check that unused bits are unset
			if ((bits < 32) && ((val >> (int)bits) != 0))
				return false;

			return Flac__BitWriter_Write_Raw_UInt32_NoCheck(val, bits);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Flac__bool Flac__BitWriter_Write_Raw_Int32(Flac__int32 val, uint32_t bits)
		{
			// Zero-out unused bits
			if (bits < 32)
				val &= (Flac__int32)(~(0xffffffff << (int)bits));

			return Flac__BitWriter_Write_Raw_UInt32_NoCheck((Flac__uint32)val, bits);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Flac__bool Flac__BitWriter_Write_Raw_UInt64(Flac__uint64 val, uint32_t bits)
		{
			// This could be a little faster but it's not used for much
			if (bits > 32)
				return Flac__BitWriter_Write_Raw_UInt32((Flac__uint32)(val >> 32), bits - 32) && Flac__BitWriter_Write_Raw_UInt32_NoCheck((Flac__uint32)val, 32);
			else
				return Flac__BitWriter_Write_Raw_UInt32((Flac__uint32)val, bits);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Flac__bool Flac__BitWriter_Write_Raw_Int64(Flac__int64 val, uint32_t bits)
		{
			Flac__uint64 uVal = (Flac__uint64)val;

			// Zero out unused bits
			if (bits < 64)
				uVal &= (~(uint64_t.MaxValue << (int)bits));

			return Flac__BitWriter_Write_Raw_UInt64(uVal, bits);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Flac__bool Flac__BitWriter_Write_Raw_UInt32_Little_Endian(Flac__uint32 val)
		{
			// This doesn't need to be that fast as currently it is only used for vorbis comments

			if (!Flac__BitWriter_Write_Raw_UInt32_NoCheck(val & 0xff, 8))
				return false;

			if (!Flac__BitWriter_Write_Raw_UInt32_NoCheck((val >> 8) & 0xff, 8))
				return false;

			if (!Flac__BitWriter_Write_Raw_UInt32_NoCheck((val >> 16) & 0xff, 8))
				return false;

			if (!Flac__BitWriter_Write_Raw_UInt32_NoCheck((val >> 24) & 0xff, 8))
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Flac__bool Flac__BitWriter_Write_Byte_Block(Flac__byte[] vals, uint32_t nVals)
		{
			// Grow capacity upfront to prevent constant reallocation during writes
			if ((bw.Capacity <= bw.Words + nVals / (Constants.Flac__Bits_Per_Word / 8) + 1) && !BitWriter_Grow(nVals * 8))
				return false;

			// This could be faster but currently we don't need it to be since it's only used for writing metadata
			for (uint32_t i = 0; i < nVals; i++)
			{
				if (!Flac__BitWriter_Write_Raw_UInt32_NoCheck(vals[i], 8))
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__BitWriter_Write_Unary_Unsigned(uint32_t val)
		{
			if (val < 32)
				return Flac__BitWriter_Write_Raw_UInt32_NoCheck(1, ++val);
			else
				return Flac__BitWriter_Write_Zeroes(val) && Flac__BitWriter_Write_Raw_UInt32_NoCheck(1, 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__BitWriter_Write_Rice_Signed_Block(Flac__int32[] vals, uint32_t offset, uint32_t nVals, uint32_t parameter)
		{
			Flac__uint32 mask1 = 0xffffffff << (int)parameter;			// We val|=mask1 to set the stop bit above it
			Flac__uint32 mask2 = 0xffffffff >> (int)(31 - parameter);	// Then mask off the bits above the stop bit with val&=mask2
			uint32_t lsBits = 1 + parameter;
			Flac__bwTemp wide_Accum = 0;
			Flac__uint32 bitPointer = Constants.Flac__Temp_Bits;

			Debug.Assert(bw != null);
			Debug.Assert(bw.Buffer != null);
			Debug.Assert(parameter < 31);

			// WATCHOUT: Code does not work with <32bit words; w can make things much faster with this assertion
			Debug.Assert(Constants.Flac__Bits_Per_Word >= 32);

			if ((bw.Bits > 0) && (bw.Bits < Constants.Flac__Half_Temp_Bits))
			{
				bitPointer -= bw.Bits;
				wide_Accum = bw.Accum << (int)bitPointer;
				bw.Bits = 0;
			}
			else if (bw.Bits > Constants.Flac__Half_Temp_Bits)
			{
				bitPointer -= (bw.Bits - Constants.Flac__Half_Temp_Bits);
				wide_Accum = bw.Accum << (int)bitPointer;
				bw.Accum >>= (int)(bw.Bits - Constants.Flac__Half_Temp_Bits);
				bw.Bits = Constants.Flac__Half_Temp_Bits;
			}

			// Reserve one FLAC__TEMP_BITS per symbol, so checks for space are only necessary when very large symbols are encountered.
			// This might be considered wasteful, but is only at most 8kB more than necessary for a blocksize of 4096
			if (((bw.Capacity * Constants.Flac__Bits_Per_Word) <= (bw.Words * Constants.Flac__Bits_Per_Word + nVals * Constants.Flac__Temp_Bits + bw.Bits)) && !BitWriter_Grow(nVals * Constants.Flac__Temp_Bits))
				return false;

			while (nVals != 0)
			{
				// Fold signed to uint32_t; actual formula is: negative(v)? -2v-1 : 2v
				Flac__uint32 uVal = (Flac__uint32)vals[offset];
				uVal <<= 1;
				uVal ^= (Flac__uint32)(vals[offset] >> 31);

				uint32_t msBits = uVal >> (int)parameter;
				uint32_t total_Bits = lsBits + msBits;

				uVal |= mask1;	// Set stop bit
				uVal &= mask2;	// Mask off unused top bits

				if (total_Bits <= bitPointer)
				{
					// There is room enough to store the symbol whole at once
					wide_Accum |= (Flac__bwTemp)uVal << (int)(bitPointer - total_Bits);
					bitPointer -= total_Bits;

					if (bitPointer <= Constants.Flac__Half_Temp_Bits)
					{
						// A word is finished, copy the upper 32 bits of the wide_accum
						Wide_Accum_To_Bw(ref wide_Accum, ref bitPointer);
					}
				}
				else
				{
					// The symbol needs to be split. This code isn't used often.
					// First check for space in the bitwriter
					if (total_Bits > Constants.Flac__Temp_Bits)
					{
						Flac__uint32 oversize_In_Bits = total_Bits - Constants.Flac__Temp_Bits;
						Flac__uint32 capacity_Needed = bw.Words * Constants.Flac__Bits_Per_Word + bw.Bits + nVals * Constants.Flac__Temp_Bits + oversize_In_Bits;

						if (((bw.Capacity * Constants.Flac__Bits_Per_Word) <= capacity_Needed) && !BitWriter_Grow(nVals * Constants.Flac__Temp_Bits + oversize_In_Bits))
							return false;
					}

					if (msBits > bitPointer)
					{
						// We have a lot of 0 bits to write, first align with bitwriter word
						msBits -= bitPointer - Constants.Flac__Half_Temp_Bits;
						bitPointer = Constants.Flac__Half_Temp_Bits;
						Wide_Accum_To_Bw(ref wide_Accum, ref bitPointer);

						while (msBits > bitPointer)
						{
							// As the accumulator is already zero, we only need to
							// assign zeroes to the bitbuffer
							Wide_Accum_To_Bw(ref wide_Accum, ref bitPointer);
							bitPointer -= Constants.Flac__Half_Temp_Bits;
							msBits -= Constants.Flac__Half_Temp_Bits;
						}

						// The remaining bits are zero, and the accumulator already is zero,
						// so just subtract the number of bits from bitpointer. When storing,
						// we can also just store 0
						bitPointer -= msBits;

						if (bitPointer <= Constants.Flac__Half_Temp_Bits)
							Wide_Accum_To_Bw(ref wide_Accum, ref bitPointer);
					}
					else
					{
						bitPointer -= msBits;

						if (bitPointer <= Constants.Flac__Half_Temp_Bits)
							Wide_Accum_To_Bw(ref wide_Accum, ref bitPointer);
					}

					// The lsbs + stop bit always fit 32 bit, so this code mirrors the code above
					wide_Accum |= (Flac__bwTemp)uVal << (int)(bitPointer - lsBits);
					bitPointer -= lsBits;

					if (bitPointer <= Constants.Flac__Half_Temp_Bits)
					{
						// A word is finished, copy the upper 32 bits of the wide_accum
						Wide_Accum_To_Bw(ref wide_Accum, ref bitPointer);
					}
				}

				offset++;
				nVals--;
			}

			// Now fixup remainder of wide_accum
			if (bitPointer < Constants.Flac__Temp_Bits)
			{
				if (bw.Bits == 0)
				{
					bw.Accum = wide_Accum >> (int)bitPointer;
					bw.Bits = Constants.Flac__Temp_Bits - bitPointer;
				}
				else if (bw.Bits == Constants.Flac__Half_Temp_Bits)
				{
					bw.Accum <<= (int)(Constants.Flac__Temp_Bits - bitPointer);
					bw.Accum |= (wide_Accum >> (int)bitPointer);
					bw.Bits = Constants.Flac__Half_Temp_Bits + Constants.Flac__Temp_Bits - bitPointer;
				}
				else
					Debug.Assert(false);
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__BitWriter_Write_Utf8_UInt32(Flac__uint32 val)
		{
			Debug.Assert(bw != null);
			Debug.Assert(bw.Buffer != null);

			if ((val & 0x80000000) != 0)	// This version only handles 31 bits
				return false;

			Flac__bool ok = true;

			if (val < 0x80)
				return Flac__BitWriter_Write_Raw_UInt32(val, 8);
			else if (val < 0x800)
			{
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0xc0 | (val >> 6), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | (val & 0x3f), 8);
			}
			else if (val < 0x10000)
			{
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0xe0 | (val >> 12), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | ((val >> 6) & 0x3f), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | (val & 0x3f), 8);
			}
			else if (val < 0x200000)
			{
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0xf0 | (val >> 18), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | ((val >> 12) & 0x3f), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | ((val >> 6) & 0x3f), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | (val & 0x3f), 8);
			}
			else if (val < 0x4000000)
			{
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0xf8 | (val >> 24), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | ((val >> 18) & 0x3f), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | ((val >> 12) & 0x3f), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | ((val >> 6) & 0x3f), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | (val & 0x3f), 8);
			}
			else
			{
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0xfc | (val >> 30), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | ((val >> 24) & 0x3f), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | ((val >> 18) & 0x3f), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | ((val >> 12) & 0x3f), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | ((val >> 6) & 0x3f), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | (val & 0x3f), 8);
			}

			return ok;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__BitWriter_Write_Utf8_UInt64(Flac__uint64 val)
		{
			Debug.Assert(bw != null);
			Debug.Assert(bw.Buffer != null);

			if ((val & 0xfffffff000000000) != 0)	// This version only handles 36 bits
				return false;

			Flac__bool ok = true;

			if (val < 0x80)
				return Flac__BitWriter_Write_Raw_UInt32((Flac__uint32)val, 8);
			else if (val < 0x800)
			{
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0xc0 | (Flac__uint32)(val >> 6), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | (Flac__uint32)(val & 0x3f), 8);
			}
			else if (val < 0x10000)
			{
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0xe0 | (Flac__uint32)(val >> 12), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | (Flac__uint32)((val >> 6) & 0x3f), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | (Flac__uint32)(val & 0x3f), 8);
			}
			else if (val < 0x200000)
			{
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0xf0 | (Flac__uint32)(val >> 18), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | (Flac__uint32)((val >> 12) & 0x3f), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | (Flac__uint32)((val >> 6) & 0x3f), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | (Flac__uint32)(val & 0x3f), 8);
			}
			else if (val < 0x4000000)
			{
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0xf8 | (Flac__uint32)(val >> 24), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | (Flac__uint32)((val >> 18) & 0x3f), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | (Flac__uint32)((val >> 12) & 0x3f), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | (Flac__uint32)((val >> 6) & 0x3f), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | (Flac__uint32)(val & 0x3f), 8);
			}
			else if (val < 0x80000000)
			{
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0xfc | (Flac__uint32)(val >> 30), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | (Flac__uint32)((val >> 24) & 0x3f), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | (Flac__uint32)((val >> 18) & 0x3f), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | (Flac__uint32)((val >> 12) & 0x3f), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | (Flac__uint32)((val >> 6) & 0x3f), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | (Flac__uint32)(val & 0x3f), 8);
			}
			else
			{
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0xfe, 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | (Flac__uint32)((val >> 30) & 0x3f), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | (Flac__uint32)((val >> 24) & 0x3f), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | (Flac__uint32)((val >> 18) & 0x3f), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | (Flac__uint32)((val >> 12) & 0x3f), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | (Flac__uint32)((val >> 6) & 0x3f), 8);
				ok &= Flac__BitWriter_Write_Raw_UInt32_NoCheck(0x80 | (Flac__uint32)(val & 0x3f), 8);
			}

			return ok;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__BitWriter_Zero_Pad_To_Byte_Boundary()
		{
			// 0-pad to byte boundary
			if ((bw.Bits & 7) != 0)
				return Flac__BitWriter_Write_Zeroes(8 - (bw.Bits & 7));
			else
				return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Flac__uint64 Swap_Be_Word_To_Host(Flac__uint64 x)
		{
			if (BitConverter.IsLittleEndian)
				return EndSwap.EndSwap_64(x);

			return x;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Wide_Accum_To_Bw(ref Flac__bwTemp wide_Accum, ref Flac__uint32 bitPointer)
		{
			Debug.Assert((bw.Bits % Constants.Flac__Half_Temp_Bits) == 0);

			if (bw.Bits == 0)
			{
				bw.Accum = wide_Accum >> (int)Constants.Flac__Half_Temp_Bits;
				wide_Accum <<= (int)Constants.Flac__Half_Temp_Bits;
				bw.Bits = Constants.Flac__Half_Temp_Bits;
			}
			else
			{
				bw.Accum <<= (int)Constants.Flac__Half_Temp_Bits;
				bw.Accum += wide_Accum >> (int)Constants.Flac__Half_Temp_Bits;
				bw.Buffer[bw.Words++] = Swap_Be_Word_To_Host(bw.Accum);
				wide_Accum <<= (int)Constants.Flac__Half_Temp_Bits;
				bw.Bits = 0;
			}

			bitPointer += Constants.Flac__Half_Temp_Bits;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool BitWriter_Grow(uint32_t bits_To_Add)
		{
			Debug.Assert(bw != null);
			Debug.Assert(bw.Buffer != null);

			// Calculate total words needed to store 'bits_To_Add' additional bits
			uint32_t new_Capacity = bw.Words + ((bw.Bits + bits_To_Add + Constants.Flac__Bits_Per_Word - 1) / Constants.Flac__Bits_Per_Word);

			// It's possible (due to pessimism in the growth estimation that
			// leads to this call) that we don't actually need to grow
			if (bw.Capacity >= new_Capacity)
				return true;

			if (new_Capacity * sizeof(bwWord) > (1U << (int)Constants.Flac__Stream_Metadata_Length_Len))
			{
				// Requested new capacity is larger than the largest possible metadata block,
				// which us also larger than the largest sane framesize. That means something
				// went very wrong somewhere and previous checks failed.
				// To prevent crashing, give up
				return false;
			}

			// As reallocation can be quite expensive, grow exponentially
			if ((new_Capacity - bw.Capacity) < (bw.Capacity >> Flac__BitWriter_Default_Grow_Fraction))
				new_Capacity = bw.Capacity + (bw.Capacity >> Flac__BitWriter_Default_Grow_Fraction);

			// Make sure we got everything right
			Debug.Assert(new_Capacity > bw.Capacity);
			Debug.Assert(new_Capacity >= bw.Words + ((bw.Bits + bits_To_Add + Constants.Flac__Bits_Per_Word - 1) / Constants.Flac__Bits_Per_Word));

			bwWord[] new_Buffer = Alloc.Safe_Realloc_NoFree_Mul_2Op(bw.Buffer, 1, new_Capacity);
			if (new_Buffer == null)
				return false;

			bw.Buffer = new_Buffer;
			bw.Capacity = new_Capacity;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Flac__bool Flac__BitWriter_Write_Raw_UInt32_NoCheck(Flac__uint32 val, uint32_t bits)
		{
			// WATCHOUT: Code does not work with <32bit words; we can make things much faster with this assertion
			Debug.Assert(Constants.Flac__Bits_Per_Word >= 32);

			if ((bw == null) || (bw.Buffer == null))
				return false;

			if (bits > 32)
				return false;

			if (bits == 0)
				return true;

			Debug.Assert((bits == 32) || ((val >> (int)bits) == 0));

			// Slightly pessimistic size check but faster than "<= bw.Words + (bw.Bits+bits+FLAC__BITS_PER_WORD-1)/FLAC__BITS_PER_WORD"
			if ((bw.Capacity <= (bw.Words + bits)) && !BitWriter_Grow(bits))
				return false;

			uint32_t left = Constants.Flac__Bits_Per_Word - bw.Bits;
			if (bits < left)
			{
				bw.Accum <<= (int)bits;
				bw.Accum |= val;
				bw.Bits += bits;
			}
			else if (bw.Bits != 0)	// WATCHOUT: if bw.Bits == 0, left == FLAC__BITS_PER_WORD and bw.Accum<<=left is a NOP instead of setting to 0
			{
				bw.Accum <<= (int)left;
				bw.Accum |= val >> (int)(bw.Bits = bits - left);
				bw.Buffer[bw.Words++] = Swap_Be_Word_To_Host(bw.Accum);
				bw.Accum = val;		// Unused top bits can contain garbage
			}
			else	// At this point bits == FLAC__BITS_PER_WORD == 32 and bw.Bits == 0
				bw.Buffer[bw.Words++] = Swap_Be_Word_To_Host(val);

			return true;
		}
		#endregion
	}
}
