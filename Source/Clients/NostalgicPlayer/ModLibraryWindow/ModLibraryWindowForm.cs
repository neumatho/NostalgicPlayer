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
using Microsoft.Extensions.DependencyInjection;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Bases;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;
using Polycode.NostalgicPlayer.Kit.Helpers;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.ModLibraryWindow
{
	/// <summary>
	/// This shows the Module Library window
	/// </summary>
	public partial class ModLibraryWindowForm : WindowFormBase, IModLibraryWindowApi
	{
		private const string AllmodsZipUrl = "https://modland.com/allmods.zip";
		private const string ModlandModulesUrl = "https://modland.com/pub/modules/";

		private readonly ModLibraryData data = new();

		private readonly IMainWindowApi mainWindow;
		private readonly Timer searchDebounceTimer;
		private readonly Timer searchTimer;
		private readonly ModLibrarySettings settings;

		private string cacheDirectory;
		private List<TreeNode> currentEntries = new();
		private string currentPath = string.Empty;
		private string initialModLibraryPath;
		private string searchBuffer = string.Empty;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModLibraryWindowForm(IMainWindowApi mainWindow, OptionSettings optionSettings)
		{
			InitializeComponent();

			this.mainWindow = mainWindow;
			Disposed += ModLibraryWindowForm_Disposed;

			// Copy icon from main window
			if (mainWindow is Form mainForm) Icon = mainForm.Icon;

			// Set context menu items (KryptonContextMenuItem cannot use ControlResource)
			updateDatabaseItem.Text = Resources.IDS_MODLIBRARY_UPDATE_DATABASE;
			clearDatabaseItem.Text = Resources.IDS_MODLIBRARY_CLEAR_DATABASE;
			deleteItem.Text = Resources.IDS_MODLIBRARY_DELETE;
			jumpToFolderItem.Text = Resources.IDS_MODLIBRARY_JUMP_TO_FOLDER;

			// Set column headers (ColumnHeader cannot use ControlResource)
			columnName.Text = Resources.IDS_MODLIBRARY_COLUMN_NAME;
			columnPath.Text = Resources.IDS_MODLIBRARY_COLUMN_PATH;
			columnSize.Text = Resources.IDS_MODLIBRARY_COLUMN_SIZE;

			// Set search mode combo box items (cannot use ControlResource)
			searchModeComboBox.Items.Clear();
			searchModeComboBox.Items.AddRange(Resources.IDS_MODLIBRARY_SEARCHMODE_FILENAME_AND_PATH,
				Resources.IDS_MODLIBRARY_SEARCHMODE_FILENAME_ONLY, Resources.IDS_MODLIBRARY_SEARCHMODE_PATH_ONLY);

			if (!DesignMode)
			{
				InitializeWindow(mainWindow, optionSettings);

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

				// Initialize search timer for keyboard quick search
				searchTimer = new Timer();
				searchTimer.Interval = 1000; // 1 second timeout
				searchTimer.Tick += (s, ev) =>
				{
					searchBuffer = string.Empty;
					searchTimer.Stop();
				};

				// Initialize debounce timer for search textbox
				searchDebounceTimer = new Timer();
				searchDebounceTimer.Interval = 100; // 100ms debounce
				searchDebounceTimer.Tick += SearchDebounceTimer_Tick;

				// Load and apply ModLibrary settings (must be done AFTER timer initialization)
				settings = new ModLibrarySettings(allWindowSettings);
				data.IsOfflineMode = settings.IsOfflineMode;
				modeTabControl.SelectedIndex = settings.IsOfflineMode ? 1 : 0;
				flatViewCheckBox.Checked = settings.FlatViewEnabled;
				playImmediatelyCheckBox.Checked = settings.PlayImmediately;
				searchTextBox.Text = settings.SearchText;
				// Set SelectedIndex which will trigger the event handler, but now settings is initialized
				searchModeComboBox.SelectedIndex = settings.SearchMode;

				// Restore last path based on mode
				currentPath = data.IsOfflineMode ? settings.LastOfflinePath : settings.LastOnlinePath;

				// Check if we already have the database
				CheckExistingDatabase();

				// Hook up data loaded event
				data.DataLoaded += OnDataLoaded;
			}
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
		public void ReloadChanges()
		{
			// Get current path from settings
			string currentPath = GetCurrentModLibraryPath();

			// Check if path has changed
			if (currentPath != initialModLibraryPath)
			{
				// Update initial path first (so UpdateWindowTitle can use it)
				initialModLibraryPath = currentPath;

				// Reload everything
				ScanLocalFiles();
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
			// Cleanup if needed
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
				settings.LastOfflinePath = currentPath;
			else
				settings.LastOnlinePath = currentPath;

			// Save all settings when closing the window
			settings.Settings.SaveSettings();
		}
		// Cleanup

		/********************************************************************/
		/// <summary>
		/// Is called when the parent button is clicked
		/// </summary>
		/********************************************************************/
		private void ParentButton_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(currentPath)) return;

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
			if (currentEntries == null || e.ItemIndex < 0 || e.ItemIndex >= currentEntries.Count) return;

			var entry = currentEntries[e.ItemIndex];
			string icon = entry.IsDirectory ? "📁" : "🎵";
			ListViewItem item = new($"{icon} {entry.Name}");

			// Add path column (only visible in flat view)
			if (flatViewCheckBox.Checked && !entry.IsDirectory)
			{
				// Extract path from FullPath (remove filename)
				int lastSlash = entry.FullPath.LastIndexOf('/');
				string pathOnly = lastSlash >= 0 ? entry.FullPath.Substring(0, lastSlash) : string.Empty;

				// In online mode, try to replace service root with display name
				if (!data.IsOfflineMode)
				{
					var service = data.GetServiceFromPath(entry.FullPath);
					if (service != null)
					{
						string relativePath = data.GetRelativePathFromService(entry.FullPath, service);
						// Remove filename from relative path
						int relativeLastSlash = relativePath.LastIndexOf('/');
						string relativePathOnly = relativeLastSlash >= 0
							? relativePath.Substring(0, relativeLastSlash)
							: string.Empty;
						// Add service name before path
						pathOnly = string.IsNullOrEmpty(relativePathOnly)
							? service.DisplayName
							: $"{service.DisplayName}/{relativePathOnly}";
					}
				}

				item.SubItems.Add(pathOnly);
			}
			else
				item.SubItems.Add(string.Empty);

			// Add size column
			if (entry.IsDirectory)
			{
				// For directories, show file count recursively from tree
				int fileCount = CountFilesInTree(entry);
				item.SubItems.Add(fileCount > 0 ? $"{fileCount:N0} files" : string.Empty);
			}
			else
				// For files, show size in KB, MB, etc.
				item.SubItems.Add(FormatFileSize(entry.Size));

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
			if (moduleListView.SelectedIndices.Count == 0) return;

			int index = moduleListView.SelectedIndices[0];
			if (index < 0 || index >= currentEntries.Count) return;

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

						if (result == DialogResult.Yes) await DownloadModLandDatabase();

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
					PlayModuleIfExists(entry);
				else
					// Online mode: Download and play module
					await DownloadAndPlayModule(entry);
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
						// Local mode: Show delete menu for files and directories (but not service roots)
						if (!entry.FullPath.EndsWith("://"))
							offlineContextMenu.Show(moduleListView.PointToScreen(e.Location));
					}
					else
					{
						// Online mode: Check if in flat view
						if (flatViewCheckBox.Checked && !entry.IsDirectory)
							// Flat view: Show "Jump to folder" context menu for files
							flatViewContextMenu.Show(moduleListView.PointToScreen(e.Location));
						else if (entry.IsDirectory && entry.FullPath.EndsWith("://"))
							// Show service context menu only for service nodes at root
							serviceContextMenu.Show(moduleListView.PointToScreen(e.Location));
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
					if (currentEntries[i].Name.ToLower().StartsWith(searchBuffer))
					{
						moduleListView.SelectedIndices.Clear();
						moduleListView.SelectedIndices.Add(i);
						moduleListView.EnsureVisible(i);
						break;
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
								PlayModuleIfExists(entry);
							else
								// Online mode: Download and play module
								await DownloadAndPlayModule(entry);
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
			if (modland == null) return;

			// Ask for confirmation
			var result = MessageBox.Show(
				Resources.IDS_MODLIBRARY_MSGBOX_CLEAR_DATABASE_CONFIRM,
				Resources.IDS_MODLIBRARY_MSGBOX_TITLE_CLEAR_DB,
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning);

			if (result == DialogResult.Yes)
				try
				{
					// Clear data in RAM
					modland.ClearOnlineFiles();
					modland.IsLoaded = false;
					modland.LastUpdate = DateTime.MinValue;

					// Delete database file
					string allmodsTxtPath = Path.Combine(cacheDirectory, "ModLand", "allmods.txt");
					if (File.Exists(allmodsTxtPath)) File.Delete(allmodsTxtPath);

					// Also delete the zip file if it exists
					string allmodsZipPath = Path.Combine(cacheDirectory, "ModLand", "allmods.zip");
					if (File.Exists(allmodsZipPath)) File.Delete(allmodsZipPath);

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
			if (!string.IsNullOrEmpty(searchFilter)) ShowListViewStatus(Resources.IDS_MODLIBRARY_STATUS_SEARCHING);

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
			columnPath.Width = flatViewCheckBox.Checked ? 300 : 0;

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
		/// Is called when the mode tab control selection changes
		/// </summary>
		/********************************************************************/
		private void ModeTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Switch offline mode based on selected tab
			bool isOffline = modeTabControl.SelectedIndex == 1;

			if (data.IsOfflineMode != isOffline)
			{
				// Save current path for old mode before switching
				if (data.IsOfflineMode)
					settings.LastOfflinePath = currentPath;
				else
					settings.LastOnlinePath = currentPath;

				data.IsOfflineMode = isOffline;

				// Save setting
				settings.IsOfflineMode = isOffline;

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
						int totalFiles = data.CountTotalFilesInFilteredCache();
						statusLabel.Text = string.Format(Resources.IDS_MODLIBRARY_STATUS_DATABASE_LOADED,
							modland.LastUpdate.ToString("yyyy-MM-dd"), modland.OnlineFiles.Count);
					}
					else
						statusLabel.Text = Resources.IDS_MODLIBRARY_STATUS_RIGHT_CLICK_SERVICE;
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
			if (moduleListView.SelectedIndices.Count == 0) return;

			// Collect all selected entries
			List<TreeNode> selectedEntries = new();
			foreach (int index in moduleListView.SelectedIndices)
				if (index >= 0 && index < currentEntries.Count)
					selectedEntries.Add(currentEntries[index]);

			if (selectedEntries.Count == 0) return;

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
				int fileCount = selectedEntries.Count(e => !e.IsDirectory);
				int folderCount = selectedEntries.Count(e => e.IsDirectory);

				if (fileCount > 0 && folderCount > 0)
					message = string.Format(Resources.IDS_MODLIBRARY_MSGBOX_DELETE_MULTIPLE_CONFIRM, fileCount,
						folderCount);
				else if (folderCount > 0)
					message = string.Format(Resources.IDS_MODLIBRARY_MSGBOX_DELETE_FOLDERS_CONFIRM, folderCount);
				else
					message = string.Format(Resources.IDS_MODLIBRARY_MSGBOX_DELETE_FILES_CONFIRM, fileCount);
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

					// Rescan local files
					ScanLocalFiles();

					// Rebuild tree to reflect changes (once at the end)
					statusLabel.Text = Resources.IDS_MODLIBRARY_STATUS_UPDATING_FILE_LIST;
					RebuildTree();

					// Show result
					if (errorCount == 0)
						statusLabel.Text = deletedCount == 1
							? Resources.IDS_MODLIBRARY_STATUS_ITEM_DELETED_SUCCESS
							: string.Format(Resources.IDS_MODLIBRARY_STATUS_ITEMS_DELETED_SUCCESS, deletedCount);
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
			if (moduleListView.SelectedIndices.Count == 0) return;

			int index = moduleListView.SelectedIndices[0];
			if (index < 0 || index >= currentEntries.Count) return;

			var entry = currentEntries[index];

			// Get service
			var service = data.GetService(entry.ServiceId);
			if (service == null) return;

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

		#endregion

		#region Private methods

		/********************************************************************/
		/// <summary>
		/// Get the current ModLibrary path from settings
		/// </summary>
		/********************************************************************/
		private string GetCurrentModLibraryPath()
		{
			var userSettings = DependencyInjection.GetDefaultProvider().GetService<ISettings>();
			userSettings.LoadSettings("Settings");
			PathSettings pathSettings = new(userSettings);
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
			if (!string.IsNullOrEmpty(initialModLibraryPath)) title = $"{title} - {initialModLibraryPath}";

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
				return Path.Combine(cacheDirectory, "Modules");

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
			if (service == null) return null;

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
			cacheDirectory = Path.Combine(Settings.SettingsDirectory, "ModLibrary");
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
		}


		/********************************************************************/
		/// <summary>
		/// Check if we already have a downloaded database
		/// </summary>
		/********************************************************************/
		private async void CheckExistingDatabase()
		{
			var modland = data.GetService("modland");
			if (modland == null) return;

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

				// Check if database is older than 7 days
				if ((DateTime.Now - modland.LastUpdate).TotalDays > 7)
				{
					var result = MessageBox.Show(
						string.Format(Resources.IDS_MODLIBRARY_MSGBOX_DATABASE_OLD,
							(int)(DateTime.Now - modland.LastUpdate).TotalDays),
						Resources.IDS_MODLIBRARY_MSGBOX_TITLE_UPDATE_DB,
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Question);

					if (result == DialogResult.Yes) await DownloadModLandDatabase();
				}
			}
			else
				// Build tree to show services (even though database is not loaded)
				// This will fire OnDataLoaded which will set proper status
				RebuildTree();
		}


		/********************************************************************/
		/// <summary>
		/// Download ModLand database
		/// </summary>
		/********************************************************************/
		private async Task DownloadModLandDatabase()
		{
			var modland = data.GetService("modland");
			if (modland == null) return;

			string modlandCacheDir = Path.Combine(cacheDirectory, "ModLand");
			string allmodsZipPath = Path.Combine(modlandCacheDir, "allmods.zip");
			string allmodsTxtPath = Path.Combine(modlandCacheDir, "allmods.txt");

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
						if (entry != null) entry.ExtractToFile(allmodsTxtPath, true);
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
			if (!File.Exists(allmodsTxtPath)) return;

			service.ClearOnlineFiles();

			// Step 1: Parse all lines into (nameWithPath, size) tuples
			List<(string nameWithPath, long size)> entries = new();

			foreach (string line in File.ReadLines(allmodsTxtPath))
			{
				// Format: "12345    path/to/file.ext"
				string[] parts = line.Split(new[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length < 2) continue;

				if (!long.TryParse(parts[0], out long fileSize)) continue;

				string nameWithPath = string.Join(" ", parts.Skip(1));

				// Skip files that should not be included (e.g., smpl.*)
				if (!ShouldIncludeFile(nameWithPath)) continue;

				entries.Add((nameWithPath, fileSize));
			}

			// Step 2: Sort strings (fast!)
			entries.Sort((a, b) => string.Compare(a.nameWithPath, b.nameWithPath, StringComparison.OrdinalIgnoreCase));

			// Step 3: Create ModEntry objects from sorted data
			foreach (var entry in entries) service.AddOnlineFile(entry.nameWithPath, entry.size);
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
				if (!string.IsNullOrEmpty(data.SearchFilter)) AddBreadcrumbLabel($"🔍 \"{data.SearchFilter}\" - ");

				AddBreadcrumbLink("Home", string.Empty);

				// Add counts in gray
				AddBreadcrumbSeparator("•");
				Label countsLabel = new()
				{
					Text = string.Format(Resources.IDS_MODLIBRARY_FOLDERS_FILES_COUNT, folderCount, fileCount),
					AutoSize = true,
					Margin = new Padding(0, 5, 0, 0),
					ForeColor = SystemColors.GrayText
				};
				breadcrumbPanel.Controls.Add(countsLabel);

				// Add total files if searching
				if (!string.IsNullOrEmpty(data.SearchFilter))
				{
					int totalFiles = data.CountTotalFilesInFilteredCache();
					Label totalLabel = new()
					{
						Text = $", {totalFiles} total files",
						AutoSize = true,
						Margin = new Padding(0, 5, 0, 0),
						ForeColor = SystemColors.GrayText
					};
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
			AddBreadcrumbLink("Home", string.Empty);
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
				Label countsLabel = new()
				{
					Text = string.Format(Resources.IDS_MODLIBRARY_FOLDERS_FILES_COUNT, folderCount, fileCount),
					AutoSize = true,
					Margin = new Padding(0, 5, 0, 0),
					ForeColor = SystemColors.GrayText
				};
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

				if (!string.IsNullOrEmpty(pathSoFar) && !pathSoFar.EndsWith("/")) pathSoFar += "/";

				pathSoFar += parts[i];

				// All parts are clickable links
				AddBreadcrumbLink(parts[i], pathSoFar);

				if (i < parts.Length - 1) AddBreadcrumbSeparator();
			}

			// Add counts in gray after path
			AddBreadcrumbSeparator("•");
			Label countsLabel2 = new()
			{
				Text = string.Format(Resources.IDS_MODLIBRARY_FOLDERS_FILES_COUNT, folderCount, fileCount),
				AutoSize = true,
				Margin = new Padding(0, 5, 0, 0),
				ForeColor = SystemColors.GrayText
			};
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
				AddBreadcrumbLink("Home", string.Empty);
			else
			{
				// Add "Home" link
				AddBreadcrumbLink("Home", string.Empty);
				AddBreadcrumbSeparator();

				// Add path parts
				BuildClickablePath(currentPath);
			}

			// Add counts in gray
			AddBreadcrumbSeparator("•");
			Label countsLabel = new()
			{
				Text = string.Format(Resources.IDS_MODLIBRARY_SEARCH_COUNT, folderCount, fileCount, totalFiles),
				AutoSize = true,
				Margin = new Padding(0, 5, 0, 0),
				ForeColor = SystemColors.GrayText
			};
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
				AddBreadcrumbLabel(serviceName);
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

				if (!string.IsNullOrEmpty(pathSoFar) && !pathSoFar.EndsWith("/")) pathSoFar += "/";

				pathSoFar += parts[i];

				// All parts are links
				AddBreadcrumbLink(parts[i], pathSoFar);

				if (i < parts.Length - 1) AddBreadcrumbSeparator();
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
			Label separator = new()
			{
				Text = text == " > " ? text : $" {text} ",
				AutoSize = true,
				Margin = new Padding(0, 5, 0, 0),
				ForeColor = SystemColors.GrayText
			};
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
					if (currentPath.StartsWith(path))
					{
						// Remove the parent path to get the child folder name
						string remainingPath = currentPath.Substring(path.Length);
						if (remainingPath.StartsWith("/"))
							remainingPath = remainingPath.Substring(1);

						// Get first folder in the remaining path
						int slashIndex = remainingPath.IndexOf('/');
						folderToSelect = slashIndex > 0 ? remainingPath.Substring(0, slashIndex) : remainingPath;
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
			if (currentEntries.Count == 0) return;

			moduleListView.SelectedIndices.Clear();

			int indexToSelect = 0;

			if (!string.IsNullOrEmpty(itemToSelect))
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
			if (!flatViewCheckBox.Checked) return;

			// Determine which column was clicked
			if (e.Column == 0) // Name column
				settings.FlatViewSortOrder = (int)FlatViewSortOrder.NameThenPath;
			else if (e.Column == 1) // Path column
				settings.FlatViewSortOrder = (int)FlatViewSortOrder.PathThenName;
			else
				// Size column or other - ignore
				return;

			// Reload current directory with new sort order
			LoadCurrentDirectory();
		}


		/********************************************************************/
		/// <summary>
		/// Download module from service and add to playlist
		/// </summary>
		/********************************************************************/
		private async Task DownloadAndPlayModule(TreeNode entry)
		{
			try
			{
				var service = data.GetService(entry.ServiceId);
				if (service == null) return;

				// Get relative path without service prefix
				string relativePath = data.GetRelativePathFromService(entry.FullPath, service);

				// Build local file path (Modules are now in Modules/<Service>/ folder)
				string modulesBasePath = GetModLibraryModulesPath();
				string localPath = Path.Combine(modulesBasePath, service.FolderName,
					relativePath.Replace('/', Path.DirectorySeparatorChar));
				string localDirectory = Path.GetDirectoryName(localPath);

				// Check if already downloaded
				if (!File.Exists(localPath))
				{
					statusLabel.Text = string.Format(Resources.IDS_MODLIBRARY_STATUS_DOWNLOADING_FILE, entry.Name);

					// Create directory if needed
					Directory.CreateDirectory(localDirectory);

					// Download from service (currently only ModLand supported)
					if (service.Id == "modland")
						using (HttpClient client = new())
						{
							// URL-encode the path to handle special characters like #, spaces, etc.
							string encodedPath = string.Join("/", relativePath.Split('/').Select(Uri.EscapeDataString));
							string downloadUrl = ModlandModulesUrl + encodedPath;
							byte[] fileBytes = await client.GetByteArrayAsync(downloadUrl);
							await File.WriteAllBytesAsync(localPath, fileBytes);

							// Check if this is an mdat.* file - if so, download the matching smpl.* file
							if (entry.Name.StartsWith("mdat.", StringComparison.OrdinalIgnoreCase))
							{
								string smplFileName = "smpl" + entry.Name.Substring(4); // Replace "mdat" with "smpl"
								string smplRelativePath = relativePath.Substring(0, relativePath.LastIndexOf('/') + 1) +
								                          smplFileName;
								string smplLocalPath = Path.Combine(localDirectory, smplFileName);
								// URL-encode the smpl path too
								string smplEncodedPath = string.Join("/",
									smplRelativePath.Split('/').Select(Uri.EscapeDataString));
								string smplDownloadUrl = ModlandModulesUrl + smplEncodedPath;

								try
								{
									statusLabel.Text = string.Format(Resources.IDS_MODLIBRARY_STATUS_DOWNLOADING_SAMPLE,
										smplFileName);
									byte[] smplBytes = await client.GetByteArrayAsync(smplDownloadUrl);
									await File.WriteAllBytesAsync(smplLocalPath, smplBytes);

									// Add smpl file to LocalFilesCache
									AddFileToLocalCache(service, smplRelativePath, smplBytes.Length);
								}
								catch
								{
									// Sample file might not exist - that's ok
								}
							}
						}

					statusLabel.Text =
						string.Format(Resources.IDS_MODLIBRARY_STATUS_DOWNLOADED_FILE, entry.Name, entry.Size);

					// Add downloaded file to LocalFilesCache
					AddFileToLocalCache(service, relativePath, entry.Size);
				}
				else
					statusLabel.Text = string.Format(Resources.IDS_MODLIBRARY_STATUS_PLAYING_CACHED, entry.Name);

				// Add to playlist using the main window API
				if (mainWindow.Form is MainWindowForm mainWindowForm)
					mainWindowForm.Invoke((Action)(() =>
					{
						// Check if module already exists in playlist
						var existingItem = mainWindowForm.FindModuleInList(localPath);
						if (existingItem != null)
						{
							// Module already in list - select and play it
							if (playImmediatelyCheckBox.Checked) mainWindowForm.SelectAndPlayModule(existingItem);
						}
						else
							// Add the file to the list and optionally play it immediately
							mainWindowForm.AddFilesToModuleList(new[] {localPath}, playImmediatelyCheckBox.Checked);
					}));
			}
			catch (Exception ex)
			{
				statusLabel.Text = string.Format(Resources.IDS_MODLIBRARY_STATUS_ERROR_PREFIX, ex.Message);
				MessageBox.Show(string.Format(Resources.IDS_MODLIBRARY_MSGBOX_FAILED_DOWNLOAD_MODULE, ex.Message),
					Resources.IDS_MODLIBRARY_MSGBOX_TITLE_DOWNLOAD_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
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
				if (localPath == null) return;

				// Check if file exists
				if (File.Exists(localPath))
				{
					statusLabel.Text = string.Format(Resources.IDS_MODLIBRARY_STATUS_PLAYING_FILE, entry.Name);

					// Add to playlist using the main window API
					if (mainWindow.Form is MainWindowForm mainWindowForm)
						mainWindowForm.Invoke((Action)(() =>
						{
							// Check if module already exists in playlist
							var existingItem = mainWindowForm.FindModuleInList(localPath);
							if (existingItem != null)
							{
								// Module already in list - select and play it
								if (playImmediatelyCheckBox.Checked) mainWindowForm.SelectAndPlayModule(existingItem);
							}
							else
								// Add the file to the list and optionally play it immediately
								mainWindowForm.AddFilesToModuleList(new[] {localPath},
									playImmediatelyCheckBox.Checked);
						}));
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

			if (!Directory.Exists(modulesDir)) return;

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
				if (!ShouldIncludeFile(fullRelativePath)) continue;

				// Get file size
				FileInfo fileInfo = new(filePath);
				long fileSize = fileInfo.Length;

				entries.Add((fullRelativePath, fileSize));
			}

			// Step 2: Sort by path
			entries.Sort((a, b) => string.Compare(a.nameWithPath, b.nameWithPath, StringComparison.OrdinalIgnoreCase));

			// Step 3: Add all files to local files list
			foreach (var entry in entries) data.AddLocalFile(entry.nameWithPath, entry.size);
		}


		/********************************************************************/
		/// <summary>
		/// Add a downloaded file to the local files list
		/// </summary>
		/********************************************************************/
		private void AddFileToLocalCache(ModuleService service, string nameWithPath, long fileSize)
		{
			// Build full path including service folder name
			string fullPath = $"{service.FolderName}/{nameWithPath}";

			// Check if file already exists in local files
			if (!data.LocalFiles.Any(e => e.FullName == fullPath))
				// Add to local files list
				data.AddLocalFile(fullPath, fileSize);
		}


		/********************************************************************/
		/// <summary>
		/// Format file size in KB, MB, GB like Windows Explorer
		/// </summary>
		/********************************************************************/
		private string FormatFileSize(long bytes)
		{
			if (bytes < 1024) return $"{bytes} bytes";

			if (bytes < 1024 * 1024) return $"{Math.Ceiling(bytes / 1024.0):0} KB";

			if (bytes < 1024 * 1024 * 1024) return $"{Math.Round(bytes / (1024.0 * 1024.0), 1):0.0} MB";

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
				if (child.IsDirectory)
					// Recursively count files in subdirectories
					count += CountFilesInTree(child);
				else
					// Count this file
					count++;

			return count;
		}

		#endregion
	}
}