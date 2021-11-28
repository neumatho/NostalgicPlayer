/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Lha.Containers;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Lha
{
	internal partial class LhaCore
	{
		public ushort crc = 0;
		private ushort bitBuf;

		private byte subBitBuf;
		private byte bitCount;

		private ushort[] crcTable;

		/********************************************************************/
		/// <summary>
		/// Create CRC table
		/// </summary>
		/********************************************************************/
		public void MakeCrcTable()
		{
			crcTable = new ushort[byte.MaxValue + 1];

			for (uint i = 0; i <= byte.MaxValue; i++)
			{
				uint r = i;

				for (uint j = 0; j < Constants.CharBit; j++)
				{
					if ((r & 1) != 0)
						r = (r >> 1) ^ Constants.CrcPoly;
					else
						r >>= 1;
				}

				crcTable[i] = (ushort)r;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Calculate CRC checksum
		/// </summary>
		/********************************************************************/
		public ushort CalcCrc(byte[] p, int offset, uint n)
		{
			int i = offset;

			while (n-- > 0)
				crc = (ushort)(crcTable[(crc ^ p[i++]) & 0xff] ^ (crc >> Constants.CharBit));

			return crc;
		}



		/********************************************************************/
		/// <summary>
		/// Shift bitBuf n bits left, read n bits
		/// </summary>
		/********************************************************************/
		private void FillBuf(byte n)
		{
			while (n > bitCount)
			{
				n -= bitCount;
				bitBuf = (ushort)((bitBuf << bitCount) + (subBitBuf >> (Constants.CharBit - bitCount)));

				if (compSize != 0)
				{
					compSize--;
					subBitBuf = (byte)stream.ReadByte();
				}
				else
					subBitBuf = 0;

				bitCount = Constants.CharBit;
			}

			bitCount -= n;
			bitBuf = (ushort)((bitBuf << n) + (subBitBuf >> (Constants.CharBit - n)));
			subBitBuf <<= n;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private ushort GetBits(byte n)
		{
			ushort x = (ushort)(bitBuf >> (2 * Constants.CharBit - n));
			FillBuf(n);

			return x;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void InitGetBits()
		{
			bitBuf = 0;
			subBitBuf = 0;
			bitCount = 0;
			FillBuf(2 * Constants.CharBit);
		}
	}
}
