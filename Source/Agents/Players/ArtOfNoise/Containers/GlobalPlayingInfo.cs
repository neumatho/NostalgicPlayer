/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.ArtOfNoise.Containers
{
	/// <summary>
	/// Holds global information about the playing state
	/// </summary>
	internal class GlobalPlayingInfo : IDeepCloneable<GlobalPlayingInfo>
	{
		public byte Tempo { get; set; }					// Tempo 32-255
		public byte Speed { get; set; }					// 0 = Off, 1-255
		public byte FrameCnt { get; set; }				// 0-speed

		public bool PatternBreak { get; set; }
		public byte PatCnt { get; set; }
		public sbyte PatDelayCnt { get; set; }

		public bool LoopFlag { get; set; }
		public byte LoopPoint { get; set; }
		public byte LoopCnt { get; set; }

		public byte Position { get; set; }				// Actual position while replaying
		public byte NewPosition { get; set; }
		public byte CurrentPattern { get; set; }

		public bool NoiseAvoid { get; set; }
		public bool Oversize { get; set; }				// Play samples > 128 kb

		public byte[] Event { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public GlobalPlayingInfo MakeDeepClone()
		{
			GlobalPlayingInfo clone = (GlobalPlayingInfo)MemberwiseClone();

			clone.Event = ArrayHelper.CloneArray(Event);

			return clone;
		}
	}
}
