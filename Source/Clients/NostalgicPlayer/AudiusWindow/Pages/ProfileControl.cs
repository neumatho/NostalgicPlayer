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
using Polycode.NostalgicPlayer.Audius;
using Polycode.NostalgicPlayer.Audius.Models.Users;
using Polycode.NostalgicPlayer.GuiKit.Extensions;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.Pages
{
	/// <summary>
	/// Shows the given profile information
	/// </summary>
	public partial class ProfileControl : UserControl
	{
		private PictureDownloader pictureDownloader;

		private UserModel userModel;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ProfileControl()
		{
			InitializeComponent();
		}



		/********************************************************************/
		/// <summary>
		/// Will initialize the control
		/// </summary>
		/********************************************************************/
		public void Initialize(UserModel user, PictureDownloader downloader)
		{
			pictureDownloader = downloader;

			userModel = user;

			SetupControls();
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
		#endregion
	}
}
