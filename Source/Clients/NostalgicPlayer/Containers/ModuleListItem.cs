/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow.ListItem;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Containers
{
	/// <summary>
	/// This is the class used for each item in the module list
	/// </summary>
	public class ModuleListItem : IComparable<ModuleListItem>
	{
		private TimeSpan duration;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleListItem(IModuleListItem listItem)
		{
			ListItem = listItem;

			duration = new TimeSpan(0);
			HaveTime = false;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the list item information
		/// </summary>
		/********************************************************************/
		public IModuleListItem ListItem
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the duration of the module this item has
		/// </summary>
		/********************************************************************/
		public TimeSpan Duration
		{
			get => duration;

			set
			{
				duration = value;
				HaveTime = value.Ticks != 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Indicate if the time has been set
		/// </summary>
		/********************************************************************/
		public bool HaveTime
		{
			get; private set;
		}



		/********************************************************************/
		/// <summary>
		/// Indicate if this item is marked as the playing one
		/// </summary>
		/********************************************************************/
		public bool IsPlaying
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the default sub-song if set
		/// </summary>
		/********************************************************************/
		public int? DefaultSubSong
		{
			get; set;
		}

		#region IComparable implementation
		/********************************************************************/
		/// <summary>
		/// Compare two module list items
		/// </summary>
		/********************************************************************/
		public int CompareTo(ModuleListItem other)
		{
			return ListItem.DisplayName.CompareTo(other.ListItem.DisplayName);
		}
		#endregion
	}
}
