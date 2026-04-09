/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using Polycode.NostalgicPlayer.Kit.Containers.Types;

namespace Polycode.NostalgicPlayer.Agent.Player.Tfmx
{
	/// <summary>
	/// Different tables needed
	/// </summary>
	internal static class Tables
	{
		/********************************************************************/
		/// <summary>
		/// This is a copy of the table from TfmxDecoder
		/// </summary>
		/********************************************************************/
		public static readonly ushort[] Periods =
		[
			0x0d5c,

			// -0xf4 extra octave
			0x0c9c, 0x0be8, 0x0b3c, 0x0a9a, 0x0a02, 0x0a02, 0x0972,
			0x08ea, 0x086a, 0x07f2, 0x0780, 0x0718,

			// +0 standard octaves
			0x06ae, 0x064e, 0x05f4, 0x059e, 0x054d, 0x0501, 0x04b9, 0x0475,
			0x0435, 0x03f9, 0x03c0, 0x038c, 

			// +0x0c
			0x0358, 0x032a, 0x02fc, 0x02d0, 0x02a8, 0x0282, 0x025e, 0x023b, 
			0x021b, 0x01fd, 0x01e0, 0x01c6,

			// +0x18
			0x01ac, 0x0194, 0x017d, 0x0168, 0x0154, 0x0140, 0x012f, 0x011e,
			0x010e, 0x00fe, 0x00f0, 0x00e3, 

			// +0x24
			0x00d6, 0x00ca, 0x00bf, 0x00b4, 0x00aa, 0x00a0, 0x0097, 0x008f,
			0x0087, 0x007f, 0x0078, 0x0071,

			// +0x30
			0x00d6, 0x00ca, 0x00bf, 0x00b4, 0x00aa, 0x00a0, 0x0097, 0x008f,
			0x0087, 0x007f, 0x0078, 0x0071,

			// +0x3c
			0x00d6, 0x00ca, 0x00bf, 0x00b4
		];



		/********************************************************************/
		/// <summary>
		/// Panning values for 4 channels modules
		/// </summary>
		/********************************************************************/
		public static readonly ChannelPanningType[] Pan4 =
		[
			ChannelPanningType.Left, ChannelPanningType.Right, ChannelPanningType.Right, ChannelPanningType.Left
		];



		/********************************************************************/
		/// <summary>
		/// Panning values for 7 channels modules
		/// </summary>
		/********************************************************************/
		public static readonly ChannelPanningType[] Pan7 =
		[
			ChannelPanningType.Left, ChannelPanningType.Right, ChannelPanningType.Right,
			ChannelPanningType.Left, ChannelPanningType.Left, ChannelPanningType.Left, ChannelPanningType.Left
		];
	}
}
