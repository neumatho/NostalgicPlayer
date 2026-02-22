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
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow.ListItem;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Mappers;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Helpers;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Library.Agent;
using Polycode.NostalgicPlayer.Library.Containers;
using Polycode.NostalgicPlayer.Library.Loaders;
using Polycode.NostalgicPlayer.Library.Players;
using Polycode.NostalgicPlayer.Logic.Databases;
using Polycode.NostalgicPlayer.Logic.Playlists;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow
{
	/// <summary>
	/// Class that scans added files to find extra information
	/// </summary>
	public class FileScanner
	{
		private class QueueInfo
		{
			public List<ModuleListItem> Items { get; set; }
		}

		private readonly OptionSettings settings;
		private readonly IModuleDatabase database;
		private readonly IPlaylistFactory playlistFactory;
		private readonly IMainWindowApi mainWindowApi;

		private readonly ModuleListControl moduleListControl;
		private readonly Manager manager;

		private ManualResetEvent shutdownEvent;
		private AutoResetEvent breakEvent;
		private Thread scannerThread;

		private Semaphore queueSemaphore;
		private Queue<QueueInfo> queue;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public FileScanner(ModuleListControl listControl, Manager agentManager)
		{
			settings = DependencyInjection.Container.GetInstance<OptionSettings>();
			database = DependencyInjection.Container.GetInstance<IModuleDatabase>();
			playlistFactory = DependencyInjection.Container.GetInstance<IPlaylistFactory>();
			mainWindowApi = DependencyInjection.Container.GetInstance<IMainWindowApi>();

			moduleListControl = listControl;
			manager = agentManager;
		}



		/********************************************************************/
		/// <summary>
		/// Start the scanner
		/// </summary>
		/********************************************************************/
		public void Start()
		{
			// Create event used to tell the thread to stop
			shutdownEvent = new ManualResetEvent(false);

			// Create event used to break a scanning loop when the queue is cleared
			breakEvent = new AutoResetEvent(false);

			// Create queue
			queue = new Queue<QueueInfo>();
			queueSemaphore = new Semaphore(0, 50000);

			// Start the background thread
			scannerThread = new Thread(DoScannerThread);
			scannerThread.Name = "File scanner";
			scannerThread.Start();
		}



		/********************************************************************/
		/// <summary>
		/// Stop the scanner
		/// </summary>
		/********************************************************************/
		public void Stop()
		{
			// Tell the thread to exit
			shutdownEvent?.Set();

			// Wait for the thread to exit
			scannerThread?.Join();

			// Cleanup
			scannerThread = null;

			queueSemaphore?.Dispose();
			queueSemaphore = null;
			queue = null;

			breakEvent?.Dispose();
			breakEvent = null;

			shutdownEvent?.Dispose();
			shutdownEvent = null;
		}



		/********************************************************************/
		/// <summary>
		/// Will tell the scanner to scan the given range of items
		/// </summary>
		/********************************************************************/
		public void ScanItems(IEnumerable<ModuleListItem> items)
		{
			if (settings.ScanFiles)
			{
				List<ModuleListItem> list = items.ToList();
				if (list.Count > 0)
				{
					lock (queue)
					{
						queue.Enqueue(new QueueInfo { Items = list });
						queueSemaphore.Release();
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will clear the current query and stop ongoing scanning
		/// </summary>
		/********************************************************************/
		public void ClearQueue()
		{
			lock (queue)
			{
				queue.Clear();
				breakEvent.Set();
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Background thread that scans added files
		/// </summary>
		/********************************************************************/
		private void DoScannerThread()
		{
			try
			{
				bool stillRunning = true;
				WaitHandle[] waitArray = { shutdownEvent, queueSemaphore };

				while (stillRunning)
				{
					int waitResult = WaitHandle.WaitAny(waitArray);
					switch (waitResult)
					{
						// ShutdownEvent
						case 0:
						{
							stillRunning = false;
							break;
						}

						// New queue element
						case 1:
						{
							QueueInfo queueInfo = null;

							lock (queue)
							{
								if (queue.Count > 0)
									queueInfo = queue.Dequeue();
							}

							if (queueInfo != null)
								ProcessQueueInfo(queueInfo);

							break;
						}
					}
				}
			}
			catch (Exception)
			{
				// If an exception is thrown, abort the thread
			}
		}



		/********************************************************************/
		/// <summary>
		/// Process the given queue element
		/// </summary>
		/********************************************************************/
		private void ProcessQueueInfo(QueueInfo queueInfo)
		{
			try
			{
				WaitHandle[] waitArray = [ shutdownEvent, breakEvent ];

				List<ModuleListItem> itemsToRemove = new List<ModuleListItem>();
				List<ModuleListItemUpdateInfo> itemsToUpdate = new List<ModuleListItemUpdateInfo>();
				int itemsToCheckBeforeUpdating = RandomGenerator.GetRandomNumber(10, 30);

				breakEvent.Reset();

				foreach (ModuleListItem listItem in queueInfo.Items)
				{
					if (WaitHandle.WaitAny(waitArray, 0) != WaitHandle.WaitTimeout)
						return;

					// Skip stream items
					if (listItem.ListItem is IStreamModuleListItem)
						continue;

					// Get needed information
					string fileName = listItem.ListItem.Source;
					bool haveTime = listItem.HaveTime;

					// If the item already have a time, skip it
					if (haveTime || (fileName == null))
						continue;

					// Check if the file is a list or archive which needs to be expanded
					if (!CheckForListOrArchive(listItem, fileName))
						DoOtherScanningChecks(listItem, fileName, itemsToRemove, itemsToUpdate);

					// Update the list items it needed
					itemsToCheckBeforeUpdating--;
					if (itemsToCheckBeforeUpdating == 0)
					{
						itemsToCheckBeforeUpdating = RandomGenerator.GetRandomNumber(10, 30);
						UpdateModuleList(itemsToRemove, itemsToUpdate);
					}
				}

				UpdateModuleList(itemsToRemove, itemsToUpdate);
			}
			catch (Exception)
			{
				// Ignore any exception
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will check the given file to see if it is a list or archive
		/// </summary>
		/********************************************************************/
		private bool CheckForListOrArchive(ModuleListItem listItem, string fileName)
		{
			if (ArchivePath.IsArchivePath(fileName))
				return false;

			List<ModuleListItem> list = new List<ModuleListItem>();

			try
			{
				string extension = Path.GetExtension(fileName);
				if (!string.IsNullOrEmpty(extension))
					extension = extension.Substring(1).ToLower();

				using (FileStream fs = File.OpenRead(fileName))
				{
					IPlaylist playlist = playlistFactory.Create(fs, extension);
					if (playlist != null)
					{
						foreach (PlaylistFileInfo info in playlist.LoadList(Path.GetDirectoryName(fileName), fs, extension))
							list.Add(ListItemMapper.Convert(info));
					}
				}

				if (list.Count == 0)
				{
					// Check if the file is an archive
					ArchiveDetector detector = new ArchiveDetector(manager);

					bool isArchive = detector.IsArchive(fileName);
					if (isArchive)
					{
						foreach (string archiveFileName in detector.GetEntries(fileName))
							list.Add(new ModuleListItem(new ArchiveFileModuleListItem(archiveFileName)));
					}
				}

				if (list.Count > 0)
				{
					// Replace the list item with the new list
					moduleListControl.Invoke(() =>
					{
						mainWindowApi.ReplaceItemInModuleList(listItem, list);
					});

					// Add the new items in the queue for a scan
					ScanItems(list);
				}

				return list.Count > 0;
			}
			catch (Exception)
			{
				return false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will check the file for different things depending on the
		/// scanning settings
		/// </summary>
		/********************************************************************/
		private void DoOtherScanningChecks(ModuleListItem moduleListItem, string fileName, List<ModuleListItem> itemsToRemove, List<ModuleListItemUpdateInfo> itemsToUpdate)
		{
			Loader loader = null;

			try
			{
				if (settings.RemoveUnknownModules)
				{
					loader = FindPlayer(fileName);
					if (loader == null)
					{
						itemsToRemove.Add(moduleListItem);
						return;
					}
				}

				if (settings.ExtractPlayingTime)
				{
					TimeSpan? duration = null;
					ModuleDatabaseInfo moduleDatabaseInfo = null;

					if (settings.UseDatabase)
					{
						moduleDatabaseInfo = database.RetrieveInformation(fileName);

						// Did we get the total time
						if ((moduleDatabaseInfo != null) && (moduleDatabaseInfo.Duration.Ticks != 0))
							duration = moduleDatabaseInfo.Duration;
					}

					if (!duration.HasValue)
					{
						if (loader == null)
							loader = FindPlayer(fileName);

						if (loader != null)
						{
							if (loader.Load(out _))
							{
								IPlayer player = loader.Player;

								if (player.InitPlayer(new PlayerConfiguration(null, loader, SurroundMode.None, false, new MixerConfiguration()), out string _))
								{
									try
									{
										if (player is IModulePlayer modulePlayer)
										{
											if (!modulePlayer.SelectSong(-1, out _))
												return;
										}

										duration = player.PlayingModuleInformation.SongTotalTime;

										if (settings.UseDatabase)
										{
											// Update the information in the database
											if (moduleDatabaseInfo != null)
												moduleDatabaseInfo = new ModuleDatabaseInfo(duration.Value, moduleDatabaseInfo.ListenCount, moduleDatabaseInfo.LastLoaded);
											else
												moduleDatabaseInfo = new ModuleDatabaseInfo(duration.Value, 0, DateTime.MinValue);

											database.StoreInformation(fileName, moduleDatabaseInfo);
										}
									}
									finally
									{
										player.CleanupPlayer();
									}
								}
							}
						}
					}

					if (duration.HasValue)
					{
						itemsToUpdate.Add(new ModuleListItemUpdateInfo(moduleListItem)
						{
							Duration = duration.Value
						});
					}
				}
			}
			catch (Exception)
			{
				// Ignore exception
			}
			finally
			{
				loader?.Unload();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will try to find a player
		/// </summary>
		/********************************************************************/
		private Loader FindPlayer(string fileName)
		{
			Loader loader = new Loader(manager);

			if (loader.FindPlayer(fileName, out string _))
				return loader;

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Tell main window to update the list if needed
		/// </summary>
		/********************************************************************/
		private void UpdateModuleList(List<ModuleListItem> itemsToRemove, List<ModuleListItemUpdateInfo> itemsToUpdate)
		{
			if (itemsToRemove.Count > 0)
			{
				List<ModuleListItem> clonedList = new List<ModuleListItem>(itemsToRemove);
				itemsToRemove.Clear();

				mainWindowApi.Form.BeginInvoke(() =>
				{
					mainWindowApi.RemoveItemsFromModuleList(clonedList);
				});
			}

			if (itemsToUpdate.Count > 0)
			{
				List<ModuleListItemUpdateInfo> clonedList = new List<ModuleListItemUpdateInfo>(itemsToUpdate);
				itemsToUpdate.Clear();

				mainWindowApi.Form.BeginInvoke(() =>
				{
					mainWindowApi.UpdateModuleList(clonedList);
				});
			}
		}
		#endregion
	}
}
