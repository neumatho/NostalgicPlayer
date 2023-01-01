/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
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

			// Add items to the combo controls
			moduleErrorComboBox.Items.AddRange(new object[]
			{
				Resources.IDS_SETTINGS_OPTIONS_LOADING_MODULEERROR_SHOW,
				Resources.IDS_SETTINGS_OPTIONS_LOADING_MODULEERROR_SKIP,
				Resources.IDS_SETTINGS_OPTIONS_LOADING_MODULEERROR_SKIPREMOVE,
				Resources.IDS_SETTINGS_OPTIONS_LOADING_MODULEERROR_STOP
			});

			moduleListEndComboBox.Items.AddRange(new object[]
			{
				Resources.IDS_SETTINGS_OPTIONS_PLAYING_MODULELISTEND_EJECT,
				Resources.IDS_SETTINGS_OPTIONS_PLAYING_MODULELISTEND_JUMPTOSTART,
				Resources.IDS_SETTINGS_OPTIONS_PLAYING_MODULELISTEND_LOOP
			});
		}

		#region ISettingsPage implementation
		/********************************************************************/
		/// <summary>
		/// Will prepare to handle the settings
		/// </summary>
		/********************************************************************/
		public void InitSettings(Manager agentManager, ModuleHandler moduleHandler, MainWindowForm mainWindow, ISettings userSettings, ISettings windowSettings)
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
			// General
			addJumpCheckBox.Checked = optionSettings.AddJump;
			addToListCheckBox.Checked = optionSettings.AddToList;
			rememberListCheckBox.Checked = optionSettings.RememberList;
			rememberListPositionCheckBox.Checked = optionSettings.RememberListPosition;
			rememberModulePositionCheckBox.Checked = optionSettings.RememberModulePosition;
			showListNumberCheckBox.Checked = optionSettings.ShowListNumber;

			tooltipsCheckBox.Checked = optionSettings.ToolTips;
			showNameInTitleCheckBox.Checked = optionSettings.ShowNameInTitle;
			separateWindowsCheckBox.Checked = optionSettings.SeparateWindows;
			showWindowsInTaskBarCheckBox.Checked = optionSettings.ShowWindowsInTaskBar;

			scanFilesCheckBox.Checked = optionSettings.ScanFiles;
			useDatabaseCheckBox.Checked = optionSettings.UseDatabase;

			// Loading
			doubleBufferingCheckBox.Checked = optionSettings.DoubleBuffering;
			doubleBufferingTrackBar.Value = optionSettings.DoubleBufferingEarlyLoad;

			moduleErrorComboBox.SelectedIndex = (int)optionSettings.ModuleError;

			// Playing
			neverEndingCheckBox.Checked = optionSettings.NeverEnding;
			neverEndingNumberTextBox.Text = optionSettings.NeverEndingTimeout.ToString();

			moduleListEndComboBox.SelectedIndex = (int)optionSettings.ModuleListEnd;
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
			// General
			optionSettings.AddJump = addJumpCheckBox.Checked;
			optionSettings.AddToList = addToListCheckBox.Checked;
			optionSettings.RememberList = rememberListCheckBox.Checked;
			optionSettings.RememberListPosition = rememberListPositionCheckBox.Checked;
			optionSettings.RememberModulePosition = rememberModulePositionCheckBox.Checked;
			optionSettings.ShowListNumber = showListNumberCheckBox.Checked;

			optionSettings.ToolTips = tooltipsCheckBox.Checked;
			optionSettings.ShowNameInTitle = showNameInTitleCheckBox.Checked;
			optionSettings.SeparateWindows = separateWindowsCheckBox.Checked;
			optionSettings.ShowWindowsInTaskBar = showWindowsInTaskBarCheckBox.Checked;

			optionSettings.ScanFiles = scanFilesCheckBox.Checked;
			optionSettings.UseDatabase = useDatabaseCheckBox.Checked;

			// Loading
			optionSettings.DoubleBuffering = doubleBufferingCheckBox.Checked;
			optionSettings.DoubleBufferingEarlyLoad = doubleBufferingTrackBar.Value;

			optionSettings.ModuleError = (OptionSettings.ModuleErrorAction)moduleErrorComboBox.SelectedIndex;

			// Playing
			optionSettings.NeverEnding = neverEndingCheckBox.Checked;
			optionSettings.NeverEndingTimeout = int.Parse(neverEndingNumberTextBox.Text);

			optionSettings.ModuleListEnd = (OptionSettings.ModuleListEndAction)moduleListEndComboBox.SelectedIndex;

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
		private void RememberListCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			rememberListPanel.Enabled = rememberListCheckBox.Checked;
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user change the remember module
		/// </summary>
		/********************************************************************/
		private void RememberListPositionCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			rememberModulePositionCheckBox.Enabled = rememberListPositionCheckBox.Checked;
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user change the separate windows
		/// </summary>
		/********************************************************************/
		private void SeparateWindows_CheckedChanged(object sender, EventArgs e)
		{
			windowPanel.Enabled = separateWindowsCheckBox.Checked;
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user change the double buffering
		/// </summary>
		/********************************************************************/
		private void DoubleBufferingCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			doubleBufferingPanel.Enabled = doubleBufferingCheckBox.Checked;
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user change the never ending
		/// </summary>
		/********************************************************************/
		private void NeverEnding_CheckedChanged(object sender, EventArgs e)
		{
			neverEndingNumberTextBox.Enabled = neverEndingCheckBox.Checked;
		}
		#endregion
	}
}
