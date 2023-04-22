/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.MultiFiles
{
	/// <summary>
	/// This class can handle the NPML module list format
	/// </summary>
	public class NpmlList : IMultiFileLoader
	{
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public string[] FileExtensions => new [] { "npml" };



		/********************************************************************/
		/// <summary>
		/// Will save a list to the given file
		/// </summary>
		/********************************************************************/
		public static void SaveList(string fileName, IEnumerable<MultiFileInfo> list)
		{
			// Open the file
			using (StreamWriter sw = new StreamWriter(fileName, false, Encoding.UTF8))
			{
				// Start to write the header
				sw.WriteLine("@*NpML*@");

				// The version of the file
				sw.WriteLine("1");

				// Write all the items to the file
				string oldPath = string.Empty;
				string line;

				foreach (MultiFileInfo listInfo in list)
				{
					switch (listInfo.Type)
					{
						// Plain file
						case MultiFileInfo.FileType.Plain:
						{
							// Check to see if the path is the same as the previous one
							string path = Path.GetDirectoryName(listInfo.FileName);

							if (path != oldPath)
							{
								// Write path switch
								sw.WriteLine();
								sw.WriteLine("@*Path*@");
								sw.WriteLine(path);

								// Write name command
								sw.WriteLine();
								sw.WriteLine("@*Names*@");

								// Remember the new path
								oldPath = path;
							}

							// Get the file name
							line = Path.GetFileName(listInfo.FileName);
							break;
						}

						// Archive file
						case MultiFileInfo.FileType.Archive:
						{
							// Check to see if the archive is the same as the previous one
							string path = ArchivePath.GetArchiveName(listInfo.FileName);

							if (path != oldPath)
							{
								// Write archive switch
								sw.WriteLine();
								sw.WriteLine("@*Archive*@");
								sw.WriteLine(path);

								// Write name command
								sw.WriteLine();
								sw.WriteLine("@*Names*@");

								// Remember the new archive
								oldPath = path;
							}

							// Get the archive entry
							line = ArchivePath.GetEntryPath(listInfo.FileName);
							break;
						}

						default:
							throw new NotImplementedException($"File type {listInfo.Type} not implemented");
					}

					// Append time if available
					if (listInfo.PlayTime.HasValue)
						line += ":" + listInfo.PlayTime.Value.Ticks;

					// And write it
					sw.WriteLine(line);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will load a list from the given file
		/// </summary>
		/********************************************************************/
		public IEnumerable<MultiFileInfo> LoadList(string directory, Stream stream)
		{
			// Make sure the file position is at the beginning of the file
			stream.Seek(0, SeekOrigin.Begin);

			using (StreamReader sr = new StreamReader(stream, leaveOpen: true))
			{
				// Skip the header
				sr.ReadLine();

				// Get the version
				string version = sr.ReadLine();
				if (version != "1")
					throw new Exception(string.Format(Resources.IDS_ERR_UNKNOWN_LIST_VERSION, version));

				string path = null;
				bool archiveMode = false;

				while (!sr.EndOfStream)
				{
					string line = sr.ReadLine();

					if (!string.IsNullOrEmpty(line))
					{
						switch (line)
						{
							// "Path" command
							case "@*Path*@":
							{
								path = sr.ReadLine();
								archiveMode = false;
								break;
							}

							// "Archive" command
							case "@*Archive*@":
							{
								path = sr.ReadLine();
								archiveMode = true;
								break;
							}

							case "@*Names*@":
								break;

							default:
							{
								// If not a command, it's a file name or archive name
								MultiFileInfo fileInfo = new MultiFileInfo
								{
									Type = archiveMode ? MultiFileInfo.FileType.Archive : MultiFileInfo.FileType.Plain
								};

								// See if there is stored a module time
								int timePos = line.LastIndexOf(':');
								if (timePos != -1)
								{
									// Set the time
									long ticks = long.Parse(line.Substring(timePos + 1));
									if (ticks != 0)
										fileInfo.PlayTime = new TimeSpan(ticks);

									line = line.Substring(0, timePos);
								}

								// Check to see if there is loaded any path
								if (string.IsNullOrEmpty(path))
								{
									if (archiveMode)
										continue;		// Skip the entry, if no archive has been set

									// Set the file name using the load path
									fileInfo.FileName = Path.Combine(directory, line);
								}
								else
								{
									// Set the file name
									if (archiveMode)
										fileInfo.FileName = ArchivePath.CombinePathParts(path, line);
									else
										fileInfo.FileName = Path.Combine(path, line);
								}

								yield return fileInfo;
								break;
							}
						}
					}
				}
			}
		}
	}
}
