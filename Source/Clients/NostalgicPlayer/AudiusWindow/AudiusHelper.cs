﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Polycode.NostalgicPlayer.Audius;
using Polycode.NostalgicPlayer.Audius.Interfaces;
using Polycode.NostalgicPlayer.Audius.Models.Playlists;
using Polycode.NostalgicPlayer.Audius.Models.Tracks;
using Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.ListItems;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow
{
	/// <summary>
	/// Different helper methods for Audius
	/// </summary>
	public static class AudiusHelper
	{
		/********************************************************************/
		/// <summary>
		/// Show error message
		/// </summary>
		/********************************************************************/
		public static void ShowErrorMessage(Exception ex, AudiusListControl audiusListControl, IMainWindowApi mainWindowApi, IAudiusWindowApi audiusWindowApi)
		{
			string message = ex is TimeoutException ? Resources.IDS_AUDIUS_ERR_TIMEOUT : ex.Message;

			audiusWindowApi.Form.BeginInvoke(() =>
			{
				audiusListControl.SetLoading(false);
				audiusListControl.SetItems([]);

				mainWindowApi.ShowSimpleErrorMessage(audiusWindowApi.Form, message);
			});
		}



		/********************************************************************/
		/// <summary>
		/// Get tracks for all playlists
		/// </summary>
		/********************************************************************/
		public static List<AudiusListItem> FillPlaylistsWithTracks(PlaylistModel[] playlists, CancellationToken cancellationToken)
		{
			AudiusApi audiusApi = new AudiusApi();

			ITrackClient trackClient = audiusApi.GetTrackClient();

			Dictionary<string, TrackModel> trackInfo = new Dictionary<string, TrackModel>();

			foreach (PlaylistModel playlist in playlists.Take(AudiusConstants.MaxSearchResults))
			{
				cancellationToken.ThrowIfCancellationRequested();

				List<string> tracksToRetrieve = playlist.Tracks
					.Select(x => x.TrackId)
					.Where(x => !trackInfo.ContainsKey(x))
					.Distinct()
					.ToList();

				TrackModel[] playlistTracks = trackClient.GetBulkTrackInfo(tracksToRetrieve, cancellationToken);

				foreach (TrackModel track in playlistTracks)
					trackInfo[track.Id] = track;
			}

			List<AudiusListItem> items = playlists
				.Select((x, i) => AudiusMapper.MapPlaylistToItem(x, trackInfo, i + 1))
				.Cast<AudiusListItem>()
				.ToList();

			return items;
		}
	}
}
