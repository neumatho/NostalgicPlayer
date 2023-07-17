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
	/// Holds all the controls for the Modules tab
	/// </summary>
	public partial class ModulesPageControl : UserControl, ISettingsPage
	{
		private MainWindowForm mainWin;

		private ModuleSettings moduleSettings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModulesPageControl()
		{
			InitializeComponent();

			// Add items to the combo controls
			moduleErrorComboBox.Items.AddRange(new object[]
			{
				Resources.IDS_SETTINGS_MODULES_LOADING_MODULEERROR_SHOW,
				Resources.IDS_SETTINGS_MODULES_LOADING_MODULEERROR_SKIP,
				Resources.IDS_SETTINGS_MODULES_LOADING_MODULEERROR_SKIPREMOVE,
				Resources.IDS_SETTINGS_MODULES_LOADING_MODULEERROR_STOP
			});

			moduleListEndComboBox.Items.AddRange(new object[]
			{
				Resources.IDS_SETTINGS_MODULES_PLAYING_MODULELISTEND_EJECT,
				Resources.IDS_SETTINGS_MODULES_PLAYING_MODULELISTEND_JUMPTOSTART,
				Resources.IDS_SETTINGS_MODULES_PLAYING_MODULELISTEND_LOOP
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

			moduleSettings = new ModuleSettings(userSettings);
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
			// Loading
			doubleBufferingCheckBox.Checked = moduleSettings.DoubleBuffering;
			doubleBufferingTrackBar.Value = moduleSettings.DoubleBufferingEarlyLoad;

			moduleErrorComboBox.SelectedIndex = (int)moduleSettings.ModuleError;

			// Playing
			neverEndingCheckBox.Checked = moduleSettings.NeverEnding;
			neverEndingNumberTextBox.Text = moduleSettings.NeverEndingTimeout.ToString();

			moduleListEndComboBox.SelectedIndex = (int)moduleSettings.ModuleListEnd;
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
			// Loading
			moduleSettings.DoubleBuffering = doubleBufferingCheckBox.Checked;
			moduleSettings.DoubleBufferingEarlyLoad = doubleBufferingTrackBar.Value;

			moduleSettings.ModuleError = (ModuleSettings.ModuleErrorAction)moduleErrorComboBox.SelectedIndex;

			// Playing
			moduleSettings.NeverEnding = neverEndingCheckBox.Checked;
			moduleSettings.NeverEndingTimeout = int.Parse(neverEndingNumberTextBox.Text);

			moduleSettings.ModuleListEnd = (ModuleSettings.ModuleListEndAction)moduleListEndComboBox.SelectedIndex;
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
