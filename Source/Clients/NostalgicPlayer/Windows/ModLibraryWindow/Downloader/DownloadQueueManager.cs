/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Events;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Sources;
using Polycode.NostalgicPlayer.External.ModLibrary.Models;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Downloader
{
	/// <summary>
	/// Manages the download queue for batch downloading modules
	/// </summary>
	public class DownloadQueueManager
	{
		private readonly IModLibraryWindowApi windowApi;
		private readonly IModLibraryHelper modLibraryHelper;

		private readonly Lock queueLock = new Lock();
		private readonly Queue<DownloadQueueItem> queue = new Queue<DownloadQueueItem>();

		// Is set as long there are items in the queue
		private ManualResetEvent queueEvent;
		private ManualResetEvent exitEvent;
		private Thread queueThread;
		private CancellationTokenSource cancellationToken = null;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public DownloadQueueManager(IModLibraryWindowApi modLibraryWindowApi, IModLibraryHelper modLibraryHelper)
		{
			windowApi = modLibraryWindowApi;
			this.modLibraryHelper = modLibraryHelper;
		}



		/********************************************************************/
		/// <summary>
		/// Start and initialize the queue
		/// </summary>
		/********************************************************************/
		public void Start()
		{
			exitEvent = new ManualResetEvent(false);
			queueEvent = new ManualResetEvent(false);

			queueThread = new Thread(WorkerLoop);
			queueThread.Name = "ModLibrary queue worker";
			queueThread.Start();
		}



		/********************************************************************/
		/// <summary>
		/// Stop the queue processing and stop any downloading
		/// </summary>
		/********************************************************************/
		public void Stop()
		{
			if (exitEvent != null)
			{
				exitEvent.Set();

				queueThread.Join();
				queueThread = null;

				exitEvent.Dispose();
				exitEvent = null;

				queueEvent.Dispose();
				queueEvent = null;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Ask to cancel downloading
		/// </summary>
		/********************************************************************/
		public void CancelDownload()
		{
			cancellationToken?.Cancel();
		}



		/********************************************************************/
		/// <summary>
		/// Will put all the items in queue to be downloaded
		/// </summary>
		/********************************************************************/
		public void EnqueueModules(IEnumerable<DownloadModuleItem> items, PlayFlag playFlag)
		{
			lock (queueLock)
			{
				if ((playFlag == PlayFlag.AddToPlayListAndPlay) && (queue.Count > 0))
					playFlag = PlayFlag.AddToPlayList;

				int itemsAdded = 0;

				foreach (DownloadModuleItem downloadItem in items)
				{
					if (!downloadItem.Entry.IsDirectory)
					{
						// Get relative path without source prefix
						string moduleName = GetRelativePathFromSource(downloadItem.Source, downloadItem.Entry.FullPath);
						string relativePath = string.Empty;

						int lastSlashIndex = moduleName.LastIndexOf('/');
						if (lastSlashIndex != -1)
						{
							relativePath = moduleName.Substring(0, lastSlashIndex);
							moduleName = moduleName.Substring(lastSlashIndex + 1);
						}

						// Build local file path
						string localPath = Path.Combine(modLibraryHelper.ModulesDirectory, downloadItem.Source.FolderName, relativePath.Replace('/', Path.DirectorySeparatorChar), moduleName);
						string localDirectory = Path.GetDirectoryName(localPath);

						queue.Enqueue(new DownloadQueueItem(relativePath, localDirectory, moduleName, downloadItem.Source.GetDownloader(), playFlag));
						itemsAdded++;

						if (playFlag == PlayFlag.AddToPlayListAndPlay)
							playFlag = PlayFlag.AddToPlayList;
					}
				}

				if (itemsAdded > 0)
				{
					windowApi.Form.Invoke(() => AddedToQueue?.Invoke(this, new AddedToQueueEventArgs(itemsAdded)));
					queueEvent.Set();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public event EventHandler<AddedToQueueEventArgs> AddedToQueue;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public event EventHandler<DownloadStartedEventArgs> DownloadStarted;



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
		public event EventHandler<AllDownloadsCompletedEventArgs> AllDownloadsCompleted;

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Background worker loop
		/// </summary>
		/********************************************************************/
		private async void WorkerLoop()
		{
			WaitHandle[] waitArray = [ exitEvent, queueEvent ];
			int failedCount = 0;
			bool isCanceled = false;

			for (;;)
			{
				int waitResult = WaitHandle.WaitAny(waitArray);

				if (waitResult == 0)
					return;

				DownloadQueueItem queueItem = null;

				lock (queueLock)
				{
					if (queue.Count == 0)
					{
						queueEvent.Reset();

						if (!isCanceled)
							windowApi.Form.Invoke((int count) => AllDownloadsCompleted?.Invoke(this, new AllDownloadsCompletedEventArgs(count)), failedCount);

						failedCount = 0;
						isCanceled = false;

						if (cancellationToken != null)
						{
							cancellationToken.Dispose();
							cancellationToken = null;
						}
					}
					else
					{
						if (cancellationToken == null)
							cancellationToken = new CancellationTokenSource();

						queueItem = queue.Dequeue();
					}
				}

				if (queueItem != null)
				{
					windowApi.Form.Invoke((string name) => DownloadStarted?.Invoke(this, new DownloadStartedEventArgs(name)), queueItem.FileName);
					bool success = true;

					try
					{
						await DownloadModuleAsync(queueItem, cancellationToken.Token);
					}
					catch (TaskCanceledException)
					{
						lock (queueLock)
						{
							queue.Clear();
							isCanceled = true;
						}
					}
					catch (Exception)
					{
						failedCount++;
						success = false;
					}

					if (!isCanceled)
						windowApi.Form.Invoke((string localPath, PlayFlag playFlag) => DownloadCompleted?.Invoke(this, new DownloadCompletedEventArgs(localPath, playFlag, success)), Path.Combine(queueItem.DestinationPath, queueItem.FileName), queueItem.PlayFlag);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Download a single module
		/// </summary>
		/********************************************************************/
		private async Task DownloadModuleAsync(DownloadQueueItem queueItem, CancellationToken token)
		{
			Directory.CreateDirectory(queueItem.DestinationPath);

			if (File.Exists(Path.Combine(queueItem.DestinationPath, queueItem.FileName)))
				return;

			DownloadedFileModel[] downloadedFiles = await queueItem.Downloader.DownloadModuleAsync(queueItem.SourcePath, queueItem.FileName, FindExtraFiles(queueItem.FileName).ToArray(), token);

			foreach (DownloadedFileModel file in downloadedFiles)
				await File.WriteAllBytesAsync(Path.Combine(queueItem.DestinationPath, file.FileName), file.Content, token);
		}



		/********************************************************************/
		/// <summary>
		/// Get relative path without source prefix
		/// </summary>
		/********************************************************************/
		private string GetRelativePathFromSource(IModLibrarySource source, string fullPath)
		{
			if (fullPath.StartsWith(source.RootPath))
				return fullPath.Substring(source.RootPath.Length);

			return string.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// Find extra files which needs to be downloaded
		/// </summary>
		/********************************************************************/
		private IEnumerable<string> FindExtraFiles(string fileName)
		{
			// Check if this is an mdat.* file - download matching smpl.* file
			if (fileName.StartsWith("mdat.", StringComparison.OrdinalIgnoreCase))
				yield return "smpl" + fileName.Substring(4);
		}
		#endregion
	}
}
