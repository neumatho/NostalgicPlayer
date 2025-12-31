/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Polycode.NostalgicPlayer.Kit.C
{
	/// <summary>
	/// C like thread methods
	/// </summary>
	public static class CThread
	{
		/// <summary>
		/// 
		/// </summary>
		public const uint64_t Clocks_Per_Sec = TimeSpan.TicksPerSecond;

		/// <summary>
		/// 
		/// </summary>
		public delegate void ThreadOnce_Init_Delegate();

		#region CondHandler class
		/// <summary>
		/// Helper class to simulate pthread_cond_* functions
		/// </summary>
		public class CondHandler : IDisposable
		{
			internal readonly SemaphoreSlim Semaphore = new SemaphoreSlim(0, int.MaxValue);
			internal c_int Waiters;

			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public void Dispose()
			{
				Semaphore.Dispose();
			}
		}
		#endregion

		#region ThreadOnce class
		/// <summary>
		/// 
		/// </summary>
		public class ThreadOnce
		{
			/********************************************************************/
			/// <summary>
			/// Indicate if the initialization method has been called
			/// </summary>
			/********************************************************************/
			internal bool HasBeenInitialized { get; set; } = false;
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Returns the approximate processor time that is consumed by the
		/// program which is the number of clock ticks used by the program
		/// since the program started
		/// </summary>
		/********************************************************************/
		public static clock_t clock()
		{
			TimeSpan processorTime = Process.GetCurrentProcess().TotalProcessorTime;

			return processorTime.Ticks;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the thread once state
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static pthread_once_t pthread_once_init()
		{
			return new pthread_once_t();
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the thread once state
		/// </summary>
		/********************************************************************/
		public static c_int pthread_once(pthread_once_t once_Control, ThreadOnce_Init_Delegate init_Routine)
		{
			lock (once_Control)
			{
				if (!once_Control.HasBeenInitialized)
				{
					init_Routine();
					once_Control.HasBeenInitialized = true;
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int pthread_mutex_init(out pthread_mutex_t mutex)
		{
			mutex = new pthread_mutex_t();

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void pthread_mutex_destroy(pthread_mutex_t mutex)
		{
			mutex?.Dispose();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void pthread_mutex_lock(pthread_mutex_t mutex)
		{
			mutex.WaitOne();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void pthread_mutex_unlock(pthread_mutex_t mutex)
		{
			mutex.ReleaseMutex();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void pthread_cond_destroy(pthread_cond_t cond)
		{
			cond?.Dispose();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void pthread_cond_signal(pthread_cond_t cond)
		{
			if (Volatile.Read(ref cond.Waiters) > 0)
				cond.Semaphore.Release(1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void pthread_cond_broadcast(pthread_cond_t cond)
		{
			c_int count = Volatile.Read(ref cond.Waiters);
			if (count > 0)
				cond.Semaphore.Release(count);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void pthread_cond_wait(pthread_cond_t cond, pthread_mutex_t mutex)
		{
			Interlocked.Increment(ref cond.Waiters);
			mutex.ReleaseMutex();

			try
			{
				cond.Semaphore.Wait();
			}
			finally
			{
				Interlocked.Decrement(ref cond.Waiters);
				mutex.WaitOne();
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void pthread_join(pthread_t thread)
		{
			thread.Join();
		}
	}
}
