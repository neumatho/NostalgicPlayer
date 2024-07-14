/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.ArtOfNoise.Containers
{
	/// <summary>
	/// Holds playing information for a single voice
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		public byte ChFlag;					// <> 0 = New wave (1 = Sample, 2 = Synth)
		public byte LastNote;

		public sbyte[] WaveForm;
		public uint WaveFormOffset;
		public ushort WaveLen;				// Wave length / 2
		public ushort OldWaveLen;			// To avoid noise when using repeat < 512 bytes
		public sbyte[] RepeatStart;			// Wave form to repeat
		public uint RepeatOffset;
		public ushort RepeatLength;			// Repeat length / 2
		public Instrument Instrument;
		public short InstrumentNumber;
		public byte Volume;

		public byte StepFxCnt;				// Note cut / Delay / Retrig

		public byte ChMode;					// 0 = Sample, 1 = Synth

		public ushort Period;				// Actual period
		public short PerSlide;				// Added to period (e.g. for portamento)

		public ushort ArpeggioOff;
		public byte ArpeggioFineTune;
		public short[] ArpeggioTab;			// 7 notes + end mark
		public byte ArpeggioSpd;			// Frame-change-speed
		public byte ArpeggioCnt;			// Countdown

		public sbyte[] SynthWaveAct;
		public uint SynthWaveActOffset;
		public uint SynthWaveEndOffset;
		public sbyte[] SynthWaveRep;
		public uint SynthWaveRepOffset;
		public uint SynthWaveRepEndOffset;
		public int SynthWaveAddBytes;
		public byte SynthWaveCnt;			// Frame counter
		public byte SynthWaveSpd;			// Change the waveform every nth frame
		public byte SynthWaveRepCtrl;		// 0 = Normal, 1 = Back, 2 = Ping-pong
		public byte SynthWaveCont;			// 0 = Normal, 1 = Let wave run through
		public byte SynthWaveStop;			// 1 = Don't continue wave until U10
		public byte SynthAdd;
		public byte SynthSub;
		public byte SynthEnd;
		public EnvelopeState SynthEnv;
		public byte SynthVol;				// Actual ADSR byte (*volume/64 = abs volume)

		public byte VibOn;					// 1 = Do vibrato
		public bool VibDone;
		public byte VibCont;				// 1 = Duration vibrato (wave table), 0 = Only '4' etc
		public byte VibratoSpd;
		public byte VibratoAmpl;
		public byte VibratoPos;
		public short VibratoTrigDelay;		// -1 = Already triggered

		public Effect FxCom;
		public byte FxDat;

		public bool SlideFlag;

		public uint OldSampleOffset;		// Used for '9' effect
		public byte GlissSpd;				// Speed for '3' effect

		public byte TrackVolume;			// 64 = Max, 0 = Track mute

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
