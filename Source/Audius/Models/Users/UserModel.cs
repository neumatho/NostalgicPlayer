/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Text.Json.Serialization;

namespace Polycode.NostalgicPlayer.Audius.Models.Users
{
	/// <summary>
	/// 
	/// </summary>
	public class UserModel
	{
		/// <summary></summary>
		[JsonPropertyName("album_count")]
		[JsonRequired]
		public int AlbumCount { get; set; }

		/// <summary></summary>
		[JsonPropertyName("artist_pick_track_id")]
		public string ArtistPickTrackId { get; set; }

		/// <summary></summary>
		[JsonPropertyName("bio")]
		public string Bio { get; set; }

		/// <summary></summary>
		[JsonPropertyName("cover_photo")]
		public CoverPhotoModel CoverPhoto { get; set; }

		/// <summary></summary>
		[JsonPropertyName("followee_count")]
		[JsonRequired]
		public int FolloweeCount { get; set; }

		/// <summary></summary>
		[JsonPropertyName("follower_count")]
		[JsonRequired]
		public int FollowerCount { get; set; }

		/// <summary></summary>
		[JsonPropertyName("does_follow_current_user")]
		public bool DoesFollowCurrentUser { get; set; }

		/// <summary></summary>
		[JsonPropertyName("handle")]
		[JsonRequired]
		public string Handle { get; set; }

		/// <summary></summary>
		[JsonPropertyName("id")]
		[JsonRequired]
		public string Id { get; set; }

		/// <summary></summary>
		[JsonPropertyName("is_verified")]
		[JsonRequired]
		public bool IsVerified { get; set; }

		/// <summary></summary>
		[JsonPropertyName("location")]
		public string Location { get; set; }

		/// <summary></summary>
		[JsonPropertyName("name")]
		public string Name { get; set; }

		/// <summary></summary>
		[JsonPropertyName("playlist_count")]
		[JsonRequired]
		public int PlaylistCount { get; set; }

		/// <summary></summary>
		[JsonPropertyName("profile_picture")]
		public ProfilePictureModel ProfilePicture { get; set; }

		/// <summary></summary>
		[JsonPropertyName("repost_count")]
		[JsonRequired]
		public int RepostCount { get; set; }

		/// <summary></summary>
		[JsonPropertyName("track_count")]
		[JsonRequired]
		public int TrackCount { get; set; }

		/// <summary></summary>
		[JsonPropertyName("is_deactivated")]
		[JsonRequired]
		public bool IsDeactivated { get; set; }

		/// <summary></summary>
		[JsonPropertyName("is_available")]
		[JsonRequired]
		public bool IsAvailable { get; set; }

		/// <summary></summary>
		[JsonPropertyName("erc_wallet")]
		[JsonRequired]
		public string ErcWallet { get; set; }

		/// <summary></summary>
		[JsonPropertyName("spl_wallet")]
		[JsonRequired]
		public string SplWallet { get; set; }

		/// <summary></summary>
		[JsonPropertyName("supporter_count")]
		[JsonRequired]
		public int SupporterCount { get; set; }

		/// <summary></summary>
		[JsonPropertyName("supporting_count")]
		[JsonRequired]
		public int SupportingCount { get; set; }

		/// <summary></summary>
		[JsonPropertyName("total_audio_balance")]
		public int? TotalAudioBalance { get; set; }
	}
}
