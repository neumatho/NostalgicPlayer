/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AudiusWindow.Events;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AudiusWindow.ListItems
{
	/// <summary>
	/// This implementation is for specific profile list items
	/// </summary>
	public interface IAudiusProfileListItem : IAudiusListItem
	{
		/// <summary>
		/// Event called when to show user information
		/// </summary>
		public event ProfileEventHandler ShowProfile;
	}
}
