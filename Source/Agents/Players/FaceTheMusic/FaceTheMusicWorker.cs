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
using Polycode.NostalgicPlayer.Agent.Player.FaceTheMusic.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.FaceTheMusic
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class FaceTheMusicWorker : ModulePlayerWithPositionDurationAgentBase
	{
		private ushort numberOfMeasures;
		private byte rowsPerMeasure;

		private ushort startCiaTimerValue;
		private byte startSpeed;

		private byte channelMuteStatus;
		private byte globalVolume;
		private byte flag;

		private byte numberOfSamples;

		private string songTitle;
		private string artist;

		private Sample[] samples;
		private SoundEffectScript[] soundEffectScripts;
		private Track[] tracks;

		private int[] channelMapping;

		private GlobalPlayingInfo playingInfo;
		private VoiceInfo[] voices;

		private bool endReached;

		private const int InfoPositionLine = 2;
		private const int InfoSpeedLine = 3;
		private const int InfoTempoLine = 4;

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => [ "ftm" ];



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			if (moduleStream.Length < 82)
				return AgentResult.Unknown;

			// Check the mark
			moduleStream.Seek(0, SeekOrigin.Begin);

			uint mark = moduleStream.Read_B_UINT32();
			if ((mark & 0xffffff00) != 0x46544d00)      // FTM
				return AgentResult.Unknown;

			if (moduleStream.Read_UINT8() != 3)
				return AgentResult.Unknown;

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Return the name of the module
		/// </summary>
		/********************************************************************/
		public override string ModuleName => songTitle;



		/********************************************************************/
		/// <summary>
		/// Return the name of the author
		/// </summary>
		/********************************************************************/
		public override string Author => artist;



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
					description = Resources.IDS_FTM_INFODESCLINE0;
					value = numberOfMeasures.ToString();
					break;
				}

				// Used samples
				case 1:
				{
					description = Resources.IDS_FTM_INFODESCLINE1;
					value = numberOfSamples.ToString();
					break;
				}

				// Playing position
				case 2:
				{
					description = Resources.IDS_FTM_INFODESCLINE2;
					value = FormatSongPosition();
					break;
				}

				// Current speed
				case 3:
				{
					description = Resources.IDS_FTM_INFODESCLINE3;
					value = playingInfo.Speed.ToString();
					break;
				}

				// Current tempo (Hz)
				case 4:
				{
					description = Resources.IDS_FTM_INFODESCLINE4;
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
				Encoding encoder = EncoderCollection.Amiga;

				ModuleStream moduleStream = fileInfo.ModuleStream;

				if (!LoadHeader(moduleStream, encoder, out byte numberOfSels))
				{
					errorMessage = Resources.IDS_FTM_ERR_LOADING_HEADER;
					return AgentResult.Error;
				}

				if (!LoadSampleNames(moduleStream, encoder))
				{
					errorMessage = Resources.IDS_FTM_ERR_LOADING_SAMPLEINFO;
					return AgentResult.Error;
				}

				if (!LoadSoundEffectScripts(moduleStream, numberOfSels))
				{
					errorMessage = Resources.IDS_FTM_ERR_LOADING_SELS;
					return AgentResult.Error;
				}

				if (!LoadTracks(moduleStream))
				{
					errorMessage = Resources.IDS_FTM_ERR_LOADING_TRACKS;
					return AgentResult.Error;
				}

				LoadSampleData(moduleStream, fileInfo, out errorMessage);
				if (!string.IsNullOrEmpty(errorMessage))
					return AgentResult.Error;
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
		/// Initializes the player
		/// </summary>
		/********************************************************************/
		public override bool InitPlayer(out string errorMessage)
		{
			if (!base.InitPlayer(out errorMessage))
				return false;

			// Build channel mapping table
			channelMapping = new int[8];

			for (int i = 0, chn = 0; i < 8; i++)
			{
				if ((channelMuteStatus & (1 << i)) != 0)
					channelMapping[i] = chn++;
				else
					channelMapping[i] = -1;
			}

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

			InitializeSound(0, (ushort)(numberOfMeasures * rowsPerMeasure));

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

				TakeNextRow();
			}

			RunEffectsForAllVoices();
			SetupHardware();

			// Have we reached the end of the module
			if (endReached)
			{
				int position = playingInfo.CurrentRow / rowsPerMeasure;
				OnEndReached(position);
				endReached = false;

				MarkPositionAsVisited(position);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the module use
		/// </summary>
		/********************************************************************/
		public override int ModuleChannelCount => channelMapping.Count(x => x != -1);



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

				for (int j = 0; j < 3 * 12 - 2; j++)
					frequencies[4 * 12 + j] = PeriodToFrequency(Tables.Periods[j * 8]);

				for (int i = 0; i < numberOfSamples; i++)
				{
					Sample sample = samples[i];

					SampleInfo sampleInfo = new SampleInfo
					{
						Name = sample.Name ?? string.Empty,
						Type = SampleInfo.SampleType.Sample,
						Flags = SampleInfo.SampleFlag.None,
						Volume = 256,
						Panning = -1,
						NoteFrequencies = frequencies
					};

					if (string.IsNullOrEmpty(sample.Name))
					{
						sampleInfo.Length = 0;
						sampleInfo.LoopStart = 0;
						sampleInfo.LoopStart = 0;
					}
					else
					{
						sampleInfo.Length = (uint)(sample.OneshotLength + sample.LoopLength) * 2U;
						sampleInfo.Sample = sample.SampleData;

						if (sample.LoopLength != 0)
						{
							sampleInfo.LoopStart = sample.OneshotLength * 2U;
							sampleInfo.LoopLength = sample.LoopLength * 2U;
							sampleInfo.Flags |= SampleInfo.SampleFlag.Loop;
						}
						else
						{
							sampleInfo.LoopStart = 0;
							sampleInfo.LoopStart = 0;
						}
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
		protected override int InitDuration(int songNumber, int startPosition)
		{
			InitializeSound((ushort)(startPosition * rowsPerMeasure), (ushort)(numberOfMeasures * rowsPerMeasure));
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
			return numberOfMeasures;
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
		/// Read header information
		/// </summary>
		/********************************************************************/
		private bool LoadHeader(ModuleStream moduleStream, Encoding encoder, out byte numberOfSels)
		{
			numberOfSels = 0;

			// Skip mark and version
			moduleStream.Seek(5, SeekOrigin.Begin);

			numberOfSamples = moduleStream.Read_UINT8();
			numberOfMeasures = moduleStream.Read_B_UINT16();
			startCiaTimerValue = moduleStream.Read_B_UINT16();

			// Skip tonality
			moduleStream.Seek(1, SeekOrigin.Current);

			channelMuteStatus = moduleStream.Read_UINT8();
			globalVolume = moduleStream.Read_UINT8();
			flag = moduleStream.Read_UINT8();

			startSpeed = moduleStream.Read_UINT8();
			rowsPerMeasure = moduleStream.Read_UINT8();

			if (moduleStream.EndOfStream)
				return false;

			if (rowsPerMeasure != (96 / startSpeed))
				return false;

			// Read song title and artist name
			songTitle = moduleStream.ReadString(encoder, 32);
			artist = moduleStream.ReadString(encoder, 32);

			numberOfSels = moduleStream.Read_UINT8();

			if (moduleStream.EndOfStream)
				return false;

			// Skip padding
			moduleStream.Seek(1, SeekOrigin.Current);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read sample names
		/// </summary>
		/********************************************************************/
		private bool LoadSampleNames(ModuleStream moduleStream, Encoding encoder)
		{
			samples = new Sample[64];

			for (int i = 0; i < numberOfSamples; i++)
			{
				string name = moduleStream.ReadString(encoder, 29).TrimEnd();

				if (moduleStream.EndOfStream)
					return false;

				if (string.IsNullOrEmpty(name))
					samples[i] = Tables.QuietSample;
				else
				{
					samples[i] = new Sample
					{
						Name = name
					};
				}

				// Skip octave number and some padding (not used in modules)
				moduleStream.Seek(3, SeekOrigin.Current);
			}

			for (int i = numberOfSamples; i < 64; i++)
				samples[i] = Tables.QuietSample;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read all the sound effect scripts
		/// </summary>
		/********************************************************************/
		private bool LoadSoundEffectScripts(ModuleStream moduleStream, byte numberOfSels)
		{
			// There can max be 64 sound effect scripts if a module. Each script have
			// an index into this array. They may not be in chronological order, that's
			// why we allocate space for all scripts
			soundEffectScripts = new SoundEffectScript[64];

			for (int i = 0; i < numberOfSels; i++)
			{
				ushort numberOfScriptLines = moduleStream.Read_B_UINT16();
				ushort scriptIndex = moduleStream.Read_B_UINT16();

				// Read the script
				SoundEffectLine[] scriptLines = new SoundEffectLine[numberOfScriptLines];

				for (int j = 0; j < numberOfScriptLines; j++)
				{
					SoundEffectLine line = new SoundEffectLine();

					line.Effect = (SoundEffect)moduleStream.Read_UINT8();
					line.Argument1 = moduleStream.Read_UINT8();
					line.Argument2 = moduleStream.Read_B_UINT16();

					if (moduleStream.EndOfStream)
						return false;

					scriptLines[j] = line;
				}

				soundEffectScripts[scriptIndex] = new SoundEffectScript
				{
					Script = scriptLines
				};
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read all the channel tracks
		/// </summary>
		/********************************************************************/
		private bool LoadTracks(ModuleStream moduleStream)
		{
			// Face The Music only have a single track for each channel
			tracks = new Track[8];

			for (int i = 0; i < 8; i++)
			{
				Track track = new Track();

				track.DefaultSpacing = moduleStream.Read_B_UINT16();

				uint trackLen = moduleStream.Read_B_UINT32();
				track.Lines = new TrackLine[trackLen / 2];

				for (int j = 0; j < track.Lines.Length; j++)
				{
					byte byt1 = moduleStream.Read_UINT8();
					byte byt2 = moduleStream.Read_UINT8();

					if (moduleStream.EndOfStream)
						return false;

					TrackEffect effect = (TrackEffect)((byt1 & 0xf0) >> 4);
					ushort arg;
					byte note;

					if (effect == TrackEffect.SkipEmptyRows)
					{
						arg = (ushort)(((byt1 & 0x0f) << 8) | byt2);
						note = 0;
					}
					else
					{
						arg = (ushort)(((byt1 & 0x0f) << 2) | ((byt2 & 0xc0) >> 6));
						note = (byte)(byt2 & 0x3f);
					}

					TrackLine trackLine = new TrackLine
					{
						Effect = effect,
						EffectArgument = arg,
						Note = note
					};

					track.Lines[j] = trackLine;
				}

				tracks[i] = track;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read all the sample data
		/// </summary>
		/********************************************************************/
		private void LoadSampleData(ModuleStream moduleStream, PlayerFileInfo fileInfo, out string errorMessage)
		{
			if ((flag & 0x01) == 0)
				LoadExternalSamples(fileInfo, out errorMessage);
			else
			{
				for (int i = 0; i < samples.Length; i++)
				{
					Sample sample = samples[i];

					if (!string.IsNullOrEmpty(sample.Name))
					{
						sample.OneshotLength = moduleStream.Read_B_UINT16();
						sample.LoopLength = moduleStream.Read_B_UINT16();

						if (moduleStream.EndOfStream)
						{
							errorMessage = Resources.IDS_FTM_ERR_LOADING_SAMPLES;
							return;
						}

						sample.LoopStart = sample.OneshotLength * 2U;
						sample.TotalLength = (uint)(sample.OneshotLength + sample.LoopLength) * 2U;

						sample.SampleData = moduleStream.ReadSampleData(i, (int)sample.TotalLength, out int readBytes);
						if (readBytes != sample.TotalLength)
						{
							errorMessage = Resources.IDS_FTM_ERR_LOADING_SAMPLES;
							return;
						}
					}
				}

				errorMessage = string.Empty;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Read all the sample data from external files
		/// </summary>
		/********************************************************************/
		private void LoadExternalSamples(PlayerFileInfo fileInfo, out string errorMessage)
		{
			errorMessage = string.Empty;

			for (int i = 0; i < samples.Length; i++)
			{
				Sample sample = samples[i];

				if (!string.IsNullOrEmpty(sample.Name))
				{
					using (ModuleStream instrumentStream = fileInfo.Loader?.TryOpenExternalFileInInstruments(sample.Name, out _))
					{
						if (instrumentStream == null)
						{
							errorMessage = string.Format(Resources.IDS_FTM_ERR_LOADING_OPEN_EXTERNAL_FILE, sample.Name);
							return;
						}

						if (!LoadExternalSampleData(instrumentStream, i, sample))
						{
							errorMessage = string.Format(Resources.IDS_FTM_ERR_LOADING_READ_EXTERNAL_FILE, sample.Name);
							return;
						}
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse and read external sample file
		/// </summary>
		/********************************************************************/
		private bool LoadExternalSampleData(ModuleStream instrumentStream, int sampleNumber, Sample sample)
		{
			instrumentStream.Seek(0, SeekOrigin.Begin);

			LoadResult result = IffSampleLoader.Load(instrumentStream, sampleNumber, out IffSample iffSample);

			if (result == LoadResult.UnknownFormat)
			{
				// Seems to be raw sample data, so load it as such
				int length = (int)instrumentStream.Length;
				sample.SampleData = instrumentStream.ReadSampleData(sampleNumber, length, out int readBytes);
				if (readBytes != length)
					return false;

				sample.OneshotLength = (ushort)(length / 2);
				sample.LoopStart = (ushort)length;
				sample.LoopLength = 0;
				sample.TotalLength = (uint)length;

				return true;
			}

			if (result != LoadResult.Ok)
				return false;

			if (iffSample.Octaves != 1)
				return false;

			sample.SampleData = iffSample.SampleData;
			sample.OneshotLength = (ushort)(iffSample.OneShotHiSamples / 2);
			sample.LoopStart = iffSample.OneShotHiSamples;
			sample.LoopLength = (ushort)(iffSample.RepeatHiSamples / 2);
			sample.TotalLength = iffSample.OneShotHiSamples + iffSample.RepeatHiSamples;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize sound structures
		/// </summary>
		/********************************************************************/
		private void InitializeSound(ushort startRow, ushort endRow)
		{
			playingInfo = new GlobalPlayingInfo
			{
				StartRow = startRow,
				EndRow = endRow,

				GlobalVolume = globalVolume,

				Speed = startSpeed,

				PatternLoopStopRow = 0,
				PatternLoopStartRow = 0,
				DoPatternLoop = false,

				SpeedCounter = 1,
				CurrentRow = startRow
			};

			Sample quietSample = Tables.QuietSample;

			voices = new VoiceInfo[8];

			for (int i = 0; i < 8; i++)
			{
				voices[i] = new VoiceInfo
				{
					ChannelNumber = i,

					CurrentSample = quietSample,
					SampleNumber = -1,
					SampleData = quietSample.SampleData,
					SampleStartOffset = 0,
					SampleCalculateOffset = 0,
					SampleOneshotLength = quietSample.OneshotLength,
					SampleLoopStart = quietSample.LoopStart,
					SampleLoopLength = quietSample.LoopLength,
					SampleTotalLength = quietSample.TotalLength,

					Volume = 64,
					NoteIndex = 0,
					RetrigSample = false,

					DetuneIndex = i / 2,

					Track = tracks[i],
					TrackPosition = 0,
					RowsLeftToSkip = 0,

					PortamentoTicks = 0,
					PortamentoNote = 0,
					PortamentoEndNote = 0,

					VolumeDownVolume = 0,
					VolumeDownSpeed = 0,

					SoundEffectState = new SoundEffectState
					{
						EffectScript = null,
						ScriptPosition = -1,

						WaitCounter = 0,
						LoopCounter = 0,

						VoiceInfo = null,

						NewPitchGotoLineNumber = 0,
						NewVolumeGotoLineNumber = 0,
						NewSampleGotoLineNumber = 0,
						ReleaseGotoLineNumber = 0,
						PortamentoGotoLineNumber = 0,
						VolumeDownGotoLineNumber = 0,

						InterruptLineNumber = 0
					}
				};

				for (int j = 0; j < 4; j++)
				{
					voices[i].LfoStates[j] = new LfoState
					{
						ModulationSpeed = 0
					};
				}

				AdjustTrackIndexes(voices[i]);
				InitializeVoiceWithLatestVolumeAndInstrument(voices[i]);
			}

			SetCiaTimerTempo(startCiaTimerValue);
			AmigaFilter = (flag & 0x02) != 0;

			endReached = false;
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player have allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			samples = null;
			soundEffectScripts = null;
			tracks = null;
			samples = null;

			channelMapping = null;

			playingInfo = null;
			voices = null;
		}



		/********************************************************************/
		/// <summary>
		/// Will find the previous volume and instrument from the current
		/// position and initialize the voice structure using them
		/// </summary>
		/********************************************************************/
		private void InitializeVoiceWithLatestVolumeAndInstrument(VoiceInfo voiceInfo)
		{
			Track track = voiceInfo.Track;

			uint oldTrackPosition = voiceInfo.TrackPosition;
			TrackLine oldTrackLine = track.Lines[oldTrackPosition];

			int trackPosition = (int)oldTrackPosition - 1;
			TrackLine trackLine = null;

			TrackEffect previousVolumeEffect = TrackEffect.None;
			ushort previousInstrument = 0;

			// Find the previous line with a volume effect
			while (trackPosition >= 0)
			{
				trackLine = track.Lines[trackPosition];

				if (trackLine.Effect < TrackEffect.SelEffect)
				{
					previousVolumeEffect = trackLine.Effect;

					if (previousVolumeEffect != TrackEffect.None)
						break;
				}

				trackPosition--;
			}

			// Find the previous line with an instrument
			trackPosition = (int)oldTrackPosition - 1;

			while (trackPosition >= 0)
			{
				trackLine = track.Lines[trackPosition];

				if (trackLine.Effect < TrackEffect.SelEffect)
				{
					previousInstrument = trackLine.EffectArgument;

					if (previousInstrument != 0)
						break;
				}

				trackPosition--;
			}

			track.Lines[oldTrackPosition] = new TrackLine
			{
				Effect = previousVolumeEffect,
				EffectArgument = previousInstrument,
				Note = 0
			};

			ParseTrackEffect(voiceInfo, voiceInfo.TrackPosition);

			track.Lines[oldTrackPosition] = oldTrackLine;
		}



		/********************************************************************/
		/// <summary>
		/// Will move position to the next row and parse it
		/// </summary>
		/********************************************************************/
		private void TakeNextRow()
		{
			if (playingInfo.DoPatternLoop)
			{
				playingInfo.DoPatternLoop = false;
				playingInfo.CurrentRow = playingInfo.PatternLoopStartRow;
			}
			else
			{
				for (int i = 0; i < 8; i++)
				{
					if (channelMapping[i] != -1)
						HandleRow(voices[i]);
				}

				playingInfo.CurrentRow++;

				if (playingInfo.CurrentRow == playingInfo.PatternLoopStopRow)
					playingInfo.CurrentRow = playingInfo.PatternLoopStartRow;
				else if (playingInfo.CurrentRow == playingInfo.EndRow)
				{
					playingInfo.CurrentRow = playingInfo.StartRow;
					endReached = true;
				}
				else if (((playingInfo.CurrentRow % rowsPerMeasure) == 0) && (playingInfo.PatternLoopStack.Count == 0) && HasPositionBeenVisited(playingInfo.CurrentRow / rowsPerMeasure))
					endReached = true;

				MarkPositionAsVisited(playingInfo.CurrentRow / rowsPerMeasure);
			}

			if ((playingInfo.CurrentRow % rowsPerMeasure) == 0)
				ShowSongPosition();

			AdjustAllTrackIndexes();
		}



		/********************************************************************/
		/// <summary>
		/// Will adjust the indexes of all tracks to match the current row
		/// </summary>
		/********************************************************************/
		private void AdjustAllTrackIndexes()
		{
			for (int i = 0; i < 8; i++)
				AdjustTrackIndexes(voices[i]);
		}



		/********************************************************************/
		/// <summary>
		/// Will adjust the indexes into the track to match the current row
		/// </summary>
		/********************************************************************/
		private void AdjustTrackIndexes(VoiceInfo voiceInfo)
		{
			Track track = voiceInfo.Track;
			ushort spacing = track.DefaultSpacing;
			uint trackPosition = 0;
			int row = 0;

			while (row < playingInfo.CurrentRow)
			{
				TrackLine line = track.Lines[trackPosition];

				if (line.Effect == TrackEffect.SkipEmptyRows)
				{
					row += line.EffectArgument;
					trackPosition++;
				}
				else
				{
					do
					{
						row++;
						trackPosition++;

						line = track.Lines[trackPosition];

						if (line.Effect == TrackEffect.SkipEmptyRows)
							break;

						row += spacing;
					}
					while (row < playingInfo.CurrentRow);
				}
			}

			voiceInfo.RowsLeftToSkip = (short)(row - playingInfo.CurrentRow);
			voiceInfo.TrackPosition = trackPosition;
		}



		/********************************************************************/
		/// <summary>
		/// Go to the next row and parse effects on the new one
		/// </summary>
		/********************************************************************/
		private void HandleRow(VoiceInfo voiceInfo)
		{
			voiceInfo.RowsLeftToSkip--;

			if (voiceInfo.RowsLeftToSkip < 0)
			{
				uint trackPosition = voiceInfo.TrackPosition;
				TrackLine trackLine = voiceInfo.Track.Lines[trackPosition];

				if (trackLine.Effect == TrackEffect.SkipEmptyRows)
				{
					if ((trackLine.EffectArgument == 0) && (trackLine.Note == 0))
						trackPosition++;
					else
					{
						voiceInfo.RowsLeftToSkip = (short)(trackLine.EffectArgument - 1);
						voiceInfo.TrackPosition++;

						return;
					}
				}

				ParseTrackEffect(voiceInfo, trackPosition);

				trackPosition++;

				if (trackPosition < voiceInfo.Track.Lines.Length)
				{
					trackLine = voiceInfo.Track.Lines[trackPosition];

					if (trackLine.Effect == TrackEffect.SkipEmptyRows)
					{
						voiceInfo.RowsLeftToSkip = (short)(trackLine.EffectArgument);
						voiceInfo.TrackPosition = trackPosition + 1;
					}
					else
					{
						voiceInfo.RowsLeftToSkip = (short)voiceInfo.Track.DefaultSpacing;
						voiceInfo.TrackPosition = trackPosition;
					}
				}
				else
				{
					voiceInfo.RowsLeftToSkip = (short)voiceInfo.Track.DefaultSpacing;
					voiceInfo.TrackPosition = trackPosition;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse track effect on a specific position
		/// </summary>
		/********************************************************************/
		private void ParseTrackEffect(VoiceInfo voiceInfo, uint trackPosition)
		{
			TrackLine trackLine = voiceInfo.Track.Lines[trackPosition];

			if ((trackLine.Effect != TrackEffect.None) || (trackLine.EffectArgument != 0) || (trackLine.Note != 0))
			{
				bool newSample = false;

				switch (trackLine.Effect)
				{
					case TrackEffect.PatternLoop:
					{
						ParseTrackPatternLoop(trackLine, trackPosition);
						break;
					}

					case TrackEffect.VolumeDown:
					{
						ParseTrackVolumeDown(voiceInfo, trackLine);
						break;
					}

					case TrackEffect.Portamento:
					{
						ParseTrackPortamento(voiceInfo, trackLine);
						return;
					}

					case TrackEffect.SelEffect:
					{
						ParseTrackSel(voiceInfo, trackLine);
						break;
					}

					default:
					{
						newSample = trackLine.EffectArgument != 0;

						ParseTrackSetVolume(voiceInfo, trackLine);
						break;
					}
				}

				SetupNoteAndSample(voiceInfo, trackLine, newSample);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse the pattern loop effect
		/// </summary>
		/********************************************************************/
		private void ParseTrackPatternLoop(TrackLine trackLine, uint trackPosition)
		{
			if (trackLine.EffectArgument == 0)
			{
				if ((playingInfo.PatternLoopStack.Count > 0) && (playingInfo.PatternLoopStopRow == 0))
				{
					PatternLoopInfo patternLoopInfo = playingInfo.PatternLoopStack.Peek();

					if (patternLoopInfo.OriginalLoopCount == 63)
					{
						// Special case for Jonahara.ftm and Slow-Singslow.ftm
						endReached = true;
					}

					patternLoopInfo.LoopCount--;

					if (patternLoopInfo.LoopCount < 0)
					{
						// Done with loop, remove the pattern loop from the stack
						playingInfo.PatternLoopStack.Pop();
						playingInfo.PatternLoopStopRow = 0;
					}
					else
					{
						playingInfo.PatternLoopStopRow = (ushort)(((playingInfo.CurrentRow / rowsPerMeasure) + 1) * rowsPerMeasure);
						playingInfo.PatternLoopStartRow = patternLoopInfo.LoopStartPosition;
					}
				}
			}
			else
			{
				if (playingInfo.PatternLoopStopRow == 0)
				{
					// Max 16 pattern loops supported
					if (playingInfo.PatternLoopStack.Count < 16)
					{
						ushort loopStartPosition = (ushort)((playingInfo.CurrentRow / rowsPerMeasure) * rowsPerMeasure);

						PatternLoopInfo patternLoopInfo = new PatternLoopInfo
						{
							TrackPosition = trackPosition,
							LoopStartPosition = loopStartPosition,
							LoopCount = (short)trackLine.EffectArgument,
							OriginalLoopCount = (short)trackLine.EffectArgument
						};

						playingInfo.PatternLoopStack.Push(patternLoopInfo);
					}
				}
				else
				{
					PatternLoopInfo patternLoopInfo = playingInfo.PatternLoopStack.Peek();

					if (trackPosition == patternLoopInfo.TrackPosition)
						playingInfo.PatternLoopStopRow = 0;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse the volume down effect
		/// </summary>
		/********************************************************************/
		private void ParseTrackVolumeDown(VoiceInfo voiceInfo, TrackLine trackLine)
		{
			if (trackLine.EffectArgument == 0)
				voiceInfo.Volume = 0;
			else
			{
				int ticksToVolumeDown = trackLine.EffectArgument * playingInfo.Speed;

				voiceInfo.VolumeDownVolume = voiceInfo.Volume * 256U;
				voiceInfo.VolumeDownSpeed = (ushort)(voiceInfo.VolumeDownVolume / ticksToVolumeDown);

				if (voiceInfo.SoundEffectState.VolumeDownGotoLineNumber != 0)
					voiceInfo.SoundEffectState.InterruptLineNumber = voiceInfo.SoundEffectState.VolumeDownGotoLineNumber;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse the portamento effect
		/// </summary>
		/********************************************************************/
		private void ParseTrackPortamento(VoiceInfo voiceInfo, TrackLine trackLine)
		{
			if (trackLine.Note != 0)
			{
				voiceInfo.PortamentoEndNote = (ushort)((trackLine.Note - 1) * 16);
				voiceInfo.PortamentoNote = voiceInfo.NoteIndex * 256U;

				int ticksToPortamento = (int)(voiceInfo.PortamentoEndNote * 256 - voiceInfo.PortamentoNote);

				if (ticksToPortamento != 0)
				{
					if (trackLine.EffectArgument == 0)
					{
						voiceInfo.NoteIndex = voiceInfo.PortamentoEndNote;
						voiceInfo.PortamentoTicks = 0;
					}
					else
					{
						ticksToPortamento /= trackLine.EffectArgument * playingInfo.Speed;

						if (voiceInfo.SoundEffectState.PortamentoGotoLineNumber != 0)
							voiceInfo.SoundEffectState.InterruptLineNumber = voiceInfo.SoundEffectState.PortamentoGotoLineNumber;
					}
				}

				voiceInfo.PortamentoTicks = (short)ticksToPortamento;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse the SEL effect
		/// </summary>
		/********************************************************************/
		private void ParseTrackSel(VoiceInfo voiceInfo, TrackLine trackLine)
		{
			SoundEffectScript script = soundEffectScripts[trackLine.EffectArgument];
			SoundEffectState state = voiceInfo.SoundEffectState;

			state.EffectScript = script;
			state.ScriptPosition = script != null ? 0 : -1;

			state.WaitCounter = 0;
			state.LoopCounter = 0;

			state.NewPitchGotoLineNumber = 0;
			state.NewVolumeGotoLineNumber = 0;
			state.NewSampleGotoLineNumber = 0;
			state.ReleaseGotoLineNumber = 0;
			state.PortamentoGotoLineNumber = 0;
			state.VolumeDownGotoLineNumber = 0;

			state.InterruptLineNumber = 0;

			state.VoiceInfo = voiceInfo;
		}



		/********************************************************************/
		/// <summary>
		/// Parse the set volume effect
		/// </summary>
		/********************************************************************/
		private void ParseTrackSetVolume(VoiceInfo voiceInfo, TrackLine trackLine)
		{
			if (trackLine.EffectArgument != 0)
			{
				voiceInfo.SampleNumber = (short)(trackLine.EffectArgument - 1);
				voiceInfo.CurrentSample = samples[voiceInfo.SampleNumber];

				if (voiceInfo.SoundEffectState.NewSampleGotoLineNumber != 0)
					voiceInfo.SoundEffectState.InterruptLineNumber = voiceInfo.SoundEffectState.NewSampleGotoLineNumber;
			}

			if (trackLine.Effect != TrackEffect.None)
			{
				voiceInfo.Volume = Tables.EffectVolume[(int)trackLine.Effect - 1];
				voiceInfo.VolumeDownSpeed = 0;

				if (voiceInfo.SoundEffectState.NewVolumeGotoLineNumber != 0)
					voiceInfo.SoundEffectState.InterruptLineNumber = voiceInfo.SoundEffectState.NewVolumeGotoLineNumber;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Setup note and sample for the voice
		/// </summary>
		/********************************************************************/
		private void SetupNoteAndSample(VoiceInfo voiceInfo, TrackLine trackLine, bool newSample)
		{
			byte note = trackLine.Note;

			if (note == 35)
			{
				// Release note
				if (voiceInfo.SoundEffectState.ReleaseGotoLineNumber != 0)
					voiceInfo.SoundEffectState.InterruptLineNumber = voiceInfo.SoundEffectState.ReleaseGotoLineNumber;
			}
			else if ((note > 0) && (note < 35))
			{
				voiceInfo.NoteIndex = (ushort)((note - 1) * 16);

				if (voiceInfo.SoundEffectState.NewPitchGotoLineNumber != 0)
					voiceInfo.SoundEffectState.InterruptLineNumber = voiceInfo.SoundEffectState.NewPitchGotoLineNumber;

				voiceInfo.PortamentoTicks = 0;

				Sample sample = voiceInfo.CurrentSample;

				if (newSample || (voiceInfo.SampleData == Tables.QuietSample.SampleData) || (sample.LoopLength == 0))
				{
					voiceInfo.RetrigSample = true;

					voiceInfo.SampleData = sample.SampleData;
					voiceInfo.SampleStartOffset = 0;
					voiceInfo.SampleCalculateOffset = 0;
					voiceInfo.SampleOneshotLength = sample.OneshotLength;
					voiceInfo.SampleLoopStart = sample.LoopStart;
					voiceInfo.SampleLoopLength = sample.LoopLength;
					voiceInfo.SampleTotalLength = sample.TotalLength;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Run the realtime effects
		/// </summary>
		/********************************************************************/
		private void RunEffectsForAllVoices()
		{
			for (int i = 0; i < 8; i++)
				RunEffects(voices[i]);
		}



		/********************************************************************/
		/// <summary>
		/// Run the realtime effects
		/// </summary>
		/********************************************************************/
		private void RunEffects(VoiceInfo voiceInfo)
		{
			RunTrackVolumeDown(voiceInfo);
			RunTrackPortamento(voiceInfo);
			RunTrackSel(voiceInfo);
			RunLfo(voiceInfo);
		}



		/********************************************************************/
		/// <summary>
		/// Run the volume down effect
		/// </summary>
		/********************************************************************/
		private void RunTrackVolumeDown(VoiceInfo voiceInfo)
		{
			if (voiceInfo.VolumeDownSpeed != 0)
			{
				int newValue = (int)(voiceInfo.VolumeDownVolume - voiceInfo.VolumeDownSpeed);

				if (newValue < 0)
				{
					voiceInfo.VolumeDownSpeed = 0;
					newValue = 0;
				}

				voiceInfo.VolumeDownVolume = (uint)newValue;
				voiceInfo.Volume = (ushort)(newValue / 256);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Run the portamento effect
		/// </summary>
		/********************************************************************/
		private void RunTrackPortamento(VoiceInfo voiceInfo)
		{
			if (voiceInfo.PortamentoTicks != 0)
			{
				voiceInfo.PortamentoNote = (uint)(voiceInfo.PortamentoNote + voiceInfo.PortamentoTicks);
				voiceInfo.NoteIndex = (ushort)((voiceInfo.PortamentoNote / 256) & 0xfffe);

				if (voiceInfo.PortamentoTicks < 0)
				{
					if (voiceInfo.NoteIndex <= voiceInfo.PortamentoEndNote)
					{
						voiceInfo.NoteIndex = voiceInfo.PortamentoEndNote;
						voiceInfo.PortamentoTicks = 0;
					}
				}
				else
				{
					if (voiceInfo.NoteIndex >= voiceInfo.PortamentoEndNote)
					{
						voiceInfo.NoteIndex = voiceInfo.PortamentoEndNote;
						voiceInfo.PortamentoTicks = 0;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Run the SEL effect
		/// </summary>
		/********************************************************************/
		private void RunTrackSel(VoiceInfo voiceInfo)
		{
			SoundEffectState state = voiceInfo.SoundEffectState;

			if (state.ScriptPosition >= 0)
			{
				if (state.InterruptLineNumber != 0)
				{
					ushort gotoLine = state.InterruptLineNumber;

					state.InterruptLineNumber = 0;
					state.WaitCounter = 0;
					state.LoopCounter = 0;

					if (!RunSelGotoLine(state, gotoLine))
						return;
				}
				else
				{
					if (state.WaitCounter != 0)
					{
						if (state.WaitCounter != 0xffff)
							state.WaitCounter--;

						return;
					}
				}

				for (;;)
				{
					SoundEffectLine line = state.EffectScript.Script[state.ScriptPosition];

					if (line.Effect != SoundEffect.Nothing)
					{
						if (!RunSpecificSelEffect(voiceInfo, state, line))
							break;
					}

					state.ScriptPosition++;

					if (state.ScriptPosition >= state.EffectScript.Script.Length)
					{
						state.ScriptPosition = -1;
						break;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Find which SEL effect to run and run it
		/// </summary>
		/********************************************************************/
		private bool RunSpecificSelEffect(VoiceInfo voiceInfo, SoundEffectState state, SoundEffectLine line)
		{
			VoiceInfo selVoiceInfo = state.VoiceInfo;

			switch (line.Effect)
			{
				case SoundEffect.Wait:
				{
					RunSelWait(state, line);
					return false;
				}

				case SoundEffect.Goto:
					return RunSelGotoLine(state, line);

				case SoundEffect.Loop:
					return RunSelLoop(state, line);

				case SoundEffect.GotoScript:
					return RunSelGotoScript(state, line);

				case SoundEffect.End:
				{
					RunSelEndOfEffect(state);
					return false;
				}

				case SoundEffect.IfPitchEqual:
					return RunSelIfPitchEqual(state, line, selVoiceInfo);

				case SoundEffect.IfPitchLessThan:
					return RunSelIfPitchLessThan(state, line, selVoiceInfo);

				case SoundEffect.IfPitchGreaterThan:
					return RunSelIfPitchGreaterThan(state, line, selVoiceInfo);

				case SoundEffect.IfVolumeEqual:
					return RunSelIfVolumeEqual(state, line, selVoiceInfo);

				case SoundEffect.IfVolumeLessThan:
					return RunSelIfVolumeLessThan(state, line, selVoiceInfo);

				case SoundEffect.IfVolumeGreaterThan:
					return RunSelIfVolumeGreaterThan(state, line, selVoiceInfo);

				case SoundEffect.OnNewPitch:
				{
					RunSelOnNewPitch(state, line);
					break;
				}

				case SoundEffect.OnNewVolume:
				{
					RunSelOnNewVolume(state, line);
					break;
				}

				case SoundEffect.OnNewSample:
				{
					RunSelOnNewSample(state, line);
					break;
				}

				case SoundEffect.OnRelease:
				{
					RunSelOnRelease(state, line);
					break;
				}

				case SoundEffect.OnPortamento:
				{
					RunSelOnPortamento(state, line);
					break;
				}

				case SoundEffect.OnVolumeDown:
				{
					RunSelOnVolumeDown(state, line);
					break;
				}

				case SoundEffect.PlayCurrentSample:
				{
					RunSelPlaySample(selVoiceInfo, selVoiceInfo.CurrentSample);
					break;
				}

				case SoundEffect.PlayQuietSample:
				{
					RunSelPlaySample(selVoiceInfo, Tables.QuietSample);
					break;
				}

				case SoundEffect.PlayPosition:
				{
					RunSelPlayPosition(line, selVoiceInfo);
					break;
				}

				case SoundEffect.PlayPositionAdd:
				{
					RunSelPlayPositionAdd(line, selVoiceInfo);
					break;
				}

				case SoundEffect.PlayPositionSub:
				{
					RunSelPlayPositionSub(line, selVoiceInfo);
					break;
				}

				case SoundEffect.Pitch:
				{
					RunSelPitch(line, selVoiceInfo);
					break;
				}

				case SoundEffect.Detune:
				{
					RunSelDetune(line, selVoiceInfo);
					break;
				}

				case SoundEffect.DetunePitchAdd:
				{
					RunSelDetunePitchAdd(line, selVoiceInfo);
					break;
				}

				case SoundEffect.DetunePitchSub:
				{
					RunSelDetunePitchSub(line, selVoiceInfo);
					break;
				}

				case SoundEffect.Volume:
				{
					RunSelVolume(line, selVoiceInfo);
					break;
				}

				case SoundEffect.VolumeAdd:
				{
					RunSelVolumeAdd(line, selVoiceInfo);
					break;
				}

				case SoundEffect.VolumeSub:
				{
					RunSelVolumeSub(line, selVoiceInfo);
					break;
				}

				case SoundEffect.CurrentSample:
				{
					RunSelCurrentSample(line, selVoiceInfo);
					break;
				}

				case SoundEffect.SampleStart:
				{
					RunSelSampleStart(line, selVoiceInfo);
					break;
				}

				case SoundEffect.SampleStartAdd:
				{
					RunSelSampleStartAdd(line, selVoiceInfo);
					break;
				}

				case SoundEffect.SampleStartSub:
				{
					RunSelSampleStartSub(line, selVoiceInfo);
					break;
				}

				case SoundEffect.OneshotLength:
				{
					RunSelOneshotLength(line, selVoiceInfo);
					break;
				}

				case SoundEffect.OneshotLengthAdd:
				{
					RunSelOneshotLengthAdd(line, selVoiceInfo);
					break;
				}

				case SoundEffect.OneshotLengthSub:
				{
					RunSelOneshotLengthSub(line, selVoiceInfo);
					break;
				}

				case SoundEffect.RepeathLength:
				{
					RunSelRepeatLength(line, selVoiceInfo);
					break;
				}

				case SoundEffect.RepeathLengthAdd:
				{
					RunSelRepeatLengthAdd(line, selVoiceInfo);
					break;
				}

				case SoundEffect.RepeathLengthSub:
				{
					RunSelRepeatLengthSub(line, selVoiceInfo);
					break;
				}

				case SoundEffect.GetPitchOfTrack:
				{
					RunSelGetPitchOfTrack(line, selVoiceInfo);
					break;
				}

				case SoundEffect.GetVolumeOfTrack:
				{
					RunSelGetVolumeOfTrack(line, selVoiceInfo);
					break;
				}

				case SoundEffect.GetSampleOfTrack:
				{
					RunSelGetSampleOfTrack(line, selVoiceInfo);
					break;
				}

				case SoundEffect.CloneTrack:
				{
					RunSelCloneTrack(line, selVoiceInfo);
					break;
				}

				case SoundEffect.FirstLfoStart:
				{
					RunSelLfoStart(line, voiceInfo.LfoStates[0]);
					break;
				}

				case SoundEffect.FirstLfoSpeedDepthAdd:
				{
					RunSelLfoSpeedDepthAdd(line, voiceInfo.LfoStates[0]);
					break;
				}

				case SoundEffect.FirstLfoSpeedDepthSub:
				{
					RunSelLfoSpeedDepthSub(line, voiceInfo.LfoStates[0]);
					break;
				}

				case SoundEffect.SecondLfoStart:
				{
					RunSelLfoStart(line, voiceInfo.LfoStates[1]);
					break;
				}

				case SoundEffect.SecondLfoSpeedDepthAdd:
				{
					RunSelLfoSpeedDepthAdd(line, voiceInfo.LfoStates[1]);
					break;
				}

				case SoundEffect.SecondLfoSpeedDepthSub:
				{
					RunSelLfoSpeedDepthSub(line, voiceInfo.LfoStates[1]);
					break;
				}

				case SoundEffect.ThirdLfoStart:
				{
					RunSelLfoStart(line, voiceInfo.LfoStates[2]);
					break;
				}

				case SoundEffect.ThirdLfoSpeedDepthAdd:
				{
					RunSelLfoSpeedDepthAdd(line, voiceInfo.LfoStates[2]);
					break;
				}

				case SoundEffect.ThirdLfoSpeedDepthSub:
				{
					RunSelLfoSpeedDepthSub(line, voiceInfo.LfoStates[2]);
					break;
				}

				case SoundEffect.FourthLfoStart:
				{
					RunSelLfoStart(line, voiceInfo.LfoStates[3]);
					break;
				}

				case SoundEffect.FourthLfoSpeedDepthAdd:
				{
					RunSelLfoSpeedDepthAdd(line, voiceInfo.LfoStates[3]);
					break;
				}

				case SoundEffect.FourthLfoSpeedDepthSub:
				{
					RunSelLfoSpeedDepthSub(line, voiceInfo.LfoStates[3]);
					break;
				}

				case SoundEffect.WorkOnTrack:
				{
					RunSelWorkOnTrack(line, voiceInfo);
					break;
				}

				case SoundEffect.WorkTrackAdd:
				{
					RunSelWorkTrackAdd(line, voiceInfo);
					break;
				}

				case SoundEffect.GlobalVolume:
				{
					RunSelGlobalVolume(line);
					break;
				}

				case SoundEffect.GlobalSpeed:
				{
					RunSelGlobalSpeed(line);
					break;
				}

				case SoundEffect.TicksPerLine:
				{
					RunSelTicksPerLine(line);
					break;
				}

				case SoundEffect.JumpToSongLine:
				{
					RunSelJumpToSongLine(line);
					break;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Wait
		/// </summary>
		/********************************************************************/
		private void RunSelWait(SoundEffectState state, SoundEffectLine line)
		{
			state.WaitCounter = (ushort)(line.Argument2 - 1);

			if (state.WaitCounter != 0xffff)
			{
				state.ScriptPosition++;

				if (state.ScriptPosition == state.EffectScript.Script.Length)
					state.ScriptPosition = -1;
			}
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Goto line
		/// </summary>
		/********************************************************************/
		private bool RunSelGotoLine(SoundEffectState state, SoundEffectLine line)
		{
			return RunSelGotoLine(state, (ushort)(line.Argument2 & 0x0fff));
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Goto line
		/// </summary>
		/********************************************************************/
		private bool RunSelGotoLine(SoundEffectState state, ushort lineNumber)
		{
			if (lineNumber < state.EffectScript.Script.Length)
			{
				state.ScriptPosition = lineNumber - 1;
				return true;
			}

			state.ScriptPosition = -1;

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Loop
		/// </summary>
		/********************************************************************/
		private bool RunSelLoop(SoundEffectState state, SoundEffectLine line)
		{
			if (state.LoopCounter == 0)
				state.LoopCounter = (ushort)((line.Argument1 << 4) | ((line.Argument2 & 0xf000) >> 12));
			else
				state.LoopCounter--;

			if (state.LoopCounter == 0)
				return true;

			return RunSelGotoLine(state, line);
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Goto script
		/// </summary>
		/********************************************************************/
		private bool RunSelGotoScript(SoundEffectState state, SoundEffectLine line)
		{
			int scriptNumber = ((line.Argument1 << 4) | ((line.Argument2 & 0xf000) >> 12));
			SoundEffectScript script = soundEffectScripts[scriptNumber];

			if (script == null)
			{
				state.ScriptPosition = -1;
				return false;
			}

			state.EffectScript = script;

			return RunSelGotoLine(state, line);
		}



		/********************************************************************/
		/// <summary>
		/// SEL: End of effect
		/// </summary>
		/********************************************************************/
		private void RunSelEndOfEffect(SoundEffectState state)
		{
			state.ScriptPosition = -1;
		}



		/********************************************************************/
		/// <summary>
		/// SEL: If pitch equal
		/// </summary>
		/********************************************************************/
		private bool RunSelIfPitchEqual(SoundEffectState state, SoundEffectLine line, VoiceInfo selVoiceInfo)
		{
			int pitch = ((line.Argument1 << 4) | ((line.Argument2 & 0xf000) >> 12));

			if (selVoiceInfo.NoteIndex == pitch)
				return RunSelGotoLine(state, line);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// SEL: If pitch less than
		/// </summary>
		/********************************************************************/
		private bool RunSelIfPitchLessThan(SoundEffectState state, SoundEffectLine line, VoiceInfo selVoiceInfo)
		{
			int pitch = ((line.Argument1 << 4) | ((line.Argument2 & 0xf000) >> 12));

			if (selVoiceInfo.NoteIndex < pitch)
				return RunSelGotoLine(state, line);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// SEL: If pitch greater than
		/// </summary>
		/********************************************************************/
		private bool RunSelIfPitchGreaterThan(SoundEffectState state, SoundEffectLine line, VoiceInfo selVoiceInfo)
		{
			int pitch = ((line.Argument1 << 4) | ((line.Argument2 & 0xf000) >> 12));

			if (selVoiceInfo.NoteIndex > pitch)
				return RunSelGotoLine(state, line);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// SEL: If volume equal
		/// </summary>
		/********************************************************************/
		private bool RunSelIfVolumeEqual(SoundEffectState state, SoundEffectLine line, VoiceInfo selVoiceInfo)
		{
			int volume = ((line.Argument1 << 4) | ((line.Argument2 & 0xf000) >> 12));

			if (selVoiceInfo.Volume == volume)
				return RunSelGotoLine(state, line);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// SEL: If volume less than
		/// </summary>
		/********************************************************************/
		private bool RunSelIfVolumeLessThan(SoundEffectState state, SoundEffectLine line, VoiceInfo selVoiceInfo)
		{
			int volume = ((line.Argument1 << 4) | ((line.Argument2 & 0xf000) >> 12));

			if (selVoiceInfo.Volume < volume)
				return RunSelGotoLine(state, line);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// SEL: If volume greater than
		/// </summary>
		/********************************************************************/
		private bool RunSelIfVolumeGreaterThan(SoundEffectState state, SoundEffectLine line, VoiceInfo selVoiceInfo)
		{
			int volume = ((line.Argument1 << 4) | ((line.Argument2 & 0xf000) >> 12));

			if (selVoiceInfo.Volume > volume)
				return RunSelGotoLine(state, line);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// SEL: On new pitch
		/// </summary>
		/********************************************************************/
		private void RunSelOnNewPitch(SoundEffectState state, SoundEffectLine line)
		{
			state.NewPitchGotoLineNumber = (ushort)((line.Argument2 & 0x0fff) + 1);
		}



		/********************************************************************/
		/// <summary>
		/// SEL: On new volume
		/// </summary>
		/********************************************************************/
		private void RunSelOnNewVolume(SoundEffectState state, SoundEffectLine line)
		{
			state.NewVolumeGotoLineNumber = (ushort)((line.Argument2 & 0x0fff) + 1);
		}



		/********************************************************************/
		/// <summary>
		/// SEL: On new sample
		/// </summary>
		/********************************************************************/
		private void RunSelOnNewSample(SoundEffectState state, SoundEffectLine line)
		{
			state.NewSampleGotoLineNumber = (ushort)((line.Argument2 & 0x0fff) + 1);
		}



		/********************************************************************/
		/// <summary>
		/// SEL: On release
		/// </summary>
		/********************************************************************/
		private void RunSelOnRelease(SoundEffectState state, SoundEffectLine line)
		{
			state.ReleaseGotoLineNumber = (ushort)((line.Argument2 & 0x0fff) + 1);
		}



		/********************************************************************/
		/// <summary>
		/// SEL: On portamento
		/// </summary>
		/********************************************************************/
		private void RunSelOnPortamento(SoundEffectState state, SoundEffectLine line)
		{
			state.PortamentoGotoLineNumber = (ushort)((line.Argument2 & 0x0fff) + 1);
		}



		/********************************************************************/
		/// <summary>
		/// SEL: On volume down
		/// </summary>
		/********************************************************************/
		private void RunSelOnVolumeDown(SoundEffectState state, SoundEffectLine line)
		{
			state.VolumeDownGotoLineNumber = (ushort)((line.Argument2 & 0x0fff) + 1);
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Play given sample
		/// </summary>
		/********************************************************************/
		private void RunSelPlaySample(VoiceInfo selVoiceInfo, Sample sample)
		{
			selVoiceInfo.SampleData = sample.SampleData;
			selVoiceInfo.SampleStartOffset = 0;
			selVoiceInfo.SampleCalculateOffset = 0;
			selVoiceInfo.SampleOneshotLength = sample.OneshotLength;
			selVoiceInfo.SampleLoopStart = sample.LoopStart;
			selVoiceInfo.SampleLoopLength = sample.LoopLength;
			selVoiceInfo.SampleTotalLength = sample.TotalLength;

			selVoiceInfo.RetrigSample = true;
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Play position
		/// </summary>
		/********************************************************************/
		private void RunSelPlayPosition(SoundEffectLine line, VoiceInfo selVoiceInfo)
		{
			uint offset = (uint)(((line.Argument1 & 0x0f) << 16) + line.Argument2);

			selVoiceInfo.SampleStartOffset = selVoiceInfo.SampleCalculateOffset + offset * 2;
			selVoiceInfo.RetrigSample = true;
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Play position add
		/// </summary>
		/********************************************************************/
		private void RunSelPlayPositionAdd(SoundEffectLine line, VoiceInfo selVoiceInfo)
		{
			uint offset = (uint)(((line.Argument1 & 0x0f) << 16) + line.Argument2);

			selVoiceInfo.SampleStartOffset += offset;
			selVoiceInfo.RetrigSample = true;
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Play position sub
		/// </summary>
		/********************************************************************/
		private void RunSelPlayPositionSub(SoundEffectLine line, VoiceInfo selVoiceInfo)
		{
			uint offset = (uint)(((line.Argument1 & 0x0f) << 16) + line.Argument2);

			selVoiceInfo.SampleStartOffset -= offset;
			selVoiceInfo.RetrigSample = true;
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Pitch
		/// </summary>
		/********************************************************************/
		private void RunSelPitch(SoundEffectLine line, VoiceInfo selVoiceInfo)
		{
			ushort noteIndex = (ushort)((line.Argument2 & 0x0fff) * 2);

			if (noteIndex >= 542)
				noteIndex = 542;

			selVoiceInfo.NoteIndex = noteIndex;
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Detune
		/// </summary>
		/********************************************************************/
		private void RunSelDetune(SoundEffectLine line, VoiceInfo selVoiceInfo)
		{
			short detune = (short)(line.Argument2 & 0x0fff);

			playingInfo.DetuneValues[selVoiceInfo.DetuneIndex] = detune;
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Detune and pitch add
		/// </summary>
		/********************************************************************/
		private void RunSelDetunePitchAdd(SoundEffectLine line, VoiceInfo selVoiceInfo)
		{
			short detune = (short)((line.Argument1 << 4) | ((line.Argument2 & 0xf000) >> 12));
			playingInfo.DetuneValues[selVoiceInfo.DetuneIndex] += detune;

			ushort noteIndex = (ushort)((line.Argument2 & 0x0fff) * 2);
			noteIndex += selVoiceInfo.NoteIndex;

			if (noteIndex > 542)
				noteIndex = 542;

			selVoiceInfo.NoteIndex = noteIndex;
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Detune and pitch sub
		/// </summary>
		/********************************************************************/
		private void RunSelDetunePitchSub(SoundEffectLine line, VoiceInfo selVoiceInfo)
		{
			short detune = (short)((line.Argument1 << 4) | ((line.Argument2 & 0xf000) >> 12));
			playingInfo.DetuneValues[selVoiceInfo.DetuneIndex] -= detune;

			int noteIndex = (line.Argument2 & 0x0fff) * 2;
			noteIndex = selVoiceInfo.NoteIndex - noteIndex;

			if (noteIndex < 0)
				noteIndex = 0;

			selVoiceInfo.NoteIndex = (ushort)noteIndex;
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Volume
		/// </summary>
		/********************************************************************/
		private void RunSelVolume(SoundEffectLine line, VoiceInfo selVoiceInfo)
		{
			int newVolume = line.Argument2 & 0x00ff;

			if (newVolume > 64)
				newVolume = 64;

			selVoiceInfo.Volume = (ushort)newVolume;
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Volume add
		/// </summary>
		/********************************************************************/
		private void RunSelVolumeAdd(SoundEffectLine line, VoiceInfo selVoiceInfo)
		{
			int newVolume = selVoiceInfo.Volume + (line.Argument2 & 0x00ff);

			if (newVolume > 64)
				newVolume = 64;

			selVoiceInfo.Volume = (ushort)newVolume;
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Volume sub
		/// </summary>
		/********************************************************************/
		private void RunSelVolumeSub(SoundEffectLine line, VoiceInfo selVoiceInfo)
		{
			int newVolume = selVoiceInfo.Volume - (line.Argument2 & 0x00ff);

			if (newVolume < 0)
				newVolume = 0;

			selVoiceInfo.Volume = (ushort)newVolume;
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Current sample
		/// </summary>
		/********************************************************************/
		private void RunSelCurrentSample(SoundEffectLine line, VoiceInfo selVoiceInfo)
		{
			selVoiceInfo.SampleNumber = (short)(line.Argument2 & 0x00ff);
			selVoiceInfo.CurrentSample = samples[selVoiceInfo.SampleNumber];
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Sample start
		/// </summary>
		/********************************************************************/
		private void RunSelSampleStart(SoundEffectLine line, VoiceInfo selVoiceInfo)
		{
			uint offset = (uint)(((line.Argument1 & 0x0f) << 16) + line.Argument2);
			if (offset > selVoiceInfo.SampleTotalLength)
				offset = selVoiceInfo.SampleTotalLength;

			selVoiceInfo.SampleCalculateOffset = offset;
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Sample start add
		/// </summary>
		/********************************************************************/
		private void RunSelSampleStartAdd(SoundEffectLine line, VoiceInfo selVoiceInfo)
		{
			uint offset = (uint)(((line.Argument1 & 0x0f) << 16) + line.Argument2);
			offset += selVoiceInfo.SampleCalculateOffset;

			if (offset > selVoiceInfo.SampleTotalLength)
				offset = selVoiceInfo.SampleTotalLength;

			selVoiceInfo.SampleCalculateOffset = offset;
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Sample start sub
		/// </summary>
		/********************************************************************/
		private void RunSelSampleStartSub(SoundEffectLine line, VoiceInfo selVoiceInfo)
		{
			int offset = ((line.Argument1 & 0x0f) << 16) + line.Argument2;
			offset = (int)(selVoiceInfo.SampleCalculateOffset - offset);

			if (offset < 0)
				offset = 0;

			selVoiceInfo.SampleCalculateOffset = (uint)offset;
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Oneshot length
		/// </summary>
		/********************************************************************/
		private void RunSelOneshotLength(SoundEffectLine line, VoiceInfo selVoiceInfo)
		{
			RunSelOneshotLengthHelper(selVoiceInfo, line.Argument2);
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Oneshot length add
		/// </summary>
		/********************************************************************/
		private void RunSelOneshotLengthAdd(SoundEffectLine line, VoiceInfo selVoiceInfo)
		{
			int newLength = selVoiceInfo.SampleOneshotLength + line.Argument2;
			if (newLength > 0xffff)
				newLength = 0xffff;

			RunSelOneshotLengthHelper(selVoiceInfo, (ushort)newLength);
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Oneshot length sub
		/// </summary>
		/********************************************************************/
		private void RunSelOneshotLengthSub(SoundEffectLine line, VoiceInfo selVoiceInfo)
		{
			int newLength = selVoiceInfo.SampleOneshotLength - line.Argument2;
			if (newLength < 0)
				newLength = 0;

			RunSelOneshotLengthHelper(selVoiceInfo, (ushort)newLength);
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Oneshot length helper
		/// </summary>
		/********************************************************************/
		private void RunSelOneshotLengthHelper(VoiceInfo selVoiceInfo, ushort newLength)
		{
			selVoiceInfo.SampleOneshotLength = newLength;
			selVoiceInfo.SampleLoopStart = selVoiceInfo.SampleCalculateOffset + newLength * 2U;
			selVoiceInfo.SampleTotalLength = selVoiceInfo.SampleLoopLength * 2U + selVoiceInfo.SampleLoopStart;
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Repeat length
		/// </summary>
		/********************************************************************/
		private void RunSelRepeatLength(SoundEffectLine line, VoiceInfo selVoiceInfo)
		{
			RunSelRepeatLengthHelper(selVoiceInfo, line.Argument2);
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Repeat length add
		/// </summary>
		/********************************************************************/
		private void RunSelRepeatLengthAdd(SoundEffectLine line, VoiceInfo selVoiceInfo)
		{
			int newLength = selVoiceInfo.SampleLoopLength + line.Argument2;
			if (newLength > 0xffff)
				newLength = 0xffff;

			RunSelRepeatLengthHelper(selVoiceInfo, (ushort)newLength);
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Repeat length sub
		/// </summary>
		/********************************************************************/
		private void RunSelRepeatLengthSub(SoundEffectLine line, VoiceInfo selVoiceInfo)
		{
			int newLength = selVoiceInfo.SampleLoopLength - line.Argument2;
			if (newLength < 0)
				newLength = 0;

			RunSelRepeatLengthHelper(selVoiceInfo, (ushort)newLength);
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Repeat length helper
		/// </summary>
		/********************************************************************/
		private void RunSelRepeatLengthHelper(VoiceInfo selVoiceInfo, ushort newLength)
		{
			selVoiceInfo.SampleLoopLength = newLength;
			selVoiceInfo.SampleTotalLength = (uint)(selVoiceInfo.SampleCalculateOffset + ((selVoiceInfo.SampleOneshotLength + newLength) * 2U));
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Get pitch of track
		/// </summary>
		/********************************************************************/
		private void RunSelGetPitchOfTrack(SoundEffectLine line, VoiceInfo selVoiceInfo)
		{
			int track = line.Argument2 & 0x0007;

			selVoiceInfo.NoteIndex = voices[track].NoteIndex;
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Get volume of track
		/// </summary>
		/********************************************************************/
		private void RunSelGetVolumeOfTrack(SoundEffectLine line, VoiceInfo selVoiceInfo)
		{
			int track = line.Argument2 & 0x0007;

			selVoiceInfo.Volume = voices[track].Volume;
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Get sample of track
		/// </summary>
		/********************************************************************/
		private void RunSelGetSampleOfTrack(SoundEffectLine line, VoiceInfo selVoiceInfo)
		{
			int track = line.Argument2 & 0x0007;

			selVoiceInfo.CurrentSample = voices[track].CurrentSample;
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Clone track
		/// </summary>
		/********************************************************************/
		private void RunSelCloneTrack(SoundEffectLine line, VoiceInfo selVoiceInfo)
		{
			int track = line.Argument2 & 0x0007;

			voices[track].CopyTo(selVoiceInfo);
		}



		/********************************************************************/
		/// <summary>
		/// SEL: LFO start
		/// </summary>
		/********************************************************************/
		private void RunSelLfoStart(SoundEffectLine line, LfoState lfoState)
		{
			int target = (line.Argument1 & 0xf0) >> 4;
			int shape = line.Argument1 & 0x0f;
			int speed = (line.Argument2 & 0xff00) >> 8;
			int depth = line.Argument2 & 0x00ff;

			lfoState.Target = (LfoTarget)target;
			lfoState.LoopModulation = (shape & 0x08) == 0;
			lfoState.ShapeTable = Tables.LfoShapes[shape & 0x07];
			lfoState.ShapeTablePosition = 0;

			if (speed > 192)
				speed = 191;

			lfoState.ModulationSpeed = (ushort)speed;
			lfoState.ModulationDepth = (ushort)(depth & 0x7f);
			lfoState.ModulationValue = 0;
		}



		/********************************************************************/
		/// <summary>
		/// SEL: LFO speed/depth add
		/// </summary>
		/********************************************************************/
		private void RunSelLfoSpeedDepthAdd(SoundEffectLine line, LfoState lfoState)
		{
			ushort speed = (ushort)(lfoState.ModulationSpeed + ((line.Argument2 & 0xff00) >> 8));

			if (speed > 192)
				speed = 191;

			lfoState.ModulationSpeed = speed;

			ushort depth = (ushort)(lfoState.ModulationDepth + (line.Argument2 & 0x00ff));

			if (depth > 127)
				depth = 127;

			lfoState.ModulationDepth = depth;
		}



		/********************************************************************/
		/// <summary>
		/// SEL: LFO speed/depth sub
		/// </summary>
		/********************************************************************/
		private void RunSelLfoSpeedDepthSub(SoundEffectLine line, LfoState lfoState)
		{
			short speed = (short)(lfoState.ModulationSpeed - ((line.Argument2 & 0xff00) >> 8));

			if (speed < 0)
				speed = 0;

			lfoState.ModulationSpeed = (ushort)speed;

			short depth = (short)(lfoState.ModulationDepth - (line.Argument2 & 0x00ff));

			if (depth < 0)
				depth = 0;

			lfoState.ModulationDepth = (ushort)depth;
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Work on track
		/// </summary>
		/********************************************************************/
		private void RunSelWorkOnTrack(SoundEffectLine line, VoiceInfo voiceInfo)
		{
			int track = line.Argument2 & 0x0007;

			voiceInfo.SoundEffectState.VoiceInfo = voices[track];
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Work track add
		/// </summary>
		/********************************************************************/
		private void RunSelWorkTrackAdd(SoundEffectLine line, VoiceInfo voiceInfo)
		{
			int track = line.Argument2 & 0x0007;
			track += voiceInfo.SoundEffectState.VoiceInfo.ChannelNumber;

			if (track >= 8)
				track -= 8;

			voiceInfo.SoundEffectState.VoiceInfo = voices[track];
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Global volume
		/// </summary>
		/********************************************************************/
		private void RunSelGlobalVolume(SoundEffectLine line)
		{
			int volume = line.Argument2 & 0x00ff;

			if (volume > 64)
				volume = 64;

			playingInfo.GlobalVolume = (byte)volume;
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Global speed
		/// </summary>
		/********************************************************************/
		private void RunSelGlobalSpeed(SoundEffectLine line)
		{
			ushort ciaTempo = line.Argument2;

			if (ciaTempo < 0x1000)
				ciaTempo = 0x1000;

			SetCiaTimerTempo(ciaTempo);
			ShowTempo();
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Ticks per line
		/// </summary>
		/********************************************************************/
		private void RunSelTicksPerLine(SoundEffectLine line)
		{
			playingInfo.Speed = (ushort)(line.Argument2 & 0x00ff);

			ShowSpeed();
		}



		/********************************************************************/
		/// <summary>
		/// SEL: Jump to song line
		/// </summary>
		/********************************************************************/
		private void RunSelJumpToSongLine(SoundEffectLine line)
		{
			playingInfo.PatternLoopStartRow = line.Argument2;
			playingInfo.DoPatternLoop = true;
		}



		/********************************************************************/
		/// <summary>
		/// Run all LFOs
		/// </summary>
		/********************************************************************/
		private void RunLfo(VoiceInfo voiceInfo)
		{
			for (int i = 0; i < 4; i++)
			{
				LfoState lfoState = voiceInfo.LfoStates[i];

				if ((lfoState.ModulationSpeed != 0) && (lfoState.Target != LfoTarget.Nothing))
				{
					VoiceInfo selVoiceInfo = voiceInfo.SoundEffectState.VoiceInfo;

					sbyte[] shape = lfoState.ShapeTable;
					int shapePos = lfoState.ShapeTablePosition;

					int value = ((shape[shapePos] * lfoState.ModulationDepth) / 128);
					int addValue = value - lfoState.ModulationValue;

					switch (lfoState.Target)
					{
						case LfoTarget.Lfo1Speed:
						{
							int newValue = selVoiceInfo.LfoStates[0].ModulationSpeed + addValue;
							selVoiceInfo.LfoStates[0].ModulationSpeed = (ushort)Math.Clamp(newValue, 0, 95);
							break;
						}

						case LfoTarget.Lfo2Speed:
						{
							int newValue = selVoiceInfo.LfoStates[1].ModulationSpeed + addValue;
							selVoiceInfo.LfoStates[1].ModulationSpeed = (ushort)Math.Clamp(newValue, 0, 95);
							break;
						}

						case LfoTarget.Lfo3Speed:
						{
							int newValue = selVoiceInfo.LfoStates[2].ModulationSpeed + addValue;
							selVoiceInfo.LfoStates[2].ModulationSpeed = (ushort)Math.Clamp(newValue, 0, 95);
							break;
						}

						case LfoTarget.Lfo4Speed:
						{
							int newValue = selVoiceInfo.LfoStates[3].ModulationSpeed + addValue;
							selVoiceInfo.LfoStates[3].ModulationSpeed = (ushort)Math.Clamp(newValue, 0, 95);
							break;
						}

						case LfoTarget.Lfo1Depth:
						{
							int newValue = selVoiceInfo.LfoStates[0].ModulationDepth + addValue;
							selVoiceInfo.LfoStates[0].ModulationDepth = (ushort)Math.Clamp(newValue, 0, 127);
							break;
						}

						case LfoTarget.Lfo2Depth:
						{
							int newValue = selVoiceInfo.LfoStates[1].ModulationDepth + addValue;
							selVoiceInfo.LfoStates[1].ModulationDepth = (ushort)Math.Clamp(newValue, 0, 127);
							break;
						}

						case LfoTarget.Lfo3Depth:
						{
							int newValue = selVoiceInfo.LfoStates[2].ModulationDepth + addValue;
							selVoiceInfo.LfoStates[2].ModulationDepth = (ushort)Math.Clamp(newValue, 0, 127);
							break;
						}

						case LfoTarget.Lfo4Depth:
						{
							int newValue = selVoiceInfo.LfoStates[3].ModulationDepth + addValue;
							selVoiceInfo.LfoStates[3].ModulationDepth = (ushort)Math.Clamp(newValue, 0, 127);
							break;
						}

						case LfoTarget.TrackAmplitude:
						{
							int newValue = selVoiceInfo.Volume + addValue;
							selVoiceInfo.Volume = (ushort)Math.Clamp(newValue, 0, 64);
							break;
						}

						case LfoTarget.TrackFrequency:
						{
							int newValue = selVoiceInfo.NoteIndex + (addValue * 2);
							selVoiceInfo.NoteIndex = (ushort)Math.Clamp(newValue, 0, 542);
							break;
						}
					}

					lfoState.ModulationValue = (short)value;

					shapePos += lfoState.ModulationSpeed;

					if (shapePos >= 192)
					{
						shapePos -= 192;

						if (!lfoState.LoopModulation)
							lfoState.ModulationSpeed = 0;
					}

					lfoState.ShapeTablePosition = shapePos;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Tell NostalgicPlayer what to play
		/// </summary>
		/********************************************************************/
		private void SetupHardware()
		{
			for (int i = 0; i < 8; i++)
			{
				int chn = channelMapping[i];

				if (chn != -1)
				{
					VoiceInfo voiceInfo = voices[i];
					IChannel channel = VirtualChannels[chn];

					if (voiceInfo.RetrigSample)
					{
						voiceInfo.RetrigSample = false;

						channel.PlaySample(voiceInfo.SampleNumber, voiceInfo.SampleData, voiceInfo.SampleStartOffset, voiceInfo.SampleTotalLength - voiceInfo.SampleStartOffset);

						if (voiceInfo.SampleLoopLength != 0)
							channel.SetLoop(voiceInfo.SampleLoopStart, voiceInfo.SampleLoopLength * 2U);
					}

					ushort period = Tables.Periods[voiceInfo.NoteIndex / 2];

					if (period != 0)
					{
						uint frequency = PeriodToFrequency(period);

						int detune = playingInfo.DetuneValues[voiceInfo.DetuneIndex] * -8;
						if (detune != 0)
							frequency = (uint)(frequency * Math.Pow(2.0, detune / (12.0 * 256.0 * 128.0)));

						channel.SetFrequency(frequency);
					}

					ushort volume = (ushort)((voiceInfo.Volume * playingInfo.GlobalVolume) / 64);
					channel.SetAmigaVolume(volume);

					channel.SetPanning((ushort)Tables.PanPos[i]);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current song position
		/// </summary>
		/********************************************************************/
		private void ShowSongPosition()
		{
			OnModuleInfoChanged(InfoPositionLine, FormatSongPosition());
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
			ShowSongPosition();
			ShowSpeed();
			ShowTempo();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the song current position
		/// </summary>
		/********************************************************************/
		private string FormatSongPosition()
		{
			return (playingInfo.CurrentRow / rowsPerMeasure).ToString();
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
