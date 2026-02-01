/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class AvDes
	{
		/// <summary>
		/// 
		/// </summary>
		public readonly uint64_t[][] Round_Keys = ArrayHelper.Initialize2Arrays<uint64_t>(3, 16);

		/// <summary>
		/// 
		/// </summary>
		public c_int Triple_Des;
	}
}
