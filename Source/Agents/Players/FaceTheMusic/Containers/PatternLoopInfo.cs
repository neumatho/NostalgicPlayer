/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.FaceTheMusic.Containers
{
	/// <summary>
	/// Holds information about a pattern loop
	/// </summary>
	internal class PatternLoopInfo : IDeepCloneable<PatternLoopInfo>
	{
		public uint TrackPosition { get; set; }
		public ushort LoopStartPosition { get; set; }
		public short LoopCount { get; set; }
		public short OriginalLoopCount { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public PatternLoopInfo MakeDeepClone()
		{
			return (PatternLoopInfo)MemberwiseClone();
		}
	}
}
