/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Sources.ModLand
{
	/// <summary>
	/// Represents a module service provider (e.g., ModLand, AMP Archive)
	/// </summary>
	internal class ModLandSource : IModLibrarySource
	{
		private const string AllModsTxt = "allmods.txt";

		private readonly IApplicationContext applicationContext;
		private readonly IModLibraryHelper modLibraryHelper;

		private readonly List<ModEntry> files = new List<ModEntry>();

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModLandSource(IApplicationContext applicationContext)
		{
			this.applicationContext = applicationContext;
			modLibraryHelper = applicationContext.Container.GetInstance<IModLibraryHelper>();
		}

		#region IModLibrarySource implementation
		/********************************************************************/
		/// <summary>
		/// Service identifier
		/// </summary>
		/********************************************************************/
		public string Id => "modland";



		/********************************************************************/
		/// <summary>
		/// Display name of the service
		/// </summary>
		/********************************************************************/
		public string DisplayName => Resources.IDS_MODLIBRARY_SOURCE_MODLAND;



		/********************************************************************/
		/// <summary>
		/// Folder name for storing service files (filesystem-safe name)
		/// </summary>
		/********************************************************************/
		public string FolderName => "ModLand";



		/********************************************************************/
		/// <summary>
		/// Root path for this service
		/// </summary>
		/********************************************************************/
		public string RootPath => Id + ":/";



		/********************************************************************/
		/// <summary>
		/// Whether the service database is loaded
		/// </summary>
		/********************************************************************/
		public bool IsLoaded
		{
			get;
			set;
		} = false;



		/********************************************************************/
		/// <summary>
		/// Last update timestamp
		/// </summary>
		/********************************************************************/
		public DateTime LastUpdate
		{
			get;
			set;
		}



		/********************************************************************/
		/// <summary>
		/// Get readonly access to files
		/// </summary>
		/********************************************************************/
		public IReadOnlyList<ModEntry> Files => files;



		/********************************************************************/
		/// <summary>
		/// Load local database into memory
		/// </summary>
		/********************************************************************/
		public void LoadDatabase()
		{
			ParseModLandDatabase();
		}



		/********************************************************************/
		/// <summary>
		/// Will delete the database
		/// </summary>
		/********************************************************************/
		public void DeleteDatabase()
		{
			// Clear data in RAM
			ClearFiles();
			IsLoaded = false;
			LastUpdate = DateTime.MinValue;

			// Delete database file
			string cacheDirectory = modLibraryHelper.CacheDirectory;

			string allModsTxtPath = Path.Combine(cacheDirectory, "ModLand", "allmods.txt");
			if (File.Exists(allModsTxtPath))
				File.Delete(allModsTxtPath);

			// Also delete the zip file if it exists
			string allModsZipPath = Path.Combine(cacheDirectory, "ModLand", "allmods.zip");
			if (File.Exists(allModsZipPath))
				File.Delete(allModsZipPath);
		}



		/********************************************************************/
		/// <summary>
		/// Add file - creates ModEntry from already sorted data
		/// </summary>
		/********************************************************************/
		public void AddFile(string nameWithPath, long size)
		{
			files.Add(new ModEntry(nameWithPath, size));
		}



		/********************************************************************/
		/// <summary>
		/// Clear file collection
		/// </summary>
		/********************************************************************/
		public void ClearFiles()
		{
			files.Clear();
			IsLoaded = false;
		}



		/********************************************************************/
		/// <summary>
		/// Return a new instance of a downloader
		/// </summary>
		/********************************************************************/
		public IModLibraryDownloader GetDownloader()
		{
			return applicationContext.Container.GetInstance<ModLandDownloader>();
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Parse ModLand allmods.txt and create ModEntry objects
		/// </summary>
		/********************************************************************/
		private void ParseModLandDatabase()
		{
			IsLoaded = false;

			string sourceCacheDir = Path.Combine(modLibraryHelper.CacheDirectory, FolderName);
			Directory.CreateDirectory(sourceCacheDir);

			string allModsTxtPath = Path.Combine(sourceCacheDir, AllModsTxt);

			if (!File.Exists(allModsTxtPath))
				return;

			FileInfo fileInfo = new(allModsTxtPath);
			LastUpdate = fileInfo.LastWriteTime;

			ClearFiles();

			// Step 1: Parse all lines into (nameWithPath, size) tuples
			List<(string nameWithPath, long size)> entries = new List<(string nameWithPath, long size)>();

			foreach (string line in File.ReadLines(allModsTxtPath))
			{
				// Format: "12345    path/to/file.ext"
				string[] parts = line.Split([ ' ', '\t' ], StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length < 2)
					continue;

				if (!long.TryParse(parts[0], out long fileSize))
					continue;

				string nameWithPath = string.Join(" ", parts.Skip(1));

				// Skip files that should not be included (e.g., smpl.*)
				if (!modLibraryHelper.ShouldIncludeFile(nameWithPath))
					continue;

				entries.Add((nameWithPath, fileSize));
			}

			// Step 2: Sort strings (fast!)
			entries.Sort((a, b) => string.Compare(a.nameWithPath, b.nameWithPath, StringComparison.OrdinalIgnoreCase));

			// Step 3: Create ModEntry objects from sorted data
			foreach (var entry in entries)
				AddFile(entry.nameWithPath, entry.size);

			IsLoaded = true;
		}
		#endregion
	}
}
