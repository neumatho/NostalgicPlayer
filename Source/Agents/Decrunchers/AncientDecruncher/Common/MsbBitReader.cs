/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Common
{
	/// <summary>
	/// </summary>
	internal class MsbBitReader
	{
		private readonly IInputStream inputStream;

		private uint bufContent = 0;
		private byte bufLength = 0;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public MsbBitReader(IInputStream inputStream)
		{
			this.inputStream = inputStream;
		}



		/********************************************************************/
		/// <summary>
		/// Read the number of bits given as argument and return the number
		/// </summary>
		/********************************************************************/
		public uint ReadBits8(uint count)
		{
			return ReadBitsInternal(count, () =>
			{
				bufContent = inputStream.ReadByte();
				bufLength = 8;
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

			while (count != 0)
			{
				if (bufLength == 0)
					readWord();

				byte maxCount = Math.Min((byte)count, bufLength);
				bufLength -= maxCount;

				ret = (uint)((ret << maxCount) | ((bufContent >> bufLength) & ((1 << maxCount) - 1)));
				count -= maxCount;
			}

			return ret;
		}
		#endregion
	}
}
