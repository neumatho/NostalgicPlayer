/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Client.GuiPlayer.Modules;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow.Pages
{
	/// <summary>
	/// Interface all settings pages need to implement
	/// </summary>
	public interface ISettingsPage
	{
		/// <summary>
		/// Will make a backup of settings that can be changed in real-time
		/// </summary>
		void MakeBackup();

		/// <summary>
		/// Will read the settings and set all the controls
		/// </summary>
		void InitSettings(Manager agentManager, ModuleHandler moduleHandler, Settings userSettings);

		/// <summary>
		/// Will read the window settings
		/// </summary>
		void InitWindowSettings(Settings windowSettings);

		/// <summary>
		/// Will read the data from the controls and store them in the
		/// settings
		/// </summary>
		void RememberSettings();

		/// <summary>
		/// Will store window specific settings
		/// </summary>
		void RememberWindowSettings();

		/// <summary>
		/// Will restore real-time values
		/// </summary>
		void CancelSettings();

		/// <summary>
		/// Will refresh the page when a module is loaded/ejected
		/// </summary>
		void RefreshWindow();
	}
}
