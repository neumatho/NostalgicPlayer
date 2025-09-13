/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.IffSmus.Instruments
{
	/// <summary>
	/// Contains play information for a single voice when using synthesis instrument
	/// </summary>
	internal class SynthesisPlayInfo : IDeepCloneable<SynthesisPlayInfo>
	{
		public ushort MappedNote { get; set; }

		public short FrequencyCounter { get; set; }
		public short FrequencySpeed { get; set; }

		public ushort SampleStartIndex { get; set; }
		public ushort PlayingOctave { get; set; }

		public ushort EnvelopeIndex { get; set; }
		public int EnvelopeVolume { get; set; }

		public ushort LfoIndex { get; set; }
		public short LfoCounter { get; set; }
		public short LfoValue { get; set; }

		public short PhaseIndex { get; set; }
		public short PhaseDirection { get; set; }

		public byte Octave { get; set; }
		public byte Note { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public SynthesisPlayInfo MakeDeepClone()
		{
			return (SynthesisPlayInfo)MemberwiseClone();
		}
	}
}
