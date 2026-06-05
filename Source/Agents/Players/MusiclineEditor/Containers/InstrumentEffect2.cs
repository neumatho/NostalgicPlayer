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
	internal enum InstrumentEffect2 : byte
	{
		None = 0,

		Transform = 1 << 0,
		Phase = 1 << 1,
		Mix = 1 << 2,
		Resonance = 1 << 3,
		Filter = 1 << 4
	}
}
