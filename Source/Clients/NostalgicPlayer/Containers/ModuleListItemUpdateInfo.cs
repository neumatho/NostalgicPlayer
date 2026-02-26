/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Containers
{
	/// <summary>
	/// Indicate which item and what to update on it
	/// </summary>
	public class ModuleListItemUpdateInfo
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleListItemUpdateInfo(ModuleListItem moduleListItem)
		{
			ListItem = moduleListItem;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the item to update
		/// </summary>
		/********************************************************************/
		public ModuleListItem ListItem
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the new duration
		/// </summary>
		/********************************************************************/
		public TimeSpan? Duration
		{
			get; set;
		}
	}
}
