/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers
{
	/// <summary>
	/// Compatibility modes
	/// </summary>
	internal enum Compatibility
	{
		/// <summary>
		/// File is C64 compatible
		/// </summary>
		C64,

		/// <summary>
		/// File is PSID specific
		/// </summary>
		PSid,

		/// <summary>
		/// File is Real C64 only
		/// </summary>
		R64,

		/// <summary>
		/// File requires C64 Basic
		/// </summary>
		Basic
	}
}
