/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Player.VoodooSupremeSynthesizer.Containers
{
	/// <summary>
	/// Synthesis mode
	/// </summary>
	[Flags]
	internal enum SynthesisFlag
	{
		None = 0,
		FrequencyBasedLength = 1 << 0,
		StopSample = 1 << 1,
		FrequencyMapped = 1 << 5,
		XorRingModulation = 1 << 6,
		Morphing = 1 << 7
	}
}
