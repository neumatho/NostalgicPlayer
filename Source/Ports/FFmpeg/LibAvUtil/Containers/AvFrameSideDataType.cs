/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public enum AvFrameSideDataType
	{
		/// <summary>
		/// The data is the AVPanScan struct defined in libavcodec
		/// </summary>
		PanScan,

		/// <summary>
		/// ATSC A53 Part 4 Closed Captions.
		/// A53 CC bitstream is stored as uint8_t in AVFrameSideData.data.
		/// The number of bytes of CC data is AVFrameSideData.size
		/// </summary>
		A53_CC,

		/// <summary>
		/// Stereoscopic 3d metadata.
		/// The data is the AVStereo3D struct defined in libavutil/stereo3d.h
		/// </summary>
		Stereo3D,

		/// <summary>
		/// The data is the AVMatrixEncoding enum defined in libavutil/channel_layout.h
		/// </summary>
		MatrixEncoding,

		/// <summary>
		/// Metadata relevant to a downmix procedure.
		/// The data is the AVDownmixInfo struct defined in libavutil/downmix_info.h
		/// </summary>
		DownMix_Info,

		/// <summary>
		/// ReplayGain information in the form of the AVReplayGain struct
		/// </summary>
		ReplayGain,

		/// <summary>
		/// This side data contains a 3x3 transformation matrix describing an affine
		/// transformation that needs to be applied to the frame for correct
		/// presentation.
		///
		/// See libavutil/display.h for a detailed description of the data
		/// </summary>
		DisplayMatrix,

		/// <summary>
		/// Active Format Description data consisting of a single byte as specified
		/// in ETSI TS 101 154 using AVActiveFormatDescription enum
		/// </summary>
		Afd,

		/// <summary>
		/// Motion vectors exported by some codecs (on demand through the export_mvs
		/// flag set in the libavcodec AVCodecContext flags2 option).
		/// The data is the AVMotionVector struct defined in
		/// libavutil/motion_vector.h
		/// </summary>
		Motion_Vectors,

		/// <summary>
		/// Recommends skipping the specified number of samples. This is exported
		/// only if the "skip_manual" AVOption is set in libavcodec.
		/// This has the same format as AV_PKT_DATA_SKIP_SAMPLES.
		///
		/// u32le number of samples to skip from start of this packet
		/// u32le number of samples to skip from end of this packet
		/// u8    reason for start skip
		/// u8    reason for end   skip (0=padding silence, 1=convergence)
		/// </summary>
		Skip_Samples,

		/// <summary>
		/// This side data must be associated with an audio frame and corresponds to
		/// enum AVAudioServiceType defined in avcodec.h
		/// </summary>
		Audio_Service_Type,

		/// <summary>
		/// Mastering display metadata associated with a video frame. The payload is
		/// an AVMasteringDisplayMetadata type and contains information about the
		/// mastering display color volume
		/// </summary>
		Mastering_Display_Metadata,

		/// <summary>
		/// The GOP timecode in 25 bit timecode format. Data format is 64-bit integer.
		/// This is set on the first frame of a GOP that has a temporal reference of 0
		/// </summary>
		Gop_TimeCode,

		/// <summary>
		/// The data represents the AVSphericalMapping structure defined in
		/// libavutil/spherical.h
		/// </summary>
		Spherical,

		/// <summary>
		/// Content light level (based on CTA-861.3). This payload contains data in
		/// the form of the AVContentLightMetadata struct
		/// </summary>
		Content_Light_Level,

		/// <summary>
		/// The data contains an ICC profile as an opaque octet buffer following the
		/// format described by ISO 15076-1 with an optional name defined in the
		/// metadata key entry "name"
		/// </summary>
		Icc_Profile,

		/// <summary>
		/// Timecode which conforms to SMPTE ST 12-1. The data is an array of 4 uint32_t
		/// where the first uint32_t describes how many (1-3) of the other timecodes are used.
		/// The timecode format is described in the documentation of av_timecode_get_smpte_from_framenum()
		/// function in libavutil/timecode.h
		/// </summary>
		S12M_TimeCode,

		/// <summary>
		/// HDR dynamic metadata associated with a video frame. The payload is
		/// an AVDynamicHDRPlus type and contains information for color
		/// volume transform - application 4 of SMPTE 2094-40:2016 standard
		/// </summary>
		Dynamic_Hdr_Plus,

		/// <summary>
		/// Regions Of Interest, the data is an array of AVRegionOfInterest type, the number of
		/// array element is implied by AVFrameSideData.size / AVRegionOfInterest.self_size
		/// </summary>
		Regions_Of_Interest,

		/// <summary>
		/// Encoding parameters for a video frame, as described by AVVideoEncParams
		/// </summary>
		Video_Enc_Params,

		/// <summary>
		/// User data unregistered metadata associated with a video frame.
		/// This is the H.26[45] UDU SEI message, and shouldn't be used for any other purpose
		/// The data is stored as uint8_t in AVFrameSideData.data which is 16 bytes of
		/// uuid_iso_iec_11578 followed by AVFrameSideData.size - 16 bytes of user_data_payload_byte
		/// </summary>
		Sei_Unregistered,

		/// <summary>
		/// Film grain parameters for a frame, described by AVFilmGrainParams.
		/// Must be present for every frame which should have film grain applied.
		///
		/// May be present multiple times, for example when there are multiple
		/// alternative parameter sets for different video signal characteristics.
		/// The user should select the most appropriate set for the application
		/// </summary>
		Film_Grain_Params,

		/// <summary>
		/// Bounding boxes for object detection and classification,
		/// as described by AVDetectionBBoxHeader
		/// </summary>
		Detection_BBoxes,

		/// <summary>
		/// Dolby Vision RPU raw data, suitable for passing to x265
		/// or other libraries. Array of uint8_t, with NAL emulation
		/// bytes intact
		/// </summary>
		Dovi_Rpu_Buffer,

		/// <summary>
		/// Parsed Dolby Vision metadata, suitable for passing to a software
		/// implementation. The payload is the AVDOVIMetadata struct defined in
		/// libavutil/dovi_meta.h
		/// </summary>
		Dovi_Metadata,

		/// <summary>
		/// HDR Vivid dynamic metadata associated with a video frame. The payload is
		/// an AVDynamicHDRVivid type and contains information for color
		/// volume transform - CUVA 005.1-2021
		/// </summary>
		Dynamic_Hdr_Vivid,

		/// <summary>
		/// Ambient viewing environment metadata, as defined by H.274
		/// </summary>
		Ambient_Viewing_Environment,

		/// <summary>
		/// Provide encoder-specific hinting information about changed/unchanged
		/// portions of a frame. It can be used to pass information about which
		/// macroblocks can be skipped because they didn't change from the
		/// corresponding ones in the previous frame. This could be useful for
		/// applications which know this information in advance to speed up
		/// encoding
		/// </summary>
		Video_Hint,

		/// <summary>
		/// Raw LCEVC payload data, as a uint8_t array, with NAL emulation
		/// bytes intact
		/// </summary>
		Lcevc,

		/// <summary>
		/// This side data must be associated with a video frame.
		/// The presence of this side data indicates that the video stream is
		/// composed of multiple views (e.g. stereoscopic 3D content,
		/// cf. H.264 Annex H or H.265 Annex G).
		/// The data is an int storing the view ID
		/// </summary>
		View_Id,

		/// <summary>
		/// This side data contains information about the reference display width(s)
		/// and reference viewing distance(s) as well as information about the
		/// corresponding reference stereo pair(s), i.e., the pair(s) of views to be
		/// displayed for the viewer's left and right eyes on the reference display
		/// at the reference viewing distance.
		/// The payload is the AV3DReferenceDisplaysInfo struct defined in
		/// libavutil/tdrdi.h
		/// </summary>
		_3D_Reference_Displays,

		/// <summary>
		/// Extensible image file format metadata. The payload is a buffer containing
		/// EXIF metadata, starting with either 49 49 2a 00, or 4d 4d 00 2a
		/// </summary>
		Exif
	}
}
