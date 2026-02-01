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
	public enum AvHwAccelCodecCap
	{
		/// <summary>
		/// HWAccel is experimental and is thus avoided in favor of non experimental
		/// codecs
		/// </summary>
		Experimental = 0x200
	}
}
