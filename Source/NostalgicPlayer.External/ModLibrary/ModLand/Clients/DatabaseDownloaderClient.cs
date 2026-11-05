/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polycode.NostalgicPlayer.External.ModLibrary.Interfaces;

namespace Polycode.NostalgicPlayer.External.ModLibrary.ModLand.Clients
{
	/// <summary>
	/// 
	/// </summary>
	internal class DatabaseDownloaderClient : IDatabaseDownloaderClient
	{
		/********************************************************************/
		/// <summary>
		/// Download and parse the database
		/// </summary>
		/********************************************************************/
		public async Task DownloadDatabaseAsync(string cacheDir, CancellationToken cancellationToken)
		{
			string modlandCacheDir = Path.Combine(cacheDir, "ModLand");
			string allModsZipPath = Path.Combine(modlandCacheDir, "allmods.zip");
			string allModsTxtPath = Path.Combine(modlandCacheDir, "allmods.txt");

			using (HttpClient client = new HttpClient())
			{
				// Download the file
				byte[] fileBytes = await client.GetByteArrayAsync(Constants.AllmodsZipUrl, cancellationToken);

				// Save to disk
				await File.WriteAllBytesAsync(allModsZipPath, fileBytes, cancellationToken);

				// Extract allmods.txt from the ZIP
				using (ZipArchive archive = ZipFile.OpenRead(allModsZipPath))
				{
					ZipArchiveEntry entry = archive.GetEntry("allmods.txt");
					if (entry != null)
						entry.ExtractToFile(allModsTxtPath, true);
				}
			}
		}
	}
}
