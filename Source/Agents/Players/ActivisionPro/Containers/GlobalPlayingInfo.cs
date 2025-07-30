/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.ActivisionPro.Containers
{
	/// <summary>
	/// Holds global information about the playing state
	/// </summary>
	internal class GlobalPlayingInfo : IDeepCloneable<GlobalPlayingInfo>
	{
		public sbyte SpeedVariationCounter { get; set; }
		public sbyte SpeedIndex { get; set; }
		public byte SpeedVariation2Counter { get; set; }
		public byte SpeedVariation2Speed { get; set; }

		public sbyte MasterVolumeFadeCounter { get; set; }
		public sbyte MasterVolumeFadeSpeed { get; set; }
		public ushort MasterVolume { get; set; }

		public sbyte GlobalTranspose { get; set; }

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
