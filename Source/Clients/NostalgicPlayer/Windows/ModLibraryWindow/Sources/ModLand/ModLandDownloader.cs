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

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModLandDownloader(IModLandClientFactory clientFactory)
		{
			this.clientFactory = clientFactory;
		}



		/********************************************************************/
		/// <summary>
		/// Download a single module
		/// </summary>
		/********************************************************************/
		public async Task<DownloadedFileModel[]> DownloadFileAsync(string relativePath, string moduleFileName, string[] extraFiles, CancellationToken cancellationToken)
		{
			return await clientFactory.GetFileDownloaderClient().DownloadFileAsync(relativePath, moduleFileName, extraFiles, cancellationToken);
		}
	}
}
