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
using System.Threading.Tasks;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Sources;
using Polycode.NostalgicPlayer.External.ModLibrary.Models;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow
{
	/// <summary>
	/// Handles downloading of module files from different sources
	/// </summary>
	internal class ModLibraryDownloadService
	{
		private readonly ModLibraryData data;
		private readonly string modulesBasePath;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModLibraryDownloadService(ModLibraryData data, string modulesBasePath)
		{
			this.data = data;
			this.modulesBasePath = modulesBasePath;
		}



		/********************************************************************/
		/// <summary>
		/// Download a single module file (synchronous, for background thread)
		/// </summary>
		/********************************************************************/
		public string DownloadModule(TreeNode entry)
		{
			return DownloadModuleAsync(entry, CancellationToken.None).GetAwaiter().GetResult();
		}



		/********************************************************************/
		/// <summary>
		/// Download a single module file
		/// </summary>
		/********************************************************************/
		private async Task<string> DownloadModuleAsync(TreeNode entry, CancellationToken cancellationToken)
		{
			IModLibrarySource source = data.GetSource(entry.SourceId);
			if (source == null)
				throw new InvalidOperationException("Source not found");

			// Get relative path without source prefix
			string moduleName = data.GetRelativePathFromSource(entry.FullPath, source);
			string relativePath = string.Empty;

			int lastSlashIndex = moduleName.LastIndexOf('/');
			if (lastSlashIndex != -1)
			{
				relativePath = moduleName.Substring(0, lastSlashIndex);
				moduleName = moduleName.Substring(lastSlashIndex + 1);
			}

			// Build local file path
			string localPath = Path.Combine(modulesBasePath, source.FolderName, relativePath.Replace('/', Path.DirectorySeparatorChar), moduleName);
			string localDirectory = Path.GetDirectoryName(localPath);

			// Check if already downloaded
			if (!File.Exists(localPath))
			{
				if (localDirectory != null)
				{
					Directory.CreateDirectory(localDirectory);

					DownloadedFileModel[] downloadedFiles = await source.GetDownloader().DownloadFileAsync(relativePath, moduleName, FindExtraFiles(entry).ToArray(), cancellationToken);

					foreach (DownloadedFileModel file in downloadedFiles)
					{
						await File.WriteAllBytesAsync(Path.Combine(localDirectory, file.FileName), file.Content, cancellationToken);
						AddFileToLocalCache(source, file.RelativeFileName, file.Content.Length);
					}
				}
			}

			return localPath;
		}



		/********************************************************************/
		/// <summary>
		/// Find extra files which needs to be downloaded
		/// </summary>
		/********************************************************************/
		private IEnumerable<string> FindExtraFiles(TreeNode entry)
		{
			// Check if this is an mdat.* file - download matching smpl.* file
			if (entry.Name.StartsWith("mdat.", StringComparison.OrdinalIgnoreCase))
				yield return "smpl" + entry.Name.Substring(4);
		}



		/********************************************************************/
		/// <summary>
		/// Add file to local cache
		/// </summary>
		/********************************************************************/
		private void AddFileToLocalCache(IModLibrarySource source, string relativePath, long size)
		{
			// Build full path including service folder name
			string fullPath = $"{source.FolderName}/{relativePath}";

			// Add to local files list if not already present
			data.AddLocalFileIfNotExists(fullPath, size);
		}
	}
}
