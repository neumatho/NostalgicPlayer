/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Text.Json.Serialization;
using Polycode.NostalgicPlayer.External.Audius.Models.Tracks;
using Polycode.NostalgicPlayer.External.Audius.Models.Users;

namespace Polycode.NostalgicPlayer.External.Audius.Models.Playlists
{
	/// <summary>
	/// 
	/// </summary>
	public class TrendingPlaylistModel
	{
		/// <summary></summary>
		[JsonPropertyName("artwork")]
		public PlaylistArtworkModel Artwork { get; set; }

		/// <summary></summary>
		[JsonPropertyName("description")]
		public string Description { get; set; }

		/// <summary></summary>
		[JsonPropertyName("permalink")]
		public string Permalink { get; set; }

		/// <summary></summary>
		[JsonPropertyName("id")]
		[JsonRequired]
		public string Id { get; set; }

		/// <summary></summary>
		[JsonPropertyName("is_album")]
		[JsonRequired]
		public bool IsAlbum { get; set; }

		/// <summary></summary>
		[JsonPropertyName("playlist_name")]
		[JsonRequired]
		public string PlaylistName { get; set; }

		/// <summary></summary>
		[JsonPropertyName("repost_count")]
		[JsonRequired]
		public int RepostCount { get; set; }

		/// <summary></summary>
		[JsonPropertyName("favorite_count")]
		[JsonRequired]
		public int FavoriteCount { get; set; }

		/// <summary></summary>
		[JsonPropertyName("user")]
		[JsonRequired]
		public UserModel User { get; set; }

		/// <summary></summary>
		[JsonPropertyName("tracks")]
		[JsonRequired]
		public TrackModel[] Tracks { get; set; }
	}
}
