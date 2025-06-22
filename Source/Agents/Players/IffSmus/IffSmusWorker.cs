/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.IffSmus.Containers;
using Polycode.NostalgicPlayer.Agent.Player.IffSmus.Instruments;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.IffSmus
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class IffSmusWorker : ModulePlayerWithSubSongDurationAgentBase
	{
		private GlobalInfo globalInfo;
		private ModuleInfo moduleInfo;

		private string songName;
		private string author;

		private int numberOfChannels;

		private GlobalPlayingInfo playingInfo;
		private VoiceInfo[] voices;

		private const int InfoTempoLine = 1;

		#region Identify
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => [ "smus" ];



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			// Check the module
			ModuleStream moduleStream = fileInfo.ModuleStream;

			moduleStream.Seek(0, SeekOrigin.Begin);

			string mark = moduleStream.ReadMark();
			if (mark != "FORM")
				return AgentResult.Unknown;

			moduleStream.Seek(8, SeekOrigin.Begin);

			mark = moduleStream.ReadMark();
			if (mark != "SMUS")
				return AgentResult.Unknown;

			return AgentResult.Ok;
		}
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
				ModuleStream moduleStream = fileInfo.ModuleStream;

				AllocateStructures();
				playingInfo.CurrentVolume = 0xffff;

				moduleInfo = new ModuleInfo();

				Encoding encoder = EncoderCollection.Amiga;

				// Skip the header
				moduleStream.Seek(4, SeekOrigin.Begin);
				int totalSize = moduleStream.Read_B_INT32();

				moduleStream.Seek(4, SeekOrigin.Current);
				totalSize -= 4;

				int trackNumber = 0;

				// Okay, now read each chunk and parse them
				for (; totalSize > 0;)
				{
					// Read the chunk name and length
					string chunkName = moduleStream.ReadMark();
					int chunkSize = moduleStream.Read_B_INT32();
					totalSize -= (chunkSize + 8);

					// Do we have any chunks left?
					if (moduleStream.EndOfStream)
						break;			// No, stop the loading

					if ((chunkSize > (moduleStream.Length - moduleStream.Position)))
					{
						Cleanup();

						errorMessage = Resources.IDS_SMUS_ERR_LOADING_HEADER;
						return AgentResult.Error;
					}

					// Find out what the chunk is and begin to parse it
					long chunkStartPosition = moduleStream.Position;

					switch (chunkName)
					{
						// Song name
						case "NAME":
						{
							ParseName(moduleStream, chunkSize, encoder, out errorMessage);
							break;
						}

						// Author
						case "AUTH":
						{
							ParseAuth(moduleStream, chunkSize, encoder, out errorMessage);
							break;
						}

						// Score header
						case "SHDR":
						{
							ParseShdr(moduleStream, out errorMessage);
							break;
						}

						// Instrument
						case "INS1":
						{
							ParseIns1(fileInfo, moduleStream, chunkSize, encoder, out errorMessage);
							break;
						}

						// Track
						case "TRAK":
						{
							ParseTrak(moduleStream, chunkSize, ref trackNumber, out errorMessage);
							break;
						}

						case "SNX1":
						case "SNX2":
						case "SNX3":
						case "SNX4":
						case "SNX5":
						case "SNX6":
						case "SNX7":
						case "SNX8":
						case "SNX9":
						{
							ParseSnx(moduleStream, out errorMessage);
							break;
						}
					}

					if (!string.IsNullOrEmpty(errorMessage))
					{
						Cleanup();
						return AgentResult.Error;
					}

					if ((chunkSize % 2) != 0)
					{
						chunkSize++;
						totalSize--;
					}

					long bytesToSkip = chunkSize - (moduleStream.Position - chunkStartPosition);		// Subtract number of bytes read in the parse method
					if (bytesToSkip != 0)
						moduleStream.Seek(bytesToSkip, SeekOrigin.Current);
				}

				if ((numberOfChannels == 0) || (moduleInfo.InstrumentMapper == null) || (moduleInfo.Tracks == null))
				{
					Cleanup();

					errorMessage = Resources.IDS_SMUS_ERR_LOADING_MISSING_CHUNK;

					return AgentResult.Error;
				}
			}
			catch (Exception)
			{
				Cleanup();
				throw;
			}

			return AgentResult.Ok;
		}
		#endregion

		#region Initialization and cleanup
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
		#endregion

		#region Playing
		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public override void Play()
		{
			FindVolume();
			GetNextNote();
			SetupAndPlayInstruments();
		}
		#endregion

		#region Information
		/********************************************************************/
		/// <summary>
		/// Return the title
		/// </summary>
		/********************************************************************/
		public override string Title => songName;



		/********************************************************************/
		/// <summary>
		/// Return the name of the author
		/// </summary>
		/********************************************************************/
		public override string Author => author;



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the module use
		/// </summary>
		/********************************************************************/
		public override int ModuleChannelCount => numberOfChannels;



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
				foreach (Instrument instr in globalInfo.Instruments)
				{
					SampleInfo sampleInfo = instr.Format.GetSampleInfo(globalInfo);

					sampleInfo.Name = instr.Name;

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
				// Used instruments
				case 0:
				{
					description = Resources.IDS_SMUS_INFODESCLINE0;
					value = globalInfo.Instruments.Count.ToString();
					break;
				}

				// Current tempo (Hz)
				case 1:
				{
					description = Resources.IDS_SMUS_INFODESCLINE1;
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
		#endregion

		#region Duration calculation
		/********************************************************************/
		/// <summary>
		/// Initialize all internal structures when beginning duration
		/// calculation on a new sub-song
		/// </summary>
		/********************************************************************/
		protected override void InitDuration(int subSong)
		{
			InitializeSound();
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
		/// Allocate needed structures
		/// </summary>
		/********************************************************************/
		private void AllocateStructures()
		{
			globalInfo = new GlobalInfo
			{
				NewTempo = 64,
				Tune = 0x80
			};

			playingInfo = new GlobalPlayingInfo
			{
				CurrentVolume = 0xff,
				CalculatedTempo = 0x8000
			};
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player have allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			globalInfo = null;
			moduleInfo = null;

			playingInfo = null;
			voices = null;
		}



		/********************************************************************/
		/// <summary>
		/// Parses the NAME chunk
		/// </summary>
		/********************************************************************/
		private void ParseName(ModuleStream moduleStream, int chunkSize, Encoding encoder, out string errorMessage)
		{
			errorMessage = string.Empty;

			songName = moduleStream.ReadString(encoder, chunkSize);

			if (moduleStream.EndOfStream)
				errorMessage = Resources.IDS_SMUS_ERR_LOADING_HEADER;
		}



		/********************************************************************/
		/// <summary>
		/// Parses the AUTH chunk
		/// </summary>
		/********************************************************************/
		private void ParseAuth(ModuleStream moduleStream, int chunkSize, Encoding encoder, out string errorMessage)
		{
			errorMessage = string.Empty;

			author = moduleStream.ReadString(encoder, chunkSize);

			if (moduleStream.EndOfStream)
				errorMessage = Resources.IDS_SMUS_ERR_LOADING_HEADER;
		}



		/********************************************************************/
		/// <summary>
		/// Parses the SHDR chunk
		/// </summary>
		/********************************************************************/
		private void ParseShdr(ModuleStream moduleStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			ushort tempo = moduleStream.Read_B_UINT16();
			if (tempo >= 0xe11)
			{
				int findTempo = 0xe100000 / tempo;
				ushort i;

				for (i = 0; i < 128; i++)
				{
					if (findTempo >= Tables.Tempo[i])
						break;
				}

				if (i == 128)
					i--;

				moduleInfo.TempoIndex = i;
			}
			else
				moduleInfo.TempoIndex = 0;

			moduleInfo.GlobalVolume = moduleStream.Read_UINT8();
			if (moduleInfo.GlobalVolume < 128)
				moduleInfo.GlobalVolume *= 2;

			numberOfChannels = moduleStream.Read_UINT8();

			if (moduleStream.EndOfStream)
			{
				errorMessage = Resources.IDS_SMUS_ERR_LOADING_HEADER;
				return;
			}

			moduleInfo.TrackVolumes = new ushort[numberOfChannels];
			moduleInfo.TracksEnabled = new ulong[numberOfChannels];

			for (int i = 0; i < numberOfChannels; i++)
			{
				moduleInfo.TrackVolumes[i] = 0xff;
				moduleInfo.TracksEnabled[i] = 1;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parses the INS1 chunk
		/// </summary>
		/********************************************************************/
		private void ParseIns1(PlayerFileInfo fileInfo, ModuleStream moduleStream, int chunkSize, Encoding encoder, out string errorMessage)
		{
			errorMessage = string.Empty;

			if (moduleInfo.InstrumentMapper == null)
				moduleInfo.InstrumentMapper = new int[256];

			// Get instrument register number (index in the mapper above)
			byte register = moduleStream.Read_UINT8();

			if (moduleInfo.InstrumentMapper[register] != 0)
				return;

			byte type = moduleStream.Read_UINT8();
			if (type != 0)
				return;		// Cannot use MIDI instruments

			// Skip data1 and data2
			moduleStream.Seek(2, SeekOrigin.Current);

			string name = moduleStream.ReadString(encoder, chunkSize - 4);
			if (string.IsNullOrEmpty(name))
				return;

			if (moduleStream.EndOfStream)
			{
				errorMessage = Resources.IDS_SMUS_ERR_LOADING_INSTRUMENTS;
				return;
			}

			int instrumentNumber = LoadInstrument(fileInfo, name, out errorMessage);
			if (instrumentNumber != -1)
				moduleInfo.InstrumentMapper[register] = instrumentNumber + 1;
		}



		/********************************************************************/
		/// <summary>
		/// Parses the TRAK chunk
		/// </summary>
		/********************************************************************/
		private void ParseTrak(ModuleStream moduleStream, int chunkSize, ref int trackNumber, out string errorMessage)
		{
			errorMessage = string.Empty;

			if (moduleInfo.Tracks == null)
			{
				if (numberOfChannels == 0)
				{
					errorMessage = Resources.IDS_SMUS_ERR_LOADING_HEADER;
					return;
				}

				moduleInfo.Tracks = new Track[numberOfChannels];
			}

			int numberOfEvents = chunkSize / 2;
			List<Event> events = new List<Event>(numberOfEvents + 1);

			for (int i = 0; i < numberOfEvents; i++)
			{
				EventType type = (EventType)moduleStream.Read_UINT8();
				byte data = moduleStream.Read_UINT8();

				if (type == EventType.Mark)		// Should never happen, but just in case
					break;

				if ((type <= EventType.LastNote) || (type == EventType.Rest))
				{
					// Only dot and division is supported. Chord, tieOut and nTuplet masked out
					data &= 0x0f;

					sbyte newData = Tables.Duration[data];
					if (newData < 0)
						continue;		// Skip note

					data = (byte)newData;
				}
				else if (type == EventType.Instrument)
				{
					;
				}
				else if (type == EventType.TimeSig)
				{
					moduleInfo.TimeSigNumerator = (ushort)(((data >> 3) & 0x1f) + 1);
					moduleInfo.TimeSigDenominator = (ushort)(1 << (data & 0x07));
					continue;
				}
				else if (type == EventType.Volume)
				{
					moduleInfo.TrackVolumes[trackNumber] = (ushort)((data & 0x7f) * 2);
					continue;
				}
				else
				{
					// All other events are ignored
					continue;
				}

				events.Add(new Event
				{
					Type = type,
					Data = data
				});
			}

			if (moduleStream.EndOfStream)
			{
				errorMessage = Resources.IDS_SMUS_ERR_LOADING_TRACKS;
				return;
			}

			events.Add(new Event
			{
				Type = EventType.Mark,
				Data = 0xff
			});

			moduleInfo.Tracks[trackNumber++] = new Track
			{
				Events = events.ToArray()
			};
		}



		/********************************************************************/
		/// <summary>
		/// Parses the SNX1 to SNX9 chunk
		/// </summary>
		/********************************************************************/
		private void ParseSnx(ModuleStream moduleStream, out string errorMessage)
		{
			if (numberOfChannels == 0)
			{
				errorMessage = Resources.IDS_SMUS_ERR_LOADING_HEADER;
				return;
			}

			errorMessage = string.Empty;

			moduleInfo.Transpose = moduleStream.Read_B_UINT16();
			moduleInfo.Tune = moduleStream.Read_B_UINT16();

			moduleStream.Seek(4, SeekOrigin.Current);

			for (int i = 0; i < numberOfChannels; i++)
				moduleInfo.TracksEnabled[i] = moduleStream.Read_B_UINT32();

			if (moduleStream.EndOfStream)
				errorMessage = Resources.IDS_SMUS_ERR_LOADING_HEADER;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single sample as extern file
		/// </summary>
		/********************************************************************/
		private int LoadInstrument(PlayerFileInfo fileInfo, string instrName, out string errorMessage)
		{
			errorMessage = string.Empty;

			// Check if the current instrument has already been added
			int instrumentNumber = -1;

			for (int i = 0; i < globalInfo.Instruments.Count; i++)
			{
				if (globalInfo.Instruments[i].Name.Equals(instrName, StringComparison.OrdinalIgnoreCase))
				{
					instrumentNumber = i;
					break;
				}
			}

			if (instrumentNumber != -1)
				return instrumentNumber;

			string instrumentFileName = $"{instrName.Replace('?', '!')}.instr";
			string instrumentPath = string.Empty;

			using (ModuleStream instrumentStream = fileInfo.Loader?.TryOpenExternalFileInInstruments(instrumentFileName, out instrumentPath))
			{
				// Did we get any file at all
				if (instrumentStream != null)
				{
					// Read first 32 bytes
					byte[] tempBuffer = new byte[32];

					instrumentStream.Seek(0, SeekOrigin.Begin);
					int bytesRead = instrumentStream.Read(tempBuffer, 0, 32);

					if (bytesRead < 32)
					{
						errorMessage = string.Format(Resources.IDS_SMUS_ERR_LOADING_READ_EXTERNAL_FILE, instrumentFileName);
						return -1;
					}

					// Try to find the instrument format
					IInstrumentFormat format = InstrumentFactory.CreateInstrumentFormat(tempBuffer);
					if (format == null)
					{
						errorMessage = string.Format(Resources.IDS_SMUS_ERR_LOADING_READ_EXTERNAL_FILE, instrumentFileName);
						return -1;
					}

					if (!format.Load(instrumentStream, fileInfo, instrumentPath, instrumentFileName, globalInfo.Instruments, out errorMessage))
						return -1;

					instrumentNumber = globalInfo.Instruments.Count;
					globalInfo.Instruments.Add(new Instrument
					{
						Name = instrName,
						Format = format
					});

					return instrumentNumber;
				}
			}

			errorMessage = string.Format(Resources.IDS_SMUS_ERR_LOADING_OPEN_EXTERNAL_FILE, instrumentFileName);
			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize sound structures
		/// </summary>
		/********************************************************************/
		private void InitializeSound()
		{
			voices = ArrayHelper.InitializeArray<VoiceInfo>(numberOfChannels);

			StopSound();

			globalInfo.StartTime = 0;
			globalInfo.EndTime = -1;

			globalInfo.TracksEnabled = ArrayHelper.CloneArray(moduleInfo.TracksEnabled);

			globalInfo.TrackStartPositions = new int[numberOfChannels];
			globalInfo.TracksInfo = ArrayHelper.InitializeArray<TrackInfo>(numberOfChannels);

			playingInfo.CurrentInstruments = new Instrument[numberOfChannels];
			playingInfo.InstrumentNumbers = new short[numberOfChannels];
			playingInfo.HoldNoteDurationCounters = new uint[numberOfChannels];
			playingInfo.ReleaseNoteDurationCounters = new uint[numberOfChannels];
			playingInfo.SynthesisPlayInfo = ArrayHelper.InitializeArray<SynthesisPlayInfo>(numberOfChannels);
			playingInfo.SamplePlayInfo = ArrayHelper.InitializeArray<SampledSoundPlayInfo>(numberOfChannels);
			playingInfo.FormPlayInfo = ArrayHelper.InitializeArray<FormPlayInfo>(numberOfChannels);

			InitializeTracks(globalInfo.StartTime);
			StartModuleAndInitializeInstruments();

			playingInfo.SpeedCounter = 0;
			playingInfo.CurrentVolume = 0;
			playingInfo.CurrentTempo = 0;
			globalInfo.NewTempo = moduleInfo.TempoIndex;
			globalInfo.Tune = moduleInfo.Tune;

			SetVolume(1, moduleInfo.GlobalVolume);

			playingInfo.RepeatCount = -1;

			SetCiaTimerTempo(11932);	// ~60 Hz
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void StopSound()
		{
			playingInfo.Flag &= 0xfe;

			ReleaseAllVoices();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ReleaseAllVoices()
		{
			if (playingInfo.RepeatCount != 0)
			{
				playingInfo.RepeatCount = 0;

				for (int i = 0; i < numberOfChannels; i++)
				{
					if (globalInfo.TracksEnabled[i] != 0)
						BeginToReleaseVoice(i);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Setup tracks to the given start time
		/// </summary>
		/********************************************************************/
		private void InitializeTracks(int startTime)
		{
			for (int i = 0; i < numberOfChannels; i++)
			{
				globalInfo.TrackStartPositions[i] = -1;
				globalInfo.TracksInfo[i].InstrumentNumber = 0;
				globalInfo.TracksInfo[i].TimeLeft = 0;

				if (moduleInfo.Tracks[i] != null)
				{
					Track track = moduleInfo.Tracks[i];
					int timeLeft = startTime;
					int eventPos = 0;

					while (timeLeft > 0)
					{
						Event @event = track.Events[eventPos++];

						if (@event.Type == EventType.Mark)
							break;

						if ((@event.Type <= EventType.LastNote) || (@event.Type == EventType.Rest))
						{
							timeLeft -= @event.Data;
							if (timeLeft < 0)
								globalInfo.TracksInfo[i].TimeLeft = (byte)-timeLeft;
						}
						else if (@event.Type == EventType.Instrument)
							globalInfo.TracksInfo[i].InstrumentNumber = (byte)(@event.Data + 1);
					}

					globalInfo.TrackStartPositions[i] = eventPos;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void StartModuleAndInitializeInstruments()
		{
			playingInfo.CurrentTrackPositions = ArrayHelper.CloneArray(globalInfo.TrackStartPositions);
			playingInfo.CurrentTime = globalInfo.StartTime;

			for (int i = 0; i < numberOfChannels; i++)
			{
				if (globalInfo.TracksEnabled[i] != 0)
					BeginToReleaseVoice(i);

				playingInfo.HoldNoteDurationCounters[i] = 0;
				playingInfo.ReleaseNoteDurationCounters[i] = globalInfo.TracksInfo[i].TimeLeft + 1U;

				byte instrumentNumber = globalInfo.TracksInfo[i].InstrumentNumber;
				if (instrumentNumber != 0)
				{
					int mappedInstrumentNumber = moduleInfo.InstrumentMapper[instrumentNumber - 1];
					if (mappedInstrumentNumber != 0)
					{
						playingInfo.CurrentInstruments[i] = globalInfo.Instruments[mappedInstrumentNumber - 1];
						playingInfo.InstrumentNumbers[i] = (short)(mappedInstrumentNumber - 1);
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Retrieve the next note and set it up
		/// </summary>
		/********************************************************************/
		private void GetNextNote()
		{
			if (playingInfo.SpeedCounter != 0)
				playingInfo.SpeedCounter--;

			if (playingInfo.SpeedCounter == 0)
			{
				if (playingInfo.CurrentTempo != globalInfo.NewTempo)
				{
					playingInfo.CurrentTempo = globalInfo.NewTempo;

					ushort tempo = Tables.Tempo[playingInfo.CurrentTempo];
					playingInfo.CalculatedSpeed = (ushort)(tempo >> 12);
					playingInfo.CalculatedTempo = (ushort)((tempo << 15) / (playingInfo.CalculatedSpeed << 12));
					SetCiaTimerTempo((ushort)((playingInfo.CalculatedTempo * 11932) >> 15));

					ShowTempo();
				}

				if (playingInfo.RepeatCount != 0)
				{
					playingInfo.SpeedCounter = playingInfo.CalculatedSpeed;

					int tracksDone = 0;
					bool beginOver;
					bool endReached = false;

					do
					{
						beginOver = false;

						for (int i = 0; i < numberOfChannels; i++)
						{
							if (playingInfo.HoldNoteDurationCounters[i] != 0)
							{
								playingInfo.HoldNoteDurationCounters[i]--;

								if ((playingInfo.HoldNoteDurationCounters[i] == 0) && (globalInfo.TracksEnabled[i] != 0))
									BeginToReleaseVoice(i);

								continue;
							}

							if (playingInfo.ReleaseNoteDurationCounters[i] != 0)
							{
								playingInfo.ReleaseNoteDurationCounters[i]--;
								if (playingInfo.ReleaseNoteDurationCounters[i] != 0)
									continue;
							}

							if (playingInfo.CurrentTrackPositions[i] == -1)
							{
								tracksDone++;
								continue;
							}

							Track track = moduleInfo.Tracks[i];
							bool oneMore;

							do
							{
								oneMore = false;

								Event @event = track.Events[playingInfo.CurrentTrackPositions[i]];

								if (@event.Type == EventType.Mark)
								{
									tracksDone++;
									continue;
								}

								playingInfo.CurrentTrackPositions[i]++;

								if (@event.Type <= EventType.LastNote)
								{
									ushort duration = @event.Data;

									if (globalInfo.TracksEnabled[i] != 0)
									{
										Instrument instr = playingInfo.CurrentInstruments[i];
										if (instr != null)
										{
											ushort note = (ushort)((ushort)@event.Type + (moduleInfo.Transpose / 16) - 8);
											ushort vol = moduleInfo.TrackVolumes[i];

											if (globalInfo.TracksEnabled[i] != 1)
												vol /= 2;

											SetupInstrumentIfFormatChanged(instr, playingInfo.InstrumentNumbers[i], i, (byte)note, vol);

											ushort temp = (ushort)((duration * 0xc000) >> 16);
											playingInfo.HoldNoteDurationCounters[i] = temp;
											duration -= temp;
										}
									}

									playingInfo.ReleaseNoteDurationCounters[i] = duration;
								}
								else if (@event.Type == EventType.Rest)
								{
									playingInfo.ReleaseNoteDurationCounters[i] = @event.Data;
								}
								else if (@event.Type == EventType.Instrument)
								{
									int instrumentNumber = moduleInfo.InstrumentMapper[@event.Data];
									if (instrumentNumber != 0)
									{
										playingInfo.CurrentInstruments[i] = globalInfo.Instruments[instrumentNumber - 1];
										playingInfo.InstrumentNumbers[i] = (short)(instrumentNumber - 1);
									}

									oneMore = true;
								}
								else
									oneMore = true;
							}
							while (oneMore);
						}

						int currentTime = playingInfo.CurrentTime++;

						if (globalInfo.EndTime < 0)
						{
							if (tracksDone == numberOfChannels)
								endReached = true;
						}
						else if (currentTime == globalInfo.EndTime)
							endReached = true;

						if (endReached)
						{
							bool restart = false;

							if (playingInfo.RepeatCount < 0)
								restart = true;
							else
							{
								playingInfo.RepeatCount--;

								if (playingInfo.RepeatCount != 0)
									restart = true;
							}

							if (restart)
							{
								StartModuleAndInitializeInstruments();
								tracksDone++;
								beginOver = true;
							}

							OnEndReachedOnAllChannels(0);
							endReached = false;
						}
					}
					while (beginOver);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void SetupAndPlayInstruments()
		{
			for (ushort i = 0; i < numberOfChannels; i++)
			{
				VoiceInfo voice = voices[i];
				IInstrumentFormat format = voice.InstrumentFormat;

				if (((voice.InstrumentSetupSequence != InstrumentSetup.Nothing) || (voice.Status != VoiceStatus.Silence)) && (format != null))
					format.Setup(globalInfo, playingInfo, voice, VirtualChannels[i], i);

				if (format != null)
				{
					if (voice.InstrumentSetupSequence == InstrumentSetup.Initialize)
						voice.Status = VoiceStatus.Playing;
					else if (voice.InstrumentSetupSequence == InstrumentSetup.ReleaseNote)
						voice.Status = VoiceStatus.Stopping;
				}
				else
					voice.Status = VoiceStatus.Silence;

				voice.InstrumentSetupSequence = InstrumentSetup.Nothing;
			}

			for (ushort i = 0; i < numberOfChannels; i++)
			{
				VoiceInfo voice = voices[i];

				if (voice.Status != VoiceStatus.Silence)
					voice.InstrumentFormat.Play(playingInfo, voice, VirtualChannels[i], i);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void SetupInstrumentIfFormatChanged(Instrument instr, short instrumentNumber, int channelNumber, byte note, ushort volume)
		{
			if ((instr != null) && (instr.Format != null))
			{
				VoiceInfo voice = voices[channelNumber];

				if (voice.Status != VoiceStatus.Silence)
				{
					Type newFormat = instr.Format.GetType();
					if (newFormat != voice.InstrumentFormat.GetType())
						MuteInstrument(channelNumber);
				}

				voice.InstrumentFormat = instr.Format;
				voice.InstrumentNumber = instrumentNumber;
				voice.Note = note;
				voice.Volume = (ushort)(volume & 0xff);
				voice.InstrumentSetupSequence = InstrumentSetup.Initialize;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MuteInstrument(int channelNumber)
		{
			VoiceInfo voice = voices[channelNumber];

			voice.InstrumentSetupSequence = InstrumentSetup.Nothing;

			if (voice.Status != VoiceStatus.Silence)
			{
				voice.InstrumentSetupSequence = InstrumentSetup.Mute;

				IInstrumentFormat format = voice.InstrumentFormat;
				if (format != null)
					format.Setup(globalInfo, playingInfo, voice, VirtualChannels[channelNumber], channelNumber);

				voice.Status = VoiceStatus.Silence;
				voice.InstrumentSetupSequence = InstrumentSetup.Nothing;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void BeginToReleaseVoice(int channelNumber)
		{
			VoiceInfo voiceInfo = voices[channelNumber];

			voiceInfo.InstrumentSetupSequence = InstrumentSetup.Nothing;

			if (voiceInfo.Status == VoiceStatus.Playing)
				voiceInfo.InstrumentSetupSequence = InstrumentSetup.ReleaseNote;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the different volumes
		/// </summary>
		/********************************************************************/
		private void SetVolume(ushort scale, ushort newVolume)
		{
			playingInfo.NewVolume = 0;

			if (scale != 0)
			{
				playingInfo.MaxVolume = (ushort)(playingInfo.CurrentVolume * 256);
				globalInfo.Volume = (ushort)(newVolume * 256);

				int volume = playingInfo.MaxVolume - globalInfo.Volume;
				if (volume < 0)
					volume = -volume;

				volume /= scale;

				if (volume == 0)
					volume++;

				playingInfo.NewVolume = (ushort)volume;
			}
			else
				playingInfo.CurrentVolume = newVolume;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FindVolume()
		{
			if (playingInfo.NewVolume != 0)
			{
				int temp1 = playingInfo.NewVolume * playingInfo.CalculatedTempo;
				if (temp1 >= 0)
				{
					temp1 >>= 15;
					temp1 &= 0xffff;

					int temp2 = temp1;
					int newVolume = globalInfo.Volume - playingInfo.MaxVolume;

					if (newVolume < 0)
					{
						temp2 = -temp2;
						newVolume = -newVolume;
					}

					if (temp1 < newVolume)
					{
						temp2 += playingInfo.MaxVolume;
						playingInfo.MaxVolume = (ushort)temp2;
						playingInfo.CurrentVolume = (ushort)(temp2 / 256);
						return;
					}
				}

				if (globalInfo.Volume == 0)
				{
					bool isSet = (playingInfo.Flag & 0x01) != 0;
					playingInfo.Flag &= 0xfe;

					if (isSet)
						ReleaseAllVoices();
				}

				playingInfo.NewVolume = 0;
				playingInfo.CurrentVolume = (ushort)(globalInfo.Volume / 256);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current tempo
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
			ShowTempo();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the current tempo
		/// </summary>
		/********************************************************************/
		private string FormatTempo()
		{
			return PlayingFrequency.ToString("F2", CultureInfo.InvariantCulture);
		}
		#endregion
	}
}
