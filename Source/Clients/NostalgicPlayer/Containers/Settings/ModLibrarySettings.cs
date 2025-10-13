/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Helpers;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings
{
	/// <summary>
	/// This class holds the ModLibrary-specific settings
	/// </summary>
	public class ModLibrarySettings
	{
		private readonly ISettings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModLibrarySettings(ISettings windowSettings)
		{
			settings = windowSettings;
		}



		/********************************************************************/
		/// <summary>
		/// Return the main settings object
		/// </summary>
		/********************************************************************/
		public ISettings Settings => settings;



		/********************************************************************/
		/// <summary>
		/// Is offline mode (true = offline tab, false = online tab)
		/// </summary>
		/********************************************************************/
		public bool IsOfflineMode
		{
			get => settings.GetBoolEntry("ModLibrary", "IsOfflineMode", false);
			set
			{
				settings.SetBoolEntry("ModLibrary", "IsOfflineMode", value);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Flat view checkbox state
		/// </summary>
		/********************************************************************/
		public bool FlatViewEnabled
		{
			get => settings.GetBoolEntry("ModLibrary", "FlatViewEnabled", false);
			set
			{
				settings.SetBoolEntry("ModLibrary", "FlatViewEnabled", value);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Play immediately checkbox state
		/// </summary>
		/********************************************************************/
		public bool PlayImmediately
		{
			get => settings.GetBoolEntry("ModLibrary", "PlayImmediately", true);
			set
			{
				settings.SetBoolEntry("ModLibrary", "PlayImmediately", value);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Search text
		/// </summary>
		/********************************************************************/
		public string SearchText
		{
			get => settings.GetStringEntry("ModLibrary", "SearchText", "");
			set
			{
				settings.SetStringEntry("ModLibrary", "SearchText", value);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Search mode combobox index (0=Contains, 1=StartsWith, 2=EndsWith)
		/// </summary>
		/********************************************************************/
		public int SearchMode
		{
			get => settings.GetIntEntry("ModLibrary", "SearchMode", 0);
			set
			{
				settings.SetIntEntry("ModLibrary", "SearchMode", value);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Flat view sort order (0=NameThenPath, 1=PathThenName)
		/// </summary>
		/********************************************************************/
		public int FlatViewSortOrder
		{
			get => settings.GetIntEntry("ModLibrary", "FlatViewSortOrder", 0);
			set
			{
				settings.SetIntEntry("ModLibrary", "FlatViewSortOrder", value);
			}
		}
	}
}
