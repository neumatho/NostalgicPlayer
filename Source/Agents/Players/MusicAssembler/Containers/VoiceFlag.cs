/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Player.MusicAssembler.Containers
{
	/// <summary>
	/// Different status flag for a single voice
	/// </summary>
	[Flags]
	internal enum VoiceFlag : byte
	{
		None = 0,
		SetLoop = 0x02,
		Synthesis = 0x04,
		Portamento = 0x20,
		Release = 0x40,
		Retrig = 0x80
	}
}
