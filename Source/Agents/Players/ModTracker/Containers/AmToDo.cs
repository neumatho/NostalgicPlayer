﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.ModTracker.Containers
{
	/// <summary>
	/// Different states when playing StarTrekker AM samples
	/// </summary>
	internal enum AmToDo
	{
		None,
		Attack1,
		Attack2,
		Sustain,
		SustainDecay,
		Release
	}
}
