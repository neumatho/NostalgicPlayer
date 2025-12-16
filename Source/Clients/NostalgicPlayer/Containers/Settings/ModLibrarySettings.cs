/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
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
		/// Favorites only checkbox state
		/// </summary>
		/********************************************************************/
		public bool FavoritesOnlyEnabled
		{
			get => settings.GetBoolEntry("ModLibrary", "FavoritesOnlyEnabled", false);
			set
			{
				settings.SetBoolEntry("ModLibrary", "FavoritesOnlyEnabled", value);
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
			get => settings.GetStringEntry("ModLibrary", "SearchText", string.Empty);
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



		/********************************************************************/
		/// <summary>
		/// Last path in online mode
		/// </summary>
		/********************************************************************/
		public string LastOnlinePath
		{
			get => settings.GetStringEntry("ModLibrary", "LastOnlinePath", string.Empty);
			set
			{
				settings.SetStringEntry("ModLibrary", "LastOnlinePath", value);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Last path in offline mode
		/// </summary>
		/********************************************************************/
		public string LastOfflinePath
		{
			get => settings.GetStringEntry("ModLibrary", "LastOfflinePath", string.Empty);
			set
			{
				settings.SetStringEntry("ModLibrary", "LastOfflinePath", value);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Check if update warning is ignored for a specific service
		/// </summary>
		/********************************************************************/
		public bool IsUpdateIgnored(string serviceId)
		{
			return settings.GetBoolEntry("ModLibrary", $"IgnoreUpdate_{serviceId}", false);
		}



		/********************************************************************/
		/// <summary>
		/// Set ignore state for update warning of a specific service
		/// </summary>
		/********************************************************************/
		public void SetUpdateIgnored(string serviceId, bool ignored)
		{
			settings.SetBoolEntry("ModLibrary", $"IgnoreUpdate_{serviceId}", ignored);
		}



		/********************************************************************/
		/// <summary>
		/// Column width for Name column
		/// </summary>
		/********************************************************************/
		public int ColumnWidthName
		{
			get => settings.GetIntEntry("ModLibrary", "ColumnWidthName", 400);
			set => settings.SetIntEntry("ModLibrary", "ColumnWidthName", value);
		}



		/********************************************************************/
		/// <summary>
		/// Column width for Path column (only used in flat view)
		/// </summary>
		/********************************************************************/
		public int ColumnWidthPath
		{
			get => settings.GetIntEntry("ModLibrary", "ColumnWidthPath", 300);
			set => settings.SetIntEntry("ModLibrary", "ColumnWidthPath", value);
		}



		/********************************************************************/
		/// <summary>
		/// Column width for Size column
		/// </summary>
		/********************************************************************/
		public int ColumnWidthSize
		{
			get => settings.GetIntEntry("ModLibrary", "ColumnWidthSize", 100);
			set => settings.SetIntEntry("ModLibrary", "ColumnWidthSize", value);
		}
	}
}
