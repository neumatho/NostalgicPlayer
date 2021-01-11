/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Drawing;
using Krypton.Toolkit;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow.ListItem;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow
{
	/// <summary>
	/// This is the class used for each item in the module list
	/// </summary>
	public class ModuleListItem : KryptonListItem
	{
		private const int ImageSize = 8;

		private readonly IModuleListItem listItem;

		private bool isPlaying;
		private TimeSpan time;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleListItem(IModuleListItem listItem)
		{
			this.listItem = listItem;

			ShortText = listItem.DisplayName;
			time = new TimeSpan(0);
			HaveTime = false;

			Image = new Bitmap(ImageSize,ImageSize);
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

				Image.Dispose();
				Image = isPlaying ? Properties.Resources.PlayingItem : new Bitmap(ImageSize, ImageSize);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Open the file
		/// </summary>
		/********************************************************************/
		public ModuleStream OpenFile()
		{
			return listItem.OpenStream();
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
				HaveTime = true;

				if (time.Ticks == 0)
					LongText = string.Empty;
				else
				{
					TimeSpan tempTime = new TimeSpan((((long)value.TotalMilliseconds + 500) / 1000 * 1000) * TimeSpan.TicksPerMillisecond);
					if ((int)tempTime.TotalHours > 0)
						LongText = tempTime.ToString(Properties.Resources.IDS_TIMEFORMAT);
					else
						LongText = tempTime.ToString(Properties.Resources.IDS_TIMEFORMAT_SMALL);
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
	}
}
