/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Ports.LibSidPlayFp.C64;
using Polycode.NostalgicPlayer.Ports.LibSidPlayFp.Exceptions;
using Polycode.NostalgicPlayer.Ports.LibSidPlayFp.SidPlayFp;

namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp
{
	/// <summary>
	/// Main library code
	/// </summary>
	internal class Player
	{
		private enum state_t
		{
			STOPPED,
			PLAYING,
			STOPPING
		}

		/// <summary>
		/// Commodore 64 emulator
		/// </summary>
		private readonly C64.C64 c64 = new C64.C64();

		/// <summary>
		/// Mixer
		/// </summary>
		private readonly Mixer mixer = new Mixer();

		/// <summary>
		/// Emulator info
		/// </summary>
		private SidPlayFp.SidTune tune;

		/// <summary>
		/// User configuration settings
		/// </summary>
		private readonly SidInfoImpl info = new SidInfoImpl();

		/// <summary>
		/// User configuration settings
		/// </summary>
		private SidConfig cfg = new SidConfig();

		/// <summary>
		/// Error message
		/// </summary>
		private string errorString;

		private volatile state_t isPlaying;

		private readonly SidRandom rand;

		private uint_least32_t startTime = 0;

		/// <summary>
		/// PAL/NTSC switch value
		/// </summary>
		private uint8_t videoSwitch;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Player()
		{
			tune = null;
			errorString = "NA";
			isPlaying = state_t.STOPPED;
			rand = new SidRandom((uint)(DateTime.Now - DateTime.UnixEpoch).TotalSeconds);

			// We need at least some minimal interrupt handling
			c64.GetMemInterface().SetKernal(null);

			Config(cfg);
		}



		/********************************************************************/
		/// <summary>
		/// Set hook for VICE tests
		/// </summary>
		/********************************************************************/
		public void SetTestHook(C64CpuBus.TestHookHandler handler)
		{
			c64.SetTestHook(handler);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetKernal(uint8_t[] rom)
		{
			c64.GetMemInterface().SetKernal(rom);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetBasic(uint8_t[] rom)
		{
			c64.GetMemInterface().SetBasic(rom);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetCharGen(uint8_t[] rom)
		{
			c64.GetMemInterface().SetCharGen(rom);
		}



		/********************************************************************/
		/// <summary>
		/// Get the current engine configuration
		/// </summary>
		/********************************************************************/
		public SidConfig Config()
		{
			return cfg;
		}



		/********************************************************************/
		/// <summary>
		/// Get the current engine information
		/// </summary>
		/********************************************************************/
		public SidInfo Info()
		{
			return info;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint_least32_t TimeMs()
		{
			return c64.GetTimeMs() - startTime;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public string Error()
		{
			return errorString;
		}



		/********************************************************************/
		/// <summary>
		/// Set new configurations
		/// </summary>
		/********************************************************************/
		public bool Config(SidConfig config, bool force = false)
		{
			// Check if configuration have been changed or forced
			if (!force && !cfg.Compare(config))
				return true;

			// Check for a sane sampling frequency
			if ((config.frequency < 8000) || (config.frequency > 192000))
			{
				errorString = Resources.IDS_SID_ERR_UNSUPPORTED_FREQ;
				return false;
			}

			// Only do these if we have a loaded tune
			if (tune != null)
			{
				SidTuneInfo tuneInfo = tune.GetInfo();

				try
				{
					SidRelease();

					List<uint> addresses = new List<uint>();

					uint_least16_t secondSidAddress = tuneInfo.SidChipBase(1) != 0 ? tuneInfo.SidChipBase(1) : config.secondSidAddress;
					if (secondSidAddress != 0)
						addresses.Add(secondSidAddress);

					uint_least16_t thirdSidAddress = tuneInfo.SidChipBase(2) != 0 ? tuneInfo.SidChipBase(2) : config.thirdSidAddress;
					if (thirdSidAddress != 0)
						addresses.Add(thirdSidAddress);

					// SID emulation setup (must be performed before the
					// environment setup call)
					SidCreate(config.sidEmulation, config.defaultSidModel, config.digiBoost, config.forceSidModel, addresses);

					// Determine C64 model
					C64.C64.model_t model = C64Model(config.defaultC64Model, config.forceC64Model);
					c64.SetModel(model);

					C64.C64.cia_model_t ciaModel = GetCiaModel(config.ciaModel);
					c64.SetCiaModel(ciaModel);

					SidParams(c64.GetMainCpuSpeed(), (int)config.frequency, config.samplingMethod, config.fastSampling);

					// Configure, setup and install C64 environment/events
					Initialize();
				}
				catch(Exception ex)
				{
					SidRelease();

					errorString = ex.Message;
					cfg.sidEmulation = null;

					if (cfg != config)
						Config(cfg);

					return false;
				}
			}

			bool isStereo = config.playback == SidConfig.playback_t.STEREO;
			info.channels = isStereo ? (uint)2 : 1;

			mixer.SetStereo(isStereo);
			mixer.SetSampleRate(config.frequency);
			mixer.SetVolume((int)config.leftVolume, (int)config.rightVolume);

			// Update configuration
			cfg = config;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load a tune
		/// </summary>
		/********************************************************************/
		public bool Load(SidPlayFp.SidTune tune)
		{
			this.tune = tune;

			if (tune != null)
			{
				// Must re-configure on fly for stereo support!
				if (!Config(cfg, true))
				{
					// Failed configuration with new tune, reject it
					this.tune = null;
					return false;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Mute/unmute a SID channel
		/// </summary>
		/********************************************************************/
		public void Mute(uint sidNum, uint voice, bool enable)
		{
			SidEmu s = mixer.GetSid(sidNum);
			if (s != null)
				s.Voice(voice, enable);
		}



		/********************************************************************/
		/// <summary>
		/// Enable/disable SID filter
		/// </summary>
		/********************************************************************/
		public void Filter(uint sidNum, bool enable)
		{
			SidEmu s = mixer.GetSid(sidNum);
			if (s != null)
				s.Filter(enable);
		}



		/********************************************************************/
		/// <summary>
		/// Run emulation and produce samples to play if a buffer is given
		/// </summary>
		/********************************************************************/
		public uint_least32_t Play(short[] leftBuffer, short[] rightBuffer, uint_least32_t count)
		{
			const uint CYCLES = 3000;

			// Make sure a tune is loaded
			if (tune == null)
				return 0;

			// Start the player loop
			if (isPlaying == state_t.STOPPED)
				isPlaying = state_t.PLAYING;

			if (isPlaying == state_t.PLAYING)
			{
				try
				{
					mixer.Begin(leftBuffer, rightBuffer, count);

					int size;

					if (mixer.GetSid(0) != null)
					{
						if ((count != 0) && (leftBuffer != null) && (rightBuffer != null))
						{
							// Reset count in case of exceptions
							count = 0;

							// Clock chips and mix into output buffers
							while ((isPlaying != state_t.STOPPED) && mixer.NotFinished())
							{
								if (!mixer.Wait())
									Run(CYCLES);

								mixer.ClockChips();
								mixer.DoMix();
							}

							count = mixer.SamplesGenerated();
						}
						else
						{
							// Clock chips and discard buffers
							size = (int)(c64.GetMainCpuSpeed() / cfg.frequency);
							while ((isPlaying != state_t.STOPPED) && (--size != 0))
							{
								Run(CYCLES);

								mixer.ClockChips();
								mixer.ResetBufs();
							}
						}
					}
					else
					{
						// Clock the machine
						size = (int)(c64.GetMainCpuSpeed() / cfg.frequency);
						while ((isPlaying != state_t.STOPPED) && (--size != 0))
						{
							Run(CYCLES);
						}
					}
				}
				catch (HaltInstructionException)
				{
					errorString = Resources.IDS_SID_ERR_ILLEGAL_INST;
					isPlaying = state_t.STOPPING;
				}
				catch (BadBufferSize)
				{
					errorString = Resources.IDS_SID_ERR_BAD_BUFFER_SIZE;
					isPlaying = state_t.STOPPING;
				}
			}

			if (isPlaying == state_t.STOPPING)
			{
				try
				{
					Initialize();
				}
				catch (ConfigErrorException)
				{
				}

				isPlaying = state_t.STOPPED;
			}

			return count;
		}



		/********************************************************************/
		/// <summary>
		/// Stop the engine
		/// </summary>
		/********************************************************************/
		public void Stop()
		{
			if ((tune != null) && (isPlaying == state_t.PLAYING))
				isPlaying = state_t.STOPPING;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Initialize the emulation
		/// </summary>
		/********************************************************************/
		private void Initialize()
		{
			isPlaying = state_t.STOPPED;

			c64.Reset();

			SidTuneInfo tuneInfo = tune.GetInfo();

			uint_least32_t size = tuneInfo.LoadAddr() + tuneInfo.C64DataLen() - 1;
			if (size > 0xffff)
				throw new ConfigErrorException(Resources.IDS_SID_ERR_UNSUPPORTED_SIZE);

			uint_least16_t powerOnDelay = cfg.powerOnDelay;

			// Delays above MAX result in random delays
			if (powerOnDelay > SidConfig.MAX_POWER_ON_DELAY)
			{
				// Limit the delay to something sensible
				powerOnDelay = (uint_least16_t)((rand.Next() >> 3) & SidConfig.MAX_POWER_ON_DELAY);
			}

			// Run for calculated number of cycles
			for (int i = 0; i <= powerOnDelay; i++)
			{
				for (int j = 0; j < 100; j++)
					c64.Clock();

				mixer.ClockChips();
				mixer.ResetBufs();
			}

			PSidDrv driver = new PSidDrv(tune.GetInfo());

			if (!driver.DrvReloc())
				throw new ConfigErrorException(driver.ErrorString());

			info.driverAddr = driver.DriverAddr();
			info.driverLength = driver.DriverLength();
			info.powerOnDelay = powerOnDelay;

			driver.Install(c64.GetMemInterface(), videoSwitch);

			if (!tune.PlaceSidTuneInC64Mem(c64.GetMemInterface()))
				throw new ConfigErrorException(tune.StatusString());

			c64.ResetCpu();

			startTime = c64.GetTimeMs();
#if false
			// Run for some cycles until the initialization routine is done
			for (int j = 0; j < 50; j++)
				c64.Clock();

			mixer.ClockChips();
			mixer.ResetBufs();
#endif
		}



		/********************************************************************/
		/// <summary>
		/// Get the CIA model for the current loaded tune
		/// </summary>
		/********************************************************************/
		private C64.C64.cia_model_t GetCiaModel(SidConfig.cia_model_t model)
		{
			switch (model)
			{
				default:
				case SidConfig.cia_model_t.MOS6526:
					return C64.C64.cia_model_t.OLD;

				case SidConfig.cia_model_t.MOS8521:
					return C64.C64.cia_model_t.NEW;

				case SidConfig.cia_model_t.MOS6526W4485:
					return C64.C64.cia_model_t.OLD_4485;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Get the C64 model for the current loaded tune
		/// </summary>
		/********************************************************************/
		private C64.C64.model_t C64Model(SidConfig.c64_model_t defaultModel, bool forced)
		{
			SidTuneInfo tuneInfo = tune.GetInfo();

			SidTuneInfo.clock_t clockSpeed = tuneInfo.ClockSpeed();

			C64.C64.model_t model;

			// Use preferred speed if forced or if song speed is unknown
			if (forced || (clockSpeed == SidTuneInfo.clock_t.CLOCK_UNKNOWN) || (clockSpeed == SidTuneInfo.clock_t.CLOCK_ANY))
			{
				switch (defaultModel)
				{
					default:
					case SidConfig.c64_model_t.PAL:
					{
						clockSpeed = SidTuneInfo.clock_t.CLOCK_PAL;
						model = C64.C64.model_t.PAL_B;
						videoSwitch = 1;
						break;
					}

					case SidConfig.c64_model_t.DREAN:
					{
						clockSpeed = SidTuneInfo.clock_t.CLOCK_PAL;
						model = C64.C64.model_t.PAL_N;
						videoSwitch = 1;	// TODO verify
						break;
					}

					case SidConfig.c64_model_t.NTSC:
					{
						clockSpeed = SidTuneInfo.clock_t.CLOCK_NTSC;
						model = C64.C64.model_t.NTSC_M;
						videoSwitch = 0;
						break;
					}

					case SidConfig.c64_model_t.OLD_NTSC:
					{
						clockSpeed = SidTuneInfo.clock_t.CLOCK_NTSC;
						model = C64.C64.model_t.OLD_NTSC_M;
						videoSwitch = 0;
						break;
					}

					case SidConfig.c64_model_t.PAL_M:
					{
						clockSpeed = SidTuneInfo.clock_t.CLOCK_NTSC;
						model = C64.C64.model_t.PAL_M;
						videoSwitch = 0;	// TODO verify
						break;
					}
				}
			}
			else
			{
				switch (clockSpeed)
				{
					default:
					case SidTuneInfo.clock_t.CLOCK_PAL:
					{
						model = C64.C64.model_t.PAL_B;
						videoSwitch = 1;
						break;
					}

					case SidTuneInfo.clock_t.CLOCK_NTSC:
					{
						model = C64.C64.model_t.NTSC_M;
						videoSwitch = 0;
						break;
					}
				}
			}

			switch (clockSpeed)
			{
				case SidTuneInfo.clock_t.CLOCK_PAL:
				{
					if (tuneInfo.SongSpeed() == SidTuneInfo.SPEED_CIA_1A)
						info.speedString = Resources.IDS_SID_SPEED_PAL_CIA;
					else if (tuneInfo.ClockSpeed() == SidTuneInfo.clock_t.CLOCK_NTSC)
						info.speedString = Resources.IDS_SID_SPEED_PAL_VBI_FIXED;
					else
						info.speedString = Resources.IDS_SID_SPEED_PAL_VBI;

					break;
				}

				case SidTuneInfo.clock_t.CLOCK_NTSC:
				{
					if (tuneInfo.SongSpeed() == SidTuneInfo.SPEED_CIA_1A)
						info.speedString = Resources.IDS_SID_SPEED_NTSC_CIA;
					else if (tuneInfo.ClockSpeed() == SidTuneInfo.clock_t.CLOCK_PAL)
						info.speedString = Resources.IDS_SID_SPEED_NTSC_VBI_FIXED;
					else
						info.speedString = Resources.IDS_SID_SPEED_NTSC_VBI;

					break;
				}
			}

			return model;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Run(uint events)
		{
			for (uint i = 0; (isPlaying != state_t.STOPPED) && (i < events); i++)
				c64.Clock();
		}



		/********************************************************************/
		/// <summary>
		/// Get the SID model
		/// </summary>
		/********************************************************************/
		private SidConfig.sid_model_t GetSidModel(SidTuneInfo.model_t sidModel, SidConfig.sid_model_t defaultModel, bool force)
		{
			SidTuneInfo.model_t tuneModel = sidModel;

			// Use preferred speed if forced or if song speed is unknown
			if (force || (tuneModel == SidTuneInfo.model_t.SIDMODEL_UNKNOWN) || (tuneModel == SidTuneInfo.model_t.SIDMODEL_ANY))
			{
				switch (defaultModel)
				{
					case SidConfig.sid_model_t.MOS6581:
					{
						tuneModel = SidTuneInfo.model_t.SIDMODEL_6581;
						break;
					}

					case SidConfig.sid_model_t.MOS8580:
					{
						tuneModel = SidTuneInfo.model_t.SIDMODEL_8580;
						break;
					}
				}
			}

			SidConfig.sid_model_t newModel;

			switch (tuneModel)
			{
				default:
				case SidTuneInfo.model_t.SIDMODEL_6581:
				{
					newModel = SidConfig.sid_model_t.MOS6581;
					break;
				}

				case SidTuneInfo.model_t.SIDMODEL_8580:
				{
					newModel = SidConfig.sid_model_t.MOS8580;
					break;
				}
			}

			return newModel;
		}



		/********************************************************************/
		/// <summary>
		/// Release the SID builders
		/// </summary>
		/********************************************************************/
		private void SidRelease()
		{
			c64.ClearSids();

			for (uint i = 0; ; i++)
			{
				SidEmu s = mixer.GetSid(i);
				if (s == null)
					break;

				SidBuilder b = s.Builder();
				if (b != null)
					b.Unlock(s);
			}

			mixer.ClearSids();
		}



		/********************************************************************/
		/// <summary>
		/// Create the SID emulation(s)
		/// </summary>
		/********************************************************************/
		private void SidCreate(SidBuilder builder, SidConfig.sid_model_t defaultModel, bool digiBoost, bool forced, List<uint> extraSidAddresses)
		{
			if (builder != null)
			{
				SidTuneInfo tuneInfo = tune.GetInfo();

				// Setup base SID
				SidConfig.sid_model_t userModel = GetSidModel(tuneInfo.SidModel(0), defaultModel, forced);
				info.sidModel = userModel;

				SidEmu s = builder.Lock(c64.GetEventScheduler(), userModel, digiBoost);
				if (!builder.GetStatus())
					throw new ConfigErrorException(builder.Error());

				c64.SetBaseSid(s);
				mixer.AddSid(s);

				// Setup extra SIDs if needed
				if (extraSidAddresses.Count != 0)
				{
					// If bits 6-7 are set to Unknown then the second SID will be set to the same SID
					// model as the first SID
					defaultModel = userModel;

					uint extraSidChips = (uint)extraSidAddresses.Count;

					for (uint i = 0; i < extraSidChips; i++)
					{
						userModel = GetSidModel(tuneInfo.SidModel(i + 1), defaultModel, forced);

						s = builder.Lock(c64.GetEventScheduler(), userModel, digiBoost);
						if (!builder.GetStatus())
							throw new ConfigErrorException(builder.Error());

						if (!c64.AddExtraSid(s, (int)extraSidAddresses[(int)i]))
							throw new ConfigErrorException(Resources.IDS_SID_ERR_UNSUPPORTED_SID_ADDR);

						mixer.AddSid(s);
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Set the SID emulation parameters
		/// </summary>
		/********************************************************************/
		private void SidParams(double cpuFreq, int frequency, SidConfig.sampling_method_t sampling, bool fastSampling)
		{
			for (uint i = 0; ; i++)
			{
				SidEmu s = mixer.GetSid(i);
				if (s == null)
					break;

				s.Sampling((float)cpuFreq, frequency, sampling, fastSampling);
			}
		}
		#endregion
	}
}
