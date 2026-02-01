/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class AvCodecHwConfig
	{
		/// <summary>
		/// For decoders, a hardware pixel format which that decoder may be
		/// able to decode to if suitable hardware is available.
		///
		/// For encoders, a pixel format which the encoder may be able to
		/// accept. If set to AV_PIX_FMT_NONE, this applies to all pixel
		/// formats supported by the codec
		/// </summary>
		public AvPixelFormat Pix_Fmt;

		/// <summary>
		/// Bit set of AV_CODEC_HW_CONFIG_METHOD_* flags, describing the possible
		/// setup methods which can be used with this configuration
		/// </summary>
		public AvCodecHwConfigMethod Methods;

		/// <summary>
		/// The device type associated with the configuration.
		///
		/// Must be set for AV_CODEC_HW_CONFIG_METHOD_HW_DEVICE_CTX and
		/// AV_CODEC_HW_CONFIG_METHOD_HW_FRAMES_CTX, otherwise unused
		/// </summary>
		public AvHwDeviceType Device_Type;
	}
}
