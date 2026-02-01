/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public enum AvCodecConfig
	{
		/// <summary>
		/// AVPixelFormat, terminated by AV_PIX_FMT_NONE
		/// </summary>
		Pix_Format,

		/// <summary>
		/// AVRational, terminated by {0, 0}
		/// </summary>
		Frame_Rate,

		/// <summary>
		/// int, terminated by 0
		/// </summary>
		Sample_Rate,

		/// <summary>
		/// AVSampleFormat, terminated by AV_SAMPLE_FMT_NONE
		/// </summary>
		Sample_Format,

		/// <summary>
		/// AVChannelLayout, terminated by {0}
		/// </summary>
		Channel_Layout,

		/// <summary>
		/// AVColorRange, terminated by AVCOL_RANGE_UNSPECIFIED
		/// </summary>
		Color_Range,

		/// <summary>
		/// AVColorSpace, terminated by AVCOL_SPC_UNSPECIFIED
		/// </summary>
		Color_Space,

		/// <summary>
		/// AVAlphaMode, terminated by AVALPHA_MODE_UNSPECIFIED
		/// </summary>
		Alpha_Mode
	}
}
