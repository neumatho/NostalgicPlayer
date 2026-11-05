/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Drawing;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Events
{
	/// <summary></summary>
	public delegate void OpenContextMenuEventHandler(object sender, OpenContextMenuEventArgs e);

	/// <summary>
	/// Event args for when the context menu should be opened for an item
	/// </summary>
	public class OpenContextMenuEventArgs : EventArgs
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public OpenContextMenuEventArgs(TreeNode entry, Point location)
		{
			Entry = entry;
			Location = location;
		}



		/********************************************************************/
		/// <summary>
		/// The tree node entry that was right-clicked
		/// </summary>
		/********************************************************************/
		public TreeNode Entry
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// The mouse position relative to the list control
		/// </summary>
		/********************************************************************/
		public Point Location
		{
			get;
		}
	}
}
