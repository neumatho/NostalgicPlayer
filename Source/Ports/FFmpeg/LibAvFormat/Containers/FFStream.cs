/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class FFStream : AvStream
	{
		/// <summary>
		/// 
		/// </summary>
		public const c_int Max_Reorder_Delay = 16;

		/// <summary>
		/// The public context
		/// </summary>
		public AvStream Pub => this;

		/// <summary>
		/// 
		/// </summary>
		public AvFormatContext FmtCtx;

		// <summary>
		// Set to 1 if the codec allows reordering, so pts can be different
		// from dts
		// </summary>
//		public c_int Reorder;

		/// <summary>
		/// Bitstream filter to run on stream
		/// - encoding: Set by muxer using ff_stream_add_bitstream_filter
		/// - decoding: unused
		/// </summary>
		public AvBsfContext BSfc;

		// <summary>
		// Whether or not check_bitstream should still be run on each packet
		// </summary>
//		public c_int Bitstream_Checked;

		/// <summary>
		/// The codec context used by avformat_find_stream_info, the parser, etc.
		/// </summary>
		public AvCodecContext AvCtx;

		/// <summary>
		/// 1 if avctx has been initialized with the values from the codec parameters
		/// </summary>
		public c_int AvCtx_Inited;

		/// <summary>
		/// The context for extracting extradata in find_stream_info()
		/// inited=1/bsf=NULL signals that extracting is not possible (codec not
		/// supported)
		/// </summary>
		public (
			AvBsfContext Bsf,
			c_int Inited
		) Extract_ExtraData;

		/// <summary>
		/// Whether the internal avctx needs to be updated from codecpar (after a late change to codecpar)
		/// </summary>
		public c_int Need_Context_Update;

		// <summary>
		// 
		// </summary>
//		public c_int Is_Intra_Only;

		// <summary>
		// 
		// </summary>
//		public FFFrac Priv_Pts;

		/// <summary>
		/// Stream information used internally by avformat_find_stream_info()
		/// </summary>
		public FFStreamInfo Info;

		/// <summary>
		/// Only used if the format does not support seeking natively
		/// </summary>
		public CPointer<AvIndexEntry> Index_Entries;

		/// <summary>
		/// 
		/// </summary>
		public c_int Nb_Index_Entries;

		/// <summary>
		/// 
		/// </summary>
		public c_uint Index_Entries_Allocated_Size;

		// <summary>
		// 
		// </summary>
//		public int64_t Interleaver_Chunk_Size;

		// <summary>
		// 
		// </summary>
//		public int64_t Interleaver_Chunk_Duration;

		/// <summary>
		/// Stream probing state
		/// -1   -> probing finished
		///  0   -> no probing requested
		/// rest -> perform probing with request_probe being the minimum score to accept
		/// </summary>
		public c_int Request_Probe;

		/// <summary>
		/// Indicates that everything up to the next keyframe
		/// should be discarded
		/// </summary>
		public c_int Skip_To_Keyframe;

		/// <summary>
		/// Number of samples to skip at the start of the frame decoded from the next packet
		/// </summary>
		public c_int Skip_Samples;

		/// <summary>
		/// If not 0, the number of samples that should be skipped from the start of
		/// the stream (the samples are removed from packets with pts==0, which also
		/// assumes negative timestamps do not happen).
		/// Intended for use with formats such as mp3 with ad-hoc gapless audio
		/// support
		/// </summary>
		public int64_t Start_Skip_Samples = 0;

		/// <summary>
		/// If not 0, the first audio sample that should be discarded from the stream.
		/// This is broken by design (needs global sample count), but can't be
		/// avoided for broken by design formats such as mp3 with ad-hoc gapless
		/// audio support
		/// </summary>
		public int64_t First_Discard_Sample = 0;

		/// <summary>
		/// The sample after last sample that is intended to be discarded after
		/// first_discard_sample. Works on frame boundaries only. Used to prevent
		/// early EOF if the gapless info is broken (considered concatenated mp3s)
		/// </summary>
		public int64_t Last_Discard_Sample = 0;

		/// <summary>
		/// Number of internally decoded frames, used internally in libavformat, do not access
		/// its lifetime differs from info which is why it is not in that structure
		/// </summary>
		public c_int Nb_Decoded_Frames;

		// <summary>
		// Timestamp offset added to timestamps before muxing
		// </summary>
//		public int64_t Mux_Ts_Offset;

		// <summary>
		// This is the lowest ts allowed in this track; it may be set by the muxer
		// during init or write_header and influences the automatic timestamp
		// shifting code
		// </summary>
//		public int64_t Lowest_Ts_Allowed;

		/// <summary>
		/// Internal data to check for wrapping of the time stamp
		/// </summary>
		public int64_t Pts_Wrap_Reference;

		/// <summary>
		/// Options for behavior, when a wrap is detected.
		///
		/// Defined by AV_PTS_WRAP_ values.
		///
		/// If correction is enabled, there are two possibilities:
		/// If the first time stamp is near the wrap point, the wrap offset
		/// will be subtracted, which will create negative time stamps.
		/// Otherwise the offset will be added
		/// </summary>
		public AvPtsWrap Pts_Wrap_Behavior;

		/// <summary>
		/// Internal data to prevent doing update_initial_durations() twice
		/// </summary>
		public c_int Update_Initial_Durations_Done;

		/// <summary>
		/// Internal data to generate dts from pts
		/// </summary>
		public readonly int64_t[] Pts_Reorder_Error = new int64_t[Max_Reorder_Delay + 1];

		/// <summary>
		/// 
		/// </summary>
		public readonly uint8_t[] Pts_Reorder_Error_Count = new uint8_t[Max_Reorder_Delay + 1];

		/// <summary>
		/// 
		/// </summary>
		public readonly int64_t[] Pts_Buffer = new int64_t[Max_Reorder_Delay + 1];

		/// <summary>
		/// Internal data to analyze DTS and detect faulty mpeg streams
		/// </summary>
		public int64_t Last_Dts_For_Order_Check;

		/// <summary>
		/// 
		/// </summary>
		public uint8_t Dts_Ordered;

		/// <summary>
		/// 
		/// </summary>
		public uint8_t Dts_Misordered;

		/// <summary>
		/// Display aspect ratio (0 if unknown)
		/// - encoding: unused
		/// - decoding: Set by libavformat to calculate sample_aspect_ratio internally
		/// </summary>
		public readonly AvRational Display_Aspect_Ratio = new AvRational();

		/// <summary>
		/// 
		/// </summary>
		public readonly AvProbeData Probe_Data = new AvProbeData();

		// <summary>
		// Last packet in packet_buffer for this stream when muxing
		// </summary>
//		public PacketListEntry Last_In_Packet_Buffer;

		/// <summary>
		/// 
		/// </summary>
		public int64_t Last_IP_Pts;

		/// <summary>
		/// 
		/// </summary>
		public c_int Last_IP_Duration;

		/// <summary>
		/// Number of packets to buffer for codec probing
		/// </summary>
		public c_int Probe_Packets;

		/// <summary>
		/// av_read_frame() support
		/// </summary>
		public AvStreamParseType Need_Parsing;

		/// <summary>
		/// 
		/// </summary>
		public AvCodecParserContext Parser;

		/// <summary>
		/// Number of frames that have been demuxed during avformat_find_stream_info()
		/// </summary>
		public c_int Codec_Info_Nb_Frames;

		// <summary>
		// Stream Identifier
		// This is the MPEG-TS stream identifier +1
		// 0 means unknown
		// </summary>
//		public c_int Stream_Identifier;

		/// <summary>
		/// Timestamp generation support:
		///
		/// Timestamp corresponding to the last dts sync point.
		///
		/// Initialized when AVCodecParserContext.dts_sync_point >= 0 and
		/// a DTS is received from the underlying container. Otherwise set to
		/// AV_NOPTS_VALUE by default
		/// </summary>
		public int64_t First_Dts;

		/// <summary>
		/// 
		/// </summary>
		public int64_t Cur_Dts;

		/// <summary>
		/// 
		/// </summary>
		public AvCodecDescriptor Codec_Desc;
	}
}
