/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Player.SoundFactory.Containers
{
	/// <summary>
	/// Different flag defined on an instrument
	/// </summary>
	[Flags]
	internal enum InstrumentFlag : byte
	{
		None = 0,
		OneShot = 0x01,
		Vibrato = 0x02,
		Arpeggio = 0x04,
		Phasing = 0x08,
		Portamento = 0x10,
		Release = 0x20,
		Tremolo = 0x40,
		Filter = 0x80
	}
}
