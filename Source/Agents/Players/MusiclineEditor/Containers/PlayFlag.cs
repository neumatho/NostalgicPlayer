/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor.Containers
{
	/// <summary>
	/// Different playing flags
	/// </summary>
	[Flags]
	internal enum PlayFlag
	{
		None = 0,

		Retrigger = 1 << 0,
		FirstRowTick = 1 << 1,
		KeepEffectFineTune = 1 << 2
	}
}
