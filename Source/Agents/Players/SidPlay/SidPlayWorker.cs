/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSid;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Builder;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Interfaces;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.SidTune;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class SidPlayWorker : ModulePlayerAgentBase, IAgentSettingsRegistrar
	{
		private const int BufferSize = 2048;

		private SidTune sidTune;

		private SidIPtr<ISidPlay2> engine;
		private SidIPtr<ISidTimer> engineTimer;
		private SidLazyIPtr<ISidUnknown> sidBuilder;

		private Sid2Config engineConfig;

		private sbyte[] leftOutputBuffer;
		private sbyte[] rightOutputBuffer;

		private bool firstTime;

		private SidPlaySettings settings;
		private static readonly SidStil sidStil = new SidStil();

		private List<string> comments;

		private const int InfoClockSpeedLine = 8;

		#region IAgentSettingsRegistrar implementation
		/********************************************************************/
		/// <summary>
		/// Return the agent ID for the agent showing the settings
		/// </summary>
		/********************************************************************/
		public Guid GetSettingsAgentId()
		{
			return new Guid("AEC0DC9F-7854-40AD-9F30-DD87D374E996");
		}
		#endregion

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => new [] { "sid" };



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			// Check the module
			SidTune temp = new SidTune();

			if (temp.Test(fileInfo))
			{
				// Found a known format, remember the SidTune for loading
				sidTune = temp;

				return AgentResult.Ok;
			}

			return AgentResult.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Return the name of the module
		/// </summary>
		/********************************************************************/
		public override string ModuleName => sidTune.GetInfo().Title;



		/********************************************************************/
		/// <summary>
		/// Return the name of the author
		/// </summary>
		/********************************************************************/
		public override string Author => sidTune.GetInfo().Author;



		/********************************************************************/
		/// <summary>
		/// Return the comment separated in lines
		/// </summary>
		/********************************************************************/
		public override string[] Comment => comments?.ToArray();



		/********************************************************************/
		/// <summary>
		/// Returns the description and value on the line given. If the line
		/// is out of range, false is returned
		/// </summary>
		/********************************************************************/
		public override bool GetInformationString(int line, out string description, out string value)
		{
			SidTuneInfo info = sidTune.GetInfo();
			Sid2Info emulationInfo = engine.Obj.GetInfo();

			int maxLine = 9;

			// Calculate the maximum number of lines
			if (info.MusPlayer)
				maxLine += 5;

			// Is the line number out of range?
			if (line > maxLine)
			{
				description = null;
				value = null;

				return false;
			}

			// Find out which line to take
			switch (line)
			{
				// Released
				case 0:
				{
					description = Resources.IDS_SID_INFODESCLINE0;
					value = info.Released;
					return true;
				}

				// File format
				case 1:
				{
					description = Resources.IDS_SID_INFODESCLINE1;
					value = info.FormatString;
					return true;
				}

				// Load range
				case 2:
				{
					description = Resources.IDS_SID_INFODESCLINE2;
					value = string.Format("${0:X4} - ${1:X4}", info.LoadAddr, info.LoadAddr + info.C64DataLen - 1);
					return true;
				}

				// Init address
				case 3:
				{
					description = Resources.IDS_SID_INFODESCLINE3;
					value = "0x" + info.InitAddr.ToString("X4");
					return true;
				}

				// Play address
				case 4:
				{
					description = Resources.IDS_SID_INFODESCLINE4;
					value = "0x" + info.PlayAddr.ToString("X4");
					return true;
				}

				// Reloc region
				case 5:
				{
					description = Resources.IDS_SID_INFODESCLINE5;

					if (info.RelocStartPage == 0)
						value = Resources.IDS_SID_NOT_PRESENT;
					else
						value = string.Format("${0:X2}00 - ${1:X2}FF", info.RelocStartPage, info.RelocStartPage + info.RelocPages - 1);

					return true;
				}

				// Driver region
				case 6:
				{
					description = Resources.IDS_SID_INFODESCLINE6;

					if (emulationInfo.DriverAddr == 0)
						value = Resources.IDS_SID_NOT_PRESENT;
					else
						value = string.Format("${0:X4} - ${1:X4}", emulationInfo.DriverAddr, emulationInfo.DriverAddr + emulationInfo.DriverLength - 1);

					return true;
				}

				// SID model
				case 7:
				{
					description = Resources.IDS_SID_INFODESCLINE7;
					value = info.SidModel1 == SidModel._8580 ? "8580" : "6581";
					return true;
				}

				// Clock speed
				case 8:
				{
					description = Resources.IDS_SID_INFODESCLINE8;
					value = info.SpeedString;
					return true;
				}

				// Environment
				case 9:
				{
					description = Resources.IDS_SID_INFODESCLINE9;

					switch (emulationInfo.Environment)
					{
						case Sid2Env.EnvPs:
						{
							value = Resources.IDS_SID_ENV_PS;
							break;
						}

						case Sid2Env.EnvTp:
						{
							value = Resources.IDS_SID_ENV_TP;
							break;
						}

						case Sid2Env.EnvBs:
						{
							value = Resources.IDS_SID_ENV_BS;
							break;
						}

						case Sid2Env.EnvR:
						{
							value = Resources.IDS_SID_ENV_R;
							break;
						}

						case Sid2Env.EnvTr:
						default:
						{
							value = Resources.IDS_SID_ENV_TR;
							break;
						}
					}

					return true;
				}
			}

			line -= 10;

			if (info.MusPlayer)
			{
				//XX
			}

			//XX
			description = null;
			value = null;

			return true;
		}
		#endregion

		#region IModulePlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Will load the file into memory
		/// </summary>
		/********************************************************************/
		public override ModulePlayerSupportFlag SupportFlags => ModulePlayerSupportFlag.BufferMode;



		/********************************************************************/
		/// <summary>
		/// Will load the file into memory
		/// </summary>
		/********************************************************************/
		public override AgentResult Load(PlayerFileInfo fileInfo, out string errorMessage)
		{
			errorMessage = string.Empty;

			try
			{
				sidTune.Load(fileInfo, out errorMessage);

				// Load STIL info if enabled
				comments = new List<string>();
				settings = new SidPlaySettings();

				if ((settings.StilEnabled || settings.BugListEnabled) && !string.IsNullOrWhiteSpace(settings.HvscPath.Trim()))
				{
					lock (sidStil)
					{
						// Load the STIL if changed
						bool stilOk = sidStil.SetBaseDir(settings.HvscPath, settings.StilEnabled, settings.BugListEnabled);
						if (stilOk)
						{
							IEnumerable<string> entries = sidStil.GetGlobalComment(fileInfo.FileName);
							if (entries != null)
								comments.AddRange(entries);

							entries = sidStil.GetFileComment(fileInfo.FileName);
							if (entries != null)
							{
								if (comments.Count > 0)
									comments.Add(string.Empty);

								comments.AddRange(entries);
							}

							entries = sidStil.GetBugComment(fileInfo.FileName);
							if (entries != null)
							{
								if (comments.Count > 0)
									comments.Add(string.Empty);

								comments.AddRange(entries);
							}
						}
					}
				}
			}
			catch (Exception)
			{
				Cleanup();
				throw;
			}

			// Ok, we're done
			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the player
		/// </summary>
		/********************************************************************/
		public override bool InitPlayer()
		{
			engine = new SidIPtr<ISidPlay2>(SidPlay2.Player.Create());
			engineTimer = new SidIPtr<ISidTimer>(engine);

			// Set configuration
			engineConfig = engine.Obj.Configuration;

			engineConfig.SampleFormat = Sid2Sample.LittleSigned;
			engineConfig.Precision = 16;
			engineConfig.PlayBack = Sid2PlayBack.Stereo;

			switch (settings.MemoryModel)
			{
				case SidPlaySettings.Environment.PlaySid:
				{
					engineConfig.Environment = Sid2Env.EnvPs;
					break;
				}

				case SidPlaySettings.Environment.Transparent:
				{
					engineConfig.Environment = Sid2Env.EnvTp;
					break;
				}

				case SidPlaySettings.Environment.FullBank:
				{
					engineConfig.Environment = Sid2Env.EnvBs;
					break;
				}

				case SidPlaySettings.Environment.Real:
				{
					engineConfig.Environment = Sid2Env.EnvR;
					break;
				}
			}

			switch (settings.ClockSpeed)
			{
				case SidPlaySettings.Clock.Pal:
				{
					engineConfig.ClockDefault = Sid2Clock.Pal;
					break;
				}

				case SidPlaySettings.Clock.Ntsc:
				{
					engineConfig.ClockDefault = Sid2Clock.Ntsc;
					break;
				}
			}

			if (settings.ClockSpeedOption == SidPlaySettings.ClockOption.Always)
			{
				engineConfig.ClockSpeed = engineConfig.ClockDefault;
				engineConfig.ClockForced = true;
			}

			switch (settings.SidModel)
			{
				case SidPlaySettings.Model.Mos6581:
				{
					engineConfig.SidDefault = Sid2Model.Mos6581;
					break;
				}

				case SidPlaySettings.Model.Mos8580:
				{
					engineConfig.SidDefault = Sid2Model.Mos8580;
					break;
				}
			}

			if (settings.SidModelOption == SidPlaySettings.ModelOption.Always)
				engineConfig.SidModel = engineConfig.SidDefault;

			// Allocate output buffer
			leftOutputBuffer = new sbyte[BufferSize];
			rightOutputBuffer = new sbyte[BufferSize];

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the player
		/// </summary>
		/********************************************************************/
		public override void CleanupPlayer()
		{
			Cleanup();
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the current song
		/// </summary>
		/********************************************************************/
		public override void InitSound(int songNumber, DurationInfo durationInfo)
		{
			sidTune.SelectSong((ushort)(songNumber + 1));

			engine.Obj.Load(sidTune);

			CreateSidEmulation();

			// Configure engine with settings
			engine.Obj.Configuration = engineConfig;

			firstTime = true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the current song
		/// </summary>
		/********************************************************************/
		public override void CleanupSound()
		{
			if (engine != null)
				engine.Obj.Stop();
		}



		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public override void Play()
		{
			if (firstTime)
			{
				// Change the module info
				OnModuleInfoChanged(InfoClockSpeedLine, sidTune.GetInfo().SpeedString);

				firstTime = false;
			}

			// Run the emulators and fill out the output buffer
			engine.Obj.Play(leftOutputBuffer, rightOutputBuffer, BufferSize);

			// Setup the NostalgicPlayer channel
			IChannel channel = VirtualChannels[0];

			channel.PlaySample(leftOutputBuffer, 0, BufferSize / 2, 16);
			channel.SetFrequency(engineConfig.Frequency);
			channel.SetVolume(256);
			channel.SetPanning((ushort)ChannelPanning.Left);

			channel = VirtualChannels[1];

			channel.PlaySample(rightOutputBuffer, 0, BufferSize / 2, 16);
			channel.SetFrequency(engineConfig.Frequency);
			channel.SetVolume(256);
			channel.SetPanning((ushort)ChannelPanning.Right);
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the module use
		/// </summary>
		/********************************************************************/
		public override int ModuleChannelCount => engineConfig.PlayBack == Sid2PlayBack.Stereo ? 2 : 1;



		/********************************************************************/
		/// <summary>
		/// Return information about sub-songs
		/// </summary>
		/********************************************************************/
		public override SubSongInfo SubSongs
		{
			get
			{
				SidTuneInfo info = sidTune.GetInfo();

				return new SubSongInfo(info.Songs, info.StartSong - 1);
			}
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player have allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			comments = null;

			leftOutputBuffer = null;
			rightOutputBuffer = null;

			sidBuilder = null;
			engineTimer = null;
			engine = null;
			sidTune = null;

			settings = null;
		}



		/********************************************************************/
		/// <summary>
		/// Creates and initialize the SID emulation
		/// </summary>
		/********************************************************************/
		private void CreateSidEmulation()
		{
			if (sidBuilder != null)
			{
				// Remove old emulation
				engineConfig.SidEmulation = null;
				engine.Obj.Configuration = engineConfig;

				sidBuilder = null;
			}

			sidBuilder = new SidLazyIPtr<ISidUnknown>(CoReSidBuilder.Create(string.Empty));
			SidLazyIPtr<IReSidBuilder> rs = new SidLazyIPtr<IReSidBuilder>(sidBuilder);

			if ((rs.Obj != null) && rs.Obj.IsOk)
			{
				ISidUnknown emulation = rs.Obj.IUnknown();

				// Setup the emulation
				rs.Obj.Create(engine.Obj.GetInfo().MaxSids);
				if (!rs.Obj.IsOk)
					throw new Exception(rs.Obj.Error);

				rs.Obj.Filter(settings.FilterEnabled);
				if (!rs.Obj.IsOk)
					throw new Exception(rs.Obj.Error);

				rs.Obj.Sampling(engineConfig.Frequency);
				if (!rs.Obj.IsOk)
					throw new Exception(rs.Obj.Error);

				// Setup filter definition
				if (settings.FilterEnabled && (settings.Filter == SidPlaySettings.FilterOption.Custom))
				{
					rs.Obj.Filter(ProvideFilter(settings.FilterFs, settings.FilterFm, settings.FilterFt));
					if (!rs.Obj.IsOk)
						throw new Exception(rs.Obj.Error);
				}

				engineConfig.SidEmulation = emulation;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Calculate a SidPlay 1 compatible filter
		/// </summary>
		/********************************************************************/
		private Spline.FCPoint[] ProvideFilter(float fs, float fm, float ft)
		{
			double fcMax = 1.0f;
			double fcMin = 0.01f;

			// Definition from ReSID
			Spline.FCPoint[] cutoff = new Spline.FCPoint[0x100];

			// Create filter
			for (uint i = 0; i < 0x100; i++)
			{
				uint rk = i << 3;
				cutoff[i].X = (int)rk;

				double fc = Math.Exp((double)rk / 0x800 * Math.Log(fs)) / fm + ft;

				if (fc < fcMin)
					fc = fcMin;

				if (fc > fcMax)
					fc = fcMax;

				cutoff[i].Y = (int)(fc * 4100);
			}

			return cutoff;
		}
		#endregion
	}
}
