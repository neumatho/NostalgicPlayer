/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Modules;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow.Pages
{
	/// <summary>
	/// Holds all the controls for the Options tab
	/// </summary>
	public partial class OptionsPageControl : UserControl, ISettingsPage
	{
		private MainWindowForm mainWin;

		private OptionSettings optionSettings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public OptionsPageControl()
		{
			InitializeComponent();
		}

		#region ISettingsPage implementation
		/********************************************************************/
		/// <summary>
		/// Will prepare to handle the settings
		/// </summary>
		/********************************************************************/
		public void InitSettings(Manager agentManager, ModuleHandler moduleHandler, MainWindowForm mainWindow, Settings userSettings, Settings windowSettings)
		{
			mainWin = mainWindow;

			optionSettings = new OptionSettings(userSettings);
		}



		/********************************************************************/
		/// <summary>
		/// Will make a backup of settings that can be changed in real-time
		/// </summary>
		/********************************************************************/
		public void MakeBackup()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will read the settings and set all the controls
		/// </summary>
		/********************************************************************/
		public void ReadSettings()
		{
			addJumpCheckBox.Checked = optionSettings.AddJump;
			addToListCheckBox.Checked = optionSettings.AddToList;
			rememberListCheckBox.Checked = optionSettings.RememberList;
			rememberListPositionCheckBox.Checked = optionSettings.RememberListPosition;
			rememberModulePositionCheckBox.Checked = optionSettings.RememberModulePosition;

			tooltipsCheckBox.Checked = optionSettings.ToolTips;
			showNameInTitleCheckBox.Checked = optionSettings.ShowNameInTitle;
			showListNumberCheckBox.Checked = optionSettings.ShowListNumber;

			scanFilesCheckBox.Checked = optionSettings.ScanFiles;
			useDatabaseCheckBox.Checked = optionSettings.UseDatabase;
		}



		/********************************************************************/
		/// <summary>
		/// Will read the window settings
		/// </summary>
		/********************************************************************/
		public void ReadWindowSettings()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will read the data from the controls and store them in the
		/// settings
		/// </summary>
		/********************************************************************/
		public void WriteSettings()
		{
			optionSettings.AddJump = addJumpCheckBox.Checked;
			optionSettings.AddToList = addToListCheckBox.Checked;
			optionSettings.RememberList = rememberListCheckBox.Checked;
			optionSettings.RememberListPosition = rememberListPositionCheckBox.Checked;
			optionSettings.RememberModulePosition = rememberModulePositionCheckBox.Checked;

			optionSettings.ToolTips = tooltipsCheckBox.Checked;
			optionSettings.ShowNameInTitle = showNameInTitleCheckBox.Checked;
			optionSettings.ShowListNumber = showListNumberCheckBox.Checked;

			optionSettings.ScanFiles = scanFilesCheckBox.Checked;
			optionSettings.UseDatabase = useDatabaseCheckBox.Checked;

			mainWin.EnableUserInterfaceSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Will store window specific settings
		/// </summary>
		/********************************************************************/
		public void WriteWindowSettings()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will restore real-time values
		/// </summary>
		/********************************************************************/
		public void CancelSettings()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will refresh the page when a module is loaded/ejected
		/// </summary>
		/********************************************************************/
		public void RefreshWindow()
		{
		}
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// Is called when the user change the remember list
		/// </summary>
		/********************************************************************/
		private void RememberListCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			rememberListPanel.Enabled = rememberListCheckBox.Checked;
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user change the remember module
		/// </summary>
		/********************************************************************/
		private void RememberListPositionCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			rememberModulePositionCheckBox.Enabled = rememberListPositionCheckBox.Checked;
		}
		#endregion
	}
}
