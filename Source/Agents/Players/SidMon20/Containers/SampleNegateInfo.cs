/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SidMon20.Containers
{
	/// <summary>
	/// Holds negate variables for a single sample
	/// </summary>
	internal class SampleNegateInfo : IDeepCloneable<SampleNegateInfo>
	{
		public uint StartOffset;
		public uint EndOffset;
		public ushort LoopIndex;
		public ushort Status;
		public short Speed;
		public int Position;
		public ushort Index;
		public short DoNegation;

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
