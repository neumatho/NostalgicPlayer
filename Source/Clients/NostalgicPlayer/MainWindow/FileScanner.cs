/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Krypton.Toolkit;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Modules;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;
using Polycode.NostalgicPlayer.PlayerLibrary.Containers;
using Polycode.NostalgicPlayer.PlayerLibrary.Loaders;
using Polycode.NostalgicPlayer.PlayerLibrary.Players;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow
{
	/// <summary>
	/// Class that scans added files to find extra information
	/// </summary>
	public class FileScanner
	{
		private class QueueInfo
		{
			public List<ModuleListItem> Items;
		}

		private readonly KryptonListBox listBox;
		private readonly OptionSettings settings;
		private readonly Manager manager;
		private readonly ModuleDatabase database;
		private readonly MainWindowForm mainWindowForm;

		private ManualResetEvent shutdownEvent;
		private Thread scannerThread;

		private Semaphore queueSemaphore;
		private Queue<QueueInfo> queue;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public FileScanner(KryptonListBox listBox, OptionSettings optionSettings, Manager agentManager, ModuleDatabase moduleDatabase, MainWindowForm mainWindow)
		{
			this.listBox = listBox;
			settings = optionSettings;
			manager = agentManager;
			database = moduleDatabase;
			mainWindowForm = mainWindow;
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

			// Create queue
			queue = new Queue<QueueInfo>();
			queueSemaphore = new Semaphore(0, 1000);

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
				lock (queue)
				{
					queue.Enqueue(new QueueInfo { Items = items.ToList() });
					queueSemaphore.Release();
				}
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
							QueueInfo queueInfo;

							lock (queue)
							{
								queueInfo = queue.Dequeue();
							}

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
				foreach (ModuleListItem listItem in queueInfo.Items)
				{
					// Get needed information via the main thread
					string fileName = listItem.ListItem.FullPath;
					bool haveTime = listItem.HaveTime;

					// If the item already have a time, skip it
					if (haveTime || (fileName == null))
						continue;

					// Now check if the file already have a time in the database
					ModuleDatabaseInfo moduleDatabaseInfo = database.RetrieveInformation(fileName);

					// Did we get the total time
					if ((moduleDatabaseInfo == null) || (moduleDatabaseInfo.Duration.Ticks == 0))
					{
						// No, try to load the file and let the player return the total time
						moduleDatabaseInfo = new ModuleDatabaseInfo(GetPlayerTime(fileName), 0, DateTime.MinValue);

						// Update the information in the database
						database.StoreInformation(fileName, moduleDatabaseInfo);
					}

					// Update the list item
					listBox.Invoke(() =>
					{
						mainWindowForm.SetTimeOnItem(listItem, moduleDatabaseInfo.Duration);
					});
				}
			}
			catch (Exception)
			{
				// Ignore any exception
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will try to load the given module and find the time
		/// </summary>
		/********************************************************************/
		private TimeSpan GetPlayerTime(string fileName)
		{
			try
			{
				Loader loader = new Loader(manager);
				if (loader.Load(fileName, out string _))
				{
					IPlayer player = loader.Player;

					if (player.InitPlayer(new PlayerConfiguration(null, loader, new MixerConfiguration()), out string _))
					{
						try
						{
							if (player is IModulePlayer modulePlayer)
							{
								if (!modulePlayer.SelectSong(-1, out _))
									return new TimeSpan(0);
							}

							return player.PlayingModuleInformation.SongTotalTime;
						}
						finally
						{
							player.CleanupPlayer();
						}
					}
				}
			}
			catch (Exception)
			{
				// Ignore exception
			}

			return new TimeSpan(0);
		}
		#endregion
	}
}
