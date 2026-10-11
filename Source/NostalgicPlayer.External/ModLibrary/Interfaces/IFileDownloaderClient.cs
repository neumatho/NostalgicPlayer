/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Threading;
using System.Threading.Tasks;
using Polycode.NostalgicPlayer.External.ModLibrary.Models;

namespace Polycode.NostalgicPlayer.External.ModLibrary.Interfaces
{
	/// <summary>
	/// 
	/// </summary>
	public interface IFileDownloaderClient
	{
		/// <summary>
		/// Download a single module with extra files
		/// </summary>
		Task<DownloadedFileModel[]> DownloadFileAsync(string relativePath, string moduleFileName, string[] extraFiles, CancellationToken cancellationToken);
	}
}
