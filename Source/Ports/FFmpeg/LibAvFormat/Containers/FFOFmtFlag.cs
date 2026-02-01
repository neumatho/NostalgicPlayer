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
	public enum FFOFmtFlag
	{
		/// <summary>
		/// This flag indicates that the muxer stores data internally
		/// and supports flushing it. Flushing is signalled by sending
		/// a NULL packet to the muxer's write_packet callback;
		/// without this flag, a muxer never receives NULL packets.
		/// So the documentation of write_packet below for the semantics
		/// of the return value in case of flushing
		/// </summary>
		Allow_Flush = 1 << 1,

		/// <summary>
		/// If this flag is set, it indicates that for each codec type
		/// whose corresponding default codec (i.e. AVOutputFormat.audio_codec,
		/// AVOutputFormat.video_codec and AVOutputFormat.subtitle_codec)
		/// is set (i.e. != AV_CODEC_ID_NONE) only one stream of this type
		/// can be muxed. It furthermore indicates that no stream with
		/// a codec type that has no default codec or whose default codec
		/// is AV_CODEC_ID_NONE can be muxed.
		/// Both of these restrictions are checked generically before
		/// the actual muxer's init/write_header callbacks
		/// </summary>
		Max_One_Of_Each = 1 << 2,

		/// <summary>
		/// If this flag is set, then the only permitted audio/video/subtitle
		/// codec ids are AVOutputFormat.audio/video/subtitle_codec;
		/// if any of the latter is unset (i.e. equal to AV_CODEC_ID_NONE),
		/// then no stream of the corresponding type is supported.
		/// In addition, codec types without default codec field
		/// are disallowed
		/// </summary>
		Only_Default_Codects = 1 << 3
	}
}
