/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.External.Download
{
	/// <summary>
	/// Create new instances of the picture downloader
	/// </summary>
	public interface IPictureDownloaderFactory
	{
		/// <summary>
		/// Create a new instance
		/// </summary>
		IPictureDownloader Create();
	}
}
