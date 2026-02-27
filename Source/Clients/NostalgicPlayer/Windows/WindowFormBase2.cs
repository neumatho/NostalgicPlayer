/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.MainWindow;
using Polycode.NostalgicPlayer.Controls.Forms;
using Polycode.NostalgicPlayer.Controls.Theme;
using Polycode.NostalgicPlayer.Controls.Theme.Purple;
using Polycode.NostalgicPlayer.Controls.Theme.Standard;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows
{
	/// <summary>
	/// Use the class as the base class for all windows
	/// </summary>
	public class WindowFormBase2 : NostalgicForm, IWindowForm
	{
		/// <summary>
		/// Holds all the settings for the form itself
		/// </summary>
		protected ISettings allWindowSettings;

		/// <summary>
		/// Set this to true, if you don't want the escape
		/// key to close the window
		/// </summary>
		protected bool disableEscapeKey = false;

		/// <summary>
		/// Holds the interface to the main window API
		/// </summary>
		private IMainWindowApi mainWindowApi;

		private OptionSettings optionSettings;
		private WindowSettings windowSettings;

		/********************************************************************/
		/// <summary>
		/// Initialize the form
		///
		/// Called from FormCreatorService
		/// </summary>
		/********************************************************************/
		public void InitializeBaseForm(IMainWindowApi mainWindowApi, ISettings settings, OptionSettings optionSettings)
		{
			this.mainWindowApi = mainWindowApi;
			allWindowSettings = settings;
			this.optionSettings = optionSettings;
		}



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
		protected void InitializeWindow()
		{
			// Set how the window should act in the task bar and task switcher
			if (!optionSettings.SeparateWindows)
			{
				Owner = mainWindowApi.Form;
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
			allWindowSettings = DependencyInjection.Container.GetInstance<ISettings>();
			allWindowSettings.LoadSettings(windowSettingsName);

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
		/// Return the URL to the help page
		/// </summary>
		/********************************************************************/
		protected virtual string HelpUrl => null;



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
			// Check for different keyboard shortcuts
			Keys modifiers = keyData & Keys.Modifiers;
			Keys key = keyData & Keys.KeyCode;

			if (modifiers == Keys.None)
			{
				switch (key)
				{
					case Keys.Escape:
					{
						if (disableEscapeKey)
							break;

						Close();
						return true;
					}

					case Keys.F1:
					{
						if (ShowWindowSpecificHelp())
							return true;

						break;
					}

					//XX Denne skal slettes når jeg er færdig
					case Keys.F12:
					{
						theme = !theme;
						ThemeManagerFactory.GetThemeManager().SwitchTheme(theme ? new PurpleTheme() : new StandardTheme());

						return true;
					}
				}
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}
		private bool theme = false;



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



		/********************************************************************/
		/// <summary>
		/// Open the help and navigate to the right page
		/// </summary>
		/********************************************************************/
		private bool ShowWindowSpecificHelp()
		{
			string url = HelpUrl;
			if (string.IsNullOrEmpty(url))
				return false;

			mainWindowApi.OpenHelpWindow(url);

			return true;
		}
	}
}
