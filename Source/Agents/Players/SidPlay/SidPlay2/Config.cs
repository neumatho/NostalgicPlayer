/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2
{
	/// <summary>
	/// Library configuration code
	/// </summary>
	internal partial class Player
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Config(Sid2Config cfg)
		{
			bool monoSid = true;

			if (running != 0)
				throw new Exception(Resources.IDS_SID_ERR_CONF_WHILST_ACTIVE);

			// Check for base sampling frequency
			if (cfg.Frequency < 4000)
				throw new Exception(Resources.IDS_SID_ERR_UNSUPPORTED_FREQ);

			// Check for legal precision
			switch (cfg.Precision)
			{
				case 8 or 16 or 24:
				{
					if (cfg.Precision > MaxPrecision)
						throw new Exception(Resources.IDS_SID_ERR_UNSUPPORTED_PRECISION);

					break;
				}

				default:
					throw new Exception(Resources.IDS_SID_ERR_UNSUPPORTED_PRECISION);
			}

			// Initialize sid mapping table
			for (int i = 0; i < MapperSize; i++)
				sidMapper[i] = 0;

			// Only do these if we have a loaded tune
			if (tune != null)
			{
				try
				{
					if (playerState != PlayerType.Paused)
						tuneInfo = tune.GetInfo();

					// SID emulation setup (must be performed before the
					// environment setup call)
					SidLazyIPtr<ISidBuilder> builder = new SidLazyIPtr<ISidBuilder>(cfg.SidEmulation);
					if (!SidCreate(builder, cfg.SidModel, cfg.SidDefault))
					{
						config.SidEmulation = null;
						throw new Exception(builder.Obj.Error);
					}

					if (playerState != PlayerType.Paused)
					{
						// Must be in this order:
						//
						// Determine clock speed
						double cpuFreq = ClockSpeed(cfg.ClockSpeed, cfg.ClockDefault, cfg.ClockForced);
						info.CpuFrequency = cpuFreq;

						// Fixed point conversion 16.16
						samplePeriod = (uint)(cpuFreq / cfg.Frequency * (1 << 16) * fastForwardFactor);

						// Setup fake cia
						sid6526.Clock((ushort)(cpuFreq / VicFreqPal + 0.5));

						if ((tuneInfo.SongSpeed == Speed.Cia_1A) || (tuneInfo.ClockSpeed == Clock.Ntsc))
							sid6526.Clock((ushort)(cpuFreq / VicFreqNtsc + 0.5));

						// @FIXME@ See Mos6525 Clock() method for details
						//
						// Setup TOD clock
						if (tuneInfo.ClockSpeed == Clock.Pal)
						{
							cia.Clock(cpuFreq / VicFreqPal);
							cia2.Clock(cpuFreq / VicFreqPal);
						}
						else
						{
							cia.Clock(cpuFreq / VicFreqNtsc);
							cia2.Clock(cpuFreq / VicFreqNtsc);
						}

						// Start the real time clock event
						rtc.Clock(cpuFreq);

						// Configure, setup and install C64 environment/events
						Environment(cfg.Environment);
					}

					// Setup sid mapping table
					//
					// Note this should be based on tuneInfo.SidChipBase1
					// but this is only temporary code anyway
					if (tuneInfo.SidChipBase2 != 0)
					{
						// Assumed to be in d4xx-d7xx range
						sidMapper[(tuneInfo.SidChipBase2 >> 5) & (MapperSize - 1)] = 1;
					}

					monoSid = tuneInfo.SidChipBase2 == 0;
				}
				catch(Exception)
				{
					if (config != cfg)
						Config(config);

					throw;
				}
			}

			SidSamples(cfg.SidSamples);

			// All parameters check out, so configure player
			info.Channels = 1;
			emulateStereo = false;

			if (cfg.PlayBack == Sid2PlayBack.Stereo)
			{
				info.Channels++;

				// Enough sids are available to perform
				// stereo splitting
				if (monoSid && (sid[1].IUnknown() != nullSid.IUnknown()))
					emulateStereo = cfg.EmulateStereo;
			}

			// Only force dual sids if second wasn't detected
			if (monoSid && cfg.ForceDualSids)
			{
				monoSid = false;
				sidMapper[(0xd500 >> 5) & (MapperSize - 1)] = 1;	// Assumed
			}

			leftVolume = (int)cfg.LeftVolume;
			rightVolume = (int)cfg.RightVolume;

			if (cfg.PlayBack != Sid2PlayBack.Mono)
			{
				// Try splitting channels across 2 sids
				if (emulateStereo)
				{
					// Mute voices
					SidLazyIPtr<ISidMixer> mixer = new SidLazyIPtr<ISidMixer>(sid[0]);
					if (mixer.Obj != null)
					{
						mixer.Obj.Mute(0, true);
						mixer.Obj.Mute(2, true);
					}

					mixer = new SidLazyIPtr<ISidMixer>(sid[1]);
					if (mixer.Obj != null)
						mixer.Obj.Mute(1, true);

					// 2 voices scaled to unity from 4 (was !SID_VOL)
					//    leftVolume *= 2;
					//    rightVolume *= 2;
					//
					// 2 voices scaled to unity from 3 (was SID_VOL)
					//    leftVolume *= 3;
					//    leftVolume /= 2;
					//    rightVolume *= 3;
					//    rightVolume /= 2;
					monoSid = false;
				}

				if (cfg.PlayBack == Sid2PlayBack.Left)
					xsid.Mute(true);
			}

			// Setup the audio side, depending on the audio hardware
			// and the information returned by SID tune
			switch (cfg.Precision)
			{
				case 8:
				{
					if (monoSid)
					{
						if (cfg.PlayBack == Sid2PlayBack.Stereo)
							output = StereoOut8MonoIn;
						else
							output = MonoOut8MonoIn;
					}
					else
					{
						switch (cfg.PlayBack)
						{
							case Sid2PlayBack.Stereo:	// Stereo hardware
							{
								output = StereoOut8StereoIn;
								break;
							}

							case Sid2PlayBack.Right:	// Mono hardware
							{
								output = MonoOut8StereoRIn;
								break;
							}

							case Sid2PlayBack.Left:
							{
								output = MonoOut8MonoIn;
								break;
							}

							case Sid2PlayBack.Mono:
							{
								output = MonoOut8StereoIn;
								break;
							}
						}
					}
					break;
				}

				case 16:
				{
					if (monoSid)
					{
						if (cfg.PlayBack == Sid2PlayBack.Stereo)
							output = StereoOut16MonoIn;
						else
							output = MonoOut16MonoIn;
					}
					else
					{
						switch (cfg.PlayBack)
						{
							case Sid2PlayBack.Stereo:	// Stereo hardware
							{
								output = StereoOut16StereoIn;
								break;
							}

							case Sid2PlayBack.Right:	// Mono hardware
							{
								output = MonoOut16StereoRIn;
								break;
							}

							case Sid2PlayBack.Left:
							{
								output = MonoOut16MonoIn;
								break;
							}

							case Sid2PlayBack.Mono:
							{
								output = MonoOut16StereoIn;
								break;
							}
						}
					}
					break;
				}
			}

			// Update configuration
			config = cfg;

			if (config.Optimization > MaxOptimization)
				config.Optimization = MaxOptimization;
		}



		/********************************************************************/
		/// <summary>
		/// Clock speed changes due to loading a new song
		/// </summary>
		/********************************************************************/
		private double ClockSpeed(Sid2Clock userClock, Sid2Clock defaultClock, bool forced)
		{
			// Detect the correct song speed
			//
			// Determine song speed when unknown
			if (tuneInfo.ClockSpeed == Clock.Unknown)
			{
				switch (defaultClock)
				{
					case Sid2Clock.Pal:
					{
						tuneInfo.ClockSpeed = Clock.Pal;
						break;
					}

					case Sid2Clock.Ntsc:
					{
						tuneInfo.ClockSpeed = Clock.Ntsc;
						break;
					}

					case Sid2Clock.Correct:
					{
						// No default so base it on emulation clock
						tuneInfo.ClockSpeed = Clock.Any;
						break;
					}
				}
			}

			// Since song will run correct at any clock speed,
			// set tune speed to the current emulation
			if (tuneInfo.ClockSpeed == Clock.Any)
			{
				if (userClock == Sid2Clock.Correct)
					userClock = defaultClock;

				switch (userClock)
				{
					case Sid2Clock.Ntsc:
					{
						tuneInfo.ClockSpeed = Clock.Ntsc;
						break;
					}

					case Sid2Clock.Pal:
					default:
					{
						tuneInfo.ClockSpeed = Clock.Pal;
						break;
					}
				}
			}

			if (userClock == Sid2Clock.Correct)
			{
				switch (tuneInfo.ClockSpeed)
				{
					case Clock.Ntsc:
					{
						userClock = Sid2Clock.Ntsc;
						break;
					}

					case Clock.Pal:
					{
						userClock = Sid2Clock.Pal;
						break;
					}
				}
			}

			if (forced)
			{
				tuneInfo.ClockSpeed = Clock.Pal;

				if (userClock == Sid2Clock.Ntsc)
					tuneInfo.ClockSpeed = Clock.Ntsc;
			}

			if (tuneInfo.ClockSpeed == Clock.Pal)
				vic.Chip(Mos656xModel.Mos6569);
			else
				vic.Chip(Mos656xModel.Mos6567R8);

			double cpuFreq;

			if (userClock == Sid2Clock.Pal)
			{
				cpuFreq = ClockFreqPal;
				tuneInfo.SpeedString = Resources.IDS_SID_SPEED_PAL_VBI;

				if (tuneInfo.SongSpeed == Speed.Cia_1A)
					tuneInfo.SpeedString = Resources.IDS_SID_SPEED_PAL_CIA;
				else if (tuneInfo.ClockSpeed == Clock.Ntsc)
					tuneInfo.SpeedString = Resources.IDS_SID_SPEED_PAL_VBI_FIXED;
			}
			else
			{
				cpuFreq = ClockFreqNtsc;
				tuneInfo.SpeedString = Resources.IDS_SID_SPEED_NTSC_VBI;

				if (tuneInfo.SongSpeed == Speed.Cia_1A)
					tuneInfo.SpeedString = Resources.IDS_SID_SPEED_NTSC_CIA;
				else if (tuneInfo.ClockSpeed == Clock.Pal)
					tuneInfo.SpeedString = Resources.IDS_SID_SPEED_NTSC_VBI_FIXED;
			}

			return cpuFreq;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Environment(Sid2Env env)
		{
			switch (tuneInfo.Compatibility)
			{
				case Compatibility.R64:
				case Compatibility.Basic:
				{
					env = Sid2Env.EnvR;
					break;
				}

				case Compatibility.PSid:
				{
					if (env == Sid2Env.EnvR)
						env = Sid2Env.EnvBs;

					break;
				}
			}

			// Environment already set?
			if (!((ram != null) && (info.Environment == env)))
			{
				// Setup new player environment
				info.Environment = env;

				ram = new byte[0x10000];

				// Setup the access functions to the environment
				// and the properties the memory has
				if (info.Environment == Sid2Env.EnvPs)
				{
					// PlaySid has no roms and SID exists in ram space
					rom = ram;

					readMemByte = ReadMemByte_Plain;
					writeMemByte = WriteMemByte_PlaySid;
					readMemDataByte = ReadMemByte_Plain;
				}
				else
				{
					rom = new byte[0x10000];

					switch (info.Environment)
					{
						case Sid2Env.EnvTp:
						{
							readMemByte = ReadMemByte_Plain;
							writeMemByte = WriteMemByte_SidPlay;
							readMemDataByte = ReadMemByte_SidPlayTp;
							break;
						}

						case Sid2Env.EnvBs:
						{
							readMemByte = ReadMemByte_Plain;
							writeMemByte = WriteMemByte_SidPlay;
							readMemDataByte = ReadMemByte_SidPlayBs;
							break;
						}

						case Sid2Env.EnvR:
						default:
						{
							readMemByte = ReadMemByte_SidPlayBs;
							writeMemByte = WriteMemByte_SidPlay;
							readMemDataByte = ReadMemByte_SidPlayBs;
							break;
						}
					}
				}
			}

			// Have to reload the song into memory as
			// everything has changed
			Sid2Env old = info.Environment;
			info.Environment = env;

			try
			{
				Initialize();
			}
			finally
			{
				info.Environment = old;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Integrate SID emulation from the builder class
		/// </summary>
		/********************************************************************/
		private bool SidCreate(SidLazyIPtr<ISidBuilder> builder, Sid2Model userModel, Sid2Model defaultModel)
		{
			sid[0] = new SidLazyIPtr<ISidEmulation>(xsid.Emulation());

			// Make xsid forget it's emulation
			{
				SidIPtr<ISidEmulation> none = new SidIPtr<ISidEmulation>(nullSid.IUnknown());
				xsid.Emulation(none);
			}

			// Release old sids
			{
				for (int i = 0; i < MaxSids; i++)
				{
					SidLazyIPtr<ISidBuilder> b = new SidLazyIPtr<ISidBuilder>(sid[i].Obj.Builder());
					if (b.Obj != null)
						b.Obj.Unlock(sid[i]);
				}
			}

			if (builder.Obj == null)
			{
				// No sid
				for (int i = 0; i < MaxSids; i++)
					sid[i] = new SidLazyIPtr<ISidEmulation>(nullSid.IUnknown());
			}
			else
			{
				// Detect the correct SID model
				//
				// Determine model when unknown
				Sid2Model[] userModels = new Sid2Model[MaxSids];

				userModels[0] = SidModel(ref tuneInfo.SidModel1, userModel, defaultModel);
				userModels[1] = SidModel(ref tuneInfo.SidModel2, userModel, userModels[0]);

				for (int i = 0; i < MaxSids; i++)
				{
					// Get first SID emulation
					sid[i] = new SidLazyIPtr<ISidEmulation>(builder.Obj.Lock(myC64, userModels[i]));

					if (sid[i].Obj == null)
						sid[i] = new SidLazyIPtr<ISidEmulation>(nullSid.IUnknown());

					if ((i == 0) && (builder.Obj == null))
						return false;

					sid[i].Obj.Optimization(config.Optimization);
					sid[i].Obj.Clock((Sid2Clock)tuneInfo.ClockSpeed);
				}
			}

			xsid.Emulation(sid[0]);
			sid[0] = new SidLazyIPtr<ISidEmulation>(xsid.IUnknown());

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Sid2Model SidModel(ref SidModel model, Sid2Model userModel, Sid2Model defaultModel)
		{
			if (model == Containers.SidModel.Unknown)
			{
				switch (defaultModel)
				{
					case Sid2Model.Mos6581:
					{
						model = Containers.SidModel._6581;
						break;
					}

					case Sid2Model.Mos8580:
					{
						model = Containers.SidModel._8580;
						break;
					}

					case Sid2Model.ModelCorrect:
					{
						// No default so base it on emulation clock
						model = Containers.SidModel.Any;
						break;
					}
				}
			}

			// Since song will run correct on any sid model,
			// set it to the current emulation
			if (model == Containers.SidModel.Any)
			{
				if (userModel == Sid2Model.ModelCorrect)
					userModel = defaultModel;

				switch (userModel)
				{
					case Sid2Model.Mos8580:
					{
						model = Containers.SidModel._8580;
						break;
					}

					case Sid2Model.Mos6581:
					default:
					{
						model = Containers.SidModel._6581;
						break;
					}
				}
			}

			switch (userModel)
			{
				case Sid2Model.ModelCorrect:
				{
					switch (model)
					{
						case Containers.SidModel._8580:
						{
							userModel = Sid2Model.Mos8580;
							break;
						}

						case Containers.SidModel._6581:
						{
							userModel = Sid2Model.Mos6581;
							break;
						}
					}
					break;
				}

				// Fixup tune information if model is forced
				case Sid2Model.Mos6581:
				{
					model = Containers.SidModel._6581;
					break;
				}

				case Sid2Model.Mos8580:
				{
					model = Containers.SidModel._8580;
					break;
				}
			}

			return userModel;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void SidSamples(bool enable)
		{
			sbyte gain = 0;
			xsid.SidSamples(enable);

			// Now balance voices
			if (!enable)
				gain = -25;

			xsid.Gain((sbyte)(-100 - gain));
			sid[0] = new SidLazyIPtr<ISidEmulation>(xsid.Emulation());

			for (int i = 0; i < MaxSids; i++)
			{
				SidIPtr<ISidMixer> mixer = new SidIPtr<ISidMixer>(sid[i]);
				if (mixer.Obj != null)
					mixer.Obj.Gain(gain);
			}

			sid[0] = new SidLazyIPtr<ISidEmulation>(xsid.IUnknown());
		}
	}
}
