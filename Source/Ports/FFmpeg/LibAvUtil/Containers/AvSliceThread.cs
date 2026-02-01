/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class AvSliceThread
	{
		/// <summary>
		/// 
		/// </summary>
		internal CPointer<WorkerContext> Workers;

		/// <summary>
		/// 
		/// </summary>
		internal c_int Nb_Threads;

		/// <summary>
		/// 
		/// </summary>
		internal c_int Nb_Active_Threads;

		/// <summary>
		/// 
		/// </summary>
		internal c_int Nb_Jobs;

		/// <summary>
		/// 
		/// </summary>
		internal c_uint First_Job;

		/// <summary>
		/// 
		/// </summary>
		internal c_uint Current_Job;

		/// <summary>
		/// 
		/// </summary>
		internal pthread_mutex_t Done_Mutex;

		/// <summary>
		/// 
		/// </summary>
		internal pthread_cond_t Done_Cond;

		/// <summary>
		/// 
		/// </summary>
		internal c_int Done;

		/// <summary>
		/// 
		/// </summary>
		internal c_int Finished;

		/// <summary>
		/// 
		/// </summary>
		internal IOpaque Priv;

		/// <summary>
		/// 
		/// </summary>
		internal UtilFunc.Thread_Worker_Func_Delegate Worker_Func;

		/// <summary>
		/// 
		/// </summary>
		internal UtilFunc.Thread_Main_Func_Delegate Main_Func;
	}
}
