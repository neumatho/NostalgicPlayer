/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor.Containers
{
	/// <summary>
	/// Different instrument effect flags
	/// </summary>
	[Flags]
	internal enum InstrumentEffect1 : byte
	{
		None = 0,

		Envelope = 1 << 0,
		Vibrato = 1 << 1,
		Tremolo = 1 << 2,
		Arpeggio = 1 << 3,
		Loop = 1 << 4,
		LoopStop = 1 << 5,
		HoldSustain = 1 << 6,
		WaveSampleLoop = 1 << 7
	}
}
