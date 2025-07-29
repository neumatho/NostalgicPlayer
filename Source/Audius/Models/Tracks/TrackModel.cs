/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Text.Json.Serialization;
using Polycode.NostalgicPlayer.Audius.Models.Users;

namespace Polycode.NostalgicPlayer.Audius.Models.Tracks
{
	/// <summary>
	/// 
	/// </summary>
	public class TrackModel
	{
		/// <summary></summary>
		[JsonPropertyName("artwork")]
		public TrackArtworkModel Artwork { get; set; }

		/// <summary></summary>
		[JsonPropertyName("description")]
		public string Description { get; set; }

		/// <summary></summary>
		[JsonPropertyName("genre")]
		public string Genre { get; set; }

		/// <summary></summary>
		[JsonPropertyName("id")]
		[JsonRequired]
		public string Id { get; set; }

		/// <summary></summary>
		[JsonPropertyName("track_cid")]
		public string TrackCid { get; set; }

		/// <summary></summary>
		[JsonPropertyName("mood")]
		public string Mood { get; set; }

		/// <summary></summary>
		[JsonPropertyName("release_date")]
		public string ReleaseDate { get; set; }

		/// <summary></summary>
		[JsonPropertyName("remix_of")]
		public RemixParentModel RemixOf { get; set; }

		/// <summary></summary>
		[JsonPropertyName("repost_count")]
		[JsonRequired]
		public int RepostCount { get; set; }

		/// <summary></summary>
		[JsonPropertyName("favorite_count")]
		[JsonRequired]
		public int FavoriteCount { get; set; }

		/// <summary></summary>
		[JsonPropertyName("tags")]
		public string Tags { get; set; }

		/// <summary></summary>
		[JsonPropertyName("title")]
		[JsonRequired]
		public string Title { get; set; }

		/// <summary></summary>
		[JsonPropertyName("user")]
		[JsonRequired]
		public UserModel User { get; set; }

		/// <summary></summary>
		[JsonPropertyName("duration")]
		public int? Duration { get; set; }

		/// <summary></summary>
		[JsonPropertyName("downloadable")]
		public bool Downloadable { get; set; }

		/// <summary></summary>
		[JsonPropertyName("play_count")]
		public int? PlayCount { get; set; }

		/// <summary></summary>
		[JsonPropertyName("permalink")]
		public string Permalink { get; set; }

		/// <summary></summary>
		[JsonPropertyName("is_streamable")]
		public bool IsStreamable { get; set; }
	}
}
