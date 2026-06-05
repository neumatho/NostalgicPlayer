/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	internal enum InstrumentFlag : byte
	{
		None = 0,

		EnvelopeHoldSustain = 1 << 0,

		TransformInit = 1 << 1,
		TransformStep = 1 << 2,

		PhaseInit = 1 << 3,
		PhaseStep = 1 << 4,
		PhaseFill = 1 << 5,

		FilterInit = 1 << 6,
		FilterStep = 1 << 7
	}
}
