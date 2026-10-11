/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Sources
{
	/// <summary>
	/// Hold the interface to a source, e.g. ModLand
	/// </summary>
	public interface IModLibrarySource
	{
		/// <summary>
		/// Service identifier
		/// </summary>
		string Id { get; }

		/// <summary>
		/// Display name of the service
		/// </summary>
		string DisplayName { get; }

		/// <summary>
		/// Folder name for storing service files (filesystem-safe name)
		/// </summary>
		string FolderName { get; }

		/// <summary>
		/// Root path for this service
		/// </summary>
		string RootPath { get; }

		/// <summary>
		/// Whether the service database is loaded
		/// </summary>
		bool IsLoaded { get; set; }

		/// <summary>
		/// Last update timestamp
		/// </summary>
		DateTime LastUpdate { get; set; }

		/// <summary>
		/// Get readonly access to files
		/// </summary>
		IReadOnlyList<ModEntry> Files { get; }

		/// <summary>
		/// Add file - creates ModEntry from already sorted data
		/// </summary>
		void AddFile(string nameWithPath, long size);

		/// <summary>
		/// Clear file collection
		/// </summary>
		void ClearFiles();

		/// <summary>
		/// Return a new instance of a downloader
		/// </summary>
		IModLibraryDownloader GetDownloader();
	}
}
