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
	public enum AvFmtEventFlag
	{
		/// <summary>
		/// - demuxing: The demuxer read new metadata from the file and updated
		///             AVFormatContext.metadata accordingly
		/// - muxing:   The user updated AVFormatContext.metadata and wishes the muxer to
		///             write it into the file
		/// </summary>
		Metadata_Updated = 0x0001
	}
}
