/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Lha.Containers;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Lha
{
	internal partial class LhaCore
	{
		public Crc16 crc = new Crc16();
		private ushort bitBuf;

		private byte subBitBuf;
		private byte bitCount;

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
