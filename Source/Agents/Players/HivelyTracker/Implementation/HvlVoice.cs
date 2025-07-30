/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Agent.Player.HivelyTracker.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.HivelyTracker.Implementation
{
	/// <summary>
	/// Handler of a single voice
	/// </summary>
	internal class HvlVoice : IDeepCloneable<HvlVoice>
	{
		// Read those variables for mixing
		public int VoiceVolume { get; set; }
		public int VoicePeriod { get; set; }
		public sbyte[] VoiceBuffer { get; set; } = new sbyte[0x281];

		public int Track { get; set; }
		public int Transpose { get; set; }
		public int NextTrack { get; set; }
		public int NextTranspose { get; set; }
		public int OverrideTranspose { get; set; }

		public int AdsrVolume { get; set; }							// Fixed point 8:8
		public HvlEnvelope Adsr { get; set; } = new HvlEnvelope();	// Frames / delta fixed 8:8

		public HvlInstrument Instrument { get; set; }				// Current instrument
		public int InstrumentNumber { get; set; }
		public int SamplePos { get; set; }
		public int Delta { get; set; }

		public int InstrPeriod { get; set; }
		public int TrackPeriod { get; set; }
		public int VibratoPeriod { get; set; }

		public int NoteMaxVolume { get; set; }
		public int PerfSubVolume { get; set; }
		public int TrackMasterVolume { get; set; }

		public bool NewWaveform { get; set; }
		public int Waveform { get; set; }
		public bool PlantSquare { get; set; }
		public bool PlantPeriod { get; set; }
		public bool KickNote { get; set; }
		public bool IgnoreSquare { get; set; }

		public bool TrackOn { get; set; }
		public bool FixedNote { get; set; }

		public int VolumeSlideUp { get; set; }
		public int VolumeSlideDown { get; set; }

		public int HardCut { get; set; }
		public bool HardCutRelease { get; set; }
		public int HardCutReleaseF { get; set; }

		public int PeriodSlideSpeed { get; set; }
		public int PeriodSlidePeriod { get; set; }
		public int PeriodSlideLimit { get; set; }
		public bool PeriodSlideOn { get; set; }
		public bool PeriodSlideWithLimit { get; set; }

		public int PeriodPerfSlideSpeed { get; set; }
		public int PeriodPerfSlidePeriod { get; set; }
		public bool PeriodPerfSlideOn { get; set; }

		public int VibratoDelay { get; set; }
		public int VibratoCurrent { get; set; }
		public int VibratoDepth { get; set; }
		public int VibratoSpeed { get; set; }

		public bool SquareOn { get; set; }
		public bool SquareInit { get; set; }
		public int SquareWait { get; set; }
		public int SquareLowerLimit { get; set; }
		public int SquareUpperLimit { get; set; }
		public int SquarePos { get; set; }
		public int SquareSign { get; set; }
		public bool SquareSlidingIn { get; set; }
		public bool SquareReverse { get; set; }

		public bool FilterOn { get; set; }
		public bool FilterInit { get; set; }
		public int FilterWait { get; set; }
		public int FilterLowerLimit { get; set; }
		public int FilterUpperLimit { get; set; }
		public int FilterPos { get; set; }
		public int FilterSign { get; set; }
		public int FilterSpeed { get; set; }
		public bool FilterSlidingIn { get; set; }
		public int IgnoreFilter { get; set; }

		public int PerfCurrent { get; set; }
		public int PerfSpeed { get; set; }
		public int PerfWait { get; set; }

		public int WaveLength { get; set; }

		public HvlPList PerfList { get; set; }

		public int NoteDelayWait { get; set; }
		public bool NoteDelayOn { get; set; }
		public int NoteCutWait { get; set; }
		public bool NoteCutOn { get; set; }

		public int Pan { get; set; }
		public int SetPan { get; set; }

		public int RingSamplePos { get; set; }
		public int RingDelta { get; set; }
		public sbyte[] RingMixSource { get; set; }
		public bool RingPlantPeriod { get; set; }
		public int RingBasePeriod { get; set; }
		public int RingAudioPeriod { get; set; }
		public bool RingNewWaveform { get; set; }
		public int RingWaveform { get; set; }
		public bool RingFixedPeriod { get; set; }

		public sbyte[] AudioSource { get; set; }
		public int AudioOffset { get; set; }

		public sbyte[] RingAudioSource { get; set; }
		public int RingAudioOffset { get; set; }

		public int AudioPeriod { get; set; }
		public int AudioVolume { get; set; }

		public int WnRandom { get; set; }
		public sbyte[] MixSource { get; set; }

		public sbyte[] SquareTempBuffer { get; set; } = new sbyte[0x80];
		public sbyte[] RingVoiceBuffer { get; set; } = new sbyte[0x282 * 4];

		/********************************************************************/
		/// <summary>
		/// Initialize the voice variables
		/// </summary>
		/********************************************************************/
		public void Init(int voiceNum, HvlSong song, int[] panningLeft, int[] panningRight)
		{
			VoiceVolume = 0;
			VoicePeriod = 0;

			Track = 0;
			Transpose = 0;
			NextTrack = 0;
			NextTranspose = 0;
			OverrideTranspose = 1000;

			AdsrVolume = 0;
			Adsr.AFrames = 0;
			Adsr.AVolume = 0;
			Adsr.DFrames = 0;
			Adsr.DVolume = 0;
			Adsr.SFrames = 0;
			Adsr.RFrames = 0;
			Adsr.RVolume = 0;

			Instrument = null;
			InstrumentNumber = 0;
			SamplePos = 0;
			Delta = 1;

			InstrPeriod = 0;
			TrackPeriod = 0;
			VibratoPeriod = 0;

			NoteMaxVolume = 0;
			PerfSubVolume = 0;
			TrackMasterVolume = 0x40;

			NewWaveform = false;
			Waveform = 0;
			PlantSquare	= false;
			PlantPeriod = false;
			KickNote = false;
			IgnoreSquare = false;

			TrackOn = true;
			FixedNote = false;

			VolumeSlideUp = 0;
			VolumeSlideDown = 0;

			HardCut = 0;
			HardCutRelease = false;
			HardCutReleaseF = 0;

			PeriodSlideSpeed = 0;
			PeriodSlidePeriod = 0;
			PeriodSlideLimit = 0;
			PeriodSlideOn = false;
			PeriodSlideWithLimit = false;

			PeriodPerfSlideSpeed = 0;
			PeriodPerfSlidePeriod = 0;
			PeriodPerfSlideOn = false;

			VibratoDelay = 0;
			VibratoCurrent = 0;
			VibratoDepth = 0;
			VibratoSpeed = 0;

			SquareOn = false;
			SquareInit = false;
			SquareWait = 0;
			SquareLowerLimit = 0;
			SquareUpperLimit = 0;
			SquarePos = 0;
			SquareSign = 0;
			SquareSlidingIn = false;
			SquareReverse = false;

			FilterOn = false;
			FilterInit = false;
			FilterWait = 0;
			FilterLowerLimit = 0;
			FilterUpperLimit = 0;
			FilterPos = 32;
			FilterSign = 0;
			FilterSpeed = 0;
			FilterSlidingIn = false;
			IgnoreFilter = 0;

			PerfCurrent = 0;
			PerfSpeed = 0;
			PerfWait = 0;

			WaveLength = 0;

			PerfList = null;

			NoteDelayWait = 0;
			NoteDelayOn = false;
			NoteCutWait = 0;
			NoteCutOn = false;

			if (((voiceNum % 4) == 0) || ((voiceNum % 4) == 3))
			{
				Pan = song.DefaultPanningLeft;
				SetPan = song.DefaultPanningLeft;
			}
			else
			{
				Pan = song.DefaultPanningRight;
				SetPan = song.DefaultPanningRight;
			}

			RingSamplePos = 0;
			RingDelta = 0;
			RingMixSource = null;
			RingPlantPeriod = false;
			RingBasePeriod = 0;
			RingAudioPeriod = 0;
			RingNewWaveform = false;
			RingWaveform = 0;
			RingFixedPeriod = false;

			AudioSource = null;
			AudioOffset = 0;

			RingAudioSource = null;

			AudioPeriod = 0;
			AudioVolume = 0;

			WnRandom = 0x280;
			MixSource = VoiceBuffer;

			Array.Clear(VoiceBuffer);
			Array.Clear(SquareTempBuffer);
			Array.Clear(RingVoiceBuffer);
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the ADSR envelope
		/// </summary>
		/********************************************************************/
		public void CalcAdsr()
		{
			HvlEnvelope source = Instrument.Envelope;
			HvlEnvelope dest = Adsr;

			dest.AFrames = source.AFrames;
			dest.AVolume = dest.AFrames != 0 ? source.AVolume * 256 / dest.AFrames : source.AVolume * 256;
			dest.DFrames = source.DFrames;
			dest.DVolume = dest.DFrames != 0 ? (source.DVolume - source.AVolume) * 256 / dest.DFrames : source.DVolume * 256;
			dest.SFrames = source.SFrames;
			dest.RFrames = source.RFrames;
			dest.RVolume = dest.RFrames != 0 ? (source.RVolume - source.DVolume) * 256 / dest.RFrames : source.RVolume * 256;
		}



		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public HvlVoice MakeDeepClone()
		{
			HvlVoice clone = (HvlVoice)MemberwiseClone();

			clone.VoiceBuffer = ArrayHelper.CloneArray(VoiceBuffer);
			clone.Adsr = Adsr.MakeDeepClone();
			clone.SquareTempBuffer = ArrayHelper.CloneArray(SquareTempBuffer);
			clone.RingVoiceBuffer = ArrayHelper.CloneArray(RingVoiceBuffer);

			return clone;
		}
	}
}
