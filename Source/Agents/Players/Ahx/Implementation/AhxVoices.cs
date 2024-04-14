/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.Ahx.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.Ahx.Implementation
{
	/// <summary>
	/// Handler of a single voice
	/// </summary>
	internal class AhxVoices : IDeepCloneable<AhxVoices>
	{
		// Read those variables for mixing
		public int VoiceVolume;
		public int VoicePeriod;
		public sbyte[] VoiceBuffer = new sbyte[0x281];

		public int Track;
		public int Transpose;
		public int NextTrack;
		public int NextTranspose;

		public int AdsrVolume;							// Fixed point 8:8
		public AhxEnvelope Adsr = new AhxEnvelope();	// Frames / delta fixed 8:8

		public AhxInstrument Instrument;				// Current instrument
		public int InstrumentNumber;

		public int InstrPeriod;
		public int TrackPeriod;
		public int VibratoPeriod;

		public int NoteMaxVolume;
		public int PerfSubVolume;
		public int TrackMasterVolume;

		public bool NewWaveform;
		public int Waveform;
		public bool PlantSquare;
		public bool PlantPeriod;
		public bool KickNote;
		public bool IgnoreSquare;
		public bool WaveformStarted;

		public bool TrackOn;
		public bool FixedNote;

		public int VolumeSlideUp;
		public int VolumeSlideDown;

		public int HardCut;
		public bool HardCutRelease;
		public int HardCutReleaseF;

		public int PeriodSlideSpeed;
		public int PeriodSlidePeriod;
		public int PeriodSlideLimit;
		public bool PeriodSlideOn;
		public bool PeriodSlideWithLimit;

		public int PeriodPerfSlideSpeed;
		public int PeriodPerfSlidePeriod;
		public bool PeriodPerfSlideOn;

		public int VibratoDelay;
		public int VibratoCurrent;
		public int VibratoDepth;
		public int VibratoSpeed;

		public bool SquareOn;
		public bool SquareInit;
		public int SquareWait;
		public int SquareLowerLimit;
		public int SquareUpperLimit;
		public int SquarePos;
		public int SquareSign;
		public bool SquareSlidingIn;
		public bool SquareReverse;

		public bool FilterOn;
		public bool FilterInit;
		public int FilterWait;
		public int FilterLowerLimit;
		public int FilterUpperLimit;
		public int FilterPos;
		public int FilterSign;
		public int FilterSpeed;
		public bool FilterSlidingIn;
		public bool IgnoreFilter;

		public int PerfCurrent;
		public int PerfSpeed;
		public int PerfWait;

		public int WaveLength;

		public AhxPList PerfList;

		public int NoteDelayWait;
		public bool NoteDelayOn;
		public int NoteCutWait;
		public bool NoteCutOn;

		public sbyte[] AudioSource;
		public int AudioOffset;

		public int AudioPeriod;
		public int AudioVolume;

		public sbyte[] SquareTempBuffer = new sbyte[0x80];

		/********************************************************************/
		/// <summary>
		/// Initialize the voice variables
		/// </summary>
		/********************************************************************/
		public void Init()
		{
			VoiceVolume = 0;
			VoicePeriod = 0;

			Track = 0;
			Transpose = 0;
			NextTrack = 0;
			NextTranspose = 0;

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
			WaveformStarted = false;

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
			FilterPos = 0;
			FilterSign = 0;
			FilterSpeed = 0;
			FilterSlidingIn = false;
			IgnoreFilter = false;

			PerfCurrent = 0;
			PerfSpeed = 0;
			PerfWait = 0;

			WaveLength = 0;

			PerfList = null;

			NoteDelayWait = 0;
			NoteDelayOn = false;
			NoteCutWait = 0;
			NoteCutOn = false;

			AudioSource = null;

			AudioPeriod = 0;
			AudioVolume = 0;

			Array.Clear(VoiceBuffer);
			Array.Clear(SquareTempBuffer);
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the ADSR envelope
		/// </summary>
		/********************************************************************/
		public void CalcAdsr()
		{
			Adsr.AFrames = Instrument.Envelope.AFrames;
			Adsr.AVolume = Instrument.Envelope.AVolume * 256 / Adsr.AFrames;
			Adsr.DFrames = Instrument.Envelope.DFrames;
			Adsr.DVolume = (Instrument.Envelope.DVolume - Instrument.Envelope.AVolume) * 256 / Adsr.DFrames;
			Adsr.SFrames = Instrument.Envelope.SFrames;
			Adsr.RFrames = Instrument.Envelope.RFrames;
			Adsr.RVolume = (Instrument.Envelope.RVolume - Instrument.Envelope.DVolume) * 256 / Adsr.RFrames;
		}



		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public AhxVoices MakeDeepClone()
		{
			AhxVoices clone = (AhxVoices)MemberwiseClone();

			clone.VoiceBuffer = ArrayHelper.CloneArray(VoiceBuffer);
			clone.Adsr = Adsr.MakeDeepClone();
			clone.SquareTempBuffer = ArrayHelper.CloneArray(SquareTempBuffer);

			return clone;
		}
	}
}
