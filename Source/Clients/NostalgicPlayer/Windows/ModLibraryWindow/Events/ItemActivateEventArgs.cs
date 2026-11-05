/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Events
{
	/// <summary></summary>
	public delegate void ItemActivateEventHandler(object sender, ItemActivateEventArgs e);

	/// <summary>
	/// Event args for when an item in the list is activated
	/// </summary>
	public class ItemActivateEventArgs : EventArgs
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ItemActivateEventArgs(TreeNode entry, int index)
		{
			Entry = entry;
			Index = index;
		}



		/********************************************************************/
		/// <summary>
		/// The tree node entry that was activated
		/// </summary>
		/********************************************************************/
		public TreeNode Entry
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// The index of the item in the list
		/// </summary>
		/********************************************************************/
		public int Index
		{
			get;
		}
	}
}
