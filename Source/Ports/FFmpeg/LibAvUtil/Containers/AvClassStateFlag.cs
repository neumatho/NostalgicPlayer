/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	public enum AvClassStateFlag
	{
		/// <summary>
		/// Object initialization has finished and it is now in the 'runtime' stage.
		/// This affects e.g. what options can be set on the object (only
		/// AV_OPT_FLAG_RUNTIME_PARAM options can be set on initialized objects)
		/// </summary>
		Initialized = 1 << 0
	}
}
