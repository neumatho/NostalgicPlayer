/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// AVStreamGroupLCEVC is meant to define the relation between video streams
	/// and a data stream containing LCEVC enhancement layer NALUs.
	///
	/// No more than one stream of AVCodecParameters.codec_type "codec_type"
	/// AVMEDIA_TYPE_DATA shall be present, and it must be of
	/// AVCodecParameters.codec_id "codec_id" AV_CODEC_ID_LCEVC
	/// </summary>
	public class AvStreamGroupLcevc : AvClass, IGroupType
	{
		/// <summary>
		/// 
		/// </summary>
		public AvClass Av_Class => this;

		/// <summary>
		/// Index of the LCEVC data stream in AVStreamGroup
		/// </summary>
		public c_uint Lcevc_Index;

		/// <summary>
		/// Width of the final stream for presentation
		/// </summary>
		public c_int Width;

		/// <summary>
		/// Height of the final image for presentation
		/// </summary>
		public c_int Height;
	}
}
