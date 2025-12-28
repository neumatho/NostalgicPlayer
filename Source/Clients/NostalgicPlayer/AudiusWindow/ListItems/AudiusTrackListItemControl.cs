/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.Events;
using Polycode.NostalgicPlayer.Kit.Gui.Extensions;
using Polycode.NostalgicPlayer.RestClients;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.ListItems
{
	/// <summary>
	/// Render a single track item in an Audius play list
	/// </summary>
	public partial class AudiusTrackListItemControl : UserControl, IAudiusMusicListItem
	{
		private AudiusMusicListItem item;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AudiusTrackListItemControl()
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

			SetupControls();
		}



		/********************************************************************/
		/// <summary>
		/// Will make sure that the item is refreshed with all missing data
		/// </summary>
		/********************************************************************/
		public void RefreshItem(PictureDownloader pictureDownloader)
		{
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
			titleLabel.Text = $"{item.Position}. {item.Title} by {item.Artist}";
			durationLabel.Text = item.Duration.ToFormattedString();
		}
		#endregion
	}
}
