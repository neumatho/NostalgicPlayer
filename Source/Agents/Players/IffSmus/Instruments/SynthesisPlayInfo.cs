/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.IffSmus.Instruments
{
	/// <summary>
	/// Contains play information for a single voice when using synthesis instrument
	/// </summary>
	internal class SynthesisPlayInfo : IDeepCloneable<SynthesisPlayInfo>
	{
		public ushort MappedNote;

		public short FrequencyCounter;
		public short FrequencySpeed;

		public ushort SampleStartIndex;
		public ushort PlayingOctave;

		public ushort EnvelopeIndex;
		public int EnvelopeVolume;

		public ushort LfoIndex;
		public short LfoCounter;
		public short LfoValue;

		public short PhaseIndex;
		public short PhaseDirection;

		public byte Octave;
		public byte Note;

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
