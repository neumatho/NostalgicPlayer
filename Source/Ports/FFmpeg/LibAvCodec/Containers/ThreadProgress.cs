/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// This struct should be treated as opaque by users
	/// </summary>
	public class ThreadProgress : IContext
	{
		/// <summary>
		/// 
		/// </summary>
		public c_int Progress;

		/// <summary>
		/// 
		/// </summary>
		public c_uint Init;

		/// <summary>
		/// 
		/// </summary>
		public AvMutex Progress_Mutex;

		/// <summary>
		/// 
		/// </summary>
		public AvCond Progress_Cond;
	}
}
