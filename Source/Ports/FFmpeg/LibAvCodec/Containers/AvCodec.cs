/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class AvCodec
	{
		/// <summary>
		/// Name of the codec implementation.
		///
		/// The name is globally unique among encoders and among decoders (but an
		/// encoder and a decoder can share the same name).
		/// This is the primary way to find a codec from the user perspective
		/// </summary>
		public CPointer<char> Name;

		/// <summary>
		/// Descriptive name for the codec, meant to be more human readable than name
		/// </summary>
		public CPointer<char> Long_Name;

		/// <summary>
		/// 
		/// </summary>
		public AvMediaType Type;

		/// <summary>
		/// 
		/// </summary>
		public AvCodecId Id;

		/// <summary>
		/// Codec capabilities
		/// </summary>
		public AvCodecCap Capabilities;

		/// <summary>
		/// Maximum value for lowres supported by the decoder
		/// </summary>
		public uint8_t Max_LowRes;

		/// <summary>
		/// Deprecated use avcodec_get_supported_config()
		/// </summary>
		public CPointer<AvRational> Supported_FrameRates;

		/// <summary>
		/// Deprecated use avcodec_get_supported_config()
		/// </summary>
		public CPointer<AvPixelFormat> Pix_Fmts;

		/// <summary>
		/// Deprecated use avcodec_get_supported_config()
		/// </summary>
		public CPointer<c_int> Supported_SampleRates;

		/// <summary>
		/// Deprecated use avcodec_get_supported_config()
		/// </summary>
		public CPointer<AvSampleFormat> Sample_Fmts;

		/// <summary>
		/// AVClass for the private context
		/// </summary>
		public AvClass Priv_Class;

		/// <summary>
		/// Array of recognized profiles, or NULL if unknown, array is terminated by {AV_PROFILE_UNKNOWN}
		/// </summary>
		public CPointer<AvProfile> Profiles;

		/// <summary>
		/// Group name of the codec implementation.
		/// This is a short symbolic name of the wrapper backing this codec. A
		/// wrapper uses some kind of external implementation for the codec, such
		/// as an external library, or a codec implementation provided by the OS or
		/// the hardware.
		/// If this field is NULL, this is a builtin, libavcodec native codec.
		/// If non-NULL, this will be the suffix in AVCodec.name in most cases
		/// (usually AVCodec.name will be of the form "‹codec_name›_‹wrapper_name›")
		/// </summary>
		public CPointer<char> Wrapper_Name;

		/// <summary>
		/// Array of supported channel layouts, terminated with a zeroed layout.
		/// Deprecated use avcodec_get_supported_config()
		/// </summary>
		public CPointer<AvChannelLayout> Ch_Layouts;
	}
}
