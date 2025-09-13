/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.DigiBooster.Containers
{
	/// <summary>
	/// Holds information from the last round-trip
	/// </summary>
	internal class LastRoundInfo : IDeepCloneable<LastRoundInfo>
	{
		public ushort Period { get; set; }
		public byte SampleNumber { get; set; }
		public Effect Effect { get; set; }
		public byte EffectArg { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public LastRoundInfo MakeDeepClone()
		{
			return (LastRoundInfo)MemberwiseClone();
		}
	}
}
