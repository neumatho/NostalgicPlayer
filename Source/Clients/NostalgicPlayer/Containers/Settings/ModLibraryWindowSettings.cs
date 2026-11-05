/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings
{
	/// <summary>
	/// This class holds the ModLibrary-specific settings
	/// </summary>
	public class ModLibraryWindowSettings
	{
		private readonly ISettings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModLibraryWindowSettings(ISettings windowSettings)
		{
			settings = windowSettings;

			// Upgrade settings
			LastOnlinePath = LastOnlinePath.Replace("://", ":/");

			if (settings.ContainsEntry("ModLibrary", "FlatViewEnabled"))
			{
				bool flatView = settings.GetBoolEntry("ModLibrary", "FlatViewEnabled");
				OnlineFlatViewEnabled = flatView;
				OfflineFlatViewEnabled = flatView;

				settings.RemoveEntry("ModLibrary", "FlatViewEnabled");
			}

			if (settings.ContainsEntry("ModLibrary", "FlatViewSortOrder"))
			{
				FlatViewSortOrder sortOrder = (FlatViewSortOrder)settings.GetIntEntry("ModLibrary", "FlatViewSortOrder");
				OnlineFlatViewSortOrder = sortOrder;
				OfflineFlatViewSortOrder = sortOrder;

				settings.RemoveEntry("ModLibrary", "FlatViewSortOrder");
			}

			if (settings.ContainsEntry("ModLibrary", "ColumnWidthName"))
			{
				int width = settings.GetIntEntry("ModLibrary", "ColumnWidthName");
				OnlineColumnWidthName = width;
				OfflineColumnWidthName = width;

				settings.RemoveEntry("ModLibrary", "ColumnWidthName");
			}

			if (settings.ContainsEntry("ModLibrary", "ColumnWidthPath"))
			{
				int width = settings.GetIntEntry("ModLibrary", "ColumnWidthPath");
				OnlineColumnWidthPath = width;
				OfflineColumnWidthPath = width;

				settings.RemoveEntry("ModLibrary", "ColumnWidthPath");
			}

			if (settings.ContainsEntry("ModLibrary", "ColumnWidthSize"))
			{
				int width = settings.GetIntEntry("ModLibrary", "ColumnWidthSize");
				OnlineColumnWidthSize= width;
				OfflineColumnWidthSize = width;

				settings.RemoveEntry("ModLibrary", "ColumnWidthSize");
			}
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

			set => settings.SetBoolEntry("ModLibrary", "IsOfflineMode", value);
		}



		/********************************************************************/
		/// <summary>
		/// Flat view checkbox state for online mode
		/// </summary>
		/********************************************************************/
		public bool OnlineFlatViewEnabled
		{
			get => settings.GetBoolEntry("ModLibrary", "OnlineFlatViewEnabled", false);

			set => settings.SetBoolEntry("ModLibrary", "OnlineFlatViewEnabled", value);
		}



		/********************************************************************/
		/// <summary>
		/// Flat view sort order for online mode
		/// (0=NameThenPath, 1=PathThenName)
		/// </summary>
		/********************************************************************/
		public FlatViewSortOrder OnlineFlatViewSortOrder
		{
			get => (FlatViewSortOrder)settings.GetIntEntry("ModLibrary", "OnlineFlatViewSortOrder", 0);

			set => settings.SetIntEntry("ModLibrary", "OnlineFlatViewSortOrder", (int)value);
		}



		/********************************************************************/
		/// <summary>
		/// Flat view checkbox state for offline mode
		/// </summary>
		/********************************************************************/
		public bool OfflineFlatViewEnabled
		{
			get => settings.GetBoolEntry("ModLibrary", "OfflineFlatViewEnabled", false);

			set => settings.SetBoolEntry("ModLibrary", "OfflineFlatViewEnabled", value);
		}



		/********************************************************************/
		/// <summary>
		/// Flat view sort order for offline mode
		/// (0=NameThenPath, 1=PathThenName)
		/// </summary>
		/********************************************************************/
		public FlatViewSortOrder OfflineFlatViewSortOrder
		{
			get => (FlatViewSortOrder)settings.GetIntEntry("ModLibrary", "OfflineFlatViewSortOrder", 0);

			set => settings.SetIntEntry("ModLibrary", "OfflineFlatViewSortOrder", (int)value);
		}



		/********************************************************************/
		/// <summary>
		/// Favorites only checkbox state
		/// </summary>
		/********************************************************************/
		public bool FavoritesOnlyEnabled
		{
			get => settings.GetBoolEntry("ModLibrary", "FavoritesOnlyEnabled", false);

			set => settings.SetBoolEntry("ModLibrary", "FavoritesOnlyEnabled", value);
		}



		/********************************************************************/
		/// <summary>
		/// Play immediately checkbox state
		/// </summary>
		/********************************************************************/
		public bool PlayImmediately
		{
			get => settings.GetBoolEntry("ModLibrary", "PlayImmediately", true);

			set => settings.SetBoolEntry("ModLibrary", "PlayImmediately", value);
		}



		/********************************************************************/
		/// <summary>
		/// Search text
		/// </summary>
		/********************************************************************/
		public string SearchText
		{
			get => settings.GetStringEntry("ModLibrary", "SearchText", string.Empty);

			set => settings.SetStringEntry("ModLibrary", "SearchText", value);
		}



		/********************************************************************/
		/// <summary>
		/// Search mode combobox index (0=Contains, 1=StartsWith, 2=EndsWith)
		/// </summary>
		/********************************************************************/
		public int SearchMode
		{
			get => settings.GetIntEntry("ModLibrary", "SearchMode", 0);

			set => settings.SetIntEntry("ModLibrary", "SearchMode", value);
		}



		/********************************************************************/
		/// <summary>
		/// Last path in online mode
		/// </summary>
		/********************************************************************/
		public string LastOnlinePath
		{
			get => settings.GetStringEntry("ModLibrary", "LastOnlinePath", string.Empty);

			set => settings.SetStringEntry("ModLibrary", "LastOnlinePath", value);
		}



		/********************************************************************/
		/// <summary>
		/// Last path in offline mode
		/// </summary>
		/********************************************************************/
		public string LastOfflinePath
		{
			get => settings.GetStringEntry("ModLibrary", "LastOfflinePath", string.Empty);

			set => settings.SetStringEntry("ModLibrary", "LastOfflinePath", value);
		}



		/********************************************************************/
		/// <summary>
		/// Check if update warning is ignored for a specific source
		/// </summary>
		/********************************************************************/
		public bool IsUpdateIgnored(string sourceId)
		{
			return settings.GetBoolEntry("ModLibrary", $"IgnoreUpdate_{sourceId}", false);
		}



		/********************************************************************/
		/// <summary>
		/// Set ignore state for update warning of a specific source
		/// </summary>
		/********************************************************************/
		public void SetUpdateIgnored(string sourceId, bool ignored)
		{
			settings.SetBoolEntry("ModLibrary", $"IgnoreUpdate_{sourceId}", ignored);
		}



		/********************************************************************/
		/// <summary>
		/// Column width for Name column in online mode
		/// </summary>
		/********************************************************************/
		public int OnlineColumnWidthName
		{
			get => settings.GetIntEntry("ModLibrary", "OnlineColumnWidthName", 400);

			set => settings.SetIntEntry("ModLibrary", "OnlineColumnWidthName", value);
		}



		/********************************************************************/
		/// <summary>
		/// Column width for Path column in online mode (only used in flat
		/// view)
		/// </summary>
		/********************************************************************/
		public int OnlineColumnWidthPath
		{
			get => settings.GetIntEntry("ModLibrary", "OnlineColumnWidthPath", 300);

			set => settings.SetIntEntry("ModLibrary", "OnlineColumnWidthPath", value);
		}



		/********************************************************************/
		/// <summary>
		/// Column width for Size column in online mode
		/// </summary>
		/********************************************************************/
		public int OnlineColumnWidthSize
		{
			get => settings.GetIntEntry("ModLibrary", "OnlineColumnWidthSize", 100);

			set => settings.SetIntEntry("ModLibrary", "OnlineColumnWidthSize", value);
		}



		/********************************************************************/
		/// <summary>
		/// Column width for Name column in offline mode
		/// </summary>
		/********************************************************************/
		public int OfflineColumnWidthName
		{
			get => settings.GetIntEntry("ModLibrary", "OfflineColumnWidthName", 400);

			set => settings.SetIntEntry("ModLibrary", "OfflineColumnWidthName", value);
		}



		/********************************************************************/
		/// <summary>
		/// Column width for Path column in offline mode (only used in flat
		/// view)
		/// </summary>
		/********************************************************************/
		public int OfflineColumnWidthPath
		{
			get => settings.GetIntEntry("ModLibrary", "OfflineColumnWidthPath", 300);

			set => settings.SetIntEntry("ModLibrary", "OfflineColumnWidthPath", value);
		}



		/********************************************************************/
		/// <summary>
		/// Column width for Size column in offline mode
		/// </summary>
		/********************************************************************/
		public int OfflineColumnWidthSize
		{
			get => settings.GetIntEntry("ModLibrary", "OfflineColumnWidthSize", 100);

			set => settings.SetIntEntry("ModLibrary", "OfflineColumnWidthSize", value);
		}
	}
}
