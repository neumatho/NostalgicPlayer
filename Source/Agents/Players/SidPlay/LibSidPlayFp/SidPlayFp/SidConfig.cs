/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.SidPlayFp
{
	/// <summary>
	/// An instance of this class is used to transport emulator settings
	/// to and from the interface class
	/// </summary>
	internal class SidConfig
	{
		/// <summary>
		/// Playback mode
		/// </summary>
		public enum playback_t
		{
			/// <summary>
			/// One channel mono playback
			/// </summary>
			MONO = 1,

			/// <summary>
			/// Two channels stereo playback
			/// </summary>
			STEREO
		}

		/// <summary>
		/// SID chip model
		/// </summary>
		public enum sid_model_t
		{
			/// <summary>
			/// Old MOS 6581
			/// </summary>
			MOS6581,

			/// <summary>
			/// New CSG 8580/MOS 6582
			/// </summary>
			MOS8580
		}

		/// <summary>
		/// CIA chip model
		/// </summary>
		public enum cia_model_t
		{
			/// <summary>
			/// Old MOS 6526/6526A with interrupts delayed by one cycle
			/// </summary>
			MOS6526,

			/// <summary>
			/// New CSG 8521, often marked 6526 216A
			/// </summary>
			MOS8521,

			/// <summary>
			/// Old MOS 6526, peculiar batch from week 4485 with different serial port behavior since 2.2
			/// </summary>
			MOS6526W4485
		}

		/// <summary>
		/// C64 model
		/// </summary>
		public enum c64_model_t
		{
			/// <summary>
			/// European PAL model
			/// </summary>
			PAL,

			/// <summary>
			/// American/Japanese NTSC model
			/// </summary>
			NTSC,

			/// <summary>
			/// Older NTSC model with different video chip revision
			/// </summary>
			OLD_NTSC,

			/// <summary>
			/// Argentinian PAL-N model
			/// </summary>
			DREAN,

			/// <summary>
			/// Brasilian PAL-M model
			/// </summary>
			PAL_M
		}

		public enum sampling_method_t
		{
			/// <summary>
			/// Interpolation
			/// </summary>
			INTERPOLATE,

			/// <summary>
			/// Resampling
			/// </summary>
			RESAMPLE_INTERPOLATE
		}

		/// <summary>
		/// Intended C64 model when unknown or forced
		/// </summary>
		public c64_model_t defaultC64Model;

		/// <summary>
		/// Force the model to defaultC64Model ignoring tune's clock setting
		/// </summary>
		public bool forceC64Model;

		/// <summary>
		/// Intended SID model when unknown or forced
		/// </summary>
		public sid_model_t defaultSidModel;

		/// <summary>
		/// Force the SID model to defaultSidModel
		/// </summary>
		public bool forceSidModel;

		/// <summary>
		/// Enable digiboost when 8580 SID model is used
		/// </summary>
		public bool digiBoost;

		/// <summary>
		/// Intended CIA model
		/// </summary>
		public cia_model_t ciaModel;

		/// <summary>
		/// Playback mode
		/// </summary>
		public playback_t playback;

		/// <summary>
		/// Sampling frequency
		/// </summary>
		public uint_least32_t frequency;

		/// <summary>
		/// Extra SID chip addresses
		/// </summary>
		public uint_least16_t secondSidAddress;

		/// <summary>
		/// Extra SID chip addresses
		/// </summary>
		public uint_least16_t thirdSidAddress;

		/// <summary>
		/// Pointer to selected emulation,
		/// reSIDfp, reSID, hardSID or exSID
		/// </summary>
		public SidBuilder sidEmulation;

		/// <summary>
		/// Left channel volume
		/// </summary>
		public uint_least32_t leftVolume;

		/// <summary>
		/// Right channel volume
		/// </summary>
		public uint_least32_t rightVolume;

		/// <summary>
		/// Power on delay cycles
		/// </summary>
		public uint_least16_t powerOnDelay;

		/// <summary>
		/// Sampling method
		/// </summary>
		public sampling_method_t samplingMethod;

		/// <summary>
		/// Faster low-quality emulation,
		/// available only for reSID
		/// </summary>
		public bool fastSampling;

		// Maximum power on delay.
		// - Delays <= MAX produce constant results
		// - Delays >  MAX produce random results
		public const uint_least16_t MAX_POWER_ON_DELAY = 0x1fff;
		private const uint_least16_t DEFAULT_POWER_ON_DELAY = MAX_POWER_ON_DELAY + 1;

		private const uint_least32_t DEFAULT_SAMPLING_FREQ = 44100;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SidConfig()
		{
			defaultC64Model = c64_model_t.PAL;
			forceC64Model = false;
			defaultSidModel = sid_model_t.MOS6581;
			forceSidModel = false;
			digiBoost = false;
			ciaModel = cia_model_t.MOS6526;
			playback = playback_t.MONO;
			frequency = DEFAULT_SAMPLING_FREQ;
			secondSidAddress = 0;
			thirdSidAddress = 0;
			sidEmulation = null;
			leftVolume = Mixer.VOLUME_MAX;
			rightVolume = Mixer.VOLUME_MAX;
			powerOnDelay = DEFAULT_POWER_ON_DELAY;
			samplingMethod = sampling_method_t.RESAMPLE_INTERPOLATE;
			fastSampling = false;
		}



		/********************************************************************/
		/// <summary>
		/// Compare two config objects. Return true if different
		/// </summary>
		/********************************************************************/
		public bool Compare(SidConfig config)
		{
			return defaultC64Model != config.defaultC64Model
				|| forceC64Model != config.forceC64Model
				|| defaultSidModel != config.defaultSidModel
				|| forceSidModel != config.forceSidModel
				|| digiBoost != config.digiBoost
				|| ciaModel != config.ciaModel
				|| playback != config.playback
				|| frequency != config.frequency
				|| secondSidAddress != config.secondSidAddress
				|| thirdSidAddress != config.thirdSidAddress
				|| sidEmulation != config.sidEmulation
				|| leftVolume != config.leftVolume
				|| rightVolume != config.rightVolume
				|| powerOnDelay != config.powerOnDelay
				|| samplingMethod != config.samplingMethod
				|| fastSampling != config.fastSampling;
		}
	}
}
