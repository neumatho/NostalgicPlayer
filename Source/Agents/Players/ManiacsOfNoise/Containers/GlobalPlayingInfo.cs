/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.ManiacsOfNoise.Containers
{
	/// <summary>
	/// Holds global information about the playing state
	/// </summary>
	internal class GlobalPlayingInfo : IDeepCloneable<GlobalPlayingInfo>
	{
		public bool EnablePlaying { get; set; }

		public ushort MasterVolume { get; set; }
		public byte FadeStartVolume { get; set; }
		public byte FadeDestination { get; set; }
		public sbyte FadeIncrement { get; set; }
		public byte FadeSpeed { get; set; }
		public byte FadeSpeedCounter { get; set; }

		public ushort CurrentSpeed { get; set; }

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
