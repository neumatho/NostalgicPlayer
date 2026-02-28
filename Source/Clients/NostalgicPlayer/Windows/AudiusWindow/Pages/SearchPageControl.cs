/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Controls;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Services;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AudiusWindow.Events;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AudiusWindow.ListItems;
using Polycode.NostalgicPlayer.External;
using Polycode.NostalgicPlayer.External.Audius;
using Polycode.NostalgicPlayer.External.Audius.Interfaces;
using Polycode.NostalgicPlayer.External.Audius.Models.Playlists;
using Polycode.NostalgicPlayer.External.Audius.Models.Tracks;
using Polycode.NostalgicPlayer.External.Audius.Models.Users;
using Polycode.NostalgicPlayer.External.Download;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AudiusWindow.Pages
{
	/// <summary>
	/// Holds all the controls for the Search tab
	/// </summary>
	public partial class SearchPageControl : UserControl, IDependencyInjectionControl, IAudiusPage
	{
		private IAudiusWindowApi audiusWindowApi;
		private IAudiusClientFactory clientFactory;
		private IAudiusHelper audiusHelper;
		private IFormCreatorService formCreatorService;

		private IPictureDownloader pictureDownloader;

		private TaskHelper taskHelper;

		private ProfileControl profileControl;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SearchPageControl()
		{
			InitializeComponent();
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the control
		///
		/// Called from FormCreatorService
		/// </summary>
		/********************************************************************/
		public void InitializeControl(IAudiusHelper audiusHelper, IAudiusClientFactory audiusClientFactory, IFormCreatorService formCreatorService)
		{
			this.audiusHelper = audiusHelper;
			clientFactory = audiusClientFactory;
			this.formCreatorService = formCreatorService;
		}

		#region IAudiusPage implementation
		/********************************************************************/
		/// <summary>
		/// Will initialize the control
		/// </summary>
		/********************************************************************/
		public void Initialize(IAudiusWindowApi audiusWindow, IPictureDownloader downloader, string id)
		{
			audiusWindowApi = audiusWindow;

			pictureDownloader = downloader;

			audiusListControl.Initialize(downloader);

			taskHelper = new TaskHelper();
		}



		/********************************************************************/
		/// <summary>
		/// Will refresh the page with new data
		/// </summary>
		/********************************************************************/
		public void RefreshPage()
		{
			using (new SleepCursor())
			{
				CleanupPage();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup used resources
		/// </summary>
		/********************************************************************/
		public void CleanupPage()
		{
			using (new SleepCursor())
			{
				taskHelper.CancelTask();

				audiusListControl.ClearItems();
			}
		}
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TypeTracks_CheckedChanged(object sender, EventArgs e)
		{
			if (typeTracksRadioButton.Checked)
				CleanupPage();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TypePlaylists_CheckedChanged(object sender, EventArgs e)
		{
			if (typePlaylistsRadioButton.Checked)
				CleanupPage();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TypeProfiles_CheckedChanged(object sender, EventArgs e)
		{
			if (typeProfilesRadioButton.Checked)
				CleanupPage();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Search_Click(object sender, EventArgs e)
		{
			DoTheSearch(searchTextBox.Text);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Search_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				searchButton.PerformClick();

				e.Handled = true;
				e.SuppressKeyPress = true;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void AudiusList_ShowProfile(object sender, ProfileEventArgs e)
		{
			using (new SleepCursor())
			{
				controlPanel.Visible = false;

				profileControl = new ProfileControl();
				profileControl.Dock = DockStyle.Fill;

				profileControl.Close += Profile_Close;

				Controls.Add(profileControl);

				formCreatorService.InitializeControl(profileControl);
				profileControl.Initialize(e.Item.User, audiusWindowApi, pictureDownloader);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Profile_Close(object sender, EventArgs e)
		{
			profileControl.Close -= Profile_Close;
			profileControl.Dispose();

			controlPanel.Visible = true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will search after the given text
		/// </summary>
		/********************************************************************/
		private void DoTheSearch(string searchText)
		{
			if (string.IsNullOrEmpty(searchText))
				return;

			using (new SleepCursor())
			{
				CleanupPage();
				audiusListControl.SetLoading(true, Resources.IDS_AUDIUS_SEARCHING);

				if (typeTracksRadioButton.Checked)
					SearchForTracks(searchText);
				else if (typePlaylistsRadioButton.Checked)
					SearchForPlaylists(searchText);
				else if (typeProfilesRadioButton.Checked)
					SearchForProfiles(searchText);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will search after tracks with the text given
		/// </summary>
		/********************************************************************/
		private void SearchForTracks(string searchText)
		{
			taskHelper.RunTask((cancellationToken) =>
			{
				ITrackClient trackClient = clientFactory.GetTrackClient();
				TrackModel[] tracks = trackClient.Search(searchText, cancellationToken);

				List<AudiusListItem> items = tracks
					.Take(AudiusConstants.MaxSearchResults)
					.Select((x, i) => AudiusMapper.MapTrackToItem(x, i + 1))
					.Cast<AudiusListItem>()
					.ToList();

				cancellationToken.ThrowIfCancellationRequested();

				Invoke(() =>
				{
					using (new SleepCursor())
					{
						audiusListControl.SetLoading(false);
						audiusListControl.SetItems(items);
					}
				});

				return Task.CompletedTask;
			}, (ex) => audiusHelper.ShowErrorMessage(ex, audiusListControl, audiusWindowApi));
		}



		/********************************************************************/
		/// <summary>
		/// Will search after playlists with the text given
		/// </summary>
		/********************************************************************/
		private void SearchForPlaylists(string searchText)
		{
			taskHelper.RunTask((cancellationToken) =>
			{
				IPlaylistClient playlistClient = clientFactory.GetPlaylistClient();
				PlaylistModel[] playlists = playlistClient.Search(searchText, cancellationToken);

				List<AudiusListItem> items = audiusHelper.FillPlaylistsWithTracks(playlists, cancellationToken);

				Invoke(() =>
				{
					using (new SleepCursor())
					{
						audiusListControl.SetLoading(false);
						audiusListControl.SetItems(items);
					}
				});

				return Task.CompletedTask;
			}, (ex) => audiusHelper.ShowErrorMessage(ex, audiusListControl, audiusWindowApi));
		}



		/********************************************************************/
		/// <summary>
		/// Will search after profiles with the text given
		/// </summary>
		/********************************************************************/
		private void SearchForProfiles(string searchText)
		{
			taskHelper.RunTask((cancellationToken) =>
			{
				IUserClient userClient = clientFactory.GetUserClient();
				UserModel[] users = userClient.Search(searchText, cancellationToken);

				List<AudiusListItem> items = users
					.Take(AudiusConstants.MaxSearchResults)
					.Select((x, i) => AudiusMapper.MapUserToItem(x, i + 1))
					.Cast<AudiusListItem>()
					.ToList();

				cancellationToken.ThrowIfCancellationRequested();

				Invoke(() =>
				{
					using (new SleepCursor())
					{
						audiusListControl.SetLoading(false);
						audiusListControl.SetItems(items);
					}
				});

				return Task.CompletedTask;
			}, (ex) => audiusHelper.ShowErrorMessage(ex, audiusListControl, audiusWindowApi));
		}
		#endregion
	}
}
