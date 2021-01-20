/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Text;

namespace Polycode.NostalgicPlayer.Kit.Encoders
{
	/// <summary>
	/// Base class for all encoders
	/// </summary>
	internal abstract class EncoderBase : Encoding
	{
		/********************************************************************/
		/// <summary>
		/// Lookup in the given high byte table to find the lower byte table
		/// to use. Then look up into this one to find the byte to use
		/// </summary>
		/********************************************************************/
		protected byte GetByteFromMultiTable(char chr, byte[][] indexTable)
		{
			// Find the lower byte table
			byte[] lowerTable = indexTable[chr >> 8];
			if (lowerTable == null)
				return 0x3f;			// Return '?' as an unknown character

			return lowerTable[chr & 0xff];
		}
	}
}
