/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.External.Download;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AudiusWindow.Pages
{
	/// <summary>
	/// All Audius pages must implement this interface
	/// </summary>
	public interface IAudiusPage
	{
		/// <summary>
		/// Will initialize the control
		/// </summary>
		void Initialize(IAudiusWindowApi audiusWindow, IPictureDownloader downloader, string id);

		/// <summary>
		/// Will refresh the page with new data
		/// </summary>
		void RefreshPage();

		/// <summary>
		/// Cleanup used resources
		/// </summary>
		void CleanupPage();
	}
}
