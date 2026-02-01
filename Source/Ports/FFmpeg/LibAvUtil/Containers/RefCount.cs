/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class RefCount : IRefCount, IOpaque
	{
		/// <summary>
		/// 
		/// </summary>
		internal c_uint Ref_Count;

		/// <summary>
		/// 
		/// </summary>
		internal AvRefStructOpaque Opaque;

		/// <summary>
		/// 
		/// </summary>
		internal UtilFunc.Free2_Cb_Delegate Free_Cb;

		/// <summary>
		/// 
		/// </summary>
		internal UtilFunc.Free_Delegate Free;
	}
}
