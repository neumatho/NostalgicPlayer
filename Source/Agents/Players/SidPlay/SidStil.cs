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
using System.Text;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay
{
	/// <summary>
	/// STIL and Bug List reader
	/// </summary>
	internal class SidStil
	{
		private string baseDir;
		private Dictionary<string, List<string>> stilLookup;
		private Dictionary<string, List<string>> bugListLookup;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SidStil()
		{
			// Initialize member variables
			baseDir = string.Empty;
			stilLookup = null;
			bugListLookup = null;
		}



		/********************************************************************/
		/// <summary>
		/// Tells the object where the HVSC base directory is - it figures
		/// that the STIL should be in /DOCUMENTS/STIL.txt and that the
		/// BUGlist should be in /DOCUMENTS/BUGlist.txt
		/// </summary>
		/********************************************************************/
		public bool SetBaseDir(string pathToHvsc, bool loadStil, bool loadBugList)
		{
			// Sanity check the length
			if (string.IsNullOrEmpty(pathToHvsc))
				return false;

			// Check if we need to reload any of the files
			if ((pathToHvsc != baseDir) || (loadStil && (stilLookup == null)) || (!loadStil && (stilLookup != null)) || (loadBugList && (bugListLookup == null)) || (!loadBugList && (bugListLookup != null)))
			{
				Dictionary<string, List<string>> tempStil = loadStil ? new Dictionary<string, List<string>>() : null;
				Dictionary<string, List<string>> tempBugList = loadBugList ? new Dictionary<string, List<string>>() : null;

				try
				{
					string documentsPath = Path.Combine(pathToHvsc, "DOCUMENTS");

					if (tempStil != null)
					{
						// Attempt to load the STIL file
						string stilPath = Path.Combine(documentsPath, "STIL.txt");
						if (File.Exists(stilPath))
						{
							if (!ReadFile(stilPath, tempStil, true))
								return false;
						}
					}

					if (tempBugList != null)
					{
						// Attempt to load the STIL file
						string bugListPath = Path.Combine(documentsPath, "BUGlist.txt");
						if (File.Exists(bugListPath))
							ReadFile(bugListPath, tempBugList, false);
					}

					// Now we have read all the data, so remember them in the object
					baseDir = pathToHvsc;
					stilLookup = tempStil;
					bugListLookup = tempBugList;
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
		/// Return the global comment if any
		/// </summary>
		/********************************************************************/
		public IEnumerable<string> GetGlobalComment(string pathToFile)
		{
			if (string.IsNullOrEmpty(baseDir) || (stilLookup == null))
				return null;

			// Determine if the base dir is in the given path
			if (!pathToFile.StartsWith(baseDir))
				return null;

			// Extract the directory relative to the base dir
			string directory = Path.GetDirectoryName(pathToFile).Substring(baseDir.Length);
			directory = directory.Replace('\\', '/');
			directory += '/';

			// Lookup the entry
			if (stilLookup.TryGetValue(directory, out List<string> entries))
				return entries;

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Return the file comment if any
		/// </summary>
		/********************************************************************/
		public IEnumerable<string> GetFileComment(string pathToFile)
		{
			if (string.IsNullOrEmpty(baseDir) || (stilLookup == null))
				return null;

			// Determine if the base dir is in the given path
			if (!pathToFile.StartsWith(baseDir))
				return null;

			// Extract the directory relative to the base dir
			string file = pathToFile.Substring(baseDir.Length);
			file = file.Replace('\\', '/');

			// Lookup the entry
			if (stilLookup.TryGetValue(file, out List<string> entries))
				return entries;

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Return the bug comment if any
		/// </summary>
		/********************************************************************/
		public IEnumerable<string> GetBugComment(string pathToFile)
		{
			if (string.IsNullOrEmpty(baseDir) || (bugListLookup == null))
				return null;

			// Determine if the base dir is in the given path
			if (!pathToFile.StartsWith(baseDir))
				return null;

			// Extract the directory relative to the base dir
			string file = pathToFile.Substring(baseDir.Length);
			file = file.Replace('\\', '/');

			// Lookup the entry
			if (bugListLookup.TryGetValue(file, out List<string> entries))
				return entries;

			return null;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Read the given file into memory
		/// </summary>
		/********************************************************************/
		private bool ReadFile(string fileName, Dictionary<string, List<string>> entries, bool isStilFile)
		{
			try
			{
				using (StreamReader sr = new StreamReader(fileName, Encoding.GetEncoding(1252)))
				{
					List<string> directoryEntries = null;

					while (!sr.EndOfStream)
					{
						// Read the next line
						string line = sr.ReadLine();

						if (!string.IsNullOrEmpty(line))
						{
							// Ignore comments
							if (line[0] == '#')
								continue;

							// Is this a dir separator?
							if (line[0] == '/')
							{
								directoryEntries = new List<string>();
								entries[line] = directoryEntries;
							}
							else if (directoryEntries != null)
							{
								directoryEntries.Add(line);
							}
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
