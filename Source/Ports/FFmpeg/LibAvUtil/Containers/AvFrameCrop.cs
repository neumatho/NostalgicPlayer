/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// Flags for frame cropping
	/// </summary>
	[Flags]
	public enum AvFrameCrop
	{
		/// <summary>
		/// 
		/// </summary>
		None,

		/// <summary>
		/// Apply the maximum possible cropping, even if it requires setting the
		/// AVFrame.data[] entries to unaligned pointers. Passing unaligned data
		/// to FFmpeg API is generally not allowed, and causes undefined behavior
		/// (such as crashes). You can pass unaligned data only to FFmpeg APIs that
		/// are explicitly documented to accept it. Use this flag only if you
		/// absolutely know what you are doing
		/// </summary>
		Unaligned = 1 << 0
	}
}
