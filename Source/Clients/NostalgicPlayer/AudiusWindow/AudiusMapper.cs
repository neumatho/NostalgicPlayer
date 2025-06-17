/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Linq;
using Polycode.NostalgicPlayer.Audius.Models.Playlists;
using Polycode.NostalgicPlayer.Audius.Models.Tracks;

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
		public static AudiusListItem MapTrackToItem(TrackModel track, int position)
		{
			return new AudiusListItem(
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
		public static AudiusListItem MapPlaylistToItem(TrendingPlaylistModel playlist, int position)
		{
			return new AudiusListItem(
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
	}
}
