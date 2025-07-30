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
		public ushort StartRow { get; set; }
		public ushort EndRow { get; set; }

		public byte GlobalVolume { get; set; }

		public ushort Speed { get; set; }

		public Stack<PatternLoopInfo> PatternLoopStack { get; set; } = new Stack<PatternLoopInfo>();
		public ushort PatternLoopStopRow { get; set; }
		public ushort PatternLoopStartRow { get; set; }
		public bool DoPatternLoop { get; set; }

		public ushort SpeedCounter { get; set; }
		public ushort CurrentRow { get; set; }

		public short[] DetuneValues { get; set; } = new short[4];

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
