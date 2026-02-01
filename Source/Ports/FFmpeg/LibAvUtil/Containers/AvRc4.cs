/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class AvRc4
	{
		/// <summary>
		/// 
		/// </summary>
		public readonly uint8_t[] State = new uint8_t[256];

		/// <summary>
		/// 
		/// </summary>
		public c_int X;

		/// <summary>
		/// 
		/// </summary>
		public c_int Y;
	}
}
