/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// Reset mode for GetLength()
	/// </summary>
	[Flags]
	internal enum EnmGetLengthResetMode
	{
		/// <summary>
		/// Never adjust global variables / mod parameters
		/// </summary>
		eNoAdjust = 0x00,

		/// <summary>
		/// Mod parameters (such as global volume, speed, tempo, etc...) will
		/// always be memorized if the target was reached (i.e. they won't be
		/// reset to the previous values). If target couldn't be reached, they
		/// are reset to their default values
		/// </summary>
		eAdjust = 0x01,

		/// <summary>
		/// Same as above, but global variables will only be memorized if the
		/// target could be reached. This does *NOT* influence the visited
		/// rows vector - it will *ALWAYS* be adjusted in this mode
		/// </summary>
		eAdjustOnSuccess = 0x02 | eAdjust,

		/// <summary>
		/// Same as previous option, but will also try to emulate sample
		/// playback so that voices from previous patterns will sound when
		/// continuing playback at the target position
		/// </summary>
		eAdjustSamplePositions = 0x04 | eAdjustOnSuccess,

		/// <summary>
		/// Only adjust the visited rows state
		/// </summary>
		eAdjustOnlyVisitedRows = 0x08
	}
}
