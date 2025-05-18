/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher
{
	/// <summary>
	/// Calculate CRC16 checksum
	/// </summary>
	internal class Crc16
	{
		private const ushort CrcPoly = 0xa001;

		private ushort[] crcTable;

		/********************************************************************/
		/// <summary>
		/// Constructore
		/// </summary>
		/********************************************************************/
		public Crc16()
		{
			MakeCrcTable();
		}



		/********************************************************************/
		/// <summary>
		/// Calculate CRC checksum
		/// </summary>
		/********************************************************************/
		public void CalcCrc(byte[] p, int offset, uint n)
		{
			int i = offset;

			while (n-- > 0)
				Crc = (ushort)(crcTable[(Crc ^ p[i++]) & 0xff] ^ (Crc >> 8));
		}



		/********************************************************************/
		/// <summary>
		/// Holds current CRC value
		/// </summary>
		/********************************************************************/
		public ushort Crc
		{
			get; private set;
		}= 0;

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Create CRC table
		/// </summary>
		/********************************************************************/
		private void MakeCrcTable()
		{
			crcTable = new ushort[byte.MaxValue + 1];

			for (uint i = 0; i <= byte.MaxValue; i++)
			{
				uint r = i;

				for (uint j = 0; j < 8; j++)
				{
					if ((r & 1) != 0)
						r = (r >> 1) ^ CrcPoly;
					else
						r >>= 1;
				}

				crcTable[i] = (ushort)r;
			}
		}
		#endregion
	}
}
