/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class ArrayInfo<T>
	{
		/// <summary>
		/// 
		/// </summary>
		public CPointer<T> Array;

		/// <summary>
		/// 
		/// </summary>
		public c_uint Count;
	}
}
