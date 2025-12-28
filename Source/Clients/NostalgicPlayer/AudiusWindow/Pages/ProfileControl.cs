/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;
using Polycode.NostalgicPlayer.Kit.Gui.Extensions;
using Polycode.NostalgicPlayer.RestClients;
using Polycode.NostalgicPlayer.RestClients.Audius.Models.Users;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.Pages
{
	/// <summary>
	/// Shows the given profile information
	/// </summary>
	public partial class ProfileControl : UserControl
	{
		private const int Page_Tracks = 0;
		private const int Page_Playlists = 1;

		private PictureDownloader pictureDownloader;

		private UserModel userModel;
		private IAudiusPage currentPage;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ProfileControl()
		{
			InitializeComponent();

			Disposed += ProfileControl_Disposed;

			if (!DesignMode)
			{
				// Set the tab titles
				navigator.Pages[Page_Tracks].Text = Resources.IDS_AUDIUS_TAB_TRACKS;
				navigator.Pages[Page_Playlists].Text = Resources.IDS_AUDIUS_TAB_PLAYLISTS;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will initialize the control
		/// </summary>
		/********************************************************************/
		public void Initialize(UserModel user, IMainWindowApi mainWindow, IAudiusWindowApi audiusWindow, PictureDownloader downloader)
		{
			pictureDownloader = downloader;

			userModel = user;

			SetupControls();

			// Initialize all pages
			profileTracksPageControl.Initialize(mainWindow, audiusWindow, pictureDownloader, user.Id);
			profilePlaylistsPageControl.Initialize(mainWindow, audiusWindow, pictureDownloader, user.Id);

			RefreshCurrentPage();
		}

		#region Events
		/********************************************************************/
		/// <summary>
		/// Event called when to close the profile
		/// </summary>
		/********************************************************************/
		public event EventHandler Close;
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ProfileControl_Disposed(object sender, EventArgs e)
		{
			currentPage?.CleanupPage();
			currentPage = null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Close_MouseEnter(object sender, EventArgs e)
		{
			closeButton.Values.Image = Resources.IDB_CLOSE_BLACK;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Close_MouseLeave(object sender, EventArgs e)
		{
			closeButton.Values.Image = Resources.IDB_CLOSE_WHITE;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Close_Click(object sender, EventArgs e)
		{
			if (Close != null)
				Close(this, EventArgs.Empty);
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
		/// Setup all the controls with the values from the item
		/// </summary>
		/********************************************************************/
		private void SetupControls()
		{
			nameLabel.Text = userModel.Name;
			handleLabel.Text = "@" + userModel.Handle;

			DownloadPictures();

			Invalidate(true);
		}



		/********************************************************************/
		/// <summary>
		/// Download pictures and initialize them
		/// </summary>
		/********************************************************************/
		private void DownloadPictures()
		{
			Bitmap coverPhoto = null;
			Bitmap profilePhoto = null;

			Task.Run(async () =>
			{
				Task<Bitmap> coverPhotoTask = pictureDownloader.GetPictureAsync(userModel.CoverPhoto?._640x, CancellationToken.None);
				Task<Bitmap> profilePhotoTask = pictureDownloader.GetPictureAsync(userModel.ProfilePicture?._150x150, CancellationToken.None);

				try
				{
					coverPhoto = await coverPhotoTask;
				}
				catch (Exception)
				{
					// Ignore any errors here
				}

				try
				{
					profilePhoto = await profilePhotoTask;
				}
				catch (Exception)
				{
					// Ignore any errors here
				}
			}).Wait();

			if (coverPhoto != null)
				infoPanel.SetBackgroundImage(coverPhoto);
			else
				infoPanel.BackColor = Color.Gainsboro;

			if (profilePhoto != null)
			{
				// Scale the bitmap to 128 x 128 pixels
				using (Bitmap scaledBitmap = new Bitmap(profilePhoto, 128, 128))
				{
					profilePictureBox.Image = scaledBitmap.CreateCircularBitmap(2);
				}
			}
		}



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
				case Page_Tracks:
				{
					currentPage = profileTracksPageControl;
					break;
				}

				case Page_Playlists:
				{
					currentPage = profilePlaylistsPageControl;
					break;
				}
			}

			currentPage?.RefreshPage();
		}
		#endregion
	}
}
