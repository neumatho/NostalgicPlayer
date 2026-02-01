/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces
{
	internal interface ITxComplexType<T> where T : struct
	{
		/// <summary>
		/// 
		/// </summary>
		T Re { get; set; }

		/// <summary>
		/// 
		/// </summary>
		T Im { get; set; }
	}
}
