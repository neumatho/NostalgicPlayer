/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings
{
	/// <summary>
	/// This class holds all the option settings
	/// </summary>
	public class OptionSettings
	{
		/// <summary>
		/// The different actions to take when a module error occur
		/// </summary>
		public enum ModuleErrorAction
		{
			/// <summary></summary>
			ShowError,
			/// <summary></summary>
			SkipFile,
			/// <summary></summary>
			SkipFileAndRemoveFromList,
			/// <summary></summary>
			StopPlaying
		}

		/// <summary>
		/// The different actions to take when reached the end of the list
		/// </summary>
		public enum ModuleListEndAction
		{
			/// <summary></summary>
			Eject,
			/// <summary></summary>
			JumpToStart,
			/// <summary></summary>
			Loop
		}

		private readonly ISettings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public OptionSettings(ISettings userSettings)
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
		/// Show item number in list
		/// </summary>
		/********************************************************************/
		public bool ShowListNumber
		{
			get => settings.GetBoolEntry("Options", "ShowListNumber", false);

			set => settings.SetBoolEntry("Options", "ShowListNumber", value);
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
		/// Separate all windows
		/// </summary>
		/********************************************************************/
		public bool SeparateWindows
		{
			get => settings.GetBoolEntry("Options", "SeparateWindows", true);

			set => settings.SetBoolEntry("Options", "SeparateWindows", value);
		}



		/********************************************************************/
		/// <summary>
		/// Show windows in task bar
		/// </summary>
		/********************************************************************/
		public bool ShowWindowsInTaskBar
		{
			get => settings.GetBoolEntry("Options", "ShowWindowsInTaskBar", false);

			set => settings.SetBoolEntry("Options", "ShowWindowsInTaskBar", value);
		}



		/********************************************************************/
		/// <summary>
		/// Scan added files
		/// </summary>
		/********************************************************************/
		public bool ScanFiles
		{
			get => settings.GetBoolEntry("Options", "ScanFiles", false);

			set => settings.SetBoolEntry("Options", "ScanFiles", value);
		}



		/********************************************************************/
		/// <summary>
		/// Use database to store module information
		/// </summary>
		/********************************************************************/
		public bool UseDatabase
		{
			get => settings.GetBoolEntry("Options", "UseDatabase", false);

			set => settings.SetBoolEntry("Options", "UseDatabase", value);
		}



		/********************************************************************/
		/// <summary>
		/// Last cleanup time
		/// </summary>
		/********************************************************************/
		public long LastCleanupTime
		{
			get => settings.GetLongEntry("Options", "LastCleanupTime", 0);

			set => settings.SetLongEntry("Options", "LastCleanupTime", value);
		}



		/********************************************************************/
		/// <summary>
		/// Double buffering
		/// </summary>
		/********************************************************************/
		public bool DoubleBuffering
		{
			get => settings.GetBoolEntry("Options", "DoubleBuffering", false);

			set => settings.SetBoolEntry("Options", "DoubleBuffering", value);
		}



		/********************************************************************/
		/// <summary>
		/// Early load
		/// </summary>
		/********************************************************************/
		public int DoubleBufferingEarlyLoad
		{
			get => settings.GetIntEntry("Options", "DoubleBufferingEarlyLoad", 2);

			set => settings.SetIntEntry("Options", "DoubleBufferingEarlyLoad", value);
		}



		/********************************************************************/
		/// <summary>
		/// Module error reached
		/// </summary>
		/********************************************************************/
		public ModuleErrorAction ModuleError
		{
			get => settings.GetEnumEntry("Options", "ModuleError", ModuleErrorAction.ShowError);

			set => settings.SetEnumEntry("Options", "ModuleError", value);
		}



		/********************************************************************/
		/// <summary>
		/// Never ending
		/// </summary>
		/********************************************************************/
		public bool NeverEnding
		{
			get => settings.GetBoolEntry("Options", "NeverEnding", false);

			set => settings.SetBoolEntry("Options", "NeverEnding", value);
		}



		/********************************************************************/
		/// <summary>
		/// Never ending timeout
		/// </summary>
		/********************************************************************/
		public int NeverEndingTimeout
		{
			get => settings.GetIntEntry("Options", "NeverEndingTimeout", 180);

			set => settings.SetIntEntry("Options", "NeverEndingTimeout", value);
		}



		/********************************************************************/
		/// <summary>
		/// Module list end reached
		/// </summary>
		/********************************************************************/
		public ModuleListEndAction ModuleListEnd
		{
			get => settings.GetEnumEntry("Options", "ModuleListEnd", ModuleListEndAction.JumpToStart);

			set => settings.SetEnumEntry("Options", "ModuleListEnd", value);
		}
	}
}
