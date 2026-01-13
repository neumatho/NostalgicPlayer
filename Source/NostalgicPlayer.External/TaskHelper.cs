/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polycode.NostalgicPlayer.External
{
	/// <summary>
	/// Helper class to run tasks with cancellation support
	/// </summary>
	public class TaskHelper
	{
		/// <summary></summary>
		public delegate Task TaskHandler(CancellationToken cancellationToken);
		/// <summary></summary>
		public delegate void TaskExceptionHandler(Exception ex);

		private readonly Lock taskLock = new Lock();
		private CancellationTokenSource runningTaskCancellationToken = null;
		private Task runningTask = null;

		/********************************************************************/
		/// <summary>
		/// Will cancel an already running task, and start a new one
		/// </summary>
		/********************************************************************/
		public void RunTask(TaskHandler handler, TaskExceptionHandler exceptionHandler)
		{
			lock (taskLock)
			{
				CancelTask();

				runningTaskCancellationToken = new CancellationTokenSource();
				runningTask = Task.Run(async () =>
				{
					try
					{
						await handler(runningTaskCancellationToken.Token);
					}
					catch (OperationCanceledException)
					{
						// Ignore cancellation exceptions
					}
					catch (Exception ex)
					{
						if (exceptionHandler == null)
							throw;

						exceptionHandler(ex);
					}
				});
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will cancel an already running task if any
		/// </summary>
		/********************************************************************/
		public void CancelTask()
		{
			lock (taskLock)
			{
				if ((runningTaskCancellationToken != null) && !runningTask.IsCompleted)
				{
					runningTaskCancellationToken.Cancel();
					runningTask.Wait();

					runningTask.Dispose();
					runningTaskCancellationToken.Dispose();
				}

				runningTaskCancellationToken = null;
				runningTask = null;
			}
		}
	}
}
