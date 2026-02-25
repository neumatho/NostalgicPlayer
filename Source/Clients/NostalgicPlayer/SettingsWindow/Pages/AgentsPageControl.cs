/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Krypton.Navigator;
using Krypton.Toolkit;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Modules;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Services;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow.Pages
{
	/// <summary>
	/// Holds all the controls for the Agents tab
	/// </summary>
	public partial class AgentsPageControl : UserControl, ISettingsPage
	{
		private SettingsAgentsWindowSettings winSettings;

		private HashSet<Guid> changedEnableStates;

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
			navigator.Pages[1].Text = Resources.IDS_SETTINGS_AGENTS_TAB_PLAYERS;
			navigator.Pages[2].Text = Resources.IDS_SETTINGS_AGENTS_TAB_OUTPUT;
			navigator.Pages[3].Text = Resources.IDS_SETTINGS_AGENTS_TAB_SAMPLECONVERTERS;
			navigator.Pages[4].Text = Resources.IDS_SETTINGS_AGENTS_TAB_VISUALS;
			navigator.Pages[5].Text = Resources.IDS_SETTINGS_AGENTS_TAB_DECRUNCHERS;
		}

		#region ISettingsPage implementation
		/********************************************************************/
		/// <summary>
		/// Will prepare to handle the settings
		/// </summary>
		/********************************************************************/
		public void InitSettings(ModuleHandler moduleHandler, SettingsService settingsService, ISettings windowSettings)
		{
			ISettings userSettings = settingsService.Settings;

			changedEnableStates = new HashSet<Guid>();

			winSettings = new SettingsAgentsWindowSettings(windowSettings, null);

			formatsListControl.InitSettings(moduleHandler, userSettings, windowSettings, "Formats", changedEnableStates, playersListControl);
			playersListControl.InitSettings(moduleHandler, userSettings, windowSettings, "Players", changedEnableStates, formatsListControl);
			outputListControl.InitSettings(moduleHandler, userSettings, windowSettings, "Output", changedEnableStates);
			sampleConvertersListControl.InitSettings(moduleHandler, userSettings, windowSettings, "SampleConverters", changedEnableStates);
			visualsListControl.InitSettings(moduleHandler, userSettings, windowSettings, "Visuals", changedEnableStates);
			decrunchersListUserControl.InitSettings(moduleHandler, userSettings, windowSettings, "Decrunchers", changedEnableStates);
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
			formatsListControl.ReadSettings();
			playersListControl.ReadSettings();
			outputListControl.ReadSettings();
			sampleConvertersListControl.ReadSettings();
			visualsListControl.ReadSettings();
			decrunchersListUserControl.ReadSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Will read the window settings
		/// </summary>
		/********************************************************************/
		public void ReadWindowSettings()
		{
			navigator.SelectedIndex = winSettings.ActiveTab;

			formatsListControl.ReadWindowSettings();
			playersListControl.ReadWindowSettings();
			outputListControl.ReadWindowSettings();
			sampleConvertersListControl.ReadWindowSettings();
			visualsListControl.ReadWindowSettings();
			decrunchersListUserControl.ReadWindowSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Will read the data from the controls and store them in the
		/// settings
		/// </summary>
		/********************************************************************/
		public void WriteSettings()
		{
			formatsListControl.WriteSettings();
			playersListControl.WriteSettings();
			outputListControl.WriteSettings();
			sampleConvertersListControl.WriteSettings();
			visualsListControl.WriteSettings();
			decrunchersListUserControl.WriteSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Will store window specific settings
		/// </summary>
		/********************************************************************/
		public void WriteWindowSettings()
		{
			winSettings.ActiveTab = navigator.SelectedIndex;

			formatsListControl.WriteWindowSettings();
			playersListControl.WriteWindowSettings();
			outputListControl.WriteWindowSettings();
			sampleConvertersListControl.WriteWindowSettings();
			visualsListControl.WriteWindowSettings();
			decrunchersListUserControl.WriteWindowSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Will restore real-time values
		/// </summary>
		/********************************************************************/
		public void CancelSettings()
		{
			formatsListControl.CancelSettings();
			playersListControl.CancelSettings();
			outputListControl.CancelSettings();
			sampleConvertersListControl.CancelSettings();
			visualsListControl.CancelSettings();
			decrunchersListUserControl.CancelSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Will refresh the page when a module is loaded/ejected
		/// </summary>
		/********************************************************************/
		public void RefreshWindow()
		{
			formatsListControl.RefreshWindow();
			playersListControl.RefreshWindow();
			outputListControl.RefreshWindow();
			sampleConvertersListControl.RefreshWindow();
			visualsListControl.RefreshWindow();
			decrunchersListUserControl.RefreshWindow();
		}
		#endregion
	}
}
