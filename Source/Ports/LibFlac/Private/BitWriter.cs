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
		/// When growing, increment 4K at a time
		/// </summary>
		private const uint32_t Flac__BitWriter_Default_Increment = 4096 / sizeof(bwWord);	// Size in words

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

			crc = Crc.Flac__Crc16(buffer, bytes);
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

			crc = Crc.Flac__Crc8(buffer, bytes);
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

			Debug.Assert(bw != null);
			Debug.Assert(bw.Buffer != null);
			Debug.Assert(parameter < 31);

			// WATCHOUT: Code does not work with <32bit words; w can make things much faster with this assertion
			Debug.Assert(Constants.Flac__Bits_Per_Word >= 32);

			while (nVals != 0)
			{
				// Fold signed to uint32_t; actual formula is: negative(v)? -2v-1 : 2v
				Flac__uint32 uVal = (Flac__uint32)vals[offset];
				uVal <<= 1;
				uVal ^= (Flac__uint32)(vals[offset] >> 31);

				uint32_t msBits = uVal >> (int)parameter;
				uint32_t total_Bits = lsBits + msBits;

				if ((bw.Bits != 0) && ((bw.Bits + total_Bits) < Constants.Flac__Bits_Per_Word))	// I.e. if the whole thing fits in the current bwWord
				{
					// ^^^ if bw.Bits is 0 then we may have filled the buffer and have no free bwWord to work in
					bw.Bits += total_Bits;

					uVal |= mask1;	// Set stop bit
					uVal &= mask2;	// Mask off unused top bits

					bw.Accum <<= (int)total_Bits;
					bw.Accum |= uVal;
				}
				else
				{
					// Slightly pessimistic size check but faster than ""<= bw->words + (bw->bits+msbits+lsbits+FLAC__BITS_PER_WORD-1)/FLAC__BITS_PER_WORD"
					// OPT: pessimism may cause flurry of false calls to grow_ which eat up all savings before it
					if ((bw.Capacity <= (bw.Words + bw.Bits + msBits + 1 /* lsBits always fit in 1 bwWord */)) && !BitWriter_Grow(total_Bits))
						return false;

					uint32_t left;

					if (msBits != 0)
					{
						// First part gets to word alignment
						if (bw.Bits != 0)
						{
							left = Constants.Flac__Bits_Per_Word - bw.Bits;
							if (msBits < left)
							{
								bw.Accum <<= (int)msBits;
								bw.Bits += msBits;
								goto break1;
							}
							else
							{
								bw.Accum <<= (int)left;
								msBits -= left;
								bw.Buffer[bw.Words++] = Swap_Be_Word_To_Host(bw.Accum);
								bw.Bits = 0;
							}
						}

						// Do whole words
						while (msBits >= Constants.Flac__Bits_Per_Word)
						{
							bw.Buffer[bw.Words++] = 0;
							msBits -= Constants.Flac__Bits_Per_Word;
						}

						// Do any leftovers
						if (msBits > 0)
						{
							bw.Accum = 0;
							bw.Bits = msBits;
						}
					}

break1:
					uVal |= mask1;	// Set stop bit
					uVal &= mask2;	// Mask off unused top bits

					left = Constants.Flac__Bits_Per_Word - bw.Bits;

					if (lsBits < left)
					{
						bw.Accum <<= (int)lsBits;
						bw.Accum |= uVal;
						bw.Bits += lsBits;
					}
					else
					{
						// If bw.Bits == 0, left==FLAC__BITS_PER_WORD which will always
						// be > lsBits (because of previous assertions) so it would have
						// triggered the (lsBits<left) case above
						Debug.Assert(bw.Bits != 0);
						Debug.Assert(left < Constants.Flac__Bits_Per_Word);

						bw.Accum <<= (int)left;
						bw.Accum |= uVal >> (int)(bw.Bits = lsBits - left);
						bw.Buffer[bw.Words++] = Swap_Be_Word_To_Host(bw.Accum);
						bw.Accum = uVal;	// Unused top bits can contain garbage
					}
				}

				offset++;
				nVals--;
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

			// Round up capacity increase to the nearest FLAC__BITWRITER_DEFAULT_INCREMENT
			if (((new_Capacity - bw.Capacity) % Flac__BitWriter_Default_Increment) != 0)
				new_Capacity += Flac__BitWriter_Default_Increment - ((new_Capacity - bw.Capacity) % Flac__BitWriter_Default_Increment);

			// Make sure we got everything right
			Debug.Assert((new_Capacity - bw.Capacity) % Flac__BitWriter_Default_Increment == 0);
			Debug.Assert(new_Capacity > bw.Capacity);
			Debug.Assert(new_Capacity >= bw.Words + ((bw.Bits + bits_To_Add + Constants.Flac__Bits_Per_Word - 1) / Constants.Flac__Bits_Per_Word));

			bwWord[] new_Buffer = Alloc.Safe_Realloc_Mul_2Op(bw.Buffer, 1, new_Capacity);
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
