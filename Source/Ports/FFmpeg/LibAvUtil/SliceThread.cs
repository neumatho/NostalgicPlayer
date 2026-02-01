/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Compat;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	public static class SliceThread
	{
		/********************************************************************/
		/// <summary>
		/// Create slice threading context
		/// </summary>
		/********************************************************************/
		public static c_int AvPriv_SliceThread_Create(out AvSliceThread pCtx, IOpaque priv, UtilFunc.Thread_Worker_Func_Delegate worker_Func, UtilFunc.Thread_Main_Func_Delegate main_Func, c_int nb_Threads)//XX 99
		{
			if (nb_Threads == 0)
			{
				c_int nb_Cpus = Cpu.Av_Cpu_Count();

				if (nb_Cpus > 1)
					nb_Threads = Macros.FFMin(nb_Cpus + 1, UtilConstants.Max_Auto_Threads);
				else
					nb_Threads = 1;
			}

			c_int nb_Workers = nb_Threads;

			if (main_Func == null)
				nb_Workers--;

			AvSliceThread ctx = pCtx = Mem.Av_MAlloczObj<AvSliceThread>();

			if (ctx == null)
				return Error.ENOMEM;

			if ((nb_Workers != 0) && ((ctx.Workers = Mem.Av_CAllocObj<WorkerContext>((size_t)nb_Workers)).IsNull))
			{
				Mem.Av_FreeP(ref pCtx);

				return Error.ENOMEM;
			}

			ctx.Priv = priv;
			ctx.Worker_Func = worker_Func;
			ctx.Main_Func = main_Func;
			ctx.Nb_Threads = nb_Threads;
			ctx.Nb_Active_Threads = 0;
			ctx.Nb_Jobs = 0;
			ctx.Finished = 0;

			StdAtomic.Atomic_Init(ref ctx.First_Job, 0U);
			StdAtomic.Atomic_Init(ref ctx.Current_Job, 0U);

			c_int ret = CThread.pthread_mutex_init(out ctx.Done_Mutex);

			if (ret != 0)
			{
				Mem.Av_FreeP(ref ctx.Workers);
				Mem.Av_FreeP(ref pCtx);

				return ret;
			}

			ret = CThread.pthread_cond_init(out ctx.Done_Cond);

			if (ret != 0)
			{
				ctx.Nb_Threads = main_Func != null ? 0 : 1;

				AvPriv_SliceThread_Free(ref pCtx);

				return ret;
			}

			ctx.Done = 0;

			for (c_int i = 0; i < nb_Workers; i++)
			{
				WorkerContext w = ctx.Workers[i];

				w.Ctx = ctx;

				ret = CThread.pthread_mutex_init(out w.Mutex);

				if (ret != 0)
				{
					ctx.Nb_Threads = main_Func != null ? i : i + 1;

					AvPriv_SliceThread_Free(ref pCtx);

					return ret;
				}

				ret = CThread.pthread_cond_init(out w.Cond);

				if (ret != 0)
				{
					CThread.pthread_mutex_destroy(w.Mutex);

					ctx.Nb_Threads = main_Func != null ? i : i + 1;

					AvPriv_SliceThread_Free(ref pCtx);

					return ret;
				}

				CThread.pthread_mutex_lock(w.Mutex);

				w.Done = 0;

				ret = CThread.pthread_create(out w.Thread, Thread_Worker, w);

				if (ret != 0)
				{
					ctx.Nb_Threads = main_Func != null ? i : i + 1;

					CThread.pthread_mutex_unlock(w.Mutex);
					CThread.pthread_cond_destroy(w.Cond);
					CThread.pthread_mutex_destroy(w.Mutex);

					AvPriv_SliceThread_Free(ref pCtx);

					return ret;
				}

				while (w.Done == 0)
					CThread.pthread_cond_wait(w.Cond, w.Mutex);

				CThread.pthread_mutex_unlock(w.Mutex);
			}

			return nb_Threads;
		}



		/********************************************************************/
		/// <summary>
		/// Execute slice threading
		/// </summary>
		/********************************************************************/
		public static void AvPriv_SliceThread_Execute(AvSliceThread ctx, c_int nb_Jobs, c_int execute_Main)//XX 191
		{
			c_int is_Last = 0;

			ctx.Nb_Jobs = nb_Jobs;
			ctx.Nb_Active_Threads = Macros.FFMin(nb_Jobs, ctx.Nb_Threads);

			StdAtomic.Atomic_Store(ref ctx.First_Job, 0);
			StdAtomic.Atomic_Store(ref ctx.Current_Job, (c_uint)ctx.Nb_Active_Threads);

			c_int nb_Workers = ctx.Nb_Active_Threads;

			if ((ctx.Main_Func == null) || (execute_Main == 0))
				nb_Workers--;

			for (c_int i = 0; i < nb_Workers; i++)
			{
				WorkerContext w = ctx.Workers[i];

				CThread.pthread_mutex_lock(w.Mutex);

				w.Done = 0;

				CThread.pthread_cond_signal(w.Cond);
				CThread.pthread_mutex_unlock(w.Mutex);
			}

			if ((ctx.Main_Func != null) && (execute_Main != 0))
				ctx.Main_Func(ctx.Priv);
			else
				is_Last = Run_Jobs(ctx);

			if (is_Last == 0)
			{
				CThread.pthread_mutex_lock(ctx.Done_Mutex);

				while (ctx.Done == 0)
					CThread.pthread_cond_wait(ctx.Done_Cond, ctx.Done_Mutex);

				ctx.Done = 0;

				CThread.pthread_mutex_unlock(ctx.Done_Mutex);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void AvPriv_SliceThread_Free(ref AvSliceThread pCtx)//XX 226
		{
			AvSliceThread ctx = pCtx;

			if (ctx == null)
				return;

			c_int nb_Workers = ctx.Nb_Threads;

			if (ctx.Main_Func == null)
				nb_Workers--;

			ctx.Finished = 1;

			for (c_int i = 0; i < nb_Workers; i++)
			{
				WorkerContext w = ctx.Workers[i];

				CThread.pthread_mutex_lock(w.Mutex);
				w.Done = 0;
				CThread.pthread_cond_signal(w.Cond);
				CThread.pthread_mutex_unlock(w.Mutex);
			}

			for (c_int i = 0; i < nb_Workers; i++)
			{
				WorkerContext w = ctx.Workers[i];

				CThread.pthread_join(w.Thread);
				CThread.pthread_cond_destroy(w.Cond);
				CThread.pthread_mutex_destroy(w.Mutex);
			}

			CThread.pthread_cond_destroy(ctx.Done_Cond);
			CThread.pthread_mutex_destroy(ctx.Done_Mutex);

			Mem.Av_FreeP(ref ctx.Workers);
			Mem.Av_FreeP(ref pCtx);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Run_Jobs(AvSliceThread ctx)//XX 57
		{
			c_uint nb_Jobs = (c_uint)ctx.Nb_Jobs;
			c_uint nb_Active_Threads = (c_uint)ctx.Nb_Active_Threads;
			c_uint first_Job = StdAtomic.Atomic_Fetch_Add(ref ctx.First_Job, 1);
			c_uint current_Job = first_Job;

			do
			{
				ctx.Worker_Func(ctx.Priv, (c_int)current_Job, (c_int)first_Job, (c_int)nb_Jobs, (c_int)nb_Active_Threads);
			}
			while ((current_Job = StdAtomic.Atomic_Fetch_Add(ref ctx.Current_Job, 1)) < nb_Jobs);

			return current_Job == (nb_Jobs + nb_Active_Threads - 1) ? 1 : 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Thread_Worker(object v)//XX 71
		{
			WorkerContext w = (WorkerContext)v;
			AvSliceThread ctx = w.Ctx;

			CThread.pthread_mutex_lock(w.Mutex);
			CThread.pthread_cond_signal(w.Cond);

			while (true)
			{
				w.Done = 1;

				while (w.Done != 0)
					CThread.pthread_cond_wait(w.Cond, w.Mutex);

				if (ctx.Finished != 0)
				{
					CThread.pthread_mutex_unlock(w.Mutex);
					return;
				}

				if (Run_Jobs(ctx) != 0)
				{
					CThread.pthread_mutex_lock(ctx.Done_Mutex);

					ctx.Done = 1;

					CThread.pthread_cond_signal(ctx.Done_Cond);
					CThread.pthread_mutex_unlock(ctx.Done_Mutex);
				}
			}
		}
		#endregion
	}
}
