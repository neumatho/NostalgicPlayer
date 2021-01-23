/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings
{
	/// <summary>
	/// This class holds all the settings window settings
	/// </summary>
	public class SettingsWindowSettings
	{
		private readonly Kit.Utility.Settings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SettingsWindowSettings(Kit.Utility.Settings windowSettings)
		{
			settings = windowSettings;
		}



		/********************************************************************/
		/// <summary>
		/// Active tab
		/// </summary>
		/********************************************************************/
		public int ActiveTab
		{
			get => settings.GetIntEntry("Window", "ActiveTab", 0);

			set => settings.SetIntEntry("Window", "ActiveTab", value);
		}
	}
}
