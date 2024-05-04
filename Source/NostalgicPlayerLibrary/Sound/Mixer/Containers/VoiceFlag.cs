/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Sound.Mixer.Containers
{
	/// <summary>
	/// Different flags used when playing a sample
	/// </summary>
	[Flags]
	internal enum VoiceFlag
	{
		None = 0,

		ChangePosition = 0x1000,
		Release = 0x2000
	}
}
