/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.MainWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Data;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Events;
using Polycode.NostalgicPlayer.Controls;
using Polycode.NostalgicPlayer.Controls.Components;
using Polycode.NostalgicPlayer.Controls.Events;
using Polycode.NostalgicPlayer.Controls.Menus;
using Polycode.NostalgicPlayer.External;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Logic.Containers;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Pages
{
	/// <summary>
	/// Holds all the controls for the offline tab
	/// </summary>
	public partial class OfflinePageControl : UserControl, IDependencyInjectionControl, IModLibraryPage
	{
		private IModLibraryHelper modLibraryHelper;

		private IApplicationContext applicationContext;
		private IMainWindowApi mainWindowApi;
		private IModLibraryWindowApi modLibraryWindowApi;

		private NostalgicToolStripMenuItem playModulesItem;
		private NostalgicToolStripMenuItem addToPlayListItem;
		private NostalgicToolStripSeparator separator1Item;
		private NostalgicToolStripMenuItem addToFavoritesItem;
		private NostalgicToolStripMenuItem removeFromFavoritesItem;
		private NostalgicToolStripSeparator separator2Item;
		private NostalgicToolStripMenuItem reloadItem;
		private NostalgicToolStripMenuItem deleteItem;

		private OfflineData data;
		private readonly TaskHelper taskHelper = new TaskHelper();
		private ModLibraryPlaylistIntegration playlistIntegration;

		private string lastSearchText;
		private SearchMode lastSearchMode;
		private bool playImmediately;

		private bool initializeRunning;
		private bool hasBeenLoaded;
		private bool needsRefresh;
		private bool needsReload;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public OfflinePageControl()
		{
			InitializeComponent();
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the control
		///
		/// Called from FormCreatorService
		/// </summary>
		/********************************************************************/
		public void InitializeControl(IApplicationContext applicationContext, IMainWindowApi mainWindowApi, IModLibraryHelper modLibraryHelper)
		{
			this.applicationContext = applicationContext;
			this.mainWindowApi = mainWindowApi;
			this.modLibraryHelper = modLibraryHelper;
		}



		/********************************************************************/
		/// <summary>
		/// Will add a newly downloaded module
		/// </summary>
		/********************************************************************/
		public void AddDownloadedModule(string modulePath)
		{
			if (!hasBeenLoaded)
				return;

			data.AddFile(modulePath);
			needsRefresh = true;

			if (Visible)
				refreshTimer.Start();
		}



		/********************************************************************/
		/// <summary>
		/// Tell the page that it needs to reload all its data the next time
		/// it is shown, e.g. because the modules directory has changed
		/// </summary>
		/********************************************************************/
		public void MarkForReload()
		{
			if (hasBeenLoaded)
				needsReload = true;
		}

		#region IModLibraryPage implementation
		/********************************************************************/
		/// <summary>
		/// Initialize the page
		/// </summary>
		/********************************************************************/
		public void Initialize(IModLibraryWindowApi modLibraryWindow, ModLibraryWindowSettings settings)
		{
			modLibraryWindowApi = modLibraryWindow;
			playImmediately = settings.PlayImmediately;

			data = new OfflineData(applicationContext);
			data.DataLoaded += DataLoaded;

			playlistIntegration = new ModLibraryPlaylistIntegration(mainWindowApi);

			CreateContextMenu();

			InitializeControls(settings);

			hasBeenLoaded = false;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup used resources
		/// </summary>
		/********************************************************************/
		public void Cleanup(ModLibraryWindowSettings settings)
		{
			using (new SleepCursor())
			{
				taskHelper.CancelTask();

				listControl.ClearItems();

				StoreSettings(settings);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will load the page with data
		/// </summary>
		/********************************************************************/
		public void LoadPage(string breadcrumbPath, string searchText, SearchMode searchMode)
		{
			if (hasBeenLoaded && needsReload)
			{
				needsReload = false;
				needsRefresh = false;

				LoadDatabase(breadcrumbControl.GetPath(), searchText, searchMode);
				return;
			}

			if (hasBeenLoaded)
			{
				Search(searchText, searchMode, needsRefresh);
				needsRefresh = false;
				return;
			}

			LoadDatabase(breadcrumbPath, searchText, searchMode);
		}



		/********************************************************************/
		/// <summary>
		/// Will reload the page with data
		/// </summary>
		/********************************************************************/
		public void ReloadPage()
		{
			LoadDatabase(breadcrumbControl.GetPath(), lastSearchText, lastSearchMode);
		}



		/********************************************************************/
		/// <summary>
		/// Do a new search and update the list
		/// </summary>
		/********************************************************************/
		public void Search(string searchText, SearchMode searchMode, bool force = false)
		{
			if (force || (lastSearchText == null) || (searchText != lastSearchText) || (searchMode != lastSearchMode))
			{
				lastSearchText = searchText;
				lastSearchMode = searchMode;

				listControl.SetLoading(true, Resources.IDS_MODLIBRARY_STATUS_SEARCHING);
				listControl.ClearItems();
				breadcrumbControl.ClearCounts();

				data.BuildTree(searchText, searchMode);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Set the play immediately state
		/// </summary>
		/********************************************************************/
		public void SetPlayImmediately(bool enabled)
		{
			playImmediately = enabled;
		}
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// Called when search/data operation is completed
		/// </summary>
		/********************************************************************/
		private void DataLoaded(object sender, EventArgs e)
		{
			listControl.SetLoading(false);

			// Do not move the focus to the list, since the user may
			// still be typing in the search field
			LoadCurrentDirectory(setFocus: false);
		}



		/********************************************************************/
		/// <summary>
		/// Called when the breadcrumb path changes
		/// </summary>
		/********************************************************************/
		private void Breadcrumb_PathChanged(object sender, BreadcrumbPathChangedEventArgs e)
		{
			string itemToSelect = null;

			if (e.NewItems.Count < e.OldItems.Count)
				itemToSelect = e.OldItems[e.NewItems.Count].Text;

			LoadCurrentDirectory(itemToSelect);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when a list item is activated
		/// </summary>
		/********************************************************************/
		private void List_ItemActivate(object sender, ItemActivateEventArgs e)
		{
			TreeNode entry = e.Entry;

			if (entry.IsDirectory)
			{
				// Changing the path will reload the list
				breadcrumbControl.SetPath(entry.FullPath);
			}
			else
				PlayModuleIfExists(entry);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user wants to go to the parent directory
		/// </summary>
		/********************************************************************/
		private void List_NavigateUp(object sender, EventArgs e)
		{
			breadcrumbControl.GotoParent();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user changes the sort order in the list
		/// </summary>
		/********************************************************************/
		private void List_SortOrderChanged(object sender, EventArgs e)
		{
			LoadCurrentDirectory();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when a context menu needs to be opened
		/// </summary>
		/********************************************************************/
		private void List_OpenContextMenu(object sender, OpenContextMenuEventArgs e)
		{
			UpdateContextMenuVisibility();
			contextMenu.Show(listControl, e.Location);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when flat view checkbox is toggled
		/// </summary>
		/********************************************************************/
		private void FlatView_CheckedChanged(object sender, EventArgs e)
		{
			if (initializeRunning)
				return;

			listControl.SetFlatView(flatViewCheckBox.Checked);

			LoadCurrentDirectory();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when favorites only checkbox is toggled
		/// </summary>
		/********************************************************************/
		private void FavoritesOnly_CheckedChanged(object sender, EventArgs e)
		{
			if (initializeRunning)
				return;

			// Reload current directory to apply filter
			LoadCurrentDirectory();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void RefreshTimer_Tick(object sender, EventArgs e)
		{
			refreshTimer.Stop();

			Search(lastSearchText, lastSearchMode, true);
			needsRefresh = false;
		}
		#endregion

		#region Context menu handlers
		/********************************************************************/
		/// <summary>
		/// Is called when "Play Module(s)" context menu item is clicked
		/// </summary>
		/********************************************************************/
		private void PlayItem_Click(object sender, EventArgs e)
		{
			List<string> localPaths = GetSelectedLocalPaths();
			if (localPaths.Count > 0)
				AddFilesToPlaylist(localPaths, true);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when "Add to Playlist" context menu item is clicked
		/// </summary>
		/********************************************************************/
		private void AddToPlaylistItem_Click(object sender, EventArgs e)
		{
			List<string> localPaths = GetSelectedLocalPaths();
			if (localPaths.Count > 0)
				AddFilesToPlaylist(localPaths, false);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when "Add to Favorites" context menu item is clicked
		/// </summary>
		/********************************************************************/
		private void AddToFavoritesItem_Click(object sender, EventArgs e)
		{
			// Add all selected entries to favorites
			foreach (TreeNode entry in listControl.GetSelectedItems())
			{
				if (!data.IsFavorite(entry.FullPath))
					entry.IsFavorite = data.ToggleFavorite(entry.FullPath);
			}

			// Refresh the list to show stars
			listControl.Invalidate(true);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when "Remove from Favorites" context menu item is
		/// clicked
		/// </summary>
		/********************************************************************/
		private void RemoveFromFavoritesItem_Click(object sender, EventArgs e)
		{
			// Remove all selected entries from favorites
			foreach (TreeNode entry in listControl.GetSelectedItems())
			{
				if (data.IsFavorite(entry.FullPath))
					entry.IsFavorite = data.ToggleFavorite(entry.FullPath);
			}

			// Refresh the list to hide stars
			listControl.Invalidate(true);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when "Rescan Local Files" context menu item is clicked
		/// </summary>
		/********************************************************************/
		private void Rescan_Click(object sender, EventArgs e)
		{
			ScanLocalFiles();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when "Delete" context menu item is clicked
		/// </summary>
		/********************************************************************/
		private void DeleteItem_Click(object sender, EventArgs e)
		{
			List<TreeNode> selectedEntries = listControl.GetSelectedItems();

			if (selectedEntries.Count == 0)
				return;

			// Ask for confirmation
			string message;

			if (selectedEntries.Count == 1)
			{
				TreeNode entry = selectedEntries[0];
				message = entry.IsDirectory
					? string.Format(Resources.IDS_MODLIBRARY_MSGBOX_DELETE_FOLDER_CONFIRM, entry.Name)
					: string.Format(Resources.IDS_MODLIBRARY_MSGBOX_DELETE_FILE_CONFIRM, entry.Name);
			}
			else
			{
				int fileCount = selectedEntries.Count(f => !f.IsDirectory);
				int folderCount = selectedEntries.Count(f => f.IsDirectory);

				if ((fileCount > 0) && (folderCount > 0))
					message = string.Format(Resources.IDS_MODLIBRARY_MSGBOX_DELETE_MULTIPLE_CONFIRM, fileCount, folderCount);
				else if (folderCount > 0)
					message = string.Format(Resources.IDS_MODLIBRARY_MSGBOX_DELETE_FOLDERS_CONFIRM, folderCount);
				else
					message = string.Format(Resources.IDS_MODLIBRARY_MSGBOX_DELETE_FILES_CONFIRM, fileCount);
			}

			bool result = mainWindowApi.ShowQuestion(message, modLibraryWindowApi.Form);

			if (result)
			{
				int deletedCount = 0;
				int errorCount = 0;
				string lastError = string.Empty;

				try
				{
					// Delete is only available in offline mode, where all files have FullPath relative to modulesBasePath
					string modulesBasePath = modLibraryHelper.ModulesDirectory;

					foreach (TreeNode entry in selectedEntries)
					{
						try
						{
							// FullPath is the relative path from modulesBasePath (e.g., "Games/file.mod" or "modland/Games/file.mod")
							string localPath = Path.Combine(modulesBasePath, entry.FullPath.Replace('/', Path.DirectorySeparatorChar));

							if (entry.IsDirectory)
							{
								// Delete directory recursively
								if (Directory.Exists(localPath))
								{
									Directory.Delete(localPath, true);
									deletedCount++;
								}
							}
							else
							{
								// Delete file
								if (File.Exists(localPath))
								{
									File.Delete(localPath);
									deletedCount++;
								}
							}
						}
						catch (Exception ex)
						{
							errorCount++;
							lastError = ex.Message;
						}
					}

					// Rescan local files
					ScanLocalFiles();

					// Show result
					if (errorCount > 0)
						mainWindowApi.ShowSimpleErrorMessage(string.Format(Resources.IDS_MODLIBRARY_MSGBOX_DELETE_COMPLETED_WITH_ERRORS, deletedCount, errorCount, lastError), modLibraryWindowApi.Form);
				}
				catch (Exception ex)
				{
					mainWindowApi.ShowSimpleErrorMessage(string.Format(Resources.IDS_MODLIBRARY_MSGBOX_FAILED_DELETE, ex.Message), modLibraryWindowApi.Form);
				}
			}
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Create the context menu
		/// </summary>
		/********************************************************************/
		private void CreateContextMenu()
		{
			playModulesItem = new NostalgicToolStripMenuItem(Resources.IDS_MODLIBRARY_CONTEXT_OFFLINE_PLAY_MODULES);
			playModulesItem.Click += PlayItem_Click;
			contextMenu.Items.Add(playModulesItem);

			addToPlayListItem = new NostalgicToolStripMenuItem(Resources.IDS_MODLIBRARY_CONTEXT_OFFLINE_ADD_TO_PLAYLIST);
			addToPlayListItem.Click += AddToPlaylistItem_Click;
			contextMenu.Items.Add(addToPlayListItem);

			separator1Item = new NostalgicToolStripSeparator();
			contextMenu.Items.Add(separator1Item);

			addToFavoritesItem = new NostalgicToolStripMenuItem(Resources.IDS_MODLIBRARY_CONTEXT_OFFLINE_ADD_TO_FAVORITES);
			addToFavoritesItem.Click += AddToFavoritesItem_Click;
			contextMenu.Items.Add(addToFavoritesItem);

			removeFromFavoritesItem = new NostalgicToolStripMenuItem(Resources.IDS_MODLIBRARY_CONTEXT_OFFLINE_REMOVE_FROM_FAVORITES);
			removeFromFavoritesItem.Click += RemoveFromFavoritesItem_Click;
			contextMenu.Items.Add(removeFromFavoritesItem);

			separator2Item = new NostalgicToolStripSeparator();
			contextMenu.Items.Add(separator2Item);

			reloadItem = new NostalgicToolStripMenuItem(Resources.IDS_MODLIBRARY_CONTEXT_OFFLINE_RESCAN);
			reloadItem.Click += Rescan_Click;
			contextMenu.Items.Add(reloadItem);

			deleteItem = new NostalgicToolStripMenuItem(Resources.IDS_MODLIBRARY_CONTEXT_OFFLINE_DELETE);
			deleteItem.Click += DeleteItem_Click;
			contextMenu.Items.Add(deleteItem);
		}



		/********************************************************************/
		/// <summary>
		/// Update context menu visibility
		/// </summary>
		/********************************************************************/
		private void UpdateContextMenuVisibility()
		{
			playModulesItem.Visible = playImmediately;
			addToPlayListItem.Visible = !playImmediately;

			// Determine favorites state for selected items
			int favoritesCount = 0;
			int nonFavoritesCount = 0;

			foreach (TreeNode entry in listControl.GetSelectedItems())
			{
				if (data.IsFavorite(entry.FullPath))
					favoritesCount++;
				else
					nonFavoritesCount++;
			}

			// Show Add/Remove from Favorites based on selection state
			// - All non-favorites: show only "Add to Favorites"
			// - All favorites: show only "Remove from Favorites"
			// - Mixed: show both
			addToFavoritesItem.Visible = nonFavoritesCount > 0;
			removeFromFavoritesItem.Visible = favoritesCount > 0;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize controls with settings
		/// </summary>
		/********************************************************************/
		private void InitializeControls(ModLibraryWindowSettings settings)
		{
			initializeRunning = true;

			try
			{
				flatViewCheckBox.Checked = settings.OfflineFlatViewEnabled;
				favoritesOnlyCheckBox.Checked = settings.FavoritesOnlyEnabled;

				breadcrumbControl.Initialize();
				listControl.Initialize(settings, false);
			}
			finally
			{
				initializeRunning = false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Store settings
		/// </summary>
		/********************************************************************/
		private void StoreSettings(ModLibraryWindowSettings settings)
		{
			settings.OfflineFlatViewEnabled = flatViewCheckBox.Checked;
			settings.FavoritesOnlyEnabled = favoritesOnlyCheckBox.Checked;
			settings.LastOfflinePath = breadcrumbControl.GetPath();

			listControl.StoreSettings(settings, false);

			data.SaveFavorites();
		}



		/********************************************************************/
		/// <summary>
		/// Load database into memory
		/// </summary>
		/********************************************************************/
		private void LoadDatabase(string breadcrumbPath, string searchText, SearchMode searchMode)
		{
			taskHelper.RunTask((cancellationToken) =>
			{
				Invoke(() =>
				{
					listControl.SetLoading(true, string.Format(Resources.IDS_MODLIBRARY_STATUS_LOADING_DATABASE, Resources.IDS_MODLIBRARY_LOCAL));
				});

				data.LoadDatabase();
				data.LoadFavorites();

				if (cancellationToken.IsCancellationRequested)
					return Task.FromCanceled(cancellationToken);

				Invoke(() =>
				{
					string newPath = data.ValidatePath(breadcrumbPath);
					breadcrumbControl.SetPath(newPath);

					Search(searchText, searchMode, true);
				});

				hasBeenLoaded = true;

				return Task.CompletedTask;
			}, (ex) => modLibraryHelper.ShowErrorMessage(ex, listControl, modLibraryWindowApi));
		}



		/********************************************************************/
		/// <summary>
		/// Load current directory into ListView
		/// </summary>
		/********************************************************************/
		private void LoadCurrentDirectory(string itemToSelect = null, bool setFocus = true)
		{
			using (new SleepCursor())
			{
				string currentPath = breadcrumbControl.GetPath();

				// Get entries from data class (already sorted)
				List<TreeNode> currentEntries = data.GetEntries(currentPath, flatViewCheckBox.Checked, listControl.SortOrder);

				// Apply favorites filter
				if (favoritesOnlyCheckBox.Checked)
				{
					currentEntries = currentEntries.Where(e =>
						(!e.IsDirectory && e.IsFavorite) ||
						(e.IsDirectory && DirectoryContainsFavorites(e))).ToList();
				}

				listControl.SetItems(currentPath, currentEntries);

				UpdateCounts(currentEntries);

				// Select specific item or default to first
				SelectItemInList(itemToSelect, setFocus);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Reload the local directory into memory
		/// </summary>
		/********************************************************************/
		private void ScanLocalFiles()
		{
			taskHelper.RunTask((cancellationToken) =>
			{
				Invoke(() =>
				{
					listControl.SetLoading(true, string.Format(Resources.IDS_MODLIBRARY_STATUS_LOADING_DATABASE, Resources.IDS_MODLIBRARY_LOCAL));
				});

				data.LoadDatabase();

				if (cancellationToken.IsCancellationRequested)
					return Task.FromCanceled(cancellationToken);

				Invoke(() =>
				{
					Search(lastSearchText, lastSearchMode, true);
				});

				hasBeenLoaded = true;

				return Task.CompletedTask;
			}, (ex) => modLibraryHelper.ShowErrorMessage(ex, listControl, modLibraryWindowApi));
		}



		/********************************************************************/
		/// <summary>
		/// Show the number of folders and files in the breadcrumb
		/// </summary>
		/********************************************************************/
		private void UpdateCounts(List<TreeNode> currentEntries)
		{
			int folderCount = currentEntries.Count(e => e.IsDirectory);
			int fileCount = currentEntries.Count - folderCount;

			breadcrumbControl.SetCounts(folderCount, fileCount, string.IsNullOrEmpty(data.SearchFilter) ? null : data.TotalFileCount);
		}



		/********************************************************************/
		/// <summary>
		/// Select the given item in the list or the first one if not found
		/// </summary>
		/********************************************************************/
		private void SelectItemInList(string itemToSelect, bool setFocus)
		{
			List<TreeNode> currentEntries = listControl.GetItems();

			if (currentEntries.Count == 0)
				return;

			int indexToSelect = 0;

			if (!string.IsNullOrEmpty(itemToSelect))
			{
				int index = currentEntries.FindIndex(entry => entry.Name.Equals(itemToSelect, StringComparison.OrdinalIgnoreCase) || (entry.IsDirectory && entry.Name.StartsWith(itemToSelect, StringComparison.OrdinalIgnoreCase)));
				if (index >= 0)
					indexToSelect = index;
			}

			listControl.SelectItem(indexToSelect, setFocus);
		}



		/********************************************************************/
		/// <summary>
		/// Play module if it exists locally (local mode)
		/// </summary>
		/********************************************************************/
		private void PlayModuleIfExists(TreeNode entry)
		{
			try
			{
				string localPath = GetLocalPathForEntry(entry);
				if (localPath == null)
					return;

				// Check if file exists
				if (File.Exists(localPath))
					playlistIntegration.AddToPlaylist(localPath, playImmediately);
				else
					mainWindowApi.ShowSimpleErrorMessage(string.Format(Resources.IDS_MODLIBRARY_MSGBOX_FILE_NOT_DOWNLOADED, entry.Name), modLibraryWindowApi.Form);
			}
			catch (Exception ex)
			{
				mainWindowApi.ShowSimpleErrorMessage(string.Format(Resources.IDS_MODLIBRARY_MSGBOX_FAILED_PLAY_MODULE, ex.Message), modLibraryWindowApi.Form);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Get local file path for local mode
		/// </summary>
		/********************************************************************/
		private string GetLocalPathForEntry(TreeNode entry)
		{
			return Path.Combine(modLibraryHelper.ModulesDirectory, entry.FullPath.Replace('/', Path.DirectorySeparatorChar));
		}



		/********************************************************************/
		/// <summary>
		/// Check if a directory contains any favorites (recursively)
		/// </summary>
		/********************************************************************/
		private bool DirectoryContainsFavorites(TreeNode directory)
		{
			// Check if the directory itself is a favorite
			if (directory.IsFavorite)
				return true;

			// Check children recursively
			foreach (TreeNode child in directory.Children)
			{
				if (child.IsDirectory)
				{
					if (DirectoryContainsFavorites(child))
						return true;
				}
				else
				{
					if (child.IsFavorite)
						return true;
				}
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Helper method to get local paths for selected entries
		/// </summary>
		/********************************************************************/
		private List<string> GetSelectedLocalPaths()
		{
			HashSet<string> seenPaths = new HashSet<string>();
			List<string> localPaths = new List<string>();

			foreach (TreeNode entry in listControl.GetSelectedItems())
			{
				if (entry.IsDirectory)
				{
					// Get all files from directory recursively
					var dirFiles = data.GetEntries(entry.FullPath, true, FlatViewSortOrder.NameThenPath)
						.Where(e => !e.IsDirectory);

					foreach (TreeNode file in dirFiles)
					{
						string localPath = GetLocalPathForEntry(file);

						if ((localPath != null) && File.Exists(localPath) && seenPaths.Add(localPath))
							localPaths.Add(localPath);
					}
				}
				else
				{
					string localPath = GetLocalPathForEntry(entry);

					if ((localPath != null) && File.Exists(localPath) && seenPaths.Add(localPath))
						localPaths.Add(localPath);
				}
			}

			return localPaths;
		}



		/********************************************************************/
		/// <summary>
		/// Helper method to add files to playlist
		/// </summary>
		/********************************************************************/
		private void AddFilesToPlaylist(List<string> localPaths, bool playNow)
		{
			if (localPaths.Count == 0)
				return;

			using (new SleepCursor())
			{
				// Build HashSet of existing paths for O(1) lookup
				HashSet<string> existingPaths = mainWindowApi.GetPlaylistPathsAsHashSet();

				List<string> newPaths = new List<string>();
				bool firstFileHandled = false;

				foreach (string localPath in localPaths)
				{
					// Check if module already exists in playlist
					if (existingPaths.Contains(localPath))
					{
						// Module already in list - play it if this is the first file and playImmediately is set
						if (playNow && !firstFileHandled)
						{
							ModuleListListItem existingItem = mainWindowApi.FindModuleInList(localPath);
							if (existingItem != null)
							{
								mainWindowApi.SelectAndPlayModule(existingItem);
								firstFileHandled = true;
							}
						}
					}
					else
						newPaths.Add(localPath);
				}

				// Add new files to playlist
				if (newPaths.Count > 0)
					mainWindowApi.AddFilesToModuleList(newPaths.ToArray(), playNow && !firstFileHandled);
			}
		}
		#endregion
	}
}
