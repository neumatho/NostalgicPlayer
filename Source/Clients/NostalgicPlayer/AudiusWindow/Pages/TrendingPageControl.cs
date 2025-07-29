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
using Krypton.Toolkit;
using Polycode.NostalgicPlayer.Audius;
using Polycode.NostalgicPlayer.Audius.Interfaces;
using Polycode.NostalgicPlayer.Audius.Models.Playlists;
using Polycode.NostalgicPlayer.Audius.Models.Tracks;
using Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.ListItems;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Controls;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.Pages
{
	/// <summary>
	/// Holds all the controls for the Trending tab
	/// </summary>
	public partial class TrendingPageControl : UserControl, IAudiusPage
	{
		private IMainWindowApi mainWindowApi;
		private IAudiusWindowApi audiusWindowApi;

		private TaskHelper taskHelper;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public TrendingPageControl()
		{
			InitializeComponent();

			genreComboBox.Items.AddRange(
			[
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ALL) { Tag = "" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC) { Tag = "Electronic" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ROCK) { Tag = "Rock" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_METAL) { Tag = "Metal" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ALTERNATIVE) { Tag = "Alternative" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_HIP_HOP_RAP) { Tag = "Hip-Hop/Rap" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_EXPERIMENTAL) { Tag = "Experimental" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_PUNK) { Tag = "Punk" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_FOLK) { Tag = "Folk" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_POP) { Tag = "Pop" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_AMBIENT) { Tag = "Ambient" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_SOUNDTRACK) { Tag = "Soundtrack" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_WORLD) { Tag = "World" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_JAZZ) { Tag = "Jazz" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ACOUSTIC) { Tag = "Acoustic" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_FUNK) { Tag = "Funk" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_R_AND_B_SOUL) { Tag = "R&B/Soul" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_DEVOTIONAL) { Tag = "Devotional" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_CLASSICAL) { Tag = "Classical" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_REGGAE) { Tag = "Reggae" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_PODCASTS) { Tag = "Podcasts" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_COUNTRY) { Tag = "Country" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_SPOKEN_WORK) { Tag = "Spoken Word" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_COMEDY) { Tag = "Comedy" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_BLUES) { Tag = "Blues" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_KIDS) { Tag = "Kids" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_AUDIOBOOKS) { Tag = "Audiobooks" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_LATIN) { Tag = "Latin" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_LOFI) { Tag = "Lo-Fi" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_HYPERPOP) { Tag = "Hyperpop" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_DANCEHALL) { Tag = "Dancehall" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_TECHNO) { Tag = "Techno" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_TRAP) { Tag = "Trap" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_HOUSE) { Tag = "House" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_TECH_HOUSE) { Tag = "Tech House" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_DEEP_HOUSE) { Tag = "Deep House" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_DISCO) { Tag = "Disco" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_ELECTRO) { Tag = "Electro" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_JUNGLE) { Tag = "Jungle" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_PROGRESSIVE_HOUSE) { Tag = "Progressive House" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_HARDSTYLE) { Tag = "Hardstyle" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_GLITCH_HOP) { Tag = "Glitch Hop" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_TRANCE) { Tag = "Trance" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_FUTURE_BASS) { Tag = "Future Bass" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_FUTURE_HOUSE) { Tag = "Future House" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_TROPICAL_HOUSE) { Tag = "Tropical House" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_DOWNTEMPO) { Tag = "Downtempo" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_DRUM_AND_BASS) { Tag = "Drum & Bass" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_DUBSTEP) { Tag = "Dubstep" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_JERSEY_CLUB) { Tag = "Jersey Club" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_VAPORWAVE) { Tag = "Vaporwave" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_MOOMBAHTON) { Tag = "Moombahton" }
			]);

			genreComboBox.SelectedIndex = 0;
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
				audiusListControl.SetLoading(true);

				if (typeTracksRadioButton.Checked)
					GetTrendingTracks();
				else if (typePlaylistsRadioButton.Checked)
					GetTrendingPlaylists();
				else if (typeUndergroundRadioButton.Checked)
					GetUndergroundTracks();
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
		private void TimeWeek_CheckedChanged(object sender, EventArgs e)
		{
			if (timeWeekRadioButton.Checked)
				RefreshPage();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TimeMonth_CheckedChanged(object sender, EventArgs e)
		{
			if (timeMonthRadioButton.Checked)
				RefreshPage();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TimeYear_CheckedChanged(object sender, EventArgs e)
		{
			if (timeYearRadioButton.Checked)
				RefreshPage();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TimeAll_CheckedChanged(object sender, EventArgs e)
		{
			if (timeAllRadioButton.Checked)
				RefreshPage();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Genre_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (taskHelper != null)
				RefreshPage();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TypeTracks_CheckedChanged(object sender, EventArgs e)
		{
			if (typeTracksRadioButton.Checked)
			{
				timePanel.Enabled = true;
				genreComboBox.Enabled = true;

				RefreshPage();
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TypePlaylists_CheckedChanged(object sender, EventArgs e)
		{
			if (typePlaylistsRadioButton.Checked)
			{
				timePanel.Enabled = true;
				genreComboBox.Enabled = false;

				RefreshPage();
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TypeUnderground_CheckedChanged(object sender, EventArgs e)
		{
			if (typeUndergroundRadioButton.Checked)
			{
				timePanel.Enabled = false;
				genreComboBox.Enabled = false;

				RefreshPage();
			}
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Return the string to use as the time parameter
		/// </summary>
		/********************************************************************/
		private string GetTimeValue()
		{
			string time = string.Empty;

			if (timeWeekRadioButton.Checked)
				time = "week";
			else if (timeMonthRadioButton.Checked)
				time = "month";
			else if (timeYearRadioButton.Checked)
				time = "year";
			else if (timeAllRadioButton.Checked)
				time = "allTime";

			return time;
		}



		/********************************************************************/
		/// <summary>
		/// Return the string to use as the genre parameter
		/// </summary>
		/********************************************************************/
		private string GetGenreValue()
		{
			return ((KryptonListItem)genreComboBox.SelectedItem).Tag!.ToString();
		}



		/********************************************************************/
		/// <summary>
		/// Retrieve the trending tracks from Audius
		/// </summary>
		/********************************************************************/
		private void GetTrendingTracks()
		{
			string time = GetTimeValue();
			string genre = GetGenreValue();

			taskHelper.RunTask((cancellationToken) =>
			{
				AudiusApi audiusApi = new AudiusApi();

				ITrackClient trackClient = audiusApi.GetTrackClient();
				TrackModel[] tracks = trackClient.GetTrendingTracks(genre, time, cancellationToken);

				List<AudiusListItem> items = tracks
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
		/// Retrieve the trending playlists from Audius
		/// </summary>
		/********************************************************************/
		private void GetTrendingPlaylists()
		{
			string time = GetTimeValue();

			taskHelper.RunTask((cancellationToken) =>
			{
				AudiusApi audiusApi = new AudiusApi();

				IPlaylistClient playlistClient = audiusApi.GetPlaylistClient();
				TrendingPlaylistModel[] playlists = playlistClient.GetTrendingPlaylists(time, cancellationToken);

				List<AudiusListItem> items = playlists
					.Select((x, i) => AudiusMapper.MapPlaylistToItem(x, i + 1))
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
		/// Retrieve the underground trending tracks from Audius
		/// </summary>
		/********************************************************************/
		private void GetUndergroundTracks()
		{
			taskHelper.RunTask((cancellationToken) =>
			{
				AudiusApi audiusApi = new AudiusApi();

				ITrackClient trackClient = audiusApi.GetTrackClient();
				TrackModel[] tracks = trackClient.GetTrendingUndergroundTracks(cancellationToken);

				List<AudiusListItem> items = tracks
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
		#endregion
	}
}
