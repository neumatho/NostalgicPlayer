/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Text.Json.Serialization;

namespace Polycode.NostalgicPlayer.External.Audius.Models.Users
{
	/// <summary>
	/// 
	/// </summary>
	public class CoverPhotoModel
	{
		/// <summary></summary>
		[JsonPropertyName("640x")]
		public string _640x { get; set; }

		/// <summary></summary>
		[JsonPropertyName("2000x")]
		public string _2000x { get; set; }
	}
}
