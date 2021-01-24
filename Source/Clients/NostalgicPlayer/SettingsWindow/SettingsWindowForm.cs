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

		private readonly Settings userSettings;
		private readonly SettingsWindowSettings windowSettings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SettingsWindowForm(Manager agentManager, ModuleHandler moduleHandler, Settings userSettings)
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
			this.userSettings = userSettings;

			if (!DesignMode)
			{
				// Load window settings
				LoadWindowSettings("SettingsWindow");
				windowSettings = new SettingsWindowSettings(allWindowSettings);

				// Set the title of the window
				Text = Resources.IDS_SETTINGS_TITLE;

				// Set the string resources on each string per tab
				navigator.Pages[0].Text = Resources.IDS_SETTINGS_TAB_PATHS;
				navigator.Pages[1].Text = Resources.IDS_SETTINGS_TAB_MIXER;
				navigator.Pages[2].Text = Resources.IDS_SETTINGS_TAB_AGENTS;

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
			pathsPageControl.RefreshWindow();
			mixerPageControl.RefreshWindow();
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
			// Cancel the settings
			CancelSettings();

			// Save any specific window settings
			windowSettings.ActiveTab = navigator.SelectedIndex;

			pathsPageControl.RememberWindowSettings();
			mixerPageControl.RememberWindowSettings();

			// Cleanup
			agentManager = null;
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
			// Cancel the settings and close
			CancelSettings();
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
			// Make a backup of the settings. This is used for real-time
			// settings, that can be restored back when clicking cancel
			MakeBackup();

			// Initialize the tab pages
			pathsPageControl.InitSettings(agentManager, moduleHandler, userSettings);
			mixerPageControl.InitSettings(agentManager, moduleHandler, userSettings);

			pathsPageControl.InitWindowSettings(allWindowSettings);
			mixerPageControl.InitWindowSettings(allWindowSettings);

			// Refresh the pages
			RefreshWindow();

			// Select the last selected tab page
			navigator.SelectedIndex = windowSettings.ActiveTab;
		}



		/********************************************************************/
		/// <summary>
		/// Will make a backup of the settings
		/// </summary>
		/********************************************************************/
		private void MakeBackup()
		{
			// Let the tab pages make a backup
			pathsPageControl.MakeBackup();
			mixerPageControl.MakeBackup();
		}



		/********************************************************************/
		/// <summary>
		/// Save all the settings
		/// </summary>
		/********************************************************************/
		private void SaveSettings()
		{
			// Save all the settings
			pathsPageControl.RememberSettings();
			mixerPageControl.RememberSettings();

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
			pathsPageControl.CancelSettings();
			mixerPageControl.CancelSettings();
		}
		#endregion
	}
}
