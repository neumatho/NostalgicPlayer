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
using Polycode.NostalgicPlayer.Agent.Player.Hippel.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.Hippel
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class HippelWorker : ModulePlayerWithPositionDurationAgentBase
	{
		private int startOffset;

		private int bytesPerTrack;
		private int numberOfPositions;

		private List<SongInfo> songInfoList;

		private byte[][] frequencies;
		private Envelope[] envelopes;
		private byte[][] tracks;
		private Position[] positionList;
		private Sample[] samples;

		private SongInfo currentSongInfo;

		private GlobalPlayingInfo playingInfo;
		private VoiceInfo[] voices;

		private bool enableMute;
		private bool enableFrequencyPreviousInfo;
		private bool skipIdCheck;
		private bool e9Ands;
		private bool e9FixSample;
		private int vibratoVersion;

		private int[] effectsEnabled;
		private int speedInitValue;

		private ushort[] periodTable;

		private bool endReached;

		private const int InfoPositionLine = 3;
		private const int InfoTrackLine = 4;

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => new [] { "hip" };



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			if (moduleStream.Length < 2048)
				return AgentResult.Unknown;

			// Read the first part of the file, so it is easier to search
			byte[] buffer = new byte[16384];

			moduleStream.Seek(0, SeekOrigin.Begin);
			moduleStream.Read(buffer, 0, buffer.Length);

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
					description = Resources.IDS_HIP_INFODESCLINE0;
					value = numberOfPositions.ToString();
					break;
				}

				// Used tracks
				case 1:
				{
					description = Resources.IDS_HIP_INFODESCLINE1;
					value = tracks.Length.ToString();
					break;
				}

				// Used samples
				case 2:
				{
					description = Resources.IDS_HIP_INFODESCLINE2;
					value = samples.Length.ToString();
					break;
				}

				// Playing position
				case 3:
				{
					description = Resources.IDS_HIP_INFODESCLINE3;
					value = FormatPosition();
					break;
				}

				// Playing tracks
				case 4:
				{
					description = Resources.IDS_HIP_INFODESCLINE4;
					value = FormatTracks();
					break;
				}

				// Current speed
				case 5:
				{
					description = Resources.IDS_HIP_INFODESCLINE5;
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

				moduleStream.Seek(startOffset, SeekOrigin.Begin);

				Header header = LoadHeader(moduleStream);
				if (header == null)
				{
					errorMessage = Resources.IDS_HIP_ERR_LOADING_HEADER;
					return AgentResult.Error;
				}

				if (!LoadFrequencies(moduleStream, header.NumberOfFrequencies + 1))
				{
					errorMessage = Resources.IDS_HIP_ERR_LOADING_FREQUENCIES;
					return AgentResult.Error;
				}

				if (!LoadEnvelopes(moduleStream, header.NumberOfEnvelopes + 1))
				{
					errorMessage = Resources.IDS_HIP_ERR_LOADING_ENVELOPES;
					return AgentResult.Error;
				}

				if (!LoadTracks(moduleStream, header.NumberOfTracks + 1))
				{
					errorMessage = Resources.IDS_HIP_ERR_LOADING_TRACKS;
					return AgentResult.Error;
				}

				if (!LoadPositionList(moduleStream, header.NumberOfPositions + 1))
				{
					errorMessage = Resources.IDS_HIP_ERR_LOADING_POSITIONLIST;
					return AgentResult.Error;
				}

				if (!LoadSongInfo(moduleStream, header.NumberOfSubSongs))
				{
					errorMessage = Resources.IDS_HIP_ERR_LOADING_SUBSONGS;
					return AgentResult.Error;
				}

				uint[] sampleOffsets = LoadSampleInfo(moduleStream, header.NumberOfSamples);
				if (sampleOffsets == null)
				{
					errorMessage = Resources.IDS_HIP_ERR_LOADING_SAMPLEINFO;
					return AgentResult.Error;
				}

				if (!LoadSampleData(moduleStream, sampleOffsets))
				{
					errorMessage = Resources.IDS_HIP_ERR_LOADING_SAMPLES;
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
					ReadNextRow(i);
			}

			for (int i = 0; i < 4; i++)
				DoEffects(i);

			if (endReached)
			{
				OnEndReached((int)voices[0].NextPosition - 1);
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
				uint[] freqs = new uint[10 * 12];

				for (int j = 0; j < Tables.Periods1.Length; j++)
				{
					uint period = Tables.Periods1[j];
					freqs[3 * 12 + j] = 3546895U / period;
				}

				foreach (Sample sample in samples)
				{
					SampleInfo sampleInfo = new SampleInfo
					{
						Name = sample.Name,
						Type = SampleInfo.SampleType.Sample,
						BitSize = SampleInfo.SampleSize._8Bit,
						Volume = (ushort)(sample.Volume * 4),
						Panning = -1,
						Sample = sample.SampleData,
						Length = sample.Length,
						NoteFrequencies = freqs
					};

					if (sample.LoopLength > 2)
					{
						sampleInfo.LoopStart = sample.LoopStart;
						sampleInfo.LoopLength = sample.LoopLength;
						sampleInfo.Flags = SampleInfo.SampleFlag.Loop;
					}
					else
						sampleInfo.Flags = SampleInfo.SampleFlag.None;

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
		protected override int InitDuration(int subSong, int startPosition)
		{
			if (subSong >= songInfoList.Count)
				return -1;

			InitializeSound(subSong);

			return currentSongInfo.StartPosition;
		}



		/********************************************************************/
		/// <summary>
		/// Return the total number of positions
		/// </summary>
		/********************************************************************/
		protected override int GetTotalNumberOfPositions()
		{
			return positionList.Length;
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
		/// Test the file to see if it's a Jochen Hippel player and extract
		/// needed information
		/// </summary>
		/********************************************************************/
		private AgentResult TestModule(byte[] buffer)
		{
			// SC68 support are handled in a converter, so ignore these modules here
			if ((buffer[0] == 0x53) && (buffer[1] == 0x43) && (buffer[2] == 0x36) && (buffer[3] == 0x38))
				return AgentResult.Unknown;

			for (int i = 0; i < buffer.Length - 4; i += 2)
			{
				if ((buffer[i] == 0x41) && (buffer[i + 1] == 0xfa))
				{
					int index = (((sbyte)buffer[i + 2] << 8) | buffer[i + 3]) + i + 2;

					if (index >= (buffer.Length - 4))
						return AgentResult.Unknown;

					if ((buffer[index] == 'T') && (buffer[index + 1] == 'F') && (buffer[index + 2] == 'M') && (buffer[index + 3] == 'X'))
					{
						startOffset = index;

						return FindFeatures(buffer, startOffset);
					}
				}
			}

			return AgentResult.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Look for different features to see if they are available or not
		/// </summary>
		/********************************************************************/
		private AgentResult FindFeatures(byte[] searchBuffer, int searchLength)
		{
			if (ExtractFromReadNextRow(searchBuffer, searchLength) != AgentResult.Ok)
				return AgentResult.Unknown;

			if (ExtractFromDoEffects(searchBuffer, searchLength) != AgentResult.Ok)
				return AgentResult.Unknown;

			if (ExtractFromInitializeStructures(searchBuffer, searchLength) != AgentResult.Ok)
				return AgentResult.Unknown;

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Extract information from the ReadNextRow function
		/// </summary>
		/********************************************************************/
		private AgentResult ExtractFromReadNextRow(byte[] searchBuffer, int searchLength)
		{
			int index;

			for (index = 0; index < searchLength - 6; index += 2)
			{
				if ((searchBuffer[index] == 0x3e) && (searchBuffer[index + 1] == 0x3a) && (searchBuffer[index + 4] == 0x22) && (searchBuffer[index + 5] == 0x68))
					break;
			}

			if (index >= (searchLength - 6))
				return AgentResult.Unknown;

			// Check to see if "mute" feature is enabled
			for (; index < searchLength - 4; index += 2)
			{
				if ((searchBuffer[index] == 0x4a) && (searchBuffer[index + 1] == 0x00) && (searchBuffer[index + 2] == 0x6b))
					break;
			}

			if (index >= (searchLength - 4))
				return AgentResult.Unknown;

			index += 4;
			enableMute = (searchBuffer[index] == 0x14) && (searchBuffer[index + 1] == 0x28);

			// Check for "use previous info for frequency table number" feature
			int startIndex = index;

			for (; index < searchLength; index += 2)
			{
				if ((searchBuffer[index] == 0x4e) && (searchBuffer[index + 1] == 0x75))
					break;
			}

			if (index >= (searchLength - 2))
				return AgentResult.Unknown;

			for (; index >= startIndex; index -= 2)
			{
				if ((searchBuffer[index] == 0x08) && (searchBuffer[index + 1] == 0x28))
					break;
			}

			enableFrequencyPreviousInfo = index > startIndex;

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Extract information from the DoEffects function
		/// </summary>
		/********************************************************************/
		private AgentResult ExtractFromDoEffects(byte[] searchBuffer, int searchLength)
		{
			int index;

			for (index = 0; index < searchLength - 8; index += 2)
			{
				if ((searchBuffer[index] == 0x4a) && (searchBuffer[index + 1] == 0x28) && (searchBuffer[index + 6] == 0x53) && (searchBuffer[index + 7] == 0x28))
					break;
			}

			if (index >= (searchLength - 8))
				return AgentResult.Unknown;

			index += 10;

			if ((searchBuffer[index] != 0x60) || (searchBuffer[index + 1] != 0x00))
				return AgentResult.Unknown;

			int runEffectIndex = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2;
			effectsEnabled = new int[16];

			// Find which effects are enabled
			int startIndex = index;

			for (; index < searchLength - 10; index += 2)
			{
				if ((searchBuffer[index] == 0x0c) && (searchBuffer[index + 8] == 0x0c))
					break;
			}

			if (index < (searchLength - 10))
			{
				index += 8;

				while ((index < (searchLength - 8)) && (searchBuffer[index] == 0x0c))
				{
					int effect = ((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3];
					if ((effect < 0xe0) || (effect > 0xef))
						return AgentResult.Unknown;

					effectsEnabled[effect - 0xe0] = 1;

					if (effect == 0xe6)
					{
						if ((searchBuffer[index + 6] == 0x0c) && (searchBuffer[index + 7] == 0x00))
							effectsEnabled[6] = 0;
					}
					else if (effect == 0xe7)
					{
						if ((searchBuffer[index + 12] == 0xb2) && (searchBuffer[index + 13] == 0x28))
							effectsEnabled[7] = 2;
					}

					int offset = searchBuffer[index + 5] == 0x00 ? ((sbyte)searchBuffer[index + 6] << 8) | searchBuffer[index + 7] : searchBuffer[index + 5];
					index += offset + 6;
				}

				if (index >= (searchLength - 8))
					return AgentResult.Unknown;

				skipIdCheck = false;
				e9Ands = false;
				e9FixSample = false;
			}
			else
			{
				for (index = startIndex; index < searchLength - 4; index += 2)
				{
					if ((searchBuffer[index] == 0x4e) && (searchBuffer[index + 1] == 0xfb))
						break;
				}

				if (index >= (searchLength - 4))
					return AgentResult.Unknown;

				int firstCodeAfterJumpTable = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2;

				for (int i = 0, j = index + 4; j < firstCodeAfterJumpTable; j += 2, i++)
					effectsEnabled[i] = 1;

				if (effectsEnabled[5] != 0)
					effectsEnabled[5] = 2;

				if (effectsEnabled[7] != 0)
					effectsEnabled[7] = 2;

				if (effectsEnabled[9] != 0)
				{
					e9Ands = true;
					e9FixSample = true;
				}

				skipIdCheck = true;
			}

			// Extract from RunEffect part
			for (index = runEffectIndex; index < searchLength - 4; index += 2)
			{
				if ((searchBuffer[index] == 0x43) && (searchBuffer[index + 1] == 0xfa))
					break;
			}

			if (index >= (searchLength - 4))
				return AgentResult.Unknown;

			int periodIndex = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2;
			if (periodIndex > (searchLength - 116))
				return AgentResult.Unknown;

			if ((searchBuffer[periodIndex] != 0x06) || (searchBuffer[periodIndex + 1] != 0xb0))
				return AgentResult.Unknown;

			if ((periodIndex < (searchLength - 128)) && (searchBuffer[periodIndex + 118] == 0x00) && (searchBuffer[periodIndex + 119] == 0x71) && (searchBuffer[periodIndex + 120] == 0x0d) && (searchBuffer[periodIndex + 121] == 0x60))
				periodTable = Tables.Periods2;
			else
				periodTable = Tables.Periods1;

			// Find vibrato version
			for (; index < searchLength - 6; index += 2)
			{
				if ((searchBuffer[index] == 0x53) && (searchBuffer[index + 1] == 0x28) || (searchBuffer[index + 4] == 0x60))
					break;
			}

			if (index >= (searchLength - 6))
				return AgentResult.Unknown;

			index += 6;

			if ((searchBuffer[index] == 0x1a) && (searchBuffer[index + 1] == 0x01) && (searchBuffer[index + 2] == 0x18) && (searchBuffer[index + 3] == 0x28))
				vibratoVersion = 1;
			else if ((searchBuffer[index] == 0x78) && (searchBuffer[index + 1] == 0x00) && (searchBuffer[index + 2] == 0x7a) && (searchBuffer[index + 3] == 0x00))
				vibratoVersion = 2;
			else
				return AgentResult.Unknown;

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Extract information from the InitializeStructures function
		/// </summary>
		/********************************************************************/
		private AgentResult ExtractFromInitializeStructures(byte[] searchBuffer, int searchLength)
		{
			int index;

			for (index = 0; index < searchLength - 6; index += 2)
			{
				if ((searchBuffer[index] == 0x3c) && (searchBuffer[index + 1] == 0xd9) && (searchBuffer[index + 2] == 0x61) && (searchBuffer[index + 3] == 0x00))
					break;
			}

			if (index >= (searchLength - 6))
				return AgentResult.Unknown;

			index = (((sbyte)searchBuffer[index + 4] << 8) | searchBuffer[index + 5]) + index + 4;
			if (index >= searchLength)
				return AgentResult.Unknown;

			for (; index < searchLength - 12; index += 2)
			{
				if ((searchBuffer[index] == 0x51) && (searchBuffer[index + 1] == 0xc8))
					break;
			}

			if (index >= (searchLength - 12))
				return AgentResult.Unknown;

			index += 8;

			if ((searchBuffer[index] == 0x30) && (searchBuffer[index + 1] == 0xbc))
				speedInitValue = (searchBuffer[index + 2] << 8) | searchBuffer[index + 3];
			else
				speedInitValue = -1;

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Load header information
		/// </summary>
		/********************************************************************/
		private Header LoadHeader(ModuleStream moduleStream)
		{
			Header header = new Header();

			// Skip ID
			moduleStream.Seek(4, SeekOrigin.Current);

			header.NumberOfFrequencies = moduleStream.Read_B_UINT16();
			header.NumberOfEnvelopes = moduleStream.Read_B_UINT16();
			header.NumberOfTracks = moduleStream.Read_B_UINT16();
			header.NumberOfPositions = moduleStream.Read_B_UINT16();
			header.BytesPerTrack = moduleStream.Read_B_UINT16();

			if (moduleStream.EndOfStream)
				return null;

			moduleStream.Seek(2, SeekOrigin.Current);

			header.NumberOfSubSongs = moduleStream.Read_B_UINT16();
			header.NumberOfSamples = moduleStream.Read_B_UINT16();

			if (moduleStream.EndOfStream)
				return null;

			moduleStream.Seek(12, SeekOrigin.Current);

			bytesPerTrack = header.BytesPerTrack;

			return header;
		}



		/********************************************************************/
		/// <summary>
		/// Load frequency tables
		/// </summary>
		/********************************************************************/
		private bool LoadFrequencies(ModuleStream moduleStream, int numberOfFrequencies)
		{
			frequencies = new byte[numberOfFrequencies][];

			for (int i = 0; i < numberOfFrequencies; i++)
			{
				frequencies[i] = new byte[64];

				moduleStream.Read(frequencies[i], 0, 64);
				if (moduleStream.EndOfStream)
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load envelope tables
		/// </summary>
		/********************************************************************/
		private bool LoadEnvelopes(ModuleStream moduleStream, int numberOfEnvelopes)
		{
			envelopes = new Envelope[numberOfEnvelopes];

			for (int i = 0; i < numberOfEnvelopes; i++)
			{
				Envelope envelope = new Envelope();

				envelope.EnvelopeSpeed = moduleStream.Read_UINT8();
				envelope.FrequencyNumber = moduleStream.Read_UINT8();
				envelope.VibratoSpeed = moduleStream.Read_UINT8();
				envelope.VibratoDepth = moduleStream.Read_UINT8();
				envelope.VibratoDelay = moduleStream.Read_UINT8();

				moduleStream.Read(envelope.EnvelopeTable, 0, envelope.EnvelopeTable.Length);
				if (moduleStream.EndOfStream)
					return false;

				envelopes[i] = envelope;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load tracks
		/// </summary>
		/********************************************************************/
		private bool LoadTracks(ModuleStream moduleStream, int numberOfTracks)
		{
			tracks = new byte[numberOfTracks][];

			for (int i = 0; i < numberOfTracks; i++)
			{
				tracks[i] = new byte[bytesPerTrack];

				moduleStream.Read(tracks[i], 0, tracks[i].Length);
				if (moduleStream.EndOfStream)
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load position list
		/// </summary>
		/********************************************************************/
		private bool LoadPositionList(ModuleStream moduleStream, int numberOfPositions)
		{
			positionList = new Position[numberOfPositions];

			for (int i = 0; i < numberOfPositions; i++)
			{
				Position position = new Position
				{
					PositionInfo = new SinglePositionInfo[4]
				};

				for (int j = 0; j < 4; j++)
				{
					SinglePositionInfo singlePositionInfo = new SinglePositionInfo();

					singlePositionInfo.Track = moduleStream.Read_UINT8();
					singlePositionInfo.NoteTranspose = moduleStream.Read_INT8();
					singlePositionInfo.EnvelopeTranspose = moduleStream.Read_INT8();

					position.PositionInfo[j] = singlePositionInfo;
				}

				if (moduleStream.EndOfStream)
					return false;

				positionList[i] = position;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load song information
		/// </summary>
		/********************************************************************/
		private bool LoadSongInfo(ModuleStream moduleStream, int numberOfSubSongs)
		{
			songInfoList = new List<SongInfo>();

			for (int i = 0; i < numberOfSubSongs; i++)
			{
				SongInfo songInfo = new SongInfo();

				songInfo.StartPosition = moduleStream.Read_B_UINT16();
				songInfo.LastPosition = (ushort)(moduleStream.Read_B_UINT16() + 1);
				songInfo.StartSpeed = moduleStream.Read_B_UINT16();

				if (moduleStream.EndOfStream)
					return false;

				if (songInfo.StartSpeed != 0)
					songInfoList.Add(songInfo);
			}

			// Skip sub-song paddings
			moduleStream.Seek(6, SeekOrigin.Current);

			numberOfPositions = songInfoList.Max(x => x.LastPosition);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the sample information
		/// </summary>
		/********************************************************************/
		private uint[] LoadSampleInfo(ModuleStream moduleStream, int numberOfSamples)
		{
			samples = new Sample[numberOfSamples];
			uint[] sampleOffsets = new uint[numberOfSamples];

			Encoding encoder = EncoderCollection.Amiga;
			byte[] buf = new byte[18];

			for (int i = 0; i < numberOfSamples; i++)
			{
				Sample sample = new Sample();

				moduleStream.Read(buf, 0, 18);
				if (moduleStream.EndOfStream)
					return null;

				sample.Name = encoder.GetString(buf);

				sampleOffsets[i] = moduleStream.Read_B_UINT32();

				sample.Length = moduleStream.Read_B_UINT16() * 2U;
				sample.Volume = moduleStream.Read_B_UINT16();
				sample.LoopStart = moduleStream.Read_B_UINT16() & ~1U;
				sample.LoopLength = moduleStream.Read_B_UINT16() * 2U;

				if (moduleStream.EndOfStream)
					return null;

				if ((sample.LoopStart + sample.LoopLength) > sample.Length)
					sample.LoopLength = sample.Length - sample.LoopStart;

				samples[i] = sample;
			}

			return sampleOffsets;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the sample data
		/// </summary>
		/********************************************************************/
		private bool LoadSampleData(ModuleStream moduleStream, uint[] sampleOffsets)
		{
			long sampleDataStartPosition = moduleStream.Position;

			for (int i = 0; i < samples.Length; i++)
			{
				Sample sample = samples[i];

				using (ModuleStream sampleDataStream = moduleStream.GetSampleDataStream(i, (int)sample.Length))
				{
					sampleDataStream.Seek(sampleDataStartPosition + sampleOffsets[i], SeekOrigin.Begin);

					sample.SampleData = new sbyte[sample.Length];
					sampleDataStream.ReadSigned(sample.SampleData, 0, (int)sample.Length);

					if (sampleDataStream.EndOfStream)
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
			currentSongInfo = songInfoList[subSong];

			playingInfo = new GlobalPlayingInfo
			{
				SpeedCounter = speedInitValue < 0 ? currentSongInfo.StartSpeed : (ushort)speedInitValue,
				Speed = currentSongInfo.StartSpeed
			};

			voices = new VoiceInfo[4];

			for (int i = 0; i < 4; i++)
			{
				SinglePositionInfo singlePositionInfo = positionList[currentSongInfo.StartPosition].PositionInfo[i];

				VoiceInfo voiceInfo = new VoiceInfo
				{
					EnvelopeTable = Tables.DefaultCommandTable,
					EnvelopePosition = 0,
					OriginalEnvelopeNumber = 0,
					CurrentEnvelopeNumber = 0,

					FrequencyTable = Tables.DefaultCommandTable,
					FrequencyPosition = 0,
					OriginalFrequencyNumber = 0,
					CurrentFrequencyNumber = 0,

					NextPosition = currentSongInfo.StartPosition + 1U,

					CurrentTrackNumber = singlePositionInfo.Track,
					Track = tracks[singlePositionInfo.Track],
					TrackPosition = 0,
					TrackTranspose = singlePositionInfo.NoteTranspose,
					EnvelopeTranspose = singlePositionInfo.EnvelopeTranspose,

					Transpose = 0,
					CurrentNote = 0,
					CurrentInfo = 0,
					PreviousInfo = 0,

					Sample = 0xff,

					Tick = 0,

					Volume = 0,
					EnvelopeCounter = 1,
					EnvelopeSpeed = 1,
					EnvelopeSustain = 0,

					VibratoFlag = 0,
					VibratoSpeed = 0,
					VibratoDelay = 0,
					VibratoDepth = 0,
					VibratoDelta = 0,

					PortaDelta = 0,

					Slide = false,
					SlideSample = 0,
					SlideEndPosition = 0,
					SlideLoopPosition = 0,
					SlideLength = 0,
					SlideDelta = 0,
					SlideCounter = 0,
					SlideSpeed = 0,
					SlideActive = false,
					SlideDone = false
				};

				voices[i] = voiceInfo;
			}

			MarkPositionAsVisited(currentSongInfo.StartPosition);

			endReached = false;
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player have allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			songInfoList = null;

			frequencies = null;
			envelopes = null;
			tracks = null;
			positionList = null;
			samples = null;

			currentSongInfo = null;

			playingInfo = null;
			voices = null;

			effectsEnabled = null;
		}



		/********************************************************************/
		/// <summary>
		/// Read the next row for a single channel
		/// </summary>
		/********************************************************************/
		private void ReadNextRow(int voice)
		{
			VoiceInfo voiceInfo = voices[voice];
			IChannel channel = VirtualChannels[voice];

			if ((voiceInfo.TrackPosition == bytesPerTrack) || ((voiceInfo.Track[voiceInfo.TrackPosition] & 0x7f) == 1))
			{
				if (voiceInfo.NextPosition == currentSongInfo.LastPosition)
				{
					voiceInfo.NextPosition = currentSongInfo.StartPosition;
					endReached = true;
				}

				SinglePositionInfo positionInfo = positionList[voiceInfo.NextPosition].PositionInfo[voice];

				voiceInfo.TrackTranspose = positionInfo.NoteTranspose;
				voiceInfo.EnvelopeTranspose = positionInfo.EnvelopeTranspose;

				if (positionInfo.EnvelopeTranspose == -128)
				{
					// Song stopped
					endReached = true;

					voiceInfo.NextPosition = currentSongInfo.StartPosition;

					positionInfo = positionList[voiceInfo.NextPosition].PositionInfo[voice];

					voiceInfo.TrackTranspose = positionInfo.NoteTranspose;
					voiceInfo.EnvelopeTranspose = positionInfo.EnvelopeTranspose;
				}

				voiceInfo.CurrentTrackNumber = positionInfo.Track;
				voiceInfo.Track = tracks[positionInfo.Track];
				voiceInfo.TrackPosition = 0;

				if (voice == 0)
				{
					if (HasPositionBeenVisited((int)voiceInfo.NextPosition))
						endReached = true;

					MarkPositionAsVisited((int)voiceInfo.NextPosition);
				}

				voiceInfo.NextPosition++;

				ShowPosition();
				ShowTracks();
			}

			byte val = voiceInfo.Track[voiceInfo.TrackPosition];
			byte note = (byte)(val & 0x7f);

			if (note != 0)
			{
				voiceInfo.PortaDelta = 0;
				voiceInfo.CurrentNote = note;

				voiceInfo.PreviousInfo = voiceInfo.Track[(voiceInfo.TrackPosition == 0 ? bytesPerTrack : voiceInfo.TrackPosition) - 1];
				voiceInfo.CurrentInfo = voiceInfo.Track[voiceInfo.TrackPosition + 1];

				if (val < 128)
				{
					if (enableMute)
						channel.Mute();

					val = (byte)((voiceInfo.CurrentInfo & 0x1f) + voiceInfo.EnvelopeTranspose);
					if (val >= envelopes.Length)
						val = 0;

					Envelope envelope = envelopes[val];

					voiceInfo.EnvelopeCounter = envelope.EnvelopeSpeed;
					voiceInfo.EnvelopeSpeed = envelope.EnvelopeSpeed;

					voiceInfo.VibratoFlag = 0x40;
					voiceInfo.VibratoSpeed = envelope.VibratoSpeed;
					voiceInfo.VibratoDepth = envelope.VibratoDepth;
					voiceInfo.VibratoDelta = envelope.VibratoDepth;
					voiceInfo.VibratoDelay = envelope.VibratoDelay;

					voiceInfo.EnvelopeTable = envelope.EnvelopeTable;
					voiceInfo.EnvelopePosition = 0;
					voiceInfo.OriginalEnvelopeNumber = val;
					voiceInfo.CurrentEnvelopeNumber = val;

					byte frequencyNumber = envelope.FrequencyNumber;

					if (enableFrequencyPreviousInfo && ((voiceInfo.CurrentInfo & 0x40) != 0))
						frequencyNumber = voiceInfo.PreviousInfo;

					voiceInfo.FrequencyTable = frequencies[frequencyNumber];
					voiceInfo.FrequencyPosition = 0;
					voiceInfo.OriginalFrequencyNumber = frequencyNumber;
					voiceInfo.CurrentFrequencyNumber = frequencyNumber;

					voiceInfo.Tick = 0;
					voiceInfo.EnvelopeSustain = 0;
				}
			}

			voiceInfo.TrackPosition += 2;
		}



		/********************************************************************/
		/// <summary>
		/// Parse and run effects
		/// </summary>
		/********************************************************************/
		private void DoEffects(int voice)
		{
			VoiceInfo voiceInfo = voices[voice];
			IChannel channel = VirtualChannels[voice];

			ParseEffects(voiceInfo, channel);
			RunEffects(voiceInfo, channel);
		}



		/********************************************************************/
		/// <summary>
		/// Parse effects
		/// </summary>
		/********************************************************************/
		private void ParseEffects(VoiceInfo voiceInfo, IChannel channel)
		{
			bool oneMoreBigLoop;

			do
			{
				oneMoreBigLoop = false;

				if (voiceInfo.Tick == 0)
				{
					bool oneMore;

					do
					{
						oneMore = false;

						if (voiceInfo.FrequencyPosition == 64)
						{
							voiceInfo.FrequencyTable = frequencies[++voiceInfo.CurrentFrequencyNumber];
							voiceInfo.FrequencyPosition = 0;
						}

						byte val = voiceInfo.FrequencyTable[voiceInfo.FrequencyPosition];

						if (val != 0xe1)
						{
							if (val == 0xe0)
							{
								voiceInfo.FrequencyPosition = voiceInfo.FrequencyTable[voiceInfo.FrequencyPosition + 1] & 0x3f;

								voiceInfo.FrequencyTable = frequencies[voiceInfo.OriginalFrequencyNumber];
								voiceInfo.CurrentFrequencyNumber = voiceInfo.OriginalFrequencyNumber;

								val = voiceInfo.FrequencyTable[voiceInfo.FrequencyPosition];
							}

							switch (val)
							{
								case 0xe2:
								{
									if (effectsEnabled[2] != 0)
									{
										voiceInfo.Sample = 0xff;

										byte sampleNumber = voiceInfo.FrequencyTable[voiceInfo.FrequencyPosition + 1];
										Sample sample = samples[sampleNumber];

										channel.PlaySample(sampleNumber, sample.SampleData, 0, sample.Length);

										if (sample.LoopLength > 2)
											channel.SetLoop(sample.LoopStart, sample.LoopLength);

										voiceInfo.EnvelopePosition = 0;
										voiceInfo.EnvelopeCounter = 1;

										voiceInfo.Slide = false;

										voiceInfo.FrequencyPosition += 2;
									}
									break;
								}

								case 0xe3:
								{
									if (effectsEnabled[3] != 0)
									{
										voiceInfo.VibratoSpeed = voiceInfo.FrequencyTable[voiceInfo.FrequencyPosition + 1];
										voiceInfo.VibratoDepth = voiceInfo.FrequencyTable[voiceInfo.FrequencyPosition + 2];

										voiceInfo.FrequencyPosition += 3;
									}
									break;
								}

								case 0xe4:
								{
									if (effectsEnabled[4] != 0)
									{
										byte sampleNumber = voiceInfo.FrequencyTable[voiceInfo.FrequencyPosition + 1];
										Sample sample = samples[sampleNumber];

										channel.SetSample(sample.SampleData, 0, sample.Length);

										if (sample.LoopLength > 2)
											channel.SetLoop(sample.LoopStart, sample.LoopLength);

										voiceInfo.Slide = false;

										voiceInfo.FrequencyPosition += 2;
									}
									break;
								}

								case 0xe5:
								{
									byte sampleNumber = voiceInfo.FrequencyTable[voiceInfo.FrequencyPosition + 1];
									Sample sample = samples[sampleNumber];

									switch (effectsEnabled[5])
									{
										case 1:
										{
											uint offset = voiceInfo.FrequencyTable[voiceInfo.FrequencyPosition + 2] * sample.LoopLength;

											channel.PlaySample(sampleNumber, sample.SampleData, offset, sample.LoopLength);

											if (sample.LoopLength > 2)
												channel.SetLoop(sample.LoopStart, sample.LoopLength);

											voiceInfo.FrequencyPosition += 3;
											break;
										}

										case 2:
										{
											channel.Mute();

											voiceInfo.SlideSample = sampleNumber;
											voiceInfo.SlideEndPosition = (int)sample.Length;

											ushort loopPosition = (ushort)((voiceInfo.FrequencyTable[voiceInfo.FrequencyPosition + 2] << 8) | voiceInfo.FrequencyTable[voiceInfo.FrequencyPosition + 3]);
											if (loopPosition == 0xffff)
												loopPosition = (ushort)(sample.Length / 2);

											voiceInfo.SlideLoopPosition = loopPosition;

											voiceInfo.SlideLength = (ushort)((voiceInfo.FrequencyTable[voiceInfo.FrequencyPosition + 4] << 8) | voiceInfo.FrequencyTable[voiceInfo.FrequencyPosition + 5]);
											voiceInfo.SlideDelta = (short)((voiceInfo.FrequencyTable[voiceInfo.FrequencyPosition + 6] << 8) | voiceInfo.FrequencyTable[voiceInfo.FrequencyPosition + 7]);
											voiceInfo.SlideSpeed = voiceInfo.FrequencyTable[voiceInfo.FrequencyPosition + 8];
											voiceInfo.SlideCounter = 0;
											voiceInfo.SlideActive = false;
											voiceInfo.SlideDone = false;
											voiceInfo.Slide = true;

											voiceInfo.FrequencyPosition += 9;
											break;
										}
									}

									voiceInfo.EnvelopePosition = 0;
									voiceInfo.EnvelopeCounter = 1;
									break;
								}

								case 0xe6:
								{
									if (effectsEnabled[6] != 0)
									{
										voiceInfo.SlideLength = (ushort)((voiceInfo.FrequencyTable[voiceInfo.FrequencyPosition + 1] << 8) | voiceInfo.FrequencyTable[voiceInfo.FrequencyPosition + 2]);
										voiceInfo.SlideDelta = (short)((voiceInfo.FrequencyTable[voiceInfo.FrequencyPosition + 3] << 8) | voiceInfo.FrequencyTable[voiceInfo.FrequencyPosition + 4]);
										voiceInfo.SlideSpeed = voiceInfo.FrequencyTable[voiceInfo.FrequencyPosition + 5];
										voiceInfo.SlideCounter = 0;
										voiceInfo.SlideActive = false;
										voiceInfo.SlideDone = false;

										voiceInfo.FrequencyPosition += 6;
									}
									break;
								}

								case 0xe7:
								{
									switch (effectsEnabled[7])
									{
										case 1:
										{
											byte frequencyNumber = voiceInfo.FrequencyTable[voiceInfo.FrequencyPosition + 1];
											voiceInfo.FrequencyTable = frequencies[frequencyNumber];
											voiceInfo.FrequencyPosition = 0;
											voiceInfo.OriginalFrequencyNumber = frequencyNumber;
											voiceInfo.CurrentFrequencyNumber = frequencyNumber;

											oneMore = true;
											break;
										}

										case 2:
										{
											byte sampleNumber = voiceInfo.FrequencyTable[voiceInfo.FrequencyPosition + 1];

											if (voiceInfo.Sample != sampleNumber)
											{
												voiceInfo.Sample = sampleNumber;
												Sample sample = samples[sampleNumber];

												channel.PlaySample(sampleNumber, sample.SampleData, 0, sample.Length);

												if (sample.LoopLength > 2)
													channel.SetLoop(sample.LoopStart, sample.LoopLength);
											}

											voiceInfo.EnvelopePosition = 0;
											voiceInfo.EnvelopeCounter = 1;

											voiceInfo.Slide = false;

											voiceInfo.FrequencyPosition += 2;
											break;
										}
									}
									break;
								}

								case 0xe8:
								{
									if (effectsEnabled[8] != 0)
									{
										voiceInfo.Tick = voiceInfo.FrequencyTable[voiceInfo.FrequencyPosition + 1];

										voiceInfo.FrequencyPosition += 2;
										oneMoreBigLoop = true;
									}
									break;
								}

								case 0xe9:
								{
									if (effectsEnabled[9] != 0)
									{
										voiceInfo.Sample = 0xff;

										byte sampleNumber = voiceInfo.FrequencyTable[voiceInfo.FrequencyPosition + 1];
										Sample sample = samples[sampleNumber];

										sbyte[] sampleData = sample.SampleData;

										if (skipIdCheck || ((sampleData[0] == 'S') && (sampleData[1] == 'S') && (sampleData[2] == 'M') && (sampleData[3] == 'P')))
										{
											int numberOfSubSamples = (sampleData[4] << 8) | (byte)sampleData[5];
											int val2 = (sampleData[6] << 8) | (byte)sampleData[7];

											int subSampleStartOffset = 8 + numberOfSubSamples * 24 + val2 * 4;

											byte subSampleNumber = voiceInfo.FrequencyTable[voiceInfo.FrequencyPosition + 2];
											int subSampleHeaderIndex = 8 + subSampleNumber * 24;

											int start = ((byte)sampleData[subSampleHeaderIndex] << 24) | ((byte)sampleData[subSampleHeaderIndex + 1] << 16) | ((byte)sampleData[subSampleHeaderIndex + 2] << 8) | (byte)sampleData[subSampleHeaderIndex + 3];
											int end = ((byte)sampleData[subSampleHeaderIndex + 4] << 24) | ((byte)sampleData[subSampleHeaderIndex + 5] << 16) | ((byte)sampleData[subSampleHeaderIndex + 6] << 8) | (byte)sampleData[subSampleHeaderIndex + 7];

											if (e9Ands)
											{
												start &= -2;
												end &= -2;
											}

											uint sampleStartOffset = (uint)(subSampleStartOffset + start);

											if (e9FixSample)
												sample.SampleData[sampleStartOffset + 1] = sample.SampleData[sampleStartOffset];

											int length = end - start;

											channel.PlaySample(sampleNumber, sample.SampleData, sampleStartOffset, (uint)(sampleStartOffset + length));

											voiceInfo.EnvelopePosition = 0;
											voiceInfo.EnvelopeCounter = 1;

											voiceInfo.Slide = false;
										}

										voiceInfo.FrequencyPosition += 3;
									}
									break;
								}
							}

							if (!oneMore && !oneMoreBigLoop)
								voiceInfo.Transpose = voiceInfo.FrequencyTable[voiceInfo.FrequencyPosition++];
						}
					}
					while (oneMore);
				}
				else
					voiceInfo.Tick--;
			}
			while (oneMoreBigLoop);
		}



		/********************************************************************/
		/// <summary>
		/// Run effects
		/// </summary>
		/********************************************************************/
		private void RunEffects(VoiceInfo voiceInfo, IChannel channel)
		{
			// Slide
			if (voiceInfo.Slide && !voiceInfo.SlideDone)
			{
				voiceInfo.SlideCounter--;

				if (voiceInfo.SlideCounter < 0)
				{
					voiceInfo.SlideCounter = (sbyte)voiceInfo.SlideSpeed;

					if (voiceInfo.SlideActive)
					{
						int slideValue = voiceInfo.SlideLoopPosition + voiceInfo.SlideDelta;

						if (slideValue < 0)
						{
							voiceInfo.SlideDone = true;
							slideValue = voiceInfo.SlideLoopPosition;
						}
						else
						{
							int slidePosition = slideValue * 2 + voiceInfo.SlideLength * 2;

							if (slidePosition > voiceInfo.SlideEndPosition)
							{
								voiceInfo.SlideDone = true;
								slideValue = voiceInfo.SlideLoopPosition;
							}
						}

						voiceInfo.SlideLoopPosition = (ushort)slideValue;
					}
					else
						voiceInfo.SlideActive = true;

					Sample sample = samples[voiceInfo.SlideSample];

					uint start = voiceInfo.SlideLoopPosition * 2U;
					uint len = voiceInfo.SlideLength * 2U;
					channel.SetSample(sample.SampleData, start, start + len);
					channel.SetLoop(start, len);
				}
			}

			// Envelope
			bool oneMoreBigLoop;

			do
			{
				oneMoreBigLoop = false;

				if (voiceInfo.EnvelopeSustain == 0)
				{
					voiceInfo.EnvelopeCounter--;

					if (voiceInfo.EnvelopeCounter == 0)
					{
						voiceInfo.EnvelopeCounter = voiceInfo.EnvelopeSpeed;

						bool oneMore;

						do
						{
							oneMore = false;

							if (voiceInfo.EnvelopePosition == voiceInfo.EnvelopeTable.Length)
							{
								// Continue into the next envelope. Because envelopes has parameters in the
								// beginning, we need to take them as envelope values as well
								Envelope newEnvelope = envelopes[++voiceInfo.CurrentEnvelopeNumber];

								byte[] newEnvelopeTable = new byte[5 + newEnvelope.EnvelopeTable.Length];
								newEnvelopeTable[0] = newEnvelope.EnvelopeSpeed;
								newEnvelopeTable[1] = newEnvelope.FrequencyNumber;
								newEnvelopeTable[2] = newEnvelope.VibratoSpeed;
								newEnvelopeTable[3] = newEnvelope.VibratoDepth;
								newEnvelopeTable[4] = newEnvelope.VibratoDelay;

								Array.Copy(newEnvelope.EnvelopeTable, 0, newEnvelopeTable, 5, newEnvelope.EnvelopeTable.Length);

								voiceInfo.EnvelopeTable = newEnvelopeTable;
								voiceInfo.EnvelopePosition = 0;
							}

							byte val = voiceInfo.EnvelopeTable[voiceInfo.EnvelopePosition];

							switch (val)
							{
								case 0xe0:
								{
									voiceInfo.EnvelopePosition = (voiceInfo.EnvelopeTable[voiceInfo.EnvelopePosition + 1] & 0x3f) - 5;

									voiceInfo.EnvelopeTable = envelopes[voiceInfo.OriginalEnvelopeNumber].EnvelopeTable;
									voiceInfo.CurrentEnvelopeNumber = voiceInfo.OriginalEnvelopeNumber;
									oneMore = true;
									break;
								}

								case 0xe1:
									break;

								case 0xe8:
								{
									voiceInfo.EnvelopeSustain = voiceInfo.EnvelopeTable[voiceInfo.EnvelopePosition + 1];

									voiceInfo.EnvelopePosition += 2;
									oneMoreBigLoop = true;
									break;
								}

								default:
								{
									voiceInfo.Volume = voiceInfo.EnvelopeTable[voiceInfo.EnvelopePosition++];
									break;
								}
							}
						}
						while (oneMore);
					}
				}
				else
					voiceInfo.EnvelopeSustain--;
			}
			while (oneMoreBigLoop);

			byte note = voiceInfo.Transpose;
			if (note < 128)
				note = (byte)(note + voiceInfo.CurrentNote + voiceInfo.TrackTranspose);

			note &= 0x7f;
			ushort period = periodTable[Math.Min(note, periodTable.Length - 1)];

			// Vibrato
			if (vibratoVersion == 1)
				period = DoVibratoVersion1(voiceInfo, note, period);
			else
				period = DoVibratoVersion2(voiceInfo, period);

			// Portamento
			if ((voiceInfo.CurrentInfo & 0x20) != 0)
			{
				int val = (sbyte)voiceInfo.PreviousInfo;

				if (val >= 0)
				{
					voiceInfo.PortaDelta += (uint)(val << 11);
					period -= (ushort)(voiceInfo.PortaDelta >> 16);
				}
				else
				{
					voiceInfo.PortaDelta -= (uint)(val << 11);
					period += (ushort)(voiceInfo.PortaDelta >> 16);
				}
			}

			channel.SetAmigaPeriod(period);
			channel.SetAmigaVolume(voiceInfo.Volume);
		}



		/********************************************************************/
		/// <summary>
		/// Handle vibrato version 1 code
		/// </summary>
		/********************************************************************/
		private ushort DoVibratoVersion1(VoiceInfo voiceInfo, byte note, ushort period)
		{
			byte vibratoFlag = voiceInfo.VibratoFlag;

			if (voiceInfo.VibratoDelay == 0)
			{
				int vibratoValue = note * 2;
				int vibratoDepth = voiceInfo.VibratoDepth * 2;
				int vibratoDelta = voiceInfo.VibratoDelta;

				if (((vibratoFlag & 0x80) == 0) || ((vibratoFlag & 0x01) == 0))
				{
					if ((vibratoFlag & 0x20) == 0)
					{
						vibratoDelta -= voiceInfo.VibratoSpeed;
						if (vibratoDelta < 0)
						{
							vibratoFlag |= 0x20;
							vibratoDelta = 0;
						}
					}
					else
					{
						vibratoDelta += voiceInfo.VibratoSpeed;

						if (vibratoDelta >= vibratoDepth)
						{
							vibratoFlag = (byte)(vibratoFlag & ~0x20);
							vibratoDelta = vibratoDepth;
						}
					}

					voiceInfo.VibratoDelta = (byte)vibratoDelta;
				}

				vibratoDelta -= voiceInfo.VibratoDepth;

				if (vibratoDelta != 0)
				{
					vibratoValue += 160;

					while (vibratoValue < 256)
					{
						vibratoDelta += vibratoDelta;
						vibratoValue += 24;
					}

					period += (ushort)vibratoDelta;
				}
			}
			else
				voiceInfo.VibratoDelay--;

			vibratoFlag ^= 0x01;
			voiceInfo.VibratoFlag = vibratoFlag;

			return period;
		}



		/********************************************************************/
		/// <summary>
		/// Handle vibrato version 2 code
		/// </summary>
		/********************************************************************/
		private ushort DoVibratoVersion2(VoiceInfo voiceInfo, ushort period)
		{
			byte vibratoFlag = voiceInfo.VibratoFlag;
			int vibratoDepth = voiceInfo.VibratoDepth * 2;
			int vibratoSpeed = voiceInfo.VibratoSpeed;
			int vibratoDelta = voiceInfo.VibratoDelta;

			if (vibratoSpeed >= 128)
			{
				vibratoSpeed &= 0x7f;
				vibratoFlag ^= 0x01;
			}

			if ((vibratoFlag & 0x01) == 0)
			{
				if ((vibratoFlag & 0x20) == 0)
				{
					vibratoDelta -= vibratoSpeed;

					if (vibratoDelta < 0)
					{
						vibratoFlag |= 0x20;
						vibratoDelta = 0;
					}
				}
				else
				{
					vibratoDelta += vibratoSpeed;

					if (vibratoDelta > vibratoDepth)
					{
						vibratoFlag = (byte)(vibratoFlag & ~0x20);
						vibratoDelta = vibratoDepth;
					}
				}
			}

			voiceInfo.VibratoDelta = (byte)vibratoDelta;
			voiceInfo.VibratoFlag = vibratoFlag;

			period += (ushort)(vibratoDelta - voiceInfo.VibratoDepth);

			return period;
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
			ShowPosition();
			ShowTracks();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the playing position
		/// </summary>
		/********************************************************************/
		private string FormatPosition()
		{
			return (voices[0].NextPosition - 1).ToString();
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
				sb.Append(voices[i].CurrentTrackNumber);
				sb.Append(", ");
			}

			sb.Remove(sb.Length - 2, 2);

			return sb.ToString();
		}
		#endregion
	}
}
