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
using System.Globalization;
using System.IO;
using System.Linq;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.SidPlayFp;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay
{
	/// <summary>
	/// Song length database reader
	/// </summary>
	internal class SidSongLength
	{
		private string baseDir;
		private Dictionary<string, List<TimeSpan>> lookupList;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SidSongLength()
		{
			// Initialize member variables
			baseDir = string.Empty;
			lookupList = null;
		}



		/********************************************************************/
		/// <summary>
		/// Tells the object where the HVSC base directory is - it figures
		/// that the database should be in /DOCUMENTS/Songlengths.md5
		/// </summary>
		/********************************************************************/
		public bool SetBaseDir(string pathToHvsc)
		{
			// Sanity check the length
			if (string.IsNullOrEmpty(pathToHvsc))
				return false;

			// Check if we need to reload any of the files
			if (pathToHvsc != baseDir)
			{
				Dictionary<string, List<TimeSpan>> tempList = new Dictionary<string, List<TimeSpan>>();

				try
				{
					string documentsPath = Path.Combine(pathToHvsc, "DOCUMENTS");

					// Attempt to load the database file
					string databaseFile = Path.Combine(documentsPath, "Songlengths.md5");
					if (File.Exists(databaseFile))
					{
						if (!ReadFile(databaseFile, tempList))
							return false;
					}

					// Now we have read all the data, so remember them in the object
					baseDir = pathToHvsc;
					lookupList = tempList;
				}
				catch(Exception)
				{
					return false;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Lookup the lengths based on the loaded file data. Returns a list
		/// with song lengths or null if not found
		/// </summary>
		/********************************************************************/
		public List<TimeSpan> GetSongLengths(SidTune sidTune)
		{
			// Check if database is loaded
			if (lookupList == null)
				return null;

			// Calculate the MD5 hash
			byte[] md5 = sidTune.CreateMD5New();
			if (md5 == null)
				return null;

			string hash = Helpers.ToHex(md5);

			if (lookupList.TryGetValue(hash, out List<TimeSpan> lengths))
				return lengths;

			// Could not be found
			return null;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Read the given file into memory
		/// </summary>
		/********************************************************************/
		private bool ReadFile(string fileName, Dictionary<string, List<TimeSpan>> entries)
		{
			try
			{
				using (StreamReader sr = new StreamReader(fileName, EncoderCollection.Win1252))
				{
					string line = sr.ReadLine();
					if (line != "[Database]")
						return false;

					while (!sr.EndOfStream)
					{
						// Read the next line
						line = sr.ReadLine();

						if (!string.IsNullOrEmpty(line))
						{
							if (line[0] == ';')
							{
								// Ignore comments
								continue;
							}

							int index = line.IndexOf('=');
							if (index == -1)
							{
								// File is corrupt, stop loading
								return false;
							}

							string md5 = line.Substring(0, index).Trim().ToUpper();
							string[] times = line.Substring(index + 1).Trim().Split(' ');
							List<TimeSpan> timeSpans = times.Select(t => TimeSpan.Parse("0:" + t, CultureInfo.InvariantCulture)).ToList();

							entries[md5] = timeSpans;
						}
					}
				}
			}
			catch(Exception)
			{
				return false;
			}

			return true;
		}
		#endregion
	}
}
