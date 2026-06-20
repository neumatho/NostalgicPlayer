/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Drawing;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AudiusWindow.Events;
using Polycode.NostalgicPlayer.Controls;
using Polycode.NostalgicPlayer.Controls.Events;
using Polycode.NostalgicPlayer.Controls.Extensions;
using Polycode.NostalgicPlayer.Controls.Images;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;
using Polycode.NostalgicPlayer.External;
using Polycode.NostalgicPlayer.External.Download;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AudiusWindow.ListItems
{
	/// <summary>
	/// Render a single profile item in an Audius list
	/// </summary>
	public partial class AudiusProfileListItemControl : UserControl, IDependencyInjectionControl, IAudiusProfileListItem
	{
		private IThemeManager themeManager;
		private INostalgicImageBank imageBank;

		private AudiusProfileListItem item;

		private TaskHelper taskHelper;
		private Bitmap profileBitmap = null;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AudiusProfileListItemControl()
		{
			InitializeComponent();

			Disposed += AudiusProfileListItemControl_Disposed;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the control
		///
		/// Called from FormCreatorService
		/// </summary>
		/********************************************************************/
		public void InitializeControl(IThemeManager themeManager, INostalgicImageBank imageBank)
		{
			this.themeManager = themeManager;
			this.imageBank = imageBank;

			themeManager.ThemeChanged += ThemeChanged;
		}

		#region IAudiusListItem implementation
		/********************************************************************/
		/// <summary>
		/// Return the control itself
		/// </summary>
		/********************************************************************/
		public Control Control => this;



		/********************************************************************/
		/// <summary>
		/// Will initialize the control
		/// </summary>
		/********************************************************************/
		public void Initialize(AudiusListItem listItem)
		{
			item = (AudiusProfileListItem)listItem;

			SetupControls();
			InitializePictures();

			taskHelper = new TaskHelper();
		}



		/********************************************************************/
		/// <summary>
		/// Will make sure that the item is refreshed with all missing data
		/// </summary>
		/********************************************************************/
		public void RefreshItem(IPictureDownloader pictureDownloader)
		{
			if (!string.IsNullOrEmpty(item.ImageUrl) && (profileBitmap == null))
			{
				taskHelper.RunTask(async (cancellationToken) =>
				{
					Bitmap originalBitmap = await pictureDownloader.GetPictureAsync(item.ImageUrl, cancellationToken);
					if (originalBitmap == null)
						return;

					// Scale the bitmap to 128 x 128 pixels
					using (Bitmap scaledBitmap = new Bitmap(originalBitmap, 128, 128))
					{
						// Create a circular bitmap
						profileBitmap = scaledBitmap.CreateCircularBitmap();
					}

					BeginInvoke(() =>
					{
						itemPictureBox.Image = profileBitmap;
					});
				}, (ex) =>
				{
					// Ignore any exceptions
				});
			}
		}
		#endregion

		#region IAudiusProfileListItem implementation
		/********************************************************************/
		/// <summary>
		/// Event called when to show user information
		/// </summary>
		/********************************************************************/
		public event ProfileEventHandler ShowProfile;
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void AudiusProfileListItemControl_Disposed(object sender, EventArgs e)
		{
			themeManager.ThemeChanged -= ThemeChanged;

			taskHelper.CancelTask();

			profileBitmap?.Dispose();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the theme changes
		/// </summary>
		/********************************************************************/
		private void ThemeChanged(object sender, ThemeChangedEventArgs e)
		{
			InitializePictures();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ShowInfo_Click(object sender, EventArgs e)
		{
			// Just call the next event handler
			if (ShowProfile != null)
				ShowProfile(sender, new ProfileEventArgs(item));
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
			positionLabel.Text = item.Position.ToString();
			nameLabel.Text = item.User.Name;
			handleLabel.Text = "@" + item.User.Handle;
		}



		/********************************************************************/
		/// <summary>
		/// Will render the picture images
		/// </summary>
		/********************************************************************/
		private void InitializePictures()
		{
			if (profileBitmap == null)
				itemPictureBox.Image = imageBank.Audius.UnknownProfile;
		}
		#endregion
	}
}
