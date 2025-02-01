/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.HivelyTracker.Containers;
using Polycode.NostalgicPlayer.Agent.Player.HivelyTracker.Implementation;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.HivelyTracker
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class HivelyTrackerWorker : ModulePlayerWithPositionDurationAgentBase
	{
		private static readonly Dictionary<Guid, ModuleType> moduleTypeLookup = new Dictionary<Guid, ModuleType>
		{
			{ HivelyTracker.Agent1Id, ModuleType.Ahx1 },
			{ HivelyTracker.Agent2Id, ModuleType.Ahx2 },
			{ HivelyTracker.Agent3Id, ModuleType.HivelyTracker }
		};

		#region VisualizerChannel class
		private class VisualizerChannel
		{
			public bool Muted;
			public bool NoteKicked;
			public short SampleNumber;
			public int? SamplePosition;
			public ushort? Volume;
			public uint? Frequency;
		}
		#endregion

		private readonly ModuleType currentModuleType;

		private HvlWaves waves;

		private HvlSong song;

		private GlobalPlayingInfo playingInfo;
		private HvlVoice[] voices;

		private short[] leftBuffer;
		private short[] rightBuffer;

		private int stereoSeparation;
		private bool[] enabledChannels;
		private MixerInfo lastMixerInfo;

		private VisualizerChannel[] visualizerChannels;

		private bool endReached;
		private bool restartSong;

		private const int InfoPositionLine = 3;
		private const int InfoTrackLine = 4;
		private const int InfoSpeedLine = 5;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public HivelyTrackerWorker(Guid typeId)
		{
			if (!moduleTypeLookup.TryGetValue(typeId, out currentModuleType))
				currentModuleType = ModuleType.Unknown;
		}

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => [ "ahx", "thx", "hvl" ];



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			// Check the module
			ModuleType checkType = TestModule(fileInfo);
			if (checkType == currentModuleType)
				return AgentResult.Ok;

			return AgentResult.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Return the name of the module
		/// </summary>
		/********************************************************************/
		public override string ModuleName => song.Name;



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
					description = Resources.IDS_HVL_INFODESCLINE0;
					value = song.PositionNr.ToString();
					break;
				}

				// Used tracks
				case 1:
				{
					description = Resources.IDS_HVL_INFODESCLINE1;
					value = (song.TrackNr + 1).ToString();
					break;
				}

				// Used instruments
				case 2:
				{
					description = Resources.IDS_HVL_INFODESCLINE2;
					value = song.InstrumentNr.ToString();
					break;
				}

				// Playing position
				case 3:
				{
					description = Resources.IDS_HVL_INFODESCLINE3;
					value = playingInfo.PosNr.ToString();
					break;
				}

				// Playing tracks
				case 4:
				{
					description = Resources.IDS_HVL_INFODESCLINE4;
					value = FormatTracks();
					break;
				}

				// Current speed
				case 5:
				{
					description = Resources.IDS_HVL_INFODESCLINE5;
					value = playingInfo.Tempo.ToString();
					break;
				}

				// Current tempo (Hz):
				case 6:
				{
					description = Resources.IDS_HVL_INFODESCLINE6;
					value = PlayingFrequency.ToString("F2", CultureInfo.InvariantCulture);
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
		public override ModulePlayerSupportFlag SupportFlags => base.SupportFlags | ModulePlayerSupportFlag.BufferMode | ModulePlayerSupportFlag.BufferDirect | ModulePlayerSupportFlag.Visualize | ModulePlayerSupportFlag.EnableChannels;



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
				ModuleStream moduleStream = fileInfo.ModuleStream;

				// Allocate the song
				song = new HvlSong();

				// Skip mark and song title offset
				moduleStream.Seek(6, SeekOrigin.Begin);

				//
				// Header
				//
				byte flag = moduleStream.Read_UINT8();
				song.SpeedMultiplier = currentModuleType == ModuleType.Ahx1 ? 1 : ((flag >> 5) & 3) + 1;

				song.PositionNr = ((flag & 0xf) << 8) | moduleStream.Read_UINT8();

				ushort restart = moduleStream.Read_B_UINT16();

				if (currentModuleType == ModuleType.HivelyTracker)
				{
					song.Channels = (restart >> 10) + 4;
					song.Restart = restart & 0x03ff;
				}
				else
				{
					song.Channels = 4;
					song.Restart = restart;
				}

				song.TrackLength = moduleStream.Read_UINT8();
				song.TrackNr = moduleStream.Read_UINT8();
				song.InstrumentNr = moduleStream.Read_UINT8();
				byte subSongNr = moduleStream.Read_UINT8();

				if (currentModuleType == ModuleType.HivelyTracker)
				{
					song.MixGain = (moduleStream.Read_UINT8() << 8) / 100;
					song.DefaultStereo = moduleStream.Read_UINT8();
				}
				else
				{
					song.DefaultStereo = 4;
					song.MixGain = (100 * 256) / 100;
				}

				song.DefaultPanningLeft = Tables.StereoPanLeft[song.DefaultStereo];
				song.DefaultPanningRight = Tables.StereoPanRight[song.DefaultStereo];

				// Validate header values
				if ((song.PositionNr < 1) || (song.PositionNr > 999))
				{
					errorMessage = Resources.IDS_HVL_ERR_CORRUPT_HEADER;
					Cleanup();

					return AgentResult.Error;
				}

				if ((song.Restart < 0) || (song.Restart >= song.PositionNr))
				{
					errorMessage = Resources.IDS_HVL_ERR_CORRUPT_HEADER;
					Cleanup();

					return AgentResult.Error;
				}

				if ((song.TrackLength < 1) || (song.TrackLength > 64))
				{
					errorMessage = Resources.IDS_HVL_ERR_CORRUPT_HEADER;
					Cleanup();

					return AgentResult.Error;
				}

				if ((song.InstrumentNr < 0) || (song.InstrumentNr > 63))
				{
					errorMessage = Resources.IDS_HVL_ERR_CORRUPT_HEADER;
					Cleanup();

					return AgentResult.Error;
				}

				// Read sub-songs (we just skip the start positions, because we have our own algorithm to find them)
				moduleStream.Seek(subSongNr * 2, SeekOrigin.Current);

				// Read position list
				song.Positions = new HvlPosition[song.PositionNr];

				for (int i = 0; i < song.PositionNr; i++)
				{
					song.Positions[i] = new HvlPosition
					{
						Track = new int[song.Channels],
						Transpose = new int[song.Channels]
					};

					for (int j = 0; j < song.Channels; j++)
					{
						song.Positions[i].Track[j] = moduleStream.Read_UINT8();
						song.Positions[i].Transpose[j] = moduleStream.Read_INT8();
					}
				}

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_HVL_ERR_LOADING_HEADER;
					Cleanup();

					return AgentResult.Error;
				}

				//
				// Tracks
				//
				int maxTrack = song.TrackNr;
				song.Tracks = new HvlStep[maxTrack + 1][];

				for (int i = 0; i <= maxTrack; i++)
				{
					song.Tracks[i] = ArrayHelper.InitializeArray<HvlStep>(song.TrackLength);

					// Check if track 0 has been saved in the file. If not, it means it is an empty track
					if (((flag & 0x80) == 0x80) && (i == 0))
					{
						for (int j = 0; j < song.TrackLength; j++)
						{
							song.Tracks[i][j].Note = 0;
							song.Tracks[i][j].Instrument = 0;
							song.Tracks[i][j].Fx = 0;
							song.Tracks[i][j].FxParam = 0;
							song.Tracks[i][j].FxB = 0;
							song.Tracks[i][j].FxBParam = 0;
						}
					}
					else
					{
						for (int j = 0; j < song.TrackLength; j++)
						{
							if (currentModuleType == ModuleType.HivelyTracker)
							{
								byte byte1 = moduleStream.Read_UINT8();

								if (byte1 == 0x3f)
								{
									song.Tracks[i][j].Note = 0;
									song.Tracks[i][j].Instrument = 0;
									song.Tracks[i][j].Fx = 0;
									song.Tracks[i][j].FxParam = 0;
									song.Tracks[i][j].FxB = 0;
									song.Tracks[i][j].FxBParam = 0;
								}
								else
								{
									byte byte2 = moduleStream.Read_UINT8();
									byte byte3 = moduleStream.Read_UINT8();
									byte byte4 = moduleStream.Read_UINT8();
									byte byte5 = moduleStream.Read_UINT8();

									song.Tracks[i][j].Note = byte1;
									song.Tracks[i][j].Instrument = byte2;
									song.Tracks[i][j].Fx = byte3 >> 4;
									song.Tracks[i][j].FxParam = byte4;
									song.Tracks[i][j].FxB = byte3 & 0xf;
									song.Tracks[i][j].FxBParam = byte5;
								}
							}
							else
							{
								// Read the 3 track bytes
								byte byte1 = moduleStream.Read_UINT8();
								byte byte2 = moduleStream.Read_UINT8();
								byte byte3 = moduleStream.Read_UINT8();

								song.Tracks[i][j].Note = (byte1 >> 2) & 0x3f;
								song.Tracks[i][j].Instrument = ((byte1 & 0x3) << 4) | (byte2 >> 4);
								song.Tracks[i][j].Fx = byte2 & 0xf;
								song.Tracks[i][j].FxParam = byte3;
								song.Tracks[i][j].FxB = 0;
								song.Tracks[i][j].FxBParam = 0;
							}
						}

						if (moduleStream.EndOfStream)
						{
							errorMessage = Resources.IDS_HVL_ERR_LOADING_TRACKS;
							Cleanup();

							return AgentResult.Error;
						}
					}
				}

				//
				// Instruments
				//
				song.Instruments = ArrayHelper.InitializeArray<HvlInstrument>(song.InstrumentNr + 1);

				for (int i = 1; i <= song.InstrumentNr; i++)
				{
					song.Instruments[i].Volume = moduleStream.Read_UINT8();

					byte byte1 = moduleStream.Read_UINT8();
					song.Instruments[i].WaveLength = byte1 & 0x7;
					song.Instruments[i].Envelope.AFrames = moduleStream.Read_UINT8();
					song.Instruments[i].Envelope.AVolume = moduleStream.Read_UINT8();
					song.Instruments[i].Envelope.DFrames = moduleStream.Read_UINT8();
					song.Instruments[i].Envelope.DVolume = moduleStream.Read_UINT8();
					song.Instruments[i].Envelope.SFrames = moduleStream.Read_UINT8();
					song.Instruments[i].Envelope.RFrames = moduleStream.Read_UINT8();
					song.Instruments[i].Envelope.RVolume = moduleStream.Read_UINT8();

					moduleStream.Seek(3, SeekOrigin.Current);

					byte byte12 = moduleStream.Read_UINT8();

					if (currentModuleType == ModuleType.Ahx1)
					{
						song.Instruments[i].FilterSpeed = 0;
						song.Instruments[i].FilterLowerLimit = 0;
					}
					else
					{
						song.Instruments[i].FilterSpeed = ((byte1 >> 3) & 0x1f) | ((byte12 >> 2) & 0x20);
						song.Instruments[i].FilterLowerLimit = byte12 & 0x7f;
					}

					song.Instruments[i].VibratoDelay = moduleStream.Read_UINT8();

					byte byte14 = moduleStream.Read_UINT8();

					if (currentModuleType == ModuleType.Ahx1)
					{
						song.Instruments[i].HardCutReleaseFrames = 0;
						song.Instruments[i].HardCutRelease = false;
					}
					else
					{
						song.Instruments[i].HardCutReleaseFrames = (byte14 >> 4) & 7;
						song.Instruments[i].HardCutRelease = (byte14 & 0x80) != 0;
					}

					song.Instruments[i].VibratoDepth = byte14 & 0xf;
					song.Instruments[i].VibratoSpeed = moduleStream.Read_UINT8();
					song.Instruments[i].SquareLowerLimit = moduleStream.Read_UINT8();
					song.Instruments[i].SquareUpperLimit = moduleStream.Read_UINT8();
					song.Instruments[i].SquareSpeed = moduleStream.Read_UINT8();

					byte byte19 = moduleStream.Read_UINT8();

					if (currentModuleType == ModuleType.Ahx1)
						song.Instruments[i].FilterUpperLimit = 0;
					else
					{
						song.Instruments[i].FilterSpeed |= ((byte19 >> 1) & 0x40);
						song.Instruments[i].FilterUpperLimit = byte19 & 0x3f;
					}

					song.Instruments[i].PlayList.Speed = moduleStream.Read_UINT8();
					song.Instruments[i].PlayList.Length = moduleStream.Read_UINT8();

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_HVL_ERR_LOADING_INSTRUMENTS;
						Cleanup();

						return AgentResult.Error;
					}

					// Load play list
					song.Instruments[i].PlayList.Entries = new HvlPListEntry[song.Instruments[i].PlayList.Length];

					for (int j = 0; j < song.Instruments[i].PlayList.Length; j++)
					{
						byte1 = moduleStream.Read_UINT8();
						byte byte2 = moduleStream.Read_UINT8();
						byte byte3 = moduleStream.Read_UINT8();
						byte byte4 = moduleStream.Read_UINT8();

						if (currentModuleType == ModuleType.HivelyTracker)
						{
							byte byte5 = moduleStream.Read_UINT8();

							song.Instruments[i].PlayList.Entries[j] = new HvlPListEntry
							{
								Fx = new [] { byte1 & 0xf, (byte2 >> 3) & 0xf },
								Waveform = byte2 & 7,
								Fixed = ((byte3 >> 6) & 1) != 0,
								Note = byte3 & 0x3f,
								FxParam = new int[] { byte4, byte5 }
							};
						}
						else
						{
							int fx1 = (byte1 >> 2) & 7;
							fx1 = fx1 == 6 ? 12 : fx1 == 7 ? 15 :fx1;
							int fx2 = (byte1 >> 5) & 7;
							fx2 = fx2 == 6 ? 12 : fx2 == 7 ? 15 :fx2;

							song.Instruments[i].PlayList.Entries[j] = new HvlPListEntry
							{
								Fx = new [] { fx1, fx2 },
								Waveform = ((byte1 << 1) & 6) | (byte2 >> 7),
								Fixed = ((byte2 >> 6) & 1) != 0,
								Note = byte2 & 0x3f,
								FxParam = new int[] { byte3, byte4 }
							};
						}
					}

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_HVL_ERR_LOADING_INSTRUMENTS;
						Cleanup();

						return AgentResult.Error;
					}
				}

				//
				// Strings
				//
				Encoding encoder = EncoderCollection.Amiga;

				// Read song title
				song.Name = moduleStream.ReadLine(encoder);

				// Read the instrument strings
				for (int i = 1; i <= song.InstrumentNr; i++)
					song.Instruments[i].Name = moduleStream.ReadLine(encoder);
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

			// Allocate helper classes
			waves = new HvlWaves();

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

			// Initialize the player
			InitializeSound(0);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Set the output frequency and number of channels
		/// </summary>
		/********************************************************************/
		public override void SetOutputFormat(uint mixerFrequency, int channels)
		{
			base.SetOutputFormat(mixerFrequency, channels);

			long bufferSize = mixerFrequency / 50 / song.SpeedMultiplier;

			leftBuffer = new short[bufferSize];
			rightBuffer = new short[bufferSize];
		}



		/********************************************************************/
		/// <summary>
		/// Is only called if BufferDirect is set in the SupportFlags. It
		/// tells your player about the different mixer settings you need to
		/// take care of
		/// </summary>
		/********************************************************************/
		public override void ChangeMixerConfiguration(MixerInfo mixerInfo)
		{
			base.ChangeMixerConfiguration(mixerInfo);

			stereoSeparation = mixerInfo.StereoSeparator;

			for (int i = 0; i < song.Channels; i++)
				enabledChannels[i] = (mixerInfo.ChannelsEnabled != null) && (i < mixerInfo.ChannelsEnabled.Length) ? mixerInfo.ChannelsEnabled[i] : true;

			lastMixerInfo = mixerInfo;
		}



		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public override void Play()
		{
			PlayIrq();
			MixChunk();
			PlayBuffer();

			if (endReached)
			{
				OnEndReached(playingInfo.PosNr);
				endReached = false;

				if (restartSong)
				{
					RestartSong();
					restartSong = false;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the module use
		/// </summary>
		/********************************************************************/
		public override int ModuleChannelCount => song.Channels;



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

				for (int j = 0; j < 5 * 12; j++)
					frequencies[2 * 12 + j] = PeriodToFrequency((ushort)Tables.PeriodTable[j + 1]);

				for (int i = 1; i <= song.InstrumentNr; i++)
				{
					HvlInstrument inst = song.Instruments[i];

					yield return new SampleInfo
					{
						Name = inst.Name,
						Flags = SampleInfo.SampleFlag.None,
						Type = SampleInfo.SampleType.Synthesis,
						Volume = (ushort)(inst.Volume * 4),
						Panning = -1,
						Sample = null,
						Length = 0,
						LoopStart = 0,
						LoopLength = 0,
						NoteFrequencies = frequencies
					};
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Holds the channels used by visuals. Only needed for players using
		/// buffer mode if possible
		/// </summary>
		/********************************************************************/
		public override ChannelChanged[] VisualChannels => visualizerChannels.Select((x, i) =>
				new ChannelChanged(enabledChannels[i], x.Muted, x.NoteKicked, x.SampleNumber, -1, -1, 0x280, true, false, x.SamplePosition, x.Volume, x.Frequency)).ToArray();
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
			InitializeSound(startPosition);

			return startPosition;
		}



		/********************************************************************/
		/// <summary>
		/// Return the total number of positions
		/// </summary>
		/********************************************************************/
		protected override int GetTotalNumberOfPositions()
		{
			return song.PositionNr;
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
			Snapshot clonedSnapshot = new Snapshot(currentSnapshot.PlayingInfo, currentSnapshot.Channels);

			playingInfo = clonedSnapshot.PlayingInfo;
			voices = clonedSnapshot.Channels;

			for (int i = 0; i < 4; i++)
			{
				HvlVoice voice = voices[i];

				voice.PlantPeriod = true;
				voice.AudioPeriod = voices[i].VoicePeriod;
				voice.NewWaveform = true;
			}

			if (lastMixerInfo != null)
				ChangeMixerConfiguration(lastMixerInfo);

			UpdateModuleInformation();

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Tests the module to see which type of module it is
		/// </summary>
		/********************************************************************/
		private ModuleType TestModule(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			if (moduleStream.Length < 14)
				return ModuleType.Unknown;

			// Read the module mark
			moduleStream.Seek(0, SeekOrigin.Begin);
			uint mark = moduleStream.Read_B_UINT32();

			// Check the mark
			if (mark == 0x54485800)					// THX\0
				return ModuleType.Ahx1;

			if (mark == 0x54485801)					// THX\1
				return ModuleType.Ahx2;

			if ((mark == 0x48564c00) || (mark == 0x48564c01))	// HVL\0 or HVL\1
				return ModuleType.HivelyTracker;

			return ModuleType.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize sound structures
		/// </summary>
		/********************************************************************/
		private void InitializeSound(int startPosition)
		{
			playingInfo = new GlobalPlayingInfo
			{
				PosNr = startPosition,
				PosJump = 0,
				PatternBreak = false,
				NoteNr = 0,
				PosJumpNote = 0,
				Tempo = 6,
				StepWaitFrames = 0,
				GetNewPosition = true,
				SquareWaveform = null
			};

			endReached = false;
			restartSong = false;

			voices = ArrayHelper.InitializeArray<HvlVoice>(song.Channels);

			for (int v = 0; v < song.Channels; v++)
				voices[v].Init(v, song, waves.PanningLeft, waves.PanningRight);

			PlayingFrequency = 50 * song.SpeedMultiplier;

			enabledChannels = new bool[song.Channels];
			visualizerChannels = ArrayHelper.InitializeArray<VisualizerChannel>(song.Channels);
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player have allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			waves = null;
			song = null;

			playingInfo = null;
			voices = null;
		}



		/********************************************************************/
		/// <summary>
		/// This is the main play method
		/// </summary>
		/********************************************************************/
		private void PlayIrq()
		{
			if (playingInfo.StepWaitFrames <= 0)
			{
				if (playingInfo.GetNewPosition)
				{
					int nextPos = playingInfo.PosNr + 1 == song.PositionNr ? song.Restart : playingInfo.PosNr + 1;

					for (int i = 0; i < song.Channels; i++)
					{
						voices[i].Track = song.Positions[playingInfo.PosNr].Track[i];
						voices[i].Transpose = song.Positions[playingInfo.PosNr].Transpose[i];
						voices[i].NextTrack = song.Positions[nextPos].Track[i];
						voices[i].NextTranspose = song.Positions[nextPos].Transpose[i];
					}

					playingInfo.GetNewPosition = false;

					ShowSongPosition();
					ShowTracks();

					if (HasPositionBeenVisited(playingInfo.PosNr))
						endReached = true;

					MarkPositionAsVisited(playingInfo.PosNr);
				}

				for (int i = 0; i < song.Channels; i++)
					ProcessStep(i);

				playingInfo.StepWaitFrames = playingInfo.Tempo;
			}

			// Do frame stuff
			for (int i = 0; i < song.Channels; i++)
				ProcessFrame(i);

			if ((playingInfo.Tempo > 0) && (--playingInfo.StepWaitFrames <= 0))
			{
				if (!playingInfo.PatternBreak)
				{
					playingInfo.NoteNr++;

					if (playingInfo.NoteNr >= song.TrackLength)
					{
						playingInfo.PosJump = playingInfo.PosNr + 1;
						playingInfo.PosJumpNote = 0;
						playingInfo.PatternBreak = true;
					}
				}

				if (playingInfo.PatternBreak)
				{
					playingInfo.PatternBreak = false;
					playingInfo.NoteNr = playingInfo.PosJumpNote;
					playingInfo.PosJumpNote = 0;
					playingInfo.PosNr = playingInfo.PosJump;
					playingInfo.PosJump = 0;

					if (playingInfo.PosNr == song.PositionNr)
						playingInfo.PosNr = song.Restart;

					playingInfo.GetNewPosition = true;
				}
			}

			for (int a = 0; a < song.Channels; a++)
				SetAudio(a);
		}



		/********************************************************************/
		/// <summary>
		/// Parse the next step
		/// </summary>
		/********************************************************************/
		private void ProcessStep(int v)
		{
			HvlVoice voice = voices[v];

			if (!voice.TrackOn)
				return;

			voice.VolumeSlideUp = 0;
			voice.VolumeSlideDown = 0;

			HvlStep step = song.Tracks[song.Positions[playingInfo.PosNr].Track[v]][playingInfo.NoteNr];
			int note = step.Note;
			int instrument = step.Instrument;
			int fx = step.Fx;
			int fxParam = step.FxParam;
			int fxB = step.FxB;
			int fxBParam = step.FxBParam;

			// Do note delay here
			bool doneNoteDel = false;

			bool DoNoteDelay(int _fx, int _fxParam)
			{
				if (((_fx & 0xf) == 0xe) && ((_fxParam & 0xf0) == 0xd0))
				{
					if (voice.NoteDelayOn)
					{
						voice.NoteDelayOn = false;
						doneNoteDel = true;
					}
					else
					{
						if ((_fxParam & 0x0f) < playingInfo.Tempo)
						{
							voice.NoteDelayWait = _fxParam & 0x0f;

							if (voice.NoteDelayWait != 0)
							{
								voice.NoteDelayOn = true;
								return true;
							}
						}
					}
				}

				return false;
			}

			if (DoNoteDelay(fx, fxParam))
				return;

			if (!doneNoteDel && DoNoteDelay(fxB, fxBParam))
				return;

			if (note != 0)
				voice.OverrideTranspose = 1000;

			ProcessStepFx1(voice, fx & 0xf, fxParam);
			ProcessStepFx1(voice, fxB & 0xf, fxBParam);

			if ((instrument != 0) && (instrument <= song.InstrumentNr))
			{
				HvlInstrument ins = song.Instruments[instrument];

				// Reset panning to last set position
				voice.Pan = voice.SetPan;

				voice.PeriodSlideSpeed = 0;
				voice.PeriodSlidePeriod = 0;
				voice.PeriodSlideLimit = 0;
				voice.PerfSubVolume = 0x40;
				voice.AdsrVolume = 0;
				voice.Instrument = ins;
				voice.InstrumentNumber = instrument;
				voice.SamplePos = 0;

				voice.CalcAdsr();

				// Init on instrument
				voice.WaveLength = ins.WaveLength;
				voice.NoteMaxVolume = ins.Volume;

				// Init vibrato
				voice.VibratoCurrent = 0;
				voice.VibratoDelay = ins.VibratoDelay;
				voice.VibratoDepth = ins.VibratoDepth;
				voice.VibratoSpeed = ins.VibratoSpeed;
				voice.VibratoPeriod = 0;

				// Init hard cut
				voice.HardCutRelease = ins.HardCutRelease;
				voice.HardCut = ins.HardCutReleaseFrames;

				// Init square
				voice.IgnoreSquare = false;
				voice.SquareSlidingIn = false;
				voice.SquareWait = 0;
				voice.SquareOn = false;

				int squareLower = ins.SquareLowerLimit >> (5 - voice.WaveLength);
				int squareUpper = ins.SquareUpperLimit >> (5 - voice.WaveLength);

				if (squareUpper < squareLower)
					(squareUpper, squareLower) = (squareLower, squareUpper);

				voice.SquareUpperLimit = squareUpper;
				voice.SquareLowerLimit = squareLower;

				// Init filter
				voice.IgnoreFilter = 0;
				voice.FilterWait = 0;
				voice.FilterOn = false;
				voice.FilterSlidingIn = false;

				int d6 = ins.FilterSpeed;
				int d3 = ins.FilterLowerLimit;
				int d4 = ins.FilterUpperLimit;

				if ((d3 & 0x80) != 0)
					d6 |= 0x20;

				if ((d4 & 0x80) != 0)
					d6 |= 0x40;

				voice.FilterSpeed = d6;

				d3 &= ~0x80;
				d4 &= ~0x80;

				if (d3 > d4)
					(d4, d3) = (d3, d4);

				voice.FilterUpperLimit = d4;
				voice.FilterLowerLimit = d3;
				voice.FilterPos = 32;

				// Init perf list
				voice.PerfWait = 0;
				voice.PerfCurrent = 0;
				voice.PerfSpeed = ins.PlayList.Speed;
				voice.PerfList = ins.PlayList;

				// Init ring modulation
				voice.RingMixSource = null;
				voice.RingSamplePos = 0;
				voice.RingPlantPeriod = false;
				voice.RingNewWaveform = false;
			}

			voice.PeriodSlideOn = false;

			ProcessStepFx2(voice, fx & 0xf, fxParam, ref note);
			ProcessStepFx2(voice, fxB & 0xf, fxBParam, ref note);

			// Note kicking
			if (note != 0)
			{
				voice.TrackPeriod = note;
				voice.PlantPeriod = true;
				voice.KickNote = true;
			}

			ProcessStepFx3(voice, fx & 0xf, fxParam);
			ProcessStepFx3(voice, fxB & 0xf, fxBParam);
		}



		/********************************************************************/
		/// <summary>
		/// Parse effect part 1
		/// </summary>
		/********************************************************************/
		private void ProcessStepFx1(HvlVoice voice, int fx, int fxParam)
		{
			switch (fx)
			{
				// Position jump HI
				case 0x0:
				{
					if (((fxParam & 0x0f) > 0) && ((fxParam & 0x0f) <= 9))
						playingInfo.PosJump = fxParam & 0x0f;

					break;
				}

				// Volume slide + Tone portamento
				// Volume slide
				case 0x5:
				case 0xa:
				{
					voice.VolumeSlideDown = fxParam & 0x0f;
					voice.VolumeSlideUp = fxParam >> 4;
					break;
				}

				// Panning
				case 0x7:
				{
					if (fxParam > 127)
						fxParam -= 256;

					voice.Pan = fxParam + 128;
					voice.SetPan = fxParam + 128;
					break;
				}

				// Position jump
				case 0xb:
				{
					playingInfo.PosJump = playingInfo.PosJump * 100 + (fxParam & 0x0f) + (fxParam >> 4) * 10;
					playingInfo.PatternBreak = true;
					break;
				}

				// Pattern break
				case 0xd:
				{
					playingInfo.PosJump = playingInfo.PosNr + 1;
					playingInfo.PosJumpNote = currentModuleType == ModuleType.Ahx1 ? 0 : (fxParam & 0x0f) + (fxParam >> 4) * 10;
					playingInfo.PatternBreak = true;

					if (playingInfo.PosJumpNote > song.TrackLength)
						playingInfo.PosJumpNote = 0;

					break;
				}

				// Enhanced commands
				case 0xe:
				{
					switch (fxParam >> 4)
					{
						// Note cut
						case 0xc:
						{
							if ((fxParam & 0x0f) < playingInfo.Tempo)
							{
								voice.NoteCutWait = fxParam & 0x0f;

								if (voice.NoteCutWait != 0)
								{
									voice.NoteCutOn = true;
									voice.HardCutRelease = false;
								}
							}
							break;
						}
					}
					break;
				}

				// Speed
				case 0xf:
				{
					playingInfo.Tempo = fxParam;

					// Tell NostalgicPlayer to end the module if tempo is 0
					if (playingInfo.Tempo == 0)
					{
						endReached = true;
						restartSong = true;
					}

					ShowSpeed();
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse effect part 2
		/// </summary>
		/********************************************************************/
		private void ProcessStepFx2(HvlVoice voice, int fx, int fxParam, ref int note)
		{
			switch (fx)
			{
				// Set square wave offset
				case 0x9:
				{
					voice.SquarePos = fxParam >> (5 - voice.WaveLength);
//					voice.PlantSquare = true;
					voice.IgnoreSquare = true;
					break;
				}

				// Tone portamento (period slide up/down w/ limit)
				case 0x3:
				{
					if (fxParam != 0)
						voice.PeriodSlideSpeed = fxParam;

					goto case 0x5;
				}

				// Tone portamento + Volume slide
				case 0x5:
				{
					if (note != 0)
					{
						int @new = Tables.PeriodTable[note];
						int diff = Tables.PeriodTable[voice.TrackPeriod];

						diff -= @new;
						@new = diff + voice.PeriodSlidePeriod;

						if (@new != 0)
							voice.PeriodSlideLimit = -diff;
					}

					voice.PeriodSlideOn = true;
					voice.PeriodSlideWithLimit = true;
					note = 0;
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse effect part 3
		/// </summary>
		/********************************************************************/
		private void ProcessStepFx3(HvlVoice voice, int fx, int fxParam)
		{
			switch (fx)
			{
				// Portamento up (period slide down)
				case 0x1:
				{
					voice.PeriodSlideSpeed = -fxParam;
					voice.PeriodSlideOn = true;
					voice.PeriodSlideWithLimit = false;
					break;
				}

				// Portamento down (period slide up)
				case 0x2:
				{
					voice.PeriodSlideSpeed = fxParam;
					voice.PeriodSlideOn = true;
					voice.PeriodSlideWithLimit = false;
					break;
				}

				// Filter override
				case 0x4:
				{
					if (currentModuleType == ModuleType.Ahx1)
						fxParam &= 0x0f;

					if ((fxParam == 0) || (fxParam == 0x40))
						break;

					if (fxParam < 0x40)
					{
						voice.IgnoreFilter = fxParam;
						break;
					}

					if (fxParam > 0x7f)
						break;

					voice.FilterPos = fxParam - 0x40;
					break;
				}

				// Volume
				case 0xc:
				{
					if (fxParam <= 0x40)
					{
						voice.NoteMaxVolume = fxParam;
						break;
					}

					if ((fxParam -= 0x50) < 0)
						break;

					if (fxParam <= 0x40)
					{
						for (int i = 0; i < song.Channels; i++)
							voices[i].TrackMasterVolume = fxParam;

						break;
					}

					if ((fxParam -= (0xa0 - 0x50)) < 0)
						break;

					if (fxParam <= 0x40)
						voice.TrackMasterVolume = fxParam;

					break;
				}

				// Enhanced commands
				case 0xe:
				{
					switch (fxParam >> 4)
					{
						// Fine slide up (period fine slide down)
						case 0x1:
						{
							voice.PeriodSlidePeriod -= (fxParam & 0x0f);
							voice.PlantPeriod = true;
							break;
						}

						// Fine slide down (period fine slide up)
						case 0x2:
						{
							voice.PeriodSlidePeriod += fxParam & 0x0f;
							voice.PlantPeriod = true;
							break;
						}

						// Vibrato control
						case 0x4:
						{
							voice.VibratoDepth = fxParam & 0x0f;
							break;
						}

						// Fine volume up
						case 0xa:
						{
							voice.NoteMaxVolume += fxParam & 0x0f;

							if (voice.NoteMaxVolume > 0x40)
								voice.NoteMaxVolume = 0x40;

							break;
						}

						// Fine volume down
						case 0xb:
						{
							voice.NoteMaxVolume -= fxParam & 0x0f;

							if (voice.NoteMaxVolume < 0)
								voice.NoteMaxVolume = 0;

							break;
						}

						// Misc flags
						case 0xf:
						{
							if (currentModuleType == ModuleType.HivelyTracker)
							{
								switch (fxParam & 0xf)
								{
									case 1:
									{
										voice.OverrideTranspose = voice.Transpose;
										break;
									}
								}
							}
							break;
						}
					}
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Process effects that run on each frame
		/// </summary>
		/********************************************************************/
		private void ProcessFrame(int v)
		{
			HvlVoice voice = voices[v];

			if (!voice.TrackOn)
				return;

			if (voice.NoteDelayOn)
			{
				if (voice.NoteDelayWait <= 0)
					ProcessStep(v);
				else
					voice.NoteDelayWait--;
			}

			if (voice.HardCut != 0)
			{
				int nextInstrument;

				if ((playingInfo.NoteNr + 1) < song.TrackLength)
					nextInstrument = song.Tracks[voice.Track][playingInfo.NoteNr + 1].Instrument;
				else
					nextInstrument = song.Tracks[voice.NextTrack][0].Instrument;

				if (nextInstrument != 0)
				{
					int d1 = playingInfo.Tempo - voice.HardCut;

					if (d1 < 0)
						d1 = 0;

					if (!voice.NoteCutOn)
					{
						voice.NoteCutOn = true;
						voice.NoteCutWait = d1;
						voice.HardCutReleaseF = -(d1 - playingInfo.Tempo);
					}
					else
						voice.HardCut = 0;
				}
			}

			if (voice.NoteCutOn)
			{
				if (voice.NoteCutWait <= 0)
				{
					voice.NoteCutOn = false;

					if (voice.HardCutRelease)
					{
						voice.Adsr.RFrames = voice.HardCutReleaseF;
						voice.Adsr.RVolume = 0;

						if (voice.Adsr.RFrames > 0)
							voice.Adsr.RVolume = -(voice.AdsrVolume - (voice.Instrument.Envelope.RVolume << 8)) / voice.Adsr.RFrames;

						voice.Adsr.AFrames = 0;
						voice.Adsr.DFrames = 0;
						voice.Adsr.SFrames = 0;
					}
					else
						voice.NoteMaxVolume = 0;
				}
				else
					voice.NoteCutWait--;
			}

			// ADSR envelope
			if (voice.Adsr.AFrames != 0)
			{
				voice.AdsrVolume += voice.Adsr.AVolume;		// Delta

				if (--voice.Adsr.AFrames <= 0)
					voice.AdsrVolume = voice.Instrument.Envelope.AVolume << 8;
			}
			else if (voice.Adsr.DFrames != 0)
			{
				voice.AdsrVolume += voice.Adsr.DVolume;		// Delta

				if (--voice.Adsr.DFrames <= 0)
					voice.AdsrVolume = voice.Instrument.Envelope.DVolume << 8;
			}
			else if (voice.Adsr.SFrames != 0)
				voice.Adsr.SFrames--;
			else if (voice.Adsr.RFrames != 0)
			{
				voice.AdsrVolume += voice.Adsr.RVolume;		// Delta

				if (--voice.Adsr.RFrames <= 0)
					voice.AdsrVolume = voice.Instrument.Envelope.RVolume << 8;
			}

			// Volume slide
			voice.NoteMaxVolume = voice.NoteMaxVolume + voice.VolumeSlideUp - voice.VolumeSlideDown;

			if (voice.NoteMaxVolume < 0)
				voice.NoteMaxVolume = 0;
			else if (voice.NoteMaxVolume > 0x40)
				voice.NoteMaxVolume = 0x40;

			// Portamento
			if (voice.PeriodSlideOn)
			{
				if (voice.PeriodSlideWithLimit)
				{
					int d0 = voice.PeriodSlidePeriod - voice.PeriodSlideLimit;
					int d2 = voice.PeriodSlideSpeed;

					if (d0 > 0)
						d2 = -d2;

					if (d0 != 0)
					{
						int d3 = (d0 + d2) ^ d0;

						if (d3 >= 0)
							d0 = voice.PeriodSlidePeriod + d2;
						else
							d0 = voice.PeriodSlideLimit;

						voice.PeriodSlidePeriod = d0;
						voice.PlantPeriod = true;
					}
				}
				else
				{
					voice.PeriodSlidePeriod += voice.PeriodSlideSpeed;
					voice.PlantPeriod = true;
				}
			}

			// Vibrato
			if (voice.VibratoDepth != 0)
			{
				if (voice.VibratoDelay <= 0)
				{
					voice.VibratoPeriod = (Tables.VibratoTable[voice.VibratoCurrent] * voice.VibratoDepth) >> 7;
					voice.PlantPeriod = true;
					voice.VibratoCurrent = (voice.VibratoCurrent + voice.VibratoSpeed) & 0x3f;
				}
				else
					voice.VibratoDelay--;
			}

			// PList
			if (voice.PerfList != null)
			{
				if ((voice.Instrument != null) && (voice.PerfCurrent < voice.Instrument.PlayList.Length))
				{
					bool signedOverflow = voice.PerfWait == 128;

					voice.PerfWait--;
					if (signedOverflow || ((sbyte)voice.PerfWait <= 0))
					{
						int cur = voice.PerfCurrent++;
						voice.PerfWait = voice.PerfSpeed;

						HvlPListEntry entry = voice.PerfList.Entries[cur];

						if (entry.Waveform != 0)
						{
							voice.Waveform = entry.Waveform - 1;
							voice.NewWaveform = true;
							voice.PeriodPerfSlideSpeed = 0;
							voice.PeriodPerfSlidePeriod = 0;
						}

						// Hold wave
						voice.PeriodPerfSlideOn = false;

						for (int i = 0; i < 2; i++)
							PListCommandParse(v, entry.Fx[i] & 0xff, entry.FxParam[i] & 0xff);

						// Get note
						if (entry.Note != 0)
						{
							voice.InstrPeriod = entry.Note;
							voice.PlantPeriod = true;
							voice.KickNote = true;
							voice.FixedNote = entry.Fixed;
						}
					}
				}
				else
				{
					if (voice.PerfWait != 0)
						voice.PerfWait--;
					else
						voice.PeriodPerfSlideSpeed = 0;
				}
			}

			// Perf portamento
			if (voice.PeriodPerfSlideOn)
			{
				voice.PeriodPerfSlidePeriod -= voice.PeriodPerfSlideSpeed;

				if (voice.PeriodPerfSlidePeriod != 0)
					voice.PlantPeriod = true;
			}

			if ((voice.Waveform == 3 - 1) && voice.SquareOn)
			{
				if (--voice.SquareWait <= 0)
				{
					int d1 = voice.SquareLowerLimit;
					int d2 = voice.SquareUpperLimit;
					int d3 = voice.SquarePos;

					if (voice.SquareInit)
					{
						voice.SquareInit = false;

						if (d3 <= d1)
						{
							voice.SquareSlidingIn = true;
							voice.SquareSign = 1;
						}
						else if (d3 >= d2)
						{
							voice.SquareSlidingIn = true;
							voice.SquareSign = -1;
						}
					}

					if ((d1 == d3) || (d2 == d3))
					{
						if (voice.SquareSlidingIn)
							voice.SquareSlidingIn = false;
						else
							voice.SquareSign = -voice.SquareSign;
					}

					d3 += voice.SquareSign;
					voice.SquarePos = d3;
					voice.PlantSquare = true;
					voice.SquareWait = voice.Instrument.SquareSpeed;
				}
			}

			if (voice.FilterOn && (--voice.FilterWait <= 0))
			{
				int d1 = voice.FilterLowerLimit;
				int d2 = voice.FilterUpperLimit;
				int d3 = voice.FilterPos;

				if (voice.FilterInit)
				{
					voice.FilterInit = false;

					if (d3 <= d1)
					{
						voice.FilterSlidingIn = true;
						voice.FilterSign = 1;
					}
					else if (d3 >= d2)
					{
						voice.FilterSlidingIn = true;
						voice.FilterSign = -1;
					}
				}

				int fMax = (voice.FilterSpeed < 4) ? (5 - voice.FilterSpeed) : 1;

				for (int i = 0; i < fMax; i++)
				{
					if ((d1 == d3) || (d2 == d3))
					{
						if (voice.FilterSlidingIn)
							voice.FilterSlidingIn = false;
						else
							voice.FilterSign = -voice.FilterSign;
					}

					d3 += voice.FilterSign;
				}

				if (d3 < 1)
					d3 = 1;

				if (d3 > 63)
					d3 = 63;

				voice.FilterPos = d3;
				voice.NewWaveform = true;
				voice.FilterWait = voice.FilterSpeed - 3;

				if (voice.FilterWait < 1)
					voice.FilterWait = 1;
			}

			if ((voice.Waveform == 3 - 1) || voice.PlantSquare)
			{
				// Calc square
				sbyte[] squarePtr = waves.filterSets[voice.FilterPos - 1].Squares;
				int x = voice.SquarePos << (5 - voice.WaveLength);

				if (x > 0x20)
				{
					x = 0x40 - x;
					voice.SquareReverse = true;
				}

				int squareOffset = x > 0 ? (x - 1) << 7 : 0;

				int delta = 32 >> voice.WaveLength;
				playingInfo.SquareWaveform = voice.SquareTempBuffer;

				for (int i = 0; i < (1 << voice.WaveLength) * 4; i++)
				{
					voice.SquareTempBuffer[i] = squarePtr[squareOffset];
					squareOffset += delta;
				}

				voice.NewWaveform = true;
				voice.Waveform = 3 - 1;
				voice.PlantSquare = false;
			}

			if (voice.Waveform == 4 - 1)
				voice.NewWaveform = true;

			if (voice.RingNewWaveform)
			{
				if (voice.RingWaveform > 1)
					voice.RingWaveform = 1;

				voice.RingAudioSource = GetWaveform(voice.RingWaveform, 32);	// 32 = No filter
				voice.RingAudioOffset = Tables.OffsetTable[voice.WaveLength];
			}

			if (voice.NewWaveform)
			{
				sbyte[] audioSource = GetWaveform(voice.Waveform, voice.FilterPos);
				int audioOffset = 0;

				if (voice.Waveform < 3 - 1)
					audioOffset = Tables.OffsetTable[voice.WaveLength];

				if (voice.Waveform == 4 - 1)
				{
					// Add random moving
					audioOffset = (voice.WnRandom & (2 * 0x280 - 1)) & ~1;

					// Go on random
					voice.WnRandom += 2239384;
					voice.WnRandom = ((((voice.WnRandom >> 8) | (voice.WnRandom << 24)) + 782323) ^ 75) - 6735;
				}

				voice.AudioSource = audioSource;
				voice.AudioOffset = audioOffset;
			}

			// Ring modulation period calculation
			if (voice.RingAudioSource != null)
			{
				voice.RingAudioPeriod = voice.RingBasePeriod;

				if (!voice.RingFixedPeriod)
				{
					if (voice.OverrideTranspose != 1000)
						voice.RingAudioPeriod += voice.OverrideTranspose + voice.TrackPeriod - 1;
					else
						voice.RingAudioPeriod += voice.Transpose + voice.TrackPeriod - 1;
				}

				if (voice.RingAudioPeriod > 5 * 12)
					voice.RingAudioPeriod = 5 * 12;

				if (voice.RingAudioPeriod < 0)
					voice.RingAudioPeriod = 0;

				voice.RingAudioPeriod = Tables.PeriodTable[voice.RingAudioPeriod];

				if (!voice.RingFixedPeriod)
					voice.RingAudioPeriod += voice.PeriodSlidePeriod;

				voice.RingAudioPeriod += voice.PeriodPerfSlidePeriod + voice.VibratoPeriod;

				if (voice.RingAudioPeriod > 0x0d60)
					voice.RingAudioPeriod = 0x0d60;

				if (voice.RingAudioPeriod < 0x0071)
					voice.RingAudioPeriod = 0x0071;
			}

			// Normal period calculation
			voice.AudioPeriod = voice.InstrPeriod;

			if (!voice.FixedNote)
			{
				if (voice.OverrideTranspose != 1000)
					voice.AudioPeriod += voice.OverrideTranspose + voice.TrackPeriod - 1;
				else
					voice.AudioPeriod += voice.Transpose + voice.TrackPeriod - 1;
			}

			if (voice.AudioPeriod > 5 * 12)
				voice.AudioPeriod = 5 * 12;

			if (voice.AudioPeriod < 0)
				voice.AudioPeriod = 0;

			voice.AudioPeriod = Tables.PeriodTable[voice.AudioPeriod];

			if (!voice.FixedNote)
				voice.AudioPeriod += voice.PeriodSlidePeriod;

			voice.AudioPeriod += voice.PeriodPerfSlidePeriod + voice.VibratoPeriod;

			if (voice.AudioPeriod > 0x0d60)
				voice.AudioPeriod = 0x0d60;

			if (voice.AudioPeriod < 0x0071)
				voice.AudioPeriod = 0x0071;

			// Audio init volume
			voice.AudioVolume = (((((((voice.AdsrVolume >> 8) * voice.NoteMaxVolume) >> 6) * voice.PerfSubVolume) >> 6) * voice.TrackMasterVolume) >> 6);
		}



		/********************************************************************/
		/// <summary>
		/// Parses PList commands
		/// </summary>
		/********************************************************************/
		private void PListCommandParse(int v, int fx, int fxParam)
		{
			HvlVoice voice = voices[v];

			switch (fx)
			{
				// Set filter
				case 0:
				{
					if ((currentModuleType != ModuleType.Ahx1) && (fxParam > 0) && (fxParam < 0x40))
					{
						if (voice.IgnoreFilter != 0)
						{
							voice.FilterPos = voice.IgnoreFilter;
							voice.IgnoreFilter = 0;
						}
						else
							voice.FilterPos = fxParam;

						voice.NewWaveform = true;
					}
					break;
				}

				// Slide up
				case 1:
				{
					voice.PeriodPerfSlideSpeed = fxParam;
					voice.PeriodPerfSlideOn = true;
					break;
				}

				// Slide down
				case 2:
				{
					voice.PeriodPerfSlideSpeed = -fxParam;
					voice.PeriodPerfSlideOn = true;
					break;
				}

				// Init square modulation
				case 3:
				{
					if (!voice.IgnoreSquare)
						voice.SquarePos = fxParam >> (5 - voice.WaveLength);
					else
						voice.IgnoreSquare = false;

					break;
				}

				// Start/stop modulation and/or filter
				case 4:
				{
					if ((currentModuleType == ModuleType.Ahx1) || (fxParam == 0))
					{
						voice.SquareOn = !voice.SquareOn;
						voice.SquareInit = voice.SquareOn;
						voice.SquareSign = 1;
					}
					else
					{
						if ((fxParam & 0x0f) != 0x00)
						{
							voice.SquareOn = !voice.SquareOn;
							voice.SquareInit = voice.SquareOn;
							voice.SquareSign = 1;

							if ((fxParam & 0x0f) == 0x0f)
								voice.SquareSign = -1;
						}

						if ((fxParam & 0xf0) != 0x00)
						{
							voice.FilterOn = !voice.FilterOn;
							voice.FilterInit = voice.FilterOn;
							voice.FilterSign = 1;

							if ((fxParam & 0xf0) == 0xf0)
								voice.FilterSign = -1;
						}
					}
					break;
				}

				// Jump to step
				case 5:
				{
					voice.PerfCurrent = fxParam;
					break;
				}

				// Ring modulate with triangle
				case 7:
				{
					if (currentModuleType == ModuleType.HivelyTracker)
					{
						if ((fxParam >= 1) && (fxParam <= 0x3c))
						{
							voice.RingBasePeriod = fxParam;
							voice.RingFixedPeriod = true;
						}
						else if ((fxParam >= 0x81) && (fxParam <= 0xbc))
						{
							voice.RingBasePeriod = fxParam - 0x80;
							voice.RingFixedPeriod = false;
						}
						else
						{
							voice.RingBasePeriod = 0;
							voice.RingFixedPeriod = false;
							voice.RingNewWaveform = false;
							voice.RingAudioSource = null;
							voice.RingAudioOffset = 0;
							voice.RingMixSource = null;
							break;
						}

						voice.RingWaveform = 0;
						voice.RingNewWaveform = true;
						voice.RingPlantPeriod = true;
					}
					break;
				}

				// Ring modulate with sawtooth
				case 8:
				{
					if (currentModuleType == ModuleType.HivelyTracker)
					{
						if ((fxParam >= 1) && (fxParam <= 0x3c))
						{
							voice.RingBasePeriod = fxParam;
							voice.RingFixedPeriod = true;
						}
						else if ((fxParam >= 0x81) && (fxParam <= 0xbc))
						{
							voice.RingBasePeriod = fxParam - 0x80;
							voice.RingFixedPeriod = false;
						}
						else
						{
							voice.RingBasePeriod = 0;
							voice.RingFixedPeriod = false;
							voice.RingNewWaveform = false;
							voice.RingAudioSource = null;
							voice.RingAudioOffset = 0;
							voice.RingMixSource = null;
							break;
						}

						voice.RingWaveform = 1;
						voice.RingNewWaveform = true;
						voice.RingPlantPeriod = true;
					}
					break;
				}

				// Stereo panning
				case 9:
				{
					if (currentModuleType == ModuleType.HivelyTracker)
					{
						if (fxParam > 127)
							fxParam -= 256;

						voice.Pan = fxParam + 128;
					}
					break;
				}

				// Set volume
				case 12:
				{
					if (fxParam <= 0x40)
					{
						voice.NoteMaxVolume = fxParam;
						break;
					}

					fxParam -= 0x50;
					if (fxParam < 0)
						break;

					if (fxParam <= 0x40)
					{
						voice.PerfSubVolume = fxParam;
						break;
					}

					fxParam -= 0xa0 - 0x50;
					if (fxParam < 0)
						break;

					if (fxParam <= 0x40)
						voice.TrackMasterVolume = fxParam;

					break;
				}

				// Set speed
				case 15:
				{
					voice.PerfSpeed = fxParam;
					voice.PerfWait = fxParam;
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Set audio buffers
		/// </summary>
		/********************************************************************/
		private void SetAudio(int v)
		{
			HvlVoice voice = voices[v];
			VisualizerChannel visualizerChan = visualizerChannels[v];

			if (!voice.TrackOn)
			{
				voice.VoiceVolume = 0;
				visualizerChan.Muted = true;
				return;
			}

			visualizerChan.Muted = false;
			visualizerChan.NoteKicked = false;

			voice.VoiceVolume = voice.AudioVolume;
			visualizerChan.Volume = (ushort)voice.VoiceVolume;

			if (voice.PlantPeriod)
			{
				voice.PlantPeriod = false;
				voice.VoicePeriod = voice.AudioPeriod;

				double freq2 = Period2Freq(voice.AudioPeriod);
				int delta = (int)(freq2 / mixerFreq);

				if (delta > (0x280 << 16))
					delta -= (0x280 << 16);

				if (delta == 0)
					delta = 1;

				voice.Delta = delta;

				visualizerChan.Frequency = (uint)(freq2 / 65536.0);
			}

			if (voice.NewWaveform)
			{
				if (voice.Waveform == 4 - 1)
					Array.Copy(voice.AudioSource, voice.AudioOffset, voice.VoiceBuffer, 0, 0x280);
				else
				{
					int waveLoops = (1 << (5 - voice.WaveLength)) * 5;
					int loopLen = 4 * (1 << voice.WaveLength);

					if (voice.AudioSource != null)
					{
						for (int i = 0; i < waveLoops; i++)
							Array.Copy(voice.AudioSource, voice.AudioOffset, voice.VoiceBuffer, i * loopLen, loopLen);
					}
					else
					{
						for (int i = 0; i < waveLoops; i++)
							Array.Clear(voice.VoiceBuffer, i * loopLen, loopLen);
					}
				}

				voice.VoiceBuffer[0x280] = voice.VoiceBuffer[0];
				voice.MixSource = voice.VoiceBuffer;
			}

			// Ring modulation
			if (voice.RingPlantPeriod)
			{
				voice.RingPlantPeriod = false;

				double freq2 = Period2Freq(voice.RingAudioPeriod);
				int delta = (int)(freq2 / mixerFreq);

				if (delta > (0x280 << 16))
					delta -= (0x280 << 16);

				if (delta == 0)
					delta = 1;

				voice.RingDelta = delta;
			}

			if (voice.RingNewWaveform)
			{
				int waveLoops = (1 << (5 - voice.WaveLength)) * 5;
				int loopLen = 4 * (1 << voice.WaveLength);

				if (voice.RingAudioSource != null)
				{
					for (int i = 0; i < waveLoops; i++)
						Array.Copy(voice.RingAudioSource, voice.RingAudioOffset, voice.RingVoiceBuffer, i * loopLen, loopLen);
				}
				else
				{
					for (int i = 0; i < waveLoops; i++)
						Array.Clear(voice.RingVoiceBuffer, i * loopLen, loopLen);
				}

				voice.RingVoiceBuffer[0x280] = voice.RingVoiceBuffer[0];
				voice.RingMixSource = voice.RingVoiceBuffer;
			}

			if (voice.KickNote)
			{
				voice.KickNote = false;

				visualizerChan.NoteKicked = true;
				visualizerChan.SampleNumber = (short)(voice.InstrumentNumber - 1);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Test for bounds values
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private double Period2Freq(int period)
		{
			return (3546895 * 65536.0) / period;
		}



		/********************************************************************/
		/// <summary>
		/// Return the right waveform to use
		/// </summary>
		/********************************************************************/
		private sbyte[] GetWaveform(int waveform, int filter)
		{
			if (waveform == 3 - 1)
				return playingInfo.SquareWaveform;

			HvlWaves.Waves wave = waves.filterSets[filter - 1];

			switch (waveform)
			{
				case 1 - 1:
					return wave.Triangles;

				case 2 - 1:
					return wave.Sawtooths;

				case 4 - 1:
					return wave.WhiteNoise;
			}

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Will mix all the channels into the output buffers
		/// </summary>
		/********************************************************************/
		private void MixChunk()
		{
			int chans = song.Channels;

			sbyte[][] src = new sbyte[chans][];
			sbyte[][] rSrc = new sbyte[chans][];
			int[] delta = new int[chans];
			int[] rDelta = new int[chans];
			int[] vol = new int[chans];
			int[] pos = new int[chans];
			int[] rPos = new int[chans];
			int[] panL = new int[chans];
			int[] panR = new int[chans];

			for (int i = 0; i < chans; i++)
			{
				HvlVoice voice = voices[i];

				int pan = (((voice.Pan - 128) * stereoSeparation) / 100) + 128;

				delta[i] = voice.Delta;
				vol[i] = enabledChannels[i] ? voice.VoiceVolume : 0;
				pos[i] = voice.SamplePos;
				src[i] = voice.MixSource;
				panL[i] = waves.PanningLeft[pan];
				panR[i] = waves.PanningRight[pan];

				// Ring modulation
				rDelta[i] = voice.RingDelta;
				rPos[i] = voice.RingSamplePos;
				rSrc[i] = voice.RingMixSource;
			}

			int samples = leftBuffer.Length;
			int outOffset = 0;

			do
			{
				int loops = samples;

				for (int i = 0; i < chans; i++)
				{
					if (pos[i] >= (0x280 << 16))
						pos[i] -= 0x280 << 16;

					int cnt = ((0x280 << 16) - pos[i] - 1) / delta[i] + 1;
					if (cnt < loops)
						loops = cnt;

					if (rSrc[i] != null)
					{
						if (rPos[i] >= (0x280 << 16))
							rPos[i] -= 0x280 << 16;

						cnt = ((0x280 << 16) - rPos[i] - 1) / rDelta[i] + 1;
						if (cnt < loops)
							loops = cnt;
					}
				}

				samples -= loops;

				// Inner loop
				do
				{
					int a = 0;
					int b = 0;

					for (int i = 0; i < chans; i++)
					{
						int j;

						if (rSrc[i] != null)
						{
							// Ring modulation
							j = ((src[i][pos[i] >> 16] * rSrc[i][rPos[i] >> 16]) >> 7) * vol[i];
							rPos[i] += rDelta[i];
						}
						else
							j = src[i][pos[i] >> 16] * vol[i];

						a += (j * panL[i]) >> 7;
						b += (j * panR[i]) >> 7;

						pos[i] += delta[i];
					}

					a = (a * song.MixGain) >> 8;
					b = (b * song.MixGain) >> 8;

					// If mono, mix the two values
					if (mixerChannels == 1)
						a += b;

					if (a < -0x8000)
						a = -0x8000;

					if (a > 0x7fff)
						a = 0x7fff;

					if (b < -0x8000)
						b = -0x8000;

					if (b > 0x7fff)
						b = 0x7fff;

					leftBuffer[outOffset] = (short)a;
					rightBuffer[outOffset] = (short)b;

					loops--;
					outOffset++;
				}
				while (loops > 0);
			}
			while (samples > 0);

			for (int i = 0; i < chans; i++)
			{
				voices[i].SamplePos = pos[i];
				voices[i].RingSamplePos = rPos[i];

				visualizerChannels[i].SamplePosition = pos[i] >> 16;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Play the mixed buffer
		/// </summary>
		/********************************************************************/
		private void PlayBuffer()
		{
			VirtualChannels[0].PlayBuffer(leftBuffer, 0, (uint)leftBuffer.Length, PlayBufferFlag._16Bit);

			if (mixerChannels != 1)
				VirtualChannels[1].PlayBuffer(rightBuffer, 0, (uint)rightBuffer.Length, PlayBufferFlag._16Bit);
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current song position
		/// </summary>
		/********************************************************************/
		private void ShowSongPosition()
		{
			OnModuleInfoChanged(InfoPositionLine, playingInfo.PosNr.ToString());
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
			OnModuleInfoChanged(InfoSpeedLine, playingInfo.Tempo.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with all dynamic values
		/// </summary>
		/********************************************************************/
		private void UpdateModuleInformation()
		{
			ShowSongPosition();
			ShowTracks();
			ShowSpeed();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the playing tracks
		/// </summary>
		/********************************************************************/
		private string FormatTracks()
		{
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < song.Channels; i++)
			{
				sb.Append(voices[i].Track);
				sb.Append(", ");
			}

			sb.Remove(sb.Length - 2, 2);

			return sb.ToString();
		}
		#endregion
	}
}
