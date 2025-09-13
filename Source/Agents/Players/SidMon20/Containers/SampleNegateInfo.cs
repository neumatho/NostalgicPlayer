/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SidMon20.Containers
{
	/// <summary>
	/// Holds negate variables for a single sample
	/// </summary>
	internal class SampleNegateInfo : IDeepCloneable<SampleNegateInfo>
	{
		public uint StartOffset { get; set; }
		public uint EndOffset { get; set; }
		public ushort LoopIndex { get; set; }
		public ushort Status { get; set; }
		public short Speed { get; set; }
		public int Position { get; set; }
		public ushort Index { get; set; }
		public short DoNegation { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public SampleNegateInfo MakeDeepClone()
		{
			return (SampleNegateInfo)MemberwiseClone();
		}
	}
}
