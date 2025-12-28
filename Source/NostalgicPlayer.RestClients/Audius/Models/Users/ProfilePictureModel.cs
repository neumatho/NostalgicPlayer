/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Text.Json.Serialization;

namespace Polycode.NostalgicPlayer.RestClients.Audius.Models.Users
{
	/// <summary>
	/// 
	/// </summary>
	public class ProfilePictureModel
	{
		/// <summary></summary>
		[JsonPropertyName("150x150")]
		public string _150x150 { get; set; }

		/// <summary></summary>
		[JsonPropertyName("480x480")]
		public string _480x480 { get; set; }

		/// <summary></summary>
		[JsonPropertyName("1000x1000")]
		public string _1000x1000 { get; set; }
	}
}
