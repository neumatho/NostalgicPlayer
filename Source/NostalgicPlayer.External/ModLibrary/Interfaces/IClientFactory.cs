/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.External.ModLibrary.Interfaces
{
	/// <summary>
	/// Creates the different clients
	/// </summary>
	public interface IClientFactory
	{
		/// <summary>
		/// Return a file downloader client
		/// </summary>
		IFileDownloaderClient GetFileDownloaderClient();
	}
}
