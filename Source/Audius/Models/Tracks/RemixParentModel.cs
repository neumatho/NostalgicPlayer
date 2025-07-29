/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Polycode.NostalgicPlayer.Audius.Models.Tracks
{
	/// <summary>
	/// 
	/// </summary>
	public class RemixParentModel
	{
		/// <summary></summary>
		[JsonPropertyName("tracks")]
		public List<TrackElementModel> Tracks { get; set; }
	}
}
