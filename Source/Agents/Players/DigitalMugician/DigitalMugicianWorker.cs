/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.DigitalMugician.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Types;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.DigitalMugician
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class DigitalMugicianWorker : ModulePlayerWithSubSongDurationAgentBase
	{
		private static readonly Dictionary<Guid, ModuleType> moduleTypeLookup = new Dictionary<Guid, ModuleType>
		{
			{ DigitalMugician.Agent1Id, ModuleType.DigitalMugician },
			{ DigitalMugician.Agent2Id, ModuleType.DigitalMugician2 }
		};

		private readonly ModuleType currentModuleType;

		private int numberOfChannels;

		private ushort numberOfTracks;
		private uint numberOfInstruments;
		private uint numberOfWaveforms;
		private uint numberOfSamples;

		private List<int> subSongMapping;
		private uint[] subSongSequenceLength;
		private SubSong[] subSongs;
		private Sequence[][,] subSongSequences;
		private Track[] tracks;
		private byte[][] arpeggios;

		private Instrument[] instruments;
		private sbyte[][] waveforms;
		private Sample[] samples;

		private GlobalPlayingInfo playingInfo;

		private int subSongNumber;
		private SubSong currentSubSong;
		private Sequence[,] currentSequence;
		private Sequence[,] currentSequence2;

		private byte[] chTab;
		private int chTabIndex;

		private bool endReached;

		private const int InfoPositionLine = 4;
		private const int InfoTrackLine = 5;
		private const int InfoSpeedLine = 6;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public DigitalMugicianWorker(Guid typeId)
		{
			currentModuleType = moduleTypeLookup.GetValueOrDefault(typeId, ModuleType.Unknown);
		}

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => new [] { "dmu", "mug" };



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
		public override string ModuleName => subSongs[subSongMapping[0]].Name;



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
					description = Resources.IDS_DMU_INFODESCLINE0;
					value = playingInfo.SongLength.ToString();
					break;
				}

				// Used tracks
				case 1:
				{
					description = Resources.IDS_DMU_INFODESCLINE1;
					value = numberOfTracks.ToString();
					break;
				}

				// Used samples
				case 2:
				{
					description = Resources.IDS_DMU_INFODESCLINE2;
					value = numberOfSamples.ToString();
					break;
				}

				// Used wave tables
				case 3:
				{
					description = Resources.IDS_DMU_INFODESCLINE3;
					value = numberOfWaveforms.ToString();
					break;
				}

				// Playing position
				case 4:
				{
					description = Resources.IDS_DMU_INFODESCLINE4;
					value = playingInfo.CurrentPosition.ToString();
					break;
				}

				// Playing tracks
				case 5:
				{
					description = Resources.IDS_DMU_INFODESCLINE5;
					value = FormatTracks();
					break;
				}

				// Current speed
				case 6:
				{
					description = Resources.IDS_DMU_INFODESCLINE6;
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
				ModuleStream moduleStream = fileInfo.ModuleStream;
				Encoding encoder = EncoderCollection.Amiga;

				// Skip identifier
				moduleStream.Seek(24, SeekOrigin.Begin);

				// Read header
				bool arpeggiosEnabled = moduleStream.Read_B_UINT16() != 0;
				numberOfTracks = moduleStream.Read_B_UINT16();

				subSongSequenceLength = new uint[8];
				moduleStream.ReadArray_B_UINT32s(subSongSequenceLength, 0, 8);

				numberOfInstruments = moduleStream.Read_B_UINT32();
				numberOfWaveforms = moduleStream.Read_B_UINT32();
				numberOfSamples = moduleStream.Read_B_UINT32();
				uint samplesSize = moduleStream.Read_B_UINT32();

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_DMU_ERR_LOADING_HEADER;
					Cleanup();

					return AgentResult.Error;
				}

				// Read sub-song information
				byte[] buf = new byte[12];
				subSongs = new SubSong[8];

				for (int i = 0; i < 8; i++)
				{
					SubSong subSong = new SubSong();

					subSong.LoopSong = moduleStream.Read_UINT8() != 0;
					subSong.LoopPosition = moduleStream.Read_UINT8();
					subSong.SongSpeed = moduleStream.Read_UINT8();
					subSong.NumberOfSequences = moduleStream.Read_UINT8();

					moduleStream.Read(buf, 0, 12);
					subSong.Name = encoder.GetString(buf, 0, 12).Trim();

					subSongs[i] = subSong;
				}

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_DMU_ERR_LOADING_HEADER;
					Cleanup();

					return AgentResult.Error;
				}

				// Read sequences
				subSongSequences = new Sequence[8][,];

				for (int i = 0; i < 8; i++)
				{
					uint sequenceLength = subSongSequenceLength[i];
					Sequence[,] sequences = new Sequence[4, sequenceLength];

					for (int j = 0; j < sequenceLength; j++)
					{
						for (int k = 0; k < 4; k++)
						{
							Sequence sequence = new Sequence();

							sequence.TrackNumber = moduleStream.Read_UINT8();
							sequence.Transpose = moduleStream.Read_INT8();

							sequences[k, j] = sequence;
						}
					}

					subSongSequences[i] = sequences;
				}

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_DMU_ERR_LOADING_HEADER;
					Cleanup();

					return AgentResult.Error;
				}

				// Read instruments
				instruments = new Instrument[numberOfInstruments];

				for (int i = 0; i < numberOfInstruments; i++)
				{
					Instrument inst = new Instrument();

					inst.WaveformNumber = moduleStream.Read_UINT8();
					inst.LoopLength = (ushort)(moduleStream.Read_UINT8() * 2);
					inst.Volume = moduleStream.Read_UINT8();
					inst.VolumeSpeed = moduleStream.Read_UINT8();
					inst.ArpeggioNumber = moduleStream.Read_UINT8();
					inst.Pitch = moduleStream.Read_UINT8();
					inst.EffectIndex = moduleStream.Read_UINT8();
					inst.Delay = moduleStream.Read_UINT8();
					inst.Finetune = moduleStream.Read_UINT8();
					inst.PitchLoop = moduleStream.Read_UINT8();
					inst.PitchSpeed = moduleStream.Read_UINT8();
					inst.Effect = (InstrumentEffect)moduleStream.Read_UINT8();
					inst.SourceWave1 = moduleStream.Read_UINT8();
					inst.SourceWave2 = moduleStream.Read_UINT8();
					inst.EffectSpeed = moduleStream.Read_UINT8();
					inst.VolumeLoop = moduleStream.Read_UINT8() != 0;

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_DMU_ERR_LOADING_INSTRUMENTS;
						Cleanup();

						return AgentResult.Error;
					}

					instruments[i] = inst;
				}

				// Read waveforms
				waveforms = new sbyte[numberOfWaveforms][];

				for (int i = 0; i < numberOfWaveforms; i++)
				{
					waveforms[i] = moduleStream.ReadSampleData(i, 128, out int readBytes);
					if (readBytes != 128)
					{
						errorMessage = Resources.IDS_DMU_ERR_LOADING_WAVEFORMS;
						Cleanup();

						return AgentResult.Error;
					}
				}

				// Read sample information
				samples = new Sample[numberOfSamples];

				for (int i = 0; i < numberOfSamples;i++)
				{
					Sample sample = new Sample();

					sample.StartOffset = moduleStream.Read_B_UINT32();
					sample.EndOffset = moduleStream.Read_B_UINT32();
					sample.LoopStart = moduleStream.Read_B_INT32();

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_DMU_ERR_LOADING_SAMPLEINFO;
						Cleanup();

						return AgentResult.Error;
					}

					moduleStream.Seek(20, SeekOrigin.Current);

					samples[i] = sample;
				}

				// Read tracks
				tracks = new Track[numberOfTracks];

				for (int i = 0; i < numberOfTracks;i++)
				{
					Track track = new Track();

					for (int j = 0; j < 64; j++)
					{
						TrackRow row = new TrackRow();

						row.Note = moduleStream.Read_UINT8();
						row.Instrument = moduleStream.Read_UINT8();
						row.Effect = moduleStream.Read_UINT8();
						row.EffectParam = moduleStream.Read_UINT8();

						track.Rows[j] = row;
					}

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_DMU_ERR_LOADING_TRACKS;
						Cleanup();

						return AgentResult.Error;
					}

					tracks[i] = track;
				}

				// Read sample data and fix-up the information
				long sampleStartOffset = moduleStream.Position;

				for (int i = 0; i < numberOfSamples; i++)
				{
					Sample sample = samples[i];

					int length = (int)(sample.EndOffset - sample.StartOffset);

					using (ModuleStream sampleDataStream = moduleStream.GetSampleDataStream(32 + i, length))
					{
						sampleDataStream.Seek(sampleStartOffset + sample.StartOffset, SeekOrigin.Begin);

						sample.SampleData = new sbyte[length];
						sampleDataStream.ReadSigned(sample.SampleData, 0, length);

						if (sampleDataStream.EndOfStream)
						{
							errorMessage = Resources.IDS_DMU_ERR_LOADING_SAMPLES;
							Cleanup();

							return AgentResult.Error;
						}
					}

					if (sample.LoopStart != 0)
						sample.LoopStart = (int)(sample.LoopStart - sample.StartOffset);
					else
						sample.LoopStart = -1;

					sample.StartOffset = 0;
					sample.EndOffset = (uint)length;
				}

				// Read arpeggios
				arpeggios = ArrayHelper.Initialize2Arrays<byte>(8, 32);

				if (arpeggiosEnabled)
				{
					moduleStream.Seek(sampleStartOffset + samplesSize, SeekOrigin.Begin);

					for (int i = 0; i < 8; i++)
					{
						moduleStream.Read(arpeggios[i], 0, 32);

						if (moduleStream.EndOfStream)
						{
							errorMessage = Resources.IDS_DMU_ERR_LOADING_ARPEGGIOS;
							Cleanup();

							return AgentResult.Error;
						}
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
			errorMessage = string.Empty;

			numberOfChannels = currentModuleType == ModuleType.DigitalMugician ? 4 : 7;
			int increment = currentModuleType == ModuleType.DigitalMugician ? 1 : 2;

			// Find sub-songs that can be played
			subSongMapping = new List<int>();

			for (int i = 0; i < 8; i += increment)
			{
				if (subSongSequenceLength[i] == subSongs[i].NumberOfSequences)
				{
					Sequence[,] sequences = subSongSequences[i];

					for (int j = 0; j < subSongSequenceLength[i]; j++)
					{
						for (int k = 0; k < 4; k++)
						{
							Sequence sequence = sequences[k, j];

							if ((sequence.TrackNumber != 0) || (sequence.Transpose != 0))
							{
								subSongMapping.Add(i);
								goto NextSong;
							}
						}
					}

					NextSong:
					;
				}
			}

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

			// Set panning if needed
			if (currentModuleType == ModuleType.DigitalMugician2)
			{
				// For 7-voices modules, the 7-voices are split into 2 sub-songs.
				// The first sub-song only use the first 3 voices, which will be played
				// normally. The second sub-song will use all 4 voices, which will be
				// mixed into one
				VirtualChannels[0].SetPanning((ushort)ChannelPanningType.Left);
				VirtualChannels[1].SetPanning((ushort)ChannelPanningType.Right);
				VirtualChannels[2].SetPanning((ushort)ChannelPanningType.Right);
				VirtualChannels[3].SetPanning((ushort)ChannelPanningType.Left);
				VirtualChannels[4].SetPanning((ushort)ChannelPanningType.Left);
				VirtualChannels[5].SetPanning((ushort)ChannelPanningType.Left);
				VirtualChannels[6].SetPanning((ushort)ChannelPanningType.Left);
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public override void Play()
		{
			// Call the player
			PlayIt();

			if (endReached)
			{
				OnEndReachedOnAllChannels(playingInfo.CurrentPosition);
				endReached = false;
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
		/// Return information about sub-songs
		/// </summary>
		/********************************************************************/
		public override SubSongInfo SubSongs => new SubSongInfo(subSongMapping.Count, 0);



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

				for (int j = 0; j < 5 * 12 - 2; j++)
					frequencies[2 * 12 + j] = 3546895U / Tables.Periods[6 + j];

				for (int i = 0; i < numberOfInstruments; i++)
				{
					SampleInfo sampleInfo;

					Instrument instr = instruments[i];

					if (instr.WaveformNumber < 32)
					{
						sampleInfo = new SampleInfo
						{
							Name = string.Empty,
							Type = SampleInfo.SampleType.Synthesis,
							Flags = SampleInfo.SampleFlag.None,
							Volume = (ushort)(instr.Volume * 4),
							Panning = -1,
							Sample = null,
							Length = 0,
							LoopStart = 0,
							LoopLength = 0,
							NoteFrequencies = frequencies
						};
					}
					else
					{
						Sample sample = samples[instr.WaveformNumber - 32];

						sampleInfo = new SampleInfo
						{
							Name = string.Empty,
							Type = SampleInfo.SampleType.Sample,
							BitSize = SampleInfo.SampleSize._8Bit,
							Volume = 256,
							Panning = -1,
							Sample = sample.SampleData,
							Length = sample.EndOffset,
							NoteFrequencies = frequencies
						};

						if (sample.LoopStart < 0)
						{
							// No loop
							sampleInfo.Flags = SampleInfo.SampleFlag.None;
							sampleInfo.LoopStart = 0;
							sampleInfo.LoopLength = 0;
						}
						else
						{
							// Sample loops
							sampleInfo.Flags = SampleInfo.SampleFlag.Loop;
							sampleInfo.LoopStart = (uint)sample.LoopStart;
							sampleInfo.LoopLength = sample.EndOffset - sampleInfo.LoopStart;
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
			MarkPositionAsVisited(0);
		}



		/********************************************************************/
		/// <summary>
		/// Return the total number of positions. You only need to derive
		/// from this method, if your player have one position list for all
		/// channels and can restart on another position than 0
		/// </summary>
		/********************************************************************/
		protected override int GetTotalNumberOfPositions()
		{
			return currentSubSong.NumberOfSequences;
		}



		/********************************************************************/
		/// <summary>
		/// Create a snapshot of all the internal structures and return it
		/// </summary>
		/********************************************************************/
		protected override ISnapshot CreateSnapshot()
		{
			return new Snapshot(playingInfo);
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
			Snapshot clonedSnapshot = new Snapshot(currentSnapshot.PlayingInfo);

			playingInfo = clonedSnapshot.PlayingInfo;

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
			if (moduleStream.Length < 204)
				return ModuleType.Unknown;

			// Read the module identifier
			byte[] buf = new byte[24];

			moduleStream.Seek(0, SeekOrigin.Begin);
			moduleStream.Read(buf, 0, 24);

			// Check the identifier
			string mark = Encoding.ASCII.GetString(buf, 0, 24);

			if (mark == " MUGICIAN/SOFTEYES 1990 ")
				return ModuleType.DigitalMugician;

			if (mark == " MUGICIAN2/SOFTEYES 1990")
				return ModuleType.DigitalMugician2;

			return ModuleType.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player have allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			chTab = null;

			currentSubSong = null;
			currentSequence = null;
			currentSequence2 = null;

			samples = null;
			waveforms = null;
			instruments = null;

			arpeggios = null;
			tracks = null;
			subSongSequences = null;
			subSongSequenceLength = null;
			subSongs = null;

			subSongMapping = null;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize sound structures
		/// </summary>
		/********************************************************************/
		private void InitializeSound(int songNumber)
		{
			subSongNumber = songNumber;

			int realSubSong = subSongMapping[subSongNumber];
			currentSubSong = subSongs[realSubSong];

			playingInfo = new GlobalPlayingInfo
			{
				Speed = currentSubSong.SongSpeed,
				CurrentSpeed = (ushort)(((currentSubSong.SongSpeed & 0x0f) << 4) | (currentSubSong.SongSpeed & 0x0f)),
				NewPattern = true,
				NewRow = true,
				CurrentPosition = 0,
				SongLength = currentSubSong.NumberOfSequences,
				CurrentRow = 0,
				PatternLength = 64,
				VoiceInfo = ArrayHelper.InitializeArray<VoiceInfo>(numberOfChannels)
			};

			playingInfo.LastShownSpeed = playingInfo.CurrentSpeed;

			currentSequence = subSongSequences[realSubSong];

			if (currentModuleType == ModuleType.DigitalMugician2)
				currentSequence2 = subSongSequences[realSubSong + 1];

			chTab = new byte[4];

			endReached = false;
		}



		/********************************************************************/
		/// <summary>
		/// Plays the module
		/// </summary>
		/********************************************************************/
		private void PlayIt()
		{
			chTab[0] = 0x80;
			chTab[1] = 0x80;
			chTab[2] = 0x80;
			chTab[3] = 0x80;
			chTabIndex = 0;

			for (int i = 0; i < numberOfChannels; i++)
				PlayNote(i);

			for (int i = 0; i < numberOfChannels; i++)
				DoEffects(i);

			playingInfo.NewPattern = false;
			playingInfo.NewRow = false;

			playingInfo.Speed--;
			if (playingInfo.Speed == 0)
			{
				playingInfo.Speed = (ushort)((playingInfo.CurrentSpeed) & 0x0f);
				ChangeSpeed((ushort)(((playingInfo.CurrentSpeed & 0x0f) << 4) | ((playingInfo.CurrentSpeed & 0xf0) >> 4)));

				playingInfo.NewRow = true;

				playingInfo.CurrentRow++;
				if ((playingInfo.CurrentRow == 64) || (playingInfo.CurrentRow == playingInfo.PatternLength))
				{
					playingInfo.CurrentRow = 0;
					playingInfo.NewPattern = true;

					playingInfo.CurrentPosition++;
					if (playingInfo.CurrentPosition == playingInfo.SongLength)
					{
						if (currentSubSong.LoopSong)
							playingInfo.CurrentPosition = currentSubSong.LoopPosition;
						else
							InitializeSound(subSongNumber);

						endReached = true;
					}
					else
						MarkPositionAsVisited(playingInfo.CurrentPosition);

					ShowSongPositions();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PlayNote(int channelNumber)
		{
			VoiceInfo voiceInfo = playingInfo.VoiceInfo[channelNumber];
			IChannel channel = VirtualChannels[channelNumber];

			if (playingInfo.NewPattern)
			{
				Sequence sequence;

				if ((currentModuleType == ModuleType.DigitalMugician) || (channelNumber < 3))
					sequence = currentSequence[channelNumber, playingInfo.CurrentPosition];
				else
					sequence = currentSequence2[channelNumber - 3, playingInfo.CurrentPosition];

				voiceInfo.Track = sequence.TrackNumber;
				voiceInfo.Transpose = sequence.Transpose;

				ShowTracks();
			}

			if (playingInfo.NewRow)
			{
				Track track = tracks[voiceInfo.Track];
				TrackRow row = track.Rows[playingInfo.CurrentRow];

				if (row.Note != 0)
				{
					if (row.Effect != ((int)Effect.NoWander + 62))
					{
						voiceInfo.LastNote = row.Note;

						if (row.Instrument != 0)
							voiceInfo.LastInstrument = (ushort)(row.Instrument - 1);
					}

					voiceInfo.LastInstrument &= 0x3f;
					voiceInfo.LastEffect = row.Effect < 64 ? Effect.PitchBend : (Effect)(row.Effect - 62);
					voiceInfo.LastEffectParam = row.EffectParam;

					if (voiceInfo.LastInstrument < instruments.Length)
					{
						Instrument instr = instruments[voiceInfo.LastInstrument];
						voiceInfo.FineTune = instr.Finetune;

						if (voiceInfo.LastEffect == Effect.NoWander)
						{
							voiceInfo.PitchBendEndNote = row.Note;
							voiceInfo.PitchBendEndPeriod = GetPeriod(voiceInfo.PitchBendEndNote, voiceInfo.Transpose, voiceInfo.FineTune);
						}
						else
						{
							voiceInfo.PitchBendEndNote = row.Effect;

							if (voiceInfo.LastEffect == Effect.PitchBend)
								voiceInfo.PitchBendEndPeriod = GetPeriod(voiceInfo.PitchBendEndNote, voiceInfo.Transpose, voiceInfo.FineTune);
						}

						if (voiceInfo.LastEffect == Effect.Arpeggio)
							instr.ArpeggioNumber = (byte)(voiceInfo.LastEffectParam & 7);

						byte waveform = instr.WaveformNumber;

						if (voiceInfo.LastEffect != Effect.NoWander)
						{
							if (waveform >= 32)
							{
								Sample sample = samples[waveform - 32];

								channel.PlaySample((short)voiceInfo.LastInstrument, sample.SampleData, sample.StartOffset, sample.EndOffset - sample.StartOffset);

								if (sample.LoopStart >= 0)
									channel.SetLoop((uint)sample.LoopStart, sample.EndOffset - (uint)sample.LoopStart);
							}
							else
							{
								sbyte[] waveformData = waveforms[waveform];

								channel.SetLoop(waveformData, 0, instr.LoopLength);

								if (voiceInfo.LastEffect != Effect.NoDma)
									channel.PlaySample((short)voiceInfo.LastInstrument, waveformData, 0, instr.LoopLength);

								if (currentModuleType == ModuleType.DigitalMugician)
								{
									if ((instr.Effect != InstrumentEffect.None) && (voiceInfo.LastEffect != Effect.NoInstrumentEffect) && (voiceInfo.LastEffect != Effect.NoInstrumentEffectAndVolume))
									{
										sbyte[] source = waveforms[instr.SourceWave1];
										Array.Copy(source, waveformData, 128);

										instr.EffectIndex = 0;
										voiceInfo.InstrumentEffectSpeed = instr.EffectSpeed;
									}
								}
							}
						}

						if ((voiceInfo.LastEffect != Effect.NoInstrumentVolume) && (voiceInfo.LastEffect != Effect.NoInstrumentEffectAndVolume) && (voiceInfo.LastEffect != Effect.NoWander))
						{
							voiceInfo.VolumeSpeedCounter = 1;
							voiceInfo.VolumeIndex = 0;
						}

						voiceInfo.CurrentPitchBendPeriod = 0;
						voiceInfo.InstrumentDelay = instr.Delay;
						voiceInfo.PitchIndex = 0;
						voiceInfo.ArpeggioIndex = 0;
					}
				}
			}

			switch (voiceInfo.LastEffect)
			{
				case Effect.PatternLength:
				{
					if ((voiceInfo.LastEffectParam != 0) && (voiceInfo.LastEffectParam <= 64))
						playingInfo.PatternLength = voiceInfo.LastEffectParam;

					break;
				}

				case Effect.SongSpeed:
				{
					byte parm = (byte)(voiceInfo.LastEffectParam & 0x0f);
					byte speed = (byte)(((parm << 4) | parm));

					if ((parm != 0) && (parm <= 15))
						ChangeSpeed(speed);

					break;
				}

				case Effect.FilterOn:
				{
					AmigaFilter = true;
					break;
				}

				case Effect.FilterOff:
				{
					AmigaFilter = false;
					break;
				}

				case Effect.Shuffle:
				{
					voiceInfo.LastEffect = Effect.None;

					if (((voiceInfo.LastEffectParam & 0x0f) != 0) && ((voiceInfo.LastEffectParam & 0xf0) != 0))
						ChangeSpeed(voiceInfo.LastEffectParam);

					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DoEffects(int channelNumber)
		{
			VoiceInfo voiceInfo = playingInfo.VoiceInfo[channelNumber];
			IChannel channel = VirtualChannels[channelNumber];

			if (voiceInfo.LastEffect == Effect.SwitchFilter)
				AmigaFilter = !AmigaFilter;

			if (voiceInfo.LastInstrument < instruments.Length)
			{
				Instrument instr = instruments[voiceInfo.LastInstrument];

				if ((instr.Effect != InstrumentEffect.None) && (instr.WaveformNumber < 32))
				{
					byte instrNum = (byte)(voiceInfo.LastInstrument + 1);

					if ((chTab[0] != instrNum) && (chTab[1] != instrNum) && (chTab[2] != instrNum) && (chTab[3] != instrNum))
					{
						chTab[chTabIndex++] = instrNum;

						if (voiceInfo.InstrumentEffectSpeed == 0)
						{
							voiceInfo.InstrumentEffectSpeed = instr.EffectSpeed;

							sbyte[] waveformData = waveforms[instr.WaveformNumber];

							switch (instr.Effect)
							{
								case InstrumentEffect.Filter:
								{
									DoInstEffect_Filter(waveformData);
									break;
								}

								case InstrumentEffect.Mixing:
								{
									DoInstEffect_Mixing(waveformData, instr);
									break;
								}

								case InstrumentEffect.ScrLeft:
								{
									DoInstEffect_ScrLeft(waveformData);
									break;
								}

								case InstrumentEffect.ScrRight:
								{
									DoInstEffect_ScrRight(waveformData);
									break;
								}

								case InstrumentEffect.Upsample:
								{
									DoInstEffect_UpSampling(waveformData);
									break;
								}

								case InstrumentEffect.Downsample:
								{
									DoInstEffect_DownSampling(waveformData);
									break;
								}

								case InstrumentEffect.Negate:
								{
									DoInstEffect_Negate(waveformData, instr);
									break;
								}

								case InstrumentEffect.MadMix1:
								{
									DoInstEffect_MadMix1(waveformData, instr);
									break;
								}

								case InstrumentEffect.Addition:
								{
									DoInstEffect_Addition(waveformData, instr);
									break;
								}

								case InstrumentEffect.Filter2:
								{
									DoInstEffect_Filter2(waveformData);
									break;
								}

								case InstrumentEffect.Morphing:
								{
									DoInstEffect_Morphing(waveformData, instr);
									break;
								}

								case InstrumentEffect.MorphF:
								{
									DoInstEffect_MorphF(waveformData, instr);
									break;
								}

								case InstrumentEffect.Filter3:
								{
									DoInstEffect_Filter3(waveformData);
									break;
								}

								case InstrumentEffect.Polygate:
								{
									DoInstEffect_Polygate(waveformData, instr);
									break;
								}

								case InstrumentEffect.Colgate:
								{
									DoInstEffect_Colgate(waveformData, instr);
									break;
								}
							}
						}
						else
							voiceInfo.InstrumentEffectSpeed--;
					}
				}

				if (voiceInfo.VolumeSpeedCounter != 0)
				{
					voiceInfo.VolumeSpeedCounter--;

					if (voiceInfo.VolumeSpeedCounter == 0)
					{
						voiceInfo.VolumeSpeedCounter = instr.VolumeSpeed;
						voiceInfo.VolumeIndex++;
						voiceInfo.VolumeIndex &= 0x7f;

						if ((voiceInfo.VolumeIndex == 0) && !instr.VolumeLoop)
							voiceInfo.VolumeSpeedCounter = 0;
						else
						{
							int volume = -(sbyte)(waveforms[instr.Volume][voiceInfo.VolumeIndex] + 0x81);
							volume = (volume & 0xff) / 4;

							channel.SetAmigaVolume((ushort)volume);
						}
					}
				}

				int note = voiceInfo.LastNote;

				if (instr.ArpeggioNumber != 0)
				{
					byte[] arpData = arpeggios[instr.ArpeggioNumber];
					note += arpData[voiceInfo.ArpeggioIndex];

					voiceInfo.ArpeggioIndex++;
					voiceInfo.ArpeggioIndex &= 0x1f;
				}

				ushort period = GetPeriod((ushort)note, voiceInfo.Transpose, voiceInfo.FineTune);
				voiceInfo.NotePeriod = period;

				if ((voiceInfo.LastEffect == Effect.NoWander) || (voiceInfo.LastEffect == Effect.PitchBend))
				{
					int periodIncrement = -(sbyte)voiceInfo.LastEffectParam;

					voiceInfo.CurrentPitchBendPeriod = (short)(voiceInfo.CurrentPitchBendPeriod + periodIncrement);
					voiceInfo.NotePeriod = (ushort)(voiceInfo.NotePeriod + voiceInfo.CurrentPitchBendPeriod);

					if (voiceInfo.LastEffectParam != 0)
					{
						if (periodIncrement < 0)
						{
							if (voiceInfo.NotePeriod <= voiceInfo.PitchBendEndPeriod)
							{
								voiceInfo.CurrentPitchBendPeriod = (short)(voiceInfo.PitchBendEndPeriod - period);
								voiceInfo.LastEffectParam = 0;
							}
						}
						else
						{
							if (voiceInfo.NotePeriod >= voiceInfo.PitchBendEndPeriod)
							{
								voiceInfo.CurrentPitchBendPeriod = (short)(voiceInfo.PitchBendEndPeriod - period);
								voiceInfo.LastEffectParam = 0;
							}
						}
					}
				}

				if (instr.Pitch != 0)
				{
					if (voiceInfo.InstrumentDelay == 0)
					{
						sbyte pitchWaveformData = waveforms[instr.Pitch][voiceInfo.PitchIndex];

						voiceInfo.PitchIndex++;
						voiceInfo.PitchIndex &= 0x7f;

						if (voiceInfo.PitchIndex == 0)
							voiceInfo.PitchIndex = instr.PitchLoop;

						voiceInfo.NotePeriod = (ushort)(voiceInfo.NotePeriod - pitchWaveformData);
					}
					else
						voiceInfo.InstrumentDelay--;
				}

				channel.SetAmigaPeriod(voiceInfo.NotePeriod);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DoInstEffect_Filter(sbyte[] waveformData)
		{
			for (int i = 0; i < 127; i++)
				waveformData[i] = (sbyte)((waveformData[i] + waveformData[i + 1]) / 2);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DoInstEffect_Mixing(sbyte[] waveformData, Instrument instr)
		{
			sbyte[] source1Data = waveforms[instr.SourceWave1];
			sbyte[] source2Data = waveforms[instr.SourceWave2];

			int index = instr.EffectIndex;

			instr.EffectIndex++;
			instr.EffectIndex &= 0x7f;

			for (int i = 0; i < instr.LoopLength; i++)
			{
				waveformData[i] = (sbyte)((source1Data[i] + source2Data[index]) / 2);

				index++;
				index &= 0x7f;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DoInstEffect_ScrLeft(sbyte[] waveformData)
		{
			sbyte first = waveformData[0];

			for (int i = 0; i < 127; i++)
				waveformData[i] = waveformData[i + 1];

			waveformData[127] = first;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DoInstEffect_ScrRight(sbyte[] waveformData)
		{
			sbyte last = waveformData[127];

			for (int i = 126; i >= 0; i--)
				waveformData[i + 1] = waveformData[i];

			waveformData[0] = last;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DoInstEffect_UpSampling(sbyte[] waveformData)
		{
			int sourceIndex = 0;
			int destIndex = 0;

			for (int i = 0; i < 64; i++)
			{
				waveformData[destIndex++] = waveformData[sourceIndex];
				sourceIndex += 2;
			}

			sourceIndex = 0;

			for (int i = 0; i < 64; i++)
				waveformData[destIndex++] = waveformData[sourceIndex++];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DoInstEffect_DownSampling(sbyte[] waveformData)
		{
			int sourceIndex = 64;
			int destIndex = 128;

			for (int i = 0; i < 64; i++)
			{
				waveformData[--destIndex] = waveformData[--sourceIndex];
				waveformData[--destIndex] = waveformData[sourceIndex];
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DoInstEffect_Negate(sbyte[] waveformData, Instrument instr)
		{
			int index = instr.EffectIndex;
			waveformData[index] = (sbyte)-waveformData[index];

			instr.EffectIndex++;
			if (instr.EffectIndex >= instr.LoopLength)
				instr.EffectIndex = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DoInstEffect_MadMix1(sbyte[] waveformData, Instrument instr)
		{
			instr.EffectIndex++;
			instr.EffectIndex &= 0x7f;

			sbyte[] source2Data = waveforms[instr.SourceWave2];

			sbyte increment = source2Data[instr.EffectIndex];
			sbyte add = 3;

			for (int i = 0; i < instr.LoopLength; i++)
			{
				waveformData[i] += add;
				add += increment;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DoInstEffect_Addition(sbyte[] waveformData, Instrument instr)
		{
			sbyte[] source2Data = waveforms[instr.SourceWave2];

			for (int i = 0; i < instr.LoopLength; i++)
				waveformData[i] = (sbyte)(source2Data[i] + waveformData[i]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DoInstEffect_Filter2(sbyte[] waveformData)
		{
			for (int i = 0; i < 126; i++)
				waveformData[i + 1] = (sbyte)((waveformData[i] * 3 + waveformData[i + 2]) / 4);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DoInstEffect_Morphing(sbyte[] waveformData, Instrument instr)
		{
			sbyte[] source1Data = waveforms[instr.SourceWave1];
			sbyte[] source2Data = waveforms[instr.SourceWave2];

			instr.EffectIndex++;
			instr.EffectIndex &= 0x7f;

			int mul1, mul2;

			if (instr.EffectIndex < 64)
			{
				mul1 = instr.EffectIndex;
				mul2 = (instr.EffectIndex ^ 0xff) & 0x3f;
			}
			else
			{
				mul1 = 127 - instr.EffectIndex;
				mul2 = (mul1 ^ 0xff) & 0x3f;
			}

			for (int i = 0; i < instr.LoopLength; i++)
				waveformData[i] = (sbyte)((source1Data[i] * mul1 + source2Data[i] * mul2) / 64);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DoInstEffect_MorphF(sbyte[] waveformData, Instrument instr)
		{
			sbyte[] source1Data = waveforms[instr.SourceWave1];
			sbyte[] source2Data = waveforms[instr.SourceWave2];

			instr.EffectIndex++;
			instr.EffectIndex &= 0x1f;

			int mul1, mul2;

			if (instr.EffectIndex < 16)
			{
				mul1 = instr.EffectIndex;
				mul2 = (instr.EffectIndex ^ 0xff) & 0x0f;
			}
			else
			{
				mul1 = 31 - instr.EffectIndex;
				mul2 = (mul1 ^ 0xff) & 0x0f;
			}

			for (int i = 0; i < instr.LoopLength; i++)
				waveformData[i] = (sbyte)((source1Data[i] * mul1 + source2Data[i] * mul2) / 16);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DoInstEffect_Filter3(sbyte[] waveformData)
		{
			for (int i = 0; i < 126; i++)
				waveformData[i + 1] = (sbyte)((waveformData[i] + waveformData[i + 2]) / 2);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DoInstEffect_Polygate(sbyte[] waveformData, Instrument instr)
		{
			int index = instr.EffectIndex;
			waveformData[index] = (sbyte)-waveformData[index];

			index = (index + instr.SourceWave2) & (instr.LoopLength - 1);
			waveformData[index] = (sbyte)-waveformData[index];

			instr.EffectIndex++;
			if (instr.EffectIndex >= instr.LoopLength)
				instr.EffectIndex = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DoInstEffect_Colgate(sbyte[] waveformData, Instrument instr)
		{
			DoInstEffect_Filter(waveformData);

			instr.EffectIndex++;
			if (instr.EffectIndex == instr.SourceWave2)
			{
				instr.EffectIndex = 0;
				DoInstEffect_UpSampling(waveformData);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will return the period based on the given arguments
		/// </summary>
		/********************************************************************/
		private ushort GetPeriod(ushort note, short transpose, ushort fineTune)
		{
			int index = Tables.PeriodStartOffset + note + transpose + fineTune * 64;
			if (index < 0)
				return 0;

			return Tables.Periods[index];
		}



		/********************************************************************/
		/// <summary>
		/// Will change the speed on the module
		/// </summary>
		/********************************************************************/
		private void ChangeSpeed(ushort newSpeed)
		{
			if (newSpeed != playingInfo.LastShownSpeed)
			{
				// Remember the speed
				playingInfo.LastShownSpeed = newSpeed;
				playingInfo.CurrentSpeed = newSpeed;

				// Change the module info
				ShowSpeed();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current song positions
		/// </summary>
		/********************************************************************/
		private void ShowSongPositions()
		{
			OnModuleInfoChanged(InfoPositionLine, playingInfo.CurrentPosition.ToString());
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
			ShowSongPositions();
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

			for (int i = 0; i < numberOfChannels; i++)
			{
				if ((currentModuleType == ModuleType.DigitalMugician) || (i < 3))
					sb.Append(currentSequence[i, playingInfo.CurrentPosition].TrackNumber);
				else
					sb.Append(currentSequence2[i - 3, playingInfo.CurrentPosition].TrackNumber);

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
			return (playingInfo.CurrentSpeed & 0x0f).ToString();
		}
		#endregion
	}
}
