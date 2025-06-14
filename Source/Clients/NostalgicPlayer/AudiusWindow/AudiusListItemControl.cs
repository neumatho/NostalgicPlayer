/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Drawing;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Audius;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow.ListItem;
using Polycode.NostalgicPlayer.GuiKit.Extensions;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow
{
	/// <summary>
	/// Render a single item in an Audius list, such as a track or playlist
	/// </summary>
	public partial class AudiusListItemControl : UserControl
	{
		private readonly AudiusListItem item;
		private readonly IMainWindowApi mainWindowApi;

		private readonly TaskHelper taskHelper;
		private Bitmap coverBitmap = null;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AudiusListItemControl(AudiusListItem item, IMainWindowApi mainWindow)
		{
			InitializeComponent();

			Disposed += AudiusListItemControl_Disposed;

			this.item = item;
			mainWindowApi = mainWindow;

			SetupControls();

			taskHelper = new TaskHelper();
		}

		#region Public methods
		/********************************************************************/
		/// <summary>
		/// Will make sure that the item is refreshed with all missing data
		/// </summary>
		/********************************************************************/
		public void RefreshItem(PictureDownloader pictureDownloader)
		{
			if (!string.IsNullOrEmpty(item.CoverUrl) && (coverBitmap == null))
			{
				taskHelper.RunTask(async (cancellationToken) =>
				{
					Bitmap originalBitmap = await pictureDownloader.GetPictureAsync(item.CoverUrl, cancellationToken);
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

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void AudiusListItemControl_Disposed(object sender, EventArgs e)
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
			ModuleListItem listItem = CreateModuleListItem();

			mainWindowApi.AddItemsToModuleList([ listItem ], true);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Add_Click(object sender, EventArgs e)
		{
			ModuleListItem listItem = CreateModuleListItem();

			mainWindowApi.AddItemsToModuleList([ listItem ], false);
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
			playsLabel.Text = string.Format(Resources.IDS_AUDIUS_ITEM_PLAYS, item.Plays.ToBeautifiedString());
		}



		/********************************************************************/
		/// <summary>
		/// Will create a module item from the current Audius item
		/// </summary>
		/********************************************************************/
		private ModuleListItem CreateModuleListItem()
		{
			return new ModuleListItem(new AudiusModuleListItem(item.Title, item.ItemId))
			{
				Duration = item.Duration
			};
		}
		#endregion
	}
}
