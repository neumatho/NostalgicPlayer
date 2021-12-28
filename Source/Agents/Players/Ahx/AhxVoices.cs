/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.Ahx.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.Ahx
{
	/// <summary>
	/// Handler of a single voice
	/// </summary>
	internal class AhxVoices
	{
		// Read those variables for mixing
		public int voiceVolume;
		public int voicePeriod;
		public readonly sbyte[] voiceBuffer = new sbyte[0x281];

		public int track;
		public int transpose;
		public int nextTrack;
		public int nextTranspose;

		public int adsrVolume;									// Fixed point 8:8
		public readonly AhxEnvelope adsr = new AhxEnvelope();	// Frames / delta fixed 8:8

		public AhxInstrument instrument;						// Current instrument

		public int instrPeriod;
		public int trackPeriod;
		public int vibratoPeriod;

		public int noteMaxVolume;
		public int perfSubVolume;
		public int trackMasterVolume;

		public bool newWaveform;
		public int waveform;
		public bool plantSquare;
		public bool plantPeriod;
		public bool ignoreSquare;

		public bool trackOn;
		public bool fixedNote;

		public int volumeSlideUp;
		public int volumeSlideDown;

		public int hardCut;
		public bool hardCutRelease;
		public int hardCutReleaseF;

		public int periodSlideSpeed;
		public int periodSlidePeriod;
		public int periodSlideLimit;
		public bool periodSlideOn;
		public bool periodSlideWithLimit;

		public int periodPerfSlideSpeed;
		public int periodPerfSlidePeriod;
		public bool periodPerfSlideOn;

		public int vibratoDelay;
		public int vibratoCurrent;
		public int vibratoDepth;
		public int vibratoSpeed;

		public bool squareOn;
		public bool squareInit;
		public int squareWait;
		public int squareLowerLimit;
		public int squareUpperLimit;
		public int squarePos;
		public int squareSign;
		public bool squareSlidingIn;
		public bool squareReverse;

		public bool filterOn;
		public bool filterInit;
		public int filterWait;
		public int filterLowerLimit;
		public int filterUpperLimit;
		public int filterPos;
		public int filterSign;
		public int filterSpeed;
		public bool filterSlidingIn;
		public bool ignoreFilter;

		public int perfCurrent;
		public int perfSpeed;
		public int perfWait;

		public int waveLength;

		public AhxPList perfList;

		public int noteDelayWait;
		public bool noteDelayOn;
		public int noteCutWait;
		public bool noteCutOn;

		public sbyte[] audioSource;
		public int audioOffset;

		public int audioPeriod;
		public int audioVolume;

		public readonly sbyte[] squareTempBuffer = new sbyte[0x80];

		/********************************************************************/
		/// <summary>
		/// Initialize the voice variables
		/// </summary>
		/********************************************************************/
		public void Init()
		{
			voiceVolume = 0;
			voicePeriod = 0;

			track = 0;
			transpose = 0;
			nextTrack = 0;
			nextTranspose = 0;

			adsrVolume = 0;
			adsr.AFrames = 0;
			adsr.AVolume = 0;
			adsr.DFrames = 0;
			adsr.DVolume = 0;
			adsr.SFrames = 0;
			adsr.RFrames = 0;
			adsr.RVolume = 0;

			instrument = null;

			instrPeriod = 0;
			trackPeriod = 0;
			vibratoPeriod = 0;

			noteMaxVolume = 0;
			perfSubVolume = 0;
			trackMasterVolume = 0x40;

			newWaveform = false;
			waveform = 0;
			plantSquare	= false;
			plantPeriod = false;
			ignoreSquare = false;

			trackOn = true;
			fixedNote = false;

			volumeSlideUp = 0;
			volumeSlideDown = 0;

			hardCut = 0;
			hardCutRelease = false;
			hardCutReleaseF = 0;

			periodSlideSpeed = 0;
			periodSlidePeriod = 0;
			periodSlideLimit = 0;
			periodSlideOn = false;
			periodSlideWithLimit = false;

			periodPerfSlideSpeed = 0;
			periodPerfSlidePeriod = 0;
			periodPerfSlideOn = false;

			vibratoDelay = 0;
			vibratoCurrent = 0;
			vibratoDepth = 0;
			vibratoSpeed = 0;

			squareOn = false;
			squareInit = false;
			squareWait = 0;
			squareLowerLimit = 0;
			squareUpperLimit = 0;
			squarePos = 0;
			squareSign = 0;
			squareSlidingIn = false;
			squareReverse = false;

			filterOn = false;
			filterInit = false;
			filterWait = 0;
			filterLowerLimit = 0;
			filterUpperLimit = 0;
			filterPos = 0;
			filterSign = 0;
			filterSpeed = 0;
			filterSlidingIn = false;
			ignoreFilter = false;

			perfCurrent = 0;
			perfSpeed = 0;
			perfWait = 0;

			waveLength = 0;

			perfList = null;

			noteDelayWait = 0;
			noteDelayOn = false;
			noteCutWait = 0;
			noteCutOn = false;

			audioSource = null;

			audioPeriod = 0;
			audioVolume = 0;

			Array.Clear(voiceBuffer);
			Array.Clear(squareTempBuffer);
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the ADSR envelope
		/// </summary>
		/********************************************************************/
		public void CalcAdsr()
		{
			adsr.AFrames = instrument.Envelope.AFrames;
			adsr.AVolume = instrument.Envelope.AVolume * 256 / adsr.AFrames;
			adsr.DFrames = instrument.Envelope.DFrames;
			adsr.DVolume = (instrument.Envelope.DVolume - instrument.Envelope.AVolume) * 256 / adsr.DFrames;
			adsr.SFrames = instrument.Envelope.SFrames;
			adsr.RFrames = instrument.Envelope.RFrames;
			adsr.RVolume = (instrument.Envelope.RVolume - instrument.Envelope.DVolume) * 256 / adsr.RFrames;
		}
	}
}
