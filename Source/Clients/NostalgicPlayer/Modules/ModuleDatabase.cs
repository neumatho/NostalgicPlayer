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
		private const int DatabaseVersion = 1;

		private class DatabaseValue
		{
			public Dictionary<string, DatabaseValue> NextLevel = new Dictionary<string, DatabaseValue>();
			public ModuleDatabaseInfo Info;
		}

		private class QueueInfo
		{
			public string FullPath;
			public ModuleDatabaseInfo Info;
		}

		private class CleanupState
		{
			public CleanupState(string path, Dictionary<string, DatabaseValue> level)
			{
				Path = path;
				Level = level;
				Index = 0;

				AllKeys = level.Keys.ToArray();
			}

			public readonly string Path;
			public readonly Dictionary<string, DatabaseValue> Level;
			public int Index;
			public readonly string[] AllKeys;
		}

		// The internal structure is built like this:
		//
		// The full path, including file name to a module is divided into
		// parts, where each directory name is a separate part. Each part is
		// stored in its own dictionary as the key. The value is then the
		// module information, but this will only be filled out for the file
		// name part
		private readonly Dictionary<string, DatabaseValue> root;

		// Used for the cleanup job
		private readonly object threadLock = new object();
		private ManualResetEvent shutdownEvent;
		private Thread cleanupThread;

		private Queue<QueueInfo> queue;
		private bool queueBeingParsed = false;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleDatabase()
		{
			root = new Dictionary<string, DatabaseValue>();

			LoadDatabase();
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
		/// Store module information
		/// </summary>
		/********************************************************************/
		public void StoreInformation(string fullPath, ModuleDatabaseInfo info)
		{
			lock (threadLock)
			{
				if (!queueBeingParsed)
				{
					queue.Enqueue(new QueueInfo { FullPath = fullPath, Info = info });
					return;
				}
			}

			lock (root)
			{
				DoStoreInformation(fullPath, info);
			}
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
			Thread thread;

			// Stop the cleanup thread if running
			lock (threadLock)
			{
				thread = cleanupThread;

				if (cleanupThread != null)
					shutdownEvent.Set();
			}

			// Wait for thread to be done
			thread?.Join();

			lock (root)
			{
				string fileName = GetDatabaseFileName();

				using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
				{
					using (WriterStream ws = new WriterStream(fs))
					{
						// Start to write the version
						ws.Write_B_UINT16(DatabaseVersion);

						WriteLevel(ws, root);
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Start cleanup job, which will check all files to see if they
		/// still exists
		/// </summary>
		/********************************************************************/
		public void StartCleanup()
		{
			lock (threadLock)
			{
				// Create event used to tell the thread to stop
				shutdownEvent = new ManualResetEvent(false);

				// Create queue
				queue = new Queue<QueueInfo>();

				// Start the background thread
				cleanupThread = new Thread(DoCleanupThread);
				cleanupThread.Priority = ThreadPriority.BelowNormal;
				cleanupThread.Start();
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Store the given information in the database
		/// </summary>
		/********************************************************************/
		private void DoStoreInformation(string fullPath, ModuleDatabaseInfo info)
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
							if (version == DatabaseVersion)
								ReadLevel(rs, root);
						}
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Read a level and all children
		/// </summary>
		/********************************************************************/
		private void ReadLevel(ReaderStream rs, Dictionary<string, DatabaseValue> level)
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
					value.Info = new ModuleDatabaseInfo(new TimeSpan((long)rs.Read_B_UINT64()));

				if ((flag & 2) != 0)
					ReadLevel(rs, value.NextLevel);
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
				}

				if (pair.Value.NextLevel != null)
					WriteLevel(ws, pair.Value.NextLevel);
			}

			// Done with a level, write an empty string as marker
			ws.WriteString(string.Empty);
		}



		/********************************************************************/
		/// <summary>
		/// Background thread that cleanup the database
		/// </summary>
		/********************************************************************/
		private void DoCleanupThread()
		{
			try
			{
				bool stillRunning = true;

				Stack<CleanupState> stack = new Stack<CleanupState>();
				stack.Push(new CleanupState(string.Empty, root));

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
							// Done with the cleanup, now parse the queue
							//
							// Set a flag indicating, that the queue is being
							// parsed, so the "store" method won't put more
							// stuff in the queue
							lock (root)
							{
								lock (threadLock)
								{
									queueBeingParsed = true;
								}

								while (queue.Count > 0)
								{
									QueueInfo queueInfo = queue.Dequeue();
									DoStoreInformation(queueInfo.FullPath, queueInfo.Info);
								}
							}

							stillRunning = false;
						}
					}
				}
			}
			catch (Exception)
			{
				// If an exception is thrown, abort the thread
			}
			finally
			{
				lock (threadLock)
				{
					shutdownEvent.Dispose();
					shutdownEvent = null;

					cleanupThread = null;
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

					while ((state.Index < state.Level.Count) && (itemsToTake > 0))
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
			}

			return false;
		}
		#endregion
	}
}
