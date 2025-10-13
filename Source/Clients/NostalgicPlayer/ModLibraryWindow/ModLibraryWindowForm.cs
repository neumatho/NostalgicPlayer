/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Bases;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;
using Polycode.NostalgicPlayer.Kit.Helpers;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.ModLibraryWindow
{
	/// <summary>
	/// This shows the Module Library window
	/// </summary>
	public partial class ModLibraryWindowForm : WindowFormBase, IModLibraryWindowApi
	{
		private const string AllmodsZipUrl = "https://modland.com/allmods.zip";
		private const string ModlandModulesUrl = "https://modland.com/pub/modules/";

		private readonly IMainWindowApi mainWindow;
		private string cacheDirectory;

		private ModLibraryData data = new ModLibraryData();
		private string currentPath = "";
		private List<TreeNode> currentEntries = new List<TreeNode>();
		private string searchBuffer = "";
		private System.Windows.Forms.Timer searchTimer;
		private System.Windows.Forms.Timer searchDebounceTimer;
		private ModLibrarySettings settings;

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

			if (!DesignMode)
			{
				InitializeWindow(mainWindow, optionSettings);

				// Load window settings
				LoadWindowSettings("ModLibraryWindow");

				// Set the title of the window
				Text = Resources.IDS_MODLIBRARY_TITLE;

				// Initialize cache directory
				InitializeCacheDirectory();

				// Initialize services
				InitializeServices();

				// Initialize search timer for keyboard quick search
				searchTimer = new System.Windows.Forms.Timer();
				searchTimer.Interval = 1000; // 1 second timeout
				searchTimer.Tick += (s, ev) => { searchBuffer = ""; searchTimer.Stop(); };

				// Initialize debounce timer for search textbox
				searchDebounceTimer = new System.Windows.Forms.Timer();
				searchDebounceTimer.Interval = 100; // 100ms debounce
				searchDebounceTimer.Tick += SearchDebounceTimer_Tick;

				// Load and apply ModLibrary settings (must be done AFTER timer initialization)
				settings = new ModLibrarySettings(allWindowSettings);
				data.IsOfflineMode = settings.IsOfflineMode;
				modeTabControl.SelectedIndex = settings.IsOfflineMode ? 1 : 0;
				flatViewCheckBox.Checked = settings.FlatViewEnabled;
				playImmediatelyCheckBox.Checked = settings.PlayImmediately;
				searchTextBox.Text = settings.SearchText;
				searchModeComboBox.SelectedIndex = settings.SearchMode;

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
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		///
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
			// Save all settings when closing the window
			settings.Settings.SaveSettings();

			// Cleanup
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the parent button is clicked
		/// </summary>
		/********************************************************************/
		private void ParentButton_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(currentPath))
				return;

			// If at service root (e.g., "modland://"), go back to root
			if (currentPath.EndsWith("://"))
			{
				currentPath = "";
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
						currentPath = service.RootPath;
					}
					else
					{
						// Find the last slash in the full path
						int lastSlash = currentPath.LastIndexOf('/');
						currentPath = currentPath.Substring(0, lastSlash);
					}
				}
				else
				{
					currentPath = "";
				}
			}

			LoadCurrentDirectory();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when virtual list needs an item
		/// </summary>
		/********************************************************************/
		private void ModuleListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
		{
			if (e.ItemIndex >= 0 && e.ItemIndex < currentEntries.Count)
			{
				TreeNode entry = currentEntries[e.ItemIndex];
				string icon = entry.IsDirectory ? "📁" : "🎵";
				ListViewItem item = new ListViewItem($"{icon} {entry.Name}");

				// Add path column (only visible in flat view)
				if (flatViewCheckBox.Checked && !entry.IsDirectory)
				{
					// Extract path from FullPath (remove service root and filename)
					var service = data.GetServiceFromPath(entry.FullPath);
					if (service != null)
					{
						string relativePath = data.GetRelativePathFromService(entry.FullPath, service);
						// Remove filename from path
						int lastSlash = relativePath.LastIndexOf('/');
						string pathOnly = lastSlash >= 0 ? relativePath.Substring(0, lastSlash) : "";
						// Add service name before path
						string fullPath = string.IsNullOrEmpty(pathOnly) ? service.DisplayName : $"{service.DisplayName}/{pathOnly}";
						item.SubItems.Add(fullPath);
					}
					else
					{
						item.SubItems.Add("");
					}
				}
				else
				{
					item.SubItems.Add("");
				}

				// Add size column
				item.SubItems.Add(entry.IsDirectory ? "" : $"{entry.Size:N0}");
				item.Tag = entry;
				e.Item = item;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when a list item is double-clicked
		/// </summary>
		/********************************************************************/
		private async void ModuleListView_DoubleClick(object sender, EventArgs e)
		{
			if (moduleListView.SelectedIndices.Count == 0)
				return;

			int index = moduleListView.SelectedIndices[0];
			if (index < 0 || index >= currentEntries.Count)
				return;

			TreeNode entry = currentEntries[index];

			if (entry.IsDirectory)
			{
				// In flat view, no navigation is possible (all files are at root)
				if (flatViewCheckBox.Checked)
					return;

				// Check if this is a service root that's not loaded (only in online mode)
				if (entry.FullPath.EndsWith("://") && !data.IsOfflineMode)
				{
					var service = data.GetService(entry.ServiceId);
					if (service != null && !service.IsLoaded)
					{
						// Ask user if they want to download the database
						var result = MessageBox.Show(
							$"The {service.DisplayName} database is not loaded yet.\n\nDo you want to download it now?",
							"Download Database",
							MessageBoxButtons.YesNo,
							MessageBoxIcon.Question);

						if (result == DialogResult.Yes)
						{
							await DownloadModLandDatabase();
							return;
						}
						else
						{
							return; // User declined, don't navigate
						}
					}
				}

				currentPath = entry.FullPath;
				LoadCurrentDirectory();
			}
			else
			{
				// In offline mode, only play if file exists locally
				if (data.IsOfflineMode)
				{
					await PlayModuleIfExists(entry);
				}
				else
				{
					// Online mode: Download and play module
					await DownloadAndPlayModule(entry);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the list view is right-clicked (for context menu)
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
						// Offline mode: Show delete menu for files and directories (but not service roots)
						if (!entry.FullPath.EndsWith("://"))
						{
							offlineContextMenu.Show(moduleListView.PointToScreen(e.Location));
						}
					}
					else
					{
						// Online mode: Check if in flat view
						if (flatViewCheckBox.Checked && !entry.IsDirectory)
						{
							// Flat view: Show "Jump to folder" context menu for files
							flatViewContextMenu.Show(moduleListView.PointToScreen(e.Location));
						}
						else if (entry.IsDirectory && entry.FullPath.EndsWith("://"))
						{
							// Show service context menu only for service nodes at root
							serviceContextMenu.Show(moduleListView.PointToScreen(e.Location));
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
						TreeNode entry = currentEntries[index];

						if (entry.IsDirectory)
						{
							// In flat view, no navigation is possible
							if (flatViewCheckBox.Checked)
							{
								e.Handled = true;
								return;
							}

							// Check if this is a service root that's not loaded (only in online mode)
							if (entry.FullPath.EndsWith("://") && !data.IsOfflineMode)
							{
								var service = data.GetService(entry.ServiceId);
								if (service != null && !service.IsLoaded)
								{
									// Ask user if they want to download the database
									var result = MessageBox.Show(
										$"The {service.DisplayName} database is not loaded yet.\n\nDo you want to download it now?",
										"Download Database",
										MessageBoxButtons.YesNo,
										MessageBoxIcon.Question);

									if (result == DialogResult.Yes)
									{
										await DownloadModLandDatabase();
										e.Handled = true;
										return;
									}
									else
									{
										e.Handled = true;
										return; // User declined, don't navigate
									}
								}
							}

							currentPath = entry.FullPath;
							LoadCurrentDirectory();
						}
						else
						{
							// In offline mode, only play if file exists locally
							if (data.IsOfflineMode)
							{
								await PlayModuleIfExists(entry);
							}
							else
							{
								// Online mode: Download and play module
								await DownloadAndPlayModule(entry);
							}
						}
					}
				}
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Back)
			{
				// Backspace = go to parent directory (not in flat view)
				if (!flatViewCheckBox.Checked)
				{
					ParentButton_Click(sender, EventArgs.Empty);
				}
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
			ModuleService modland = data.GetService("modland");
			if (modland == null)
				return;

			// Ask for confirmation
			var result = MessageBox.Show(
				"Are you sure you want to clear the database?\n\nThis will delete the local database file and clear all data from memory.",
				"Clear Database",
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
					currentPath = "";
					RebuildTree();

					statusLabel.Text = "Database cleared successfully";
				}
				catch (Exception ex)
				{
					statusLabel.Text = $"Error clearing database: {ex.Message}";
					MessageBox.Show($"Failed to clear database:\n\n{ex.Message}", "Clear Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when debounce timer ticks (100ms after last keystroke)
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
				ShowListViewStatus("Searching...");
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
				ShowListViewStatus("Searching...");
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

			// In flat view, disable parent button and go to root
			if (flatViewCheckBox.Checked)
			{
				currentPath = "";
				parentButton.Enabled = false;
			}

			// Rebuild tree with current settings
			ShowListViewStatus(flatViewCheckBox.Checked ? "Building flat view..." : "Building tree view...");
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
			bool isOffline = (modeTabControl.SelectedIndex == 1);

			if (data.IsOfflineMode != isOffline)
			{
				data.IsOfflineMode = isOffline;

				// Save setting
				settings.IsOfflineMode = isOffline;

				// Show "Switching mode..." in ListView
				ShowListViewStatus(isOffline ? "Switching to offline mode..." : "Switching to online mode...");

				// Rebuild tree for new mode (will fire DataLoaded event when done)
				currentPath = "";

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
			if (InvokeRequired)
			{
				Invoke(new Action(() => OnDataLoaded(sender, e)));
				return;
			}

			// Go to root to show filtered results (or normal root if search cleared)
			if (!string.IsNullOrEmpty(data.SearchFilter))
				currentPath = "";

			// Update UI
			LoadCurrentDirectory();

			// Update status
			if (!string.IsNullOrEmpty(data.SearchFilter))
			{
				int totalFiles = data.CountTotalFilesInFilteredCache();
				statusLabel.Text = $"Found {totalFiles} file(s) matching \"{data.SearchFilter}\"";
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
						statusLabel.Text = $"Database loaded: {modland.LastUpdate:yyyy-MM-dd} ({modland.OnlineFiles.Count:N0} files)";
					}
					else
					{
						statusLabel.Text = "Right-click on a service to update its database";
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
				return;

			// Collect all selected entries
			List<TreeNode> selectedEntries = new List<TreeNode>();
			foreach (int index in moduleListView.SelectedIndices)
			{
				if (index >= 0 && index < currentEntries.Count)
				{
					selectedEntries.Add(currentEntries[index]);
				}
			}

			if (selectedEntries.Count == 0)
				return;

			// Ask for confirmation
			string message;
			if (selectedEntries.Count == 1)
			{
				TreeNode entry = selectedEntries[0];
				message = entry.IsDirectory
					? $"Are you sure you want to delete the folder '{entry.Name}' and all its contents?\n\nThis will permanently delete the files from your computer."
					: $"Are you sure you want to delete the file '{entry.Name}'?\n\nThis will permanently delete the file from your computer.";
			}
			else
			{
				int fileCount = selectedEntries.Count(e => !e.IsDirectory);
				int folderCount = selectedEntries.Count(e => e.IsDirectory);

				if (fileCount > 0 && folderCount > 0)
					message = $"Are you sure you want to delete {fileCount} file(s) and {folderCount} folder(s)?\n\nThis will permanently delete all selected items from your computer.";
				else if (folderCount > 0)
					message = $"Are you sure you want to delete {folderCount} folder(s) and all their contents?\n\nThis will permanently delete all selected items from your computer.";
				else
					message = $"Are you sure you want to delete {fileCount} file(s)?\n\nThis will permanently delete all selected items from your computer.";
			}

			var result = MessageBox.Show(message, "Delete Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

			if (result == DialogResult.Yes)
			{
				int deletedCount = 0;
				int errorCount = 0;
				string lastError = "";

				try
				{
					// Group entries by service (important for flat view with multiple services)
					var entriesByService = selectedEntries.GroupBy(e => e.ServiceId);

					foreach (var serviceGroup in entriesByService)
					{
						var service = data.GetService(serviceGroup.Key);
						if (service == null)
							continue;

						string serviceCacheDir = Path.Combine(cacheDirectory, service.DisplayName);

						foreach (var entry in serviceGroup)
						{
							try
							{
								// Get relative path and local file path
								string relativePath = data.GetRelativePathFromService(entry.FullPath, service);
								string localPath = Path.Combine(serviceCacheDir, "Modules", relativePath.Replace('/', Path.DirectorySeparatorChar));

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

						// Rescan local files for this service
						ScanLocalFiles(service);
					}

					// Rebuild tree to reflect changes (once at the end)
					statusLabel.Text = "Updating file list...";
					RebuildTree();

					// Show result
					if (errorCount == 0)
					{
						statusLabel.Text = deletedCount == 1
							? "Item deleted successfully"
							: $"{deletedCount} items deleted successfully";
					}
					else
					{
						statusLabel.Text = $"Deleted {deletedCount} item(s), {errorCount} error(s)";
						MessageBox.Show($"Deleted {deletedCount} item(s) successfully.\n\n{errorCount} error(s) occurred. Last error:\n{lastError}", "Delete Completed with Errors", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					}
				}
				catch (Exception ex)
				{
					statusLabel.Text = $"Error deleting: {ex.Message}";
					MessageBox.Show($"Failed to delete:\n\n{ex.Message}", "Delete Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
				return;

			int index = moduleListView.SelectedIndices[0];
			if (index < 0 || index >= currentEntries.Count)
				return;

			TreeNode entry = currentEntries[index];

			// Get service
			var service = data.GetService(entry.ServiceId);
			if (service == null)
				return;

			// Get relative path and extract folder path (without filename)
			string relativePath = data.GetRelativePathFromService(entry.FullPath, service);
			int lastSlash = relativePath.LastIndexOf('/');
			string folderPath = lastSlash >= 0 ? relativePath.Substring(0, lastSlash) : "";

			// Build full folder path
			string fullFolderPath = service.RootPath + folderPath;

			// Clear search text
			searchTextBox.Text = "";

			// Disable flat view
			flatViewCheckBox.Checked = false;

			// Navigate to folder
			currentPath = fullFolderPath;
			LoadCurrentDirectory();
		}
		#endregion

		#region Private methods
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
			ModuleService modland = data.GetService("modland");
			if (modland == null)
				return;

			string allmodsTxtPath = Path.Combine(cacheDirectory, "ModLand", "allmods.txt");

			// Always scan local files at startup
			statusLabel.Text = "Scanning local files...";
			await Task.Run(() =>
			{
				ScanLocalFiles(modland);
				CleanupEmptyDirectories(modland);
			});

			if (File.Exists(allmodsTxtPath))
			{
				FileInfo fileInfo = new FileInfo(allmodsTxtPath);
				modland.LastUpdate = fileInfo.LastWriteTime;

				long fileSizeKB = fileInfo.Length / 1024;
				statusLabel.Text = $"Database found ({fileInfo.LastWriteTime:yyyy-MM-dd}, {fileSizeKB:N0} KB). Loading...";

				// Show "Loading..." in ListView
				ShowListViewStatus("Loading database...");

				// Parse and load the existing database in background
				await Task.Run(() => ParseModLandDatabase(modland, allmodsTxtPath));
				modland.IsLoaded = true;

				// Build full tree from loaded data (will fire DataLoaded event when done)
				statusLabel.Text = "Building tree structure...";
				RebuildTree();

				// Check if database is older than 7 days
				if ((DateTime.Now - modland.LastUpdate).TotalDays > 7)
				{
					var result = MessageBox.Show(
						$"The ModLand database is {(int)(DateTime.Now - modland.LastUpdate).TotalDays} days old.\n\nDo you want to update it now?",
						"Update Database",
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Question);

					if (result == DialogResult.Yes)
					{
						await DownloadModLandDatabase();
					}
				}
			}
			else
			{
				// Build tree to show services (even though database is not loaded)
				// This will fire OnDataLoaded which will set proper status
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
			ModuleService modland = data.GetService("modland");
			if (modland == null)
				return;

			string modlandCacheDir = Path.Combine(cacheDirectory, "ModLand");
			string allmodsZipPath = Path.Combine(modlandCacheDir, "allmods.zip");
			string allmodsTxtPath = Path.Combine(modlandCacheDir, "allmods.txt");

			try
			{
				// Show "Updating database..." in ListView (high-level status)
				ShowListViewStatus("Updating database...");

				progressBar.Visible = true;
				progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
				statusLabel.Text = "Downloading allmods.zip...";

				using (HttpClient client = new HttpClient())
				{
					// Download the file
					byte[] fileBytes = await client.GetByteArrayAsync(AllmodsZipUrl);

					// Save to disk
					await File.WriteAllBytesAsync(allmodsZipPath, fileBytes);

					statusLabel.Text = "Extracting allmods.txt...";

					// Extract allmods.txt from the ZIP
					using (ZipArchive archive = ZipFile.OpenRead(allmodsZipPath))
					{
						ZipArchiveEntry entry = archive.GetEntry("allmods.txt");
						if (entry != null)
						{
							entry.ExtractToFile(allmodsTxtPath, overwrite: true);
						}
					}

					// Update status
					FileInfo fileInfo = new FileInfo(allmodsTxtPath);
					modland.LastUpdate = fileInfo.LastWriteTime;

					long fileSizeKB = fileInfo.Length / 1024;
					statusLabel.Text = $"Download complete! Database: {fileSizeKB:N0} KB ({fileInfo.LastWriteTime:yyyy-MM-dd HH:mm})";

					progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
					progressBar.Value = 100;

					statusLabel.Text = "Parsing database...";

					// Parse and load the database in background
					await Task.Run(() => ParseModLandDatabase(modland, allmodsTxtPath));
					modland.IsLoaded = true;

					// Build full tree from loaded data (will fire DataLoaded event when done)
					statusLabel.Text = "Building tree structure...";
					RebuildTree();
				}
			}
			catch (Exception ex)
			{
				statusLabel.Text = $"Error: {ex.Message}";
				MessageBox.Show($"Failed to download allmods.zip:\n\n{ex.Message}", "Download Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

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
			if (!File.Exists(allmodsTxtPath))
				return;

			service.ClearOnlineFiles();

			// Step 1: Parse all lines into (nameWithPath, size) tuples
			var entries = new List<(string nameWithPath, long size)>();

			foreach (string line in File.ReadLines(allmodsTxtPath))
			{
				// Format: "12345    path/to/file.ext"
				string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length < 2)
					continue;

				if (!long.TryParse(parts[0], out long fileSize))
					continue;

				string nameWithPath = string.Join(" ", parts.Skip(1));

				// Skip sample files (smpl.*)
				string fileName = nameWithPath.Substring(nameWithPath.LastIndexOf('/') + 1);
				if (fileName.StartsWith("smpl.", StringComparison.OrdinalIgnoreCase))
					continue;

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
		/// Show status message in ListView (e.g., "Loading...", "Searching...")
		/// </summary>
		/********************************************************************/
		private void ShowListViewStatus(string message)
		{
			currentEntries = new List<TreeNode>
			{
				new TreeNode { Name = message, IsDirectory = false, FullPath = "", Size = 0, ServiceId = "" }
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
		private void LoadCurrentDirectory()
		{
			// Get entries from data class (already sorted)
			FlatViewSortOrder sortOrder = (FlatViewSortOrder)settings.FlatViewSortOrder;
			currentEntries = data.GetEntries(currentPath, flatViewCheckBox.Checked, sortOrder);

			// Update column headers with sort indicators
			UpdateColumnHeaders();

			// Update breadcrumb
			if (flatViewCheckBox.Checked)
			{
				// Flat view: Always show all files at root, disable parent button
				parentButton.Enabled = false;
				if (!string.IsNullOrEmpty(data.SearchFilter))
				{
					breadcrumbLabel.Values.Text = $"📋 Flat View - 🔍 \"{data.SearchFilter}\" ({currentEntries.Count} files)";
				}
				else
				{
					breadcrumbLabel.Values.Text = $"📋 Flat View - All Files ({currentEntries.Count} files)";
				}
			}
			else if (string.IsNullOrEmpty(currentPath))
			{
				parentButton.Enabled = false;
				breadcrumbLabel.Values.Text = $"Root ({currentEntries.Count} services)";
			}
			else if (!string.IsNullOrEmpty(data.SearchFilter))
			{
				// Search mode
				int totalFiles = data.CountTotalFilesInFilteredCache();
				string pathDisplay = data.GetDisplayPath(currentPath);
				breadcrumbLabel.Values.Text = $"🔍 \"{data.SearchFilter}\" - {pathDisplay} ({currentEntries.Count} items, {totalFiles} total files)";
				parentButton.Enabled = !string.IsNullOrEmpty(currentPath);
			}
			else
			{
				// Normal navigation
				string displayPath = data.GetDisplayPath(currentPath);
				breadcrumbLabel.Values.Text = $"{displayPath} ({currentEntries.Count} items)";
				parentButton.Enabled = true;
			}

			// Update virtual list
			moduleListView.VirtualListSize = currentEntries.Count;
			moduleListView.Invalidate();

			// Set focus to first item if list is not empty
			if (currentEntries.Count > 0)
			{
				moduleListView.SelectedIndices.Clear();
				moduleListView.SelectedIndices.Add(0);
				moduleListView.EnsureVisible(0);
				moduleListView.Focus();
			}
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
				columnName.Text = "Name";
				columnPath.Text = "Path";
				return;
			}

			FlatViewSortOrder sortOrder = (FlatViewSortOrder)settings.FlatViewSortOrder;

			// Update column headers with Unicode arrows
			if (sortOrder == FlatViewSortOrder.NameThenPath)
			{
				columnName.Text = "Name ▲";
				columnPath.Text = "Path";
			}
			else // PathThenName
			{
				columnName.Text = "Name";
				columnPath.Text = "Path ▲";
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle column header clicks to change sort order
		/// </summary>
		/********************************************************************/
		private void ModuleListView_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
		{
			// Only allow sorting in flat view mode
			if (!flatViewCheckBox.Checked)
				return;

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
			{
				// Size column or other - ignore
				return;
			}

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
				if (service == null)
					return;

				// Get relative path without service prefix
				string relativePath = data.GetRelativePathFromService(entry.FullPath, service);

				// Build local file path (preserve directory structure in service's Modules folder)
				string serviceCacheDir = Path.Combine(cacheDirectory, service.DisplayName);
				string localPath = Path.Combine(serviceCacheDir, "Modules", relativePath.Replace('/', Path.DirectorySeparatorChar));
				string localDirectory = Path.GetDirectoryName(localPath);

				// Check if already downloaded
				if (!File.Exists(localPath))
				{
					statusLabel.Text = $"Downloading {entry.Name}...";

					// Create directory if needed
					Directory.CreateDirectory(localDirectory);

					// Download from service (currently only ModLand supported)
					if (service.Id == "modland")
					{
						using (HttpClient client = new HttpClient())
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
								string smplRelativePath = relativePath.Substring(0, relativePath.LastIndexOf('/') + 1) + smplFileName;
								string smplLocalPath = Path.Combine(localDirectory, smplFileName);
								// URL-encode the smpl path too
								string smplEncodedPath = string.Join("/", smplRelativePath.Split('/').Select(Uri.EscapeDataString));
								string smplDownloadUrl = ModlandModulesUrl + smplEncodedPath;

								try
								{
									statusLabel.Text = $"Downloading sample file {smplFileName}...";
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
					}

					statusLabel.Text = $"Downloaded {entry.Name} ({entry.Size:N0} bytes)";

					// Add downloaded file to LocalFilesCache
					AddFileToLocalCache(service, relativePath, entry.Size);
				}
				else
				{
					statusLabel.Text = $"Playing {entry.Name} (cached)";
				}

				// Add to playlist using the main window API
				if (mainWindow.Form is MainWindowForm mainWindowForm)
				{
					mainWindowForm.Invoke((Action)(() =>
					{
						// Check if module already exists in playlist
						ModuleListItem existingItem = mainWindowForm.FindModuleInList(localPath);
						if (existingItem != null)
						{
							// Module already in list - select and play it
							if (playImmediatelyCheckBox.Checked)
								mainWindowForm.SelectAndPlayModule(existingItem);
						}
						else
						{
							// Add the file to the list and optionally play it immediately
							mainWindowForm.AddFilesToModuleList(new[] { localPath }, playImmediatelyCheckBox.Checked);
						}
					}));
				}
			}
			catch (Exception ex)
			{
				statusLabel.Text = $"Error: {ex.Message}";
				MessageBox.Show($"Failed to download module:\n\n{ex.Message}", "Download Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Play module if it exists locally (offline mode)
		/// </summary>
		/********************************************************************/
		private async Task PlayModuleIfExists(TreeNode entry)
		{
			try
			{
				var service = data.GetService(entry.ServiceId);
				if (service == null)
					return;

				// Get relative path without service prefix
				string relativePath = data.GetRelativePathFromService(entry.FullPath, service);

				// Build local file path
				string serviceCacheDir = Path.Combine(cacheDirectory, service.DisplayName);
				string localPath = Path.Combine(serviceCacheDir, "Modules", relativePath.Replace('/', Path.DirectorySeparatorChar));

				// Check if file exists
				if (File.Exists(localPath))
				{
					statusLabel.Text = $"Playing {entry.Name}";

					// Add to playlist using the main window API
					if (mainWindow.Form is MainWindowForm mainWindowForm)
					{
						mainWindowForm.Invoke((Action)(() =>
						{
							// Check if module already exists in playlist
							ModuleListItem existingItem = mainWindowForm.FindModuleInList(localPath);
							if (existingItem != null)
							{
								// Module already in list - select and play it
								if (playImmediatelyCheckBox.Checked)
									mainWindowForm.SelectAndPlayModule(existingItem);
							}
							else
							{
								// Add the file to the list and optionally play it immediately
								mainWindowForm.AddFilesToModuleList(new[] { localPath }, playImmediatelyCheckBox.Checked);
							}
						}));
					}
				}
				else
				{
					statusLabel.Text = $"File not found: {entry.Name}";
					MessageBox.Show($"The file '{entry.Name}' has not been downloaded yet.\n\nSwitch to Online mode to download it.", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
			catch (Exception ex)
			{
				statusLabel.Text = $"Error: {ex.Message}";
				MessageBox.Show($"Failed to play module:\n\n{ex.Message}", "Play Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Scan local files for a service
		/// </summary>
		/********************************************************************/
		private void ScanLocalFiles(ModuleService service)
		{
			service.ClearOfflineFiles();

			string modulesDir = Path.Combine(cacheDirectory, service.DisplayName, "Modules");

			if (!Directory.Exists(modulesDir))
				return;

			// Recursively scan all files
			var allFiles = Directory.GetFiles(modulesDir, "*", SearchOption.AllDirectories);

			// Step 1: Build list of (nameWithPath, size) tuples
			var entries = new List<(string nameWithPath, long size)>();

			foreach (string filePath in allFiles)
			{
				// Get relative path from Modules directory
				string nameWithPath = filePath.Substring(modulesDir.Length + 1).Replace(Path.DirectorySeparatorChar, '/');

				// Get file size
				FileInfo fileInfo = new FileInfo(filePath);
				long fileSize = fileInfo.Length;

				entries.Add((nameWithPath, fileSize));
			}

			// Step 2: Sort strings (fast!)
			entries.Sort((a, b) => string.Compare(a.nameWithPath, b.nameWithPath, StringComparison.OrdinalIgnoreCase));

			// Step 3: Create ModEntry objects from sorted data
			foreach (var entry in entries)
			{
				service.AddOfflineFile(entry.nameWithPath, entry.size);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Add a downloaded file to the OfflineFiles list
		/// </summary>
		/********************************************************************/
		private void AddFileToLocalCache(ModuleService service, string nameWithPath, long fileSize)
		{
			// Check if file already exists in cache
			if (!service.OfflineFiles.Any(e => e.FullName == nameWithPath))
			{
				// Create ModEntry - constructor will parse path automatically
				service.AddOfflineFile(nameWithPath, fileSize);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup empty directories recursively for a service
		/// </summary>
		/********************************************************************/
		private void CleanupEmptyDirectories(ModuleService service)
		{
			string modulesDir = Path.Combine(cacheDirectory, service.DisplayName, "Modules");

			if (!Directory.Exists(modulesDir))
				return;

			try
			{
				// Process subdirectories of Modules, but don't delete Modules itself
				foreach (string subDir in Directory.GetDirectories(modulesDir))
				{
					CleanupEmptyDirectoriesRecursive(subDir, modulesDir);
				}
			}
			catch
			{
				// Ignore errors during cleanup
			}
		}



		/********************************************************************/
		/// <summary>
		/// Recursively delete empty directories (bottom-up approach)
		/// </summary>
		/********************************************************************/
		private void CleanupEmptyDirectoriesRecursive(string directory, string rootDirectory)
		{
			// Process all subdirectories first (bottom-up)
			foreach (string subDir in Directory.GetDirectories(directory))
			{
				CleanupEmptyDirectoriesRecursive(subDir, rootDirectory);
			}

			// After processing subdirectories, check if this directory is now empty
			// Don't delete the root directory itself
			if (directory != rootDirectory && Directory.GetFileSystemEntries(directory).Length == 0)
			{
				try
				{
					Directory.Delete(directory);
				}
				catch
				{
					// Ignore errors (e.g., directory in use, permission denied)
				}
			}
		}
		#endregion
	}
}
