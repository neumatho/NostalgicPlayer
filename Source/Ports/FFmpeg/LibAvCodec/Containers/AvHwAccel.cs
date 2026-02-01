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
	public class AvHwAccel
	{
		/// <summary>
		/// Name of the hardware accelerated codec.
		/// The name is globally unique among encoders and among decoders (but an
		/// encoder and a decoder can share the same name)
		/// </summary>
		public CPointer<char> Name;

		/// <summary>
		/// Type of codec implemented by the hardware accelerator.
		///
		/// See AVMEDIA_TYPE_xxx
		/// </summary>
		public AvMediaType Type;

		/// <summary>
		/// Codec implemented by the hardware accelerator.
		///
		/// See AV_CODEC_ID_xxx
		/// </summary>
		public AvCodecId Id;

		/// <summary>
		/// Supported pixel format.
		///
		/// Only hardware accelerated formats are supported here
		/// </summary>
		public AvPixelFormat Pix_Fmt;

		/// <summary>
		/// Hardware accelerated codec capabilities.
		/// see AV_HWACCEL_CODEC_CAP_*
		/// </summary>
		public AvHwAccelCodecCap Capabilities;
	}
}
