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
	internal class AvCodecHwConfigInternal : AvCodecHwConfig
	{
		/// <summary>
		/// This is the structure which will be returned to the user by
		/// avcodec_get_hw_config()
		/// </summary>
		public AvCodecHwConfig Public => this;

		// <summary>
		// If this configuration uses a hwaccel, a pointer to it.
		// If not, NULL
		// </summary>
//		public FFHwAccel HwAccel;
	}
}
