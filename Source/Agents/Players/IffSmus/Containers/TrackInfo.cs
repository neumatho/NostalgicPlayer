/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.IffSmus.Containers
{
	/// <summary>
	/// Hold current info for a single track
	/// </summary>
	internal class TrackInfo : IDeepCloneable<TrackInfo>
	{
		public byte InstrumentNumber { get; set; }
		public byte TimeLeft { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public TrackInfo MakeDeepClone()
		{
			return (TrackInfo)MemberwiseClone();
		}
	}
}
