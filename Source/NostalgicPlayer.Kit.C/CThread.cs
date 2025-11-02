/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Diagnostics;
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
		/// 
		/// </summary>
		/********************************************************************/
		public static void PThread_Mutex_Destroy(pthread_mutex_t mutex)
		{
			mutex?.Dispose();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void PThread_Mutex_Lock(pthread_mutex_t mutex)
		{
			mutex.WaitOne();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void PThread_Mutex_Unlock(pthread_mutex_t mutex)
		{
			mutex.ReleaseMutex();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void PThread_Cond_Destroy(pthread_cond_t cond)
		{
			cond?.Dispose();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void PThread_Cond_Signal(pthread_cond_t cond)
		{
			if (Volatile.Read(ref cond.Waiters) > 0)
				cond.Semaphore.Release(1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void PThread_Cond_Broadcast(pthread_cond_t cond)
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
		public static void PThread_Cond_Wait(pthread_cond_t cond, pthread_mutex_t mutex)
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
		public static void PThread_Join(pthread_t thread)
		{
			thread.Join();
		}
	}
}
