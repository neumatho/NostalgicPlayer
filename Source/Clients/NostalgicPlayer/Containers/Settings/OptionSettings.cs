/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings
{
	/// <summary>
	/// This class holds all the option settings
	/// </summary>
	public class OptionSettings
	{
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
		/// Show full path as tool tip in module list
		/// </summary>
		/********************************************************************/
		public bool ShowFullPath
		{
			get => settings.GetBoolEntry("Options", "ShowFullPath", false);

			set => settings.SetBoolEntry("Options", "ShowFullPath", value);
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
		/// Remove unknown modules from list
		/// </summary>
		/********************************************************************/
		public bool RemoveUnknownModules
		{
			get => settings.GetBoolEntry("Options", "RemoveUnknownModules", false);

			set => settings.SetBoolEntry("Options", "RemoveUnknownModules", value);
		}



		/********************************************************************/
		/// <summary>
		/// Extract playing time from module
		/// </summary>
		/********************************************************************/
		public bool ExtractPlayingTime
		{
			get => settings.GetBoolEntry("Options", "ExtractPlayingTime", false);

			set => settings.SetBoolEntry("Options", "ExtractPlayingTime", value);
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
	}
}
