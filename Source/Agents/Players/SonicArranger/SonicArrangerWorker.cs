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
using Polycode.NostalgicPlayer.Agent.Player.SonicArranger.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.Ancient;
using Polycode.NostalgicPlayer.Ports.Ancient.Exceptions;

namespace Polycode.NostalgicPlayer.Agent.Player.SonicArranger
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class SonicArrangerWorker : ModulePlayerWithPositionDurationAgentBase
	{
		private class PeriodInfo
		{
			public ushort Period;
			public ushort PreviousPeriod;
		}

		private SongInfo[] subSongs;
		private SinglePositionInfo[][] positions;
		private TrackLine[] trackLines;
		private Instrument[] instruments;

		private sbyte[][] sampleData;
		private sbyte[][] waveformData;
		private byte[][] adsrTables;
		private sbyte[][] amfTables;

		private SongInfo currentSongInfo;

		private GlobalPlayingInfo playingInfo;
		private VoiceInfo[] voices;

		private bool endReached;
		private bool compressed;

		private const int InfoPositionLine = 5;
		private const int InfoTrackLine = 6;
		private const int InfoSpeedLine = 7;

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => [ "sa", "sonic" ];



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			long fileSize = moduleStream.Length;
			if (fileSize < 16)
				return AgentResult.Unknown;

			// Check the mark
			moduleStream.Seek(0, SeekOrigin.Begin);

			byte[] buf = new byte[8];
			moduleStream.ReadExactly(buf, 0, 8);

			string mark = Encoding.ASCII.GetString(buf, 0, 8);

			if (mark == "SOARV1.0")
			{
				compressed = false;

				return AgentResult.Ok;
			}

			if (mark == "@OARV1.0")
			{
				// Module is packed with lh.library
				compressed = true;

				return AgentResult.Ok;
			}

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
					description = Resources.IDS_SA_INFODESCLINE0;
					value = positions.Length.ToString();
					break;
				}

				// Used track rows
				case 1:
				{
					description = Resources.IDS_SA_INFODESCLINE1;
					value = trackLines.Length.ToString();
					break;
				}

				// Used instruments
				case 2:
				{
					description = Resources.IDS_SA_INFODESCLINE2;
					value = instruments.Length.ToString();
					break;
				}

				// Used samples
				case 3:
				{
					description = Resources.IDS_SA_INFODESCLINE3;
					value = sampleData.Length.ToString();
					break;
				}

				// Used wave tables
				case 4:
				{
					description = Resources.IDS_SA_INFODESCLINE4;
					value = waveformData.Length.ToString();
					break;
				}

				// Playing position
				case 5:
				{
					description = Resources.IDS_SA_INFODESCLINE5;
					value = FormatPosition();
					break;
				}

				// Playing tracks from line
				case 6:
				{
					description = Resources.IDS_SA_INFODESCLINE6;
					value = FormatTracks();
					break;
				}

				// Current speed
				case 7:
				{
					description = Resources.IDS_SA_INFODESCLINE7;
					value = FormatSpeed();
					break;
				}

				// Current tempo (Hz)
				case 8:
				{
					description = Resources.IDS_SA_INFODESCLINE8;
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
			try
			{
				ModuleStream moduleStream = fileInfo.ModuleStream;

				// Skip mark
				moduleStream.Seek(8, SeekOrigin.Begin);

				if (compressed)
					return LoadCompressedModule(moduleStream, out errorMessage);

				return LoadNormalModule(moduleStream, out errorMessage);
			}
			catch (Exception)
			{
				Cleanup();
				throw;
			}
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
			playingInfo.SpeedCounter++;

			if (playingInfo.SpeedCounter >= playingInfo.CurrentSpeed)
			{
				playingInfo.SpeedCounter = 0;

				GetNextRow();
			}

			UpdateEffects();

			if (endReached)
			{
				OnEndReached(playingInfo.SongPosition);
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

				for (int j = 0; j < 9 * 12; j++)
					frequencies[j] = 3546895U / Tables.Periods[1 + j];

				foreach (Instrument instr in instruments)
				{
					SampleInfo sampleInfo = new SampleInfo
					{
						Name = instr.Name,
						Flags = SampleInfo.SampleFlag.None,
						Volume = (ushort)(instr.Volume * 4),
						Panning = -1,
						NoteFrequencies = frequencies
					};

					if (instr.Type == InstrumentType.Sample)
					{
						sampleInfo.Type = SampleInfo.SampleType.Sample;
						sampleInfo.Sample = sampleData[instr.WaveformNumber];
						sampleInfo.Length = instr.WaveformLength * 2U;

						if ((instr.RepeatLength != 1) && (instr.WaveformLength != 0))
						{
							sampleInfo.Flags |= SampleInfo.SampleFlag.Loop;

							if (instr.RepeatLength == 0)
							{
								sampleInfo.LoopStart = 0;
								sampleInfo.LoopLength = sampleInfo.Length;
							}
							else
							{
								sampleInfo.LoopStart = instr.WaveformLength * 2U;
								sampleInfo.LoopLength = instr.RepeatLength * 2U;
								sampleInfo.Length += sampleInfo.LoopLength;
							}
						}
						else
						{
							sampleInfo.LoopStart = 0;
							sampleInfo.LoopLength = 0;
						}

						if (sampleInfo.Sample != null)
						{
							sampleInfo.Length = (uint)Math.Min(sampleInfo.Length, sampleInfo.Sample.Length);
							if ((sampleInfo.LoopStart + sampleInfo.LoopLength) > sampleInfo.Length)
								sampleInfo.LoopLength = sampleInfo.Length - sampleInfo.LoopStart;
						}
					}
					else
					{
						sampleInfo.Type = SampleInfo.SampleType.Synthesis;
						sampleInfo.Length = 0;
						sampleInfo.LoopStart = 0;
						sampleInfo.LoopLength = 0;
					}

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
		protected override int InitDuration(int subSong, int startPosition)
		{
			if (subSong >= subSongs.Length)
				return -1;

			InitializeSound(subSong);

			return currentSongInfo.FirstPosition;
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
			return positions.Length;
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
		/// Read the whole module
		/// </summary>
		/********************************************************************/
		private AgentResult LoadNormalModule(ModuleStream moduleStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			// STBL
			if ((moduleStream.Read_B_UINT32() != 0x5354424c) || !ReadSubSongs(moduleStream))
			{
				errorMessage = Resources.IDS_SA_ERR_LOADING_HEADER;
				return AgentResult.Error;
			}

			// OVTB
			if ((moduleStream.Read_B_UINT32() != 0x4f565442) || !ReadPositionInformation(moduleStream))
			{
				errorMessage = Resources.IDS_SA_ERR_LOADING_HEADER;
				return AgentResult.Error;
			}

			// NTBL
			if ((moduleStream.Read_B_UINT32() != 0x4e54424c) || !ReadTrackRows(moduleStream))
			{
				errorMessage = Resources.IDS_SA_ERR_LOADING_TRACK;
				return AgentResult.Error;
			}

			// INST
			if ((moduleStream.Read_B_UINT32() != 0x494e5354) || !ReadInstrumentInformation(moduleStream))
			{
				errorMessage = Resources.IDS_SA_ERR_LOADING_INSTRUMENT;
				return AgentResult.Error;
			}

			// SD8B
			if ((moduleStream.Read_B_UINT32() != 0x53443842) || !ReadSampleInformation(moduleStream, out int numberOfSamples))
			{
				errorMessage = Resources.IDS_SA_ERR_LOADING_SAMPLEINFO;
				return AgentResult.Error;
			}

			if (!ReadSampleData(moduleStream, numberOfSamples))
			{
				errorMessage = Resources.IDS_SA_ERR_LOADING_SAMPLE;
				return AgentResult.Error;
			}

			// SYWT
			if ((moduleStream.Read_B_UINT32() != 0x53595754) || !ReadWaveformData(moduleStream))
			{
				errorMessage = Resources.IDS_SA_ERR_LOADING_WAVEFORM;
				return AgentResult.Error;
			}

			// SYAR
			if ((moduleStream.Read_B_UINT32() != 0x53594152) || !ReadAdsrTables(moduleStream))
			{
				errorMessage = Resources.IDS_SA_ERR_LOADING_ADSR;
				return AgentResult.Error;
			}

			// SYAF
			if ((moduleStream.Read_B_UINT32() != 0x53594146) || !ReadAmfTables(moduleStream))
			{
				errorMessage = Resources.IDS_SA_ERR_LOADING_AMF;
				return AgentResult.Error;
			}

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Read the whole module with compressed data
		/// </summary>
		/********************************************************************/
		private AgentResult LoadCompressedModule(ModuleStream moduleStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			// @TBL
			if ((moduleStream.Read_B_UINT32() != 0x4054424c) || !ReadSubSongs(moduleStream))
			{
				errorMessage = Resources.IDS_SA_ERR_LOADING_HEADER;
				return AgentResult.Error;
			}

			// @VTB
			if ((moduleStream.Read_B_UINT32() != 0x40565442) || !ReadCompressedData(moduleStream, 4 * 4, ReadPositionInformation))
			{
				errorMessage = Resources.IDS_SA_ERR_LOADING_HEADER;
				return AgentResult.Error;
			}

			// @TBL
			if ((moduleStream.Read_B_UINT32() != 0x4054424c) || !ReadCompressedData(moduleStream, 4, ReadTrackRows))
			{
				errorMessage = Resources.IDS_SA_ERR_LOADING_TRACK;
				return AgentResult.Error;
			}

			// @NST
			if ((moduleStream.Read_B_UINT32() != 0x404e5354) || !ReadCompressedData(moduleStream, 152, ReadInstrumentInformation))
			{
				errorMessage = Resources.IDS_SA_ERR_LOADING_INSTRUMENT;
				return AgentResult.Error;
			}

			// @D8B
			if ((moduleStream.Read_B_UINT32() != 0x40443842) || !ReadSampleInformation(moduleStream, out int numberOfSamples))
			{
				errorMessage = Resources.IDS_SA_ERR_LOADING_SAMPLEINFO;
				return AgentResult.Error;
			}

			if (!ReadCompressedSampleData(moduleStream, numberOfSamples))
			{
				errorMessage = Resources.IDS_SA_ERR_LOADING_SAMPLE;
				return AgentResult.Error;
			}

			// @YWT
			if ((moduleStream.Read_B_UINT32() != 0x40595754) || !ReadCompressedData(moduleStream, 128, ReadWaveformData))
			{
				errorMessage = Resources.IDS_SA_ERR_LOADING_WAVEFORM;
				return AgentResult.Error;
			}

			// @YAR
			if ((moduleStream.Read_B_UINT32() != 0x40594152) || !ReadCompressedData(moduleStream, 128, ReadAdsrTables))
			{
				errorMessage = Resources.IDS_SA_ERR_LOADING_ADSR;
				return AgentResult.Error;
			}

			// @YAF
			if ((moduleStream.Read_B_UINT32() != 0x40594146) || !ReadCompressedData(moduleStream, 128, ReadAmfTables))
			{
				errorMessage = Resources.IDS_SA_ERR_LOADING_AMF;
				return AgentResult.Error;
			}

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Read compressed information
		/// </summary>
		/********************************************************************/
		private bool ReadCompressedData(ModuleStream moduleStream, uint elementSize, Func<ModuleStream, uint?, bool> readMethod)
		{
			uint count = moduleStream.Read_B_UINT32();
			if (count == 0)
				return readMethod(moduleStream, 0);

			uint compressedLength = moduleStream.Read_B_UINT32();
			uint decompressedLength = count * elementSize;

			byte[] decompressedData = DecompressData(moduleStream, compressedLength, decompressedLength);
			if (decompressedData == null)
				return false;

			using (ModuleStream ms = new ModuleStream(new MemoryStream(decompressedData), false))
			{
				return readMethod(ms, count);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Decompress data and return it
		/// </summary>
		/********************************************************************/
		private byte[] DecompressData(ModuleStream moduleStream, uint compressedLength, uint decompressedLength)
		{
			try
			{
				long oldPosition = moduleStream.Position;

				using (Decompressor decompressor = new Decompressor(new SliceStream(moduleStream, true, oldPosition, compressedLength), DecompressorType.LhLibrary, decompressedLength))
				{
					// Decompress the data and flatten all the arrays
					byte[] decompressedData = decompressor.Decompress().SelectMany(x => x).ToArray();

					// Make sure that the original module stream has the right position for the next part
					moduleStream.Seek(oldPosition + compressedLength, SeekOrigin.Begin);

					return decompressedData;
				}
			}
			catch (DecompressionException)
			{
				return null;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Read sub-song information
		/// </summary>
		/********************************************************************/
		private bool ReadSubSongs(ModuleStream moduleStream)
		{
			uint numberOfSubSongs = moduleStream.Read_B_UINT32();
			List<SongInfo> songs = new List<SongInfo>();

			for (int i = 0; i < numberOfSubSongs; i++)
			{
				SongInfo songInfo = new SongInfo();

				songInfo.StartSpeed = moduleStream.Read_B_UINT16();
				songInfo.RowsPerTrack = moduleStream.Read_B_UINT16();
				songInfo.FirstPosition = moduleStream.Read_B_UINT16();
				songInfo.LastPosition = moduleStream.Read_B_UINT16();
				songInfo.RestartPosition = moduleStream.Read_B_UINT16();
				songInfo.Tempo = moduleStream.Read_B_UINT16();

				if (moduleStream.EndOfStream)
					return false;

				if ((songInfo.LastPosition == 0xffff) || (songInfo.RestartPosition == 0xffff))
					continue;

				if (songs.Any(x => (songInfo.FirstPosition == x.FirstPosition) && (songInfo.LastPosition == x.LastPosition)))
					continue;

				songs.Add(songInfo);
			}

			subSongs = songs.ToArray();

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read position information
		/// </summary>
		/********************************************************************/
		private bool ReadPositionInformation(ModuleStream moduleStream, uint? totalNumberOfPositions = null)
		{
			totalNumberOfPositions ??= moduleStream.Read_B_UINT32();
			positions = new SinglePositionInfo[totalNumberOfPositions.Value][];

			for (int i = 0; i < totalNumberOfPositions; i++)
			{
				positions[i] = new SinglePositionInfo[4];

				for (int j = 0; j < 4; j++)
				{
					SinglePositionInfo singlePosition = new SinglePositionInfo();

					singlePosition.StartTrackRow = moduleStream.Read_B_UINT16();
					singlePosition.SoundTranspose = moduleStream.Read_INT8();
					singlePosition.NoteTranspose = moduleStream.Read_INT8();

					positions[i][j] = singlePosition;
				}

				if (moduleStream.EndOfStream)
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read all the track rows
		/// </summary>
		/********************************************************************/
		private bool ReadTrackRows(ModuleStream moduleStream, uint? totalNumberOfTrackRows = null)
		{
			totalNumberOfTrackRows ??= moduleStream.Read_B_UINT32();
			trackLines = new TrackLine[totalNumberOfTrackRows.Value];

			for (int i = 0; i < totalNumberOfTrackRows; i++)
			{
				TrackLine trackLine = new TrackLine();

				byte byt1 = moduleStream.Read_UINT8();
				byte byt2 = moduleStream.Read_UINT8();
				byte byt3 = moduleStream.Read_UINT8();
				byte byt4 = moduleStream.Read_UINT8();

				trackLine.Note = byt1;
				trackLine.Instrument = byt2;
				trackLine.DisableSoundTranspose = (byt3 & 0x80) != 0;
				trackLine.DisableNoteTranspose = (byt3 & 0x40) != 0;
				trackLine.Arpeggio = (byte)((byt3 & 0x30) >> 4);
				trackLine.Effect = (Effect)(byt3 & 0x0f);
				trackLine.EffectArg = byt4;

				if (moduleStream.EndOfStream)
					return false;

				trackLines[i] = trackLine;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read synth instrument information
		/// </summary>
		/********************************************************************/
		private bool ReadInstrumentInformation(ModuleStream moduleStream, uint? numberOfInstruments = null)
		{
			Encoding encoder = EncoderCollection.Amiga;

			numberOfInstruments ??= moduleStream.Read_B_UINT32();
			instruments = new Instrument[numberOfInstruments.Value];

			for (int i = 0; i < numberOfInstruments; i++)
			{
				Instrument instr = new Instrument();

				instr.Type = (InstrumentType)moduleStream.Read_B_UINT16();
				instr.WaveformNumber = moduleStream.Read_B_UINT16();
				instr.WaveformLength = moduleStream.Read_B_UINT16();
				instr.RepeatLength = moduleStream.Read_B_UINT16();

				if (moduleStream.EndOfStream)
					return false;

				moduleStream.Seek(8, SeekOrigin.Current);

				instr.Volume = moduleStream.Read_B_UINT16();
				instr.FineTuning = (sbyte)moduleStream.Read_B_UINT16();

				instr.PortamentoSpeed = moduleStream.Read_B_UINT16();

				instr.VibratoDelay = moduleStream.Read_B_UINT16();
				instr.VibratoSpeed = moduleStream.Read_B_UINT16();
				instr.VibratoLevel = moduleStream.Read_B_UINT16();

				instr.AmfNumber = moduleStream.Read_B_UINT16();
				instr.AmfDelay = moduleStream.Read_B_UINT16();
				instr.AmfLength = moduleStream.Read_B_UINT16();
				instr.AmfRepeat = moduleStream.Read_B_UINT16();

				instr.AdsrNumber = moduleStream.Read_B_UINT16();
				instr.AdsrDelay = moduleStream.Read_B_UINT16();
				instr.AdsrLength = moduleStream.Read_B_UINT16();
				instr.AdsrRepeat = moduleStream.Read_B_UINT16();
				instr.SustainPoint = moduleStream.Read_B_UINT16();
				instr.SustainDelay = moduleStream.Read_B_UINT16();

				if (moduleStream.EndOfStream)
					return false;

				moduleStream.Seek(16, SeekOrigin.Current);

				instr.EffectArg1 = moduleStream.Read_B_UINT16();
				instr.Effect = (SynthesisEffect)moduleStream.Read_B_UINT16();
				instr.EffectArg2 = moduleStream.Read_B_UINT16();
				instr.EffectArg3 = moduleStream.Read_B_UINT16();
				instr.EffectDelay = moduleStream.Read_B_UINT16();

				for (int j = 0; j < 3; j++)
				{
					instr.Arpeggios[j].Length = moduleStream.Read_UINT8();
					instr.Arpeggios[j].Repeat = moduleStream.Read_UINT8();

					int bytesRead = moduleStream.Read(instr.Arpeggios[j].Values, 0, 14);
					if (bytesRead < 14)
						return false;
				}

				instr.Name = moduleStream.ReadString(encoder, 30);

				instruments[i] = instr;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read sample instrument information
		/// </summary>
		/********************************************************************/
		private bool ReadSampleInformation(ModuleStream moduleStream, out int numberOfSamples)
		{
			numberOfSamples = moduleStream.Read_B_INT32();
			if (numberOfSamples != 0)
			{
				// Skip sample lengths and loop lengths stored in words + sample names
				moduleStream.Seek(numberOfSamples * 4 * 2 + numberOfSamples * 30, SeekOrigin.Current);
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read sample data
		/// </summary>
		/********************************************************************/
		private bool ReadSampleData(ModuleStream moduleStream, int numberOfSamples)
		{
			sampleData = new sbyte[numberOfSamples][];

			if (numberOfSamples > 0)
			{
				uint[] sampleLengths = new uint[numberOfSamples];
				moduleStream.ReadArray_B_UINT32s(sampleLengths, 0, numberOfSamples);

				if (moduleStream.EndOfStream)
					return false;

				for (int i = 0; i < numberOfSamples; i++)
				{
					uint sampleLen = sampleLengths[i];

					if (sampleLen > 0)
					{
						sampleData[i] = moduleStream.ReadSampleData(i, (int)sampleLen, out int readBytes);
						if (readBytes < sampleLen)
							return false;
					}
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read compressed sample data
		/// </summary>
		/********************************************************************/
		private bool ReadCompressedSampleData(ModuleStream moduleStream, int numberOfSamples)
		{
			sampleData = new sbyte[numberOfSamples][];

			if (numberOfSamples > 0)
			{
				uint[] sampleLengths = new uint[numberOfSamples];
				moduleStream.ReadArray_B_UINT32s(sampleLengths, 0, numberOfSamples);

				if (moduleStream.EndOfStream)
					return false;

				for (int i = 0; i < numberOfSamples; i++)
				{
					uint sampleLen = sampleLengths[i];

					if (sampleLen > 0)
					{
						uint compressedLength = moduleStream.Read_B_UINT32();

						sampleData[i] = (sbyte[])(Array)DecompressData(moduleStream, compressedLength, sampleLen);
						if (sampleData[i] == null)
							return false;
					}
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read waveform data
		/// </summary>
		/********************************************************************/
		private bool ReadWaveformData(ModuleStream moduleStream, uint? numberOfWaveforms = null)
		{
			numberOfWaveforms ??= moduleStream.Read_B_UINT32();
			waveformData = ArrayHelper.Initialize2Arrays<sbyte>((int)numberOfWaveforms, 128);

			for (int i = 0; i < numberOfWaveforms; i++)
			{
				int bytesRead = moduleStream.ReadSigned(waveformData[i], 0, 128);
				if (bytesRead < 128)
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read ADSR tables
		/// </summary>
		/********************************************************************/
		private bool ReadAdsrTables(ModuleStream moduleStream, uint? numberOfAdsrTables = null)
		{
			numberOfAdsrTables ??= moduleStream.Read_B_UINT32();
			adsrTables = ArrayHelper.Initialize2Arrays<byte>((int)numberOfAdsrTables, 128);

			for (int i = 0; i < numberOfAdsrTables; i++)
			{
				int bytesRead = moduleStream.Read(adsrTables[i], 0, 128);
				if (bytesRead < 128)
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read AMF tables
		/// </summary>
		/********************************************************************/
		private bool ReadAmfTables(ModuleStream moduleStream, uint? numberOfAmfTables = null)
		{
			numberOfAmfTables ??= moduleStream.Read_B_UINT32();
			amfTables = ArrayHelper.Initialize2Arrays<sbyte>((int)numberOfAmfTables, 128);

			for (int i = 0; i < numberOfAmfTables; i++)
			{
				int bytesRead = moduleStream.ReadSigned(amfTables[i], 0, 128);
				if (bytesRead < 128)
					return false;
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
			currentSongInfo = subSongs[subSong];

			playingInfo = new GlobalPlayingInfo
			{
				MasterVolume = 64,

				SpeedCounter = currentSongInfo.StartSpeed,
				CurrentSpeed = currentSongInfo.StartSpeed,

				SongPosition = (short)(currentSongInfo.FirstPosition - 1),
				RowPosition = currentSongInfo.RowsPerTrack,
				RowsPerTrack = currentSongInfo.RowsPerTrack,
			};

			voices = new VoiceInfo[4];

			for (int i = 0; i < 4; i++)
			{
				voices[i] = new VoiceInfo
				{
					StartTrackRow = 0,
					SoundTranspose = 0,
					NoteTranspose = 0,

					Note = 0,
					Instrument = 0,
					DisableSoundTranspose = false,
					DisableNoteTranspose = false,
					Arpeggio = 0,
					Effect = Effect.Arpeggio,
					EffectArg = 0,

					TransposedNote = 0,
					PreviousTransposedNote = 0,

					InstrumentType = InstrumentType.Sample,
					InstrumentInfo = null,
					TransposedInstrument = 0,

					CurrentVolume = 0,
					VolumeSlideSpeed = 0,

					VibratoPosition = 0,
					VibratoDelay = 0,
					VibratoSpeed = 0,
					VibratoLevel = 0,

					PortamentoSpeed = 0,
					PortamentoPeriod = 0,

					ArpeggioPosition = 0,

					SlideSpeed = 0,
					SlideValue = 0,

					AdsrPosition = 0,
					AdsrDelayCounter = 0,
					SustainDelayCounter = 0,

					AmfPosition = 0,
					AmfDelayCounter = 0,

					SynthEffectPosition = 0,
					SynthEffectWavePosition = 0,
					EffectDelayCounter = 0,

					Flag = 0x01,
				};
			}

			PlayingFrequency = currentSongInfo.Tempo;

			endReached = false;
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player has allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			subSongs = null;
			positions = null;
			trackLines = null;
			instruments = null;

			sampleData = null;
			waveformData = null;
			adsrTables = null;
			amfTables = null;

			currentSongInfo = null;
			playingInfo = null;
			voices = null;
		}



		/********************************************************************/
		/// <summary>
		/// Get and parse next row
		/// </summary>
		/********************************************************************/
		private void GetNextRow()
		{
			playingInfo.RowPosition++;

			if (playingInfo.RowPosition >= playingInfo.RowsPerTrack)
			{
				playingInfo.RowPosition = 0;

				GetNextPosition();
			}

			GetNewNotes();
		}



		/********************************************************************/
		/// <summary>
		/// Get and parse next position
		/// </summary>
		/********************************************************************/
		private void GetNextPosition()
		{
			playingInfo.SongPosition++;

			if ((playingInfo.SongPosition > currentSongInfo.LastPosition) || (playingInfo.SongPosition >= positions.Length))
				playingInfo.SongPosition = (short)currentSongInfo.RestartPosition;

			if (HasPositionBeenVisited(playingInfo.SongPosition))
				endReached = true;

			MarkPositionAsVisited(playingInfo.SongPosition);

			SinglePositionInfo[] positionRow = positions[playingInfo.SongPosition];

			for (int i = 0; i < 4; i++)
			{
				SinglePositionInfo posInfo = positionRow[i];
				VoiceInfo voiceInfo = voices[i];

				voiceInfo.StartTrackRow = posInfo.StartTrackRow;
				voiceInfo.SoundTranspose = posInfo.SoundTranspose;
				voiceInfo.NoteTranspose = posInfo.NoteTranspose;
			}

			ShowPosition();
			ShowTracks();
		}



		/********************************************************************/
		/// <summary>
		/// Get and parse row information
		/// </summary>
		/********************************************************************/
		private void GetNewNotes()
		{
			for (int i = 0; i < 4; i++)
			{
				VoiceInfo voiceInfo = voices[i];
				IChannel channel = VirtualChannels[i];

				int position = voiceInfo.StartTrackRow + playingInfo.RowPosition;
				TrackLine trackLine = position < trackLines.Length ? trackLines[position] : new TrackLine();

				voiceInfo.Note = trackLine.Note;
				voiceInfo.Instrument = trackLine.Instrument;
				voiceInfo.DisableSoundTranspose = trackLine.DisableSoundTranspose;
				voiceInfo.DisableNoteTranspose = trackLine.DisableNoteTranspose;
				voiceInfo.Arpeggio = trackLine.Arpeggio;
				voiceInfo.Effect = trackLine.Effect;
				voiceInfo.EffectArg = trackLine.EffectArg;

				PlayVoice(voiceInfo, channel);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Initialize a single voice to play a sample
		/// </summary>
		/********************************************************************/
		private void PlayVoice(VoiceInfo voiceInfo, IChannel channel)
		{
			ushort note = voiceInfo.Note;
			short instrNum = voiceInfo.Instrument;

			if (note == 0)
			{
				if (instrNum != 0)
					SetInstrument(voiceInfo, (ushort)instrNum);
			}
			else
			{
				if (note != 0x80)
				{
					if (note == 0x7f)
						ForceQuiet(voiceInfo, channel);
					else
					{
						if (!voiceInfo.DisableNoteTranspose)
							note = (ushort)(note + voiceInfo.NoteTranspose);

						if ((instrNum != 0) && !voiceInfo.DisableSoundTranspose)
							instrNum = (short)(instrNum + voiceInfo.SoundTranspose);

						voiceInfo.PreviousTransposedNote = voiceInfo.TransposedNote;
						voiceInfo.TransposedNote = note;

						if (voiceInfo.PreviousTransposedNote == 0)
							voiceInfo.PreviousTransposedNote = note;

						Instrument instr;

						if (instrNum < 0)
						{
							voiceInfo.Flag |= 0x01;
							ForceQuiet(voiceInfo, channel);
							return;
						}

						if (instrNum == 0)
						{
							instr = voiceInfo.InstrumentInfo;

							if (instr == null)
							{
								ForceQuiet(voiceInfo, channel);
								return;
							}

							SetSynthInstrument(voiceInfo, instr);
						}
						else
							instr = SetInstrument(voiceInfo, (ushort)instrNum);

						voiceInfo.InstrumentType = instr.Type;

						if (voiceInfo.InstrumentType == InstrumentType.Sample)
							PlaySampleInstrument(voiceInfo, channel, instr);
						else
							PlaySynthInstrument(voiceInfo, channel, instr);
					}
				}
			}

			SetEffects(voiceInfo);
		}



		/********************************************************************/
		/// <summary>
		/// Make sure the voice is silent
		/// </summary>
		/********************************************************************/
		private void ForceQuiet(VoiceInfo voiceInfo, IChannel channel)
		{
			channel.Mute();

			voiceInfo.CurrentVolume = 0;
			voiceInfo.TransposedInstrument = 0;
			voiceInfo.InstrumentInfo = null;
			voiceInfo.InstrumentType = InstrumentType.Sample;
		}



		/********************************************************************/
		/// <summary>
		/// Set up the voice with a new instrument
		/// </summary>
		/********************************************************************/
		private Instrument SetInstrument(VoiceInfo voiceInfo, ushort instrNum)
		{
			voiceInfo.TransposedInstrument = (ushort)(instrNum & 0xff);

			Instrument instr = instruments[voiceInfo.TransposedInstrument - 1];
			voiceInfo.InstrumentInfo = instr;

			voiceInfo.CurrentVolume = instr.Volume;

			voiceInfo.PortamentoSpeed = instr.PortamentoSpeed;
			voiceInfo.PortamentoPeriod = 0;

			voiceInfo.VibratoPosition = 0;
			voiceInfo.VibratoDelay = instr.VibratoDelay;
			voiceInfo.VibratoSpeed = instr.VibratoSpeed;
			voiceInfo.VibratoLevel = instr.VibratoLevel;

			SetSynthInstrument(voiceInfo, instr);

			return instr;
		}



		/********************************************************************/
		/// <summary>
		/// Set up the voice with a new synth instrument
		/// </summary>
		/********************************************************************/
		private void SetSynthInstrument(VoiceInfo voiceInfo, Instrument instr)
		{
			voiceInfo.SlideValue = instr.FineTuning;

			voiceInfo.AdsrDelayCounter = instr.AdsrDelay;
			voiceInfo.AdsrPosition = 0;

			voiceInfo.AmfDelayCounter = instr.AmfDelay;
			voiceInfo.AmfPosition = 0;

			voiceInfo.SynthEffectPosition = instr.EffectArg2;
			voiceInfo.SynthEffectWavePosition = 0;
			voiceInfo.EffectDelayCounter = instr.EffectDelay;

			voiceInfo.ArpeggioPosition = 0;

			voiceInfo.Flag = 0x00;
		}



		/********************************************************************/
		/// <summary>
		/// Begin to play a sample instrument
		/// </summary>
		/********************************************************************/
		private void PlaySampleInstrument(VoiceInfo voiceInfo, IChannel channel, Instrument instr)
		{
			sbyte[] data = sampleData[instr.WaveformNumber];
			if (data == null)
			{
				voiceInfo.Flag |= 0x01;
				ForceQuiet(voiceInfo, channel);
			}
			else
			{
				uint playLength = instr.WaveformLength;
				uint loopStart = 0;
				uint loopLength = 0;

				if (instr.RepeatLength == 0)
					loopLength = instr.WaveformLength;
				else if (instr.RepeatLength != 1)
				{
					playLength += instr.RepeatLength;

					loopStart = instr.WaveformLength * 2U;
					loopLength = instr.RepeatLength * 2U;
				}

				playLength = (uint)Math.Min(playLength * 2, data.Length);
				if ((loopStart + loopLength) > playLength)
					loopLength = playLength - loopStart;

				channel.PlaySample((short)(voiceInfo.TransposedInstrument - 1), data, 0, playLength);

				if (loopLength != 0)
					channel.SetLoop(loopStart, loopLength);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Begin to play a synthesis instrument
		/// </summary>
		/********************************************************************/
		private void PlaySynthInstrument(VoiceInfo voiceInfo, IChannel channel, Instrument instr)
		{
			sbyte[] data = waveformData[instr.WaveformNumber];

			uint length = instr.WaveformLength * 2U;

			channel.PlaySample((short)(voiceInfo.TransposedInstrument - 1), voiceInfo.WaveformBuffer, 0, length);
			channel.SetLoop(0, length);

			Array.Copy(data, voiceInfo.WaveformBuffer, length);
		}



		/********************************************************************/
		/// <summary>
		/// Run effects and generate new synth sounds
		/// </summary>
		/********************************************************************/
		private void UpdateEffects()
		{
			for (int i = 0; i < 4; i++)
			{
				VoiceInfo voiceInfo = voices[i];
				IChannel channel = VirtualChannels[i];

				UpdateVoiceEffect(voiceInfo, channel);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Run effects for a single channel
		/// </summary>
		/********************************************************************/
		private void UpdateVoiceEffect(VoiceInfo voiceInfo, IChannel channel)
		{
			if (((voiceInfo.Flag & 0x01) != 0) || (voiceInfo.InstrumentInfo == null))
			{
				channel.Mute();
				return;
			}

			Instrument instr = voiceInfo.InstrumentInfo;

			PeriodInfo periodInfo = DoArpeggio(voiceInfo, instr);
			DoPortamento(periodInfo, voiceInfo);
			DoVibrato(periodInfo, voiceInfo, instr);
			DoAmf(periodInfo, voiceInfo, instr);
			DoSlide(periodInfo, voiceInfo);

			channel.SetAmigaPeriod(periodInfo.Period);

			if (instr.Type == InstrumentType.Synth)
				DoSynthEffects(voiceInfo, instr);

			DoAdsr(voiceInfo, channel, instr);
			DoVolumeSlide(voiceInfo);
		}



		/********************************************************************/
		/// <summary>
		/// Handle arpeggio
		/// </summary>
		/********************************************************************/
		private PeriodInfo DoArpeggio(VoiceInfo voiceInfo, Instrument instr)
		{
			ushort note = voiceInfo.TransposedNote;
			ushort prevNote = voiceInfo.PreviousTransposedNote;

			if (voiceInfo.Arpeggio != 0)
			{
				Arpeggio arp = instr.Arpeggios[voiceInfo.Arpeggio - 1];

				byte arpVal = arp.Values[voiceInfo.ArpeggioPosition];
				note += arpVal;
				prevNote += arpVal;

				voiceInfo.ArpeggioPosition++;

				int maxLength = Math.Min(arp.Length + arp.Repeat, 13);
				if (voiceInfo.ArpeggioPosition > maxLength)
					voiceInfo.ArpeggioPosition = arp.Length;
			}
			else
			{
				if ((voiceInfo.Effect == Effect.Arpeggio) && (voiceInfo.EffectArg != 0))
				{
					ushort arpVal;

					switch (playingInfo.SpeedCounter % 3)
					{
						default:
						case 0:
						{
							arpVal = 0;
							break;
						}

						case 1:
						{
							arpVal = (ushort)(voiceInfo.EffectArg >> 4);
							break;
						}

						case 2:
						{
							arpVal = (ushort)(voiceInfo.EffectArg & 0x0f);
							break;
						}
					}

					note += arpVal;
					prevNote += arpVal;
				}
			}

			if (note >= 109)
				note = 0;

			if (prevNote >= 109)
				prevNote = 0;

			ushort period = Tables.Periods[note];
			ushort previousPeriod = Tables.Periods[prevNote];

			return new PeriodInfo
			{
				Period = period,
				PreviousPeriod = previousPeriod
			};
		}



		/********************************************************************/
		/// <summary>
		/// Handle portamento
		/// </summary>
		/********************************************************************/
		private void DoPortamento(PeriodInfo periodInfo, VoiceInfo voiceInfo)
		{
			if (voiceInfo.PortamentoSpeed != 0)
			{
				if (voiceInfo.PortamentoPeriod == 0)
					voiceInfo.PortamentoPeriod = periodInfo.PreviousPeriod;

				int diff = periodInfo.Period - voiceInfo.PortamentoPeriod;
				if (diff < 0)
					diff = -diff;

				diff -= voiceInfo.PortamentoSpeed;
				if (diff < 0)
					voiceInfo.PortamentoSpeed = 0;
				else
				{
					ushort newPeriod = voiceInfo.PortamentoPeriod;

					if (newPeriod >= periodInfo.Period)
						newPeriod -= voiceInfo.PortamentoSpeed;
					else
						newPeriod += voiceInfo.PortamentoSpeed;

					voiceInfo.PortamentoPeriod = newPeriod;
					periodInfo.Period = newPeriod;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle vibrato
		/// </summary>
		/********************************************************************/
		private void DoVibrato(PeriodInfo periodInfo, VoiceInfo voiceInfo, Instrument instr)
		{
			if (voiceInfo.VibratoDelay != 255)
			{
				if (voiceInfo.VibratoDelay == 0)
				{
					sbyte vibVal = Tables.Vibrato[voiceInfo.VibratoPosition];
					ushort vibLevel = instr.VibratoLevel;

					if (vibVal != 0)
						periodInfo.Period += (ushort)((vibVal * 4) / vibLevel);

					voiceInfo.VibratoPosition = (ushort)((voiceInfo.VibratoPosition + instr.VibratoSpeed) & 0xff);
				}
				else
					voiceInfo.VibratoDelay--;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle AMF
		/// </summary>
		/********************************************************************/
		private void DoAmf(PeriodInfo periodInfo, VoiceInfo voiceInfo, Instrument instr)
		{
			if ((instr.AmfLength + instr.AmfRepeat) != 0)
			{
				sbyte[] amfTable = amfTables[instr.AmfNumber];
				sbyte amfVal = amfTable[voiceInfo.AmfPosition];

				periodInfo.Period = (ushort)(periodInfo.Period - amfVal);

				voiceInfo.AmfDelayCounter--;
				if (voiceInfo.AmfDelayCounter == 0)
				{
					voiceInfo.AmfDelayCounter = instr.AmfDelay;

					voiceInfo.AmfPosition++;

					if (voiceInfo.AmfPosition >= (instr.AmfLength + instr.AmfRepeat))
					{
						voiceInfo.AmfPosition = instr.AmfLength;

						if (instr.AmfRepeat == 0)
							voiceInfo.AmfPosition--;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle slide
		/// </summary>
		/********************************************************************/
		private void DoSlide(PeriodInfo periodInfo, VoiceInfo voiceInfo)
		{
			periodInfo.Period = (ushort)(periodInfo.Period - voiceInfo.SlideValue);
			if (periodInfo.Period < 113)
				periodInfo.Period = 113;

			if (playingInfo.SpeedCounter != 0)
				voiceInfo.SlideValue += voiceInfo.SlideSpeed;
		}



		/********************************************************************/
		/// <summary>
		/// Handle ADSR
		/// </summary>
		/********************************************************************/
		private void DoAdsr(VoiceInfo voiceInfo, IChannel channel, Instrument instr)
		{
			if ((instr.AdsrLength + instr.AdsrRepeat) == 0)
				channel.SetAmigaVolume((ushort)((voiceInfo.CurrentVolume * playingInfo.MasterVolume) / 64));
			else
			{
				byte[] adsrTable = adsrTables[instr.AdsrNumber];
				byte adsrVal = adsrTable[voiceInfo.AdsrPosition];

				ushort vol = (ushort)((playingInfo.MasterVolume * voiceInfo.CurrentVolume * adsrVal) / 4096);
				if (vol > 64)
					vol = 64;

				channel.SetAmigaVolume(vol);

				if ((voiceInfo.Note == 0x80) && (voiceInfo.AdsrPosition >= instr.SustainPoint))
				{
					if (instr.SustainDelay == 0)
						return;

					if (voiceInfo.SustainDelayCounter != 0)
					{
						voiceInfo.SustainDelayCounter--;
						return;
					}

					voiceInfo.SustainDelayCounter = instr.SustainDelay;
				}

				voiceInfo.AdsrDelayCounter--;

				if (voiceInfo.AdsrDelayCounter == 0)
				{
					voiceInfo.AdsrDelayCounter = instr.AdsrDelay;

					voiceInfo.AdsrPosition++;

					if (voiceInfo.AdsrPosition >= (instr.AdsrLength + instr.AdsrRepeat))
					{
						voiceInfo.AdsrPosition = instr.AdsrLength;

						if (instr.AdsrRepeat == 0)
							voiceInfo.AdsrPosition--;

						if ((instr.AdsrRepeat == 0) && (vol == 0))
							voiceInfo.Flag |= 0x01;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle volume slide
		/// </summary>
		/********************************************************************/
		private void DoVolumeSlide(VoiceInfo voiceInfo)
		{
			short vol = (short)(voiceInfo.CurrentVolume + voiceInfo.VolumeSlideSpeed);

			if (vol < 0)
				vol = 0;
			else if (vol > 64)
				vol = 64;

			voiceInfo.CurrentVolume = (ushort)vol;
		}



		/********************************************************************/
		/// <summary>
		/// Parse the effect and set up the voice
		/// </summary>
		/********************************************************************/
		private void SetEffects(VoiceInfo voiceInfo)
		{
			voiceInfo.SlideSpeed = 0;
			voiceInfo.VolumeSlideSpeed = 0;

			switch (voiceInfo.Effect)
			{
				case Effect.SetSlideSpeed:
				{
					voiceInfo.SlideSpeed = (sbyte)voiceInfo.EffectArg;
					break;
				}

				case Effect.RestartAdsr:
				{
					voiceInfo.AdsrPosition = voiceInfo.EffectArg;
					break;
				}

				case Effect.SetVibrato:
				{
					voiceInfo.VibratoDelay = 0;
					voiceInfo.VibratoSpeed = (ushort)(((voiceInfo.EffectArg & 0xf0) >> 3));
					voiceInfo.VibratoLevel = (ushort)((-((voiceInfo.EffectArg & 0x0f) << 4)) + 160);
					break;
				}

				case Effect.SetMasterVolume:
				{
					playingInfo.MasterVolume = (ushort)(voiceInfo.EffectArg == 64 ? 64 : (voiceInfo.EffectArg & 0x3f));
					break;
				}

				case Effect.SetPortamento:
				{
					voiceInfo.PortamentoSpeed = voiceInfo.EffectArg;
					break;
				}

				case Effect.SkipPortamento:
				{
					voiceInfo.PortamentoSpeed = 0;
					break;
				}

				case Effect.SetTrackLen:
				{
					if (voiceInfo.EffectArg <= 64)
						playingInfo.RowsPerTrack = voiceInfo.EffectArg;

					break;
				}

				case Effect.VolumeSlide:
				{
					voiceInfo.VolumeSlideSpeed = (sbyte)(voiceInfo.EffectArg);
					break;
				}

				case Effect.PositionJump:
				{
					playingInfo.SongPosition = (short)(voiceInfo.EffectArg - 1);
					playingInfo.RowPosition = playingInfo.RowsPerTrack;
					break;
				}

				case Effect.SetVolume:
				{
					byte newVol = voiceInfo.EffectArg;

					if (newVol > 64)
						newVol = 64;

					voiceInfo.CurrentVolume = newVol;
					break;
				}

				case Effect.TrackBreak:
				{
					playingInfo.RowPosition = playingInfo.RowsPerTrack;
					break;
				}

				case Effect.SetFilter:
				{
					AmigaFilter = voiceInfo.EffectArg == 0;
					break;
				}

				case Effect.SetSpeed:
				{
					if ((voiceInfo.EffectArg > 0) && (voiceInfo.EffectArg <= 16))
					{
						playingInfo.CurrentSpeed = voiceInfo.EffectArg;
						ShowSpeed();
					}
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle the synth effects
		/// </summary>
		/********************************************************************/
		private void DoSynthEffects(VoiceInfo voiceInfo, Instrument instr)
		{
			voiceInfo.EffectDelayCounter--;

			if (voiceInfo.EffectDelayCounter == 0)
			{
				voiceInfo.EffectDelayCounter = instr.EffectDelay;

				switch (instr.Effect)
				{
					case SynthesisEffect.WaveNegator:
					{
						DoSynthEffectWaveNegator(voiceInfo, instr);
						break;
					}

					case SynthesisEffect.FreeNegator:
					{
						DoSynthEffectFreeNegator(voiceInfo, instr);
						break;
					}

					case SynthesisEffect.RotateVertical:
					{
						DoSynthEffectRotateVertical(voiceInfo, instr);
						break;
					}

					case SynthesisEffect.RotateHorizontal:
					{
						DoSynthEffectRotateHorizontal(voiceInfo, instr);
						break;
					}

					case SynthesisEffect.AlienVoice:
					{
						DoSynthEffectAlienVoice(voiceInfo, instr);
						break;
					}

					case SynthesisEffect.PolyNegator:
					{
						DoSynthEffectPolyNegator(voiceInfo, instr);
						break;
					}

					case SynthesisEffect.ShackWave1:
					{
						DoSynthEffectShackWave1(voiceInfo, instr);
						break;
					}

					case SynthesisEffect.ShackWave2:
					{
						DoSynthEffectShackWave2(voiceInfo, instr);
						break;
					}

					case SynthesisEffect.Metamorph:
					{
						DoSynthEffectMetamorph(voiceInfo, instr);
						break;
					}

					case SynthesisEffect.Laser:
					{
						DoSynthEffectLaser(voiceInfo, instr);
						break;
					}

					case SynthesisEffect.WaveAlias:
					{
						DoSynthEffectWaveAlias(voiceInfo, instr);
						break;
					}

					case SynthesisEffect.NoiseGenerator1:
					{
						DoSynthEffectNoiseGenerator1(voiceInfo, instr);
						break;
					}

					case SynthesisEffect.LowPassFilter1:
					{
						DoSynthEffectLowPassFilter1(voiceInfo, instr);
						break;
					}

					case SynthesisEffect.LowPassFilter2:
					{
						DoSynthEffectLowPassFilter2(voiceInfo, instr);
						break;
					}

					case SynthesisEffect.Oszilator:
					{
						DoSynthEffectOszilator(voiceInfo, instr);
						break;
					}

					case SynthesisEffect.NoiseGenerator2:
					{
						DoSynthEffectNoiseGenerator2(voiceInfo, instr);
						break;
					}

					case SynthesisEffect.FmDrum:
					{
						DoSynthEffectFmDrum(voiceInfo, instr);
						break;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Increment synth effect position
		/// </summary>
		/********************************************************************/
		private void IncrementSynthEffectPosition(VoiceInfo voiceInfo, Instrument instr)
		{
			ushort startPosition = instr.EffectArg2;
			ushort stopPosition = instr.EffectArg3;

			voiceInfo.SynthEffectPosition++;

			if (voiceInfo.SynthEffectPosition >= stopPosition)
				voiceInfo.SynthEffectPosition = startPosition;
		}



		/********************************************************************/
		/// <summary>
		/// Handle the Wave Negator synth effect
		/// </summary>
		/********************************************************************/
		private void DoSynthEffectWaveNegator(VoiceInfo voiceInfo, Instrument instr)
		{
			voiceInfo.WaveformBuffer[voiceInfo.SynthEffectPosition] = (sbyte)-voiceInfo.WaveformBuffer[voiceInfo.SynthEffectPosition];

			IncrementSynthEffectPosition(voiceInfo, instr);
		}



		/********************************************************************/
		/// <summary>
		/// Handle the Free Negator synth effect
		/// </summary>
		/********************************************************************/
		private void DoSynthEffectFreeNegator(VoiceInfo voiceInfo, Instrument instr)
		{
			if ((voiceInfo.Flag & 0x04) == 0)
			{
				sbyte[] destWaveform = voiceInfo.WaveformBuffer;

				ushort waveformNumber = instr.EffectArg1;
				ushort waveLength = instr.EffectArg2;
				ushort waveRepeat = instr.EffectArg3;

				sbyte[] waveform = waveformData[waveformNumber];
				short waveVal = (short)(waveform[voiceInfo.SynthEffectWavePosition] & 0x7f);

				sbyte[] sourceWaveform = waveformData[instr.WaveformNumber];
				short count = (short)(instr.WaveformLength * 2);

				int bufferOffset = count;

				while ((count > 0) && (count >= waveVal))
				{
					bufferOffset--;

					destWaveform[bufferOffset] = sourceWaveform[bufferOffset];
					count--;
				}

				short left = (short)(waveVal - count);
				count += left;
				bufferOffset += left;

				while (count > 0)
				{
					bufferOffset--;

					destWaveform[bufferOffset] = (sbyte)-sourceWaveform[bufferOffset];
					count--;
				}

				voiceInfo.SynthEffectWavePosition++;

				if (voiceInfo.SynthEffectWavePosition > (waveLength + waveRepeat))
				{
					voiceInfo.SynthEffectWavePosition = waveLength;

					if ((waveRepeat == 0) && (waveVal == 0))
						voiceInfo.Flag |= 0x04;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle the Rotate Vertical synth effect
		/// </summary>
		/********************************************************************/
		private void DoSynthEffectRotateVertical(VoiceInfo voiceInfo, Instrument instr)
		{
			sbyte[] destWaveform = voiceInfo.WaveformBuffer;

			sbyte deltaValue = (sbyte)instr.EffectArg1;
			ushort startPosition = instr.EffectArg2;
			ushort stopPosition = instr.EffectArg3;

			short count = (short)(stopPosition - startPosition);
			int bufferOffset = startPosition;

			while (count >= 0)
			{
				destWaveform[bufferOffset++] += deltaValue;
				count--;
			}

			IncrementSynthEffectPosition(voiceInfo, instr);
		}



		/********************************************************************/
		/// <summary>
		/// Handle the Rotate Horizontal synth effect
		/// </summary>
		/********************************************************************/
		private void DoSynthEffectRotateHorizontal(VoiceInfo voiceInfo, Instrument instr)
		{
			sbyte[] destWaveform = voiceInfo.WaveformBuffer;

			ushort startPosition = instr.EffectArg2;
			ushort stopPosition = instr.EffectArg3;

			short count = (short)(stopPosition - startPosition);
			int bufferOffset = startPosition;

			sbyte firstByte = destWaveform[bufferOffset];

			do
			{
				destWaveform[bufferOffset] = destWaveform[bufferOffset + 1];
				bufferOffset++;
				count--;
			}
			while (count >= 0);

			destWaveform[bufferOffset] = firstByte;

			IncrementSynthEffectPosition(voiceInfo, instr);
		}



		/********************************************************************/
		/// <summary>
		/// Handle the Alien Voice synth effect
		/// </summary>
		/********************************************************************/
		private void DoSynthEffectAlienVoice(VoiceInfo voiceInfo, Instrument instr)
		{
			sbyte[] destWaveform = voiceInfo.WaveformBuffer;

			ushort waveformNumber = instr.EffectArg1;
			ushort startPosition = instr.EffectArg2;
			ushort stopPosition = instr.EffectArg3;

			sbyte[] sourceWaveform = waveformData[waveformNumber];
			int bufferOffset = startPosition;

			short count = (short)(stopPosition - startPosition);

			while (count >= 0)
			{
				destWaveform[bufferOffset] += sourceWaveform[bufferOffset];
				bufferOffset++;
				count--;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle the Poly Negator synth effect
		/// </summary>
		/********************************************************************/
		private void DoSynthEffectPolyNegator(VoiceInfo voiceInfo, Instrument instr)
		{
			sbyte[] destWaveform = voiceInfo.WaveformBuffer;
			sbyte[] sourceWaveform = waveformData[instr.WaveformNumber];

			ushort position = voiceInfo.SynthEffectPosition;
			ushort startPosition = instr.EffectArg2;
			ushort stopPosition = instr.EffectArg3;

			destWaveform[position] = sourceWaveform[position];

			if (position >= stopPosition)
				position = (ushort)(startPosition - 1);

			destWaveform[position + 1] = (sbyte)-destWaveform[position + 1];

			IncrementSynthEffectPosition(voiceInfo, instr);
		}



		/********************************************************************/
		/// <summary>
		/// Handle the Shack Wave 1 synth effect
		/// </summary>
		/********************************************************************/
		private void DoSynthEffectShackWave1(VoiceInfo voiceInfo, Instrument instr)
		{
			ShackWaveHelper(voiceInfo, instr);
			IncrementSynthEffectPosition(voiceInfo, instr);
		}



		/********************************************************************/
		/// <summary>
		/// Handle the Shack Wave 2 synth effect
		/// </summary>
		/********************************************************************/
		private void DoSynthEffectShackWave2(VoiceInfo voiceInfo, Instrument instr)
		{
			ShackWaveHelper(voiceInfo, instr);

			sbyte[] destWaveform = voiceInfo.WaveformBuffer;

			ushort startPosition = instr.EffectArg2;
			ushort stopPosition = instr.EffectArg3;

			destWaveform[startPosition] = (sbyte)-destWaveform[startPosition];

			voiceInfo.SynthEffectWavePosition++;

			if (voiceInfo.SynthEffectWavePosition > (stopPosition - startPosition))
				voiceInfo.SynthEffectWavePosition = 0;

			IncrementSynthEffectPosition(voiceInfo, instr);
		}



		/********************************************************************/
		/// <summary>
		/// Handle the Metamorph synth effect
		/// </summary>
		/********************************************************************/
		private void DoSynthEffectMetamorph(VoiceInfo voiceInfo, Instrument instr)
		{
			if ((voiceInfo.Flag & 0x01) == 0)
			{
				ushort waveformNumber = instr.EffectArg1;

				sbyte[] waveform = waveformData[waveformNumber];
				MetamorphAndOszilatorHelper(voiceInfo, instr, waveform);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle the Laser synth effect
		/// </summary>
		/********************************************************************/
		private void DoSynthEffectLaser(VoiceInfo voiceInfo, Instrument instr)
		{
			sbyte detune = (sbyte)instr.EffectArg2;
			ushort repeats = instr.EffectArg3;

			if (voiceInfo.SynthEffectWavePosition < repeats)
			{
				voiceInfo.SlideValue += detune;
				voiceInfo.SynthEffectWavePosition++;
			}

			IncrementSynthEffectPosition(voiceInfo, instr);
		}



		/********************************************************************/
		/// <summary>
		/// Handle the Wave Alias synth effect
		/// </summary>
		/********************************************************************/
		private void DoSynthEffectWaveAlias(VoiceInfo voiceInfo, Instrument instr)
		{
			sbyte[] destWaveform = voiceInfo.WaveformBuffer;

			sbyte deltaValue = (sbyte)instr.EffectArg1;
			ushort startPosition = instr.EffectArg2;
			ushort stopPosition = instr.EffectArg3;

			short count = (short)(stopPosition - startPosition);
			int bufferOffset = startPosition;

			while (count >= 0)
			{
				sbyte val = destWaveform[bufferOffset];

				if (((bufferOffset + 1) >= destWaveform.Length) || (val > destWaveform[bufferOffset + 1]))
					val -= deltaValue;
				else
					val += deltaValue;

				destWaveform[bufferOffset++] = val;
				count--;
			}

			IncrementSynthEffectPosition(voiceInfo, instr);
		}



		/********************************************************************/
		/// <summary>
		/// Handle the Noise Generator 1 synth effect
		/// </summary>
		/********************************************************************/
		private void DoSynthEffectNoiseGenerator1(VoiceInfo voiceInfo, Instrument instr)
		{
			sbyte[] destWaveform = voiceInfo.WaveformBuffer;
			destWaveform[voiceInfo.SynthEffectPosition] ^= (sbyte)RandomGenerator.GetRandomNumber(-128, 127);

			IncrementSynthEffectPosition(voiceInfo, instr);
		}



		/********************************************************************/
		/// <summary>
		/// Handle the Low Pass Filter 1 synth effect
		/// </summary>
		/********************************************************************/
		private void DoSynthEffectLowPassFilter1(VoiceInfo voiceInfo, Instrument instr)
		{
			sbyte[] destWaveform = voiceInfo.WaveformBuffer;

			ushort deltaValue = instr.EffectArg1;
			ushort startPosition = instr.EffectArg2;
			ushort stopPosition = instr.EffectArg3;

			for (int i = startPosition; i <= stopPosition; i++)
			{
				bool flag = false;

				sbyte val1 = destWaveform[i];
				sbyte val2 = i == stopPosition ? destWaveform[startPosition] : destWaveform[i + 1];

				if (val1 <= val2)
					flag = true;

				val1 -= val2;
				if (val1 < 0)
					val1 = (sbyte)-val1;

				if (val1 > deltaValue)
				{
					if (flag)
						destWaveform[i] += 2;
					else
						destWaveform[i] -= 2;
				}
			}

			IncrementSynthEffectPosition(voiceInfo, instr);
		}



		/********************************************************************/
		/// <summary>
		/// Handle the Low Pass Filter 2 synth effect
		/// </summary>
		/********************************************************************/
		private void DoSynthEffectLowPassFilter2(VoiceInfo voiceInfo, Instrument instr)
		{
			ushort waveformNumber = instr.EffectArg1;
			ushort startPosition = instr.EffectArg2;
			ushort stopPosition = instr.EffectArg3;

			sbyte[] waveform = waveformData[waveformNumber];
			sbyte[] destWaveform = voiceInfo.WaveformBuffer;
			int bufferIndex = stopPosition;

			for (int i = startPosition; i <= stopPosition; i++)
			{
				bool flag = false;

				sbyte val1 = destWaveform[i];
				sbyte val2 = i == stopPosition ? destWaveform[startPosition] : destWaveform[i + 1];

				if (val1 <= val2)
					flag = true;

				val1 -= val2;
				if (val1 < 0)
					val1 = (sbyte)-val1;

				byte deltaValue = (byte)(waveform[bufferIndex++] & 0x7f);

				if (val1 > deltaValue)
				{
					if (flag)
						destWaveform[i] += 2;
					else
						destWaveform[i] -= 2;
				}
			}

			IncrementSynthEffectPosition(voiceInfo, instr);
		}



		/********************************************************************/
		/// <summary>
		/// Handle the Oszilator synth effect
		/// </summary>
		/********************************************************************/
		private void DoSynthEffectOszilator(VoiceInfo voiceInfo, Instrument instr)
		{
			if ((voiceInfo.Flag & 0x02) != 0)
			{
				voiceInfo.Flag ^= 0x08;
				voiceInfo.Flag &= 0b11111101; // ~0x02;
			}

			sbyte[] sourceWaveform;

			if ((voiceInfo.Flag & 0x08) != 0)
				sourceWaveform = waveformData[instr.WaveformNumber];
			else
				sourceWaveform = waveformData[instr.EffectArg1];

			MetamorphAndOszilatorHelper(voiceInfo, instr, sourceWaveform);
		}



		/********************************************************************/
		/// <summary>
		/// Handle the Noise Generator 2 synth effect
		/// </summary>
		/********************************************************************/
		private void DoSynthEffectNoiseGenerator2(VoiceInfo voiceInfo, Instrument instr)
		{
			sbyte[] destWaveform = voiceInfo.WaveformBuffer;

			ushort startPosition = instr.EffectArg2;
			ushort stopPosition = instr.EffectArg3;

			short count = (short)(stopPosition - startPosition);
			int bufferOffset = startPosition;

			do
			{
				sbyte val = destWaveform[bufferOffset + count];
				val ^= 0x05;
				val = (sbyte)((val << 2) | (val >> (8 - 2)));
				val += (sbyte)RandomGenerator.GetRandomNumber(-128, 127);

				destWaveform[bufferOffset + count] = val;
				count--;
			}
			while (count >= 0);
		}



		/********************************************************************/
		/// <summary>
		/// Handle the FM Drum synth effect
		/// </summary>
		/********************************************************************/
		private void DoSynthEffectFmDrum(VoiceInfo voiceInfo, Instrument instr)
		{
			byte level = (byte)instr.EffectArg1;
			ushort factor = instr.EffectArg2;
			ushort repeats = instr.EffectArg3;

			if (voiceInfo.SynthEffectWavePosition >= repeats)
			{
				voiceInfo.SlideValue = instr.FineTuning;
				voiceInfo.SynthEffectWavePosition = 0;
			}

			ushort decrement = (ushort)((factor << 8) | level);
			voiceInfo.SlideValue = (short)(voiceInfo.SlideValue - decrement);

			voiceInfo.SynthEffectWavePosition++;

			IncrementSynthEffectPosition(voiceInfo, instr);
		}



		/********************************************************************/
		/// <summary>
		/// Shack Wave synth effect helper
		/// </summary>
		/********************************************************************/
		private void ShackWaveHelper(VoiceInfo voiceInfo, Instrument instr)
		{
			sbyte[] destWaveform = voiceInfo.WaveformBuffer;

			ushort waveformNumber = instr.EffectArg1;
			ushort startPosition = instr.EffectArg2;
			ushort stopPosition = instr.EffectArg3;

			sbyte[] sourceWaveform = waveformData[waveformNumber];
			sbyte deltaValue = sourceWaveform[startPosition + voiceInfo.SynthEffectPosition];

			int bufferOffset = startPosition;
			short count = (short)(stopPosition - startPosition);

			while (count >= 0)
			{
				destWaveform[bufferOffset++] += deltaValue;
				count--;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Metamorph and Oszilator synth effect helper
		/// </summary>
		/********************************************************************/
		private void MetamorphAndOszilatorHelper(VoiceInfo voiceInfo, Instrument instr, sbyte[] sourceWaveform)
		{
			sbyte[] destWaveform = voiceInfo.WaveformBuffer;

			ushort startPosition = instr.EffectArg2;
			ushort stopPosition = instr.EffectArg3;

			int bufferOffset = startPosition;
			short count = (short)(stopPosition - startPosition);
			bool setFlag = false;

			while (count >= 0)
			{
				sbyte val = destWaveform[bufferOffset];
				if (val != sourceWaveform[bufferOffset])
				{
					setFlag = true;

					if (val < sourceWaveform[bufferOffset])
						val++;
					else
						val--;

					destWaveform[bufferOffset] = val;
				}

				bufferOffset++;
				count--;
			}

			if (!setFlag)
				voiceInfo.Flag |= 0x02;
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
		/// Will update the module information with all dynamic values
		/// </summary>
		/********************************************************************/
		private void UpdateModuleInformation()
		{
			ShowPosition();
			ShowTracks();
			ShowSpeed();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the playing position
		/// </summary>
		/********************************************************************/
		private string FormatPosition()
		{
			if (playingInfo.SongPosition < 0)
				return "0";

			return playingInfo.SongPosition.ToString();
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
				sb.Append(voices[i].StartTrackRow);
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
			return playingInfo.CurrentSpeed.ToString();
		}
		#endregion
	}
}
