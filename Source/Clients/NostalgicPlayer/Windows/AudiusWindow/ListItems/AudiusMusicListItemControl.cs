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
	/// Render a single music item in an Audius list, such as a track or playlist
	/// </summary>
	public partial class AudiusMusicListItemControl : UserControl, IDependencyInjectionControl, IAudiusMusicListItem
	{
		private IThemeManager themeManager;
		private INostalgicImageBank imageBank;

		private AudiusMusicListItem item;

		private TaskHelper taskHelper;
		private Bitmap coverBitmap;

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
			item = (AudiusMusicListItem)listItem;

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
			themeManager.ThemeChanged -= ThemeChanged;

			taskHelper.CancelTask();

			coverBitmap?.Dispose();
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



		/********************************************************************/
		/// <summary>
		/// Will render the picture images
		/// </summary>
		/********************************************************************/
		private void InitializePictures()
		{
			repostsPictureBox.Image = imageBank.Audius.Repost;
			favoritePictureBox.Image = imageBank.Audius.Favorite;

			if (coverBitmap == null)
				itemPictureBox.Image = imageBank.Audius.UnknownAlbumCover;
		}
		#endregion
	}
}
