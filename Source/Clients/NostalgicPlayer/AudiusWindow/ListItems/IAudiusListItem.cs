/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Windows.Forms;
using Polycode.NostalgicPlayer.External.Download;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.ListItems
{
	/// <summary>
	/// All the different controls that can be added as a list item,
	/// should implement this interface
	/// </summary>
	public interface IAudiusListItem
	{
		/// <summary>
		/// Return the control itself
		/// </summary>
		Control Control { get; }

		/// <summary>
		/// Will initialize the control
		/// </summary>
		void Initialize(AudiusListItem listItem);

		/// <summary>
		/// Will make sure that the item is refreshed with all missing data
		/// </summary>
		void RefreshItem(IPictureDownloader pictureDownloader);
	}
}
