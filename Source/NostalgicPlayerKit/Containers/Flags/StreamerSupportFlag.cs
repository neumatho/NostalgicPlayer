/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.Containers.Flags
{
	/// <summary>
	/// Different flags indicating what a sample player supports
	/// </summary>
	[Flags]
	public enum StreamerSupportFlag
	{
		/// <summary>
		/// Nothing
		/// </summary>
		None = 0,

		/// <summary>
		/// Set this if your player can change to a certain position. You also
		/// need to implement SetSongPosition() in the IDuration interface
		/// </summary>
		SetPosition = 0x0001
	}
}
