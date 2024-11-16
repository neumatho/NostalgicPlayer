/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.Fred.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.Fred
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class FredWorker : ModulePlayerWithSubSongDurationAgentBase
	{
		private const byte TrackEndCode = 0x80;
		private const byte TrackPortCode = 0x81;
		private const byte TrackTempoCode = 0x82;
		private const byte TrackInstCode = 0x83;
		private const byte TrackPauseCode = 0x84;
		private const byte TrackMaxCode = 0xa0;

		private ushort subSongNum;
		private ushort instNum;
		private int trackNum;

		private byte[] startTempos;
		private sbyte[][][] positions;
		private byte[][] tracks;
		private Instrument[] instruments;

		private int[,] positionLengths;
		private bool[,] hasNotes;

		private int currentSong;

		private GlobalPlayingInfo playingInfo;
		private ChannelInfo[] channels;

		private const int InfoPositionLine = 3;
		private const int InfoTrackLine = 4;
		private const int InfoSpeedLine = 5;

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => new [] { "frd", "fred" };



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			if (moduleStream.Length < 20)
				return AgentResult.Unknown;

			// Check the mark
			moduleStream.Seek(0, SeekOrigin.Begin);

			byte[] buf = new byte[12];
			moduleStream.ReadExactly(buf, 0, 12);

			if (Encoding.ASCII.GetString(buf, 0, 12) != "Fred Editor ")
				return AgentResult.Unknown;

			// Check the version
			if (moduleStream.Read_B_UINT16() != 0x0000)
				return AgentResult.Unknown;

			// Check the number of songs
			if (moduleStream.Read_B_UINT16() > 10)
				return AgentResult.Unknown;

			// Check the end mark
			moduleStream.Seek(-4, SeekOrigin.End);

			if (moduleStream.Read_B_UINT32() != 0x12345678)
				return AgentResult.Unknown;

			return AgentResult.Ok;
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
				// Number of positions:
				case 0:
				{
					description = Resources.IDS_FRED_INFODESCLINE0;
					value = FormatPositionLengths();
					break;
				}

				// Used tracks
				case 1:
				{
					description = Resources.IDS_FRED_INFODESCLINE1;
					value = trackNum.ToString();
					break;
				}

				// Used instruments
				case 2:
				{
					description = Resources.IDS_FRED_INFODESCLINE2;
					value = instNum.ToString();
					break;
				}

				// Playing positions
				case 3:
				{
					description = Resources.IDS_FRED_INFODESCLINE3;
					value = FormatPositions();
					break;
				}

				// Playing tracks
				case 4:
				{
					description = Resources.IDS_FRED_INFODESCLINE4;
					value = FormatTracks();
					break;
				}

				// Current speed
				case 5:
				{
					description = Resources.IDS_FRED_INFODESCLINE5;
					value = playingInfo.CurrentTempo.ToString();
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
				Encoding encoder = EncoderCollection.Amiga;

				// Skip the signature and version
				moduleStream.Seek(14, SeekOrigin.Begin);

				// Get number of sub-songs
				subSongNum = moduleStream.Read_B_UINT16();

				// Read the sub-song start tempos
				startTempos = new byte[subSongNum];
				int bytesRead = moduleStream.Read(startTempos, 0, subSongNum);

				if (bytesRead < subSongNum)
				{
					errorMessage = Resources.IDS_FRED_ERR_LOADING_HEADER;
					Cleanup();

					return AgentResult.Error;
				}

				// Allocate memory to hold the positions
				positions = new sbyte[subSongNum][][];

				// Take one sub-song at the time
				for (int i = 0; i < subSongNum; i++)
				{
					positions[i] = new sbyte[4][];

					// There are 4 channels in each sub-song
					for (int j = 0; j < 4; j++)
					{
						// Read one channel position data
						positions[i][j] = new sbyte[256];

						moduleStream.ReadSigned(positions[i][j], 0, 256);

						if (moduleStream.EndOfStream)
						{
							errorMessage = Resources.IDS_FRED_ERR_LOADING_HEADER;
							Cleanup();

							return AgentResult.Error;
						}
					}
				}

				// Allocate memory to hold the track data
				tracks = new byte[128][];

				// Now load the track data
				for (int i = 0; i < 128; i++)
				{
					// Read the current track size
					int trackSize = moduleStream.Read_B_INT32();

					// Allocate memory for the track data
					tracks[i] = new byte[trackSize];

					// Read the track data
					bytesRead = moduleStream.Read(tracks[i], 0, trackSize);

					// Did we get some problems?
					if (bytesRead < trackSize)
					{
						errorMessage = Resources.IDS_FRED_ERR_LOADING_SEQUENCES;
						Cleanup();

						return AgentResult.Error;
					}
				}

				// Get the number of instruments
				instNum = moduleStream.Read_B_UINT16();

				// Allocate memory to hold the instrument info
				instruments = new Instrument[instNum];

				// Read the instrument info
				byte[] name = new byte[33];
				Dictionary<uint, Instrument> instIndexMap = new Dictionary<uint, Instrument>();

				for (short i = 0; i < instNum; i++)
				{
					Instrument inst = new Instrument();

					inst.InstrumentNumber = i;

					moduleStream.ReadInto(name, 0, 32);
					inst.Name = encoder.GetString(name);

					uint instIndex = moduleStream.Read_B_UINT32();
					if (!instIndexMap.ContainsKey(instIndex))
						instIndexMap[instIndex] = inst;

					inst.RepeatLen = moduleStream.Read_B_UINT16();
					inst.Length = (ushort)(moduleStream.Read_B_UINT16() * 2);
					inst.Period = moduleStream.Read_B_UINT16();
					inst.VibDelay = moduleStream.Read_UINT8();
					moduleStream.Seek(1, SeekOrigin.Current);

					inst.VibSpeed = moduleStream.Read_INT8();
					inst.VibAmpl = moduleStream.Read_INT8();
					inst.EnvVol = moduleStream.Read_UINT8();
					inst.AttackSpeed = moduleStream.Read_UINT8();
					inst.AttackVolume = moduleStream.Read_UINT8();
					inst.DecaySpeed = moduleStream.Read_UINT8();
					inst.DecayVolume = moduleStream.Read_UINT8();
					inst.SustainDelay = moduleStream.Read_UINT8();
					inst.ReleaseSpeed = moduleStream.Read_UINT8();
					inst.ReleaseVolume = moduleStream.Read_UINT8();

					moduleStream.ReadSigned(inst.Arpeggio, 0, 16);

					inst.ArpSpeed = moduleStream.Read_UINT8();
					inst.InstType = (InstrumentType)moduleStream.Read_UINT8();
					inst.PulseRateMin = moduleStream.Read_INT8();
					inst.PulseRatePlus = moduleStream.Read_INT8();
					inst.PulseSpeed = moduleStream.Read_UINT8();
					inst.PulseStart = moduleStream.Read_UINT8();
					inst.PulseEnd = moduleStream.Read_UINT8();
					inst.PulseDelay = moduleStream.Read_UINT8();
					inst.InstSync = (SynchronizeFlag)moduleStream.Read_UINT8();
					inst.Blend = moduleStream.Read_UINT8();
					inst.BlendDelay = moduleStream.Read_UINT8();
					inst.PulseShotCounter = moduleStream.Read_UINT8();
					inst.BlendShotCounter = moduleStream.Read_UINT8();
					inst.ArpCount = moduleStream.Read_UINT8();

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_FRED_ERR_LOADING_INSTRUMENTS;
						Cleanup();

						return AgentResult.Error;
					}

					instruments[i] = inst;

					moduleStream.Seek(12, SeekOrigin.Current);
				}

				// Read number of samples
				ushort sampNum = moduleStream.Read_B_UINT16();

				for (int i = 0; i < sampNum; i++)
				{
					// Read the instrument index
					ushort instIndex = moduleStream.Read_B_UINT16();

					// Find the instrument info
					if (!instIndexMap.TryGetValue(instIndex, out Instrument inst))
					{
						errorMessage = Resources.IDS_FRED_ERR_LOADING_SAMPLES;
						Cleanup();

						return AgentResult.Error;
					}

					// Get the size of the sample data
					int sampSize = moduleStream.Read_B_UINT16();

					// Read the sample data
					inst.SampleAddr = moduleStream.ReadSampleData(instIndex, sampSize, out _);

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_FRED_ERR_LOADING_SAMPLES;
						Cleanup();

						return AgentResult.Error;
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
		public override bool InitPlayer(out string errorMessage)
		{
			if (!base.InitPlayer(out errorMessage))
				return false;

			CalculatePositionLengthsAndNumberOfTracks();

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

			InitializeSound(songNumber);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public override void Play()
		{
			// Well, play all channels
			for (int i = 0; i < 4; i++)
			{
				ChannelInfo chanInfo = channels[i];

				// Decrement the note delay counter
				chanInfo.TrackDuration--;

				// Check to see if we need to get the next track line
				if (chanInfo.TrackDuration == 0)
				{
					byte retVal = DoNewLine(chanInfo, VirtualChannels[i]);

					if (retVal == 1)	// Take the next voice
						continue;

					if (retVal == 2)	// Do the same voice again
					{
						i--;
						continue;
					}
				}
				else
				{
					// Check to see if we need to mute the channel
					if ((chanInfo.TrackDuration == 1) && (chanInfo.TrackTable[chanInfo.TrackPosition] < TrackMaxCode))
						VirtualChannels[i].Mute();
				}

				ModifySound(chanInfo, VirtualChannels[i]);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return information about sub-songs
		/// </summary>
		/********************************************************************/
		public override SubSongInfo SubSongs => new SubSongInfo(subSongNum, 0);



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
				foreach (Instrument inst in instruments)
				{
					// Build frequency table
					uint[] frequencies = new uint[10 * 12];

					for (int j = 0; j < 6 * 12; j++)
						frequencies[2 * 12 + j] = 3546895U / ((Tables.PeriodTable[j] * inst.Period) / 1024);

					SampleInfo sampleInfo = new SampleInfo
					{
						Name = inst.Name,
						Flags = SampleInfo.SampleFlag.None,
						Volume = inst.EnvVol,
						Panning = -1,
						NoteFrequencies = frequencies
					};

					if (inst.InstType != InstrumentType.Sample)
					{
						sampleInfo.Type = SampleInfo.SampleType.Synthesis;
						sampleInfo.Sample = null;
						sampleInfo.Length = 0;
						sampleInfo.LoopStart = 0;
						sampleInfo.LoopLength = 0;
					}
					else
					{
						sampleInfo.Type = SampleInfo.SampleType.Sample;
						sampleInfo.Sample = inst.SampleAddr;
						sampleInfo.Length = inst.Length;

						if ((inst.RepeatLen == 0) || (inst.RepeatLen == 0xffff))
						{
							// No loop
							sampleInfo.LoopStart = 0;
							sampleInfo.LoopLength = 0;
						}
						else
						{
							// Sample loops
							sampleInfo.Flags |= SampleInfo.SampleFlag.Loop;
							sampleInfo.LoopStart = 0;
							sampleInfo.LoopLength = inst.RepeatLen;
						}
					}

					yield return sampleInfo;
				}
			}
		}
		#endregion

		#region ModulePlayerWithSubSongDurationAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Initialize all internal structures when beginning duration
		/// calculation on a new sub-song
		/// </summary>
		/********************************************************************/
		protected override void InitDuration(int subSong)
		{
			InitializeSound(subSong);
		}



		/********************************************************************/
		/// <summary>
		/// Create a snapshot of all the internal structures and return it
		/// </summary>
		/********************************************************************/
		protected override ISnapshot CreateSnapshot()
		{
			return new Snapshot(playingInfo, channels);
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
			channels = clonedSnapshot.Channels;

			UpdateModuleInformation();

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Calculate the lengths of the position lists for each sub-song
		/// </summary>
		/********************************************************************/
		private void CalculatePositionLengthsAndNumberOfTracks()
		{
			positionLengths = new int[subSongNum, 4];
			hasNotes = new bool[subSongNum, 4];
			trackNum = 0;

			for (int i = 0; i < subSongNum; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					sbyte[] positionList = positions[i][j];

					for (int k = 0; k < positionList.Length; k++)
					{
						if (positionList[k] < 0)
						{
							positionLengths[i, j] = k;
							break;
						}

						trackNum = Math.Max(trackNum, positionList[k]);

						if (!hasNotes[i, j])
						{
							byte[] track = tracks[positionList[k]];

							for (int m = 0; ; m++)
							{
								byte cmd = track[m];

								if (cmd < 0x80)
								{
									hasNotes[i, j] = true;
									break;
								}

								if (cmd == TrackEndCode)
									break;

								switch (cmd)
								{
									case TrackInstCode:
									case TrackTempoCode:
									{
										m++;
										break;
									}

									case TrackPortCode:
									{
										m += 3;
										break;
									}
								}
							}
						}
					}
				}
			}

			trackNum++;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize sound structures
		/// </summary>
		/********************************************************************/
		private void InitializeSound(int songNumber)
		{
			// Remember the song number
			currentSong = songNumber;

			// Initialize player variables
			playingInfo = new GlobalPlayingInfo
			{
				CurrentTempo = startTempos[songNumber]
			};

			// Initialize the channel structure
			channels = new ChannelInfo[4];

			for (int i = 0; i < 4; i++)
			{
				sbyte[] positionTable = positions[songNumber][i];

				channels[i] = new ChannelInfo
				{
					ChanNum = i,
					PositionTable = positionTable,
					TrackTable = tracks[positionTable[0]],
					Position = 0,
					TrackPosition = 0,
					TrackDuration = 1,
					TrackNote = 0,
					TrackPeriod = 0,
					TrackVolume = 0,
					Instrument = null,
					VibFlags = VibFlags.None,
					VibDelay = 0,
					VibSpeed = 0,
					VibAmpl = 0,
					VibValue = 0,
					PortRunning = false,
					PortDelay = 0,
					PortLimit = 0,
					PortTargetNote = 0,
					PortStartPeriod = 0,
					PeriodDiff = 0,
					PortCounter = 0,
					PortSpeed = 0,
					EnvState = EnvelopeState.Attack,
					SustainDelay = 0,
					ArpPosition = 0,
					ArpSpeed = 0,
					PulseWay = false,
					PulsePosition = 0,
					PulseDelay = 0,
					PulseSpeed = 0,
					PulseShot = 0,
					BlendWay = false,
					BlendPosition = 0,
					BlendDelay = 0,
					BlendShot = 0,
					SynthSample = new sbyte[64]		// Well, it seems only 32 bytes are needed, but we allocate 64 just in case
				};
			}
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player has allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			startTempos = null;
			positions = null;
			tracks = null;
			instruments = null;

			playingInfo = null;
			channels = null;
		}



		/********************************************************************/
		/// <summary>
		/// Take the next track line and parse it
		/// </summary>
		/********************************************************************/
		private byte DoNewLine(ChannelInfo chanInfo, IChannel channel)
		{
			bool instChange = false;

			// For channels that does not contain any notes, mark them as done immediately
			if (!hasNotes[currentSong, chanInfo.ChanNum])
				OnEndReached(chanInfo.ChanNum);

			// Get the current track position
			ushort trackPos = chanInfo.TrackPosition;

			for (;;)
			{
				byte cmd = chanInfo.TrackTable[trackPos++];

				// Is the command a note?
				if (cmd < 0x80)
				{
					// Yes, play it
					Instrument inst = chanInfo.Instrument;

					// Store the new position
					chanInfo.TrackPosition = trackPos;

					// Do we play an invalid instrument?
					if (inst == null)
					{
						// Stop all effects
						chanInfo.PortRunning = false;
						chanInfo.VibFlags = 0;
						chanInfo.TrackVolume = 0;
						channel.Mute();

						// Take the next channel
						return 0;
					}

					// Initialize the channel
					chanInfo.TrackNote = cmd;
					chanInfo.ArpPosition = 0;
					chanInfo.ArpSpeed = inst.ArpSpeed;
					chanInfo.VibDelay = inst.VibDelay;
					chanInfo.VibSpeed = inst.VibSpeed;
					chanInfo.VibAmpl = inst.VibAmpl;
					chanInfo.VibFlags = VibFlags.VibDirection | VibFlags.PeriodDirection;
					chanInfo.VibValue = 0;

					// Create synth sample if the instrument used is a synth instrument
					if ((inst.InstType == InstrumentType.Pulse) && (instChange || ((inst.InstSync & SynchronizeFlag.PulseSync) != 0)))
						CreateSynthSamplePulse(inst, chanInfo);
					else
					{
						if ((inst.InstType == InstrumentType.Blend) && (instChange || ((inst.InstSync & SynchronizeFlag.BlendSync) != 0)))
							CreateSynthSampleBlend(inst, chanInfo);
					}

					// Set the track duration (speed)
					chanInfo.TrackDuration = playingInfo.CurrentTempo;

					// Play the instrument
					if (inst.InstType == InstrumentType.Sample)
					{
						if (inst.SampleAddr != null)
						{
							// Play sample
							channel.PlaySample(inst.InstrumentNumber, inst.SampleAddr, 0, inst.Length);

							if ((inst.RepeatLen != 0) && (inst.RepeatLen != 0xffff))
							{
								// There is no bug in this line. The original player calculate
								// the wrong start and length too!
								channel.SetLoop(inst.RepeatLen, (uint)inst.Length - inst.RepeatLen);
							}
						}
					}
					else
					{
						// Play synth sound
						channel.PlaySample(inst.InstrumentNumber, chanInfo.SynthSample, 0, inst.Length);
						channel.SetLoop(0, inst.Length);
					}

					// Set the volume (mute the channel)
					channel.SetVolume(0);

					chanInfo.TrackVolume = 0;
					chanInfo.EnvState = EnvelopeState.Attack;
					chanInfo.SustainDelay = inst.SustainDelay;

					// Set the period
					chanInfo.TrackPeriod = (ushort)((Tables.PeriodTable[chanInfo.TrackNote] * inst.Period) / 1024);
					channel.SetAmigaPeriod(chanInfo.TrackPeriod);

					// Initialize portamento
					if (chanInfo.PortRunning && (chanInfo.PortStartPeriod == 0))
					{
						chanInfo.PeriodDiff = (short)(chanInfo.PortLimit - chanInfo.TrackPeriod);
						chanInfo.PortCounter = 1;
						chanInfo.PortStartPeriod = chanInfo.TrackPeriod;
					}

					// Take the next channel
					return 0;
				}

				// It's a command
				switch (cmd)
				{
					// Change instrument
					case TrackInstCode:
					{
						byte newInst = chanInfo.TrackTable[trackPos++];

						if (newInst >= instNum)
							chanInfo.Instrument = null;
						else
						{
							// Find the instrument information
							chanInfo.Instrument = instruments[newInst];

							if (chanInfo.Instrument.InstType == InstrumentType.Unused)
								chanInfo.Instrument = null;
							else
								instChange = true;
						}
						break;
					}

					// Change tempo
					case TrackTempoCode:
					{
						playingInfo.CurrentTempo = chanInfo.TrackTable[trackPos++];

						ShowSpeed();
						break;
					}

					// Start portamento
					case TrackPortCode:
					{
						ushort instPeriod = 428;

						if (chanInfo.Instrument != null)
							instPeriod = chanInfo.Instrument.Period;

						chanInfo.PortSpeed = (ushort)(chanInfo.TrackTable[trackPos++] * playingInfo.CurrentTempo);
						chanInfo.PortTargetNote = chanInfo.TrackTable[trackPos++];
						chanInfo.PortLimit = (ushort)((Tables.PeriodTable[chanInfo.PortTargetNote] * instPeriod) / 1024);
						chanInfo.PortStartPeriod = 0;
						chanInfo.PortDelay = (ushort)(chanInfo.TrackTable[trackPos++] * playingInfo.CurrentTempo);
						chanInfo.PortRunning = true;
						break;
					}

					// Execute pause
					case TrackPauseCode:
					{
						chanInfo.TrackDuration = playingInfo.CurrentTempo;
						chanInfo.TrackPosition = trackPos;
						channel.Mute();

						// Take the next channel
						return 1;
					}

					// End, goto next pattern
					case TrackEndCode:
					{
						chanInfo.Position++;

						for (;;)
						{
							// If the song ends, start it over
							if (chanInfo.PositionTable[chanInfo.Position] == -1)
							{
								chanInfo.Position = 0;
								playingInfo.CurrentTempo = startTempos[currentSong];

								ShowSpeed();

								// Tell NostalgicPlayer that the channel has ended
								OnEndReached(chanInfo.ChanNum);
								continue;
							}

							if (chanInfo.PositionTable[chanInfo.Position] < 0)
							{
								// Jump to a new position
								ushort oldPosition = chanInfo.Position;
								chanInfo.Position = (ushort)(chanInfo.PositionTable[chanInfo.Position] & 0x7f);

								// Do we jump back in the song
								if (chanInfo.Position < oldPosition)
									OnEndReached(chanInfo.ChanNum);

								continue;
							}

							// Stop the loop
							break;
						}

						// Find new track
						chanInfo.TrackTable = tracks[chanInfo.PositionTable[chanInfo.Position]];
						chanInfo.TrackPosition = 0;
						chanInfo.TrackDuration = 1;

						ShowChannelPositions();
						ShowTracks();

						// Take the same channel again
						return 2;
					}

					// Note delay
					default:
					{
						chanInfo.TrackDuration = (ushort)(-(sbyte)cmd * playingInfo.CurrentTempo);
						chanInfo.TrackPosition = trackPos;

						// Take the next channel
						return 0;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Create a pulse synth sample
		/// </summary>
		/********************************************************************/
		private void CreateSynthSamplePulse(Instrument inst, ChannelInfo chanInfo)
		{
			// Initialize the pulse variables
			chanInfo.PulseShot = inst.PulseShotCounter;
			chanInfo.PulseDelay = inst.PulseDelay;
			chanInfo.PulseSpeed = inst.PulseSpeed;
			chanInfo.PulseWay = false;
			chanInfo.PulsePosition = inst.PulseStart;

			// Create first part of the sample
			int i;
			for (i = 0; i < inst.PulseStart; i++)
				chanInfo.SynthSample[i] = inst.PulseRateMin;

			// Create the second part
			for (; i < inst.Length; i++)
				chanInfo.SynthSample[i] = inst.PulseRatePlus;
		}



		/********************************************************************/
		/// <summary>
		/// Create a blend synth sample
		/// </summary>
		/********************************************************************/
		private void CreateSynthSampleBlend(Instrument inst, ChannelInfo chanInfo)
		{
			// Initialize the blend variables
			chanInfo.BlendWay = false;
			chanInfo.BlendPosition = 1;
			chanInfo.BlendShot = inst.BlendShotCounter;
			chanInfo.BlendDelay = inst.BlendDelay;

			for (int i = 0; i < 32; i++)
				chanInfo.SynthSample[i] = inst.SampleAddr[i];
		}



		/********************************************************************/
		/// <summary>
		/// Runs all the effects on the sound
		/// </summary>
		/********************************************************************/
		private void ModifySound(ChannelInfo chanInfo, IChannel channel)
		{
			Instrument inst = chanInfo.Instrument;

			// If the channel doesn't have any instruments, don't do anything
			if (inst == null)
				return;

			// Arpeggio
			byte newNote = (byte)(chanInfo.TrackNote + inst.Arpeggio[chanInfo.ArpPosition]);

			chanInfo.ArpSpeed--;
			if (chanInfo.ArpSpeed == 0)
			{
				chanInfo.ArpSpeed = inst.ArpSpeed;

				// Go the the next position
				chanInfo.ArpPosition++;

				if (chanInfo.ArpPosition >= inst.ArpCount)
					chanInfo.ArpPosition = 0;
			}

			// Find the new period
			chanInfo.TrackPeriod = (ushort)((Tables.PeriodTable[newNote] * inst.Period) / 1024);

			// Portamento
			if (chanInfo.PortRunning)
			{
				// Should we delay the portamento?
				if (chanInfo.PortDelay != 0)
					chanInfo.PortDelay--;
				else
				{
					// Do it, calculate the new period
					chanInfo.TrackPeriod = (ushort)(chanInfo.TrackPeriod + (chanInfo.PortCounter * chanInfo.PeriodDiff / chanInfo.PortSpeed));

					chanInfo.PortCounter++;

					if (chanInfo.PortCounter > chanInfo.PortSpeed)
					{
						chanInfo.TrackNote = chanInfo.PortTargetNote;
						chanInfo.PortRunning = false;
					}
				}
			}

			// Vibrato
			ushort period = chanInfo.TrackPeriod;

			if (chanInfo.VibDelay != 0)
				chanInfo.VibDelay--;
			else
			{
				if (chanInfo.VibFlags != VibFlags.None)
				{
					// Vibrato is running, now check the vibrato direction
					if ((chanInfo.VibFlags & VibFlags.VibDirection) != 0)
					{
						chanInfo.VibValue += chanInfo.VibSpeed;

						if (chanInfo.VibValue == chanInfo.VibAmpl)
							chanInfo.VibFlags &= ~VibFlags.VibDirection;
					}
					else
					{
						chanInfo.VibValue -= chanInfo.VibSpeed;

						if (chanInfo.VibValue == 0)
							chanInfo.VibFlags |= VibFlags.VibDirection;
					}

					// Change the direction of the period
					if (chanInfo.VibValue == 0)
						chanInfo.VibFlags ^= VibFlags.PeriodDirection;

					if ((chanInfo.VibFlags & VibFlags.PeriodDirection) != 0)
						period = (ushort)(period + chanInfo.VibValue);
					else
						period = (ushort)(period - chanInfo.VibValue);
				}
			}

			// Set the period
			channel.SetAmigaPeriod(period);

			// Envelope
			switch (chanInfo.EnvState)
			{
				case EnvelopeState.Attack:
				{
					chanInfo.TrackVolume += inst.AttackSpeed;

					if (chanInfo.TrackVolume >= inst.AttackVolume)
					{
						chanInfo.TrackVolume = inst.AttackVolume;
						chanInfo.EnvState = EnvelopeState.Decay;
					}
					break;
				}

				case EnvelopeState.Decay:
				{
					chanInfo.TrackVolume -= inst.DecaySpeed;

					if (chanInfo.TrackVolume <= inst.DecayVolume)
					{
						chanInfo.TrackVolume = inst.DecayVolume;
						chanInfo.EnvState = EnvelopeState.Sustain;
					}
					break;
				}

				case EnvelopeState.Sustain:
				{
					if (chanInfo.SustainDelay == 0)
						chanInfo.EnvState = EnvelopeState.Release;
					else
						chanInfo.SustainDelay--;

					break;
				}

				case EnvelopeState.Release:
				{
					chanInfo.TrackVolume -= inst.ReleaseSpeed;

					if (chanInfo.TrackVolume <= inst.ReleaseVolume)
					{
						chanInfo.TrackVolume = inst.ReleaseVolume;
						chanInfo.EnvState = EnvelopeState.Done;
					}
					break;
				}
			}

			// Set the volume
			channel.SetVolume((ushort)(inst.EnvVol * chanInfo.TrackVolume / 256));

			// Pulse on synth samples
			if (inst.InstType == InstrumentType.Pulse)
			{
				if (chanInfo.PulseDelay != 0)
					chanInfo.PulseDelay--;
				else
				{
					if (chanInfo.PulseSpeed != 0)
						chanInfo.PulseSpeed--;
					else
					{
						if (((inst.InstSync & SynchronizeFlag.PulseXShot) == 0) || (chanInfo.PulseShot != 0))
						{
							chanInfo.PulseSpeed = inst.PulseSpeed;

							for (;;)
							{
								if (chanInfo.PulseWay)
								{
									if (chanInfo.PulsePosition >= inst.PulseStart)
									{
										// Change the sample at the pulse position
										chanInfo.SynthSample[chanInfo.PulsePosition] = inst.PulseRatePlus;
										chanInfo.PulsePosition--;
										break;
									}

									// Switch direction
									chanInfo.PulseWay = false;
									chanInfo.PulseShot--;
									chanInfo.PulsePosition++;
								}
								else
								{
									if (chanInfo.PulsePosition <= inst.PulseEnd)
									{
										// Change the sample at the pulse position
										chanInfo.SynthSample[chanInfo.PulsePosition] = inst.PulseRateMin;
										chanInfo.PulsePosition++;
										break;
									}

									// Switch direction
									chanInfo.PulseWay = true;
									chanInfo.PulseShot--;
									chanInfo.PulsePosition--;
								}
							}
						}
					}
				}
			}

			// Blend on synth samples
			if (inst.InstType == InstrumentType.Blend)
			{
				if (chanInfo.BlendDelay != 0)
					chanInfo.BlendDelay--;
				else
				{
					for (;;)
					{
						if (((inst.InstSync & SynchronizeFlag.BlendXShot) == 0) || (chanInfo.BlendShot != 0))
						{
							if (chanInfo.BlendWay)
							{
								if (chanInfo.BlendPosition == 1)
								{
									chanInfo.BlendWay = false;
									chanInfo.BlendShot--;
									continue;
								}

								chanInfo.BlendPosition--;
								break;
							}

							if (chanInfo.BlendPosition == (1 << inst.Blend))
							{
								chanInfo.BlendWay = true;
								chanInfo.BlendShot--;
								continue;
							}

							chanInfo.BlendPosition++;
							break;
						}

						return;		// Well, done with the effects
					}

					// Create new synth sample
					for (int i = 0; i < 32; i++)
						chanInfo.SynthSample[i] = (sbyte)(((chanInfo.BlendPosition * inst.SampleAddr[i + 32]) >> inst.Blend) + inst.SampleAddr[i]);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current channel positions
		/// </summary>
		/********************************************************************/
		private void ShowChannelPositions()
		{
			OnModuleInfoChanged(InfoPositionLine, FormatPositions());
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
			OnModuleInfoChanged(InfoSpeedLine, playingInfo.CurrentTempo.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with all dynamic values
		/// </summary>
		/********************************************************************/
		private void UpdateModuleInformation()
		{
			ShowChannelPositions();
			ShowTracks();
			ShowSpeed();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the songs position lengths
		/// </summary>
		/********************************************************************/
		private string FormatPositionLengths()
		{
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < 4; i++)
			{
				sb.Append(positionLengths[currentSong, i]);
				sb.Append(", ");
			}

			sb.Remove(sb.Length - 2, 2);

			return sb.ToString();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the playing positions
		/// </summary>
		/********************************************************************/
		private string FormatPositions()
		{
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < 4; i++)
			{
				sb.Append(channels[i].Position);
				sb.Append(", ");
			}

			sb.Remove(sb.Length - 2, 2);

			return sb.ToString();
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
				sb.Append(channels[i].PositionTable[channels[i].Position]);
				sb.Append(", ");
			}

			sb.Remove(sb.Length - 2, 2);

			return sb.ToString();
		}
		#endregion
	}
}
