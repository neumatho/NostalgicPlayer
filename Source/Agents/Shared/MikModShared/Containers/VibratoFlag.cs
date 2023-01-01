/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Shared.MikMod.Containers
{
	/// <summary>
	/// Vibrato flags (AV_)
	/// </summary>
	[Flags]
	public enum VibratoFlag : byte
	{
		/// <summary></summary>
		None = 0,

		/// <summary>
		/// IT vs. XM vibrato info
		/// </summary>
		It = 1
	}
}
