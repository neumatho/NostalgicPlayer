/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.ModLibraryWindow
{
	/// <summary>
	/// Manages favorites for the ModLibrary
	/// </summary>
	internal class ModLibraryFavorites
	{
		private readonly HashSet<string> favorites = new(StringComparer.OrdinalIgnoreCase);
		private readonly string favoritesFilePath;
		private bool hasChanges;

		/********************************************************************/
		/// <summary>
		/// Constructor - loads favorites from file
		/// </summary>
		/********************************************************************/
		public ModLibraryFavorites(string modLibraryPath)
		{
			favoritesFilePath = Path.Combine(modLibraryPath, "ModLibFavorites.txt");
			Load();
		}



		/********************************************************************/
		/// <summary>
		/// Check if a path is a favorite
		/// </summary>
		/********************************************************************/
		public bool IsFavorite(string fullPath)
		{
			return favorites.Contains(fullPath);
		}



		/********************************************************************/
		/// <summary>
		/// Add a path to favorites
		/// </summary>
		/********************************************************************/
		public void Add(string fullPath)
		{
			if (favorites.Add(fullPath))
			{
				hasChanges = true;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Remove a path from favorites
		/// </summary>
		/********************************************************************/
		public void Remove(string fullPath)
		{
			if (favorites.Remove(fullPath))
			{
				hasChanges = true;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Toggle favorite status for a path
		/// </summary>
		/********************************************************************/
		public bool Toggle(string fullPath)
		{
			if (favorites.Contains(fullPath))
			{
				favorites.Remove(fullPath);
				hasChanges = true;
				return false;
			}

			favorites.Add(fullPath);
			hasChanges = true;
			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Save favorites to file (only if changed)
		/// </summary>
		/********************************************************************/
		public void Save()
		{
			if (!hasChanges)
			{
				return;
			}

			try
			{
				string fileName = GetFavoritesFileName();
				File.WriteAllLines(fileName, favorites);
				hasChanges = false;
			}
			catch
			{
				// Ignore save errors
			}
		}



		/********************************************************************/
		/// <summary>
		/// Load favorites from file
		/// </summary>
		/********************************************************************/
		private void Load()
		{
			try
			{
				string fileName = GetFavoritesFileName();

				if (File.Exists(fileName))
				{
					foreach (string line in File.ReadAllLines(fileName))
					{
						if (!string.IsNullOrWhiteSpace(line))
						{
							favorites.Add(line.Trim());
						}
					}
				}
			}
			catch
			{
				// Ignore load errors
			}

			hasChanges = false;
		}



		/********************************************************************/
		/// <summary>
		/// Get the path to the favorites file
		/// </summary>
		/********************************************************************/
		private string GetFavoritesFileName()
		{
			return favoritesFilePath;
		}
	}
}
