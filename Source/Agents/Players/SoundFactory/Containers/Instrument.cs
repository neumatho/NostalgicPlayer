/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SoundFactory.Containers
{
	/// <summary>
	/// Holds information about a single instrument
	/// </summary>
	internal class Instrument : IDeepCloneable<Instrument>
	{
		public short InstrumentNumber { get; set; }

		public ushort SampleLength { get; set; }
		public ushort SamplingPeriod { get; set; }

		public InstrumentFlag EffectByte { get; set; }

		public byte TremoloSpeed { get; set; }
		public byte TremoloStep { get; set; }
		public byte TremoloRange { get; set; }

		public ushort PortamentoStep { get; set; }
		public byte PortamentoSpeed { get; set; }

		public byte ArpeggioSpeed { get; set; }

		public byte VibratoDelay { get; set; }
		public byte VibratoSpeed { get; set; }
		public sbyte VibratoStep { get; set; }
		public byte VibratoAmount { get; set; }

		public byte AttackTime { get; set; }
		public byte DecayTime { get; set; }
		public byte SustainLevel { get; set; }
		public byte ReleaseTime { get; set; }

		public byte PhasingStart { get; set; }
		public byte PhasingEnd { get; set; }
		public byte PhasingSpeed { get; set; }
		public sbyte PhasingStep { get; set; }

		public byte WaveCount { get; set; }
		public byte Octave { get; set; }

		public byte FilterFrequency { get; set; }
		public byte FilterEnd { get; set; }
		public byte FilterSpeed { get; set; }

		/// DASR - Digital Attack, Sustain, Release
		public ushort DASR_SustainOffset { get; set; }
		public ushort DASR_ReleaseOffset { get; set; }

		public sbyte[] SampleData { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public Instrument MakeDeepClone()
		{
			return (Instrument)MemberwiseClone();
		}
	}
}
