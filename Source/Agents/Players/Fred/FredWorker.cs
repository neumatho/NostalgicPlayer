/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.Fred.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.Fred
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class FredWorker : ModulePlayerAgentBase
	{
		private const byte TrackEndCode = 0x80;
		private const byte TrackPortCode = 0x81;
		private const byte TrackTempoCode = 0x82;
		private const byte TrackInstCode = 0x83;
		private const byte TrackPauseCode = 0x84;
		private const byte TrackMaxCode = 0xa0;

		private static readonly uint[] periodTable =
		{
			8192, 7728, 7296, 6888, 6504, 6136, 5792, 5464, 5160, 4872, 4600, 4336,
			4096, 3864, 3648, 3444, 3252, 3068, 2896, 2732, 2580, 2436, 2300, 2168,
			2048, 1932, 1824, 1722, 1626, 1534, 1448, 1366, 1290, 1218, 1150, 1084,
			1024,  966,  912,  861,  813,  767,  724,  683,  645,  609,  575,  542,
 			 512,  483,  456,  430,  406,  383,  362,  341,  322,  304,  287,  271,
			 256,  241,  228,  215,  203,  191,  181,  170,  161,  152,  143,  135
		};

		private ushort subSongNum;
		private ushort instNum;

		private byte[] startTempos;
		private sbyte[][][] positions;
		private byte[][] tracks;
		private Instrument[] instruments;

		private ChannelInfo[] channels;
		private int currentSong;
		private byte currentTempo;

		private PosLength[] posLength;

		private const int InfoSpeedLine = 2;

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
			moduleStream.Read(buf, 0, 12);

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
				// Song length
				case 0:
				{
					description = Resources.IDS_FRED_INFODESCLINE0;
					value = posLength[currentSong].Length.ToString();
					break;
				}

				// Used instruments
				case 1:
				{
					description = Resources.IDS_FRED_INFODESCLINE1;
					value = instNum.ToString();
					break;
				}

				// Current speed
				case 2:
				{
					description = Resources.IDS_FRED_INFODESCLINE2;
					value = currentTempo.ToString();
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
		public override ModulePlayerSupportFlag SupportFlags => ModulePlayerSupportFlag.SetPosition;



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
				moduleStream.Read(startTempos, 0, subSongNum);

				if (moduleStream.EndOfStream)
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
					moduleStream.Read(tracks[i], 0, trackSize);

					// Did we get some problems?
					if (moduleStream.EndOfStream)
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

					moduleStream.Read(name, 0, 32);
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
		public override bool InitSound(int songNumber, DurationInfo durationInfo, out string errorMessage)
		{
			if (!base.InitSound(songNumber, durationInfo, out errorMessage))
				return false;

			// Remember the song number
			currentSong = songNumber;

			// Get the start tempo
			currentTempo = startTempos[songNumber];

			// Initialize the channel structure
			channels = Helpers.InitializeArray<ChannelInfo>(4);

			for (int i = 0; i < 4; i++)
			{
				ChannelInfo chanInfo = channels[i];

				chanInfo.ChanNum = i;
				chanInfo.PositionTable = positions[songNumber][i];
				chanInfo.TrackTable = tracks[chanInfo.PositionTable[0]];
				chanInfo.Position = 0;
				chanInfo.TrackPosition = 0;
				chanInfo.TrackDuration = 1;
				chanInfo.TrackNote = 0;
				chanInfo.TrackPeriod = 0;
				chanInfo.TrackVolume = 0;
				chanInfo.Instrument = null;
				chanInfo.VibFlags = VibFlags.None;
				chanInfo.VibDelay = 0;
				chanInfo.VibSpeed = 0;
				chanInfo.VibAmpl = 0;
				chanInfo.VibValue = 0;
				chanInfo.PortRunning = false;
				chanInfo.PortDelay = 0;
				chanInfo.PortLimit = 0;
				chanInfo.PortTargetNote = 0;
				chanInfo.PortStartPeriod = 0;
				chanInfo.PeriodDiff = 0;
				chanInfo.PortCounter = 0;
				chanInfo.PortSpeed = 0;
				chanInfo.EnvState = EnvelopeState.Attack;
				chanInfo.SustainDelay = 0;
				chanInfo.ArpPosition = 0;
				chanInfo.ArpSpeed = 0;
				chanInfo.PulseWay = false;
				chanInfo.PulsePosition = 0;
				chanInfo.PulseDelay = 0;
				chanInfo.PulseSpeed = 0;
				chanInfo.PulseShot = 0;
				chanInfo.BlendWay = false;
				chanInfo.BlendPosition = 0;
				chanInfo.BlendDelay = 0;
				chanInfo.BlendShot = 0;
				chanInfo.SynthSample = new sbyte[64];	// Well, it seems only 32 bytes are needed, but we allocate 64 just in case
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the duration for all sub-songs
		/// </summary>
		/********************************************************************/
		public override DurationInfo[] CalculateDuration()
		{
			// Find the longest (in time) channel and create
			// position structures. Do it for each sub-song
			DurationInfo[] result = new DurationInfo[subSongNum];
			posLength = new PosLength[subSongNum];

			for (ushort i = 0; i < subSongNum; i++)
				result[i] = FindLongestChannel(i);

			return result;
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
		/// Return the length of the current song
		/// </summary>
		/********************************************************************/
		public override int SongLength => posLength[currentSong].Length;



		/********************************************************************/
		/// <summary>
		/// Return the current position of the song
		/// </summary>
		/********************************************************************/
		public override int GetSongPosition()
		{
			return channels[posLength[currentSong].Channel].Position;
		}



		/********************************************************************/
		/// <summary>
		/// Set a new position of the song
		/// </summary>
		/********************************************************************/
		public override void SetSongPosition(int position, PositionInfo positionInfo)
		{
			// Change the position
			ExtraPositionInfo[] extraInfo = (ExtraPositionInfo[])positionInfo.ExtraInfo;

			for (int i = 0; i < 4; i++)
			{
				channels[i].Position = extraInfo[i].Position;
				channels[i].TrackTable = tracks[channels[i].PositionTable[channels[i].Position]];
				channels[i].TrackPosition = extraInfo[i].TrackPosition;
				channels[i].TrackDuration = extraInfo[i].TrackDuration;
			}

			// Change the speed
			currentTempo = positionInfo.Speed;

			OnModuleInfoChanged(InfoSpeedLine, currentTempo.ToString());

			base.SetSongPosition(position, positionInfo);
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

				foreach (Instrument inst in instruments)
				{
					// Build frequency table
					uint[] frequencies = new uint[10 * 12];

					for (int j = 0; j < 6 * 12; j++)
						frequencies[12 + j] = 3546895U / ((periodTable[j] * inst.Period) / 1024);

					SampleInfo sampleInfo = new SampleInfo
					{
						Name = inst.Name,
						BitSize = 8,
						MiddleC = frequencies[12 + 3 * 12],
						Volume = inst.EnvVol,
						Panning = -1,
						NoteFrequencies = frequencies
					};

					if (inst.InstType != InstrumentType.Sample)
					{
						sampleInfo.Type = SampleInfo.SampleType.Synth;
						sampleInfo.Flags = SampleInfo.SampleFlags.None;
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
							sampleInfo.Flags = SampleInfo.SampleFlags.None;
							sampleInfo.LoopStart = 0;
							sampleInfo.LoopLength = 0;
						}
						else
						{
							// Sample loops
							sampleInfo.Flags = SampleInfo.SampleFlags.Loop;
							sampleInfo.LoopStart = 0;
							sampleInfo.LoopLength = inst.RepeatLen;
						}
					}

					result.Add(sampleInfo);
				}

				return result.ToArray();
			}
		}
		#endregion

		#region Private methods
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
		}



		/********************************************************************/
		/// <summary>
		/// Scans all the channels to find the channel which take longest
		/// time to play
		/// </summary>
		/********************************************************************/
		private DurationInfo FindLongestChannel(ushort songNum)
		{
			// Get the start tempo
			byte curTempo = startTempos[songNum];

			// Initialize the channels
			channels = Helpers.InitializeArray<ChannelInfo>(4);

			for (int i = 0; i < 4; i++)
			{
				ChannelInfo chanInfo = channels[i];

				chanInfo.PositionTable = positions[songNum][i];
				chanInfo.TrackTable = tracks[chanInfo.PositionTable[0]];
				chanInfo.Position = 0;
				chanInfo.TrackPosition = 0;
				chanInfo.TrackDuration = 1;
			}

			// Okay, now calculate the time for each channel
			float[] chanTimes = { 0.0f, 0.0f, 0.0f, 0.0f };
			bool[] doneFlags = { false, false, false, false };
			bool[] hasNote = { false, false, false, false };
			int[] posBeforeRestart = { 0, 0, 0, 0 };
			List<PositionInfo>[] posTimes = Helpers.InitializeArray<List<PositionInfo>>(4);

			for (int i = 0; i < 4; i++)
			{
				posTimes[i].Add(new PositionInfo(curTempo, 125, new TimeSpan(0), songNum, new ExtraPositionInfo[]
				{
					new ExtraPositionInfo(0, 0, 1),
					new ExtraPositionInfo(0, 0, 1),
					new ExtraPositionInfo(0, 0, 1),
					new ExtraPositionInfo(0, 0, 1)
				}));
			}

			for (;;)
			{
				// Check to see if all channels has been parsed
				if (doneFlags[0] && doneFlags[1] && doneFlags[2] && doneFlags[3])
					break;

				bool[] changePos = { false, false, false, false };

				ExtraPositionInfo[] extraInfo = new ExtraPositionInfo[]
				{
					new ExtraPositionInfo(channels[0].Position, channels[0].TrackPosition, channels[0].TrackDuration),
					new ExtraPositionInfo(channels[1].Position, channels[1].TrackPosition, channels[1].TrackDuration),
					new ExtraPositionInfo(channels[2].Position, channels[2].TrackPosition, channels[2].TrackDuration),
					new ExtraPositionInfo(channels[3].Position, channels[3].TrackPosition, channels[3].TrackDuration)
				};

				for (int i = 0; i < 4; i++)
				{
					ChannelInfo chanInfo = channels[i];
					bool chanDone = false;
					bool doneFlag = false;

					chanInfo.TrackDuration--;
					if (chanInfo.TrackDuration == 0)
					{
						do
						{
							// Get the channel command
							byte cmd = chanInfo.TrackTable[chanInfo.TrackPosition++];

							// Is the command a note?
							if (cmd < 0x80)
							{
								// Yes
								chanInfo.TrackDuration = curTempo;

								hasNote[i] = true;
								chanDone = true;
							}
							else
							{
								// It's a real command
								switch (cmd)
								{
									case TrackInstCode:
									{
										chanInfo.TrackPosition++;
										break;
									}

									case TrackTempoCode:
									{
										curTempo = chanInfo.TrackTable[chanInfo.TrackPosition++];
										break;
									}

									case TrackPortCode:
									{
										chanInfo.TrackPosition += 3;
										break;
									}

									case TrackPauseCode:
									{
										chanInfo.TrackDuration = curTempo;
										chanDone = true;
										break;
									}

									case TrackEndCode:
									{
										chanInfo.Position++;

										for (;;)
										{
											if (chanInfo.PositionTable[chanInfo.Position] == -1)
											{
												posBeforeRestart[i] = chanInfo.Position;

												chanInfo.Position = 0;
												curTempo = startTempos[songNum];

												doneFlag = true;
												chanDone = true;
												continue;
											}

											if (chanInfo.PositionTable[chanInfo.Position] < 0)
											{
												posBeforeRestart[i] = chanInfo.Position;
												chanInfo.Position = (ushort)(chanInfo.PositionTable[chanInfo.Position] & 0x7f);

												doneFlag = true;
												chanDone = true;
												continue;
											}
											break;
										}

										chanInfo.TrackTable = tracks[chanInfo.PositionTable[chanInfo.Position]];
										chanInfo.TrackPosition = 0;
										chanInfo.TrackDuration = 1;

										extraInfo[i] = new ExtraPositionInfo(chanInfo.Position, 0, 1);
										changePos[i] = true;
										break;
									}

									default:	// Note delay
									{
										chanInfo.TrackDuration = (ushort)(-(sbyte)cmd * curTempo);
										chanDone = true;
										break;
									}
								}
							}
						}
						while (!chanDone);
					}

					if (!doneFlags[i])
						chanTimes[i] += 1000.0f / 50.0f;

					if (doneFlag)
						doneFlags[i] = true;
				}

				for (int i = 0; i < 4; i++)
				{
					if (changePos[i])
						posTimes[i].Add(new PositionInfo(curTempo, 125, new TimeSpan((long)chanTimes[i] * TimeSpan.TicksPerMillisecond), songNum, extraInfo));
				}
			}

			// Find the channel which takes the longest time to play
			float time = 0.0f;

			for (int i = 0; i < 4; i++)
			{
				// Skip channels that does not have any notes
				if (hasNote[i])
				{
					if (chanTimes[i] >= time)
					{
						posLength[songNum] = new PosLength
						{
							Channel = i,
							Length = posBeforeRestart[i]
						};

						time = chanTimes[i];
					}
				}
			}

			// Some songs may have zero length, which means
			// that the PosLength object hasn't been created
			if (posLength[songNum] == null)
				posLength[songNum] = new PosLength();

			// Now return a duration object with the position times
			return new DurationInfo(new TimeSpan((long)time * TimeSpan.TicksPerMillisecond), posTimes[posLength[songNum].Channel].ToArray());
		}



		/********************************************************************/
		/// <summary>
		/// Take the next track line and parse it
		/// </summary>
		/********************************************************************/
		private byte DoNewLine(ChannelInfo chanInfo, IChannel channel)
		{
			bool instChange = false;

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
					chanInfo.TrackDuration = currentTempo;

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
					chanInfo.TrackPeriod = (ushort)((periodTable[chanInfo.TrackNote] * inst.Period) / 1024);
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
						currentTempo = chanInfo.TrackTable[trackPos++];

						// Change the module info
						OnModuleInfoChanged(InfoSpeedLine, currentTempo.ToString());
						break;
					}

					// Start portamento
					case TrackPortCode:
					{
						ushort instPeriod = 428;

						if (chanInfo.Instrument != null)
							instPeriod = chanInfo.Instrument.Period;

						chanInfo.PortSpeed = (ushort)(chanInfo.TrackTable[trackPos++] * currentTempo);
						chanInfo.PortTargetNote = chanInfo.TrackTable[trackPos++];
						chanInfo.PortLimit = (ushort)((periodTable[chanInfo.PortTargetNote] * instPeriod) / 1024);
						chanInfo.PortStartPeriod = 0;
						chanInfo.PortDelay = (ushort)(chanInfo.TrackTable[trackPos++] * currentTempo);
						chanInfo.PortRunning = true;
						break;
					}

					// Execute pause
					case TrackPauseCode:
					{
						chanInfo.TrackDuration = currentTempo;
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
								currentTempo = startTempos[currentSong];

								// Change the module info
								OnModuleInfoChanged(InfoSpeedLine, currentTempo.ToString());

								// Tell NostalgicPlayer that the song has ended
								if (chanInfo.ChanNum == posLength[currentSong].Channel)
									OnEndReached();

								continue;
							}

							if (chanInfo.PositionTable[chanInfo.Position] < 0)
							{
								// Jump to a new position
								ushort oldPosition = chanInfo.Position;
								chanInfo.Position = (ushort)(chanInfo.PositionTable[chanInfo.Position] & 0x7f);

								if (chanInfo.ChanNum == posLength[currentSong].Channel)
								{
									// Do we jump back in the song
									if (chanInfo.Position < oldPosition)
										OnEndReached();
								}
								continue;
							}

							// Stop the loop
							break;
						}

						// Find new track
						chanInfo.TrackTable = tracks[chanInfo.PositionTable[chanInfo.Position]];
						chanInfo.TrackPosition = 0;
						chanInfo.TrackDuration = 1;

						// Tell NostalgicPlayer we have changed the position
						if (chanInfo.ChanNum == posLength[currentSong].Channel)
							OnPositionChanged();

						// Take the same channel again
						return 2;
					}

					// Note delay
					default:
					{
						chanInfo.TrackDuration = (ushort)(-(sbyte)cmd * currentTempo);
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
			chanInfo.TrackPeriod = (ushort)((periodTable[newNote] * inst.Period) / 1024);

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
		#endregion
	}
}
