/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Windows.Forms;
using Krypton.Navigator;
using Krypton.Toolkit;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Modules;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow.Pages
{
	/// <summary>
	/// Holds all the controls for the Agents tab
	/// </summary>
	public partial class AgentsPageControl : UserControl, ISettingsPage
	{
		private SettingsAgentsWindowSettings winSettings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AgentsPageControl()
		{
			InitializeComponent();

			// Some controls need to be initialized here, since the
			// designer remove the properties
			navigator.Panel.PanelBackStyle = PaletteBackStyle.TabLowProfile;
			navigator.Button.CloseButtonDisplay = ButtonDisplay.Hide;
			navigator.Button.ContextButtonDisplay = ButtonDisplay.Hide;

			// Set the string resources on each string per tab
			navigator.Pages[0].Text = Resources.IDS_SETTINGS_AGENTS_TAB_FORMATS;
			navigator.Pages[1].Text = Resources.IDS_SETTINGS_AGENTS_TAB_OUTPUT;
			navigator.Pages[2].Text = Resources.IDS_SETTINGS_AGENTS_TAB_SAMPLECONVERTERS;
			navigator.Pages[3].Text = Resources.IDS_SETTINGS_AGENTS_TAB_VISUALS;
		}

		#region ISettingsPage implementation
		/********************************************************************/
		/// <summary>
		/// Will prepare to handle the settings
		/// </summary>
		/********************************************************************/
		public void InitSettings(Manager agentManager, ModuleHandler moduleHandler, MainWindowForm mainWindow, Settings userSettings, Settings windowSettings)
		{
			winSettings = new SettingsAgentsWindowSettings(windowSettings, null);

			playersListControl.InitSettings(agentManager, moduleHandler, mainWindow, userSettings, windowSettings, Manager.AgentType.Players);
			outputListControl.InitSettings(agentManager, moduleHandler, mainWindow, userSettings, windowSettings, Manager.AgentType.Output);
			sampleConvertersListControl.InitSettings(agentManager, moduleHandler, mainWindow, userSettings, windowSettings, Manager.AgentType.SampleConverters);
			visualsListControl.InitSettings(agentManager, moduleHandler, mainWindow, userSettings, windowSettings, Manager.AgentType.Visuals);
		}



		/********************************************************************/
		/// <summary>
		/// Will make a backup of settings that can be changed in real-time
		/// </summary>
		/********************************************************************/
		public void MakeBackup()
		{
			playersListControl.MakeBackup();
			outputListControl.MakeBackup();
			sampleConvertersListControl.MakeBackup();
			visualsListControl.MakeBackup();
		}



		/********************************************************************/
		/// <summary>
		/// Will read the settings and set all the controls
		/// </summary>
		/********************************************************************/
		public void ReadSettings()
		{
			playersListControl.ReadSettings();
			outputListControl.ReadSettings();
			sampleConvertersListControl.ReadSettings();
			visualsListControl.ReadSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Will read the window settings
		/// </summary>
		/********************************************************************/
		public void ReadWindowSettings()
		{
			navigator.SelectedIndex = winSettings.ActiveTab;

			playersListControl.ReadWindowSettings();
			outputListControl.ReadWindowSettings();
			sampleConvertersListControl.ReadWindowSettings();
			visualsListControl.ReadWindowSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Will read the data from the controls and store them in the
		/// settings
		/// </summary>
		/********************************************************************/
		public void WriteSettings()
		{
			playersListControl.WriteSettings();
			outputListControl.WriteSettings();
			sampleConvertersListControl.WriteSettings();
			visualsListControl.WriteSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Will store window specific settings
		/// </summary>
		/********************************************************************/
		public void WriteWindowSettings()
		{
			winSettings.ActiveTab = navigator.SelectedIndex;

			playersListControl.WriteWindowSettings();
			outputListControl.WriteWindowSettings();
			sampleConvertersListControl.WriteWindowSettings();
			visualsListControl.WriteWindowSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Will restore real-time values
		/// </summary>
		/********************************************************************/
		public void CancelSettings()
		{
			playersListControl.CancelSettings();
			outputListControl.CancelSettings();
			sampleConvertersListControl.CancelSettings();
			visualsListControl.CancelSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Will refresh the page when a module is loaded/ejected
		/// </summary>
		/********************************************************************/
		public void RefreshWindow()
		{
			playersListControl.RefreshWindow();
			outputListControl.RefreshWindow();
			sampleConvertersListControl.RefreshWindow();
			visualsListControl.RefreshWindow();
		}
		#endregion
	}
}
