/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
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
		private class ModuleInfoOrderListBoxItem
		{
			public string Name { get; set; }
			public ModuleSettings.ModuleInfoTab Tab { get; set; }

			public override string ToString()
			{
				return Name;
			}
		}

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
			moduleErrorComboBox.Items.AddRange(
			[
				Resources.IDS_SETTINGS_MODULES_LOADING_MODULEERROR_SHOW,
				Resources.IDS_SETTINGS_MODULES_LOADING_MODULEERROR_SKIP,
				Resources.IDS_SETTINGS_MODULES_LOADING_MODULEERROR_SKIPREMOVE,
				Resources.IDS_SETTINGS_MODULES_LOADING_MODULEERROR_STOP
			]);

			moduleEndComboBox.Items.AddRange(
			[
				Resources.IDS_SETTINGS_MODULES_PLAYING_MODULEEND_MODULE,
				Resources.IDS_SETTINGS_MODULES_PLAYING_MODULEEND_SUBSONG
			]);

			moduleListEndComboBox.Items.AddRange(
			[
				Resources.IDS_SETTINGS_MODULES_PLAYING_MODULELISTEND_EJECT,
				Resources.IDS_SETTINGS_MODULES_PLAYING_MODULELISTEND_JUMPTOSTART,
				Resources.IDS_SETTINGS_MODULES_PLAYING_MODULELISTEND_LOOP
			]);
		}

		#region ISettingsPage implementation
		/********************************************************************/
		/// <summary>
		/// Will prepare to handle the settings
		/// </summary>
		/********************************************************************/
		public void InitSettings(Manager agentManager, ModuleHandler moduleHandler, IMainWindowApi mainWindow, ISettings userSettings, ISettings windowSettings)
		{
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

			moduleEndComboBox.SelectedIndex = (int)moduleSettings.ModuleEnd;
			moduleListEndComboBox.SelectedIndex = (int)moduleSettings.ModuleListEnd;

			// Showing
			Dictionary<ModuleSettings.ModuleInfoTab, string> moduleInfoTabs = new Dictionary<ModuleSettings.ModuleInfoTab, string>
			{
				{ ModuleSettings.ModuleInfoTab.Info, Resources.IDS_SETTINGS_MODULES_SHOWING_MODULEINFO_TAB_INFO },
				{ ModuleSettings.ModuleInfoTab.Comments, Resources.IDS_SETTINGS_MODULES_SHOWING_MODULEINFO_TAB_COMMENTS },
				{ ModuleSettings.ModuleInfoTab.Lyrics, Resources.IDS_SETTINGS_MODULES_SHOWING_MODULEINFO_TAB_LYRICS },
				{ ModuleSettings.ModuleInfoTab.Pictures, Resources.IDS_SETTINGS_MODULES_SHOWING_MODULEINFO_TAB_PICTURES }
			};

			moduleInfoOrderListBox.Items.Clear();

			foreach (ModuleSettings.ModuleInfoTab tab in moduleSettings.ModuleInfoActivateTabOrder)
			{
				if (moduleInfoTabs.TryGetValue(tab, out string name))
				{
					moduleInfoOrderListBox.Items.Add(new ModuleInfoOrderListBoxItem
					{
						Name = name,
						Tab = tab
					});

					moduleInfoTabs.Remove(tab);
				}
			}

			// If any items left in the dictionary, it means that new tabs has been added
			foreach (KeyValuePair<ModuleSettings.ModuleInfoTab, string> pair in moduleInfoTabs)
			{
				moduleInfoOrderListBox.Items.Insert(0, new ModuleInfoOrderListBoxItem
				{
					Name = pair.Value,
					Tab = pair.Key
				});
			}
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

			moduleSettings.ModuleEnd = (ModuleSettings.ModuleEndAction)moduleEndComboBox.SelectedIndex;
			moduleSettings.ModuleListEnd = (ModuleSettings.ModuleListEndAction)moduleListEndComboBox.SelectedIndex;

			// Showing
			moduleSettings.ModuleInfoActivateTabOrder = moduleInfoOrderListBox.Items.Cast<ModuleInfoOrderListBoxItem>().Select(x => x.Tab).ToArray();
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



		/********************************************************************/
		/// <summary>
		/// Is called when the user selects an item in the module info order
		/// list box
		/// </summary>
		/********************************************************************/
		private void ModuleInfoOrderListBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			int selectedIndex = moduleInfoOrderListBox.SelectedIndex;
			if (selectedIndex == -1)
			{
				moduleInfoOrderUpButton.Enabled = false;
				moduleInfoOrderDownButton.Enabled = false;
			}
			else
			{
				moduleInfoOrderUpButton.Enabled = selectedIndex > 0;
				moduleInfoOrderDownButton.Enabled = selectedIndex < (moduleInfoOrderListBox.Items.Count - 1);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user clicks on the module info order up button
		/// </summary>
		/********************************************************************/
		private void ModuleInfoOrderUpButton_Click(object sender, EventArgs e)
		{
			int selectedIndex = moduleInfoOrderListBox.SelectedIndex;

			moduleInfoOrderListBox.Items.Insert(selectedIndex - 1, moduleInfoOrderListBox.Items[selectedIndex]);
			moduleInfoOrderListBox.Items.RemoveAt(selectedIndex + 1);

			moduleInfoOrderListBox.SelectedIndex = selectedIndex - 1;
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user clicks on the module info order down
		/// button
		/// </summary>
		/********************************************************************/
		private void ModuleInfoOrderDownButton_Click(object sender, EventArgs e)
		{
			int selectedIndex = moduleInfoOrderListBox.SelectedIndex;

			moduleInfoOrderListBox.Items.Insert(selectedIndex + 2, moduleInfoOrderListBox.Items[selectedIndex]);
			moduleInfoOrderListBox.Items.RemoveAt(selectedIndex);

			moduleInfoOrderListBox.SelectedIndex = selectedIndex + 1;
		}
		#endregion
	}
}
