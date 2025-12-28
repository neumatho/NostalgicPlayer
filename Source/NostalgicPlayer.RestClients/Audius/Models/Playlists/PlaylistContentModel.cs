/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Text.Json.Serialization;

namespace Polycode.NostalgicPlayer.RestClients.Audius.Models.Playlists
{
	/// <summary>
	/// 
	/// </summary>
	public class PlaylistContentModel
	{
		/// <summary></summary>
		[JsonPropertyName("track_id")]
		[JsonRequired]
		public string TrackId { get; set; }
	}
}
