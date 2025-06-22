/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.InStereo10.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.InStereo10
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class InStereo10Worker : ModulePlayerWithPositionDurationAgentBase
	{
		private class PeriodInfo
		{
			public ushort Period;
			public ushort PreviousPeriod;
		}

		private string moduleName;
		private SongInfo[] subSongs;
		private SinglePositionInfo[][] positions;
		private TrackLine[] trackLines;

		private Sample[] samples;
		private sbyte[][] waveforms;
		private Instrument[] instruments;

		private byte[] envelopeGeneratorTables;
		private byte[] adsrTables;
		private byte[] arpeggioTables;

		private sbyte[] vibratoTable;

		private SongInfo currentSongInfo;

		private GlobalPlayingInfo playingInfo;
		private VoiceInfo[] voices;

		private bool endReached;

		private const int EnvelopeGeneratorTableLength = 128;
		private const int AdsrTableLength = 256;
		private const int ArpeggioTableLength = 16;

		private const int InfoPositionLine = 5;
		private const int InfoTrackLine = 6;
		private const int InfoSpeedLine = 7;

		#region Identify
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => [ "is", "is10" ];



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
			if (fileSize < 204)
				return AgentResult.Unknown;

			// Check the mark
			moduleStream.Seek(0, SeekOrigin.Begin);

			if (moduleStream.ReadMark(8) == "ISM!V1.2")
				return AgentResult.Ok;

			return AgentResult.Unknown;
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
				Encoding encoder = EncoderCollection.Amiga;

				// Read meta data, so we know the size of the different parts
				moduleStream.Seek(8, SeekOrigin.Begin);

				ushort totalNumberOfPositions = moduleStream.Read_B_UINT16();
				ushort totalNumberOfTrackRows = moduleStream.Read_B_UINT16();

				moduleStream.Seek(4, SeekOrigin.Current);

				byte numberOfSamples = moduleStream.Read_UINT8();
				byte numberOfWaveforms = moduleStream.Read_UINT8();
				byte numberOfInstruments = moduleStream.Read_UINT8();
				byte numberOfSubSongs = moduleStream.Read_UINT8();
				byte numberOfEnvelopeGeneratorTables = moduleStream.Read_UINT8();
				byte numberOfAdsrTables = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_IS10_ERR_LOADING_HEADER;
					return AgentResult.Error;
				}

				// Read the module name
				moduleStream.Seek(14, SeekOrigin.Current);

				moduleName = moduleStream.ReadString(encoder, 28);

				// Skip some text
				moduleStream.Seek(140, SeekOrigin.Current);

				// Read sample information
				samples = new Sample[numberOfSamples];

				for (int i = 0; i < numberOfSamples; i++)
				{
					Sample sample = new Sample();

					moduleStream.Seek(1, SeekOrigin.Current);
					sample.Name = moduleStream.ReadString(encoder, 23);

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_IS10_ERR_LOADING_SAMPLEINFO;
						return AgentResult.Error;
					}

					moduleStream.Seek(4, SeekOrigin.Current);

					samples[i] = sample;
				}

				for (int i = 0; i < numberOfSamples; i++)
					samples[i].Length = moduleStream.Read_B_UINT32();

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_IS10_ERR_LOADING_SAMPLEINFO;
					return AgentResult.Error;
				}

				// Read envelope generator tables
				envelopeGeneratorTables = new byte[numberOfEnvelopeGeneratorTables * EnvelopeGeneratorTableLength];
				int bytesRead = moduleStream.Read(envelopeGeneratorTables, 0, envelopeGeneratorTables.Length);

				if (bytesRead < envelopeGeneratorTables.Length)
				{
					errorMessage = Resources.IDS_IS10_ERR_LOADING_ENVELOPEGENERATOR;
					return AgentResult.Error;
				}

				// Read ADSR tables
				adsrTables = new byte[numberOfAdsrTables * AdsrTableLength];
				bytesRead = moduleStream.Read(adsrTables, 0, adsrTables.Length);

				if (bytesRead < adsrTables.Length)
				{
					errorMessage = Resources.IDS_IS10_ERR_LOADING_ADSR;
					return AgentResult.Error;
				}

				// Read instrument information
				instruments = new Instrument[numberOfInstruments];

				for (int i = 0; i < numberOfInstruments; i++)
				{
					Instrument instr = new Instrument();

					instr.WaveformNumber = moduleStream.Read_UINT8();
					instr.SynthesisEnabled = moduleStream.Read_UINT8() != 0;
					instr.WaveformLength = moduleStream.Read_B_UINT16();
					instr.RepeatLength = moduleStream.Read_B_UINT16();
					instr.Volume = moduleStream.Read_UINT8();
					instr.PortamentoSpeed = moduleStream.Read_INT8();
					instr.AdsrEnabled = moduleStream.Read_UINT8() != 0;
					instr.AdsrTableNumber = moduleStream.Read_UINT8();
					instr.AdsrTableLength = moduleStream.Read_B_UINT16();
					moduleStream.Seek(2, SeekOrigin.Current);
					instr.PortamentoEnabled = moduleStream.Read_UINT8() != 0;
					moduleStream.Seek(5, SeekOrigin.Current);
					instr.VibratoDelay = moduleStream.Read_UINT8();
					instr.VibratoSpeed = moduleStream.Read_UINT8();
					instr.VibratoLevel = moduleStream.Read_UINT8();
					instr.EnvelopeGeneratorCounterOffset = moduleStream.Read_UINT8();
					instr.EnvelopeGeneratorCounterMode = (EnvelopeGeneratorCounterMode)moduleStream.Read_UINT8();
					instr.EnvelopeGeneratorCounterTableNumber = moduleStream.Read_UINT8();
					instr.EnvelopeGeneratorCounterTableLength = moduleStream.Read_B_UINT16();

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_IS10_ERR_LOADING_INSTRUMENT;
						return AgentResult.Error;
					}

					instruments[i] = instr;
				}

				// Read arpeggio tables
				arpeggioTables = new byte[16 * ArpeggioTableLength];
				bytesRead = moduleStream.Read(arpeggioTables, 0, arpeggioTables.Length);

				if (bytesRead < arpeggioTables.Length)
				{
					errorMessage = Resources.IDS_IS10_ERR_LOADING_ARPEGGIO;
					return AgentResult.Error;
				}

				// Read sub-song information
				subSongs = new SongInfo[numberOfSubSongs];

				for (int i = 0; i < numberOfSubSongs; i++)
				{
					moduleStream.Seek(4, SeekOrigin.Current);

					SongInfo songInfo = new SongInfo();

					songInfo.StartSpeed = moduleStream.Read_UINT8();
					songInfo.RowsPerTrack = moduleStream.Read_UINT8();
					songInfo.FirstPosition = moduleStream.Read_B_UINT16();
					songInfo.LastPosition = moduleStream.Read_B_UINT16();
					songInfo.RestartPosition = moduleStream.Read_B_UINT16();

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_IS10_ERR_LOADING_HEADER;
						return AgentResult.Error;
					}

					moduleStream.Seek(2, SeekOrigin.Current);

					subSongs[i] = songInfo;
				}

				// Skip extra sub-song information, since it is not needed
				moduleStream.Seek(14, SeekOrigin.Current);

				// Read waveforms
				waveforms = new sbyte[numberOfWaveforms][];

				for (int i = 0; i < numberOfWaveforms; i++)
				{
					waveforms[i] = new sbyte[256];

					moduleStream.ReadSigned(waveforms[i], 0, 256);

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_IS10_ERR_LOADING_WAVEFORM;
						return AgentResult.Error;
					}
				}

				// Read position information
				positions = new SinglePositionInfo[totalNumberOfPositions][];

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
					{
						errorMessage = Resources.IDS_IS10_ERR_LOADING_HEADER;
						return AgentResult.Error;
					}
				}

				// Read track rows
				trackLines = new TrackLine[totalNumberOfTrackRows + 64];	// Add extra 64 empty rows

				for (int i = 0; i < trackLines.Length; i++)
				{
					TrackLine trackLine = new TrackLine();

					byte byt1 = moduleStream.Read_UINT8();
					byte byt2 = moduleStream.Read_UINT8();
					byte byt3 = moduleStream.Read_UINT8();
					byte byt4 = moduleStream.Read_UINT8();

					trackLine.Note = byt1;
					trackLine.Instrument = byt2;
					trackLine.Arpeggio = (byte)((byt3 & 0xf0) >> 4);
					trackLine.Effect = (Effect)(byt3 & 0x0f);
					trackLine.EffectArg = byt4;

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_IS10_ERR_LOADING_TRACK;
						return AgentResult.Error;
					}

					trackLines[i] = trackLine;
				}

				// Read sample data
				for (int i = 0; i < numberOfSamples; i++)
				{
					samples[i].SampleAddr = moduleStream.ReadSampleData(i, (int)samples[i].Length, out int readBytes);

					if (readBytes != samples[i].Length)
					{
						errorMessage = Resources.IDS_IS10_ERR_LOADING_SAMPLE;
						return AgentResult.Error;
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
		#endregion

		#region Initialization and cleanup
		/********************************************************************/
		/// <summary>
		/// Initializes the player
		/// </summary>
		/********************************************************************/
		public override bool InitPlayer(out string errorMessage)
		{
			if (!base.InitPlayer(out errorMessage))
				return false;

			BuildVibratoTable();

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
			playingInfo.SpeedCounter++;

			if (playingInfo.SpeedCounter >= playingInfo.CurrentSpeed)
				GetNextRow();

			DoEffectsForAllVoices();

			if (endReached)
			{
				OnEndReached(playingInfo.SongPosition - 1);
				endReached = false;
			}
		}
		#endregion

		#region Information
		/********************************************************************/
		/// <summary>
		/// Return the title
		/// </summary>
		/********************************************************************/
		public override string Title => moduleName;



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
					frequencies[j] = PeriodToFrequency(Tables.Periods[1 + j]);

				foreach (Instrument instr in instruments)
				{
					SampleInfo sampleInfo = new SampleInfo
					{
						Flags = SampleInfo.SampleFlag.None,
						Volume = (ushort)(instr.Volume * 4),
						Panning = -1,
						LoopStart = 0,
						LoopLength = 0,
						NoteFrequencies = frequencies
					};

					if (instr.SynthesisEnabled)
					{
						sampleInfo.Type = SampleInfo.SampleType.Synthesis;
						sampleInfo.Name = string.Empty;
						sampleInfo.Length = 0;
					}
					else
					{
						if (instr.WaveformNumber < samples.Length)
						{
							Sample sample = samples[instr.WaveformNumber];

							sampleInfo.Type = SampleInfo.SampleType.Sample;
							sampleInfo.Name = sample.Name;
							sampleInfo.Sample = sample.SampleAddr;
							sampleInfo.Length = sample.Length;

							if (instr.RepeatLength != 2)
							{
								sampleInfo.Flags |= SampleInfo.SampleFlag.Loop;

								if (instr.RepeatLength == 0)
								{
									sampleInfo.LoopStart = 0;
									sampleInfo.LoopLength = sampleInfo.Length;
								}
								else
								{
									sampleInfo.LoopStart = instr.WaveformLength;
									sampleInfo.LoopLength = instr.RepeatLength;
								}
							}
						}
						else
						{
							sampleInfo.Type = SampleInfo.SampleType.Sample;
							sampleInfo.Name = string.Empty;
							sampleInfo.Sample = null;
							sampleInfo.Length = 0;
						}
					};

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
					description = Resources.IDS_IS10_INFODESCLINE0;
					value = positions.Length.ToString();
					break;
				}

				// Used track rows
				case 1:
				{
					description = Resources.IDS_IS10_INFODESCLINE1;
					value = trackLines.Length.ToString();
					break;
				}

				// Used instruments
				case 2:
				{
					description = Resources.IDS_IS10_INFODESCLINE2;
					value = instruments.Length.ToString();
					break;
				}

				// Used samples
				case 3:
				{
					description = Resources.IDS_IS10_INFODESCLINE3;
					value = samples.Length.ToString();
					break;
				}

				// Used wave tables
				case 4:
				{
					description = Resources.IDS_IS10_INFODESCLINE4;
					value = waveforms.Length.ToString();
					break;
				}

				// Playing position
				case 5:
				{
					description = Resources.IDS_IS10_INFODESCLINE5;
					value = FormatPosition();
					break;
				}

				// Playing tracks from line
				case 6:
				{
					description = Resources.IDS_IS10_INFODESCLINE6;
					value = FormatTracks();
					break;
				}

				// Current speed
				case 7:
				{
					description = Resources.IDS_IS10_INFODESCLINE7;
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

		#region Duration calculation
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
		/// Build the vibrato table (triangle)
		/// </summary>
		/********************************************************************/
		private void BuildVibratoTable()
		{
			vibratoTable = new sbyte[256];
			sbyte vibVal = 0;
			int offset = 0;

			for (int i = 0; i < 64; i++)
			{
				vibratoTable[offset++] = vibVal;
				vibVal += 2;
			}

			vibVal++;

			for (int i = 0; i < 128; i++)
			{
				vibVal -= 2;
				vibratoTable[offset++] = vibVal;
			}

			vibVal--;

			for (int i = 0; i < 64; i++)
			{
				vibratoTable[offset++] = vibVal;
				vibVal += 2;
			}
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
				SpeedCounter = currentSongInfo.StartSpeed,
				CurrentSpeed = currentSongInfo.StartSpeed,

				SongPosition = currentSongInfo.FirstPosition,
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
					Arpeggio = 0,
					Effect = Effect.None,
					EffectArg = 0,

					UseBuffer = 0,

					TransposedNote = 0,
					PreviousTransposedNote = 0,

					TransposedInstrument = 0,

					CurrentVolume = 0,

					SlideSpeed = 0,
					SlideIncrement = 0,

					PortamentoEnabled = false,
					PortamentoSpeed = 0,
					PortamentoSpeedCounter = 0,

					VibratoDelay = 0,
					VibratoPosition = 0,

					AdsrEnabled = false,
					AdsrPosition = 0,

					EnvelopeGeneratorCounterMode = EnvelopeGeneratorCounterMode.Off,
					EnvelopeGeneratorCounterPosition = 0,
				};
			}

			endReached = false;
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player has allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			samples = null;
			instruments = null;
			envelopeGeneratorTables = null;
			adsrTables = null;
			arpeggioTables = null;
			subSongs = null;
			waveforms = null;
			positions = null;
			trackLines = null;

			vibratoTable = null;

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
			playingInfo.SpeedCounter = 0;

			if (playingInfo.RowPosition >= playingInfo.RowsPerTrack)
			{
				playingInfo.RowPosition = 0;

				if (playingInfo.SongPosition > currentSongInfo.LastPosition)
					playingInfo.SongPosition = currentSongInfo.RestartPosition;

				if (HasPositionBeenVisited(playingInfo.SongPosition))
					endReached = true;

				MarkPositionAsVisited(playingInfo.SongPosition);
				playingInfo.SongPosition++;

				SinglePositionInfo[] positionRow = positions[playingInfo.SongPosition - 1];

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

			for (int i = 0; i < 4; i++)
			{
				VoiceInfo voiceInfo = voices[i];

				int position = voiceInfo.StartTrackRow + playingInfo.RowPosition;
				TrackLine trackLine = position < trackLines.Length ? trackLines[position] : new TrackLine();

				voiceInfo.Note = trackLine.Note;
				voiceInfo.Instrument = trackLine.Instrument;
				voiceInfo.Arpeggio = trackLine.Arpeggio;
				voiceInfo.Effect = trackLine.Effect;
				voiceInfo.EffectArg = trackLine.EffectArg;
			}

			for (int i = 0; i < 4; i++)
				PlayRow(voices[i], VirtualChannels[i]);

			playingInfo.RowPosition++;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize channel for a single row
		/// </summary>
		/********************************************************************/
		private void PlayRow(VoiceInfo voiceInfo, IChannel channel)
		{
			voiceInfo.CurrentVolume = 0;
			voiceInfo.SlideSpeed = 0;

			if (voiceInfo.Effect != Effect.None)
			{
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
						voiceInfo.AdsrEnabled = true;
						break;
					}

					case Effect.RestartEgc:
					{
						voiceInfo.EnvelopeGeneratorCounterPosition = voiceInfo.EffectArg;
						voiceInfo.EnvelopeGeneratorCounterMode = EnvelopeGeneratorCounterMode.Ones;
						break;
					}

					case Effect.SetSlideIncrement:
					{
						voiceInfo.SlideIncrement = (sbyte)voiceInfo.EffectArg;
						break;
					}

					case Effect.SetVibratoDelay:
					{
						voiceInfo.VibratoDelay = voiceInfo.EffectArg;
						voiceInfo.VibratoPosition = 0;
						break;
					}

					case Effect.SetVibratoPosition:
					{
						voiceInfo.VibratoPosition = voiceInfo.EffectArg;
						break;
					}

					case Effect.SetVolume:
					{
						voiceInfo.CurrentVolume = (byte)(voiceInfo.EffectArg & 0x3f);
						break;
					}

					case Effect.SetTrackLen:
					{
						if (voiceInfo.EffectArg <= 64)
							playingInfo.RowsPerTrack = voiceInfo.EffectArg;

						break;
					}

					// These effect does nothing in the replayer source I have
					case Effect.EffC:
					case Effect.EffD:
					{
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

			byte note = voiceInfo.Note;
			if (note != 0)
			{
				if (note == 0x7f)
				{
					channel.Mute();
					voiceInfo.CurrentVolume = 0;
					return;
				}

				if (voiceInfo.Effect != Effect.SkipNt)
					note = (byte)(note + voiceInfo.NoteTranspose);

				voiceInfo.PreviousTransposedNote = voiceInfo.TransposedNote;
				voiceInfo.TransposedNote = note;

				channel.SetAmigaPeriod(Tables.Periods[note]);

				byte instrNum = voiceInfo.Instrument;
				if (instrNum != 0)
				{
					if (voiceInfo.Effect != Effect.SkipSt)
						instrNum = (byte)(instrNum + voiceInfo.SoundTranspose);

					voiceInfo.TransposedInstrument = instrNum;

					Instrument instr = instruments[instrNum - 1];

					voiceInfo.AdsrEnabled = false;
					voiceInfo.AdsrPosition = 0;

					voiceInfo.VibratoDelay = 0;
					voiceInfo.VibratoPosition = 0;

					voiceInfo.EnvelopeGeneratorCounterMode = EnvelopeGeneratorCounterMode.Off;
					voiceInfo.EnvelopeGeneratorCounterPosition = 0;

					voiceInfo.SlideIncrement = 0;

					voiceInfo.PortamentoEnabled = instr.PortamentoEnabled;

					voiceInfo.PortamentoSpeed = instr.PortamentoSpeed;
					voiceInfo.PortamentoSpeedCounter = instr.PortamentoSpeed;

					if (voiceInfo.Effect == Effect.SkipPortamento)
					{
						voiceInfo.PortamentoEnabled = false;
						voiceInfo.PortamentoSpeed = 0;
						voiceInfo.PortamentoSpeedCounter = 0;
					}

					voiceInfo.VibratoDelay = instr.VibratoDelay;

					if (instr.AdsrEnabled)
						voiceInfo.AdsrEnabled = true;

					if (instr.SynthesisEnabled)
					{
						sbyte[] waveform = waveforms[instr.WaveformNumber];

						voiceInfo.UseBuffer = 1;

						voiceInfo.EnvelopeGeneratorCounterMode = instr.EnvelopeGeneratorCounterMode;

						if (instr.EnvelopeGeneratorCounterMode == EnvelopeGeneratorCounterMode.Off)
						{
							ushort length = Math.Min(instr.WaveformLength, (ushort)256);

							channel.PlaySample(instrNum, voiceInfo.SynthSample1, 0, length);
							channel.SetLoop(0, length);

							Array.Copy(waveform, voiceInfo.SynthSample1, length);

							if (instr.EnvelopeGeneratorCounterOffset != 0)
							{
								for (int i = 0; i < instr.EnvelopeGeneratorCounterOffset; i++)
									voiceInfo.SynthSample1[i] = (sbyte)-voiceInfo.SynthSample1[i];
							}
						}

						voiceInfo.CurrentVolume = instr.Volume;
						channel.SetAmigaVolume(voiceInfo.CurrentVolume);
					}
					else
					{
						byte sampleNum = (byte)(instr.WaveformNumber & 0x3f);
						if (sampleNum >= samples.Length)
						{
							channel.Mute();
							return;
						}

						Sample sample = samples[sampleNum];

						uint playLength = instr.WaveformLength;
						uint loopStart = 0;
						uint loopLength = 0;

						if (instr.RepeatLength == 0)
							loopLength = instr.WaveformLength;
						else if (instr.RepeatLength != 2)
						{
							playLength += instr.RepeatLength;

							loopStart = instr.WaveformLength;
							loopLength = instr.RepeatLength;
						}

						channel.PlaySample(sampleNum, sample.SampleAddr, 0, Math.Min(playLength, sample.Length));

						if (loopLength != 0)
							channel.SetLoop(loopStart, loopLength);

						channel.SetAmigaVolume(instr.Volume);

						if (sampleNum == 7)
							voiceInfo.CurrentVolume = (byte)(voiceInfo.EffectArg & 0x3f);
						else
							voiceInfo.CurrentVolume = instr.Volume;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Run effects and generate new synth sounds
		/// </summary>
		/********************************************************************/
		private void DoEffectsForAllVoices()
		{
			for (int i = 0; i < 4; i++)
				DoEffects(voices[i], VirtualChannels[i]);
		}



		/********************************************************************/
		/// <summary>
		/// Run effects for a single channel
		/// </summary>
		/********************************************************************/
		private void DoEffects(VoiceInfo voiceInfo, IChannel channel)
		{
			PeriodInfo periodInfo = DoArpeggio(voiceInfo);
			DoPortamento(periodInfo, voiceInfo);

			if (DoVibrato(periodInfo, voiceInfo))
			{
				periodInfo.Period = (ushort)(periodInfo.Period + voiceInfo.SlideIncrement);

				channel.SetAmigaPeriod(periodInfo.Period);

				voiceInfo.SlideIncrement = (short)(voiceInfo.SlideIncrement - voiceInfo.SlideSpeed);

				DoAdsr(voiceInfo, channel);
			}

			DoEnvelopeGeneratorCounter(voiceInfo, channel);
		}



		/********************************************************************/
		/// <summary>
		/// Handle arpeggio
		/// </summary>
		/********************************************************************/
		private PeriodInfo DoArpeggio(VoiceInfo voiceInfo)
		{
			byte note = voiceInfo.TransposedNote;
			byte prevNote = voiceInfo.PreviousTransposedNote;

			byte arpVal = arpeggioTables[voiceInfo.Arpeggio * 16 + playingInfo.SpeedCounter];
			note += arpVal;
			prevNote += arpVal;

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
			if (voiceInfo.PortamentoEnabled && (voiceInfo.PortamentoSpeedCounter != 0) && (periodInfo.Period != periodInfo.PreviousPeriod))
			{
				voiceInfo.PortamentoSpeedCounter--;

				(periodInfo.Period, periodInfo.PreviousPeriod) = (periodInfo.PreviousPeriod, periodInfo.Period);

				int newPeriod = (periodInfo.Period - periodInfo.PreviousPeriod) * voiceInfo.PortamentoSpeedCounter;

				if (voiceInfo.PortamentoSpeed != 0)
					newPeriod /= voiceInfo.PortamentoSpeed;

				newPeriod += periodInfo.PreviousPeriod;
				periodInfo.Period = (ushort)newPeriod;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle vibrato
		/// </summary>
		/********************************************************************/
		private bool DoVibrato(PeriodInfo periodInfo, VoiceInfo voiceInfo)
		{
			if (voiceInfo.VibratoDelay == 0)
			{
				if (voiceInfo.TransposedInstrument == 0)
					return false;

				Instrument instr = instruments[voiceInfo.TransposedInstrument - 1];

				sbyte vibVal = vibratoTable[voiceInfo.VibratoPosition];
				byte vibLevel = instr.VibratoLevel;

				if (vibVal < 0)
				{
					if (vibLevel != 0)
						periodInfo.Period -= (ushort)((-vibVal * 4) / vibLevel);
				}
				else
				{
					if (vibLevel != 0)
						periodInfo.Period += (ushort)((vibVal * 4) / vibLevel);
					else
						periodInfo.Period = 0;
				}

				voiceInfo.VibratoPosition += instr.VibratoSpeed;
			}
			else
				voiceInfo.VibratoDelay--;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Handle ADSR
		/// </summary>
		/********************************************************************/
		private void DoAdsr(VoiceInfo voiceInfo, IChannel channel)
		{
			Instrument instr = instruments[voiceInfo.TransposedInstrument - 1];

			if (voiceInfo.AdsrEnabled)
			{
				if (voiceInfo.AdsrPosition >= instr.AdsrTableLength)
					voiceInfo.AdsrEnabled = false;

				ushort adsrVal = adsrTables[instr.AdsrTableNumber * AdsrTableLength + voiceInfo.AdsrPosition];
				adsrVal++;

				ushort volume = instr.Volume;
				volume = (ushort)((volume * adsrVal) / 128);
				if (volume > 64)
					volume = 64;

				channel.SetAmigaVolume(volume);

				voiceInfo.AdsrPosition++;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle envelope generator counter
		/// </summary>
		/********************************************************************/
		private void DoEnvelopeGeneratorCounter(VoiceInfo voiceInfo, IChannel channel)
		{
			if (voiceInfo.EnvelopeGeneratorCounterMode != EnvelopeGeneratorCounterMode.Off)
			{
				Instrument instr = instruments[voiceInfo.TransposedInstrument - 1];

				sbyte[] waveform = waveforms[instr.WaveformNumber];
				sbyte[] synthBuf;

				voiceInfo.UseBuffer ^= 1;

				if (voiceInfo.UseBuffer == 0)
					synthBuf = voiceInfo.SynthSample2;
				else
					synthBuf = voiceInfo.SynthSample1;

				channel.SetSample(synthBuf, 0, instr.WaveformLength);
				channel.SetLoop(synthBuf, 0, instr.WaveformLength);
				channel.SetSampleNumber((short)(voiceInfo.TransposedInstrument - 1));

				Array.Copy(waveform, synthBuf, instr.WaveformLength);

				byte egcVal = envelopeGeneratorTables[instr.EnvelopeGeneratorCounterTableNumber * EnvelopeGeneratorTableLength + voiceInfo.EnvelopeGeneratorCounterPosition];
				egcVal += instr.EnvelopeGeneratorCounterOffset;

				if (egcVal != 0)
				{
					for (int i = 0; i < egcVal; i++)
						synthBuf[i] = (sbyte)-synthBuf[i];
				}

				voiceInfo.EnvelopeGeneratorCounterPosition++;

				if (voiceInfo.EnvelopeGeneratorCounterPosition >= instr.EnvelopeGeneratorCounterTableLength)
				{
					if (voiceInfo.EnvelopeGeneratorCounterMode == EnvelopeGeneratorCounterMode.Ones)
						voiceInfo.EnvelopeGeneratorCounterMode = EnvelopeGeneratorCounterMode.Off;
					else
						voiceInfo.EnvelopeGeneratorCounterPosition = 0;
				}
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
			if (playingInfo.SongPosition == 0)
				return "0";

			return (playingInfo.SongPosition - 1).ToString();
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
