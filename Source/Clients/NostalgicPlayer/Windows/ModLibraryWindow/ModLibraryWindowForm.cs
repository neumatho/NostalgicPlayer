/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.MainWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Events;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Timer = System.Windows.Forms.Timer;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow
{
	/// <summary>
	/// This shows the Module Library window
	/// </summary>
	public partial class ModLibraryWindowForm : WindowFormBase, IModLibraryWindowApi
	{
		private const string AllmodsZipUrl = "https://modland.com/allmods.zip";
		private readonly Dictionary<string, InfoBarControl> activeInfoBars = new();

		private IMainWindowApi mainWindow;
		private IPlatformPath platformPath;

		private readonly ModLibraryData data = new();

		// Download queue
		private DownloadQueueManager downloadQueueManager;

		private Timer searchDebounceTimer;
		private Timer searchTimer;
		private ModLibraryWindowSettings settings;
		private bool addDownloadsToPlaylist = true;

		private string cacheDirectory;
		private List<TreeNode> currentEntries = new();
		private string currentPath = string.Empty;
		private ModLibraryDownloadService downloadService;
		private ModLibraryFavorites favorites;
		private string initialModLibraryPath;
		private int busyCount;
		private bool isClosing;
		private ModLibraryPlaylistIntegration playlistIntegration;
		private string searchBuffer = string.Empty;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModLibraryWindowForm()
		{
			InitializeComponent();
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the form
		///
		/// Called from FormCreatorService
		/// </summary>
		/********************************************************************/
		public void InitializeForm(IMainWindowApi mainWindow, IPlatformPath platformPath)
		{
			this.mainWindow = mainWindow;
			this.platformPath = platformPath;

			Disposed += ModLibraryWindowForm_Disposed;

			// Set context menu items (KryptonContextMenuItem cannot use ControlResource)
			updateDatabaseItem.Text = Resources.IDS_MODLIBRARY_UPDATE_DATABASE;
			clearDatabaseItem.Text = Resources.IDS_MODLIBRARY_CLEAR_DATABASE;
			deleteItem.Text = Resources.IDS_MODLIBRARY_DELETE;
			reloadItem.Text = Resources.IDS_MODLIBRARY_RESCAN_LOCAL_FILES;
			addToFavoritesItem.Text = Resources.IDS_MODLIBRARY_ADD_TO_FAVORITES;
			removeFromFavoritesItem.Text = Resources.IDS_MODLIBRARY_REMOVE_FROM_FAVORITES;
			playItem.Text = Resources.IDS_MODLIBRARY_PLAY;
			addToPlaylistItem.Text = Resources.IDS_MODLIBRARY_ADD_TO_PLAYLIST;
			jumpToFolderItem.Text = Resources.IDS_MODLIBRARY_JUMP_TO_FOLDER;
			downloadToDiskItem.Text = Resources.IDS_MODLIBRARY_DOWNLOAD_TO_DISK;
			downloadAndPlayItem.Text = Resources.IDS_MODLIBRARY_DOWNLOAD_AND_PLAY;
			downloadToPlaylistItem.Text = Resources.IDS_MODLIBRARY_DOWNLOAD_TO_PLAYLIST;
			cancelDownloadButton.Text = Resources.IDS_MODLIBRARY_CANCEL;

			// Set column headers (ColumnHeader cannot use ControlResource)
			columnName.Text = Resources.IDS_MODLIBRARY_COLUMN_NAME;
			columnPath.Text = Resources.IDS_MODLIBRARY_COLUMN_PATH;
			columnSize.Text = Resources.IDS_MODLIBRARY_COLUMN_SIZE;

			// Set search mode combo box items (cannot use ControlResource)
			searchModeComboBox.Items.Clear();
			searchModeComboBox.Items.AddRange(Resources.IDS_MODLIBRARY_SEARCHMODE_FILENAME_AND_PATH,
				Resources.IDS_MODLIBRARY_SEARCHMODE_FILENAME_ONLY, Resources.IDS_MODLIBRARY_SEARCHMODE_PATH_ONLY);

			InitializeWindow();

			// Load window settings
			LoadWindowSettings("ModLibraryWindow");

			// Remember initial path
			initialModLibraryPath = GetCurrentModLibraryPath();

			// Set the title of the window
			UpdateWindowTitle();

			// Set initial status text
			statusLabel.Text = Resources.IDS_MODLIBRARY_STATUS_DEFAULT;

			// Initialize cache directory
			InitializeCacheDirectory();

			// Initialize services
			InitializeServices();

			// Initialize favorites (uses ModLibrary path)
			favorites = new ModLibraryFavorites(GetModLibraryModulesPath());

			// Initialize search timer for keyboard quick search
			searchTimer = new Timer();
			searchTimer.Interval = 1000; // 1 second timeout
			searchTimer.Tick += SearchTimer_Tick;

			// Initialize debounce timer for search textbox
			searchDebounceTimer = new Timer();
			searchDebounceTimer.Interval = 100; // 100ms debounce
			searchDebounceTimer.Tick += SearchDebounceTimer_Tick;

			// Load and apply ModLibrary settings (must be done AFTER timer initialization)
			settings = new ModLibraryWindowSettings(allWindowSettings);
			data.IsOfflineMode = settings.IsOfflineMode;
			modeTabControl.SelectedIndex = settings.IsOfflineMode ? 1 : 0;
			flatViewCheckBox.Checked = settings.FlatViewEnabled;
			playImmediatelyCheckBox.Checked = settings.PlayImmediately;
			searchTextBox.Text = settings.SearchText;
			// Set SelectedIndex which will trigger the event handler, but now settings is initialized
			searchModeComboBox.SelectedIndex = settings.SearchMode;

			// Restore column widths
			columnName.Width = settings.ColumnWidthName;
			columnPath.Width = flatViewCheckBox.Checked ? settings.ColumnWidthPath : 0;
			columnSize.Width = settings.ColumnWidthSize;

			// Restore favorites only checkbox (only visible in offline mode)
			favoritesOnlyCheckBox.Checked = settings.FavoritesOnlyEnabled;
			favoritesOnlyCheckBox.Visible = data.IsOfflineMode;

			// Restore last path based on mode
			currentPath = data.IsOfflineMode ? settings.LastOfflinePath : settings.LastOnlinePath;

			// Check if we already have the database
			CheckExistingDatabase();

			// Hook up data loaded event
			data.DataLoaded += OnDataLoaded;
		}

		#region WindowFormBase overrides
		/********************************************************************/
		/// <summary>
		/// Return the URL to the help page
		/// </summary>
		/********************************************************************/
		protected override string HelpUrl => "modlibrary.html";
		#endregion

		#region IModLibraryWindowApi implementation
		/********************************************************************/
		/// <summary>
		/// Return the form of the Module Library window
		/// </summary>
		/********************************************************************/
		public Form Form => this;



		/********************************************************************/
		/// <summary>
		/// Reload if path has changed
		/// </summary>
		/********************************************************************/
		public async void ReloadChanges()
		{
			// Get current path from settings
			string currentModLibPath = GetCurrentModLibraryPath();

			// Check if path has changed
			if (currentModLibPath != initialModLibraryPath)
			{
				// Save favorites to old path before switching
				favorites.Save();

				// Update initial path first (so UpdateWindowTitle can use it)
				initialModLibraryPath = currentModLibPath;

				// Reload favorites from new path
				favorites = new ModLibraryFavorites(GetModLibraryModulesPath());

				// Show loading status
				ShowListViewStatus(Resources.IDS_MODLIBRARY_STATUS_SCANNING_LOCAL_FILES);
				statusLabel.Text = Resources.IDS_MODLIBRARY_STATUS_SCANNING_LOCAL_FILES;

				// Reload everything in background
				await Task.Run(() => ScanLocalFiles());
				RebuildTree();
				UpdateWindowTitle();
			}
		}
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// </summary>
		/********************************************************************/
		private void ModLibraryWindowForm_Disposed(object sender, EventArgs e)
		{
			// Set closing flag to prevent event handlers from accessing disposed controls
			isClosing = true;

			// Terminate worker thread and remove event handlers
			downloadQueueManager.Terminate();
			downloadQueueManager.ProgressChanged -= DownloadQueue_ProgressChanged;
			downloadQueueManager.DownloadCompleted -= DownloadQueue_DownloadCompleted;
			downloadQueueManager.QueueCompleted -= DownloadQueue_QueueCompleted;
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the form is shown for the first time
		/// </summary>
		/********************************************************************/
		private void ModLibraryForm_Shown(object sender, EventArgs e)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the form is closed
		/// </summary>
		/********************************************************************/
		private void ModLibraryForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			// Save current path based on mode
			if (data.IsOfflineMode)
			{
				settings.LastOfflinePath = currentPath;
			}
			else
			{
				settings.LastOnlinePath = currentPath;
			}

			// Save column widths
			settings.ColumnWidthName = columnName.Width;
			if (flatViewCheckBox.Checked)
			{
				settings.ColumnWidthPath = columnPath.Width;
			}

			settings.ColumnWidthSize = columnSize.Width;

			// Save favorites
			favorites.Save();

			// Save all settings when closing the window
			settings.Settings.SaveSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the parent button is clicked
		/// </summary>
		/********************************************************************/
		private void ParentButton_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(currentPath))
			{
				return;
			}

			// Remember the folder we're leaving to select it in the parent directory
			string folderToSelect;

			if (data.IsOfflineMode)
			{
				// Local mode: Simple path navigation
				int lastSlash = currentPath.LastIndexOf('/');
				if (lastSlash > 0)
				{
					folderToSelect = currentPath.Substring(lastSlash + 1);
					currentPath = currentPath.Substring(0, lastSlash);
				}
				else
				{
					// At root level
					folderToSelect = currentPath;
					currentPath = string.Empty;
				}
			}
			else
			{
				// Online mode: Service-based navigation
				// If at service root (e.g., "modland://"), go back to root
				if (currentPath.EndsWith("://"))
				{
					// Extract service name from path (e.g., "modland://" -> "modland")
					folderToSelect = currentPath.Substring(0, currentPath.Length - 3);
					currentPath = string.Empty;
				}
				else
				{
					// Get the service to find where the service root ends
					var service = data.GetServiceFromPath(currentPath);
					if (service != null)
					{
						// If we're directly under service root (e.g., "modland://Amiga"), go back to service root
						string relativePath = data.GetRelativePathFromService(currentPath, service);

						// Check if we're at first level (no slashes in relative path)
						if (!relativePath.Contains('/'))
						{
							folderToSelect = relativePath;
							currentPath = service.RootPath;
						}
						else
						{
							// Find the last slash in the full path
							int lastSlash = currentPath.LastIndexOf('/');
							folderToSelect = currentPath.Substring(lastSlash + 1);
							currentPath = currentPath.Substring(0, lastSlash);
						}
					}
					else
					{
						folderToSelect = null;
						currentPath = string.Empty;
					}
				}
			}

			LoadCurrentDirectory(folderToSelect);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when virtual list needs an item
		/// </summary>
		/********************************************************************/
		private void ModuleListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
		{
			// Simply return if index is out of range
			if (currentEntries == null || e.ItemIndex < 0 || e.ItemIndex >= currentEntries.Count)
			{
				return;
			}

			var entry = currentEntries[e.ItemIndex];
			string icon = entry.IsDirectory ? "📁" : "🎵";
			string star = data.IsOfflineMode && favorites.IsFavorite(entry.FullPath) ? "⭐ " : "";
			ListViewItem item = new($"{star}{icon} {entry.Name}");

			// Add path column (only visible in flat view)
			if (flatViewCheckBox.Checked && !entry.IsDirectory)
			{
				// Extract path from FullPath (remove filename)
				int lastSlash = entry.FullPath.LastIndexOf('/');
				string pathOnly = lastSlash >= 0 ? entry.FullPath.Substring(0, lastSlash) : string.Empty;

				// Remove current path prefix to show only relative path
				if (!string.IsNullOrEmpty(currentPath) && pathOnly.StartsWith(currentPath))
				{
					pathOnly = pathOnly.Substring(currentPath.Length);
					if (pathOnly.StartsWith("/"))
					{
						pathOnly = pathOnly.Substring(1);
					}
				}

				item.SubItems.Add(pathOnly);
			}
			else
			{
				item.SubItems.Add(string.Empty);
			}

			// Add size column
			if (entry.IsDirectory)
			{
				// For directories, show file count recursively from tree
				int fileCount = CountFilesInTree(entry);
				item.SubItems.Add(fileCount > 0 ? string.Format(Resources.IDS_MODLIBRARY_FILE_COUNT, fileCount) : string.Empty);
			}
			else
				// For files, show size in KB, MB, etc.
			{
				item.SubItems.Add(FormatFileSize(entry.Size));
			}

			item.Tag = entry;
			e.Item = item;
		}



		/********************************************************************/
		/// <summary>
		/// Is called when a list item is double-clicked
		/// </summary>
		/********************************************************************/
		private async void ModuleListView_DoubleClick(object sender, EventArgs e)
		{
			if (moduleListView.SelectedIndices.Count == 0)
			{
				return;
			}

			int index = moduleListView.SelectedIndices[0];
			if (index < 0 || index >= currentEntries.Count)
			{
				return;
			}

			var entry = currentEntries[index];

			if (entry.IsDirectory)
			{
				// Check if this is a service root that's not loaded (only in online mode)
				if (entry.FullPath.EndsWith("://") && !data.IsOfflineMode)
				{
					var service = data.GetService(entry.ServiceId);
					if (service != null && !service.IsLoaded)
					{
						// Ask user if they want to download the database
						var result = MessageBox.Show(
							string.Format(Resources.IDS_MODLIBRARY_MSGBOX_DATABASE_NOT_LOADED, service.DisplayName),
							Resources.IDS_MODLIBRARY_MSGBOX_TITLE_DOWNLOAD_DB,
							MessageBoxButtons.YesNo,
							MessageBoxIcon.Question);

						if (result == DialogResult.Yes)
						{
							await DownloadModLandDatabase();
						}

						return; // User declined, don't navigate
					}
				}

				currentPath = entry.FullPath;
				LoadCurrentDirectory();
			}
			else
			{
				// In local mode, only play if file exists locally
				if (data.IsOfflineMode)
				{
					PlayModuleIfExists(entry);
				}
				else
				{
					// Online mode: Add to download queue
					downloadQueueManager.EnqueueFiles(new[] {entry}, playImmediatelyCheckBox.Checked);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the list view is right-clicked (for context
		/// menu)
		/// </summary>
		/********************************************************************/
		private void ModuleListView_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				// Check if we clicked on an item
				var hitTest = moduleListView.HitTest(e.Location);
				if (hitTest.Item != null && hitTest.Item.Tag is TreeNode entry)
				{
					if (data.IsOfflineMode)
					{
						// Local mode: Show context menu for files and directories (but not service roots)
						if (!entry.FullPath.EndsWith("://"))
						{
							UpdateOfflineContextMenuVisibility();
							offlineContextMenu.Show(moduleListView.PointToScreen(e.Location));
						}
					}
					else
					{
						// Online mode
						if (entry.IsDirectory && entry.FullPath.EndsWith("://"))
						{
							// Show service context menu only for service nodes at root
							serviceContextMenu.Show(moduleListView.PointToScreen(e.Location));
						}
						else
						{
							// Show download menu for files, directories, and multi-selection
							UpdateOnlineContextMenuVisibility();
							onlineContextMenu.Show(moduleListView.PointToScreen(e.Location));
						}
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when a key is pressed in the list view
		/// </summary>
		/********************************************************************/
		private void ModuleListView_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (char.IsLetterOrDigit(e.KeyChar) || char.IsWhiteSpace(e.KeyChar))
			{
				// Add to search buffer
				searchBuffer += e.KeyChar.ToString().ToLower();
				searchTimer.Stop();
				searchTimer.Start();

				// Find first matching item
				for (int i = 0; i < currentEntries.Count; i++)
				{
					if (currentEntries[i].Name.ToLower().StartsWith(searchBuffer))
					{
						moduleListView.SelectedIndices.Clear();
						moduleListView.SelectedIndices.Add(i);
						moduleListView.EnsureVisible(i);
						break;
					}
				}

				e.Handled = true;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when a key is pressed down in the list view
		/// </summary>
		/********************************************************************/
		private async void ModuleListView_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				// Enter = navigate into folder or play module
				if (moduleListView.SelectedIndices.Count > 0)
				{
					int index = moduleListView.SelectedIndices[0];
					if (index >= 0 && index < currentEntries.Count)
					{
						var entry = currentEntries[index];

						if (entry.IsDirectory)
						{
							// Check if this is a service root that's not loaded (only in online mode)
							if (entry.FullPath.EndsWith("://") && !data.IsOfflineMode)
							{
								var service = data.GetService(entry.ServiceId);
								if (service != null && !service.IsLoaded)
								{
									// Ask user if they want to download the database
									var result = MessageBox.Show(
										string.Format(Resources.IDS_MODLIBRARY_MSGBOX_DATABASE_NOT_LOADED,
											service.DisplayName),
										Resources.IDS_MODLIBRARY_MSGBOX_TITLE_DOWNLOAD_DB,
										MessageBoxButtons.YesNo,
										MessageBoxIcon.Question);

									if (result == DialogResult.Yes)
									{
										await DownloadModLandDatabase();
										e.Handled = true;
										return;
									}

									e.Handled = true;
									return; // User declined, don't navigate
								}
							}

							currentPath = entry.FullPath;
							LoadCurrentDirectory();
						}
						else
						{
							// In local mode, only play if file exists locally
							if (data.IsOfflineMode)
							{
								PlayModuleIfExists(entry);
							}
							else
							{
								// Online mode: Add to download queue
								downloadQueueManager.EnqueueFiles(new[] {entry}, playImmediatelyCheckBox.Checked);
							}
						}
					}
				}

				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Back)
			{
				// Backspace = go to parent directory
				ParentButton_Click(sender, EventArgs.Empty);

				e.Handled = true;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when search text changes
		/// </summary>
		/********************************************************************/
		private void SearchTextBox_TextChanged(object sender, EventArgs e)
		{
			// Restart debounce timer
			searchDebounceTimer.Stop();
			searchDebounceTimer.Start();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when search button is clicked
		/// </summary>
		/********************************************************************/
		private void SearchButton_Click(object sender, EventArgs e)
		{
			// Trigger search immediately (same as debounce timer)
			SearchDebounceTimer_Tick(sender, e);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when "Update Database" context menu item is clicked
		/// </summary>
		/********************************************************************/
		private async void UpdateDatabaseItem_Click(object sender, EventArgs e)
		{
			// Reset ignore flag when user manually triggers update
			settings.SetUpdateIgnored("modland", false);

			// Hide info bar if visible
			HideInfoBar("modland");

			// For now, only ModLand is supported
			await DownloadModLandDatabase();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when "Clear Database" context menu item is clicked
		/// </summary>
		/********************************************************************/
		private void ClearDatabaseItem_Click(object sender, EventArgs e)
		{
			var modland = data.GetService("modland");
			if (modland == null)
			{
				return;
			}

			// Ask for confirmation
			var result = MessageBox.Show(
				Resources.IDS_MODLIBRARY_MSGBOX_CLEAR_DATABASE_CONFIRM,
				Resources.IDS_MODLIBRARY_MSGBOX_TITLE_CLEAR_DB,
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning);

			if (result == DialogResult.Yes)
			{
				try
				{
					// Clear data in RAM
					modland.ClearOnlineFiles();
					modland.IsLoaded = false;
					modland.LastUpdate = DateTime.MinValue;

					// Delete database file
					string allmodsTxtPath = Path.Combine(cacheDirectory, "ModLand", "allmods.txt");
					if (File.Exists(allmodsTxtPath))
					{
						File.Delete(allmodsTxtPath);
					}

					// Also delete the zip file if it exists
					string allmodsZipPath = Path.Combine(cacheDirectory, "ModLand", "allmods.zip");
					if (File.Exists(allmodsZipPath))
					{
						File.Delete(allmodsZipPath);
					}

					// Reload display
					currentPath = string.Empty;
					RebuildTree();

					statusLabel.Text = Resources.IDS_MODLIBRARY_STATUS_DB_CLEARED_SUCCESS;
				}
				catch (Exception ex)
				{
					statusLabel.Text = string.Format(Resources.IDS_MODLIBRARY_STATUS_ERROR_CLEARING_DB, ex.Message);
					MessageBox.Show(string.Format(Resources.IDS_MODLIBRARY_MSGBOX_FAILED_CLEAR_DB, ex.Message),
						Resources.IDS_MODLIBRARY_MSGBOX_TITLE_CLEAR_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Update offline context menu visibility based on Play Immediately
		/// checkbox and favorites state
		/// </summary>
		/********************************************************************/
		private void UpdateOfflineContextMenuVisibility()
		{
			// Show "Play Module(s)" when Play Immediately is checked
			// Show "Add to Playlist" when Play Immediately is unchecked
			playItem.Visible = playImmediatelyCheckBox.Checked;
			addToPlaylistItem.Visible = !playImmediatelyCheckBox.Checked;

			// Determine favorites state for selected items
			int favoritesCount = 0;
			int nonFavoritesCount = 0;
			foreach (int index in moduleListView.SelectedIndices)
			{
				if (index >= 0 && index < currentEntries.Count)
				{
					var entry = currentEntries[index];
					if (favorites.IsFavorite(entry.FullPath))
					{
						favoritesCount++;
					}
					else
					{
						nonFavoritesCount++;
					}
				}
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
		/// Update online context menu visibility based on Play Immediately
		/// checkbox
		/// </summary>
		/********************************************************************/
		private void UpdateOnlineContextMenuVisibility()
		{
			// "Download to Disk" is always visible
			// Show "Download and Play" when Play Immediately is checked
			// Show "Download to Playlist" when Play Immediately is unchecked
			downloadAndPlayItem.Visible = playImmediatelyCheckBox.Checked;
			downloadToPlaylistItem.Visible = !playImmediatelyCheckBox.Checked;

			// Show "Jump to Folder" only in flat view mode with single file selected
			bool isSingleFile = moduleListView.SelectedIndices.Count == 1 &&
			                    moduleListView.SelectedIndices[0] < currentEntries.Count &&
			                    !currentEntries[moduleListView.SelectedIndices[0]].IsDirectory;
			jumpToFolderItem.Visible = flatViewCheckBox.Checked && isSingleFile;

			// Show/hide separator based on Jump to Folder visibility
			onlineSeparator.Visible = jumpToFolderItem.Visible;
		}



		/********************************************************************/
		/// <summary>
		/// Is called when "Add to Favorites" context menu item is clicked
		/// </summary>
		/********************************************************************/
		private void AddToFavoritesItem_Click(object sender, EventArgs e)
		{
			// Add all selected entries to favorites
			foreach (int index in moduleListView.SelectedIndices)
			{
				if (index >= 0 && index < currentEntries.Count)
				{
					var entry = currentEntries[index];
					if (!favorites.IsFavorite(entry.FullPath))
					{
						favorites.Toggle(entry.FullPath);
					}
				}
			}

			// Refresh the list to show stars
			moduleListView.Invalidate();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when "Remove from Favorites" context menu item is clicked
		/// </summary>
		/********************************************************************/
		private void RemoveFromFavoritesItem_Click(object sender, EventArgs e)
		{
			// Remove all selected entries from favorites
			foreach (int index in moduleListView.SelectedIndices)
			{
				if (index >= 0 && index < currentEntries.Count)
				{
					var entry = currentEntries[index];
					if (favorites.IsFavorite(entry.FullPath))
					{
						favorites.Toggle(entry.FullPath);
					}
				}
			}

			// Refresh the list to hide stars
			moduleListView.Invalidate();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when "Rescan Local Files" context menu item is clicked
		/// </summary>
		/********************************************************************/
		private async void ReloadItem_Click(object sender, EventArgs e)
		{
			BeginBusy();

			try
			{
				// Show loading status
				ShowListViewStatus(Resources.IDS_MODLIBRARY_STATUS_SCANNING_LOCAL_FILES);
				statusLabel.Text = Resources.IDS_MODLIBRARY_STATUS_SCANNING_LOCAL_FILES;

				// Rescan local files in background
				await Task.Run(() => ScanLocalFiles());

				// Rebuild tree
				RebuildTree();
			}
			finally
			{
				EndBusy();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when "Play Module(s)" context menu item is clicked
		/// </summary>
		/********************************************************************/
		private void PlayItem_Click(object sender, EventArgs e)
		{
			var localPaths = GetSelectedLocalPaths();
			if (localPaths.Count > 0)
			{
				AddFilesToPlaylist(localPaths, true);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when "Add to Playlist" context menu item is clicked
		/// </summary>
		/********************************************************************/
		private void AddToPlaylistItem_Click(object sender, EventArgs e)
		{
			var localPaths = GetSelectedLocalPaths();
			if (localPaths.Count > 0)
			{
				AddFilesToPlaylist(localPaths, false);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Helper method to get local paths for selected entries
		/// </summary>
		/********************************************************************/
		private List<string> GetSelectedLocalPaths()
		{
			HashSet<string> seenPaths = new();
			List<string> localPaths = new();

			foreach (int index in moduleListView.SelectedIndices)
			{
				if (index >= 0 && index < currentEntries.Count)
				{
					var entry = currentEntries[index];
					if (entry.IsDirectory)
					{
						// Get all files from directory recursively
						var dirFiles = data.GetEntries(entry.FullPath, true, FlatViewSortOrder.NameThenPath)
							.Where(e => !e.IsDirectory);
						foreach (var file in dirFiles)
						{
							string localPath = GetLocalPathForEntry(file);
							if (localPath != null && File.Exists(localPath) && seenPaths.Add(localPath))
							{
								localPaths.Add(localPath);
							}
						}
					}
					else
					{
						string localPath = GetLocalPathForEntry(entry);
						if (localPath != null && File.Exists(localPath) && seenPaths.Add(localPath))
						{
							localPaths.Add(localPath);
						}
					}
				}
			}

			return localPaths;
		}



		/********************************************************************/
		/// <summary>
		/// Helper method to add files to playlist
		/// </summary>
		/********************************************************************/
		private void AddFilesToPlaylist(List<string> localPaths, bool playImmediately)
		{
			if (localPaths.Count == 0)
			{
				return;
			}

			// Add to playlist using the main window API
			if (mainWindow.Form is MainWindowForm mainWindowForm)
			{
				mainWindowForm.Invoke((Action)(() =>
				{
					// Build HashSet of existing paths for O(1) lookup
					var existingPaths = mainWindowForm.GetPlaylistPathsAsHashSet();

					List<string> newPaths = new();
					bool firstFileHandled = false;

					foreach (string localPath in localPaths)
					{
						// Check if module already exists in playlist
						if (existingPaths.Contains(localPath))
						{
							// Module already in list - play it if this is the first file and playImmediately is set
							if (playImmediately && !firstFileHandled)
							{
								var existingItem = mainWindowForm.FindModuleInList(localPath);
								if (existingItem != null)
								{
									mainWindowForm.SelectAndPlayModule(existingItem);
									firstFileHandled = true;
								}
							}
						}
						else
						{
							newPaths.Add(localPath);
						}
					}

					// Add new files to playlist
					if (newPaths.Count > 0)
					{
						mainWindowForm.AddFilesToModuleList(newPaths.ToArray(), playImmediately && !firstFileHandled);
					}
				}));
			}

			statusLabel.Text = string.Format(Resources.IDS_MODLIBRARY_STATUS_ADDED_FILES, localPaths.Count);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when keyboard quick search timer expires (1 second)
		/// </summary>
		/********************************************************************/
		private void SearchTimer_Tick(object sender, EventArgs e)
		{
			searchBuffer = string.Empty;
			searchTimer.Stop();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when debounce timer ticks (100ms after last
		/// keystroke)
		/// </summary>
		/********************************************************************/
		private void SearchDebounceTimer_Tick(object sender, EventArgs e)
		{
			searchDebounceTimer.Stop();

			string searchFilter = searchTextBox.Text.Trim();

			// Save search text setting
			settings.SearchText = searchTextBox.Text;

			// Show "Searching..." in ListView
			if (!string.IsNullOrEmpty(searchFilter))
			{
				ShowListViewStatus(Resources.IDS_MODLIBRARY_STATUS_SEARCHING);
			}

			// Get search mode from combobox
			SearchMode searchMode = (SearchMode)searchModeComboBox.SelectedIndex;

			// Start search (will fire DataLoaded event when done)
			data.BuildTree(searchFilter, searchMode, flatViewCheckBox.Checked);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when search mode combobox selection changes
		/// </summary>
		/********************************************************************/
		private void SearchModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Save setting
			settings.SearchMode = searchModeComboBox.SelectedIndex;

			// Trigger a new search with current filter and new mode
			string searchFilter = searchTextBox.Text.Trim();
			if (!string.IsNullOrEmpty(searchFilter))
			{
				ShowListViewStatus(Resources.IDS_MODLIBRARY_STATUS_SEARCHING);
				SearchMode searchMode = (SearchMode)searchModeComboBox.SelectedIndex;
				data.BuildTree(searchFilter, searchMode, flatViewCheckBox.Checked);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when flat view checkbox is toggled
		/// </summary>
		/********************************************************************/
		private void FlatViewCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			// Save setting
			settings.FlatViewEnabled = flatViewCheckBox.Checked;

			// Show/hide path column based on flat view mode
			columnPath.Width = flatViewCheckBox.Checked ? settings.ColumnWidthPath : 0;

			// Rebuild tree with current settings
			ShowListViewStatus(flatViewCheckBox.Checked
				? Resources.IDS_MODLIBRARY_STATUS_BUILDING_FLAT_VIEW
				: Resources.IDS_MODLIBRARY_STATUS_BUILDING_TREE_VIEW);
			RebuildTree();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when play immediately checkbox is toggled
		/// </summary>
		/********************************************************************/
		private void PlayImmediatelyCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			// Save setting
			settings.PlayImmediately = playImmediatelyCheckBox.Checked;
		}



		/********************************************************************/
		/// <summary>
		/// Is called when favorites only checkbox is toggled
		/// </summary>
		/********************************************************************/
		private void FavoritesOnlyCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			// Save setting
			settings.FavoritesOnlyEnabled = favoritesOnlyCheckBox.Checked;

			// Reload current directory to apply filter
			LoadCurrentDirectory();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the mode tab control selection changes
		/// </summary>
		/********************************************************************/
		private void ModeTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Block tab switch during busy operations
			if (IsBusy)
			{
				return;
			}

			// Switch offline mode based on selected tab
			bool isOffline = modeTabControl.SelectedIndex == 1;

			if (data.IsOfflineMode != isOffline)
			{
				// Save current path for old mode before switching
				if (data.IsOfflineMode)
				{
					settings.LastOfflinePath = currentPath;
				}
				else
				{
					settings.LastOnlinePath = currentPath;
				}

				data.IsOfflineMode = isOffline;

				// Save setting
				settings.IsOfflineMode = isOffline;

				// Show/hide favorites only checkbox (only available in offline mode)
				favoritesOnlyCheckBox.Visible = isOffline;

				// Show "Switching mode..." in ListView
				ShowListViewStatus(isOffline
					? Resources.IDS_MODLIBRARY_STATUS_SWITCHING_TO_LOCAL
					: Resources.IDS_MODLIBRARY_STATUS_SWITCHING_TO_ONLINE);

				// Restore last path for new mode
				currentPath = isOffline ? settings.LastOfflinePath : settings.LastOnlinePath;

				// Rebuild tree for new mode (will fire DataLoaded event when done)
				RebuildTree();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Called when search/data operation is completed
		/// </summary>
		/********************************************************************/
		private void OnDataLoaded(object sender, EventArgs e)
		{
			// Stay at current path, even if search finds nothing there
			// Update UI
			LoadCurrentDirectory();

			// Update status
			if (!string.IsNullOrEmpty(data.SearchFilter))
			{
				int totalFiles = data.CountTotalFilesInFilteredCache();
				statusLabel.Text =
					string.Format(Resources.IDS_MODLIBRARY_STATUS_FOUND_FILES, totalFiles, data.SearchFilter);
			}
			else
			{
				// Full tree loaded
				var modland = data.GetService("modland");
				if (modland != null)
				{
					if (modland.IsLoaded)
					{
						data.CountTotalFilesInFilteredCache();
						statusLabel.Text = string.Format(Resources.IDS_MODLIBRARY_STATUS_DATABASE_LOADED,
							modland.LastUpdate.ToString("yyyy-MM-dd"), modland.OnlineFiles.Count);
					}
					else
					{
						statusLabel.Text = Resources.IDS_MODLIBRARY_STATUS_RIGHT_CLICK_SERVICE;
					}
				}
			}

			// Restore UI state
			moduleListView.Enabled = true;
			Cursor = Cursors.Default;
		}



		/********************************************************************/
		/// <summary>
		/// Is called when "Delete" context menu item is clicked
		/// </summary>
		/********************************************************************/
		private void DeleteItem_Click(object sender, EventArgs e)
		{
			if (moduleListView.SelectedIndices.Count == 0)
			{
				return;
			}

			// Collect all selected entries
			List<TreeNode> selectedEntries = new();
			foreach (int index in moduleListView.SelectedIndices)
			{
				if (index >= 0 && index < currentEntries.Count)
				{
					selectedEntries.Add(currentEntries[index]);
				}
			}

			if (selectedEntries.Count == 0)
			{
				return;
			}

			// Ask for confirmation
			string message;
			if (selectedEntries.Count == 1)
			{
				var entry = selectedEntries[0];
				message = entry.IsDirectory
					? string.Format(Resources.IDS_MODLIBRARY_MSGBOX_DELETE_FOLDER_CONFIRM, entry.Name)
					: string.Format(Resources.IDS_MODLIBRARY_MSGBOX_DELETE_FILE_CONFIRM, entry.Name);
			}
			else
			{
				int fileCount = selectedEntries.Count(f => !f.IsDirectory);
				int folderCount = selectedEntries.Count(f => f.IsDirectory);

				if (fileCount > 0 && folderCount > 0)
				{
					message = string.Format(Resources.IDS_MODLIBRARY_MSGBOX_DELETE_MULTIPLE_CONFIRM, fileCount,
						folderCount);
				}
				else if (folderCount > 0)
				{
					message = string.Format(Resources.IDS_MODLIBRARY_MSGBOX_DELETE_FOLDERS_CONFIRM, folderCount);
				}
				else
				{
					message = string.Format(Resources.IDS_MODLIBRARY_MSGBOX_DELETE_FILES_CONFIRM, fileCount);
				}
			}

			var result = MessageBox.Show(message, Resources.IDS_MODLIBRARY_MSGBOX_TITLE_DELETE_CONFIRM,
				MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

			if (result == DialogResult.Yes)
			{
				int deletedCount = 0;
				int errorCount = 0;
				string lastError = string.Empty;

				try
				{
					// Delete is only available in offline mode, where all files have FullPath relative to modulesBasePath
					string modulesBasePath = GetModLibraryModulesPath();

					foreach (var entry in selectedEntries)
					{
						try
						{
							// FullPath is the relative path from modulesBasePath (e.g., "Games/file.mod" or "modland/Games/file.mod")
							string localPath = Path.Combine(modulesBasePath,
								entry.FullPath.Replace('/', Path.DirectorySeparatorChar));

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

					// Rebuild tree to reflect changes (once at the end)
					statusLabel.Text = Resources.IDS_MODLIBRARY_STATUS_UPDATING_FILE_LIST;
					RebuildTree();

					// Show result
					if (errorCount == 0)
					{
						statusLabel.Text = deletedCount == 1
							? Resources.IDS_MODLIBRARY_STATUS_ITEM_DELETED_SUCCESS
							: string.Format(Resources.IDS_MODLIBRARY_STATUS_ITEMS_DELETED_SUCCESS, deletedCount);
					}
					else
					{
						statusLabel.Text = string.Format(Resources.IDS_MODLIBRARY_STATUS_DELETED_WITH_ERRORS,
							deletedCount,
							errorCount);
						MessageBox.Show(
							string.Format(Resources.IDS_MODLIBRARY_MSGBOX_DELETE_COMPLETED_WITH_ERRORS, deletedCount,
								errorCount, lastError), Resources.IDS_MODLIBRARY_MSGBOX_TITLE_DELETE_ERRORS,
							MessageBoxButtons.OK,
							MessageBoxIcon.Warning);
					}
				}
				catch (Exception ex)
				{
					statusLabel.Text = string.Format(Resources.IDS_MODLIBRARY_STATUS_ERROR_DELETING, ex.Message);
					MessageBox.Show(string.Format(Resources.IDS_MODLIBRARY_MSGBOX_FAILED_DELETE, ex.Message),
						Resources.IDS_MODLIBRARY_MSGBOX_TITLE_DELETE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when "Jump to folder" context menu item is clicked
		/// </summary>
		/********************************************************************/
		private void JumpToFolderItem_Click(object sender, EventArgs e)
		{
			if (moduleListView.SelectedIndices.Count == 0)
			{
				return;
			}

			int index = moduleListView.SelectedIndices[0];
			if (index < 0 || index >= currentEntries.Count)
			{
				return;
			}

			var entry = currentEntries[index];

			// Get service
			var service = data.GetService(entry.ServiceId);
			if (service == null)
			{
				return;
			}

			// Get relative path and extract folder path (without filename)
			string relativePath = data.GetRelativePathFromService(entry.FullPath, service);
			int lastSlash = relativePath.LastIndexOf('/');
			string folderPath = lastSlash >= 0 ? relativePath.Substring(0, lastSlash) : string.Empty;

			// Build full folder path
			string fullFolderPath = service.RootPath + folderPath;

			// Clear search text
			searchTextBox.Text = string.Empty;

			// Navigate to folder
			currentPath = fullFolderPath;
			LoadCurrentDirectory();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when "Download to Disk" context menu item is clicked
		/// </summary>
		/********************************************************************/
		private void DownloadToDiskItem_Click(object sender, EventArgs e)
		{
			// Download only, don't add to playlist
			DownloadSelectedFiles(false, false);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when "Download and Play" context menu item is clicked
		/// </summary>
		/********************************************************************/
		private void DownloadAndPlayItem_Click(object sender, EventArgs e)
		{
			// Download, add to playlist, and play first file
			DownloadSelectedFiles(true, true);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when "Download to Playlist" context menu item is clicked
		/// </summary>
		/********************************************************************/
		private void DownloadToPlaylistItem_Click(object sender, EventArgs e)
		{
			// Download and add to playlist, but don't play
			DownloadSelectedFiles(true, false);
		}



		/********************************************************************/
		/// <summary>
		/// Helper method to download selected files
		/// </summary>
		/********************************************************************/
		private void DownloadSelectedFiles(bool addToPlaylist, bool playFirst)
		{
			if (moduleListView.SelectedIndices.Count == 0)
			{
				return;
			}

			// Collect selected entries
			List<TreeNode> selectedEntries = new();
			foreach (int index in moduleListView.SelectedIndices)
			{
				if (index >= 0 && index < currentEntries.Count)
				{
					selectedEntries.Add(currentEntries[index]);
				}
			}

			// Collect all files (directly selected + from directories), avoiding duplicates
			HashSet<string> seenPaths = new();
			List<TreeNode> allFiles = new();

			foreach (var entry in selectedEntries)
			{
				if (entry.IsDirectory)
				{
					// Get all files from directory recursively
					var dirFiles = data.GetEntries(entry.FullPath, true, FlatViewSortOrder.NameThenPath)
						.Where(e => !e.IsDirectory);
					foreach (var file in dirFiles)
					{
						if (seenPaths.Add(file.FullPath))
						{
							allFiles.Add(file);
						}
					}
				}
				else
				{
					if (seenPaths.Add(entry.FullPath))
					{
						allFiles.Add(entry);
					}
				}
			}

			if (allFiles.Count == 0)
			{
				return;
			}

			// Ask for confirmation if more than 10 files
			if (allFiles.Count > 10)
			{
				var result = MessageBox.Show(
					string.Format(Resources.IDS_MODLIBRARY_MSGBOX_DOWNLOAD_CONFIRM, allFiles.Count),
					Resources.IDS_MODLIBRARY_MSGBOX_TITLE_DOWNLOAD_CONFIRM,
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Question);

				if (result != DialogResult.Yes)
				{
					return;
				}
			}

			// Set flag to control whether downloads are added to playlist
			addDownloadsToPlaylist = addToPlaylist;

			// Enqueue all files (playFirst determines if the first file should be played immediately)
			downloadQueueManager.EnqueueFiles(allFiles, playFirst);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the Cancel download button is clicked
		/// </summary>
		/********************************************************************/
		private void CancelDownloadButton_Click(object sender, EventArgs e)
		{
			downloadQueueManager.Clear();
			cancelDownloadButton.Visible = false;
			progressBar.Visible = false;
			statusLabel.Text = Resources.IDS_MODLIBRARY_STATUS_DOWNLOAD_CANCELLED;
		}



		/********************************************************************/
		/// <summary>
		/// Is called when download progress changes
		/// </summary>
		/********************************************************************/
		private void DownloadQueue_ProgressChanged(object sender, DownloadProgressEventArgs e)
		{
			if (isClosing)
			{
				return;
			}

			if (InvokeRequired)
			{
				Invoke(() => DownloadQueue_ProgressChanged(sender, e));
				return;
			}

			// Update status label
			string relativePath = data.GetRelativePathFromService(e.CurrentEntry.FullPath,
				data.GetService(e.CurrentEntry.ServiceId));
			statusLabel.Text = string.Format(Resources.IDS_MODLIBRARY_STATUS_DOWNLOADING_BATCH,
				e.RemainingCount, relativePath);

			// Update progress bar (show indeterminate style since count is dynamic)
			progressBar.Visible = true;
			progressBar.Style = ProgressBarStyle.Marquee;

			// Show cancel button
			cancelDownloadButton.Visible = true;
		}



		/********************************************************************/
		/// <summary>
		/// Is called when a single download completes
		/// </summary>
		/********************************************************************/
		private void DownloadQueue_DownloadCompleted(object sender, DownloadCompletedEventArgs e)
		{
			if (isClosing)
			{
				return;
			}

			if (InvokeRequired)
			{
				Invoke(() => DownloadQueue_DownloadCompleted(sender, e));
				return;
			}

			if (e.Success)
			{
				// Add to playlist only if addDownloadsToPlaylist flag is set
				if (addDownloadsToPlaylist)
				{
					playlistIntegration.AddToPlaylist(e.LocalPath, e.ShouldPlayImmediately);
				}
			}
			else
			{
				// Show error
				statusLabel.Text = string.Format(Resources.IDS_MODLIBRARY_STATUS_ERROR_PREFIX, e.ErrorMessage);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the entire download queue completes
		/// </summary>
		/********************************************************************/
		private void DownloadQueue_QueueCompleted(object sender, EventArgs e)
		{
			if (isClosing)
			{
				return;
			}

			if (InvokeRequired)
			{
				Invoke(() => DownloadQueue_QueueCompleted(sender, e));
				return;
			}

			// Hide progress UI
			cancelDownloadButton.Visible = false;
			progressBar.Visible = false;
			progressBar.Style = ProgressBarStyle.Blocks;

			// Show completion summary
			int successCount = downloadQueueManager.SuccessCount;
			int failureCount = downloadQueueManager.FailureCount;

			if (failureCount > 0)
			{
				statusLabel.Text = string.Format(Resources.IDS_MODLIBRARY_STATUS_DOWNLOAD_COMPLETED_WITH_ERRORS,
					successCount, failureCount);
			}
			else
			{
				statusLabel.Text = string.Format(Resources.IDS_MODLIBRARY_STATUS_DOWNLOAD_COMPLETED_SUCCESS,
					successCount);
			}
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Get the current ModLibrary path from settings
		/// </summary>
		/********************************************************************/
		private string GetCurrentModLibraryPath()
		{
			PathSettings pathSettings = DependencyInjection.Container.GetInstance<PathSettings>();
			return pathSettings.ModLibrary ?? string.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// Update the window title, showing custom path if configured
		/// </summary>
		/********************************************************************/
		private void UpdateWindowTitle()
		{
			string title = Resources.IDS_MODLIBRARY_TITLE;

			// Use the remembered path
			if (!string.IsNullOrEmpty(initialModLibraryPath))
			{
				title = $"{title} - {initialModLibraryPath}";
			}

			Text = title;
		}



		/********************************************************************/
		/// <summary>
		/// Get the configured ModLibrary modules path, or fallback to
		/// default
		/// </summary>
		/********************************************************************/
		private string GetModLibraryModulesPath()
		{
			// Use the cached path from initialization or last reload
			if (string.IsNullOrEmpty(initialModLibraryPath))
				// Fallback to default location
			{
				return Path.Combine(cacheDirectory, "Modules");
			}

			return initialModLibraryPath;
		}



		/********************************************************************/
		/// <summary>
		/// Check if a file should be included (filters out smpl.* files)
		/// </summary>
		/********************************************************************/
		private bool ShouldIncludeFile(string nameWithPath)
		{
			// Skip sample files (smpl.*)
			string fileName = nameWithPath.Substring(nameWithPath.LastIndexOf('/') + 1);
			return !fileName.StartsWith("smpl.", StringComparison.OrdinalIgnoreCase);
		}



		/********************************************************************/
		/// <summary>
		/// Get local file path for a tree entry
		/// </summary>
		/********************************************************************/
		private string GetLocalPathForEntry(TreeNode entry)
		{
			return data.IsOfflineMode ? GetLocalPathForLocalMode(entry) : GetLocalPathForOnlineMode(entry);
		}



		/********************************************************************/
		/// <summary>
		/// Get local file path for local mode
		/// </summary>
		/********************************************************************/
		private string GetLocalPathForLocalMode(TreeNode entry)
		{
			string modulesBasePath = GetModLibraryModulesPath();
			// entry.FullPath is already the relative path from Modules directory
			return Path.Combine(modulesBasePath, entry.FullPath.Replace('/', Path.DirectorySeparatorChar));
		}



		/********************************************************************/
		/// <summary>
		/// Get local file path for online mode
		/// </summary>
		/********************************************************************/
		private string GetLocalPathForOnlineMode(TreeNode entry)
		{
			var service = data.GetService(entry.ServiceId);
			if (service == null)
			{
				return null;
			}

			// Get relative path without service prefix
			string relativePath = data.GetRelativePathFromService(entry.FullPath, service);

			// Build local file path
			string modulesBasePath = GetModLibraryModulesPath();
			return Path.Combine(modulesBasePath, service.FolderName,
				relativePath.Replace('/', Path.DirectorySeparatorChar));
		}



		/********************************************************************/
		/// <summary>
		/// Initialize cache directory for downloaded files
		/// </summary>
		/********************************************************************/
		private void InitializeCacheDirectory()
		{
			cacheDirectory = Path.Combine(platformPath.SettingsPath, "ModLibrary");
			Directory.CreateDirectory(cacheDirectory);
		}



		/********************************************************************/
		/// <summary>
		/// Initialize available services
		/// </summary>
		/********************************************************************/
		private void InitializeServices()
		{
			// ModLand service
			string modlandCacheDir = Path.Combine(cacheDirectory, "ModLand");
			Directory.CreateDirectory(modlandCacheDir);

			data.AddService(new ModuleService
			{
				Id = "modland",
				DisplayName = "ModLand",
				FolderName = "ModLand",
				RootPath = "modland://",
				IsLoaded = false
			});

			// Initialize helper services
			downloadService = new ModLibraryDownloadService(data, GetModLibraryModulesPath());
			downloadQueueManager = new DownloadQueueManager(downloadService);
			downloadQueueManager.ProgressChanged += DownloadQueue_ProgressChanged;
			downloadQueueManager.DownloadCompleted += DownloadQueue_DownloadCompleted;
			downloadQueueManager.QueueCompleted += DownloadQueue_QueueCompleted;
			playlistIntegration = new ModLibraryPlaylistIntegration(mainWindow);
		}



		/********************************************************************/
		/// <summary>
		/// Check if we already have a downloaded database
		/// </summary>
		/********************************************************************/
		private async void CheckExistingDatabase()
		{
			var modland = data.GetService("modland");
			if (modland == null)
			{
				return;
			}

			string allmodsTxtPath = Path.Combine(cacheDirectory, "ModLand", "allmods.txt");

			// Always scan local files at startup
			statusLabel.Text = Resources.IDS_MODLIBRARY_STATUS_SCANNING_LOCAL_FILES;
			await Task.Run(() =>
			{
				ScanLocalFiles();
			});

			if (File.Exists(allmodsTxtPath))
			{
				FileInfo fileInfo = new(allmodsTxtPath);
				modland.LastUpdate = fileInfo.LastWriteTime;

				long fileSizeKB = fileInfo.Length / 1024;
				statusLabel.Text = string.Format(Resources.IDS_MODLIBRARY_STATUS_DB_FOUND_LOADING,
					fileInfo.LastWriteTime.ToString("yyyy-MM-dd"), fileSizeKB);

				// Show "Loading..." in ListView
				ShowListViewStatus(Resources.IDS_MODLIBRARY_STATUS_LOADING_DATABASE);

				// Parse and load the existing database in background
				await Task.Run(() => ParseModLandDatabase(modland, allmodsTxtPath));
				modland.IsLoaded = true;

				// Build full tree from loaded data (will fire DataLoaded event when done)
				statusLabel.Text = Resources.IDS_MODLIBRARY_STATUS_BUILDING_TREE_STRUCTURE;
				RebuildTree();

				// Check if database is older than 7 days and show info bar (non-blocking)
				int daysOld = (int)(DateTime.Now - modland.LastUpdate).TotalDays;
				if (daysOld >= 7 && !settings.IsUpdateIgnored("modland"))
				{
					ShowDatabaseOldInfoBar("modland", modland.DisplayName, daysOld);
				}
			}
			else
				// Build tree to show services (even though database is not loaded)
				// This will fire OnDataLoaded which will set proper status
			{
				RebuildTree();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Download ModLand database
		/// </summary>
		/********************************************************************/
		private async Task DownloadModLandDatabase()
		{
			var modland = data.GetService("modland");
			if (modland == null)
			{
				return;
			}

			string modlandCacheDir = Path.Combine(cacheDirectory, "ModLand");
			string allmodsZipPath = Path.Combine(modlandCacheDir, "allmods.zip");
			string allmodsTxtPath = Path.Combine(modlandCacheDir, "allmods.txt");

			BeginBusy();

			try
			{
				// Show "Updating database..." in ListView (high-level status)
				ShowListViewStatus(Resources.IDS_MODLIBRARY_STATUS_UPDATING_DATABASE);

				progressBar.Visible = true;
				progressBar.Style = ProgressBarStyle.Marquee;
				statusLabel.Text = Resources.IDS_MODLIBRARY_STATUS_DOWNLOADING_ALLMODS;

				using (HttpClient client = new())
				{
					// Download the file
					byte[] fileBytes = await client.GetByteArrayAsync(AllmodsZipUrl);

					// Save to disk
					await File.WriteAllBytesAsync(allmodsZipPath, fileBytes);

					statusLabel.Text = Resources.IDS_MODLIBRARY_STATUS_EXTRACTING_ALLMODS;

					// Extract allmods.txt from the ZIP
					using (var archive = ZipFile.OpenRead(allmodsZipPath))
					{
						var entry = archive.GetEntry("allmods.txt");
						if (entry != null)
						{
							entry.ExtractToFile(allmodsTxtPath, true);
						}
					}

					// Update status
					FileInfo fileInfo = new(allmodsTxtPath);
					modland.LastUpdate = fileInfo.LastWriteTime;

					long fileSizeKB = fileInfo.Length / 1024;
					statusLabel.Text = string.Format(Resources.IDS_MODLIBRARY_STATUS_DOWNLOAD_COMPLETE, fileSizeKB,
						fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm"));

					progressBar.Style = ProgressBarStyle.Continuous;
					progressBar.Value = 100;

					statusLabel.Text = Resources.IDS_MODLIBRARY_STATUS_PARSING_DATABASE;

					// Parse and load the database in background
					await Task.Run(() => ParseModLandDatabase(modland, allmodsTxtPath));
					modland.IsLoaded = true;

					// Build full tree from loaded data (will fire DataLoaded event when done)
					statusLabel.Text = Resources.IDS_MODLIBRARY_STATUS_BUILDING_TREE_STRUCTURE;
					RebuildTree();
				}
			}
			catch (Exception ex)
			{
				statusLabel.Text = string.Format(Resources.IDS_MODLIBRARY_STATUS_ERROR_PREFIX, ex.Message);
				MessageBox.Show(string.Format(Resources.IDS_MODLIBRARY_MSGBOX_FAILED_DOWNLOAD, ex.Message),
					Resources.IDS_MODLIBRARY_MSGBOX_TITLE_DOWNLOAD_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

				// Restore tree view to show services (remove "Updating database..." status)
				RebuildTree();
			}
			finally
			{
				EndBusy();
				progressBar.Visible = false;
				moduleListView.Enabled = true;
				Cursor = Cursors.Default;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse ModLand allmods.txt and create ModEntry objects
		/// </summary>
		/********************************************************************/
		private void ParseModLandDatabase(ModuleService service, string allmodsTxtPath)
		{
			if (!File.Exists(allmodsTxtPath))
			{
				return;
			}

			service.ClearOnlineFiles();

			// Step 1: Parse all lines into (nameWithPath, size) tuples
			List<(string nameWithPath, long size)> entries = new();

			foreach (string line in File.ReadLines(allmodsTxtPath))
			{
				// Format: "12345    path/to/file.ext"
				string[] parts = line.Split(new[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length < 2)
				{
					continue;
				}

				if (!long.TryParse(parts[0], out long fileSize))
				{
					continue;
				}

				string nameWithPath = string.Join(" ", parts.Skip(1));

				// Skip files that should not be included (e.g., smpl.*)
				if (!ShouldIncludeFile(nameWithPath))
				{
					continue;
				}

				entries.Add((nameWithPath, fileSize));
			}

			// Step 2: Sort strings (fast!)
			entries.Sort((a, b) => string.Compare(a.nameWithPath, b.nameWithPath, StringComparison.OrdinalIgnoreCase));

			// Step 3: Create ModEntry objects from sorted data
			foreach (var entry in entries)
			{
				service.AddOnlineFile(entry.nameWithPath, entry.size);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Check if a busy operation is in progress
		/// </summary>
		/********************************************************************/
		private bool IsBusy => busyCount > 0;



		/********************************************************************/
		/// <summary>
		/// Begin a busy operation (blocks tab switching)
		/// </summary>
		/********************************************************************/
		private void BeginBusy()
		{
			busyCount++;
			modeTabControl.Enabled = false;
		}



		/********************************************************************/
		/// <summary>
		/// End a busy operation
		/// </summary>
		/********************************************************************/
		private void EndBusy()
		{
			if (busyCount > 0)
			{
				busyCount--;
			}

			if (busyCount == 0)
			{
				modeTabControl.Enabled = true;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Show status message in ListView (e.g., "Loading...",
		/// "Searching...")
		/// </summary>
		/********************************************************************/
		private void ShowListViewStatus(string message)
		{
			currentEntries = new List<TreeNode>
			{
				new()
				{
					Name = message,
					IsDirectory = false,
					FullPath = string.Empty,
					Size = 0,
					ServiceId = string.Empty
				}
			};
			moduleListView.VirtualListSize = 1;
			moduleListView.Invalidate();
			moduleListView.Enabled = false;
			Cursor = Cursors.WaitCursor;
		}



		/********************************************************************/
		/// <summary>
		/// Rebuild tree with current search filter
		/// </summary>
		/********************************************************************/
		private void RebuildTree()
		{
			SearchMode searchMode = (SearchMode)searchModeComboBox.SelectedIndex;
			data.BuildTree(searchTextBox.Text.Trim(), searchMode, flatViewCheckBox.Checked);
		}



		/********************************************************************/
		/// <summary>
		/// Load current directory into ListView
		/// </summary>
		/********************************************************************/
		private void LoadCurrentDirectory(string itemToSelect = null)
		{
			// Reset virtual list size first to prevent invalid RetrieveVirtualItem calls
			moduleListView.VirtualListSize = 0;

			// Get entries from data class (already sorted)
			FlatViewSortOrder sortOrder = (FlatViewSortOrder)settings.FlatViewSortOrder;
			currentEntries = data.GetEntries(currentPath, flatViewCheckBox.Checked, sortOrder);

			// Apply favorites filter (only in offline mode when checkbox is checked)
			if (data.IsOfflineMode && favoritesOnlyCheckBox.Checked)
			{
				currentEntries = currentEntries.Where(e =>
					(!e.IsDirectory && favorites.IsFavorite(e.FullPath)) ||
					(e.IsDirectory && DirectoryContainsFavorites(e))).ToList();
			}

			// Update column headers with sort indicators
			UpdateColumnHeaders();

			// Update breadcrumb and parent button
			UpdateBreadcrumbAndParentButton();

			// Update virtual list
			moduleListView.VirtualListSize = currentEntries.Count;
			moduleListView.Invalidate();

			// Select specific item or default to first
			SelectItemInList(itemToSelect);
		}



		/********************************************************************/
		/// <summary>
		/// Update breadcrumb label and parent button state
		/// </summary>
		/********************************************************************/
		private void UpdateBreadcrumbAndParentButton()
		{
			// Count folders and files in current entries
			int folderCount = currentEntries.Count(e => e.IsDirectory);
			int fileCount = currentEntries.Count - folderCount;

			// Clear breadcrumb panel
			breadcrumbPanel.Controls.Clear();

			if (string.IsNullOrEmpty(currentPath))
			{
				parentButton.Enabled = false;

				// If search is active, show search indicator
				if (!string.IsNullOrEmpty(data.SearchFilter))
				{
					AddBreadcrumbLabel($"🔍 \"{data.SearchFilter}\" - ");
				}

				AddBreadcrumbLink(Resources.IDS_MODLIBRARY_HOME, string.Empty);

				// Add counts in gray
				AddBreadcrumbSeparator("•");
				Label countsLabel = new() {Text = string.Format(Resources.IDS_MODLIBRARY_FOLDERS_FILES_COUNT, folderCount, fileCount), AutoSize = true, Margin = new Padding(0, 5, 0, 0), ForeColor = SystemColors.GrayText};
				breadcrumbPanel.Controls.Add(countsLabel);

				// Add total files if searching
				if (!string.IsNullOrEmpty(data.SearchFilter))
				{
					int totalFiles = data.CountTotalFilesInFilteredCache();
					Label totalLabel = new() {Text = ", " + string.Format(Resources.IDS_MODLIBRARY_TOTAL_FILES_COUNT, totalFiles), AutoSize = true, Margin = new Padding(0, 5, 0, 0), ForeColor = SystemColors.GrayText};
					breadcrumbPanel.Controls.Add(totalLabel);
				}
			}
			else if (!string.IsNullOrEmpty(data.SearchFilter))
			{
				parentButton.Enabled = true;
				int totalFiles = data.CountTotalFilesInFilteredCache();

				// Build clickable breadcrumb for search
				AddBreadcrumbSearchPath(folderCount, fileCount, totalFiles);
			}
			else
			{
				parentButton.Enabled = true;

				// Build clickable breadcrumb for normal navigation
				BuildClickableBreadcrumb(folderCount, fileCount);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Build clickable breadcrumb path
		/// </summary>
		/********************************************************************/
		private void BuildClickableBreadcrumb(int folderCount, int fileCount)
		{
			// Add "Home" link
			AddBreadcrumbLink(Resources.IDS_MODLIBRARY_HOME, string.Empty);
			AddBreadcrumbSeparator();

			// Check if path is a service root (ends with "://")
			if (currentPath.EndsWith("://"))
			{
				// Service root like "modland://" - use DisplayName from service
				var service = data.GetServiceFromPath(currentPath);
				string serviceName = service?.DisplayName ?? currentPath.Substring(0, currentPath.Length - 3);
				// Make service root clickable (allows reload/refresh)
				AddBreadcrumbLink(serviceName, currentPath);

				// Add counts in gray
				AddBreadcrumbSeparator("•");
				Label countsLabel = new() {Text = string.Format(Resources.IDS_MODLIBRARY_FOLDERS_FILES_COUNT, folderCount, fileCount), AutoSize = true, Margin = new Padding(0, 5, 0, 0), ForeColor = SystemColors.GrayText};
				breadcrumbPanel.Controls.Add(countsLabel);
				return;
			}

			// Split path into parts, filtering out empty entries
			string[] parts = currentPath.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
			string pathSoFar = string.Empty;

			for (int i = 0; i < parts.Length; i++)
			{
				// Handle service protocol parts (like "modland:")
				if (parts[i].EndsWith(":"))
				{
					pathSoFar = parts[i] + "//";
					// Get service DisplayName instead of using path part
					var service = data.GetServiceFromPath(pathSoFar);
					string serviceName = service?.DisplayName ?? parts[i].TrimEnd(':');
					AddBreadcrumbLink(serviceName, pathSoFar);
					AddBreadcrumbSeparator();
					continue;
				}

				if (!string.IsNullOrEmpty(pathSoFar) && !pathSoFar.EndsWith("/"))
				{
					pathSoFar += "/";
				}

				pathSoFar += parts[i];

				// All parts are clickable links
				AddBreadcrumbLink(parts[i], pathSoFar);

				if (i < parts.Length - 1)
				{
					AddBreadcrumbSeparator();
				}
			}

			// Add counts in gray after path
			AddBreadcrumbSeparator("•");
			Label countsLabel2 = new() {Text = string.Format(Resources.IDS_MODLIBRARY_FOLDERS_FILES_COUNT, folderCount, fileCount), AutoSize = true, Margin = new Padding(0, 5, 0, 0), ForeColor = SystemColors.GrayText};
			breadcrumbPanel.Controls.Add(countsLabel2);
		}



		/********************************************************************/
		/// <summary>
		/// Add breadcrumb for search mode
		/// </summary>
		/********************************************************************/
		private void AddBreadcrumbSearchPath(int folderCount, int fileCount, int totalFiles)
		{
			// Add search indicator
			AddBreadcrumbLabel($"🔍 \"{data.SearchFilter}\" - ");

			// Add clickable path if not at root
			if (string.IsNullOrEmpty(currentPath))
			{
				AddBreadcrumbLink(Resources.IDS_MODLIBRARY_HOME, string.Empty);
			}
			else
			{
				// Add "Home" link
				AddBreadcrumbLink(Resources.IDS_MODLIBRARY_HOME, string.Empty);
				AddBreadcrumbSeparator();

				// Add path parts
				BuildClickablePath(currentPath);
			}

			// Add counts in gray
			AddBreadcrumbSeparator("•");
			Label countsLabel = new() {Text = string.Format(Resources.IDS_MODLIBRARY_SEARCH_COUNT, folderCount, fileCount, totalFiles), AutoSize = true, Margin = new Padding(0, 5, 0, 0), ForeColor = SystemColors.GrayText};
			breadcrumbPanel.Controls.Add(countsLabel);
		}



		/********************************************************************/
		/// <summary>
		/// Build clickable path parts (without counts at end)
		/// </summary>
		/********************************************************************/
		private void BuildClickablePath(string path)
		{
			// Check if path is a service root (ends with "://")
			if (path.EndsWith("://"))
			{
				var service = data.GetServiceFromPath(path);
				string serviceName = service?.DisplayName ?? path.Substring(0, path.Length - 3);
				AddBreadcrumbLink(serviceName, path);
				return;
			}

			// Split path into parts, filtering out empty entries
			string[] parts = path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
			string pathSoFar = string.Empty;

			for (int i = 0; i < parts.Length; i++)
			{
				// Handle service protocol parts (like "modland:")
				if (parts[i].EndsWith(":"))
				{
					pathSoFar = parts[i] + "//";
					var service = data.GetServiceFromPath(pathSoFar);
					string serviceName = service?.DisplayName ?? parts[i].TrimEnd(':');
					AddBreadcrumbLink(serviceName, pathSoFar);
					AddBreadcrumbSeparator();
					continue;
				}

				if (!string.IsNullOrEmpty(pathSoFar) && !pathSoFar.EndsWith("/"))
				{
					pathSoFar += "/";
				}

				pathSoFar += parts[i];

				// All parts are links
				AddBreadcrumbLink(parts[i], pathSoFar);

				if (i < parts.Length - 1)
				{
					AddBreadcrumbSeparator();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Add a clickable breadcrumb link
		/// </summary>
		/********************************************************************/
		private void AddBreadcrumbLink(string text, string path)
		{
			LinkLabel link = new() {Text = text, AutoSize = true, Margin = new Padding(0, 5, 0, 0), Tag = path};
			link.LinkClicked += BreadcrumbLink_Clicked;
			breadcrumbPanel.Controls.Add(link);
		}



		/********************************************************************/
		/// <summary>
		/// Add a non-clickable breadcrumb label
		/// </summary>
		/********************************************************************/
		private void AddBreadcrumbLabel(string text)
		{
			Label label = new() {Text = text, AutoSize = true, Margin = new Padding(0, 5, 0, 0)};
			breadcrumbPanel.Controls.Add(label);
		}



		/********************************************************************/
		/// <summary>
		/// Add a breadcrumb separator
		/// </summary>
		/********************************************************************/
		private void AddBreadcrumbSeparator(string text = " > ")
		{
			Label separator = new() {Text = text == " > " ? text : $" {text} ", AutoSize = true, Margin = new Padding(0, 5, 0, 0), ForeColor = SystemColors.GrayText};
			breadcrumbPanel.Controls.Add(separator);
		}



		/********************************************************************/
		/// <summary>
		/// Handle breadcrumb link click
		/// </summary>
		/********************************************************************/
		private void BreadcrumbLink_Clicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (sender is LinkLabel link && link.Tag is string path)
			{
				// Remember the folder we're leaving to select it in the parent directory
				string folderToSelect = null;

				// Only calculate folderToSelect if we're navigating to a different (parent) path
				if (!string.IsNullOrEmpty(currentPath) && currentPath != path)
					// Extract the folder name that we're currently in
				{
					if (currentPath.StartsWith(path))
					{
						// Remove the parent path to get the child folder name
						string remainingPath = currentPath.Substring(path.Length);
						if (remainingPath.StartsWith("/"))
						{
							remainingPath = remainingPath.Substring(1);
						}

						// Get first folder in the remaining path
						int slashIndex = remainingPath.IndexOf('/');
						folderToSelect = slashIndex > 0 ? remainingPath.Substring(0, slashIndex) : remainingPath;
					}
				}

				// Always allow navigation, even if currentPath == path (e.g., when clicking service root from subdirectory in search mode)
				currentPath = path;
				LoadCurrentDirectory(folderToSelect);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Select an item in the list view
		/// </summary>
		/********************************************************************/
		private void SelectItemInList(string itemToSelect)
		{
			if (currentEntries.Count == 0)
			{
				return;
			}

			moduleListView.SelectedIndices.Clear();

			int indexToSelect = 0;

			if (!string.IsNullOrEmpty(itemToSelect))
			{
				for (int i = 0; i < currentEntries.Count; i++)
				{
					var entry = currentEntries[i];

					if (entry.Name.Equals(itemToSelect, StringComparison.OrdinalIgnoreCase) ||
					    (entry.IsDirectory && entry.Name.StartsWith(itemToSelect, StringComparison.OrdinalIgnoreCase)))
					{
						indexToSelect = i;
						break;
					}
				}
			}

			moduleListView.SelectedIndices.Add(indexToSelect);
			moduleListView.EnsureVisible(indexToSelect);
			moduleListView.Focus();
		}



		/********************************************************************/
		/// <summary>
		/// Update column headers with sort indicators
		/// </summary>
		/********************************************************************/
		private void UpdateColumnHeaders()
		{
			// Only show sort indicators in flat view mode
			if (!flatViewCheckBox.Checked)
			{
				columnName.Text = Resources.IDS_MODLIBRARY_COLUMN_NAME;
				columnPath.Text = Resources.IDS_MODLIBRARY_COLUMN_PATH;
				return;
			}

			FlatViewSortOrder sortOrder = (FlatViewSortOrder)settings.FlatViewSortOrder;

			// Update column headers with Unicode arrows
			if (sortOrder == FlatViewSortOrder.NameThenPath)
			{
				columnName.Text = Resources.IDS_MODLIBRARY_COLUMN_NAME + " ▲";
				columnPath.Text = Resources.IDS_MODLIBRARY_COLUMN_PATH;
			}
			else // PathThenName
			{
				columnName.Text = Resources.IDS_MODLIBRARY_COLUMN_NAME;
				columnPath.Text = Resources.IDS_MODLIBRARY_COLUMN_PATH + " ▲";
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle column header clicks to change sort order
		/// </summary>
		/********************************************************************/
		private void ModuleListView_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			// Only allow sorting in flat view mode
			if (!flatViewCheckBox.Checked)
			{
				return;
			}

			// Determine which column was clicked
			if (e.Column == 0) // Name column
			{
				settings.FlatViewSortOrder = (int)FlatViewSortOrder.NameThenPath;
			}
			else if (e.Column == 1) // Path column
			{
				settings.FlatViewSortOrder = (int)FlatViewSortOrder.PathThenName;
			}
			else
				// Size column or other - ignore
			{
				return;
			}

			// Reload current directory with new sort order
			LoadCurrentDirectory();
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
				{
					return;
				}

				// Check if file exists
				if (File.Exists(localPath))
				{
					statusLabel.Text = string.Format(Resources.IDS_MODLIBRARY_STATUS_PLAYING_FILE, entry.Name);

					// Add to playlist using the main window API
					if (mainWindow.Form is MainWindowForm mainWindowForm)
					{
						mainWindowForm.Invoke((Action)(() =>
						{
							// Check if module already exists in playlist
							var existingItem = mainWindowForm.FindModuleInList(localPath);
							if (existingItem != null)
							{
								// Module already in list - select and play it
								if (playImmediatelyCheckBox.Checked)
								{
									mainWindowForm.SelectAndPlayModule(existingItem);
								}
							}
							else
								// Add the file to the list and optionally play it immediately
							{
								mainWindowForm.AddFilesToModuleList(new[] {localPath},
									playImmediatelyCheckBox.Checked);
							}
						}));
					}
				}
				else
				{
					statusLabel.Text = string.Format(Resources.IDS_MODLIBRARY_STATUS_FILE_NOT_FOUND, entry.Name);
					MessageBox.Show(string.Format(Resources.IDS_MODLIBRARY_MSGBOX_FILE_NOT_DOWNLOADED, entry.Name),
						Resources.IDS_MODLIBRARY_MSGBOX_TITLE_FILE_NOT_FOUND, MessageBoxButtons.OK,
						MessageBoxIcon.Information);
				}
			}
			catch (Exception ex)
			{
				statusLabel.Text = string.Format(Resources.IDS_MODLIBRARY_STATUS_ERROR_PREFIX, ex.Message);
				MessageBox.Show(string.Format(Resources.IDS_MODLIBRARY_MSGBOX_FAILED_PLAY_MODULE, ex.Message),
					Resources.IDS_MODLIBRARY_MSGBOX_TITLE_PLAY_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Scan local files for a service
		/// </summary>
		/********************************************************************/
		private void ScanLocalFiles()
		{
			// Clear local files list
			data.ClearLocalFiles();

			string modulesDir = GetModLibraryModulesPath();

			if (!Directory.Exists(modulesDir))
			{
				return;
			}

			// Recursively scan all files in the unified Modules folder
			string[] allFiles = Directory.GetFiles(modulesDir, "*", SearchOption.AllDirectories);

			// Step 1: Build list of (nameWithPath, size) tuples
			List<(string nameWithPath, long size)> entries = new();

			foreach (string filePath in allFiles)
			{
				// Get relative path from Modules directory (includes full path with service folder)
				string fullRelativePath =
					filePath.Substring(modulesDir.Length + 1).Replace(Path.DirectorySeparatorChar, '/');

				// Skip files that should not be included (e.g., smpl.*)
				if (!ShouldIncludeFile(fullRelativePath))
				{
					continue;
				}

				// Get file size
				FileInfo fileInfo = new(filePath);
				long fileSize = fileInfo.Length;

				entries.Add((fullRelativePath, fileSize));
			}

			// Step 2: Sort by path
			entries.Sort((a, b) => string.Compare(a.nameWithPath, b.nameWithPath, StringComparison.OrdinalIgnoreCase));

			// Step 3: Add all files to local files list
			foreach (var entry in entries)
			{
				data.AddLocalFile(entry.nameWithPath, entry.size);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Format file size in KB, MB, GB like Windows Explorer
		/// </summary>
		/********************************************************************/
		private string FormatFileSize(long bytes)
		{
			if (bytes < 1024)
			{
				return $"{bytes} bytes";
			}

			if (bytes < 1024 * 1024)
			{
				return $"{Math.Ceiling(bytes / 1024.0):0} KB";
			}

			if (bytes < 1024 * 1024 * 1024)
			{
				return $"{Math.Round(bytes / (1024.0 * 1024.0), 1):0.0} MB";
			}

			return $"{Math.Round(bytes / (1024.0 * 1024.0 * 1024.0), 2):0.00} GB";
		}



		/********************************************************************/
		/// <summary>
		/// Count files recursively in a tree node
		/// </summary>
		/********************************************************************/
		private int CountFilesInTree(TreeNode node)
		{
			int count = 0;

			foreach (var child in node.Children)
			{
				if (child.IsDirectory)
					// Recursively count files in subdirectories
				{
					count += CountFilesInTree(child);
				}
				else
					// Count this file
				{
					count++;
				}
			}

			return count;
		}



		/********************************************************************/
		/// <summary>
		/// Check if a directory contains any favorites (recursively)
		/// </summary>
		/********************************************************************/
		private bool DirectoryContainsFavorites(TreeNode directory)
		{
			// Check if the directory itself is a favorite
			if (favorites.IsFavorite(directory.FullPath))
			{
				return true;
			}

			// Check children recursively
			foreach (var child in directory.Children)
			{
				if (child.IsDirectory)
				{
					if (DirectoryContainsFavorites(child))
					{
						return true;
					}
				}
				else
				{
					if (favorites.IsFavorite(child.FullPath))
					{
						return true;
					}
				}
			}

			return false;
		}
		#endregion

		#region InfoBar methods
		/********************************************************************/
		/// <summary>
		/// Show an info bar warning that the database is old
		/// </summary>
		/********************************************************************/
		private void ShowDatabaseOldInfoBar(string serviceId, string serviceName, int daysOld)
		{
			// Don't show if already visible
			if (activeInfoBars.ContainsKey(serviceId))
			{
				return;
			}

			InfoBarControl infoBar = new();
			infoBar.Configure(
				serviceId,
				string.Format(Resources.IDS_MODLIBRARY_INFOBAR_DATABASE_OLD, serviceName, daysOld),
				Resources.IDS_MODLIBRARY_INFOBAR_UPDATE_NOW,
				Resources.IDS_MODLIBRARY_INFOBAR_IGNORE,
				Resources.IDS_MODLIBRARY_INFOBAR_DISMISS
			);

			infoBar.ActionClicked += InfoBar_ActionClicked;
			infoBar.IgnoreClicked += InfoBar_IgnoreClicked;
			infoBar.CloseClicked += InfoBar_CloseClicked;

			activeInfoBars[serviceId] = infoBar;
			infoBarPanel.Controls.Add(infoBar);
			infoBarPanel.Visible = true;
		}



		/********************************************************************/
		/// <summary>
		/// Hide info bar for a specific service
		/// </summary>
		/********************************************************************/
		private void HideInfoBar(string serviceId)
		{
			if (activeInfoBars.TryGetValue(serviceId, out var infoBar))
			{
				infoBarPanel.Controls.Remove(infoBar);
				activeInfoBars.Remove(serviceId);
				infoBar.Dispose();

				// Hide panel if no more info bars
				if (infoBarPanel.Controls.Count == 0)
				{
					infoBarPanel.Visible = false;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle "Update Now" button click on info bar
		/// </summary>
		/********************************************************************/
		private async void InfoBar_ActionClicked(object sender, InfoBarActionEventArgs e)
		{
			// Hide the info bar
			HideInfoBar(e.ServiceId);

			// Reset ignore flag
			settings.SetUpdateIgnored(e.ServiceId, false);

			// Trigger update
			if (e.ServiceId == "modland")
			{
				await DownloadModLandDatabase();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle "Ignore" button click on info bar
		/// </summary>
		/********************************************************************/
		private void InfoBar_IgnoreClicked(object sender, InfoBarActionEventArgs e)
		{
			// Set ignore flag
			settings.SetUpdateIgnored(e.ServiceId, true);

			// Hide the info bar
			HideInfoBar(e.ServiceId);
		}



		/********************************************************************/
		/// <summary>
		/// Handle close button click on info bar
		/// </summary>
		/********************************************************************/
		private void InfoBar_CloseClicked(object sender, InfoBarActionEventArgs e)
		{
			// Just hide without setting ignore flag
			HideInfoBar(e.ServiceId);
		}
		#endregion
	}
}
