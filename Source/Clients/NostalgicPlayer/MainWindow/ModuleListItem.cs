/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.ComponentModel;
using System.Drawing;
using Krypton.Toolkit;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow.ListItem;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow
{
	/// <summary>
	/// This is the class used for each item in the module list
	/// </summary>
	public class ModuleListItem : Component, IContentValues, IComparable<ModuleListItem>
	{
		private const int ImageSize = 8;

		private bool isPlaying;
		private TimeSpan time;

		private Image image;
		private readonly string displayName;
		private string moduleTime;

		private KryptonListBox listBox;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleListItem(IModuleListItem listItem)
		{
			ListItem = listItem;

			displayName = listItem.DisplayName;
			time = new TimeSpan(0);
			HaveTime = false;

			image = new Bitmap(ImageSize,ImageSize);
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
		/// Return the module name
		/// </summary>
		/********************************************************************/
		public string ShortText
		{
			get
			{
				return GetShortText();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Indicate if this item is marked as the playing one
		/// </summary>
		/********************************************************************/
		public bool IsPlaying
		{
			get
			{
				return isPlaying;
			}

			set
			{
				isPlaying = value;

				image.Dispose();
				image = isPlaying ? Resources.IDB_PLAYING_ITEM : new Bitmap(ImageSize, ImageSize);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Holds the current time this item has
		/// </summary>
		/********************************************************************/
		public TimeSpan Time
		{
			get
			{
				return time;
			}

			set
			{
				time = value;

				if (time.Ticks == 0)
				{
					moduleTime = string.Empty;
					HaveTime = false;
				}
				else
				{
					HaveTime = true;

					TimeSpan tempTime = new TimeSpan((((long)value.TotalMilliseconds + 500) / 1000 * 1000) * TimeSpan.TicksPerMillisecond);
					if ((int)tempTime.TotalHours > 0)
						moduleTime = tempTime.ToString(Resources.IDS_TIMEFORMAT);
					else
						moduleTime = tempTime.ToString(Resources.IDS_TIMEFORMAT_SMALL);
				}
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
		/// Assign a list box control where the item is added
		/// </summary>
		/********************************************************************/
		public void SetListControl(KryptonListBox listBoxControl)
		{
			listBox = listBoxControl;
		}

		#region IContentValues implementation
		/********************************************************************/
		/// <summary>
		/// Return the image to use
		/// </summary>
		/********************************************************************/
		public Image GetImage(PaletteState state)
		{
			return image;
		}



		/********************************************************************/
		/// <summary>
		/// Return the transparent color
		/// </summary>
		/********************************************************************/
		public Color GetImageTransparentColor(PaletteState state)
		{
			return Color.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// Return the module name
		/// </summary>
		/********************************************************************/
		public string GetShortText()
		{
			if (listBox == null)
				return displayName;

			return (listBox.Items.IndexOf(this) + 1) + ". " + displayName;
		}



		/********************************************************************/
		/// <summary>
		/// Return the length of the module
		/// </summary>
		/********************************************************************/
		public string GetLongText()
		{
			return moduleTime;
		}
		#endregion

		#region IComparable implementation
		/********************************************************************/
		/// <summary>
		/// Compare two module list items
		/// </summary>
		/********************************************************************/
		public int CompareTo(ModuleListItem other)
		{
			return displayName.CompareTo(other.displayName);
		}
		#endregion
	}
}
