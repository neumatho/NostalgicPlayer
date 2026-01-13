/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Kit.Helpers;

namespace Polycode.NostalgicPlayer.Logic.MultiFiles
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
		public string[] FileExtensions => [ "npml" ];



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
				MultiFileInfo.FileType oldType = (MultiFileInfo.FileType)(-1);
				string line;

				foreach (MultiFileInfo listInfo in list)
				{
					switch (listInfo.Type)
					{
						// Plain file
						case MultiFileInfo.FileType.Plain:
						{
							// Check to see if the path is the same as the previous one
							string path = Path.GetDirectoryName(listInfo.Source);

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
							line = Path.GetFileName(listInfo.Source);
							break;
						}

						// Archive file
						case MultiFileInfo.FileType.Archive:
						{
							// Check to see if the archive is the same as the previous one
							string path = ArchivePath.GetArchiveName(listInfo.Source);

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
							line = ArchivePath.GetEntryPath(listInfo.Source);
							break;
						}

						// URL
						case MultiFileInfo.FileType.Url:
						{
							if (oldType != MultiFileInfo.FileType.Url)
							{
								sw.WriteLine();
								sw.WriteLine("@*URL*@");
							}

							line = $"{listInfo.Source}|{listInfo.DisplayName}";

							oldPath = string.Empty;
							break;
						}

						// Audius track
						case MultiFileInfo.FileType.Audius:
						{
							if (oldType != MultiFileInfo.FileType.Audius)
							{
								sw.WriteLine();
								sw.WriteLine("@*Audius*@");
							}

							line = $"{listInfo.Source}|{listInfo.DisplayName}";

							oldPath = string.Empty;
							break;
						}

						default:
							throw new NotImplementedException($"File type {listInfo.Type} not implemented");
					}

					oldType = listInfo.Type;

					// Append time if available
					if (listInfo.PlayTime.HasValue)
						line += ":" + listInfo.PlayTime.Value.Ticks;

					if (listInfo.DefaultSubSong.HasValue)
						line += "?" + listInfo.DefaultSubSong.Value;

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
		public IEnumerable<MultiFileInfo> LoadList(string directory, Stream stream, string fileExtension)
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
					throw new InvalidDataException(string.Format(Resources.IDS_ERR_UNKNOWN_LIST_VERSION, version));

				string path = null;
				MultiFileInfo.FileType mode = MultiFileInfo.FileType.Plain;

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
								mode = MultiFileInfo.FileType.Plain;
								break;
							}

							// "Archive" command
							case "@*Archive*@":
							{
								path = sr.ReadLine();
								mode = MultiFileInfo.FileType.Archive;
								break;
							}

							// "URL" command
							case "@*URL*@":
							{
								mode = MultiFileInfo.FileType.Url;
								break;
							}

							// "Audius" command
							case "@*Audius*@":
							{
								mode = MultiFileInfo.FileType.Audius;
								break;
							}

							case "@*Names*@":
								break;

							default:
							{
								// If not a command, it's the source, e.g. a file name or URL
								MultiFileInfo fileInfo = null;

								switch (mode)
								{
									case MultiFileInfo.FileType.Plain:
									case MultiFileInfo.FileType.Archive:
									{
										fileInfo = ParseFileLine(line, path, directory, mode);
										break;
									}

									case MultiFileInfo.FileType.Url:
									{
										fileInfo = ParseStreamLine(line);
										break;
									}

									case MultiFileInfo.FileType.Audius:
									{
										fileInfo = ParseAudiusLine(line);
										break;
									}
								}

								if (fileInfo != null)
									yield return fileInfo;

								break;
							}
						}
					}
				}
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Parse file line
		/// </summary>
		/********************************************************************/
		private MultiFileInfo ParseFileLine(string line, string path, string directory, MultiFileInfo.FileType mode)
		{
			MultiFileInfo fileInfo = new MultiFileInfo
			{
				Type = mode
			};

			line = ApplyExtraInformation(line, fileInfo);

			// Check to see if there is loaded any path
			if (string.IsNullOrEmpty(path))
			{
				if (mode == MultiFileInfo.FileType.Archive)
					return null;		// Skip the entry, if no archive has been set

				// Set the file name using the load path
				fileInfo.Source = Path.Combine(directory, line);
			}
			else
			{
				// Set the file name
				if (mode == MultiFileInfo.FileType.Archive)
					fileInfo.Source = ArchivePath.CombinePathParts(path, line);
				else
					fileInfo.Source = Path.Combine(path, line);
			}

			return fileInfo;
		}



		/********************************************************************/
		/// <summary>
		/// Parse stream line
		/// </summary>
		/********************************************************************/
		private MultiFileInfo ParseStreamLine(string line)
		{
			MultiFileInfo fileInfo = new MultiFileInfo
			{
				Type = MultiFileInfo.FileType.Url
			};

			int searchIndex = line.IndexOf('|');
			if (searchIndex == -1)
			{
				// Invalid entry
				return null;
			}

			fileInfo.Source = line.Substring(0, searchIndex);
			line = line.Substring(searchIndex + 1);

			fileInfo.DisplayName = ApplyExtraInformation(line, fileInfo);

			return fileInfo;
		}



		/********************************************************************/
		/// <summary>
		/// Parse Audius line
		/// </summary>
		/********************************************************************/
		private MultiFileInfo ParseAudiusLine(string line)
		{
			MultiFileInfo fileInfo = new MultiFileInfo()
			{
				Type = MultiFileInfo.FileType.Audius
			};

			int searchIndex = line.IndexOf('|');
			if (searchIndex == -1)
			{
				// Invalid entry
				return null;
			}

			fileInfo.Source = line.Substring(0, searchIndex);
			line = line.Substring(searchIndex + 1);

			fileInfo.DisplayName = ApplyExtraInformation(line, fileInfo);

			return fileInfo;
		}



		/********************************************************************/
		/// <summary>
		/// Check for extra information in the line
		/// </summary>
		/********************************************************************/
		private string ApplyExtraInformation(string line, MultiFileInfo fileInfo)
		{
			// See if there is stored a default sub-song
			int searchIndex = line.LastIndexOf('?');
			if (searchIndex != -1)
			{
				// Set the default sub-song
				// It seems that it is possible to have ? in filenames on the Amiga,
				// so we check to see if we could parse the rest
				if (int.TryParse(line.Substring(searchIndex + 1), out int defaultSong) && (defaultSong >= 0))
				{
					fileInfo.DefaultSubSong = defaultSong;
					line = line.Substring(0, searchIndex);
				}
			}

			// See if there is stored a module time
			searchIndex = line.LastIndexOf(':');
			if (searchIndex != -1)
			{
				// Set the time
				long ticks = long.Parse(line.Substring(searchIndex + 1));
				if (ticks != 0)
					fileInfo.PlayTime = new TimeSpan(ticks);

				line = line.Substring(0, searchIndex);
			}

			return line;
		}
		#endregion
	}
}
