/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.ModLibraryWindow
{
	/// <summary>
	/// Handles downloading of module files from ModLand
	/// </summary>
	internal class ModLibraryDownloadService
	{
		private const string ModlandModulesUrl = "https://modland.com/pub/modules/";

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
		public async Task<string> DownloadModuleAsync(TreeNode entry, CancellationToken cancellationToken)
		{
			var service = data.GetService(entry.ServiceId);
			if (service == null)
			{
				throw new InvalidOperationException("Service not found");
			}

			// Get relative path without service prefix
			string relativePath = data.GetRelativePathFromService(entry.FullPath, service);

			// Build local file path
			string localPath = Path.Combine(modulesBasePath, service.FolderName,
				relativePath.Replace('/', Path.DirectorySeparatorChar));
			string localDirectory = Path.GetDirectoryName(localPath);

			// Check if already downloaded
			if (!File.Exists(localPath))
			{
				// Create directory if needed
				if (localDirectory != null)
				{
					Directory.CreateDirectory(localDirectory);

					// Download from service (currently only ModLand supported)
					if (service.Id == "modland")
					{
						using HttpClient client = new();

						// URL-encode the path to handle special characters
						string encodedPath = string.Join("/", relativePath.Split('/').Select(Uri.EscapeDataString));
						string downloadUrl = ModlandModulesUrl + encodedPath;
						byte[] fileBytes = await client.GetByteArrayAsync(downloadUrl, cancellationToken);
						await File.WriteAllBytesAsync(localPath, fileBytes, cancellationToken);

						// Check if this is an mdat.* file - download matching smpl.* file
						if (entry.Name.StartsWith("mdat.", StringComparison.OrdinalIgnoreCase))
						{
							await DownloadSampleFileAsync(client, entry, service, relativePath, localDirectory,
								cancellationToken);
						}
					}
				}

				// Add downloaded file to LocalFilesCache
				AddFileToLocalCache(service, relativePath, entry.Size);
			}

			return localPath;
		}


		/********************************************************************/
		/// <summary>
		/// Download matching sample file for mdat.* files
		/// </summary>
		/********************************************************************/
		private async Task DownloadSampleFileAsync(HttpClient client, TreeNode entry, ModuleService service,
			string relativePath, string localDirectory, CancellationToken cancellationToken)
		{
			string smplFileName = "smpl" + entry.Name.Substring(4);
			string smplRelativePath = relativePath.Substring(0, relativePath.LastIndexOf('/') + 1) + smplFileName;
			string smplLocalPath = Path.Combine(localDirectory, smplFileName);

			string smplEncodedPath = string.Join("/", smplRelativePath.Split('/').Select(Uri.EscapeDataString));
			string smplDownloadUrl = ModlandModulesUrl + smplEncodedPath;

			try
			{
				byte[] smplBytes = await client.GetByteArrayAsync(smplDownloadUrl, cancellationToken);
				await File.WriteAllBytesAsync(smplLocalPath, smplBytes, cancellationToken);

				// Add smpl file to LocalFilesCache
				AddFileToLocalCache(service, smplRelativePath, smplBytes.Length);
			}
			catch
			{
				// Sample file might not exist - that's ok
			}
		}


		/********************************************************************/
		/// <summary>
		/// Add file to local cache
		/// </summary>
		/********************************************************************/
		private void AddFileToLocalCache(ModuleService service, string relativePath, long size)
		{
			// Build full path including service folder name
			string fullPath = $"{service.FolderName}/{relativePath}";

			// Add to local files list if not already present
			data.AddLocalFileIfNotExists(fullPath, size);
		}
	}
}
