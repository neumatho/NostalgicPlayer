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
		public ushort OldSongPos { get; set; }
		public ushort SongPos { get; set; }
		public ushort PatternPos { get; set; }
		public ushort BreakPos { get; set; }
		public bool PosJumpFlag { get; set; }
		public bool BreakFlag { get; set; }
		public bool GotBreak { get; set; }
		public bool GotPositionJump { get; set; }
		public byte Tempo { get; set; }
		public byte SpeedEven { get; set; }
		public byte SpeedOdd { get; set; }
		public byte LastShownSpeed { get; set; }
		public byte Counter { get; set; }
		public byte LowMask { get; set; }
		public byte PattDelayTime { get; set; }
		public byte PattDelayTime2 { get; set; }
		public short LastUsedPositionJumpArgument { get; set; }
		public short LastUsedBreakPositionArgument { get; set; }

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
