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
	/// This class holds all the option settings
	/// </summary>
	public class OptionSettings
	{
		private readonly Kit.Utility.Settings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public OptionSettings(Kit.Utility.Settings userSettings)
		{
			settings = userSettings;
		}



		/********************************************************************/
		/// <summary>
		/// Jump to added module
		/// </summary>
		/********************************************************************/
		public bool AddJump
		{
			get => settings.GetBoolEntry("Options", "AddJump", false);

			set => settings.SetBoolEntry("Options", "AddJump", value);
		}



		/********************************************************************/
		/// <summary>
		/// Add to list as default
		/// </summary>
		/********************************************************************/
		public bool AddToList
		{
			get => settings.GetBoolEntry("Options", "AddToList", true);

			set => settings.SetBoolEntry("Options", "AddToList", value);
		}



		/********************************************************************/
		/// <summary>
		/// Remember list on exit
		/// </summary>
		/********************************************************************/
		public bool RememberList
		{
			get => settings.GetBoolEntry("Options", "RememberList", false);

			set => settings.SetBoolEntry("Options", "RememberList", value);
		}



		/********************************************************************/
		/// <summary>
		/// Remember playing module
		/// </summary>
		/********************************************************************/
		public bool RememberListPosition
		{
			get => settings.GetBoolEntry("Options", "RememberListPosition", true);

			set => settings.SetBoolEntry("Options", "RememberListPosition", value);
		}



		/********************************************************************/
		/// <summary>
		/// Remember module position
		/// </summary>
		/********************************************************************/
		public bool RememberModulePosition
		{
			get => settings.GetBoolEntry("Options", "RememberModulePosition", false);

			set => settings.SetBoolEntry("Options", "RememberModulePosition", value);
		}



		/********************************************************************/
		/// <summary>
		/// Tool tips
		/// </summary>
		/********************************************************************/
		public bool ToolTips
		{
			get => settings.GetBoolEntry("Options", "ToolTips", true);

			set => settings.SetBoolEntry("Options", "ToolTips", value);
		}



		/********************************************************************/
		/// <summary>
		/// Show module name in titlebar
		/// </summary>
		/********************************************************************/
		public bool ShowNameInTitle
		{
			get => settings.GetBoolEntry("Options", "ShowNameInTitle", false);

			set => settings.SetBoolEntry("Options", "ShowNameInTitle", value);
		}



		/********************************************************************/
		/// <summary>
		/// Show item number in list
		/// </summary>
		/********************************************************************/
		public bool ShowListNumber
		{
			get => settings.GetBoolEntry("Options", "ShowListNumber", false);

			set => settings.SetBoolEntry("Options", "ShowListNumber", value);
		}
	}
}
