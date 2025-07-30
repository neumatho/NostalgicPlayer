/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.HivelyTracker.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class HvlEnvelope : IDeepCloneable<HvlEnvelope>
	{
		public int AFrames { get; set; }
		public int AVolume { get; set; }

		public int DFrames { get; set; }
		public int DVolume { get; set; }

		public int SFrames { get; set; }

		public int RFrames { get; set; }
		public int RVolume { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public HvlEnvelope MakeDeepClone()
		{
			return (HvlEnvelope)MemberwiseClone();
		}
	}
}
