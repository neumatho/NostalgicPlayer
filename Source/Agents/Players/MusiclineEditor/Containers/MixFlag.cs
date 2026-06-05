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
	internal enum MixFlag : byte
	{
		None = 0,

		MixInit = 1 << 0,
		MixStep = 1 << 1,
		MixBuff = 1 << 2,
		MixCounter = 1 << 3,

		ResonanceInit = 1 << 4,
		ResonanceStep = 1 << 5,

		LoopInit = 1 << 6,
		LoopStep = 1 << 7
	}
}
