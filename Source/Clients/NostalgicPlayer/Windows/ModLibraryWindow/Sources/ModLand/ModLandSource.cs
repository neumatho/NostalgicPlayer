/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Sources.ModLand
{
	/// <summary>
	/// Represents a module service provider (e.g., ModLand, AMP Archive)
	/// </summary>
	internal class ModLandSource : IModLibrarySource
	{
		private const string ModlandModulesUrl = "https://modland.com/pub/modules/";

		private readonly List<ModEntry> offlineFiles = new();
		private readonly List<ModEntry> onlineFiles = new();

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
		public string RootPath => "modland://";



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
		/// Add online file - creates ModEntry from already sorted data
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



		/********************************************************************/
		/// <summary>
		/// Download a single module
		/// </summary>
		/********************************************************************/
		public async Task<(string ExtraPath, int ExtraSize)[]> DownloadAsync(TreeNode entry, string relativePath, string[] extraFiles, string localPath, string localDirectory, CancellationToken cancellationToken)
		{
			List<(string extraRelativePath, int Length)> extraDownloaded = new List<(string ExtraPath, int ExtraSize)>();

			using (HttpClient client = new HttpClient())
			{
				// URL-encode the path to handle special characters
				string encodedPath = string.Join("/", relativePath.Split('/').Select(Uri.EscapeDataString));
				string downloadUrl = ModlandModulesUrl + encodedPath;
				byte[] fileBytes = await client.GetByteArrayAsync(downloadUrl, cancellationToken);
				await File.WriteAllBytesAsync(localPath, fileBytes, cancellationToken);

				foreach (string extraFile in extraFiles)
				{
					string extraRelativePath = relativePath.Substring(0, relativePath.LastIndexOf('/') + 1) + extraFile;
					string extraLocalPath = Path.Combine(localDirectory, extraFile);

					string extraEncodedPath = string.Join("/", extraRelativePath.Split('/').Select(Uri.EscapeDataString));
					string extraDownloadUrl = ModlandModulesUrl + extraEncodedPath;

					try
					{
						byte[] extraBytes = await client.GetByteArrayAsync(extraDownloadUrl, cancellationToken);
						await File.WriteAllBytesAsync(extraLocalPath, extraBytes, cancellationToken);

						extraDownloaded.Add((extraRelativePath, extraBytes.Length));
					}
					catch
					{
						// Extra file might not exist - that's ok
					}
				}
			}

			return extraDownloaded.ToArray();
		}
	}
}
