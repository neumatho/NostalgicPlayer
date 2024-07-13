/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.SidMon20.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.SidMon20
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class SidMon20Worker : ModulePlayerWithSubSongDurationAgentBase
	{
		#region BlockInfo class
		private class BlockInfo
		{
			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public BlockInfo(uint offset, uint length)
			{
				Offset = offset;
				Length = length;
			}

			public uint Offset;
			public uint Length;
		}
		#endregion

		private byte[][] waveformInfo;
		private sbyte[][] arpeggios;
		private sbyte[][] vibratoes;
		private Sequence[][] sequences;
		private Track[] tracks;
		private Instrument[] instruments;
		private Sample[] samples;
		private SampleNegateInfo[] sampleNegateInfo;

		private byte numberOfPositions;
		private byte startSpeed;

		private GlobalPlayingInfo playingInfo;

		private bool endReached;

		private const int InfoPositionLine = 3;
		private const int InfoTrackLine = 4;
		private const int InfoSpeedLine = 5;

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => new [] { "sd2", "sid2", "sid" };



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			if (moduleStream.Length < 86)
				return AgentResult.Unknown;

			// Check the identifier string
			byte[] buf = new byte[28];

			moduleStream.Seek(58, SeekOrigin.Begin);
			moduleStream.Read(buf, 0, 28);

			string id = Encoding.ASCII.GetString(buf);
			if (id == "SIDMON II - THE MIDI VERSION")
				return AgentResult.Ok;

			return AgentResult.Unknown;
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
					description = Resources.IDS_SD2_INFODESCLINE0;
					value = (numberOfPositions + 1).ToString();
					break;
				}

				// Used tracks
				case 1:
				{
					description = Resources.IDS_SD2_INFODESCLINE1;
					value = tracks.Length.ToString();
					break;
				}

				// Used samples
				case 2:
				{
					description = Resources.IDS_SD2_INFODESCLINE2;
					value = samples.Length.ToString();
					break;
				}

				// Playing position
				case 3:
				{
					description = Resources.IDS_SD2_INFODESCLINE3;
					value = playingInfo.CurrentPosition.ToString();
					break;
				}

				// Playing tracks
				case 4:
				{
					description = Resources.IDS_SD2_INFODESCLINE4;
					value = FormatTracks();
					break;
				}

				// Current speed
				case 5:
				{
					description = Resources.IDS_SD2_INFODESCLINE5;
					value = playingInfo.Speed.ToString();
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

				if (!LoadSongData(moduleStream, out ushort numberOfSamples, out BlockInfo positionTableInfo, out BlockInfo noteTransposeInfo, out BlockInfo instrumentTransposeInfo, out BlockInfo instrumentsInfo, out BlockInfo waveformListInfo, out BlockInfo arpeggiosInfo, out BlockInfo vibratoesInfo, out BlockInfo sampleInfoInfo, out BlockInfo trackTableInfo, out BlockInfo tracksInfo))
				{
					errorMessage = Resources.IDS_SD2_ERR_LOADING_SONGDATA;
					return AgentResult.Error;
				}

				if (!LoadLists(moduleStream, waveformListInfo, arpeggiosInfo, vibratoesInfo))
				{
					errorMessage = Resources.IDS_SD2_ERR_LOADING_SONGDATA;
					return AgentResult.Error;
				}

				if (!LoadSequences(moduleStream, positionTableInfo, noteTransposeInfo, instrumentTransposeInfo))
				{
					errorMessage = Resources.IDS_SD2_ERR_LOADING_SEQUENCES;
					return AgentResult.Error;
				}

				if (!LoadTracks(moduleStream, trackTableInfo, tracksInfo, out uint sampleOffset))
				{
					errorMessage = Resources.IDS_SD2_ERR_LOADING_TRACKS;
					return AgentResult.Error;
				}

				if (!LoadInstruments(moduleStream, instrumentsInfo))
				{
					errorMessage = Resources.IDS_SD2_ERR_LOADING_INSTRUMENTS;
					return AgentResult.Error;
				}

				if (!LoadSamples(moduleStream, sampleOffset, numberOfSamples, sampleInfoInfo))
				{
					errorMessage = Resources.IDS_SD2_ERR_LOADING_SAMPLES;
					return AgentResult.Error;
				}

				// At this point, the numberOfPositions holds the real number of position, but the player itself need one less
				numberOfPositions--;
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
			PlayIt();

			if (endReached)
			{
				OnEndReachedOnAllChannels(playingInfo.CurrentPosition);
				endReached = false;
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

				for (int i = 0; i < 6 * 12; i++)
					frequencies[2 * 12 - 9 + i] = 3546895U / Tables.Periods[1 + i];

				foreach (Sample sample in samples)
				{
					SampleInfo sampleInfo = new SampleInfo
					{
						Name = sample.Name,
						Flags = SampleInfo.SampleFlag.None,
						Type = SampleInfo.SampleType.Sample,
						Volume = 256,
						Panning = -1,
						Sample = sample.SampleData,
						Length = (uint)sample.SampleData.Length,
						NoteFrequencies = frequencies
					};

					if (sample.LoopLength <= 2)
					{
						// No loop
						sampleInfo.LoopStart = 0;
						sampleInfo.LoopLength = 0;
					}
					else
					{
						// Sample loops
						sampleInfo.Flags |= SampleInfo.SampleFlag.Loop;
						sampleInfo.LoopStart = sample.LoopStart;
						sampleInfo.LoopLength = sample.LoopLength;
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
			InitializeSound();
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
			return numberOfPositions + 1;
		}



		/********************************************************************/
		/// <summary>
		/// Create a snapshot of all the internal structures and return it
		/// </summary>
		/********************************************************************/
		protected override ISnapshot CreateSnapshot()
		{
			return new Snapshot(playingInfo, instruments, sampleNegateInfo);
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
			Snapshot clonedSnapshot = new Snapshot(currentSnapshot.PlayingInfo, currentSnapshot.Instruments, currentSnapshot.SampleNegateInfo);

			playingInfo = clonedSnapshot.PlayingInfo;
			instruments = clonedSnapshot.Instruments;
			sampleNegateInfo = clonedSnapshot.SampleNegateInfo;

			UpdateModuleInformation();

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Load the song data information
		/// </summary>
		/********************************************************************/
		private bool LoadSongData(ModuleStream moduleStream, out ushort numberOfSamples, out BlockInfo positionTableInfo, out BlockInfo noteTransposeInfo, out BlockInfo instrumentTransposeInfo, out BlockInfo instrumentsInfo, out BlockInfo waveformListInfo, out BlockInfo arpeggiosInfo, out BlockInfo vibratoesInfo, out BlockInfo sampleInfoInfo, out BlockInfo trackTableInfo, out BlockInfo tracksInfo)
		{
			// Skip MIDI mode
			moduleStream.Seek(2, SeekOrigin.Begin);

			numberOfPositions = moduleStream.Read_UINT8();
			numberOfPositions++;

			startSpeed = moduleStream.Read_UINT8();

			numberOfSamples = moduleStream.Read_B_UINT16();
			numberOfSamples /= 64;

			uint offset = 58;		// Offset to ID
			offset += moduleStream.Read_B_UINT32();		// ID length
			offset += moduleStream.Read_B_UINT32();		// Song length length

			uint length = moduleStream.Read_B_UINT32();
			positionTableInfo = new BlockInfo(offset, length);

			offset += length;
			length = moduleStream.Read_B_UINT32();
			noteTransposeInfo = new BlockInfo(offset, length);

			offset += length;
			length = moduleStream.Read_B_UINT32();
			instrumentTransposeInfo = new BlockInfo(offset, length);

			offset += length;
			length = moduleStream.Read_B_UINT32();
			instrumentsInfo = new BlockInfo(offset, length);

			offset += length;
			length = moduleStream.Read_B_UINT32();
			waveformListInfo = new BlockInfo(offset, length);

			offset += length;
			length = moduleStream.Read_B_UINT32();
			arpeggiosInfo = new BlockInfo(offset, length);

			offset += length;
			length = moduleStream.Read_B_UINT32();
			vibratoesInfo = new BlockInfo(offset, length);

			offset += length;
			length = moduleStream.Read_B_UINT32();
			sampleInfoInfo = new BlockInfo(offset, length);

			offset += length;
			length = moduleStream.Read_B_UINT32();
			trackTableInfo = new BlockInfo(offset, length);

			offset += length;
			length = moduleStream.Read_B_UINT32();
			tracksInfo = new BlockInfo(offset, length);

			if (moduleStream.EndOfStream)
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the different lists
		/// </summary>
		/********************************************************************/
		private bool LoadLists(ModuleStream moduleStream, BlockInfo waveformListInfo, BlockInfo arpeggiosInfo, BlockInfo vibratoesInfo)
		{
			waveformInfo = LoadSingleList<byte>(moduleStream, waveformListInfo);
			if (waveformInfo == null)
				return false;

			arpeggios = LoadSingleList<sbyte>(moduleStream, arpeggiosInfo);
			if (arpeggios == null)
				return false;

			vibratoes = LoadSingleList<sbyte>(moduleStream, vibratoesInfo);
			if (vibratoes == null)
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single list
		/// </summary>
		/********************************************************************/
		private T[][] LoadSingleList<T>(ModuleStream moduleStream, BlockInfo blockInfo)
		{
			if ((blockInfo.Length % 16) != 0)
				return null;

			moduleStream.Seek(blockInfo.Offset, SeekOrigin.Begin);

			uint count = blockInfo.Length / 16;
			T[][] list = new T[count][];

			for (int i = 0; i < count; i++)
			{
				list[i] = new T[16];

				moduleStream.Read((byte[])(Array)list[i], 0, 16);
				if (moduleStream.EndOfStream)
					return null;
			}

			return list;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the sequences
		/// </summary>
		/********************************************************************/
		private bool LoadSequences(ModuleStream moduleStream, BlockInfo positionTableInfo, BlockInfo noteTransposeInfo, BlockInfo instrumentTransposeInfo)
		{
			if ((positionTableInfo.Length != noteTransposeInfo.Length) || (noteTransposeInfo.Length != instrumentTransposeInfo.Length))
				return false;

			if (positionTableInfo.Length != (numberOfPositions * 4))
				return false;

			byte[] positions = new byte[positionTableInfo.Length];
			sbyte[] noteTranspose = new sbyte[noteTransposeInfo.Length];
			sbyte[] instrumentTranspose = new sbyte[instrumentTransposeInfo.Length];

			moduleStream.Seek(positionTableInfo.Offset, SeekOrigin.Begin);
			moduleStream.Read(positions, 0, positions.Length);

			if (moduleStream.EndOfStream)
				return false;

			moduleStream.Seek(noteTransposeInfo.Offset, SeekOrigin.Begin);
			moduleStream.ReadSigned(noteTranspose, 0, positions.Length);

			if (moduleStream.EndOfStream)
				return false;

			moduleStream.Seek(instrumentTransposeInfo.Offset, SeekOrigin.Begin);
			moduleStream.ReadSigned(instrumentTranspose, 0, positions.Length);

			if (moduleStream.EndOfStream)
				return false;

			sequences = new Sequence[4][];
			int startOffset = 0;

			for (int i = 0; i < 4; i++, startOffset += numberOfPositions)
			{
				sequences[i] = new Sequence[numberOfPositions];

				for (int j = 0; j < numberOfPositions; j++)
				{
					sequences[i][j] = new Sequence
					{
						TrackNumber = positions[startOffset + j],
						NoteTranspose = noteTranspose[startOffset + j],
						InstrumentTranspose = instrumentTranspose[startOffset + j]
					};
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the tracks
		/// </summary>
		/********************************************************************/
		private bool LoadTracks(ModuleStream moduleStream, BlockInfo trackTableInfo, BlockInfo tracksInfo, out uint sampleOffset)
		{
			sampleOffset = 0;

			// Read track offset table
			uint numberOfTracks = trackTableInfo.Length / 2;
			ushort[] trackOffsetTable = new ushort[numberOfTracks];

			moduleStream.Seek(trackTableInfo.Offset, SeekOrigin.Begin);
			moduleStream.ReadArray_B_UINT16s(trackOffsetTable, 0, trackOffsetTable.Length);

			if (moduleStream.EndOfStream)
				return false;

			tracks = new Track[numberOfTracks];

			for (int i = 0; i < numberOfTracks; i++)
			{
				uint trackLength = (i == numberOfTracks - 1 ? tracksInfo.Length : trackOffsetTable[i + 1]) - trackOffsetTable[i];
				byte[] trackData = new byte[trackLength];

				moduleStream.Seek(tracksInfo.Offset + trackOffsetTable[i], SeekOrigin.Begin);
				moduleStream.Read(trackData, 0, (int)trackLength);

				if (moduleStream.EndOfStream)
					return false;

				tracks[i] = new Track
				{
					TrackData = trackData
				};
			}

			// Stream position now points to start of sample, but make sure it's on an even position
			sampleOffset = (uint)moduleStream.Position;

			if ((sampleOffset % 1) != 0)
				sampleOffset++;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the instruments
		/// </summary>
		/********************************************************************/
		private bool LoadInstruments(ModuleStream moduleStream, BlockInfo instrumentsInfo)
		{
			if ((instrumentsInfo.Length % 32) != 0)
				return false;

			moduleStream.Seek(instrumentsInfo.Offset, SeekOrigin.Begin);

			uint count = instrumentsInfo.Length / 32;
			instruments = new Instrument[count];

			for (uint i = 0; i < count; i++)
			{
				Instrument instrument = new Instrument();

				instrument.WaveformListNumber = moduleStream.Read_UINT8();
				instrument.WaveformListLength = moduleStream.Read_UINT8();
				instrument.WaveformListSpeed = moduleStream.Read_UINT8();
				instrument.WaveformListDelay = moduleStream.Read_UINT8();
				instrument.ArpeggioNumber = moduleStream.Read_UINT8();
				instrument.ArpeggioLength = moduleStream.Read_UINT8();
				instrument.ArpeggioSpeed = moduleStream.Read_UINT8();
				instrument.ArpeggioDelay = moduleStream.Read_UINT8();
				instrument.VibratoNumber = moduleStream.Read_UINT8();
				instrument.VibratoLength = moduleStream.Read_UINT8();
				instrument.VibratoSpeed = moduleStream.Read_UINT8();
				instrument.VibratoDelay = moduleStream.Read_UINT8();
				instrument.PitchBendSpeed = moduleStream.Read_INT8();
				instrument.PitchBendDelay = moduleStream.Read_UINT8();

				moduleStream.Seek(2, SeekOrigin.Current);

				instrument.AttackMax = moduleStream.Read_UINT8();
				instrument.AttackSpeed = moduleStream.Read_UINT8();
				instrument.DecayMin = moduleStream.Read_UINT8();
				instrument.DecaySpeed = moduleStream.Read_UINT8();
				instrument.SustainTime = moduleStream.Read_UINT8();
				instrument.ReleaseMin = moduleStream.Read_UINT8();
				instrument.ReleaseSpeed = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
					return false;

				moduleStream.Seek(9, SeekOrigin.Current);

				instruments[i] = instrument;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the samples
		/// </summary>
		/********************************************************************/
		private bool LoadSamples(ModuleStream moduleStream, uint sampleOffset, ushort numberOfSamples, BlockInfo sampleInfoInfo)
		{
			Encoding encoding = EncoderCollection.Amiga;
			byte[] name = new byte[32];

			samples = new Sample[numberOfSamples];
			sampleNegateInfo = new SampleNegateInfo[numberOfSamples];

			// First read the sample information
			moduleStream.Seek(sampleInfoInfo.Offset, SeekOrigin.Begin);

			for (int i = 0; i < numberOfSamples; i++)
			{
				Sample sample = new Sample();
				SampleNegateInfo negateInfo = new SampleNegateInfo();

				// Skip sample data pointer
				moduleStream.Seek(4, SeekOrigin.Current);

				sample.Length = moduleStream.Read_B_UINT16() * 2U;
				sample.LoopStart = moduleStream.Read_B_UINT16() * 2U;
				sample.LoopLength = moduleStream.Read_B_UINT16() * 2U;

				if ((sample.LoopStart > sample.Length))
				{
					sample.LoopStart = 0;
					sample.LoopLength = 0;
				}
				else if ((sample.LoopStart + sample.LoopLength) > sample.Length)
					sample.LoopLength = sample.Length - sample.LoopStart;

				// Load negate variables
				negateInfo.StartOffset = (uint)(moduleStream.Read_B_UINT16() * 2);
				negateInfo.EndOffset = (uint)(moduleStream.Read_B_UINT16() * 2);
				negateInfo.LoopIndex = moduleStream.Read_B_UINT16();
				negateInfo.Status = moduleStream.Read_B_UINT16();
				negateInfo.Speed = moduleStream.Read_B_INT16();
				negateInfo.Position = moduleStream.Read_B_INT32();
				negateInfo.Index = moduleStream.Read_B_UINT16();
				negateInfo.DoNegation = moduleStream.Read_B_INT16();

				if (moduleStream.EndOfStream)
					return false;

				moduleStream.Seek(4, SeekOrigin.Current);

				moduleStream.Read(name, 0, 32);
				if (moduleStream.EndOfStream)
					return false;

				sample.Name = encoding.GetString(name);

				samples[i] = sample;
				sampleNegateInfo[i] = negateInfo;
			}

			// Now read the sample data
			moduleStream.Seek(sampleOffset, SeekOrigin.Begin);

			for (int i = 0; i < numberOfSamples; i++)
			{
				Sample sample = samples[i];

				sample.SampleData = moduleStream.ReadSampleData(i, (int)sample.Length, out int readBytes);
				if (readBytes < sample.Length)
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player have allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			waveformInfo = null;
			arpeggios = null;
			vibratoes = null;
			sequences = null;
			tracks = null;
			instruments = null;
			samples = null;
			sampleNegateInfo = null;
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
				CurrentPosition = 0,
				CurrentRow = 0,
				PatternLength = 64,
				CurrentRast = 0,
				CurrentRast2 = 0,
				Speed = startSpeed,

				VoiceInfo = new VoiceInfo[4]
			};

			for (int i = 0; i < 4; i++)
			{
				Instrument instrument = instruments[0];
				byte sampleNumber = waveformInfo[instrument.WaveformListNumber][0];
				Sample sample = samples[sampleNumber];

				VoiceInfo voiceInfo = new VoiceInfo
				{
					SequenceList = sequences[i],

					PitchBendValue = 0,

					SampleVolume = 0,
					WaveListDelay = 0,
					WaveListOffset = 0,
					ArpeggioDelay = 0,
					ArpeggioOffset = 0,
					VibratoDelay = 0,
					VibratoOffset = 0,
					PitchBendCounter = 0,
					NoteSlideSpeed = 0,
					EnvelopeState = EnvelopeState.Attack,
					SustainCounter = 0,

					Instrument = instrument,
					CurrentSample = sampleNumber,
					SampleData = sample.SampleData,
					SampleLength = sample.Length,
					LoopSample = sample.SampleData,
					LoopOffset = sample.LoopStart,
					LoopLength = sample.LoopLength,

					SamplePeriod = 0,
					OriginalNote = 0,
					CurrentNote = 0,
					CurrentInstrument = 0,
					CurrentEffect = 0,
					CurrentEffectArg = 0,
					NoteSlideNote = 0
				};

				FindNote(voiceInfo);

				playingInfo.VoiceInfo[i] = voiceInfo;
			}

			endReached = false;
		}



		/********************************************************************/
		/// <summary>
		/// The main play method
		/// </summary>
		/********************************************************************/
		private void PlayIt()
		{
			playingInfo.CurrentRast2++;
			if (playingInfo.CurrentRast2 == 3)
				playingInfo.CurrentRast2 = 0;

			playingInfo.CurrentRast++;
			if (playingInfo.CurrentRast >= playingInfo.Speed)
			{
				playingInfo.CurrentRast = 0;
				playingInfo.CurrentRast2 = 0;

				for (int i = 0; i < 4; i++)
					GetNote(playingInfo.VoiceInfo[i]);

				for (int i = 0; i < 4; i++)
					PlayVoice(playingInfo.VoiceInfo[i], VirtualChannels[i]);

				DoNegation();

				playingInfo.CurrentRow++;
				if (playingInfo.CurrentRow == playingInfo.PatternLength)
				{
					playingInfo.CurrentRow = 0;

					if (playingInfo.CurrentPosition == numberOfPositions)
					{
						playingInfo.CurrentPosition = -1;
						endReached = true;
					}

					playingInfo.CurrentPosition++;

					MarkPositionAsVisited(playingInfo.CurrentPosition);
					ShowSongPositions();

					for (int i = 0; i < 4; i++)
						FindNote(playingInfo.VoiceInfo[i]);

					ShowTracks();
				}
			}

			for (int i = 0; i < 4; i++)
				DoEffect(playingInfo.VoiceInfo[i], VirtualChannels[i]);

			if (playingInfo.CurrentRast != 0)
				DoNegation();
		}



		/********************************************************************/
		/// <summary>
		/// Negate samples
		/// </summary>
		/********************************************************************/
		private void DoNegation()
		{
			SampleNegateInfo[] workingSamples = new SampleNegateInfo[4];

			for (int i = 0; i < 4; i++)
			{
				VoiceInfo voiceInfo = playingInfo.VoiceInfo[i];

				ushort sampleNumber = voiceInfo.CurrentSample;
				SampleNegateInfo negateInfo = sampleNegateInfo[sampleNumber];
				workingSamples[i] = negateInfo;

				if (negateInfo.DoNegation == 0)
				{
					negateInfo.DoNegation = -1;

					if (negateInfo.Index == 0)
					{
						negateInfo.Index = negateInfo.LoopIndex;

						if (negateInfo.Status != 0)
						{
							Sample sample = samples[sampleNumber];
							uint endOffset = negateInfo.EndOffset - 1;

							int position = (int)(negateInfo.StartOffset + negateInfo.Position);
							sample.SampleData[position] = (sbyte)~sample.SampleData[position];

							negateInfo.Position += negateInfo.Speed;
							if (negateInfo.Position < 0)
							{
								if (negateInfo.Status == 2)
									negateInfo.Position = (int)endOffset;
								else
								{
									negateInfo.Position += -negateInfo.Speed;
									negateInfo.Speed = (short)-negateInfo.Speed;
								}
							}
							else
							{
								if (negateInfo.Position > endOffset)
								{
									if (negateInfo.Status == 1)
										negateInfo.Position = 0;
									else
									{
										negateInfo.Position += -negateInfo.Speed;
										negateInfo.Speed = (short)-negateInfo.Speed;
									}
								}
							}
						}
					}
					else
					{
						negateInfo.Index++;
						negateInfo.Index &= 0x1f;
					}
				}
			}

			for (int i = 0; i < 4; i++)
				workingSamples[i].DoNegation = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize sequence variables
		/// </summary>
		/********************************************************************/
		private void FindNote(VoiceInfo voiceInfo)
		{
			Sequence sequence = voiceInfo.SequenceList[playingInfo.CurrentPosition];

			voiceInfo.CurrentTrack = tracks[sequence.TrackNumber].TrackData;
			voiceInfo.TrackPosition = 0;
			voiceInfo.NoteTranspose = sequence.NoteTranspose;
			voiceInfo.InstrumentTranspose = sequence.InstrumentTranspose;
			voiceInfo.EmptyNotesCounter = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Find the next note to play
		/// </summary>
		/********************************************************************/
		private void GetNote(VoiceInfo voiceInfo)
		{
			GetNote2(voiceInfo);

			if (voiceInfo.CurrentNote != 0)
				voiceInfo.CurrentNote = (ushort)(voiceInfo.CurrentNote + voiceInfo.NoteTranspose);
		}



		/********************************************************************/
		/// <summary>
		/// Find the next note to play
		/// </summary>
		/********************************************************************/
		private void GetNote2(VoiceInfo voiceInfo)
		{
			byte[] trackData = voiceInfo.CurrentTrack;

			voiceInfo.CurrentNote = 0;
			voiceInfo.CurrentInstrument = 0;
			voiceInfo.CurrentEffect = 0;
			voiceInfo.CurrentEffectArg = 0;

			if (voiceInfo.EmptyNotesCounter == 0)
			{
				sbyte trackValue = (sbyte)(voiceInfo.TrackPosition < trackData.Length ? trackData[voiceInfo.TrackPosition++] : -1);

				if (trackValue == 0)
				{
					voiceInfo.CurrentEffect = trackData[voiceInfo.TrackPosition++];
					voiceInfo.CurrentEffectArg = trackData[voiceInfo.TrackPosition++];
					return;
				}

				if (trackValue > 0)
				{
					if (trackValue >= 0x70)
					{
						voiceInfo.CurrentEffect = (byte)trackValue;
						voiceInfo.CurrentEffectArg = trackData[voiceInfo.TrackPosition++];
						return;
					}

					voiceInfo.CurrentNote = (ushort)trackValue;

					trackValue = (sbyte)trackData[voiceInfo.TrackPosition++];
					if (trackValue >= 0)
					{
						if (trackValue >= 0x70)
						{
							voiceInfo.CurrentEffect = (byte)trackValue;
							voiceInfo.CurrentEffectArg = trackData[voiceInfo.TrackPosition++];
							return;
						}

						voiceInfo.CurrentInstrument = (ushort)trackValue;

						trackValue = (sbyte)trackData[voiceInfo.TrackPosition++];
						if (trackValue >= 0)
						{
							voiceInfo.CurrentEffect = (byte)trackValue;
							voiceInfo.CurrentEffectArg = trackData[voiceInfo.TrackPosition++];
							return;
						}
					}
				}

				voiceInfo.EmptyNotesCounter = (ushort)(~trackValue);
			}
			else
				voiceInfo.EmptyNotesCounter--;
		}



		/********************************************************************/
		/// <summary>
		/// Playing the voice
		/// </summary>
		/********************************************************************/
		private void PlayVoice(VoiceInfo voiceInfo, IChannel channel)
		{
			voiceInfo.PitchBendValue = 0;

			if (voiceInfo.CurrentNote != 0)
			{
				voiceInfo.SampleVolume = 0;
				voiceInfo.WaveListDelay = 0;
				voiceInfo.WaveListOffset = 0;
				voiceInfo.ArpeggioDelay = 0;
				voiceInfo.ArpeggioOffset = 0;
				voiceInfo.VibratoDelay = 0;
				voiceInfo.VibratoOffset = 0;
				voiceInfo.PitchBendCounter = 0;
				voiceInfo.NoteSlideSpeed = 0;

				voiceInfo.EnvelopeState = EnvelopeState.Attack;
				voiceInfo.SustainCounter = 0;

				ushort instrument = voiceInfo.CurrentInstrument;
				if (instrument != 0)
				{
					instrument = (ushort)(instrument - 1 + voiceInfo.InstrumentTranspose);
					if (instrument < instruments.Length)
					{
						voiceInfo.Instrument = instruments[instrument];

						byte[] waveFormList = waveformInfo[voiceInfo.Instrument.WaveformListNumber];
						voiceInfo.CurrentSample = waveFormList[0];

						Sample sample = samples[voiceInfo.CurrentSample];
						voiceInfo.SampleData = sample.SampleData;
						voiceInfo.SampleLength = sample.Length;
						voiceInfo.LoopSample = sample.SampleData;
						voiceInfo.LoopOffset = sample.LoopStart;
						voiceInfo.LoopLength = sample.LoopLength;
					}
				}

				sbyte[] arpeggio = arpeggios[voiceInfo.Instrument.ArpeggioNumber];
				int note = voiceInfo.CurrentNote + arpeggio[0];
				if ((note >= 0) && (note < Tables.Periods.Length))
				{
					voiceInfo.OriginalNote = (ushort)note;
					voiceInfo.SamplePeriod = Tables.Periods[note];

					if (voiceInfo.SampleLength != 0)
					{
						channel.PlaySample((short)voiceInfo.CurrentSample, voiceInfo.SampleData, 0, voiceInfo.SampleLength);

						if (voiceInfo.LoopLength > 2)
							channel.SetLoop(voiceInfo.LoopSample, voiceInfo.LoopOffset, voiceInfo.LoopLength);
					}
					else
						channel.Mute();
				}
				else
					channel.Mute();

				channel.SetAmigaPeriod(voiceInfo.SamplePeriod);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle effects
		/// </summary>
		/********************************************************************/
		private void DoEffect(VoiceInfo voiceInfo, IChannel channel)
		{
			DoAdsrCurve(voiceInfo, channel);
			DoWaveForm(voiceInfo, channel);
			DoArpeggio(voiceInfo);
			DoSoundTracker(voiceInfo, channel);
			DoVibrato(voiceInfo);
			DoPitchBend(voiceInfo);
			DoNoteSlide(voiceInfo);

			voiceInfo.SamplePeriod = (ushort)(voiceInfo.SamplePeriod + voiceInfo.PitchBendValue);

			if (voiceInfo.SamplePeriod < 95)
				voiceInfo.SamplePeriod = 95;
			else if (voiceInfo.SamplePeriod > 5760)
				voiceInfo.SamplePeriod = 5760;

			channel.SetAmigaPeriod(voiceInfo.SamplePeriod);
		}



		/********************************************************************/
		/// <summary>
		/// Handle ADSR
		/// </summary>
		/********************************************************************/
		private void DoAdsrCurve(VoiceInfo voiceInfo, IChannel channel)
		{
			Instrument instrument = voiceInfo.Instrument;

			switch (voiceInfo.EnvelopeState)
			{
				case EnvelopeState.Attack:
				{
					voiceInfo.SampleVolume += instrument.AttackSpeed;

					if (voiceInfo.SampleVolume >= instrument.AttackMax)
					{
						voiceInfo.SampleVolume = instrument.AttackMax;
						voiceInfo.EnvelopeState = EnvelopeState.Decay;
					}
					break;
				}

				case EnvelopeState.Decay:
				{
					if (instrument.DecaySpeed == 0)
						voiceInfo.EnvelopeState = EnvelopeState.Sustain;
					else
					{
						voiceInfo.SampleVolume -= instrument.DecaySpeed;

						if (voiceInfo.SampleVolume <= instrument.DecayMin)
						{
							voiceInfo.SampleVolume = instrument.DecayMin;
							voiceInfo.EnvelopeState = EnvelopeState.Sustain;
						}
					}
					break;
				}

				case EnvelopeState.Sustain:
				{
					if (voiceInfo.SustainCounter == instrument.SustainTime)
						voiceInfo.EnvelopeState = EnvelopeState.Release;
					else
						voiceInfo.SustainCounter++;

					break;
				}

				case EnvelopeState.Release:
				{
					if (instrument.ReleaseSpeed == 0)
						voiceInfo.EnvelopeState = EnvelopeState.Done;
					else
					{
						voiceInfo.SampleVolume -= instrument.ReleaseSpeed;

						if (voiceInfo.SampleVolume <= instrument.ReleaseMin)
						{
							voiceInfo.SampleVolume = instrument.ReleaseMin;
							voiceInfo.EnvelopeState = EnvelopeState.Done;
						}
					}
					break;
				}
			}

			channel.SetVolume((ushort)voiceInfo.SampleVolume);
		}



		/********************************************************************/
		/// <summary>
		/// Handle wave form list change
		/// </summary>
		/********************************************************************/
		private void DoWaveForm(VoiceInfo voiceInfo, IChannel channel)
		{
			Instrument instrument = voiceInfo.Instrument;

			if (instrument.WaveformListLength != 0)
			{
				if (voiceInfo.WaveListDelay == instrument.WaveformListDelay)
				{
					voiceInfo.WaveListDelay -= instrument.WaveformListSpeed;

					if (voiceInfo.WaveListOffset == instrument.WaveformListLength)
						voiceInfo.WaveListOffset = -1;

					voiceInfo.WaveListOffset++;

					sbyte waveFormValue = (sbyte)waveformInfo[instrument.WaveformListNumber][voiceInfo.WaveListOffset];
					if (waveFormValue >= 0)
					{
						voiceInfo.CurrentSample = (ushort)waveFormValue;

						Sample sample = samples[voiceInfo.CurrentSample];
						voiceInfo.LoopSample = sample.SampleData;
						voiceInfo.LoopOffset = sample.LoopStart;
						voiceInfo.LoopLength = sample.LoopLength;

						if (channel.IsActive)
						{
							channel.SetSample(voiceInfo.LoopSample, voiceInfo.LoopOffset, voiceInfo.LoopOffset + voiceInfo.LoopLength);
							channel.SetLoop(voiceInfo.LoopOffset, voiceInfo.LoopLength);
						}
					}
					else
						voiceInfo.WaveListOffset--;
				}
				else
					voiceInfo.WaveListDelay++;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle arpeggios
		/// </summary>
		/********************************************************************/
		private void DoArpeggio(VoiceInfo voiceInfo)
		{
			Instrument instrument = voiceInfo.Instrument;

			if (instrument.ArpeggioLength != 0)
			{
				if (voiceInfo.ArpeggioDelay == instrument.ArpeggioDelay)
				{
					voiceInfo.ArpeggioDelay -= instrument.ArpeggioSpeed;

					if (voiceInfo.ArpeggioOffset == instrument.ArpeggioLength)
						voiceInfo.ArpeggioOffset = -1;

					voiceInfo.ArpeggioOffset++;

					sbyte arpeggioValue = arpeggios[instrument.ArpeggioNumber][voiceInfo.ArpeggioOffset];
					short newNote = (short)(voiceInfo.OriginalNote + arpeggioValue);
					voiceInfo.SamplePeriod = Tables.Periods[newNote];
				}
				else
					voiceInfo.ArpeggioDelay++;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle vibratos
		/// </summary>
		/********************************************************************/
		private void DoVibrato(VoiceInfo voiceInfo)
		{
			Instrument instrument = voiceInfo.Instrument;

			if (instrument.VibratoLength != 0)
			{
				if (voiceInfo.VibratoDelay == instrument.VibratoDelay)
				{
					voiceInfo.VibratoDelay -= instrument.VibratoSpeed;

					if (voiceInfo.VibratoOffset == instrument.VibratoLength)
						voiceInfo.VibratoOffset = -1;

					voiceInfo.VibratoOffset++;

					sbyte vibratoValue = vibratoes[instrument.VibratoNumber][voiceInfo.VibratoOffset];
					voiceInfo.SamplePeriod = (ushort)(voiceInfo.SamplePeriod + vibratoValue);
				}
				else
					voiceInfo.VibratoDelay++;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle pitch bend
		/// </summary>
		/********************************************************************/
		private void DoPitchBend(VoiceInfo voiceInfo)
		{
			Instrument instrument = voiceInfo.Instrument;

			if (instrument.PitchBendSpeed != 0)
			{
				if (voiceInfo.PitchBendCounter == instrument.PitchBendDelay)
					voiceInfo.PitchBendValue = (short)(voiceInfo.PitchBendValue + instrument.PitchBendSpeed);
				else
					voiceInfo.PitchBendCounter++;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle note slide
		/// </summary>
		/********************************************************************/
		private void DoNoteSlide(VoiceInfo voiceInfo)
		{
			if ((voiceInfo.CurrentEffect != 0) && (voiceInfo.CurrentEffect < 0x70) && (voiceInfo.CurrentEffectArg != 0))
			{
				voiceInfo.NoteSlideNote = Tables.Periods[voiceInfo.CurrentEffect];

				int direction = voiceInfo.NoteSlideNote - voiceInfo.SamplePeriod;
				if (direction == 0)
					return;

				short effectArg = (short)voiceInfo.CurrentEffectArg;
				if (direction < 0)
					effectArg = (short)-effectArg;

				voiceInfo.NoteSlideSpeed = effectArg;
			}

			short speed = voiceInfo.NoteSlideSpeed;
			if (speed != 0)
			{
				if (speed < 0)
				{
					voiceInfo.SamplePeriod = (ushort)(voiceInfo.SamplePeriod + speed);

					if (voiceInfo.SamplePeriod <= voiceInfo.NoteSlideNote)
					{
						voiceInfo.SamplePeriod = voiceInfo.NoteSlideNote;
						voiceInfo.NoteSlideSpeed = 0;
					}
				}
				else
				{
					voiceInfo.SamplePeriod = (ushort)(voiceInfo.SamplePeriod + speed);

					if (voiceInfo.SamplePeriod >= voiceInfo.NoteSlideNote)
					{
						voiceInfo.SamplePeriod = voiceInfo.NoteSlideNote;
						voiceInfo.NoteSlideSpeed = 0;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle SoundTracker effects
		/// </summary>
		/********************************************************************/
		private void DoSoundTracker(VoiceInfo voiceInfo, IChannel channel)
		{
			ushort effect = voiceInfo.CurrentEffect;
			if (effect >= 0x70)
			{
				effect &= 0x0f;

				if ((playingInfo.CurrentRast != 0) || (effect >= 5))
				{
					switch (effect)
					{
						// Arpeggio
						case 0x0:
						{
							byte[] arpTab =
							[
								(byte)(voiceInfo.CurrentEffectArg >> 4),
								0,
								(byte)(voiceInfo.CurrentEffectArg & 0x0f),
								0
							];

							byte arpValue = arpTab[playingInfo.CurrentRast2];
							voiceInfo.SamplePeriod = Tables.Periods[voiceInfo.OriginalNote + arpValue];
							break;
						}

						// Pitch up
						case 0x1:
						{
							voiceInfo.PitchBendValue = (short)-voiceInfo.CurrentEffectArg;
							break;
						}

						// Pitch down
						case 0x2:
						{
							voiceInfo.PitchBendValue = (short)voiceInfo.CurrentEffectArg;
							break;
						}

						// Volume up
						case 0x3:
						{
							if (voiceInfo.EnvelopeState == EnvelopeState.Done)
							{
								if ((playingInfo.CurrentRast == 0) && (voiceInfo.CurrentInstrument != 0))
									voiceInfo.SampleVolume = voiceInfo.Instrument.AttackSpeed;

								short volume = (short)(voiceInfo.SampleVolume + voiceInfo.CurrentEffectArg * 4);
								if (volume >= 256)
									volume = 255;

								voiceInfo.SampleVolume = volume;
							}
							break;
						}

						// Volume down
						case 0x4:
						{
							if (voiceInfo.EnvelopeState == EnvelopeState.Done)
							{
								if ((playingInfo.CurrentRast == 0) && (voiceInfo.CurrentInstrument != 0))
									voiceInfo.SampleVolume = voiceInfo.Instrument.AttackSpeed;

								short volume = (short)(voiceInfo.SampleVolume - voiceInfo.CurrentEffectArg * 4);
								if (volume < 0)
									volume = 0;

								voiceInfo.SampleVolume = volume;
							}
							break;
						}

						// Set ADSR attack
						case 0x5:
						{
							Instrument instrument = voiceInfo.Instrument;

							instrument.AttackMax = (byte)voiceInfo.CurrentEffectArg;
							instrument.AttackSpeed = (byte)voiceInfo.CurrentEffectArg;
							break;
						}

						// Set pattern length
						case 0x6:
						{
							if (voiceInfo.CurrentEffectArg != 0)
								playingInfo.PatternLength = (byte)voiceInfo.CurrentEffectArg;

							break;
						}

						// Volume change
						case 0xc:
						{
							ushort volume = voiceInfo.CurrentEffectArg;
							channel.SetAmigaVolume((ushort)(volume > 64 ? 64 : volume));

							volume *= 4;
							if (volume >= 255)
								volume = 255;

							voiceInfo.SampleVolume = (short)volume;
							break;
						}

						// Speed change
						case 0xf:
						{
							byte speed = (byte)(voiceInfo.CurrentEffectArg & 0x0f);
							if ((speed != 0) && (speed != playingInfo.Speed))
							{
								playingInfo.Speed = speed;
								ShowSpeed();
							}
							break;
						}
					}
				}
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
			OnModuleInfoChanged(InfoSpeedLine, playingInfo.Speed.ToString());
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

			for (int i = 0; i < 4; i++)
			{
				sb.Append(playingInfo.VoiceInfo[i].SequenceList[playingInfo.CurrentPosition].TrackNumber);
				sb.Append(", ");
			}

			sb.Remove(sb.Length - 2, 2);

			return sb.ToString();
		}
		#endregion
	}
}
