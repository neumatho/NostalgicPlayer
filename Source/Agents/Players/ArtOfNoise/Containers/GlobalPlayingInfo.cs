/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.ArtOfNoise.Containers
{
	/// <summary>
	/// Holds global information about the playing state
	/// </summary>
	internal class GlobalPlayingInfo : IDeepCloneable<GlobalPlayingInfo>
	{
		public byte Tempo;					// Tempo 32-255
		public byte Speed;					// 0 = Off, 1-255
		public byte FrameCnt;				// 0-speed

		public bool PatternBreak;
		public byte PatCnt;
		public sbyte PatDelayCnt;

		public bool LoopFlag;
		public byte LoopPoint;
		public byte LoopCnt;

		public byte Position;				// Actual position while replaying
		public byte NewPosition;
		public byte CurrentPattern;

		public bool NoiseAvoid;
		public bool Oversize;				// Play samples > 128 kb

		public byte[] Event;

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
