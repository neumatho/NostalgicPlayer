/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Runtime.CompilerServices;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.Ahx.Containers;
using Polycode.NostalgicPlayer.Agent.Player.Ahx.Implementation;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Events;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.Ahx
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class AhxWorker : ModulePlayerAgentBase
	{
		private static readonly Dictionary<Guid, ModuleType> moduleTypeLookup = new Dictionary<Guid, ModuleType>
		{
			{ Ahx.Agent1Id, ModuleType.Ahx1 },
			{ Ahx.Agent2Id, ModuleType.Ahx2 }
		};

		private readonly ModuleType currentModuleType;

		private DurationInfo[] allDurations;
		private int currentSong;

		private AhxWaves waves;
		private AhxOutput output;

		private uint bufLen;
		private short[][] sampBuffers;

		public AhxSong song;

		public readonly AhxVoices[] voices = Helpers.InitializeArray<AhxVoices>(4);

		private int stepWaitFrames;
		private bool getNewPosition;

		private bool patternBreak;
		private int mainVolume;

		private int tempo;

		private int posNr;
		private int posJump;

		private int noteNr;
		private int posJumpNote;

		private int wnRandom;

		private int calculateSubSong;

		private const int InfoSpeedLine = 3;

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
				// Song length
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
					value = song.TrackNr.ToString();
					break;
				}

				// Used instruments
				case 2:
				{
					description = Resources.IDS_AHX_INFODESCLINE2;
					value = song.InstrumentNr.ToString();
					break;
				}

				// Current speed
				case 3:
				{
					description = Resources.IDS_AHX_INFODESCLINE3;
					value = tempo.ToString();
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
		public override ModulePlayerSupportFlag SupportFlags => ModulePlayerSupportFlag.BufferMode | ModulePlayerSupportFlag.SetPosition;



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
				song.SubSongNr = moduleStream.Read_UINT8();

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

				// Read sub-songs
				song.SubSongs = new int[song.SubSongNr];

				for (int i = 0; i < song.SubSongNr; i++)
				{
					song.SubSongs[i] = moduleStream.Read_B_UINT16();

					// Do we need to fix the start index?
					if (song.SubSongs[i] >= song.PositionNr)
						song.SubSongs[i] = 0;
				}

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
					song.Tracks[i] = Helpers.InitializeArray<AhxStep>(song.TrackLength);

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
				song.Instruments = Helpers.InitializeArray<AhxInstrument>(song.InstrumentNr + 1);

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
			// Allocate helper classes
			output = new AhxOutput();
			waves = new AhxWaves();

			return base.InitPlayer(out errorMessage);
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
		public override bool InitSound(int songNumber, DurationInfo durationInfo, out string errorMessage)
		{
			// Initialize the player
			InitSubSong(songNumber);

			return base.InitSound(songNumber, durationInfo, out errorMessage);
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the duration for all sub-songs
		/// </summary>
		/********************************************************************/
		public override DurationInfo[] CalculateDuration()
		{
			allDurations = CalculateDurationBySongPosition(true);

			return allDurations;
		}



		/********************************************************************/
		/// <summary>
		/// Set the output frequency and number of channels
		/// </summary>
		/********************************************************************/
		public override void SetOutputFormat(uint mixerFrequency, int channels)
		{
			base.SetOutputFormat(mixerFrequency, channels);

			// Allocate channel buffers
			sampBuffers = new short[4][];

			bufLen = mixerFreq / 50;

			for (int i = 0; i < 4; i++)
				sampBuffers[i] = new short[bufLen];

			// Initialize the output class
			output.Init(this, (int)mixerFreq, 16, 1, 1.0f, 50);
		}



		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public override void Play()
		{
			// Play and prepare the sample buffers
			output.PrepareBuffers(sampBuffers);

			// Feed NostalgicPlayer with the data
			for (int i = 0; i < 4; i++)
			{
				IChannel channel = VirtualChannels[i];

				channel.PlayBuffer(sampBuffers[i], 0, bufLen, 16);
				channel.SetFrequency(mixerFreq);
				channel.SetVolume(256);
				channel.SetPanning((ushort)(((i % 3) == 0) ? ChannelPanning.Left : ChannelPanning.Right));
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return information about sub-songs
		/// </summary>
		/********************************************************************/
		public override SubSongInfo SubSongs => new SubSongInfo(song.SubSongNr + 1, 0);



		/********************************************************************/
		/// <summary>
		/// Return the length of the current song
		/// </summary>
		/********************************************************************/
		public override int SongLength => song.PositionNr;



		/********************************************************************/
		/// <summary>
		/// Return the current position of the song
		/// </summary>
		/********************************************************************/
		public override int GetSongPosition()
		{
			return posNr;
		}



		/********************************************************************/
		/// <summary>
		/// Set a new position of the song
		/// </summary>
		/********************************************************************/
		public override void SetSongPosition(int position, PositionInfo positionInfo)
		{
			// Change the position
			posNr = position;
			noteNr = 0;
			posJumpNote = 0;
			stepWaitFrames = 0;
			patternBreak = false;
			getNewPosition = true;

			// Set the tempo
			tempo = positionInfo.Speed;

			// Change the module info
			OnModuleInfoChanged(InfoSpeedLine, tempo.ToString());
			OnSubSongChanged(new SubSongChangedEventArgs(allDurations[currentSong].PositionInfo[position].SubSong, allDurations[currentSong]));
		}



		/********************************************************************/
		/// <summary>
		/// Returns all the samples available in the module. If none, null
		/// is returned
		/// </summary>
		/********************************************************************/
		public override SampleInfo[] Samples
		{
			get
			{
				List<SampleInfo> result = new List<SampleInfo>();

				for (int i = 1; i <= song.InstrumentNr; i++)
				{
					AhxInstrument inst = song.Instruments[i];

					SampleInfo sampleInfo = new SampleInfo
					{
						Name = inst.Name,
						Flags = SampleInfo.SampleFlags.None,
						Type = SampleInfo.SampleType.Synth,
						BitSize = 8,
						MiddleC = 4144,
						Volume = (ushort)(inst.Volume * 4),
						Panning = -1,
						Sample = null,
						Length = 0,
						LoopStart = 0,
						LoopLength = 0
					};

					result.Add(sampleInfo);
				}

				return result.ToArray();
			}
		}
		#endregion

		#region Duration calculation methods
		/********************************************************************/
		/// <summary>
		/// Initialize all internal structures when beginning duration
		/// calculation on a new sub-song
		/// </summary>
		/********************************************************************/
		protected override int InitDurationCalculationByStartPos(int startPosition)
		{
			if (startPosition == 0)
				calculateSubSong = 0;
			else
			{
				calculateSubSong++;
				if (calculateSubSong > song.SubSongNr)
					return -1;
			}

			InitSubSong(calculateSubSong);
			SetOutputFormat(44100, 2);		// Just use some defaults while calculation the duration

			return posNr;
		}



		/********************************************************************/
		/// <summary>
		/// Return the current speed
		/// </summary>
		/********************************************************************/
		protected override byte GetCurrentSpeed()
		{
			return (byte)tempo;
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// This is the main play method
		/// </summary>
		/********************************************************************/
		public void PlayIrq()
		{
			if (stepWaitFrames <= 0)
			{
				if (getNewPosition)
				{
					int nextPos = posNr + 1 == song.PositionNr ? 0 : posNr + 1;

					for (int i = 0; i < 4; i++)
					{
						voices[i].track = song.Positions[posNr].Track[i];
						voices[i].transpose = song.Positions[posNr].Transpose[i];
						voices[i].nextTrack = song.Positions[nextPos].Track[i];
						voices[i].nextTranspose = song.Positions[nextPos].Transpose[i];
					}

					getNewPosition = false;

					// Tell NostalgicPlayer we have changed the position
					OnPositionChanged();

					if (allDurations != null)
						ChangeSubSong(allDurations[currentSong].PositionInfo[posNr].SubSong);
				}

				for (int i = 0; i < 4; i++)
					ProcessStep(i);

				stepWaitFrames = tempo;
			}

			// Do frame stuff
			for (int i = 0; i < 4; i++)
				ProcessFrame(i);

			if ((tempo > 0) && (--stepWaitFrames <= 0))
			{
				if (!patternBreak)
				{
					noteNr++;

					if (noteNr >= song.TrackLength)
					{
						posJump = posNr + 1;
						posJumpNote = 0;
						patternBreak = true;
					}
				}

				if (patternBreak)
				{
					if (posJump <= posNr)
					{
						// Tell NostalgicPlayer we have changed the position and
						// the module has ended
						OnPositionChanged();
						OnEndReached();
					}

					patternBreak = false;
					noteNr = posJumpNote;
					posJumpNote = 0;
					posNr = posJump;
					posJump = 0;

					if (posNr == song.PositionNr)
					{
						posNr = song.Restart;

						// Tell NostalgicPlayer we have changed the position and
						// the module has ended
						OnPositionChanged();
						OnEndReached();

						if (allDurations != null)
							ChangeSubSong(allDurations[currentSong].PositionInfo[posNr].SubSong);
					}

					getNewPosition = true;
				}
			}

			// Remain position
			for (int a = 0; a < 4; a++)
				SetAudio(a);
		}

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
		/// Frees all the memory the player have allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			output = null;
			waves = null;

			sampBuffers = null;

			song = null;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the player to use the sub-song given
		/// </summary>
		/********************************************************************/
		private void InitSubSong(int nr)
		{
			if (nr > song.SubSongNr)
				return;

			if (nr == 0)
				posNr = 0;
			else
				posNr = song.SubSongs[nr - 1];

			currentSong = nr;
			posJump = 0;
			patternBreak = false;
			mainVolume = 0x40;
			noteNr = 0;
			posJumpNote = 0;
			tempo = 6;
			stepWaitFrames = 0;
			getNewPosition = true;

			for (int v = 0; v < 4; v++)
				voices[v].Init();
		}



		/********************************************************************/
		/// <summary>
		/// Will change the sub-song if needed
		/// </summary>
		/********************************************************************/
		private void ChangeSubSong(int subSong)
		{
			if (subSong != currentSong)
			{
				currentSong = subSong;
				OnSubSongChanged(new SubSongChangedEventArgs(subSong, allDurations[subSong]));
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse the next step
		/// </summary>
		/********************************************************************/
		private void ProcessStep(int v)
		{
			AhxVoices voice = voices[v];

			if (!voice.trackOn)
				return;

			voice.volumeSlideUp = 0;
			voice.volumeSlideDown = 0;

			AhxStep step = song.Tracks[song.Positions[posNr].Track[v]][noteNr];
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
						posJump = fxParam & 0x0f;

					break;
				}

				case 0x5:	// Volume slide + Tone portamento
				case 0xa:	// Volume slide
				{
					voice.volumeSlideDown = fxParam & 0x0f;
					voice.volumeSlideUp = fxParam >> 4;
					break;
				}

				// Position jump
				case 0xb:
				{
					posJump = posJump * 100 + (fxParam & 0x0f) + (fxParam >> 4) * 10;
					patternBreak = true;
					break;
				}

				// Pattern break
				case 0xd:
				{
					posJump = posNr + 1;
					posJumpNote = song.Revision == 0 ? 0 : (fxParam & 0x0f) + (fxParam >> 4) * 10;

					if (posJumpNote > song.TrackLength)
						posJumpNote = 0;

					patternBreak = true;
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
							if ((fxParam & 0x0f) < tempo)
							{
								voice.noteCutWait = fxParam & 0x0f;

								if (voice.noteCutWait != 0)
								{
									voice.noteCutOn = true;
									voice.hardCutRelease = false;
								}
							}
							break;
						}

						// Note delay
						case 0xd:
						{
							if (voice.noteDelayOn)
								voice.noteDelayOn = false;
							else
							{
								if ((fxParam & 0x0f) < tempo)
								{
									voice.noteDelayWait = fxParam & 0x0f;

									if (voice.noteDelayWait != 0)
									{
										voice.noteDelayOn = true;
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
					tempo = fxParam;

					// Tell NostalgicPlayer to end the module if tempo is 0
					if (tempo == 0)
						OnEndReached();

					// Change the module info
					OnModuleInfoChanged(InfoSpeedLine, tempo.ToString());
					break;
				}
			}

			// Instrument range check by Thomas Neumann
			if ((instrument != 0) && (instrument <= song.InstrumentNr))
			{
				voice.perfSubVolume = 0x40;
				voice.periodSlideSpeed = 0;
				voice.periodSlidePeriod = 0;
				voice.periodSlideLimit = 0;
				voice.adsrVolume = 0;
				voice.instrument = song.Instruments[instrument];
				voice.CalcAdsr();

				// Init on instrument
				voice.waveLength = voice.instrument.WaveLength;
				voice.noteMaxVolume = voice.instrument.Volume;

				// Init vibrato
				voice.vibratoCurrent = 0;
				voice.vibratoDelay = voice.instrument.VibratoDelay;
				voice.vibratoDepth = voice.instrument.VibratoDepth;
				voice.vibratoSpeed = voice.instrument.VibratoSpeed;
				voice.vibratoPeriod = 0;

				// Init hard cut
				voice.hardCutRelease = voice.instrument.HardCutRelease;
				voice.hardCut = voice.instrument.HardCutReleaseFrames;

				// Init square
				voice.ignoreSquare = false;
				voice.squareSlidingIn = false;
				voice.squareWait = 0;
				voice.squareOn = false;

				int squareLower = voice.instrument.SquareLowerLimit >> (5 - voice.waveLength);
				int squareUpper = voice.instrument.SquareUpperLimit >> (5 - voice.waveLength);

				if (squareUpper < squareLower)
					(squareUpper, squareLower) = (squareLower, squareUpper);

				voice.squareUpperLimit = squareUpper;
				voice.squareLowerLimit = squareLower;

				// Init filter
				voice.ignoreFilter = false;
				voice.filterWait = 0;
				voice.filterOn = false;
				voice.filterSlidingIn = false;

				int d6 = voice.instrument.FilterSpeed;
				int d3 = voice.instrument.FilterLowerLimit;
				int d4 = voice.instrument.FilterUpperLimit;

				if ((d3 & 0x80) != 0)
					d6 |= 0x20;

				if ((d4 & 0x80) != 0)
					d6 |= 0x40;

				voices[v].filterSpeed = d6;

				d3 &= ~0x80;
				d4 &= ~0x80;

				if (d3 > d4)
					(d4, d3) = (d3, d4);

				voice.filterUpperLimit = d4;
				voice.filterLowerLimit = d3;
				voice.filterPos = 32;

				// Init perf list
				voice.perfWait = 0;
				voice.perfCurrent = 0;
				voice.perfSpeed = voice.instrument.PlayList.Speed;
				voice.perfList = voice.instrument.PlayList;
			}

			voice.periodSlideOn = false;

			bool skipNoteKick = false;

			switch (fx)
			{
				// Override filter
				case 0x4:
					break;

				// Set square wave offset
				case 0x9:
				{
					voice.squarePos = fxParam >> (5 - voices[v].waveLength);
					voice.plantSquare = true;
					voice.ignoreSquare = true;
					break;
				}

				case 0x5:	// Tone portamento + Volume slide
				case 0x3:	// Tone portamento (period slide up/down w/ limit)
				{
					if (fxParam != 0)
						voice.periodSlideSpeed = fxParam;

					if (note != 0)
					{
						int neue = Tables.PeriodTable[note];
						int alte = Tables.PeriodTable[voice.trackPeriod];

						alte -= neue;
						neue = alte + voice.periodSlidePeriod;

						if (neue != 0)
							voice.periodSlideLimit = -alte;
					}

					voice.periodSlideOn = true;
					voice.periodSlideWithLimit = true;

					skipNoteKick = true;
					break;
				}
			}

			if (!skipNoteKick)
			{
				// Note kicking
				if (note != 0)
				{
					voice.trackPeriod = note;
					voice.plantPeriod = true;
				}
			}

			switch (fx)
			{
				// Portamento up (period slide down)
				case 0x1:
				{
					voice.periodSlideSpeed = -fxParam;
					voice.periodSlideOn = true;
					voice.periodSlideWithLimit = false;
					break;
				}

				// Portamento down (period slide up)
				case 0x2:
				{
					voice.periodSlideSpeed = fxParam;
					voice.periodSlideOn = true;
					voice.periodSlideWithLimit = false;
					break;
				}

				// Volume
				case 0xc:
				{
					if (fxParam <= 0x40)
						voice.noteMaxVolume = fxParam;
					else
					{
						if (fxParam >= 0x50)
						{
							fxParam -= 0x50;

							if (fxParam <= 0x40)
							{
								for (int i = 0; i < 4; i++)
									voices[i].trackMasterVolume = fxParam;
							}
							else
							{
								fxParam -= (0xa0 - 0x50);

								if (fxParam <= 0x40)
									voice.trackMasterVolume = fxParam;
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
							voice.periodSlidePeriod = -(fxParam & 0x0f);
							voice.plantPeriod = true;
							break;
						}

						// Fine slide down (period fine slide up)
						case 0x2:
						{
							voice.periodSlidePeriod = fxParam & 0x0f;
							voice.plantPeriod = true;
							break;
						}

						// Vibrato control
						case 0x4:
						{
							voice.vibratoDepth = fxParam & 0x0f;
							break;
						}

						// Fine volume up
						case 0xa:
						{
							voice.noteMaxVolume += fxParam & 0x0f;

							if (voice.noteMaxVolume > 0x40)
								voice.noteMaxVolume = 0x40;

							break;
						}

						// Fine volume down
						case 0xb:
						{
							voice.noteMaxVolume -= fxParam & 0x0f;

							if (voice.noteMaxVolume < 0)
								voice.noteMaxVolume = 0;

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

			if (!voice.trackOn)
				return;

			if (voice.noteDelayOn)
			{
				if (voice.noteDelayWait <= 0)
					ProcessStep(v);
				else
					voice.noteDelayWait--;
			}

			if (voice.hardCut != 0)
			{
				int nextInstrument;

				if ((noteNr + 1) < song.TrackLength)
					nextInstrument = song.Tracks[voice.track][noteNr + 1].Instrument;
				else
					nextInstrument = song.Tracks[voice.nextTrack][0].Instrument;

				if (nextInstrument != 0)
				{
					int d1 = tempo - voice.hardCut;

					if (d1 < 0)
						d1 = 0;

					if (!voice.noteCutOn)
					{
						voice.noteCutOn = true;
						voice.noteCutWait = d1;
						voice.hardCutReleaseF = -(d1 - tempo);
					}
					else
						voice.hardCut = 0;
				}
			}

			if (voice.noteCutOn)
			{
				if (voice.noteCutWait <= 0)
				{
					voice.noteCutOn = false;

					if (voice.hardCutRelease)
					{
						voice.adsr.RVolume = -(voice.adsrVolume - (voice.instrument.Envelope.RVolume << 8)) / voice.hardCutReleaseF;
						voice.adsr.RFrames = voice.hardCutReleaseF;
						voice.adsr.AFrames = 0;
						voice.adsr.DFrames = 0;
						voice.adsr.SFrames = 0;
					}
					else
						voice.noteMaxVolume = 0;
				}
				else
					voice.noteCutWait--;
			}

			// ADSR envelope
			if (voice.adsr.AFrames != 0)
			{
				voice.adsrVolume += voice.adsr.AVolume;		// Delta

				if (--voice.adsr.AFrames <= 0)
					voice.adsrVolume = voice.instrument.Envelope.AVolume << 8;
			}
			else
			{
				if (voice.adsr.DFrames != 0)
				{
					voice.adsrVolume += voice.adsr.DVolume;	// Delta

					if (--voice.adsr.DFrames <= 0)
						voice.adsrVolume = voice.instrument.Envelope.DVolume << 8;
				}
				else
				{
					if (voice.adsr.SFrames != 0)
						voice.adsr.SFrames--;
					else
					{
						if (voice.adsr.RFrames != 0)
						{
							voice.adsrVolume += voice.adsr.RVolume;	// Delta

							if (--voice.adsr.RFrames <= 0)
								voice.adsrVolume = voice.instrument.Envelope.RVolume << 8;
						}
					}
				}
			}

			// Volume slide
			voice.noteMaxVolume = voice.noteMaxVolume + voice.volumeSlideUp - voice.volumeSlideDown;

			if (voice.noteMaxVolume < 0)
				voice.noteMaxVolume = 0;

			if (voice.noteMaxVolume > 0x40)
				voice.noteMaxVolume = 0x40;

			// Portamento
			if (voice.periodSlideOn)
			{
				if (voice.periodSlideWithLimit)
				{
					int d0 = voice.periodSlidePeriod - voice.periodSlideLimit;
					int d2 = voice.periodSlideSpeed;

					if (d0 > 0)
						d2 = -d2;

					if (d0 != 0)
					{
						int d3 = (d0 + d2) ^ d0;

						if (d3 >= 0)
							d0 = voice.periodSlidePeriod + d2;
						else
							d0 = voice.periodSlideLimit;

						voice.periodSlidePeriod = d0;
						voice.plantPeriod = true;
					}
				}
				else
				{
					voice.periodSlidePeriod += voice.periodSlideSpeed;
					voice.plantPeriod = true;
				}
			}

			// Vibrato
			if (voice.vibratoDepth != 0)
			{
				if (voice.vibratoDelay <= 0)
				{
					voice.vibratoPeriod = (Tables.VibratoTable[voice.vibratoCurrent] * voice.vibratoDepth) >> 7;
					voice.plantPeriod = true;
					voice.vibratoCurrent = (voice.vibratoCurrent + voice.vibratoSpeed) & 0x3f;
				}
				else
					voice.vibratoDelay--;
			}

			// PList
			if ((voice.instrument != null) && (voice.perfCurrent < voice.instrument.PlayList.Length))
			{
				if (--voice.perfWait <= 0)
				{
					int cur = voice.perfCurrent++;
					voice.perfWait = voice.perfSpeed;

					AhxPListEntry entry = voice.perfList.Entries[cur];

					if (entry.Waveform != 0)
					{
						voice.waveform = entry.Waveform - 1;
						voice.newWaveform = true;
						voice.periodPerfSlideSpeed = 0;
						voice.periodPerfSlidePeriod = 0;
					}

					// Hold wave
					voice.periodPerfSlideOn = false;

					for (int i = 0; i < 2; i++)
						PListCommandParse(v, entry.Fx[i], entry.FxParam[i]);

					// Get note
					if (entry.Note != 0)
					{
						voice.instrPeriod = entry.Note;
						voice.plantPeriod = true;
						voice.fixedNote = entry.Fixed;
					}
				}
			}
			else
			{
				if (voice.perfWait != 0)
					voice.perfWait--;
				else
					voice.periodPerfSlideSpeed = 0;
			}

			// Perf portamento
			if (voice.periodPerfSlideOn)
			{
				voice.periodPerfSlidePeriod -= voice.periodPerfSlideSpeed;

				if (voice.periodPerfSlidePeriod != 0)
					voice.plantPeriod = true;
			}

			if ((voice.waveform == 3 - 1) && voice.squareOn)
			{
				if (--voice.squareWait <= 0)
				{
					int d1 = voice.squareLowerLimit;
					int d2 = voice.squareUpperLimit;
					int d3 = voice.squarePos;

					if (voice.squareInit)
					{
						voice.squareInit = false;

						if (d3 <= d1)
						{
							voice.squareSlidingIn = true;
							voice.squareSign = 1;
						}
						else
						{
							if (d3 >= d2)
							{
								voice.squareSlidingIn = true;
								voice.squareSign = -1;
							}
						}
					}

					if ((d3 == d1) || (d3 == d2))
					{
						if (voice.squareSlidingIn)
							voice.squareSlidingIn = false;
						else
							voice.squareSign = -voice.squareSign;
					}

					d3 += voice.squareSign;
					voice.squarePos = d3;
					voice.plantSquare = true;
					voice.squareWait = voice.instrument.SquareSpeed;
				}
			}

			if (voice.filterOn && (--voice.filterWait <= 0))
			{
				int d1 = voice.filterLowerLimit;
				int d2 = voice.filterUpperLimit;
				int d3 = voice.filterPos;

				if (voice.filterInit)
				{
					voice.filterInit = false;

					if (d3 <= d1)
					{
						voice.filterSlidingIn = true;
						voice.filterSign = 1;
					}
					else
					{
						if (d3 >= d2)
						{
							voice.filterSlidingIn = true;
							voice.filterSign = -1;
						}
					}
				}

				int fMax = (voice.filterSpeed < 3) ? (5 - voice.filterSpeed) : 1;

				for (int i = 0; i < fMax; i++)
				{
					if ((d1 == d3) || (d2 == d3))
					{
						if (voice.filterSlidingIn)
							voice.filterSlidingIn = false;
						else
							voice.filterSign = -voice.filterSign;
					}

					d3 += voice.filterSign;
				}

				voice.filterPos = d3;
				voice.newWaveform = true;
				voice.filterWait = voice.filterSpeed - 3;

				if (voice.filterWait < 1)
					voice.filterWait = 1;
			}

			if ((voice.waveform == 3 - 1) || voice.plantSquare)
			{
				// Calc square
				sbyte[] squarePtr = waves.filterSets[ToSixtyTwo(voice.filterPos - 1)].Squares;
				int x = voice.squarePos << (5 - voice.waveLength);

				if (x > 0x20)
				{
					x = 0x40 - x;
					voice.squareReverse = true;
				}

				// Range fix by Thomas Neumann
				if (--x < 0)
					x = 0;

				int squareOffset = x << 7;

				int delta = 32 >> voice.waveLength;

				for (int i = 0; i < (1 << voice.waveLength) * 4; i++)
				{
					voice.squareTempBuffer[i] = squarePtr[squareOffset];
					squareOffset += delta;
				}

				voice.audioSource = voice.squareTempBuffer;
				voice.audioOffset = 0;

				voice.newWaveform = true;
				voice.waveform = 3 - 1;
				voice.plantSquare = false;
			}

			if (voice.waveform == 4 - 1)
				voice.newWaveform = true;

			if (voice.newWaveform)
			{
				// Don't process squares
				if (voice.waveform != 3 - 1)
				{
					int filterSet = ToSixtyTwo(voice.filterPos - 1);

					if (voice.waveform == 4 - 1)	// White noise
					{
						voice.audioSource = waves.filterSets[filterSet].WhiteNoiseBig;
						voice.audioOffset = (wnRandom & (2 * 0x280 - 1)) & ~1;

						// Go on random
						wnRandom += 2239384;
						wnRandom = ((((wnRandom >> 8) | (wnRandom << 24)) + 782323) ^ 75) - 6735;
					}
					else if (voice.waveform == 1 - 1)	// Triangle
					{
						voice.audioSource = waves.filterSets[filterSet].Triangles;
						voice.audioOffset = Tables.OffsetTable[voice.waveLength];
					}
					else if (voice.waveform == 2 - 1)	// Sawtooth
					{
						voice.audioSource = waves.filterSets[filterSet].Sawtooths;
						voice.audioOffset = Tables.OffsetTable[voice.waveLength];
					}
				}
			}

			voice.audioPeriod = voice.instrPeriod;

			if (!voice.fixedNote)
				voice.audioPeriod += voice.transpose + voice.trackPeriod - 1;

			if (voice.audioPeriod > 5 * 12)
				voice.audioPeriod = 5 * 12;

			if (voice.audioPeriod < 0)
				voice.audioPeriod = 0;

			voice.audioPeriod = Tables.PeriodTable[voice.audioPeriod];

			if (!voice.fixedNote)
				voice.audioPeriod += voice.periodSlidePeriod;

			voice.audioPeriod += voice.periodPerfSlidePeriod + voice.vibratoPeriod;

			if (voice.audioPeriod > 0x0d60)
				voice.audioPeriod = 0x0d60;

			if (voice.audioPeriod < 0x0071)
				voice.audioPeriod = 0x0071;

			// Audio init volume
			voice.audioVolume = ((((((((voice.adsrVolume >> 8) * voice.noteMaxVolume) >> 6) * voice.perfSubVolume) >> 6) * voice.trackMasterVolume) >> 6) * mainVolume) >> 6;
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
						if (voice.ignoreFilter)
						{
							voice.filterPos = 1;
							voice.ignoreFilter = false;
						}
						else
							voice.filterPos = fxParam;

						voice.newWaveform = true;
					}
					break;
				}

				// Slide up
				case 1:
				{
					voice.periodPerfSlideSpeed = fxParam;
					voice.periodPerfSlideOn = true;
					break;
				}

				// Slide down
				case 2:
				{
					voice.periodPerfSlideSpeed = -fxParam;
					voice.periodPerfSlideOn = true;
					break;
				}

				// Init square modulation
				case 3:
				{
					if (!voice.ignoreSquare)
						voice.squarePos = fxParam >> (5 - voice.waveLength);
					else
						voice.ignoreSquare = false;

					break;
				}

				// Start/stop modulation
				case 4:
				{
					if ((song.Revision == 0) || (fxParam == 0))
					{
						voice.squareOn = !voice.squareOn;
						voice.squareInit = voice.squareOn;
						voice.squareSign = 1;
					}
					else
					{
						if ((fxParam & 0x0f) != 0x00)
						{
							voice.squareOn = !voice.squareOn;
							voice.squareInit = voice.squareOn;
							voice.squareSign = 1;

							if ((fxParam & 0x0f) == 0x0f)
								voice.squareSign = -1;
						}

						if ((fxParam & 0xf0) != 0x00)
						{
							voice.filterOn = !voice.filterOn;
							voice.filterInit = voice.filterOn;
							voice.filterSign = 1;

							if ((fxParam & 0xf0) == 0xf0)
								voice.filterSign = -1;
						}
					}
					break;
				}

				// Jump to step
				case 5:
				{
					voice.perfCurrent = fxParam;
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
								voice.perfSubVolume = fxParam;
							else
							{
								if ((fxParam -= 0xa0 - 0x50) >= 0)
								{
									if (fxParam <= 0x40)
										voice.trackMasterVolume = fxParam;
								}
							}
						}
					}
					else
						voice.noteMaxVolume = fxParam;

					break;
				}

				// Set speed
				case 7:
				{
					voice.perfSpeed = fxParam;
					voice.perfWait = fxParam;
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

			if (!voice.trackOn)
			{
				voice.voiceVolume = 0;
				return;
			}

			voice.voiceVolume = voice.audioVolume;

			if (voice.plantPeriod)
			{
				voice.plantPeriod = false;
				voice.voicePeriod = voice.audioPeriod;
			}

			if (voice.newWaveform)
			{
				if (voice.waveform == 4 - 1)
					Array.Copy(voice.audioSource, voice.audioOffset, voice.voiceBuffer, 0, 0x280);
				else
				{
					int waveLoops = (1 << (5 - voice.waveLength)) * 5;
					int loopLen = 4 * (1 << voice.waveLength);

					if (voice.audioSource != null)
					{
						for (int i = 0; i < waveLoops; i++)
							Array.Copy(voice.audioSource, voice.audioOffset, voice.voiceBuffer, i * loopLen, loopLen);
					}
					else
					{
						for (int i = 0; i < waveLoops; i++)
							Array.Clear(voice.voiceBuffer, i * loopLen, loopLen);
					}
				}

				voice.voiceBuffer[0x280] = voice.voiceBuffer[0];
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
		#endregion
	}
}
