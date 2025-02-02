/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.Linq;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.FaceTheMusic.Containers
{
	/// <summary>
	/// Holds global information about the playing state
	/// </summary>
	internal class GlobalPlayingInfo : IDeepCloneable<GlobalPlayingInfo>
	{
		public ushort StartRow;
		public ushort EndRow;

		public byte GlobalVolume;

		public ushort Speed;

		public Stack<PatternLoopInfo> PatternLoopStack = new Stack<PatternLoopInfo>();
		public ushort PatternLoopStopRow;
		public ushort PatternLoopStartRow;
		public bool DoPatternLoop;

		public ushort SpeedCounter;
		public ushort CurrentRow;

		public short[] DetuneValues = new short[4];

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public GlobalPlayingInfo MakeDeepClone()
		{
			GlobalPlayingInfo clone = (GlobalPlayingInfo)MemberwiseClone();

			clone.DetuneValues = ArrayHelper.CloneArray(DetuneValues);

			// Make sure the cloned stack has the elements in same order as the original stack
			// and each element is a clone
			PatternLoopStack = new Stack<PatternLoopInfo>(PatternLoopStack.Reverse().Select(x => x.MakeDeepClone()));

			return clone;
		}
	}
}
