/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Linq;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AudiusWindow.Events;
using Polycode.NostalgicPlayer.External.Download;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AudiusWindow.ListItems
{
	/// <summary>
	/// Render a single play list item
	/// </summary>
	public partial class AudiusPlaylistListItemControl : UserControl, IAudiusMusicListItem
	{
		private AudiusMusicListItem item;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AudiusPlaylistListItemControl()
		{
			InitializeComponent();
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

			audiusMusicListItemControl.Initialize(listItem);

			audiusMusicListItemControl.PlayTracks += ListItem_PlayAllTracks;
			audiusMusicListItemControl.AddTracks += ListItem_AddAllTracks;

			if (item.Tracks != null)
			{
				tracksFlowLayoutPanel.SuspendLayout();

				try
				{
					foreach (AudiusMusicListItem trackItem in item.Tracks.Take(AudiusConstants.MaxTracksPerPlaylist))
					{
						IAudiusMusicListItem trackListItem = new AudiusTrackListItemControl();

						trackListItem.Initialize(trackItem);

						trackListItem.PlayTracks += ListItem_PlayTracks;
						trackListItem.AddTracks += ListItem_AddTracks;

						tracksFlowLayoutPanel.Controls.Add(trackListItem.Control);
					}

					// Calculate the height of the first 6 tracks
					int trackHeight = 0;

					for (int i = Math.Min(6, tracksFlowLayoutPanel.Controls.Count) - 1; i >= 0; i--)
						trackHeight += tracksFlowLayoutPanel.Controls[i].Height;

					Height = audiusMusicListItemControl.Height + 2 + trackHeight;
				}
				finally
				{
					tracksFlowLayoutPanel.ResumeLayout();
				}

				// This need to be done after the layout is resumed, otherwise it will not work correctly
				SetItemWidths();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will make sure that the item is refreshed with all missing data
		/// </summary>
		/********************************************************************/
		public void RefreshItem(IPictureDownloader pictureDownloader)
		{
			audiusMusicListItemControl.RefreshItem(pictureDownloader);
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
		/// Is called when the flow layout resizes
		/// </summary>
		/********************************************************************/
		private void TrackFlowLayout_Resize(object sender, EventArgs e)
		{
			SetItemWidths();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when to clear the list and start playing the given
		/// track
		/// </summary>
		/********************************************************************/
		private void ListItem_PlayAllTracks(object sender, TrackEventArgs e)
		{
			if (PlayTracks != null)
				PlayTracks(this, new TrackEventArgs(item.Tracks));
		}



		/********************************************************************/
		/// <summary>
		/// Is called when to add the given track to the list
		/// </summary>
		/********************************************************************/
		private void ListItem_AddAllTracks(object sender, TrackEventArgs e)
		{
			if (AddTracks != null)
				AddTracks(this, new TrackEventArgs(item.Tracks));
		}



		/********************************************************************/
		/// <summary>
		/// Is called when to clear the list and start playing the given
		/// track
		/// </summary>
		/********************************************************************/
		private void ListItem_PlayTracks(object sender, TrackEventArgs e)
		{
			if (PlayTracks != null)
				PlayTracks(sender, e);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when to add the given track to the list
		/// </summary>
		/********************************************************************/
		private void ListItem_AddTracks(object sender, TrackEventArgs e)
		{
			if (AddTracks != null)
				AddTracks(sender, e);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Set width of all items
		/// </summary>
		/********************************************************************/
		private void SetItemWidths()
		{
			tracksFlowLayoutPanel.SuspendLayout();

			try
			{
				foreach (Control ctrl in tracksFlowLayoutPanel.Controls)
					ctrl.Width = tracksFlowLayoutPanel.ClientSize.Width - ctrl.Margin.Horizontal;
			}
			finally
			{
				tracksFlowLayoutPanel.ResumeLayout();
			}
		}
		#endregion
	}
}
