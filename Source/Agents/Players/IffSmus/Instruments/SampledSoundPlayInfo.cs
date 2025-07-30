/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.IffSmus.Instruments
{
	/// <summary>
	/// Contains play information for a single voice when using Sampled Sound instrument
	/// </summary>
	internal class SampledSoundPlayInfo : IDeepCloneable<SampledSoundPlayInfo>
	{
		public sbyte[] LoopSampleData { get; set; }
		public uint LoopStart { get; set; }
		public ushort LoopLengthInWords { get; set; }

		public ushort MappedNote { get; set; }

		public ushort EnvelopeIndex { get; set; }
		public int EnvelopeVolume { get; set; }

		public ushort VibratoIndex { get; set; }
		public ushort VibratoDelayCounter { get; set; }
		public short VibratoValue { get; set; }

		public byte Octave { get; set; }
		public byte Note { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public SampledSoundPlayInfo MakeDeepClone()
		{
			return (SampledSoundPlayInfo)MemberwiseClone();
		}
	}
}
