/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.MainWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Data;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Downloader;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Events;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Sources;
using Polycode.NostalgicPlayer.Controls;
using Polycode.NostalgicPlayer.Controls.Components;
using Polycode.NostalgicPlayer.Controls.Dialogs;
using Polycode.NostalgicPlayer.Controls.Events;
using Polycode.NostalgicPlayer.Controls.Forms;
using Polycode.NostalgicPlayer.Controls.Menus;
using Polycode.NostalgicPlayer.External;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Pages
{
	/// <summary>
	/// Holds all the controls for the online tab
	/// </summary>
	public partial class OnlinePageControl : UserControl, IDependencyInjectionControl, IModLibraryPage
	{
		private ISourceFactory sourceFactory;
		private IModLibraryHelper modLibraryHelper;
		private IFormCreatorService formCreatorService;

		private IMainWindowApi mainWindowApi;
		private IModLibraryWindowApi modLibraryWindowApi;
		private ModLibraryWindowSettings settings;
		private ModLibraryDownloadControl downloadControl;

		private NostalgicToolStripMenuItem downloadAndPlayItem;
		private NostalgicToolStripMenuItem downloadToPlaylistItem;
		private NostalgicToolStripMenuItem downloadToDiskItem;
		private NostalgicToolStripSeparator separatorItem;
		private NostalgicToolStripMenuItem jumpToFolderItem;

		private readonly OnlineData data = new OnlineData();
		private readonly TaskHelper taskHelper = new TaskHelper();
		private DownloadQueueManager downloadQueueManager;
		private ModLibraryPlaylistIntegration playlistIntegration;

		private string lastSearchText;
		private SearchMode lastSearchMode;
		private bool playImmediately;

		private bool initializeRunning;
		private bool checkForOldDatabases;
		private bool hasBeenLoaded;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public OnlinePageControl()
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
		public void InitializeControl(IMainWindowApi mainWindowApi, ISourceFactory sourceFactory, IModLibraryHelper modLibraryHelper, IFormCreatorService formCreatorService)
		{
			this.mainWindowApi = mainWindowApi;
			this.sourceFactory = sourceFactory;
			this.modLibraryHelper = modLibraryHelper;
			this.formCreatorService = formCreatorService;
		}



		/********************************************************************/
		/// <summary>
		/// Set the controls needed from parent
		/// </summary>
		/********************************************************************/
		public void SetControls(ModLibraryDownloadControl downloadControl)
		{
			this.downloadControl = downloadControl;
		}



		/********************************************************************/
		/// <summary>
		/// Ask to cancel downloading
		/// </summary>
		/********************************************************************/
		public void CancelDownload()
		{
			downloadQueueManager.CancelDownload();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public event EventHandler<ModuleDownloadedEventArgs> ModuleDownloaded;

		#region IModLibraryPage implementation
		/********************************************************************/
		/// <summary>
		/// Initialize the page
		/// </summary>
		/********************************************************************/
		public void Initialize(IModLibraryWindowApi modLibraryWindow, ModLibraryWindowSettings settings)
		{
			modLibraryWindowApi = modLibraryWindow;
			this.settings = settings;

			playImmediately = settings.PlayImmediately;

			data.DataLoaded += DataLoaded;

			playlistIntegration = new ModLibraryPlaylistIntegration(mainWindowApi);

			downloadQueueManager = new DownloadQueueManager(modLibraryWindow, modLibraryHelper);
			downloadQueueManager.AddedToQueue += DownloadManager_AddedToQueue;
			downloadQueueManager.DownloadStarted += DownloadManager_DownloadStarted;
			downloadQueueManager.DownloadCompleted += DownloadManager_DownloadCompleted;
			downloadQueueManager.AllDownloadsCompleted += DownloadManager_AllDownloadsCompleted;
			downloadQueueManager.Start();

			CreateContextMenus();

			InitializeControls();
			InitializeSources();

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

				downloadQueueManager.Stop();
				downloadQueueManager.AllDownloadsCompleted -= DownloadManager_AllDownloadsCompleted;
				downloadQueueManager.DownloadCompleted -= DownloadManager_DownloadCompleted;
				downloadQueueManager.DownloadStarted -= DownloadManager_DownloadStarted;
				downloadQueueManager.AddedToQueue -= DownloadManager_AddedToQueue;

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
			if (hasBeenLoaded)
			{
				Search(searchText, searchMode);
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

			if (checkForOldDatabases)
			{
				CheckForOldDatabases();
				checkForOldDatabases = false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Called when a new batch of modules has been added to the queue
		/// </summary>
		/********************************************************************/
		private void DownloadManager_AddedToQueue(object sender, AddedToQueueEventArgs e)
		{
			downloadControl.AddBatch(e.ItemsAdded);
		}



		/********************************************************************/
		/// <summary>
		/// Called when a new download has started
		/// </summary>
		/********************************************************************/
		private void DownloadManager_DownloadStarted(object sender, DownloadStartedEventArgs e)
		{
			downloadControl.StartDownload(e.ModuleName);
		}



		/********************************************************************/
		/// <summary>
		/// Called when a single module has been downloaded
		/// </summary>
		/********************************************************************/
		private void DownloadManager_DownloadCompleted(object sender, DownloadCompletedEventArgs e)
		{
			downloadControl.DownloadDone();

			if (e.Success)
			{
				// Add to playlist only if addDownloadsToPlaylist flag is set
				switch (e.PlayFlag)
				{
					case PlayFlag.AddToPlayList:
					{
						playlistIntegration.AddToPlaylist(e.LocalPath, false);
						break;
					}

					case PlayFlag.AddToPlayListAndPlay:
					{
						playlistIntegration.AddToPlaylist(e.LocalPath, true);
						break;
					}
				}

				ModuleDownloaded?.Invoke(this, new ModuleDownloadedEventArgs(e.LocalPath));
			}
		}



		/********************************************************************/
		/// <summary>
		/// Called when all modules in the queue has been downloaded
		/// </summary>
		/********************************************************************/
		private void DownloadManager_AllDownloadsCompleted(object sender, AllDownloadsCompletedEventArgs e)
		{
			string message = e.FailedCount > 0 ? string.Format(Resources.IDS_MODLIBRARY_STATUS_DOWNLOAD_COMPLETED_WITH_ERRORS, e.FailedCount) : Resources.IDS_MODLIBRARY_STATUS_DOWNLOAD_COMPLETED_SUCCESS;

			downloadControl.AllDownloadsCompleted(message);
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

			IModLibrarySource source = data.GetSource(entry.SourceId);
			if (source == null)
				return;

			if (entry.IsDirectory)
			{
				// Check if this is a source root that's not loaded
				if (entry.FullPath.EndsWith(":/") && !source.IsLoaded)
				{
					// Ask user if they want to download the database
					bool result = mainWindowApi.ShowQuestion(string.Format(Resources.IDS_MODLIBRARY_MSGBOX_DATABASE_NOT_LOADED, source.DisplayName), modLibraryWindowApi.Form);

					if (result)
						DownloadDatabase([source]);

					return; // User declined, don't navigate
				}

				// Changing the path will reload the list
				breadcrumbControl.SetPath(data.ConvertPathToDisplay(entry.FullPath));
			}
			else
				downloadQueueManager.EnqueueModules([new DownloadModuleItem(source, entry)], playImmediately ? PlayFlag.AddToPlayListAndPlay : PlayFlag.AddToPlayList);
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
			if (e.Entry.IsDirectory && e.Entry.FullPath.EndsWith(":/"))
			{
				sourceContextMenu.Tag = e.Entry;
				sourceContextMenu.Show(listControl, e.Location);
			}
			else
			{
				UpdateContextMenuVisibility();
				contextMenu.Show(listControl, e.Location);
			}
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
		#endregion

		#region Context menu handlers
		/********************************************************************/
		/// <summary>
		/// Is called when "Update Database" context menu item is clicked
		/// </summary>
		/********************************************************************/
		private void UpdateDatabaseItem_Click(object sender, EventArgs e)
		{
			TreeNode entry = (TreeNode)sourceContextMenu.Tag;
			if (entry == null)
				return;

			IModLibrarySource source = data.GetSource(entry.SourceId);
			if (source == null)
				return;

			// Reset ignore flag when user manually triggers update
			settings.SetUpdateIgnored(source.Id, false);

			DownloadDatabase([source]);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when "Clear Database" context menu item is clicked
		/// </summary>
		/********************************************************************/
		private void ClearDatabaseItem_Click(object sender, EventArgs e)
		{
			TreeNode entry = (TreeNode)sourceContextMenu.Tag;
			if (entry == null)
				return;

			IModLibrarySource source = data.GetSource(entry.SourceId);
			if (source == null)
				return;

			// Ask for confirmation
			bool result = mainWindowApi.ShowQuestion(Resources.IDS_MODLIBRARY_MSGBOX_CLEAR_DATABASE_CONFIRM, modLibraryWindowApi.Form);

			if (result)
			{
				try
				{
					source.DeleteDatabase();

					// Go back to the root if the current path is inside the cleared source,
					// since the path does not exist anymore
					if (data.ConvertPathToInternal(breadcrumbControl.GetPath()).StartsWith(source.RootPath))
						breadcrumbControl.SetPath(string.Empty);

					Search(lastSearchText, lastSearchMode, true);
				}
				catch (Exception ex)
				{
					mainWindowApi.ShowSimpleErrorMessage(string.Format(Resources.IDS_MODLIBRARY_MSGBOX_FAILED_CLEAR_DB, ex.Message), modLibraryWindowApi.Form);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when "Download and Play" context menu item is clicked
		/// </summary>
		/********************************************************************/
		private void DownloadAndPlayItem_Click(object sender, EventArgs e)
		{
			DownloadSelectedFiles(PlayFlag.AddToPlayListAndPlay);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when "Download to Playlist" context menu item is clicked
		/// </summary>
		/********************************************************************/
		private void DownloadToPlayListItem_Click(object sender, EventArgs e)
		{
			DownloadSelectedFiles(PlayFlag.AddToPlayList);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when "Download to Disk" context menu item is clicked
		/// </summary>
		/********************************************************************/
		private void DownloadToDiskItem_Click(object sender, EventArgs e)
		{
			DownloadSelectedFiles(PlayFlag.JustDownload);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when "Jump to folder" context menu item is clicked
		/// </summary>
		/********************************************************************/
		private void JumpToFolderItem_Click(object sender, EventArgs e)
		{
			List<TreeNode> selectedEntries = listControl.GetSelectedItems();

			if (selectedEntries.Count == 0)
				return;

			TreeNode entry = selectedEntries[0];

			// Get source
			IModLibrarySource source = data.GetSource(entry.SourceId);
			if (source == null)
				return;

			// Extract folder path (without filename)
			int lastSlash = entry.FullPath.LastIndexOf('/');
			string folderPath = lastSlash >= 0 ? entry.FullPath.Substring(0, lastSlash) : string.Empty;

			// Navigate to folder
			breadcrumbControl.SetPath(data.ConvertPathToDisplay(folderPath));
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Create all needed context menus
		/// </summary>
		/********************************************************************/
		private void CreateContextMenus()
		{
			CreateContextMenu();
			CreateSourceContextMenu();
		}



		/********************************************************************/
		/// <summary>
		/// Create the context menu
		/// </summary>
		/********************************************************************/
		private void CreateContextMenu()
		{
			downloadAndPlayItem = new NostalgicToolStripMenuItem(Resources.IDS_MODLIBRARY_CONTEXT_ONLINE_DOWNLOAD_AND_PLAY);
			downloadAndPlayItem.Click += DownloadAndPlayItem_Click;
			contextMenu.Items.Add(downloadAndPlayItem);

			downloadToPlaylistItem = new NostalgicToolStripMenuItem(Resources.IDS_MODLIBRARY_CONTEXT_ONLINE_DOWNLOAD_TO_PLAYLIST);
			downloadToPlaylistItem.Click += DownloadToPlayListItem_Click;
			contextMenu.Items.Add(downloadToPlaylistItem);

			downloadToDiskItem = new NostalgicToolStripMenuItem(Resources.IDS_MODLIBRARY_CONTEXT_ONLINE_DOWNLOAD_TO_DISK);
			downloadToDiskItem.Click += DownloadToDiskItem_Click;
			contextMenu.Items.Add(downloadToDiskItem);

			separatorItem = new NostalgicToolStripSeparator();
			contextMenu.Items.Add(separatorItem);

			jumpToFolderItem = new NostalgicToolStripMenuItem(Resources.IDS_MODLIBRARY_CONTEXT_ONLINE_JUMP_TO_FOLDER);
			jumpToFolderItem.Click += JumpToFolderItem_Click;
			contextMenu.Items.Add(jumpToFolderItem);
		}



		/********************************************************************/
		/// <summary>
		/// Create the source context menu
		/// </summary>
		/********************************************************************/
		private void CreateSourceContextMenu()
		{
			NostalgicToolStripMenuItem item = new NostalgicToolStripMenuItem(Resources.IDS_MODLIBRARY_CONTEXT_SOURCE_UPDATE_DATABASE);
			item.Click += UpdateDatabaseItem_Click;
			sourceContextMenu.Items.Add(item);

			item = new NostalgicToolStripMenuItem(Resources.IDS_MODLIBRARY_CONTEXT_SOURCE_CLEAR_DATABASE);
			item.Click += ClearDatabaseItem_Click;
			sourceContextMenu.Items.Add(item);
		}



		/********************************************************************/
		/// <summary>
		/// Update context menu visibility
		/// </summary>
		/********************************************************************/
		private void UpdateContextMenuVisibility()
		{
			downloadAndPlayItem.Visible = playImmediately;
			downloadToPlaylistItem.Visible = !playImmediately;

			List<TreeNode> selectedItems = listControl.GetSelectedItems();

			bool isSingleFile = (selectedItems.Count == 1) && !selectedItems[0].IsDirectory;
			bool showJump = flatViewCheckBox.Checked && isSingleFile;

			separatorItem.Visible = showJump;
			jumpToFolderItem.Visible = showJump;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize controls with settings
		/// </summary>
		/********************************************************************/
		private void InitializeControls()
		{
			initializeRunning = true;

			try
			{
				flatViewCheckBox.Checked = settings.OnlineFlatViewEnabled;

				breadcrumbControl.Initialize();
				listControl.Initialize(settings, true);
			}
			finally
			{
				initializeRunning = false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Initialize available sources
		/// </summary>
		/********************************************************************/
		private void InitializeSources()
		{
			data.AddSource(sourceFactory.GetModLandSource());
		}



		/********************************************************************/
		/// <summary>
		/// Store settings
		/// </summary>
		/********************************************************************/
		private void StoreSettings(ModLibraryWindowSettings settings)
		{
			settings.OnlineFlatViewEnabled = flatViewCheckBox.Checked;
			settings.LastOnlinePath = data.ConvertPathToInternal(breadcrumbControl.GetPath());

			listControl.StoreSettings(settings, true);
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
				foreach (IModLibrarySource source in data.Sources)
				{
					Invoke(() =>
					{
						listControl.SetLoading(true, string.Format(Resources.IDS_MODLIBRARY_STATUS_LOADING_DATABASE, source.DisplayName));
					});

					source.LoadDatabase();

					if (cancellationToken.IsCancellationRequested)
						return Task.FromCanceled(cancellationToken);
				}

				checkForOldDatabases = true;

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
				string currentPath = data.ConvertPathToInternal(breadcrumbControl.GetPath());

				// Get entries from data class (already sorted)
				List<TreeNode> currentEntries = data.GetEntries(currentPath, flatViewCheckBox.Checked, listControl.SortOrder);
				listControl.SetItems(currentPath, currentEntries);

				UpdateCounts(currentEntries);

				// Select specific item or default to first
				SelectItemInList(itemToSelect, setFocus);
			}
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
		/// Download source database
		/// </summary>
		/********************************************************************/
		private void DownloadDatabase(IEnumerable<IModLibrarySource> sources)
		{
			taskHelper.RunTask(async (cancellationToken) =>
			{
				try
				{
					foreach (IModLibrarySource source in sources)
					{
						Invoke(() =>
						{
							listControl.SetLoading(true, string.Format(Resources.IDS_MODLIBRARY_STATUS_UPDATING_DATABASE, source.DisplayName));
						});

						await source.GetDownloader().DownloadDatabaseAsync(cancellationToken);

						Invoke(() =>
						{
							listControl.SetLoading(true, string.Format(Resources.IDS_MODLIBRARY_STATUS_LOADING_DATABASE, source.DisplayName));
						});

						source.LoadDatabase();

						cancellationToken.ThrowIfCancellationRequested();
					}
				}
				catch (Exception ex) when (ex is not OperationCanceledException)
				{
					// Show the error here instead of via RunTask's exception handler.
					// ShowErrorMessage() posts its UI work with BeginInvoke, so doing it
					// before the finally block guarantees that the list is cleared
					// before the search below is posted, and not after it
					modLibraryHelper.ShowErrorMessage(ex, listControl, modLibraryWindowApi);
				}
				finally
				{
					BeginInvoke(() =>
					{
						Search(lastSearchText, lastSearchMode, true);
					});
				}
			}, (ex) => modLibraryHelper.ShowErrorMessage(ex, listControl, modLibraryWindowApi));
		}



		/********************************************************************/
		/// <summary>
		/// Helper method to download selected files
		/// </summary>
		/********************************************************************/
		private void DownloadSelectedFiles(PlayFlag playFlag)
		{
			List<TreeNode> selectedEntries = listControl.GetSelectedItems();

			if (selectedEntries.Count == 0)
				return;

			// Collect all files (directly selected + from directories), avoiding duplicates
			HashSet<string> seenPaths = new HashSet<string>();
			List<DownloadModuleItem> allFiles = new List<DownloadModuleItem>();

			foreach (TreeNode entry in selectedEntries)
			{
				if (entry.IsDirectory)
				{
					// Get all files from directory recursively
					var dirFiles = data.GetEntries(entry.FullPath, true, FlatViewSortOrder.NameThenPath)
						.Where(e => !e.IsDirectory);

					foreach (TreeNode file in dirFiles)
					{
						if (seenPaths.Add(file.FullPath))
							allFiles.Add(new DownloadModuleItem(data.GetSource(file.SourceId), file));
					}
				}
				else
				{
					if (seenPaths.Add(entry.FullPath))
						allFiles.Add(new DownloadModuleItem(data.GetSource(entry.SourceId), entry));
				}
			}

			if (allFiles.Count == 0)
				return;

			// Ask for confirmation if more than 10 files
			if (allFiles.Count > 10)
			{
				bool result = mainWindowApi.ShowQuestion(string.Format(Resources.IDS_MODLIBRARY_MSGBOX_DOWNLOAD_CONFIRM, allFiles.Count), modLibraryWindowApi.Form);

				if (!result)
					return;
			}

			// Enqueue all files
			downloadQueueManager.EnqueueModules(allFiles, playFlag);
		}



		/********************************************************************/
		/// <summary>
		/// Will check all sources if they are too old
		/// </summary>
		/********************************************************************/
		private void CheckForOldDatabases()
		{
			List<IModLibrarySource> databasesToBeDownloaded = new List<IModLibrarySource>();

			foreach (IModLibrarySource source in data.Sources)
			{
				// Only check databases that has been downloaded
				if (!source.IsLoaded)
					continue;

				// Check if database is older than 7 days
				int daysOld = (int)(DateTime.Now - source.LastUpdate).TotalDays;
				if ((daysOld >= 7) && !settings.IsUpdateIgnored(source.Id))
				{
					using (CustomMessageBox dialog = formCreatorService.GetMessageBox(string.Format(Resources.IDS_MODLIBRARY_MSGBOX_DATABASE_OLD, source.DisplayName, daysOld), Resources.IDS_MAIN_TITLE, CustomMessageBox.IconType.Question))
					{
						dialog.AddButton(Resources.IDS_MODLIBRARY_MSGBOX_BUTTON_UPDATE_NOW, 'u');
						dialog.AddButton(Resources.IDS_MODLIBRARY_MSGBOX_BUTTON_IGNORE, 'i');
						dialog.AddButton(Resources.IDS_MODLIBRARY_MSGBOX_BUTTON_DISMISS, 'd');
						dialog.ShowDialog(modLibraryWindowApi.Form);

						switch (dialog.GetButtonResult('d'))
						{
							case 'u':
							{
								settings.SetUpdateIgnored(source.Id, false);
								databasesToBeDownloaded.Add(source);
								break;
							}

							case 'i':
							{
								settings.SetUpdateIgnored(source.Id, true);
								break;
							}
						}
					}
				}
			}

			if (databasesToBeDownloaded.Count > 0)
				DownloadDatabase(databasesToBeDownloaded);
		}
		#endregion
	}
}
