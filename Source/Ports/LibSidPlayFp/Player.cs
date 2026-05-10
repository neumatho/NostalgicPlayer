/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Kit.C;
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
		// Limit to roughly 20 ms
		private const uint MAX_CYCLES = 20000;

		/// <summary>
		/// Commodore 64 emulator
		/// </summary>
		private readonly C64.C64 c64 = new C64.C64();

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

		private readonly SidRandom rand;

		private uint_least32_t startTime = 0;

		/// <summary>
		/// PAL/NTSC switch value
		/// </summary>
		private uint8_t videoSwitch;

		private readonly List<SidEmu> chips = new List<SidEmu>();

		private SimpleMixer simpleMixer;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Player()
		{
			tune = null;
			errorString = "NA";
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
		/// 
		/// </summary>
		/********************************************************************/
		public uint_least16_t GetCia1TimerA()
		{
			return c64.GetCia1TimerA();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint InstalledSids()
		{
			return (uint)chips.Count;
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

					SidParams(c64.GetMainCpuSpeed(), (int)config.frequency, config.samplingMethod);

					// Configure, setup and install C64 environment/events
					Initialize();
				}
				catch(ConfigErrorException e)
				{
					SidRelease();

					errorString = e.Message;
					cfg.sidEmulation = null;

					if (cfg != config)
						Config(cfg);

					return false;
				}
			}

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
			SidEmu s = sidNum < chips.Count ? chips[(int)sidNum] : null;
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
			SidEmu s = sidNum < chips.Count ? chips[(int)sidNum] : null;
			if (s != null)
				s.Filter(enable);
		}



		/********************************************************************/
		/// <summary>
		/// Init mixer
		/// </summary>
		/********************************************************************/
		public void InitMixer(bool stereo)
		{
			short[][] bufs = new short[chips.Count][];
			Buffers(bufs);
			simpleMixer = new SimpleMixer(stereo, bufs, (int)InstalledSids());
		}



		/********************************************************************/
		/// <summary>
		/// Mix buffers
		/// </summary>
		/********************************************************************/
		public uint Mix(short[] buffer, uint samples)
		{
			return simpleMixer.DoMix(buffer, samples);
		}



		/********************************************************************/
		/// <summary>
		/// Get the buffer pointers for each of the installed SID chips
		/// </summary>
		/********************************************************************/
		public void Buffers(short[][] buffers)
		{
			for (size_t i = 0; i < (size_t)chips.Count; i++)
				buffers[i] = chips[(int)i].Buffer();
		}



		/********************************************************************/
		/// <summary>
		/// Run the emulation for selected number of cycles.
		/// The value will be limited to a reasonable amount if too large
		/// </summary>
		/********************************************************************/
		public int Play(uint cycles)
		{
			// Make sure a tune is loaded
			if (tune == null)
			{
				errorString = Resources.IDS_SID_ERR_NO_TUNE_LOADED;
				return -1;
			}

			if (cycles > MAX_CYCLES)
				cycles = MAX_CYCLES;

			try
			{
				for (uint i = 0; i < cycles; i++)
					c64.Clock();

				int sampleCount = 0;

				foreach (SidEmu s in chips)
				{
					// Clock the chip and get the buffer
					// buffersize is expected to be the same
					// for all chips
					s.Clock();
					sampleCount = s.BufferPos();

					// Reset the buffer
					s.BufferPos(0);
				}

				return sampleCount;
			}
			catch (HaltInstructionException ill)
			{
				errorString = ill.Message;
				return -1;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Reinitialize the engine
		/// </summary>
		/********************************************************************/
		public bool Reset()
		{
			try
			{
				Initialize();

				return true;
			}
			catch (ConfigErrorException)
			{
				return false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int GetBufSize(uint cycles)
		{
			if (simpleMixer == null)
				return 0;

			if (cycles > MAX_CYCLES)
				cycles = MAX_CYCLES;

			double size = cfg.frequency / c64.GetMainCpuSpeed() * cycles;
			return (int)((int)CMath.ceil(size) * simpleMixer.Channels());
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Initialize the emulation
		/// </summary>
		/********************************************************************/
		private void Initialize()
		{
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

			powerOnDelay += 8000;

			// Run for ~[25000,50000] cycles
			for (int i = 0; i <= powerOnDelay; i++)
			{
				for (int j = 0; j < 3; j++)
					c64.Clock();

				foreach (SidEmu chip in chips)
				{
					chip.Clock();
					chip.BufferPos(0);
				}
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
#if false
			// Run for some cycles until the initialization routine is done
			for (int j = 0; j < 50; j++)
				c64.Clock();

			foreach (SidEmu chip in chips)
			{
				chip.Clock();
				chip.BufferPos(0);
			}
#endif

			startTime = c64.GetTimeMs();
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
		/// Get the SID model
		/// </summary>
		/********************************************************************/
		private SidTuneInfo.model_t GetSidModel(SidConfig.sid_model_t sidModel)
		{
			switch (sidModel)
			{
				case SidConfig.sid_model_t.MOS6581:
					return SidTuneInfo.model_t.SIDMODEL_6581;

				case SidConfig.sid_model_t.MOS8580:
					return SidTuneInfo.model_t.SIDMODEL_8580;

				default:
					return SidTuneInfo.model_t.SIDMODEL_UNKNOWN;
			}
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

			switch (tuneModel)
			{
				default:
				case SidTuneInfo.model_t.SIDMODEL_6581:
					return SidConfig.sid_model_t.MOS6581;

				case SidTuneInfo.model_t.SIDMODEL_8580:
					return SidConfig.sid_model_t.MOS8580;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Release the SID builders
		/// </summary>
		/********************************************************************/
		private void SidRelease()
		{
			c64.ClearSids();

			foreach (SidEmu s in chips)
			{
				SidBuilder b = s.Builder();
				if (b != null)
					b.Unlock(s);
			}

			chips.Clear();
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
				chips.Clear();
				info.sidModels.Clear();

				SidTuneInfo tuneInfo = tune.GetInfo();

				// Setup base SID
				SidConfig.sid_model_t userModel = GetSidModel(tuneInfo.SidModel(0), defaultModel, forced);

				SidEmu s = builder.Lock(c64.GetEventScheduler(), userModel, digiBoost);
				if (s == null)
					throw new ConfigErrorException(builder.Error());

				c64.SetBaseSid(s);
				chips.Add(s);
				info.sidModels.Add(GetSidModel(userModel));

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
						if (s == null)
							throw new ConfigErrorException(builder.Error());

						if (!c64.AddExtraSid(s, (int)extraSidAddresses[(int)i]))
							throw new ConfigErrorException(Resources.IDS_SID_ERR_UNSUPPORTED_SID_ADDR);

						chips.Add(s);
						info.sidModels.Add(GetSidModel(userModel));
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Set the SID emulation parameters
		/// </summary>
		/********************************************************************/
		private void SidParams(double cpuFreq, int frequency, SidConfig.sampling_method_t sampling)
		{
			foreach (SidEmu s in chips)
				s.Sampling((float)cpuFreq, frequency, sampling);
		}
		#endregion
	}
}
