﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Audius;
using Polycode.NostalgicPlayer.Audius.Interfaces;
using Polycode.NostalgicPlayer.Audius.Models.Playlists;
using Polycode.NostalgicPlayer.Audius.Models.Tracks;
using Polycode.NostalgicPlayer.Audius.Models.Users;
using Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.Events;
using Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.ListItems;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Controls;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.Pages
{
	/// <summary>
	/// Holds all the controls for the Search tab
	/// </summary>
	public partial class SearchPageControl : UserControl, IAudiusPage
	{
		private IMainWindowApi mainWindowApi;
		private IAudiusWindowApi audiusWindowApi;

		private PictureDownloader pictureDownloader;

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

		#region IAudiusPage implementation
		/********************************************************************/
		/// <summary>
		/// Will initialize the control
		/// </summary>
		/********************************************************************/
		public void Initialize(IMainWindowApi mainWindow, IAudiusWindowApi audiusWindow, PictureDownloader downloader, string id)
		{
			mainWindowApi = mainWindow;
			audiusWindowApi = audiusWindow;

			pictureDownloader = downloader;

			audiusListControl.Initialize(mainWindow, downloader);

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

				profileControl.Initialize(e.Item.User, mainWindowApi, audiusWindowApi, pictureDownloader);
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
				AudiusApi audiusApi = new AudiusApi();

				ITrackClient trackClient = audiusApi.GetTrackClient();
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
			}, (ex) => AudiusHelper.ShowErrorMessage(ex, audiusListControl, mainWindowApi, audiusWindowApi));
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
				AudiusApi audiusApi = new AudiusApi();

				IPlaylistClient playlistClient = audiusApi.GetPlaylistClient();
				PlaylistModel[] playlists = playlistClient.Search(searchText, cancellationToken);

				List<AudiusListItem> items = AudiusHelper.FillPlaylistsWithTracks(playlists, cancellationToken);

				Invoke(() =>
				{
					using (new SleepCursor())
					{
						audiusListControl.SetLoading(false);
						audiusListControl.SetItems(items);
					}
				});

				return Task.CompletedTask;
			}, (ex) => AudiusHelper.ShowErrorMessage(ex, audiusListControl, mainWindowApi, audiusWindowApi));
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
				AudiusApi audiusApi = new AudiusApi();

				IUserClient userClient = audiusApi.GetUserClient();
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
			}, (ex) => AudiusHelper.ShowErrorMessage(ex, audiusListControl, mainWindowApi, audiusWindowApi));
		}
		#endregion
	}
}
