/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	public enum FFThread
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// Decode more than one frame at once
		/// </summary>
		Frame = 1,

		/// <summary>
		/// Decode more than one part of a single frame at once
		/// </summary>
		Slice = 2
	}
}
