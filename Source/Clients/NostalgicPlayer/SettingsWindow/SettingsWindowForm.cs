/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Windows.Forms;
using Krypton.Navigator;
using Krypton.Toolkit;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Services;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.MainWindow;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow
{
	/// <summary>
	/// This shows the settings window
	/// </summary>
	public partial class SettingsWindowForm : WindowFormBase
	{
		private IMainWindowApi mainWindowApi;

		private readonly ISettingsService settingsService;
		private readonly SettingsWindowSettings windowSettings;

		private const int Page_Options = 0;
		private const int Page_Modules = 1;
		private const int Page_Paths = 2;
		private const int Page_Mixer = 3;
		private const int Page_Agents = 4;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SettingsWindowForm()
		{
			InitializeComponent();

			// Some controls need to be initialized here, since the
			// designer remove the properties
			navigator.Panel.PanelBackStyle = PaletteBackStyle.TabLowProfile;
			navigator.Button.CloseButtonDisplay = ButtonDisplay.Hide;
			navigator.Button.ContextButtonDisplay = ButtonDisplay.Hide;

			if (!DesignMode)
			{
				mainWindowApi = DependencyInjection.Container.GetInstance<IMainWindowApi>();
				settingsService = DependencyInjection.Container.GetInstance<ISettingsService>();

				InitializeWindow();

				// Load window settings
				LoadWindowSettings("SettingsWindow");
				windowSettings = new SettingsWindowSettings(allWindowSettings);

				// Set the title of the window
				Text = Resources.IDS_SETTINGS_TITLE;

				// Set the string resources on each string per tab
				navigator.Pages[Page_Options].Text = Resources.IDS_SETTINGS_TAB_OPTIONS;
				navigator.Pages[Page_Modules].Text = Resources.IDS_SETTINGS_TAB_MODULES;
				navigator.Pages[Page_Paths].Text = Resources.IDS_SETTINGS_TAB_PATHS;
				navigator.Pages[Page_Mixer].Text = Resources.IDS_SETTINGS_TAB_MIXER;
				navigator.Pages[Page_Agents].Text = Resources.IDS_SETTINGS_TAB_AGENTS;

				// Initialize the pages
				InitSettings();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will refresh different tabs
		/// </summary>
		/********************************************************************/
		public void RefreshWindow()
		{
			if (mainWindowApi != null)
			{
				optionsPageControl.RefreshWindow();
				modulesPageControl.RefreshWindow();
				pathsPageControl.RefreshWindow();
				mixerPageControl.RefreshWindow();
				agentsPageControl.RefreshWindow();
			}
		}

		#region WindowFormBase overrides
		/********************************************************************/
		/// <summary>
		/// Return the URL to the help page
		/// </summary>
		/********************************************************************/
		protected override string HelpUrl
		{
			get
			{
				switch (navigator.SelectedIndex)
				{
					case Page_Options:
						return "settings.html#options";

					case Page_Modules:
						return "settings.html#modules";

					case Page_Paths:
						return "settings.html#paths";

					case Page_Mixer:
						return "settings.html#mixer";

					case Page_Agents:
						return "settings.html#agents";

					default:
						return null;
				}
			}
		}

		#endregion

		#region Event handlers

		#region Form events
		/********************************************************************/
		/// <summary>
		/// Is called when the window is closed
		/// </summary>
		/********************************************************************/
		private void SettingsWindowForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (mainWindowApi != null)
			{
				// Cancel the settings
				CancelSettings();

				// Save any specific window settings
				windowSettings.ActiveTab = navigator.SelectedIndex;

				optionsPageControl.WriteWindowSettings();
				modulesPageControl.WriteWindowSettings();
				pathsPageControl.WriteWindowSettings();
				mixerPageControl.WriteWindowSettings();
				agentsPageControl.WriteWindowSettings();

				// Cleanup
				mainWindowApi = null;
			}
		}
		#endregion

		#region Button events
		/********************************************************************/
		/// <summary>
		/// Is called when the user clicked the ok button
		/// </summary>
		/********************************************************************/
		private void OkButton_Click(object sender, EventArgs e)
		{
			// Save the settings and close
			SaveSettings();
			Close();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user clicked the cancel button
		/// </summary>
		/********************************************************************/
		private void CancelButton_Click(object sender, EventArgs e)
		{
			// Just close the window, which will also do a cancel
			Close();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user clicked the apply button
		/// </summary>
		/********************************************************************/
		private void ApplyButton_Click(object sender, EventArgs e)
		{
			// Save the settings
			SaveSettings();
		}
		#endregion

		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will initialize all the pages, so they show the current settings
		/// </summary>
		/********************************************************************/
		private void InitSettings()
		{
			// Initialize the tab pages
			optionsPageControl.InitSettings(allWindowSettings);
			modulesPageControl.InitSettings(allWindowSettings);
			pathsPageControl.InitSettings(allWindowSettings);
			mixerPageControl.InitSettings(allWindowSettings);
			agentsPageControl.InitSettings(allWindowSettings);

			// Make a backup of the settings. This is used for real-time
			// settings, that can be restored back when clicking cancel
			optionsPageControl.MakeBackup();
			modulesPageControl.MakeBackup();
			pathsPageControl.MakeBackup();
			mixerPageControl.MakeBackup();
			agentsPageControl.MakeBackup();

			// Then load all the settings
			LoadSettings();

			// Refresh the pages
			RefreshWindow();

			// Select the last selected tab page
			navigator.SelectedIndex = windowSettings.ActiveTab;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the settings
		/// </summary>
		/********************************************************************/
		private void LoadSettings()
		{
			// Load all the window settings
			optionsPageControl.ReadWindowSettings();
			modulesPageControl.ReadWindowSettings();
			pathsPageControl.ReadWindowSettings();
			mixerPageControl.ReadWindowSettings();
			agentsPageControl.ReadWindowSettings();

			// Load all the settings
			optionsPageControl.ReadSettings();
			modulesPageControl.ReadSettings();
			pathsPageControl.ReadSettings();
			mixerPageControl.ReadSettings();
			agentsPageControl.ReadSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Save all the settings
		/// </summary>
		/********************************************************************/
		private void SaveSettings()
		{
			// Save all the settings
			optionsPageControl.WriteSettings();
			modulesPageControl.WriteSettings();
			pathsPageControl.WriteSettings();
			mixerPageControl.WriteSettings();
			agentsPageControl.WriteSettings();

			// Save the settings to disk
			settingsService.SaveSettings();

			// Update main window UI based on changed settings
			mainWindowApi?.EnableUserInterfaceSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Will restore real-time values
		/// </summary>
		/********************************************************************/
		private void CancelSettings()
		{
			// Cancel all the settings
			optionsPageControl.CancelSettings();
			modulesPageControl.CancelSettings();
			pathsPageControl.CancelSettings();
			mixerPageControl.CancelSettings();
			agentsPageControl.CancelSettings();
		}
		#endregion
	}
}
