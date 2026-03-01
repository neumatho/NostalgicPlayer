/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Events;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow
{
	/// <summary>
	/// Manages the download queue for batch downloading modules
	/// </summary>
	internal class DownloadQueueManager
	{
		private readonly Queue<DownloadQueueItem> queue = new();
		private readonly HashSet<string> queuedPaths = new();
		private readonly object queueLock = new();
		private readonly AutoResetEvent queueEvent = new(false);
		private readonly ModLibraryDownloadService downloadService;

		private volatile bool stopRequested;
		private Thread workerThread;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public DownloadQueueManager(ModLibraryDownloadService downloadService)
		{
			this.downloadService = downloadService;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int SuccessCount
		{
			get;
			private set;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int FailureCount
		{
			get;
			private set;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int QueueCount
		{
			get
			{
				lock (queueLock)
				{
					return queue.Count;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public event EventHandler<DownloadProgressEventArgs> ProgressChanged;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public event EventHandler<DownloadCompletedEventArgs> DownloadCompleted;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public event EventHandler QueueCompleted;



		/********************************************************************/
		/// <summary>
		/// Enqueue files for download (avoiding duplicates)
		/// </summary>
		/********************************************************************/
		public void EnqueueFiles(IEnumerable<TreeNode> entries, bool playFirst)
		{
			lock (queueLock)
			{
				bool isFirst = true;

				foreach (var entry in entries)
				{
					if (!entry.IsDirectory && !queuedPaths.Contains(entry.FullPath))
					{
						queuedPaths.Add(entry.FullPath);
						queue.Enqueue(new DownloadQueueItem(entry, isFirst && playFirst));
						isFirst = false;
					}
				}
			}

			// Start worker thread if not running
			if (workerThread == null || !workerThread.IsAlive)
			{
				stopRequested = false;

				workerThread = new Thread(WorkerLoop)
				{
					IsBackground = true,
					Name = "DownloadQueueWorker"
				};
				workerThread.Start();
			}
			else
			{
				// Wake up the worker thread
				queueEvent.Set();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Recursively collect all files from a directory
		/// </summary>
		/********************************************************************/
		public void EnqueueDirectory(TreeNode directoryNode, ModLibraryData data, bool playFirst)
		{
			// Use flat view mode to get all files in one call instead of recursing
			var allEntries = data.GetEntries(directoryNode.FullPath, true, FlatViewSortOrder.NameThenPath);

			// Filter to only files (flat view may include direct subfolders)
			var files = allEntries.Where(e => !e.IsDirectory);

			EnqueueFiles(files, playFirst);
		}



		/********************************************************************/
		/// <summary>
		/// Clears pending downloads from the queue
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			lock (queueLock)
			{
				queue.Clear();
				queuedPaths.Clear();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Terminates the worker thread (call when window is closing)
		/// </summary>
		/********************************************************************/
		public void Terminate()
		{
			stopRequested = true;

			// Wake up thread so it can exit
			queueEvent.Set();
		}



		/********************************************************************/
		/// <summary>
		/// Background worker loop
		/// </summary>
		/********************************************************************/
		private void WorkerLoop()
		{
			SuccessCount = 0;
			FailureCount = 0;

			while (!stopRequested)
			{
				DownloadQueueItem item = null;
				int remainingCount = 0;

				lock (queueLock)
				{
					if (queue.Count > 0)
					{
						item = queue.Dequeue();
						queuedPaths.Remove(item.Entry.FullPath);
						remainingCount = queue.Count;
					}
				}

				if (item != null)
				{
					// Fire progress event
					ProgressChanged?.Invoke(this, new DownloadProgressEventArgs(remainingCount, item.Entry));

					// Download file
					try
					{
						string localPath = downloadService.DownloadModule(item.Entry);

						SuccessCount++;

						// Fire completed event
						DownloadCompleted?.Invoke(this,
							new DownloadCompletedEventArgs(item.Entry, localPath, item.ShouldPlayImmediately, true));
					}
					catch (Exception ex)
					{
						FailureCount++;

						// Fire completed event with error
						DownloadCompleted?.Invoke(this,
							new DownloadCompletedEventArgs(item.Entry, null, item.ShouldPlayImmediately, false, ex.Message));
					}

					// Check if queue is now empty
					bool queueEmpty;
					lock (queueLock)
					{
						queueEmpty = queue.Count == 0;
					}

					if (queueEmpty)
					{
						QueueCompleted?.Invoke(this, EventArgs.Empty);
					}
				}
				else
				{
					// Wait for new items
					queueEvent.WaitOne();

					if (stopRequested)
						return;
				}
			}
		}
	}
}
