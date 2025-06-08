/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Audius;
using Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.Pages;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Bases;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow
{
	/// <summary>
	/// This shows the Audius window
	/// </summary>
	public partial class AudiusWindowForm : WindowFormBase
	{
		private const int Page_Trending = 0;

		private readonly MainWindowForm mainWindow;
		private readonly AudiusApi audiusApi;

		private IAudiusPage currentPage;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AudiusWindowForm(MainWindowForm mainWindow, OptionSettings optionSettings)
		{
			InitializeComponent();

			// Remember the arguments
			this.mainWindow = mainWindow;

			if (!DesignMode)
			{
				InitializeWindow(mainWindow, optionSettings);

				// Load window settings
				LoadWindowSettings("AudiusWindow");

				// Set the title of the window
				Text = Resources.IDS_AUDIUS_TITLE;

				// Set the tab titles
				navigator.Pages[Page_Trending].Text = Resources.IDS_AUDIUS_TAB_TRENDING;

				// Initialize the Audius API
				audiusApi = new AudiusApi();

				// Initialize all pages
				trendingPageControl.Initialize(mainWindow, this, audiusApi);
			}
		}

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// Is called when the form is shown for the first time
		/// </summary>
		/********************************************************************/
		private void AudiusForm_Shown(object sender, EventArgs e)
		{
			RefreshCurrentPage();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the form is closed
		/// </summary>
		/********************************************************************/
		private void AudiusForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			trendingPageControl.CleanupPage();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when a tab is selected
		/// </summary>
		/********************************************************************/
		private void Navigator_SelectedPageChanged(object sender, EventArgs e)
		{
			RefreshCurrentPage();
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Refresh current page
		/// </summary>
		/********************************************************************/
		private void RefreshCurrentPage()
		{
			currentPage?.CleanupPage();
			currentPage = null;

			switch (navigator.SelectedIndex)
			{
				case Page_Trending:
				{
					currentPage = trendingPageControl;
					break;
				}
			}

			currentPage?.RefreshPage();
		}
		#endregion
	}
}
