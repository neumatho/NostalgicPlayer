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
	/// Render a single music item in an Audius list, such as a track or playlist
	/// </summary>
	public partial class AudiusMusicListItemControl : UserControl, IAudiusMusicListItem
	{
		private AudiusMusicListItem item;

		private TaskHelper taskHelper;
		private Bitmap coverBitmap = null;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AudiusMusicListItemControl()
		{
			InitializeComponent();

			Disposed += AudiusMusicListItemControl_Disposed;
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
			item = (AudiusMusicListItem)listItem;

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
			if (!string.IsNullOrEmpty(item.ImageUrl) && (coverBitmap == null))
			{
				taskHelper.RunTask(async (cancellationToken) =>
				{
					Bitmap originalBitmap = await pictureDownloader.GetPictureAsync(item.ImageUrl, cancellationToken);
					if (originalBitmap == null)
						return;

					// Scale the bitmap to 128 x 128 pixels
					coverBitmap = new Bitmap(originalBitmap, 128, 128);

					BeginInvoke(() =>
					{
						itemPictureBox.Image = coverBitmap;
					});
				}, (ex) =>
				{
					// Ignore any exceptions
				});
			}
		}
		#endregion

		#region IAudiusMusicListItem implementation
		/********************************************************************/
		/// <summary>
		/// Event called when to play tracks
		/// </summary>
		/********************************************************************/
		public event TrackEventHandler PlayTracks;



		/********************************************************************/
		/// <summary>
		/// Event called when to add tracks
		/// </summary>
		/********************************************************************/
		public event TrackEventHandler AddTracks;
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void AudiusMusicListItemControl_Disposed(object sender, EventArgs e)
		{
			taskHelper.CancelTask();

			coverBitmap?.Dispose();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Play_Click(object sender, EventArgs e)
		{
			// Just call the next event handler
			if (PlayTracks != null)
				PlayTracks(sender, new TrackEventArgs(item));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Add_Click(object sender, EventArgs e)
		{
			// Just call the next event handler
			if (AddTracks != null)
				AddTracks(sender, new TrackEventArgs(item));
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
			titleLabel.Text = item.Title;
			artistLabel.Text = item.Artist;
			durationLabel.Text = item.Duration.ToFormattedString();
			repostsLabel.Text = string.Format(Resources.IDS_AUDIUS_ITEM_REPOSTS, item.Reposts.ToBeautifiedString());
			favoritesLabel.Text = item.Favorites.ToBeautifiedString();

			if (item.Plays != 0)
			{
				playsLabel.Visible = true;
				playsLabel.Text = string.Format(Resources.IDS_AUDIUS_ITEM_PLAYS, item.Plays.ToBeautifiedString());
			}
			else
				playsLabel.Visible = false;
		}
		#endregion
	}
}
