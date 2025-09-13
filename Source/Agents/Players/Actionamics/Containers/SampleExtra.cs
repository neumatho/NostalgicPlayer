/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.Actionamics.Containers
{
	/// <summary>
	/// This contains sample information which are stored in the original
	/// sample structure, but modified by the player. I have split this
	/// into two structures
	/// </summary>
	internal class SampleExtra : IDeepCloneable<SampleExtra>
	{
		public sbyte[] ModifiedSampleData { get; set; }

		public short EffectIncrementValue { get; set; }
		public int EffectPosition { get; set; }
		public ushort EffectSpeedCounter { get; set; }
		public bool AlreadyTaken { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public SampleExtra MakeDeepClone()
		{
			SampleExtra clone = (SampleExtra)MemberwiseClone();

			if (ModifiedSampleData != null)
				clone.ModifiedSampleData = ArrayHelper.CloneArray(ModifiedSampleData);

			return clone;
		}
	}
}
