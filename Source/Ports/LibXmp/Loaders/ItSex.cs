/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Loaders
{
	/// <summary>
	/// Public domain IT sample decompressor by Olivier Lapicque
	///
	/// Modified by Alice Rowan (2023)- more or less complete rewrite of the input
	/// stream to add buffering
	/// </summary>
	internal static partial class Sample
	{
		private class It_Stream
		{
			public CPointer<uint8> Pos;
			public size_t Left;
			public uint32 Bits;
			public c_int Num_Bits;
			public c_int Err;
		}

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int ItSex_Decompress8(Hio src, CPointer<uint8> dst, c_int len, CPointer<uint8> tmp, c_int tmpLen, bool it215)
		{
			It_Stream @in = new It_Stream();
			uint32 block_Count = 0;
			uint8 left = 0, temp = 0, temp2 = 0;

			while (len != 0)
			{
				if (block_Count == 0)
				{
					block_Count = 0x8000;
					left = 9;
					temp = temp2 = 0;

					if (Init_Block(@in, tmp, tmpLen, src) < 0)
						return -1;
				}

				uint32 d = block_Count;
				if (d > len)
					d = (uint32)len;

				// Unpacking
				uint32 pos = 0;

				do
				{
					uint16 bits = (uint16)Read_Bits(@in, left);
					if (@in.Err != 0)
						return -1;

					if (left < 7)
					{
						uint32 i = 1U << (left - 1);
						uint32 j = bits & 0xffffU;

						if (i != j)
							goto Unpack_Byte;

						bits = (uint16)((Read_Bits(@in, 3) + 1) & 0xff);
						if (@in.Err != 0)
							return -1;

						left = ((uint8)bits < left) ? (uint8)bits : (uint8)((bits + 1) & 0xff);
						goto Next;
					}

					if (left < 9)
					{
						uint16 i = (uint16)((0xff >> (9 - left)) + 4);
						uint16 j = (uint16)(i - 8);

						if ((bits <= j) || (bits > i))
							goto Unpack_Byte;

						bits -= j;
						left = ((uint8)(bits & 0xff) < left) ? (uint8)(bits & 0xff) : (uint8)((bits + 1) & 0xff);
						goto Next;
					}

					if (left >= 10)
						goto Skip_Byte;

					if (bits >= 256)
					{
						left = (uint8)((bits + 1) & 0xff);
						goto Next;
					}

					Unpack_Byte:
					if (left < 8)
					{
						uint8 shift = (uint8)(8 - left);
						sbyte c = (sbyte)(bits << shift);
						c >>= shift;
						bits = (uint16)c;
					}

					bits += temp;
					temp = (uint8)bits;
					temp2 += temp;
					dst[pos] = it215 ? temp2 : temp;

					Skip_Byte:
					pos++;

					Next:
					;
				}
				while (pos < d);

				// Move on
				block_Count -= d;
				len -= (c_int)d;
				dst += d;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int ItSex_Decompress16(Hio src, Span<int16> dst, c_int len, CPointer<uint8> tmp, c_int tmpLen, bool it215)
		{
			It_Stream @in = new It_Stream();
			uint32 block_Count = 0;
			uint8 left = 0;
			int16 temp = 0, temp2 = 0;
			int dstOffset = 0;

			while (len != 0)
			{
				if (block_Count == 0)
				{
					block_Count = 0x4000;
					left = 17;
					temp = temp2 = 0;

					if (Init_Block(@in, tmp, tmpLen, src) < 0)
						return -1;
				}

				uint32 d = block_Count;
				if (d > len)
					d = (uint32)len;

				// Unpacking
				uint32 pos = 0;

				do
				{
					uint32 bits = Read_Bits(@in, left);
					if (@in.Err != 0)
						return -1;

					if (left < 7)
					{
						uint32 i = 1U << (left - 1);
						uint32 j = bits;

						if (i != j)
							goto Unpack_Byte;

						bits = Read_Bits(@in, 4) + 1;
						if (@in.Err != 0)
							return -1;

						left = ((uint8)(bits & 0xff) < left) ? (uint8)(bits & 0xff) : (uint8)((bits + 1) & 0xff);
						goto Next;
					}

					if (left < 17)
					{
						uint32 i = (uint32)((0xffff >> (17 - left)) + 8);
						uint32 j = (i - 16) & 0xffff;

						if ((bits <= j) || (bits > (i & 0xffff)))
							goto Unpack_Byte;

						bits -= j;
						left = ((uint8)(bits & 0xff) < left) ? (uint8)(bits & 0xff) : (uint8)((bits + 1) & 0xff);
						goto Next;
					}

					if (left >= 18)
						goto Skip_Byte;

					if (bits >= 0x10000)
					{
						left = (uint8)((bits + 1) & 0xff);
						goto Next;
					}

					Unpack_Byte:
					if (left < 16)
					{
						uint8 shift = (uint8)(16 - left);
						int16 c = (int16)(bits << shift);
						c >>= shift;
						bits = (uint32)c;
					}

					bits = (uint32)(bits + temp);
					temp = (int16)bits;
					temp2 += temp;
					dst[(int)(dstOffset + pos)] = it215 ? temp2 : temp;

					Skip_Byte:
					pos++;

					Next:
					;
				}
				while (pos < d);

				// Move on
				block_Count -= d;
				len -= (c_int)d;
				dstOffset += (c_int)d;

				if (len <= 0)
					break;
			}

			return 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static uint32 Read_Bits_Mask(c_int n)
		{
			return (1U << n) - 1U;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static uint32 Read_Bits(It_Stream @in, c_int n)
		{
			if ((n <= 0) || (n >= 32))
			{
				// Invalid shift value
				@in.Err = -2;
				return 0;
			}

			uint32 retVal = @in.Bits & Read_Bits_Mask(n);

			if (@in.Num_Bits < n)
			{
				uint32 offset = (uint32)@in.Num_Bits;

				if (@in.Left == 0)
				{
					@in.Err = Constants.EOF;
					return 0;
				}

				// Buffer should be zero-padded to 4-byte alignment
				@in.Bits = (uint32)(@in.Pos[0] | (@in.Pos[1] << 8) | (@in.Pos[2] << 16) | (@in.Pos[3] << 24));

				uint32 used = (uint32)Math.Min(@in.Left, 4);

				@in.Num_Bits = (c_int)(used * 8);
				@in.Pos += 4;
				@in.Left -= used;

				n = (c_int)(n - offset);
				retVal |= (@in.Bits & Read_Bits_Mask(n)) << (c_int)offset;
			}

			@in.Bits >>= n;
			@in.Num_Bits -= n;

			return retVal;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Init_Block(It_Stream @in, CPointer<uint8> tmp, c_int tmpLen, Hio src)
		{
			@in.Pos = tmp;
			@in.Left = src.Hio_Read16L();
			@in.Bits = 0;
			@in.Num_Bits = 0;
			@in.Err = 0;

			// tmp should be INT16_MAX rounded up to a multiple of 4 bytes long
			if (tmpLen < (c_int)((@in.Left + 4) & ~3U))
				return -1;

			if (src.Hio_Read(tmp, 1, @in.Left) < @in.Left)
				return -1;

			// Zero pad to a multiple of 4 bytes for read_bits
			for (size_t i = @in.Left; (i & 3) != 0; i++)
				tmp[i] = 0;

			return 0;
		}
		#endregion
	}
}
