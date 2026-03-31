/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Services;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.MainWindow;
using Polycode.NostalgicPlayer.Library.Containers;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModuleInfoWindow
{
	/// <summary>
	/// This shows the module information
	/// </summary>
	public partial class ModuleInfoWindowForm : WindowFormBase2
	{
		private IMainWindowApi mainWindowApi;
		private IModuleHandlerService moduleHandler;

		private ModuleInfoWindowSettings settings;
		private ModuleSettings moduleSettings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleInfoWindowForm()
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
		public void InitializeForm(IMainWindowApi mainWindowApi, IModuleHandlerService moduleHandlerService, ModuleSettings moduleSettings)
		{
			// Remember the arguments
			this.mainWindowApi = mainWindowApi;
			moduleHandler = moduleHandlerService;
			this.moduleSettings = moduleSettings;

			// Load window settings
			LoadWindowSettings("ModuleInfoWindow");
			settings = new ModuleInfoWindowSettings(allWindowSettings);

			// Set the title of the window
			Text = Resources.IDS_MODULE_INFO_TITLE;

			// Set the tab titles
			tabControl.Pages[(int)ModuleSettings.ModuleInfoTab.Info].Text = Resources.IDS_MODULE_INFO_TAB_INFO;
			tabControl.Pages[(int)ModuleSettings.ModuleInfoTab.Comments].Text = Resources.IDS_MODULE_INFO_TAB_COMMENT;
			tabControl.Pages[(int)ModuleSettings.ModuleInfoTab.Lyrics].Text = Resources.IDS_MODULE_INFO_TAB_LYRICS;
			tabControl.Pages[(int)ModuleSettings.ModuleInfoTab.Pictures].Text = Resources.IDS_MODULE_INFO_TAB_PICTURES;

			// Initialize all pages
			infoPageControl.InitControl(mainWindowApi, settings);
			picturesPageControl.InitControl();

			// Make sure that the content is up-to date
			RefreshWindow(false);
		}



		/********************************************************************/
		/// <summary>
		/// Will clear the window and add all the items again
		/// </summary>
		/********************************************************************/
		public void RefreshWindow(bool onLoad)
		{
			if (moduleHandler != null)
			{
				bool isPlaying = moduleHandler.IsPlaying;
				ModuleInfoStatic staticInfo = moduleHandler.StaticModuleInformation;
				ModuleInfoFloating floatingInfo = moduleHandler.PlayingModuleInformation;

				infoPageControl.RefreshControl(isPlaying, staticInfo, floatingInfo);
				tabControl.Pages[(int)ModuleSettings.ModuleInfoTab.Comments].Visible = commentPageControl.RefreshControl(isPlaying, staticInfo);
				tabControl.Pages[(int)ModuleSettings.ModuleInfoTab.Lyrics].Visible = lyricsPageControl.RefreshControl(isPlaying, staticInfo);
				tabControl.Pages[(int)ModuleSettings.ModuleInfoTab.Pictures].Visible = picturesPageControl.RefreshControl(isPlaying, staticInfo);

				// Find out which tab to activate
				if (onLoad)
				{
					foreach (ModuleSettings.ModuleInfoTab tab in moduleSettings.ModuleInfoActivateTabOrder)
					{
						if (tabControl.Pages[(int)tab].Visible)
						{
							tabControl.SelectedIndex = (int)tab;
							break;
						}
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will be called every time a new value has changed
		/// </summary>
		/********************************************************************/
		public void UpdateWindow(int line, string newValue)
		{
			if (moduleHandler != null)
			{
				// Check to see if there are any module loaded at the moment
				if (moduleHandler.IsModuleLoaded)
					infoPageControl.UpdateControl(line, newValue);
			}
		}

		#region WindowFormBase overrides
		/********************************************************************/
		/// <summary>
		/// Return the URL to the help page
		/// </summary>
		/********************************************************************/
		protected override string HelpUrl => "modinfo.html";
		#endregion

		#region Keyboard shortcuts
		/********************************************************************/
		/// <summary>
		/// Is called when a key is pressed in the window
		/// </summary>
		/********************************************************************/
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			// Check for different keyboard shortcuts
			Keys key = keyData & Keys.KeyCode;

			if (tabControl.SelectedIndex == (int)ModuleSettings.ModuleInfoTab.Pictures)
			{
				if (picturesPageControl.ProcessKey(key))
					return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// Is called when the window is closed
		/// </summary>
		/********************************************************************/
		private void ModuleInfoWindowForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (mainWindowApi != null)     // Main window is null, if the window has already been closed (because Owner has been set)
			{
				// Save the settings
				infoPageControl.SaveSettings(settings);

				// Cleanup
				picturesPageControl.CleanupControl();

				mainWindowApi = null;
				moduleHandler = null;
			}
		}
		#endregion
	}
}
