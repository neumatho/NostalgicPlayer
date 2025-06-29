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
	/// Holds all the controls for the Paths tab
	/// </summary>
	public partial class PathsPageControl : UserControl, ISettingsPage
	{
		private PathSettings pathSettings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public PathsPageControl()
		{
			InitializeComponent();
		}

		#region ISettingsPage implementation
		/********************************************************************/
		/// <summary>
		/// Will prepare to handle the settings
		/// </summary>
		/********************************************************************/
		public void InitSettings(Manager agentManager, ModuleHandler moduleHandler, IMainWindowApi mainWindow, ISettings userSettings, ISettings windowSettings)
		{
			pathSettings = new PathSettings(userSettings);
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
			startScanTextBox.Text = pathSettings.StartScan;
			moduleTextBox.Text = pathSettings.Modules;
			listTextBox.Text = pathSettings.ModuleList;
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
			pathSettings.StartScan = startScanTextBox.Text;
			pathSettings.Modules = moduleTextBox.Text;
			pathSettings.ModuleList = listTextBox.Text;
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
		/// Is called when the user clicked the start scan button
		/// </summary>
		/********************************************************************/
		private void StartScanButton_Click(object sender, EventArgs e)
		{
			string newDirectory = SelectDirectory(startScanTextBox.Text);
			if (!string.IsNullOrEmpty(newDirectory))
				startScanTextBox.Text = newDirectory;
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user clicked the module button
		/// </summary>
		/********************************************************************/
		private void ModuleButton_Click(object sender, EventArgs e)
		{
			string newDirectory = SelectDirectory(moduleTextBox.Text);
			if (!string.IsNullOrEmpty(newDirectory))
				moduleTextBox.Text = newDirectory;
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user clicked the list module button
		/// </summary>
		/********************************************************************/
		private void ListButton_Click(object sender, EventArgs e)
		{
			string newDirectory = SelectDirectory(listTextBox.Text);
			if (!string.IsNullOrEmpty(newDirectory))
				listTextBox.Text = newDirectory;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Show a directory chooser and return the path. Null if no path
		/// has been selected
		/// </summary>
		/********************************************************************/
		private string SelectDirectory(string startDirectory)
		{
			using (FolderBrowserDialog dialog = new FolderBrowserDialog())
			{
				dialog.SelectedPath = startDirectory;

				if (dialog.ShowDialog() == DialogResult.OK)
					return dialog.SelectedPath;
			}

			return null;
		}
		#endregion
	}
}
