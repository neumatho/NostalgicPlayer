/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.InStereo20.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.InStereo20
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class InStereo20Worker : ModulePlayerWithPositionDurationAgentBase
	{
		private class PeriodInfo
		{
			public ushort Period;
			public ushort PreviousPeriod;
		}

		private SongInfo[] subSongs;
		private SinglePositionInfo[][] positions;
		private TrackLine[] trackLines;
		private Sample[] samples;
		private sbyte[][] sampleData;
		private Instrument[] instruments;

		private SongInfo currentSongInfo;

		private GlobalPlayingInfo playingInfo;
		private VoiceInfo[] voices;

		private bool endReached;

		private const int InfoPositionLine = 4;
		private const int InfoTrackLine = 5;
		private const int InfoSpeedLine = 6;

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => new [] { "is", "is20" };



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

			if (Encoding.ASCII.GetString(buf, 0, 8) == "IS20DF10")
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
					description = Resources.IDS_IS20_INFODESCLINE0;
					value = positions.Length.ToString();
					break;
				}

				// Used track rows
				case 1:
				{
					description = Resources.IDS_IS20_INFODESCLINE1;
					value = trackLines.Length.ToString();
					break;
				}

				// Used instruments
				case 2:
				{
					description = Resources.IDS_IS20_INFODESCLINE2;
					value = instruments.Length.ToString();
					break;
				}

				// Used samples
				case 3:
				{
					description = Resources.IDS_IS20_INFODESCLINE3;
					value = samples.Length.ToString();
					break;
				}

				// Playing position
				case 4:
				{
					description = Resources.IDS_IS20_INFODESCLINE4;
					value = FormatPosition();
					break;
				}

				// Playing tracks from line
				case 5:
				{
					description = Resources.IDS_IS20_INFODESCLINE5;
					value = FormatTracks();
					break;
				}

				// Current speed
				case 6:
				{
					description = Resources.IDS_IS20_INFODESCLINE6;
					value = FormatSpeed();
					break;
				}

				// Current tempo (Hz)
				case 7:
				{
					description = Resources.IDS_IS20_INFODESCLINE7;
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
				Encoding encoder = EncoderCollection.Amiga;

				// Skip mark
				moduleStream.Seek(8, SeekOrigin.Begin);

				if (!ReadSubSongs(moduleStream))
				{
					errorMessage = Resources.IDS_IS20_ERR_LOADING_HEADER;
					return AgentResult.Error;
				}

				if (!ReadPositionInformation(moduleStream))
				{
					errorMessage = Resources.IDS_IS20_ERR_LOADING_HEADER;
					return AgentResult.Error;
				}

				if (!ReadTrackRows(moduleStream))
				{
					errorMessage = Resources.IDS_IS20_ERR_LOADING_TRACK;
					return AgentResult.Error;
				}

				if (!ReadSampleInformation(moduleStream, encoder))
				{
					errorMessage = Resources.IDS_IS20_ERR_LOADING_SAMPLEINFO;
					return AgentResult.Error;
				}

				if (!ReadSampleData(moduleStream))
				{
					errorMessage = Resources.IDS_IS20_ERR_LOADING_SAMPLE;
					return AgentResult.Error;
				}

				if (!ReadSynthInstrumentInformation(moduleStream, encoder))
				{
					errorMessage = Resources.IDS_IS20_ERR_LOADING_INSTRUMENT;
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

				foreach (Sample sample in samples)
				{
					SampleInfo sampleInfo = new SampleInfo
					{
						Type = SampleInfo.SampleType.Sample,
						Name = sample.Name,
						Sample = sampleData[sample.SampleNumber],
						Length = sample.OneShotLength * 2U,
						Flags = SampleInfo.SampleFlag.None,
						Volume = (ushort)(sample.Volume * 4),
						Panning = -1,
						NoteFrequencies = frequencies
					};

					if ((sample.RepeatLength != 1) && (sample.OneShotLength != 0))
					{
						sampleInfo.Flags |= SampleInfo.SampleFlag.Loop;

						if (sample.RepeatLength == 0)
						{
							sampleInfo.LoopStart = 0;
							sampleInfo.LoopLength = sampleInfo.Length;
						}
						else
						{
							sampleInfo.LoopStart = sample.OneShotLength * 2U;
							sampleInfo.LoopLength = sample.RepeatLength * 2U;
							sampleInfo.Length += sampleInfo.LoopLength;
						}
					}
					else
					{
						sampleInfo.LoopStart = 0;
						sampleInfo.LoopLength = 0;
					}

					yield return sampleInfo;
				}

				foreach (Instrument instr in instruments)
				{
					SampleInfo sampleInfo = new SampleInfo
					{
						Type = SampleInfo.SampleType.Synthesis,
						Name = instr.Name,
						Flags = SampleInfo.SampleFlag.None,
						Volume = instr.Volume,
						Panning = -1,
						Length = 0,
						LoopStart = 0,
						LoopLength = 0,
						NoteFrequencies = frequencies
					};

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
		/// Read sub-song information
		/// </summary>
		/********************************************************************/
		private bool ReadSubSongs(ModuleStream moduleStream)
		{
			uint mark = moduleStream.Read_B_UINT32();
			if (mark != 0x5354424c)		// STBL
				return false;

			uint numberOfSubSongs = moduleStream.Read_B_UINT32();
			subSongs = new SongInfo[numberOfSubSongs];

			for (int i = 0; i < numberOfSubSongs; i++)
			{
				SongInfo songInfo = new SongInfo();

				songInfo.StartSpeed = moduleStream.Read_UINT8();
				songInfo.RowsPerTrack = moduleStream.Read_UINT8();
				songInfo.FirstPosition = moduleStream.Read_B_UINT16();
				songInfo.LastPosition = moduleStream.Read_B_UINT16();
				songInfo.RestartPosition = moduleStream.Read_B_UINT16();
				songInfo.Tempo = moduleStream.Read_B_UINT16();

				if (moduleStream.EndOfStream)
					return false;

				subSongs[i] = songInfo;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read position information
		/// </summary>
		/********************************************************************/
		private bool ReadPositionInformation(ModuleStream moduleStream)
		{
			uint mark = moduleStream.Read_B_UINT32();
			if (mark != 0x4f565442)		// OVTB
				return false;

			uint totalNumberOfPositions = moduleStream.Read_B_UINT32();
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
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read all the track rows
		/// </summary>
		/********************************************************************/
		private bool ReadTrackRows(ModuleStream moduleStream)
		{
			uint mark = moduleStream.Read_B_UINT32();
			if (mark != 0x4e54424c)		// NTBL
				return false;

			uint totalNumberOfTrackRows = moduleStream.Read_B_UINT32();
			trackLines = new TrackLine[totalNumberOfTrackRows];

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
		/// Read sample information
		/// </summary>
		/********************************************************************/
		private bool ReadSampleInformation(ModuleStream moduleStream, Encoding encoder)
		{
			uint mark = moduleStream.Read_B_UINT32();
			if (mark != 0x53414d50)		// SAMP
				return false;

			uint numberOfSamples = moduleStream.Read_B_UINT32();
			samples = new Sample[numberOfSamples];

			for (int i = 0; i < numberOfSamples; i++)
			{
				Sample sample = new Sample();

				sample.OneShotLength = moduleStream.Read_B_UINT16();
				sample.RepeatLength = moduleStream.Read_B_UINT16();
				sample.SampleNumber = moduleStream.Read_INT8();
				sample.Volume = moduleStream.Read_UINT8();
				sample.VibratoDelay = moduleStream.Read_UINT8();
				sample.VibratoSpeed = moduleStream.Read_UINT8();
				sample.VibratoLevel = moduleStream.Read_UINT8();
				sample.PortamentoSpeed = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
					return false;

				moduleStream.Seek(6, SeekOrigin.Current);

				samples[i] = sample;
			}

			for (int i = 0; i < numberOfSamples; i++)
			{
				samples[i].Name = moduleStream.ReadString(encoder, 20);

				if (moduleStream.EndOfStream)
					return false;
			}

			// Skip copy of sample lengths and loop lengths stored in words
			moduleStream.Seek(numberOfSamples * 4 * 2, SeekOrigin.Current);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read sample data
		/// </summary>
		/********************************************************************/
		private bool ReadSampleData(ModuleStream moduleStream)
		{
			int numberOfSamples = samples.Length;

			if (numberOfSamples > 0)
			{
				uint[] sampleLengths = new uint[numberOfSamples];
				moduleStream.ReadArray_B_UINT32s(sampleLengths, 0, numberOfSamples);

				if (moduleStream.EndOfStream)
					return false;

				sampleData = new sbyte[numberOfSamples][];

				// Sample data are stored in reverse order
				for (int i = numberOfSamples - 1; i >= 0; i--)
				{
					uint sampleLen = sampleLengths[i];

					sampleData[i] = moduleStream.ReadSampleData(i, (int)sampleLen, out int readBytes);
					if (readBytes < sampleLen)
						return false;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read synth instrument information
		/// </summary>
		/********************************************************************/
		private bool ReadSynthInstrumentInformation(ModuleStream moduleStream, Encoding encoder)
		{
			uint mark = moduleStream.Read_B_UINT32();
			if (mark != 0x53594e54)		// SYNT
				return false;

			uint numberOfInstruments = moduleStream.Read_B_UINT32();
			instruments = new Instrument[numberOfInstruments];

			for (int i = 0; i < numberOfInstruments; i++)
			{
				Instrument instr = new Instrument();

				mark = moduleStream.Read_B_UINT32();
				if (mark != 0x49533230)		// IS20
					return false;

				instr.Name = moduleStream.ReadString(encoder, 20);
				instr.WaveformLength = moduleStream.Read_B_UINT16();
				instr.Volume = moduleStream.Read_UINT8();
				instr.VibratoDelay = moduleStream.Read_UINT8();
				instr.VibratoSpeed = moduleStream.Read_UINT8();
				instr.VibratoLevel = moduleStream.Read_UINT8();
				instr.PortamentoSpeed = moduleStream.Read_UINT8();
				instr.AdsrLength = moduleStream.Read_UINT8();
				instr.AdsrRepeat = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
					return false;

				moduleStream.Seek(4, SeekOrigin.Current);

				instr.SustainPoint = moduleStream.Read_UINT8();
				instr.SustainSpeed = moduleStream.Read_UINT8();
				instr.AmfLength = moduleStream.Read_UINT8();
				instr.AmfRepeat = moduleStream.Read_UINT8();

				byte egMode = moduleStream.Read_UINT8();
				byte egEnabled = moduleStream.Read_UINT8();

				instr.EnvelopeGeneratorMode = egEnabled == 0 ? EnvelopeGeneratorMode.Disabled : egMode == 0 ? EnvelopeGeneratorMode.Calc : EnvelopeGeneratorMode.Free;

				instr.StartLen = moduleStream.Read_UINT8();
				instr.StopRep = moduleStream.Read_UINT8();
				instr.SpeedUp = moduleStream.Read_UINT8();
				instr.SpeedDown = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
					return false;

				moduleStream.Seek(19, SeekOrigin.Current);

				int bytesRead = moduleStream.Read(instr.AdsrTable, 0, 128);
				if (bytesRead < 128)
					return false;

				bytesRead = moduleStream.ReadSigned(instr.LfoTable, 0, 128);
				if (bytesRead < 128)
					return false;

				for (int j = 0; j < 3; j++)
				{
					instr.Arpeggios[j].Length = moduleStream.Read_UINT8();
					instr.Arpeggios[j].Repeat = moduleStream.Read_UINT8();

					bytesRead = moduleStream.Read(instr.Arpeggios[j].Values, 0, 14);
					if (bytesRead < 14)
						return false;
				}

				bytesRead = moduleStream.Read(instr.EnvelopeGeneratorTable, 0, 128);
				if (bytesRead < 128)
					return false;

				bytesRead = moduleStream.ReadSigned(instr.Waveform1, 0, 256);
				if (bytesRead < 256)
					return false;

				bytesRead = moduleStream.ReadSigned(instr.Waveform2, 0, 256);
				if (bytesRead < 256)
					return false;

				instruments[i] = instr;
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

					TransposedInstrument = 0,
					PlayingMode = VoicePlayingMode.Sample,

					CurrentVolume = 0,

					ArpeggioPosition = 0,
					ArpeggioEffectNibble = false,

					SlideSpeed = 0,
					SlideValue = 0,

					PortamentoSpeedCounter = 0,
					PortamentoSpeed = 0,

					VibratoDelay = 0,
					VibratoSpeed = 0,
					VibratoLevel = 0,
					VibratoPosition = 0,

					AdsrPosition = 0,
					SustainCounter = 0,

					EnvelopeGeneratorDuration = 0,
					EnvelopeGeneratorPosition = 0,

					LfoPosition = 0
				};
			}

			ushort tempo = currentSongInfo.Tempo;
			if (tempo == 0)
				tempo = 50;

			PlayingFrequency = tempo;

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
			samples = null;
			sampleData = null;
			instruments = null;

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
			CheckForNewInstruments();
			UpdateVoices();
		}



		/********************************************************************/
		/// <summary>
		/// Get and parse next position
		/// </summary>
		/********************************************************************/
		private void GetNextPosition()
		{
			playingInfo.SongPosition++;

			if (playingInfo.SongPosition > currentSongInfo.LastPosition)
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

				int position = voiceInfo.StartTrackRow + playingInfo.RowPosition;
				TrackLine trackLine = position < trackLines.Length ? trackLines[position] : new TrackLine();

				voiceInfo.Note = trackLine.Note;
				voiceInfo.Instrument = trackLine.Instrument;
				voiceInfo.DisableSoundTranspose = trackLine.DisableSoundTranspose;
				voiceInfo.DisableNoteTranspose = trackLine.DisableNoteTranspose;
				voiceInfo.Arpeggio = trackLine.Arpeggio;
				voiceInfo.Effect = trackLine.Effect;
				voiceInfo.EffectArg = trackLine.EffectArg;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Prepare voice if new instrument is played
		/// </summary>
		/********************************************************************/
		private void CheckForNewInstruments()
		{
			for (int i = 0; i < 4; i++)
			{
				VoiceInfo voiceInfo = voices[i];

				if ((voiceInfo.Note != 0) && (voiceInfo.Instrument != 0))
					voiceInfo.TransposedInstrument = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Play new samples if needed for all voices
		/// </summary>
		/********************************************************************/
		private void UpdateVoices()
		{
			for (int i = 0; i < 4; i++)
			{
				VoiceInfo voiceInfo = voices[i];
				IChannel channel = VirtualChannels[i];

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
			SetEffects(voiceInfo);

			byte note = voiceInfo.Note;
			byte instrNum = voiceInfo.Instrument;

			if (note == 0)
			{
				if (instrNum != 0)
					RestoreVoice(voiceInfo, instrNum);
			}
			else
			{
				if (note != 0x80)
				{
					if (note == 0x7f)
						ForceQuiet(voiceInfo, channel);
					else
					{
						SetTranspose(voiceInfo, ref note, ref instrNum);

						voiceInfo.PreviousTransposedNote = voiceInfo.TransposedNote;
						voiceInfo.TransposedNote = note;

						if (instrNum <= 127)
						{
							ResetVoice(voiceInfo);

							if (instrNum >= 64)
								SetSampleInstrument(voiceInfo, channel, instrNum);
							else
								SetSynthInstrument(voiceInfo, channel, instrNum);
						}
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse the effect and set up the voice
		/// </summary>
		/********************************************************************/
		private void SetEffects(VoiceInfo voiceInfo)
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
					break;
				}

				case Effect.SetPortamento:
				{
					voiceInfo.PortamentoSpeedCounter = voiceInfo.EffectArg;
					voiceInfo.PortamentoSpeed = voiceInfo.EffectArg;
					break;
				}

				case Effect.SetVolumeIncrement:
				{
					int newVol = voiceInfo.CurrentVolume + (sbyte)voiceInfo.EffectArg;
					if (newVol < 0)
						newVol = 0;
					else
					{
						if (voiceInfo.PlayingMode == VoicePlayingMode.Sample)
						{
							if (newVol > 64)
								newVol = 64;
						}
						else
						{
							if (newVol > 255)
								newVol = 255;
						}
					}

					voiceInfo.CurrentVolume = (byte)newVol;
					break;
				}

				case Effect.PositionJump:
				{
					playingInfo.SongPosition = voiceInfo.EffectArg;
					playingInfo.RowPosition = playingInfo.RowsPerTrack;
					break;
				}

				case Effect.TrackBreak:
				{
					playingInfo.RowPosition = playingInfo.RowsPerTrack;
					break;
				}

				case Effect.SetVolume:
				{
					byte newVol = voiceInfo.EffectArg;

					if ((voiceInfo.PlayingMode == VoicePlayingMode.Sample) && (newVol > 64))
						newVol = 64;

					voiceInfo.CurrentVolume = newVol;
					break;
				}

				case Effect.SetTrackLen:
				{
					if (voiceInfo.EffectArg <= 64)
						playingInfo.RowsPerTrack = voiceInfo.EffectArg;

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

				case Effect.SetVibrato:
				{
					voiceInfo.VibratoDelay = 0;
					voiceInfo.VibratoSpeed = (byte)(((voiceInfo.EffectArg >> 4) & 0x0f) * 2);
					voiceInfo.VibratoLevel = (byte)((-((voiceInfo.EffectArg & 0x0f) << 4)) + 160);
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Initialize voice with new instrument
		/// </summary>
		/********************************************************************/
		private void RestoreVoice(VoiceInfo voiceInfo, byte instrNum)
		{
			instrNum &= 0x3f;
			voiceInfo.TransposedInstrument = instrNum;

			if (voiceInfo.PlayingMode == VoicePlayingMode.Sample)
			{
				Sample sample = samples[instrNum - 1];

				if (voiceInfo.Effect != Effect.SetVibrato)
				{
					voiceInfo.VibratoDelay = sample.VibratoDelay;
					voiceInfo.VibratoSpeed = sample.VibratoSpeed;
					voiceInfo.VibratoLevel = sample.VibratoLevel;
				}

				if ((voiceInfo.Effect != Effect.SkipPortamento) && (voiceInfo.Effect != Effect.SetPortamento))
				{
					voiceInfo.PortamentoSpeedCounter = sample.PortamentoSpeed;
					voiceInfo.PortamentoSpeed = sample.PortamentoSpeed;
				}

				if ((voiceInfo.Effect != Effect.SetVolume) && (voiceInfo.Effect != Effect.SetVolumeIncrement))
					voiceInfo.CurrentVolume = sample.Volume;
			}
			else
			{
				Instrument instr = instruments[instrNum - 1];

				if (voiceInfo.Effect != Effect.SetVibrato)
				{
					voiceInfo.VibratoDelay = instr.VibratoDelay;
					voiceInfo.VibratoSpeed = instr.VibratoSpeed;
					voiceInfo.VibratoLevel = instr.VibratoLevel;
				}

				if ((voiceInfo.Effect != Effect.SkipPortamento) && (voiceInfo.Effect != Effect.SetPortamento))
				{
					voiceInfo.PortamentoSpeedCounter = instr.PortamentoSpeed;
					voiceInfo.PortamentoSpeed = instr.PortamentoSpeed;
				}

				if ((voiceInfo.Effect != Effect.SetVolume) && (voiceInfo.Effect != Effect.SetVolumeIncrement))
					voiceInfo.CurrentVolume = instr.Volume;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Clear voice information
		/// </summary>
		/********************************************************************/
		private void ResetVoice(VoiceInfo voiceInfo)
		{
			voiceInfo.TransposedInstrument = 0;

			voiceInfo.ArpeggioPosition = 0;

			voiceInfo.SlideSpeed = 0;
			voiceInfo.SlideValue = 0;

			voiceInfo.VibratoPosition = 0;

			voiceInfo.AdsrPosition = 0;
			voiceInfo.SustainCounter = 0;

			voiceInfo.EnvelopeGeneratorDuration = 0;
			voiceInfo.EnvelopeGeneratorPosition = 0;

			voiceInfo.LfoPosition = 0;

			if (voiceInfo.Effect != Effect.SetPortamento)
			{
				voiceInfo.PortamentoSpeedCounter = 0;
				voiceInfo.PortamentoSpeed = 0;
			}

			if (voiceInfo.Effect != Effect.SetVibrato)
			{
				voiceInfo.VibratoDelay = 0;
				voiceInfo.VibratoSpeed = 0;
				voiceInfo.VibratoLevel = 0;
			}
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
			voiceInfo.PlayingMode = VoicePlayingMode.Sample;
		}



		/********************************************************************/
		/// <summary>
		/// Add transpose if needed
		/// </summary>
		/********************************************************************/
		private void SetTranspose(VoiceInfo voiceInfo, ref byte note, ref byte instrNum)
		{
			if (!voiceInfo.DisableNoteTranspose || voiceInfo.DisableSoundTranspose)
				note = (byte)(note + voiceInfo.NoteTranspose);

			if (!voiceInfo.DisableSoundTranspose || voiceInfo.DisableNoteTranspose)
				instrNum = (byte)(instrNum + voiceInfo.SoundTranspose);
		}



		/********************************************************************/
		/// <summary>
		/// Set up the voice to play a sample instrument
		/// </summary>
		/********************************************************************/
		private void SetSampleInstrument(VoiceInfo voiceInfo, IChannel channel, byte instrNum)
		{
			voiceInfo.PlayingMode = VoicePlayingMode.Sample;

			instrNum &= 0x3f;
			voiceInfo.TransposedInstrument = instrNum;

			Sample sample = samples[instrNum - 1];

			if (voiceInfo.Effect != Effect.SetVibrato)
			{
				voiceInfo.VibratoDelay = sample.VibratoDelay;
				voiceInfo.VibratoSpeed = sample.VibratoSpeed;
				voiceInfo.VibratoLevel = sample.VibratoLevel;
			}

			if ((voiceInfo.Effect != Effect.SkipPortamento) && (voiceInfo.Effect != Effect.SetPortamento))
			{
				voiceInfo.PortamentoSpeedCounter = sample.PortamentoSpeed;
				voiceInfo.PortamentoSpeed = sample.PortamentoSpeed;
			}

			if ((sample.SampleNumber < 0) || (sampleData[sample.SampleNumber] == null))
			{
				ForceQuiet(voiceInfo, channel);
				return;
			}

			sbyte[] data = sampleData[sample.SampleNumber];

			uint playLength = sample.OneShotLength;
			uint loopStart = 0;
			uint loopLength = 0;

			if (sample.RepeatLength == 0)
				loopLength = sample.OneShotLength;
			else if (sample.RepeatLength != 1)
			{
				playLength += sample.RepeatLength;

				loopStart = sample.OneShotLength;
				loopLength = sample.RepeatLength;
			}

			channel.PlaySample(instrNum, data, 0, playLength * 2U);

			if (loopLength != 0)
				channel.SetLoop(loopStart * 2U, loopLength * 2U);

			if ((voiceInfo.Effect != Effect.SetVolume) && (voiceInfo.Effect != Effect.SetVolumeIncrement))
			{
				voiceInfo.CurrentVolume = sample.Volume;
				channel.SetAmigaVolume(sample.Volume);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Set up the voice to play a synth instrument
		/// </summary>
		/********************************************************************/
		private void SetSynthInstrument(VoiceInfo voiceInfo, IChannel channel, byte instrNum)
		{
			voiceInfo.PlayingMode = VoicePlayingMode.Synth;

			instrNum &= 0x3f;
			voiceInfo.TransposedInstrument = instrNum;

			if (instrNum == 0)
			{
				ForceQuiet(voiceInfo, channel);
				return;
			}

			Instrument instr = instruments[instrNum - 1];

			if (voiceInfo.Effect != Effect.SetVibrato)
			{
				voiceInfo.VibratoDelay = instr.VibratoDelay;
				voiceInfo.VibratoSpeed = instr.VibratoSpeed;
				voiceInfo.VibratoLevel = instr.VibratoLevel;
			}

			if ((voiceInfo.Effect != Effect.SkipPortamento) && (voiceInfo.Effect != Effect.SetPortamento))
			{
				voiceInfo.PortamentoSpeedCounter = instr.PortamentoSpeed;
				voiceInfo.PortamentoSpeed = instr.PortamentoSpeed;
			}

			byte egVal;

			switch (instr.EnvelopeGeneratorMode)
			{
				case EnvelopeGeneratorMode.Disabled:
				default:
				{
					voiceInfo.EnvelopeGeneratorDuration = 0;
					egVal = 0;
					break;
				}

				case EnvelopeGeneratorMode.Free:
				{
					voiceInfo.EnvelopeGeneratorPosition = 0;

					if ((byte)(instr.StartLen + instr.StopRep) == 0)
						goto case EnvelopeGeneratorMode.Disabled;

					egVal = instr.EnvelopeGeneratorTable[0];
					break;
				}

				case EnvelopeGeneratorMode.Calc:
				{
					voiceInfo.EnvelopeGeneratorPosition = (ushort)(instr.StartLen << 8);
					voiceInfo.EnvelopeGeneratorDuration = 1;

					egVal = instr.StartLen;
					break;
				}
			}

			sbyte[] waveform = (egVal & 1) != 0 ? instr.Waveform2 : instr.Waveform1;
			uint length = instr.WaveformLength;
			uint startOffset = (uint)egVal & 0xfe;

			channel.PlaySample((short)(instrNum + samples.Length), waveform, startOffset, length);
			channel.SetLoop(startOffset, length);

			if ((voiceInfo.Effect != Effect.SetVolume) && (voiceInfo.Effect != Effect.SetVolumeIncrement))
				voiceInfo.CurrentVolume = instr.Volume;
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
			if (voiceInfo.TransposedInstrument == 0)
			{
				channel.Mute();
				return;
			}

			if (voiceInfo.PlayingMode == VoicePlayingMode.Sample)
			{
				PeriodInfo periodInfo = DoSampleArpeggio(voiceInfo);
				DoPortamento(periodInfo, voiceInfo);
				DoVibrato(periodInfo, voiceInfo);

				channel.SetAmigaPeriod(periodInfo.Period);
				channel.SetAmigaVolume(voiceInfo.CurrentVolume);
			}
			else
			{
				Instrument instr = instruments[voiceInfo.TransposedInstrument - 1];

				PeriodInfo periodInfo = DoSynthArpeggio(voiceInfo, instr);
				DoPortamento(periodInfo, voiceInfo);
				DoVibrato(periodInfo, voiceInfo);
				DoLfo(periodInfo, voiceInfo, instr);

				channel.SetAmigaPeriod(periodInfo.Period);

				DoAdsr(voiceInfo, channel, instr);
				DoEnvelopeGenerator(voiceInfo, channel, instr);
			}

			voiceInfo.SlideValue -= voiceInfo.SlideSpeed;
		}



		/********************************************************************/
		/// <summary>
		/// Handle arpeggio for samples
		/// </summary>
		/********************************************************************/
		private PeriodInfo DoSampleArpeggio(VoiceInfo voiceInfo)
		{
			byte note = voiceInfo.TransposedNote;
			byte prevNote = voiceInfo.PreviousTransposedNote;

			if (voiceInfo.Effect == Effect.Arpeggio)
			{
				byte arpVal = (byte)(voiceInfo.ArpeggioEffectNibble ? voiceInfo.EffectArg & 0x0f : voiceInfo.EffectArg >> 4);
				voiceInfo.ArpeggioEffectNibble = !voiceInfo.ArpeggioEffectNibble;

				note += arpVal;
				prevNote += arpVal;
			}

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
		/// Handle arpeggio for synths
		/// </summary>
		/********************************************************************/
		private PeriodInfo DoSynthArpeggio(VoiceInfo voiceInfo, Instrument instr)
		{
			byte note = voiceInfo.TransposedNote;
			byte prevNote = voiceInfo.PreviousTransposedNote;

			if (voiceInfo.Arpeggio != 0)
			{
				Arpeggio arp = instr.Arpeggios[voiceInfo.Arpeggio - 1];

				byte arpVal = arp.Values[voiceInfo.ArpeggioPosition];
				note += arpVal;
				prevNote += arpVal;

				if (voiceInfo.ArpeggioPosition == (arp.Length + arp.Repeat))
					voiceInfo.ArpeggioPosition = arp.Length;
				else
					voiceInfo.ArpeggioPosition++;
			}

			ushort period = note < Tables.Periods.Length ? Tables.Periods[note] : (ushort)0;
			ushort previousPeriod = prevNote < Tables.Periods.Length ? Tables.Periods[prevNote] : (ushort)0;

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
			if ((voiceInfo.PortamentoSpeed != 0) && (voiceInfo.PortamentoSpeedCounter != 0) && (periodInfo.Period != periodInfo.PreviousPeriod))
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
		private void DoVibrato(PeriodInfo periodInfo, VoiceInfo voiceInfo)
		{
			if (voiceInfo.VibratoDelay != 255)
			{
				if (voiceInfo.VibratoDelay == 0)
				{
					sbyte vibVal = Tables.Vibrato[voiceInfo.VibratoPosition];
					byte vibLevel = voiceInfo.VibratoLevel;

					if (vibVal < 0)
					{
						if (vibLevel != 0)
							periodInfo.Period -= (ushort)((-vibVal * 4) / vibLevel);
					}
					else
					{
						if (vibLevel != 0)
							periodInfo.Period += (ushort)((vibVal * 4) / vibLevel);
					}

					voiceInfo.VibratoPosition = (ushort)((voiceInfo.VibratoPosition + voiceInfo.VibratoSpeed) & 0xff);
				}
				else
					voiceInfo.VibratoDelay--;
			}

			periodInfo.Period = (ushort)(periodInfo.Period + voiceInfo.SlideValue);
		}



		/********************************************************************/
		/// <summary>
		/// Handle LFO
		/// </summary>
		/********************************************************************/
		private void DoLfo(PeriodInfo periodInfo, VoiceInfo voiceInfo, Instrument instr)
		{
			if ((instr.AmfLength + instr.AmfRepeat) != 0)
			{
				sbyte lfoVal = instr.LfoTable[voiceInfo.LfoPosition];
				periodInfo.Period = (ushort)(periodInfo.Period - lfoVal);

				if (voiceInfo.LfoPosition == (instr.AmfLength + instr.AmfRepeat))
					voiceInfo.LfoPosition = instr.AmfLength;
				else
					voiceInfo.LfoPosition++;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle ADSR
		/// </summary>
		/********************************************************************/
		private void DoAdsr(VoiceInfo voiceInfo, IChannel channel, Instrument instr)
		{
			if ((instr.AdsrLength + instr.AdsrRepeat) == 0)
				channel.SetVolume(voiceInfo.CurrentVolume);
			else
			{
				byte adsrVal = instr.AdsrTable[voiceInfo.AdsrPosition];
				ushort vol = (ushort)((voiceInfo.CurrentVolume * adsrVal) / 256);
				channel.SetVolume(vol);

				if (voiceInfo.AdsrPosition >= (instr.AdsrLength + instr.AdsrRepeat))
					voiceInfo.AdsrPosition = instr.AdsrLength;
				else
				{
					if ((voiceInfo.Note != 0x80) || (instr.SustainSpeed == 1) || (voiceInfo.AdsrPosition < instr.SustainPoint))
						voiceInfo.AdsrPosition++;
					else
					{
						if (instr.SustainSpeed != 0)
						{
							if (voiceInfo.SustainCounter == 0)
							{
								voiceInfo.SustainCounter = instr.SustainSpeed;
								voiceInfo.AdsrPosition++;
							}
							else
								voiceInfo.SustainCounter--;
						}
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle envelope generator
		/// </summary>
		/********************************************************************/
		private void DoEnvelopeGenerator(VoiceInfo voiceInfo, IChannel channel, Instrument instr)
		{
			if (instr.EnvelopeGeneratorMode != EnvelopeGeneratorMode.Disabled)
			{
				byte egVal;

				if (instr.EnvelopeGeneratorMode == EnvelopeGeneratorMode.Free)
				{
					int len = instr.StartLen + instr.StopRep;
					if (len == 0)
						return;

					if (voiceInfo.EnvelopeGeneratorPosition >= len)
						voiceInfo.EnvelopeGeneratorPosition = instr.StartLen;
					else
						voiceInfo.EnvelopeGeneratorPosition++;

					egVal = instr.EnvelopeGeneratorTable[voiceInfo.EnvelopeGeneratorPosition];
				}
				else
				{
					if (voiceInfo.EnvelopeGeneratorDuration == 0)
						return;

					ushort position = voiceInfo.EnvelopeGeneratorPosition;

					if (voiceInfo.EnvelopeGeneratorDuration > 0)
					{
						position = (ushort)(position + instr.SpeedUp * 32);

						if ((position >> 8) >= instr.StopRep)
						{
							voiceInfo.EnvelopeGeneratorPosition = (ushort)(instr.StopRep << 8);
							voiceInfo.EnvelopeGeneratorDuration = -1;
						}
						else
							voiceInfo.EnvelopeGeneratorPosition = position;
					}
					else
					{
						position = (ushort)(position - instr.SpeedDown * 32);

						if ((position >> 8) >= instr.StartLen)
						{
							voiceInfo.EnvelopeGeneratorPosition = (ushort)(instr.StartLen << 8);
							voiceInfo.EnvelopeGeneratorDuration = 1;
						}
						else
							voiceInfo.EnvelopeGeneratorPosition = position;
					}

					egVal = (byte)(voiceInfo.EnvelopeGeneratorPosition >> 8);
				}

				sbyte[] waveform = (egVal & 1) != 0 ? instr.Waveform2 : instr.Waveform1;
				uint length = instr.WaveformLength;
				uint startOffset = (uint)egVal & 0xfe;

				channel.SetSample(waveform, startOffset, length);
				channel.SetLoop(startOffset, length);
				channel.SetSampleNumber((short)(voiceInfo.TransposedInstrument - 1 + samples.Length));
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
