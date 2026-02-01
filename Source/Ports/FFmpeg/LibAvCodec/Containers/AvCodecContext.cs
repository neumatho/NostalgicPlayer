/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// Main external API structure.
	/// New fields can be added to the end with minor version bumps.
	/// Removal, reordering and changes to existing fields require a major
	/// version bump.
	/// You can use AVOptions (av_opt* / av_set/get*()) to access these fields from user
	/// applications.
	/// The name string for AVOptions options matches the associated command line
	/// parameter name and can be found in libavcodec/options_table.h
	/// The AVOption/command line parameter names differ in some cases from the C
	/// structure field names for historic reasons or brevity.
	/// sizeof(AVCodecContext) must not be used outside libav*
	/// </summary>
	public class AvCodecContext : AvClass, IOpaque, IDeepCloneable<AvCodecContext>
	{
		/// <summary>
		/// Information on struct for av_log
		/// - set by avcodec_alloc_context3
		/// </summary>
		public AvClass Av_Class => this;

		/// <summary>
		/// 
		/// </summary>
		public c_int Log_Level_Offset;

		/// <summary>
		/// 
		/// </summary>
		public AvMediaType Codec_Type;

		/// <summary>
		/// 
		/// </summary>
		public AvCodec Codec;

		/// <summary>
		/// 
		/// </summary>
		public AvCodecId Codec_Id;

		/// <summary>
		/// fourcc (LSB first, so "ABCD" -› ('D'‹‹24) + ('C'‹‹16) + ('B'‹‹8) + 'A').
		/// This is used to work around some encoder bugs.
		/// A demuxer should set this to what is stored in the field used to identify the codec.
		/// If there are multiple such fields in a container then the demuxer should choose the one
		/// which maximizes the information about the used codec.
		/// If the codec tag field in a container is larger than 32 bits then the demuxer should
		/// remap the longer ID to 32 bits with a table or other structure. Alternatively a new
		/// extra_codec_tag + size could be added but for this a clear advantage must be demonstrated
		/// first.
		/// - encoding: Set by user, if not then the default based on codec_id will be used
		/// - decoding: Set by user, will be converted to uppercase by libavcodec during init
		/// </summary>
		public c_uint Codec_Tag;

		/// <summary>
		/// 
		/// </summary>
		public IPrivateData Priv_Data;

		/// <summary>
		/// Private context used for internal data.
		///
		/// Unlike priv_data, this is not codec-specific. It is used in general
		/// libavcodec functions
		/// </summary>
		internal AvCodecInternal Internal;

		/// <summary>
		/// Private data of the user, can be used to carry app specific stuff.
		/// - encoding: Set by user
		/// - decoding: Set by user
		/// </summary>
		public IOpaque Opaque;

		/// <summary>
		/// The average bitrate
		/// - encoding: Set by user; unused for constant quantizer encoding
		/// - decoding: Set by user, may be overwritten by libavcodec
		///             if this info is available in the stream
		/// </summary>
		public int64_t Bit_Rate;

		/// <summary>
		/// AV_CODEC_FLAG_*.
		/// - encoding: Set by user
		/// - decoding: Set by user
		/// </summary>
		public AvCodecFlag Flags;

		/// <summary>
		/// AV_CODEC_FLAG2_*
		/// - encoding: Set by user
		/// - decoding: Set by user
		/// </summary>
		public AvCodecFlag2 Flags2;

		/// <summary>
		/// Out-of-band global headers that may be used by some codecs.
		///
		/// - decoding: Should be set by the caller when available (typically from a
		///   demuxer) before opening the decoder; some decoders require this to be
		///   set and will fail to initialize otherwise.
		///
		///   The array must be allocated with the av_malloc() family of functions;
		///   allocated size must be at least AV_INPUT_BUFFER_PADDING_SIZE bytes
		///   larger than extradata_size
		///
		/// - encoding: May be set by the encoder in avcodec_open2() (possibly
		///   depending on whether the AV_CODEC_FLAG_GLOBAL_HEADER flag is set)
		///
		/// After being set, the array is owned by the codec and freed in
		/// avcodec_free_context()
		/// </summary>
		public IDataContext ExtraData;

		/// <summary>
		/// This is the fundamental unit of time (in seconds) in terms
		/// of which frame timestamps are represented. For fixed-fps content,
		/// timebase should be 1/framerate and timestamp increments should be
		/// identically 1.
		/// This often, but not always is the inverse of the frame rate or field rate
		/// for video. 1/time_base is not the average frame rate if the frame rate is not
		/// constant.
		///
		/// Like containers, elementary streams also can store timestamps, 1/time_base
		/// is the unit in which these timestamps are specified.
		/// As example of such codec time base see ISO/IEC 14496-2:2001(E)
		/// vop_time_increment_resolution and fixed_vop_rate
		/// (fixed_vop_rate == 0 implies that it is different from the framerate)
		///
		/// - encoding: MUST be set by user
		/// - decoding: unused
		/// </summary>
		public AvRational Time_Base;

		/// <summary>
		/// Timebase in which pkt_dts/pts and AVPacket.dts/pts are expressed.
		/// - encoding: unused
		/// - decoding: set by user
		/// </summary>
		public AvRational Pkt_TimeBase;

		/// <summary>
		/// - decoding: For codecs that store a framerate value in the compressed
		///             bitstream, the decoder may export it here. { 0, 1} when
		///             unknown
		/// - encoding: May be used to signal the framerate of CFR content to an
		///             encoder
		/// </summary>
		public AvRational FrameRate;

		/// <summary>
		/// Codec delay.
		///
		/// Encoding: Number of frames delay there will be from the encoder input to
		///           the decoder output. (we assume the decoder matches the spec)
		/// Decoding: Number of frames delay in addition to what a standard decoder
		///           as specified in the spec would produce
		///
		/// Video:
		///   Number of frames the decoded output will be delayed relative to the
		///   encoded input.
		///
		/// Audio:
		///   For encoding, this field is unused (see initial_padding).
		///
		///   For decoding, this is the number of samples the decoder needs to
		///   output before the decoder's output is valid. When seeking, you should
		///   start decoding this many samples prior to your desired seek point.
		///
		/// - encoding: Set by libavcodec
		/// - decoding: Set by libavcodec
		/// </summary>
		public c_int Delay;

		/// <summary>
		/// Picture width / height.
		///
		/// Note: Those fields may not match the values of the last
		/// AVFrame output by avcodec_receive_frame() due frame
		/// reordering.
		///
		/// - encoding: MUST be set by user
		/// - decoding: May be set by the user before opening the decoder if known e.g.
		///             from the container. Some decoders will require the dimensions
		///             to be set by the caller. During decoding, the decoder may
		///             overwrite those values as required while parsing the data
		/// </summary>
		public SizeInfo PictureSize;

		/// <summary>
		/// Bitstream width / height, may be different from width/height e.g. when
		/// the decoded frame is cropped before being output or lowres is enabled.
		///
		/// Note: Those field may not match the value of the last
		/// AVFrame output by avcodec_receive_frame() due frame
		/// reordering.
		///
		/// - encoding: unused
		/// - decoding: May be set by the user before opening the decoder if known
		///             e.g. from the container. During decoding, the decoder may
		///             overwrite those values as required while parsing the data
		/// </summary>
		public c_int Coded_Width;

		/// <summary>
		/// 
		/// </summary>
		public c_int Coded_Height;

		/// <summary>
		/// Sample aspect ratio (0 if unknown)
		/// That is the width of a pixel divided by the height of the pixel.
		/// Numerator and denominator must be relatively prime and smaller than 256 for some video standards.
		/// - encoding: Set by user
		/// - decoding: Set by libavcodec
		/// </summary>
		public AvRational Sample_Aspect_Ratio;

		/// <summary>
		/// Pixel format, see AV_PIX_FMT_xxx.
		/// May be set by the demuxer if known from headers.
		/// May be overridden by the decoder if it knows better.
		///
		/// Note: This field may not match the value of the last
		/// AVFrame output by avcodec_receive_frame() due frame
		/// reordering.
		///
		/// - encoding: Set by user
		/// - decoding: Set by user if known, overridden by libavcodec while
		///             parsing the data
		/// </summary>
		public AvPixelFormat Pix_Fmt;

		/// <summary>
		/// Nominal unaccelerated pixel format, see AV_PIX_FMT_xxx.
		/// - encoding: unused
		/// - decoding: Set by libavcodec before calling get_format()
		/// </summary>
		public AvPixelFormat Sw_Pix_Fmt;

		/// <summary>
		/// Chromaticity coordinates of the source primaries.
		/// - encoding: Set by user
		/// - decoding: Set by libavcodec
		/// </summary>
		public AvColorPrimaries Color_Primaries;

		/// <summary>
		/// Color Transfer Characteristic.
		/// - encoding: Set by user
		/// - decoding: Set by libavcodec
		/// </summary>
		public AvColorTransferCharacteristic Color_Trc;

		/// <summary>
		/// YUV colorspace type.
		/// - encoding: Set by user
		/// - decoding: Set by libavcodec
		/// </summary>
		public AvColorSpace ColorSpace;

		/// <summary>
		/// MPEG vs JPEG YUV range.
		/// - encoding: Set by user to override the default output color range value,
		///   If not specified, libavcodec sets the color range depending on the
		///   output format
		/// - decoding: Set by libavcodec, can be set by the user to propagate the
		///   color range to components reading from the decoder context
		/// </summary>
		public AvColorRange Color_Range;

		/// <summary>
		/// This defines the location of chroma samples.
		/// - encoding: Set by user
		/// - decoding: Set by libavcodec
		/// </summary>
		public AvChromaLocation Chroma_Sample_Location;

		/// <summary>
		/// Field order
		/// - encoding: set by libavcodec
		/// - decoding: Set by user
		/// </summary>
		public AvFieldOrder Field_Order;

		/// <summary>
		/// Number of reference frames
		/// - encoding: Set by user
		/// - decoding: Set by lavc
		/// </summary>
		public c_int Refs;

		/// <summary>
		/// Size of the frame reordering buffer in the decoder.
		/// For MPEG-2 it is 1 IPB or 0 low delay IP.
		/// - encoding: Set by libavcodec
		/// - decoding: Set by libavcodec
		/// </summary>
		public c_int Has_B_Frames;

		/// <summary>
		/// Slice flags
		/// - encoding: unused
		/// - decoding: Set by user
		/// </summary>
		public SliceFlag Slice_Flags;

		/// <summary>
		/// If non NULL, 'draw_horiz_band' is called by the libavcodec
		/// decoder to draw a horizontal band. It improves cache usage. Not
		/// all codecs can do that. You must check the codec capabilities
		/// beforehand.
		/// When multithreading is used, it may be called from multiple threads
		/// at the same time; threads might draw different parts of the same AVFrame,
		/// or multiple AVFrames, and there is no guarantee that slices will be drawn
		/// in order.
		/// The function is also used by hardware acceleration APIs.
		/// It is called at least once during frame decoding to pass
		/// the data needed for hardware render.
		/// In that mode instead of pixel data, AVFrame points to
		/// a structure specific to the acceleration API. The application
		/// reads the structure and can change some fields to indicate progress
		/// or mark state.
		/// - encoding: unused
		/// - decoding: Set by user
		/// </summary>
		public CodecFunc.Draw_Horiz_Band_Delegate Draw_Horiz_Band;

		/// <summary>
		/// Callback to negotiate the pixel format. Decoding only, may be set by the
		/// caller before avcodec_open2().
		///
		/// Called by some decoders to select the pixel format that will be used for
		/// the output frames. This is mainly used to set up hardware acceleration,
		/// then the provided format list contains the corresponding hwaccel pixel
		/// formats alongside the "software" one. The software pixel format may also
		/// be retrieved from sw_pix_fmt.
		///
		/// This callback will be called when the coded frame properties (such as
		/// resolution, pixel format, etc.) change and more than one output format is
		/// supported for those new properties. If a hardware pixel format is chosen
		/// and initialization for it fails, the callback may be called again
		/// immediately.
		///
		/// This callback may be called from different threads if the decoder is
		/// multi-threaded, but not from more than one thread simultaneously
		/// </summary>
		public CodecFunc.Get_Format_Delegate Get_Format;

		/// <summary>
		/// Maximum number of B-frames between non-B-frames
		/// Note: The output will be delayed by max_b_frames+1 relative to the input.
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_int Max_B_Frames;

		/// <summary>
		/// Qscale factor between IP and B-frames
		/// If › 0 then the last P-frame quantizer will be used (q= lastp_q*factor+offset).
		/// If ‹ 0 then normal ratecontrol will be done (q= -normal_q*factor+offset).
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_float B_Quant_Factor;

		/// <summary>
		/// Qscale offset between IP and B-frames
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_float B_Quant_Offset;

		/// <summary>
		/// Qscale factor between P- and I-frames
		/// If › 0 then the last P-frame quantizer will be used (q = lastp_q * factor + offset).
		/// If ‹ 0 then normal ratecontrol will be done (q= -normal_q*factor+offset).
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_float I_Quant_Factor;

		/// <summary>
		/// Qscale offset between P and I-frames
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_float I_Quant_Offset;

		/// <summary>
		/// Luminance masking (0-> disabled)
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_float Lumi_Masking;

		/// <summary>
		/// Temporary complexity masking (0-> disabled)
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_float Temporal_Cplx_Masking;

		/// <summary>
		/// Spatial complexity masking (0-> disabled)
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_float Spatial_Cplx_Masking;

		/// <summary>
		/// P block masking (0-> disabled)
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_float P_Masking;

		/// <summary>
		/// Darkness masking (0-> disabled)
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_float Dark_Masking;

		/// <summary>
		/// Noise vs. sse weight for the nsse comparison function
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_int Nsse_Weight;

		/// <summary>
		/// Motion estimation comparison function
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_int Me_Cmp;

		/// <summary>
		/// Subpixel motion estimation comparison function
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_int Me_Sub_Cmp;

		/// <summary>
		/// Macroblock comparison function (not supported yet)
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_int Mb_Cmp;

		/// <summary>
		/// Interlaced DCT comparison function
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public FFCmp Ildct_Cmp;

		/// <summary>
		/// ME diamond size and shape
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_int Dia_Size;

		/// <summary>
		/// Amount of previous MV predictors (2a+1 x 2a+1 square)
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_int Last_Predictor_Count;

		/// <summary>
		/// Motion estimation prepass comparison function
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_int Me_Pre_Cmp;

		/// <summary>
		/// ME prepass diamond size and shape
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_int Me_Dia_Size;

		/// <summary>
		/// Subpel ME quality
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_int Me_SubPel_Quality;

		/// <summary>
		/// Maximum motion estimation search range in subpel units
		/// If 0 then no limit.
		///
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_int Me_Range;

		/// <summary>
		/// Macroblock decision mode
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public FFMbDecision Mb_Decision;

		/// <summary>
		/// Custom intra quantization matrix
		/// Must be allocated with the av_malloc() family of functions, and will be freed in
		/// avcodec_free_context().
		/// - encoding: Set/allocated by user, freed by libavcodec. Can be NULL
		/// - decoding: Set/allocated/freed by libavcodec
		/// </summary>
		public CPointer<uint16_t> Intra_Matrix;

		/// <summary>
		/// Custom inter quantization matrix
		/// Must be allocated with the av_malloc() family of functions, and will be freed in
		/// avcodec_free_context().
		/// - encoding: Set/allocated by user, freed by libavcodec. Can be NULL
		/// - decoding: Set/allocated/freed by libavcodec
		/// </summary>
		public CPointer<uint16_t> Inter_Matrix;

		/// <summary>
		/// Custom intra quantization matrix
		/// - encoding: Set by user, can be NULL
		/// - decoding: unused
		/// </summary>
		public CPointer<uint16_t> Chroma_Intra_Matrix;

		/// <summary>
		/// Precision of the intra DC coefficient - 8
		/// - encoding: Set by user
		/// - decoding: Set by libavcodec
		/// </summary>
		public c_int Intra_Dc_Precision;

		/// <summary>
		/// Minimum MB Lagrange multiplier
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_int Mb_LMin;

		/// <summary>
		/// Maximum MB Lagrange multiplier
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_int Mb_LMax;

		/// <summary>
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_int BiDir_Refine;

		/// <summary>
		/// Minimum GOP size
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_int KeyInt_Min;

		/// <summary>
		/// The number of pictures in a group of pictures, or 0 for intra_only
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_int Gop_Size;

		/// <summary>
		/// Note: Value depends upon the compare function used for fullpel ME.
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_int Mv0_Threshold;

		/// <summary>
		/// Number of slices.
		/// Indicates number of picture subdivisions. Used for parallelized
		/// decoding.
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_int Slices;

		/// <summary>
		/// Samples per second
		/// </summary>
		public c_int Sample_Rate;

		/// <summary>
		/// Audio sample format
		/// - encoding: Set by user
		/// - decoding: Set by libavcodec
		/// </summary>
		public AvSampleFormat Sample_Fmt;

		/// <summary>
		/// Audio channel layout.
		/// - encoding: must be set by the caller, to one of AVCodec.ch_layouts
		/// - decoding: may be set by the caller if known e.g. from the container.
		///             The decoder can then override during decoding as needed
		/// </summary>
		public readonly AvChannelLayout Ch_Layout = new AvChannelLayout();

		/// <summary>
		/// Number of samples per channel in an audio frame.
		///
		/// - encoding: set by libavcodec in avcodec_open2(). Each submitted frame
		///   except the last must contain exactly frame_size samples per channel.
		///   May be 0 when the codec has AV_CODEC_CAP_VARIABLE_FRAME_SIZE set, then the
		///   frame size is not restricted
		/// - decoding: may be set by some decoders to indicate constant frame size
		/// </summary>
		public c_int Frame_Size;

		/// <summary>
		/// Number of bytes per packet if constant and known or 0
		/// Used by some WAV based audio codecs
		/// </summary>
		public c_int Block_Align;

		/// <summary>
		/// Audio cutoff bandwidth (0 means "automatic")
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_int Cutoff;

		/// <summary>
		/// Type of service that the audio stream conveys.
		/// - encoding: Set by user
		/// - decoding: Set by libavcodec
		/// </summary>
		public AvAudioServiceType Audio_Service_Type;

		/// <summary>
		/// Desired sample format
		/// - encoding: Not used.
		/// - decoding: Set by user.
		/// Decoder will decode to this format if it can
		/// </summary>
		public AvSampleFormat Request_Sample_Fmt;

		/// <summary>
		/// Audio only. The number of "priming" samples (padding) inserted by the
		/// encoder at the beginning of the audio. I.e. this number of leading
		/// decoded samples must be discarded by the caller to get the original audio
		/// without leading padding.
		///
		/// - decoding: unused
		/// - encoding: Set by libavcodec. The timestamps on the output packets are
		///             adjusted by the encoder so that they always refer to the
		///             first sample of the data actually contained in the packet,
		///             including any added padding.  E.g. if the timebase is
		///             1/samplerate and the timestamp of the first input sample is
		///             0, the timestamp of the first output packet will be
		///             -initial_padding
		/// </summary>
		public c_int Initial_Padding;

		/// <summary>
		/// Audio only. The amount of padding (in samples) appended by the encoder to
		/// the end of the audio. I.e. this number of decoded samples must be
		/// discarded by the caller from the end of the stream to get the original
		/// audio without any trailing padding.
		///
		/// - decoding: unused
		/// - encoding: unused
		/// </summary>
		public c_int Trailing_Padding;

		/// <summary>
		/// Number of samples to skip after a discontinuity
		/// - decoding: unused
		/// - encoding: set by libavcodec
		/// </summary>
		public c_int Seek_Preroll;

		/// <summary>
		/// This callback is called at the beginning of each frame to get data
		/// buffer(s) for it. There may be one contiguous buffer for all the data or
		/// there may be a buffer per each data plane or anything in between. What
		/// this means is, you may set however many entries in buf[] you feel necessary.
		/// Each buffer must be reference-counted using the AVBuffer API (see description
		/// of buf[] below).
		///
		/// The following fields will be set in the frame before this callback is
		/// called:
		/// - format
		/// - width, height (video only)
		/// - sample_rate, channel_layout, nb_samples (audio only)
		/// Their values may differ from the corresponding values in
		/// AVCodecContext. This callback must use the frame values, not the codec
		/// context values, to calculate the required buffer size.
		///
		/// This callback must fill the following fields in the frame:
		/// - data[]
		/// - linesize[]
		/// - extended_data:
		///   * if the data is planar audio with more than 8 channels, then this
		///     callback must allocate and fill extended_data to contain all pointers
		///     to all data planes. data[] must hold as many pointers as it can.
		///     extended_data must be allocated with av_malloc() and will be freed in
		///     av_frame_unref().
		///   * otherwise extended_data must point to data
		/// - buf[] must contain one or more pointers to AVBufferRef structures. Each of
		///   the frame's data and extended_data pointers must be contained in these. That
		///   is, one AVBufferRef for each allocated chunk of memory, not necessarily one
		///   AVBufferRef per data[] entry. See: av_buffer_create(), av_buffer_alloc(),
		///   and av_buffer_ref().
		/// - extended_buf and nb_extended_buf must be allocated with av_malloc() by
		///   this callback and filled with the extra buffers if there are more
		///   buffers than buf[] can hold. extended_buf will be freed in
		///   av_frame_unref().
		///   Decoders will generally initialize the whole buffer before it is output
		///   but it can in rare error conditions happen that uninitialized data is passed
		///   through. Important: The buffers returned by get_buffer* should thus not contain sensitive
		///   data.
		///
		/// If AV_CODEC_CAP_DR1 is not set then get_buffer2() must call
		/// avcodec_default_get_buffer2() instead of providing buffers allocated by
		/// some other means.
		///
		/// Each data plane must be aligned to the maximum required by the target
		/// CPU.
		///
		/// See avcodec_default_get_buffer2()
		///
		/// Video:
		///
		/// If AV_GET_BUFFER_FLAG_REF is set in flags then the frame may be reused
		/// (read and/or written to if it is writable) later by libavcodec.
		///
		/// avcodec_align_dimensions2() should be used to find the required width and
		/// height, as they normally need to be rounded up to the next multiple of 16.
		///
		/// Some decoders do not support linesizes changing between frames.
		///
		/// If frame multithreading is used, this callback may be called from a
		/// different thread, but not from more than one at once. Does not need to be
		/// reentrant.
		///
		/// See avcodec_align_dimensions2()
		///
		/// Audio:
		///
		/// Decoders request a buffer of a particular size by setting
		/// AVFrame.nb_samples prior to calling get_buffer2(). The decoder may,
		/// however, utilize only part of the buffer by setting AVFrame.nb_samples
		/// to a smaller value in the output frame.
		///
		/// As a convenience, av_samples_get_buffer_size() and
		/// av_samples_fill_arrays() in libavutil may be used by custom get_buffer2()
		/// functions to find the required data size and to fill data pointers and
		/// linesize. In AVFrame.linesize, only linesize[0] may be set for audio
		/// since all planes must be the same size.
		///
		/// See av_samples_get_buffer_size(), av_samples_fill_arrays()
		///
		/// - encoding: unused
		/// - decoding: Set by libavcodec, user can override
		/// </summary>
		public CodecFunc.Get_Buffer2_Delegate Get_Buffer2;

		/// <summary>
		/// Number of bits the bitstream is allowed to diverge from the reference.
		///           the reference can be CBR (for CBR pass1) or VBR (for pass2)
		/// - encoding: Set by user; unused for constant quantizer encoding
		/// - decoding: unused
		/// </summary>
		public c_int Bit_Rate_Tolerance;

		/// <summary>
		/// Global quality for codecs which cannot change it per frame.
		/// This should be proportional to MPEG-1/2/4 qscale.
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_int Global_Quality;

		/// <summary>
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public FFCompression Compression_Level;

		/// <summary>
		/// Amount of qscale change between easy and hard scenes (0.0 - 1.0)
		/// </summary>
		public c_float QCompress;

		/// <summary>
		/// Amount of qscale smoothing over time (0.0 - 1.0)
		/// </summary>
		public c_float QBlur;

		/// <summary>
		/// Minimum quantizer
		/// - encoding: Set by user.
		/// - decoding: unused
		/// </summary>
		public c_int QMin;

		/// <summary>
		/// Maximum quantizer
		/// - encoding: Set by user.
		/// - decoding: unused
		/// </summary>
		public c_int QMax;

		/// <summary>
		/// Maximum quantizer difference between frames
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_int Max_QDiff;

		/// <summary>
		/// Decoder bitstream buffer size
		/// - encoding: Set by user
		/// - decoding: May be set by libavcodec
		/// </summary>
		public c_int Rc_Buffer_Size;

		/// <summary>
		/// Ratecontrol override, see RcOverride
		/// - encoding: Allocated/set/freed by user
		/// - decoding: unused
		/// </summary>
		public c_int Rc_Override_Count;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<RcOverride> Rc_Override;

		/// <summary>
		/// Maximum bitrate
		/// - encoding: Set by user
		/// - decoding: Set by user, may be overwritten by libavcodec
		/// </summary>
		public int64_t Rc_Max_Rate;

		/// <summary>
		/// Minimum bitrate
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public int64_t Rc_Min_Rate;

		/// <summary>
		/// Ratecontrol attempt to use, at maximum, ‹value› of what can be used without an underflow.
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_float Rc_Max_Available_Vbv_Use;

		/// <summary>
		/// Ratecontrol attempt to use, at least, ‹value› times the amount needed to prevent a vbv overflow.
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_float Rc_Min_Vbv_Overflow_Use;

		/// <summary>
		/// Number of bits which should be loaded into the rc buffer before decoding starts.
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_int Rc_Initial_Buffer_Occupancy;

		/// <summary>
		/// Trellis RD quantization
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public c_int Trellis;

		/// <summary>
		/// Pass1 encoding statistics output buffer
		/// - encoding: Set by libavcodec
		/// - decoding: unused
		/// </summary>
		public CPointer<char> Stats_Out;

		/// <summary>
		/// Pass2 encoding statistics input buffer
		/// Concatenated stuff from stats_out of pass1 should be placed here.
		/// - encoding: Allocated/set/freed by user
		/// - decoding: unused
		/// </summary>
		public CPointer<char> Stats_In;

		/// <summary>
		/// Work around bugs in encoders which sometimes cannot be detected automatically.
		/// - encoding: Set by user
		/// - decoding: Set by user
		/// </summary>
		public FFBug Workaround_Bugs;

		/// <summary>
		/// strictly follow the standard (MPEG-4, ...).
		/// - encoding: Set by user
		/// - decoding: Set by user
		/// Setting this to STRICT or higher means the encoder and decoder will
		/// generally do stupid things, whereas setting it to unofficial or lower
		/// will mean the encoder might produce output that is not supported by all
		/// spec-compliant decoders. Decoders don't differentiate between normal,
		/// unofficial and experimental (that is, they always try to decode things
		/// when they can) unless they are explicitly asked to behave stupidly
		/// (=strictly conform to the specs)
		/// This may only be set to one of the FF_COMPLIANCE_* values in defs.h.
		/// </summary>
		public FFCompliance Strict_Std_Compliance;

		/// <summary>
		/// Error concealment flags
		/// - encoding: unused
		/// - decoding: Set by user
		/// </summary>
		public FFEc Error_Concealment;

		/// <summary>
		/// Debug
		/// - encoding: Set by user
		/// - decoding: Set by user
		/// </summary>
		public FFDebug Debug;//XX Skal nok slettes

		/// <summary>
		/// Error recognition; may misdetect some more or less valid parts as errors.
		/// This is a bitfield of the AV_EF_* values defined in defs.h.
		///
		/// - encoding: Set by user
		/// - decoding: Set by user
		/// </summary>
		public AvEf Err_Recognition;

		/// <summary>
		/// Hardware accelerator in use
		/// - encoding: unused
		/// - decoding: Set by libavcodec
		/// </summary>
		public AvHwAccel HwAccel;

		/// <summary>
		/// Legacy hardware accelerator context.
		///
		/// For some hardware acceleration methods, the caller may use this field to
		/// signal hwaccel-specific data to the codec. The struct pointed to by this
		/// pointer is hwaccel-dependent and defined in the respective header. Please
		/// refer to the FFmpeg HW accelerator documentation to know how to fill
		/// this.
		///
		/// In most cases this field is optional - the necessary information may also
		/// be provided to libavcodec through hw_frames_ctx or
		/// hw_device_ctx (see avcodec_get_hw_config()). However, in some cases it
		/// may be the only method of signalling some (optional) information.
		///
		/// The struct and its contents are owned by the caller.
		///
		/// - encoding: May be set by the caller before avcodec_open2(). Must remain
		///             valid until avcodec_free_context().
		/// - decoding: May be set by the caller in the get_format() callback.
		///             Must remain valid until the next get_format() call,
		///             or avcodec_free_context() (whichever comes first)
		/// </summary>
		public IContext HwAccel_Context;

		/// <summary>
		/// A reference to the AVHWFramesContext describing the input (for encoding)
		/// or output (decoding) frames. The reference is set by the caller and
		/// afterwards owned (and freed) by libavcodec - it should never be read by
		/// the caller after being set.
		///
		/// - decoding: This field should be set by the caller from the get_format()
		///             callback. The previous reference (if any) will always be
		///             unreffed by libavcodec before the get_format() call.
		///
		///             If the default get_buffer2() is used with a hwaccel pixel
		///             format, then this AVHWFramesContext will be used for
		///             allocating the frame buffers.
		///
		/// - encoding: For hardware encoders configured to use a hwaccel pixel
		///             format, this field should be set by the caller to a reference
		///             to the AVHWFramesContext describing input frames.
		///             AVHWFramesContext.format must be equal to
		///             AVCodecContext.pix_fmt.
		///
		///             This field should be set before avcodec_open2() is called
		/// </summary>
		public AvBufferRef Hw_Frames_Ctx;

		/// <summary>
		/// A reference to the AVHWDeviceContext describing the device which will
		/// be used by a hardware encoder/decoder. The reference is set by the
		/// caller and afterwards owned (and freed) by libavcodec.
		///
		/// This should be used if either the codec device does not require
		/// hardware frames or any that are used are to be allocated internally by
		/// libavcodec. If the user wishes to supply any of the frames used as
		/// encoder input or decoder output then hw_frames_ctx should be used
		/// instead. When hw_frames_ctx is set in get_format() for a decoder, this
		/// field will be ignored while decoding the associated stream segment, but
		/// may again be used on a following one after another get_format() call.
		///
		/// For both encoders and decoders this field should be set before
		/// avcodec_open2() is called and must not be written to thereafter.
		///
		/// Note that some decoders may require this field to be set initially in
		/// order to support hw_frames_ctx at all - in that case, all frames
		/// contexts used must be created on the same device
		/// </summary>
		public AvBufferRef Hw_Device_Ctx;

		/// <summary>
		/// Bit set of AV_HWACCEL_FLAG_* flags, which affect hardware accelerated
		/// decoding (if active).
		/// - encoding: unused
		/// - decoding: Set by user (either before avcodec_open2(), or in the
		///             AVCodecContext.get_format callback)
		/// </summary>
		public AvHwAccelFlag HwAccel_Flags;

		/// <summary>
		/// Video decoding only. Sets the number of extra hardware frames which
		/// the decoder will allocate for use by the caller. This must be set
		/// before avcodec_open2() is called.
		///
		/// Some hardware decoders require all frames that they will use for
		/// output to be defined in advance before decoding starts. For such
		/// decoders, the hardware frame pool must therefore be of a fixed size.
		/// The extra frames set here are on top of any number that the decoder
		/// needs internally in order to operate normally (for example, frames
		/// used as reference pictures)
		/// </summary>
		public c_int Extra_Hw_Frames;

		/// <summary>
		/// Error
		/// - encoding: Set by libavcodec if flags and AV_CODEC_FLAG_PSNR
		/// - decoding: unused
		/// </summary>
		public readonly uint64_t[] Error = new uint64_t[AvFrame.Av_Num_Data_Pointers];

		/// <summary>
		/// DCT algorithm, see FF_DCT_* below
		/// - encoding: Set by user
		/// - decoding: unused
		/// </summary>
		public FFDct Dct_Algo;

		/// <summary>
		/// IDCT algorithm, see FF_IDCT_* below.
		/// - encoding: Set by user
		/// - decoding: Set by user
		/// </summary>
		public FFIdct Idct_Algo;

		/// <summary>
		/// Bits per sample/pixel from the demuxer (needed for huffyuv).
		/// - encoding: Set by libavcodec
		/// - decoding: Set by user
		/// </summary>
		public c_int Bits_Per_Coded_Sample;

		/// <summary>
		/// Bits per sample/pixel of internal libavcodec pixel/sample format.
		/// - encoding: set by user
		/// - decoding: set by libavcodec
		/// </summary>
		public c_int Bits_Per_Raw_Sample;

		/// <summary>
		/// Thread count
		/// Is used to decide how many independent tasks should be passed to execute()
		/// - encoding: Set by user
		/// - decoding: Set by user
		/// </summary>
		public c_int Thread_Count;

		/// <summary>
		/// Which multithreading methods to use.
		/// Use of FF_THREAD_FRAME will increase decoding delay by one frame per thread,
		/// so clients which cannot provide future frames should not use it.
		///
		/// - encoding: Set by user, otherwise the default is used
		/// - decoding: Set by user, otherwise the default is used
		/// </summary>
		public FFThread Thread_Type;

		/// <summary>
		/// Which multithreading methods are in use by the codec.
		/// - encoding: Set by libavcodec
		/// - decoding: Set by libavcodec
		/// </summary>
		public FFThread Active_Thread_Type;

		/// <summary>
		/// The codec may call this to execute several independent things.
		/// It will return only after finishing all tasks.
		/// The user may replace this with some multithreaded implementation,
		/// the default implementation will execute the parts serially
		/// </summary>
		public CodecFunc.Execute_Delegate Execute;

		/// <summary>
		/// The codec may call this to execute several independent things.
		/// It will return only after finishing all tasks.
		/// The user may replace this with some multithreaded implementation,
		/// the default implementation will execute the parts serially
		/// </summary>
		public CodecFunc.Execute2_Delegate Execute2;

		/// <summary>
		/// Profile
		/// - encoding: Set by user.
		/// - decoding: Set by libavcodec.
		/// See the AV_PROFILE_* defines in defs.h
		/// </summary>
		public AvProfileType Profile;

		/// <summary>
		/// Encoding level descriptor.
		/// - encoding: Set by user, corresponds to a specific level defined by the
		///   codec, usually corresponding to the profile level, if not specified it
		///   is set to AV_LEVEL_UNKNOWN.
		/// - decoding: Set by libavcodec.
		/// See AV_LEVEL_* in defs.h
		/// </summary>
		public AvLevel Level;

		/// <summary>
		/// Skip loop filtering for selected frames.
		/// - encoding: unused
		/// - decoding: Set by user
		/// </summary>
		public AvDiscard Skip_Loop_Filter;

		/// <summary>
		/// Skip IDCT/dequantization for selected frames.
		/// - encoding: unused
		/// - decoding: Set by user
		/// </summary>
		public AvDiscard Skip_Idct;

		/// <summary>
		/// Skip decoding for selected frames.
		/// - encoding: unused
		/// - decoding: Set by user
		/// </summary>
		public AvDiscard Skip_Frame;

		/// <summary>
		/// Skip processing alpha if supported by codec.
		/// Note that if the format uses pre-multiplied alpha (common with VP6,
		/// and recommended due to better video quality/compression)
		/// the image will look as if alpha-blended onto a black background.
		/// However for formats that do not use pre-multiplied alpha
		/// there might be serious artefacts (though e.g. libswscale currently
		/// assumes pre-multiplied alpha anyway).
		///
		/// - decoding: set by user
		/// - encoding: unused
		/// </summary>
		public c_int Skip_Alpha;

		/// <summary>
		/// Number of macroblock rows at the top which are skipped.
		/// - encoding: unused
		/// - decoding: Set by user
		/// </summary>
		public c_int Skip_Top;

		/// <summary>
		/// Number of macroblock rows at the bottom which are skipped.
		/// - encoding: unused
		/// - decoding: Set by user
		/// </summary>
		public c_int Skip_Bottom;

		/// <summary>
		/// Low resolution decoding, 1-> 1/2 size, 2->1/4 size
		/// - encoding: unused
		/// - decoding: Set by user
		/// </summary>
		public c_int LowRes;

		/// <summary>
		/// AVCodecDescriptor
		/// - encoding: unused
		/// - decoding: set by libavcodec
		/// </summary>
		public AvCodecDescriptor Codec_Descriptor;

		/// <summary>
		/// Character encoding of the input subtitles file.
		/// - decoding: set by user
		/// - encoding: unused
		/// </summary>
		public CPointer<char> Sub_CharEnc;

		/// <summary>
		/// Subtitles character encoding mode. Formats or codecs might be adjusting
		/// this setting (if they are doing the conversion themselves for instance).
		/// - decoding: set by libavcodec
		/// - encoding: unused
		/// </summary>
		public FFSubCharEncMode Sub_CharEnc_Mode;

		/// <summary>
		/// Header containing style information for text subtitles.
		/// For SUBTITLE_ASS subtitle type, it should contain the whole ASS
		/// [Script Info] and [V4+ Styles] section, plus the [Events] line and
		/// the Format line following. It shouldn't include any Dialogue line.
		///
		/// - encoding: May be set by the caller before avcodec_open2() to an array
		///   allocated with the av_malloc() family of functions.
		/// - decoding: May be set by libavcodec in avcodec_open2().
		///
		/// After being set, the array is owned by the codec and freed in
		/// avcodec_free_context()
		/// </summary>
		public c_int Subtitle_Header_Size;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<char> Subtitle_Header;

		/// <summary>
		/// dump format separator.
		/// can be ", " or "\n      " or anything else
		/// - encoding: Set by user
		/// - decoding: Set by user
		/// </summary>
		public CPointer<char> Dump_Separator;

		/// <summary>
		/// ',' separated list of allowed decoders.
		/// If NULL then all are allowed
		/// - encoding: unused
		/// - decoding: set by user
		/// </summary>
		public CPointer<char> Codec_Whitelist;

		/// <summary>
		/// Additional data associated with the entire coded stream.
		///
		/// - decoding: may be set by user before calling avcodec_open2().
		/// - encoding: may be set by libavcodec after avcodec_open2()
		/// </summary>
		public readonly ArrayInfo<AvPacketSideData> Coded_Side_Data = new ArrayInfo<AvPacketSideData>();

		/// <summary>
		/// Bit set of AV_CODEC_EXPORT_DATA_* flags, which affects the kind of
		/// metadata exported in frame, packet, or coded stream side data by
		/// decoders and encoders.
		///
		/// - decoding: set by user
		/// - encoding: set by user
		/// </summary>
		public AvCodecExportData Export_Side_Data;

		/// <summary>
		/// The number of pixels per image to maximally accept.
		///
		/// - decoding: set by user
		/// - encoding: set by user
		/// </summary>
		public int64_t Max_Pixels;

		/// <summary>
		/// Video decoding only. Certain video codecs support cropping, meaning that
		/// only a sub-rectangle of the decoded frame is intended for display. This
		/// option controls how cropping is handled by libavcodec.
		///
		/// When set to 1 (the default), libavcodec will apply cropping internally.
		/// I.e. it will modify the output frame width/height fields and offset the
		/// data pointers (only by as much as possible while preserving alignment, or
		/// by the full amount if the AV_CODEC_FLAG_UNALIGNED flag is set) so that
		/// the frames output by the decoder refer only to the cropped area. The
		/// crop_* fields of the output frames will be zero.
		///
		/// When set to 0, the width/height fields of the output frames will be set
		/// to the coded dimensions and the crop_* fields will describe the cropping
		/// rectangle. Applying the cropping is left to the caller.
		///
		/// Warning: When hardware acceleration with opaque output frames is used,
		/// libavcodec is unable to apply cropping from the top/left border.
		///
		/// Note when this option is set to zero, the width/height fields of the
		/// AVCodecContext and output AVFrames have different meanings. The codec
		/// context fields store display dimensions (with the coded dimensions in
		/// coded_width/height), while the frame fields store the coded dimensions
		/// (with the display dimensions being determined by the crop_* fields)
		/// </summary>
		public c_int Apply_Cropping;

		/// <summary>
		/// The percentage of damaged samples to discard a frame.
		///
		/// - decoding: set by user
		/// - encoding: unused
		/// </summary>
		public c_int Discard_Damaged_Percentage;

		/// <summary>
		/// The number of samples per frame to maximally accept.
		///
		/// - decoding: set by user
		/// - encoding: set by user
		/// </summary>
		public int64_t Max_Samples;

		/// <summary>
		/// This callback is called at the beginning of each packet to get a data
		/// buffer for it.
		///
		/// The following field will be set in the packet before this callback is
		/// called:
		/// - size
		/// This callback must use the above value to calculate the required buffer size,
		/// which must padded by at least AV_INPUT_BUFFER_PADDING_SIZE bytes.
		///
		/// In some specific cases, the encoder may not use the entire buffer allocated by this
		/// callback. This will be reflected in the size value in the packet once returned by
		/// avcodec_receive_packet().
		///
		/// This callback must fill the following fields in the packet:
		/// - data: alignment requirements for AVPacket apply, if any. Some architectures and
		///   encoders may benefit from having aligned data.
		/// - buf: must contain a pointer to an AVBufferRef structure. The packet's
		///   data pointer must be contained in it. See: av_buffer_create(), av_buffer_alloc(),
		///   and av_buffer_ref().
		///
		/// If AV_CODEC_CAP_DR1 is not set then get_encode_buffer() must call
		/// avcodec_default_get_encode_buffer() instead of providing a buffer allocated by
		/// some other means.
		///
		/// The flags field may contain a combination of AV_GET_ENCODE_BUFFER_FLAG_ flags.
		/// They may be used for example to hint what use the buffer may get after being
		/// created.
		/// Implementations of this callback may ignore flags they don't understand.
		/// If AV_GET_ENCODE_BUFFER_FLAG_REF is set in flags then the packet may be reused
		/// (read and/or written to if it is writable) later by libavcodec.
		///
		/// This callback must be thread-safe, as when frame threading is used, it may
		/// be called from multiple threads simultaneously.
		///
		/// See avcodec_default_get_encode_buffer()
		///
		/// - encoding: Set by libavcodec, user can override.
		/// - decoding: unused
		/// </summary>
		public CodecFunc.Get_Encode_Buffer_Delegate Get_Encode_Buffer;

		/// <summary>
		/// Frame counter, set by libavcodec.
		///
		/// - decoding: total number of frames returned from the decoder so far.
		/// - encoding: total number of frames passed to the encoder so far.
		///
		/// Note the counter is not incremented if encoding/decoding resulted in
		/// an error
		/// </summary>
		public int64_t Frame_Num;

		/// <summary>
		/// Decoding only. May be set by the caller before avcodec_open2() to an
		/// av_malloc()'ed array (or via AVOptions). Owned and freed by the decoder
		/// afterwards.
		///
		/// Side data attached to decoded frames may come from several sources:
		/// 1. coded_side_data, which the decoder will for certain types translate
		///    from packet-type to frame-type and attach to frames;
		/// 2. side data attached to an AVPacket sent for decoding (same
		///    considerations as above);
		/// 3. extracted from the coded bytestream.
		/// The first two cases are supplied by the caller and typically come from a
		/// container.
		///
		/// This array configures decoder behaviour in cases when side data of the
		/// same type is present both in the coded bytestream and in the
		/// user-supplied side data (items 1. and 2. above). In all cases, at most
		/// one instance of each side data type will be attached to output frames. By
		/// default it will be the bytestream side data. Adding an
		/// AVPacketSideDataType value to this array will flip the preference for
		/// this type, thus making the decoder prefer user-supplied side data over
		/// bytestream. In case side data of the same type is present both in
		/// coded_data and attacked to a packet, the packet instance always has
		/// priority.
		///
		/// The array may also contain a single -1, in which case the preference is
		/// switched for all side data types
		/// </summary>
		public readonly ArrayInfo<c_int> Side_Data_Prefer_Packet = new ArrayInfo<c_int>();

		/// <summary>
		/// Array containing static side data, such as HDR10 CLL / MDCV structures.
		/// Side data entries should be allocated by usage of helpers defined in
		/// libavutil/frame.h.
		///
		/// - encoding: may be set by user before calling avcodec_open2() for
		///             encoder configuration. Afterwards owned and freed by the
		///             encoder.
		/// - decoding: may be set by libavcodec in avcodec_open2()
		/// </summary>
		public CPointer<AvFrameSideData> Decoded_Side_Data;

		/// <summary>
		/// 
		/// </summary>
		public c_int Nb_Decoded_Side_Data;

		/// <summary>
		/// Indicates how the alpha channel of the video is represented.
		/// - encoding: Set by user
		/// - decoding: Set by libavcodec
		/// </summary>
		public AvAlphaMode Alpha_Mode;

		/********************************************************************/
		/// <summary>
		/// Clear all fields
		/// </summary>
		/********************************************************************/
		public override void Clear()
		{
			base.Clear();

			Log_Level_Offset = 0;
			Codec_Type = AvMediaType.Video;
			Codec = null;
			Codec_Id = AvCodecId.None;
			Codec_Tag = 0;
			Priv_Data = null;
			Internal = null;
			Opaque = null;
			Bit_Rate = 0;
			Flags = AvCodecFlag.None;
			Flags2 = AvCodecFlag2.None;
			ExtraData = null;
			Time_Base.Clear();
			Pkt_TimeBase.Clear();
			FrameRate.Clear();
			Delay = 0;
			PictureSize.Width = 0;
			PictureSize.Height = 0;
			Coded_Width = 0;
			Coded_Height = 0;
			Sample_Aspect_Ratio.Clear();
			Pix_Fmt = AvPixelFormat.YUV420P;
			Color_Primaries = AvColorPrimaries.Reserved0;
			Color_Trc = AvColorTransferCharacteristic.Reserved0;
			ColorSpace = AvColorSpace.Rgb;
			Color_Range = AvColorRange.Unspecified;
			Chroma_Sample_Location = AvChromaLocation.Unspecified;
			Field_Order = AvFieldOrder.Unknown;
			Refs = 0;
			Has_B_Frames = 0;
			Slice_Flags = SliceFlag.None;
			Draw_Horiz_Band = null;
			Get_Format = null;
			Max_B_Frames = 0;
			B_Quant_Factor = 0;
			B_Quant_Offset = 0;
			I_Quant_Factor = 0;
			I_Quant_Factor = 0;
			Lumi_Masking = 0;
			Temporal_Cplx_Masking = 0;
			Spatial_Cplx_Masking = 0;
			P_Masking = 0;
			Dark_Masking = 0;
			Nsse_Weight = 0;
			Me_Cmp = 0;
			Me_Sub_Cmp = 0;
			Mb_Cmp = 0;
			Ildct_Cmp = FFCmp.Sad;
			Dia_Size = 0;
			Last_Predictor_Count = 0;
			Me_Pre_Cmp = 0;
			Me_Dia_Size = 0;
			Me_SubPel_Quality = 0;
			Me_Range = 0;
			Mb_Decision = FFMbDecision.Simple;
			Intra_Matrix.SetToNull();
			Inter_Matrix.SetToNull();
			Chroma_Intra_Matrix.SetToNull();
			Intra_Dc_Precision = 0;
			Mb_LMin = 0;
			Mb_LMax = 0;
			BiDir_Refine = 0;
			KeyInt_Min = 0;
			Gop_Size = 0;
			Mv0_Threshold = 0;
			Slices = 0;
			Sample_Rate = 0;
			Sample_Fmt = AvSampleFormat.U8;
			Ch_Layout.Clear();
			Frame_Size = 0;
			Block_Align = 0;
			Cutoff = 0;
			Audio_Service_Type = AvAudioServiceType.Main;
			Request_Sample_Fmt = AvSampleFormat.U8;
			Initial_Padding = 0;
			Trailing_Padding = 0;
			Seek_Preroll = 0;
			Get_Buffer2 = null;
			Bit_Rate_Tolerance = 0;
			Global_Quality = 0;
			Compression_Level = FFCompression.None;
			QCompress = 0;
			QBlur = 0;
			QMin = 0;
			QMax = 0;
			Max_QDiff = 0;
			Rc_Buffer_Size = 0;
			Rc_Override_Count = 0;
			Rc_Override.SetToNull();
			Rc_Max_Rate = 0;
			Rc_Min_Rate = 0;
			Rc_Max_Available_Vbv_Use = 0;
			Rc_Min_Vbv_Overflow_Use = 0;
			Rc_Initial_Buffer_Occupancy = 0;
			Trellis = 0;
			Stats_Out.SetToNull();
			Stats_In.SetToNull();
			Workaround_Bugs = FFBug.None;
			Strict_Std_Compliance = FFCompliance.Normal;
			Error_Concealment = FFEc.None;
			Debug = FFDebug.None;
			Err_Recognition = 0;
			HwAccel = null;
			HwAccel_Context = null;
			Hw_Frames_Ctx = null;
			Hw_Device_Ctx = null;
			HwAccel_Flags = AvHwAccelFlag.None;
			Extra_Hw_Frames = 0;
			Array.Clear(Error);
			Dct_Algo = FFDct.Auto;
			Idct_Algo = FFIdct.Auto;
			Bits_Per_Coded_Sample = 0;
			Bits_Per_Raw_Sample = 0;
			Thread_Count = 0;
			Thread_Type = FFThread.None;
			Active_Thread_Type = FFThread.None;
			Execute = null;
			Execute2 = null;
			Profile = AvProfileType.Aac_Main;
			Level = AvLevel.None;
			Skip_Loop_Filter = AvDiscard.Default;
			Skip_Idct = AvDiscard.Default;
			Skip_Frame = AvDiscard.Default;
			Skip_Alpha = 0;
			Skip_Top = 0;
			Skip_Bottom = 0;
			LowRes = 0;
			Codec_Descriptor = null;
			Sub_CharEnc.SetToNull();
			Sub_CharEnc_Mode = FFSubCharEncMode.Automatic;
			Subtitle_Header_Size = 0;
			Dump_Separator.SetToNull();
			Codec_Whitelist.SetToNull();
			Coded_Side_Data.Array.SetToNull();
			Coded_Side_Data.Count = 0;
			Export_Side_Data = 0;
			Max_Pixels = 0;
			Apply_Cropping = 0;
			Discard_Damaged_Percentage = 0;
			Max_Samples = 0;
			Get_Encode_Buffer = null;
			Frame_Num = 0;
			Side_Data_Prefer_Packet.Array.SetToNull();
			Side_Data_Prefer_Packet.Count = 0;
			Decoded_Side_Data.SetToNull();
			Nb_Decoded_Side_Data = 0;
			Alpha_Mode = AvAlphaMode.Unspecified;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public AvCodecContext MakeDeepClone()
		{
			return (AvCodecContext)MemberwiseClone();
		}
	}
}
