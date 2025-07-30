/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.QuadraComposer.Containers
{
	/// <summary>
	/// Holds global information about the playing state
	/// </summary>
	internal class GlobalPlayingInfo : IDeepCloneable<GlobalPlayingInfo>
	{
		public TrackLine[,] CurrentPattern { get; set; }
		public ushort CurrentPosition { get; set; }
		public ushort NewPosition { get; set; }
		public ushort BreakRow { get; set; }
		public ushort NewRow { get; set; }
		public ushort RowCount { get; set; }
		public ushort LoopRow { get; set; }
		public byte PatternWait { get; set; }

		public ushort Tempo { get; set; }
		public ushort Speed { get; set; }
		public ushort SpeedCount { get; set; }

		public bool NewPositionFlag { get; set; }
		public bool JumpBreakFlag { get; set; }
		public byte LoopCount { get; set; }
		public bool IntroRow { get; set; }

		public bool SetTempo { get; set; }

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
