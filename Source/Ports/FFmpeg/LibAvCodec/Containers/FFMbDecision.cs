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
	public enum FFMbDecision
	{
		/// <summary>
		/// Uses mb_cmp
		/// </summary>
		Simple = 0,

		/// <summary>
		/// Chooses the one which needs the fewest bits
		/// </summary>
		Bits = 1,

		/// <summary>
		/// Rate distortion
		/// </summary>
		Rd = 2
	}
}
