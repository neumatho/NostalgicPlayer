/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Threading;
using Krypton.Toolkit;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Modules;
using Polycode.NostalgicPlayer.Kit.Interfaces;
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
			public int StartIndex;
			public int Count;
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
		public void ScanItems(int startIndex, int count)
		{
			if (settings.ScanFiles)
			{
				lock (queue)
				{
					queue.Enqueue(new QueueInfo { StartIndex = startIndex, Count = count });
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
				for (int index = queueInfo.StartIndex, count = queueInfo.Count; count > 0; index++, count--)
				{
					// Get needed information via the main thread
					string fileName = null;
					bool haveTime = true;
					ILoader loader = null;

					listBox.Invoke(new Action<int>((idx) =>
					{
						if (idx < listBox.Items.Count)
						{
							ModuleListItem listItem = (ModuleListItem)listBox.Items[idx];

							fileName = listItem.ListItem.FullPath;
							haveTime = listItem.HaveTime;
							loader = listItem.GetLoader();
						}
					}), index);

					// If the item already have a time, skip it
					if (haveTime || (fileName == null))
						continue;

					// Now check if the file already have a time in the database
					TimeSpan totalTime = CheckDatabase(fileName);

					// Did we get the total time
					if (totalTime.Ticks == 0)
					{
						// No, try to load the file and let the player return the total time
						totalTime = GetPlayerTime(fileName, loader);
					}

					// Update the list item
					listBox.Invoke(new Action<int>((idx) =>
					{
						if (idx < listBox.Items.Count)
						{
							ModuleListItem listItem = (ModuleListItem)listBox.Items[idx];

							// Is the item we processed still the same in the list?
							if (listItem.ListItem.FullPath == fileName)
							{
								// Yes, set the time
								mainWindowForm.SetTimeOnItem(listItem, totalTime);
							}
						}
					}), index);

					// Update the information in the database
					StoreInDatabase(fileName, totalTime);
				}
			}
			catch (Exception)
			{
				// Ignore any exception
			}
		}



		/********************************************************************/
		/// <summary>
		/// Check the database to see if a duration is stored there
		/// </summary>
		/********************************************************************/
		private TimeSpan CheckDatabase(string fileName)
		{
			ModuleDatabaseInfo databaseInfo = database.RetrieveInformation(fileName);
			if (databaseInfo == null)
				return new TimeSpan(0);

			return databaseInfo.Duration;
		}



		/********************************************************************/
		/// <summary>
		/// Update the database with the duration
		/// </summary>
		/********************************************************************/
		private void StoreInDatabase(string fileName, TimeSpan totalTime)
		{
			database.StoreInformation(fileName, new ModuleDatabaseInfo(totalTime));
		}



		/********************************************************************/
		/// <summary>
		/// Will try to load the given module and find the time
		/// </summary>
		/********************************************************************/
		private TimeSpan GetPlayerTime(string fileName, ILoader itemLoader)
		{
			try
			{
				Loader loader = new Loader(manager);
				if (loader.Load(fileName, itemLoader, out string _))
				{
					IPlayer player = loader.Player;

					if (player.InitPlayer(new PlayerConfiguration(null, loader, new MixerConfiguration()), out string _))
					{
						try
						{
							if (player is IModulePlayer modulePlayer)
								modulePlayer.SelectSong(-1);

							return player.PlayingModuleInformation.DurationInfo?.TotalTime ?? new TimeSpan(0);
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
