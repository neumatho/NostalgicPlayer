/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.Synthesis.Containers
{
	/// <summary>
	/// Holds playing information for a single voice
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		// Position information
		public ushort StartTrackRow { get; set; }
		public sbyte SoundTranspose { get; set; }
		public sbyte NoteTranspose { get; set; }

		// Track row information
		public byte Note { get; set; }
		public byte Instrument { get; set; }
		public byte Arpeggio { get; set; }
		public Effect Effect { get; set; }
		public byte EffectArg { get; set; }

		public byte UseBuffer { get; set; }
		public sbyte[] SynthSample1 { get; set; } = new sbyte[256];
		public sbyte[] SynthSample2 { get; set; } = new sbyte[256];

		public byte TransposedNote { get; set; }
		public byte PreviousTransposedNote { get; set; }

		public byte TransposedInstrument { get; set; }

		public byte CurrentVolume { get; set; }
		public byte NewVolume { get; set; }

		public byte ArpeggioPosition { get; set; }

		public sbyte SlideSpeed { get; set; }
		public short SlideIncrement { get; set; }

		public sbyte PortamentoSpeed { get; set; }
		public short PortamentoSpeedCounter { get; set; }

		public byte VibratoDelay { get; set; }
		public byte VibratoPosition { get; set; }

		public bool AdsrEnabled { get; set; }
		public ushort AdsrPosition { get; set; }

		public bool EnvelopeGeneratorCounterDisabled { get; set; }
		public EnvelopeGeneratorCounterMode EnvelopeGeneratorCounterMode { get; set; }
		public ushort EnvelopeGeneratorCounterPosition { get; set; }

		public bool SynthEffectDisabled { get; set; }
		public SynthesisEffect SynthEffect { get; set; }
		public byte SynthEffectArg1 { get; set; }
		public byte SynthEffectArg2 { get; set; }
		public byte SynthEffectArg3 { get; set; }

		public byte SynthPosition { get; set; }
		public byte SlowMotionCounter { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public VoiceInfo MakeDeepClone()
		{
			VoiceInfo clone = (VoiceInfo)MemberwiseClone();

			clone.SynthSample1 = ArrayHelper.CloneArray(SynthSample1);
			clone.SynthSample2 = ArrayHelper.CloneArray(SynthSample2);

			return clone;
		}
	}
}
