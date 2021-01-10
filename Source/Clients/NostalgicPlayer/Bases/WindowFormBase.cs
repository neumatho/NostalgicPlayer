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
using Polycode.NostalgicPlayer.NostalgicPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.NostalgicPlayerKit.Utility;

namespace Polycode.NostalgicPlayer.NostalgicPlayer.Bases
{
	/// <summary>
	/// Use the class as the base class for all windows
	/// </summary>
	public class WindowFormBase : KryptonForm
	{
		/// <summary>
		/// Holds all the settings for the form itself
		/// </summary>
		protected Settings allWindowSettings;

		private WindowSettings windowSettings;

		/********************************************************************/
		/// <summary>
		/// Call this to load window settings
		/// </summary>
		/********************************************************************/
		protected void LoadWindowSettings(string windowSettingsName)
		{
			// Load the windows settings
			allWindowSettings = new Settings(windowSettingsName);
			allWindowSettings.LoadSettings();

			windowSettings = new WindowSettings(allWindowSettings);

			string geometry = windowSettings.Geometry;
			if (!string.IsNullOrEmpty(geometry) && (ScreensGeometry() == geometry))
			{
				StartPosition = FormStartPosition.Manual;
				Location = windowSettings.Location;
			}

			Size windowSize = windowSettings.Size;

			if (windowSize.Width < MinimumSize.Width)
				windowSize.Width = MinimumSize.Width;

			if (windowSize.Height < MinimumSize.Height)
				windowSize.Height = MinimumSize.Height;

			if ((windowSize.Width != 0) && (windowSize.Height != 0))
				Size = new Size(windowSize.Width, windowSize.Height);
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
				windowSettings.Location = Location;

				if (FormBorderStyle == FormBorderStyle.Sizable)
					windowSettings.Size = Size;

				windowSettings.Geometry = ScreensGeometry();

				// Save the window settings
				allWindowSettings.SaveSettings();
				allWindowSettings.Dispose();
				allWindowSettings = null;

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
