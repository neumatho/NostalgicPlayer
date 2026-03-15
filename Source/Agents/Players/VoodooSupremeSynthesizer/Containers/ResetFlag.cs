/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Player.VoodooSupremeSynthesizer.Containers
{
	/// <summary>
	/// Envelopes which should not be reset when a new note if played
	/// </summary>
	[Flags]
	internal enum ResetFlag
	{
		None = 0,
		WaveformTable = 1 << 5,
		PeriodTable = 1 << 6,
		VolumeEnvelope = 1 << 7
	}
}
