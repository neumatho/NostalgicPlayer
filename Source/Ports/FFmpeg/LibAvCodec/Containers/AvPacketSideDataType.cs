/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// Types and functions for working with AVPacketSideData
	/// </summary>
	public enum AvPacketSideDataType
	{
		/// <summary>
		/// An AV_PKT_DATA_PALETTE side data packet contains exactly AVPALETTE_SIZE
		/// bytes worth of palette. This side data signals that a new palette is
		/// present
		/// </summary>
		Palette,

		/// <summary>
		/// The AV_PKT_DATA_NEW_EXTRADATA is used to notify the codec or the format
		/// that the extradata buffer was changed and the receiving side should
		/// act upon it appropriately. The new extradata is embedded in the side
		/// data buffer and should be immediately used for processing the current
		/// frame or packet
		/// </summary>
		New_ExtraData,

		/// <summary>
		/// An AV_PKT_DATA_PARAM_CHANGE side data packet is laid out as follows:
		///
		/// u32le param_flags
		/// if (param_flags &amp; AV_SIDE_DATA_PARAM_CHANGE_SAMPLE_RATE)
		///     s32le sample_rate
		/// if (param_flags &amp; AV_SIDE_DATA_PARAM_CHANGE_DIMENSIONS)
		///     s32le width
		///     s32le height
		/// </summary>
		Param_Change,

        /// <summary>
        /// An AV_PKT_DATA_H263_MB_INFO side data packet contains a number of
        /// structures with info about macroblocks relevant to splitting the
        /// packet into smaller packets on macroblock edges (e.g. as for RFC 2190).
        /// That is, it does not necessarily contain info about all macroblocks,
        /// as long as the distance between macroblocks in the info is smaller
        /// than the target payload size.
        /// Each MB info structure is 12 bytes, and is laid out as follows:
        ///
        /// u32le bit offset from the start of the packet
        /// u8    current quantizer at the start of the macroblock
        /// u8    GOB number
        /// u16le macroblock address within the GOB
        /// u8    horizontal MV predictor
        /// u8    vertical MV predictor
        ///
        /// u8    horizontal MV predictor for block number 3
        /// u8    vertical MV predictor for block number 3
		/// </summary>
		H263_Mb_Info,

		/// <summary>
		/// This side data should be associated with an audio stream and contains
		/// ReplayGain information in form of the AVReplayGain struct.
		/// </summary>
		ReplayGain,

		/// <summary>
		/// This side data contains a 3x3 transformation matrix describing an affine
		/// transformation that needs to be applied to the decoded video frames for
		/// correct presentation.
		/// See libavutil/display.h for a detailed description of the data.
		/// </summary>
		DisplayMatrix,

		/// <summary>
		/// This side data should be associated with a video stream and contains
		/// Stereoscopic 3D information in form of the AVStereo3D struct.
		/// </summary>
		Stereo3D,

		/// <summary>
		/// This side data should be associated with an audio stream and corresponds
		/// to enum AVAudioServiceType.
		/// </summary>
		Audio_Service_Type,

		/// <summary>
		/// This side data contains quality related information from the encoder.
		/// u32le quality factor (1 = good, FF_LAMBDA_MAX = bad)
		/// u8    picture type
		/// u8    error count
		/// u16   reserved
		/// u64le[error count] sum of squared differences
		/// </summary>
		Quality_Stats,

		/// <summary>
		/// Contains an integer stream index of a fallback track to use when the
		/// current track cannot be decoded.
		/// </summary>
		Fallback_Track,

		/// <summary>
		/// Corresponds to the AVCPBProperties struct.
		/// </summary>
		Cpb_Properties,

		/// <summary>
		/// Recommends skipping the specified number of samples.
		/// u32le skip start
		/// u32le skip end
		/// u8    reason start
		/// u8    reason end
		/// </summary>
		Skip_Samples,

		/// <summary>
		/// Indicates possible Japanese dual mono audio and selected channel.
		/// u8 selected channels (0=main/left,1=sub/right,2=both)
		/// </summary>
		Jp_DualMono,

		/// <summary>
		/// List of zero terminated key/value strings (size delimits end).
		/// </summary>
		Strings_Metadata,

		/// <summary>
		/// Subtitle event position (x1,y1,x2,y2 - all u32le).
		/// </summary>
		Subtitle_Position,

		/// <summary>
		/// Matroska BlockAdditional: 8 byte id + data (size delimits end).
		/// </summary>
		Matroska_BlockAdditional,

		/// <summary>
		/// First identifier line of a WebVTT cue.
		/// </summary>
		WebVtt_Identifier,

		/// <summary>
		/// Settings (rendering instructions) following timestamp in a WebVTT cue.
		/// </summary>
		WebVtt_Settings,

		/// <summary>
		/// Updated stream metadata (key/value strings, size delimits end).
		/// </summary>
		Metadata_Update,

		/// <summary>
		/// MPEGTS stream id (uint8_t) passed from demuxer to muxer.
		/// </summary>
		MpegTs_Stream_Id,

		/// <summary>
		/// Mastering display metadata (SMPTE-2086) - AVMasteringDisplayMetadata.
		/// </summary>
		Mastering_Display_Metadata,

		/// <summary>
		/// Spherical video mapping - AVSphericalMapping.
		/// </summary>
		Spherical,

		/// <summary>
		/// Content light level (CTA-861.3) - AVContentLightMetadata.
		/// </summary>
		Content_Light_Level,

		/// <summary>
		/// ATSC A53 Part 4 Closed Captions bitstream.
		/// </summary>
		A53_CC,

		/// <summary>
		/// Encryption initialization data (opaque - use helper API).
		/// </summary>
		Encryption_Init_Info,

		/// <summary>
		/// Encryption info for decrypting the packet (opaque - use helper API).
		/// </summary>
		Encryption_Info,

		/// <summary>
		/// Active Format Description byte (ETSI TS 101 154) - AVActiveFormatDescription.
		/// </summary>
		Afd,

		/// <summary>
		/// Producer Reference Time - AVProducerReferenceTime (export_side_data prft flag).
		/// </summary>
		Prft,

		/// <summary>
		/// ICC profile data (ISO 15076-1).
		/// </summary>
		Icc_Profile,

		/// <summary>
		/// Dolby Vision configuration - AVDOVIDecoderConfigurationRecord.
		/// </summary>
		Dovi_Conf,

		/// <summary>
		/// SMPTE ST 12-1 timecode array (first u32 = count of following timecodes).
		/// </summary>
		S12M_TimeCode,

		/// <summary>
		/// HDR10+ dynamic metadata - AVDynamicHDRPlus.
		/// </summary>
		Dynamic_Hdr10_Plus,

		/// <summary>
		/// IAMF Mix Gain Parameter - AVIAMFParamDefinition (sections 3.6.1/3.8.1).
		/// </summary>
		Iamf_Mix_Gain_Param,

		/// <summary>
		/// IAMF Demixing Info Parameter - AVIAMFParamDefinition (3.6.1/3.8.2).
		/// </summary>
		Iamf_Demixing_Info_Param,

		/// <summary>
		/// IAMF Recon Gain Info Parameter - AVIAMFParamDefinition (3.6.1/3.8.3).
		/// </summary>
		Iamf_Recon_Gain_Info_Param,

		/// <summary>
		/// Ambient viewing environment metadata - AVAmbientViewingEnvironment (H.274).
		/// </summary>
		Ambient_Viewing_Environment,

		/// <summary>
		/// Frame cropping rectangle (crop_top/bottom/left/right - all u32le).
		/// </summary>
		Frame_Cropping,

		/// <summary>
		/// Raw LCEVC payload data (uint8_t array with NAL emulation bytes intact).
		/// </summary>
		Lcevc,

		/// <summary>
		/// 3D reference display info - AV3DReferenceDisplaysInfo.
		/// </summary>
		_3D_Reference_Displays,

		/// <summary>
		/// Last received RTCP SR information - AVRTCPSenderReport.
		/// </summary>
		Rtcp_Sr,

		/// <summary>
		/// EXIF metadata buffer (starts with TIFF header 49 49 2A 00 or 4D 4D 00 2A).
		/// </summary>
		Exif,

		/// <summary>
		/// Number of side data types (must remain last; may change when new types are added).
		/// </summary>
		Nb
	}
}
