/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.Roms;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibSidPlayFp.Builders.ReSidFpBuilder;
using Polycode.NostalgicPlayer.Ports.LibSidPlayFp.SidPlayFp;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class SidPlayWorker : ModulePlayerAgentBase, IDurationPlayer, IAgentSettingsRegistrar
	{
		private const int BufferSize = 2048;

		private SidTune sidTune;

		private SidPlayFp engine;

		private SidConfig engineConfig;

		private short[] leftOutputBuffer;
		private short[] rightOutputBuffer;

		private bool firstTime;

		private bool haveDuration;
		private Timer durationTimer;
		private uint startTime;

		private SidPlaySettings settings;
		private static readonly SidStil sidStil = new SidStil();
		private static readonly SidSongLength sidSongLength = new SidSongLength();

		private List<string> comments;
		private List<string> lyrics;

		[DllImport("gdi32.dll")]
		private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont, IntPtr pdv, [In] ref uint pcFonts);

		private PrivateFontCollection fonts;
		private Font commentFont;

		private DurationInfo[] allDurationInfo;

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
		public override string[] FileExtensions => new [] { "sid", "c64", "mus", "str", "prg" };



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			// Load the tune
			SidTune temp = new SidTune(fileInfo);

			// Check if the tune is valid
			if (!temp.GetStatus())
				return AgentResult.Unknown;

			// Found a known format, remember the SidTune.
			//
			// Note that there could be load errors here, but
			// that will be caught in the Load() method
			sidTune = temp;

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Return the name of the module
		/// </summary>
		/********************************************************************/
		public override string ModuleName => sidTune.GetInfo().InfoString(0);



		/********************************************************************/
		/// <summary>
		/// Return the name of the author
		/// </summary>
		/********************************************************************/
		public override string Author => sidTune.GetInfo().InfoString(1);



		/********************************************************************/
		/// <summary>
		/// Return the comment separated in lines
		/// </summary>
		/********************************************************************/
		public override string[] Comment => comments.ToArray();



		/********************************************************************/
		/// <summary>
		/// Return a specific font to be used for the comments
		/// </summary>
		/********************************************************************/
		[SupportedOSPlatform("windows")]
		public override Font CommentFont
		{
			get
			{
				if (!sidTune.GetInfo().IsMusFormat())
					return null;

				if (commentFont == null)
				{
					// Load the font from the resources
					byte[] fontData = Resources.C64_Pro_Mono_STYLE;
					IntPtr fontPtr = Marshal.AllocCoTaskMem(fontData.Length);
					Marshal.Copy(fontData, 0, fontPtr, fontData.Length);

					uint dummy = 0;
					fonts = new PrivateFontCollection();
					fonts.AddMemoryFont(fontPtr, fontData.Length);
					AddFontMemResourceEx(fontPtr, (uint)fontData.Length, IntPtr.Zero, ref dummy);

					Marshal.FreeCoTaskMem(fontPtr);

					commentFont = new Font(fonts.Families[0], 6);
				}

				return commentFont;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the lyrics separated in lines
		/// </summary>
		/********************************************************************/
		public override string[] Lyrics => lyrics.ToArray();



		/********************************************************************/
		/// <summary>
		/// Return a specific font to be used for the lyrics
		/// </summary>
		/********************************************************************/
		[SupportedOSPlatform("windows")]
		public override Font LyricsFont => CommentFont;



		/********************************************************************/
		/// <summary>
		/// Returns the description and value on the line given. If the line
		/// is out of range, false is returned
		/// </summary>
		/********************************************************************/
		public override bool GetInformationString(int line, out string description, out string value)
		{
			SidTuneInfo tuneInfo = sidTune.GetInfo();
			SidInfo engineInfo = engine.Info();

			// Find out which line to take
			switch (line)
			{
				// Released
				case 0:
				{
					description = Resources.IDS_SID_INFODESCLINE0;
					value = tuneInfo.InfoString(2);
					break;
				}

				// File format
				case 1:
				{
					description = Resources.IDS_SID_INFODESCLINE1;
					value = tuneInfo.FormatString();
					break;
				}

				// Load range
				case 2:
				{
					description = Resources.IDS_SID_INFODESCLINE2;
					value = string.Format("${0:X4} - ${1:X4}", tuneInfo.LoadAddr(), tuneInfo.LoadAddr() + tuneInfo.C64DataLen() - 1);
					break;
				}

				// Init address
				case 3:
				{
					description = Resources.IDS_SID_INFODESCLINE3;
					value = "0x" + tuneInfo.InitAddr().ToString("X4");
					break;
				}

				// Play address
				case 4:
				{
					description = Resources.IDS_SID_INFODESCLINE4;
					value = "0x" + tuneInfo.PlayAddr().ToString("X4");
					break;
				}

				// Reloc region
				case 5:
				{
					description = Resources.IDS_SID_INFODESCLINE5;

					if (tuneInfo.RelocStartPage() == 0)
						value = Resources.IDS_SID_NOT_PRESENT;
					else
						value = string.Format("${0:X2}00 - ${1:X2}FF", tuneInfo.RelocStartPage(), tuneInfo.RelocStartPage() + tuneInfo.RelocPages() - 1);

					break;
				}

				// Driver region
				case 6:
				{
					description = Resources.IDS_SID_INFODESCLINE6;

					if (engineInfo.DriverAddr() == 0)
						value = Resources.IDS_SID_NOT_PRESENT;
					else
						value = string.Format("${0:X4} - ${1:X4}", engineInfo.DriverAddr(), engineInfo.DriverAddr() + engineInfo.DriverLength() - 1);

					break;
				}

				// SID model
				case 7:
				{
					description = Resources.IDS_SID_INFODESCLINE7;
					value = engineInfo.SidModel() == SidConfig.sid_model_t.MOS8580 ? "8580" : "6581";
					break;
				}

				// Clock speed
				case 8:
				{
					description = Resources.IDS_SID_INFODESCLINE8;
					value = engineInfo.SpeedString();
					break;
				}

				default:
				{
					description = null;
					value = null;

					return false;
				}
			}

			return true;
		}
		#endregion

		#region IModulePlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Return some flags telling what the player supports
		/// </summary>
		/********************************************************************/
		public override ModulePlayerSupportFlag SupportFlags => base.SupportFlags | ModulePlayerSupportFlag.BufferMode | ModulePlayerSupportFlag.BufferDirect;



		/********************************************************************/
		/// <summary>
		/// Will load the file into memory
		/// </summary>
		/********************************************************************/
		public override AgentResult Load(PlayerFileInfo fileInfo, out string errorMessage)
		{
			// Because the module is loaded in the Identify() method, we check
			// here if it failed and return the error if so
			if (!sidTune.GetStatus())
			{
				errorMessage = sidTune.StatusString();
				return AgentResult.Error;
			}

			errorMessage = string.Empty;

			try
			{
				settings = new SidPlaySettings();

				SidTuneInfo tuneInfo = sidTune.GetInfo();

				comments = new List<string>();

				uint commentCount = tuneInfo.NumberOfCommentStrings();
				if (commentCount > 0)
				{
					// Tune have it's own comments, so use those
					for (uint i = 0; i < commentCount; i++)
						comments.Add(tuneInfo.CommentString(i));
				}
				else
				{
					// Load STIL info if enabled
					if (!string.IsNullOrWhiteSpace(settings.HvscPath.Trim()))
					{
						if (settings.StilEnabled || settings.BugListEnabled)
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
				}

				// Load song length
				if (!string.IsNullOrWhiteSpace(settings.HvscPath.Trim()))
				{
					if (settings.SongLengthEnabled)
					{
						lock (sidSongLength)
						{
							// Load the database if changed
							sidSongLength.SetBaseDir(settings.HvscPath);
						}
					}
				}

				// Set lyrics
				lyrics = new List<string>();

				uint lyricsCount = tuneInfo.NumberOfLyricsStrings();
				if (lyricsCount > 0)
				{
					// Retrieve the lyrics
					for (uint i = 0; i < lyricsCount; i++)
						lyrics.Add(tuneInfo.LyricsString(i));
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
		public override bool InitPlayer(out string errorMessage)
		{
			if (!base.InitPlayer(out errorMessage))
				return false;

			engine = new SidPlayFp();

			// Add ROMs
			engine.SetRoms(Kernal.Data, Basic.Data, Character.Data);

			// Get default configuration
			engineConfig = new SidConfig();

			// Set sampling method
			switch (settings.Mixer)
			{
				case SidPlaySettings.MixerType.Interpolate:
				{
					engineConfig.samplingMethod = SidConfig.sampling_method_t.INTERPOLATE;
					break;
				}

				case SidPlaySettings.MixerType.ResampleInterpolate:
				{
					engineConfig.samplingMethod = SidConfig.sampling_method_t.RESAMPLE_INTERPOLATE;
					break;
				}
			}

			// Setup our configuration
			switch (settings.CiaModel)
			{
				case SidPlaySettings.CiaModelType.Mos6526:
				{
					engineConfig.ciaModel = SidConfig.cia_model_t.MOS6526;
					break;
				}

				case SidPlaySettings.CiaModelType.Mos8521:
				{
					engineConfig.ciaModel = SidConfig.cia_model_t.MOS8521;
					break;
				}

				case SidPlaySettings.CiaModelType.Mos6526_W4485:
				{
					engineConfig.ciaModel = SidConfig.cia_model_t.MOS6526W4485;
					break;
				}
			}

			switch (settings.ClockSpeed)
			{
				case SidPlaySettings.ClockType.Pal:
				{
					engineConfig.defaultC64Model = SidConfig.c64_model_t.PAL;
					break;
				}

				case SidPlaySettings.ClockType.Ntsc:
				{
					engineConfig.defaultC64Model = SidConfig.c64_model_t.NTSC;
					break;
				}

				case SidPlaySettings.ClockType.NtscOld:
				{
					engineConfig.defaultC64Model = SidConfig.c64_model_t.OLD_NTSC;
					break;
				}

				case SidPlaySettings.ClockType.Drean:
				{
					engineConfig.defaultC64Model = SidConfig.c64_model_t.DREAN;
					break;
				}

				case SidPlaySettings.ClockType.PalM:
				{
					engineConfig.defaultC64Model = SidConfig.c64_model_t.PAL_M;
					break;
				}
			}

			if (settings.ClockSpeedOption == SidPlaySettings.ClockOptionType.Always)
				engineConfig.forceC64Model = true;
			else
				engineConfig.forceC64Model = false;

			switch (settings.SidModel)
			{
				case SidPlaySettings.SidModelType.Mos6581:
				{
					engineConfig.defaultSidModel = SidConfig.sid_model_t.MOS6581;
					break;
				}

				case SidPlaySettings.SidModelType.Mos8580:
				{
					engineConfig.defaultSidModel = SidConfig.sid_model_t.MOS8580;
					break;
				}
			}

			if (settings.SidModelOption == SidPlaySettings.SidModelOptionType.Always)
				engineConfig.forceSidModel = true;
			else
				engineConfig.forceSidModel = false;

			engineConfig.digiBoost = settings.DigiBoostEnabled;

			// Allocate output buffer
			leftOutputBuffer = new short[BufferSize];
			rightOutputBuffer = new short[BufferSize];

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

			base.CleanupPlayer();
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the current song
		/// </summary>
		/********************************************************************/
		public override bool InitSound(int songNumber, out string errorMessage)
		{
			if (!base.InitSound(songNumber, out errorMessage))
				return false;

			// Select sub-song to play
			sidTune.SelectSong((ushort)(songNumber + 1));

			if (!CreateSidEmulation(out errorMessage))
				return false;

			// Configure engine with settings
			if (!engine.Config(engineConfig))
			{
				errorMessage = engine.Error();
				return false;
			}

			// Load tune into engine
			if (!engine.Load(sidTune))
			{
				errorMessage = engine.Error();
				return false;
			}

			firstTime = true;

			// Create duration handler if needed
			if (haveDuration)
			{
				startTime = 0;
				durationTimer = new Timer(DurationTimerHandler, allDurationInfo[songNumber].TotalTime, 0, 900);
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the current song
		/// </summary>
		/********************************************************************/
		public override void CleanupSound()
		{
			if (durationTimer != null)
			{
				durationTimer.Dispose();
				durationTimer = null;
			}

			if (engine != null)
				engine.Stop();
		}



		/********************************************************************/
		/// <summary>
		/// Set the output frequency and number of channels
		/// </summary>
		/********************************************************************/
		public override void SetOutputFormat(uint mixerFrequency, int channels)
		{
			base.SetOutputFormat(mixerFrequency, channels);

			engineConfig.frequency = mixerFrequency;
			engineConfig.playback = channels == 1 ? SidConfig.playback_t.MONO : SidConfig.playback_t.STEREO;

			engine.Config(engineConfig, true);
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
				OnModuleInfoChanged(InfoClockSpeedLine, engine.Info().SpeedString());

				firstTime = false;
			}

			// Run the emulators and fill out the output buffer
			engine.Play(leftOutputBuffer, rightOutputBuffer, BufferSize);

			// Setup the NostalgicPlayer channel
			IChannel channel = VirtualChannels[0];
			channel.PlayBuffer(leftOutputBuffer, 0, BufferSize, PlayBufferFlag._16Bit);

			channel = VirtualChannels[1];
			channel.PlayBuffer(rightOutputBuffer, 0, BufferSize, PlayBufferFlag._16Bit);
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the module use
		/// </summary>
		/********************************************************************/
		public override int ModuleChannelCount => sidTune.GetInfo().SidChips() * 3;



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

				return new SubSongInfo((int)info.Songs(), (int)info.StartSong() - 1);
			}
		}
		#endregion

		#region IDurationPlayer implementation
		/********************************************************************/
		/// <summary>
		/// Calculate the duration for all sub-songs
		/// </summary>
		/********************************************************************/
		public DurationInfo[] CalculateDuration()
		{
			haveDuration = false;

			List<TimeSpan> songLengths = sidSongLength.GetSongLengths(sidTune);
			if (songLengths == null)
				return null;

			DurationInfo[] result = new DurationInfo[songLengths.Count];
			TimeSpan increment = new TimeSpan(0, 0, (int)IDurationPlayer.NumberOfSecondsBetweenEachSnapshot);

			for (int i = songLengths.Count - 1; i >= 0; i--)
			{
				List<PositionInfo> positionInfoList = new List<PositionInfo>();

				TimeSpan currentTotalTime = TimeSpan.Zero;
				
				for (;;)
				{
					positionInfoList.Add(new PositionInfo(currentTotalTime, PlayingFrequency, null));

					currentTotalTime += increment;
					if (currentTotalTime >= songLengths[i])
						break;
				}

				result[i] = new DurationInfo(songLengths[i], positionInfoList.ToArray());
			}

			haveDuration = true;
			allDurationInfo = result;

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Will tell the player to change its current state to match the
		/// position given
		/// </summary>
		/********************************************************************/
		public void SetSongPosition(PositionInfo positionInfo)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Return the time into the song when restarting
		/// </summary>
		/********************************************************************/
		public TimeSpan GetRestartTime()
		{
			return TimeSpan.Zero;
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

			engine = null;
			sidTune = null;

			engineConfig = null;
			settings = null;

			allDurationInfo = null;
		}



		/********************************************************************/
		/// <summary>
		/// Creates and initialize the SID emulation
		/// </summary>
		/********************************************************************/
		private bool CreateSidEmulation(out string errorMessage)
		{
			errorMessage = string.Empty;

			// Set up a SID builder
			SidBuilder sidBuilder = new ReSidFpBuilder();

			// Check if the builder is ok
			if (!sidBuilder.GetStatus())
			{
				errorMessage = sidBuilder.Error();
				return false;
			}

			// Get the number of SIDs supported by the engine
			uint maxSids = engine.Info().MaxSids();

			// Create the SID emulators
			sidBuilder.Create(maxSids);

			// Check if the builder is ok
			if (!sidBuilder.GetStatus())
			{
				errorMessage = sidBuilder.Error();
				return false;
			}

			// Setup filter
			sidBuilder.Filter(settings.FilterEnabled);

			engineConfig.sidEmulation = sidBuilder;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Is called every second to check if the song has reached the end
		/// </summary>
		/********************************************************************/
		private void DurationTimerHandler(object stateInfo)
		{
			if (engine != null)
			{
				uint seconds = engine.Time() - startTime;

				// Check if the module has reached the end
				if (seconds >= ((TimeSpan)stateInfo).TotalSeconds)
					OnEndReached();
			}
		}
		#endregion
	}
}
