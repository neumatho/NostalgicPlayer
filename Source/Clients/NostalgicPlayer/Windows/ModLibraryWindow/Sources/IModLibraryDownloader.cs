/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Threading;
using System.Threading.Tasks;
using Polycode.NostalgicPlayer.External.ModLibrary.Models;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Sources
{
	/// <summary>
	/// Hold the interface to a downloader
	/// </summary>
	public interface IModLibraryDownloader
	{
		/// <summary>
		/// Download a single module
		/// </summary>
		Task<DownloadedFileModel[]> DownloadFileAsync(string relativePath, string moduleFileName, string[] extraFiles, CancellationToken cancellationToken);
	}
}
