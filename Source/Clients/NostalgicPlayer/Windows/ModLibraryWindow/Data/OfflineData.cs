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
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Data
{
	/// <summary>
	/// Manages locally module library data
	/// </summary>
	public class OfflineData : DataBase
	{
		private readonly IModLibraryFavorites favorites;
		private readonly IModLibraryHelper modLibraryHelper;

		private readonly Lock listLock = new Lock();
		private readonly List<ModEntry> files = new List<ModEntry>();

		private static readonly Comparer<ModEntry> entryComparer = Comparer<ModEntry>.Create((a, b) => string.Compare(a.FullName, b.FullName, StringComparison.OrdinalIgnoreCase));

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public OfflineData(IApplicationContext applicationContext)
		{
			modLibraryHelper = applicationContext.Container.GetInstance<IModLibraryHelper>();
			favorites = applicationContext.Container.GetInstance<IModLibraryFavorites>();
		}



		/********************************************************************/
		/// <summary>
		/// Load local database into memory
		/// </summary>
		/********************************************************************/
		public void LoadDatabase()
		{
			string modulesDirectory = modLibraryHelper.ModulesDirectory;

			if (!Directory.Exists(modulesDirectory))
				return;

			// Recursively scan all files in the unified Modules folder
			string[] allFiles = Directory.GetFiles(modulesDirectory, "*", SearchOption.AllDirectories);

			// Step 1: Build list of (nameWithPath, size) tuples
			List<(string nameWithPath, long size)> entries = new List<(string nameWithPath, long size)>();

			foreach (string filePath in allFiles)
			{
				// Get relative path from Modules directory
				string fullRelativePath = filePath.Substring(modulesDirectory.Length + 1).Replace(Path.DirectorySeparatorChar, '/');

				// Skip files that should not be included (e.g., smpl.*)
				if (!modLibraryHelper.ShouldIncludeFile(fullRelativePath))
					continue;

				// Get file size
				FileInfo fileInfo = new FileInfo(filePath);
				long fileSize = fileInfo.Length;

				entries.Add((fullRelativePath, fileSize));
			}

			// Step 2: Sort by path
			entries.Sort((a, b) => string.Compare(a.nameWithPath, b.nameWithPath, StringComparison.OrdinalIgnoreCase));

			// Step 3: Add all files to local files list
			List<ModEntry> list = new List<ModEntry>();

			foreach (var entry in entries)
				list.Add(new ModEntry(entry.nameWithPath, entry.size));

			lock (listLock)
			{
				files.Clear();
				files.AddRange(list);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Load favorites
		/// </summary>
		/********************************************************************/
		public void LoadFavorites()
		{
			favorites.Load();
		}



		/********************************************************************/
		/// <summary>
		/// Save favorites back to disk
		/// </summary>
		/********************************************************************/
		public void SaveFavorites()
		{
			favorites.Save();
		}



		/********************************************************************/
		/// <summary>
		/// Validate the given breadcrumb path and return a new valid path
		/// where each part is the display name
		/// </summary>
		/********************************************************************/
		public string ValidatePath(string path)
		{
			string[] pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
			if (pathParts.Length == 0)
				return string.Empty;

			string modulesDirectory = modLibraryHelper.ModulesDirectory;

			// Find the longest number of leading path parts that matches
			// a directory in one of the files
			for (int i = pathParts.Length; i > 0; i--)
			{
				string pathToTest = string.Join(Path.DirectorySeparatorChar, pathParts.Take(i));

				if (Directory.Exists(Path.Combine(modulesDirectory, pathToTest)))
					return string.Join('/', pathParts.Take(i));
			}

			return string.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// Will add a new file to the internal list
		/// </summary>
		/********************************************************************/
		public void AddFile(string modulePath)
		{
			FileInfo fileInfo = new FileInfo(modulePath);
			long size = fileInfo.Length;

			string modulesDirectory = modLibraryHelper.ModulesDirectory;

			// Get relative path from Modules directory
			string fullRelativePath = modulePath.Substring(modulesDirectory.Length + 1).Replace(Path.DirectorySeparatorChar, '/');
			ModEntry newEntry = new ModEntry(fullRelativePath, size);

			lock (listLock)
			{
				int index = files.BinarySearch(newEntry, entryComparer);
				if (index < 0)
					files.Insert(~index, newEntry);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Create new tree builder
		/// </summary>
		/********************************************************************/
		public bool IsFavorite(string fullPath)
		{
			return favorites.IsFavorite(fullPath);
		}



		/********************************************************************/
		/// <summary>
		/// Toggle favorite status for a path
		/// </summary>
		/********************************************************************/
		public bool ToggleFavorite(string fullPath)
		{
			return favorites.Toggle(fullPath);
		}

		#region DataBase overrides
		/********************************************************************/
		/// <summary>
		/// Create new tree builder
		/// </summary>
		/********************************************************************/
		protected override FilteredTreeBuilder CreateTreeBuilder(string searchText, SearchMode searchMode)
		{
			List<ModEntry> snapshot;

			lock (listLock)
			{
				snapshot = new List<ModEntry>(files);
			}

			return new OfflineFilteredTreeBuilder(snapshot, favorites, searchText, searchMode);
		}
		#endregion
	}
}
