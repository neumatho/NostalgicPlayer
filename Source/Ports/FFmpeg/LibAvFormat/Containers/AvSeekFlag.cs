/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	public enum AvSeekFlag
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// Seek backward
		/// </summary>
		Backward = 1,

		/// <summary>
		/// Seeking based on position in bytes
		/// </summary>
		Byte = 2,

		/// <summary>
		/// Seek to any frame, even non-keyframes
		/// </summary>
		Any = 4,

		/// <summary>
		/// Seeking based on frame number
		/// </summary>
		Frame = 8
	}
}
