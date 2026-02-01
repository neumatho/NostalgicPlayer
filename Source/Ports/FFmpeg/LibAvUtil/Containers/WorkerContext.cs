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
	internal class WorkerContext
	{
		/// <summary>
		/// 
		/// </summary>
		public AvSliceThread Ctx;

		/// <summary>
		/// 
		/// </summary>
		public pthread_mutex_t Mutex;

		/// <summary>
		/// 
		/// </summary>
		public pthread_cond_t Cond;

		/// <summary>
		/// 
		/// </summary>
		public pthread_t Thread;

		/// <summary>
		/// 
		/// </summary>
		public c_int Done;
	}
}
