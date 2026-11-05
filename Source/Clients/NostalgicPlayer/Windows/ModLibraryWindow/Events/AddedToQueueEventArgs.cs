/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Events
{
	/// <summary>
	/// Event args for when new items has been added to the queue
	/// </summary>
	public class AddedToQueueEventArgs : EventArgs
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AddedToQueueEventArgs(int itemsAdded)
		{
			ItemsAdded = itemsAdded;
		}



		/********************************************************************/
		/// <summary>
		/// Number of items added to the queue
		/// </summary>
		/********************************************************************/
		public int ItemsAdded
		{
			get;
		}
	}
}
