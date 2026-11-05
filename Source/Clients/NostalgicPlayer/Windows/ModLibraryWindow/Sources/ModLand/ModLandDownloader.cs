/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Threading;
using System.Threading.Tasks;
using Polycode.NostalgicPlayer.External.ModLibrary.Models;
using Polycode.NostalgicPlayer.External.ModLibrary.ModLand;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Sources.ModLand
{
	/// <summary>
	/// Downloader for ModLand
	/// </summary>
	internal class ModLandDownloader : IModLibraryDownloader
	{
		private readonly IModLandClientFactory clientFactory;
		private readonly IModLibraryHelper modLibraryHelper;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModLandDownloader(IModLandClientFactory clientFactory, IModLibraryHelper modLibraryHelper)
		{
			this.clientFactory = clientFactory;
			this.modLibraryHelper = modLibraryHelper;
		}



		/********************************************************************/
		/// <summary>
		/// Download the database
		/// </summary>
		/********************************************************************/
		public async Task DownloadDatabaseAsync(CancellationToken cancellationToken)
		{
			await clientFactory.GetDatabaseDownloaderClient().DownloadDatabaseAsync(modLibraryHelper.CacheDirectory, cancellationToken);
		}



		/********************************************************************/
		/// <summary>
		/// Download a single module
		/// </summary>
		/********************************************************************/
		public async Task<DownloadedFileModel[]> DownloadModuleAsync(string relativePath, string moduleFileName, string[] extraFiles, CancellationToken cancellationToken)
		{
			return await clientFactory.GetModuleDownloaderClient().DownloadModuleAsync(relativePath, moduleFileName, extraFiles, cancellationToken);
		}
	}
}
