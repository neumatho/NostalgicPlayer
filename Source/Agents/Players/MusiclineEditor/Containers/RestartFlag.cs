/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor.Containers
{
	/// <summary>
	/// Indicate what needs to be restarted
	/// </summary>
	[Flags]
	internal enum RestartFlag : byte
	{
		None = 0,

		RestartFromPartFx = 1 << 0,
		RestartFromArp = 1 << 1,
		ArpHasSetVolume = 1 << 2
	}
}
