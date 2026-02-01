/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// This structure describes decoded (raw) audio or video data.
	///
	/// AVFrame must be allocated using av_frame_alloc(). Note that this only
	/// allocates the AVFrame itself, the buffers for the data must be managed
	/// through other means (see below).
	/// AVFrame must be freed with av_frame_free().
	///
	/// AVFrame is typically allocated once and then reused multiple times to hold
	/// different data (e.g. a single AVFrame to hold frames received from a
	/// decoder). In such a case, av_frame_unref() will free any references held by
	/// the frame and reset it to its original clean state before it
	/// is reused again.
	///
	/// The data described by an AVFrame is usually reference counted through the
	/// AVBuffer API. The underlying buffer references are stored in AVFrame.buf /
	/// AVFrame.extended_buf. An AVFrame is considered to be reference counted if at
	/// least one reference is set, i.e. if AVFrame.buf[0] != NULL. In such a case,
	/// every single data plane must be contained in one of the buffers in
	/// AVFrame.buf or AVFrame.extended_buf.
	/// There may be a single buffer for all the data, or one separate buffer for
	/// each plane, or anything in between.
	///
	/// sizeof(AVFrame) is not a part of the public ABI, so new fields may be added
	/// to the end with a minor bump.
	///
	/// Fields can be accessed through AVOptions, the name string used, matches the
	/// C structure field name for fields accessible through AVOptions
	/// </summary>
	public class AvFrame : ICopyTo<AvFrame>
	{
		/// <summary>
		/// 
		/// </summary>
		public const c_int Av_Num_Data_Pointers = 8;

		/// <summary>
		/// Pointer to the picture/channel planes.
		/// This might be different from the first allocated byte. For video,
		/// it could even point to the end of the image data.
		///
		/// All pointers in data and extended_data must point into one of the
		/// AVBufferRef in buf or extended_buf.
		///
		/// Some decoders access areas outside 0,0 - width,height, please
		/// see avcodec_align_dimensions2(). Some filters and swscale can read
		/// up to 16 bytes beyond the planes, if these filters are to be used,
		/// then 16 extra bytes must be allocated.
		///
		/// NOTE: Pointers not needed by the format MUST be set to NULL.
		///
		/// Attention: In case of video, the data[] pointers can point to the
		/// end of image data in order to reverse line order, when used in
		/// combination with negative values in the linesize[] array
		/// </summary>
		public readonly CPointer<CPointer<uint8_t>> Data = new CPointer<CPointer<uint8_t>>(Av_Num_Data_Pointers);

		/// <summary>
		/// For video, a positive or negative value, which is typically indicating
		/// the size in bytes of each picture line, but it can also be:
		/// - the negative byte size of lines for vertical flipping
		///   (with data[n] pointing to the end of the data
		/// - a positive or negative multiple of the byte size as for accessing
		///   even and odd fields of a frame (possibly flipped)
		///
		/// For audio, only linesize[0] may be set. For planar audio, each channel
		/// plane must be the same size.
		///
		/// For video the linesizes should be multiples of the CPUs alignment
		/// preference, this is 16 or 32 for modern desktop CPUs.
		/// Some code requires such alignment other code can be slower without
		/// correct alignment, for yet other it makes no difference.
		///
		/// Note: The linesize may be larger than the size of usable data -- there
		/// may be extra padding present for performance reasons.
		///
		/// Attention: In case of video, line size values can be negative to achieve
		/// a vertically inverted iteration over image lines
		/// </summary>
		public CPointer<c_int> LineSize = new CPointer<c_int>(Av_Num_Data_Pointers);

		/// <summary>
		/// Pointers to the data planes/channels.
		///
		/// For video, this should simply point to data[].
		///
		/// For planar audio, each channel has a separate data pointer, and
		/// linesize[0] contains the size of each channel buffer.
		/// For packed audio, there is just one data pointer, and linesize[0]
		/// contains the total size of the buffer for all channels.
		///
		/// Note: Both data and extended_data should always be set in a valid frame,
		/// but for planar audio with more channels that can fit in data,
		/// extended_data must be used in order to access all channels
		/// </summary>
		public CPointer<CPointer<uint8_t>> Extended_Data;

		/// <summary>
		/// Video frames only. The coded dimensions (in pixels) of the video frame,
		/// i.e. the size of the rectangle that contains some well-defined values.
		///
		/// Note: The part of the frame intended for display/presentation is further
		/// restricted by the @ref cropping "Cropping rectangle"
		/// </summary>
		public c_int Width;

		/// <summary>
		/// 
		/// </summary>
		public c_int Height;

		/// <summary>
		/// Number of audio samples (per channel) described by this frame
		/// </summary>
		public c_int Nb_Samples;

		/// <summary>
		/// format of the frame, -1 if unknown or unset
		/// Values correspond to enum AVPixelFormat for video frames,
		/// enum AVSampleFormat for audio
		/// </summary>
		public FormatUnion Format;

		/// <summary>
		/// Picture type of the frame
		/// </summary>
		public AvPictureType Pict_Type;

		/// <summary>
		/// Sample aspect ratio for the video frame, 0/1 if unknown/unspecified
		/// </summary>
		public AvRational Sample_Aspect_Ratio;

		/// <summary>
		/// Presentation timestamp in time_base units (time when frame should be shown to user)
		/// </summary>
		public int64_t Pts;

		/// <summary>
		/// DTS copied from the AVPacket that triggered returning this frame. (if frame threading isn't used)
		/// This is also the Presentation time of this AVFrame calculated from
		/// only AVPacket.dts values without pts values
		/// </summary>
		public int64_t Pkt_Dts;

		/// <summary>
		/// Time base for the timestamps in this frame.
		/// In the future, this field may be set on frames output by decoders or
		/// filters, but its value will be by default ignored on input to encoders
		/// or filters
		/// </summary>
		public AvRational Time_Base;

		/// <summary>
		/// Quality (between 1 (good) and FF_LAMBDA_MAX (bad))
		/// </summary>
		public c_int Quality;

		/// <summary>
		/// Frame owner's private data.
		///
		/// This field may be set by the code that allocates/owns the frame data.
		/// It is then not touched by any library functions, except:
		/// - it is copied to other references by av_frame_copy_props() (and hence by
		///   av_frame_ref());
		/// - it is set to NULL when the frame is cleared by av_frame_unref()
		/// - on the caller's explicit request. E.g. libavcodec encoders/decoders
		///   will copy this field to/from @ref AVPacket "AVPackets" if the caller sets
		///   AV_CODEC_FLAG_COPY_OPAQUE.
		///
		/// See opaque_ref the reference-counted analogue
		/// </summary>
		public IOpaque Opaque;

		/// <summary>
		/// Number of fields in this frame which should be repeated, i.e. the total
		/// duration of this frame should be repeat_pict + 2 normal field durations.
		///
		/// For interlaced frames this field may be set to 1, which signals that this
		/// frame should be presented as 3 fields: beginning with the first field (as
		/// determined by AV_FRAME_FLAG_TOP_FIELD_FIRST being set or not), followed
		/// by the second field, and then the first field again.
		///
		/// For progressive frames this field may be set to a multiple of 2, which
		/// signals that this frame's duration should be (repeat_pict + 2) / 2
		/// normal frame durations.
		///
		/// Note: This field is computed from MPEG2 repeat_first_field flag and its
		/// associated flags, H.264 pic_struct from picture timing SEI, and
		/// their analogues in other codecs. Typically it should only be used when
		/// higher-layer timing information is not available
		/// </summary>
		public c_int Repeat_Pict;

		/// <summary>
		/// Sample rate of the audio data
		/// </summary>
		public c_int Sample_Rate;

		/// <summary>
		/// AVBuffer references backing the data for this frame. All the pointers in
		/// data and extended_data must point inside one of the buffers in buf or
		/// extended_buf. This array must be filled contiguously -- if buf[i] is
		/// non-NULL then buf[j] must also be non-NULL for all j ‹ i.
		///
		/// There may be at most one AVBuffer per data plane, so for video this array
		/// always contains all the references. For planar audio with more than
		/// AV_NUM_DATA_POINTERS channels, there may be more buffers than can fit in
		/// this array. Then the extra AVBufferRef pointers are stored in the
		/// extended_buf array
		/// </summary>
		public readonly AvBufferRef[] Buf = new AvBufferRef[Av_Num_Data_Pointers];

		/// <summary>
		/// For planar audio which requires more than AV_NUM_DATA_POINTERS
		/// AVBufferRef pointers, this array will hold all the references which
		/// cannot fit into AVFrame.buf.
		///
		/// Note that this is different from AVFrame.extended_data, which always
		/// contains all the pointers. This array only contains the extra pointers,
		/// which cannot fit into AVFrame.buf.
		///
		/// This array is always allocated using av_malloc() by whoever constructs
		/// the frame. It is freed in av_frame_unref()
		/// </summary>
		public CPointer<AvBufferRef> Extended_Buf;

		/// <summary>
		/// Number of elements in extended_buf
		/// </summary>
		public c_int Nb_Extended_Buf;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<AvFrameSideData> Side_Data;

		/// <summary>
		/// 
		/// </summary>
		public c_int Nb_Side_Data;

		/// <summary>
		/// Frame flags
		/// </summary>
		public AvFrameFlag Flags;

		/// <summary>
		/// MPEG vs JPEG YUV range.
		/// - encoding: Set by user
		/// - decoding: Set by libavcodec
		/// </summary>
		public AvColorRange Color_Range;

		/// <summary>
		/// 
		/// </summary>
		public AvColorPrimaries Color_Primaries;

		/// <summary>
		/// 
		/// </summary>
		public AvColorTransferCharacteristic Color_Trc;

		/// <summary>
		/// 
		/// </summary>
		public AvColorSpace ColorSpace;

		/// <summary>
		/// YUV colorspace type.
		/// - encoding: Set by user
		/// - decoding: Set by libavcodec
		/// </summary>
		public AvChromaLocation Chroma_Location;

		/// <summary>
		/// Frame timestamp estimated using various heuristics, in stream time base
		/// - encoding: unused
		/// - decoding: set by libavcodec, read by user
		/// </summary>
		public int64_t Best_Effort_Timestamp;

		/// <summary>
		/// Metadata.
		/// - encoding: Set by user
		/// - decoding: Set by libavcodec
		/// </summary>
		public AvDictionary Metadata;

		/// <summary>
		/// decode error flags of the frame, set to a combination of
		/// FF_DECODE_ERROR_xxx flags if the decoder produced a frame, but there
		/// were errors during the decoding.
		/// - encoding: unused
		/// - decoding: set by libavcodec, read by user
		/// </summary>
		public FFDecodeError Decode_Error_Flags;

		/// <summary>
		/// For hwaccel-format frames, this should be a reference to the
		/// AVHWFramesContext describing the frame
		/// </summary>
		public AvBufferRef Hw_Frames_Ctx;

		/// <summary>
		/// Frame owner's private data.
		///
		/// This field may be set by the code that allocates/owns the frame data.
		/// It is then not touched by any library functions, except:
		/// - a new reference to the underlying buffer is propagated by
		///   av_frame_copy_props() (and hence by av_frame_ref());
		/// - it is unreferenced in av_frame_unref();
		/// - on the caller's explicit request. E.g. libavcodec encoders/decoders
		///   will propagate a new reference to/from @ref AVPacket "AVPackets" if the
		///   caller sets @ref AV_CODEC_FLAG_COPY_OPAQUE.
		///
		/// See opaque the plain pointer analogue
		/// </summary>
		public AvBufferRef Opaque_Ref;

		/// <summary>
		/// Video frames only. The number of pixels to discard from the the
		/// top/bottom/left/right border of the frame to obtain the sub-rectangle of
		/// the frame intended for presentation
		/// </summary>
		public size_t Crop_Top;

		/// <summary>
		/// 
		/// </summary>
		public size_t Crop_Bottom;

		/// <summary>
		/// 
		/// </summary>
		public size_t Crop_Left;

		/// <summary>
		/// 
		/// </summary>
		public size_t Crop_Right;

		/// <summary>
		/// RefStruct reference for internal use by a single libav* library.
		/// Must not be used to transfer data between libraries.
		/// Has to be NULL when ownership of the frame leaves the respective library.
		///
		/// Code outside the FFmpeg libs must never check or change private_ref
		/// </summary>
		public IRefCount Private_Refs;

		/// <summary>
		/// Channel layout of the audio data
		/// </summary>
		public readonly AvChannelLayout Ch_Layout = new AvChannelLayout();

		/// <summary>
		/// Duration of the frame, in the same units as pts. 0 if unknown
		/// </summary>
		public int64_t Duration;

		/// <summary>
		/// Indicates how the alpha channel of the video is to be handled.
		/// - encoding: Set by user
		/// - decoding: Set by libavcodec
		/// </summary>
		public AvAlphaMode Alpha_Mode;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			CMemory.memset(Data, null, (size_t)Data.Length);
			LineSize.Clear();
			Extended_Data.SetToNull();
			Width = 0;
			Height = 0;
			Nb_Samples = 0;
			Format.Pixel = 0;
			Format.Sample = 0;
			Pict_Type = AvPictureType.None;
			Sample_Aspect_Ratio.Clear();
			Pts = 0;
			Pkt_Dts = 0;
			Time_Base.Clear();
			Quality = 0;
			Opaque = null;
			Repeat_Pict = 0;
			Sample_Rate = 0;
			Array.Clear(Buf);
			Extended_Buf.SetToNull();
			Nb_Extended_Buf = 0;
			Side_Data.SetToNull();
			Nb_Side_Data = 0;
			Flags = AvFrameFlag.None;
			Color_Range = AvColorRange.Unspecified;
			Color_Primaries = AvColorPrimaries.Reserved0;
			Color_Trc = AvColorTransferCharacteristic.Reserved0;
			ColorSpace = AvColorSpace.Rgb;
			Chroma_Location = AvChromaLocation.Unspecified;
			Best_Effort_Timestamp = 0;
			Metadata = null;
			Decode_Error_Flags = FFDecodeError.None;
			Hw_Frames_Ctx = null;
			Opaque_Ref = null;
			Crop_Top = 0;
			Crop_Bottom = 0;
			Crop_Left = 0;
			Crop_Right = 0;
			Private_Refs = null;
			Ch_Layout.Clear();
			Duration = 0;
			Alpha_Mode = AvAlphaMode.Unspecified;
		}



		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object into another object
		/// </summary>
		/********************************************************************/
		public void CopyTo(AvFrame destination)
		{
			destination.LineSize = LineSize;
			destination.Extended_Data = Extended_Data;
			destination.Width = Width;
			destination.Height = Height;
			destination.Nb_Samples = Nb_Samples;
			destination.Format = Format;
			destination.Pict_Type = Pict_Type;
			destination.Sample_Aspect_Ratio = Sample_Aspect_Ratio;
			destination.Pts = Pts;
			destination.Pkt_Dts = Pkt_Dts;
			destination.Time_Base = Time_Base;
			destination.Quality = Quality;
			destination.Opaque = Opaque;
			destination.Repeat_Pict = Repeat_Pict;
			destination.Sample_Rate = Sample_Rate;
			destination.Extended_Buf = Extended_Buf;
			destination.Nb_Extended_Buf = Nb_Extended_Buf;
			destination.Side_Data = Side_Data;
			destination.Nb_Side_Data = Nb_Side_Data;
			destination.Flags = Flags;
			destination.Color_Range = Color_Range;
			destination.Color_Primaries = Color_Primaries;
			destination.Color_Trc = Color_Trc;
			destination.ColorSpace = ColorSpace;
			destination.Chroma_Location = Chroma_Location;
			destination.Best_Effort_Timestamp = Best_Effort_Timestamp;
			destination.Metadata = Metadata;
			destination.Decode_Error_Flags = Decode_Error_Flags;
			destination.Hw_Frames_Ctx = Hw_Frames_Ctx;
			destination.Opaque_Ref = Opaque_Ref;
			destination.Crop_Top = Crop_Top;
			destination.Crop_Bottom = Crop_Bottom;
			destination.Crop_Left = Crop_Left;
			destination.Crop_Right = Crop_Right;
			destination.Private_Refs = Private_Refs;
			destination.Duration = Duration;
			destination.Alpha_Mode = Alpha_Mode;

			for (c_int i = destination.Data.Length - 1; i >= 0; i--)
				destination.Data[i] = Data[i];

			for (c_int i = destination.Buf.Length - 1; i >= 0; i--)
				destination.Buf[i] = Buf[i];

			Ch_Layout.CopyTo(destination.Ch_Layout);
		}
	}
}
