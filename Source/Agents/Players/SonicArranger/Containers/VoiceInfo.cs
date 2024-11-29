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
		public ushort StartTrackRow;
		public sbyte SoundTranspose;
		public sbyte NoteTranspose;

		// Track row information
		public byte Note;
		public byte Instrument;
		public bool DisableSoundTranspose;
		public bool DisableNoteTranspose;
		public byte Arpeggio;
		public Effect Effect;
		public byte EffectArg;

		public ushort TransposedNote;
		public ushort PreviousTransposedNote;

		public InstrumentType InstrumentType;
		public Instrument InstrumentInfo;
		public ushort TransposedInstrument;

		public ushort CurrentVolume;
		public short VolumeSlideSpeed;

		public ushort VibratoPosition;
		public ushort VibratoDelay;
		public ushort VibratoSpeed;
		public ushort VibratoLevel;

		public ushort PortamentoSpeed;
		public ushort PortamentoPeriod;

		public ushort ArpeggioPosition;

		public short SlideSpeed;
		public short SlideValue;

		public ushort AdsrPosition;
		public ushort AdsrDelayCounter;
		public ushort SustainDelayCounter;

		public ushort AmfPosition;
		public ushort AmfDelayCounter;

		public ushort SynthEffectPosition;
		public ushort SynthEffectWavePosition;
		public ushort EffectDelayCounter;

		public byte Flag;

		public sbyte[] WaveformBuffer = new sbyte[128];

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
