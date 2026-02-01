/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// This struct describes the properties of an encoded stream.
	///
	/// sizeof(AVCodecParameters) is not a part of the public ABI, this struct must
	/// be allocated with avcodec_parameters_alloc() and freed with
	/// avcodec_parameters_free()
	/// </summary>
	public class AvCodecParameters : IClearable, ICopyTo<AvCodecParameters>
	{
		/// <summary>
		/// General type of the encoded data
		/// </summary>
		public AvMediaType Codec_Type;

		/// <summary>
		/// Specific type of the encoded data (the codec used)
		/// </summary>
		public AvCodecId Codec_Id;

		/// <summary>
		/// Additional information about the codec (corresponds to the AVI FOURCC)
		/// </summary>
		public uint32_t Codec_Tag;

		/// <summary>
		/// Extra binary data needed for initializing the decoder, codec-dependent.
		///
		/// Must be allocated with av_malloc() and will be freed by
		/// avcodec_parameters_free(). The allocated size of extradata must be at
		/// least extradata_size + AV_INPUT_BUFFER_PADDING_SIZE, with the padding
		/// bytes zeroed
		/// </summary>
		public IDataContext ExtraData;

		/// <summary>
		/// Additional data associated with the entire stream.
		///
		/// Should be allocated with av_packet_side_data_new() or
		/// av_packet_side_data_add(), and will be freed by avcodec_parameters_free()
		/// </summary>
		public CPointer<AvPacketSideData> Coded_Side_Data;

		/// <summary>
		/// Amount of entries in coded_side_data
		/// </summary>
		public c_int Nb_Coded_Side_Data;

		/// <summary>
		/// - video: the pixel format, the value corresponds to enum AVPixelFormat
		/// - audio: the sample format, the value corresponds to enum AVSampleFormat
		/// </summary>
		public FormatUnion Format;

		/// <summary>
		/// The average bitrate of the encoded data (in bits per second)
		/// </summary>
		public int64_t Bit_Rate;

		/// <summary>
		/// The number of bits per sample in the codedwords.
		///
		/// This is basically the bitrate per sample. It is mandatory for a bunch of
		/// formats to actually decode them. It's the number of bits for one sample in
		/// the actual coded bitstream.
		///
		/// This could be for example 4 for ADPCM
		/// For PCM formats this matches bits_per_raw_sample
		/// Can be 0
		/// </summary>
		public c_int Bits_Per_Coded_Sample;

		/// <summary>
		/// This is the number of valid bits in each output sample. If the
		/// sample format has more bits, the least significant bits are additional
		/// padding bits, which are always 0. Use right shifts to reduce the sample
		/// to its actual size. For example, audio formats with 24 bit samples will
		/// have bits_per_raw_sample set to 24, and format set to AV_SAMPLE_FMT_S32.
		/// To get the original sample use "(int32_t)sample >> 8"."
		///
		/// For ADPCM this might be 12 or 16 or similar
		/// Can be 0
		/// </summary>
		public c_int Bits_Per_Raw_Sample;

		/// <summary>
		/// Codec-specific bitstream restrictions that the stream conforms to
		/// </summary>
		public AvProfileType Profile;

		/// <summary>
		/// 
		/// </summary>
		public AvLevel Level;

		/// <summary>
		/// Video only. The dimensions of the video frame in pixels
		/// </summary>
		public c_int Width;

		/// <summary>
		/// 
		/// </summary>
		public c_int Height;

		/// <summary>
		/// Video only. The aspect ratio (width / height) which a single pixel
		/// should have when displayed.
		///
		/// When the aspect ratio is unknown / undefined, the numerator should be
		/// set to 0 (the denominator may have any value)
		/// </summary>
		public AvRational Sample_Aspect_Ratio;

		/// <summary>
		/// Video only. Number of frames per second, for streams with constant frame
		/// durations. Should be set to { 0, 1 } when some frames have differing
		/// durations or if the value is not known.
		///
		/// Note: This field corresponds to values that are stored in codec-level
		/// headers and is typically overridden by container/transport-layer
		/// timestamps, when available. It should thus be used only as a last resort,
		/// when no higher-level timing information is available
		/// </summary>
		public AvRational FrameRate;

		/// <summary>
		/// Video only. The order of the fields in interlaced video
		/// </summary>
		public AvFieldOrder Field_Order;

		/// <summary>
		/// Video only. Additional colorspace characteristics
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
		public AvColorSpace Color_Space;

		/// <summary>
		/// 
		/// </summary>
		public AvChromaLocation Chroma_Location;

		/// <summary>
		/// Video only. Number of delayed frames
		/// </summary>
		public c_int Video_Delay;

		/// <summary>
		/// Audio only. The channel layout and number of channels
		/// </summary>
		public readonly AvChannelLayout Ch_Layout = new AvChannelLayout();

		/// <summary>
		/// Audio only. The number of audio samples per second
		/// </summary>
		public c_int Sample_Rate;

		/// <summary>
		/// Audio only. The number of bytes per coded audio frame, required by some
		/// formats.
		///
		/// Corresponds to nBlockAlign in WAVEFORMATEX
		/// </summary>
		public c_int Block_Align;

		/// <summary>
		/// Audio only. Audio frame size, if known. Required by some formats to be static
		/// </summary>
		public c_int Frame_Size;

		/// <summary>
		/// Audio only. The amount of padding (in samples) inserted by the encoder at
		/// the beginning of the audio. I.e. this number of leading decoded samples
		/// must be discarded by the caller to get the original audio without leading
		/// padding
		/// </summary>
		public c_int Initial_Padding;

		/// <summary>
		/// Audio only. The amount of padding (in samples) appended by the encoder to
		/// the end of the audio. I.e. this number of decoded samples must be
		/// discarded by the caller from the end of the stream to get the original
		/// audio without any trailing padding
		/// </summary>
		public c_int Trailing_Padding;

		/// <summary>
		/// Audio only. Number of samples to skip after a discontinuity
		/// </summary>
		public c_int Seek_Preroll;

		/// <summary>
		/// Video with alpha channel only. Alpha channel handling
		/// </summary>
		public AvAlphaMode Alpha_Mode;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			Codec_Type = AvMediaType.Video;
			Codec_Id = AvCodecId.None;
			Codec_Tag = 0;
			ExtraData = null;
			Coded_Side_Data.SetToNull();
			Nb_Coded_Side_Data = 0;
			Format.Pixel = AvPixelFormat.YUV420P;
			Format.Sample = AvSampleFormat.U8;
			Bit_Rate = 0;
			Bits_Per_Coded_Sample = 0;
			Bits_Per_Raw_Sample = 0;
			Profile = AvProfileType.Aac_Main;
			Level = AvLevel.None;
			Width = 0;
			Height = 0;
			Sample_Aspect_Ratio.Clear();
			FrameRate.Clear();
			Field_Order = AvFieldOrder.Unknown;
			Color_Range = AvColorRange.Unspecified;
			Color_Primaries = AvColorPrimaries.Reserved0;
			Color_Trc = AvColorTransferCharacteristic.Reserved0;
			Color_Space = AvColorSpace.Rgb;
			Chroma_Location = AvChromaLocation.Unspecified;
			Video_Delay = 0;
			Ch_Layout.Clear();
			Sample_Rate = 0;
			Block_Align = 0;
			Frame_Size = 0;
			Initial_Padding = 0;
			Trailing_Padding = 0;
			Seek_Preroll = 0;
			Alpha_Mode = AvAlphaMode.Unspecified;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void CopyTo(AvCodecParameters destination)
		{
			destination.Codec_Type = Codec_Type;
			destination.Codec_Id = Codec_Id;
			destination.Codec_Tag = Codec_Tag;
			destination.ExtraData = ExtraData;
			destination.Coded_Side_Data = Coded_Side_Data;
			destination.Nb_Coded_Side_Data = Nb_Coded_Side_Data;
			destination.Format = Format;
			destination.Bit_Rate = Bit_Rate;
			destination.Bits_Per_Coded_Sample = Bits_Per_Coded_Sample;
			destination.Bits_Per_Raw_Sample = Bits_Per_Raw_Sample;
			destination.Profile = Profile;
			destination.Level = Level;
			destination.Width = Width;
			destination.Height = Height;
			destination.Sample_Aspect_Ratio = Sample_Aspect_Ratio;
			destination.FrameRate = FrameRate;
			destination.Field_Order = Field_Order;
			destination.Color_Range = Color_Range;
			destination.Color_Primaries = Color_Primaries;
			destination.Color_Trc = Color_Trc;
			destination.Color_Space = Color_Space;
			destination.Chroma_Location = Chroma_Location;
			destination.Video_Delay = Video_Delay;
			destination.Sample_Rate = Sample_Rate;
			destination.Block_Align = Block_Align;
			destination.Frame_Size = Frame_Size;
			destination.Initial_Padding = Initial_Padding;
			destination.Trailing_Padding = Trailing_Padding;
			destination.Seek_Preroll = Seek_Preroll;
			destination.Alpha_Mode = Alpha_Mode;

			Ch_Layout.CopyTo(destination.Ch_Layout);
		}
	}
}
