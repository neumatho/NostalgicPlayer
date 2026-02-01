/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class AvCodecParserContext
	{
		/// <summary></summary>
		public const c_int Av_Parser_Pts_Nb = 4;

		/// <summary>
		/// 
		/// </summary>
		public IPrivateData Priv_Data;

		/// <summary>
		/// 
		/// </summary>
		public AvCodecParser Parser;

		/// <summary>
		/// Offset of the current frame
		/// </summary>
		public int64_t Frame_Offset;

		/// <summary>
		/// Current offset
		/// (incremented by each av_parser_parse())
		/// </summary>
		public int64_t Cur_Offset;

		/// <summary>
		/// Offset of the next frame
		/// </summary>
		public int64_t Next_Frame_Offset;

		/// <summary>
		/// 
		/// </summary>
		public AvPictureType Pict_Type;

		/// <summary>
		/// This field is used for proper frame duration computation in lavf.
		/// It signals, how much longer the frame duration of the current frame
		/// is compared to normal frame duration.
		///
		/// frame_duration = (1 + repeat_pict) * time_base
		///
		/// It is used by codecs like H.264 to display telecined material
		/// </summary>
		public c_int Repeat_Pict;

		/// <summary>
		/// Pts of the current frame
		/// </summary>
		public int64_t Pts;

		/// <summary>
		/// Dts of the current frame
		/// </summary>
		public int64_t Dts;

		/// <summary>
		/// 
		/// </summary>
		public int64_t Last_Pts;

		/// <summary>
		/// 
		/// </summary>
		public int64_t Last_Dts;

		/// <summary>
		/// 
		/// </summary>
		public c_int Fetch_Timestamp;

		/// <summary>
		/// 
		/// </summary>
		public c_int Cur_Frame_Start_Index;

		/// <summary>
		/// 
		/// </summary>
		public readonly int64_t[] Cur_Frame_Offset = new int64_t[Av_Parser_Pts_Nb];

		/// <summary>
		/// 
		/// </summary>
		public readonly int64_t[] Cur_Frame_Pts = new int64_t[Av_Parser_Pts_Nb];

		/// <summary>
		/// 
		/// </summary>
		public readonly int64_t[] Cur_Frame_Dts = new int64_t[Av_Parser_Pts_Nb];

		/// <summary>
		/// 
		/// </summary>
		public ParserFlag Flags;

		/// <summary>
		/// Byte offset from starting packet start
		/// </summary>
		public int64_t Offset;

		/// <summary>
		/// 
		/// </summary>
		public readonly int64_t[] Cur_Frame_End = new int64_t[Av_Parser_Pts_Nb];

		/// <summary>
		/// Set by parser to 1 for key frames and 0 for non-key frames.
		/// It is initialized to -1, so if the parser doesn't set this flag,
		/// old-style fallback using AV_PICTURE_TYPE_I picture type as key frames
		/// will be used
		/// </summary>
		public c_int Key_Frame;

		/// <summary>
		/// Synchronization point for start of timestamp generation.
		///
		/// Set to ›0 for sync point, 0 for no sync point and ‹0 for undefined
		/// (default).
		///
		/// For example, this corresponds to presence of H.264 buffering period
		/// SEI message
		/// </summary>
		public c_int Dts_Sync_Point;

		/// <summary>
		/// Offset of the current timestamp against last timestamp sync point in
		/// units of AVCodecContext.time_base.
		///
		/// Set to INT_MIN when dts_sync_point unused. Otherwise, it must
		/// contain a valid timestamp offset.
		///
		/// Note that the timestamp of sync point has usually a nonzero
		/// dts_ref_dts_delta, which refers to the previous sync point. Offset of
		/// the next frame after timestamp sync point will be usually 1.
		///
		/// For example, this corresponds to H.264 cpb_removal_delay
		/// </summary>
		public c_int Dts_Ref_Dts_Delta;

		/// <summary>
		/// Presentation delay of current frame in units of AVCodecContext.time_base.
		///
		/// Set to INT_MIN when dts_sync_point unused. Otherwise, it must
		/// contain valid non-negative timestamp delta (presentation time of a frame
		/// must not lie in the past).
		///
		/// This delay represents the difference between decoding and presentation
		/// time of the frame.
		///
		/// For example, this corresponds to H.264 dpb_output_delay
		/// </summary>
		public c_int Pts_Dts_Delta;

		/// <summary>
		/// Position of the packet in file.
		///
		/// Analogous to cur_frame_pts/dts
		/// </summary>
		public readonly int64_t[] Cur_Frame_Pos = new int64_t[Av_Parser_Pts_Nb];

		/// <summary>
		/// Byte position of currently parsed frame in stream
		/// </summary>
		public int64_t Pos;

		/// <summary>
		/// Previous frame byte position
		/// </summary>
		public int64_t Last_Pos;

		/// <summary>
		/// Duration of the current frame.
		/// For audio, this is in units of 1 / AVCodecContext.sample_rate.
		/// For all other types, this is in units of AVCodecContext.time_base
		/// </summary>
		public c_int Duration;

		/// <summary>
		/// 
		/// </summary>
		public AvFieldOrder Field_Order;

		/// <summary>
		/// Indicate whether a picture is coded as a frame, top field or bottom field.
		///
		/// For example, H.264 field_pic_flag equal to 0 corresponds to
		/// AV_PICTURE_STRUCTURE_FRAME. An H.264 picture with field_pic_flag
		/// equal to 1 and bottom_field_flag equal to 0 corresponds to
		/// AV_PICTURE_STRUCTURE_TOP_FIELD
		/// </summary>
		public AvPictureStructure Picture_Structure;

		/// <summary>
		/// Picture number incremented in presentation or output order.
		/// This field may be reinitialized at the first picture of a new sequence.
		///
		/// For example, this corresponds to H.264 PicOrderCnt
		/// </summary>
		public c_int Output_Picture_Number;

		/// <summary>
		/// Dimensions of the decoded video intended for presentation
		/// </summary>
		public c_int Width;

		/// <summary>
		/// 
		/// </summary>
		public c_int Height;

		/// <summary>
		/// Dimensions of the coded video
		/// </summary>
		public c_int Coded_Width;

		/// <summary>
		/// 
		/// </summary>
		public c_int Coded_Height;

		/// <summary>
		/// The format of the coded data, corresponds to enum AVPixelFormat for video
		/// and for enum AVSampleFormat for audio.
		///
		/// Note that a decoder can have considerable freedom in how exactly it
		/// decodes the data, so the format reported here might be different from the
		/// one returned by a decoder
		/// </summary>
		public FormatUnion Format;
	}
}
