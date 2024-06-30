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
		private static readonly Dictionary<Guid, ModuleType> moduleTypeLookup = new Dictionary<Guid, ModuleType>
		{
			{ Hippel.Agent1Id, ModuleType.Hippel },
			{ Hippel.Agent2Id, ModuleType.HippelCoso },
			{ Hippel.Agent3Id, ModuleType.Hippel7V }
		};

		private readonly ModuleType currentModuleType;

		private int startOffset;

		private int numberOfPositions;
		private int numberOfChannels;

		private List<SongInfo> songInfoList;

		private byte[][] frequencies;
		private Envelope[] envelopes;
		private byte[][] tracks;
		private Position[] positionList;
		private Sample[] samples;

		private SongInfo currentSongInfo;

		private GlobalPlayingInfo playingInfo;
		private VoiceInfo[] voices;

		private bool cosoModeEnabled;

		private bool enableMute;
		private bool enableFrequencyPreviousInfo;
		private bool enablePositionEffect;
		private bool enableFrequencyResetCheck;
		private bool enableVolumeFade;
		private bool enableEffectLoop;
		private bool convertEffects;
		private bool skipIdCheck;
		private bool e9Ands;
		private bool e9FixSample;
		private bool resetSustain;
		private int vibratoVersion;
		private int portamentoVersion;

		private int[] effectsEnabled;
		private int speedInitValue;

		private ushort[] periodTable;

		private bool endReached;

		private const int InfoPositionLine = 3;
		private const int InfoTrackLine = 4;
		private const int InfoSpeedLine = 5;
		private const int InfoTempoLine = 6;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public HippelWorker(Guid typeId)
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
		public override string[] FileExtensions => new [] { "hip", "hipc", "hip7" };



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

			if (currentModuleType == ModuleType.HippelCoso)
				return TestModuleForCoso(moduleStream);

			// Read the first part of the file, so it is easier to search
			byte[] buffer = new byte[currentModuleType == ModuleType.Hippel7V ? 32768 : 16384];

			moduleStream.Seek(0, SeekOrigin.Begin);
			moduleStream.Read(buffer, 0, buffer.Length);

			// SC68 support are handled in a converter, so ignore these modules here
			if ((buffer[0] == 0x53) && (buffer[1] == 0x43) && (buffer[2] == 0x36) && (buffer[3] == 0x38))
				return AgentResult.Unknown;

			return currentModuleType == ModuleType.Hippel7V ? TestModuleFor7Voices(buffer, moduleStream) : TestModule(buffer, moduleStream);
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
					value = FormatSpeed();
					break;
				}

				// Current tempo (Hz)
				case 6:
				{
					if (currentModuleType != ModuleType.Hippel7V)
						goto default;

					description = Resources.IDS_HIP_INFODESCLINE6;
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

				moduleStream.Seek(startOffset, SeekOrigin.Begin);

				numberOfChannels = currentModuleType == ModuleType.Hippel7V ? 7 : 4;

				CosoHeader cosoHeader = null;
				if (cosoModeEnabled)
				{
					cosoHeader = LoadCosoHeader(moduleStream);
					if (cosoHeader == null)
					{
						errorMessage = Resources.IDS_HIP_ERR_LOADING_HEADER;
						return AgentResult.Error;
					}
				}

				Header header = LoadHeader(moduleStream);
				if (header == null)
				{
					errorMessage = Resources.IDS_HIP_ERR_LOADING_HEADER;
					return AgentResult.Error;
				}

				if (!LoadFrequencies(moduleStream, header.NumberOfFrequencies + 1, cosoHeader))
				{
					errorMessage = Resources.IDS_HIP_ERR_LOADING_FREQUENCIES;
					return AgentResult.Error;
				}

				if (!LoadEnvelopes(moduleStream, header.NumberOfEnvelopes + 1, cosoHeader))
				{
					errorMessage = Resources.IDS_HIP_ERR_LOADING_ENVELOPES;
					return AgentResult.Error;
				}

				if (!LoadTracks(moduleStream, header.NumberOfTracks + 1, header.BytesPerTrack, cosoHeader))
				{
					errorMessage = Resources.IDS_HIP_ERR_LOADING_TRACKS;
					return AgentResult.Error;
				}

				if (!LoadPositionList(moduleStream, header.NumberOfPositions + 1, cosoHeader))
				{
					errorMessage = Resources.IDS_HIP_ERR_LOADING_POSITIONLIST;
					return AgentResult.Error;
				}

				if (!LoadSongInfo(moduleStream, header.NumberOfSubSongs, cosoHeader))
				{
					errorMessage = Resources.IDS_HIP_ERR_LOADING_SUBSONGS;
					return AgentResult.Error;
				}

				uint[] sampleOffsets = LoadSampleInfo(moduleStream, header.NumberOfSamples, cosoHeader);
				if (sampleOffsets == null)
				{
					errorMessage = Resources.IDS_HIP_ERR_LOADING_SAMPLEINFO;
					return AgentResult.Error;
				}

				if (!LoadSampleData(fileInfo, moduleStream, sampleOffsets, cosoHeader))
				{
					errorMessage = Resources.IDS_HIP_ERR_LOADING_SAMPLES;
					return AgentResult.Error;
				}

				if (cosoModeEnabled)
					EnableCosoFeatures(moduleStream, cosoHeader);
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

				if (cosoModeEnabled)
				{
					for (int i = 0; i < numberOfChannels; i++)
						ReadNextRowCoso(i);
				}
				else if (currentModuleType == ModuleType.Hippel7V)
				{
					for (int i = 0; i < numberOfChannels; i++)
						ReadNextRow7Voices(i);
				}
				else
				{
					for (int i = 0; i < numberOfChannels; i++)
						ReadNextRow(i);
				}
			}

			for (int i = 0; i < numberOfChannels; i++)
				DoEffects(i);

			if (endReached)
			{
				OnEndReached((int)voices[0].NextPosition - 1);
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
						Flags = SampleInfo.SampleFlag.None,
						Type = SampleInfo.SampleType.Sample,
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
						sampleInfo.Flags |= SampleInfo.SampleFlag.Loop;
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
		private AgentResult TestModule(byte[] buffer, ModuleStream moduleStream)
		{
			for (int i = 0; i < buffer.Length - 4; i += 2)
			{
				if ((buffer[i] == 0x41) && (buffer[i + 1] == 0xfa))
				{
					int index = (((sbyte)buffer[i + 2] << 8) | buffer[i + 3]) + i + 2;

					if (index >= (buffer.Length - 4))
						return AgentResult.Unknown;

					if ((buffer[index] == 'T') && (buffer[index + 1] == 'F') && (buffer[index + 2] == 'M') && (buffer[index + 3] == 'X'))
					{
						if (Has7VoicesStructures(moduleStream, index))
							return AgentResult.Unknown;

						startOffset = index;
						cosoModeEnabled = false;

						if (FindFeatures(buffer, startOffset) != AgentResult.Ok)
							return AgentResult.Unknown;

						enablePositionEffect = false;
						enableFrequencyResetCheck = false;
						enableVolumeFade = false;
						convertEffects = false;
						resetSustain = true;
						portamentoVersion = 1;

						return AgentResult.Ok;
					}
				}
			}

			return AgentResult.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it's COSO format
		/// </summary>
		/********************************************************************/
		private AgentResult TestModuleForCoso(ModuleStream moduleStream)
		{
			// Check the two marks
			moduleStream.Seek(0, SeekOrigin.Begin);
			uint mark = moduleStream.Read_B_UINT32();

			if (mark != 0x434f534f)		// COSO
				return AgentResult.Unknown;

			moduleStream.Seek(32, SeekOrigin.Begin);
			mark = moduleStream.Read_B_UINT32();

			if (mark != 0x54464d58)		// TFMX
				return AgentResult.Unknown;

			// If number of samples is 0, it is probably an ST module
			moduleStream.Seek(50, SeekOrigin.Begin);

			ushort numberOfSamples = moduleStream.Read_B_UINT16();
			if (numberOfSamples == 0)
				return AgentResult.Unknown;

			// I do not know what the next word stands for, but it seems that
			// some ST modules has non-zero here
			if (moduleStream.Read_B_UINT16() != 0)
				return AgentResult.Unknown;

			// Still, some ST modules have a count, but it does not match the difference
			// between sub-song and sample info as for Amiga modules
			moduleStream.Seek(24, SeekOrigin.Begin);

			uint offset = moduleStream.Read_B_UINT32();

			moduleStream.Seek(offset + numberOfSamples * 4, SeekOrigin.Begin);
			if ((moduleStream.Read_B_UINT16() == 0) && (moduleStream.Read_B_UINT16() == 0))
				return AgentResult.Unknown;

			// Last check, is it 7 voices COSO module
			if (Has7VoicesStructuresInCoso(moduleStream, 0))
				return AgentResult.Unknown;

			startOffset = 0;
			cosoModeEnabled = true;

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it's a Jochen Hippel 7 voices player
		/// </summary>
		/********************************************************************/
		private AgentResult TestModuleFor7Voices(byte[] buffer, ModuleStream moduleStream)
		{
			// Check for clean TFMX format
			if ((buffer.Length > 4) && (buffer[0] == 'T') && (buffer[1] == 'F') && (buffer[2] == 'M') && (buffer[3] == 'X') && (buffer[4] != ' '))
			{
				if (!Has7VoicesStructures(moduleStream, 0))
					return AgentResult.Unknown;

				startOffset = 0;
				cosoModeEnabled = false;

				SetDefault7VoicesFeatures();

				return AgentResult.Ok;
			}

			// Check for COSO format
			if ((buffer.Length > 36) && (buffer[0] == 'C') && (buffer[1] == 'O') && (buffer[2] == 'S') && (buffer[3] == 'O') &&
				(buffer[32] == 'T') && (buffer[33] == 'F') && (buffer[34] == 'M') && (buffer[35] == 'X'))
			{
				if (!Has7VoicesStructuresInCoso(moduleStream, 0))
					return AgentResult.Unknown;

				startOffset = 0;
				cosoModeEnabled = true;

				SetDefault7VoicesFeatures();

				return AgentResult.Ok;
			}

			// Embedded player check
			for (int i = 0; i < buffer.Length - 4; i += 2)
			{
				if ((buffer[i] == 0x41) && (buffer[i + 1] == 0xfa))
				{
					int index = (((sbyte)buffer[i + 2] << 8) | buffer[i + 3]) + i + 2;

					if ((index < 0) || (index >= (buffer.Length - 4)))
						return AgentResult.Unknown;

					if ((buffer[index] == 'T') && (buffer[index + 1] == 'F') && (buffer[index + 2] == 'M') && (buffer[index + 3] == 'X'))
					{
						if (!Has7VoicesStructures(moduleStream, index))
							return AgentResult.Unknown;

						startOffset = index;
						cosoModeEnabled = false;

						if (FindFeatures(buffer, startOffset) != AgentResult.Ok)
							return AgentResult.Unknown;

						enablePositionEffect = false;
						enableFrequencyResetCheck = true;
						enableVolumeFade = true;
						convertEffects = false;
						resetSustain = true;
						portamentoVersion = 2;

						return AgentResult.Ok;
					}
				}
			}

			return AgentResult.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Check if the TFMX structures contains 7 voices information
		/// </summary>
		/********************************************************************/
		private bool Has7VoicesStructures(ModuleStream moduleStream, int headerIndex)
		{
			moduleStream.Seek(headerIndex + 4, SeekOrigin.Begin);

			int offset = 0x20;

			ushort value = moduleStream.Read_B_UINT16();	// Frequency tables
			offset += (value + 1) * 64;

			value = moduleStream.Read_B_UINT16();			// Envelope tables
			offset += (value + 1) * 64;

			ushort trks = moduleStream.Read_B_UINT16();		// Tracks
			ushort positions = moduleStream.Read_B_UINT16();
			ushort bytesPerTrack = moduleStream.Read_B_UINT16();

			moduleStream.Seek(2, SeekOrigin.Current);
			ushort subSongs = moduleStream.Read_B_UINT16();

			offset += (trks + 1) * bytesPerTrack;
			offset += (positions + 1) * 4 * 7;
			offset += subSongs * 8;

			if ((headerIndex + offset) >= moduleStream.Length)
				return false;

			// If it's 7 voices, there should be 4 words with are all zero
			moduleStream.Seek(headerIndex + offset, SeekOrigin.Begin);

			if ((moduleStream.Read_B_UINT32() != 0) || (moduleStream.Read_B_UINT32() != 0))
				return false;

			// Now the sample information should start, so check
			// the first two letters in the sample name
			byte c1 = moduleStream.Read_UINT8();
			byte c2 = moduleStream.Read_UINT8();

			if ((c1 < 0x20) || (c2 < 0x20) || (c1 > 0x7f) || (c2 > 0x7f))
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Check if the TFMX structures contains 7 voices information in a
		/// COSO module
		/// </summary>
		/********************************************************************/
		private bool Has7VoicesStructuresInCoso(ModuleStream moduleStream, int headerIndex)
		{
			moduleStream.Seek(headerIndex + 20, SeekOrigin.Begin);
			int subSongOffset = moduleStream.Read_B_INT32();
			int sampleDataOffset = moduleStream.Read_B_INT32();

			moduleStream.Seek(headerIndex + 48, SeekOrigin.Begin);
			ushort subSongs = moduleStream.Read_B_UINT16();

			subSongOffset += subSongs * 8;

			if ((headerIndex + subSongOffset) >= moduleStream.Length)
				return false;

			// If it's 7 voices, there should be 4 words with are all zero
			moduleStream.Seek(headerIndex + subSongOffset, SeekOrigin.Begin);

			if ((moduleStream.Read_B_UINT32() != 0) || (moduleStream.Read_B_UINT32() != 0))
				return false;

			if (moduleStream.Position != sampleDataOffset)
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Set default features for 7 voices modules
		/// </summary>
		/********************************************************************/
		private void SetDefault7VoicesFeatures()
		{
			effectsEnabled = new int[16];

			enableMute = false;
			enableFrequencyPreviousInfo = true;
			enablePositionEffect = false;
			enableFrequencyResetCheck = true;
			enableVolumeFade = true;
			enableEffectLoop = true;
			convertEffects = false;
			skipIdCheck = true;
			e9Ands = true;
			e9FixSample = true;
			resetSustain = true;

			vibratoVersion = 3;
			portamentoVersion = 2;

			speedInitValue = 1;

			effectsEnabled[2] = 1;
			effectsEnabled[3] = 1;
			effectsEnabled[4] = 1;
			effectsEnabled[5] = 2;
			effectsEnabled[6] = 1;
			effectsEnabled[7] = 2;
			effectsEnabled[8] = 1;
			effectsEnabled[9] = 1;
			effectsEnabled[10] = 1;

			periodTable = Tables.Periods2;
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
				if ((searchBuffer[index] == 0x0c) && (searchBuffer[index + 4] == 0x67) && (searchBuffer[index + 8] == 0x0c))
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
				enableEffectLoop = false;
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
				enableEffectLoop = true;
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
			else if ((searchBuffer[index] == 0x72) && (searchBuffer[index + 1] == 0x00) && (searchBuffer[index + 2] == 0x78) && (searchBuffer[index + 3] == 0x00))
				vibratoVersion = 3;
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
				if (((searchBuffer[index] == 0x3c) && (searchBuffer[index + 1] == 0xd9) && (searchBuffer[index + 2] == 0x61) && (searchBuffer[index + 3] == 0x00)) ||
					((searchBuffer[index] == 0x3c) && (searchBuffer[index + 1] == 0xe9) && (searchBuffer[index + 4] == 0x61) && (searchBuffer[index + 5] == 0x00)))
					break;
			}

			if (index >= (searchLength - 6))
				return AgentResult.Unknown;

			if (searchBuffer[index + 1] == 0xe9)
				index += 2;

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
		/// Load COSO header information
		/// </summary>
		/********************************************************************/
		private CosoHeader LoadCosoHeader(ModuleStream moduleStream)
		{
			CosoHeader cosoHeader = new CosoHeader();

			// Skip ID
			moduleStream.Seek(4, SeekOrigin.Current);

			cosoHeader.FrequenciesOffset = moduleStream.Read_B_UINT32();
			cosoHeader.EnvelopesOffset = moduleStream.Read_B_UINT32();
			cosoHeader.TracksOffset = moduleStream.Read_B_UINT32();
			cosoHeader.PositionListOffset = moduleStream.Read_B_UINT32();
			cosoHeader.SubSongsOffset = moduleStream.Read_B_UINT32();
			cosoHeader.SampleInfoOffset = moduleStream.Read_B_UINT32();
			cosoHeader.SampleDataOffset = moduleStream.Read_B_INT32();

			if (moduleStream.EndOfStream)
				return null;

			if (cosoHeader.SampleDataOffset == moduleStream.Length)
			{
				// Sample data is stored in an external file, so mark that
				cosoHeader.SampleDataOffset = -1;
			}

			return cosoHeader;
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

			return header;
		}



		/********************************************************************/
		/// <summary>
		/// Load frequency tables
		/// </summary>
		/********************************************************************/
		private bool LoadFrequencies(ModuleStream moduleStream, int numberOfFrequencies, CosoHeader cosoHeader)
		{
			frequencies = new byte[numberOfFrequencies][];

			if (cosoModeEnabled)
			{
				if ((cosoHeader.FrequenciesOffset == 0) || (cosoHeader.EnvelopesOffset == 0))
					return false;

				moduleStream.Seek(startOffset + cosoHeader.FrequenciesOffset, SeekOrigin.Begin);

				ushort[] offsets = new ushort[numberOfFrequencies + 1];
				moduleStream.ReadArray_B_UINT16s(offsets, 0, numberOfFrequencies);
				offsets[numberOfFrequencies] = (ushort)cosoHeader.EnvelopesOffset;

				if (moduleStream.EndOfStream)
					return false;

				for (int i = 0; i < numberOfFrequencies; i++)
				{
					int length = offsets[i + 1] - offsets[i];

					for (int j = 2; (length == 0) && (i + j < numberOfFrequencies); j++)		// If length is 0, it means that the envelope table expands over multiple envelopes, but only the last part is stored
						length = offsets[i + j] - offsets[i];

					if (length == 0)
						frequencies[i] = [];
					else
					{
						moduleStream.Seek(startOffset + offsets[i], SeekOrigin.Begin);

						frequencies[i] = new byte[length];

						moduleStream.Read(frequencies[i], 0, length);
						if (moduleStream.EndOfStream)
							return false;
					}
				}
			}
			else
			{
				for (int i = 0; i < numberOfFrequencies; i++)
				{
					frequencies[i] = new byte[64];

					moduleStream.Read(frequencies[i], 0, 64);
					if (moduleStream.EndOfStream)
						return false;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load envelope tables
		/// </summary>
		/********************************************************************/
		private bool LoadEnvelopes(ModuleStream moduleStream, int numberOfEnvelopes, CosoHeader cosoHeader)
		{
			envelopes = new Envelope[numberOfEnvelopes];

			Envelope ReadEnvelope(int length)
			{
				Envelope envelope = new Envelope();

				envelope.EnvelopeSpeed = moduleStream.Read_UINT8();
				envelope.FrequencyNumber = moduleStream.Read_UINT8();
				envelope.VibratoSpeed = moduleStream.Read_UINT8();
				envelope.VibratoDepth = moduleStream.Read_UINT8();
				envelope.VibratoDelay = moduleStream.Read_UINT8();
				envelope.EnvelopeTable = new byte[Math.Max(0, length - 5)];

				moduleStream.Read(envelope.EnvelopeTable, 0, envelope.EnvelopeTable.Length);
				if (moduleStream.EndOfStream)
					return null;

				return envelope;
			}

			if (cosoModeEnabled)
			{
				if ((cosoHeader.EnvelopesOffset == 0) || (cosoHeader.TracksOffset == 0))
					return false;

				moduleStream.Seek(startOffset + cosoHeader.EnvelopesOffset, SeekOrigin.Begin);

				ushort[] offsets = new ushort[numberOfEnvelopes + 1];
				moduleStream.ReadArray_B_UINT16s(offsets, 0, numberOfEnvelopes);
				offsets[numberOfEnvelopes] = (ushort)cosoHeader.TracksOffset;

				if (moduleStream.EndOfStream)
					return false;

				for (int i = 0; i < numberOfEnvelopes; i++)
				{
					int length = offsets[i + 1] - offsets[i];

					for (int j = 2; (length == 0) && (i + j < numberOfEnvelopes); j++)		// If length is 0, it means that the envelope table expands over multiple envelopes, but only the last part is stored
						length = offsets[i + j] - offsets[i];

					if (length == 0)
						envelopes[i] = new Envelope();
					else
					{
						moduleStream.Seek(startOffset + offsets[i], SeekOrigin.Begin);

						envelopes[i] = ReadEnvelope(length);
						if (envelopes[i] == null)
							return false;
					}
				}
			}
			else
			{
				for (int i = 0; i < numberOfEnvelopes; i++)
				{
					envelopes[i] = ReadEnvelope(64);
					if (envelopes[i] == null)
						return false;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load tracks
		/// </summary>
		/********************************************************************/
		private bool LoadTracks(ModuleStream moduleStream, int numberOfTracks, ushort bytesPerTrack, CosoHeader cosoHeader)
		{
			tracks = new byte[numberOfTracks][];

			if (cosoModeEnabled)
			{
				if ((cosoHeader.TracksOffset == 0) || (cosoHeader.PositionListOffset == 0))
					return false;

				moduleStream.Seek(startOffset + cosoHeader.TracksOffset, SeekOrigin.Begin);

				ushort[] offsets = new ushort[numberOfTracks + 1];
				moduleStream.ReadArray_B_UINT16s(offsets, 0, numberOfTracks);
				offsets[numberOfTracks] = (ushort)cosoHeader.PositionListOffset;

				if (moduleStream.EndOfStream)
					return false;

				for (int i = 0; i < numberOfTracks; i++)
				{
					int length = offsets[i + 1] - offsets[i];
					tracks[i] = new byte[length];

					moduleStream.Read(tracks[i], 0, tracks[i].Length);
					if (moduleStream.EndOfStream)
						return false;
				}
			}
			else
			{
				for (int i = 0; i < numberOfTracks; i++)
				{
					tracks[i] = new byte[bytesPerTrack];

					moduleStream.Read(tracks[i], 0, tracks[i].Length);
					if (moduleStream.EndOfStream)
						return false;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load position list
		/// </summary>
		/********************************************************************/
		private bool LoadPositionList(ModuleStream moduleStream, int numOfPositions, CosoHeader cosoHeader)
		{
			positionList = new Position[numOfPositions];

			if (cosoModeEnabled)
			{
				if (cosoHeader.PositionListOffset == 0)
					return false;

				moduleStream.Seek(startOffset + cosoHeader.PositionListOffset, SeekOrigin.Begin);
			}

			for (int i = 0; i < numOfPositions; i++)
			{
				Position position = new Position
				{
					PositionInfo = new SinglePositionInfo[numberOfChannels]
				};

				for (int j = 0; j < numberOfChannels; j++)
				{
					SinglePositionInfo singlePositionInfo = new SinglePositionInfo();

					singlePositionInfo.Track = moduleStream.Read_UINT8();
					singlePositionInfo.NoteTranspose = moduleStream.Read_INT8();
					singlePositionInfo.EnvelopeTranspose = moduleStream.Read_INT8();

					if (currentModuleType == ModuleType.Hippel7V)
						singlePositionInfo.Command = moduleStream.Read_UINT8();

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
		private bool LoadSongInfo(ModuleStream moduleStream, int numberOfSubSongs, CosoHeader cosoHeader)
		{
			songInfoList = new List<SongInfo>();

			if (cosoModeEnabled)
			{
				if (cosoHeader.SubSongsOffset == 0)
					return false;

				moduleStream.Seek(startOffset + cosoHeader.SubSongsOffset, SeekOrigin.Begin);
			}

			for (int i = 0; i < numberOfSubSongs; i++)
			{
				SongInfo songInfo = new SongInfo();

				songInfo.StartPosition = moduleStream.Read_B_UINT16();
				songInfo.LastPosition = (ushort)(moduleStream.Read_B_UINT16() + 1);

				if (currentModuleType == ModuleType.Hippel7V)
					moduleStream.Seek(2, SeekOrigin.Current);

				songInfo.StartSpeed = moduleStream.Read_B_UINT16();

				if (moduleStream.EndOfStream)
					return false;

				if ((songInfo.StartSpeed != 0) && (songInfo.StartPosition <= songInfo.LastPosition))
					songInfoList.Add(songInfo);
			}

			// Skip sub-song paddings
			moduleStream.Seek(currentModuleType == ModuleType.Hippel7V ? 8 : 6, SeekOrigin.Current);

			numberOfPositions = songInfoList.Max(x => x.LastPosition);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the sample information
		/// </summary>
		/********************************************************************/
		private uint[] LoadSampleInfo(ModuleStream moduleStream, int numberOfSamples, CosoHeader cosoHeader)
		{
			samples = new Sample[numberOfSamples];
			uint[] sampleOffsets = new uint[numberOfSamples];

			Encoding encoder = EncoderCollection.Amiga;
			byte[] buf = new byte[18];

			if (cosoModeEnabled)
			{
				if (cosoHeader.SampleInfoOffset == 0)
					return null;

				moduleStream.Seek(startOffset + cosoHeader.SampleInfoOffset, SeekOrigin.Begin);
			}

			for (int i = 0; i < numberOfSamples; i++)
			{
				Sample sample = new Sample();

				if (!cosoModeEnabled)
				{
					moduleStream.Read(buf, 0, 18);
					if (moduleStream.EndOfStream)
						return null;

					sample.Name = encoder.GetString(buf);
				}
				else
					sample.Name = string.Empty;

				sampleOffsets[i] = moduleStream.Read_B_UINT32();

				sample.Length = moduleStream.Read_B_UINT16() * 2U;

				if (cosoModeEnabled)
					sample.Volume = 64;
				else
				{
					sample.Volume = moduleStream.Read_B_UINT16();

					if (currentModuleType == ModuleType.Hippel7V)
						sample.Volume = 64;
				}

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
		private bool LoadSampleData(PlayerFileInfo fileInfo, ModuleStream moduleStream, uint[] sampleOffsets, CosoHeader cosoHeader)
		{
			ModuleStream extraSampleStream = null;

			try
			{
				if (cosoModeEnabled)
				{
					if (cosoHeader.SampleDataOffset == -1)
					{
						extraSampleStream = fileInfo.Loader?.OpenExtraFileByExtension("samp");
						if (extraSampleStream == null)
							return false;

						moduleStream = extraSampleStream;
					}
					else
						moduleStream.Seek(startOffset + cosoHeader.SampleDataOffset, SeekOrigin.Begin);
				}

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
			}
			finally
			{
				extraSampleStream?.Dispose();
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize sound structures
		/// </summary>
		/********************************************************************/
		private void EnableCosoFeatures(ModuleStream moduleStream, CosoHeader cosoHeader)
		{
			// Enable features based on module. Calculate a checksum to find out which
			// module it is. This list is taken from Flod by Christian Corti (https://www.neoartcr.com/flod.htm)
			long checksum = cosoHeader.FrequenciesOffset + cosoHeader.EnvelopesOffset + cosoHeader.TracksOffset + cosoHeader.PositionListOffset +
			               cosoHeader.SubSongsOffset + cosoHeader.SampleInfoOffset + cosoHeader.SampleDataOffset;

			moduleStream.Seek(startOffset + 47, SeekOrigin.Begin);
			checksum += moduleStream.Read_UINT8();

			effectsEnabled = new int[16];

			switch (checksum)
			{
				case 22660:		// Astaroth
				case 22670:
				case 18845:
				case 30015:		// Chambers of Shaolin
				case 22469:
				case 3549:		// Over the net
				{
					enableMute = true;
					enablePositionEffect = false;
					enableFrequencyResetCheck = false;
					enableVolumeFade = false;
					convertEffects = false;

					vibratoVersion = 1;
					portamentoVersion = 1;

					effectsEnabled[2] = 1;
					effectsEnabled[3] = 1;
					effectsEnabled[4] = 1;
					effectsEnabled[7] = 1;
					effectsEnabled[8] = 1;
					break;
				}

				case 16948:		// Dragonflight
				case 18337:
				case 13704:
				{
					enableMute = false;
					enablePositionEffect = false;
					enableFrequencyResetCheck = true;
					enableVolumeFade = false;
					convertEffects = false;

					vibratoVersion = 1;
					portamentoVersion = 1;

					effectsEnabled[2] = 1;
					effectsEnabled[3] = 1;
					effectsEnabled[4] = 1;
					effectsEnabled[5] = 1;
					effectsEnabled[7] = 2;
					effectsEnabled[8] = 1;
					effectsEnabled[9] = 1;
					break;
				}

				case 18548:		// Wings of Death
				case 13928:
				case 8764:
				case 17244:
				case 11397:
				case 14496:
				case 14394:
				case 13578:		// Dragonflight
				case 6524:
				{
					enableMute = false;
					enablePositionEffect = false;
					enableFrequencyResetCheck = true;
					enableVolumeFade = false;
					convertEffects = true;

					vibratoVersion = 2;
					portamentoVersion = 1;

					effectsEnabled[2] = 1;
					effectsEnabled[3] = 1;
					effectsEnabled[4] = 1;
					effectsEnabled[5] = 2;
					effectsEnabled[6] = 1;
					effectsEnabled[7] = 2;
					effectsEnabled[8] = 1;
					effectsEnabled[9] = 1;
					break;
				}

				default:
				{
					enableMute = false;
					enablePositionEffect = true;
					enableFrequencyResetCheck = true;
					enableVolumeFade = true;
					convertEffects = false;

					vibratoVersion = 3;
					portamentoVersion = 2;

					effectsEnabled[2] = 1;
					effectsEnabled[3] = 1;
					effectsEnabled[4] = 1;
					effectsEnabled[5] = 2;
					effectsEnabled[6] = 1;
					effectsEnabled[7] = 2;
					effectsEnabled[8] = 1;
					effectsEnabled[9] = 1;
					effectsEnabled[10] = 1;
					break;
				}
			}

			enableFrequencyPreviousInfo = true;
			skipIdCheck = true;
			e9Ands = true;
			e9FixSample = true;
			enableEffectLoop = true;
			resetSustain = false;

			speedInitValue = 1;

			periodTable = Tables.Periods2;
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
				Speed = currentSongInfo.StartSpeed,
				Random = 0
			};

			voices = new VoiceInfo[numberOfChannels];

			for (int i = 0; i < numberOfChannels; i++)
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

					NextPosition = currentSongInfo.StartPosition + (currentModuleType == ModuleType.Hippel7V ? 0U : 1U),

					CurrentTrackNumber = singlePositionInfo.Track,
					Track = tracks[singlePositionInfo.Track],
					TrackPosition = currentModuleType == ModuleType.Hippel7V ? (uint)tracks[singlePositionInfo.Track].Length : 0U,
					TrackTranspose = singlePositionInfo.NoteTranspose,
					EnvelopeTranspose = singlePositionInfo.EnvelopeTranspose,

					Transpose = 0,
					CurrentNote = 0,
					CurrentInfo = 0,
					PreviousInfo = 0,

					Sample = 0xff,

					Tick = 0,

					CosoCounter = 0,
					CosoSpeed = 0,

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
					SlideDone = false,

					VolumeFade = 100,
					VolumeVariationDepth = 0,
					VolumeVariation = 0
				};

				voices[i] = voiceInfo;
			}

			if (currentModuleType == ModuleType.Hippel7V)
				SetTempo(positionList[0].PositionInfo[0].EnvelopeTranspose);
			else
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

			if ((voiceInfo.TrackPosition == voiceInfo.Track.Length) || ((voiceInfo.Track[voiceInfo.TrackPosition] & 0x7f) == 1))
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

				voiceInfo.PreviousInfo = voiceInfo.Track[(voiceInfo.TrackPosition == 0 ? voiceInfo.Track.Length : voiceInfo.TrackPosition) - 1];
				voiceInfo.CurrentInfo = voiceInfo.Track[voiceInfo.TrackPosition + 1];

				if (val < 128)
					SetupEnvelope(voice);
			}

			voiceInfo.TrackPosition += 2;
		}



		/********************************************************************/
		/// <summary>
		/// Read the next row for a single channel in COSO mode
		/// </summary>
		/********************************************************************/
		private void ReadNextRowCoso(int voice)
		{
			VoiceInfo voiceInfo = voices[voice];
			IChannel channel = VirtualChannels[voice];

			voiceInfo.CosoCounter--;
			if (voiceInfo.CosoCounter < 0)
			{
				voiceInfo.CosoCounter = voiceInfo.CosoSpeed;

				bool oneMore;

				do
				{
					oneMore = false;

					byte val = (currentModuleType == ModuleType.Hippel7V) && (voiceInfo.TrackPosition == voiceInfo.Track.Length) ? (byte)0xff : voiceInfo.Track[voiceInfo.TrackPosition];

					switch (val)
					{
						case 0xff:
						{
							if (voiceInfo.NextPosition == currentSongInfo.LastPosition)
							{
								voiceInfo.NextPosition = currentSongInfo.StartPosition;
								endReached = true;
							}

							SinglePositionInfo positionInfo = positionList[voiceInfo.NextPosition].PositionInfo[voice];

							voiceInfo.TrackTranspose = positionInfo.NoteTranspose;
							val = (byte)positionInfo.EnvelopeTranspose;

							if (enablePositionEffect && (val > 127))
							{
								byte val2 = (byte)((val >> 4) & 15);
								val &= 15;

								if (val2 == 15)
								{
									val2 = 100;

									if (val != 0)
										val2 = (byte)((15 - val + 1) * 6);

									voiceInfo.VolumeFade = val2;
								}
								else if (val2 == 8)
								{
									// Song stopped
									endReached = true;

									voiceInfo.NextPosition = currentSongInfo.StartPosition;

									positionInfo = positionList[voiceInfo.NextPosition].PositionInfo[voice];

									voiceInfo.TrackTranspose = positionInfo.NoteTranspose;
									voiceInfo.EnvelopeTranspose = positionInfo.EnvelopeTranspose;
								}
								else if (val2 == 14)
								{
									playingInfo.Speed = (ushort)(val & 15);
									ShowSpeed();
								}
							}
							else
								voiceInfo.EnvelopeTranspose = (sbyte)val;

							if (currentModuleType == ModuleType.Hippel7V)
								ParsePositionCommand(voice, ref positionInfo);

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

							oneMore = true;
							break;
						}

						case 0xfe:
						{
							voiceInfo.CosoSpeed = (sbyte)voiceInfo.Track[voiceInfo.TrackPosition + 1];
							voiceInfo.CosoCounter = voiceInfo.CosoSpeed;

							voiceInfo.TrackPosition += 2;
							oneMore = true;
							break;
						}

						case 0xfd:
						{
							voiceInfo.CosoSpeed = (sbyte)voiceInfo.Track[voiceInfo.TrackPosition + 1];
							voiceInfo.CosoCounter = voiceInfo.CosoSpeed;

							voiceInfo.TrackPosition += 2;
							return;
						}

						default:
						{
							voiceInfo.CurrentNote = val;
							voiceInfo.CurrentInfo = voiceInfo.Track[voiceInfo.TrackPosition + 1];

							if ((voiceInfo.CurrentInfo & 0xe0) != 0)
							{
								voiceInfo.PreviousInfo = voiceInfo.Track[voiceInfo.TrackPosition + 2];
								voiceInfo.TrackPosition += 3;
							}
							else
								voiceInfo.TrackPosition += 2;

							voiceInfo.PortaDelta = 0;

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
								voiceInfo.EnvelopeSustain = 0;

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

								if (!enableFrequencyResetCheck || (frequencyNumber != 0x80))
								{
									if ((voiceInfo.CurrentInfo & 0x40) != 0)
										frequencyNumber = voiceInfo.PreviousInfo;

									if (frequencyNumber < frequencies.Length)
										voiceInfo.FrequencyTable = frequencies[frequencyNumber];
									else
										voiceInfo.FrequencyTable = Tables.DefaultCommandTable;

									voiceInfo.FrequencyPosition = 0;
									voiceInfo.OriginalFrequencyNumber = frequencyNumber;
									voiceInfo.CurrentFrequencyNumber = frequencyNumber;

									voiceInfo.Tick = 0;
								}
							}
							break;
						}
					}
				}
				while (oneMore);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Read the next row for a single channel in 7 voices mode
		/// </summary>
		/********************************************************************/
		private void ReadNextRow7Voices(int voice)
		{
			VoiceInfo voiceInfo = voices[voice];

			if ((voiceInfo.TrackPosition == voiceInfo.Track.Length) || ((voiceInfo.Track[voiceInfo.TrackPosition] & 0x7f) == 1))
			{
				if (voiceInfo.NextPosition == currentSongInfo.LastPosition)
				{
					voiceInfo.NextPosition = currentSongInfo.StartPosition;
					endReached = true;
				}

				SinglePositionInfo positionInfo = positionList[voiceInfo.NextPosition].PositionInfo[voice];

				voiceInfo.TrackTranspose = positionInfo.NoteTranspose;
				voiceInfo.EnvelopeTranspose = positionInfo.EnvelopeTranspose;

				ParsePositionCommand(voice, ref positionInfo);

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

				voiceInfo.PreviousInfo = voiceInfo.Track[(voiceInfo.TrackPosition == 0 ? voiceInfo.Track.Length : voiceInfo.TrackPosition) - 1];
				voiceInfo.CurrentInfo = voiceInfo.Track[voiceInfo.TrackPosition + 1];

				if (val < 128)
					SetupEnvelope(voice);
			}

			voiceInfo.TrackPosition += 2;
		}



		/********************************************************************/
		/// <summary>
		/// Will check the position command and execute them (only used in
		/// 7 voices modules)
		/// </summary>
		/********************************************************************/
		private void ParsePositionCommand(int voice, ref SinglePositionInfo positionInfo)
		{
			VoiceInfo voiceInfo = voices[voice];

			switch (positionInfo.Command & 0xf0)
			{
				case 0x80:
				{
					// Song stopped
					endReached = true;

					voiceInfo.NextPosition = currentSongInfo.StartPosition;

					positionInfo = positionList[voiceInfo.NextPosition].PositionInfo[voice];

					voiceInfo.TrackTranspose = positionInfo.NoteTranspose;
					voiceInfo.EnvelopeTranspose = positionInfo.EnvelopeTranspose;
					break;
				}

				case 0xd0:
				{
					SinglePositionInfo getFrom;

					if (voice == 6)
						getFrom = positionList[voiceInfo.NextPosition + 1].PositionInfo[0];
					else
						getFrom = positionList[voiceInfo.NextPosition].PositionInfo[voice + 1];

					SetTempo((sbyte)getFrom.Command);
					break;
				}

				case 0xe0:
				{
					playingInfo.Speed = (ushort)(positionInfo.Command & 0x0f);
					ShowSpeed();
					break;
				}

				case 0xf0:
				{
					byte cmdArg = (byte)(positionInfo.Command & 0x0f);
					voiceInfo.VolumeFade = (byte)(cmdArg == 0 ? 64 : (15 - cmdArg + 1) * 6);
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Setup the envelope
		/// </summary>
		/********************************************************************/
		private void SetupEnvelope(int voice)
		{
			VoiceInfo voiceInfo = voices[voice];
			IChannel channel = VirtualChannels[voice];

			if (enableMute)
				channel.Mute();

			byte val = (byte)((voiceInfo.CurrentInfo & 0x1f) + voiceInfo.EnvelopeTranspose);
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

			if (!enableFrequencyResetCheck || (frequencyNumber != 0x80))
			{
				if (enableFrequencyPreviousInfo && ((voiceInfo.CurrentInfo & 0x40) != 0))
					frequencyNumber = voiceInfo.PreviousInfo;

				voiceInfo.FrequencyTable = frequencies[frequencyNumber];
				voiceInfo.FrequencyPosition = 0;
				voiceInfo.OriginalFrequencyNumber = frequencyNumber;
				voiceInfo.CurrentFrequencyNumber = frequencyNumber;

				voiceInfo.Tick = 0;

				if (resetSustain)
					voiceInfo.EnvelopeSustain = 0;
			}
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

			if (numberOfChannels == 7)
				channel.SetPanning((ushort)Tables.Pan7[voice]);
			else
				channel.SetPanning((ushort)Tables.Pan4[voice]);
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

						if (voiceInfo.FrequencyPosition == voiceInfo.FrequencyTable.Length)
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

							if (convertEffects)
							{
								if (val == 0xe5)
									val = 0xe2;
								else if (val == 0xe6)
									val = 0xe4;
							}

							switch (val)
							{
								case 0xe2:
								{
									if (effectsEnabled[2] != 0)
									{
										voiceInfo.VolumeVariationDepth = 0;
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
										oneMore = enableEffectLoop;
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
										oneMore = enableEffectLoop;
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
										oneMore = enableEffectLoop;
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

											voiceInfo.VolumeVariationDepth = 0;

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
											oneMore = true;
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
										oneMore = true;
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
											oneMore = enableEffectLoop;
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
										voiceInfo.VolumeVariationDepth = 0;
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
										oneMore = enableEffectLoop;
									}
									break;
								}

								case 0xea:
								{
									if (effectsEnabled[10] != 0)
									{
										voiceInfo.VolumeVariationDepth = voiceInfo.FrequencyTable[voiceInfo.FrequencyPosition + 1];
										voiceInfo.VolumeVariation = 0;

										voiceInfo.FrequencyPosition += 2;
										oneMore = true;
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
					uint len = Math.Min(voiceInfo.SlideLength * 2U, sample.Length - start);
					if (len == 0)
						len = sample.Length - start;	// Fix for Ninja 2

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
			else if (vibratoVersion == 2)
				period = DoVibratoVersion2(voiceInfo, period);
			else
				period = DoVibratoVersion3(voiceInfo, period);

			// Portamento
			if (portamentoVersion == 1)
				period = DoPortamentoVersion1(voiceInfo, period);
			else
				period = DoPortamentoVersion2(voiceInfo, period);

			channel.SetAmigaPeriod(period);

			if (enableVolumeFade)
			{
				byte volume = voiceInfo.Volume;
				byte volFade = voiceInfo.VolumeFade;

				byte volDepth = voiceInfo.VolumeVariationDepth;
				if (volDepth != 0)
				{
					byte volVariation = voiceInfo.VolumeVariation;
					if (volVariation == 0)
					{
						voiceInfo.VolumeVariationDepth = 0;

						int rnd = GetRandomVariation() & 0xff;
						voiceInfo.VolumeVariation = (byte)(volDepth * rnd / 255);
						volVariation = voiceInfo.VolumeVariation;
					}

					volFade -= volVariation;
				}

				if (volFade > 127)
					volFade = 0;

				channel.SetAmigaVolume((ushort)(volFade * volume / 100));
			}
			else
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
			if (voiceInfo.VibratoDelay == 0)
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
			}
			else
				voiceInfo.VibratoDelay--;

			return period;
		}



		/********************************************************************/
		/// <summary>
		/// Handle vibrato version 3 code
		/// </summary>
		/********************************************************************/
		private ushort DoVibratoVersion3(VoiceInfo voiceInfo, ushort period)
		{
			if (voiceInfo.VibratoDelay == 0)
			{
				byte vibratoFlag = voiceInfo.VibratoFlag;
				int vibratoDepth = voiceInfo.VibratoDepth;
				int vibratoSpeed = voiceInfo.VibratoSpeed;
				int vibratoDelta = voiceInfo.VibratoDelta;

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

				voiceInfo.VibratoDelta = (byte)vibratoDelta;
				voiceInfo.VibratoFlag = vibratoFlag;

				period += (ushort)(((vibratoDelta - (voiceInfo.VibratoDepth >> 1)) * period) >> 10);
			}
			else
				voiceInfo.VibratoDelay--;

			return period;
		}



		/********************************************************************/
		/// <summary>
		/// Handle portamento version 1 code
		/// </summary>
		/********************************************************************/
		private ushort DoPortamentoVersion1(VoiceInfo voiceInfo, ushort period)
		{
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

			return period;
		}



		/********************************************************************/
		/// <summary>
		/// Handle portamento version 2 code
		/// </summary>
		/********************************************************************/
		private ushort DoPortamentoVersion2(VoiceInfo voiceInfo, ushort period)
		{
			if ((voiceInfo.CurrentInfo & 0x20) != 0)
			{
				int val = (sbyte)voiceInfo.PreviousInfo;

				if (val >= 0)
				{
					voiceInfo.PortaDelta = (uint)(voiceInfo.PortaDelta + val);
					val = (int)(voiceInfo.PortaDelta * period);
					period -= (ushort)(val >> 10);
				}
				else
				{
					voiceInfo.PortaDelta = (uint)(voiceInfo.PortaDelta - val);
					val = (int)(voiceInfo.PortaDelta * period);
					period += (ushort)(val >> 10);
				}
			}

			return period;
		}



		/********************************************************************/
		/// <summary>
		/// Return the next random number
		/// </summary>
		/********************************************************************/
		private ushort GetRandomVariation()
		{
			int val = playingInfo.Random + 0x4793;
			int rotated = (val >> 6) | (val << (16 - 6));
			playingInfo.Random = (ushort)(rotated ^ val);

			return playingInfo.Random;
		}



		/********************************************************************/
		/// <summary>
		/// Will calculate and set the current tempo based on the value given
		/// </summary>
		/********************************************************************/
		private void SetTempo(sbyte tempo)
		{
			PlayingFrequency = (3546895.0f / ((tempo + 256) * 14318)) * 50.0f;
			ShowTempo();
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
			ShowTracks();
			ShowSpeed();

			if (currentModuleType == ModuleType.Hippel7V)
				ShowTempo();
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

			for (int i = 0; i < numberOfChannels; i++)
			{
				sb.Append(voices[i].CurrentTrackNumber);
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
			return playingInfo.Speed.ToString();
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
