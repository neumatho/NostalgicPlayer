/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.PumaTracker.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.PumaTracker
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class PumaTrackerWorker : ModulePlayerWithPositionDurationAgentBase
	{
		private string songName;
		private Position[] positions;
		private Track[][] tracks;
		private Instrument[] instruments;
		private byte[][] samples;

		private byte[][] allSamples;

		private GlobalPlayingInfo playingInfo;
		private VoiceInfo[] voices;

		private const int InfoPositionLine = 4;
		private const int InfoTrackLine = 5;
		private const int InfoSpeedLine = 6;

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => [ "puma" ];



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			if (moduleStream.Length < 80)
				return AgentResult.Unknown;

			// Find the first track and check the mark
			moduleStream.Seek(12, SeekOrigin.Begin);

			ushort numberOfTracks = moduleStream.Read_B_UINT16();
			int offset = (numberOfTracks + 1) * 14 + 80;

			if (offset >= moduleStream.Length)
				return AgentResult.Unknown;

			moduleStream.Seek(offset, SeekOrigin.Begin);

			if (moduleStream.Read_B_UINT32() == 0x70617474)		// patt
				return AgentResult.Ok;

			return AgentResult.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Return the name of the module
		/// </summary>
		/********************************************************************/
		public override string ModuleName => songName;



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
					description = Resources.IDS_PUMA_INFODESCLINE0;
					value = positions.Length.ToString();
					break;
				}

				// Used tracks
				case 1:
				{
					description = Resources.IDS_PUMA_INFODESCLINE1;
					value = tracks.Length.ToString();
					break;
				}

				// Used instruments
				case 2:
				{
					description = Resources.IDS_PUMA_INFODESCLINE2;
					value = instruments.Length.ToString();
					break;
				}

				// Supported / used samples
				case 3:
				{
					description = Resources.IDS_PUMA_INFODESCLINE3;
					value = "10";
					break;
				}

				// Playing position
				case 4:
				{
					description = Resources.IDS_PUMA_INFODESCLINE4;
					value = FormatPosition();
					break;
				}

				// Playing tracks
				case 5:
				{
					description = Resources.IDS_PUMA_INFODESCLINE5;
					value = FormatTracks();
					break;
				}

				// Current speed
				case 6:
				{
					description = Resources.IDS_PUMA_INFODESCLINE6;
					value = FormatSpeed();
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
		/// Will load the file into memory
		/// </summary>
		/********************************************************************/
		public override AgentResult Load(PlayerFileInfo fileInfo, out string errorMessage)
		{
			errorMessage = string.Empty;

			try
			{
				byte[] buf = new byte[13];

				ModuleStream moduleStream = fileInfo.ModuleStream;
				Encoding encoder = EncoderCollection.Amiga;

				// Read the song name
				buf[12] = 0x00;
				moduleStream.ReadInto(buf, 0, 12);

				songName = encoder.GetString(buf);

				// Read different counts
				int numberOfPositions = moduleStream.Read_B_UINT16() + 1;
				int numberOfTracks = moduleStream.Read_B_UINT16();
				int numberOfInstruments = moduleStream.Read_B_UINT16();

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_PUMA_ERR_LOADING_HEADER;
					return AgentResult.Error;
				}

				moduleStream.Seek(2, SeekOrigin.Current);

				uint[] sampleDataOffsets = new uint[10];
				ushort[] sampleLengths = new ushort[10];

				moduleStream.ReadArray_B_UINT32s(sampleDataOffsets, 0, 10);
				moduleStream.ReadArray_B_UINT16s(sampleLengths, 0, 10);

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_PUMA_ERR_LOADING_HEADER;
					return AgentResult.Error;
				}

				if (!LoadPositionList(moduleStream, numberOfPositions))
				{
					errorMessage = Resources.IDS_PUMA_ERR_LOADING_POSITION_LIST;
					return AgentResult.Error;
				}

				if (!LoadTracks(moduleStream, numberOfTracks))
				{
					errorMessage = Resources.IDS_PUMA_ERR_LOADING_TRACKS;
					return AgentResult.Error;
				}

				if (!LoadInstruments(moduleStream, numberOfInstruments))
				{
					errorMessage = Resources.IDS_PUMA_ERR_LOADING_INSTRUMENTINFO;
					return AgentResult.Error;
				}

				if (!LoadSampleData(moduleStream, sampleDataOffsets, sampleLengths))
				{
					errorMessage = Resources.IDS_PUMA_ERR_LOADING_SAMPLES;
					return AgentResult.Error;
				}
			}
			catch (Exception)
			{
				Cleanup();
				throw;
			}

			// Everything is loaded alright
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

			BuildSampleList();

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

			InitializeSound();

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public override void Play()
		{
			if (playingInfo.CurrentRowNumber == 32)
			{
				playingInfo.CurrentPosition++;

				if (playingInfo.CurrentPosition == positions.Length)
					playingInfo.CurrentPosition = 0;

				GetNextPosition();

				ShowPosition();
				ShowTracks();

				if (HasPositionBeenVisited(playingInfo.CurrentPosition))
					OnEndReached(playingInfo.CurrentPosition);

				MarkPositionAsVisited(playingInfo.CurrentPosition);
			}

			playingInfo.SpeedCounter--;

			if (playingInfo.SpeedCounter == 0)
			{
				playingInfo.CurrentRowNumber++;
				playingInfo.SpeedCounter = playingInfo.CurrentSpeed;

				for (int i = 0; i < 4; i++)
				{
					VoiceInfo voiceInfo = voices[i];

					voiceInfo.RowCounter--;

					if (voiceInfo.RowCounter == 0)
					{
						voiceInfo.TrackPosition++;

						GetNextTrackInfo(voiceInfo, VirtualChannels[i]);
					}
				}
			}

			for (int i = 0; i < 4; i++)
			{
				VoiceInfo voiceInfo = voices[i];

				if (voiceInfo.VoiceFlag.HasFlag(VoiceFlag.VoiceRunning))
					EffectHandler(voiceInfo, VirtualChannels[i]);
			}
		}



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
				// Build frequency table
				uint[] frequencies = new uint[10 * 12];

				for (int j = 0; j < 6 * 12; j++)
					frequencies[1 * 12 + j] = PeriodToFrequency(Tables.Periods[1 + j]);

				for (int i = 0; i < 10; i++)
				{
					SampleInfo sampleInfo = new SampleInfo
					{
						Name = string.Empty,
						Type = SampleInfo.SampleType.Sample,
						Flags = SampleInfo.SampleFlag.None,
						Volume = 256,
						Panning = -1,
						Sample = samples[i],
						Length = samples[i] != null ? (uint)samples[i].Length : 0,
						LoopStart = 0,
						LoopLength = 0,
						NoteFrequencies = frequencies
					};

					yield return sampleInfo;
				}

				for (int i = 0; i < Tables.Waveforms.Length; i++)
				{
					SampleInfo sampleInfo = new SampleInfo
					{
						Name = string.Empty,
						Type = SampleInfo.SampleType.Synthesis,
						Flags = SampleInfo.SampleFlag.None,
						Volume = 256,
						Panning = -1,
						Sample = null,
						Length = 0,
						LoopStart = 0,
						LoopLength = 0,
						NoteFrequencies = frequencies
					};

					yield return sampleInfo;
				}
			}
		}
		#endregion

		#region ModulePlayerWithPositionDurationAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Initialize all internal structures when beginning duration
		/// calculation on a new sub-song
		/// </summary>
		/********************************************************************/
		protected override int InitDuration(int songNumber, int startPosition)
		{
			InitializeSound();

			return startPosition;
		}



		/********************************************************************/
		/// <summary>
		/// Return the total number of positions
		/// </summary>
		/********************************************************************/
		protected override int GetTotalNumberOfPositions()
		{
			return positions.Length;
		}



		/********************************************************************/
		/// <summary>
		/// Create a snapshot of all the internal structures and return it
		/// </summary>
		/********************************************************************/
		protected override ISnapshot CreateSnapshot()
		{
			return new Snapshot(playingInfo, voices);
		}



		/********************************************************************/
		/// <summary>
		/// Initialize internal structures based on the snapshot given
		/// </summary>
		/********************************************************************/
		protected override bool SetSnapshot(ISnapshot snapshot, out string errorMessage)
		{
			errorMessage = string.Empty;

			// Start to make a clone of the snapshot
			Snapshot currentSnapshot = (Snapshot)snapshot;
			Snapshot clonedSnapshot = new Snapshot(currentSnapshot.PlayingInfo, currentSnapshot.Voices);

			playingInfo = clonedSnapshot.PlayingInfo;
			voices = clonedSnapshot.Voices;

			UpdateModuleInformation();

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Load the position list
		/// </summary>
		/********************************************************************/
		private bool LoadPositionList(ModuleStream moduleStream, int numberOfPositions)
		{
			positions = new Position[numberOfPositions];

			for (int i = 0; i < numberOfPositions; i++)
			{
				Position position = new Position();

				for (int j = 0; j < 4; j++)
				{
					VoicePosition voicePosition = new VoicePosition();

					voicePosition.TrackNumber = moduleStream.Read_UINT8();
					voicePosition.InstrumentTranspose = moduleStream.Read_INT8();
					voicePosition.NoteTranspose = moduleStream.Read_INT8();

					position.VoicePosition[j] = voicePosition;
				}

				position.Speed = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
					return false;

				moduleStream.Seek(1, SeekOrigin.Current);

				positions[i] = position;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the tracks
		/// </summary>
		/********************************************************************/
		private bool LoadTracks(ModuleStream moduleStream, int numberOfTracks)
		{
			tracks = new Track[numberOfTracks][];

			for (int i = 0; i < numberOfTracks; i++)
			{
				// Check mark
				if (moduleStream.Read_B_UINT32() != 0x70617474)     // patt
					return false;

				Track[] track = LoadSingleTrack(moduleStream);
				if (track == null)
					return false;

				tracks[i] = track;
			}

			// Check for extra mark
			if (moduleStream.Read_B_UINT32() != 0x70617474)     // patt
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single track
		/// </summary>
		/********************************************************************/
		private Track[] LoadSingleTrack(ModuleStream moduleStream)
		{
			List<Track> track = new List<Track>();

			for (int row = 0; row < 32;)
			{
				byte byt1 = moduleStream.Read_UINT8();
				byte byt2 = moduleStream.Read_UINT8();
				byte byt3 = moduleStream.Read_UINT8();
				byte byt4 = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
					return null;

				track.Add(new Track
				{
					Note = byt1,
					Instrument = (byte)(byt2 & 0x1f),
					Effect = (TrackEffect)((byt2 & 0xe0) >> 5),
					EffectArgument = byt3,
					RowsToWait = byt4
				});

				row += byt4;
			}

			return track.ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Load all the instruments
		/// </summary>
		/********************************************************************/
		private bool LoadInstruments(ModuleStream moduleStream, int numberOfInstruments)
		{
			instruments = new Instrument[numberOfInstruments];

			for (int i = 0; i < numberOfInstruments; i++)
			{
				Instrument instr = new Instrument();

				if (moduleStream.Read_B_UINT32() != 0x696e7374)     // inst
					return false;

				instr.VolumeCommands = LoadCommands(moduleStream);
				if (instr.VolumeCommands == null)
					return false;

				if (moduleStream.Read_B_UINT32() != 0x696e7366)     // insf
					return false;

				instr.FrequencyCommands = LoadCommands(moduleStream);
				if (instr.FrequencyCommands == null)
					return false;

				instruments[i] = instr;
			}

			// Check for extra mark, but only if there are samples
			if ((moduleStream.Position < moduleStream.Length) && (moduleStream.Read_B_UINT32() != 0x696e7374))     // inst
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load commands for an instrument
		/// </summary>
		/********************************************************************/
		private InstrumentCommand[] LoadCommands(ModuleStream moduleStream)
		{
			List<InstrumentCommand> commands = new List<InstrumentCommand>();

			for (;;)
			{
				InstrumentCommand cmd = new InstrumentCommand
				{
					Command = moduleStream.Read_UINT8(),
					Argument1 = moduleStream.Read_UINT8(),
					Argument2 = moduleStream.Read_UINT8(),
					Argument3 = moduleStream.Read_UINT8()
				};

				if (moduleStream.EndOfStream)
					return null;

				// Vimto does not have an end mark for some instruments, so check if reached a new instrument
				if ((cmd.Command == 'i') && (cmd.Argument1 == 'n') && (cmd.Argument2  == 's') && (cmd.Argument3 == 't'))
				{
					cmd = new InstrumentCommand
					{
						Command = 0xe0,
						Argument1 = 0,
						Argument2 = 0,
						Argument3 = 0
					};

					moduleStream.Seek(-4, SeekOrigin.Current);
				}

				commands.Add(cmd);

				if ((cmd.Command == 0xe0) || (cmd.Command == 0xb0))
					break;
			}

			return commands.ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Load all the samples
		/// </summary>
		/********************************************************************/
		private bool LoadSampleData(ModuleStream moduleStream, uint[] sampleDataOffsets, ushort[] sampleLengths)
		{
			samples = new byte[10][];

			for (int i = 0; i < 10; i++)
			{
				if (sampleLengths[i] != 0)
				{
					moduleStream.Seek(sampleDataOffsets[i], SeekOrigin.Begin);

					// Because of a bug, the sample length is one word too many
					int length = (sampleLengths[i] - 1) * 2;

					// Read the samples as unsigned, even when they are signed.
					// This is because the waveforms in Tables as unsigned as
					// this format is more read friendly
					using (ModuleStream sampleStream = moduleStream.GetSampleDataStream(i, length))
					{
						samples[i] = new byte[length];

						int readBytes = sampleStream.Read(samples[i], 0, length);
						if (readBytes < (length - 8))
							return false;
					}
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Build list with all the samples and waveforms
		/// </summary>
		/********************************************************************/
		private void BuildSampleList()
		{
			allSamples = new byte[10 + Tables.Waveforms.Length][];

			for (int i = 0; i < 10; i++)
				allSamples[i] = samples[i];

			for (int i = 0; i < Tables.Waveforms.Length; i++)
				allSamples[10 + i] = Tables.Waveforms[i];
		}



		/********************************************************************/
		/// <summary>
		/// Initialize sound structures
		/// </summary>
		/********************************************************************/
		private void InitializeSound()
		{
			playingInfo = new GlobalPlayingInfo
			{
				CurrentSpeed = 3,
				SpeedCounter = 3,
				CurrentPosition = -1,
				CurrentRowNumber = 32
			};

			voices = new VoiceInfo[4];

			for (int i = 0; i < 4; i++)
			{
				voices[i] = new VoiceInfo
				{
					Track = null,
					TrackPosition = 0,

					RowCounter = 1,

					VolumeCommands = null,
					VolumeCommandPosition = 0,

					FrequencyCommands = null,
					FrequencyCommandPosition = 0,

					InstrumentTranspose = 0,
					NoteTranspose = 0,
					TransposedNote = 0,

					SampleData = null,
					SampleLength = 0,
					SampleNumber = 0,

					Period = 0,
					Volume = 0,

					PortamentoAddValue = 0,

					VoiceFlag = VoiceFlag.None,

					VolumeSlideCounter = 0,
					VolumeSlideRemainingTime = 0,
					VolumeSlideDelta = 0,
					VolumeSlideDirection = 0,
					VolumeSlideValue = 0,

					FrequencyCounter = 0,
					FrequencyVaryAddValue = 0,
					FrequencyVaryValue = 0,

					WaveformChangeCounter = 0,
					WaveformAddValue = 0,
					WaveformValue = 0
				};
			}
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player have allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			songName = null;
			positions = null;
			tracks = null;
			instruments = null;
			samples = null;

			allSamples = null;
		}



		/********************************************************************/
		/// <summary>
		/// Will get the next position for all tracks
		/// </summary>
		/********************************************************************/
		private void GetNextPosition()
		{
			playingInfo.CurrentRowNumber = 0;

			Position position = positions[playingInfo.CurrentPosition];

			for (int i = 0; i < 4; i++)
			{
				VoiceInfo voiceInfo = voices[i];

				VoicePosition voicePosition = position.VoicePosition[i];
				byte trackNumber = voicePosition.TrackNumber;
				voiceInfo.Track = trackNumber >= tracks.Length ? Tables.EmptyTrack : tracks[trackNumber];
				voiceInfo.TrackPosition = -1;

				voiceInfo.InstrumentTranspose = voicePosition.InstrumentTranspose;
				voiceInfo.NoteTranspose = voicePosition.NoteTranspose;
				voiceInfo.RowCounter = 1;
			}

			if (position.Speed != 0)
			{
				playingInfo.CurrentSpeed = position.Speed;
				ShowSpeed();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will parse the next track bytes for the given voice
		/// </summary>
		/********************************************************************/
		private void GetNextTrackInfo(VoiceInfo voiceInfo, IChannel channel)
		{
			Track track = voiceInfo.Track[voiceInfo.TrackPosition];

			byte note = track.Note;
			if (note != 0)
			{
				channel.Mute();

				voiceInfo.TransposedNote = (byte)(note + voiceInfo.NoteTranspose);

				int instrNumber = track.Instrument + voiceInfo.InstrumentTranspose;
				Instrument instr = instrNumber == 0 ? Tables.EmptyInstrument : instruments[track.Instrument + voiceInfo.InstrumentTranspose - 1];

				voiceInfo.VolumeCommands = instr.VolumeCommands;
				voiceInfo.FrequencyCommands = instr.FrequencyCommands;

				voiceInfo.VoiceFlag = VoiceFlag.SetVolumeSlide | VoiceFlag.SetFrequency | VoiceFlag.VoiceRunning;
				voiceInfo.VolumeCommandPosition = 0;
				voiceInfo.FrequencyCommandPosition = 0;
			}

			voiceInfo.RowCounter = track.RowsToWait;

			switch (track.Effect)
			{
				case TrackEffect.None:
				{
					voiceInfo.Volume = 64;
					voiceInfo.PortamentoAddValue = 0;
					break;
				}

				case TrackEffect.SetVolume:
				{
					voiceInfo.Volume = track.EffectArgument;
					voiceInfo.PortamentoAddValue = 0;
					break;
				}

				case TrackEffect.PortamentoDown:
				{
					voiceInfo.Volume = 64;
					voiceInfo.PortamentoAddValue = track.EffectArgument;
					break;
				}

				case TrackEffect.PortamentoUp:
				{
					voiceInfo.Volume = 64;
					voiceInfo.PortamentoAddValue = (short)(-track.EffectArgument);
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will run instrument commands and do all real time effects
		/// </summary>
		/********************************************************************/
		private void EffectHandler(VoiceInfo voiceInfo, IChannel channel)
		{
			DoInstrumentVolumeCommands(voiceInfo);
			DoInstrumentFrequencyCommands(voiceInfo);
			SetupHardware(voiceInfo, channel);
		}



		/********************************************************************/
		/// <summary>
		/// Will parse the instrument volume commands
		/// </summary>
		/********************************************************************/
		private void DoInstrumentVolumeCommands(VoiceInfo voiceInfo)
		{
			bool oneMore;

			do
			{
				oneMore = false;

				InstrumentCommand command = voiceInfo.VolumeCommands[voiceInfo.VolumeCommandPosition];

				switch (command.Command)
				{
					// Volume slide
					case 0xa0:
					{
						oneMore = DoInstrumentVolumeSlide(command, voiceInfo);
						break;
					}

					// Set sample
					case 0xc0:
					{
						DoInstrumentSetSample(command, voiceInfo);
						oneMore = true;
						break;
					}

					// Goto
					case 0xb0:
					{
						voiceInfo.VolumeCommandPosition = (byte)(command.Argument1 / 4);
						oneMore = true;
						break;
					}

					// End
					default:
					{
						voiceInfo.VolumeSlideValue = 0;
						voiceInfo.VoiceFlag &= ~VoiceFlag.VoiceRunning;
						break;
					}
				}
			}
			while (oneMore);
		}



		/********************************************************************/
		/// <summary>
		/// Will handle the volume slide command
		/// </summary>
		/********************************************************************/
		private bool DoInstrumentVolumeSlide(InstrumentCommand command, VoiceInfo voiceInfo)
		{
			bool hasFlag = voiceInfo.VoiceFlag.HasFlag(VoiceFlag.SetVolumeSlide);
			voiceInfo.VoiceFlag &= ~VoiceFlag.SetVolumeSlide;

			if (hasFlag)
			{
				byte volumeBegin = command.Argument1;
				byte volumeEnd = command.Argument2;

				voiceInfo.VolumeSlideCounter = (byte)(command.Argument3 + 1);
				voiceInfo.VolumeSlideValue = volumeBegin;
				voiceInfo.VolumeSlideRemainingTime = 0;
				voiceInfo.VolumeSlideDirection = 1;

				int delta = volumeEnd - volumeBegin;
				if (delta < 0)
				{
					delta = -delta;
					voiceInfo.VolumeSlideDirection = -1;
				}

				voiceInfo.VolumeSlideDelta = (sbyte)delta;
			}
			else
			{
				voiceInfo.VolumeSlideCounter--;

				if (voiceInfo.VolumeSlideCounter == 0)
				{
					voiceInfo.VolumeCommandPosition++;
					voiceInfo.VoiceFlag |= VoiceFlag.SetVolumeSlide;

					return true;
				}

				byte time = command.Argument3;
				int timeCount = (voiceInfo.VolumeSlideRemainingTime + voiceInfo.VolumeSlideDelta) - time;

				if (timeCount >= 0)
				{
					short valueToAdd = 0;

					do
					{
						valueToAdd += voiceInfo.VolumeSlideDirection;
						timeCount -= time;
					}
					while (timeCount >= 0);

					voiceInfo.VolumeSlideValue += valueToAdd;
				}

				timeCount += time;
				voiceInfo.VolumeSlideRemainingTime = (byte)timeCount;
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Will handle the set sample command
		/// </summary>
		/********************************************************************/
		private void DoInstrumentSetSample(InstrumentCommand command, VoiceInfo voiceInfo)
		{
			byte sampleNumber = command.Argument1;

			voiceInfo.WaveformAddValue = (sbyte)command.Argument2;
			voiceInfo.WaveformValue = (sbyte)command.Argument2;
			voiceInfo.WaveformChangeCounter = command.Argument3;

			byte[] sampleData = allSamples[sampleNumber];

			ushort sampleLength = (ushort)(sampleData?.Length ?? 0);
			voiceInfo.SampleLength = sampleLength;

			if (sampleLength >= 80 * 2)
				voiceInfo.VoiceFlag |= VoiceFlag.NoLoop;

			voiceInfo.SampleData = sampleData;
			voiceInfo.SampleNumber = sampleNumber;

			voiceInfo.VolumeCommandPosition++;
			voiceInfo.VoiceFlag |= VoiceFlag.SetVolumeSlide;
		}



		/********************************************************************/
		/// <summary>
		/// Will parse the instrument frequency commands
		/// </summary>
		/********************************************************************/
		private void DoInstrumentFrequencyCommands(VoiceInfo voiceInfo)
		{
			bool oneMore;

			do
			{
				oneMore = false;

				InstrumentCommand command = voiceInfo.FrequencyCommands[voiceInfo.FrequencyCommandPosition];

				switch (command.Command)
				{
					// Very
					case 0xa0:
					{
						oneMore = DoInstrumentVaryFrequency(command, voiceInfo);
						break;
					}

					// Constant
					case 0xd0:
					{
						oneMore = DoInstrumentConstantFrequency(command, voiceInfo);
						break;
					}

					// Goto
					case 0xb0:
					{
						voiceInfo.FrequencyCommandPosition = (byte)(command.Argument1 / 4);
						oneMore = true;
						break;
					}
				}
			}
			while (oneMore);
		}



		/********************************************************************/
		/// <summary>
		/// Will handle the frequency vary command
		/// </summary>
		/********************************************************************/
		private bool DoInstrumentVaryFrequency(InstrumentCommand command, VoiceInfo voiceInfo)
		{
			bool hasFlag = voiceInfo.VoiceFlag.HasFlag(VoiceFlag.SetFrequency);
			voiceInfo.VoiceFlag &= ~VoiceFlag.SetFrequency;

			if (hasFlag)
			{
				short beginAdd = (sbyte)command.Argument1;
				short endAdd = (sbyte)command.Argument2;

				voiceInfo.FrequencyCounter = command.Argument3;

				ushort period = Tables.Periods[voiceInfo.TransposedNote / 2];
				beginAdd += (short)period;
				endAdd += (short)period;

				voiceInfo.Period = (ushort)beginAdd;
				voiceInfo.FrequencyVaryValue = beginAdd << 16;

				short delta = (short)(endAdd - beginAdd);
				int addValue = delta * 256 / voiceInfo.FrequencyCounter;

				if (addValue < 65536)
					voiceInfo.FrequencyVaryAddValue = addValue << 8;
				else
				{
					addValue = delta / voiceInfo.FrequencyCounter;
					voiceInfo.FrequencyVaryAddValue = addValue << 16;
				}
			}
			else
			{
				voiceInfo.FrequencyCounter--;

				if (voiceInfo.FrequencyCounter == 0)
				{
					voiceInfo.FrequencyCommandPosition++;
					voiceInfo.VoiceFlag |= VoiceFlag.SetFrequency;

					return true;
				}

				voiceInfo.FrequencyVaryValue += voiceInfo.FrequencyVaryAddValue;
				voiceInfo.Period = (ushort)(voiceInfo.FrequencyVaryValue >> 16);
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Will handle the frequency constant command
		/// </summary>
		/********************************************************************/
		private bool DoInstrumentConstantFrequency(InstrumentCommand command, VoiceInfo voiceInfo)
		{
			bool hasFlag = voiceInfo.VoiceFlag.HasFlag(VoiceFlag.SetFrequency);
			voiceInfo.VoiceFlag &= ~VoiceFlag.SetFrequency;

			if (hasFlag)
			{
				voiceInfo.FrequencyCounter = command.Argument3;

				int index = ((sbyte)command.Argument1 + voiceInfo.TransposedNote) / 2;
				index = Math.Min(index, Tables.Periods.Length - 1);
				voiceInfo.Period = Tables.Periods[index];
			}
			else
			{
				voiceInfo.FrequencyCounter--;

				if (voiceInfo.FrequencyCounter == 0)
				{
					voiceInfo.VoiceFlag |= VoiceFlag.SetFrequency;
					voiceInfo.FrequencyCommandPosition++;

					return true;
				}
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Will tell NostalgicPlayer what to play
		/// </summary>
		/********************************************************************/
		private void SetupHardware(VoiceInfo voiceInfo, IChannel channel)
		{
			voiceInfo.Period = (ushort)(voiceInfo.Period + voiceInfo.PortamentoAddValue);

			byte[] sampleData = voiceInfo.SampleData;

			if (voiceInfo.WaveformAddValue != 0)
			{
				if ((voiceInfo.WaveformValue <= 0) || (voiceInfo.WaveformValue >= voiceInfo.WaveformChangeCounter))
					voiceInfo.WaveformAddValue = (sbyte)-voiceInfo.WaveformAddValue;

				voiceInfo.WaveformValue = (sbyte)(voiceInfo.WaveformValue + voiceInfo.WaveformAddValue);

				sampleData = allSamples[voiceInfo.SampleNumber + voiceInfo.WaveformValue];
			}

			if (sampleData != null)
			{
				if (voiceInfo.SampleLength > 2)
				{
					channel.SetSample(sampleData, 0, voiceInfo.SampleLength);
					channel.SetSampleNumber(voiceInfo.SampleNumber);

					bool hasFlag = voiceInfo.VoiceFlag.HasFlag(VoiceFlag.NoLoop);
					voiceInfo.VoiceFlag &= ~VoiceFlag.NoLoop;

					if (!hasFlag && (voiceInfo.SampleLength > 2))
						channel.SetLoop(0, voiceInfo.SampleLength);
					else
						voiceInfo.SampleLength = 2;
				}

				channel.SetAmigaPeriod(voiceInfo.Period);

				int volume = voiceInfo.VolumeSlideValue + voiceInfo.Volume - 64;
				if (volume < 0)
					volume = 0;

				channel.SetAmigaVolume((ushort)volume);
			}
			else
				channel.Mute();
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current position
		/// </summary>
		/********************************************************************/
		private void ShowPosition()
		{
			OnModuleInfoChanged(InfoPositionLine, FormatPosition());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with track numbers
		/// </summary>
		/********************************************************************/
		private void ShowTracks()
		{
			OnModuleInfoChanged(InfoTrackLine, FormatTracks());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current speed
		/// </summary>
		/********************************************************************/
		private void ShowSpeed()
		{
			OnModuleInfoChanged(InfoSpeedLine, FormatSpeed());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with all dynamic values
		/// </summary>
		/********************************************************************/
		private void UpdateModuleInformation()
		{
			ShowPosition();
			ShowTracks();
			ShowSpeed();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the playing position
		/// </summary>
		/********************************************************************/
		private string FormatPosition()
		{
			if (playingInfo.CurrentPosition < 0)
				return "0";

			return playingInfo.CurrentPosition.ToString();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the playing tracks
		/// </summary>
		/********************************************************************/
		private string FormatTracks()
		{
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < 4; i++)
			{
				byte trackNumber = playingInfo.CurrentPosition < 0 ? (byte)0 : positions[playingInfo.CurrentPosition].VoicePosition[i].TrackNumber;
				sb.Append(trackNumber);

				sb.Append(", ");
			}

			sb.Remove(sb.Length - 2, 2);

			return sb.ToString();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the current speed
		/// </summary>
		/********************************************************************/
		private string FormatSpeed()
		{
			return playingInfo.CurrentSpeed.ToString();
		}
		#endregion
	}
}
