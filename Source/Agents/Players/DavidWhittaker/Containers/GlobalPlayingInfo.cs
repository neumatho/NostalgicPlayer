/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.DavidWhittaker.Containers
{
	/// <summary>
	/// Holds global information about the playing state
	/// </summary>
	internal class GlobalPlayingInfo : IDeepCloneable<GlobalPlayingInfo>
	{
		public sbyte Transpose { get; set; }

		public ushort VolumeFadeSpeed { get; set; }

		public ushort GlobalVolume { get; set; }
		public byte GlobalVolumeFadeSpeed { get; set; }
		public byte GlobalVolumeFadeCounter { get; set; }

		public ushort SquareChangePosition { get; set; }
		public bool SquareChangeDirection { get; set; }

		public byte ExtraCounter { get; set; }

		public byte DelayCounterSpeed { get; set; }
		public ushort DelayCounter { get; set; }

		public ushort Speed { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public GlobalPlayingInfo MakeDeepClone()
		{
			return (GlobalPlayingInfo)MemberwiseClone();
		}
	}
}
