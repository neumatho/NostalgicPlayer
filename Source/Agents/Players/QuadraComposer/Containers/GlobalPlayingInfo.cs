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
		public TrackLine[,] CurrentPattern;
		public ushort CurrentPosition;
		public ushort NewPosition;
		public ushort BreakRow;
		public ushort NewRow;
		public ushort RowCount;
		public ushort LoopRow;
		public byte PatternWait;

		public ushort Tempo;
		public ushort Speed;
		public ushort SpeedCount;

		public bool NewPositionFlag;
		public bool JumpBreakFlag;
		public byte LoopCount;
		public bool IntroRow;

		public bool SetTempo;

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
