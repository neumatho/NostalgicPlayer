using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polycode.NostalgicPlayer.External.ModLibrary.Interfaces;
using Polycode.NostalgicPlayer.External.ModLibrary.Models;

namespace Polycode.NostalgicPlayer.External.ModLibrary.ModLand.Clients
{
	internal class FileDownloaderClient : IFileDownloaderClient
	{
		/********************************************************************/
		/// <summary>
		/// Download a single module with extra files
		/// </summary>
		/********************************************************************/
		public async Task<DownloadedFileModel[]> DownloadFileAsync(string relativePath, string moduleFileName, string[] extraFiles, CancellationToken cancellationToken)
		{
			List<DownloadedFileModel> result = new List<DownloadedFileModel>();

			using (HttpClient client = new HttpClient())
			{
				var urlInfo = BuildUrl(relativePath, moduleFileName);
				byte[] fileBytes = await client.GetByteArrayAsync(urlInfo.Url, cancellationToken);
				result.Add(new DownloadedFileModel(urlInfo.RelativeFileName, moduleFileName, fileBytes));

				foreach (string file in extraFiles)
				{
					urlInfo = BuildUrl(relativePath, file);

					try
					{
						fileBytes = await client.GetByteArrayAsync(urlInfo.Url, cancellationToken);
						result.Add(new DownloadedFileModel(urlInfo.RelativeFileName, file, fileBytes));
					}
					catch
					{
						// Extra file might not exist - that's ok
					}
				}
			}

			return result.ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Download a single module with extra files
		/// </summary>
		/********************************************************************/
		private (string RelativeFileName, string Url) BuildUrl(string relativePath, string fileName)
		{
			string filePath = $"{relativePath}/{fileName}";

			// URL-encode the path to handle special characters
			string encodedPath = string.Join("/", filePath.Split('/').Select(Uri.EscapeDataString));
			string downloadUrl = Constants.ModlandModulesUrl + encodedPath;

			return (filePath, downloadUrl);
		}
	}
}
