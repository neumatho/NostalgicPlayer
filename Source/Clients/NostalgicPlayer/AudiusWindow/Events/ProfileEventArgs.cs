/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.ListItems;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.Events
{
	/// <summary></summary>
	public delegate void ProfileEventHandler(object sender, ProfileEventArgs e);

	/// <summary>
	/// Event class holding needed information when showing a profile
	/// </summary>
	public class ProfileEventArgs : EventArgs
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ProfileEventArgs(AudiusProfileListItem item)
		{
			Item = item;
		}



		/********************************************************************/
		/// <summary>
		/// Holding the track item
		/// </summary>
		/********************************************************************/
		public AudiusProfileListItem Item
		{
			get;
		}
	}
}
