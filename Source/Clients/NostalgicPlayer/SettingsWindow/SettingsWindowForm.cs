/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Windows.Forms;
using Krypton.Navigator;
using Krypton.Toolkit;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Bases;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Modules;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow
{
	/// <summary>
	/// This shows the settings window
	/// </summary>
	public partial class SettingsWindowForm : WindowFormBase
	{
		private Manager agentManager;
		private ModuleHandler moduleHandler;
		private MainWindowForm mainWindow;

		private readonly ISettings userSettings;
		private readonly SettingsWindowSettings windowSettings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SettingsWindowForm(Manager agentManager, ModuleHandler moduleHandler, MainWindowForm mainWindow, OptionSettings optionSettings, ISettings userSettings)
		{
			InitializeComponent();

			// Some controls need to be initialized here, since the
			// designer remove the properties
			navigator.Panel.PanelBackStyle = PaletteBackStyle.TabLowProfile;
			navigator.Button.CloseButtonDisplay = ButtonDisplay.Hide;
			navigator.Button.ContextButtonDisplay = ButtonDisplay.Hide;

			// Remember the arguments
			this.agentManager = agentManager;
			this.moduleHandler = moduleHandler;
			this.mainWindow = mainWindow;
			this.userSettings = userSettings;

			if (!DesignMode)
			{
				InitializeWindow(mainWindow, optionSettings);

				// Load window settings
				LoadWindowSettings("SettingsWindow");
				windowSettings = new SettingsWindowSettings(allWindowSettings);

				// Set the title of the window
				Text = Resources.IDS_SETTINGS_TITLE;

				// Set the string resources on each string per tab
				navigator.Pages[0].Text = Resources.IDS_SETTINGS_TAB_OPTIONS;
				navigator.Pages[1].Text = Resources.IDS_SETTINGS_TAB_PATHS;
				navigator.Pages[2].Text = Resources.IDS_SETTINGS_TAB_MIXER;
				navigator.Pages[3].Text = Resources.IDS_SETTINGS_TAB_AGENTS;

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
			if (mainWindow != null)
			{
				optionsPageControl.RefreshWindow();
				pathsPageControl.RefreshWindow();
				mixerPageControl.RefreshWindow();
				agentsPageControl.RefreshWindow();
			}
		}

		#region Event handlers

		#region Form events
		/********************************************************************/
		/// <summary>
		/// Is called when the window is closed
		/// </summary>
		/********************************************************************/
		private void SettingsWindowForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (mainWindow != null)
			{
				// Cancel the settings
				CancelSettings();

				// Save any specific window settings
				windowSettings.ActiveTab = navigator.SelectedIndex;

				optionsPageControl.WriteWindowSettings();
				pathsPageControl.WriteWindowSettings();
				mixerPageControl.WriteWindowSettings();
				agentsPageControl.WriteWindowSettings();

				// Cleanup
				agentManager = null;
				moduleHandler = null;
				mainWindow = null;
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
			optionsPageControl.InitSettings(agentManager, moduleHandler, mainWindow, userSettings, allWindowSettings);
			pathsPageControl.InitSettings(agentManager, moduleHandler, mainWindow, userSettings, allWindowSettings);
			mixerPageControl.InitSettings(agentManager, moduleHandler, mainWindow, userSettings, allWindowSettings);
			agentsPageControl.InitSettings(agentManager, moduleHandler, mainWindow, userSettings, allWindowSettings);

			// Make a backup of the settings. This is used for real-time
			// settings, that can be restored back when clicking cancel
			optionsPageControl.MakeBackup();
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
			pathsPageControl.ReadWindowSettings();
			mixerPageControl.ReadWindowSettings();
			agentsPageControl.ReadWindowSettings();

			// Load all the settings
			optionsPageControl.ReadSettings();
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
			pathsPageControl.WriteSettings();
			mixerPageControl.WriteSettings();
			agentsPageControl.WriteSettings();

			// Save the settings to disk
			userSettings.SaveSettings();
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
			pathsPageControl.CancelSettings();
			mixerPageControl.CancelSettings();
			agentsPageControl.CancelSettings();
		}
		#endregion
	}
}
