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
	internal enum BoostFlag : byte
	{
		None = 0,

		Filter = 1 << 0,
		Resonance = 1 << 1,
		Mix = 1 << 2
	}
}
