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
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Modules
{
	/// <summary>
	/// This class stores extra information from the modules into a kind of database
	/// </summary>
	public class ModuleDatabase
	{
		private const int DatabaseVersion = 2;
		private const int MinutesBetweenEachSave = 7;

		private class DatabaseValue
		{
			public Dictionary<string, DatabaseValue> NextLevel { get; set; } = new Dictionary<string, DatabaseValue>(StringComparer.InvariantCultureIgnoreCase);
			public ModuleDatabaseInfo Info { get; set; }
		}

		private class QueueInfo
		{
			public string FullPath { get; set; }
			public ModuleDatabaseInfo Info { get; set; }
		}

		private class CleanupState
		{
			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public CleanupState(string path, Dictionary<string, DatabaseValue> level)
			{
				Path = path;
				Level = level;
				Index = 0;

				AllKeys = level.Keys.ToArray();
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public string Path
			{
				get;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public Dictionary<string, DatabaseValue> Level
			{
				get;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public int Index
			{
				get; set;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public string[] AllKeys
			{
				get;
			}
		}

		// The internal structure is built like this:
		//
		// The full path, including file name to a module is divided into
		// parts, where each directory name is a separate part. Each part is
		// stored in its own dictionary as the key. The value is then the
		// module information, but this will only be filled out for the file
		// name part
		private readonly Dictionary<string, DatabaseValue> root;
		private bool hasChanges;

		private ManualResetEvent shutdownEvent;
		private Thread handlerThread;

		private readonly Queue<QueueInfo> queue;
		private Semaphore queueSemaphore;

		private ManualResetEvent cleanupEvent;
		private CleanupDoneHandler cleanupDoneHandler;

		/// <summary>
		/// Return a new object if you want to change it or null to leave
		/// the original in place
		/// </summary>
		public delegate ModuleDatabaseInfo ActionHandler(string fullPath, ModuleDatabaseInfo moduleDatabaseInfo);

		/// <summary>
		/// Is called when cleanup is done
		/// </summary>
		public delegate void CleanupDoneHandler();

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleDatabase()
		{
			root = new Dictionary<string, DatabaseValue>(StringComparer.InvariantCultureIgnoreCase);
			queue = new Queue<QueueInfo>();

			LoadDatabase();

			shutdownEvent = new ManualResetEvent(false);
			queueSemaphore = new Semaphore(0, 1000);

			cleanupEvent = new ManualResetEvent(false);

			// Start the background thread
			handlerThread = new Thread(DoTaskThread);
			handlerThread.Name = "Module database handler";
			handlerThread.Priority = ThreadPriority.BelowNormal;
			handlerThread.Start();
		}



		/********************************************************************/
		/// <summary>
		/// Close down the database handler and cleanup
		/// </summary>
		/********************************************************************/
		public void CloseDown()
		{
			if (shutdownEvent != null)
			{
				shutdownEvent.Set();

				handlerThread.Join(10000);

				shutdownEvent.Dispose();
				shutdownEvent = null;
			}

			queueSemaphore?.Dispose();
			queueSemaphore = null;

			cleanupEvent?.Dispose();
			cleanupEvent = null;

			handlerThread = null;
		}



		/********************************************************************/
		/// <summary>
		/// Retrieve module information
		/// </summary>
		/********************************************************************/
		public ModuleDatabaseInfo RetrieveInformation(string fullPath)
		{
			lock (root)
			{
				Dictionary<string, DatabaseValue> workingList = root;
				DatabaseValue value = null;

				foreach (string part in fullPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
				{
					if (!workingList.TryGetValue(part, out value))
						return null;

					workingList = value.NextLevel;
				}

				// Now we have found the right value
				return value?.Info;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Retrieve all module information
		/// </summary>
		/********************************************************************/
		public IEnumerable<KeyValuePair<string, ModuleDatabaseInfo>> RetrieveAllInformation()
		{
			lock (root)
			{
				foreach (KeyValuePair<string, DatabaseValue> pair in root)
				{
					foreach (KeyValuePair<string, DatabaseValue> databasePair in RetrieveAllInformationOnLevel(pair.Key, pair.Value))
						yield return new KeyValuePair<string, ModuleDatabaseInfo>(databasePair.Key, databasePair.Value.Info);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Store module information
		/// </summary>
		/********************************************************************/
		public void StoreInformation(string fullPath, ModuleDatabaseInfo info)
		{
			lock (queue)
			{
				queue.Enqueue(new QueueInfo { FullPath = fullPath, Info = info });
				queueSemaphore.Release();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Do some action on all items in the database
		/// </summary>
		/********************************************************************/
		public void RunAction(ActionHandler handler)
		{
			lock (root)
			{
				foreach (KeyValuePair<string, DatabaseValue> pair in root)
				{
					foreach (KeyValuePair<string, DatabaseValue> databasePair in RetrieveAllInformationOnLevel(pair.Key, pair.Value))
					{
						ModuleDatabaseInfo newModuleDatabaseInfo = handler(databasePair.Key, databasePair.Value.Info);
						if (newModuleDatabaseInfo != null)
						{
							databasePair.Value.Info = newModuleDatabaseInfo;
							hasChanges = true;
						}
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Start the cleanup job
		/// </summary>
		/********************************************************************/
		public void StartCleanup(CleanupDoneHandler handler)
		{
			cleanupDoneHandler = handler;
			cleanupEvent.Set();
		}



		/********************************************************************/
		/// <summary>
		/// Delete the database from disk
		/// </summary>
		/********************************************************************/
		public void DeleteDatabase()
		{
			string fileName = GetDatabaseFileName();

			if (File.Exists(fileName))
				File.Delete(fileName);
		}



		/********************************************************************/
		/// <summary>
		/// Save the database to disk
		/// </summary>
		/********************************************************************/
		public void SaveDatabase()
		{
			lock (root)
			{
				if (hasChanges)
				{
					string fileName = GetDatabaseFileName();

					using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
					{
						using (WriterStream ws = new WriterStream(fs, true))
						{
							// Start to write the version
							ws.Write_B_UINT16(DatabaseVersion);

							WriteLevel(ws, root);
						}
					}

					hasChanges = false;
				}
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Retrieve all module information on one level
		/// </summary>
		/********************************************************************/
		private IEnumerable<KeyValuePair<string, DatabaseValue>> RetrieveAllInformationOnLevel(string path, DatabaseValue databaseValue)
		{
			if (databaseValue.Info != null)
				yield return new KeyValuePair<string, DatabaseValue>(path, databaseValue);

			if (databaseValue.NextLevel != null)
			{
				foreach (KeyValuePair<string, DatabaseValue> pair in databaseValue.NextLevel)
				{
					foreach (KeyValuePair<string, DatabaseValue> databasePair in RetrieveAllInformationOnLevel(Path.Combine(path, pair.Key), pair.Value))
						yield return databasePair;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Store the given information in the database
		/// </summary>
		/********************************************************************/
		private void DoStoreInformation(string fullPath, ModuleDatabaseInfo info)
		{
			lock (root)
			{
				Dictionary<string, DatabaseValue> workingList = root;
				DatabaseValue value = null;

				foreach (string part in fullPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
				{
					if (!workingList.TryGetValue(part, out value))
					{
						value = new DatabaseValue();
						workingList[part] = value;
					}

					workingList = value.NextLevel;
				}

				// Now we have the right list to store the module information
				if (value != null)
				{
					value.Info = info;
					value.NextLevel = null;		// No need for this, so null it to save memory
				}

				hasChanges = true;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the file name to the database
		/// </summary>
		/********************************************************************/
		private string GetDatabaseFileName()
		{
			return Path.Combine(Settings.SettingsDirectory, "ModuleInfo.db");
		}



		/********************************************************************/
		/// <summary>
		/// Load the database from disk
		/// </summary>
		/********************************************************************/
		private void LoadDatabase()
		{
			lock (root)
			{
				string fileName = GetDatabaseFileName();

				if (File.Exists(fileName))
				{
					using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
					{
						using (ReaderStream rs = new ReaderStream(fs))
						{
							// Check the version
							ushort version = rs.Read_B_UINT16();
							if (version <= DatabaseVersion)
								ReadLevel(rs, version, root);
						}
					}
				}

				hasChanges = false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Read a level and all children
		/// </summary>
		/********************************************************************/
		private void ReadLevel(ReaderStream rs, ushort version, Dictionary<string, DatabaseValue> level)
		{
			for (;;)
			{
				// Read the file part
				string part = rs.ReadString();
				if (string.IsNullOrEmpty(part))
					break;

				DatabaseValue value = new DatabaseValue();

				byte flag = rs.Read_UINT8();

				if ((flag & 1) != 0)
				{
					long duration = (long)rs.Read_B_UINT64();
					int listenCount;
					long lastLoaded;

					if (version == 1)
					{
						listenCount = 0;
						lastLoaded = 0;
					}
					else
					{
						listenCount = (int)rs.Read_B_UINT32();
						lastLoaded = (long)rs.Read_B_UINT64();
					}

					value.Info = new ModuleDatabaseInfo(new TimeSpan(duration), listenCount, new DateTime(lastLoaded));
				}

				if ((flag & 2) != 0)
					ReadLevel(rs, version, value.NextLevel);
				else
					value.NextLevel = null;

				level[part] = value;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Write a level and all children
		/// </summary>
		/********************************************************************/
		private void WriteLevel(WriterStream ws, Dictionary<string, DatabaseValue> level)
		{
			// Write the current level
			foreach (KeyValuePair<string, DatabaseValue> pair in level)
			{
				ws.WriteString(pair.Key);

				// Build the flag
				byte flag = (byte)(pair.Value.Info != null ? 1 : 0);
				if (pair.Value.NextLevel != null)
					flag |= 2;

				ws.Write_UINT8(flag);

				if (pair.Value.Info != null)
				{
					ModuleDatabaseInfo info = pair.Value.Info;

					ws.Write_B_UINT64((ulong)info.Duration.Ticks);
					ws.Write_B_UINT32((uint)info.ListenCount);
					ws.Write_B_UINT64((ulong)info.LastLoaded.Ticks);
				}

				if (pair.Value.NextLevel != null)
					WriteLevel(ws, pair.Value.NextLevel);
			}

			// Done with a level, write an empty string as marker
			ws.WriteString(string.Empty);
		}



		/********************************************************************/
		/// <summary>
		/// Background thread that handles different tasks on the database
		/// </summary>
		/********************************************************************/
		private void DoTaskThread()
		{
			try
			{
				bool stillRunning = true;
				WaitHandle[] waitArray = { queueSemaphore, shutdownEvent, cleanupEvent };
				DateTime lastSaved = DateTime.Now;

				while (stillRunning)
				{
					int waitResult = WaitHandle.WaitAny(waitArray, 1 * 60 * 1000);

					switch (waitResult)
					{
						// QueueSemaphore
						case 0:
						{
							HandleQueueElement();
							break;
						}

						// ShutdownEvent
						case 1:
						{
							// Empty queue just to be sure it's empty
							lock (queue)
							{
								while (queue.Count > 0)
									HandleQueueElement();
							}

							stillRunning = false;
							break;
						}

						// CleanupEvent
						case 2:
						{
							DoCleanup();

							cleanupEvent.Reset();

							if (cleanupDoneHandler != null)
							{
								cleanupDoneHandler();
								cleanupDoneHandler = null;
							}
							break;
						}

						// Save the database
						case WaitHandle.WaitTimeout:
						{
							// Is it time to save the database
							if (lastSaved.AddMinutes(MinutesBetweenEachSave) <= DateTime.Now)
							{
								SaveDatabase();
								lastSaved = DateTime.Now;
							}
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
		/// Handle the next queue element
		/// </summary>
		/********************************************************************/
		private void HandleQueueElement()
		{
			QueueInfo queueInfo;

			lock (queue)
			{
				queueInfo = queue.Dequeue();
			}

			DoStoreInformation(queueInfo.FullPath, queueInfo.Info);
		}



		/********************************************************************/
		/// <summary>
		/// Do the cleanup of the database
		/// </summary>
		/********************************************************************/
		private void DoCleanup()
		{
			Stack<CleanupState> stack = new Stack<CleanupState>();
			stack.Push(new CleanupState(string.Empty, root));

			bool stillRunning = true;

			while (stillRunning)
			{
				bool waitResult = shutdownEvent.WaitOne(10);
				if (waitResult)
				{
					// Shut down event triggered
					stillRunning = false;
				}
				else
				{
					if (CleanupLevel(stack))
					{
						// Done with the cleanup
						stillRunning = false;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup some items and then return
		/// </summary>
		/********************************************************************/
		private bool CleanupLevel(Stack<CleanupState> stack)
		{
			lock (root)
			{
				CleanupState state = null;

				int itemsToTake = 20;
				while (itemsToTake > 0)
				{
					if (stack.Count == 0)
						return true;

					state = stack.Pop();

					while ((state.Index < state.AllKeys.Length) && (itemsToTake > 0))
					{
						string part = state.AllKeys[state.Index];
						string fullPath = Path.Combine(state.Path, part);

						// Adjust counters immediately
						state.Index++;
						itemsToTake--;

						DatabaseValue value = state.Level[part];
						if (value.NextLevel == null)
						{
							// Reached a file, check if it exists
							if (ArchivePath.IsArchivePath(fullPath))
							{
								// Just check if the archive file exists
								fullPath = ArchivePath.GetArchiveName(fullPath);
							}

							if (!File.Exists(fullPath))
								state.Level.Remove(part);
						}
						else
						{
							// Reached a directory, check if it exists
							if (!Directory.Exists(fullPath))
								state.Level.Remove(part);
							else
							{
								// Go to next level
								stack.Push(state);

								state = new CleanupState(fullPath, value.NextLevel);
							}
						}
					}
				}

				// Remember current state
				stack.Push(state);

				hasChanges = true;
			}

			return false;
		}
		#endregion
	}
}
