/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Containers;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Extensions;

namespace Polycode.NostalgicPlayer.Agent.Player.OpenMpt
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class OpenMptWorker : ModulePlayerAgentBase, IDuration
	{
		private readonly Guid formatId;

		private Module_Ext module;

		private string title;
		private string author;
		private string comment;
		private string extraInfo;
		private SubSongInfo subSongs;
		private DurationInfo[] durations;
		private bool hasInstruments;
		private bool useSurround;
		private SurroundMode surroundMode;

		private DurationInfo currentDuration;
		private int playingPosition;
		private int playingPattern;
		private int currentSpeed;
		private double currentTempo;
		private bool endReached;

		private short[] leftBuffer;
		private short[] rightBuffer;
		private short[] leftRearBuffer;
		private short[] rightRearBuffer;

		private const int InfoPositionLine = 4;
		private const int InfoPatternLine = 5;
		private const int InfoSpeedLine = 6;
		private const int InfoTempoLine = 7;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public OpenMptWorker(Guid formatId)
		{
			this.formatId = formatId;
		}



		/********************************************************************/
		/// <summary>
		/// Return some flags telling what the player supports
		/// </summary>
		/********************************************************************/
		public override ModulePlayerSupportFlag SupportFlags => base.SupportFlags | ModulePlayerSupportFlag.SetPosition | ModulePlayerSupportFlag.BufferMode | ModulePlayerSupportFlag.BufferDirect | ModulePlayerSupportFlag.Visualize | ModulePlayerSupportFlag.EnableChannels;

		#region Identify
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => OpenMptIdentifier.FileExtensions;



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			return AgentResult.Unknown;
		}



		/********************************************************************/
		// <summary>
		// Return some extra information about the format. If it returns
		// null or an empty string, nothing extra is shown
		// </summary>
		/********************************************************************/
		public override string ExtraFormatInfo => extraInfo;
		#endregion

		#region Loading
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
				module = new Module_Ext(fileInfo.ModuleStream, formatId);

				INostalgicPlayer nostalgicPlayer = (INostalgicPlayer)module.Get_Interface("nostalgicplayer");
				extraInfo = nostalgicPlayer.GetExtraInformation();
			}
			catch (Exception ex)
			{
				errorMessage = string.Format(Resources.IDS_MPT_ERR_LOADING, ex.Message);
				return AgentResult.Error;
			}

			return AgentResult.Ok;
		}



		/********************************************************************/
		// <summary>
		// Return a string containing a warning string. If there is no
		// warning, an empty string is returned
		// </summary>
		/********************************************************************/
/*		public override string GetWarning()
		{
			string[] dspEffectNames = libXmp.Xmp_Get_Used_Dsp_Effects();
			if (dspEffectNames != null)
				return string.Format(Resources.IDS_XMP_ERR_HAVE_DSP, string.Join("\n", dspEffectNames));

			return string.Empty;
		}*///XX
		#endregion

		#region Initialization and cleanup
		/********************************************************************/
		/// <summary>
		/// Initializes the player
		/// </summary>
		/********************************************************************/
		public override bool InitPlayer(out string errorMessage)
		{
			if (!base.InitPlayer(out errorMessage))
				return false;

			title = module.Get_Metadata("title");
			author = module.Get_Metadata("artist");
			comment = module.Get_Metadata("message_raw");

			int numberOfSongs = module.Get_Num_SubSongs();
			string[] subSongNames = module.Get_SubSong_Names().data().ToArray();

			subSongs = new SubSongInfo(numberOfSongs, 0, subSongNames);

			hasInstruments = module.Get_Num_Instruments() != 0;
			useSurround = DoesModuleUseSurround();

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the player
		/// </summary>
		/********************************************************************/
		public override void CleanupPlayer()
		{
			module = null;

			leftBuffer = null;
			rightBuffer = null;
			leftRearBuffer = null;
			rightRearBuffer = null;

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

			module.Select_SubSong(songNumber);
			module.Ctl_Set_Text("play.at_end", "continue");

			currentDuration = durations[songNumber];
			endReached = false;

			SetModuleInformation();

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Is only called if BufferDirect is set in the SupportFlags. It
		/// tells your player about the different mixer settings you need to
		/// take care of
		/// </summary>
		/********************************************************************/
		public override void ChangeMixerConfiguration(PlayerMixerInfo mixerInfo)
		{
			base.ChangeMixerConfiguration(mixerInfo);

			module.Set_Render_Param(Render_Param.StereoSeparation_Percent, mixerInfo.StereoSeparator);
			surroundMode = mixerInfo.SurroundMode;

			INostalgicPlayer nostalgicPlayer = (INostalgicPlayer)module.Get_Interface("nostalgicplayer");
			nostalgicPlayer.EnableSurround(surroundMode != SurroundMode.None);

			IInteractive interactive = (IInteractive)module.Get_Interface("interactive");

			for (int i = 0; i < ModuleChannelCount; i++)
			{
				bool channelEnabled = (mixerInfo.ChannelsEnabled != null) && (i < mixerInfo.ChannelsEnabled.Length) ? mixerInfo.ChannelsEnabled[i] : true;
				interactive.Set_Channel_Mute_Status(i, !channelEnabled);
			}
		}
		#endregion

		#region Playing
		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public override void Play()
		{
			PlayBuffer();

			int newPlayingPosition = module.Get_Current_Order();
			int newPlayingPattern = module.Get_Current_Pattern();
			int newCurrentSpeed = module.Get_Current_Speed();
			double newCurrentTempo = module.Get_Current_Tempo2();

			if (currentSpeed != newCurrentSpeed)
			{
				currentSpeed = newCurrentSpeed;

				ShowSpeed();
			}

			if (currentTempo != newCurrentTempo)
			{
				currentTempo = newCurrentTempo;

				ShowTempo();
			}

			if (playingPosition != newPlayingPosition)
			{
				playingPosition = newPlayingPosition;
				playingPattern = newPlayingPattern;

				ShowSongPosition();
				ShowPattern();
			}

			if (endReached)
			{
				OnEndReached();
				endReached = false;
			}
		}
		#endregion

		#region Information
		/********************************************************************/
		/// <summary>
		/// Return the title
		/// </summary>
		/********************************************************************/
		public override string Title => title;



		/********************************************************************/
		/// <summary>
		/// Return the name of the author
		/// </summary>
		/********************************************************************/
		public override string Author => author;



		/********************************************************************/
		/// <summary>
		/// Return the comment separated in lines
		/// </summary>
		/********************************************************************/
		public override string[] Comment => string.IsNullOrWhiteSpace(comment) ? Array.Empty<string>() : comment.Split('\n');



		/********************************************************************/
		/// <summary>
		/// Return information about sub-songs
		/// </summary>
		/********************************************************************/
		public override SubSongInfo SubSongs => subSongs;



		/********************************************************************/
		/// <summary>
		/// Return which speakers the player uses
		/// </summary>
		/********************************************************************/
		public override SpeakerFlag SpeakerFlags
		{
			get
			{
				SpeakerFlag flags = SpeakerFlag.FrontLeft | SpeakerFlag.FrontRight;

				if (useSurround && (surroundMode == SurroundMode.Real))
					flags |= SpeakerFlag.BackLeft | SpeakerFlag.BackRight;

				return flags;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the module use
		/// </summary>
		/********************************************************************/
		public override int ModuleChannelCount => module.Get_Num_Channels();



		/********************************************************************/
		// <summary>
		// Returns all the instruments available in the module. If none,
		// null is returned
		// </summary>
		/********************************************************************/
/*		public override IEnumerable<InstrumentInfo> Instruments
		{
			get
			{
				if (hasInstruments)
				{
					for (int i = 0; i < moduleInfo.Mod.Ins; i++)
					{
						Xmp_Instrument inst = moduleInfo.Mod.Xxi[i];

						InstrumentInfo instInfo = new InstrumentInfo
						{
							Name = inst.Name,
							Flags = InstrumentInfo.InstrumentFlags.None
						};

						// Fill out the note samples
						if (inst.Nsm > 0)
						{
							for (int j = 0; j < InstrumentInfo.Octaves; j++)
							{
								for (int k = 0; k < InstrumentInfo.NotesPerOctave; k++)
								{
									byte ins = inst.Map[j * InstrumentInfo.NotesPerOctave + k].Ins;
									instInfo.Notes[j, k] = (ins != 0xff) && (ins < inst.Nsm) ? inst.Sub[ins].Sid : -1;
								}
							}
						}

						yield return instInfo;
					}
				}
			}
		}*///XX



		/********************************************************************/
		/// <summary>
		/// Returns all the samples available in the module. If none, null
		/// is returned
		/// </summary>
		/********************************************************************/
		public override IEnumerable<SampleInfo> Samples
		{
			get
			{
				INostalgicPlayer nostalgicPlayer = (INostalgicPlayer)module.Get_Interface("nostalgicplayer");

				for (int i = 0; i < module.Get_Num_Samples(); i++)
				{
					SampleInformation sample = nostalgicPlayer.GetSampleInformation(i);

					SampleInfo sampleInfo = new SampleInfo
					{
						Name = sample.Name,
						Flags = SampleInfo.SampleFlag.None,
						Type = SampleInfo.SampleType.Sample,
						Volume = sample.Volume,
						Panning = sample.Panning,
						Sample = sample.SampleData,
						SampleOffset = sample.SampleOffset,
						Length = sample.Length,
						LoopStart = sample.LoopStart,
						LoopLength = sample.LoopLength
					};

					if ((sample.Flags & SampleInformation.SampleFlags._16Bit) != 0)
						sampleInfo.Flags |= SampleInfo.SampleFlag._16Bit;

					if ((sample.Flags & SampleInformation.SampleFlags.Stereo) != 0)
						sampleInfo.Flags |= SampleInfo.SampleFlag.Stereo;

					// Add extra loop flags if any
					if ((sample.Flags & SampleInformation.SampleFlags.Loop) != 0)
					{
						// Set loop flag
						sampleInfo.Flags |= SampleInfo.SampleFlag.Loop;

						// Is the loop ping-pong?
						if ((sample.Flags & SampleInformation.SampleFlags.PingPong) != 0)
							sampleInfo.Flags |= SampleInfo.SampleFlag.PingPong;
					}
					else
					{
						sampleInfo.LoopStart = 0;
						sampleInfo.LoopLength = 0;
					}

					// Set the frequency table
					sampleInfo.NoteFrequencies = sample.NoteFrequencies;

					yield return sampleInfo;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Returns the description and value on the line given. If the line
		/// is out of range, false is returned
		/// </summary>
		/********************************************************************/
		public override bool GetInformationString(int line, out string description, out string value)
		{
			// Find out which line to take
			switch (line)
			{
				// Number of positions
				case 0:
				{
					description = Resources.IDS_MPT_INFODESCLINE0;
					value = module.Get_Num_Orders().ToString();
					break;
				}

				// Used patterns
				case 1:
				{
					description = Resources.IDS_MPT_INFODESCLINE1;
					value = module.Get_Num_Patterns().ToString();
					break;
				}

				// Used instruments
				case 2:
				{
					description = Resources.IDS_MPT_INFODESCLINE2;
					value = module.Get_Num_Instruments().ToString();
					break;
				}

				// Used samples
				case 3:
				{
					description = Resources.IDS_MPT_INFODESCLINE3;
					value = module.Get_Num_Samples().ToString();
					break;
				}

				// Playing position
				case 4:
				{
					description = Resources.IDS_MPT_INFODESCLINE4;
					value = playingPosition.ToString();
					break;
				}

				// Playing pattern
				case 5:
				{
					description = Resources.IDS_MPT_INFODESCLINE5;
					value = playingPattern.ToString();
					break;
				}

				// Current speed
				case 6:
				{
					description = Resources.IDS_MPT_INFODESCLINE6;
					value = currentSpeed.ToString();
					break;
				}

				// Current tempo (BPM)
				case 7:
				{
					description = Resources.IDS_MPT_INFODESCLINE7;
					value = FormatTempo();
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



		/********************************************************************/
		/// <summary>
		/// Holds the channels used by visuals. Only needed for players using
		/// buffer mode if possible
		/// </summary>
		/********************************************************************/
		public override ChannelChanged[] VisualChannels
		{
			get
			{
				INostalgicPlayer nostalgicPlayer = (INostalgicPlayer)module.Get_Interface("nostalgicplayer");

				return nostalgicPlayer.GetVisualChannels();
			}
		}
		#endregion

		#region Duration calculation
		/********************************************************************/
		/// <summary>
		/// Calculate the duration for all sub-songs
		/// </summary>
		/********************************************************************/
		public DurationInfo[] CalculateDuration()
		{
			int subSongCount = module.Get_Num_SubSongs();
			durations = new DurationInfo[subSongCount];

			for (int i = 0; i < subSongCount; i++)
			{
				module.Select_SubSong(i);
				double duration = module.Get_Duration_Seconds();

				List<PositionInfo> positionList = new List<PositionInfo>();

				for (double time = 0.0; time < duration; time += IDuration.NumberOfSecondsBetweenEachSnapshot)
					positionList.Add(new PositionInfo(TimeSpan.FromSeconds(time)));

				int restartOrder = module.Get_Restart_Order(i);
				int restartRow = module.Get_Restart_Row(i);
				double restartTime = module.Get_Time_At_Position(restartOrder, restartRow);

				durations[i] = new DurationInfo(TimeSpan.FromSeconds(duration), positionList.ToArray(), Array.Empty<TimeSpan>(), TimeSpan.FromSeconds(restartTime));
			}

			return durations;
		}



		/********************************************************************/
		/// <summary>
		/// Will tell the player to change its current state to match the
		/// position given
		/// </summary>
		/********************************************************************/
		public void SetSongPosition(PositionInfo positionInfo)
		{
			module.Set_Position_Seconds(positionInfo.Time.TotalSeconds);

			UpdateModuleInformation();
		}



		/********************************************************************/
		/// <summary>
		/// Return the time into the song when restarting
		/// </summary>
		/********************************************************************/
		public TimeSpan GetRestartTime()
		{
			return currentDuration.RestartTime!.Value;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will set local variables to hold different information shown to
		/// the user
		/// </summary>
		/********************************************************************/
		private void SetModuleInformation()
		{
			playingPosition = module.Get_Current_Order();
			playingPattern = module.Get_Current_Pattern();
			currentSpeed = module.Get_Current_Speed();
			currentTempo = module.Get_Current_Tempo2();
		}



		/********************************************************************/
		/// <summary>
		/// Fill in buffers with audio data and play them
		/// </summary>
		/********************************************************************/
		private void PlayBuffer()
		{
			INostalgicPlayer nostalgicPlayer = (INostalgicPlayer)module.Get_Interface("nostalgicplayer");

			uint samplesPerTick = nostalgicPlayer.GetSamplesPerTick();
			bool realSurroundEnabled = VirtualChannels.Length == 4;

			if ((leftBuffer == null) || (leftBuffer.Length < samplesPerTick))
			{
				leftBuffer = new short[samplesPerTick];
				rightBuffer = new short[samplesPerTick];

				if (realSurroundEnabled)
				{
					leftRearBuffer = new short[samplesPerTick];
					rightRearBuffer = new short[samplesPerTick];
				}
			}

			if (realSurroundEnabled)
			{
				uint read = (uint)module.Read((int)mixerFreq, samplesPerTick, leftBuffer, rightBuffer, leftRearBuffer, rightRearBuffer);
				if (read == 0)
					endReached = true;
				else
				{
					VirtualChannels[0].PlayBuffer(leftBuffer, 0, read, PlayBufferFlag._16Bit);
					VirtualChannels[1].PlayBuffer(rightBuffer, 0, read, PlayBufferFlag._16Bit);
					VirtualChannels[2].PlayBuffer(leftRearBuffer, 0, read, PlayBufferFlag._16Bit);
					VirtualChannels[3].PlayBuffer(rightRearBuffer, 0, read, PlayBufferFlag._16Bit);
				}
			}
			else
			{
				uint read = (uint)module.Read((int)mixerFreq, samplesPerTick, leftBuffer, rightBuffer);
				if (read == 0)
					endReached = true;
				else
				{
					VirtualChannels[0].PlayBuffer(leftBuffer, 0, read, PlayBufferFlag._16Bit);
					VirtualChannels[1].PlayBuffer(rightBuffer, 0, read, PlayBufferFlag._16Bit);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Check if the module uses surround
		/// </summary>
		/********************************************************************/
		private bool DoesModuleUseSurround()
		{
			INostalgicPlayer nostalgicPlayer = (INostalgicPlayer)module.Get_Interface("nostalgicplayer");

			return nostalgicPlayer.DoesModuleUseSurround();
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current song position
		/// </summary>
		/********************************************************************/
		private void ShowSongPosition()
		{
			OnModuleInfoChanged(InfoPositionLine, playingPosition.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with pattern number
		/// </summary>
		/********************************************************************/
		private void ShowPattern()
		{
			OnModuleInfoChanged(InfoPatternLine, playingPattern.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current speed
		/// </summary>
		/********************************************************************/
		private void ShowSpeed()
		{
			OnModuleInfoChanged(InfoSpeedLine, currentSpeed.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with tempo
		/// </summary>
		/********************************************************************/
		private void ShowTempo()
		{
			OnModuleInfoChanged(InfoTempoLine, FormatTempo());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with all dynamic values
		/// </summary>
		/********************************************************************/
		private void UpdateModuleInformation()
		{
			ShowSongPosition();
			ShowPattern();
			ShowSpeed();
			ShowTempo();
		}



		/********************************************************************/
		/// <summary>
		/// Will format the tempo
		/// </summary>
		/********************************************************************/
		private string FormatTempo()
		{
			return ((uint)Math.Round(currentTempo, MidpointRounding.AwayFromZero)).ToString();
		}
		#endregion
	}
}
