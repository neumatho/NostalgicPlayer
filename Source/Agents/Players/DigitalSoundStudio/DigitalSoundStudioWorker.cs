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
using Polycode.NostalgicPlayer.Agent.Player.DigitalSoundStudio.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.DigitalSoundStudio
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class DigitalSoundStudioWorker : ModulePlayerWithPositionDurationAgentBase
	{
		private byte startSongTempo;
		private byte startSongSpeed;

		private Sample[] samples;
		private byte[] positions;
		private Pattern[] patterns;

		private GlobalPlayingInfo playingInfo;
		private VoiceInfo[] voices;

		private bool endReached;

		private const int InfoPositionLine = 3;
		private const int InfoPatternLine = 4;
		private const int InfoSpeedLine = 5;
		private const int InfoTempoLine = 6;

		#region Identify
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => [ "dss" ];



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
			if (fileSize < 0x61e)
				return AgentResult.Unknown;

			// Check the mark
			moduleStream.Seek(0, SeekOrigin.Begin);

			if (moduleStream.ReadMark() != "MMU2")
				return AgentResult.Unknown;

			// Now check to see if it is a song or module. The only difference
			// between a DSS module and a DSS song is that the module includes
			// the samples at the end of the song data. So we calculate the
			// end of the song data and check if it is not the end of the
			// loaded file. If so, the file includes the samples and is playable
			moduleStream.Seek(0x59c, SeekOrigin.Begin);

			ushort numberOfPositions = moduleStream.Read_B_UINT16();
			if (numberOfPositions > 128)
				return AgentResult.Unknown;

			byte highestPatternNum = 0;

			for (int i = 0; i < numberOfPositions; i++)
				highestPatternNum = Math.Max(highestPatternNum, moduleStream.Read_UINT8());

			if ((((highestPatternNum + 1) * 1024) + 0x61e) < fileSize)
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

				// Skip mark
				moduleStream.Seek(8, SeekOrigin.Begin);

				startSongTempo = moduleStream.Read_UINT8();
				startSongSpeed = moduleStream.Read_UINT8();

				if (!ReadSampleInformation(moduleStream))
				{
					errorMessage = Resources.IDS_DSS_ERR_LOADING_SAMPLEINFO;
					return AgentResult.Error;
				}

				if (!ReadPositionInformation(moduleStream))
				{
					errorMessage = Resources.IDS_DSS_ERR_LOADING_HEADER;
					return AgentResult.Error;
				}

				if (!ReadPatterns(moduleStream))
				{
					errorMessage = Resources.IDS_DSS_ERR_LOADING_PATTERNS;
					return AgentResult.Error;
				}

				if (!ReadSamples(moduleStream))
				{
					errorMessage = Resources.IDS_DSS_ERR_LOADING_SAMPLES;
					return AgentResult.Error;
				}

				return AgentResult.Ok;
			}
			catch (Exception)
			{
				Cleanup();
				throw;
			}
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

			InitializeSound(0);

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
			playingInfo.SongSpeedCounter++;

			if (playingInfo.SongSpeedCounter == playingInfo.CurrentSongSpeed)
			{
				playingInfo.SongSpeedCounter = 0;
				playingInfo.NextRetrigTickNumber = 0;
				playingInfo.ArpeggioCounter = 3;

				PlayNextRow();
			}
			else
			{
				for (int i = 0; i < 4; i++)
				{
					VoiceInfo voiceInfo = voices[i];
					IChannel channel = VirtualChannels[i];

					if (voiceInfo.Period != 0)
						MakeEffects(voiceInfo, channel);
				}
			}

			if (endReached)
			{
				OnEndReached(playingInfo.CurrentPosition);
				endReached = false;

				MarkPositionAsVisited(playingInfo.CurrentPosition);
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
				foreach (Sample sample in samples)
				{
					// Build frequency table
					uint[] frequencies = new uint[10 * 12];

					for (int j = 0; j < 4 * 12; j++)
						frequencies[3 * 12 + j] = PeriodToFrequency(Tables.Periods[sample.FineTune, j]);

					SampleInfo sampleInfo = new SampleInfo
					{
						Name = sample.Name,
						Flags = SampleInfo.SampleFlag.None,
						Type = SampleInfo.SampleType.Sample,
						Volume = (ushort)(sample.Volume * 4),
						Sample = sample.Data,
						SampleOffset = sample.StartOffset,
						Length = sample.Length * 2U,
						Panning = -1,
						NoteFrequencies = frequencies
					};

					if (sample.LoopLength > 1)
					{
						sampleInfo.Length += sample.LoopLength * 2U;
						sampleInfo.LoopStart = sample.LoopStart;
						sampleInfo.LoopLength = sample.LoopLength * 2U;

						sampleInfo.Flags |= SampleInfo.SampleFlag.Loop;
					}
					else
					{
						sampleInfo.LoopStart = 0;
						sampleInfo.LoopLength = 0;
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
					description = Resources.IDS_DSS_INFODESCLINE0;
					value = positions.Length.ToString();
					break;
				}

				// Used patterns
				case 1:
				{
					description = Resources.IDS_DSS_INFODESCLINE1;
					value = patterns.Length.ToString();
					break;
				}

				// Supported / used samples
				case 2:
				{
					description = Resources.IDS_DSS_INFODESCLINE2;
					value = samples.Length.ToString();
					break;
				}

				// Playing position
				case 3:
				{
					description = Resources.IDS_DSS_INFODESCLINE3;
					value = FormatPosition();
					break;
				}

				// Playing pattern
				case 4:
				{
					description = Resources.IDS_DSS_INFODESCLINE4;
					value = FormatPattern();
					break;
				}

				// Current speed
				case 5:
				{
					description = Resources.IDS_DSS_INFODESCLINE5;
					value = FormatSpeed();
					break;
				}

				// Current tempo (BPM)
				case 6:
				{
					description = Resources.IDS_DSS_INFODESCLINE6;
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

		#region Duration calculation
		/********************************************************************/
		/// <summary>
		/// Initialize all internal structures when beginning duration
		/// calculation on a new sub-song
		/// </summary>
		/********************************************************************/
		protected override int InitDuration(int songNumber, int startPosition)
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
		/// Read sample instrument information
		/// </summary>
		/********************************************************************/
		private bool ReadSampleInformation(ModuleStream moduleStream)
		{
			Encoding encoder = EncoderCollection.Amiga;

			samples = new Sample[31];

			for (int i = 0; i < 31; i++)
			{
				Sample sample = new Sample();

				sample.Name = moduleStream.ReadString(encoder, 30);
				sample.StartOffset = moduleStream.Read_B_UINT32() & 0xfffffffe;
				sample.Length = moduleStream.Read_B_UINT16();
				sample.LoopStart = moduleStream.Read_B_UINT32();
				sample.LoopLength = moduleStream.Read_B_UINT16();
				sample.FineTune = moduleStream.Read_UINT8();
				sample.Volume = moduleStream.Read_UINT8();
				sample.Frequency = moduleStream.Read_B_UINT16();

				if (moduleStream.EndOfStream)
					return false;

				samples[i] = sample;
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
			ushort numberOfPositions = moduleStream.Read_B_UINT16();

			positions = new byte[numberOfPositions];
			moduleStream.ReadInto(positions, 0, numberOfPositions);

			if (moduleStream.EndOfStream)
				return false;

			moduleStream.Seek(128 - numberOfPositions, SeekOrigin.Current);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read pattern data
		/// </summary>
		/********************************************************************/
		private bool ReadPatterns(ModuleStream moduleStream)
		{
			int numberOfPatterns = positions.Max() + 1;
			patterns = new Pattern[numberOfPatterns];

			for (int i = 0; i < numberOfPatterns; i++)
			{
				Pattern pattern = new Pattern();

				for (int j = 0; j < 64; j++)
				{
					for (int k = 0; k < 4; k++)
					{
						TrackLine trackLine = new TrackLine();

						byte byt1 = moduleStream.Read_UINT8();
						byte byt2 = moduleStream.Read_UINT8();
						byte byt3 = moduleStream.Read_UINT8();
						byte byt4 = moduleStream.Read_UINT8();

						trackLine.Sample = (byte)(byt1 >> 3);
						trackLine.Period = (ushort)(((byt1 & 0x07) << 8) | byt2);
						trackLine.Effect = (Effect)byt3;
						trackLine.EffectArg = byt4;

						pattern.Tracks[k, j] = trackLine;
					}
				}

				if (moduleStream.EndOfStream)
					return false;

				patterns[i] = pattern;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read sample data
		/// </summary>
		/********************************************************************/
		private bool ReadSamples(ModuleStream moduleStream)
		{
			for (int i = 0; i < samples.Length; i++)
			{
				Sample sample = samples[i];
				uint length = sample.Length;

				if (length != 0)
				{
					if (sample.LoopLength > 1)
						length += sample.LoopLength;

					length *= 2;
					length += sample.StartOffset;

					sample.Data = moduleStream.ReadSampleData(i, (int)length, out int readBytes);

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
		private void InitializeSound(int startPosition)
		{
			playingInfo = new GlobalPlayingInfo
			{
				CurrentSongTempo = startSongTempo == 0 ? (byte)125 : startSongTempo,
				CurrentSongSpeed = startSongSpeed,
				SongSpeedCounter = 0,

				CurrentPosition = (ushort)startPosition,
				CurrentRow = 0,
				PositionJump = false,
				NewPosition = 0,

				SetLoopRow = false,
				LoopRow = 0,

				InverseMasterVolume = 0,
				NextRetrigTickNumber = 0,
				ArpeggioCounter = 3
			};

			endReached = false;

			voices = new VoiceInfo[4];

			for (int i = 0; i < 4; i++)
			{
				voices[i] = new VoiceInfo
				{
					Sample = 0,
					Period = 0,
					Effect = 0,
					EffectArg = 0,

					FineTune = 0,
					Volume = 0,

					PlayingSampleNumber = 0,
					SampleData = null,
					SampleStartOffset = 0,
					SampleLength = 0,
					LoopStart = 0,
					LoopLength = 0,

					PitchPeriod = 0,
					PortamentoDirection = false,
					PortamentoEndPeriod = 0,
					PortamentoSpeed = 0,

					UseTonePortamentoForSlideEffects = false,
					UseTonePortamentoForPortamentoEffects = false,

					LoopRow = 0,
					LoopCounter = 0
				};
			}

			SetBpmTempo(playingInfo.CurrentSongTempo);
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player has allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			samples = null;
			positions = null;
			patterns = null;
		}



		/********************************************************************/
		/// <summary>
		/// Will take the next row for all channels and parse it
		/// </summary>
		/********************************************************************/
		private void PlayNextRow()
		{
			playingInfo.NewPosition = playingInfo.CurrentPosition;

			Pattern pattern = patterns[positions[playingInfo.CurrentPosition]];
			short row = playingInfo.CurrentRow;

			for (int i = 0; i < 4; i++)
			{
				VoiceInfo voiceInfo = voices[i];
				IChannel channel = VirtualChannels[i];

				SetupInstrument(pattern.Tracks[i, row], voiceInfo, channel);
			}

			for (int i = 0; i < 4; i++)
			{
				VoiceInfo voiceInfo = voices[i];
				IChannel channel = VirtualChannels[i];

				if (voiceInfo.Effect != Effect.NoteDelay)
				{
					if (voiceInfo.LoopLength != 0)
						channel.SetLoop(voiceInfo.SampleData, voiceInfo.LoopStart, voiceInfo.LoopLength * 2U);
				}
			}

			if (playingInfo.SetLoopRow)
			{
				playingInfo.CurrentRow = playingInfo.LoopRow;
				playingInfo.SetLoopRow = false;
			}

			playingInfo.CurrentRow++;

			if ((playingInfo.CurrentRow >= 64) || playingInfo.PositionJump)
			{
				playingInfo.PositionJump = false;
				playingInfo.CurrentRow = 0;

				voices[0].LoopRow = -1;
				voices[1].LoopRow = -1;
				voices[2].LoopRow = -1;
				voices[3].LoopRow = -1;

				playingInfo.CurrentPosition = (ushort)(playingInfo.NewPosition + 1);

				if (playingInfo.CurrentPosition == positions.Length)
					playingInfo.CurrentPosition = 0;

				ShowSongPosition();
				ShowPattern();

				if (HasPositionBeenVisited(playingInfo.CurrentPosition))
					endReached = true;

				MarkPositionAsVisited(playingInfo.CurrentPosition);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Setup channel to play new instrument
		/// </summary>
		/********************************************************************/
		private void SetupInstrument(TrackLine trackLine, VoiceInfo voiceInfo, IChannel channel)
		{
			voiceInfo.Sample = trackLine.Sample;
			voiceInfo.Period = trackLine.Period;
			voiceInfo.Effect = trackLine.Effect;
			voiceInfo.EffectArg = trackLine.EffectArg;

			if (voiceInfo.Sample != 0)
			{
				Sample sample = samples[voiceInfo.Sample - 1];

				voiceInfo.PlayingSampleNumber = (byte)(voiceInfo.Sample - 1);
				voiceInfo.SampleData = sample.Data;
				voiceInfo.LoopStart = 0;
				voiceInfo.LoopLength = 0;

				if (voiceInfo.SampleData != null)
				{
					voiceInfo.SampleStartOffset = sample.StartOffset;
					voiceInfo.SampleLength = sample.Length;

					if (sample.LoopLength > 1)
					{
						voiceInfo.LoopStart = sample.StartOffset + sample.LoopStart;
						voiceInfo.LoopLength = sample.LoopLength;
					}

					voiceInfo.FineTune = sample.FineTune;
					voiceInfo.Volume = sample.Volume;
				}
			}

			ushort period = voiceInfo.Period;

			if ((voiceInfo.Sample != 0) && (period != 0))
			{
				if ((voiceInfo.Effect == Effect.Portamento) || (voiceInfo.Effect == Effect.PortamentoVolumeSlideUp) || (voiceInfo.Effect == Effect.PortamentoVolumeSlideDown))
				{
					SetupPortamento(period, voiceInfo);
					SetVolume(voiceInfo, channel);
					return;
				}

				if (period == 0x7ff)
				{
					channel.Mute();
					return;
				}

				if (voiceInfo.Effect == Effect.SetFineTune)
					DoEffectSetFineTune(voiceInfo);
				else if (voiceInfo.Effect == Effect.NoteDelay)
				{
					SetVolume(voiceInfo, channel);
					return;
				}
				else if (voiceInfo.Effect == Effect.SetSampleOffset)
					DoEffectSetSampleOffset(voiceInfo);

				if ((voiceInfo.SampleData != null) && (voiceInfo.SampleLength > 0))
				{
					channel.PlaySample(voiceInfo.PlayingSampleNumber, voiceInfo.SampleData, voiceInfo.SampleStartOffset, (uint)((voiceInfo.SampleLength + voiceInfo.LoopLength) * 2));

					period = AdjustFineTune(period, voiceInfo);
					voiceInfo.PitchPeriod = period;

					channel.SetAmigaPeriod(period);
				}
				else
					channel.Mute();
			}

			switch (voiceInfo.Effect)
			{
				case Effect.SetMasterVolume:
				{
					DoEffectSetMasterVolume(voiceInfo);
					break;
				}

				case Effect.VolumeUp:
				{
					DoEffectVolumeUp(voiceInfo, channel);
					break;
				}

				case Effect.VolumeDown:
				{
					DoEffectVolumeDown(voiceInfo, channel);
					break;
				}

				case Effect.MasterVolumeUp:
				{
					DoEffectMasterVolumeUp(voiceInfo);
					break;
				}

				case Effect.MasterVolumeDown:
				{
					DoEffectMasterVolumeDown(voiceInfo);
					break;
				}

				case Effect.SetVolume:
				{
					DoEffectSetVolume(voiceInfo, channel);
					return;
				}
			}

			if (period != 0)
				SetVolume(voiceInfo, channel);

			switch (voiceInfo.Effect)
			{
				case Effect.SetSongSpeed:
				{
					DoEffectSetSongSpeed(voiceInfo);
					break;
				}

				case Effect.PositionJump:
				{
					DoEffectPositionJump(voiceInfo);
					break;
				}

				case Effect.SetFilter:
				{
					DoEffectSetFilter(voiceInfo);
					break;
				}

				case Effect.PitchUp:
				{
					DoEffectPitchUp(voiceInfo, channel);
					break;
				}

				case Effect.PitchDown:
				{
					DoEffectPitchDown(voiceInfo, channel);
					break;
				}

				case Effect.PitchControl:
				{
					DoEffectPitchControl(voiceInfo);
					break;
				}

				case Effect.SetSongTempo:
				{
					DoEffectSetSongTempo(voiceInfo);
					break;
				}

				case Effect.SetLoopStart:
				{
					DoEffectSetLoopStart(voiceInfo);
					break;
				}

				case Effect.JumpToLoop:
				{
					DoEffectJumpToLoop(voiceInfo);
					break;
				}

				case Effect.PortamentoControl:
				{
					DoEffectPortamentoControl(voiceInfo);
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will apply real-time effects
		/// </summary>
		/********************************************************************/
		private void MakeEffects(VoiceInfo voiceInfo, IChannel channel)
		{
			if (voiceInfo.EffectArg != 0)
			{
				switch (voiceInfo.Effect)
				{
					case Effect.Arpeggio:
					{
						DoEffectArpeggio(voiceInfo, channel);
						break;
					}

					case Effect.SlideUp:
					{
						DoEffectSlideUp(voiceInfo, channel);
						break;
					}

					case Effect.SlideDown:
					{
						DoEffectSlideDown(voiceInfo, channel);
						break;
					}

					case Effect.VolumeSlideUp:
					{
						DoEffectVolumeSlideUp(voiceInfo, channel);
						break;
					}

					case Effect.VolumeSlideDown:
					{
						DoEffectVolumeSlideDown(voiceInfo, channel);
						break;
					}

					case Effect.MasterVolumeSlideUp:
					{
						DoEffectMasterVolumeSlideUp(voiceInfo);
						break;
					}

					case Effect.MasterVolumeSlideDown:
					{
						DoEffectMasterVolumeSlideDown(voiceInfo);
						break;
					}

					case Effect.RetrigNote:
					{
						DoEffectRetrigNote(voiceInfo, channel);
						break;
					}

					case Effect.NoteDelay:
					{
						DoEffectNoteDelay(voiceInfo, channel);
						break;
					}

					case Effect.NoteCut:
					{
						DoEffectNoteCut(voiceInfo, channel);
						break;
					}

					case Effect.PortamentoVolumeSlideUp:
					{
						DoEffectPortamentoVolumeSlideUp(voiceInfo, channel);
						break;
					}

					case Effect.PortamentoVolumeSlideDown:
					{
						DoEffectPortamentoVolumeSlideDown(voiceInfo, channel);
						break;
					}
				}
			}

			if (voiceInfo.Effect == Effect.Portamento)
				DoEffectPortamento(voiceInfo, channel);
		}



		/********************************************************************/
		/// <summary>
		/// Will set current volume for the channel
		/// </summary>
		/********************************************************************/
		private void SetVolume(VoiceInfo voiceInfo, IChannel channel)
		{
			int newVolume = voiceInfo.Volume - playingInfo.InverseMasterVolume;
			if (newVolume < 0)
				newVolume = 0;

			channel.SetAmigaVolume((ushort)newVolume);
		}



		/********************************************************************/
		/// <summary>
		/// Will set current volume on all channels
		/// </summary>
		/********************************************************************/
		private void SetVolumeOnAllChannels()
		{
			for (int i = 0; i < 4; i++)
				SetVolume(voices[i], VirtualChannels[i]);
		}



		/********************************************************************/
		/// <summary>
		/// Will add the given value to the volume for the channel
		/// </summary>
		/********************************************************************/
		private void AddVolume(byte volumeAdd, VoiceInfo voiceInfo, IChannel channel)
		{
			voiceInfo.Volume += volumeAdd;

			if (voiceInfo.Volume > 64)
				voiceInfo.Volume = 64;

			SetVolume(voiceInfo, channel);
		}



		/********************************************************************/
		/// <summary>
		/// Will subtract the given value from the volume for the channel
		/// </summary>
		/********************************************************************/
		private void SubVolume(byte volumeSub, VoiceInfo voiceInfo, IChannel channel)
		{
			int newVolume = voiceInfo.Volume - volumeSub;

			if (newVolume < 0)
				newVolume = 0;

			voiceInfo.Volume = (byte)newVolume;

			SetVolume(voiceInfo, channel);
		}



		/********************************************************************/
		/// <summary>
		/// Will add the given value to the master volume
		/// </summary>
		/********************************************************************/
		private void AddMasterVolume(byte volumeAdd)
		{
			int newVolume = playingInfo.InverseMasterVolume - volumeAdd;

			if (newVolume < 0)
				newVolume = 0;

			playingInfo.InverseMasterVolume = (ushort)newVolume;

			SetVolumeOnAllChannels();
		}



		/********************************************************************/
		/// <summary>
		/// Will subtract the given value from the master volume
		/// </summary>
		/********************************************************************/
		private void SubMasterVolume(byte volumeSub)
		{
			playingInfo.InverseMasterVolume += volumeSub;

			if (playingInfo.InverseMasterVolume > 64)
				playingInfo.InverseMasterVolume = 64;

			SetVolumeOnAllChannels();
		}



		/********************************************************************/
		/// <summary>
		/// Prepare portamento
		/// </summary>
		/********************************************************************/
		private void SetupPortamento(ushort period, VoiceInfo voiceInfo)
		{
			voiceInfo.PortamentoEndPeriod = AdjustFineTune(period, voiceInfo);
			voiceInfo.PortamentoDirection = false;

			if (voiceInfo.PortamentoEndPeriod == voiceInfo.PitchPeriod)
				voiceInfo.PortamentoEndPeriod = 0;
			else if (voiceInfo.PortamentoEndPeriod < voiceInfo.PitchPeriod)
				voiceInfo.PortamentoDirection = true;
		}



		/********************************************************************/
		/// <summary>
		/// Adjust given period for fine tune and return new period
		/// </summary>
		/********************************************************************/
		private ushort AdjustFineTune(ushort period, VoiceInfo voiceInfo)
		{
			if (voiceInfo.FineTune != 0)
			{
				int i;
				for (i = 0; i < 48; i++)
				{
					if (Tables.Periods[0, i] == period)
					{
						i++;
						break;
					}
				}

				period = Tables.Periods[voiceInfo.FineTune, i - 1];
			}

			return period;
		}



		/********************************************************************/
		/// <summary>
		/// Apply pitch period to the right channel
		/// </summary>
		/********************************************************************/
		private void ApplyPitchPeriod(VoiceInfo voiceInfo, IChannel channel)
		{
			ushort period = voiceInfo.PitchPeriod;

			if (voiceInfo.UseTonePortamentoForSlideEffects)
				period = AdjustForTonePortamento(period, voiceInfo);

			channel.SetAmigaPeriod(period);
		}



		/********************************************************************/
		/// <summary>
		/// Adjust given period to a note period
		/// </summary>
		/********************************************************************/
		private ushort AdjustForTonePortamento(ushort period, VoiceInfo voiceInfo)
		{
			int i;
			for (i = 0; i < 48; i++)
			{
				if (period >= Tables.Periods[voiceInfo.FineTune, i])
				{
					i++;
					break;
				}
			}

			return Tables.Periods[voiceInfo.FineTune, i - 1];
		}



		/********************************************************************/
		/// <summary>
		/// Play current sample
		/// </summary>
		/********************************************************************/
		private void PlaySample(VoiceInfo voiceInfo, IChannel channel)
		{
			if ((voiceInfo.SampleData != null) && (voiceInfo.SampleLength > 0))
			{
				channel.PlaySample(voiceInfo.PlayingSampleNumber, voiceInfo.SampleData, voiceInfo.SampleStartOffset, (uint)((voiceInfo.SampleLength + voiceInfo.LoopLength) * 2));

				if (voiceInfo.LoopLength != 0)
					channel.SetLoop(voiceInfo.LoopStart, voiceInfo.LoopLength * 2U);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Do the actual portamento
		/// </summary>
		/********************************************************************/
		private void DoPortamento(VoiceInfo voiceInfo, IChannel channel)
		{
			if (voiceInfo.PortamentoEndPeriod != 0)
			{
				if (voiceInfo.PortamentoDirection)
				{
					voiceInfo.PitchPeriod -= voiceInfo.PortamentoSpeed;

					if (voiceInfo.PitchPeriod <= voiceInfo.PortamentoEndPeriod)
					{
						voiceInfo.PitchPeriod = voiceInfo.PortamentoEndPeriod;
						voiceInfo.PortamentoEndPeriod = 0;
					}
				}
				else
				{
					voiceInfo.PitchPeriod += voiceInfo.PortamentoSpeed;

					if (voiceInfo.PitchPeriod >= voiceInfo.PortamentoEndPeriod)
					{
						voiceInfo.PitchPeriod = voiceInfo.PortamentoEndPeriod;
						voiceInfo.PortamentoEndPeriod = 0;
					}
				}

				ushort period = voiceInfo.PitchPeriod;

				if (voiceInfo.UseTonePortamentoForPortamentoEffects)
					period = AdjustForTonePortamento(period, voiceInfo);

				channel.SetAmigaPeriod(period);
			}
		}

		#region Effect handlers
		/********************************************************************/
		/// <summary>
		/// 0x00 - Arpeggio
		/// </summary>
		/********************************************************************/
		private void DoEffectArpeggio(VoiceInfo voiceInfo, IChannel channel)
		{
			int arpeggio = playingInfo.ArpeggioCounter - playingInfo.SongSpeedCounter;
			int arpeggioOffset;

			if (arpeggio == 0)
			{
				playingInfo.ArpeggioCounter += 3;

				channel.SetAmigaPeriod(voiceInfo.PitchPeriod);
				return;
			}

			if (arpeggio == 1)
				arpeggioOffset = voiceInfo.EffectArg & 0x0f;
			else
				arpeggioOffset = voiceInfo.EffectArg >> 4;

			for (int i = 0;; i++)
			{
				ushort period = Tables.Periods[voiceInfo.FineTune, i];

				if ((period == Tables.PeriodLimits[voiceInfo.FineTune, 1]) || (period == voiceInfo.PitchPeriod))
				{
					channel.SetAmigaPeriod(period);
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0x01 - Slide up
		/// </summary>
		/********************************************************************/
		private void DoEffectSlideUp(VoiceInfo voiceInfo, IChannel channel)
		{
			voiceInfo.PitchPeriod -= voiceInfo.EffectArg;

			if (voiceInfo.PitchPeriod < Tables.PeriodLimits[voiceInfo.FineTune, 1])
				voiceInfo.PitchPeriod = Tables.PeriodLimits[voiceInfo.FineTune, 1];

			ApplyPitchPeriod(voiceInfo, channel);
		}



		/********************************************************************/
		/// <summary>
		/// 0x02 - Slide down
		/// </summary>
		/********************************************************************/
		private void DoEffectSlideDown(VoiceInfo voiceInfo, IChannel channel)
		{
			voiceInfo.PitchPeriod += voiceInfo.EffectArg;

			if (voiceInfo.PitchPeriod > Tables.PeriodLimits[voiceInfo.FineTune, 0])
				voiceInfo.PitchPeriod = Tables.PeriodLimits[voiceInfo.FineTune, 0];

			ApplyPitchPeriod(voiceInfo, channel);
		}



		/********************************************************************/
		/// <summary>
		/// 0x03 - Set volume
		/// </summary>
		/********************************************************************/
		private void DoEffectSetVolume(VoiceInfo voiceInfo, IChannel channel)
		{
			voiceInfo.Volume = voiceInfo.EffectArg;

			SetVolume(voiceInfo, channel);
		}



		/********************************************************************/
		/// <summary>
		/// 0x04 - Set master volume
		/// </summary>
		/********************************************************************/
		private void DoEffectSetMasterVolume(VoiceInfo voiceInfo)
		{
			int newVolume = 64 - voiceInfo.EffectArg;
			if (newVolume >= 0)
				playingInfo.InverseMasterVolume = (ushort)newVolume;
		}



		/********************************************************************/
		/// <summary>
		/// 0x05 - Set song speed
		/// </summary>
		/********************************************************************/
		private void DoEffectSetSongSpeed(VoiceInfo voiceInfo)
		{
			if (voiceInfo.EffectArg != 0)
			{
				playingInfo.SongSpeedCounter = 0;
				playingInfo.NextRetrigTickNumber = 0;
				playingInfo.ArpeggioCounter = 3;
				playingInfo.CurrentSongSpeed = voiceInfo.EffectArg;

				ShowSpeed();
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0x06 - Position jump
		/// </summary>
		/********************************************************************/
		private void DoEffectPositionJump(VoiceInfo voiceInfo)
		{
			if ((voiceInfo.EffectArg == 0xff) || (voiceInfo.EffectArg == 0))
			{
				playingInfo.NewPosition = playingInfo.CurrentPosition;
				playingInfo.PositionJump = true;
			}
			else if (voiceInfo.EffectArg <= positions.Length)
			{
				playingInfo.NewPosition = (ushort)(voiceInfo.EffectArg - 2);
				playingInfo.PositionJump = true;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0x07 - Set filter
		/// </summary>
		/********************************************************************/
		private void DoEffectSetFilter(VoiceInfo voiceInfo)
		{
			AmigaFilter = voiceInfo.EffectArg != 0;
		}



		/********************************************************************/
		/// <summary>
		/// 0x08 - Pitch up
		/// </summary>
		/********************************************************************/
		private void DoEffectPitchUp(VoiceInfo voiceInfo, IChannel channel)
		{
			if (voiceInfo.EffectArg != 0)
			{
				bool oldValue = voiceInfo.UseTonePortamentoForSlideEffects;
				voiceInfo.UseTonePortamentoForSlideEffects = false;

				DoEffectSlideUp(voiceInfo, channel);

				voiceInfo.UseTonePortamentoForSlideEffects = oldValue;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0x09 - Pitch down
		/// </summary>
		/********************************************************************/
		private void DoEffectPitchDown(VoiceInfo voiceInfo, IChannel channel)
		{
			if (voiceInfo.EffectArg != 0)
			{
				bool oldValue = voiceInfo.UseTonePortamentoForSlideEffects;
				voiceInfo.UseTonePortamentoForSlideEffects = false;

				DoEffectSlideDown(voiceInfo, channel);

				voiceInfo.UseTonePortamentoForSlideEffects = oldValue;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0x0A - Pitch control
		/// </summary>
		/********************************************************************/
		private void DoEffectPitchControl(VoiceInfo voiceInfo)
		{
			voiceInfo.UseTonePortamentoForSlideEffects = voiceInfo.EffectArg != 0;
		}



		/********************************************************************/
		/// <summary>
		/// 0x0B - Set song tempo
		/// </summary>
		/********************************************************************/
		private void DoEffectSetSongTempo(VoiceInfo voiceInfo)
		{
			if (voiceInfo.EffectArg >= 28)
			{
				playingInfo.CurrentSongTempo = voiceInfo.EffectArg;
				SetBpmTempo(playingInfo.CurrentSongTempo);

				playingInfo.SongSpeedCounter = 0;
				playingInfo.NextRetrigTickNumber = 0;
				playingInfo.ArpeggioCounter = 3;

				ShowTempo();
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0x0C - Volume up
		/// </summary>
		/********************************************************************/
		private void DoEffectVolumeUp(VoiceInfo voiceInfo, IChannel channel)
		{
			if (voiceInfo.EffectArg != 0)
				AddVolume(voiceInfo.EffectArg, voiceInfo, channel);
		}



		/********************************************************************/
		/// <summary>
		/// 0x0D - Volume down
		/// </summary>
		/********************************************************************/
		private void DoEffectVolumeDown(VoiceInfo voiceInfo, IChannel channel)
		{
			if (voiceInfo.EffectArg != 0)
				SubVolume(voiceInfo.EffectArg, voiceInfo, channel);
		}



		/********************************************************************/
		/// <summary>
		/// 0x0E - Volume slide up
		/// </summary>
		/********************************************************************/
		private void DoEffectVolumeSlideUp(VoiceInfo voiceInfo, IChannel channel)
		{
			AddVolume(voiceInfo.EffectArg, voiceInfo, channel);
		}



		/********************************************************************/
		/// <summary>
		/// 0x0F - Volume slide down
		/// </summary>
		/********************************************************************/
		private void DoEffectVolumeSlideDown(VoiceInfo voiceInfo, IChannel channel)
		{
			SubVolume(voiceInfo.EffectArg, voiceInfo, channel);
		}



		/********************************************************************/
		/// <summary>
		/// 0x10 - Master volume up
		/// </summary>
		/********************************************************************/
		private void DoEffectMasterVolumeUp(VoiceInfo voiceInfo)
		{
			if (voiceInfo.EffectArg != 0)
				AddMasterVolume(voiceInfo.EffectArg);
		}



		/********************************************************************/
		/// <summary>
		/// 0x11 - Master volume down
		/// </summary>
		/********************************************************************/
		private void DoEffectMasterVolumeDown(VoiceInfo voiceInfo)
		{
			if (voiceInfo.EffectArg != 0)
				SubMasterVolume(voiceInfo.EffectArg);
		}



		/********************************************************************/
		/// <summary>
		/// 0x12 - Master volume slide up
		/// </summary>
		/********************************************************************/
		private void DoEffectMasterVolumeSlideUp(VoiceInfo voiceInfo)
		{
			AddMasterVolume(voiceInfo.EffectArg);
		}



		/********************************************************************/
		/// <summary>
		/// 0x13 - Master volume slide down
		/// </summary>
		/********************************************************************/
		private void DoEffectMasterVolumeSlideDown(VoiceInfo voiceInfo)
		{
			SubMasterVolume(voiceInfo.EffectArg);
		}



		/********************************************************************/
		/// <summary>
		/// 0x14 - Set loop start
		/// </summary>
		/********************************************************************/
		private void DoEffectSetLoopStart(VoiceInfo voiceInfo)
		{
			if (voiceInfo.EffectArg != 0)
				voiceInfo.LoopRow = playingInfo.CurrentRow;
		}



		/********************************************************************/
		/// <summary>
		/// 0x15 - Jump to loop
		/// </summary>
		/********************************************************************/
		private void DoEffectJumpToLoop(VoiceInfo voiceInfo)
		{
			if (voiceInfo.EffectArg == 0)
				voiceInfo.LoopRow = -1;
			else if (voiceInfo.LoopRow >= 0)
			{
				if (voiceInfo.LoopCounter == 0)
					voiceInfo.LoopCounter = voiceInfo.EffectArg;
				else
				{
					voiceInfo.LoopCounter--;
					if (voiceInfo.LoopCounter == 0)
					{
						voiceInfo.LoopRow = -1;
						return;
					}
				}

				playingInfo.LoopRow = (short)(voiceInfo.LoopRow - 1);
				playingInfo.SetLoopRow = true;

				voiceInfo.LoopRow = -1;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0x16 - Retrig note
		/// </summary>
		/********************************************************************/
		private void DoEffectRetrigNote(VoiceInfo voiceInfo, IChannel channel)
		{
			if (voiceInfo.EffectArg < playingInfo.CurrentSongSpeed)
			{
				if (playingInfo.NextRetrigTickNumber == 0)
					playingInfo.NextRetrigTickNumber = voiceInfo.EffectArg;

				if (playingInfo.SongSpeedCounter == playingInfo.NextRetrigTickNumber)
				{
					playingInfo.NextRetrigTickNumber += voiceInfo.EffectArg;

					PlaySample(voiceInfo, channel);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0x17 - Note delay
		/// </summary>
		/********************************************************************/
		private void DoEffectNoteDelay(VoiceInfo voiceInfo, IChannel channel)
		{
			if (voiceInfo.EffectArg < playingInfo.CurrentSongSpeed)
			{
				if (voiceInfo.EffectArg == playingInfo.SongSpeedCounter)
				{
					if (voiceInfo.Sample != 0)
					{
						if (voiceInfo.Period != 0)
						{
							ushort period = AdjustFineTune(voiceInfo.Period, voiceInfo);

							voiceInfo.PitchPeriod = period;
							channel.SetAmigaPeriod(period);
						}

						PlaySample(voiceInfo, channel);
					}
				}
			}
			else
			{
				voiceInfo.Effect = 0;
				voiceInfo.EffectArg = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0x18 - Note cut
		/// </summary>
		/********************************************************************/
		private void DoEffectNoteCut(VoiceInfo voiceInfo, IChannel channel)
		{
			if (voiceInfo.EffectArg < playingInfo.CurrentSongSpeed)
			{
				if (voiceInfo.EffectArg == playingInfo.SongSpeedCounter)
				{
					voiceInfo.Volume = 0;
					channel.Mute();
				}
			}
			else
			{
				voiceInfo.Effect = 0;
				voiceInfo.EffectArg = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0x19 - Set sample offset
		/// </summary>
		/********************************************************************/
		private void DoEffectSetSampleOffset(VoiceInfo voiceInfo)
		{
			if (voiceInfo.EffectArg != 0)
				voiceInfo.SampleOffset = (ushort)(voiceInfo.EffectArg << 7);

			if (voiceInfo.SampleOffset != 0)
			{
				if (voiceInfo.SampleOffset >= voiceInfo.SampleLength)
					voiceInfo.SampleLength = 0;
				else
				{
					voiceInfo.SampleLength -= voiceInfo.SampleOffset;
					voiceInfo.SampleStartOffset += voiceInfo.SampleOffset * 2U;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0x1A - Set fine tune
		/// </summary>
		/********************************************************************/
		private void DoEffectSetFineTune(VoiceInfo voiceInfo)
		{
			voiceInfo.FineTune = (byte)(voiceInfo.EffectArg & 0x0f);
		}



		/********************************************************************/
		/// <summary>
		/// 0x1B - Portamento
		/// </summary>
		/********************************************************************/
		private void DoEffectPortamento(VoiceInfo voiceInfo, IChannel channel)
		{
			if (voiceInfo.EffectArg != 0)
			{
				voiceInfo.PortamentoSpeed = voiceInfo.EffectArg;
				voiceInfo.EffectArg = 0;
			}

			DoPortamento(voiceInfo, channel);
		}



		/********************************************************************/
		/// <summary>
		/// 0x1C - Portamento + volume slide up
		/// </summary>
		/********************************************************************/
		private void DoEffectPortamentoVolumeSlideUp(VoiceInfo voiceInfo, IChannel channel)
		{
			DoPortamento(voiceInfo, channel);
			DoEffectVolumeSlideUp(voiceInfo, channel);
		}



		/********************************************************************/
		/// <summary>
		/// 0x1D - Portamento + volume slide down
		/// </summary>
		/********************************************************************/
		private void DoEffectPortamentoVolumeSlideDown(VoiceInfo voiceInfo, IChannel channel)
		{
			DoPortamento(voiceInfo, channel);
			DoEffectVolumeSlideDown(voiceInfo, channel);
		}



		/********************************************************************/
		/// <summary>
		/// 0x1E - Portamento control
		/// </summary>
		/********************************************************************/
		private void DoEffectPortamentoControl(VoiceInfo voiceInfo)
		{
			voiceInfo.UseTonePortamentoForPortamentoEffects = voiceInfo.EffectArg != 0;
		}
		#endregion


		/********************************************************************/
		/// <summary>
		/// Will update the module information with current song position
		/// </summary>
		/********************************************************************/
		private void ShowSongPosition()
		{
			OnModuleInfoChanged(InfoPositionLine, FormatPosition());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with pattern number
		/// </summary>
		/********************************************************************/
		private void ShowPattern()
		{
			OnModuleInfoChanged(InfoPatternLine, FormatPattern());
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
		/// Will update the module information with tempo
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
			ShowPattern();
			ShowSpeed();
			ShowTempo();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the playing position
		/// </summary>
		/********************************************************************/
		private string FormatPosition()
		{
			return playingInfo.CurrentPosition.ToString();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the playing pattern
		/// </summary>
		/********************************************************************/
		private string FormatPattern()
		{
			return positions[playingInfo.CurrentPosition].ToString();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the current speed
		/// </summary>
		/********************************************************************/
		private string FormatSpeed()
		{
			return playingInfo.CurrentSongSpeed.ToString();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the current tempo
		/// </summary>
		/********************************************************************/
		private string FormatTempo()
		{
			return playingInfo.CurrentSongTempo.ToString();
		}
		#endregion
	}
}
