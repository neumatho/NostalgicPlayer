/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Player.PumaTracker.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	internal enum VoiceFlag
	{
		None = 0,
		SetVolumeSlide = 0x01,
		SetFrequency = 0x02,
		VoiceRunning = 0x04,
		NoLoop = 0x08,
	}
}
