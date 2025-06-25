/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Text.Json.Serialization;
using Polycode.NostalgicPlayer.Audius.Models.Users;

namespace Polycode.NostalgicPlayer.Audius.Models.Playlists
{
	/// <summary>
	/// 
	/// </summary>
	public class PlaylistModel
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
		[JsonPropertyName("total_play_count")]
		[JsonRequired]
		public int TotalPlayCount { get; set; }

		/// <summary></summary>
		[JsonPropertyName("user")]
		[JsonRequired]
		public UserModel User { get; set; }

		/// <summary></summary>
		[JsonPropertyName("playlist_contents")]
		[JsonRequired]
		public PlaylistContentModel[] Tracks { get; set; }
	}
}
