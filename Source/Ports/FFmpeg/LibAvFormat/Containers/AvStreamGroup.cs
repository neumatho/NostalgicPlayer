/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class AvStreamGroup : AvClass
	{
		/// <summary>
		/// A class for avoptions. Set by avformat_stream_group_create()
		/// </summary>
		public AvClass Av_Class => this;

		/// <summary>
		/// 
		/// </summary>
		public IOpaque Priv_Data;

		/// <summary>
		/// Group index in AVFormatContext
		/// </summary>
		public c_uint Index;

		/// <summary>
		/// Group type-specific group ID.
		///
		/// decoding: set by libavformat
		/// encoding: may set by the user
		/// </summary>
		public int64_t Id;

		/// <summary>
		/// Group type
		///
		/// decoding: set by libavformat on group creation
		/// encoding: set by avformat_stream_group_create()
		/// </summary>
		public AvStreamGroupParamsType Type;

		/// <summary>
		/// Group type-specific parameters
		/// </summary>
		public IGroupType Params;

		/// <summary>
		/// Metadata that applies to the whole group.
		///
		/// - demuxing: set by libavformat on group creation
		/// - muxing: may be set by the caller before avformat_write_header()
		///
		/// Freed by libavformat in avformat_free_context()
		/// </summary>
		public AvDictionary Metadata;

		/// <summary>
		/// Number of elements in AVStreamGroup.streams.
		///
		/// Set by avformat_stream_group_add_stream() must not be modified by any other code
		/// </summary>
		public c_uint Nb_Streams;

		/// <summary>
		/// A list of streams in the group. New entries are created with
		/// avformat_stream_group_add_stream().
		///
		/// - demuxing: entries are created by libavformat on group creation.
		///             If AVFMTCTX_NOHEADER is set in ctx_flags, then new entries may also
		///             appear in av_read_frame().
		/// - muxing: entries are created by the user before avformat_write_header().
		///
		/// Freed by libavformat in avformat_free_context()
		/// </summary>
		public CPointer<AvStream> Streams;

		/// <summary>
		/// Stream group disposition - a combination of AV_DISPOSITION_* flags.
		/// This field currently applies to all defined AVStreamGroupParamsType.
		///
		/// - demuxing: set by libavformat when creating the group or in
		///             avformat_find_stream_info().
		/// - muxing: may be set by the caller before avformat_write_header()
		/// </summary>
		public c_int Disposition;
	}
}
