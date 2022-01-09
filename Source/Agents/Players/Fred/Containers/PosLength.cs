/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Fred.Containers
{
	/// <summary>
	/// Holds information about which channel to
	/// take position information from
	/// </summary>
	internal class PosLength
	{
		/// <summary>
		/// The channel to use positions from
		/// </summary>
		public int Channel;

		/// <summary>
		/// The number of positions in the channel
		/// </summary>
		public int Length;
	}
}
