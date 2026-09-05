/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	internal enum PlayFlags : uint16
	{
		/// <summary>
		/// Loop current pattern (pattern editor)
		/// </summary>
		Song_PatternLoop = 0x01,

		/// <summary>
		/// Song is in "step" mode (pattern editor)
		/// </summary>
		Song_Step = 0x02,

		/// <summary>
		/// Song is paused (no tick processing, just rendering audio)
		/// </summary>
		Song_Paused = 0x04,

		/// <summary>
		/// Song is fading out
		/// </summary>
		Song_FadingSong = 0x08,

		/// <summary>
		/// Song is finished
		/// </summary>
		Song_EndReached = 0x10,

		/// <summary>
		/// Is set when the current tick is the first tick of the row
		/// </summary>
		Song_FirstTick = 0x20,

		/// <summary>
		/// Local filter mode (reset filter on each note)
		/// </summary>
		Song_MptFilterMode = 0x40,

		/// <summary>
		/// Pan in the rear channels
		/// </summary>
		Song_SurroundPan = 0x80,

		/// <summary>
		/// Position jump encountered
		/// </summary>
		Song_PosJump = 0x100,

		/// <summary>
		/// Break to row command encountered
		/// </summary>
		Song_BreakToRow = 0x200,

		/// <summary>
		/// Report to plugins that we jumped around in the module
		/// </summary>
		Song_PositionChanged = 0x400
	}
}
