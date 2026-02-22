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

namespace Polycode.NostalgicPlayer.Logic.Playlists
{
	/// <summary>
	/// This class can handle the NPML module list format
	/// </summary>
	internal class NpmlList : IPlaylist
	{
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this list
		/// </summary>
		/********************************************************************/
		public string[] FileExtensions => [ "npml" ];



		/********************************************************************/
		/// <summary>
		/// Will load a list from the given file
		/// </summary>
		/********************************************************************/
		public IEnumerable<PlaylistFileInfo> LoadList(string directory, Stream stream, string fileExtension)
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
				PlaylistFileInfo.FileType mode = PlaylistFileInfo.FileType.Plain;

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
								mode = PlaylistFileInfo.FileType.Plain;
								break;
							}

							// "Archive" command
							case "@*Archive*@":
							{
								path = sr.ReadLine();
								mode = PlaylistFileInfo.FileType.Archive;
								break;
							}

							// "URL" command
							case "@*URL*@":
							{
								mode = PlaylistFileInfo.FileType.Url;
								break;
							}

							// "Audius" command
							case "@*Audius*@":
							{
								mode = PlaylistFileInfo.FileType.Audius;
								break;
							}

							case "@*Names*@":
								break;

							default:
							{
								// If not a command, it's the source, e.g. a file name or URL
								PlaylistFileInfo fileInfo = null;

								switch (mode)
								{
									case PlaylistFileInfo.FileType.Plain:
									case PlaylistFileInfo.FileType.Archive:
									{
										fileInfo = ParseFileLine(line, path, directory, mode);
										break;
									}

									case PlaylistFileInfo.FileType.Url:
									{
										fileInfo = ParseStreamLine(line);
										break;
									}

									case PlaylistFileInfo.FileType.Audius:
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



		/********************************************************************/
		/// <summary>
		/// Will save a list to the given file
		/// </summary>
		/********************************************************************/
		public void SaveList(string fileName, IEnumerable<PlaylistFileInfo> list)
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
				PlaylistFileInfo.FileType oldType = (PlaylistFileInfo.FileType)(-1);
				string line;

				foreach (PlaylistFileInfo listInfo in list)
				{
					switch (listInfo.Type)
					{
						// Plain file
						case PlaylistFileInfo.FileType.Plain:
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
						case PlaylistFileInfo.FileType.Archive:
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
						case PlaylistFileInfo.FileType.Url:
						{
							if (oldType != PlaylistFileInfo.FileType.Url)
							{
								sw.WriteLine();
								sw.WriteLine("@*URL*@");
							}

							line = $"{listInfo.Source}|{listInfo.DisplayName}";

							oldPath = string.Empty;
							break;
						}

						// Audius track
						case PlaylistFileInfo.FileType.Audius:
						{
							if (oldType != PlaylistFileInfo.FileType.Audius)
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

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Parse file line
		/// </summary>
		/********************************************************************/
		private PlaylistFileInfo ParseFileLine(string line, string path, string directory, PlaylistFileInfo.FileType mode)
		{
			PlaylistFileInfo fileInfo = new PlaylistFileInfo
			{
				Type = mode
			};

			line = ApplyExtraInformation(line, fileInfo);

			// Check to see if there is loaded any path
			if (string.IsNullOrEmpty(path))
			{
				if (mode == PlaylistFileInfo.FileType.Archive)
					return null;		// Skip the entry, if no archive has been set

				// Set the file name using the load path
				fileInfo.Source = Path.Combine(directory, line);
			}
			else
			{
				// Set the file name
				if (mode == PlaylistFileInfo.FileType.Archive)
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
		private PlaylistFileInfo ParseStreamLine(string line)
		{
			PlaylistFileInfo fileInfo = new PlaylistFileInfo
			{
				Type = PlaylistFileInfo.FileType.Url
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
		private PlaylistFileInfo ParseAudiusLine(string line)
		{
			PlaylistFileInfo fileInfo = new PlaylistFileInfo()
			{
				Type = PlaylistFileInfo.FileType.Audius
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
		private string ApplyExtraInformation(string line, PlaylistFileInfo fileInfo)
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
