/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.ModTracker.Containers
{
	/// <summary>
	/// Holds global information about the playing state
	/// </summary>
	internal class GlobalPlayingInfo : IDeepCloneable<GlobalPlayingInfo>
	{
		public ushort OldSongPos;
		public ushort SongPos;
		public ushort PatternPos;
		public ushort BreakPos;
		public bool PosJumpFlag;
		public bool BreakFlag;
		public bool GotBreak;
		public bool GotPositionJump;
		public byte Tempo;
		public byte Speed;
		public byte Counter;
		public byte LowMask;
		public byte PattDelayTime;
		public byte PattDelayTime2;
		public short LastUsedPositionJumpArgument;
		public short LastUsedBreakPositionArgument;

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
