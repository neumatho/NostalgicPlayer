/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Drawing;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Audius;
using Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.Events;
using Polycode.NostalgicPlayer.GuiKit.Extensions;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.ListItems
{
	/// <summary>
	/// Render a single profile item in an Audius list
	/// </summary>
	public partial class AudiusProfileListItemControl : UserControl, IAudiusProfileListItem
	{
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

			taskHelper = new TaskHelper();
		}



		/********************************************************************/
		/// <summary>
		/// Will make sure that the item is refreshed with all missing data
		/// </summary>
		/********************************************************************/
		public void RefreshItem(PictureDownloader pictureDownloader)
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
			taskHelper.CancelTask();

			profileBitmap?.Dispose();
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
		#endregion
	}
}
