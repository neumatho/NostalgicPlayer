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
using Polycode.NostalgicPlayer.Agent.Player.MusicAssembler.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using PositionInfo = Polycode.NostalgicPlayer.Agent.Player.MusicAssembler.Containers.PositionInfo;

namespace Polycode.NostalgicPlayer.Agent.Player.MusicAssembler
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class MusicAssemblerWorker : ModulePlayerWithSubSongDurationAgentBase
	{
		private int subSongCount;
		private int subSongSpeedOffset;
		private int subSongPositionListOffset;
		private int moduleStartOffset;
		private int instrumentInfoOffsetOffset;
		private int sampleInfoOffsetOffset;
		private int tracksOffsetOffset;

		private List<SongInfo> subSongs;
		private Dictionary<ushort, PositionInfo[]> positionLists;
		private byte[][] tracks;
		private Instrument[] instruments;
		private Sample[] samples;

		private GlobalPlayingInfo playingInfo;
		private VoiceInfo[] voices;

		private const int InfoPositionLine = 4;
		private const int InfoTrackLine = 5;

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => [ "ma" ];



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			if (moduleStream.Length < 0x622)
				return AgentResult.Unknown;

			// Read the first part of the file, so it is easier to search
			byte[] buffer = new byte[0x700];

			moduleStream.Seek(0, SeekOrigin.Begin);
			moduleStream.ReadInto(buffer, 0, buffer.Length);

			return TestModule(buffer);
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
					description = Resources.IDS_MA_INFODESCLINE0;
					value = FormatPositionLengths();
					break;
				}

				// Used tracks
				case 1:
				{
					description = Resources.IDS_MA_INFODESCLINE1;
					value = tracks.Length.ToString();
					break;
				}

				// Used instruments
				case 2:
				{
					description = Resources.IDS_MA_INFODESCLINE2;
					value = instruments.Length.ToString();
					break;
				}

				// Used samples
				case 3:
				{
					description = Resources.IDS_MA_INFODESCLINE3;
					value = samples.Length.ToString();
					break;
				}

				// Playing positions
				case 4:
				{
					description = Resources.IDS_MA_INFODESCLINE4;
					value = FormatPositions();
					break;
				}

				// Playing tracks
				case 5:
				{
					description = Resources.IDS_MA_INFODESCLINE5;
					value = FormatTracks();
					break;
				}

				// Current speed
				case 6:
				{
					description = Resources.IDS_MA_INFODESCLINE6;
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

				if (!LoadSubSongInfo(moduleStream))
				{
					errorMessage = Resources.IDS_MA_ERR_LOADING_SUBSONG;
					return AgentResult.Error;
				}

				if (!LoadPositionLists(moduleStream))
				{
					errorMessage = Resources.IDS_MA_ERR_LOADING_POSITION_LISTS;
					return AgentResult.Error;
				}

				if (!LoadTracks(moduleStream))
				{
					errorMessage = Resources.IDS_MA_ERR_LOADING_TRACKS;
					return AgentResult.Error;
				}

				if (!LoadInstrumentInfo(moduleStream))
				{
					errorMessage = Resources.IDS_MA_ERR_LOADING_INSTRUMENTINFO;
					return AgentResult.Error;
				}

				if (!LoadSampleInfo(moduleStream, out int[] sampleDataOffsets))
				{
					errorMessage = Resources.IDS_MA_ERR_LOADING_SAMPLEINFO;
					return AgentResult.Error;
				}

				if (!LoadSampleData(moduleStream, sampleDataOffsets))
				{
					errorMessage = Resources.IDS_MA_ERR_LOADING_SAMPLES;
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
			playingInfo.SpeedCounter--;

			if (playingInfo.SpeedCounter == 0)
			{
				playingInfo.SpeedCounter = playingInfo.Speed;

				for (int i = 0; i < 4; i++)
				{
					if (GetNextRowInTrack(voices[i], VirtualChannels[i]))
						DoVoice(voices[i], VirtualChannels[i]);
				}
			}
			else
			{
				for (int i = 0; i < 4; i++)
					DoVoice(voices[i], VirtualChannels[i]);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return information about sub-songs
		/// </summary>
		/********************************************************************/
		public override SubSongInfo SubSongs => new SubSongInfo(subSongs.Count, 0);



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

				for (int j = 0; j < 4 * 12; j++)
					frequencies[3 * 12 + j] = 3546895U / Tables.Periods[j];

				foreach (Sample sample in samples)
				{
					SampleInfo sampleInfo = new SampleInfo
					{
						Name = sample.Name,
						Flags = SampleInfo.SampleFlag.None,
						Volume = 256,
						Panning = -1,
						NoteFrequencies = frequencies
					};

					if ((sample.Length > 1) && (sample.Length <= 128))
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
						sampleInfo.Sample = sample.SampleData;
						sampleInfo.Length = sample.Length > 1 ? sample.Length * 2U : 0;

						if (sample.LoopLength != 0)
						{
							sampleInfo.LoopStart = (uint)(sample.Length - sample.LoopLength) * 2;
							sampleInfo.LoopLength = sample.LoopLength * 2U;
							sampleInfo.Flags |= SampleInfo.SampleFlag.Loop;
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
		/// Test the file to see if it's a Music Assembler player and extract
		/// needed information
		/// </summary>
		/********************************************************************/
		private AgentResult TestModule(byte[] buffer)
		{
			// First check some places in the file for required
			// assembler code
			if ((buffer[0] != 0x60) || (buffer[1] != 0x00) || (buffer[4] != 0x60) || (buffer[5] != 0x00) || (buffer[8] != 0x60) || (buffer[9] != 00) || (buffer[12] != 0x48) || (buffer[13] != 0xe7))
				return AgentResult.Unknown;

			if (!ExtractInfoFromInitFunction(buffer))
				return AgentResult.Unknown;

			if (!ExtractInfoFromPlayFunction(buffer))
				return AgentResult.Unknown;

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Try to find the init function in the player and extract needed
		/// information from it
		/// </summary>
		/********************************************************************/
		private bool ExtractInfoFromInitFunction(byte[] searchBuffer)
		{
			int searchLength = searchBuffer.Length;
			int index;

			// Find the init function
			int startOfInit = (((sbyte)searchBuffer[2] << 8) | searchBuffer[3]) + 2;
			if (startOfInit >= searchBuffer.Length)
				return false;

			// Find sub-song information
			for (index = startOfInit; index < (searchLength - 4); index += 2)
			{
				if ((searchBuffer[index] == 0xb0) && (searchBuffer[index + 1] == 0x7c))
					break;
			}

			if (index >= (searchLength - 4))
				return false;

			subSongCount = (searchBuffer[index + 2] << 8) | searchBuffer[index + 3];
			index += 4;

			for (; index < (searchLength - 4); index += 2)
			{
				if ((searchBuffer[index] == 0x49) && (searchBuffer[index + 1] == 0xfa))
					break;
			}

			if (index >= (searchLength - 4))
				return false;

			subSongSpeedOffset = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2;
			index += 4;

			for (; index < (searchLength - 4); index += 2)
			{
				if ((searchBuffer[index] == 0x49) && (searchBuffer[index + 1] == 0xfb))
					break;
			}

			if (index >= (searchLength - 4))
				return false;

			subSongPositionListOffset = (sbyte)searchBuffer[index + 3] + index + 2;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Try to find the play function in the player and extract needed
		/// information from it
		/// </summary>
		/********************************************************************/
		private bool ExtractInfoFromPlayFunction(byte[] searchBuffer)
		{
			int searchLength = searchBuffer.Length;
			int index;

			int startOfPlay = 0x0c;

			for (index = startOfPlay; index < (searchLength - 4); index += 2)
			{
				if ((searchBuffer[index] == 0x43) && (searchBuffer[index + 1] == 0xfa))
					break;
			}

			if (index >= (searchLength - 4))
				return false;

			moduleStartOffset = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2;
			index += 4;

			for (; index < (searchLength - 8); index += 2)
			{
				if ((searchBuffer[index] == 0xd3) && (searchBuffer[index + 1] == 0xfa))
					break;
			}

			if (index >= (searchLength - 8))
				return false;

			instrumentInfoOffsetOffset = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2;

			if ((searchBuffer[index + 4] != 0xd5) || (searchBuffer[index + 5] != 0xfa))
				return false;

			sampleInfoOffsetOffset = (((sbyte)searchBuffer[index + 6] << 8) | searchBuffer[index + 7]) + index + 6;
			index += 8;

			for (; index < (searchLength - 2); index += 2)
			{
				if (searchBuffer[index] == 0x61)
					break;
			}

			if (index >= (searchLength - 2))
				return false;

			index = (sbyte)searchBuffer[index + 1] + index + 2;
			if (index >= searchLength)
				return false;

			for (; index < (searchLength - 4); index += 2)
			{
				if ((searchBuffer[index] == 0xdb) && (searchBuffer[index + 1] == 0xfa))
					break;
			}

			if (index >= (searchLength - 4))
				return false;

			tracksOffsetOffset = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load the sub-song information for all sub-songs
		/// </summary>
		/********************************************************************/
		private bool LoadSubSongInfo(ModuleStream moduleStream)
		{
			subSongs = new List<SongInfo>();

			byte[] speedList = new byte[subSongCount];
			ushort[] positionList = new ushort[subSongCount * 4];

			// Read the sub-song speed list
			moduleStream.Seek(subSongSpeedOffset, SeekOrigin.Begin);
			if (moduleStream.Read(speedList, 0, subSongCount) != subSongCount)
				return false;

			// Read the sub-song position list
			moduleStream.Seek(subSongPositionListOffset, SeekOrigin.Begin);
			moduleStream.ReadArray_B_UINT16s(positionList, 0, subSongCount * 4);

			if (moduleStream.EndOfStream)
				return false;

			for (int i = 0; i < subSongCount; i++)
			{
				ushort voice1PositionList = positionList[i * 4 + 0];
				ushort voice2PositionList = positionList[i * 4 + 1];
				ushort voice3PositionList = positionList[i * 4 + 2];
				ushort voice4PositionList = positionList[i * 4 + 3];

				if (((voice1PositionList + 2) == voice2PositionList) && ((voice2PositionList + 2) == voice3PositionList) && ((voice3PositionList + 2) == voice4PositionList))
					continue;

				SongInfo songInfo = new SongInfo();

				songInfo.StartSpeed = speedList[i];
				songInfo.PositionLists[0] = voice1PositionList;
				songInfo.PositionLists[1] = voice2PositionList;
				songInfo.PositionLists[2] = voice3PositionList;
				songInfo.PositionLists[3] = voice4PositionList;

				subSongs.Add(songInfo);
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the position lists
		/// </summary>
		/********************************************************************/
		private bool LoadPositionLists(ModuleStream moduleStream)
		{
			positionLists = new Dictionary<ushort, PositionInfo[]>();

			foreach (SongInfo song in subSongs)
			{
				for (int i = 0; i < 4; i++)
				{
					ushort positionListOffset = song.PositionLists[i];
					if (positionLists.ContainsKey(positionListOffset))
						continue;

					// Seek to position list
					moduleStream.Seek(moduleStartOffset + positionListOffset, SeekOrigin.Begin);

					// Load the position list
					PositionInfo[] positionList = LoadSinglePositionList(moduleStream);
					if (positionList == null)
						return false;

					positionLists[positionListOffset] = positionList;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single position list
		/// </summary>
		/********************************************************************/
		private PositionInfo[] LoadSinglePositionList(ModuleStream moduleStream)
		{
			List<PositionInfo> positionList = new List<PositionInfo>();

			for (;;)
			{
				PositionInfo posInfo = new PositionInfo();

				posInfo.TrackNumber = moduleStream.Read_UINT8();

				byte byt = moduleStream.Read_UINT8();

				ushort val = (ushort)(byt << 4);
				byt = (byte)((val & 0xff) >> 1);

				posInfo.Transpose = (byte)(val >> 8);
				posInfo.RepeatCounter = (sbyte)byt;

				if (moduleStream.EndOfStream)
					return null;

				positionList.Add(posInfo);

				if ((posInfo.TrackNumber == 0xff) || (posInfo.TrackNumber == 0xfe))	// End of position list?
					break;
			}

			return positionList.ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Load all the tracks
		/// </summary>
		/********************************************************************/
		private bool LoadTracks(ModuleStream moduleStream)
		{
			// Find number of tracks
			int numberOfTracks = positionLists.SelectMany(x => x.Value)
				.Where(x => (x.TrackNumber != 0xfe) && (x.TrackNumber != 0xff))
				.Max(x => x.TrackNumber) + 1;

			tracks = new byte[numberOfTracks][];

			// Read the tracks start offset
			moduleStream.Seek(tracksOffsetOffset, SeekOrigin.Begin);
			int tracksStartOffset = moduleStream.Read_B_INT32();

			if (moduleStream.EndOfStream)
				return false;

			tracksStartOffset += moduleStartOffset;

			// Read the track offset table
			ushort[] trackOffsetTable = new ushort[numberOfTracks];

			moduleStream.Seek(moduleStartOffset, SeekOrigin.Begin);
			moduleStream.ReadArray_B_UINT16s(trackOffsetTable, 0, numberOfTracks);

			if (moduleStream.EndOfStream)
				return false;

			for (int i = 0; i < numberOfTracks; i++)
			{
				moduleStream.Seek(tracksStartOffset + trackOffsetTable[i], SeekOrigin.Begin);

				byte[] track = LoadSingleTrack(moduleStream);
				if (track == null)
					return false;

				tracks[i] = track;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single track
		/// </summary>
		/********************************************************************/
		private byte[] LoadSingleTrack(ModuleStream moduleStream)
		{
			List<byte> trackBytes = new List<byte>();

			for (;;)
			{
				byte byt = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
					return null;

				trackBytes.Add(byt);

				if ((byt & 0x80) != 0)
				{
					if ((byt & 0x40) != 0)
					{
						byt = moduleStream.Read_UINT8();

						if (moduleStream.EndOfStream)
							return null;

						trackBytes.Add(byt);

						byt = moduleStream.Read_UINT8();

						if (moduleStream.EndOfStream)
							return null;

						trackBytes.Add(byt);

						if ((byt & 0x80) != 0)
						{
							byt = moduleStream.Read_UINT8();

							if (moduleStream.EndOfStream)
								return null;

							trackBytes.Add(byt);
						}
					}
				}
				else
				{
					byt = moduleStream.Read_UINT8();

					if (moduleStream.EndOfStream)
						return null;

					trackBytes.Add(byt);

					if ((byt & 0x80) != 0)
					{
						byt = moduleStream.Read_UINT8();

						if (moduleStream.EndOfStream)
							return null;

						trackBytes.Add(byt);
					}
				}

				byte nextByte = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
					return null;

				if (nextByte == 0xff)	// End of track?
				{
					trackBytes.Add(nextByte);
					break;
				}

				moduleStream.Seek(-1, SeekOrigin.Current);
			}

			return trackBytes.ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Load all the instruments
		/// </summary>
		/********************************************************************/
		private bool LoadInstrumentInfo(ModuleStream moduleStream)
		{
			// Read the instruments start offset
			moduleStream.Seek(instrumentInfoOffsetOffset, SeekOrigin.Begin);
			int instrumentsStartOffset = moduleStream.Read_B_INT32();

			if (moduleStream.EndOfStream)
				return false;

			// We also need the sample info start offset, so we can calculate
			// how many instruments to load
			moduleStream.Seek(sampleInfoOffsetOffset, SeekOrigin.Begin);
			int sampleStartOffset = moduleStream.Read_B_INT32();

			if (moduleStream.EndOfStream)
				return false;

			int numberOfInstruments = (sampleStartOffset - instrumentsStartOffset) / 16;
			instruments = new Instrument[numberOfInstruments];

			moduleStream.Seek(moduleStartOffset + instrumentsStartOffset, SeekOrigin.Begin);

			for (int i = 0; i < numberOfInstruments; i++)
			{
				Instrument instr = new Instrument();

				instr.SampleNumber = moduleStream.Read_UINT8();
				instr.Attack = moduleStream.Read_UINT8();
				instr.Decay_Sustain = moduleStream.Read_UINT8();
				instr.VibratoDelay = moduleStream.Read_UINT8();
				instr.Release = moduleStream.Read_UINT8();
				instr.VibratoSpeed = moduleStream.Read_UINT8();
				instr.VibratoLevel = moduleStream.Read_UINT8();
				instr.Arpeggio = moduleStream.Read_UINT8();
				instr.FxArp_SpdLp = moduleStream.Read_UINT8();
				instr.Hold = moduleStream.Read_UINT8();
				instr.Key_WaveRate = moduleStream.Read_UINT8();
				instr.WaveLevel_Speed = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
					return false;

				moduleStream.Seek(4, SeekOrigin.Current);

				instruments[i] = instr;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the sample information
		/// </summary>
		/********************************************************************/
		private bool LoadSampleInfo(ModuleStream moduleStream, out int[] sampleDataOffsets)
		{
			// Read the sample start offset
			moduleStream.Seek(sampleInfoOffsetOffset, SeekOrigin.Begin);
			int sampleStartOffset = moduleStream.Read_B_INT32();

			if (moduleStream.EndOfStream)
			{
				sampleDataOffsets = null;
				return false;
			}

			sampleStartOffset += moduleStartOffset;
			moduleStream.Seek(sampleStartOffset, SeekOrigin.Begin);

			int numberOfSamples = (positionLists.Keys.Min() + moduleStartOffset - sampleStartOffset) / 24;
			samples = new Sample[numberOfSamples];
			sampleDataOffsets = new int[numberOfSamples];

			Encoding encoder = EncoderCollection.Amiga;

			for (int i = 0; i < numberOfSamples; i++)
			{
				Sample sample = new Sample();

				int offset = moduleStream.Read_B_INT32();
				if (offset < 0)
					sampleDataOffsets[i] = -1;
				else
					sampleDataOffsets[i] = sampleStartOffset + offset;

				sample.Length = moduleStream.Read_B_UINT16();
				sample.LoopLength = moduleStream.Read_B_UINT16();
				sample.Name = moduleStream.ReadString(encoder, 16).TrimEnd();

				if (moduleStream.EndOfStream)
					return false;

				samples[i] = sample;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the samples
		/// </summary>
		/********************************************************************/
		private bool LoadSampleData(ModuleStream moduleStream, int[] sampleDataOffsets)
		{
			for (int i = 0; i < samples.Length; i++)
			{
				Sample sample = samples[i];

				if (sampleDataOffsets[i] < 0)
				{
					sample.SampleData = Tables.EmptySample;
					sample.Length = 1;
				}
				else
				{
					moduleStream.Seek(sampleDataOffsets[i], SeekOrigin.Begin);

					int length = sample.Length * 2;
					sample.SampleData = moduleStream.ReadSampleData(i, length, out int readBytes);

					if (readBytes != length)
						return false;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize sound structures
		/// </summary>
		/********************************************************************/
		private void InitializeSound(int subSong)
		{
			SongInfo song = subSongs[subSong];

			playingInfo = new GlobalPlayingInfo
			{
				MasterVolume = 64,

				Speed = song.StartSpeed,
				SpeedCounter = 1
			};

			voices = new VoiceInfo[4];

			for (int i = 0; i < 4; i++)
			{
				PositionInfo[] positionList = positionLists[song.PositionLists[Tables.ChannelMap[i]]];

				voices[i] = new VoiceInfo
				{
					ChannelNumber = i,

					PositionList = positionList,

					CurrentPosition = 0,
					CurrentTrackRow = 0,
					TrackRepeatCounter = positionList[0].RepeatCounter,
					RowDelayCounter = 0,

					Flag = VoiceFlag.None,

					CurrentInstrument = instruments[0],

					CurrentNote = 0,
					Transpose = positionList[0].Transpose,

					Volume = 0,
					DecreaseVolume = false,
					SustainCounter = 0,

					ArpeggioCounter = 0,
					ArpeggioValueToUse = 0,

					PortamentoOrVibratoValue = 0,

					PortamentoAddValue = 0,

					VibratoDelayCounter = 0,
					VibratoDirection = 0,
					VibratoSpeedCounter = 0,
					VibratoAddValue = 0,

					WaveLengthModifier = 0,
					WaveDirection = false,
					WaveSpeedCounter = 0,

					SampleData = null,
					SampleStartOffset = 0,
					SampleLength = 0
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
			subSongs = null;
			positionLists = null;
			tracks = null;
			instruments = null;
			samples = null;

			playingInfo = null;
			voices = null;
		}



		/********************************************************************/
		/// <summary>
		/// Take the next row and/or position if it is time to do so
		/// </summary>
		/********************************************************************/
		private bool GetNextRowInTrack(VoiceInfo voiceInfo, IChannel channel)
		{
			voiceInfo.RowDelayCounter--;

			if (voiceInfo.RowDelayCounter >= 0)
				return true;

			PositionInfo positionInfo = voiceInfo.PositionList[voiceInfo.CurrentPosition];

			if (positionInfo.TrackNumber == 0xfe)
			{
				OnEndReached(voiceInfo.ChannelNumber);

				return true;
			}

			byte[] trackData = tracks[positionInfo.TrackNumber];
			ushort trackRow = voiceInfo.CurrentTrackRow;

			byte trackByte = trackData[trackRow++];

			void SetNote()
			{
				voiceInfo.PortamentoAddValue = 0;
				voiceInfo.CurrentNote = (byte)((trackByte & 0x3f) + voiceInfo.Transpose);

				trackByte = trackData[trackRow++];

				if ((trackByte & 0x80) == 0)
					voiceInfo.PortamentoOrVibratoValue = 0;
				else
				{
					trackByte &= 0x7f;

					voiceInfo.PortamentoOrVibratoValue = trackData[trackRow++];
				}
			}

			if ((trackByte & 0x80) == 0)
			{
				if ((trackByte & 0x40) == 0)
				{
					InitializeVoice(voiceInfo);
					ForceRetrigger(voiceInfo, channel);
				}

				SetNote();
			}
			else
			{
				if ((trackByte & 0x40) == 0)
				{
					trackByte &= 0x3f;

					voiceInfo.Flag |= VoiceFlag.Release;
					voiceInfo.SustainCounter = 0;
				}
				else
				{
					voiceInfo.CurrentInstrument = instruments[trackByte & 0x3f];

					trackByte = trackData[trackRow++];

					if ((trackByte & 0x40) == 0)
					{
						voiceInfo.WaveLengthModifier = 0;
						voiceInfo.WaveDirection = false;
						voiceInfo.WaveSpeedCounter = 1;

						InitializeVoice(voiceInfo);
					}

					ForceRetrigger(voiceInfo, channel);
					SetNote();
				}
			}

			voiceInfo.RowDelayCounter = (sbyte)trackByte;

			if (trackData[trackRow] == 0xff)
			{
				voiceInfo.TrackRepeatCounter -= 8;

				if (voiceInfo.TrackRepeatCounter < 0)
				{
					voiceInfo.CurrentPosition++;

					positionInfo = voiceInfo.PositionList[voiceInfo.CurrentPosition];

					if (positionInfo.TrackNumber == 0xff)
					{
						voiceInfo.CurrentPosition = 0;
						positionInfo = voiceInfo.PositionList[0];

						OnEndReached(voiceInfo.ChannelNumber);
					}

					voiceInfo.Transpose = positionInfo.Transpose;
					voiceInfo.TrackRepeatCounter = positionInfo.RepeatCounter;

					ShowChannelPositions();
					ShowTracks();
				}

				trackRow = 0;
			}

			voiceInfo.CurrentTrackRow = trackRow;

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the voice to be ready for new data
		/// </summary>
		/********************************************************************/
		private void InitializeVoice(VoiceInfo voiceInfo)
		{
			voiceInfo.DecreaseVolume = false;
			voiceInfo.ArpeggioValueToUse = 0;
			voiceInfo.Volume = 0;
			voiceInfo.VibratoDirection = 0;
			voiceInfo.VibratoSpeedCounter = 0;
			voiceInfo.VibratoAddValue = 0;
			voiceInfo.Flag = VoiceFlag.None;

			Instrument instr = voiceInfo.CurrentInstrument;

			voiceInfo.VibratoDelayCounter = instr.VibratoDelay;
			voiceInfo.ArpeggioCounter = instr.FxArp_SpdLp;
			voiceInfo.SustainCounter = instr.Hold;
		}



		/********************************************************************/
		/// <summary>
		/// Make sure that the voice retrigs a new note when setting up the
		/// sample
		/// </summary>
		/********************************************************************/
		private void ForceRetrigger(VoiceInfo voiceInfo, IChannel channel)
		{
			voiceInfo.Flag &= VoiceFlag.Release;
			voiceInfo.Flag |= VoiceFlag.Retrig;

			channel.Mute();
		}



		/********************************************************************/
		/// <summary>
		/// Setup a new note + run real-time effects on the voice
		/// </summary>
		/********************************************************************/
		private void DoVoice(VoiceInfo voiceInfo, IChannel channel)
		{
			Instrument instr = voiceInfo.CurrentInstrument;

			SetupSample(instr, voiceInfo, channel);

			ushort period = DoArpeggio(instr, voiceInfo);
			period = DoPortamento(period, voiceInfo);
			period = DoVibrato(period, instr, voiceInfo);

			DoAdsr(instr, voiceInfo);

			ushort volume = (ushort)((voiceInfo.Volume * playingInfo.MasterVolume) / 256);

			if ((voiceInfo.VibratoDelayCounter == 0) && voiceInfo.DecreaseVolume)
				volume /= 4;

			if (period < 127)
				period = 127;

			channel.SetAmigaVolume(volume);
			channel.SetAmigaPeriod(period);
		}



		/********************************************************************/
		/// <summary>
		/// Setup the sample on the voice
		/// </summary>
		/********************************************************************/
		private void SetupSample(Instrument instr, VoiceInfo voiceInfo, IChannel channel)
		{
			Sample sample = samples[instr.SampleNumber];

			bool setSample = false;

			if (voiceInfo.Flag.HasFlag(VoiceFlag.SetLoop))
			{
				voiceInfo.Flag &= ~VoiceFlag.SetLoop;

				if (sample.LoopLength != 0)
				{
					voiceInfo.SampleLength = sample.LoopLength;
					setSample = true;

					if (sample.Length <= 128)
						DoWaveLengthModifying(instr, sample, voiceInfo);
					else
					{
						voiceInfo.SampleStartOffset = (uint)((sample.Length - sample.LoopLength) * 2);
						voiceInfo.SampleData = sample.SampleData;
					}

					goto SetSample;
				}
				else
				{
					voiceInfo.SampleData = Tables.EmptySample;
					voiceInfo.SampleStartOffset = 0;
					voiceInfo.SampleLength = 2;
					setSample = true;

					voiceInfo.Flag &= ~VoiceFlag.Synthesis;
				}
			}
			else
			{
				if (voiceInfo.Flag.HasFlag(VoiceFlag.Retrig))
				{
					voiceInfo.Flag &= VoiceFlag.Release;
					voiceInfo.Flag |= VoiceFlag.SetLoop;

					sbyte[] sampleData = sample.SampleData;
					uint startOffset = 0;

					ushort sampleLength = sample.Length;
					ushort loopLength = sample.LoopLength;
					byte keyWaveRate = instr.Key_WaveRate;

					if ((sampleLength <= 128) && (sampleData != Tables.EmptySample))
					{
						// Synthesis sample
						voiceInfo.Flag |= VoiceFlag.Synthesis;

						startOffset = keyWaveRate;

						sampleLength = loopLength;

						if (sampleLength == 0)
							sampleLength = 1;
					}

					voiceInfo.SampleData = sampleData;
					voiceInfo.SampleStartOffset = startOffset;
					voiceInfo.SampleLength = sampleLength;
					setSample = true;

					goto SetSample;
				}
			}

			if (voiceInfo.Flag.HasFlag(VoiceFlag.Synthesis))
			{
				DoWaveLengthModifying(instr, sample, voiceInfo);
				setSample = true;
			}

			SetSample:

			if (setSample)
			{
				channel.SetSampleNumber(instr.SampleNumber);
				channel.SetSample(voiceInfo.SampleData, voiceInfo.SampleStartOffset, voiceInfo.SampleLength * 2U);

				if (voiceInfo.SampleData != Tables.EmptySample)
					channel.SetLoop(voiceInfo.SampleData, voiceInfo.SampleStartOffset, voiceInfo.SampleLength * 2U);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DoWaveLengthModifying(Instrument instr, Sample sample, VoiceInfo voiceInfo)
		{
			sbyte waveLength = (sbyte)(instr.WaveLevel_Speed >> 4);
			byte waveSpeed = (byte)(instr.WaveLevel_Speed & 0x0f);

			if (waveSpeed != 0)
			{
				waveSpeed *= 2;

				voiceInfo.WaveSpeedCounter++;

				if (waveSpeed < voiceInfo.WaveSpeedCounter)
				{
					voiceInfo.WaveSpeedCounter = 0;
					voiceInfo.WaveDirection = !voiceInfo.WaveDirection;
				}

				if (voiceInfo.WaveDirection)
					waveLength = (sbyte)-waveLength;
			}

			voiceInfo.WaveLengthModifier = (ushort)(voiceInfo.WaveLengthModifier + waveLength);

			ushort length = sample.Length;
			uint startOffset = ((voiceInfo.WaveLengthModifier / 4U) + instr.Key_WaveRate) & (length - 1U);

			voiceInfo.SampleData = sample.SampleData;
			voiceInfo.SampleStartOffset = startOffset;
		}



		/********************************************************************/
		/// <summary>
		/// Handle the arpeggio effect
		/// </summary>
		/********************************************************************/
		private ushort DoArpeggio(Instrument instr, VoiceInfo voiceInfo)
		{
			byte arp = instr.Arpeggio;
			sbyte arpToUse = (sbyte)(voiceInfo.ArpeggioValueToUse - 2);

			if (arp != 0)
			{
				if (arpToUse == 0)
					arp >>= 4;
				else if (arpToUse < 0)
					arp = 0;
				else
					arp &= 0x0f;
			}

			if (instr.FxArp_SpdLp != 0)
			{
				voiceInfo.ArpeggioCounter += 0x10;

				if (voiceInfo.ArpeggioCounter >= instr.FxArp_SpdLp)
				{
					voiceInfo.ArpeggioCounter = 0;

					byte spd = (byte)(instr.FxArp_SpdLp & 0x03);

					if (spd == 0)
					{
						voiceInfo.DecreaseVolume = !voiceInfo.DecreaseVolume;
						spd = 1;
					}

					arpToUse += 3;

					if (arpToUse >= 4)
						arpToUse = (sbyte)spd;

					voiceInfo.ArpeggioValueToUse = (byte)arpToUse;
				}
			}

			byte note = (byte)(voiceInfo.CurrentNote + arp);

			if (!voiceInfo.Flag.HasFlag(VoiceFlag.Synthesis))
				note += instr.Key_WaveRate;

			if (note >= 48)
				note = 47;

			return Tables.Periods[note];
		}



		/********************************************************************/
		/// <summary>
		/// Handle the portamento effect
		/// </summary>
		/********************************************************************/
		private ushort DoPortamento(ushort period, VoiceInfo voiceInfo)
		{
			if (voiceInfo.PortamentoOrVibratoValue != 0)
			{
				bool flagSet = voiceInfo.Flag.HasFlag(VoiceFlag.Portamento);
				voiceInfo.Flag ^= VoiceFlag.Portamento;

				if (!flagSet || ((voiceInfo.PortamentoOrVibratoValue & 0x01) == 0))
				{
					period <<= 1;
					period = (ushort)(period + voiceInfo.PortamentoAddValue);
					period >>= 1;

					if ((voiceInfo.PortamentoOrVibratoValue & 0x80) != 0)
						voiceInfo.PortamentoAddValue = (short)(voiceInfo.PortamentoAddValue + ((-(sbyte)voiceInfo.PortamentoOrVibratoValue + 1) >> 1));
					else
						voiceInfo.PortamentoAddValue = (short)(voiceInfo.PortamentoAddValue - ((voiceInfo.PortamentoOrVibratoValue + 1) >> 1));
				}
			}

			return period;
		}



		/********************************************************************/
		/// <summary>
		/// Handle the vibrato effect
		/// </summary>
		/********************************************************************/
		private ushort DoVibrato(ushort period, Instrument instr, VoiceInfo voiceInfo)
		{
			if (voiceInfo.VibratoDelayCounter == 0)
			{
				if ((voiceInfo.PortamentoOrVibratoValue == 0) && (instr.VibratoLevel != 0))
				{
					sbyte level = (sbyte)instr.VibratoLevel;
					byte direction = (byte)(voiceInfo.VibratoDirection & 0x03);

					if ((direction != 0) && (direction != 3))
						level = (sbyte)-level;

					voiceInfo.VibratoAddValue = (short)(voiceInfo.VibratoAddValue + level);
					voiceInfo.VibratoSpeedCounter++;

					if (voiceInfo.VibratoSpeedCounter == instr.VibratoSpeed)
					{
						voiceInfo.VibratoSpeedCounter = 0;
						voiceInfo.VibratoDirection++;
					}

					period <<= 1;
					period = (ushort)(period + voiceInfo.VibratoAddValue);
					period >>= 1;
				}
			}
			else
				voiceInfo.VibratoDelayCounter--;

			return period;
		}



		/********************************************************************/
		/// <summary>
		/// Handle the vibrato effect
		/// </summary>
		/********************************************************************/
		private void DoAdsr(Instrument instr, VoiceInfo voiceInfo)
		{
			if (voiceInfo.Flag.HasFlag(VoiceFlag.Release))
			{
				if (voiceInfo.SustainCounter == 0)
				{
					int newVolume = voiceInfo.Volume - instr.Release;

					if (newVolume < 0)
						newVolume = 0;

					voiceInfo.Volume = (byte)newVolume;
				}
				else
					voiceInfo.SustainCounter--;
			}
			else
			{
				int newVolume = voiceInfo.Volume + instr.Attack;

				int decaySustain = instr.Decay_Sustain & 0xf0 | 0x0f;

				if (newVolume >= decaySustain)
				{
					voiceInfo.Flag |= VoiceFlag.Release;
					newVolume = instr.Decay_Sustain << 4;
				}

				voiceInfo.Volume = (byte)newVolume;
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
		/// Will update the module information with all dynamic values
		/// </summary>
		/********************************************************************/
		private void UpdateModuleInformation()
		{
			ShowChannelPositions();
			ShowTracks();
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
				sb.Append(voices[i].PositionList.Length);
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
				sb.Append(voices[i].CurrentPosition);
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
				byte trackNumber = voices[i].PositionList[voices[i].CurrentPosition].TrackNumber;

				if (trackNumber == 0xfe)
					sb.Append("-");
				else
					sb.Append(trackNumber);

				sb.Append(", ");
			}

			sb.Remove(sb.Length - 2, 2);

			return sb.ToString();
		}
		#endregion
	}
}
