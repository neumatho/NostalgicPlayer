/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers;
using Polycode.NostalgicPlayer.Kit;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.MultiFiles
{
	/// <summary>
	/// This class can load M3U or extended M3U lists
	/// </summary>
	public class M3UList : IMultiFileLoader
	{
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public string[] FileExtensions => [ "m3u", "m3u8" ];



		/********************************************************************/
		/// <summary>
		/// Will load a list from the given file
		/// </summary>
		/********************************************************************/
		public IEnumerable<MultiFileInfo> LoadList(string directory, Stream stream, string fileExtension)
		{
			// Make sure the file position is at the beginning of the file
			stream.Seek(0, SeekOrigin.Begin);

			Encoding encoder = fileExtension == "m3u8" ? Encoding.UTF8 : EncoderCollection.Win1252;

			using (StreamReader sr = new StreamReader(stream, encoder, leaveOpen: true))
			{
				// Check header
				string line = sr.ReadLine();

				if (line == "#EXTM3U")
					return ParseExtended(directory, sr).ToList();

				return ParseSimple(directory, sr, line).ToList();
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Parse simple M3U list
		/// </summary>
		/********************************************************************/
		private IEnumerable<MultiFileInfo> ParseSimple(string directory, StreamReader sr, string line)
		{
			do
			{
				if (!string.IsNullOrEmpty(line))
					yield return ParseFileLine(line, directory);

				line = sr.ReadLine();
			}
			while (line != null);
		}



		/********************************************************************/
		/// <summary>
		/// Parse extended M3U list
		/// </summary>
		/********************************************************************/
		private IEnumerable<MultiFileInfo> ParseExtended(string directory, StreamReader sr)
		{
			TimeSpan? playTime = null;
			string displayName = string.Empty;

			while (!sr.EndOfStream)
			{
				string line = sr.ReadLine();

				if (!string.IsNullOrEmpty(line))
				{
					if (line[0] == '#')
					{
						if (line.StartsWith("#EXTINF:"))
						{
							line = line.Substring(8).Trim();

							int index = line.IndexOf(',');
							string length = index != -1 ? line.Substring(0, index).TrimStart() : line;

							if (int.TryParse(length, out int seconds))
							{
								if (seconds > 0)
									playTime = TimeSpan.FromSeconds(seconds);

								if (index != -1)
									displayName = line.Substring(index + 1).TrimStart();
							}
						}
					}
					else
					{
						MultiFileInfo fileInfo = ParseFileLine(line, directory);

						if (playTime.HasValue || !string.IsNullOrEmpty(displayName))
						{
							fileInfo.PlayTime = playTime;

							if (!string.IsNullOrEmpty(displayName))
								fileInfo.DisplayName = displayName;

							displayName = string.Empty;
							playTime = null;
						}

						yield return fileInfo;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse a single line and convert it
		/// </summary>
		/********************************************************************/
		private MultiFileInfo ParseFileLine(string line, string directory)
		{
			MultiFileInfo.FileType fileType;

			string fileName = line.Trim();

			if (line.StartsWith("http://") || line.StartsWith("https://"))
				fileType = MultiFileInfo.FileType.Url;
			else
			{
				fileType = MultiFileInfo.FileType.Plain;

				// If the file name is relative, make it absolute
				if (!Path.IsPathRooted(fileName))
					fileName = Path.Combine(directory, fileName);
			}

			return new MultiFileInfo
			{
				Type = fileType,
				Source = fileName,
				DisplayName = fileName
			};
		}
		#endregion
	}
}
