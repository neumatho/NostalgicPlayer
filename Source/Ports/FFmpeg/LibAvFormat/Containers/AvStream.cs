/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// Stream structure.
	/// New fields can be added to the end with minor version bumps.
	/// Removal, reordering and changes to existing fields require a major
	/// version bump.
	/// sizeof(AVStream) must not be used outside libav*
	/// </summary>
	public class AvStream : AvClass
	{
		/// <summary>
		/// A class for avoptions. Set on stream creation
		/// </summary>
		public AvClass Av_Class => this;

		/// <summary>
		/// Stream index in AVFormatContext
		/// </summary>
		public c_int Index;

		/// <summary>
		/// Format-specific stream ID.
		/// decoding: set by libavformat
		/// encoding: set by the user, replaced by libavformat if left unset
		/// </summary>
		public c_int Id;

		/// <summary>
		/// Codec parameters associated with this stream. Allocated and freed by
		/// libavformat in avformat_new_stream() and avformat_free_context()
		/// respectively.
		///
		/// - demuxing: filled by libavformat on stream creation or in
		///             avformat_find_stream_info()
		/// - muxing: filled by the caller before avformat_write_header()
		/// </summary>
		public AvCodecParameters CodecPar;

		/// <summary>
		/// 
		/// </summary>
		public IOpaque Priv_Data;

		/// <summary>
		/// This is the fundamental unit of time (in seconds) in terms
		/// of which frame timestamps are represented.
		///
		/// decoding: set by libavformat
		/// encoding: May be set by the caller before avformat_write_header() to
		///           provide a hint to the muxer about the desired timebase. In
		///           avformat_write_header(), the muxer will overwrite this field
		///           with the timebase that will actually be used for the timestamps
		///           written into the file (which may or may not be related to the
		///           user-provided one, depending on the format)
		/// </summary>
		public AvRational Time_Base;

		/// <summary>
		/// Decoding: pts of the first frame of the stream in presentation order, in stream time base.
		/// Only set this if you are absolutely 100% sure that the value you set
		/// it to really is the pts of the first frame.
		/// This may be undefined (AV_NOPTS_VALUE).
		/// Note: The ASF header does NOT contain a correct start_time the ASF
		/// demuxer must NOT set this
		/// </summary>
		public int64_t Start_Time;

		/// <summary>
		/// Decoding: duration of the stream, in stream time base.
		/// If a source file does not specify a duration, but does specify
		/// a bitrate, this value will be estimated from bitrate and file size.
		///
		/// Encoding: May be set by the caller before avformat_write_header() to
		/// provide a hint to the muxer about the estimated duration
		/// </summary>
		public int64_t Duration;

		/// <summary>
		/// Number of frames in this stream if known or 0
		/// </summary>
		public int64_t Nb_Frames;

		/// <summary>
		/// Stream disposition - a combination of AV_DISPOSITION_* flags.
		/// - demuxing: set by libavformat when creating the stream or in
		///             avformat_find_stream_info().
		/// - muxing: may be set by the caller before avformat_write_header()
		/// </summary>
		public AvDisposition Disposition;

		/// <summary>
		/// Selects which packets can be discarded at will and do not need to be demuxed
		/// </summary>
		public AvDiscard Discard;

		/// <summary>
		/// sample aspect ratio (0 if unknown)
		/// - encoding: Set by user
		/// - decoding: Set by libavformat
		/// </summary>
		public AvRational Sample_Aspects_Ratio;

		/// <summary>
		/// 
		/// </summary>
		public AvDictionary Metadata;

		/// <summary>
		/// Average framerate
		///
		/// - demuxing: May be set by libavformat when creating the stream or in
		///             avformat_find_stream_info()
		/// - muxing: May be set by the caller before avformat_write_header()
		/// </summary>
		public AvRational Avg_Frame_Rate;

		/// <summary>
		/// For streams with AV_DISPOSITION_ATTACHED_PIC disposition, this packet
		/// will contain the attached picture.
		///
		/// decoding: set by libavformat, must not be modified by the caller
		/// encoding: unused
		/// </summary>
		public readonly AvPacket Attached_Pic = new AvPacket();

		/// <summary>
		/// Flags indicating events happening on the stream, a combination of
		/// AVSTREAM_EVENT_FLAG_*.
		///
		/// - demuxing: may be set by the demuxer in avformat_open_input(),
		///   avformat_find_stream_info() and av_read_frame(). Flags must be cleared
		///   by the user once the event has been handled.
		/// - muxing: may be set by the user after avformat_write_header(). to
		///   indicate a user-triggered event.  The muxer will clear the flags for
		///   events it has handled in av_[interleaved]_write_frame()
		/// </summary>
		public AvStreamEventFlag Event_Flags;

		/// <summary>
		/// Real base framerate of the stream.
		/// This is the lowest framerate with which all timestamps can be
		/// represented accurately (it is the least common multiple of all
		/// framerates in the stream). Note, this value is just a guess!
		/// For example, if the time base is 1/90000 and all frames have either
		/// approximately 3600 or 1800 timer ticks, then r_frame_rate will be 50/1
		/// </summary>
		public AvRational R_Frame_Rate;

		/// <summary>
		/// Number of bits in timestamps. Used for wrapping control.
		///
		/// - demuxing: set by libavformat
		/// - muxing: set by libavformat
		/// </summary>
		public c_int Pts_Wrap_Bits;
	}
}
