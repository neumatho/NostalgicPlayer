/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.HivelyTracker.Containers
{
	/// <summary>
	/// Holds global information about the playing state
	/// </summary>
	internal class GlobalPlayingInfo : IDeepCloneable<GlobalPlayingInfo>
	{
		public int StepWaitFrames { get; set; }
		public bool GetNewPosition { get; set; }

		public bool PatternBreak { get; set; }

		public int Tempo { get; set; }

		public int PosNr { get; set; }
		public int PosJump { get; set; }

		public int NoteNr { get; set; }
		public int PosJumpNote { get; set; }

		public sbyte[] SquareWaveform { get; set; }

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
