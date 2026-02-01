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
	public enum AvIoSeekable
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// Seeking works like for a local file
		/// </summary>
		Normal = 1 << 0,

		/// <summary>
		/// Seeking by timestamp with avio_seek_time() is possible
		/// </summary>
		Time = 1 << 1
	}
}
