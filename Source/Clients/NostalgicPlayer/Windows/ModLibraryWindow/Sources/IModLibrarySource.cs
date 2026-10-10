/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Sources
{
	/// <summary>
	/// Hold the interface to a source, e.g. ModLand
	/// </summary>
	internal interface IModLibrarySource
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
		/// Get readonly access to online files
		/// </summary>
		IReadOnlyList<ModEntry> OnlineFiles { get; }

		/// <summary>
		/// Add online file - creates ModEntry from already sorted data
		/// </summary>
		void AddOnlineFile(string nameWithPath, long size);

		/// <summary>
		/// Clear online files
		/// </summary>
		void ClearOnlineFiles();

		/// <summary>
		/// Download a single module
		/// </summary>
		Task<(string ExtraPath, int ExtraSize)[]> DownloadAsync(TreeNode entry, string relativePath, string[] extraFiles, string localPath, string localDirectory, CancellationToken cancellationToken);
	}
}
