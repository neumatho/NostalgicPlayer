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
	public enum AvStreamEventFlag
	{
		/// <summary>
		/// - demuxing: the demuxer read new metadata from the file and updated
		///     AVStream.metadata accordingly
		/// - muxing: the user updated AVStream.metadata and wishes the muxer to write
		///     it into the file
		/// </summary>
		Metadata_Updated = 0x0001,

		/// <summary>
		/// - demuxing: new packets for this stream were read from the file. This
		///   event is informational only and does not guarantee that new packets
		///   for this stream will necessarily be returned from av_read_frame()
		/// </summary>
		New_Packets = 1 << 1
	}
}
