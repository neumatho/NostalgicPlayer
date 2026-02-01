/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec
{
	/// <summary>
	/// 
	/// </summary>
	public static class To_Upper4
	{
		/********************************************************************/
		/// <summary>
		/// Converting FOURCCs to uppercase
		/// </summary>
		/********************************************************************/
		public static c_int FF_ToUpper4(c_uint x)
		{
			return AvString.Av_ToUpper((c_int)(x & 0xff)) |
			       (AvString.Av_ToUpper((c_int)((x >> 8) & 0xff)) << 8) |
			       (AvString.Av_ToUpper((c_int)((x >> 16) & 0xff)) << 16) |
			       (AvString.Av_ToUpper((c_int)((x >> 24) & 0xff)) << 24);
		}
	}
}
