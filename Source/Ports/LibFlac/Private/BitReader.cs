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
using brWord = System.UInt64;

namespace Polycode.NostalgicPlayer.Ports.LibFlac.Private
{
	/// <summary>
	/// 
	/// </summary>
	internal class BitReader : IBitReader
	{
		// This should be at least twice as large as the largest number of words
		// required to represent any 'number' (in any encoding) you are going to
		// read. With FLAC this is on the order of maybe a few hundred bits.
		// If the buffer is smaller than that, the decoder won't be able to read
		// in a whole number that is in a variable length encoding (e.g. Rice).
		// But to be practical it should be at least 1K bytes.
		//
		// Increase this number to decrease the number of read callbacks, at the
		// expense of using more memory. Or decrease for the reverse effect,
		// keeping in mind the limit from the first paragraph. The optimal size
		// also depends on the CPU cache size and other factors; some twiddling
		// may be necessary to squeeze out the best performance
		private const uint32_t Flac__BitReader_Default_Capacity = 65536 / Constants.Flac__Bits_Per_Word;	// In words

		private class Flac__BitReader
		{
			// Any partially-consumed word at the head will stay right-justified as bits are consumed from the left.
			// Any incomplete word at the tail will be left-justified, and bytes from the read callback are added on the right
			public brWord[] Buffer;
			public uint32_t Capacity;		// In words
			public uint32_t Words;			// # of completed words in buffer
			public uint32_t Bytes;			// # of bytes in incomplete word at buffer[words]
			public uint32_t Consumed_Words;	// #words ...
			public uint32_t Consumed_Bits;	// ... + (#bits of head word) already consumed from the front of buffer
			public uint32_t Read_Crc16;		// The running frame CRC
			public uint32_t Crc16_Offset;	// The number of words in the current buffer that should not be CRC'd
			public uint32_t Crc16_Align;	// The number of bits in the current consumed word that should not be CRC'd
			public Flac__bool Read_Limit_Set;	// Whether reads are limited
			public uint32_t Read_Limit;		// The remaining size of what can be read
			public uint32_t Last_Seen_Framesync;	// The location of the last seen framesync, if it is in the buffer, in bits from front of buffer
			public Flac__BitReaderReadCallback Read_Callback;
			public object Client_Data;
		}

		private Flac__BitReader br;

		public delegate Flac__bool Flac__BitReaderReadCallback(Span<Flac__byte> buffer, ref size_t bytes, object client_Data);

		#region Construction, deletion, initialization, etc methods
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private BitReader()
		{
			br = new Flac__BitReader();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static BitReader Flac__BitReader_New()
		{
			return new BitReader();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Flac__BitReader_Delete()
		{
			Debug.Assert(br != null);

			Flac__BitReader_Free();
			br = null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__BitReader_Init(Flac__BitReaderReadCallback rcb, object cd)
		{
			Debug.Assert(br != null);

			br.Words = br.Bytes = 0;
			br.Consumed_Words = br.Consumed_Bits = 0;
			br.Capacity = Flac__BitReader_Default_Capacity;
			br.Buffer = new brWord[br.Capacity];
			br.Read_Callback = rcb;
			br.Client_Data = cd;
			br.Read_Limit_Set = false;
			br.Read_Limit = uint32_t.MaxValue;
			br.Last_Seen_Framesync = uint32_t.MaxValue;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Flac__BitReader_Free()
		{
			Debug.Assert(br != null);

			br.Buffer = null;
			br.Capacity = 0;
			br.Words = br.Bytes = 0;
			br.Consumed_Words = br.Consumed_Bits = 0;
			br.Read_Callback = null;
			br.Client_Data = null;
			br.Read_Limit_Set = false;
			br.Read_Limit = uint32_t.MaxValue;
			br.Last_Seen_Framesync = uint32_t.MaxValue;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__BitReader_Clear()
		{
			br.Words = br.Bytes = 0;
			br.Consumed_Words = br.Consumed_Bits = 0;
			br.Read_Limit_Set = false;
			br.Read_Limit = uint32_t.MaxValue;
			br.Last_Seen_Framesync = uint32_t.MaxValue;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Flac__BitReader_Set_Framesync_Location()
		{
			br.Last_Seen_Framesync = br.Consumed_Words * Constants.Flac__Bytes_Per_Word + br.Consumed_Bits / 8;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__BitReader_Rewind_To_After_Last_Seen_Framesync()
		{
			if (br.Last_Seen_Framesync == uint32_t.MaxValue)
			{
				br.Consumed_Words = br.Consumed_Bits = 0;

				return false;
			}
			else
			{
				br.Consumed_Words = (br.Last_Seen_Framesync + 1) / Constants.Flac__Bytes_Per_Word;
				br.Consumed_Bits = ((br.Last_Seen_Framesync + 1) % Constants.Flac__Bytes_Per_Word) * 8;

				return true;
			}
		}
		#endregion

		#region CRC methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Flac__BitReader_Reset_Read_Crc16(Flac__uint16 seed)
		{
			Debug.Assert(br != null);
			Debug.Assert(br.Buffer != null);
			Debug.Assert((br.Consumed_Bits & 7) == 0);

			br.Read_Crc16 = seed;
			br.Crc16_Offset = br.Consumed_Words;
			br.Crc16_Align = br.Consumed_Bits;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Flac__uint16 Flac__BitReader_Get_Read_Crc16()
		{
			Debug.Assert(br != null);
			Debug.Assert(br.Buffer != null);

			// CRC consumed words up to here
			Crc16_Update_Block();

			Debug.Assert((br.Consumed_Bits & 7) == 0);
			Debug.Assert(br.Crc16_Align <= br.Consumed_Bits);

			// CRC any tail bytes in a partially-consumed word
			if (br.Consumed_Bits != 0)
			{
				brWord tail = br.Buffer[br.Consumed_Words];

				for (; br.Crc16_Align < br.Consumed_Bits; br.Crc16_Align += 8)
					br.Read_Crc16 = Crc.Flac__Crc16_Update((Flac__byte)((tail >> (Flac__int32)(Constants.Flac__Bits_Per_Word - 8 - br.Crc16_Align)) & 0xff), (Flac__uint16)br.Read_Crc16);
			}

			return (Flac__uint16)br.Read_Crc16;
		}
		#endregion

		#region Info methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Flac__bool Flac__BitReader_Is_Consumed_Byte_Aligned()
		{
			return (br.Consumed_Bits & 7) == 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint32_t Flac__BitReader_Bits_Left_For_Byte_Alignment()
		{
			return 8 - (br.Consumed_Bits & 7);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint32_t Flac__BitReader_Get_Input_Bits_Unconsumed()
		{
			return (br.Words - br.Consumed_Words) * Constants.Flac__Bits_Per_Word + br.Bytes * 8 - br.Consumed_Bits;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Flac__BitReader_Set_Limit(uint32_t limit)
		{
			br.Read_Limit = limit;
			br.Read_Limit_Set = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Flac__BitReader_Remove_Limit()
		{
			br.Read_Limit_Set = false;
			br.Read_Limit = uint32_t.MaxValue;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint32_t Flac__BitReader_Limit_Remaining()
		{
			Debug.Assert(br.Read_Limit_Set);

			return br.Read_Limit;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Flac__BitReader_Limit_Invalidate()
		{
			br.Read_Limit = uint32_t.MaxValue;
		}
		#endregion

		#region Read methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__BitReader_Read_Raw_UInt32(out Flac__uint32 val, uint32_t bits)
		{
			Debug.Assert(br != null);
			Debug.Assert(br.Buffer != null);

			Debug.Assert(bits <= 32);
			Debug.Assert((br.Capacity * Constants.Flac__Bits_Per_Word) * 2 >= bits);
			Debug.Assert(br.Consumed_Words <= br.Words);

			// WATCHOUT: Code does not work with <32bit words; we can make things much faster with this assertion
			Debug.Assert(Constants.Flac__Bits_Per_Word >= 32);

			if (bits == 0)	// OPT: Investigate if this can ever happen, maybe change to assertion
			{
				val = 0;
				return true;
			}

			if (br.Read_Limit_Set && (br.Read_Limit < uint32_t.MaxValue))
			{
				if (br.Read_Limit < bits)
				{
					br.Read_Limit = uint32_t.MaxValue;
					val = 0;
					return false;
				}
				else
					br.Read_Limit -= bits;
			}

			while (((br.Words - br.Consumed_Words) * Constants.Flac__Bits_Per_Word + br.Bytes * 8 - br.Consumed_Bits) < bits)
			{
				if (!BitReader_Read_From_Client())
				{
					val = 0;
					return false;
				}
			}

			if (br.Consumed_Words < br.Words)	// If we've not consumed up to a partial tail word...
			{
				// OPT: Taking out the consumed_Bits==0 "else" case below might make things faster if less code allows the compiler to inline this method
				if (br.Consumed_Bits != 0)
				{
					// This also works when consumed_Bits==0, it's just a little slower than necessary for that case
					uint32_t n = Constants.Flac__Bits_Per_Word - br.Consumed_Bits;
					brWord word = br.Buffer[br.Consumed_Words];
					brWord mask = br.Consumed_Bits < Constants.Flac__Bits_Per_Word ? Constants.Flac__Word_All_Ones >> (int32_t)br.Consumed_Bits : 0;

					if (bits < n)
					{
						uint32_t shift = n - bits;
						val = shift < Constants.Flac__Bits_Per_Word ? (Flac__uint32)((word & mask) >> (int32_t)shift) : 0;	// The result has <= 32 non-zero bits
						br.Consumed_Bits += bits;

						return true;
					}

					// (Flac__Bits_Per_Word - br.consumed_Bits <= bits) ==> (Flac__Word_All_Ones >> br.consumed_Bits) has no more than 'bits' non-zero bits
					val = (Flac__uint32)(word & mask);
					bits -= n;
					br.Consumed_Words++;
					br.Consumed_Bits = 0;

					if (bits != 0)	// If there are still bits left to read, there have to be less than 32 so they will all be in the next word
					{
						uint32_t shift = Constants.Flac__Bits_Per_Word - bits;
						val = bits < 32 ? val << (int32_t)bits : 0;
						val |= shift < Constants.Flac__Bits_Per_Word ? (Flac__uint32)(br.Buffer[br.Consumed_Words] >> (int32_t)shift) : 0;
						br.Consumed_Bits = bits;
					}

					return true;
				}
				else	// br.consumed_Bits == 0
				{
					brWord word = br.Buffer[br.Consumed_Words];

					if (bits < Constants.Flac__Bits_Per_Word)
					{
						val = (Flac__uint32)(word >> (int32_t)(Constants.Flac__Bits_Per_Word - bits));
						br.Consumed_Bits = bits;

						return true;
					}

					// At this point bits == Flac__Bits_Per_Word == 32; because of previous assertions, it can't be larger
					val = (Flac__uint32)word;
					br.Consumed_Words++;

					return true;
				}
			}
			else
			{
				// In this case we're starting our read at a partial tail word;
				// the reader has guaranteed that we have at least 'bits' bits
				// available to read, which makes this case simpler.
				//
				// OPT: Taking out the consumed bits==0 "else" case below might make things faster if less code allows the compiler to inline this method
				if (br.Consumed_Bits != 0)
				{
					// This also works when consumed_Bits==0, it's just a little slower than necessary for that case
					Debug.Assert(br.Consumed_Bits + bits <= br.Bytes * 8);

					val = (Flac__uint32)((br.Buffer[br.Consumed_Words] & (Constants.Flac__Word_All_Ones >> (int32_t)br.Consumed_Bits)) >> (int32_t)(Constants.Flac__Bits_Per_Word - br.Consumed_Bits - bits));
					br.Consumed_Bits += bits;

					return true;
				}
				else
				{
					val = (Flac__uint32)(br.Buffer[br.Consumed_Words] >> (int32_t)(Constants.Flac__Bits_Per_Word - bits));
					br.Consumed_Bits += bits;

					return true;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__BitReader_Read_Raw_Int32(out Flac__int32 val, uint32_t bits)
		{
			if ((bits < 1) || !Flac__BitReader_Read_Raw_UInt32(out Flac__uint32 uVal, bits))
			{
				val = 0;
				return false;
			}

			// Sign-extend *val assuming it is currently bits wide
			// From: https://graphics.stanford.edu/~seander/bithacks.html#FixedSignExtend
			Flac__uint32 mask = bits >= 33 ? 0 : 1U << ((int)bits - 1);
			val = (Flac__int32)((uVal ^ mask) - mask);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__BitReader_Read_Raw_UInt64(out Flac__uint64 val, uint32_t bits)
		{
			val = 0;

			if (bits > 32)
			{
				if (!Flac__BitReader_Read_Raw_UInt32(out Flac__uint32 hi, bits - 32))
					return false;

				if (!Flac__BitReader_Read_Raw_UInt32(out Flac__uint32 lo, 32))
					return false;

				val = hi;
				val <<= 32;
				val |= lo;
			}
			else
			{
				if (!Flac__BitReader_Read_Raw_UInt32(out Flac__uint32 lo, bits))
					return false;

				val = lo;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__BitReader_Read_Raw_Int64(out Flac__int64 val, uint32_t bits)
		{
			val = 0;

			if ((bits < 1) || !Flac__BitReader_Read_Raw_UInt64(out Flac__uint64 uVal, bits))
				return false;

			// Sign-extend *val assuming it is currently bits wide
			// From: https://graphics.stanford.edu/~seander/bithacks.html#FixedSignExtend
			Flac__uint64 mask = bits >= 65 ? 0 : 1LU << (int)(bits - 1);
			val = (Flac__int64)((uVal ^ mask) - mask);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Flac__bool Flac__BitReader_Read_UInt32_Little_Endian(out Flac__uint32 val)
		{
			val = 0;

			// This doesn't need to be that fast as currently it is only used for vorbis comments
			if (!Flac__BitReader_Read_Raw_UInt32(out Flac__uint32 x32, 8))
				return false;

			if (!Flac__BitReader_Read_Raw_UInt32(out Flac__uint32 x8, 8))
				return false;

			x32 |= (x8 << 8);

			if (!Flac__BitReader_Read_Raw_UInt32(out x8, 8))
				return false;

			x32 |= (x8 << 16);

			if (!Flac__BitReader_Read_Raw_UInt32(out x8, 8))
				return false;

			x32 |= (x8 << 24);

			val = x32;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__BitReader_Skip_Bits_No_Crc(uint32_t bits)
		{
			// OPT: A faster implementation is possible but probably not that useful
			// since this is only called a couple of times in the metadata readers
			Debug.Assert(br != null);
			Debug.Assert(br.Buffer != null);

			if (bits > 0)
			{
				uint32_t n = br.Consumed_Bits & 7;
				uint32_t m;

				if (n != 0)
				{
					m = Math.Min(8 - n, bits);
					if (!Flac__BitReader_Read_Raw_UInt32(out _, m))
						return false;

					bits -= m;
				}

				m = bits / 8;
				if (m > 0)
				{
					if (!Flac__BitReader_Skip_Byte_Block_Aligned_No_Crc(m))
						return false;

					bits %= 8;
				}

				if (bits > 0)
				{
					if (!Flac__BitReader_Read_Raw_UInt32(out _, bits))
						return false;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__BitReader_Skip_Byte_Block_Aligned_No_Crc(uint32_t nVals)
		{
			Debug.Assert(br != null);
			Debug.Assert(br.Buffer != null);
			Debug.Assert(Flac__BitReader_Is_Consumed_Byte_Aligned());

			if (br.Read_Limit_Set && (br.Read_Limit < uint32_t.MaxValue))
			{
				if (br.Read_Limit < nVals * 8)
				{
					br.Read_Limit = uint32_t.MaxValue;
					return false;
				}
			}

			// Step 1: Skip over partial head word to get word aligned
			while ((nVals != 0) && (br.Consumed_Bits != 0))		// I.e. run until we read 'nVals' bytes or we hit the end of the head word
			{
				if (!Flac__BitReader_Read_Raw_UInt32(out _, 8))
					return false;

				nVals--;
			}

			if (nVals == 0)
				return true;

			// Step 2: Skip whole words in chunks
			while (nVals >= Constants.Flac__Bytes_Per_Word)
			{
				if (br.Consumed_Words < br.Words)
				{
					br.Consumed_Words++;
					nVals -= Constants.Flac__Bytes_Per_Word;

					if (br.Read_Limit_Set)
						br.Read_Limit -= Constants.Flac__Bits_Per_Word;
				}
				else
				{
					if (!BitReader_Read_From_Client())
						return false;
				}
			}

			// Step 3: Skip any remainder from partial tail bytes
			while (nVals != 0)
			{
				if (!Flac__BitReader_Read_Raw_UInt32(out _, 8))
					return false;

				nVals--;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__BitReader_Read_Unary_Unsigned(out uint32_t val)
		{
			Debug.Assert(br != null);
			Debug.Assert(br.Buffer != null);

			val = 0;
			while (true)
			{
				while (br.Consumed_Words < br.Words)	// If we've not consumed up to a partial tail word
				{
					brWord b = br.Consumed_Bits < Constants.Flac__Bits_Per_Word ? br.Buffer[br.Consumed_Words] << (int32_t)br.Consumed_Bits : 0;

					if (b != 0)
					{
						uint32_t i = Count_Zero_Msbs(b);
						val += i;
						i++;
						br.Consumed_Bits += i;

						if (br.Consumed_Bits >= Constants.Flac__Bits_Per_Word)
						{
							br.Consumed_Words++;
							br.Consumed_Bits = 0;
						}

						return true;
					}
					else
					{
						val += Constants.Flac__Bits_Per_Word - br.Consumed_Bits;
						br.Consumed_Words++;
						br.Consumed_Bits = 0;

						// Didn't find stop bit yet, have to keep going
					}
				}

				// At this point we've eaten up all the whole words; have to try
				// reading through any tail bytes before calling the read callback.
				// This is a repeat of the above logic adjusted for the fact we
				// don't have a whole word. Note though if the client is feeding
				// us data a byte at a time (unlikely), br.Consumed_Bits may not
				// be zero
				if ((br.Bytes * 8) > br.Consumed_Bits)
				{
					uint32_t end = br.Bytes * 8;
					brWord b = (br.Buffer[br.Consumed_Words] & (Constants.Flac__Word_All_Ones << (int32_t)(Constants.Flac__Bits_Per_Word - end))) << (int32_t)br.Consumed_Bits;

					if (b != 0)
					{
						uint32_t i = Count_Zero_Msbs(b);
						val += i;
						i++;
						br.Consumed_Bits += i;
						Debug.Assert(br.Consumed_Bits < Constants.Flac__Bits_Per_Word);

						return true;
					}
					else
					{
						val += end - br.Consumed_Bits;
						br.Consumed_Bits = end;
						Debug.Assert(br.Consumed_Bits < Constants.Flac__Bits_Per_Word);

						// Didn't find stop bit yet, have to keep going
					}
				}

				if (!BitReader_Read_From_Client())
					return false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__BitReader_Read_Byte_Block_Aligned_No_Crc(Flac__byte[] val, uint32_t nVals)
		{
			Debug.Assert(br != null);
			Debug.Assert(br.Buffer != null);
			Debug.Assert(Flac__BitReader_Is_Consumed_Byte_Aligned());

			if (br.Read_Limit_Set && (br.Read_Limit < uint32_t.MaxValue))
			{
				if (br.Read_Limit < nVals * 8)
				{
					br.Read_Limit = uint32_t.MaxValue;
					return false;
				}
			}

			uint32_t offset = 0;

			// Step 1: Read from partial head word to get word aligned
			while ((nVals != 0) && (br.Consumed_Bits != 0))		// I.e. run until we read 'nVals' bytes or we hit the end of the head word
			{
				if (!Flac__BitReader_Read_Raw_UInt32(out Flac__uint32 x, 8))
					return false;

				val[offset++] = (Flac__byte)x;
				nVals--;
			}

			if (nVals == 0)
				return true;

			// Step 2: Skip whole words in chunks
			while (nVals >= Constants.Flac__Bytes_Per_Word)
			{
				if (br.Consumed_Words < br.Words)
				{
					brWord word = br.Buffer[br.Consumed_Words++];

					val[offset] = (Flac__byte)(word >> 56);
					val[offset + 1] = (Flac__byte)(word >> 48);
					val[offset + 2] = (Flac__byte)(word >> 40);
					val[offset + 3] = (Flac__byte)(word >> 32);
					val[offset + 4] = (Flac__byte)(word >> 24);
					val[offset + 5] = (Flac__byte)(word >> 16);
					val[offset + 6] = (Flac__byte)(word >> 8);
					val[offset + 7] = (Flac__byte)word;

					offset += Constants.Flac__Bytes_Per_Word;
					nVals -= Constants.Flac__Bytes_Per_Word;

					if (br.Read_Limit_Set)
						br.Read_Limit -= Constants.Flac__Bits_Per_Word;
				}
				else
				{
					if (!BitReader_Read_From_Client())
						return false;
				}
			}

			// Step 3: Skip any remainder from partial tail bytes
			while (nVals != 0)
			{
				if (!Flac__BitReader_Read_Raw_UInt32(out Flac__uint32 x, 8))
					return false;

				val[offset++] = (Flac__byte)x;
				nVals--;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// This is far the most heavily used reader call. It ain't pretty
		/// but it's fast
		/// </summary>
		/********************************************************************/
		public Flac__bool Read_Rice_Signed_Block(int[] vals, uint32_t offset, uint32_t nVals, uint32_t parameter)
		{
			return BitReader_Read_Rice_Signed_Block(vals, offset, nVals, parameter);
		}



		/********************************************************************/
		/// <summary>
		/// On return, if val == 0xffffffff then the utf-8 sequence was
		/// invalid, but the return value will be true
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__BitReader_Read_Utf8_UInt32(out Flac__uint32 val, Flac__byte[] raw, ref uint32_t rawLen)
		{
			Flac__uint32 v = 0;
			uint32_t i;

			val = 0;

			if (!Flac__BitReader_Read_Raw_UInt32(out Flac__uint32 x, 8))
				return false;

			if (raw != null)
				raw[rawLen++] = (Flac__byte)x;

			if ((x & 0x80) == 0)	// 0xxxxxxx
			{
				v = x;
				i = 0;
			}
			else if ((x & 0xe0) == 0xc0)	// 110xxxxx
			{
				v = x & 0x1f;
				i = 1;
			}
			else if ((x & 0xf0) == 0xe0)	// 1110xxxx
			{
				v = x & 0x0f;
				i = 2;
			}
			else if ((x & 0xf8) == 0xf0)	// 11110xxx
			{
				v = x & 0x07;
				i = 3;
			}
			else if ((x & 0xfc) == 0xf8)	// 111110xx
			{
				v = x & 0x03;
				i = 4;
			}
			else if ((x & 0xfe) == 0xfc)	// 1111110x
			{
				v = x & 0x01;
				i = 5;
			}
			else
			{
				val = Flac__uint32.MaxValue;
				return true;
			}

			for (; i != 0; i--)
			{
				if (!Flac__BitReader_Read_Raw_UInt32(out x, 8))
					return false;

				if (raw != null)
					raw[rawLen++] = (Flac__byte)x;

				if (((x & 0x80) == 0) || ((x & 0x40) != 0))	// 10xxxxxx
				{
					val = Flac__uint32.MaxValue;
					return true;
				}

				v <<= 6;
				v |= (x & 0x3f);
			}

			val = v;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// On return, if val == 0xffffffffffffffff then the utf-8 sequence
		/// was invalid, but the return value will be true
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__BitReader_Read_Utf8_UInt64(out Flac__uint64 val, Flac__byte[] raw, ref uint32_t rawLen)
		{
			Flac__uint64 v = 0;
			uint32_t i;

			val = 0;

			if (!Flac__BitReader_Read_Raw_UInt32(out Flac__uint32 x, 8))
				return false;

			if (raw != null)
				raw[rawLen++] = (Flac__byte)x;

			if ((x & 0x80) == 0)	// 0xxxxxxx
			{
				v = x;
				i = 0;
			}
			else if ((x & 0xe0) == 0xc0)	// 110xxxxx
			{
				v = x & 0x1f;
				i = 1;
			}
			else if ((x & 0xf0) == 0xe0)	// 1110xxxx
			{
				v = x & 0x0f;
				i = 2;
			}
			else if ((x & 0xf8) == 0xf0)	// 11110xxx
			{
				v = x & 0x07;
				i = 3;
			}
			else if ((x & 0xfc) == 0xf8)	// 111110xx
			{
				v = x & 0x03;
				i = 4;
			}
			else if ((x & 0xfe) == 0xfc)	// 1111110x
			{
				v = x & 0x01;
				i = 5;
			}
			else if (x == 0xfe)				// 11111110
			{
				v = 0;
				i = 6;
			}
			else
			{
				val = Flac__uint64.MaxValue;
				return true;
			}

			for (; i != 0; i--)
			{
				if (!Flac__BitReader_Read_Raw_UInt32(out x, 8))
					return false;

				if (raw != null)
					raw[rawLen++] = (Flac__byte)x;

				if (((x & 0x80) == 0) || ((x & 0x40) != 0))	// 10xxxxxx
				{
					val = Flac__uint64.MaxValue;
					return true;
				}

				v <<= 6;
				v |= (x & 0x3f);
			}

			val = v;

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
		private Flac__uint32 Count_Zero_Msbs(Flac__uint64 word)
		{
			return BitMath.Flac__Clz_UInt64(word);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Flac__uint32 Count_Zero_Msbs2(Flac__uint64 word)
		{
			return BitMath.Flac__Clz2_UInt64(word);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool BitReader_Read_From_Client()
		{
			uint32_t start, end;
			brWord preswap_Backup = 0;

			// First shift the unconsumed buffer data toward the front as much as possible
			if (br.Consumed_Words > 0)
			{
				// Invalidate last seen framesync
				br.Last_Seen_Framesync = uint32_t.MaxValue;

				Crc16_Update_Block();	// CRC consumed words

				start = br.Consumed_Words;
				end = (uint32_t)(br.Words + (br.Bytes != 0 ? 1 : 0));
				Buffer.BlockCopy(br.Buffer, (int)(start * Constants.Flac__Bytes_Per_Word), br.Buffer, 0, (int)(Constants.Flac__Bytes_Per_Word * (end - start)));

				br.Words -= start;
				br.Consumed_Words = 0;
			}

			// Set the target for reading, taking into account word alignment and endianness
			size_t bytes = (br.Capacity - br.Words) * Constants.Flac__Bytes_Per_Word - br.Bytes;
			if (bytes == 0)
				return false;	// No space left, buffer is too small; see note for Flac__BitReader_Default_Capacity

			Span<Flac__byte> target = MemoryMarshal.Cast<brWord, Flac__byte>(new Span<brWord>(br.Buffer).Slice((int)br.Words)).Slice((int)br.Bytes);

			// Before reading, if the existing reader looks like this (say brWord is 32 bits wide)
			//   bitstream : 11 22 33 44 55            br.words=1 br.bytes=1 (partial tail word is left-justified)
			//   buffer[BE]: 11 22 33 44 55 ?? ?? ??   (shown laid out as bytes sequentially in memory)
			//   buffer[LE]: 44 33 22 11 ?? ?? ?? 55   (?? being don't-care)
			//                              ^^-------target, bytes=3
			// On LE machines, have to byteswap the old tail word so nothing is
			// overwritten
			if (BitConverter.IsLittleEndian)
			{
				preswap_Backup = br.Buffer[br.Words];

				if (br.Bytes != 0)
					br.Buffer[br.Words] = Swap_Be_Word_To_Host(br.Buffer[br.Words]);
			}

			// Now it looks like this:
			//   bitstream : 11 22 33 44 55            br.words=1 br.bytes=1
			//   buffer[BE]: 11 22 33 44 55 ?? ?? ??
			//   buffer[LE]: 44 33 22 11 55 ?? ?? ??
			//                              ^^-------target, bytes=3

			// Read in the data; note that the callback may return a smaller number of bytes
			if (!br.Read_Callback(target, ref bytes, br.Client_Data))
			{
				// Despite the read callback failing, the data in the target
				// might be used later, when the buffer is rewound. Therefore
				// we revert the swap that was just done
				if (BitConverter.IsLittleEndian)
					br.Buffer[br.Words] = preswap_Backup;

				return false;
			}

			// After reading bytes 66 77 88 99 AA BB CC DD EE FF from client:
			//   bitstream : 11 22 33 44 55 66 77 88 99 AA BB CC DD EE FF
			//   buffer[BE]: 11 22 33 44 55 66 77 88 99 AA BB CC DD EE FF ??
			//   buffer[LE]: 44 33 22 11 55 66 77 88 99 AA BB CC DD EE FF ??
			// Now have to byteswap on LE machines
			if (BitConverter.IsLittleEndian)
			{
				end = (uint32_t)((br.Words * Constants.Flac__Bytes_Per_Word + br.Bytes + bytes + (Constants.Flac__Bytes_Per_Word - 1)) / Constants.Flac__Bytes_Per_Word);

				for (start = br.Words; start < end; start++)
					br.Buffer[start] = Swap_Be_Word_To_Host(br.Buffer[start]);
			}

			// Now it looks like:
			//   bitstream : 11 22 33 44 55 66 77 88 99 AA BB CC DD EE FF
			//   buffer[BE]: 11 22 33 44 55 66 77 88 99 AA BB CC DD EE FF ??
			//   buffer[LE]: 44 33 22 11 88 77 66 55 CC BB AA 99 ?? FF EE DD
			// Finally we'll update the reader values
			end = (uint32_t)(br.Words * Constants.Flac__Bytes_Per_Word + br.Bytes + bytes);
			br.Words = end / Constants.Flac__Bytes_Per_Word;
			br.Bytes = end % Constants.Flac__Bytes_Per_Word;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Crc16_Update_Word(brWord word)
		{
			uint32_t crc = br.Read_Crc16;

			for (; br.Crc16_Align < Constants.Flac__Bits_Per_Word; br.Crc16_Align += 8)
			{
				uint32_t shift = Constants.Flac__Bits_Per_Word - 8 - br.Crc16_Align;
				crc = Crc.Flac__Crc16_Update((Flac__byte)(shift < Constants.Flac__Bits_Per_Word ? (word >> (int32_t)shift) & 0xff : 0), (Flac__uint16)crc);
			}

			br.Read_Crc16 = crc;
			br.Crc16_Align = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Crc16_Update_Block()
		{
			if ((br.Consumed_Words > br.Crc16_Offset) && (br.Crc16_Align != 0))
				Crc16_Update_Word(br.Buffer[br.Crc16_Offset++]);

			// Prevent OOB read due to wrap-around
			if (br.Consumed_Words > br.Crc16_Offset)
			{
				Debug.Assert(Constants.Flac__Bytes_Per_Word == 8);

				br.Read_Crc16 = Crc.Flac__Crc16_Update_Words64(br.Buffer, br.Crc16_Offset, br.Consumed_Words - br.Crc16_Offset, (Flac__uint16)br.Read_Crc16);
			}

			br.Crc16_Offset = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Flac__bool BitReader_Read_Rice_Signed_Block(int[] vals, uint32_t offset, uint32_t nVals, uint32_t parameter)
		{
			Debug.Assert(br != null);
			Debug.Assert(br.Buffer != null);

			// WATCHOUT: Code does not work with <32 words; we can make things much faster with this assertion
			Debug.Assert(Constants.Flac__Bits_Per_Word >= 32);
			Debug.Assert(parameter < 32);
			// The above two asserts also guarantee that the binary part never straddles more than 2 words, so we don't have to loop to read it

			uint32_t limit = uint32_t.MaxValue >> (int)parameter;	// Maximal msbs that can occur with residual bounded to int32_t

			uint32_t val = offset;
			uint32_t end = offset + nVals;
			uint32_t msbs = 0;

			if (parameter == 0)
			{
				while (val < end)
				{
					// Read the unary MSBs and end bit
					if (!Flac__BitReader_Read_Unary_Unsigned(out msbs))
						return false;

					// Checking limit here would be overzealous: coding UINT32_MAX
					// with parameter == 0 would take 4GiB
					vals[val++] = (int)(msbs >> 1) ^ -(int)(msbs & 1);
				}

				return true;
			}

			Debug.Assert(parameter > 0);

			uint32_t cWords = br.Consumed_Words;
			uint32_t words = br.Words;

			uint32_t x = 0, y;
			Flac__bool process_Tail = false;
			Flac__bool incomplete_Msbs = false;
			Flac__bool incomplete_Lsbs = false;
			uint32_t ucBits = 0;
			brWord b = 0;

			// If we've not consumed up to a partial tail word
			if (cWords >= words)
			{
				x = 0;
				process_Tail = true;
			}
			else
			{
				ucBits = Constants.Flac__Bits_Per_Word - br.Consumed_Bits;
				b = br.Buffer[cWords] << (int)br.Consumed_Bits;	// Keep unconsumed bits aligned to left
			}

			while (val < end)
			{
				if (process_Tail)
				{
					process_Tail = false;
					goto Process_Tail;
				}

				// Read the unary MSBs and end bit
				x = y = Count_Zero_Msbs2(b);

				if (x == Constants.Flac__Bits_Per_Word)
				{
					x = ucBits;

					do
					{
						// Didn't find stop bit yet, have to keep going
						cWords++;
						if (cWords >= words)
						{
							incomplete_Msbs = true;
							goto Process_Tail;
						}

						b = br.Buffer[cWords];
						y = Count_Zero_Msbs2(b);
						x += y;
					}
					while (y == Constants.Flac__Bits_Per_Word);
				}

				b <<= (int)y;
				b <<= 1;	// Account for stop bit
				ucBits = (ucBits - x - 1) % Constants.Flac__Bits_Per_Word;
				msbs = x;

				if (x > limit)
					return false;

				// Read the binary LSBs
				x = (Flac__uint32)(b >> (int)(Constants.Flac__Bits_Per_Word - parameter));	// Parameter < 32, so we can cast to 32-bit uint32_t

				if (parameter <= ucBits)
				{
					ucBits -= parameter;
					b <<= (int)parameter;
				}
				else
				{
					// There are still bits left to read, they will all be in the next word
					cWords++;
					if (cWords >= words)
					{
						incomplete_Lsbs = true;
						goto Process_Tail;
					}

					b = br.Buffer[cWords];
					ucBits += Constants.Flac__Bits_Per_Word - parameter;
					x |= (Flac__uint32)(b >> (int)ucBits);
					b <<= (int)(Constants.Flac__Bits_Per_Word - ucBits);
				}

				uint32_t lsbs = x;

				// Compose the value
				x = (msbs << (int)parameter) | lsbs;
				vals[val++] = (int)(x >> 1) ^ -(int)(x & 1);

				continue;

				// At this point we've eaten up all the whole words
Process_Tail:
				do
				{
					if (!incomplete_Lsbs)
					{
						if (incomplete_Msbs)
						{
							incomplete_Msbs = false;
							br.Consumed_Bits = 0;
							br.Consumed_Words = cWords;
						}

						// Read the unary MSBs and end bit
						if (!Flac__BitReader_Read_Unary_Unsigned(out msbs))
							return false;

						msbs += x;
						x = ucBits = 0;
					}

					if (incomplete_Lsbs)
					{
						incomplete_Lsbs = false;
						br.Consumed_Bits = 0;
						br.Consumed_Words = cWords;
					}

					// Read the binary LSBs
					if (!Flac__BitReader_Read_Raw_UInt32(out lsbs, parameter - ucBits))
						return false;

					lsbs = x | lsbs;

					// Compose the value
					x = (msbs << (int)parameter) | lsbs;
					vals[val++] = (int)(x >> 1) ^ -(int)(x & 1);
					x = 0;

					cWords = br.Consumed_Words;
					words = br.Words;
					ucBits = Constants.Flac__Bits_Per_Word - br.Consumed_Bits;
					b = cWords < br.Capacity ? br.Buffer[cWords] << (int)br.Consumed_Bits : 0;
				}
				while ((cWords >= words) && (val < end));
			}

			if ((ucBits == 0) && (cWords < words))
			{
				// Don't leave the head word with no consumed bits
				cWords++;
				ucBits = Constants.Flac__Bits_Per_Word;
			}

			br.Consumed_Bits = Constants.Flac__Bits_Per_Word - ucBits;
			br.Consumed_Words = cWords;

			return true;
		}
		#endregion
	}
}
