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
using Polycode.NostalgicPlayer.Agent.Player.SidMon10.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.SidMon10
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class SidMon10Worker : ModulePlayerWithSubSongDurationAgentBase
	{
		private class SpecialSampleInfo
		{
			public uint InstrumentNumber { get; set; }
			public int SampleOffset { get; set; }
			public int Length { get; set; }
		}

		private int baseOffset;

		private int[] sequenceOffsets;
		private int trackDataOffset;
		private int trackOffsetTableOffset;
		private int instrumentOffset;
		private int waveformListOffset;
		private int waveformOffset;
		private int sampleOffset;
		private int songDataOffset;

		private uint noLoopValue;

		private Sequence[][] sequences;
		private Track[] tracks;
		private Instrument[] instruments;
		private byte[][] waveformInfo;
		private sbyte[][] waveforms;
		private Sample[] samples;

		private ushort[] periods;
		private int fineTuneMultiply;
		private bool enableFilter;
		private bool enableMixing;
		private List<SpecialSampleInfo> specialSamples;

		private MixInfo mix1Information;
		private MixInfo mix2Information;

		private uint startNumberOfRows;
		private uint startSpeed;

		private short[] instrumentToSampleInfoMapping;

		private GlobalPlayingInfo playingInfo;

		private bool endReached;

		private const int InfoPositionLine = 4;
		private const int InfoTrackLine = 5;
		private const int InfoSpeedLine = 6;

		#region Identify
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => [ "sd1", "sid1", "sid" ];



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			if (moduleStream.Length < 1024)
				return AgentResult.Unknown;

			// Read the first part of the file, so it is easier to search
			byte[] buffer = new byte[4096];

			moduleStream.Seek(0, SeekOrigin.Begin);
			moduleStream.ReadInto(buffer, 0, buffer.Length);

			return TestModule(buffer);
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

				FindCounts(moduleStream, out int numberOfTracks, out int numberOfInstruments, out int numberOfWaveformList, out int numberOfWaveforms);

				if (!LoadSongData(moduleStream, out uint numberOfPositions))
				{
					errorMessage = Resources.IDS_SD1_ERR_LOADING_SONGDATA;
					return AgentResult.Error;
				}

				if (!LoadInstruments(moduleStream, numberOfInstruments))
				{
					errorMessage = Resources.IDS_SD1_ERR_LOADING_INSTRUMENTS;
					return AgentResult.Error;
				}

				if (!LoadTracks(moduleStream, numberOfTracks))
				{
					errorMessage = Resources.IDS_SD1_ERR_LOADING_TRACKS;
					return AgentResult.Error;
				}

				if (!LoadSequences(moduleStream, numberOfPositions))
				{
					errorMessage = Resources.IDS_SD1_ERR_LOADING_SEQUENCES;
					return AgentResult.Error;
				}

				if (!LoadWaveforms(moduleStream, numberOfWaveformList, numberOfWaveforms))
				{
					errorMessage = Resources.IDS_SD1_ERR_LOADING_WAVEFORMS;
					return AgentResult.Error;
				}

				if (!LoadSamples(moduleStream))
				{
					errorMessage = Resources.IDS_SD1_ERR_LOADING_SAMPLES;
					return AgentResult.Error;
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
			for (int i = 0; i < 4; i++)
				PlayNote(sequences[i], playingInfo.VoiceInfo[i], VirtualChannels[i]);

			playingInfo.SpeedCounter++;
			if (playingInfo.SpeedCounter > playingInfo.Speed)
				playingInfo.SpeedCounter = 0;

			playingInfo.NewTrack = false;
			playingInfo.LoopSong = false;

			if (playingInfo.SpeedCounter == 0)
			{
				playingInfo.CurrentRow++;
				if (playingInfo.CurrentRow == playingInfo.NumberOfRows)
				{
					playingInfo.CurrentRow = 0;
					playingInfo.NewTrack = true;

					playingInfo.CurrentPosition++;
					if (playingInfo.CurrentPosition > sequences[0].Length)
					{
						playingInfo.CurrentPosition = 1;
						playingInfo.LoopSong = true;

						endReached = true;
					}
					else
						MarkPositionAsVisited((int)playingInfo.CurrentPosition - 1);

					ShowSongPositions();
				}
			}

			DoWaveformMixing(mix1Information, playingInfo.Mix1PlayingInfo);
			DoWaveformMixing(mix2Information, playingInfo.Mix2PlayingInfo);
			DoFilterEffect();

			if (endReached)
			{
				OnEndReachedOnAllChannels((int)playingInfo.CurrentPosition - 1);
				endReached = false;
			}
		}
		#endregion

		#region Information
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
				for (int i = 0, j = 0; i < instruments.Length; i++)
				{
					if (instrumentToSampleInfoMapping[i] < j)
						continue;

					j++;
					Instrument instr = instruments[i];

					// Build frequency table
					uint[] frequencies = new uint[10 * 12];

					for (int k = 0; k < 5 * 12; k++)
						frequencies[2 * 12 - 2 + k] = PeriodToFrequency(periods[8 + instr.FineTune * fineTuneMultiply + k]);

					SampleInfo sampleInfo;

					if (instr.WaveformNumber < 16)
					{
						sampleInfo = new SampleInfo
						{
							Name = string.Empty,
							Type = SampleInfo.SampleType.Synthesis,
							Flags = SampleInfo.SampleFlag.None,
							Volume = 256,
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
						uint sampleNum = instr.WaveformNumber - 16;
						if (sampleNum >= samples.Length) 
							continue;

						Sample sample = samples[sampleNum];
						if (sample.SampleData == null)
							continue;

						sampleInfo = new SampleInfo
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

						if (sample.LoopStart < 0)
						{
							// No loop
							sampleInfo.LoopStart = 0;
							sampleInfo.LoopLength = 0;
						}
						else
						{
							// Sample loops
							sampleInfo.Flags |= SampleInfo.SampleFlag.Loop;
							sampleInfo.LoopStart = (uint)sample.LoopStart;
							sampleInfo.LoopLength = (uint)sample.SampleData.Length - sampleInfo.LoopStart;
						}
					}

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
				// Number of positions
				case 0:
				{
					description = Resources.IDS_SD1_INFODESCLINE0;
					value = sequences[0].Length.ToString();
					break;
				}

				// Used tracks
				case 1:
				{
					description = Resources.IDS_SD1_INFODESCLINE1;
					value = tracks.Length.ToString();
					break;
				}

				// Used samples
				case 2:
				{
					description = Resources.IDS_SD1_INFODESCLINE2;
					value = samples.Length.ToString();
					break;
				}

				// Used wave tables
				case 3:
				{
					description = Resources.IDS_SD1_INFODESCLINE3;
					value = waveforms.Length.ToString();
					break;
				}

				// Playing position
				case 4:
				{
					description = Resources.IDS_SD1_INFODESCLINE4;
					value = (playingInfo.CurrentPosition - 1).ToString();
					break;
				}

				// Playing tracks
				case 5:
				{
					description = Resources.IDS_SD1_INFODESCLINE5;
					value = FormatTracks();
					break;
				}

				// Current speed
				case 6:
				{
					description = Resources.IDS_SD1_INFODESCLINE6;
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
			return sequences[0].Length;
		}



		/********************************************************************/
		/// <summary>
		/// Create a snapshot of all the internal structures and return it
		/// </summary>
		/********************************************************************/
		protected override ISnapshot CreateSnapshot()
		{
			return new Snapshot(playingInfo, instruments);
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
			Snapshot clonedSnapshot = new Snapshot(currentSnapshot.PlayingInfo, currentSnapshot.Instruments);

			playingInfo = clonedSnapshot.PlayingInfo;
			instruments = clonedSnapshot.Instruments;

			UpdateModuleInformation();

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Test the file to see if it's a SidMon 1.0 player and extract
		/// needed information
		/// </summary>
		/********************************************************************/
		private AgentResult TestModule(byte[] buffer)
		{
			// SC68 support are handled in a converter, so ignore these modules here
			if ((buffer[0] == 0x53) && (buffer[1] == 0x43) && (buffer[2] == 0x36) && (buffer[3] == 0x38))
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
			int index, channel;

			for (index = 0; index < searchLength - 2; index += 2)
			{
				if ((searchBuffer[index] == 0x41) && (searchBuffer[index + 1] == 0xfa))
					break;
			}

			if (index >= (searchLength - 4))
				return false;

			baseOffset = ((searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2;

			sequenceOffsets = new int[4];
			index += 4;

			for (channel = 0; (index < searchLength - 2) && (channel < 4); index += 2)
			{
				if ((searchBuffer[index] == 0xd1) && (searchBuffer[index + 1] == 0xe8))
					sequenceOffsets[channel++] = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + baseOffset;
			}

			if (index >= (searchLength - 4))
				return false;

			for (; index < searchLength - 2; index += 2)
			{
				if (((searchBuffer[index] == 0x70) && (searchBuffer[index + 1] == 0x03)) || ((searchBuffer[index] == 0x20) && (searchBuffer[index + 1] == 0x3c) && (searchBuffer[index + 5] == 0x03)))
					break;
			}

			if (index >= (searchLength - 4))
				return false;

			if (searchBuffer[index] == 0x20)
				index += 4;

			if ((searchBuffer[index + 2] != 0x41) || (searchBuffer[index + 3] != 0xfa) || (searchBuffer[index + 6] != 0xd1) || (searchBuffer[index + 7] != 0xe8))
				return false;

			trackDataOffset = (((sbyte)searchBuffer[index + 8] << 8) | searchBuffer[index + 9]) + baseOffset;
			index += 10;

			for (; index < searchLength - 2; index += 2)
			{
				if ((searchBuffer[index] == 0xd9) && (searchBuffer[index + 1] == 0xec))
					break;
			}

			if (index >= (searchLength - 4))
				return false;

			trackOffsetTableOffset = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + baseOffset;
			index += 4;

			for (; index < searchLength - 2; index += 2)
			{
				if ((searchBuffer[index] == 0xd9) && (searchBuffer[index + 1] == 0xec))
					break;
			}

			if (index >= (searchLength - 4))
				return false;

			instrumentOffset = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + baseOffset;

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
			int index, index1;

			enableFilter = false;
			enableMixing = false;
			specialSamples = null;

			// Start to find the play function in the player
			for (index = 0; index < searchLength - 4; index += 2)
			{
				if ((searchBuffer[index] == 0x48) && (searchBuffer[index + 1] == 0xe7) && (searchBuffer[index + 2] == 0xff) && (searchBuffer[index + 3] == 0xfe))
					break;
			}

			if (index >= (searchLength - 4))
				return false;

			// Find "PlayNote" function
			for (; index < searchLength - 4; index += 2)
			{
				if ((searchBuffer[index] == 0x61) && (searchBuffer[index + 1] == 0x00))
					break;
			}

			if (index >= (searchLength - 4))
				return false;

			int playNoteOffset = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2;

			// Find song data
			for (; index < searchLength - 4; index += 2)
			{
				if ((searchBuffer[index] == 0xd3) && (searchBuffer[index + 1] == 0xe9))
					break;
			}

			if (index >= (searchLength - 4))
				return false;

			songDataOffset = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + baseOffset;

			// Find out if filter effect is enabled or not
			for (; index < searchLength - 4; index += 2)
			{
				if ((searchBuffer[index] == 0x4c) && (searchBuffer[index + 1] == 0xdf))
					break;
			}

			if (index >= (searchLength - 4))
				return false;

			index -= 4;

			if (((searchBuffer[index] == 0x00) && (searchBuffer[index + 1] == 0xdf)) || ((searchBuffer[index] == 0x4e) && (searchBuffer[index + 1] == 0x71)))
				index -= 6;

			for (int i = 0; i < 3; i++)
			{
				if ((searchBuffer[index] != 0x61) || (searchBuffer[index + 1] != 00))
					break;

				int checkOffset = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2;

				if ((searchBuffer[checkOffset + 14] == 0x44) && (searchBuffer[checkOffset + 15] == 0x10))
					enableFilter = true;
				else if (((searchBuffer[checkOffset + 12] == 0x20) && (searchBuffer[checkOffset + 13] == 0x10)) || ((searchBuffer[checkOffset + 14] == 0x20) && (searchBuffer[checkOffset + 15] == 0x10)))
					enableMixing = true;

				index -= 4;
			}

			// Find rest of the offsets
			for (index = playNoteOffset; index < searchLength; index += 2)
			{
				if ((searchBuffer[index] == 0xd9) && (searchBuffer[index + 1] == 0xec))
					break;
			}

			if (index < (searchLength - 4))
				waveformListOffset = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + baseOffset;
			else
				index = playNoteOffset;

			for (; index < searchLength; index += 2)
			{
				if ((searchBuffer[index] == 0xeb) && ((searchBuffer[index + 1] == 0x81) || (searchBuffer[index + 1] == 0x49)))
					break;
			}

			if (index >= (searchLength - 4))
				return false;

			for (; index < searchLength; index += 2)
			{
				if ((searchBuffer[index] == 0xdd) && (searchBuffer[index + 1] == 0xee))
					break;
			}

			if (index >= (searchLength - 4))
				return false;

			waveformOffset = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + baseOffset;

			// Find period table to use
			for (index1 = index; index1 < searchLength; index1 += 2)
			{
				if ((searchBuffer[index1] == 0x16) && (searchBuffer[index1 + 1] == 0x2c))
					break;
			}

			if (index1 >= (searchLength - 4))
			{
				periods = Tables.Periods3;
				fineTuneMultiply = 0;
			}
			else if ((searchBuffer[index1 + 4] == 0xc6) && (searchBuffer[index1 + 5] == 0xfc))
			{
				periods = Tables.Periods1;
				fineTuneMultiply = 67;
			}
			else
			{
				periods = Tables.Periods2;
				fineTuneMultiply = 68;
			}

			// Find "PlaySample" function
			for (; index >= 0; index -= 2)
			{
				if ((searchBuffer[index] == 0x64) && (searchBuffer[index + 1] == 0x00))
					break;
			}

			if (index < 4)
				return false;

			if ((searchBuffer[index - 2] == 0x00) && (searchBuffer[index - 1] == 0x3c))
			{
				index = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2;

				specialSamples = new List<SpecialSampleInfo>();

				for (; index < searchLength; index += 2)
				{
					if ((searchBuffer[index] == 0x4e) && (searchBuffer[index + 1] == 0x75))
						break;

					if ((searchBuffer[index] == 0x0c) && (searchBuffer[index + 1] == 0x00))
					{
						uint instrumentNumber = (uint)((searchBuffer[index + 2] << 8) | searchBuffer[index + 3]);
						index += 8;

						if ((searchBuffer[index] != 0x4d) || (searchBuffer[index + 1] != 0xfa))
							return false;

						int startOffset = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2;

						if ((searchBuffer[index + 4] == 0xdd) && (searchBuffer[index + 5] == 0xfc))
						{
							startOffset += (searchBuffer[index + 6] << 24) | (searchBuffer[index + 7] << 16) | (searchBuffer[index + 8] << 8) | searchBuffer[index + 9];
							index += 6;
						}

						index += 10;

						if ((searchBuffer[index] != 0x33) || (searchBuffer[index + 1] != 0xfc))
							return false;

						int length = ((searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) * 2;

						specialSamples.Add(new SpecialSampleInfo
						{
							InstrumentNumber = instrumentNumber,
							SampleOffset = startOffset,
							Length = length
						});
					}
				}

				if (index >= (searchLength - 4))
					return false;
			}
			else
			{
				index = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2;

				for (; index < searchLength; index += 2)
				{
					if ((searchBuffer[index] == 0xd9) && (searchBuffer[index + 1] == 0xec))
						break;
				}

				if (index >= (searchLength - 4))
					return false;

				sampleOffset = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + baseOffset;

				// Find "no loop" special value
				for (; index < searchLength; index += 2)
				{
					if ((searchBuffer[index] == 0x0c) && (searchBuffer[index + 1] == 0xab))
						break;
				}

				if (index >= (searchLength - 4))
					return false;

				noLoopValue = (uint)((searchBuffer[index + 2] << 24) | (searchBuffer[index + 3] << 16) | (searchBuffer[index + 4] << 8) | searchBuffer[index + 5]);
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Find the counts of the different parts
		/// </summary>
		/********************************************************************/
		private void FindCounts(ModuleStream moduleStream, out int numberOfTracks, out int numberOfInstruments, out int numberOfWaveformList, out int numberOfWaveforms)
		{
			for (int i = 0; i < 4; i++)
			{
				moduleStream.Seek(sequenceOffsets[i], SeekOrigin.Begin);
				sequenceOffsets[i] = moduleStream.Read_B_INT32() + baseOffset;
			}

			moduleStream.Seek(trackDataOffset, SeekOrigin.Begin);
			trackDataOffset = moduleStream.Read_B_INT32() + baseOffset;

			moduleStream.Seek(trackOffsetTableOffset, SeekOrigin.Begin);
			trackOffsetTableOffset = moduleStream.Read_B_INT32() + baseOffset;

			moduleStream.Seek(instrumentOffset, SeekOrigin.Begin);
			instrumentOffset = moduleStream.Read_B_INT32() + baseOffset;

			if (waveformListOffset != 0)
			{
				moduleStream.Seek(waveformListOffset, SeekOrigin.Begin);
				waveformListOffset = moduleStream.Read_B_INT32() + baseOffset;
			}

			moduleStream.Seek(waveformOffset, SeekOrigin.Begin);
			waveformOffset = moduleStream.Read_B_INT32() + baseOffset;

			if (sampleOffset != 0)
			{
				moduleStream.Seek(sampleOffset, SeekOrigin.Begin);
				sampleOffset = moduleStream.Read_B_INT32() + baseOffset;
			}

			moduleStream.Seek(songDataOffset, SeekOrigin.Begin);
			songDataOffset = moduleStream.Read_B_INT32() + baseOffset;

			int[] sortedOffsets = new int[]
			{
				sequenceOffsets[0], sequenceOffsets[1], sequenceOffsets[2], sequenceOffsets[3],
				trackDataOffset, trackOffsetTableOffset,
				instrumentOffset,
				waveformListOffset, waveformOffset,
				sampleOffset,
				songDataOffset
			};
			Array.Sort(sortedOffsets);

			int FindOffsetIndex(int offset)
			{
				return Array.FindIndex(sortedOffsets, x => x == offset);
			}

			int index = FindOffsetIndex(trackOffsetTableOffset);
			if (index == (sortedOffsets.Length - 1))
			{
				// Last block of data, so count the track offsets by looking at them
				numberOfTracks = 1;		// Skip the first one, since it will always be 0

				moduleStream.Seek(trackOffsetTableOffset + 4, SeekOrigin.Begin);

				do
				{
					uint offset = moduleStream.Read_B_UINT32();
					if ((offset == 0) || (offset > moduleStream.Length))
						break;

					numberOfTracks++;
				}
				while (!moduleStream.EndOfStream);
			}
			else
				numberOfTracks = (sortedOffsets[index + 1] - sortedOffsets[index]) / 4;

			index = FindOffsetIndex(instrumentOffset);
			numberOfInstruments = (sortedOffsets[index + 1] - sortedOffsets[index]) / 32;
			if (numberOfInstruments > 63)
				numberOfInstruments = 63;

			index = FindOffsetIndex(waveformListOffset);
			numberOfWaveformList = (sortedOffsets[index + 1] - sortedOffsets[index]) / 16;

			index = FindOffsetIndex(waveformOffset);
			numberOfWaveforms = (sortedOffsets[index + 1] - sortedOffsets[index]) / 32;
		}



		/********************************************************************/
		/// <summary>
		/// Load the song data information
		/// </summary>
		/********************************************************************/
		private bool LoadSongData(ModuleStream moduleStream, out uint numberOfPositions)
		{
			moduleStream.Seek(songDataOffset, SeekOrigin.Begin);

			mix1Information = new MixInfo();
			mix2Information = new MixInfo();

			mix1Information.Source1 = moduleStream.Read_B_UINT32();
			mix2Information.Source1 = moduleStream.Read_B_UINT32();
			mix1Information.Source2 = moduleStream.Read_B_UINT32();
			mix2Information.Source2 = moduleStream.Read_B_UINT32();
			mix1Information.Destination = moduleStream.Read_B_UINT32();
			mix2Information.Destination = moduleStream.Read_B_UINT32();

			startNumberOfRows = moduleStream.Read_B_UINT32();
			numberOfPositions = moduleStream.Read_B_UINT32() - 1;
			
			startSpeed = moduleStream.Read_B_UINT32();

			mix1Information.Speed = moduleStream.Read_B_UINT32();
			mix2Information.Speed = moduleStream.Read_B_UINT32();

			if (moduleStream.EndOfStream)
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the tracks
		/// </summary>
		/********************************************************************/
		private bool LoadTracks(ModuleStream moduleStream, int numberOfTracks)
		{
			// Read track offset table
			int[] trackOffsetTable = new int[numberOfTracks];

			moduleStream.Seek(trackOffsetTableOffset, SeekOrigin.Begin);
			moduleStream.ReadArray_B_INT32s(trackOffsetTable, 0, numberOfTracks);

			if (moduleStream.EndOfStream)
				return false;

			tracks = new Track[numberOfTracks];

			for (int i = 0; i < numberOfTracks; i++)
			{
				tracks[i] = LoadSingleTrack(moduleStream, trackOffsetTable[i]);

				if (moduleStream.EndOfStream)
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single track
		/// </summary>
		/********************************************************************/
		private Track LoadSingleTrack(ModuleStream moduleStream, int trackOffset)
		{
			moduleStream.Seek(trackDataOffset + trackOffset, SeekOrigin.Begin);

			List<TrackRow> rows = new List<TrackRow>();
			int rowLine = 0;

			do
			{
				TrackRow row = new TrackRow();

				row.Note = moduleStream.Read_INT8();
				row.Instrument = moduleStream.Read_UINT8();
				row.Effect = moduleStream.Read_UINT8();
				row.EffectParam = moduleStream.Read_UINT8();
				row.Duration = moduleStream.Read_UINT8();

				if ((specialSamples != null) && (row.Instrument >= 60))
					row.Instrument = (byte)(instruments.Length - specialSamples.Count + (row.Instrument - 60));

				rows.Add(row);

				rowLine += row.Duration + 1;
			}
			while (rowLine < 64);

			return new Track
			{
				Rows = rows.ToArray()
			};
		}



		/********************************************************************/
		/// <summary>
		/// Load all the sequences
		/// </summary>
		/********************************************************************/
		private bool LoadSequences(ModuleStream moduleStream, uint numberOfPositions)
		{
			sequences = new Sequence[4][];

			for (int i = 0; i < 4; i++)
			{
				moduleStream.Seek(sequenceOffsets[i], SeekOrigin.Begin);

				sequences[i] = new Sequence[numberOfPositions];

				for (int j = 0; j < numberOfPositions; j++)
				{
					Sequence sequence = new Sequence();

					sequence.TrackNumber = moduleStream.Read_B_UINT32();
					moduleStream.Seek(1, SeekOrigin.Current);
					sequence.Transpose = moduleStream.Read_INT8();

					if (moduleStream.EndOfStream)
						return false;

					sequences[i][j] = sequence;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the instruments
		/// </summary>
		/********************************************************************/
		private bool LoadInstruments(ModuleStream moduleStream, int numberOfInstruments)
		{
			moduleStream.Seek(instrumentOffset, SeekOrigin.Begin);

			int extraInstruments = specialSamples == null ? 0 : specialSamples.Count;

			instruments = new Instrument[numberOfInstruments + extraInstruments];
			instrumentToSampleInfoMapping = new short[numberOfInstruments + extraInstruments];

			Dictionary<uint, short> samplesTaken = new Dictionary<uint, short>();
			short sampleInfoIndex = 0;

			for (short i = 0; i < numberOfInstruments; i++)
			{
				Instrument instr = new Instrument();

				instr.WaveformNumber = moduleStream.Read_B_UINT32();

				moduleStream.ReadInto(instr.Arpeggio, 0, 16);

				instr.AttackSpeed = moduleStream.Read_UINT8();
				instr.AttackMax = moduleStream.Read_UINT8();
				instr.DecaySpeed = moduleStream.Read_UINT8();
				instr.DecayMin = moduleStream.Read_UINT8();
				instr.SustainTime = moduleStream.Read_UINT8();

				moduleStream.Seek(1, SeekOrigin.Current);

				instr.ReleaseSpeed = moduleStream.Read_UINT8();
				instr.ReleaseMin = moduleStream.Read_UINT8();
				instr.PhaseShift = moduleStream.Read_UINT8();
				instr.PhaseSpeed = moduleStream.Read_UINT8();
				instr.FineTune = moduleStream.Read_UINT8();
				instr.PitchFall = moduleStream.Read_INT8();

				if (moduleStream.EndOfStream)
					return false;

				if (extraInstruments != 0)	// If there are extra instruments, we know which version of the player it is and therefore know, we need to move some information around
				{
					instr.PitchFall = (sbyte)instr.FineTune;
					instr.FineTune = 0;
				}
				else if (instr.FineTune > 6)
					instr.FineTune = 0;

				if (instr.WaveformNumber < 16)
					instrumentToSampleInfoMapping[i] = sampleInfoIndex++;
				else
				{
					if (samplesTaken.TryGetValue(instr.WaveformNumber, out short instrumentNumber))
						instrumentToSampleInfoMapping[i] = instrumentNumber;
					else
					{
						samplesTaken[instr.WaveformNumber] = sampleInfoIndex;
						instrumentToSampleInfoMapping[i] = sampleInfoIndex++;
					}
				}

				instruments[i] = instr;
			}

			// Add extra instruments
			if (specialSamples != null)
			{
				for (int i = 0; i < extraInstruments; i++)
				{
					SpecialSampleInfo sampleInfo = specialSamples[i];

					Instrument instr = new Instrument
					{
						WaveformNumber = sampleInfo.InstrumentNumber - 60 + 16,
						Volume = 64
					};

					instrumentToSampleInfoMapping[numberOfInstruments + i] = sampleInfoIndex++;
					instruments[numberOfInstruments + i] = instr;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load the waveform list and waveform data
		/// </summary>
		/********************************************************************/
		private bool LoadWaveforms(ModuleStream moduleStream, int numberOfWaveformList, int numberOfWaveforms)
		{
			if (waveformListOffset == 0)
			{
				// The module does not have any waveform offset list, so create one by ourselves since we need it
				waveformInfo = new byte[instruments.Length][];

				for (byte i = 1; i <= waveformInfo.Length; i++)
					waveformInfo[i - 1] = new byte[16] { i, 0x01, 0xff, 0x10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
			}
			else
			{
				moduleStream.Seek(waveformListOffset, SeekOrigin.Begin);

				waveformInfo = new byte[numberOfWaveformList][];

				for (int i = 0; i < numberOfWaveformList; i++)
				{
					byte[] waveInfo = new byte[16];

					int bytesRead = moduleStream.Read(waveInfo, 0, 16);

					if (bytesRead < 16)
						return false;

					waveformInfo[i] = waveInfo;
				}
			}

			moduleStream.Seek(waveformOffset, SeekOrigin.Begin);

			waveforms = new sbyte[numberOfWaveforms][];

			for (int i = 0; i < numberOfWaveforms; i++)
			{
				sbyte[] waveform = new sbyte[32];

				moduleStream.ReadSigned(waveform, 0, 32);
				if (moduleStream.EndOfStream)
					return false;

				waveforms[i] = waveform;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the samples
		/// </summary>
		/********************************************************************/
		private bool LoadSamples(ModuleStream moduleStream)
		{
			uint[] orderedWaveforms = instruments.Where(x => x.WaveformNumber >= 16).Select(x => x.WaveformNumber - 16).Distinct().OrderBy(x => x).ToArray();
			List<Sample> sampleList = new List<Sample>(orderedWaveforms.Length);

			if (sampleOffset == 0)
			{
				for (int i = 0; i < specialSamples.Count; i++)
				{
					SpecialSampleInfo specialSampleInfo = specialSamples[i];

					Sample sample = new Sample
					{
						Name = string.Empty,
						LoopStart = -1
					};

					moduleStream.Seek(specialSampleInfo.SampleOffset, SeekOrigin.Begin);
					sample.SampleData = moduleStream.ReadSampleData(i, specialSampleInfo.Length, out int readBytes);

					if (readBytes < (specialSampleInfo.Length - 32))
						return false;

					sampleList.Add(sample);
				}
			}
			else
			{
				Encoding encoding = EncoderCollection.Amiga;

				moduleStream.Seek(sampleOffset, SeekOrigin.Begin);

				uint sampleBaseOffset = moduleStream.Read_B_UINT32();
				long sampleInfoOffset = moduleStream.Position;

				long firstSamplePosition = sampleBaseOffset != 0 ? sampleBaseOffset : uint.MaxValue;

				byte[] name = new byte[20];

				for (int sampleNumber = 0, orderedIndex = 0; orderedIndex < orderedWaveforms.Length; sampleNumber++)
				{
					Sample sample = new Sample();

					if (sampleNumber == orderedWaveforms[orderedIndex])
					{
						long newPosition = orderedWaveforms[orderedIndex] * 32;
						if (newPosition < firstSamplePosition)
						{
							moduleStream.Seek(sampleInfoOffset + newPosition, SeekOrigin.Begin);

							uint startOffset = moduleStream.Read_B_UINT32();
							int loopStart = moduleStream.Read_B_INT32();
							uint endOffset = moduleStream.Read_B_UINT32();

							int bytesRead = moduleStream.Read(name, 0, 20);

							if (bytesRead < 20)
								return false;

							if ((loopStart == noLoopValue) || (loopStart >= endOffset) || (startOffset > loopStart))
								loopStart = -1;
							else
								loopStart -= (int)startOffset;

							int length = (int)(endOffset - startOffset);

							sample.LoopStart = loopStart;
							sample.Name = encoding.GetString(name);

							// Load sample data
							long samplePosition = sampleInfoOffset + sampleBaseOffset + startOffset;
							firstSamplePosition = Math.Min(firstSamplePosition, samplePosition);

							moduleStream.Seek(samplePosition, SeekOrigin.Begin);
							sample.SampleData = moduleStream.ReadSampleData((int)orderedWaveforms[orderedIndex], length, out int readBytes);

							if (readBytes < (length - 32))
								return false;
						}

						orderedIndex++;
					}

					sampleList.Add(sample);
				}
			}

			samples = sampleList.ToArray();

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player have allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			sequenceOffsets = null;

			sequences = null;
			tracks = null;
			instruments = null;
			waveformInfo = null;
			waveforms = null;
			samples = null;

			periods = null;

			mix1Information = null;
			mix2Information = null;

			instrumentToSampleInfoMapping = null;
			specialSamples = null;

			playingInfo = null;
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
				NumberOfRows = startNumberOfRows,
				Speed = startSpeed,
				SpeedCounter = startSpeed,

				NewTrack = false,
				LoopSong = false,

				CurrentRow = -1,
				CurrentPosition = 1,

				VoiceInfo = new VoiceInfo[4]
			};

			for (int i = 0; i < 4; i++)
			{
				VoiceInfo voiceInfo = new VoiceInfo
				{
					SequenceIndex = 0,
					RowIndex = 0,
					NotePeriod = 0x9999,
					BendTo = 0,
					BendSpeed = 0,
					NoteOffset = 0,
					ArpeggioIndex = 0,
					EnvelopeInProgress = EnvelopeState.Done,
					RowCount = 0,
					Volume = 0,
					SustainControl = 0,
					PitchControl = 0,
					PhaseIndex = 0,
					PhaseSpeed = 0,
					PitchFallControl = 0,
					WaveIndex = 0,
					WaveformNumber = 0,
					WaveSpeed = 0,
					LoopControl = false
				};

				voiceInfo.CurrentSequence = sequences[i][0];
				voiceInfo.CurrentTrack = tracks[voiceInfo.CurrentSequence.TrackNumber];

				int instrNumber = voiceInfo.CurrentTrack.Rows[0].Instrument - 1;
				if ((instrNumber >= 0) && (instrNumber < instruments.Length))
				{
					voiceInfo.CurrentInstrument = instruments[instrNumber];
					voiceInfo.InstrumentNumber = instrNumber;
				}

				playingInfo.VoiceInfo[i] = voiceInfo;
			}

			endReached = false;
		}



		/********************************************************************/
		/// <summary>
		/// Play the next row
		/// </summary>
		/********************************************************************/
		private void PlayNote(Sequence[] sequence, VoiceInfo voiceInfo, IChannel channel)
		{
			sbyte[] sampleAddress = null;
			int loopStart = -1;
			bool sampleAddressSet = false;
			uint period = 0;
			Instrument instr;

			if (playingInfo.SpeedCounter == 0)
			{
				if (playingInfo.NewTrack)
				{
					voiceInfo.RowCount = 0;
					voiceInfo.SequenceIndex++;

					if (playingInfo.LoopSong)
						voiceInfo.SequenceIndex = 0;

					voiceInfo.CurrentSequence = sequence[voiceInfo.SequenceIndex];
					voiceInfo.CurrentTrack = tracks[voiceInfo.CurrentSequence.TrackNumber];
					voiceInfo.RowIndex = 0;

					ShowTracks();
				}

				if (voiceInfo.RowCount == 0)
				{
					void PlaySample(uint sampleNumber)
					{
						if (voiceInfo.LoopControl)
							channel.Mute();

						Sample sample = samples[sampleNumber - 16];
						sampleAddress = sample.SampleData;
						sampleAddressSet = true;
						loopStart = sample.LoopStart;

						voiceInfo.LoopControl = true;
					}

					TrackRow row = voiceInfo.CurrentTrack.Rows[voiceInfo.RowIndex];

					if (row.Instrument != 0)
					{
						instr = instruments[row.Instrument - 1];

						if (voiceInfo.LoopControl)
						{
							channel.Mute();
							voiceInfo.LoopControl = false;
						}

						uint waveformNumber = instr.WaveformNumber;
						if (waveformNumber >= 16)
							PlaySample(waveformNumber);

						voiceInfo.WaveIndex = 0;
						voiceInfo.WaveformNumber = (byte)waveformNumber;

						if (!sampleAddressSet && (waveformNumber != 0))
						{
							byte[] waveInfo = waveformInfo[voiceInfo.WaveformNumber - 1];
							voiceInfo.WaveSpeed = waveInfo[1];

							sampleAddress = waveforms[waveInfo[0] - 1];
							loopStart = 0;
						}

						voiceInfo.CurrentInstrument = instr;
						voiceInfo.InstrumentNumber = row.Instrument - 1;

						voiceInfo.EnvelopeInProgress = 0;
						voiceInfo.PitchFallControl = 0;
						voiceInfo.PitchControl = 0;
						voiceInfo.RowCount = row.Duration;
					}
					else
					{
						if (row.Note != 0)
						{
							voiceInfo.RowCount = row.Duration;

							if (voiceInfo.LoopControl && (row.Note != -1))
								PlaySample(voiceInfo.WaveformNumber);
						}
					}

					sbyte note = row.Note;
					if (note != 0)
					{
						voiceInfo.RowCount = row.Duration;

						if (note != -1)
						{
							instr = voiceInfo.CurrentInstrument;

							sbyte transpose = voiceInfo.CurrentSequence.Transpose;
							note += transpose;

							int index = note + instr.FineTune * fineTuneMultiply + 1;
							period = index >= periods.Length ? (ushort)0 : periods[index];
							voiceInfo.NotePeriod = (ushort)period;
							voiceInfo.NoteOffset = note;

							voiceInfo.EnvelopeInProgress = 0;
							voiceInfo.Volume = 0;
							voiceInfo.SustainControl = 0;
							voiceInfo.PitchControl = 0;
							voiceInfo.PitchFallControl = 0;
							voiceInfo.BendSpeed = 0;
							voiceInfo.PhaseSpeed = instr.PhaseSpeed;

							switch (row.Effect)
							{
								case 0:
								{
									if (row.EffectParam != 0)
									{
										instr.AttackMax = row.EffectParam;
										instr.AttackSpeed = row.EffectParam;

										voiceInfo.WaveSpeed = 0;
									}
									break;
								}

								case 2:
								{
									playingInfo.Speed = row.EffectParam;
									voiceInfo.WaveSpeed = 0;

									ShowSpeed();
									break;
								}

								case 3:
								{
									playingInfo.NumberOfRows = row.EffectParam;
									voiceInfo.WaveSpeed = 0;
									break;
								}

								default:
								{
									voiceInfo.BendTo = (byte)(row.Effect + transpose);
									voiceInfo.BendSpeed = (sbyte)row.EffectParam;
									break;
								}
							}
						}
					}

					voiceInfo.RowIndex++;
				}
				else
					voiceInfo.RowCount--;
			}

			instr = voiceInfo.CurrentInstrument;
			if ((instr != null) && (voiceInfo.WaveformNumber > 0))
			{
				int volume = ProcessEnvelope(voiceInfo);
				period = DoArpeggio(voiceInfo);
				DoBending(voiceInfo);
				DoPhaseShift(voiceInfo);

				voiceInfo.PitchFallControl = (ushort)(voiceInfo.PitchFallControl - instr.PitchFall);
				voiceInfo.NotePeriod += voiceInfo.PitchFallControl;

				if (!voiceInfo.LoopControl)
				{
					if (voiceInfo.WaveSpeed == 0)
					{
						if (voiceInfo.WaveIndex != 16)
						{
							byte[] waveInfo = waveformInfo[voiceInfo.WaveformNumber - 1];

							if (waveInfo[voiceInfo.WaveIndex] == 0xff)
								voiceInfo.WaveIndex = (byte)(waveInfo[voiceInfo.WaveIndex + 1] & 0xfe);
							else
							{
								sampleAddress = waveforms[waveInfo[voiceInfo.WaveIndex] - 1];
								loopStart = 0;

								voiceInfo.WaveSpeed = waveInfo[voiceInfo.WaveIndex + 1];
								voiceInfo.WaveIndex += 2;
							}
						}
					}
					else
						voiceInfo.WaveSpeed--;
				}

				if (period != 0)
					channel.SetAmigaPeriod(voiceInfo.NotePeriod);

				if (volume != 0)
					channel.SetVolume((ushort)volume);

				if (sampleAddress != null)
				{
					channel.PlaySample(instrumentToSampleInfoMapping[voiceInfo.InstrumentNumber], sampleAddress, 0, (uint)sampleAddress.Length);

					if (loopStart != -1)
						channel.SetLoop((uint)loopStart, (uint)(sampleAddress.Length - loopStart));
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Process the envelope
		/// </summary>
		/********************************************************************/
		private int ProcessEnvelope(VoiceInfo voiceInfo)
		{
			Instrument instr = voiceInfo.CurrentInstrument;
			if (instr.Volume != 0)
				return instr.Volume * 4;

			int volume = 0;

			switch (voiceInfo.EnvelopeInProgress)
			{
				case EnvelopeState.Attack:
				{
					volume = voiceInfo.Volume + instr.AttackSpeed;

					if (volume > instr.AttackMax)
					{
						volume = instr.AttackMax;
						voiceInfo.EnvelopeInProgress = EnvelopeState.Decay;
					}

					voiceInfo.Volume = (byte)volume;
					break;
				}

				case EnvelopeState.Decay:
				{
					volume = voiceInfo.Volume - instr.DecaySpeed;

					if (volume <= instr.DecayMin)
					{
						volume = instr.DecayMin;
						voiceInfo.SustainControl = instr.SustainTime;
						voiceInfo.EnvelopeInProgress = EnvelopeState.Sustain;
					}

					voiceInfo.Volume = (byte)volume;
					break;
				}

				case EnvelopeState.Sustain:
				{
					volume = voiceInfo.Volume;

					voiceInfo.SustainControl--;
					if (voiceInfo.SustainControl == 0)
						voiceInfo.EnvelopeInProgress = EnvelopeState.Release;

					break;
				}

				case EnvelopeState.Release:
				{
					volume = voiceInfo.Volume - instr.ReleaseSpeed;

					if (volume <= instr.ReleaseMin)
					{
						volume = instr.ReleaseMin;
						voiceInfo.EnvelopeInProgress = EnvelopeState.Done;
					}

					voiceInfo.Volume = (byte)volume;
					break;
				}
			}

			return volume;
		}



		/********************************************************************/
		/// <summary>
		/// Handle the arpeggio
		/// </summary>
		/********************************************************************/
		private uint DoArpeggio(VoiceInfo voiceInfo)
		{
			Instrument instr = voiceInfo.CurrentInstrument;

			voiceInfo.ArpeggioIndex++;
			voiceInfo.ArpeggioIndex &= 0x0f;

			byte arp = instr.Arpeggio[voiceInfo.ArpeggioIndex];
			int index = voiceInfo.NoteOffset + arp + instr.FineTune * fineTuneMultiply;
			ushort period = index >= periods.Length ? (ushort)0 : periods[index];
			voiceInfo.NotePeriod = period;

			return period;
		}



		/********************************************************************/
		/// <summary>
		/// Handle bending
		/// </summary>
		/********************************************************************/
		private void DoBending(VoiceInfo voiceInfo)
		{
			if (voiceInfo.BendSpeed != 0)
			{
				short bendSpeed = (short)-voiceInfo.BendSpeed;
				int index = voiceInfo.BendTo + voiceInfo.CurrentInstrument.FineTune * fineTuneMultiply;
				if (index >= periods.Length)
				{
					voiceInfo.BendSpeed = 0;
					return;
				}

				ushort bendTo = periods[index];

				if (bendSpeed < 0)
				{
					voiceInfo.PitchControl = (ushort)(voiceInfo.PitchControl + bendSpeed);
					voiceInfo.NotePeriod += voiceInfo.PitchControl;

					if (voiceInfo.NotePeriod < bendTo)
					{
						voiceInfo.NoteOffset = (sbyte)voiceInfo.BendTo;
						voiceInfo.NotePeriod = bendTo;
						voiceInfo.PitchControl = 0;
						voiceInfo.BendSpeed = 0;
					}
				}
				else
				{
					voiceInfo.PitchControl = (ushort)(voiceInfo.PitchControl + bendSpeed);
					voiceInfo.NotePeriod += voiceInfo.PitchControl;

					if (voiceInfo.NotePeriod > bendTo)
					{
						voiceInfo.NoteOffset = (sbyte)voiceInfo.BendTo;
						voiceInfo.NotePeriod = bendTo;
						voiceInfo.PitchControl = 0;
						voiceInfo.BendSpeed = 0;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle phase shift
		/// </summary>
		/********************************************************************/
		private void DoPhaseShift(VoiceInfo voiceInfo)
		{
			Instrument instr = voiceInfo.CurrentInstrument;

			if (instr.PhaseShift != 0)
			{
				if (voiceInfo.PhaseSpeed == 0)
				{
					if ((instr.PhaseShift - 1) < waveforms.Length)
					{
						sbyte[] waveform = waveforms[instr.PhaseShift - 1];

						voiceInfo.PhaseIndex++;
						voiceInfo.PhaseIndex &= 0x1f;

						voiceInfo.NotePeriod = (ushort)(voiceInfo.NotePeriod + (waveform[voiceInfo.PhaseIndex] / 4));
					}
				}
				else
					voiceInfo.PhaseSpeed--;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Do the different waveform mixings
		/// </summary>
		/********************************************************************/
		private void DoWaveformMixing(MixInfo mixInfo, MixPlayingInfo mixPlayingInfo)
		{
			if (enableMixing)
			{
				if (mixInfo.Speed != 0)
				{
					if (mixPlayingInfo.Counter == 0)
					{
						mixPlayingInfo.Counter = mixInfo.Speed;

						mixPlayingInfo.Position++;
						mixPlayingInfo.Position &= 0x1f;

						if ((mixInfo.Source1 <= waveforms.Length) && (mixInfo.Source2 <= waveforms.Length) && (mixInfo.Destination <= waveforms.Length))
						{
							sbyte[] source1 = waveforms[mixInfo.Source1 - 1];
							sbyte[] source2 = waveforms[mixInfo.Source2 - 1];
							sbyte[] dest = waveforms[mixInfo.Destination - 1];

							uint pos = mixPlayingInfo.Position;

							for (int i = 31; i >= 0; i--)
							{
								dest[i] = (sbyte)((source1[i] + source2[pos]) / 2);

								pos--;
								pos &= 0x1f;
							}
						}
					}

					mixPlayingInfo.Counter--;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Add filter effect on the waveform
		/// </summary>
		/********************************************************************/
		private void DoFilterEffect()
		{
			if (enableFilter)
				waveforms[0][playingInfo.Mix1PlayingInfo.Position] = (sbyte)-waveforms[0][playingInfo.Mix1PlayingInfo.Position];
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current song positions
		/// </summary>
		/********************************************************************/
		private void ShowSongPositions()
		{
			OnModuleInfoChanged(InfoPositionLine, (playingInfo.CurrentPosition - 1).ToString());
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
				sb.Append(playingInfo.VoiceInfo[i].CurrentSequence.TrackNumber);
				sb.Append(", ");
			}

			sb.Remove(sb.Length - 2, 2);

			return sb.ToString();
		}
		#endregion
	}
}
