/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Data;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Pages;
using Polycode.NostalgicPlayer.Controls.Forms;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow
{
	/// <summary>
	/// This shows the Module Library window
	/// </summary>
	public partial class ModLibraryWindowForm : WindowFormBase2, IModLibraryWindowApi
	{
		private const int Page_Online = 0;
		private const int Page_Offline = 1;

		private IPlatformPath platformPath;
		private PathSettings pathSettings;
		private IModLibraryHelper modLibraryHelper;

		private bool initializeRunning;

		private ModLibraryWindowSettings settings;
		private string initialModLibraryPath;

		private IModLibraryPage currentPage;

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
		public void InitializeForm(IPlatformPath platformPath, PathSettings pathSettings, IModLibraryHelper modLibraryHelper)
		{
			// Remember the arguments
			this.platformPath = platformPath;
			this.pathSettings = pathSettings;
			this.modLibraryHelper = modLibraryHelper;

			// Set the tab titles
			modeTabControl.Pages[Page_Online].Text = Resources.IDS_MODLIBRARY_TAB_ONLINE;
			modeTabControl.Pages[Page_Offline].Text = Resources.IDS_MODLIBRARY_TAB_OFFLINE;

			// Set search mode combo box items
			searchModeComboBox.Items.Clear();
			searchModeComboBox.Items.AddRange(Resources.IDS_MODLIBRARY_SEARCHMODE_FILENAME_AND_PATH, Resources.IDS_MODLIBRARY_SEARCHMODE_FILENAME_ONLY, Resources.IDS_MODLIBRARY_SEARCHMODE_PATH_ONLY);

			// Set the title of the window
			Text = Resources.IDS_MODLIBRARY_TITLE;

			InitializeDirectories();

			InitializeControls();
		}



		/********************************************************************/
		/// <summary>
		/// Reload if path has changed
		/// </summary>
		/********************************************************************/
		public void ReloadChanges()
		{
			// Get current path from settings
			string currentModLibPath = pathSettings.ModLibrary;

			// Check if path has changed
			if (currentModLibPath != initialModLibraryPath)
			{
				initialModLibraryPath = currentModLibPath;
				InitializeDirectories();

				// Reload the offline page now if it is shown, else
				// when it is shown the next time
				if (modeTabControl.SelectedIndex == Page_Offline)
					offlinePageControl.ReloadPage();
				else
					offlinePageControl.MarkForReload();
			}
		}

		#region WindowFormBase overrides
		/********************************************************************/
		/// <summary>
		/// Return the window settings name
		/// </summary>
		/********************************************************************/
		protected override string WindowSettingsName => "ModLibraryWindow";



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
		public NostalgicForm Form => this;
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// Is called when the window is shown for the first time
		/// </summary>
		/********************************************************************/
		private void ModLibraryForm_Shown(object sender, EventArgs e)
		{
			LoadCurrentPage();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the form is closed
		/// </summary>
		/********************************************************************/
		private void ModLibraryForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			onlinePageControl.Cleanup(settings);
			offlinePageControl.Cleanup(settings);

			// Save settings
			settings.IsOfflineMode = modeTabControl.SelectedIndex == Page_Offline;
			settings.PlayImmediately = playImmediatelyCheckBox.Checked;

			settings.SearchText = searchTextBox.Text;
			settings.SearchMode = searchModeComboBox.SelectedIndex;

			settings.Settings.SaveSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when a tab is selected
		/// </summary>
		/********************************************************************/
		private void Tab_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (initializeRunning)
				return;

			LoadCurrentPage();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when search mode combobox selection changes
		/// </summary>
		/********************************************************************/
		private void SearchModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (initializeRunning)
				return;

			DoSearch();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when search text changes
		/// </summary>
		/********************************************************************/
		private void SearchTextBox_TextChanged(object sender, EventArgs e)
		{
			if (initializeRunning)
				return;

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
			if (initializeRunning)
				return;

			DoSearch();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when debounce timer ticks (100ms after last keystroke)
		/// </summary>
		/********************************************************************/
		private void SearchDebounceTimer_Tick(object sender, EventArgs e)
		{
			searchDebounceTimer.Stop();

			DoSearch();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when play immediately checkbox is toggled
		/// </summary>
		/********************************************************************/
		private void PlayImmediatelyCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			onlinePageControl.SetPlayImmediately(playImmediatelyCheckBox.Checked);
			offlinePageControl.SetPlayImmediately(playImmediatelyCheckBox.Checked);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the canceled button is clicked
		/// </summary>
		/********************************************************************/
		private void Download_Canceled(object sender, EventArgs e)
		{
			onlinePageControl.CancelDownload();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when a module has been downloaded
		/// </summary>
		/********************************************************************/
		private void Online_ModuleDownloaded(object sender, Events.ModuleDownloadedEventArgs e)
		{
			offlinePageControl.AddDownloadedModule(e.LocalPath);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Initialize needed directories
		/// </summary>
		/********************************************************************/
		private void InitializeDirectories()
		{
			string cacheDirectory = Path.Combine(platformPath.SettingsPath, "ModLibrary");
			Directory.CreateDirectory(cacheDirectory);

			string modulesDirectory = pathSettings.ModLibrary;
			initialModLibraryPath = modulesDirectory;

			if (string.IsNullOrEmpty(modulesDirectory))
			{
				// Fallback to default location
				modulesDirectory = Path.Combine(cacheDirectory, "Modules");
			}

			modLibraryHelper.SetDirectories(cacheDirectory, modulesDirectory);
		}



		/********************************************************************/
		/// <summary>
		/// Load settings and initialize controls
		/// </summary>
		/********************************************************************/
		private void InitializeControls()
		{
			initializeRunning = true;

			try
			{
				settings = new ModLibraryWindowSettings(allWindowSettings);
				searchTextBox.Text = settings.SearchText;
				searchModeComboBox.SelectedIndex = settings.SearchMode;
				playImmediatelyCheckBox.Checked = settings.PlayImmediately;
				modeTabControl.SelectedIndex = settings.IsOfflineMode ? Page_Offline : Page_Online;

				downloadControl.Initialize();

				onlinePageControl.SetControls(downloadControl);
				onlinePageControl.Initialize(this, settings);
				offlinePageControl.Initialize(this, settings);
			}
			finally
			{
				initializeRunning = false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will make sure the current page has data loaded
		/// </summary>
		/********************************************************************/
		private void LoadCurrentPage()
		{
			currentPage = null;

			string path = string.Empty;

			switch (modeTabControl.SelectedIndex)
			{
				case Page_Online:
				{
					currentPage = onlinePageControl;
					path = settings.LastOnlinePath;
					break;
				}

				case Page_Offline:
				{
					currentPage = offlinePageControl;
					path = settings.LastOfflinePath;
					break;
				}
			}

			currentPage?.LoadPage(path, searchTextBox.Text, (SearchMode)searchModeComboBox.SelectedIndex);
		}



		/********************************************************************/
		/// <summary>
		/// Make a new search
		/// </summary>
		/********************************************************************/
		private void DoSearch()
		{
			currentPage.Search(searchTextBox.Text.Trim(), (SearchMode)searchModeComboBox.SelectedIndex);
		}
		#endregion
	}
}
