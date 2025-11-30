/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.ModLibraryWindow
{
	/// <summary>
	/// Manages the download queue for batch downloading modules
	/// </summary>
	internal class DownloadQueueManager
	{
		private readonly Queue<DownloadQueueItem> queue = new();
		private readonly HashSet<string> queuedPaths = new();
		private readonly object queueLock = new();
		private CancellationTokenSource cancellationTokenSource;
		private int successCount = 0;
		private int failureCount = 0;

		public bool IsProcessing
		{
			get;
			private set;
		}

		public int SuccessCount => successCount;
		public int FailureCount => failureCount;

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

		public event EventHandler<DownloadProgressEventArgs> ProgressChanged;
		public event EventHandler<DownloadCompletedEventArgs> DownloadCompleted;
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
					if (!entry.IsDirectory && !queuedPaths.Contains(entry.FullPath))
					{
						queuedPaths.Add(entry.FullPath);
						queue.Enqueue(new DownloadQueueItem(entry, isFirst && playFirst));
						isFirst = false;
					}
			}
		}


		/********************************************************************/
		/// <summary>
		/// Recursively collect all files from a directory
		/// </summary>
		/********************************************************************/
		public void EnqueueDirectory(TreeNode directoryNode, ModLibraryData data, bool playFirst)
		{
			var files = CollectFilesRecursive(directoryNode, data);
			EnqueueFiles(files, playFirst);
		}


		/********************************************************************/
		/// <summary>
		/// Collect all files recursively from a directory node
		/// </summary>
		/********************************************************************/
		private List<TreeNode> CollectFilesRecursive(TreeNode node, ModLibraryData data)
		{
			List<TreeNode> result = new();

			if (node.IsDirectory)
			{
				// Get children from data
				var children = data.GetEntries(node.FullPath, false, FlatViewSortOrder.NameThenPath);

				foreach (var child in children)
					if (child.IsDirectory)
						result.AddRange(CollectFilesRecursive(child, data));
					else
						result.Add(child);
			}
			else
				result.Add(node);

			return result;
		}


		/********************************************************************/
		/// <summary>
		/// Start processing the download queue
		/// </summary>
		/********************************************************************/
		public async Task ProcessQueueAsync(Func<TreeNode, bool, CancellationToken, Task<string>> downloadFunc)
		{
			if (IsProcessing)
				return;

			IsProcessing = true;
			cancellationTokenSource = new CancellationTokenSource();

			// Reset counters
			successCount = 0;
			failureCount = 0;

			try
			{
				while (true)
				{
					if (cancellationTokenSource.Token.IsCancellationRequested)
						break;

					DownloadQueueItem item;
					int remainingCount;

					// Dequeue with lock
					lock (queueLock)
					{
						if (queue.Count == 0)
							break;

						item = queue.Dequeue();
						queuedPaths.Remove(item.Entry.FullPath);
						remainingCount = queue.Count; // Files still in queue
					}

					// Fire progress event
					ProgressChanged?.Invoke(this, new DownloadProgressEventArgs(remainingCount, item.Entry));

					// Download file
					try
					{
						string localPath = await downloadFunc(item.Entry, item.ShouldPlayImmediately,
							cancellationTokenSource.Token);

						successCount++;

						// Fire completed event
						DownloadCompleted?.Invoke(this,
							new DownloadCompletedEventArgs(item.Entry, localPath, item.ShouldPlayImmediately, true));
					}
					catch (Exception ex)
					{
						failureCount++;

						// Fire completed event with error
						DownloadCompleted?.Invoke(this,
							new DownloadCompletedEventArgs(item.Entry, null, item.ShouldPlayImmediately, false, ex.Message));
					}
				}
			}
			finally
			{
				IsProcessing = false;
				cancellationTokenSource?.Dispose();
				cancellationTokenSource = null;

				QueueCompleted?.Invoke(this, EventArgs.Empty);
			}
		}


		/********************************************************************/
		/// <summary>
		/// Cancel the current download queue
		/// </summary>
		/********************************************************************/
		public void Cancel()
		{
			cancellationTokenSource?.Cancel();

			// Clear the queue
			lock (queueLock)
			{
				queue.Clear();
				queuedPaths.Clear();
			}
		}
	}
}