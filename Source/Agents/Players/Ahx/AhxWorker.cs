/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.Ahx.Containers;
using Polycode.NostalgicPlayer.Agent.Player.Ahx.Implementation;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.Ahx
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class AhxWorker : ModulePlayerWithPositionDurationAgentBase
	{
		private static readonly Dictionary<Guid, ModuleType> moduleTypeLookup = new Dictionary<Guid, ModuleType>
		{
			{ Ahx.Agent1Id, ModuleType.Ahx1 },
			{ Ahx.Agent2Id, ModuleType.Ahx2 }
		};

		private readonly ModuleType currentModuleType;

		private AhxWaves waves;

		private AhxSong song;

		private GlobalPlayingInfo playingInfo;
		private AhxVoices[] voices;

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
		public AhxWorker(Guid typeId)
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
		public override string[] FileExtensions => new [] { "ahx", "thx" };



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
					description = Resources.IDS_AHX_INFODESCLINE0;
					value = song.PositionNr.ToString();
					break;
				}

				// Used tracks
				case 1:
				{
					description = Resources.IDS_AHX_INFODESCLINE1;
					value = (song.TrackNr + 1).ToString();
					break;
				}

				// Used instruments
				case 2:
				{
					description = Resources.IDS_AHX_INFODESCLINE2;
					value = song.InstrumentNr.ToString();
					break;
				}

				// Playing position
				case 3:
				{
					description = Resources.IDS_AHX_INFODESCLINE3;
					value = playingInfo.PosNr.ToString();
					break;
				}

				// Playing tracks
				case 4:
				{
					description = Resources.IDS_AHX_INFODESCLINE4;
					value = FormatTracks();
					break;
				}

				// Current speed
				case 5:
				{
					description = Resources.IDS_AHX_INFODESCLINE5;
					value = playingInfo.Tempo.ToString();
					break;
				}

				// Current tempo (Hz):
				case 6:
				{
					description = Resources.IDS_AHX_INFODESCLINE6;
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
				song = new AhxSong();

				// Seek to the revision and read it
				moduleStream.Seek(3, SeekOrigin.Begin);
				song.Revision = moduleStream.Read_UINT8();

				// Skip song title offset
				moduleStream.Seek(2, SeekOrigin.Current);

				//
				// Header
				//
				byte flag = moduleStream.Read_UINT8();
				song.SpeedMultiplier = song.Revision == 0 ? 1 : ((flag >> 5) & 3) + 1;

				song.PositionNr = ((flag & 0xf) << 8) | moduleStream.Read_UINT8();
				song.Restart = moduleStream.Read_B_UINT16();
				song.TrackLength = moduleStream.Read_UINT8();
				song.TrackNr = moduleStream.Read_UINT8();
				song.InstrumentNr = moduleStream.Read_UINT8();
				byte subSongNr = moduleStream.Read_UINT8();

				// Validate header values
				if ((song.PositionNr < 1) || (song.PositionNr > 999))
				{
					errorMessage = Resources.IDS_AHX_ERR_CORRUPT_HEADER;
					Cleanup();

					return AgentResult.Error;
				}

				if ((song.Restart < 0) || (song.Restart >= song.PositionNr))
				{
					errorMessage = Resources.IDS_AHX_ERR_CORRUPT_HEADER;
					Cleanup();

					return AgentResult.Error;
				}

				if ((song.TrackLength < 1) || (song.TrackLength > 64))
				{
					errorMessage = Resources.IDS_AHX_ERR_CORRUPT_HEADER;
					Cleanup();

					return AgentResult.Error;
				}

				if ((song.InstrumentNr < 0) || (song.InstrumentNr > 63))
				{
					errorMessage = Resources.IDS_AHX_ERR_CORRUPT_HEADER;
					Cleanup();

					return AgentResult.Error;
				}

				// Read sub-songs (we just skip the start positions, because we have our own algorithm to find them)
				moduleStream.Seek(subSongNr * 2, SeekOrigin.Current);

				// Read position list
				song.Positions = new AhxPosition[song.PositionNr];

				for (int i = 0; i < song.PositionNr; i++)
				{
					song.Positions[i] = new AhxPosition();

					for (int j = 0; j < 4; j++)
					{
						song.Positions[i].Track[j] = moduleStream.Read_UINT8();
						song.Positions[i].Transpose[j] = moduleStream.Read_INT8();
					}
				}

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_AHX_ERR_LOADING_HEADER;
					Cleanup();

					return AgentResult.Error;
				}

				//
				// Tracks
				//
				int maxTrack = song.TrackNr;
				song.Tracks = new AhxStep[maxTrack + 1][];

				for (int i = 0; i <= maxTrack; i++)
				{
					song.Tracks[i] = ArrayHelper.InitializeArray<AhxStep>(song.TrackLength);

					// Check if track 0 has been saved in the file. If not, it means it is an empty track
					if (((flag & 0x80) == 0x80) && (i == 0))
					{
						for (int j = 0; j < song.TrackLength; j++)
						{
							song.Tracks[i][j].Note = 0;
							song.Tracks[i][j].Instrument = 0;
							song.Tracks[i][j].Fx = 0;
							song.Tracks[i][j].FxParam = 0;
						}
					}
					else
					{
						for (int j = 0; j < song.TrackLength; j++)
						{
							// Read the 3 track bytes
							byte byte1 = moduleStream.Read_UINT8();
							byte byte2 = moduleStream.Read_UINT8();
							byte byte3 = moduleStream.Read_UINT8();

							song.Tracks[i][j].Note = (byte1 >> 2) & 0x3f;
							song.Tracks[i][j].Instrument = ((byte1 & 0x3) << 4) | (byte2 >> 4);
							song.Tracks[i][j].Fx = byte2 & 0xf;
							song.Tracks[i][j].FxParam = byte3;
						}

						if (moduleStream.EndOfStream)
						{
							errorMessage = Resources.IDS_AHX_ERR_LOADING_TRACKS;
							Cleanup();

							return AgentResult.Error;
						}
					}
				}

				//
				// Instruments
				//
				song.Instruments = ArrayHelper.InitializeArray<AhxInstrument>(song.InstrumentNr + 1);

				for (int i = 1; i <= song.InstrumentNr; i++)
				{
					song.Instruments[i].Volume = moduleStream.Read_UINT8();

					byte byte1 = moduleStream.Read_UINT8();
					song.Instruments[i].FilterSpeed = song.Revision == 0 ? 0 : (byte1 >> 3) & 0x1f;
					song.Instruments[i].WaveLength = byte1 & 0x7;
					song.Instruments[i].Envelope.AFrames = moduleStream.Read_UINT8();
					song.Instruments[i].Envelope.AVolume = moduleStream.Read_UINT8();
					song.Instruments[i].Envelope.DFrames = moduleStream.Read_UINT8();
					song.Instruments[i].Envelope.DVolume = moduleStream.Read_UINT8();
					song.Instruments[i].Envelope.SFrames = moduleStream.Read_UINT8();
					song.Instruments[i].Envelope.RFrames = moduleStream.Read_UINT8();
					song.Instruments[i].Envelope.RVolume = moduleStream.Read_UINT8();

					if (song.Instruments[i].Envelope.DFrames == 0)
						song.Instruments[i].Envelope.DFrames = 1;

					if (song.Instruments[i].Envelope.RFrames == 0)
						song.Instruments[i].Envelope.RFrames = 1;

					moduleStream.Seek(3, SeekOrigin.Current);

					byte1 = moduleStream.Read_UINT8();

					if (song.Revision == 0)
						song.Instruments[i].FilterLowerLimit = 0;
					else
					{
						song.Instruments[i].FilterSpeed |= ((byte1 >> 2) & 0x20);
						song.Instruments[i].FilterLowerLimit = byte1 & 0x7f;
					}

					song.Instruments[i].VibratoDelay = moduleStream.Read_UINT8();

					byte1 = moduleStream.Read_UINT8();

					if (song.Revision == 0)
					{
						song.Instruments[i].HardCutReleaseFrames = 0;
						song.Instruments[i].HardCutRelease = false;
					}
					else
					{
						song.Instruments[i].HardCutReleaseFrames = (byte1 >> 4) & 7;
						song.Instruments[i].HardCutRelease = (byte1 & 0x80) != 0;
					}

					song.Instruments[i].VibratoDepth = byte1 & 0xf;
					song.Instruments[i].VibratoSpeed = moduleStream.Read_UINT8();
					song.Instruments[i].SquareLowerLimit = moduleStream.Read_UINT8();
					song.Instruments[i].SquareUpperLimit = moduleStream.Read_UINT8();
					song.Instruments[i].SquareSpeed = moduleStream.Read_UINT8();

					byte1 = moduleStream.Read_UINT8();

					if (song.Revision == 0)
						song.Instruments[i].FilterUpperLimit = 0;
					else
					{
						song.Instruments[i].FilterSpeed |= ((byte1 >> 1) & 0x40);
						song.Instruments[i].FilterUpperLimit = byte1 & 0x3f;
					}

					song.Instruments[i].PlayList.Speed = moduleStream.Read_UINT8();
					song.Instruments[i].PlayList.Length = moduleStream.Read_UINT8();

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_AHX_ERR_LOADING_INSTRUMENTS;
						Cleanup();

						return AgentResult.Error;
					}

					// Load play list
					song.Instruments[i].PlayList.Entries = new AhxPListEntry[song.Instruments[i].PlayList.Length];

					for (int j = 0; j < song.Instruments[i].PlayList.Length; j++)
					{
						byte1 = moduleStream.Read_UINT8();
						byte byte2 = moduleStream.Read_UINT8();
						byte byte3 = moduleStream.Read_UINT8();
						byte byte4 = moduleStream.Read_UINT8();

						song.Instruments[i].PlayList.Entries[j] = new AhxPListEntry
						{
							Fx = new [] { (byte1 >> 2) & 7, (byte1 >> 5) & 7 },
							Waveform = ((byte1 << 1) & 6) | (byte2 >> 7),
							Fixed = ((byte2 >> 6) & 1) != 0,
							Note = byte2 & 0x3f,
							FxParam = new int[] { byte3, byte4 }
						};
					}

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_AHX_ERR_LOADING_INSTRUMENTS;
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
			waves = new AhxWaves();

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
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public override void Play()
		{
			PlayIrq();

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
					frequencies[2 * 12 + j] = (uint)(3546895 / Tables.PeriodTable[j + 1]);

				for (int i = 1; i <= song.InstrumentNr; i++)
				{
					AhxInstrument inst = song.Instruments[i];

					yield return new SampleInfo
					{
						Name = inst.Name,
						Flags = SampleInfo.SampleFlag.None,
						Type = SampleInfo.SampleType.Synthesis,
						BitSize = SampleInfo.SampleSize._8Bit,
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
				AhxVoices voice = voices[i];

				voice.PlantPeriod = true;
				voice.AudioPeriod = voices[i].VoicePeriod;
				voice.NewWaveform = true;
				voice.WaveformStarted = false;
			}

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
				MainVolume = 0x40,
				NoteNr = 0,
				PosJumpNote = 0,
				Tempo = 6,
				StepWaitFrames = 0,
				GetNewPosition = true
			};

			endReached = false;
			restartSong = false;

			voices = ArrayHelper.InitializeArray<AhxVoices>(4);

			for (int v = 0; v < 4; v++)
				voices[v].Init();

			PlayingFrequency = 50 * song.SpeedMultiplier;
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

					for (int i = 0; i < 4; i++)
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

				for (int i = 0; i < 4; i++)
					ProcessStep(i);

				playingInfo.StepWaitFrames = playingInfo.Tempo;
			}

			// Do frame stuff
			for (int i = 0; i < 4; i++)
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

			for (int a = 0; a < 4; a++)
				SetAudio(a);
		}



		/********************************************************************/
		/// <summary>
		/// Parse the next step
		/// </summary>
		/********************************************************************/
		private void ProcessStep(int v)
		{
			AhxVoices voice = voices[v];

			if (!voice.TrackOn)
				return;

			voice.VolumeSlideUp = 0;
			voice.VolumeSlideDown = 0;

			AhxStep step = song.Tracks[song.Positions[playingInfo.PosNr].Track[v]][playingInfo.NoteNr];
			int note = step.Note;
			int instrument = step.Instrument;
			int fx = step.Fx;
			int fxParam = step.FxParam;

			switch (fx)
			{
				// Position jump HI
				case 0x0:
				{
					if (((fxParam & 0x0f) > 0) && ((fxParam & 0x0f) <= 9))
						playingInfo.PosJump = fxParam & 0x0f;

					break;
				}

				case 0x5:	// Volume slide + Tone portamento
				case 0xa:	// Volume slide
				{
					voice.VolumeSlideDown = fxParam & 0x0f;
					voice.VolumeSlideUp = fxParam >> 4;
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
					playingInfo.PosJumpNote = song.Revision == 0 ? 0 : (fxParam & 0x0f) + (fxParam >> 4) * 10;

					if (playingInfo.PosJumpNote > song.TrackLength)
						playingInfo.PosJumpNote = 0;

					playingInfo.PatternBreak = true;
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

						// Note delay
						case 0xd:
						{
							if (voice.NoteDelayOn)
								voice.NoteDelayOn = false;
							else
							{
								if ((fxParam & 0x0f) < playingInfo.Tempo)
								{
									voice.NoteDelayWait = fxParam & 0x0f;

									if (voice.NoteDelayWait != 0)
									{
										voice.NoteDelayOn = true;
										return;
									}
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

					// Change the module info
					OnModuleInfoChanged(InfoSpeedLine, playingInfo.Tempo.ToString());
					break;
				}
			}

			// Instrument range check by Thomas Neumann
			if ((instrument != 0) && (instrument <= song.InstrumentNr))
			{
				voice.PerfSubVolume = 0x40;
				voice.PeriodSlideSpeed = 0;
				voice.PeriodSlidePeriod = 0;
				voice.PeriodSlideLimit = 0;
				voice.AdsrVolume = 0;
				voice.Instrument = song.Instruments[instrument];
				voice.InstrumentNumber = instrument;
				voice.CalcAdsr();

				// Init on instrument
				voice.WaveLength = voice.Instrument.WaveLength;
				voice.NoteMaxVolume = voice.Instrument.Volume;

				// Init vibrato
				voice.VibratoCurrent = 0;
				voice.VibratoDelay = voice.Instrument.VibratoDelay;
				voice.VibratoDepth = voice.Instrument.VibratoDepth;
				voice.VibratoSpeed = voice.Instrument.VibratoSpeed;
				voice.VibratoPeriod = 0;

				// Init hard cut
				voice.HardCutRelease = voice.Instrument.HardCutRelease;
				voice.HardCut = voice.Instrument.HardCutReleaseFrames;

				// Init square
				voice.IgnoreSquare = false;
				voice.SquareSlidingIn = false;
				voice.SquareWait = 0;
				voice.SquareOn = false;

				int squareLower = voice.Instrument.SquareLowerLimit >> (5 - voice.WaveLength);
				int squareUpper = voice.Instrument.SquareUpperLimit >> (5 - voice.WaveLength);

				if (squareUpper < squareLower)
					(squareUpper, squareLower) = (squareLower, squareUpper);

				voice.SquareUpperLimit = squareUpper;
				voice.SquareLowerLimit = squareLower;

				// Init filter
				voice.IgnoreFilter = false;
				voice.FilterWait = 0;
				voice.FilterOn = false;
				voice.FilterSlidingIn = false;

				int d6 = voice.Instrument.FilterSpeed;
				int d3 = voice.Instrument.FilterLowerLimit;
				int d4 = voice.Instrument.FilterUpperLimit;

				if ((d3 & 0x80) != 0)
					d6 |= 0x20;

				if ((d4 & 0x80) != 0)
					d6 |= 0x40;

				voices[v].FilterSpeed = d6;

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
				voice.PerfSpeed = voice.Instrument.PlayList.Speed;
				voice.PerfList = voice.Instrument.PlayList;
			}

			voice.PeriodSlideOn = false;

			bool skipNoteKick = false;

			switch (fx)
			{
				// Override filter
				case 0x4:
					break;

				// Set square wave offset
				case 0x9:
				{
					voice.SquarePos = fxParam >> (5 - voices[v].WaveLength);
					voice.PlantSquare = true;
					voice.IgnoreSquare = true;
					break;
				}

				case 0x5:	// Tone portamento + Volume slide
				case 0x3:	// Tone portamento (period slide up/down w/ limit)
				{
					if (fxParam != 0)
						voice.PeriodSlideSpeed = fxParam;

					if (note != 0)
					{
						int neue = Tables.PeriodTable[note];
						int alte = Tables.PeriodTable[voice.TrackPeriod];

						alte -= neue;
						neue = alte + voice.PeriodSlidePeriod;

						if (neue != 0)
							voice.PeriodSlideLimit = -alte;
					}

					voice.PeriodSlideOn = true;
					voice.PeriodSlideWithLimit = true;

					skipNoteKick = true;
					break;
				}
			}

			if (!skipNoteKick)
			{
				// Note kicking
				if (note != 0)
				{
					voice.TrackPeriod = note;
					voice.PlantPeriod = true;
					voice.KickNote = true;
				}
			}

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

				// Volume
				case 0xc:
				{
					if (fxParam <= 0x40)
						voice.NoteMaxVolume = fxParam;
					else
					{
						if (fxParam >= 0x50)
						{
							fxParam -= 0x50;

							if (fxParam <= 0x40)
							{
								for (int i = 0; i < 4; i++)
									voices[i].TrackMasterVolume = fxParam;
							}
							else
							{
								fxParam -= (0xa0 - 0x50);

								if ((fxParam > 0) && (fxParam <= 0x40))
									voice.TrackMasterVolume = fxParam;
							}
						}
					}
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
							voice.PeriodSlidePeriod = -(fxParam & 0x0f);
							voice.PlantPeriod = true;
							break;
						}

						// Fine slide down (period fine slide up)
						case 0x2:
						{
							voice.PeriodSlidePeriod = fxParam & 0x0f;
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
			AhxVoices voice = voices[v];

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
						voice.Adsr.RVolume = -(voice.AdsrVolume - (voice.Instrument.Envelope.RVolume << 8)) / voice.HardCutReleaseF;
						voice.Adsr.RFrames = voice.HardCutReleaseF;
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
			else
			{
				if (voice.Adsr.DFrames != 0)
				{
					voice.AdsrVolume += voice.Adsr.DVolume;	// Delta

					if (--voice.Adsr.DFrames <= 0)
						voice.AdsrVolume = voice.Instrument.Envelope.DVolume << 8;
				}
				else
				{
					if (voice.Adsr.SFrames != 0)
						voice.Adsr.SFrames--;
					else
					{
						if (voice.Adsr.RFrames != 0)
						{
							voice.AdsrVolume += voice.Adsr.RVolume;	// Delta

							if (--voice.Adsr.RFrames <= 0)
								voice.AdsrVolume = voice.Instrument.Envelope.RVolume << 8;
						}
					}
				}
			}

			// Volume slide
			voice.NoteMaxVolume = voice.NoteMaxVolume + voice.VolumeSlideUp - voice.VolumeSlideDown;

			if (voice.NoteMaxVolume < 0)
				voice.NoteMaxVolume = 0;

			if (voice.NoteMaxVolume > 0x40)
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
			if ((voice.Instrument != null) && (voice.PerfCurrent < voice.Instrument.PlayList.Length))
			{
				if (--voice.PerfWait <= 0)
				{
					int cur = voice.PerfCurrent++;
					voice.PerfWait = voice.PerfSpeed;

					AhxPListEntry entry = voice.PerfList.Entries[cur];

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
						PListCommandParse(v, entry.Fx[i], entry.FxParam[i]);

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
						else
						{
							if (d3 >= d2)
							{
								voice.SquareSlidingIn = true;
								voice.SquareSign = -1;
							}
						}
					}

					if ((d3 == d1) || (d3 == d2))
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
					else
					{
						if (d3 >= d2)
						{
							voice.FilterSlidingIn = true;
							voice.FilterSign = -1;
						}
					}
				}

				int fMax = (voice.FilterSpeed < 3) ? (5 - voice.FilterSpeed) : 1;

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

				voice.FilterPos = d3;
				voice.NewWaveform = true;
				voice.FilterWait = voice.FilterSpeed - 3;

				if (voice.FilterWait < 1)
					voice.FilterWait = 1;
			}

			if ((voice.Waveform == 3 - 1) || voice.PlantSquare)
			{
				// Calc square
				sbyte[] squarePtr = waves.filterSets[ToSixtyTwo(voice.FilterPos - 1)].Squares;
				int x = voice.SquarePos << (5 - voice.WaveLength);

				if (x > 0x20)
				{
					x = 0x40 - x;
					voice.SquareReverse = true;
				}

				// Range fix by Thomas Neumann
				if (--x < 0)
					x = 0;

				int squareOffset = x << 7;

				int delta = 32 >> voice.WaveLength;

				for (int i = 0; i < (1 << voice.WaveLength) * 4; i++)
				{
					voice.SquareTempBuffer[i] = squarePtr[squareOffset];
					squareOffset += delta;
				}

				voice.AudioSource = voice.SquareTempBuffer;
				voice.AudioOffset = 0;

				voice.NewWaveform = true;
				voice.Waveform = 3 - 1;
				voice.PlantSquare = false;
			}

			if (voice.Waveform == 4 - 1)
				voice.NewWaveform = true;

			if (voice.NewWaveform)
			{
				// Don't process squares
				if (voice.Waveform != 3 - 1)
				{
					int filterSet = ToSixtyTwo(voice.FilterPos - 1);

					if (voice.Waveform == 4 - 1)	// White noise
					{
						voice.AudioSource = waves.filterSets[filterSet].WhiteNoiseBig;
						voice.AudioOffset = (playingInfo.WnRandom & (2 * 0x280 - 1)) & ~1;

						// Go on random
						playingInfo.WnRandom += 2239384;
						playingInfo.WnRandom = ((((playingInfo.WnRandom >> 8) | (playingInfo.WnRandom << 24)) + 782323) ^ 75) - 6735;
					}
					else if (voice.Waveform == 1 - 1)	// Triangle
					{
						voice.AudioSource = waves.filterSets[filterSet].Triangles;
						voice.AudioOffset = Tables.OffsetTable[voice.WaveLength];
					}
					else if (voice.Waveform == 2 - 1)	// Sawtooth
					{
						voice.AudioSource = waves.filterSets[filterSet].Sawtooths;
						voice.AudioOffset = Tables.OffsetTable[voice.WaveLength];
					}
				}
			}

			voice.AudioPeriod = voice.InstrPeriod;

			if (!voice.FixedNote)
				voice.AudioPeriod += voice.Transpose + voice.TrackPeriod - 1;

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
			voice.AudioVolume = ((((((((voice.AdsrVolume >> 8) * voice.NoteMaxVolume) >> 6) * voice.PerfSubVolume) >> 6) * voice.TrackMasterVolume) >> 6) * playingInfo.MainVolume) >> 6;
		}



		/********************************************************************/
		/// <summary>
		/// Parses PList commands
		/// </summary>
		/********************************************************************/
		private void PListCommandParse(int v, int fx, int fxParam)
		{
			AhxVoices voice = voices[v];

			switch (fx)
			{
				// Set filter
				case 0:
				{
					if ((song.Revision > 0) && (fxParam != 0))
					{
						if (voice.IgnoreFilter)
						{
							voice.FilterPos = 1;
							voice.IgnoreFilter = false;
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

				// Start/stop modulation
				case 4:
				{
					if ((song.Revision == 0) || (fxParam == 0))
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

				// Set volume
				case 6:
				{
					if (fxParam > 0x40)
					{
						if ((fxParam -= 0x50) >= 0)
						{
							if (fxParam <= 0x40)
								voice.PerfSubVolume = fxParam;
							else
							{
								if ((fxParam -= 0xa0 - 0x50) >= 0)
								{
									if (fxParam <= 0x40)
										voice.TrackMasterVolume = fxParam;
								}
							}
						}
					}
					else
						voice.NoteMaxVolume = fxParam;

					break;
				}

				// Set speed
				case 7:
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
			AhxVoices voice = voices[v];
			IChannel channel = VirtualChannels[v];

			if (!voice.TrackOn)
			{
				voice.VoiceVolume = 0;
				return;
			}

			voice.VoiceVolume = voice.AudioVolume;
			channel.SetAmigaVolume((ushort)voice.VoiceVolume);

			if (voice.PlantPeriod)
			{
				voice.PlantPeriod = false;
				voice.VoicePeriod = voice.AudioPeriod;
				channel.SetAmigaPeriod((uint)voice.VoicePeriod);
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

				// To avoid clicks in Stranglehold, we do only kick the playing buffer once and then change it doing loops
				if (!voice.WaveformStarted)
				{
					voice.WaveformStarted = true;
					channel.PlaySample((short)(voice.InstrumentNumber - 1), voice.VoiceBuffer, 0, 0x280);
				}

				if (voice.KickNote)
				{
					voice.KickNote = false;
					channel.SetSampleNumber((short)(voice.InstrumentNumber - 1));
				}

				channel.SetLoop(0, 0x280);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Test for bounds values
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int ToSixtyTwo(int a)
		{
			if (a < 0)
				a = 0;
			else if (a > 62)
				a = 62;

			return a;
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

			for (int i = 0; i < 4; i++)
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
