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
	public enum HwAccelCap
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// 
		/// </summary>
		Async_Safe = 1 << 0,

		/// <summary>
		/// 
		/// </summary>
		Thread_Safe = 1 << 1
	}
}
