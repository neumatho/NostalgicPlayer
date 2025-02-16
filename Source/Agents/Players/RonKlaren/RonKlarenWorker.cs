/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.RonKlaren.Containers;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.RonKlaren
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class RonKlarenWorker : ModulePlayerWithSubSongDurationAgentBase
	{
		private const int AmigaExecutableHunkSize = 32;
		private const int HeaderSize = 32;
		private const int MinFileSize = 0xa40;

		private ushort ciaValue;
		private int currentSong;

		private SongInfo[] subSongs;
		private byte[][] tracks;
		private sbyte[][] arpeggios;
		private sbyte[][] vibratos;
		private Instrument[] instruments;
		private Sample[] samples;
		private sbyte[][] sampleData;

		private GlobalPlayingInfo playingInfo;
		private VoiceInfo[] voices;

		private const int InfoPositionLine = 3;
		private const int InfoTrackLine = 4;

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => [ "rk" ];



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			if (moduleStream.Length < MinFileSize)
				return AgentResult.Unknown;

			// Check for Amiga executable hunks
			moduleStream.Seek(0, SeekOrigin.Begin);

			if (moduleStream.Read_B_UINT32() != 0x3f3)
				return AgentResult.Unknown;

			// Check the identifier
			byte[] buf = new byte[23];

			moduleStream.Seek(40, SeekOrigin.Begin);
			moduleStream.ReadInto(buf, 0, buf.Length);

			if (Encoding.ASCII.GetString(buf, 0, 23) != "RON_KLAREN_SOUNDMODULE!")
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
				// Number of positions
				case 0:
				{
					description = Resources.IDS_RK_INFODESCLINE0;
					value = FormatPositionLengths();
					break;
				}

				// Used tracks
				case 1:
				{
					description = Resources.IDS_RK_INFODESCLINE1;
					value = tracks.Length.ToString();
					break;
				}

				// Used samples
				case 2:
				{
					description = Resources.IDS_RK_INFODESCLINE2;
					value = (instruments.Length - 1).ToString();
					break;
				}

				// Playing positions
				case 3:
				{
					description = Resources.IDS_RK_INFODESCLINE3;
					value = FormatPositions();
					break;
				}

				// Playing tracks
				case 4:
				{
					description = Resources.IDS_RK_INFODESCLINE4;
					value = FormatTracks();
					break;
				}

				// Current tempo (Hz)
				case 5:
				{
					description = Resources.IDS_RK_INFODESCLINE5;
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

				// Load the player code, which is used to search in to find different information
				byte[] searchBuffer = new byte[MinFileSize];

				moduleStream.Seek(AmigaExecutableHunkSize, SeekOrigin.Begin);
				moduleStream.ReadInto(searchBuffer, 0, searchBuffer.Length);

				if (!FindNumberOfSubSongs(searchBuffer, out int numberOfSubSongs))
				{
					errorMessage = Resources.IDS_RK_ERR_LOADING_INFO;
					return AgentResult.Error;
				}

				if (!FindSongSpeedAndIrqOffset(searchBuffer, out uint irqOffset))
				{
					errorMessage = Resources.IDS_RK_ERR_LOADING_INFO;
					return AgentResult.Error;
				}

				if (!FindSubSongInfo(searchBuffer, moduleStream.Length, irqOffset, out uint subSongOffset))
				{
					errorMessage = Resources.IDS_RK_ERR_LOADING_INFO;
					return AgentResult.Error;
				}

				if (!FindInstrumentAndArpeggioStartOffsets(searchBuffer, moduleStream.Length, out uint instrumentOffset, out uint arpeggioOffset))
				{
					errorMessage = Resources.IDS_RK_ERR_LOADING_INFO;
					return AgentResult.Error;
				}

				uint[,] subSongTrackOffsets = LoadSubSongInfo(moduleStream, ref numberOfSubSongs, subSongOffset);
				if (subSongTrackOffsets == null)
				{
					errorMessage = Resources.IDS_RK_ERR_LOADING_SUBSONG;
					return AgentResult.Error;
				}

				if (!LoadTrackLists(moduleStream, numberOfSubSongs, subSongTrackOffsets))
				{
					errorMessage = Resources.IDS_RK_ERR_LOADING_POSITION_LIST;
					return AgentResult.Error;
				}

				if (!LoadTracks(moduleStream))
				{
					errorMessage = Resources.IDS_RK_ERR_LOADING_TRACKS;
					return AgentResult.Error;
				}

				FindMaxNumberUsedByDifferentEffects(out int instrumentCount, out int arpeggionCount);

				if (!LoadArpeggios(moduleStream, arpeggioOffset, arpeggionCount))
				{
					errorMessage = Resources.IDS_RK_ERR_LOADING_ARPEGGIOS;
					return AgentResult.Error;
				}

				if (!LoadInstruments(moduleStream, instrumentOffset, instrumentCount))
				{
					errorMessage = Resources.IDS_RK_ERR_LOADING_INSTRUMENTINFO;
					return AgentResult.Error;
				}

				if (!LoadVibratos(moduleStream))
				{
					errorMessage = Resources.IDS_RK_ERR_LOADING_VIBRATOS;
					return AgentResult.Error;
				}

				if (!LoadSamples(moduleStream))
				{
					errorMessage = Resources.IDS_RK_ERR_LOADING_SAMPLEINFO;
					return AgentResult.Error;
				}

				if (!LoadSampleData(moduleStream))
				{
					errorMessage = Resources.IDS_RK_ERR_LOADING_SAMPLES;
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

			// Remember the song number
			currentSong = songNumber;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public override void Play()
		{
			if (playingInfo.SetupNewSubSong)
				SwitchSubSong();
			else
			{
				for (int i = 0; i < 4; i++)
					ProcessVoice(voices[i], VirtualChannels[i]);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return information about sub-songs
		/// </summary>
		/********************************************************************/
		public override SubSongInfo SubSongs => new SubSongInfo(subSongs.Length, 0);



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

				for (int j = 0; j < 6 * 12 - 2; j++)
					frequencies[1 * 12 + j] = PeriodToFrequency(Tables.Periods[j]);

				// The first instrument is always the "empty" instrument, so skip it
				for (int i = 1; i < instruments.Length; i++)
				{
					Instrument instr = instruments[i];
					Sample sample = samples[instr.SampleNumber];

					SampleInfo sampleInfo = new SampleInfo
					{
						Name = string.Empty,
						Flags = SampleInfo.SampleFlag.None,
						Volume = 256,
						Panning = -1,
						Sample = sampleData[sample.SampleNumber],
						LoopStart = 0,
						LoopLength = 0,
						NoteFrequencies = frequencies
					};

					if (instr.Type == InstrumentType.Synthesis)
					{
						sampleInfo.Type = SampleInfo.SampleType.Synthesis;
						sampleInfo.Length = 0;
					}
					else
					{
						sampleInfo.Type = SampleInfo.SampleType.Sample;
						sampleInfo.Length = sample.LengthInWords * 2U;
						
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
			return new Snapshot(playingInfo, voices, instruments,
				instruments.Where(x => x.Type == InstrumentType.Synthesis)
					.Select(x => sampleData[samples[x.SampleNumber].SampleNumber])
					.ToArray());
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
			Snapshot clonedSnapshot = new Snapshot(currentSnapshot.PlayingInfo, currentSnapshot.Voices, currentSnapshot.Instruments, currentSnapshot.SynthSamples);

			playingInfo = clonedSnapshot.PlayingInfo;
			voices = clonedSnapshot.Voices;
			instruments = clonedSnapshot.Instruments;

			for (int i = 0, j = 0; i < instruments.Length; i++)
			{
				if (instruments[i].Type == InstrumentType.Synthesis)
					sampleData[samples[instruments[i].SampleNumber].SampleNumber] = clonedSnapshot.SynthSamples[j++];
			}

			UpdateModuleInformation();

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Try to find the number of sub-songs the module contains
		/// </summary>
		/********************************************************************/
		private bool FindNumberOfSubSongs(byte[] searchBuffer, out int numberOfSubSongs)
		{
			int searchLength = searchBuffer.Length;
			int index;

			numberOfSubSongs = 0;

			// Skip all the JMP instructions
			for (index = HeaderSize; index < (searchLength - 6); index += 6)
			{
				if ((searchBuffer[index] != 0x4e) || (searchBuffer[index + 1] != 0xf9))
					break;
			}

			if (index >= (searchLength - 6))
				return false;

			for (; index < (searchLength - 6); index += 8)
			{
				if ((searchBuffer[index] != 0x30) || (searchBuffer[index + 1] != 0x3c))
					break;

				numberOfSubSongs++;
			}

			return numberOfSubSongs != 0;
		}



		/********************************************************************/
		/// <summary>
		/// Try to find the song speed and the offset to the IRQ routine
		/// </summary>
		/********************************************************************/
		private bool FindSongSpeedAndIrqOffset(byte[] searchBuffer, out uint irqOffset)
		{
			int searchLength = searchBuffer.Length;
			int index = HeaderSize;

			int initOffset = 0;
			irqOffset = 0;

			int i;
			for (i = 0; i < 2; i++)
			{
				if ((searchBuffer[index] != 0x4e) || (searchBuffer[index + 1] != 0xf9))
					return false;

				initOffset = (searchBuffer[index + 2] << 24) | (searchBuffer[index + 3] << 16) | (searchBuffer[index + 4] << 8) | searchBuffer[index + 5];

				if ((initOffset < 0) || (initOffset >= searchLength))
					return false;

				if (((searchBuffer[initOffset] == 0x61) && (searchBuffer[initOffset + 1] == 0x00)) || ((searchBuffer[initOffset] == 0x33) && (searchBuffer[initOffset + 1] == 0xfc)))
					break;

				index += 6;
			}

			if (i == 2)
				return CheckForVBlankPlayer(searchBuffer, out irqOffset);

			index = initOffset;

			byte ciaLoValue = 0, ciaHiValue = 0;

			for (; index < (searchLength - 10); index += 2)
			{
				if ((searchBuffer[index] == 0x4e) && (searchBuffer[index + 1] == 0x75))
					break;

				if ((searchBuffer[index] == 0x13) && (searchBuffer[index + 1] == 0xfc))
				{
					byte value = searchBuffer[index + 3];
					uint adr = (uint)((searchBuffer[index + 4] << 24) | (searchBuffer[index + 5] << 16) | (searchBuffer[index + 6] << 8) | searchBuffer[index + 7]);
					index += 6;

					if (adr == 0xbfd400)
						ciaLoValue = value;
					else if (adr == 0xbfd500)
						ciaHiValue = value;
				}
				else if ((searchBuffer[index] == 0x23) && (searchBuffer[index + 1] == 0xfc))
				{
					uint adr = (uint)((searchBuffer[index + 2] << 24) | (searchBuffer[index + 3] << 16) | (searchBuffer[index + 4] << 8) | searchBuffer[index + 5]);
					uint destAdr = (uint)((searchBuffer[index + 6] << 24) | (searchBuffer[index + 7] << 16) | (searchBuffer[index + 8] << 8) | searchBuffer[index + 9]);
					index += 8;

					if (destAdr == 0x00000078)
						irqOffset = adr;
				}
			}

			ciaValue = (ushort)((ciaHiValue << 8) | ciaLoValue);

			return (irqOffset != 0) && (irqOffset < searchLength) && (ciaValue != 0);
		}



		/********************************************************************/
		/// <summary>
		/// Try to find the song speed and the offset to the IRQ routine
		/// in a VBlank player
		/// </summary>
		/********************************************************************/
		private bool CheckForVBlankPlayer(byte[] searchBuffer, out uint irqOffset)
		{
			int searchLength = searchBuffer.Length;
			int index = HeaderSize + 6;

			irqOffset = 0;

			if ((searchBuffer[index] != 0x4e) || (searchBuffer[index + 1] != 0xf9))
				return false;

			int playOffset = (searchBuffer[index + 2] << 24) | (searchBuffer[index + 3] << 16) | (searchBuffer[index + 4] << 8) | searchBuffer[index + 5];

			if ((playOffset < 0) || (playOffset >= searchLength))
				return false;

			index = playOffset;

			if ((searchBuffer[index] != 0x41) || (searchBuffer[index + 1] != 0xfa))
				return false;

			irqOffset = (uint)playOffset;
			ciaValue = 14187;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Try to find the sub-song information
		/// </summary>
		/********************************************************************/
		private bool FindSubSongInfo(byte[] searchBuffer, long moduleLength, uint irqOffset, out uint subSongInfoOffset)
		{
			int searchLength = searchBuffer.Length;
			int index;

			subSongInfoOffset = 0;

			// First find where the global address is set into the A0 register
			for (index = (int)irqOffset; index < (searchLength - 2); index += 2)
			{
				if ((searchBuffer[index] == 0x41) && (searchBuffer[index + 1] == 0xfa))
					break;
			}

			if (index >= (searchLength - 2))
				return false;

			uint globalOffset = (uint)(((searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2);
			index += 4;

			if (globalOffset >= moduleLength)
				return false;

			// Find where the sub-song is initialized
			for (; index < (searchLength - 12); index += 2)
			{
				if ((searchBuffer[index] == 0x4e) && ((searchBuffer[index + 1] == 0x73) || (searchBuffer[index + 1] == 0x75)))
					return false;

				if ((searchBuffer[index] == 0x02) && (searchBuffer[index + 1] == 0x40) && (searchBuffer[index + 2] == 0x00) && (searchBuffer[index + 3] == 0x0f) &&
					(searchBuffer[index + 4] == 0x53) && (searchBuffer[index + 5] == 0x40) &&
					(searchBuffer[index + 6] == 0xe9) && (searchBuffer[index + 7] == 0x48) &&
					(searchBuffer[index + 8] == 0x47) && (searchBuffer[index + 9] == 0xf0))
				{
					break;
				}
			}

			if (index >= (searchLength - 12))
				return false;

			subSongInfoOffset = globalOffset + (uint)((searchBuffer[index + 10] << 8) | searchBuffer[index + 11]) + AmigaExecutableHunkSize;

			return subSongInfoOffset < moduleLength;
		}



		/********************************************************************/
		/// <summary>
		/// Try to find the instrument information and arpeggio tables
		/// </summary>
		/********************************************************************/
		private bool FindInstrumentAndArpeggioStartOffsets(byte[] searchBuffer, long moduleLength, out uint instrumentOffset, out uint arpeggioOffset)
		{
			int searchLength = searchBuffer.Length;
			int index;

			instrumentOffset = 0;
			arpeggioOffset = 0;

			for (index = HeaderSize; index < (searchLength - 4); index += 2)
			{
				if ((searchBuffer[index] == 0x0c) && (searchBuffer[index + 1] == 0x12) && (searchBuffer[index + 2] == 0x00))
				{
					if (searchBuffer[index + 3] == 0x82)
					{
						for (; index < (searchLength - 4); index += 2)
						{
							if ((searchBuffer[index] == 0x49) && (searchBuffer[index + 1] == 0xfa))
							{
								instrumentOffset = (uint)(((searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2) + AmigaExecutableHunkSize;
								break;
							}
						}
					}

					if (searchBuffer[index + 3] == 0x80)
					{
						for (; index < (searchLength - 4); index += 2)
						{
							if ((searchBuffer[index] == 0x49) && (searchBuffer[index + 1] == 0xfa))
							{
								arpeggioOffset = (uint)(((searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2) + AmigaExecutableHunkSize;
								break;
							}
						}
					}
				}

				if ((instrumentOffset != 0) && (arpeggioOffset != 0))
					break;
			}

			if (index >= (searchLength - 4))
				return false;

			return (instrumentOffset < moduleLength) && (arpeggioOffset < moduleLength);
		}



		/********************************************************************/
		/// <summary>
		/// Load all the track list offsets for each sub-song
		/// </summary>
		/********************************************************************/
		private uint[,] LoadSubSongInfo(ModuleStream moduleStream, ref int numberOfSubSongs, uint subSongInfoOffset)
		{
			moduleStream.Seek(subSongInfoOffset, SeekOrigin.Begin);

			uint[,] subSongTrackOffsets = new uint[numberOfSubSongs, 4];

			for (int i = 0; i < numberOfSubSongs; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					subSongTrackOffsets[i, j] = moduleStream.Read_B_UINT32() + AmigaExecutableHunkSize;

					if (subSongTrackOffsets[i, j] >= moduleStream.Length)
					{
						numberOfSubSongs = i;

						return subSongTrackOffsets;
					}
				}

				if (moduleStream.EndOfStream)
					return null;
			}

			return subSongTrackOffsets;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the track lists for each sub-song
		/// </summary>
		/********************************************************************/
		private bool LoadTrackLists(ModuleStream moduleStream, int numberOfSubSongs, uint[,] subSongTrackOffsets)
		{
			subSongs = new SongInfo[numberOfSubSongs];

			for (int i = 0; i < numberOfSubSongs; i++)
			{
				SongInfo songInfo = new SongInfo();

				for (int j = 0; j < 4; j++)
				{
					uint trackListOffset = subSongTrackOffsets[i, j];

					moduleStream.Seek(trackListOffset, SeekOrigin.Begin);

					Track[] trackList = LoadSingleTrackList(moduleStream);
					if (trackList == null)
						return false;

					songInfo.Positions[j] = new PositionList
					{
						Tracks = trackList
					};
				}

				subSongs[i] = songInfo;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single track list and return it
		/// </summary>
		/********************************************************************/
		private Track[] LoadSingleTrackList(ModuleStream moduleStream)
		{
			List<Track> trackList = new List<Track>();

			for (;;)
			{
				int trackOffset = moduleStream.Read_B_INT32();
				if (trackOffset < 0)
					break;

				moduleStream.Seek(2, SeekOrigin.Current);
				short transpose = moduleStream.Read_B_INT16();

				moduleStream.Seek(2, SeekOrigin.Current);
				ushort repeatTimes = moduleStream.Read_B_UINT16();

				if (moduleStream.EndOfStream)
					return null;

				trackList.Add(new Track
				{
					TrackNumber = trackOffset + AmigaExecutableHunkSize,  // This will be converted later on to a real track number
					Transpose = transpose,
					NumberOfRepeatTimes = repeatTimes
				});
			}

			return trackList.ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Load all the tracks and fix the track offsets
		/// </summary>
		/********************************************************************/
		private bool LoadTracks(ModuleStream moduleStream)
		{
			// Load all the track data into this dictionary.
			// The key is the track offset and the value is the track data
			Dictionary<int, byte[]> trackData = new Dictionary<int, byte[]>();

			foreach (SongInfo songInfo in subSongs)
			{
				for (int j = 0; j < 4; j++)
				{
					PositionList positionList = songInfo.Positions[j];

					foreach (Track track in positionList.Tracks)
					{
						if (!trackData.ContainsKey(track.TrackNumber))
						{
							moduleStream.Seek(track.TrackNumber, SeekOrigin.Begin);

							byte[] data = LoadSingleTrack(moduleStream);
							if (data == null)
								return false;

							trackData.Add(track.TrackNumber, data);
						}
					}
				}
			}

			// Now convert the track offset to track numbers in the track lists
			tracks = BuildOffsetToIndexLists(trackData, out Dictionary<int, int> offsetToIndexMap);

			foreach (SongInfo songInfo in subSongs)
			{
				for (int j = 0; j < 4; j++)
				{
					PositionList positionList = songInfo.Positions[j];

					foreach (Track track in positionList.Tracks)
						track.TrackNumber = offsetToIndexMap[track.TrackNumber];
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single track and return it
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

				if (byt == 0xff)
					break;

				int effectCount = FindEffectByteCount((Effect)byt);

				for (; effectCount > 0; effectCount--)
					trackBytes.Add(moduleStream.Read_UINT8());
			}

			return trackBytes.ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Find out how many bytes the given effect uses after the effect
		/// itself. -1 means stop parsing
		/// </summary>
		/********************************************************************/
		private int FindEffectByteCount(Effect effect)
		{
			if ((int)effect < 128)
				return 1;

			switch (effect)
			{
				case Effect.SetArpeggio:
				case Effect.SetInstrument:
				case Effect.ChangeAdsrSpeed:
					return 1;

				case Effect.SetPortamento:
					return 3;

				case Effect.EndSong:
				case Effect.EndOfTrack:
					return 0;
			}

			throw new Exception($"Invalid track byte detected ({(byte)effect})");
		}



		/********************************************************************/
		/// <summary>
		/// Find the max number of instruments and arpeggios by scanning
		/// the track data
		/// </summary>
		/********************************************************************/
		private void FindMaxNumberUsedByDifferentEffects(out int instrumentCount, out int arpeggioCount)
		{
			instrumentCount = 0;
			arpeggioCount = 0;

			foreach (byte[] track in tracks)
			{
				for (int i = 0; i < track.Length; i++)
				{
					Effect effect = (Effect)track[i];

					if (effect == Effect.SetInstrument)
						instrumentCount = Math.Max(instrumentCount, track[i + 1] + 1);
					else if (effect == Effect.SetArpeggio)
						arpeggioCount = Math.Max(arpeggioCount, track[i + 1] + 1);

					i += FindEffectByteCount(effect);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Load the arpeggio tables
		/// </summary>
		/********************************************************************/
		private bool LoadArpeggios(ModuleStream moduleStream, uint arpeggioOffset, int arpeggioCount)
		{
			moduleStream.Seek(arpeggioOffset, SeekOrigin.Begin);

			arpeggios = new sbyte[arpeggioCount][];

			for (int i = 0; i < arpeggioCount; i++)
			{
				arpeggios[i] = new sbyte[12];

				if (moduleStream.ReadSigned(arpeggios[i], 0, 12) != 12)
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load the instrument information
		/// </summary>
		/********************************************************************/
		private bool LoadInstruments(ModuleStream moduleStream, uint instrumentOffset, int instrumentCount)
		{
			moduleStream.Seek(instrumentOffset, SeekOrigin.Begin);

			instruments = new Instrument[instrumentCount];

			for (int i = 0; i < instrumentCount; i++)
			{
				Instrument instr = new Instrument();

				instr.SampleNumber = moduleStream.Read_B_INT32() + AmigaExecutableHunkSize;		// This will be converted later on to a real track number
				instr.VibratoNumber = moduleStream.Read_B_INT32() + AmigaExecutableHunkSize;	// This will be converted later on to a real track number
				instr.Type = moduleStream.Read_UINT8() == 0 ? InstrumentType.Synthesis : InstrumentType.Sample;
				instr.PhaseSpeed = moduleStream.Read_UINT8();
				instr.PhaseLengthInWords = moduleStream.Read_UINT8();
				instr.VibratoSpeed = moduleStream.Read_UINT8();
				instr.VibratoDepth = moduleStream.Read_UINT8();
				instr.VibratoDelay = moduleStream.Read_UINT8();

				for (int j = 0; j < 4; j++)
					instr.Adsr[j].Point = moduleStream.Read_UINT8();

				for (int j = 0; j < 4; j++)
					instr.Adsr[j].Increment = moduleStream.Read_UINT8();

				instr.PhaseValue = moduleStream.Read_INT8();
				instr.PhaseDirection = moduleStream.Read_INT8() < 0;
				instr.PhasePosition = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
					return false;

				moduleStream.Seek(7, SeekOrigin.Current);

				if (instr.VibratoSpeed == 0)
					instr.VibratoNumber = -1;

				instruments[i] = instr;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load the vibrato tables
		/// </summary>
		/********************************************************************/
		private bool LoadVibratos(ModuleStream moduleStream)
		{
			// Load all the vibrato tables into this dictionary.
			// The key is the vibrato offset and the value is the vibrato table
			Dictionary<int, sbyte[]> vibratoTables = new Dictionary<int, sbyte[]>();

			foreach (Instrument instr in instruments.Where(x => x.VibratoNumber != -1))
			{
				if (!vibratoTables.ContainsKey(instr.VibratoNumber))
				{
					moduleStream.Seek(instr.VibratoNumber, SeekOrigin.Begin);

					uint tableOffset = moduleStream.Read_B_UINT32() + AmigaExecutableHunkSize;
					ushort length = moduleStream.Read_B_UINT16();

					if (moduleStream.EndOfStream)
						return false;

					sbyte[] table = new sbyte[length * 2];

					moduleStream.Seek(tableOffset, SeekOrigin.Begin);

					if (moduleStream.ReadSigned(table, 0, table.Length) != table.Length)
						return false;

					vibratoTables.Add(instr.VibratoNumber, table);
				}
			}

			// Now convert the vibrato offset to vibrato numbers in the instruments
			vibratos = BuildOffsetToIndexLists(vibratoTables, out Dictionary<int, int> offsetToIndexMap);

			foreach (Instrument instr in instruments.Where(x => x.VibratoNumber != -1))
				instr.VibratoNumber = offsetToIndexMap[instr.VibratoNumber];

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load the sample information
		/// </summary>
		/********************************************************************/
		private bool LoadSamples(ModuleStream moduleStream)
		{
			// Load all the sample information into this dictionary.
			Dictionary<int, Sample> sampleInfo = new Dictionary<int, Sample>();

			foreach (Instrument instr in instruments)
			{
				if (!sampleInfo.ContainsKey(instr.SampleNumber))
				{
					moduleStream.Seek(instr.SampleNumber, SeekOrigin.Begin);

					Sample sample = new Sample();

					sample.SampleNumber = moduleStream.Read_B_INT32() + AmigaExecutableHunkSize;	// This will be converted later on to a real sample number
					sample.LengthInWords = moduleStream.Read_B_UINT16();
					sample.PhaseIndex = moduleStream.Read_B_UINT16();

					if (moduleStream.EndOfStream)
						return false;

					sampleInfo.Add(instr.SampleNumber, sample);
				}
			}

			// Now convert the sample info offset to sample info numbers in the instruments
			samples = BuildOffsetToIndexLists(sampleInfo, out Dictionary<int, int> offsetToIndexMap);

			foreach (Instrument instr in instruments)
				instr.SampleNumber = offsetToIndexMap[instr.SampleNumber];

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the samples
		/// </summary>
		/********************************************************************/
		private bool LoadSampleData(ModuleStream moduleStream)
		{
			// Load all the sample data into this dictionary.
			Dictionary<int, sbyte[]> data = new Dictionary<int, sbyte[]>();

			for (int i = 0; i < samples.Length; i++)
			{
				Sample sample = samples[i];

				if (!data.ContainsKey(sample.SampleNumber))
				{
					moduleStream.Seek(sample.SampleNumber, SeekOrigin.Begin);

					sbyte[] sampData = moduleStream.ReadSampleData(i, sample.LengthInWords * 2, out int readBytes);
					if (readBytes != (sample.LengthInWords * 2))
						return false;

					data.Add(sample.SampleNumber, sampData);
				}
			}

			// Now convert the sample data offset to sample data numbers
			sampleData = BuildOffsetToIndexLists(data, out Dictionary<int, int> offsetToIndexMap);

			foreach (Sample sample in samples)
				sample.SampleNumber = offsetToIndexMap[sample.SampleNumber];

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Build array from dictionary and lookup dictionary to help convert
		/// offsets to indexes
		/// </summary>
		/********************************************************************/
		private T[] BuildOffsetToIndexLists<T>(Dictionary<int, T> offsetDictionary, out Dictionary<int, int> offsetToIndexMap)
		{
			T[] result = offsetDictionary
				.OrderBy(x => x.Key)
				.Select(x => x.Value)
				.ToArray();

			offsetToIndexMap = offsetDictionary
				.OrderBy(x => x.Key)
				.Select((x, index) => (x.Key, Index: index))
				.ToDictionary(x => x.Key, x => x.Index);

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize sound structures
		/// </summary>
		/********************************************************************/
		private void InitializeSound(int songNumber)
		{
			playingInfo = new GlobalPlayingInfo
			{
				GlobalVolume = 0x3f,

				SetupNewSubSong = true,
				SubSongNumber = (ushort)(songNumber + 1)
			};

			voices = new VoiceInfo[4];

			for (int i = 0; i < 4; i++)
				InitializeVoice(i);

			SetCiaTimerTempo(ciaValue);
		}



		/********************************************************************/
		/// <summary>
		/// Initialize a single voice
		/// </summary>
		/********************************************************************/
		private void InitializeVoice(int voice)
		{
			PositionList positionList = subSongs[playingInfo.SubSongNumber - 1].Positions[voice];

			voices[voice] = new VoiceInfo
			{
				ChannelNumber = voice,

				PositionList = positionList,
				TrackListPosition = 0,
				TrackData = tracks[positionList.Tracks[0].TrackNumber],
				TrackDataPosition = 0,
				TrackRepeatCounter = (ushort)(positionList.Tracks[0].NumberOfRepeatTimes - 1),
				WaitCounter = 0,

				Instrument = instruments[0],
				InstrumentNumber = 0,

				ArpeggioValues = new sbyte[12],
				ArpeggioPosition = 0,

				CurrentNote = 0,
				Transpose = positionList.Tracks[0].Transpose,
				Period = 0,

				PortamentoEndPeriod = 0,
				PortamentoIncrement = 0,

				VibratoDelay = 0,
				VibratoPosition = 0,

				AdsrState = 0,
				AdsrSpeed = 0,
				AdsrSpeedCounter = 0,
				Volume = 0,

				PhaseSpeedCounter = 0,

				SetHardware = false,
				SampleNumber = 0,
				SampleData = null,
				SampleLength = 0,
				SetLoop = false
			};
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player have allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			subSongs = null;
			tracks = null;
			arpeggios = null;
			vibratos = null;
			instruments = null;
			samples = null;
			sampleData = null;

			playingInfo = null;
			voices = null;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize a new sub-song
		/// </summary>
		/********************************************************************/
		private void SwitchSubSong()
		{
			if (playingInfo.SubSongNumber == 0)
				playingInfo.SubSongNumber = (ushort)(currentSong + 1);

			for (int i = 0; i < 4; i++)
				InitializeVoice(i);

			playingInfo.SetupNewSubSong = false;
			playingInfo.SubSongNumber = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Playing a single voice
		/// </summary>
		/********************************************************************/
		private void ProcessVoice(VoiceInfo voiceInfo, IChannel channel)
		{
			if (voiceInfo.SetHardware)
			{
				channel.PlaySample(voiceInfo.SampleNumber, voiceInfo.SampleData, 0, voiceInfo.SampleLength);

				if (voiceInfo.SetLoop)
					channel.SetLoop(0, voiceInfo.SampleLength);

				voiceInfo.SetHardware = false;
			}

			if (voiceInfo.WaitCounter == 0)
				ParseTrackData(voiceInfo, channel);
			else
			{
				voiceInfo.WaitCounter--;

				EffectHandler(voiceInfo, channel);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse the next track command
		/// </summary>
		/********************************************************************/
		private void ParseTrackData(VoiceInfo voiceInfo, IChannel channel)
		{
			voiceInfo.PortamentoIncrement = 0;

			if (voiceInfo.Instrument.VibratoDelay != 0)
				voiceInfo.VibratoDelay = (byte)(voiceInfo.Instrument.VibratoDelay * 4 - 1);

			bool takeOneMore = true;

			do
			{
				byte cmd = voiceInfo.TrackData[voiceInfo.TrackDataPosition];

				switch ((Effect)cmd)
				{
					case Effect.SetArpeggio:
					{
						ParseTrackArpeggio(voiceInfo);
						break;
					}

					case Effect.SetPortamento:
					{
						ParseTrackPortamento(voiceInfo);
						takeOneMore = false;
						break;
					}

					case Effect.SetInstrument:
					{
						ParseTrackInstrument(voiceInfo, channel);
						break;
					}

					case Effect.EndSong:
					{
						ParseTrackEndSong();
						return;
					}

					case Effect.ChangeAdsrSpeed:
					{
						ParseTrackChangeAdsrSpeed(voiceInfo);
						break;
					}

					case Effect.EndOfTrack:
					{
						ParseTrackEndOfTrack(voiceInfo);
						break;
					}

					default:
					{
						ParseTrackNewNote(voiceInfo, channel);
						takeOneMore = false;
						break;
					}
				}
			}
			while (takeOneMore);

			channel.SetAmigaPeriod(voiceInfo.Period);
		}



		/********************************************************************/
		/// <summary>
		/// Set new arpeggio table
		/// </summary>
		/********************************************************************/
		private void ParseTrackArpeggio(VoiceInfo voiceInfo)
		{
			byte arpeggioNumber = voiceInfo.TrackData[voiceInfo.TrackDataPosition + 1];
			voiceInfo.TrackDataPosition += 2;

			voiceInfo.ArpeggioValues = arpeggios[arpeggioNumber];
		}



		/********************************************************************/
		/// <summary>
		/// Set portamento
		/// </summary>
		/********************************************************************/
		private void ParseTrackPortamento(VoiceInfo voiceInfo)
		{
			byte endNote = voiceInfo.TrackData[voiceInfo.TrackDataPosition + 1];
			byte increment = voiceInfo.TrackData[voiceInfo.TrackDataPosition + 2];
			byte waitCounter = voiceInfo.TrackData[voiceInfo.TrackDataPosition + 3];
			voiceInfo.TrackDataPosition += 4;

			int transposedNote = endNote + voiceInfo.Transpose;
			if (transposedNote >= Tables.Periods.Length)
				transposedNote = (ushort)(Tables.Periods.Length - 1);

			voiceInfo.AdsrState = 0;
			voiceInfo.PortamentoEndPeriod = Tables.Periods[transposedNote];
			voiceInfo.PortamentoIncrement = increment;
			voiceInfo.WaitCounter = (byte)(waitCounter * 4 - 1);
		}



		/********************************************************************/
		/// <summary>
		/// Set new instrument
		/// </summary>
		/********************************************************************/
		private void ParseTrackInstrument(VoiceInfo voiceInfo, IChannel channel)
		{
			byte instrumentNumber = voiceInfo.TrackData[voiceInfo.TrackDataPosition + 1];
			voiceInfo.TrackDataPosition += 2;

			voiceInfo.Volume = 0;
			voiceInfo.AdsrState = 0;
			voiceInfo.VibratoPosition = 0;

			voiceInfo.Instrument = instruments[instrumentNumber];
			voiceInfo.InstrumentNumber = instrumentNumber;

			PlaySample(voiceInfo, channel);
		}



		/********************************************************************/
		/// <summary>
		/// End the song
		/// </summary>
		/********************************************************************/
		private void ParseTrackEndSong()
		{
			playingInfo.SetupNewSubSong = true;
			playingInfo.SubSongNumber = 0;

			OnEndReachedOnAllChannels(0);
		}



		/********************************************************************/
		/// <summary>
		/// Change ADSR speed
		/// </summary>
		/********************************************************************/
		private void ParseTrackChangeAdsrSpeed(VoiceInfo voiceInfo)
		{
			byte speed = voiceInfo.TrackData[voiceInfo.TrackDataPosition + 1];
			voiceInfo.TrackDataPosition += 2;

			voiceInfo.AdsrSpeed = speed;
		}



		/********************************************************************/
		/// <summary>
		/// End of track
		/// </summary>
		/********************************************************************/
		private void ParseTrackEndOfTrack(VoiceInfo voiceInfo)
		{
			if (voiceInfo.TrackRepeatCounter == 0)
			{
				voiceInfo.TrackListPosition++;

				if (voiceInfo.TrackListPosition == voiceInfo.PositionList.Tracks.Length)
				{
					voiceInfo.TrackListPosition = 0;

					OnEndReached(voiceInfo.ChannelNumber);
				}

				Track track = voiceInfo.PositionList.Tracks[voiceInfo.TrackListPosition];

				voiceInfo.TrackData = tracks[track.TrackNumber];
				voiceInfo.TrackDataPosition = 0;
				voiceInfo.Transpose = track.Transpose;
				voiceInfo.TrackRepeatCounter = (ushort)(track.NumberOfRepeatTimes - 1);

				ShowChannelPositions();
				ShowTracks();
			}
			else
			{
				voiceInfo.TrackRepeatCounter--;
				voiceInfo.TrackDataPosition = 0;
				voiceInfo.Transpose = voiceInfo.PositionList.Tracks[voiceInfo.TrackListPosition].Transpose;

				// Battle Squadron title has a track that repeats more than 9000 times,
				// so we need to check for this
				if (voiceInfo.TrackRepeatCounter > 9000)
					OnEndReached(voiceInfo.ChannelNumber);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse new note
		/// </summary>
		/********************************************************************/
		private void ParseTrackNewNote(VoiceInfo voiceInfo, IChannel channel)
		{
			byte note = voiceInfo.TrackData[voiceInfo.TrackDataPosition];
			byte waitCount = voiceInfo.TrackData[voiceInfo.TrackDataPosition + 1];
			voiceInfo.TrackDataPosition += 2;

			int transposedNote = note + voiceInfo.Transpose;
			if (transposedNote >= Tables.Periods.Length)
				transposedNote = (ushort)(Tables.Periods.Length - 1);

			voiceInfo.CurrentNote = note;
			voiceInfo.Period = Tables.Periods[transposedNote];

			if (waitCount != 0)
			{
				voiceInfo.WaitCounter = (byte)(waitCount * 4 - 1);
				voiceInfo.AdsrState = 0;

				PlaySample(voiceInfo, channel);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Play instrument sample
		/// </summary>
		/********************************************************************/
		private void PlaySample(VoiceInfo voiceInfo, IChannel channel)
		{
			Instrument instr = voiceInfo.Instrument;
			Sample sample = samples[instr.SampleNumber];
			sbyte[] data = sampleData[sample.SampleNumber];

			voiceInfo.SetHardware = true;
			voiceInfo.SampleNumber = voiceInfo.InstrumentNumber;
			voiceInfo.SampleData = data;
			voiceInfo.SampleLength = sample.LengthInWords * 2U;
			voiceInfo.SetLoop = instr.Type == InstrumentType.Synthesis;

			channel.Mute();
		}



		/********************************************************************/
		/// <summary>
		/// Do real time effects
		/// </summary>
		/********************************************************************/
		private void EffectHandler(VoiceInfo voiceInfo, IChannel channel)
		{
			Instrument instr = voiceInfo.Instrument;

			DoEffectPhasing(voiceInfo, instr);
			DoEffectPortamento(voiceInfo, channel);
			DoEffectArpeggio(voiceInfo, channel);
			DoEffectVibrato(voiceInfo, channel, instr);
			DoEffectAdsr(voiceInfo, channel, instr);
		}



		/********************************************************************/
		/// <summary>
		/// Do phasing effect
		/// </summary>
		/********************************************************************/
		private void DoEffectPhasing(VoiceInfo voiceInfo, Instrument instr)
		{
			if (instr.PhaseSpeed != 0)
			{
				if (voiceInfo.PhaseSpeedCounter == 0)
				{
					voiceInfo.PhaseSpeedCounter = (byte)(instr.PhaseSpeed - 1);

					Sample sample = samples[instr.SampleNumber];
					sbyte[] data = sampleData[sample.SampleNumber];

					int phaseStart = sample.PhaseIndex - instr.PhaseLengthInWords;
					int phaseIndex = phaseStart + instr.PhasePosition;

					if (phaseIndex < data.Length)
						data[phaseIndex] = instr.PhaseValue;

					if (instr.PhaseDirection)
					{
						instr.PhasePosition--;

						if (instr.PhasePosition == 0)
						{
							instr.PhaseDirection = !instr.PhaseDirection;
							instr.PhaseValue = (sbyte)~instr.PhaseValue;
						}
					}
					else
					{
						instr.PhasePosition++;

						if ((instr.PhaseLengthInWords * 2) == instr.PhasePosition)
						{
							instr.PhaseDirection = !instr.PhaseDirection;
							instr.PhaseValue = (sbyte)~instr.PhaseValue;
						}
					}
				}
				else
					voiceInfo.PhaseSpeedCounter--;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Do portamento
		/// </summary>
		/********************************************************************/
		private void DoEffectPortamento(VoiceInfo voiceInfo, IChannel channel)
		{
			if (voiceInfo.PortamentoIncrement != 0)
			{
				ushort period = voiceInfo.Period;

				if (period <= voiceInfo.PortamentoEndPeriod)
				{
					period += voiceInfo.PortamentoIncrement;

					if (period > voiceInfo.PortamentoEndPeriod)
						period = voiceInfo.PortamentoEndPeriod;
				}
				else
				{
					period -= voiceInfo.PortamentoIncrement;

					if (period < voiceInfo.PortamentoEndPeriod)
						period = voiceInfo.PortamentoEndPeriod;
				}

				voiceInfo.Period = period;
				channel.SetAmigaPeriod(period);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Do arpeggio
		/// </summary>
		/********************************************************************/
		private void DoEffectArpeggio(VoiceInfo voiceInfo, IChannel channel)
		{
			// Only do arpeggio if no portamento is active
			if (voiceInfo.PortamentoIncrement == 0)
			{
				int transposedNote = voiceInfo.ArpeggioValues[voiceInfo.ArpeggioPosition] + voiceInfo.CurrentNote + voiceInfo.Transpose;
				if (transposedNote >= Tables.Periods.Length)
					transposedNote = Tables.Periods.Length - 1;

				voiceInfo.Period = Tables.Periods[transposedNote];
				channel.SetAmigaPeriod(voiceInfo.Period);

				if (voiceInfo.ArpeggioPosition == 0)
					voiceInfo.ArpeggioPosition = 11;
				else
					voiceInfo.ArpeggioPosition--;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Do vibrato
		/// </summary>
		/********************************************************************/
		private void DoEffectVibrato(VoiceInfo voiceInfo, IChannel channel, Instrument instr)
		{
			if (instr.VibratoSpeed != 0)
			{
				if (voiceInfo.VibratoDelay == 0)
				{
					sbyte[] vibratoTable = vibratos[instr.VibratoNumber];

					ushort period = (ushort)(vibratoTable[voiceInfo.VibratoPosition] * instr.VibratoDepth + voiceInfo.Period);
					channel.SetAmigaPeriod(period);

					int newPosition = voiceInfo.VibratoPosition - instr.VibratoSpeed;
					if (newPosition < 0)
						newPosition = vibratoTable.Length - 1;

					voiceInfo.VibratoPosition = (ushort)newPosition;
				}
				else
					voiceInfo.VibratoDelay--;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Do ADSR
		/// </summary>
		/********************************************************************/
		private void DoEffectAdsr(VoiceInfo voiceInfo, IChannel channel, Instrument instr)
		{
			voiceInfo.AdsrSpeedCounter--;

			if (voiceInfo.AdsrSpeedCounter < 0)
			{
				voiceInfo.AdsrSpeedCounter = (sbyte)voiceInfo.AdsrSpeed;

				AdsrPoint point = instr.Adsr[voiceInfo.AdsrState];

				void NextState()
				{
					if (voiceInfo.AdsrState < 3)
						voiceInfo.AdsrState++;
				}

				int volume = voiceInfo.Volume;

				if (volume > point.Point)
				{
					volume -= point.Increment;

					if (volume <= point.Point)
					{
						volume = point.Point;
						NextState();
					}
				}
				else
				{
					volume += point.Increment;

					if (volume >= playingInfo.GlobalVolume)
					{
						volume = playingInfo.GlobalVolume;
						NextState();
					}
					else if (volume >= point.Point)
					{
						volume = point.Point;
						NextState();
					}
				}

				voiceInfo.Volume = (byte)volume;
				channel.SetAmigaVolume((ushort)(volume & 0x3f));
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
				sb.Append(voices[i].PositionList.Tracks.Length);
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
				sb.Append(voices[i].TrackListPosition);
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
				sb.Append(voices[i].PositionList.Tracks[voices[i].TrackListPosition].TrackNumber);
				sb.Append(", ");
			}

			sb.Remove(sb.Length - 2, 2);

			return sb.ToString();
		}
		#endregion
	}
}
