/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Containers.Types;

namespace Polycode.NostalgicPlayer.Agent.Player.Oktalyzer
{
	/// <summary>
	/// Different tables needed
	/// </summary>
	internal static class Tables
	{
		/********************************************************************/
		/// <summary>
		/// Periods
		/// </summary>
		/********************************************************************/
		public static readonly short[] Periods =
		[
			856, 808, 762, 720, 678, 640, 604, 570, 538, 508, 480, 453,
			428, 404, 381, 360, 339, 320, 302, 285, 269, 254, 240, 226,
			214, 202, 190, 180, 170, 160, 151, 143, 135, 127, 120, 113
		];



		/********************************************************************/
		/// <summary>
		/// Panning value for each channel
		/// </summary>
		/********************************************************************/
		public static readonly ChannelPanningType[] PanPos =
		[
			ChannelPanningType.Left, ChannelPanningType.Left, ChannelPanningType.Right, ChannelPanningType.Right,
			ChannelPanningType.Right, ChannelPanningType.Right, ChannelPanningType.Left, ChannelPanningType.Left
		];



		/********************************************************************/
		/// <summary>
		/// Arpeggio values for type 1
		/// </summary>
		/********************************************************************/
		public static readonly sbyte[] Arp10 =
		[
			0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0
		];



		/********************************************************************/
		/// <summary>
		/// Arpeggio values for type 3
		/// </summary>
		/********************************************************************/
		public static readonly sbyte[] Arp12 =
		[
			0, 1, 2, 3, 1, 2, 3, 1, 2, 3, 1, 2, 3, 1, 2, 3
		];
	}
}
