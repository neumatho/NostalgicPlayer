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
	internal enum BufferFlag
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// The buffer was av_realloc()ed, so it is reallocatable
		/// </summary>
		Reallocatable = 1 << 0,

		/// <summary>
		/// The AVBuffer structure is part of a larger structure
		/// and should not be freed
		/// </summary>
		NoFree = 1 << 1
	}
}
