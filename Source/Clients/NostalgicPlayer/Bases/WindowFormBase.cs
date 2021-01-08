/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Drawing;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;
using Polycode.NostalgicPlayer.NostalgicPlayerKit.Utility;

namespace Polycode.NostalgicPlayer.NostalgicPlayer.Bases
{
	/// <summary>
	/// Use the class as the base class for all windows
	/// </summary>
	public class WindowFormBase : KryptonForm
	{
		/// <summary>
		/// Holds the settings for the form itself
		/// </summary>
		protected Settings windowSettings;

		/********************************************************************/
		/// <summary>
		/// Call this to load window settings
		/// </summary>
		/********************************************************************/
		protected void LoadWindowSettings(string windowSettingsName)
		{
			// Load the windows settings
			windowSettings = new Settings(windowSettingsName);
			windowSettings.LoadSettings();

			string geometry = windowSettings.GetStringEntry("Window", "Geometry");
			if (!string.IsNullOrEmpty(geometry) && (ScreensGeometry() == geometry))
			{
				StartPosition = FormStartPosition.Manual;
				Location = new Point(windowSettings.GetIntEntry("Window", "X"), windowSettings.GetIntEntry("Window", "Y"));
			}

			int width = windowSettings.GetIntEntry("Window", "Width");
			int height = windowSettings.GetIntEntry("Window", "Height");

			if (width < MinimumSize.Width)
				width = MinimumSize.Width;

			if (height < MinimumSize.Height)
				height = MinimumSize.Height;

			if ((width != 0) && (height != 0))
				Size = new Size(width, height);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the form is closed
		/// </summary>
		/********************************************************************/
		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			base.OnFormClosed(e);

			if (windowSettings != null)
			{
				// Update the settings with the window position
				windowSettings.SetIntEntry("Window", "X", Location.X);
				windowSettings.SetIntEntry("Window", "Y", Location.Y);

				if (FormBorderStyle == FormBorderStyle.Sizable)
				{
					windowSettings.SetIntEntry("Window", "Width", Size.Width);
					windowSettings.SetIntEntry("Window", "Height", Size.Height);
				}

				windowSettings.SetStringEntry("Window", "Geometry", ScreensGeometry());

				// Save the window settings
				windowSettings.SaveSettings();
				windowSettings.Dispose();
				windowSettings = null;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Find all screens geometry and return them in a single string
		/// </summary>
		/********************************************************************/
		private string ScreensGeometry()
		{
			string geometry = string.Empty;

			foreach (Screen screen in Screen.AllScreens)
				geometry += screen.WorkingArea;

			return geometry;
		}
	}
}
