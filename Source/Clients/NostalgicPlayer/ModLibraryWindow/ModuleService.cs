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
		private List<ModEntry> onlineFiles = new List<ModEntry>();
		private List<ModEntry> offlineFiles = new List<ModEntry>();

		public string Id { get; set; }
		public string DisplayName { get; set; }
		public DateTime LastUpdate { get; set; }
		public string RootPath { get; set; }
		public bool IsLoaded { get; set; }

		/// <summary>
		/// Get readonly access to online files
		/// </summary>
		public IReadOnlyList<ModEntry> OnlineFiles => onlineFiles;

		/// <summary>
		/// Get readonly access to offline files
		/// </summary>
		public IReadOnlyList<ModEntry> OfflineFiles => offlineFiles;



		/********************************************************************/
		/// <summary>
		/// Clear online files
		/// </summary>
		/********************************************************************/
		public void ClearOnlineFiles()
		{
			onlineFiles.Clear();
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
		/// Add online file - creates ModEntry from already sorted data
		/// </summary>
		/********************************************************************/
		public void AddOnlineFile(string nameWithPath, long size)
		{
			onlineFiles.Add(new ModEntry(nameWithPath, size));
		}



		/********************************************************************/
		/// <summary>
		/// Add offline file - creates ModEntry from already sorted data
		/// </summary>
		/********************************************************************/
		public void AddOfflineFile(string nameWithPath, long size)
		{
			offlineFiles.Add(new ModEntry(nameWithPath, size));
		}
	}
}
