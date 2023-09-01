/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Loaders
{
	/// <summary>
	/// Public domain IT sample decompressor by Olivier Lapicque
	/// </summary>
	internal static partial class Sample
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int ItSex_Decompress8(Hio src, Span<uint8> dst, c_int len, bool it215)
		{
			uint32 block_Count = 0;
			uint32 bitBuf = 0;
			c_int bitNum = 0;
			uint8 left = 0, temp = 0, temp2 = 0;
			c_int err = 0;
			int dstOffset = 0;

			while (len != 0)
			{
				if (block_Count == 0)
				{
					block_Count = 0x8000;
					/* size =*/ src.Hio_Read16L();
					left = 9;
					temp = temp2 = 0;
					bitBuf = 0;
					bitNum = 0;
				}

				uint32 d = block_Count;
				if (d > len)
					d = (uint32)len;

				// Unpacking
				uint32 pos = 0;

				do
				{
					uint16 bits = (uint16)Read_Bits(src, ref bitBuf, ref bitNum, left, out err);
					if (err != 0)
						return -1;

					if (left < 7)
					{
						uint32 i = 1U << (left - 1);
						uint32 j = bits & 0xffffU;

						if (i != j)
							goto Unpack_Byte;

						bits = (uint16)((Read_Bits(src, ref bitBuf, ref bitNum, 3, out err) + 1) & 0xff);
						if (err != 0)
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
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int ItSex_Decompress16(Hio src, Span<int16> dst, c_int len, bool it215)
		{
			uint32 block_Count = 0;
			uint32 bitBuf = 0;
			c_int bitNum = 0;
			uint8 left = 0;
			int16 temp = 0, temp2 = 0;
			c_int err = 0;
			int dstOffset = 0;

			while (len != 0)
			{
				if (block_Count == 0)
				{
					block_Count = 0x4000;
					/* size =*/ src.Hio_Read16L();
					left = 17;
					temp = temp2 = 0;
					bitBuf = 0;
					bitNum = 0;
				}

				uint32 d = block_Count;
				if (d > len)
					d = (uint32)len;

				// Unpacking
				uint32 pos = 0;

				do
				{
					uint32 bits = Read_Bits(src, ref bitBuf, ref bitNum, left, out err);
					if (err != 0)
						return -1;

					if (left < 7)
					{
						uint32 i = 1U << (left - 1);
						uint32 j = bits;

						if (i != j)
							goto Unpack_Byte;

						bits = Read_Bits(src, ref bitBuf, ref bitNum, 4, out err) + 1;
						if (err != 0)
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
		private static uint32 Read_Bits(Hio iBuf, ref uint32 bitBuf, ref c_int bitNum, c_int n, out c_int err)
		{
			uint32 retVal = 0;
			c_int i = n;
			c_int bNum = bitNum;
			uint32 bBuf = bitBuf;

			if ((n > 0) && (n < 32))
			{
				do
				{
					if (bNum == 0)
					{
						if (iBuf.Hio_Eof())
						{
							err = Constants.EOF;
							return 0;
						}

						bBuf = iBuf.Hio_Read8();
						bNum = 8;
					}

					retVal >>= 1;
					retVal |= bBuf << 31;
					bBuf >>= 1;
					bNum--;
					i--;
				}
				while (i != 0);

				i = n;

				bitNum = bNum;
				bitBuf = bBuf;
			}
			else
			{
				// Invalid shift value
				err = -2;
				return 0;
			}

			err = 0;
			return retVal >> (32 - i);
		}
		#endregion
	}
}
