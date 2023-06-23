/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Globalization;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.Med.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.Med
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class MedWorker : ModulePlayerWithPositionDurationAgentBase
	{
		private static readonly Dictionary<Guid, ModuleType> moduleTypeLookup = new Dictionary<Guid, ModuleType>
		{
			{ Med.Agent1Id, ModuleType.Med112 },
			{ Med.Agent2Id, ModuleType.Med200 }
		};

		#region PlaySampleInfo class
		/// <summary>
		/// Result from the GetSampleData() method
		/// </summary>
		private struct PlaySampleInfo
		{
			public ushort Period;
			public uint StartOffset;
			public uint Length;
			public uint LoopStart;
			public uint LoopLength;
		}
		#endregion

		private readonly ModuleType currentModuleType;
		private ushort[] periods;

		private ushort numberOfBlocks;

		private ushort songLength;
		private byte[] orders;

		private ushort startTempo;
		private sbyte playTranspose;
		private ModuleFlag moduleFlags;
		private ushort sliding;

		private Block[] blocks;
		private Sample[] samples;

		private GlobalPlayingInfo playingInfo;

		private bool endReached;

		private const int InfoPositionLine = 3;
		private const int InfoPatternLine = 4;
		private const int InfoTempoLine = 5;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public MedWorker(Guid typeId)
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
		public override string[] FileExtensions => new [] { "med" };



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
					description = Resources.IDS_MED_INFODESCLINE0;
					value = songLength.ToString();
					break;
				}

				// Used patterns
				case 1:
				{
					description = Resources.IDS_MED_INFODESCLINE1;
					value = numberOfBlocks.ToString();
					break;
				}

				// Supported / used samples
				case 2:
				{
					description = Resources.IDS_MED_INFODESCLINE2;
					value = "31";
					break;
				}

				// Playing position
				case 3:
				{
					description = Resources.IDS_MED_INFODESCLINE3;
					value = playingInfo.PlayPositionNumber.ToString();
					break;
				}

				// Playing pattern
				case 4:
				{
					description = Resources.IDS_MED_INFODESCLINE4;
					value = playingInfo.PlayBlock.ToString();
					break;
				}

				// Current tempo (Hz)
				case 5:
				{
					description = Resources.IDS_MED_INFODESCLINE5;
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
				switch (currentModuleType)
				{
					case ModuleType.Med112:
						return LoadMed2Format(fileInfo, out errorMessage);

					case ModuleType.Med200:
						return LoadMed3Format(fileInfo, out errorMessage);
				}
			}
			catch (Exception)
			{
				Cleanup();
				throw;
			}

			return AgentResult.Error;
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the player
		/// </summary>
		/********************************************************************/
		public override bool InitPlayer(out string errorMessage)
		{
			if (!base.InitPlayer(out errorMessage))
				return false;

			periods = currentModuleType == ModuleType.Med112 ? Tables.Periods112 : Tables.Periods200;

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
			playingInfo.Counter++;
			if (playingInfo.Counter == 6)
			{
				playingInfo.Counter = 0;
				ParseNextRow();
			}

			HandleEffects();

			// Have we reached the end of the module
			if (endReached)
			{
				OnEndReached(playingInfo.PlayPositionNumber);
				endReached = false;

				MarkPositionAsVisited(playingInfo.PlayPositionNumber);
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

				if (currentModuleType == ModuleType.Med112)
				{
					for (byte j = 0; j < 4 * 12; j++)
						frequencies[4 * 12 + j] = 3546895U / Tables.Periods112[j];
				}
				else
				{
					for (byte j = 0; j < 3 * 12; j++)
						frequencies[4 * 12 + j] = 3546895U / Tables.Periods200[j];
				}

				for (uint i = 1; i < 32; i++)		// Skip first sample, since it will always be empty
				{
					Sample sample = samples[i];

					SampleInfo sampleInfo = new SampleInfo
					{
						Name = sample.Name,
						Type = SampleInfo.SampleType.Sample,
						BitSize = SampleInfo.SampleSize._8Bit,
						Volume = (ushort)(sample.Volume * 4),
						Panning = -1,
						LoopStart = sample.LoopStart,
						LoopLength = sample.LoopLength,
						NoteFrequencies = frequencies
					};

/*					if (sample.Type != SampleType.Normal)
					{
						// Don't have any modules with multiple octaves, so this part is missing
					}
					else*/
					{
						if (sample.SampleData != null)
						{
							sampleInfo.Sample = sample.SampleData;
							sampleInfo.Length = (uint)sample.SampleData.Length;
						}

						sampleInfo.Flags = SampleInfo.SampleFlag.None;
					}

					// Find out the loop information
					if (sampleInfo.LoopLength <= 2)
					{
						sampleInfo.LoopStart = 0;
						sampleInfo.LoopLength = 0;
					}
					else
						sampleInfo.Flags = SampleInfo.SampleFlag.Loop;

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
		protected override int InitDuration(int startPosition)
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
			return songLength;
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

			// First check the length
			if (moduleStream.Length < 36)
				return ModuleType.Unknown;

			// Now check the mark
			moduleStream.Seek(0, SeekOrigin.Begin);
			uint mark = moduleStream.Read_B_UINT32();

			// Check the mark
			if (mark == 0x4d454402)		// MED\x02
				return ModuleType.Med112;

			if (mark == 0x4d454403)		// MED\x03
				return ModuleType.Med200;

			return ModuleType.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Load a module in MED2 format
		/// </summary>
		/********************************************************************/
		private AgentResult LoadMed2Format(PlayerFileInfo fileInfo, out string errorMessage)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			moduleStream.Seek(4, SeekOrigin.Begin);

			LoadMed2SampleInformation(moduleStream);

			if (moduleStream.EndOfStream)
			{
				errorMessage = Resources.IDS_MED_ERR_LOADING_SAMPLEINFO;
				return AgentResult.Error;
			}

			numberOfBlocks = moduleStream.Read_B_UINT16();

			byte[] buffer = new byte[100];
			moduleStream.Read(buffer, 0, 100);

			songLength = moduleStream.Read_B_UINT16();
			orders = new byte[songLength];
			Array.Copy(buffer, orders, songLength);

			startTempo = moduleStream.Read_B_UINT16();
			playTranspose = 0;
			moduleFlags = (ModuleFlag)moduleStream.Read_B_UINT16();
			sliding = moduleStream.Read_B_UINT16();

			if (moduleStream.EndOfStream)
			{
				errorMessage = Resources.IDS_MED_ERR_LOADING_HEADER;
				return AgentResult.Error;
			}

			// Skip jumping mask and colors
			moduleStream.Seek(4 + 16, SeekOrigin.Current);

			if (!LoadBlocks(moduleStream))
			{
				errorMessage = Resources.IDS_MED_ERR_LOADING_BLOCKS;
				return AgentResult.Error;
			}

			return LoadExternalSamples(fileInfo, out errorMessage);
		}



		/********************************************************************/
		/// <summary>
		/// Load all the sample information
		/// </summary>
		/********************************************************************/
		private void LoadMed2SampleInformation(ModuleStream moduleStream)
		{
			string[] sampleNames = LoadMed2SampleNames(moduleStream);
			byte[] sampleVolumes = LoadMed2SampleVolumes(moduleStream);
			ushort[] sampleLoopStart = LoadMed2SampleLoopInformation(moduleStream);
			ushort[] sampleLoopLength = LoadMed2SampleLoopInformation(moduleStream);

			samples = new Sample[32];

			for (int i = 0; i < 32; i++)
			{
				samples[i] = new Sample
				{
					Name = sampleNames[i],
					LoopStart = sampleLoopStart[i],
					LoopLength = sampleLoopLength[i],
					Volume = sampleVolumes[i]
				};
			}
		}



		/********************************************************************/
		/// <summary>
		/// Load all the sample names
		/// </summary>
		/********************************************************************/
		private string[] LoadMed2SampleNames(ModuleStream moduleStream)
		{
			string[] names = new string[32];

			Encoding encoder = EncoderCollection.Amiga;

			for (int i = 0; i < 32; i++)
				names[i] = moduleStream.ReadString(encoder, 40);

			return names;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the sample volumes
		/// </summary>
		/********************************************************************/
		private byte[] LoadMed2SampleVolumes(ModuleStream moduleStream)
		{
			byte[] volumes = new byte[32];

			moduleStream.Read(volumes, 0, 32);

			return volumes;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the sample loop information (either loop start or loop
		/// length)
		/// </summary>
		/********************************************************************/
		private ushort[] LoadMed2SampleLoopInformation(ModuleStream moduleStream)
		{
			ushort[] loopInfo = new ushort[32];

			moduleStream.ReadArray_B_UINT16s(loopInfo, 0, 32);

			return loopInfo;
		}



		/********************************************************************/
		/// <summary>
		/// Load all blocks
		/// </summary>
		/********************************************************************/
		private bool LoadBlocks(ModuleStream moduleStream)
		{
			blocks = new Block[numberOfBlocks];

			for (int i = 0; i < numberOfBlocks; i++)
			{
				moduleStream.Seek(4, SeekOrigin.Current);		// Skip unknown bytes

				TrackLine[,] track = new TrackLine[4, 64];

				for (int j = 0; j < 64; j++)
				{
					for (int k = 0; k < 4; k++)
					{
						ushort period = moduleStream.Read_B_UINT16();
						byte sample = moduleStream.Read_UINT8();
						Effect effect = (Effect)(sample & 0x0f);
						byte effectArg = moduleStream.Read_UINT8();

						sample >>= 4;
						if ((period & 0x9000) != 0)
						{
							period = (ushort)(period & ~0x9000);
							sample += 16;
						}

						track[k, j] = new TrackLine
						{
							Note = FindNote(period),
							SampleNumber = sample,
							Effect = effect,
							EffectArg = effectArg
						};
					}
				}

				if (moduleStream.EndOfStream)
					return false;

				blocks[i] = new Block
				{
					Rows = track
				};
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Find the note number from a period
		/// </summary>
		/********************************************************************/
		private byte FindNote(ushort period)
		{
			byte note = 0;

			if (period != 0)
			{
				while (period < Tables.Periods112[note++])
				{
				}
			}

			return note;
		}



		/********************************************************************/
		/// <summary>
		/// Load a module in MED3 format
		/// </summary>
		/********************************************************************/
		private AgentResult LoadMed3Format(PlayerFileInfo fileInfo, out string errorMessage)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			moduleStream.Seek(4, SeekOrigin.Begin);

			LoadMed3SampleInformation(moduleStream);

			if (moduleStream.EndOfStream)
			{
				errorMessage = Resources.IDS_MED_ERR_LOADING_SAMPLEINFO;
				return AgentResult.Error;
			}

			numberOfBlocks = moduleStream.Read_B_UINT16();
			songLength = moduleStream.Read_B_UINT16();

			orders = new byte[songLength];
			moduleStream.Read(orders, 0, songLength);

			moduleStream.Seek(2, SeekOrigin.Current);		// Skip tempo. Not used in the original player
			startTempo = 6;
			playTranspose = moduleStream.Read_INT8();
			moduleFlags = (ModuleFlag)moduleStream.Read_UINT8();
			sliding = moduleStream.Read_B_UINT16();

			if (moduleStream.EndOfStream)
			{
				errorMessage = Resources.IDS_MED_ERR_LOADING_HEADER;
				return AgentResult.Error;
			}

			// Skip jumping mask and colors
			moduleStream.Seek(4 + 16, SeekOrigin.Current);

			// Skip midi information
			SkipMidi(moduleStream);		// Channels
			SkipMidi(moduleStream);		// Presets

			if (!LoadPackedBlocks(moduleStream))
			{
				errorMessage = Resources.IDS_MED_ERR_LOADING_BLOCKS;
				return AgentResult.Error;
			}

			if ((moduleFlags & ModuleFlag.SamplesAttached) != 0) 
				return LoadAttachedSamples(moduleStream, out errorMessage);

			return LoadExternalSamples(fileInfo, out errorMessage);
		}



		/********************************************************************/
		/// <summary>
		/// Load all the sample information
		/// </summary>
		/********************************************************************/
		private void LoadMed3SampleInformation(ModuleStream moduleStream)
		{
			string[] sampleNames = LoadMed3SampleNames(moduleStream);
			byte[] sampleVolumes = LoadMed3SampleVolumes(moduleStream);
			ushort[] sampleLoopStart = LoadMed3SampleLoopInformation(moduleStream);
			ushort[] sampleLoopLength = LoadMed3SampleLoopInformation(moduleStream);

			samples = new Sample[32];

			for (int i = 0; i < 32; i++)
			{
				samples[i] = new Sample
				{
					Name = sampleNames[i],
					LoopStart = sampleLoopStart[i],
					LoopLength = sampleLoopLength[i],
					Volume = sampleVolumes[i]
				};
			}
		}



		/********************************************************************/
		/// <summary>
		/// Load all the sample names
		/// </summary>
		/********************************************************************/
		private string[] LoadMed3SampleNames(ModuleStream moduleStream)
		{
			string[] names = new string[32];

			Encoding encoder = EncoderCollection.Amiga;
			byte[] buffer = new byte[40];

			for (int i = 0; i < 32; i++)
			{
				for (int j = 0; j < buffer.Length - 1; j++)
				{
					byte chr = moduleStream.Read_UINT8();
					buffer[j] = chr;

					if (chr == 0x00)
						break;
				}

				names[i] = encoder.GetString(buffer);
			}

			return names;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the sample volumes
		/// </summary>
		/********************************************************************/
		private byte[] LoadMed3SampleVolumes(ModuleStream moduleStream)
		{
			byte[] volumes = new byte[32];

			uint mask = moduleStream.Read_B_UINT32();

			for (int i = 0; i < 32; i++)
			{
				if ((mask & 0x80000000) != 0)
					volumes[i] = moduleStream.Read_UINT8();
				else
					volumes[i] = 0;

				mask <<= 1;
			}

			return volumes;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the sample loop information (either loop start or loop
		/// length)
		/// </summary>
		/********************************************************************/
		private ushort[] LoadMed3SampleLoopInformation(ModuleStream moduleStream)
		{
			ushort[] loopInfo = new ushort[32];

			uint mask = moduleStream.Read_B_UINT32();

			for (int i = 0; i < 32; i++)
			{
				if ((mask & 0x80000000) != 0)
					loopInfo[i] = moduleStream.Read_B_UINT16();
				else
					loopInfo[i] = 0;

				mask <<= 1;
			}

			return loopInfo;
		}



		/********************************************************************/
		/// <summary>
		/// Skip midi information
		/// </summary>
		/********************************************************************/
		private void SkipMidi(ModuleStream moduleStream)
		{
			int bytesToSkip = 0;

			uint mask = moduleStream.Read_B_UINT32();

			for (int i = 0; i < 32; i++)
			{
				if ((mask & 0x80000000) != 0)
					bytesToSkip++;

				mask <<= 1;
			}

			moduleStream.Seek(bytesToSkip, SeekOrigin.Current);
		}



		/********************************************************************/
		/// <summary>
		/// Load all blocks in packed format
		/// </summary>
		/********************************************************************/
		private bool LoadPackedBlocks(ModuleStream moduleStream)
		{
			blocks = new Block[numberOfBlocks];

			for (int i = 0; i < numberOfBlocks; i++)
			{
				byte trackCount = moduleStream.Read_UINT8();

				TrackLine[,] track = new TrackLine[trackCount, 64];

				BlockFlag flag = (BlockFlag)moduleStream.Read_UINT8();
				ushort length = moduleStream.Read_B_UINT16();

				uint lineMask1, lineMask2, effectMask1, effectMask2;

				if ((flag & BlockFlag.FirstHalfLineNone) != 0)
					lineMask1 = 0;
				else if ((flag & BlockFlag.FirstHalfLineAll) != 0)
					lineMask1 = 0xffffffff;
				else
					lineMask1 = moduleStream.Read_B_UINT32();

				if ((flag & BlockFlag.SecondHalfLineNone) != 0)
					lineMask2 = 0;
				else if ((flag & BlockFlag.SecondHalfLineAll) != 0)
					lineMask2 = 0xffffffff;
				else
					lineMask2 = moduleStream.Read_B_UINT32();

				if ((flag & BlockFlag.FirstHalfEffectNone) != 0)
					effectMask1 = 0;
				else if ((flag & BlockFlag.FirstHalfEffectAll) != 0)
					effectMask1 = 0xffffffff;
				else
					effectMask1 = moduleStream.Read_B_UINT32();

				if ((flag & BlockFlag.SecondHalfEffectNone) != 0)
					effectMask2 = 0;
				else if ((flag & BlockFlag.SecondHalfEffectAll) != 0)
					effectMask2 = 0xffffffff;
				else
					effectMask2 = moduleStream.Read_B_UINT32();

				byte[] blockData = new byte[length];
				moduleStream.Read(blockData, 0, length);

				if (moduleStream.EndOfStream)
					return false;

				UnpackBlock(blockData, track, lineMask1, lineMask2, effectMask1, effectMask2);

				blocks[i] = new Block
				{
					Rows = track
				};
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single block and unpack it
		/// </summary>
		/********************************************************************/
		private void UnpackBlock(byte[] blockData, TrackLine[,] track, uint lineMask1, uint lineMask2, uint effectMask1, uint effectMask2)
		{
			int trackCount = track.GetLength(0);
			uint currentLineMask = lineMask1;
			uint currentEffectMask = effectMask1;
			ushort nibbleNumber = 0;

			for (int i = 0; i < 64; i++)
			{
				if (i == 32)
				{
					currentLineMask = lineMask2;
					currentEffectMask = effectMask2;
				}

				if ((currentLineMask & 0x80000000) != 0)
				{
					ushort channelMask = GetNibbles(blockData, ref nibbleNumber, trackCount / 4);
					channelMask <<= (16 - trackCount);

					for (int j = 0; j < trackCount; j++)
					{
						TrackLine trackLine = new TrackLine();

						if ((channelMask & 0x8000) != 0)
						{
							trackLine.Note = (byte)GetNibbles(blockData, ref nibbleNumber, 2);
							trackLine.SampleNumber = GetNibble(blockData, ref nibbleNumber);

							if ((trackLine.Note & 0x80) != 0)
							{
								trackLine.Note &= 0x7f;
								trackLine.SampleNumber += 16;
							}
						}

						channelMask <<= 1;
						track[j, i] = trackLine;
					}
				}
				else
				{
					for (int j = 0; j < trackCount; j++)
						track[j, i] = new TrackLine();
				}

				if ((currentEffectMask & 0x80000000) != 0)
				{
					ushort channelMask = GetNibbles(blockData, ref nibbleNumber, trackCount / 4);
					channelMask <<= (16 - trackCount);

					for (int j = 0; j < trackCount; j++)
					{
						if ((channelMask & 0x8000) != 0)
						{
							TrackLine trackLine = track[j, i];

							trackLine.Effect = (Effect)GetNibble(blockData, ref nibbleNumber);
							trackLine.EffectArg = (byte)GetNibbles(blockData, ref nibbleNumber, 2);
						}

						channelMask <<= 1;
					}
				}

				currentLineMask <<= 1;
				currentEffectMask <<= 1;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Read a single nibble
		/// </summary>
		/********************************************************************/
		private byte GetNibble(byte[] blockData, ref ushort nibbleNumber)
		{
			byte result;

			int offset = nibbleNumber / 2;

			if ((nibbleNumber & 1) != 0)
				result = (byte)(blockData[offset] & 0x0f);
			else
				result = (byte)(blockData[offset] >> 4);

			nibbleNumber++;

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Read a multiple number of nibbles
		/// </summary>
		/********************************************************************/
		private ushort GetNibbles(byte[] blockData, ref ushort nibbleNumber, int count)
		{
			ushort result = 0;

			while (count-- > 0)
			{
				result <<= 4;
				result |= GetNibble(blockData, ref nibbleNumber);
			}

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Load attached samples
		/// </summary>
		/********************************************************************/
		private AgentResult LoadAttachedSamples(ModuleStream moduleStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			uint mask = moduleStream.Read_B_UINT32();

			for (int i = 1; i < 32; i++)	// There's no sample #0, so we start from 1
			{
				mask <<= 1;

				if ((mask & 0x80000000) != 0)
				{
					Sample sample = samples[i];

					uint length = moduleStream.Read_B_UINT32();
					sample.Type = (SampleType)moduleStream.Read_B_UINT16();

					sample.SampleData = moduleStream.ReadSampleData(i, (int)length, out int readBytes);

					if (readBytes != length)
					{
						errorMessage = Resources.IDS_MED_ERR_LOADING_SAMPLES;
						return AgentResult.Error;
					}
				}
			}

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Load samples in extern files
		/// </summary>
		/********************************************************************/
		private AgentResult LoadExternalSamples(PlayerFileInfo fileInfo, out string errorMessage)
		{
			errorMessage = string.Empty;

			bool isArchive = ArchivePath.IsArchivePath(fileInfo.FileName);
			string directoryName = isArchive ? Path.GetDirectoryName(ArchivePath.GetEntryName(fileInfo.FileName)) : Path.GetDirectoryName(fileInfo.FileName);

			for (int i = 1; i < 32; i++)
			{
				if (!string.IsNullOrEmpty(samples[i].Name))
				{
					if (LoadSingleExternalSample(fileInfo, isArchive, directoryName, i, out errorMessage) != AgentResult.Ok)
						return AgentResult.Error;
				}
			}

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single sample as extern file
		/// </summary>
		/********************************************************************/
		private AgentResult LoadSingleExternalSample(PlayerFileInfo fileInfo, bool isArchive, string directoryName, int sampleNumber, out string errorMessage)
		{
			errorMessage = string.Empty;

			Sample sample = samples[sampleNumber];

			for (;;)
			{
				string newDirectory = isArchive ? ArchivePath.CombinePathParts(ArchivePath.GetArchiveName(fileInfo.FileName), directoryName) : directoryName;
				string samplePath = Path.Combine(newDirectory, "Instruments", sample.Name);

				using (ModuleStream moduleStream = fileInfo.Loader?.OpenExtraFileByFileName(samplePath, true))
				{
					// Did we get any file at all
					if (moduleStream != null)
					{
						moduleStream.Seek(0, SeekOrigin.Begin);
						sample.SampleData = moduleStream.ReadSampleData(sampleNumber, (int)moduleStream.Length, out _);

						sample.SampleData = FixIfIff(sample.SampleData);
						if (sample.SampleData == null)
						{
							errorMessage = string.Format(Resources.IDS_MED_ERR_LOADING_EXTERNAL_SAMPLE, sample.Name);
							return AgentResult.Error;
						}

						sample.Type = SampleType.Normal;

						return AgentResult.Ok;
					}
				}

				int index = directoryName.LastIndexOf(Path.DirectorySeparatorChar);
				if (index == -1)
					break;

				directoryName = directoryName.Substring(0, index);
				if (string.IsNullOrEmpty(directoryName) || (directoryName[^1] == ':'))
					break;
			}

			errorMessage = string.Format(Resources.IDS_MED_ERR_LOADING_EXTERNAL_SAMPLE, sample.Name);
			return AgentResult.Error;
		}



		/********************************************************************/
		/// <summary>
		/// Check to see if the loaded sample is in IFF format and if so,
		/// return only the sample part
		/// </summary>
		/********************************************************************/
		private sbyte[] FixIfIff(sbyte[] sampleData)
		{
			if ((sampleData[0] == 0x46) && (sampleData[1] == 0x4f) && (sampleData[2] == 0x52) && (sampleData[3] == 0x4d))	// FORM
			{
				if ((sampleData[8] == 0x38) && (sampleData[9] == 0x53) && (sampleData[10] == 0x56) && (sampleData[11] == 0x58))	// 8SVX
				{
					int offset = 12;

					while (offset < sampleData.Length)
					{
						int length = ((byte)sampleData[offset + 4] << 24) | ((byte)sampleData[offset + 5] << 16) | ((byte)sampleData[offset + 6] << 8) | (byte)sampleData[offset + 7];

						if ((sampleData[offset] == 0x42) && (sampleData[offset + 1] == 0x4f) && (sampleData[offset + 2] == 0x44) && (sampleData[offset + 3] == 0x59))	// BODY
						{
							if (length > (sampleData.Length - offset))
								return null;

							return sampleData.AsSpan(offset + 8, length).ToArray();
						}

						offset += 8 + length;
					}
				}
			}

			return sampleData;
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
				PlayPositionNumber = (ushort)startPosition,
				PlayBlock = orders[startPosition],
				Counter = 5,
				NextBlock = false,

				PreviousPeriod = new ushort[4],
				PreviousNotes = new byte[16],
				PreviousSamples = new byte[16],
				PreviousVolumes = new byte[16],
				Effects = new Effect[16],
				EffectArgs = new byte[16]
			};

			endReached = false;

			AmigaFilter = (moduleFlags & ModuleFlag.FilterOn) != 0;
			SetTempo((byte)startTempo);
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player have allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			blocks = null;
			samples = null;
			orders = null;

			playingInfo = null;
		}



		/********************************************************************/
		/// <summary>
		/// Change the tempo
		/// </summary>
		/********************************************************************/
		private void SetTempo(byte newTempo)
		{
			if (currentModuleType == ModuleType.Med112)
				PlayingFrequency = 1f / ((625000f / newTempo) / 1000000);
			else
			{
				int ciaTempo;

				if (newTempo > 10)
					ciaTempo = 470000 / newTempo;
				else
					ciaTempo = Tables.SoundTrackerTempos[newTempo];

				PlayingFrequency = 709379f / ciaTempo;
			}

			ShowTempo();
		}



		/********************************************************************/
		/// <summary>
		/// Get the next row and parse it
		/// </summary>
		/********************************************************************/
		private void ParseNextRow()
		{
			Block block = blocks[playingInfo.PlayBlock];
			playingInfo.CurrentTrackCount = (ushort)block.Rows.GetLength(0);

			for (int i = 0; i < playingInfo.CurrentTrackCount; i++)
			{
				TrackLine trackLine = block.Rows[i, playingInfo.PlayLine];

				byte note = trackLine.Note;
				playingInfo.EffectArgs[i] = trackLine.EffectArg;

				if (trackLine.SampleNumber != 0)
				{
					playingInfo.PreviousSamples[i] = trackLine.SampleNumber;
					playingInfo.PreviousVolumes[i] = samples[trackLine.SampleNumber].Volume;
				}

				playingInfo.Effects[i] = trackLine.Effect;
				if (trackLine.Effect != Effect.None)
				{
					if (trackLine.Effect == Effect.SetTempo)
						HandleSetTempo(i, trackLine.EffectArg, ref note);
					else if (trackLine.Effect == Effect.SetVolume)
						HandleSetVolume(i, trackLine.EffectArg);
				}

				if (note != 0)
				{
					playingInfo.PreviousNotes[i] = note;
					PlayNote(i, note, playingInfo.PreviousVolumes[i], playingInfo.PreviousSamples[i]);
				}
			}

			playingInfo.PlayLine++;
			if ((playingInfo.PlayLine > 63) || playingInfo.NextBlock)
			{
				playingInfo.PlayLine = 0;

				playingInfo.PlayPositionNumber++;
				if (playingInfo.PlayPositionNumber >= songLength)
					playingInfo.PlayPositionNumber = 0;

				byte newBlockNumber = orders[playingInfo.PlayPositionNumber];
				if (newBlockNumber < numberOfBlocks)
					playingInfo.PlayBlock = newBlockNumber;

				playingInfo.NextBlock = false;

				if (HasPositionBeenVisited(playingInfo.PlayPositionNumber))
					endReached = true;

				MarkPositionAsVisited(playingInfo.PlayPositionNumber);

				ShowSongPosition();
				ShowPattern();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle all the effects
		/// </summary>
		/********************************************************************/
		private void HandleEffects()
		{
			for (int i = 0; i < playingInfo.CurrentTrackCount; i++)
			{
				byte effectArg = playingInfo.EffectArgs[i];

				ushort newPeriod = 0;
				bool setHardware = true;

				switch (playingInfo.Effects[i])
				{
					case Effect.SlideUp:
					{
						if ((sliding == 5) && (playingInfo.Counter == 0))
							break;

						playingInfo.PreviousPeriod[i] -= effectArg;
						if (playingInfo.PreviousPeriod[i] < 113)
							playingInfo.PreviousPeriod[i] = 113;

						break;
					}

					case Effect.SlideDown:
					{
						if ((sliding == 5) && (playingInfo.Counter == 0))
							break;

						playingInfo.PreviousPeriod[i] += effectArg;
						if (playingInfo.PreviousPeriod[i] > 856)
							playingInfo.PreviousPeriod[i] = 856;

						break;
					}

					case Effect.Arpeggio:
					{
						if (effectArg == 0x00)		// No arpeggio, if no argument is given
						{
							setHardware = false;
							break;
						}

						byte newNote = DoArpeggio(playingInfo.PreviousNotes[i], effectArg);
						newPeriod = periods[newNote - 1 + playTranspose];
						break;
					}

					case Effect.Crescendo:
					{
						sbyte newVolume = (sbyte)playingInfo.PreviousVolumes[i];

						if ((effectArg & 0xf0) != 0)
						{
							newVolume += (sbyte)((effectArg & 0xf0) >> 4);
							if (newVolume > 64)
								newVolume = 64;
						}
						else
						{
							newVolume -= (sbyte)effectArg;
							if (newVolume < 0)
								newVolume = 0;
						}

						playingInfo.PreviousVolumes[i] = (byte)newVolume;
						break;
					}

					case Effect.Vibrato:
					{
						newPeriod = playingInfo.PreviousPeriod[i];

						if (playingInfo.Counter < 3)
							newPeriod -= effectArg;

						break;
					}

					case Effect.SetTempo:
					{
						if (currentModuleType == ModuleType.Med200)
						{
							if (effectArg == 0xff)		// Note off
							{
								VirtualChannels[i].Mute();
								setHardware = false;
								break;
							}

							if (effectArg == 0xf1)		// Play this note twice
							{
								if (playingInfo.Counter != 3)
									break;
							}
							else if (effectArg == 0xf2)	// Play this note in the second half of this note
							{
								if (playingInfo.Counter != 3)
									break;
							}
							else if (effectArg == 0xf3)	// Play this note three times during the note
							{
								if ((playingInfo.Counter & 6) == 0)
									break;
							}
							else
							{
								setHardware = false;
								break;
							}

							PlayNote(i, playingInfo.PreviousNotes[i], playingInfo.PreviousVolumes[i], playingInfo.PreviousSamples[i]);
						}

						setHardware = false;
						break;
					}

					case Effect.Filter:
					{
						if (currentModuleType == ModuleType.Med112)
							goto case Effect.Crescendo;				// In MED 1.12, effect E is the same as D

						AmigaFilter = effectArg == 0;
						setHardware = false;
						break;
					}

					case Effect.SetVolume:
					{
						break;
					}

					default:
					{
						setHardware = false;
						break;
					}
				}

				if (setHardware)
				{
					if (newPeriod == 0)
						newPeriod = playingInfo.PreviousPeriod[i];

					VirtualChannels[i].SetAmigaPeriod(newPeriod);
					VirtualChannels[i].SetAmigaVolume(playingInfo.PreviousVolumes[i]);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle the SetTempo effect
		/// </summary>
		/********************************************************************/
		private void HandleSetTempo(int trackNumber, byte effectArg, ref byte note)
		{
			if (effectArg == 0)		// F00 is the same as block break
			{
				playingInfo.NextBlock = true;
				return;
			}

			if ((effectArg <= 0xf0) || (currentModuleType == ModuleType.Med112))
			{
				SetTempo(effectArg);
				return;
			}

			if (effectArg == 0xf2)	// Play the note in the second half of the note (delay the note)
			{
				playingInfo.PreviousNotes[trackNumber] = note;
				note = 0;
				return;
			}

			if (effectArg == 0xfe)	// Stop the playing
			{
				playingInfo.NextBlock = true;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle the SetVolume effect
		/// </summary>
		/********************************************************************/
		private void HandleSetVolume(int trackNumber, byte effectArg)
		{
			effectArg = (byte)(((effectArg >> 4) * 10) + (effectArg & 0x0f));	// Convert from BCD

			if (effectArg > 64)
				effectArg = 64;

			playingInfo.PreviousVolumes[trackNumber] = effectArg;
		}



		/********************************************************************/
		/// <summary>
		/// Do the arpeggio
		/// </summary>
		/********************************************************************/
		private byte DoArpeggio(byte note, byte effectArg)
		{
			if ((playingInfo.Counter == 0) || (playingInfo.Counter == 3))
				return (byte)(note + (effectArg & 0x0f));

			if ((playingInfo.Counter == 1) || (playingInfo.Counter == 4))
				return (byte)(note + (effectArg >> 4));

			return note;
		}



		/********************************************************************/
		/// <summary>
		/// Play a new note
		/// </summary>
		/********************************************************************/
		private void PlayNote(int trackNumber, byte note, byte volume, byte sampleNumber)
		{
			Sample sample = samples[sampleNumber];
			if (sample.SampleData == null)
				return;

			int newNote = note + playTranspose;

			if (currentModuleType == ModuleType.Med200)
			{
				if (newNote < 0)
					newNote += 12;
				else if (newNote > 72)
					newNote -= 12;
			}

			newNote--;

			PlaySampleInfo toPlay = GetSampleData(sample, newNote);

			IChannel channel = VirtualChannels[trackNumber];
			channel.PlaySample((short)(sampleNumber - 1), sample.SampleData, toPlay.StartOffset, toPlay.Length);

			if (toPlay.LoopLength > 2)
				channel.SetLoop(toPlay.LoopStart, toPlay.LoopLength);

			channel.SetAmigaPeriod(toPlay.Period);
			playingInfo.PreviousPeriod[trackNumber] = toPlay.Period;

			channel.SetAmigaVolume(volume);
		}



		/********************************************************************/
		/// <summary>
		/// Find needed information to play the sample
		/// </summary>
		/********************************************************************/
		private PlaySampleInfo GetSampleData(Sample sample, int note)
		{
			PlaySampleInfo result = new PlaySampleInfo();

			if (sample.Type == SampleType.Normal)
			{
				result.Period = periods[note];
				result.StartOffset = 0;
				result.Length = (uint)sample.SampleData.Length;
				result.LoopStart = sample.LoopStart;
				result.LoopLength = sample.LoopLength;
			}
			else
			{
				int octave = note / 12;
				note %= 12;
				uint length = (uint)sample.SampleData.Length;

				if (sample.Type == SampleType.Iff3Octave)
				{
					octave += 6;
					length /= 7;
				}
				else
					length /= 31;

				result.Length = length;
				result.LoopStart = sample.LoopStart;
				result.LoopLength = sample.LoopLength;

				byte shiftCount = Tables.ShiftCount[octave];
				result.LoopStart <<= shiftCount;
				result.LoopLength <<= shiftCount;
				result.Length <<= shiftCount;

				result.StartOffset = length * Tables.MultiplyLengthCount[octave];
				result.Period = periods[note + Tables.OctaveStart[octave]];
			}

			if (result.LoopLength > 2)
			{
				if (result.Length > (result.LoopStart + result.LoopLength))
					result.Length = result.LoopStart + result.LoopLength;
			}

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current song position
		/// </summary>
		/********************************************************************/
		private void ShowSongPosition()
		{
			OnModuleInfoChanged(InfoPositionLine, playingInfo.PlayPositionNumber.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with pattern number
		/// </summary>
		/********************************************************************/
		private void ShowPattern()
		{
			OnModuleInfoChanged(InfoPatternLine, playingInfo.PlayBlock.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with tempo
		/// </summary>
		/********************************************************************/
		private void ShowTempo()
		{
			OnModuleInfoChanged(InfoTempoLine, PlayingFrequency.ToString("F2", CultureInfo.InvariantCulture));
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with all dynamic values
		/// </summary>
		/********************************************************************/
		private void UpdateModuleInformation()
		{
			ShowSongPosition();
			ShowPattern();
			ShowTempo();
		}
		#endregion
	}
}
