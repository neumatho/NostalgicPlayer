/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using Polycode.NostalgicPlayer.Audius.Models.Playlists;
using Polycode.NostalgicPlayer.Audius.Models.Tracks;
using Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.ListItems;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow
{
	/// <summary>
	/// Different mappers for Audius
	/// </summary>
	public static class AudiusMapper
	{
		/********************************************************************/
		/// <summary>
		/// Map from a track to item
		/// </summary>
		/********************************************************************/
		public static AudiusMusicListItem MapTrackToItem(TrackModel track, int position)
		{
			return new AudiusMusicListItem(
				position,
				track.Id,
				track.Title,
				track.User.Name,
				track.Duration.HasValue ? TimeSpan.FromSeconds(track.Duration.Value) : TimeSpan.Zero,
				track.RepostCount,
				track.FavoriteCount,
				track.PlayCount ?? 0,
				track.Artwork?._150x150
			);
		}



		/********************************************************************/
		/// <summary>
		/// Map from a playlist to item
		/// </summary>
		/********************************************************************/
		public static AudiusMusicListItem MapPlaylistToItem(TrendingPlaylistModel playlist, int position)
		{
			return new AudiusMusicListItem(
				position,
				playlist.Id,
				playlist.PlaylistName,
				playlist.User.Name,
				TimeSpan.FromSeconds(playlist.Tracks.Sum(x => x.Duration ?? 0)),
				playlist.RepostCount,
				playlist.FavoriteCount,
				0,
				playlist.Artwork?._150x150
			)
			{
				Tracks = playlist.Tracks.Select((x, i) => MapTrackToItem(x, i + 1)).ToArray()
			};
		}



		/********************************************************************/
		/// <summary>
		/// Map from a playlist to item
		/// </summary>
		/********************************************************************/
		public static AudiusMusicListItem MapPlaylistToItem(PlaylistModel playlist, Dictionary<string, TrackModel> tracks, int position)
		{
			TrackModel[] playlistTracks = playlist.Tracks
				.Select(x => tracks.GetValueOrDefault(x.TrackId))
				.Where(x => x != null)
				.ToArray();

			return new AudiusMusicListItem(
				position,
				playlist.Id,
				playlist.PlaylistName,
				playlist.User.Name,
				TimeSpan.FromSeconds(playlistTracks.Sum(x => x.Duration ?? 0)),
				playlist.RepostCount,
				playlist.FavoriteCount,
				playlist.TotalPlayCount,
				playlist.Artwork?._150x150
			)
			{
				Tracks = playlistTracks.Select((x, i) => MapTrackToItem(x, i + 1)).ToArray()
			};
		}
	}
}
