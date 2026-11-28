/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.ManiacsOfNoise.Containers
{
	/// <summary>
	/// Holds playing information for a single voice
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		public int ChannelNumber { get; set; }

		public bool Playing { get; set; }

		public ushort[] PositionList { get; set; }
		public int PositionListIndex { get; set; }

		public byte TrackRepeatCount { get; set; }
		public int TrackNumber { get; set; }
		public byte[] Track { get; set; }
		public int TrackIndex { get; set; }

		public bool FirstTickFlag { get; set; }
		public ushort NoteTickCounter { get; set; }

		public Sample CurrentSample { get; set; }
		public short SampleNumber { get; set; }
		public bool EffectFlag { get; set; }

		public ushort Volume { get; set; }
		public ushort FinalVolume { get; set; }

		public sbyte Transpose { get; set; }
		public byte CurrentNote { get; set; }
		public ushort NotePeriod { get; set; }
		public ushort FinalPeriod { get; set; }
		public byte NoteDelay { get; set; }						// v3+

		public ushort PortamentoTickCounter { get; set; }		// Only v1
		public ushort PortamentoDuration { get; set; }			// v2+
		public ushort PortamentoStartPeriod { get; set; }
		public ushort PortamentoTick { get; set; }				// v2+
		public ushort PortamentoCurrentPeriod { get; set; }
		public short PortamentoStep { get; set; }				// Only v1
		public short PortamentoStepLeftOver { get; set; }		// Only v1
		public short PortamentoDifference { get; set; }			// v2+

		public byte EnvelopeSpeedCounter { get; set; }
		public byte EnvelopeIndex { get; set; }
		public bool EnvelopeFlag { get; set;}					// v3+

		public ushort ArpeggioOffset { get; set; }				// v3+
		public byte ArpeggioSpeedCounter { get; set; }
		public byte ArpeggioIndex { get; set; }
		public short ArpeggioDifference { get; set; }

		public byte VibratoDelay { get; set; }
		public byte VibratoSpeedCounter { get; set; }
		public byte VibratoIndex { get; set; }
		public short VibratoValue { get; set; }

		public byte WaveformSpeedCounter { get; set; }			// Only v1/v2
		public ushort WaveformIndex { get; set; }				// Only v1/v2
		public ushort WaveformLength { get; set; }				// Only v1/v2

		public sbyte[] SampleSequence { get; set; }				// Only v5
		public sbyte[] SampleSequenceToPlay { get; set; }		// Only v5
		public ushort SampleSequenceLength { get; set; }		// Only v5
		public byte SampleSequenceSpeedCounter { get; set; }	// Only v5
		public byte SampleSequenceSpeed { get; set; }			// Only v5
		public byte SampleSequenceIndex { get; set; }			// Only v5

		public byte VoiceState { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public VoiceInfo MakeDeepClone()
		{
			return (VoiceInfo)MemberwiseClone();
		}
	}
}
