/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AudiusWindow.Pages;
using Polycode.NostalgicPlayer.External.Download;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AudiusWindow
{
	/// <summary>
	/// This shows the Audius window
	/// </summary>
	public partial class AudiusWindowForm : WindowFormBase2, IAudiusWindowApi
	{
		private const int Page_Trending = 0;
		private const int Page_Search = 1;

		private IPictureDownloader pictureDownloader;

		private IAudiusPage currentPage;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AudiusWindowForm()
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
		public void InitializeForm(IPictureDownloaderFactory pictureDownloaderFactory)
		{
			// Initialize picture downloader
			pictureDownloader = pictureDownloaderFactory.GetDownloader();
			pictureDownloader.SetMaxNumberInCache(100);

			// Set the title of the window
			Text = Resources.IDS_AUDIUS_TITLE;

			// Set the tab titles
			tabControl.Pages[Page_Trending].Text = Resources.IDS_AUDIUS_TAB_TRENDING;
			tabControl.Pages[Page_Search].Text = Resources.IDS_AUDIUS_TAB_SEARCH;

			// Initialize all pages
			trendingPageControl.Initialize(this, pictureDownloader, null);
			searchPageControl.Initialize(this, pictureDownloader, null);
		}

		#region WindowFormBase overrides
		/********************************************************************/
		/// <summary>
		/// Return the window settings name
		/// </summary>
		/********************************************************************/
		protected override string WindowSettingsName => "AudiusWindow";



		/********************************************************************/
		/// <summary>
		/// Return the URL to the help page
		/// </summary>
		/********************************************************************/
		protected override string HelpUrl => "audius.html";
		#endregion

		#region IAudiusWindowApi implementation
		/********************************************************************/
		/// <summary>
		/// Return the form of the Audius window
		/// </summary>
		/********************************************************************/
		public Form Form => this;
		#endregion

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
			searchPageControl.CleanupPage();

			pictureDownloader?.Dispose();
			pictureDownloader = null;
		}



		/********************************************************************/
		/// <summary>
		/// Is called when a tab is selected
		/// </summary>
		/********************************************************************/
		private void Tab_SelectedIndexChanged(object sender, EventArgs e)
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

			switch (tabControl.SelectedIndex)
			{
				case Page_Trending:
				{
					currentPage = trendingPageControl;
					break;
				}

				case Page_Search:
				{
					currentPage = searchPageControl;
					break;
				}
			}

			currentPage?.RefreshPage();
		}
		#endregion
	}
}
