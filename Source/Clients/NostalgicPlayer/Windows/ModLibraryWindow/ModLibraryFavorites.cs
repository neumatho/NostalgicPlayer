/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow
{
	/// <summary>
	/// Manages favorites for the ModLibrary
	/// </summary>
	public class ModLibraryFavorites : IModLibraryFavorites
	{
		private const string FavoriteFileName = "ModLibFavorites.txt";

		private readonly IModLibraryHelper modLibraryHelper;

		private readonly HashSet<string> favorites = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		private bool hasChanges;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModLibraryFavorites(IModLibraryHelper modLibraryHelper)
		{
			this.modLibraryHelper = modLibraryHelper;
		}



		/********************************************************************/
		/// <summary>
		/// Load favorites from file
		/// </summary>
		/********************************************************************/
		public void Load()
		{
			try
			{
				MoveOldFile();

				string fileName = GetFavoritesFileName();

				if (File.Exists(fileName))
				{
					foreach (string line in File.ReadAllLines(fileName))
					{
						if (!string.IsNullOrWhiteSpace(line))
							favorites.Add(line.Trim());
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
		/// Save favorites to file (only if changed)
		/// </summary>
		/********************************************************************/
		public void Save()
		{
			if (!hasChanges)
				return;

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
		/// Check if a path is a favorite
		/// </summary>
		/********************************************************************/
		public bool IsFavorite(string fullPath)
		{
			return favorites.Contains(fullPath);
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

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Previously, the favorite file was stored inside the modules
		/// directory which made it appear in the local files
		/// </summary>
		/********************************************************************/
		private void MoveOldFile()
		{
			string oldFileName = Path.Combine(modLibraryHelper.ModulesDirectory, FavoriteFileName);

			if (File.Exists(oldFileName))
				File.Move(oldFileName, GetFavoritesFileName(), true);
		}



		/********************************************************************/
		/// <summary>
		/// Get the path to the favorites file
		/// </summary>
		/********************************************************************/
		private string GetFavoritesFileName()
		{
			return Path.Combine(modLibraryHelper.CacheDirectory, FavoriteFileName);
		}
		#endregion
	}
}
