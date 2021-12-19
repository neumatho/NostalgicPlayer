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
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings
{
	/// <summary>
	/// This class holds the window position and size
	/// </summary>
	public class WindowSettings
	{
		private readonly ISettings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public WindowSettings(ISettings windowSettings)
		{
			settings = windowSettings;
		}



		/********************************************************************/
		/// <summary>
		/// Screen geometry
		/// </summary>
		/********************************************************************/
		public string Geometry
		{
			get => settings.GetStringEntry("Window", "Geometry");

			set => settings.SetStringEntry("Window", "Geometry", value);
		}



		/********************************************************************/
		/// <summary>
		/// Window position
		/// </summary>
		/********************************************************************/
		public Point Location
		{
			get
			{
				return new Point(settings.GetIntEntry("Window", "X", 0), settings.GetIntEntry("Window", "Y", 0));
			}

			set
			{
				settings.SetIntEntry("Window", "X", value.X);
				settings.SetIntEntry("Window", "Y", value.Y);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Window size
		/// </summary>
		/********************************************************************/
		public Size Size
		{
			get
			{
				return new Size(settings.GetIntEntry("Window", "Width", 10), settings.GetIntEntry("Window", "Height", 10));
			}

			set
			{
				settings.SetIntEntry("Window", "Width", value.Width);
				settings.SetIntEntry("Window", "Height", value.Height);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Tells if the window is maximized
		/// </summary>
		/********************************************************************/
		public bool? Maximized
		{
			get
			{
				if (settings.ContainsEntry("Window", "Maximized"))
					return settings.GetBoolEntry("Window", "Maximized");

				return null;
			}

			set
			{
				if (!value.HasValue)
					throw new ArgumentNullException(nameof(value));

				settings.SetBoolEntry("Window", "Maximized", value.Value);
			}
		}
	}
}
