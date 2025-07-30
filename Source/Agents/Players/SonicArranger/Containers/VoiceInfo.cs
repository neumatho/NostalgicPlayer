/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.SonicArranger.Containers
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
		public bool DisableSoundTranspose { get; set; }
		public bool DisableNoteTranspose { get; set; }
		public byte Arpeggio { get; set; }
		public Effect Effect { get; set; }
		public byte EffectArg { get; set; }

		public ushort TransposedNote { get; set; }
		public ushort PreviousTransposedNote { get; set; }

		public InstrumentType InstrumentType { get; set; }
		public Instrument InstrumentInfo { get; set; }
		public ushort TransposedInstrument { get; set; }

		public ushort CurrentVolume { get; set; }
		public short VolumeSlideSpeed { get; set; }

		public ushort VibratoPosition { get; set; }
		public ushort VibratoDelay { get; set; }
		public ushort VibratoSpeed { get; set; }
		public ushort VibratoLevel { get; set; }

		public ushort PortamentoSpeed { get; set; }
		public ushort PortamentoPeriod { get; set; }

		public ushort ArpeggioPosition { get; set; }

		public short SlideSpeed { get; set; }
		public short SlideValue { get; set; }

		public ushort AdsrPosition { get; set; }
		public ushort AdsrDelayCounter { get; set; }
		public ushort SustainDelayCounter { get; set; }

		public ushort AmfPosition { get; set; }
		public ushort AmfDelayCounter { get; set; }

		public ushort SynthEffectPosition { get; set; }
		public ushort SynthEffectWavePosition { get; set; }
		public ushort EffectDelayCounter { get; set; }

		public byte Flag { get; set; }

		public sbyte[] WaveformBuffer { get; set; } = new sbyte[128];

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public VoiceInfo MakeDeepClone()
		{
			VoiceInfo clone = (VoiceInfo)MemberwiseClone();

			clone.WaveformBuffer = ArrayHelper.CloneArray(WaveformBuffer);

			return clone;
		}
	}
}
