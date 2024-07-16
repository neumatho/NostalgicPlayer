/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.ArtOfNoise.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Types;
using Polycode.NostalgicPlayer.Kit.Extensions;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.ArtOfNoise
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class ArtOfNoiseWorker : ModulePlayerWithPositionDurationAgentBase
	{
		private static readonly Dictionary<Guid, ModuleType> moduleTypeLookup = new Dictionary<Guid, ModuleType>
		{
			{ ArtOfNoise.Agent1Id, ModuleType.ArtOfNoise },
			{ ArtOfNoise.Agent2Id, ModuleType.ArtOfNoise8V }
		};

		private readonly ModuleType currentModuleType;

		private string songName;
		private string author;
		private string[] comments;

		private int numberOfChannels;

		private byte numberOfPositions;
		private byte restartPosition;
		private byte[] positionList;

		private Pattern[] patterns;
		private Instrument[] instruments;

		private byte[][] arpeggios;
		private sbyte[][] waveForms;

		private GlobalPlayingInfo playingInfo;
		private VoiceInfo[] voices;

		private bool endReached;
		private bool restartSong;

		private const int InfoPositionLine = 3;
		private const int InfoPatternLine = 4;
		private const int InfoSpeedLine = 5;
		private const int InfoTempoLine = 6;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ArtOfNoiseWorker(Guid typeId)
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
		public override string[] FileExtensions => new [] { "aon", "aon8" };



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
		public override string ModuleName => songName;



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
		public override string[] Comment => comments;



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
					description = Resources.IDS_AON_INFODESCLINE0;
					value = numberOfPositions.ToString();
					break;
				}

				// Used patterns
				case 1:
				{
					description = Resources.IDS_AON_INFODESCLINE1;
					value = patterns.Length.ToString();
					break;
				}

				// Supported / used samples
				case 2:
				{
					description = Resources.IDS_AON_INFODESCLINE2;
					value = instruments.Length.ToString();
					break;
				}

				// Playing position
				case 3:
				{
					description = Resources.IDS_AON_INFODESCLINE3;
					value = FormatPosition();
					break;
				}

				// Playing pattern
				case 4:
				{
					description = Resources.IDS_AON_INFODESCLINE4;
					value = FormatPattern();
					break;
				}

				// Current speed
				case 5:
				{
					description = Resources.IDS_AON_INFODESCLINE5;
					value = FormatSpeed();
					break;
				}

				// Current tempo (BPM)
				case 6:
				{
					description = Resources.IDS_AON_INFODESCLINE6;
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

				// Skip the header
				moduleStream.Seek(46, SeekOrigin.Begin);

				// Set number of channels based on the format
				numberOfChannels = currentModuleType == ModuleType.ArtOfNoise8V ? 8 : 4;

				// Set default values
				comments = [];

				// Okay, now read each chunk and parse them
				for (;;)
				{
					// Read the chunk name and length
					uint chunkName = moduleStream.Read_B_UINT32();
					int chunkSize = moduleStream.Read_B_INT32();

					// Do we have any chunks left?
					if (moduleStream.EndOfStream)
						break;			// No, stop the loading

					if ((chunkSize > (moduleStream.Length - moduleStream.Position)))
					{
						errorMessage = Resources.IDS_AON_ERR_LOADING_HEADER;
						return AgentResult.Error;
					}

					// Find out what the chunk is and begin to parse it
					switch (chunkName)
					{
						// Song name (NAME)
						case 0x4e414d45:
						{
							ParseName(moduleStream, chunkSize, out errorMessage);
							break;
						}

						// Author (AUTH)
						case 0x41555448:
						{
							ParseAuth(moduleStream, chunkSize, out errorMessage);
							break;
						}

						// Remark (RMRK)
						case 0x524d524b:
						{
							ParseRmrk(moduleStream, chunkSize, out errorMessage);
							break;
						}

						// Information (INFO)
						case 0x494e464f:
						{
							ParseInfo(moduleStream, chunkSize, out errorMessage);
							break;
						}

						// Arpeggios (ARPG)
						case 0x41525047:
						{
							ParseArpg(moduleStream, out errorMessage);
							break;
						}

						// Position list (PLST)
						case 0x504c5354:
						{
							ParsePlst(moduleStream, chunkSize, out errorMessage);
							break;
						}

						// Patterns (PATT)
						case 0x50415454:
						{
							ParsePatt(moduleStream, chunkSize, out errorMessage);
							break;
						}

						// Instruments (INST)
						case 0x494e5354:
						{
							ParseInst(moduleStream, chunkSize, out errorMessage);
							break;
						}

						// Instrument names (INAM) - Does not always exist
						case 0x494e414d:
						{
							ParseInam(moduleStream, chunkSize, out errorMessage);
							break;
						}

						// Wave form lengths (WLEN)
						case 0x574c454e:
						{
							ParseWlen(moduleStream, chunkSize, out errorMessage);
							break;
						}

						// Sample data (WAVE)
						case 0x57415645:
						{
							ParseWave(moduleStream, out errorMessage);
							break;
						}

						// Unknown chunks
						default:
						{
							moduleStream.Seek(chunkSize, SeekOrigin.Current);
							break;
						}
					}

					if (!string.IsNullOrEmpty(errorMessage))
					{
						Cleanup();
						return AgentResult.Error;
					}
				}

				if ((numberOfPositions == 0) || (arpeggios == null) || (positionList == null) || (patterns == null) || (instruments == null) || (waveForms == null))
				{
					Cleanup();

					errorMessage = Resources.IDS_AON_ERR_LOADING_HEADER;

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
			playingInfo.FrameCnt++;

			if (playingInfo.Speed != 0)
			{
				if (playingInfo.FrameCnt >= playingInfo.Speed)
				{
					playingInfo.FrameCnt = 0;
					PlayNewStep();
				}
			}

			PlayFx();

			ChannelPanningType[] pannings = currentModuleType == ModuleType.ArtOfNoise8V ? Tables.Pan8 : Tables.Pan4;

			for (int i = 0; i < numberOfChannels; i++)
			{
				SetupChannel(voices[i], VirtualChannels[i]);
				VirtualChannels[i].SetPanning((ushort)(pannings[i]));
			}

			// Have we reached the end of the module
			if (endReached)
			{
				OnEndReached(playingInfo.Position);
				endReached = false;

				if (restartSong)
				{
					RestartSong();
					restartSong = false;
				}

				MarkPositionAsVisited(playingInfo.Position);
			}
		}



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
				foreach (Instrument instr in instruments)
				{
					// Build frequency table
					uint[] freqs = new uint[10 * 12];

					for (int j = 0; j < Tables.Periods.GetLength(1); j++)
					{
						uint period = Tables.Periods[instr.FineTune, j];
						freqs[2 * 12 + j] = 3546895U / period;
					}

					SampleInfo sampleInfo = new SampleInfo
					{
						Name = instr.Name,
						Flags = SampleInfo.SampleFlag.None,
						Volume = (ushort)(instr.Volume * 4),
						Panning = -1,
						NoteFrequencies = freqs
					};

					if (instr is SampleInstrument sampleInstr)
					{
						sampleInfo.Type = SampleInfo.SampleType.Sample;
						sampleInfo.Sample = waveForms[sampleInstr.WaveForm];
						sampleInfo.SampleOffset = sampleInstr.StartOffset * 2;
						sampleInfo.Length = sampleInstr.Length * 2;

						if (sampleInstr.LoopLength > 1)
						{
							sampleInfo.LoopStart = sampleInstr.LoopStart * 2;
							sampleInfo.LoopLength = sampleInstr.LoopLength * 2;
							sampleInfo.Flags |= SampleInfo.SampleFlag.Loop;
						}
					}
					else if (instr is SynthInstrument synthInstr)
					{
						sampleInfo.Type = SampleInfo.SampleType.Synthesis;
						sampleInfo.Length = synthInstr.Length * 2U;
					}
					else
						continue;

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
			InitializeSound(startPosition);
			MarkPositionAsVisited(startPosition);

			return startPosition;
		}



		/********************************************************************/
		/// <summary>
		/// Return the total number of positions
		/// </summary>
		/********************************************************************/
		protected override int GetTotalNumberOfPositions()
		{
			return numberOfPositions;
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

			foreach (VoiceInfo voiceInfo in voices)
			{
				if (voiceInfo.WaveForm != null)
					voiceInfo.ChFlag |= 0x02;
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
			if (moduleStream.Length < 54)
				return ModuleType.Unknown;

			// Read the module mark
			moduleStream.Seek(0, SeekOrigin.Begin);
			uint mark = moduleStream.Read_B_UINT32();

			// Check the mark
			if (mark == 0x414f4e34)					// AON4
				return ModuleType.ArtOfNoise;

			if (mark == 0x414f4e38)					// AON8
				return ModuleType.ArtOfNoise8V;

			return ModuleType.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Parses the NAME chunk
		/// </summary>
		/********************************************************************/
		private void ParseName(ModuleStream moduleStream, int chunkSize, out string errorMessage)
		{
			errorMessage = string.Empty;

			Encoding encoder = EncoderCollection.Amiga;

			songName = moduleStream.ReadString(encoder, chunkSize);

			if (moduleStream.EndOfStream)
				errorMessage = Resources.IDS_AON_ERR_LOADING_HEADER;
		}



		/********************************************************************/
		/// <summary>
		/// Parses the AUTH chunk
		/// </summary>
		/********************************************************************/
		private void ParseAuth(ModuleStream moduleStream, int chunkSize, out string errorMessage)
		{
			errorMessage = string.Empty;

			Encoding encoder = EncoderCollection.Amiga;

			author = moduleStream.ReadString(encoder, chunkSize);

			if (moduleStream.EndOfStream)
				errorMessage = Resources.IDS_AON_ERR_LOADING_HEADER;
		}



		/********************************************************************/
		/// <summary>
		/// Parses the RMRK chunk
		/// </summary>
		/********************************************************************/
		private void ParseRmrk(ModuleStream moduleStream, int chunkSize, out string errorMessage)
		{
			errorMessage = string.Empty;

			if (chunkSize > 0)
			{
				Encoding encoder = EncoderCollection.Amiga;

				string remark = moduleStream.ReadString(encoder, chunkSize);

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_AON_ERR_LOADING_HEADER;
					return;
				}

				comments = string.IsNullOrWhiteSpace(remark) ? [] : remark.Split('\n').Select(x => x.ConvertTabs(8)).ToArray();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parses the INFO chunk
		/// </summary>
		/********************************************************************/
		private void ParseInfo(ModuleStream moduleStream, int chunkSize, out string errorMessage)
		{
			errorMessage = string.Empty;

			// Skip version
			moduleStream.Seek(1, SeekOrigin.Current);

			numberOfPositions = moduleStream.Read_UINT8();
			restartPosition = moduleStream.Read_UINT8();

			if (restartPosition >= numberOfPositions)
				restartPosition = 0;

			if (moduleStream.EndOfStream)
			{
				errorMessage = Resources.IDS_AON_ERR_LOADING_HEADER;
				return;
			}

			// Skip rest of chunk
			moduleStream.Seek(chunkSize - 3, SeekOrigin.Current);
		}



		/********************************************************************/
		/// <summary>
		/// Parses the ARPG chunk
		/// </summary>
		/********************************************************************/
		private void ParseArpg(ModuleStream moduleStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			// 16 arpeggios, 4 bytes each
			arpeggios = ArrayHelper.InitializeArray<byte>(16, 4);

			for (int i = 0; i < 16; i++)
				moduleStream.Read(arpeggios[i], 0, 4);

			if (moduleStream.EndOfStream)
				errorMessage = Resources.IDS_AON_ERR_LOADING_HEADER;
		}



		/********************************************************************/
		/// <summary>
		/// Parses the PLST chunk
		/// </summary>
		/********************************************************************/
		private void ParsePlst(ModuleStream moduleStream, int chunkSize, out string errorMessage)
		{
			errorMessage = string.Empty;

			positionList = new byte[chunkSize];

			moduleStream.Read(positionList, 0, chunkSize);

			if (moduleStream.EndOfStream)
				errorMessage = Resources.IDS_AON_ERR_LOADING_HEADER;
		}



		/********************************************************************/
		/// <summary>
		/// Parses the PATT chunk
		/// </summary>
		/********************************************************************/
		private void ParsePatt(ModuleStream moduleStream, int chunkSize, out string errorMessage)
		{
			errorMessage = string.Empty;

			int numberOfPatterns = chunkSize / (4 * numberOfChannels * 64);
			patterns = new Pattern[numberOfPatterns];

			for (int i = 0; i < numberOfPatterns; i++)
			{
				Pattern pattern = new Pattern
				{
					Tracks = new TrackLine[64, numberOfChannels]
				};

				for (int r = 0; r < 64; r++)
				{
					for (int c = 0; c < numberOfChannels; c++)
					{
						TrackLine trackLine = new TrackLine();

						byte b1 = moduleStream.Read_UINT8();
						byte b2 = moduleStream.Read_UINT8();
						byte b3 = moduleStream.Read_UINT8();
						byte b4 = moduleStream.Read_UINT8();

						trackLine.Instrument = (byte)(b2 & 0x3f);
						trackLine.Note = (byte)(b1 & 0x3f);
						trackLine.Arpeggio = (byte)(((b3 & 0xc0) >> 4) | ((b2 & 0xc0) >> 6));
						trackLine.Effect = (Effect)(b3 & 0x3f);
						trackLine.EffectArg = b4;

						pattern.Tracks[r, c] = trackLine;
					}
				}

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_AON_ERR_LOADING_PATTERNS;
					return;
				}

				patterns[i] = pattern;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parses the INST chunk
		/// </summary>
		/********************************************************************/
		private void ParseInst(ModuleStream moduleStream, int chunkSize, out string errorMessage)
		{
			errorMessage = string.Empty;

			int numberOfInstruments = chunkSize / 32;
			instruments = new Instrument[numberOfInstruments];

			for (int i = 0; i < numberOfInstruments; i++)
			{
				byte type = moduleStream.Read_UINT8();
				byte volume = moduleStream.Read_UINT8();
				byte fineTune = moduleStream.Read_UINT8();
				byte waveForm = moduleStream.Read_UINT8();

				Instrument instr;

				switch (type)
				{
					// Sample
					case 0:
					{
						SampleInstrument sampleInstr = new SampleInstrument();
						instr = sampleInstr;

						sampleInstr.StartOffset = moduleStream.Read_B_UINT32();
						sampleInstr.Length = moduleStream.Read_B_UINT32();
						sampleInstr.LoopStart = moduleStream.Read_B_UINT32();
						sampleInstr.LoopLength = moduleStream.Read_B_UINT32();

						moduleStream.Seek(8, SeekOrigin.Current);
						break;
					}

					// Synth
					case 1:
					{
						SynthInstrument synthInstr = new SynthInstrument();
						instr = synthInstr;

						synthInstr.Length = moduleStream.Read_UINT8();

						moduleStream.Seek(5, SeekOrigin.Current);

						synthInstr.VibParam = moduleStream.Read_UINT8();
						synthInstr.VibDelay = moduleStream.Read_UINT8();
						synthInstr.VibWave = moduleStream.Read_UINT8();
						synthInstr.WaveSpeed = moduleStream.Read_UINT8();
						synthInstr.WaveLength = moduleStream.Read_UINT8();
						synthInstr.WaveLoopStart = moduleStream.Read_UINT8();
						synthInstr.WaveLoopLength = moduleStream.Read_UINT8();
						synthInstr.WaveLoopControl = moduleStream.Read_UINT8();

						moduleStream.Seek(10, SeekOrigin.Current);
						break;
					}

					default:
					{
						errorMessage = Resources.IDS_AON_ERR_LOADING_INSTRUMENTS;
						return;
					}
				}

				instr.EnvelopeStart = moduleStream.Read_UINT8();
				instr.EnvelopeAdd = moduleStream.Read_UINT8();
				instr.EnvelopeEnd = moduleStream.Read_UINT8();
				instr.EnvelopeSub = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_AON_ERR_LOADING_INSTRUMENTS;
					return;
				}

				instr.Name = string.Empty;
				instr.Volume = volume;
				instr.FineTune = fineTune;
				instr.WaveForm = waveForm;

				instruments[i] = instr;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parses the INAM chunk
		/// </summary>
		/********************************************************************/
		private void ParseInam(ModuleStream moduleStream, int chunkSize, out string errorMessage)
		{
			errorMessage = string.Empty;

			Encoding encoder = EncoderCollection.Amiga;
			byte[] buffer = new byte[32 + 1];

			foreach (Instrument instr in instruments)
			{
				moduleStream.ReadString(buffer, 32);

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_AON_ERR_LOADING_INSTRUMENTS;
					return;
				}

				instr.Name = encoder.GetString(buffer);
				chunkSize -= 32;
			}

			// Skip the rest of the instrument names
			moduleStream.Seek(chunkSize, SeekOrigin.Current);
		}



		/********************************************************************/
		/// <summary>
		/// Parses the WLEN chunk
		/// </summary>
		/********************************************************************/
		private void ParseWlen(ModuleStream moduleStream, int chunkSize, out string errorMessage)
		{
			errorMessage = string.Empty;

			int numberOfWaveForms = chunkSize / 4;
			waveForms = new sbyte[numberOfWaveForms][];

			for (int i = 0; i < numberOfWaveForms; i++)
			{
				uint length = moduleStream.Read_B_UINT32();
				if (length != 0)
					waveForms[i] = new sbyte[length];
			}

			if (moduleStream.EndOfStream)
				errorMessage = Resources.IDS_AON_ERR_LOADING_SAMPLES;
		}



		/********************************************************************/
		/// <summary>
		/// Parses the WAVE chunk
		/// </summary>
		/********************************************************************/
		private void ParseWave(ModuleStream moduleStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			for (int i = 0; i < waveForms.Length; i++)
			{
				sbyte[] w = waveForms[i];

				if (w != null)
				{
					int length = w.Length;

					moduleStream.ReadSampleData(i, w, length);

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_AON_ERR_LOADING_SAMPLES;
						return;
					}
				}
			}
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
				Tempo = 125,
				Speed = 6,
				FrameCnt = 0,

				PatternBreak = false,
				PatCnt = 0,
				PatDelayCnt = 0,

				LoopFlag = false,
				LoopPoint = 0,
				LoopCnt = 0,

				Position = (byte)startPosition,
				CurrentPattern = positionList[startPosition],

				NoiseAvoid = false,
				Oversize = false,

				Event = new byte[3]
			};

			voices = new VoiceInfo[numberOfChannels];

			for (int i = 0; i < numberOfChannels; i++)
			{
				voices[i] = new VoiceInfo
				{
					ChFlag = 0,
					LastNote = 0,

					WaveForm = null,
					WaveFormOffset = 0,
					WaveLen = 0,
					OldWaveLen = 0,
					Instrument = null,
					InstrumentNumber = 0,
					Volume = 0,

					StepFxCnt = 0,

					ChMode = 0,

					Period = 0,
					PerSlide = 0,

					ArpeggioOff = 0,
					ArpeggioFineTune = 0,
					ArpeggioTab = new short[8],
					ArpeggioSpd = 0,
					ArpeggioCnt = 0,

					SynthWaveAct = null,
					SynthWaveActOffset = 0,
					SynthWaveEndOffset = 0,
					SynthWaveRep = null,
					SynthWaveRepOffset = 0,
					SynthWaveRepEndOffset = 0,
					SynthWaveAddBytes = 0,
					SynthWaveCnt = 0,
					SynthWaveSpd = 0,
					SynthWaveRepCtrl = 0,
					SynthWaveCont = 0,
					SynthWaveStop = 0,
					SynthAdd = 0,
					SynthSub = 0,
					SynthEnd = 0,
					SynthEnv = EnvelopeState.Done,
					SynthVol = 0,

					VibOn = 0,
					VibDone = false,
					VibCont = 0,
					VibratoSpd = 0,
					VibratoAmpl = 0,
					VibratoPos = 0,
					VibratoTrigDelay = 0,

					FxCom = Effect.Arpeggio,
					FxDat = 0,

					SlideFlag = false,

					OldSampleOffset = 0,
					GlissSpd = 0,

					TrackVolume = 64
				};
			}

			endReached = false;
			restartSong = false;
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player have allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			comments = null;

			positionList = null;
			patterns = null;
			instruments = null;
			waveForms = null;

			arpeggios = null;

			playingInfo = null;
			voices = null;
		}



		/********************************************************************/
		/// <summary>
		/// Move to next row/position
		/// </summary>
		/********************************************************************/
		private void PlayNewStep()
		{
			bool oneMoreTime;

			do
			{
				oneMoreTime = false;

				if (!playingInfo.PatternBreak)
				{
					if (playingInfo.PatDelayCnt > 0)
					{
						playingInfo.PatDelayCnt--;
						return;
					}

					playingInfo.PatDelayCnt = -1;

					Pattern pattern = patterns[playingInfo.CurrentPattern];

					for (int i = 0; i < numberOfChannels; i++)
						GetDaChannel(pattern.Tracks[playingInfo.PatCnt, i], voices[i]);

					if (playingInfo.LoopFlag)
					{
						playingInfo.LoopFlag = false;
						playingInfo.PatCnt = playingInfo.LoopPoint;
						return;
					}

					playingInfo.PatCnt++;
					if (playingInfo.PatCnt < 64)
						return;
				}
				else
					playingInfo.Position = playingInfo.NewPosition;

				playingInfo.PatCnt = 0;
				playingInfo.PatDelayCnt = 0;
				playingInfo.LoopPoint = 0;
				playingInfo.LoopCnt = 0;

				playingInfo.Position++;
				if (playingInfo.Position >= numberOfPositions)
				{
					playingInfo.Position = restartPosition;
					endReached = true;
				}
				else
				{
					if (HasPositionBeenVisited(playingInfo.Position))
						endReached = true;
				}

				MarkPositionAsVisited(playingInfo.Position);
				ShowPosition();

				if (playingInfo.PatternBreak)
				{
					playingInfo.PatternBreak = false;
					playingInfo.CurrentPattern = positionList[playingInfo.Position];

					ShowPattern();

					oneMoreTime = true;
				}
			}
			while (oneMoreTime);
		}



		/********************************************************************/
		/// <summary>
		/// Parse new step for a single channel
		/// </summary>
		/********************************************************************/
		private void GetDaChannel(TrackLine trackLine, VoiceInfo voiceInfo)
		{
			voiceInfo.FxCom = trackLine.Effect;
			voiceInfo.FxDat = trackLine.EffectArg;

			if (voiceInfo.FxCom == Effect.PatternBreak)
			{
				if (playingInfo.PatCnt == 63)
					playingInfo.PatCnt = 62;
			}

			byte argHi = (byte)(voiceInfo.FxDat & 0xf0);
			byte argLow = (byte)(voiceInfo.FxDat & 0x0f);

			if (voiceInfo.FxCom == Effect.NewVolume)
				voiceInfo.StepFxCnt = argLow;
			else if (voiceInfo.FxCom == Effect.ExtraEffects)
			{
				ExtraEffect extra = (ExtraEffect)argHi;

				if (extra == ExtraEffect.NoteCut)
					voiceInfo.StepFxCnt = argLow;
				else if (extra == ExtraEffect.PatternDelay)
				{
					if (playingInfo.PatDelayCnt < 0)
						playingInfo.PatDelayCnt = (sbyte)argLow;
				}
				else if (extra == ExtraEffect.PatternLoop)
				{
					if ((playingInfo.LoopCnt != 0xf0) && (argLow != 0))
					{
						if (playingInfo.LoopCnt == 0)
							playingInfo.LoopCnt = argLow;

						playingInfo.LoopCnt--;
						if (playingInfo.LoopCnt == 0)
							playingInfo.LoopCnt = 0xf0;

						playingInfo.LoopFlag = true;
					}
				}
			}

			Instrument instr;

			short instrNum = (short)(trackLine.Instrument - 1);
			if (instrNum < 0)
			{
				// Old instrument
				instr = voiceInfo.Instrument;
				if (instr == null)
					return;		// No instrument

				if (trackLine.Note != 0)
				{
					if ((voiceInfo.FxCom != Effect.TonePortamento) && (voiceInfo.FxCom != Effect.TonePortamentoAndVolumeSlide) && (voiceInfo.FxCom != Effect.SetVolumeAndTonePortamento) && (voiceInfo.FxCom != Effect.FineVolumeSlideAndTonePortamento))
						UseOldInstrument(trackLine, voiceInfo, instr);
				}
			}
			else
			{
				if (instrNum >= instruments.Length)
					return;

				if (trackLine.Note == 0)	// No note, then only set repeat
				{
					instr = instruments[instrNum];

					if (instr is SampleInstrument sampleInstrument)
					{
						if (voiceInfo.Instrument != sampleInstrument)
						{
							voiceInfo.Instrument = sampleInstrument;
							voiceInfo.InstrumentNumber = instrNum;
							voiceInfo.ChFlag = 1;

							StartRepeat(trackLine, voiceInfo, sampleInstrument);
						}
					}

					voiceInfo.Volume = instr.Volume;
				}
				else
				{
					voiceInfo.OldSampleOffset = 0;

					instr = instruments[instrNum];

					if ((voiceInfo.Instrument != instr) || ((voiceInfo.FxCom != Effect.TonePortamento) && (voiceInfo.FxCom != Effect.TonePortamentoAndVolumeSlide) && (voiceInfo.FxCom != Effect.SetVolumeAndTonePortamento) && (voiceInfo.FxCom != Effect.FineVolumeSlideAndTonePortamento)))
					{
						voiceInfo.Instrument = instr;
						voiceInfo.InstrumentNumber = instrNum;
						UseOldInstrument(trackLine, voiceInfo, instr);
					}

					voiceInfo.Volume = instr.Volume;
				}
			}

			byte note = trackLine.Note;
			if (note == 0)
			{
				note = voiceInfo.LastNote;
				if ((note == 0) || note > 60)
					return;
			}
			else
			{
				voiceInfo.SlideFlag = false;
				voiceInfo.LastNote = note;

				if (note > 60)
					return;
			}

			voiceInfo.ArpeggioFineTune = instr.FineTune;
			note--;

			if ((voiceInfo.FxCom == Effect.SetVolumeAndTonePortamento) || (voiceInfo.FxCom == Effect.FineVolumeSlideAndTonePortamento) || (voiceInfo.FxCom == Effect.TonePortamentoAndVolumeSlide) || (voiceInfo.FxCom == Effect.TonePortamento))
			{
				if (trackLine.Note != 0)
				{
					voiceInfo.SlideFlag = true;

					ushort period = Tables.Periods[voiceInfo.ArpeggioFineTune, note];
					voiceInfo.PerSlide = (short)(voiceInfo.Period + voiceInfo.PerSlide - period);
				}
			}

			if (voiceInfo.ArpeggioTab[1] == -1)
			{
				voiceInfo.ArpeggioOff = 0;
				voiceInfo.ArpeggioCnt = 0;
			}

			if ((voiceInfo.FxCom == Effect.Arpeggio) && (voiceInfo.FxDat != 0))
			{
				// ProTracker arpeggio
				byte arp1 = (byte)((voiceInfo.FxDat & 0xf0) >> 4);
				byte arp2 = (byte)(voiceInfo.FxDat & 0x0f);

				voiceInfo.ArpeggioTab[0] = note;
				voiceInfo.ArpeggioTab[1] = (short)(note + arp1);
				voiceInfo.ArpeggioTab[2] = (short)(note + arp2);
				voiceInfo.ArpeggioTab[3] = -1;
			}
			else
			{
				// Professional arpeggio
				byte[] arp = arpeggios[trackLine.Arpeggio];

				int readOffset = 0;
				int writeOffset = 0;

				byte arpByte = arp[readOffset++];
				byte count = (byte)(arpByte >> 4);
				if (count != 0)
				{
					voiceInfo.ArpeggioTab[writeOffset++] = (short)((arpByte & 0x0f) + note);
					count--;

					while (count > 0)
					{
						arpByte = arp[readOffset];

						voiceInfo.ArpeggioTab[writeOffset++] = (short)(((arpByte & 0xf0) >> 4) + note);
						count--;
						if (count == 0)
							break;

						voiceInfo.ArpeggioTab[writeOffset++] = (short)((arpByte & 0x0f) + note);
						count--;
					}
				}
				else
				{
					voiceInfo.ArpeggioOff = 0;
					voiceInfo.ArpeggioCnt = (byte)(voiceInfo.ArpeggioSpd - 1);

					voiceInfo.ArpeggioTab[writeOffset++] = note;
				}

				voiceInfo.ArpeggioTab[writeOffset] = -1;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Start playing a sample or synth instrument
		/// </summary>
		/********************************************************************/
		private void UseOldInstrument(TrackLine trackLine, VoiceInfo voiceInfo, Instrument instr)
		{
			voiceInfo.VibCont = 0;

			InitAdsr(voiceInfo, instr);

			if (instr is SampleInstrument sampleInstrument)
				StartSample(trackLine, voiceInfo, sampleInstrument);
			else
				InitSynth(trackLine, voiceInfo, (SynthInstrument)instr);
		}



		/********************************************************************/
		/// <summary>
		/// Start playing a sample instrument
		/// </summary>
		/********************************************************************/
		private void StartSample(TrackLine trackLine, VoiceInfo voiceInfo, SampleInstrument sampleInstrument)
		{
			voiceInfo.VibOn = 0x21;
			voiceInfo.ChMode = 0;

			if (voiceInfo.FxCom == Effect.ExtraEffects)
			{
				ExtraEffect extraEffect = (ExtraEffect)(voiceInfo.FxDat & 0xf0);
				byte argLow = (byte)(voiceInfo.FxDat & 0x0f);

				if (extraEffect == ExtraEffect.NoteDelay)
					voiceInfo.StepFxCnt = argLow;
				else
				{
					if (extraEffect == ExtraEffect.RetrigNote)
						voiceInfo.StepFxCnt = argLow;

					voiceInfo.ChFlag = 3;		// 3 = New sample wave
				}
			}
			else
				voiceInfo.ChFlag = 3;		// 3 = New sample wave

			StartRepeat(trackLine, voiceInfo, sampleInstrument);
		}



		/********************************************************************/
		/// <summary>
		/// Setup repeat for the sample
		/// </summary>
		/********************************************************************/
		private void StartRepeat(TrackLine trackLine, VoiceInfo voiceInfo, SampleInstrument sampleInstrument)
		{
			if (trackLine.Note != 0)
				voiceInfo.PerSlide = 0;

			sbyte[] waveForm = waveForms[sampleInstrument.WaveForm];

			voiceInfo.OldWaveLen = voiceInfo.WaveLen;
			voiceInfo.WaveLen = (ushort)sampleInstrument.Length;

			// Is there any repeat?
			if (sampleInstrument.LoopLength != 0)
			{
				if (playingInfo.Oversize || (sampleInstrument.LoopStart != 0))
				{
					voiceInfo.RepeatStart = waveForm;
					voiceInfo.RepeatOffset = sampleInstrument.LoopStart * 2;
					voiceInfo.RepeatLength = (ushort)sampleInstrument.LoopLength;

					if (!playingInfo.Oversize)
						voiceInfo.WaveLen = (ushort)(sampleInstrument.LoopLength + sampleInstrument.LoopStart);
				}
				else
				{
					voiceInfo.RepeatStart = waveForm;
					voiceInfo.RepeatOffset = 0;
					voiceInfo.RepeatLength = (ushort)sampleInstrument.LoopLength;
				}
			}
			else
			{
				voiceInfo.RepeatStart = null;
				voiceInfo.RepeatOffset = 0;
				voiceInfo.RepeatLength = 0;
			}

			uint offset = sampleInstrument.StartOffset * 2;
			voiceInfo.WaveLen -= (ushort)(voiceInfo.OldSampleOffset / 2);

			if (voiceInfo.FxCom == Effect.SetSampleOffset)
			{
				uint newOffset = voiceInfo.FxDat * 256U;
				short newWaveLen = (short)(voiceInfo.WaveLen - (newOffset / 2));

				if (newWaveLen < 0)
				{
					voiceInfo.WaveForm = voiceInfo.RepeatStart;
					voiceInfo.WaveFormOffset = voiceInfo.RepeatOffset;
					voiceInfo.WaveLen = voiceInfo.RepeatLength;
				}
				else
				{
					voiceInfo.WaveLen = (ushort)newWaveLen;
					voiceInfo.OldSampleOffset += newOffset;

					voiceInfo.WaveForm = waveForm;
					voiceInfo.WaveFormOffset = offset + voiceInfo.OldSampleOffset;
				}
			}
			else
			{
				voiceInfo.WaveForm = waveForm;
				voiceInfo.WaveFormOffset = offset + voiceInfo.OldSampleOffset;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Initialize envelope
		/// </summary>
		/********************************************************************/
		private void InitAdsr(VoiceInfo voiceInfo, Instrument instr)
		{
			if ((voiceInfo.FxCom != Effect.SynthControl) || ((voiceInfo.FxDat & 0x01) == 0))
			{
				voiceInfo.SynthVol = instr.EnvelopeStart;

				if (instr.EnvelopeAdd != 0)
				{
					voiceInfo.SynthAdd = instr.EnvelopeAdd;
					voiceInfo.SynthSub = instr.EnvelopeSub;
					voiceInfo.SynthEnd = instr.EnvelopeEnd;

					voiceInfo.SynthEnv = EnvelopeState.Add;
				}
				else
				{
					voiceInfo.SynthVol = 127;
					voiceInfo.SynthEnv = EnvelopeState.Done;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Initialize synthesis instrument
		/// </summary>
		/********************************************************************/
		private void InitSynth(TrackLine trackLine, VoiceInfo voiceInfo, SynthInstrument synthInstrument)
		{
			voiceInfo.ChMode = 1;

			Effect effect = voiceInfo.FxCom;
			ExtraEffect extraEffect = (ExtraEffect)(voiceInfo.FxDat & 0xf0);

			if ((effect == Effect.ExtraEffects) && (extraEffect == ExtraEffect.RetrigNote))
				voiceInfo.StepFxCnt = (byte)(voiceInfo.FxDat & 0x0f);

			if (trackLine.Note != 0)
			{
				voiceInfo.PerSlide = 0;

				byte arg = effect == Effect.SynthControl ? voiceInfo.FxDat : (byte)0;

				if ((arg & 0x10) == 0)
				{
					if ((effect == Effect.ExtraEffects) && (extraEffect == ExtraEffect.NoteDelay))
						voiceInfo.StepFxCnt = (byte)(voiceInfo.FxDat & 0x0f);
					else
						voiceInfo.ChFlag = 3;		// 3 = New wave

					sbyte[] waveForm = waveForms[synthInstrument.WaveForm];

					bool reset = true;

					if ((voiceInfo.WaveForm == waveForm) && (voiceInfo.WaveFormOffset == 0))
					{
						voiceInfo.ChFlag = 0;

						reset = voiceInfo.SynthWaveCont == 0;
					}

					if (reset)
					{
						voiceInfo.WaveForm = waveForm;
						voiceInfo.WaveFormOffset = 0;

						ushort waveLength = synthInstrument.Length;
						uint offset = 0;

						if (effect == Effect.SetSampleOffset)
						{
							offset = (uint)voiceInfo.FxDat * waveLength;

							if (voiceInfo.SynthWaveStop != 0)
							{
								voiceInfo.SynthWaveAct = waveForm;
								voiceInfo.SynthWaveActOffset = offset;

								voiceInfo.RepeatStart = waveForm;
								voiceInfo.RepeatOffset = offset;
							}
						}

						if (voiceInfo.SynthWaveStop == 0)
						{
							voiceInfo.SynthWaveAct = waveForm;
							voiceInfo.SynthWaveActOffset = offset;

							voiceInfo.OldWaveLen = voiceInfo.WaveLen;

							voiceInfo.WaveLen = waveLength;
							voiceInfo.RepeatLength = waveLength;

							waveLength *= 2;
							voiceInfo.SynthWaveAddBytes = waveLength;

							voiceInfo.RepeatStart = waveForm;
							voiceInfo.RepeatOffset = (uint)synthInstrument.WaveLoopStart * waveLength + offset;

							voiceInfo.SynthWaveEndOffset = (uint)synthInstrument.WaveLength * waveLength + offset;

							voiceInfo.SynthWaveRep = waveForm;
							voiceInfo.SynthWaveRepOffset = (uint)synthInstrument.WaveLoopStart * waveLength + offset;

							voiceInfo.SynthWaveRepEndOffset = (uint)(synthInstrument.WaveLoopLength + synthInstrument.WaveLoopStart) * waveLength + offset;

							voiceInfo.SynthWaveCnt = synthInstrument.WaveSpeed;
							voiceInfo.SynthWaveSpd = synthInstrument.WaveSpeed;
							voiceInfo.SynthWaveRepCtrl = synthInstrument.WaveLoopControl;
						}
					}
				}

				// Initialize vibrator
				voiceInfo.VibOn = 0;

				if (synthInstrument.VibWave != 3)
				{
					voiceInfo.VibratoTrigDelay = synthInstrument.VibDelay;

					if (synthInstrument.VibParam != 0)
					{
						DoFxVibrato(voiceInfo, synthInstrument.VibParam);

						voiceInfo.VibratoAmpl &= 0b10011111;
						voiceInfo.VibratoAmpl |= (byte)((byte)(synthInstrument.VibWave >> 3) | (byte)(synthInstrument.VibWave << (8 - 3)));
						voiceInfo.VibCont = 1;
					}
					else
						voiceInfo.VibratoTrigDelay = -2;
				}
				else
					voiceInfo.VibOn = 0x21;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle all effects
		/// </summary>
		/********************************************************************/
		private void PlayFx()
		{
			byte pattern = positionList[playingInfo.Position];

			if (pattern != playingInfo.CurrentPattern)
			{
				playingInfo.CurrentPattern = pattern;
				ShowPattern();
			}

			for (int i = 0; i < numberOfChannels; i++)
				DoFx(voices[i]);
		}



		/********************************************************************/
		/// <summary>
		/// Handle effects for a single voice
		/// </summary>
		/********************************************************************/
		private void DoFx(VoiceInfo voiceInfo)
		{
			if (voiceInfo.VibCont == 0)
				voiceInfo.VibOn = 0x21;

			voiceInfo.ArpeggioCnt++;
			if (voiceInfo.ArpeggioCnt >= voiceInfo.ArpeggioSpd)
				voiceInfo.ArpeggioCnt = 0;

			short periodOffset = voiceInfo.ArpeggioTab[voiceInfo.ArpeggioOff];
			if (periodOffset >= 0)
			{
				if (periodOffset >= 60)
					periodOffset = 59;

				voiceInfo.Period = Tables.Periods[voiceInfo.ArpeggioFineTune, periodOffset];

				voiceInfo.ArpeggioOff++;
				voiceInfo.ArpeggioOff &= 0x07;
			}
			else
				voiceInfo.ArpeggioOff = 0;

			if (voiceInfo.FxCom != Effect.Arpeggio)
			{
				byte arg = voiceInfo.FxDat;

				if (playingInfo.FrameCnt != 0)
				{
					switch (voiceInfo.FxCom)
					{
						case Effect.SlideUp:
						{
							DoFxPortamentoUp(voiceInfo, arg);
							break;
						}

						case Effect.SlideDown:
						{
							DoFxPortamentoDown(voiceInfo, arg);
							break;
						}

						case Effect.TonePortamento:
						{
							if (arg != 0)
								voiceInfo.GlissSpd = arg;

							DoFxToneSlide(voiceInfo);
							break;
						}

						case Effect.Vibrato:
						{
							voiceInfo.VibOn = 1;
							DoFxVibrato(voiceInfo, arg);
							break;
						}

						case Effect.TonePortamentoAndVolumeSlide:
						{
							DoFxGlissVolumeSlide(voiceInfo, arg);
							break;
						}

						case Effect.VibratoAndVolumeSlide:
						{
							DoFxVibVolumeSlide(voiceInfo, arg);
							break;
						}

						case Effect.VolumeSlide:
						{
							DoFxVolumeSlide(voiceInfo, arg);
							break;
						}
					}
				}

				switch (voiceInfo.FxCom)
				{
					case Effect.PositionJump:
					{
						DoFxBreakTo(arg);
						break;
					}

					case Effect.SetVolume:
					{
						DoFxSetVolume(voiceInfo, arg);
						break;
					}

					case Effect.PatternBreak:
					{
						DoFxBreakPat(arg);
						break;
					}

					case Effect.ExtraEffects:
					{
						DoFxECommands(voiceInfo, arg);
						break;
					}

					case Effect.SetSpeed:
					{
						DoFxSetSpd(arg);
						break;
					}

					case Effect.NewVolume:
					{
						DoFxSetVolDel(voiceInfo, arg);
						break;
					}

					case Effect.WaveTableSpeedControl:
					{
						DoFxSetWaveAdsrSpd(voiceInfo, arg);
						break;
					}

					case Effect.SetArpSpeed:
					{
						DoFxSetArpSpd(voiceInfo, arg);
						break;
					}

					case Effect.SetVolumeAndVibrato:
					{
						DoFxVibSetVolume(voiceInfo, arg);
						break;
					}

					case Effect.FineSlideAndPortamentoUp:
					{
						DoFxPortVolSlideUp(voiceInfo, arg);
						break;
					}

					case Effect.FineSlideAndPortamentoDown:
					{
						DoFxPortVolSlideDown(voiceInfo, arg);
						break;
					}

					case Effect.AvoidNoise:
					{
						DoFxToggleNoiseAvoid(arg);
						break;
					}

					case Effect.Oversize:
					{
						DoFxToggleOversize(arg);
						break;
					}

					case Effect.FineVolumeSlideAndVibrato:
					{
						DoFxFineVolSlideVib(voiceInfo, arg);
						break;
					}

					case Effect.VolumeAndPortDownSlide:
					{
						DoFxSynthDrums(voiceInfo, arg);
						break;
					}

					case Effect.SetVolumeAndTonePortamento:
					{
						DoFxSetVolumePort(voiceInfo, arg);
						break;
					}

					case Effect.FineVolumeSlideAndTonePortamento:
					{
						DoFxFineVolSlidePort(voiceInfo, arg);
						break;
					}

					case Effect.SetTrackVolume:
					{
						DoFxSetTrackVol(voiceInfo, arg);
						break;
					}

					case Effect.SetWaveTableMode:
					{
						DoFxSetWaveCont(voiceInfo, arg);
						break;
					}

					case Effect.ExternalEvent:
					{
						DoFxExternalEvent(arg);
						break;
					}
				}
			}

			DoSynth(voiceInfo);
		}



		/********************************************************************/
		/// <summary>
		/// Effect 1xx: Slide up (Up speed)
		/// </summary>
		/********************************************************************/
		private void DoFxPortamentoUp(VoiceInfo voiceInfo, byte arg)
		{
			voiceInfo.PerSlide -= arg;
		}



		/********************************************************************/
		/// <summary>
		/// Effect 2xx: Slide down (Down speed)
		/// </summary>
		/********************************************************************/
		private void DoFxPortamentoDown(VoiceInfo voiceInfo, byte arg)
		{
			voiceInfo.PerSlide += arg;
		}



		/********************************************************************/
		/// <summary>
		/// Effect 3xx: Tone portamento (Up/down speed)
		/// </summary>
		/********************************************************************/
		private void DoFxToneSlide(VoiceInfo voiceInfo)
		{
			if (voiceInfo.SlideFlag && (voiceInfo.PerSlide != 0))
			{
				if (voiceInfo.PerSlide < 0)
				{
					voiceInfo.PerSlide += voiceInfo.GlissSpd;
					if (voiceInfo.PerSlide >= 0)
						voiceInfo.PerSlide = 0;
				}
				else
				{
					voiceInfo.PerSlide -= voiceInfo.GlissSpd;
					if (voiceInfo.PerSlide < 0)
						voiceInfo.PerSlide = 0;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Effect 4xy: Vibrato (x-Speed, y-Depth)
		/// </summary>
		/********************************************************************/
		private void DoFxVibrato(VoiceInfo voiceInfo, byte arg)
		{
			if (arg != 0)
			{
				byte spd = (byte)((arg & 0xf0) >> 4);
				if (spd != 0)
					voiceInfo.VibratoSpd = spd;

				byte ampl = (byte)(arg & 0x0f);
				if (ampl != 0)
				{
					voiceInfo.VibratoAmpl &= 0xf0;
					voiceInfo.VibratoAmpl |= ampl;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DoFxVibOldAmpl(VoiceInfo voiceInfo)
		{
			if (!voiceInfo.VibDone)
			{
				voiceInfo.VibDone = true;

				byte[] table;
				byte vibAmpl = (byte)(voiceInfo.VibratoAmpl & 0x60);

				if (vibAmpl == 0)
					table = Tables.VibratoSine;
				else if (vibAmpl == 32)
					table = Tables.VibratoRampDown;
				else
					table = Tables.VibratoSquare;

				byte vibVal = table[voiceInfo.VibratoPos];
				ushort add = (ushort)(((voiceInfo.VibratoAmpl & 0x0f) * vibVal) >> 7);

				if ((voiceInfo.VibratoAmpl & 0x80) != 0)
					voiceInfo.Period -= add;
				else
					voiceInfo.Period += add;

				voiceInfo.VibratoPos += voiceInfo.VibratoSpd;
				if (voiceInfo.VibratoPos >= 32)
				{
					voiceInfo.VibratoPos &= 0x1f;
					voiceInfo.VibratoAmpl ^= 0x80;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Effect 5xy: Tone portamento + volume slide (x-Up speed,
		/// y-Down speed)
		/// </summary>
		/********************************************************************/
		private void DoFxGlissVolumeSlide(VoiceInfo voiceInfo, byte arg)
		{
			DoFxToneSlide(voiceInfo);
			DoFxVolumeSlide(voiceInfo, arg);
		}



		/********************************************************************/
		/// <summary>
		/// Effect 6xy: Vibrato + volume slide (x-Up speed, y-Down speed)
		/// </summary>
		/********************************************************************/
		private void DoFxVibVolumeSlide(VoiceInfo voiceInfo, byte arg)
		{
			DoFxVibOldAmpl(voiceInfo);
			DoFxVolumeSlide(voiceInfo, arg);
		}



		/********************************************************************/
		/// <summary>
		/// Effect Axy: Volume slide (x-Up speed, y-Down speed)
		/// </summary>
		/********************************************************************/
		private void DoFxVolumeSlide(VoiceInfo voiceInfo, byte arg)
		{
			byte argLow = (byte)(arg & 0x0f);
			byte argHi = (byte)((arg & 0xf0) >> 4);
			sbyte vol = (sbyte)voiceInfo.Volume;

			if (argHi == 0)
			{
				vol = (sbyte)(vol - argLow);
				if (vol < 0)
					vol = 0;
			}
			else
			{
				vol = (sbyte)(vol + argHi);
				if (vol > 64)
					vol = 64;
			}

			voiceInfo.Volume = (byte)vol;
		}



		/********************************************************************/
		/// <summary>
		/// Effect Bxx: Position jump (Song position)
		/// </summary>
		/********************************************************************/
		private void DoFxBreakTo(byte arg)
		{
			playingInfo.NewPosition = (byte)(arg - 1);
			playingInfo.PatCnt = 0;
			playingInfo.PatternBreak = true;
		}



		/********************************************************************/
		/// <summary>
		/// Effect Cxx: Set volume (Volume 00-40)
		/// </summary>
		/********************************************************************/
		private void DoFxSetVolume(VoiceInfo voiceInfo, byte arg)
		{
			if (arg > 64)
				arg = 64;

			voiceInfo.Volume = arg;
		}



		/********************************************************************/
		/// <summary>
		/// Effect Dxx: Pattern break (Break position in next pattern)
		/// </summary>
		/********************************************************************/
		private void DoFxBreakPat(byte arg)
		{
			// Convert from hexadecimal to decimal
			byte argLow = (byte)(arg & 0x0f);
			byte argHi = (byte)(arg & 0xf0);

			byte temp = (byte)(argHi >> 1);
			byte pos = (byte)(temp + (temp >> 2));
			pos += argLow;

			if (pos >= 64)
				pos = 0;

			playingInfo.PatCnt = pos;
			playingInfo.NewPosition = playingInfo.Position;
			playingInfo.PatternBreak = true;
		}



		/********************************************************************/
		/// <summary>
		/// Effect Fxx: Set speed (Speed 00-1F / Tempo 20-FF)
		/// </summary>
		/********************************************************************/
		private void DoFxSetSpd(byte arg)
		{
			if (arg != 0)
			{
				if (arg <= 32)		// There seems to be a bug in the original player which include 0x20 as speed. We keep it
				{
					playingInfo.Speed = arg;
					ShowSpeed();
				}
				else
				{
					if (arg <= 200)
					{
						SetBpmTempo(arg);
						ShowTempo();
					}
				}
			}
			else
			{
				endReached = true;
				restartSong = true;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Effect Gxy: New volume (Set volume to 4+(x*4)+y vblanks)
		/// </summary>
		/********************************************************************/
		private void DoFxSetVolDel(VoiceInfo voiceInfo, byte arg)
		{
			if (voiceInfo.StepFxCnt == 0)
				voiceInfo.Volume = (byte)(((arg & 0xf0) >> 4) * 4 + 4);
			else
				voiceInfo.StepFxCnt--;
		}



		/********************************************************************/
		/// <summary>
		/// Effect Ixy: Wave table speed control (Wave speed=x)
		/// </summary>
		/********************************************************************/
		private void DoFxSetWaveAdsrSpd(VoiceInfo voiceInfo, byte arg)
		{
			voiceInfo.SynthWaveSpd = (byte)((arg & 0xf0) >> 4);
		}



		/********************************************************************/
		/// <summary>
		/// Effect Jxy: Set arg speed (Set arp spd=y)
		/// </summary>
		/********************************************************************/
		private void DoFxSetArpSpd(VoiceInfo voiceInfo, byte arg)
		{
			byte argLow = (byte)(arg & 0x0f);
			if (argLow != 0)
				voiceInfo.ArpeggioSpd = argLow;
		}



		/********************************************************************/
		/// <summary>
		/// Effect Kxx: Set volume and do vibrato (Set volume=xx)
		/// </summary>
		/********************************************************************/
		private void DoFxVibSetVolume(VoiceInfo voiceInfo, byte arg)
		{
			DoFxVibOldAmpl(voiceInfo);
			DoFxSetVolume(voiceInfo, arg);
		}



		/********************************************************************/
		/// <summary>
		/// Effect Lxy: Fine slide and portamento up
		/// (Vol slide=x, Portamento=y)
		/// </summary>
		/********************************************************************/
		private void DoFxPortVolSlideUp(VoiceInfo voiceInfo, byte arg)
		{
			byte argLow = (byte)(arg & 0x0f);
			byte argHi = (byte)((arg & 0xf0) >> 4);

			sbyte nibVal = Tables.NibbleTab[argHi];
			if (nibVal < 0)
				DoFxFineVolDown(voiceInfo, (byte)-nibVal);
			else
				DoFxFineVolUp(voiceInfo, (byte)nibVal);

			if (playingInfo.FrameCnt != 0)
				DoFxPortamentoUp(voiceInfo, argLow);
		}



		/********************************************************************/
		/// <summary>
		/// Effect Mxy: Fine slide and portamento down
		/// (Vol slide=x, Portamento=y)
		/// </summary>
		/********************************************************************/
		private void DoFxPortVolSlideDown(VoiceInfo voiceInfo, byte arg)
		{
			byte argLow = (byte)(arg & 0x0f);
			byte argHi = (byte)((arg & 0xf0) >> 4);

			sbyte nibVal = Tables.NibbleTab[argHi];
			if (nibVal < 0)
				DoFxFineVolDown(voiceInfo, (byte)-nibVal);
			else
				DoFxFineVolUp(voiceInfo, (byte)nibVal);

			if (playingInfo.FrameCnt != 0)
				DoFxPortamentoDown(voiceInfo, argLow);
		}



		/********************************************************************/
		/// <summary>
		/// Effect Nxy: Avoid noise (Wave less than 512 bytes)
		/// (y!=0 = Noise avoid on, y=0 = Off)
		/// </summary>
		/********************************************************************/
		private void DoFxToggleNoiseAvoid(byte arg)
		{
			playingInfo.NoiseAvoid = arg != 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect Oxy: Play samples bigger than 128 kb
		/// (y!=0 = On, y=0 = Off)
		/// </summary>
		/********************************************************************/
		private void DoFxToggleOversize(byte arg)
		{
			playingInfo.Oversize = arg != 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect Pxy: Fine volume slide and vibrato (x=Up, y=Down)
		/// </summary>
		/********************************************************************/
		private void DoFxFineVolSlideVib(VoiceInfo voiceInfo, byte arg)
		{
			DoFxVibOldAmpl(voiceInfo);
			DoFxFineVolUpDown(voiceInfo, arg);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DoFxFineVolUpDown(VoiceInfo voiceInfo, byte arg)
		{
			byte argHi = (byte)((arg & 0xf0) >> 4);
			if (argHi == 0)
				DoFxFineVolDown(voiceInfo, (byte)(arg & 0x0f));
			else
				DoFxFineVolUp(voiceInfo, argHi);
		}



		/********************************************************************/
		/// <summary>
		/// Effect Qxy: Volume and portamento down slide (synth drums)
		/// (x=Port down, y=Vol slide down)
		/// </summary>
		/********************************************************************/
		private void DoFxSynthDrums(VoiceInfo voiceInfo, byte arg)
		{
			DoFxPortamentoDown(voiceInfo, (byte)((arg >> 4) * 8));
			DoFxVolumeSlide(voiceInfo, (byte)(arg & 0x0f));
		}



		/********************************************************************/
		/// <summary>
		/// Effect Rxx: Set volume and tone portamento (xx=New volume)
		/// </summary>
		/********************************************************************/
		private void DoFxSetVolumePort(VoiceInfo voiceInfo, byte arg)
		{
			DoFxToneSlide(voiceInfo);
			DoFxSetVolume(voiceInfo, arg);
		}



		/********************************************************************/
		/// <summary>
		/// Effect Sxy: Fine volume slide and tone portamento
		/// (x=Fine vol up, y=Fine vol down)
		/// </summary>
		/********************************************************************/
		private void DoFxFineVolSlidePort(VoiceInfo voiceInfo, byte arg)
		{
			DoFxToneSlide(voiceInfo);
			DoFxFineVolUpDown(voiceInfo, arg);
		}



		/********************************************************************/
		/// <summary>
		/// Effect Txx: Set track volume (xx=New track volume)
		/// </summary>
		/********************************************************************/
		private void DoFxSetTrackVol(VoiceInfo voiceInfo, byte arg)
		{
			if (arg > 64)
				arg = 64;

			voiceInfo.TrackVolume = arg;
		}



		/********************************************************************/
		/// <summary>
		/// Effect Uxy: Set wave table mode
		/// (x!=0 = Freeze wave table, y!=0 = Never reset wave table)
		/// </summary>
		/********************************************************************/
		private void DoFxSetWaveCont(VoiceInfo voiceInfo, byte arg)
		{
			voiceInfo.SynthWaveCont = (byte)(arg & 0x0f);
			voiceInfo.SynthWaveStop = (byte)((arg & 0xf0) >> 4);
		}



		/********************************************************************/
		/// <summary>
		/// Effect Xxx: External event 1 (Synchro) (Event #1 set to xx)
		/// </summary>
		/********************************************************************/
		private void DoFxExternalEvent(byte arg)
		{
			if (playingInfo.FrameCnt == 0)
				playingInfo.Event[0] = arg;
		}



		/********************************************************************/
		/// <summary>
		/// Effect Exy: Extra commands
		/// </summary>
		/********************************************************************/
		private void DoFxECommands(VoiceInfo voiceInfo, byte arg)
		{
			byte argLow = (byte)(arg & 0x0f);

			switch ((ExtraEffect)(arg & 0xf0))
			{
				case ExtraEffect.SetFilter:
				{
					DoFxSetFilter(argLow);
					break;
				}

				case ExtraEffect.FineSlideUp:
				{
					DoFxFinePortamentoUp(voiceInfo, argLow);
					break;
				}

				case ExtraEffect.FineSlideDown:
				{
					DoFxFinePortamentoDown(voiceInfo, argLow);
					break;
				}

				case ExtraEffect.SetVibratoWaveform:
				{
					DoFxSetVibratoWave(voiceInfo, argLow);
					break;
				}

				case ExtraEffect.SetLoop:
				{
					DoFxSetLoopPoint();
					break;
				}

				case ExtraEffect.PatternLoop:
				{
					DoFxJump2Loop(argLow);
					break;
				}

				case ExtraEffect.RetrigNote:
				{
					DoFxRetrigNote(voiceInfo);
					break;
				}

				case ExtraEffect.FineVolumeSlideUp:
				{
					DoFxFineVolUp(voiceInfo, argLow);
					break;
				}

				case ExtraEffect.FineVolumeSlideDown:
				{
					DoFxFineVolDown(voiceInfo, argLow);
					break;
				}

				case ExtraEffect.NoteCut:
				{
					DoFxNoteCut(voiceInfo);
					break;
				}

				case ExtraEffect.NoteDelay:
				{
					DoFxRetrigNote(voiceInfo);
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Effect E0x: Set filter (x=0 = Filter on, x=1 Filter off)
		/// </summary>
		/********************************************************************/
		private void DoFxSetFilter(byte arg)
		{
			AmigaFilter = arg == 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect E1x: Fine slide up (x=value)
		/// </summary>
		/********************************************************************/
		private void DoFxFinePortamentoUp(VoiceInfo voiceInfo, byte arg)
		{
			if (playingInfo.FrameCnt == 0)
				voiceInfo.PerSlide -= arg;
		}



		/********************************************************************/
		/// <summary>
		/// Effect E2x: Fine slide down (x=value)
		/// </summary>
		/********************************************************************/
		private void DoFxFinePortamentoDown(VoiceInfo voiceInfo, byte arg)
		{
			if (playingInfo.FrameCnt == 0)
				voiceInfo.PerSlide += arg;
		}



		/********************************************************************/
		/// <summary>
		/// Effect E4x: Set vibrato wave form
		/// (x=0 = Sine, x=1 = Ramp down, x=2 = Square)
		/// </summary>
		/********************************************************************/
		private void DoFxSetVibratoWave(VoiceInfo voiceInfo, byte arg)
		{
			arg &= 0x03;
			arg = (byte)((byte)(arg >> 3) | (byte)(arg << (8 - 3)));

			voiceInfo.VibratoAmpl &= 0b10011111;
			voiceInfo.VibratoAmpl |= arg;
		}



		/********************************************************************/
		/// <summary>
		/// Effect E5x: Set loop (Set loop point)
		/// </summary>
		/********************************************************************/
		private void DoFxSetLoopPoint()
		{
			byte loopPoint = (byte)(playingInfo.PatCnt - 1);

			if (loopPoint != playingInfo.LoopPoint)
			{
				playingInfo.LoopPoint = loopPoint;
				playingInfo.LoopCnt = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Effect E6x: Jump to loop (Jump to loop, play x times)
		/// </summary>
		/********************************************************************/
		private void DoFxJump2Loop(byte arg)
		{
			if (arg == 0)
				DoFxSetLoopPoint();
		}



		/********************************************************************/
		/// <summary>
		/// Effect E9x: Retrig note (Retrig from note + x vblanks)
		/// Effect EDx: Note delay (Delay note x vblanks)
		/// </summary>
		/********************************************************************/
		private void DoFxRetrigNote(VoiceInfo voiceInfo)
		{
			if (voiceInfo.StepFxCnt == 0)
			{
				voiceInfo.ChFlag = 3;
				voiceInfo.FxCom = (Effect)0xef;		// ??
			}
			else
				voiceInfo.StepFxCnt--;
		}



		/********************************************************************/
		/// <summary>
		/// Effect EAx: Fine volume slide up (Add x to volume)
		/// </summary>
		/********************************************************************/
		private void DoFxFineVolUp(VoiceInfo voiceInfo, byte arg)
		{
			if (playingInfo.FrameCnt == 0)
			{
				voiceInfo.Volume += arg;
				if (voiceInfo.Volume > 64)
					voiceInfo.Volume = 64;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Effect EBx: Fine volume slide down (Subtract x from volume)
		/// </summary>
		/********************************************************************/
		private void DoFxFineVolDown(VoiceInfo voiceInfo, byte arg)
		{
			if (playingInfo.FrameCnt == 0)
			{
				sbyte newVol = (sbyte)(voiceInfo.Volume - arg);
				if (newVol < 0)
					newVol = 0;

				voiceInfo.Volume = (byte)newVol;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Effect ECx: Note cut (Cut from note + x vblanks)
		/// </summary>
		/********************************************************************/
		private void DoFxNoteCut(VoiceInfo voiceInfo)
		{
			if (voiceInfo.StepFxCnt == 0)
				voiceInfo.Volume = 0;
			else
				voiceInfo.StepFxCnt--;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DoSynth(VoiceInfo voiceInfo)
		{
			voiceInfo.VibDone = false;

			void ResetLoop()
			{
				switch (voiceInfo.SynthWaveRepCtrl)
				{
					// Normal
					case 0:
					{
						voiceInfo.SynthWaveAct = voiceInfo.SynthWaveRep;
						voiceInfo.SynthWaveActOffset = voiceInfo.SynthWaveRepOffset;
						voiceInfo.SynthWaveEndOffset = voiceInfo.SynthWaveRepEndOffset;
						break;
					}

					// Backwards
					case 1:
					{
						voiceInfo.SynthWaveAct = voiceInfo.SynthWaveRep;
						voiceInfo.SynthWaveActOffset = voiceInfo.SynthWaveEndOffset;

						int offset = voiceInfo.SynthWaveAddBytes;
						if (offset >= 0)
							offset = -offset;

						if (voiceInfo.SynthWaveStop != 0)
							return;

						voiceInfo.SynthWaveActOffset = (uint)(voiceInfo.SynthWaveActOffset + offset);
						voiceInfo.SynthWaveAddBytes = offset;
						break;
					}

					// Ping-pong
					default:
					{
						voiceInfo.SynthWaveAct = voiceInfo.SynthWaveRep;
						voiceInfo.SynthWaveEndOffset = voiceInfo.SynthWaveRepEndOffset;
						voiceInfo.SynthWaveActOffset = (uint)(voiceInfo.SynthWaveActOffset - voiceInfo.SynthWaveAddBytes);
						voiceInfo.SynthWaveAddBytes = -voiceInfo.SynthWaveAddBytes;
						break;
					}
				}
			}

			if (voiceInfo.ChFlag == 0)
			{
				if ((voiceInfo.ChMode != 0) && (voiceInfo.WaveForm != null))
				{
					if (voiceInfo.SynthWaveStop == 0)
					{
						voiceInfo.SynthWaveCnt++;

						if (voiceInfo.SynthWaveCnt >= voiceInfo.SynthWaveSpd)
						{
							voiceInfo.SynthWaveCnt = 0;

							int addBytes = voiceInfo.SynthWaveAddBytes;
							voiceInfo.SynthWaveActOffset = (uint)(voiceInfo.SynthWaveActOffset + addBytes);

							if (addBytes < 0)
							{
								if ((int)voiceInfo.SynthWaveActOffset < voiceInfo.SynthWaveRepOffset)
									ResetLoop();
							}
							else
							{
								if ((int)voiceInfo.SynthWaveActOffset >= voiceInfo.SynthWaveEndOffset)
									ResetLoop();
							}

							voiceInfo.ChFlag = 1;	// New repeat offset
							voiceInfo.RepeatStart = voiceInfo.SynthWaveAct;
							voiceInfo.RepeatOffset = voiceInfo.SynthWaveActOffset;
						}
					}
				}

				if (voiceInfo.WaveForm != null)
				{
					int vol = voiceInfo.SynthVol;

					switch (voiceInfo.SynthEnv)
					{
						case EnvelopeState.Add:
						{
							vol += voiceInfo.SynthAdd;
							if (vol > 127)
							{
								vol = 127;
								voiceInfo.SynthEnv = EnvelopeState.Sub;
							}
							break;
						}

						case EnvelopeState.Sub:
						{
							vol -= voiceInfo.SynthSub;
							if (vol <= voiceInfo.SynthEnd)
							{
								vol = voiceInfo.SynthEnd;
								voiceInfo.SynthEnv = EnvelopeState.Done;
							}
							break;
						}
					}

					voiceInfo.SynthVol = (byte)vol;

					// Vibrato
					if (voiceInfo.VibOn != 0x21)
					{
						if (voiceInfo.VibratoTrigDelay == -1)
							voiceInfo.VibOn = 1;
						else
							voiceInfo.VibratoTrigDelay--;
					}
				}
			}

			if (voiceInfo.VibOn == 1)
				DoFxVibOldAmpl(voiceInfo);
		}



		/********************************************************************/
		/// <summary>
		/// Tell NostalgicPlayer what to play
		/// </summary>
		/********************************************************************/
		private void SetupChannel(VoiceInfo voiceInfo, IChannel channel)
		{
			if ((voiceInfo.FxCom != Effect.ExtraEffects) || ((ExtraEffect)(voiceInfo.FxDat & 0xf0) != ExtraEffect.NoteDelay))
			{
				if (((voiceInfo.ChFlag & 0x02) != 0) && (voiceInfo.WaveForm != null))
				{
					if (playingInfo.NoiseAvoid)
					{
						if ((voiceInfo.OldWaveLen > 255) || (voiceInfo.OldWaveLen == 0) || (voiceInfo.WaveLen > 255))
							channel.PlaySample(voiceInfo.InstrumentNumber, voiceInfo.WaveForm, voiceInfo.WaveFormOffset, voiceInfo.WaveLen * 2U);
						else
						{
							channel.SetSample(voiceInfo.WaveForm, voiceInfo.WaveFormOffset, voiceInfo.WaveLen * 2U);
							channel.SetSampleNumber(voiceInfo.InstrumentNumber);
						}
					}
					else
						channel.PlaySample(voiceInfo.InstrumentNumber, voiceInfo.WaveForm, voiceInfo.WaveFormOffset, voiceInfo.WaveLen * 2U);

					if ((voiceInfo.RepeatStart != null) && (voiceInfo.RepeatLength > 1))
					{
						uint length = voiceInfo.RepeatLength * 2U;
						if ((voiceInfo.RepeatOffset + length) > voiceInfo.RepeatStart.Length)
							length = (uint)(voiceInfo.RepeatStart.Length - voiceInfo.RepeatOffset);

						if ((length > 0) && (voiceInfo.RepeatOffset < voiceInfo.RepeatStart.Length))
							channel.SetLoop(voiceInfo.RepeatStart, voiceInfo.RepeatOffset, length);
					}
				}
				else
				{
					if ((voiceInfo.RepeatStart != null) && (voiceInfo.RepeatLength > 1))
					{
						uint length = voiceInfo.RepeatLength * 2U;
						if ((voiceInfo.RepeatOffset + length) > voiceInfo.RepeatStart.Length)
							length = (uint)(voiceInfo.RepeatStart.Length - voiceInfo.RepeatOffset);

						if ((length > 0) && (voiceInfo.RepeatOffset < voiceInfo.RepeatStart.Length))
						{
							channel.SetSample(voiceInfo.RepeatStart, voiceInfo.RepeatOffset, length);
							channel.SetLoop(voiceInfo.RepeatStart, voiceInfo.RepeatOffset, length);
						}
					}
				}

				uint period = (uint)(voiceInfo.Period + voiceInfo.PerSlide);
				if (period < 103)
					period = 103;

				channel.SetAmigaPeriod(period);

				ushort volume = (ushort)((((voiceInfo.Volume * voiceInfo.SynthVol) / 128) * voiceInfo.TrackVolume) / 64);
				channel.SetAmigaVolume(volume);

				voiceInfo.ChFlag = 0;
			}
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
		/// Will update the module information with current pattern
		/// </summary>
		/********************************************************************/
		private void ShowPattern()
		{
			OnModuleInfoChanged(InfoPatternLine, FormatPattern());
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
			ShowPosition();
			ShowPattern();
			ShowSpeed();
			ShowTempo();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the playing position
		/// </summary>
		/********************************************************************/
		private string FormatPosition()
		{
			return playingInfo.Position.ToString();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the playing pattern
		/// </summary>
		/********************************************************************/
		private string FormatPattern()
		{
			return playingInfo.CurrentPattern.ToString();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the current speed
		/// </summary>
		/********************************************************************/
		private string FormatSpeed()
		{
			return playingInfo.Speed.ToString();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the current tempo
		/// </summary>
		/********************************************************************/
		private string FormatTempo()
		{
			return playingInfo.Tempo.ToString();
		}
		#endregion
	}
}
