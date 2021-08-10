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
using Krypton.Toolkit;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Bases
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

		/// <summary>
		/// Set this to true, if you don't want the escape
		/// key to close the window
		/// </summary>
		protected bool disableEscapeKey = false;

		private WindowSettings windowSettings;

		/********************************************************************/
		/// <summary>
		/// Will update the window settings
		/// </summary>
		/********************************************************************/
		public void UpdateWindowSettings()
		{
			if (WindowState == FormWindowState.Normal)
			{
				// Update the settings with the window position
				windowSettings.Location = Location;

				if (FormBorderStyle == FormBorderStyle.Sizable)
					windowSettings.Size = Size;

				windowSettings.Geometry = ScreensGeometry();
			}

			windowSettings.Maximized = WindowState == FormWindowState.Maximized;
		}



		/********************************************************************/
		/// <summary>
		/// Call this to initialize the window with basis settings
		/// </summary>
		/********************************************************************/
		protected void InitializeWindow(MainWindowForm mainWindow, OptionSettings optionSettings)
		{
			// Set how the window should act in the task bar and task switcher
			if (!optionSettings.SeparateWindows)
			{
				Owner = mainWindow;
				ShowInTaskbar = false;
			}
			else
				ShowInTaskbar = optionSettings.ShowWindowsInTaskBar;
		}



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

			if (FormBorderStyle == FormBorderStyle.Sizable)
			{
				Size windowSize = windowSettings.Size;

				if (windowSize.Width < MinimumSize.Width)
					windowSize.Width = MinimumSize.Width;

				if (windowSize.Height < MinimumSize.Height)
					windowSize.Height = MinimumSize.Height;

				if ((windowSize.Width != 0) && (windowSize.Height != 0))
					Size = new Size(windowSize.Width, windowSize.Height);

				bool? maximized = windowSettings.Maximized;
				if (maximized.HasValue)
					WindowState = maximized.Value ? FormWindowState.Maximized : FormWindowState.Normal;
			}
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
				UpdateWindowSettings();

				// Save the window settings
				allWindowSettings.SaveSettings();
				allWindowSettings.Dispose();
				allWindowSettings = null;

				windowSettings = null;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when a key is pressed in the main window
		/// </summary>
		/********************************************************************/
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (!disableEscapeKey)
			{
				// Check for different keyboard shortcuts
				Keys modifiers = keyData & Keys.Modifiers;
				Keys key = keyData & Keys.KeyCode;

				if (modifiers == Keys.None)
				{
					if (key == Keys.Escape)
					{
						Close();
						return true;
					}
				}
			}

			return base.ProcessCmdKey(ref msg, keyData);
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
