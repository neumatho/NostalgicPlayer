/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/

using System;
using System.Collections.Generic;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.ModLibraryWindow
{
	/// <summary>
	/// Represents a module service provider (e.g., ModLand, AMP Archive)
	/// </summary>
	internal class ModuleService
	{
		private readonly List<ModEntry> offlineFiles = new();
		private readonly List<ModEntry> onlineFiles = new();

		/********************************************************************/
		/// <summary>
		/// Display name of the service
		/// </summary>
		/********************************************************************/
		public string DisplayName
		{
			get;
			set;
		}

		/********************************************************************/
		/// <summary>
		/// Folder name for storing service files (filesystem-safe name)
		/// </summary>
		/********************************************************************/
		public required string FolderName
		{
			get;
			init;
		}

		/********************************************************************/
		/// <summary>
		/// Service identifier
		/// </summary>
		/********************************************************************/
		public string Id
		{
			get;
			set;
		}

		/********************************************************************/
		/// <summary>
		/// Whether the service database is loaded
		/// </summary>
		/********************************************************************/
		public bool IsLoaded
		{
			get;
			set;
		}

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
		/// Get readonly access to offline files
		/// </summary>
		/********************************************************************/
		public IReadOnlyList<ModEntry> OfflineFiles => offlineFiles;

		/********************************************************************/
		/// <summary>
		/// Get readonly access to online files
		/// </summary>
		/********************************************************************/
		public IReadOnlyList<ModEntry> OnlineFiles => onlineFiles;

		/********************************************************************/
		/// <summary>
		/// Root path for this service
		/// </summary>
		/********************************************************************/
		public string RootPath
		{
			get;
			set;
		}


		/********************************************************************/
		/// <summary>
		/// Add offline file - creates ModEntry from already sorted
		/// data
		/// </summary>
		/********************************************************************/
		public void AddOfflineFile(string nameWithPath, long size)
		{
			offlineFiles.Add(new ModEntry(nameWithPath, size));
		}


		/********************************************************************/
		/// <summary>
		/// Add online file - creates ModEntry from already sorted
		/// data
		/// </summary>
		/********************************************************************/
		public void AddOnlineFile(string nameWithPath, long size)
		{
			onlineFiles.Add(new ModEntry(nameWithPath, size));
		}


		/********************************************************************/
		/// <summary>
		/// Clear offline files
		/// </summary>
		/********************************************************************/
		public void ClearOfflineFiles()
		{
			offlineFiles.Clear();
		}


		/********************************************************************/
		/// <summary>
		/// Clear online files
		/// </summary>
		/********************************************************************/
		public void ClearOnlineFiles()
		{
			onlineFiles.Clear();
		}
	}
}
