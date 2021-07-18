/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Common
{
	/// <summary>
	/// </summary>
	internal class LsbBitReader
	{
		private readonly IInputStream inputStream;

		private uint bufContent = 0;
		private byte bufLength = 0;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public LsbBitReader(IInputStream inputStream)
		{
			this.inputStream = inputStream;
		}



		/********************************************************************/
		/// <summary>
		/// Read count number of bits in big endian format
		/// </summary>
		/********************************************************************/
		public uint ReadBitsBe32(uint count)
		{
			return ReadBitsInternal(count, () =>
			{
				byte[] tmp = new byte[4];
				byte[] buf = inputStream.Consume(4, tmp);

				bufContent = (uint)((buf[0] << 24) | (buf[1] << 16) | (buf[2] << 8) | buf[3]);
				bufLength = 32;
			});
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Read count number of bits by calling the function given
		/// </summary>
		/********************************************************************/
		private uint ReadBitsInternal(uint count, Action readWord)
		{
			uint ret = 0;
			int pos = 0;

			while (count != 0)
			{
				if (bufLength == 0)
					readWord();

				byte maxCount = Math.Min((byte)count, bufLength);
				ret |= (uint)((bufContent & ((1 << maxCount) - 1)) << pos);

				bufContent >>= maxCount;
				bufLength -= maxCount;
				count -= maxCount;
				pos += maxCount;
			}

			return ret;
		}
		#endregion
	}
}
