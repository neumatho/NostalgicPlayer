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
		public byte ChFlag { get; set; }				// <> 0 = New wave (1 = Sample, 2 = Synth)
		public byte LastNote { get; set; }

		public sbyte[] WaveForm { get; set; }
		public uint WaveFormOffset { get; set; }
		public ushort WaveLen { get; set; }				// Wave length / 2
		public ushort OldWaveLen { get; set; }			// To avoid noise when using repeat < 512 bytes
		public sbyte[] RepeatStart { get; set; }		// Wave form to repeat
		public uint RepeatOffset { get; set; }
		public ushort RepeatLength { get; set; }		// Repeat length / 2
		public Instrument Instrument { get; set; }
		public short InstrumentNumber { get; set; }
		public byte Volume { get; set; }

		public byte StepFxCnt { get; set; }				// Note cut / Delay / Retrig

		public byte ChMode { get; set; }				// 0 = Sample, 1 = Synth

		public ushort Period { get; set; }				// Actual period
		public short PerSlide { get; set; }				// Added to period (e.g. for portamento)

		public ushort ArpeggioOff { get; set; }
		public byte ArpeggioFineTune { get; set; }
		public short[] ArpeggioTab { get; set; }		// 7 notes + end mark
		public byte ArpeggioSpd { get; set; }			// Frame-change-speed
		public byte ArpeggioCnt { get; set; }			// Countdown

		public sbyte[] SynthWaveAct { get; set; }
		public uint SynthWaveActOffset { get; set; }
		public uint SynthWaveEndOffset { get; set; }
		public sbyte[] SynthWaveRep { get; set; }
		public uint SynthWaveRepOffset { get; set; }
		public uint SynthWaveRepEndOffset { get; set; }
		public int SynthWaveAddBytes { get; set; }
		public byte SynthWaveCnt { get; set; }			// Frame counter
		public byte SynthWaveSpd { get; set; }			// Change the waveform every nth frame
		public byte SynthWaveRepCtrl { get; set; }		// 0 = Normal, 1 = Back, 2 = Ping-pong
		public byte SynthWaveCont { get; set; }			// 0 = Normal, 1 = Let wave run through
		public byte SynthWaveStop { get; set; }			// 1 = Don't continue wave until U10
		public byte SynthAdd { get; set; }
		public byte SynthSub { get; set; }
		public byte SynthEnd { get; set; }
		public EnvelopeState SynthEnv { get; set; }
		public byte SynthVol { get; set; }				// Actual ADSR byte (*volume/64 = abs volume)

		public byte VibOn { get; set; }					// 1 = Do vibrato
		public bool VibDone { get; set; }
		public byte VibCont { get; set; }				// 1 = Duration vibrato (wave table), 0 = Only '4' etc
		public byte VibratoSpd { get; set; }
		public byte VibratoAmpl { get; set; }
		public byte VibratoPos { get; set; }
		public short VibratoTrigDelay { get; set; }		// -1 = Already triggered

		public Effect FxCom { get; set; }
		public byte FxDat { get; set; }

		public bool SlideFlag { get; set; }

		public uint OldSampleOffset { get; set; }		// Used for '9' effect
		public byte GlissSpd { get; set; }				// Speed for '3' effect

		public byte TrackVolume { get; set; }			// 64 = Max, 0 = Track mute

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
