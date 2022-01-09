/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Common
{
	/// <summary>
	/// Different helper methods
	/// </summary>
	internal static class Common
	{
		private static readonly byte[] rotateNibble =
		{
			0x0, 0x8, 0x4, 0xc,
			0x2, 0xa, 0x6, 0xe,
			0x1, 0x9, 0x5, 0xd,
			0x3, 0xb, 0x7, 0xf
		};

		/********************************************************************/
		/// <summary>
		/// Rotate the bits in the value given to the right count number of
		/// times
		/// </summary>
		/********************************************************************/
		public static uint RotateBits(uint value, uint count)
		{
			uint ret = 0;

			for (uint i = 0; i < count; i += 4)
			{
				ret = (ret << 4) | rotateNibble[value & 0xf];
				value >>= 4;
			}

			ret >>= (int)((4 - count) & 3);

			return ret;
		}
	}
}
