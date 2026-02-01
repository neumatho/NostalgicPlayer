/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Demuxer;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Supported
	{
		/// <summary>
		/// 
		/// </summary>
		public static readonly UrlProtocol[] Url_Protocols =
		[
		];

		/// <summary>
		/// 
		/// </summary>
		public static readonly FFOutputFormat[] Muxer_List =
		[
		];

		/// <summary>
		/// 
		/// </summary>
		public static readonly FFInputFormat[] Demuxer_List =
		[
			AsfDec_F.FF_Asf_Demuxer
		];
	}
}
