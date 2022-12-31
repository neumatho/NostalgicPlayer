/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.QuadraComposer.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.QuadraComposer
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class QuadraComposerWorker : ModulePlayerAgentBase
	{
		private string songName;
		private string composer;

		private byte startTempo;

		private byte numberOfSamples;
		private Sample[] samples;

		private byte numberOfPatterns;
		private Pattern[] patterns;

		private byte numberOfPositions;
		private byte[] positionList;

		private TrackLine[,] currentPattern;
		private ushort currentPosition;
		private ushort newPosition;
		private ushort breakRow;
		private ushort newRow;
		private ushort rowCount;
		private ushort loopRow;
		private byte patternWait;

		private ushort tempo;
		private ushort speed;
		private ushort speedCount;

		private bool newPositionFlag;
		private bool jumpBreakFlag;
		private byte loopCount;
		private bool introRow;

		private bool setTempo;

		private ChannelInfo[] channels;

		private const int InfoSpeedLine = 3;
		private const int InfoTempoLine = 4;

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => new [] { "emod" };



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			if (moduleStream.Length < 64)
				return AgentResult.Unknown;

			// Check chunk names
			moduleStream.Seek(0, SeekOrigin.Begin);

			uint chunk = moduleStream.Read_B_UINT32();
			if (chunk != 0x464f524d)				// FORM
				return AgentResult.Unknown;

			moduleStream.Seek(8, SeekOrigin.Begin);

			ulong bigChunk = moduleStream.Read_B_UINT64();
			if (bigChunk != 0x454d4f44454d4943)		// EMODEMIC
				return AgentResult.Unknown;

			// Check version
			moduleStream.Seek(20, SeekOrigin.Begin);

			if (moduleStream.Read_B_UINT16() != 1)
				return AgentResult.Unknown;

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Return the name of the module
		/// </summary>
		/********************************************************************/
		public override string ModuleName => songName;



		/********************************************************************/
		/// <summary>
		/// Return the name of the author
		/// </summary>
		/********************************************************************/
		public override string Author => composer;



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
				// Song length
				case 0:
				{
					description = Resources.IDS_EMOD_INFODESCLINE0;
					value = numberOfPositions.ToString();
					break;
				}

				// Used patterns
				case 1:
				{
					description = Resources.IDS_EMOD_INFODESCLINE1;
					value = numberOfPatterns.ToString();
					break;
				}

				// Supported / used samples
				case 2:
				{
					description = Resources.IDS_EMOD_INFODESCLINE2;
					value = numberOfSamples.ToString();
					break;
				}

				// Current speed
				case 3:
				{
					description = Resources.IDS_EMOD_INFODESCLINE3;
					value = speed.ToString();
					break;
				}

				// BPM
				case 4:
				{
					description = Resources.IDS_EMOD_INFODESCLINE4;
					value = tempo.ToString();
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
		public override ModulePlayerSupportFlag SupportFlags => ModulePlayerSupportFlag.SetPosition;



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

				// Load the different chunks
				moduleStream.Seek(12, SeekOrigin.Begin);

				for (;;)
				{
					// Read the chunk name and length
					uint chunkName = moduleStream.Read_B_UINT32();
					uint chunkSize = moduleStream.Read_B_UINT32();

					// Do we have any chunks left?
					if (moduleStream.EndOfStream)
						break;			// No, stop the loading

					if ((chunkSize > (moduleStream.Length - moduleStream.Position)))
					{
						errorMessage = Resources.IDS_EMOD_ERR_LOADING_HEADER;
						return AgentResult.Error;
					}

					switch (chunkName)
					{
						// Extended module info (EMIC)
						case 0x454d4943:
						{
							ParseEmic(moduleStream, out errorMessage);
							break;
						}

						// Pattern data (PATT)
						case 0x50415454:
						{
							ParsePatt(moduleStream, out errorMessage);
							break;
						}

						// Sample data (8SMP)
						case 0x38534d50:
						{
							Parse8Smp(moduleStream, out errorMessage);
							break;
						}

						// Unknown chunks
						default:
						{
							moduleStream.Seek(chunkSize, SeekOrigin.Current);
							break;
						}
					}

					if (!string.IsNullOrEmpty(errorMessage))
					{
						Cleanup();
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
		public override bool InitSound(int songNumber, DurationInfo durationInfo, out string errorMessage)
		{
			if (!base.InitSound(songNumber, durationInfo, out errorMessage))
				return false;

			InitializeSound(durationInfo.StartPosition);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the duration for all sub-songs
		/// </summary>
		/********************************************************************/
		public override DurationInfo[] CalculateDuration()
		{
			return CalculateDurationBySongPosition();
		}



		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public override void Play()
		{
			ChangeTempoIfNeeded();

			speedCount++;
			if (speedCount < speed)
				RunTickEffects();
			else
			{
				if (patternWait != 0)
				{
					patternWait--;
					speedCount = 0;

					RunTickEffects();
				}
				else
					GetNotes();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the length of the current song
		/// </summary>
		/********************************************************************/
		public override int SongLength => numberOfPositions;



		/********************************************************************/
		/// <summary>
		/// Return the current position of the song
		/// </summary>
		/********************************************************************/
		public override int GetSongPosition()
		{
			return currentPosition;
		}



		/********************************************************************/
		/// <summary>
		/// Set a new position of the song
		/// </summary>
		/********************************************************************/
		public override void SetSongPosition(int position, PositionInfo positionInfo)
		{
			// Change the position
			currentPosition = (ushort)position;

			Pattern pattern = patterns[positionList[currentPosition]];
			currentPattern = pattern.Tracks;
			breakRow = pattern.NumberOfRows;

			rowCount = 0;
			newRow = 0;

			// Change the speed
			ChangeSpeed(positionInfo.Speed);

			tempo = positionInfo.Bpm;
			setTempo = true;

			base.SetSongPosition(position, positionInfo);
		}



		/********************************************************************/
		/// <summary>
		/// Returns all the samples available in the module. If none, null
		/// is returned
		/// </summary>
		/********************************************************************/
		public override SampleInfo[] Samples
		{
			get
			{
				List<SampleInfo> result = new List<SampleInfo>();

				foreach (Sample sample in samples)
				{
					if (sample != null)
					{
						// Build frequency table
						uint[] frequencies = new uint[10 * 12];

						for (int j = 0; j < 3 * 12; j++)
							frequencies[3 * 12 + j] = 3546895U / Tables.Periods[sample.FineTune, j];

						SampleInfo sampleInfo = new SampleInfo
						{
							Name = sample.Name,
							Type = SampleInfo.SampleType.Sample,
							BitSize = 8,
							MiddleC = frequencies[3 * 12 + 12],
							Volume = (ushort)(sample.Volume * 4),
							Panning = -1,
							Sample = sample.Data,
							Length = sample.Length,
							NoteFrequencies = frequencies
						};

						if ((sample.ControlByte & SampleControlFlag.Loop) != 0)
						{
							// Sample loops
							sampleInfo.Flags = SampleInfo.SampleFlags.Loop;
							sampleInfo.LoopStart = sample.LoopStart;
							sampleInfo.LoopLength = sample.LoopLength;
						}
						else
						{
							// No loop
							sampleInfo.Flags = SampleInfo.SampleFlags.None;
							sampleInfo.LoopStart = 0;
							sampleInfo.LoopLength = 0;
						}

						result.Add(sampleInfo);
					}
				}

				return result.ToArray();
			}
		}
		#endregion

		#region Duration calculation methods
		/********************************************************************/
		/// <summary>
		/// Initialize all internal structures when beginning duration
		/// calculation on a new sub-song
		/// </summary>
		/********************************************************************/
		protected override int InitDurationCalculationByStartPos(int startPosition)
		{
			InitializeSound(startPosition);

			return startPosition;
		}



		/********************************************************************/
		/// <summary>
		/// Return the current speed
		/// </summary>
		/********************************************************************/
		protected override byte GetCurrentSpeed()
		{
			return (byte)speed;
		}



		/********************************************************************/
		/// <summary>
		/// Return the current BPM
		/// </summary>
		/********************************************************************/
		protected override ushort GetCurrentBpm()
		{
			return tempo;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Initialize sound structures
		/// </summary>
		/********************************************************************/
		private void InitializeSound(int startPosition)
		{
			// Initialize work variables
			currentPosition = (ushort)startPosition;
			
			Pattern pattern = patterns[positionList[currentPosition]];
			currentPattern = pattern.Tracks;
			breakRow = pattern.NumberOfRows;
			newRow = 0;
			rowCount = 0;

			tempo = startTempo;
			speed = 6;
			speedCount = speed;
			setTempo = true;

			newPositionFlag = false;
			jumpBreakFlag = false;
			introRow = true;

			// Initialize channel structure
			channels = Helpers.InitializeArray<ChannelInfo>(4);

			MarkPositionAsVisited(startPosition);
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player has allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			samples = null;
			patterns = null;
			positionList = null;
		}



		/********************************************************************/
		/// <summary>
		/// Parses the EMIC chunk
		/// </summary>
		/********************************************************************/
		private void ParseEmic(ModuleStream moduleStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			Encoding encoder = EncoderCollection.Amiga;
			byte[] buffer = new byte[21];

			// Skip version
			moduleStream.Seek(2, SeekOrigin.Current);

			// Read name of song
			moduleStream.ReadString(buffer, 20);
			songName = encoder.GetString(buffer);

			// Read composer
			moduleStream.ReadString(buffer, 20);
			composer = encoder.GetString(buffer);

			// Read tempo
			startTempo = moduleStream.Read_UINT8();

			// Read sample information
			numberOfSamples = moduleStream.Read_UINT8();

			samples = new Sample[256];

			for (int i = 0; i < numberOfSamples; i++)
			{
				Sample sample = new Sample();

				byte number = moduleStream.Read_UINT8();

				sample.Volume = moduleStream.Read_UINT8();
				sample.Length = moduleStream.Read_B_UINT16() * 2U;

				moduleStream.ReadString(buffer, 20);
				sample.Name = encoder.GetString(buffer);

				sample.ControlByte = (SampleControlFlag)moduleStream.Read_UINT8();
				sample.FineTune = (byte)(moduleStream.Read_UINT8() & 0x0f);
				sample.LoopStart = moduleStream.Read_B_UINT16() * 2U;
				sample.LoopLength = moduleStream.Read_B_UINT16() * 2U;

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_EMOD_ERR_LOADING_HEADER;
					return;
				}

				// Skip offset
				moduleStream.Seek(4, SeekOrigin.Current);

				samples[number - 1] = sample;
			}

			// Read pattern information
			moduleStream.Seek(1, SeekOrigin.Current);
			numberOfPatterns = moduleStream.Read_UINT8();

			patterns = new Pattern[256];

			for (int i = 0; i < numberOfPatterns; i++)
			{
				Pattern pattern = new Pattern();

				byte number = moduleStream.Read_UINT8();

				pattern.NumberOfRows = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_EMOD_ERR_LOADING_HEADER;
					return;
				}

				// Skip name and offset
				moduleStream.Seek(24, SeekOrigin.Current);

				patterns[number] = pattern;
			}

			// Read position list
			numberOfPositions = moduleStream.Read_UINT8();

			positionList = new byte[numberOfPositions];

			moduleStream.Read(positionList, 0, numberOfPositions);
			if (moduleStream.EndOfStream)
			{
				errorMessage = Resources.IDS_EMOD_ERR_LOADING_HEADER;
				return;
			}

			if ((moduleStream.Position % 2) != 0)
				moduleStream.Seek(1, SeekOrigin.Current);
		}



		/********************************************************************/
		/// <summary>
		/// Parses the PATT chunk
		/// </summary>
		/********************************************************************/
		private void ParsePatt(ModuleStream moduleStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			foreach (Pattern pattern in patterns.Where(p => p != null))
			{
				pattern.Tracks = new TrackLine[4, pattern.NumberOfRows + 1];

				for (int j = 0; j <= pattern.NumberOfRows; j++)
				{
					for (int k = 0; k < 4; k++)
					{
						byte byt1 = moduleStream.Read_UINT8();
						sbyte byt2 = moduleStream.Read_INT8();
						byte byt3 = moduleStream.Read_UINT8();
						byte byt4 = moduleStream.Read_UINT8();

						pattern.Tracks[k, j] = new TrackLine
						{
							Sample = byt1,
							Note = byt2,
							Effect = (Effect)(byt3 & 0x0f),
							EffectArg = byt4
						};
					}
				}

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_EMOD_ERR_LOADING_PATTERNS;
					return;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parses the 8SMP chunk
		/// </summary>
		/********************************************************************/
		private void Parse8Smp(ModuleStream moduleStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			for (int i = 0; i < samples.Length; i++)
			{
				Sample sample = samples[i];

				if ((sample == null) || (sample.Length == 0))
					continue;

				sample.Data = new sbyte[sample.Length];

				moduleStream.ReadSampleData(i, sample.Data, (int)sample.Length);

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_EMOD_ERR_LOADING_SAMPLES;
					return;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will check if a tempo changed is needed and if so, do it
		/// </summary>
		/********************************************************************/
		private void ChangeTempoIfNeeded()
		{
			if (setTempo)
			{
				setTempo = false;
				SetBpmTempo(tempo);

				// Change the module info
				OnModuleInfoChanged(InfoTempoLine, tempo.ToString());
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will change the speed on the module
		/// </summary>
		/********************************************************************/
		private void ChangeSpeed(ushort newSpeed)
		{
			if (newSpeed != speed)
			{
				// Change the module info
				OnModuleInfoChanged(InfoSpeedLine, newSpeed.ToString());

				// Remember the speed
				speed = newSpeed;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse next row
		/// </summary>
		/********************************************************************/
		private void GetNotes()
		{
			if (!introRow)
			{
				ChangeTempoIfNeeded();

				if (newPositionFlag)
				{
					newPositionFlag = false;

					currentPosition = newPosition;
					InitializeNewPosition();
				}
				else
				{
					if (jumpBreakFlag)
					{
						jumpBreakFlag = false;

						if (loopRow <= breakRow)
							rowCount = loopRow;
					}
					else
					{
						rowCount++;

						if (rowCount > breakRow)
						{
							currentPosition++;
							InitializeNewPosition();
						}
					}
				}
			}

			introRow = false;
			speedCount = 0;

			for (int i = 0; i < 4; i++)
				PlayNote(channels[i], VirtualChannels[i], currentPattern[i, rowCount]);
		}



		/********************************************************************/
		/// <summary>
		/// Initialize position variables to new position
		/// </summary>
		/********************************************************************/
		private void InitializeNewPosition()
		{
			if (currentPosition >= numberOfPositions)
				currentPosition = 0;

			if (HasPositionBeenVisited(currentPosition))
			{
				if (currentDurationInfo == null)
				{
					speed = 6;
					tempo = startTempo;
				}
				else
				{
					PositionInfo positionInfo = currentDurationInfo.PositionInfo[currentPosition];

					tempo = positionInfo.Bpm;
					setTempo = true;

					ChangeSpeed(positionInfo.Speed);
					ChangeSubSong(positionInfo.SubSong);
				}

				// Tell NostalgicPlayer that the module has ended
				OnEndReached();
			}

			// Tell NostalgicPlayer we have changed the song position
			OnPositionChanged();

			MarkPositionAsVisited(currentPosition);

			Pattern pattern = patterns[positionList[currentPosition]];
			currentPattern = pattern.Tracks;
			breakRow = pattern.NumberOfRows;

			rowCount = newRow;
			newRow = 0;

			if (breakRow < rowCount)
				rowCount = breakRow;
		}



		/********************************************************************/
		/// <summary>
		/// Play a new note on a single channel
		/// </summary>
		/********************************************************************/
		private void PlayNote(ChannelInfo channelInfo, IChannel channel, TrackLine trackLine)
		{
			channelInfo.TrackLine = trackLine;

			if (trackLine.Sample != 0)
			{
				Sample sample = samples[trackLine.Sample - 1];
				if (sample == null)
					return;

				channelInfo.Volume = sample.Volume;
				channelInfo.Length = sample.Length;
				channelInfo.FineTune = sample.FineTune;
				channelInfo.SampleData = sample.Data;
				channelInfo.Start = 0;

				channel.SetAmigaVolume(channelInfo.Volume);

				if ((sample.ControlByte & SampleControlFlag.Loop) != 0)
				{
					channelInfo.Loop = sample.LoopStart;
					channelInfo.Length = sample.LoopStart + sample.LoopLength;
					channelInfo.LoopLength = sample.LoopLength;
				}
				else
				{
					channelInfo.Loop = 0;
					channelInfo.LoopLength = 0;
				}
			}

			if (trackLine.Note >= 0)
			{
				channelInfo.NoteNr = trackLine.Note;

				if ((trackLine.Effect == Effect.ExtraEffects) && ((trackLine.EffectArg & 0xf0) == 0x50))
					channelInfo.FineTune = (byte)(trackLine.EffectArg & 0x0f);
				else if ((trackLine.Effect == Effect.TonePortamento) || (trackLine.Effect == Effect.TonePortamentoAndVolumeSlide))
				{
					SetTonePortamento(channelInfo);
					return;
				}

				channelInfo.Period = Tables.Periods[channelInfo.FineTune, channelInfo.NoteNr];

				if ((trackLine.Effect == Effect.ExtraEffects) && ((trackLine.EffectArg & 0xf0) == 0xd0))
				{
					DoNoteDelay(channelInfo, channel);
					return;
				}

				channel.PlaySample(trackLine.Sample, channelInfo.SampleData, channelInfo.Start, channelInfo.Length);
				channel.SetAmigaPeriod(channelInfo.Period);

				if (channelInfo.LoopLength != 0)
					channel.SetLoop(channelInfo.Loop, channelInfo.LoopLength);
			}

			PlayAfterPeriodEffect(channelInfo, channel);
		}



		/********************************************************************/
		/// <summary>
		/// Run every tick effects
		/// </summary>
		/********************************************************************/
		private void RunTickEffects()
		{
			for (int i = 0; i < 4; i++)
				PlayTickEffect(channels[i], VirtualChannels[i]);
		}



		/********************************************************************/
		/// <summary>
		/// Run the current active effect on a single channel
		/// </summary>
		/********************************************************************/
		private void PlayTickEffect(ChannelInfo channelInfo, IChannel channel)
		{
			switch (channelInfo.TrackLine.Effect)
			{
				case Effect.Arpeggio:
				{
					DoArpeggio(channelInfo, channel);
					break;
				}

				case Effect.SlideUp:
				{
					DoSlideUp(channelInfo, channel);
					break;
				}

				case Effect.SlideDown:
				{
					DoSlideDown(channelInfo, channel);
					break;
				}

				case Effect.TonePortamento:
				{
					DoTonePortamento(channelInfo, channel);
					break;
				}

				case Effect.Vibrato:
				{
					DoVibrato(channelInfo, channel);
					break;
				}

				case Effect.TonePortamentoAndVolumeSlide:
				{
					DoTonePortamentoAndVolumeSlide(channelInfo, channel);
					break;
				}

				case Effect.VibratoAndVolumeSlide:
				{
					DoVibratoAndVolumeSlide(channelInfo, channel);
					break;
				}

				case Effect.Tremolo:
				{
					DoTremolo(channelInfo, channel);
					break;
				}

				case Effect.VolumeSlide:
				{
					DoVolumeSlide(channelInfo, channel);
					break;
				}

				case Effect.ExtraEffects:
				{
					PlayTickExtraEffect(channelInfo, channel);
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Run the current active extra effect on a single channel
		/// </summary>
		/********************************************************************/
		private void PlayTickExtraEffect(ChannelInfo channelInfo, IChannel channel)
		{
			switch ((ExtraEffect)(channelInfo.TrackLine.EffectArg & 0xf0))
			{
				case ExtraEffect.RetrigNote:
				{
					DoRetrigNote(channelInfo, channel);
					break;
				}

				case ExtraEffect.NoteCut:
				{
					DoNoteCut(channelInfo, channel);
					break;
				}

				case ExtraEffect.NoteDelay:
				{
					DoNoteDelay(channelInfo, channel);
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Run the effect on a single channel
		/// </summary>
		/********************************************************************/
		private void PlayAfterPeriodEffect(ChannelInfo channelInfo, IChannel channel)
		{
			switch (channelInfo.TrackLine.Effect)
			{
				case Effect.Arpeggio:
				{
					DoArpeggio(channelInfo, channel);
					break;
				}

				case Effect.SetSampleOffset:
				{
					DoSetSampleOffset(channelInfo, channel);
					break;
				}

				case Effect.PositionJump:
				{
					DoPositionJump(channelInfo);
					break;
				}

				case Effect.SetVolume:
				{
					DoSetVolume(channelInfo, channel);
					break;
				}

				case Effect.PatternBreak:
				{
					DoPatternBreak(channelInfo);
					break;
				}

				case Effect.ExtraEffects:
				{
					PlayAfterPeriodExtraEffect(channelInfo, channel);
					break;
				}

				case Effect.SetSpeed:
				{
					DoSetSpeed(channelInfo);
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Run the extra effect on a single channel
		/// </summary>
		/********************************************************************/
		private void PlayAfterPeriodExtraEffect(ChannelInfo channelInfo, IChannel channel)
		{
			switch ((ExtraEffect)(channelInfo.TrackLine.EffectArg & 0xf0))
			{
				case ExtraEffect.SetFilter:
				{
					DoSetFilter(channelInfo);
					break;
				}

				case ExtraEffect.FineSlideUp:
				{
					DoFineSlideUp(channelInfo, channel);
					break;
				}

				case ExtraEffect.FineSlideDown:
				{
					DoFineSlideDown(channelInfo, channel);
					break;
				}

				case ExtraEffect.SetGlissando:
				{
					DoSetGlissando(channelInfo);
					break;
				}

				case ExtraEffect.SetVibratoWaveform:
				{
					DoSetVibratoWaveform(channelInfo);
					break;
				}

				case ExtraEffect.SetFineTune:
				{
					DoSetFineTune(channelInfo);
					break;
				}

				case ExtraEffect.PatternLoop:
				{
					DoPatternLoop(channelInfo);
					break;
				}

				case ExtraEffect.SetTremoloWaveform:
				{
					DoSetTremoloWaveform(channelInfo);
					break;
				}

				case ExtraEffect.RetrigNote:
				{
					DoInitRetrig(channelInfo, channel);
					break;
				}

				case ExtraEffect.FineVolumeSlideUp:
				{
					DoFineVolumeSlideUp(channelInfo, channel);
					break;
				}

				case ExtraEffect.FineVolumeSlideDown:
				{
					DoFineVolumeSlideDown(channelInfo, channel);
					break;
				}

				case ExtraEffect.NoteCut:
				{
					DoNoteCut(channelInfo, channel);
					break;
				}

				case ExtraEffect.NoteDelay:
				{
					DoNoteDelay(channelInfo, channel);
					break;
				}

				case ExtraEffect.PatternDelay:
				{
					DoPatternDelay(channelInfo);
					break;
				}
			}
		}

		#region Methods to all the normal effects
		/********************************************************************/
		/// <summary>
		/// 0x00 - Plays arpeggio or normal note
		/// </summary>
		/********************************************************************/
		private void DoArpeggio(ChannelInfo channelInfo, IChannel channel)
		{
			if (channelInfo.TrackLine.EffectArg != 0)
			{
				sbyte offset = Tables.ArpeggioOffsets[speedCount % 3];

				if (offset < 0)
					channel.SetAmigaPeriod(channelInfo.Period);
				else
				{
					ushort note;

					if (offset == 0)
						note = (ushort)((channelInfo.TrackLine.EffectArg >> 4) + channelInfo.NoteNr);
					else
					{
						note = (ushort)((channelInfo.TrackLine.EffectArg & 0x0f) + channelInfo.NoteNr);
					}

					if (note > 35)
						note = 35;

					channel.SetAmigaPeriod(Tables.Periods[channelInfo.FineTune, note]);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0x01 - Slides the frequency up
		/// </summary>
		/********************************************************************/
		private void DoSlideUp(ChannelInfo channelInfo, IChannel channel)
		{
			channelInfo.Period -= channelInfo.TrackLine.EffectArg;
			if (channelInfo.Period < 113)
				channelInfo.Period = 113;

			channel.SetAmigaPeriod(channelInfo.Period);
		}



		/********************************************************************/
		/// <summary>
		/// 0x02 - Slides the frequency down
		/// </summary>
		/********************************************************************/
		private void DoSlideDown(ChannelInfo channelInfo, IChannel channel)
		{
			channelInfo.Period += channelInfo.TrackLine.EffectArg;
			if (channelInfo.Period > 856)
				channelInfo.Period = 856;

			channel.SetAmigaPeriod(channelInfo.Period);
		}



		/********************************************************************/
		/// <summary>
		/// Initialize tone portamento
		/// </summary>
		/********************************************************************/
		private void SetTonePortamento(ChannelInfo channelInfo)
		{
			channelInfo.WantedPeriod = Tables.Periods[channelInfo.FineTune, channelInfo.NoteNr];
			if (channelInfo.WantedPeriod > channelInfo.Period)
				channelInfo.PortDirection = true;
			else
				channelInfo.PortDirection = false;
		}



		/********************************************************************/
		/// <summary>
		/// 0x03 - Slides the frequency to the current note
		/// </summary>
		/********************************************************************/
		private void DoTonePortamento(ChannelInfo channelInfo, IChannel channel)
		{
			if (channelInfo.WantedPeriod == 0)
				return;

			if (channelInfo.TrackLine.EffectArg != 0)
				channelInfo.PortSpeed = channelInfo.TrackLine.EffectArg;

			DoActualTonePortamento(channelInfo, channel);
		}



		/********************************************************************/
		/// <summary>
		/// Do the actual tone portamento
		/// </summary>
		/********************************************************************/
		private void DoActualTonePortamento(ChannelInfo channelInfo, IChannel channel)
		{
			ushort portSpeed = channelInfo.PortSpeed;

			if (channelInfo.PortDirection)
			{
				channelInfo.Period += portSpeed;

				if (channelInfo.Period >= channelInfo.WantedPeriod)
				{
					channelInfo.Period = channelInfo.WantedPeriod;
					channelInfo.WantedPeriod = 0;

					channel.SetAmigaPeriod(channelInfo.Period);
					return;
				}
			}
			else
			{
				short newPeriod = (short)(channelInfo.Period - portSpeed);

				if (newPeriod <= channelInfo.WantedPeriod)
				{
					channelInfo.Period = channelInfo.WantedPeriod;
					channelInfo.WantedPeriod = 0;

					channel.SetAmigaPeriod(channelInfo.Period);
					return;
				}

				channelInfo.Period = (ushort)newPeriod;
			}

			if (channelInfo.GlissandoControl != 0)
			{
				for (int i = 0; ; i++)
				{
					if (channelInfo.Period >= Tables.Periods[channelInfo.FineTune, i])
					{
						channel.SetAmigaPeriod(Tables.Periods[channelInfo.FineTune, i]);
						break;
					}
				}
			}
			else
				channel.SetAmigaPeriod(channelInfo.Period);
		}



		/********************************************************************/
		/// <summary>
		/// 0x04 - Vibrates the frequency
		/// </summary>
		/********************************************************************/
		private void DoVibrato(ChannelInfo channelInfo, IChannel channel)
		{
			byte arg = channelInfo.TrackLine.EffectArg;
			if (arg != 0)
			{
				if ((arg & 0x0f) != 0)
					channelInfo.VibratoCommand = (byte)((channelInfo.VibratoCommand & 0xf0) | (arg & 0x0f));

				if ((arg & 0xf0) != 0)
					channelInfo.VibratoCommand = (byte)((channelInfo.VibratoCommand & 0x0f) | (arg & 0xf0));
			}

			DoActualVibrato(channelInfo, channel);
		}



		/********************************************************************/
		/// <summary>
		/// Do the actual vibrato
		/// </summary>
		/********************************************************************/
		private void DoActualVibrato(ChannelInfo channelInfo, IChannel channel)
		{
			channelInfo.VibratoPosition += (ushort)(channelInfo.VibratoCommand >> 4);
			channelInfo.VibratoPosition &= 0x3f;

			short vibVal = Tables.Vibrato[channelInfo.VibratoWave, channelInfo.VibratoPosition];
			short newPeriod = (short)(channelInfo.Period + (((channelInfo.VibratoCommand & 0x0f) * vibVal) >> 14));

			if (newPeriod > 856)
				newPeriod = 856;
			else if (newPeriod < 113)
				newPeriod = 113;

			channel.SetAmigaPeriod((ushort)newPeriod);
		}



		/********************************************************************/
		/// <summary>
		/// 0x05 - Is both effect 0x03 and 0x0a
		/// </summary>
		/********************************************************************/
		private void DoTonePortamentoAndVolumeSlide(ChannelInfo channelInfo, IChannel channel)
		{
			if (channelInfo.WantedPeriod != 0)
				DoActualTonePortamento(channelInfo, channel);

			DoVolumeSlide(channelInfo, channel);
		}



		/********************************************************************/
		/// <summary>
		/// 0x06 - Is both effect 0x04 and 0x0a
		/// </summary>
		/********************************************************************/
		private void DoVibratoAndVolumeSlide(ChannelInfo channelInfo, IChannel channel)
		{
			DoActualVibrato(channelInfo, channel);
			DoVolumeSlide(channelInfo, channel);
		}



		/********************************************************************/
		/// <summary>
		/// 0x07 - Makes vibrato on the volume
		/// </summary>
		/********************************************************************/
		private void DoTremolo(ChannelInfo channelInfo, IChannel channel)
		{
			byte arg = channelInfo.TrackLine.EffectArg;
			if (arg != 0)
			{
				if ((arg & 0x0f) != 0)
					channelInfo.TremoloCommand = (byte)((channelInfo.TremoloCommand & 0xf0) | (arg & 0x0f));

				if ((arg & 0xf0) != 0)
					channelInfo.TremoloCommand = (byte)((channelInfo.TremoloCommand & 0x0f) | (arg & 0xf0));
			}

			channelInfo.TremoloPosition += (ushort)(channelInfo.TremoloCommand >> 4);
			channelInfo.TremoloPosition &= 0x3f;

			short vibVal = Tables.Vibrato[channelInfo.TremoloWave, channelInfo.TremoloPosition];
			short newVolume = (short)(channelInfo.Volume + (((channelInfo.TremoloCommand & 0x0f) * vibVal) >> 14));

			if (newVolume > 64)
				newVolume = 64;
			else if (newVolume < 0)
				newVolume = 0;

			channel.SetAmigaVolume((ushort)newVolume);
		}



		/********************************************************************/
		/// <summary>
		/// 0x09 - Starts the sample somewhere else, but the start
		/// </summary>
		/********************************************************************/
		private void DoSetSampleOffset(ChannelInfo channelInfo, IChannel channel)
		{
			if (channelInfo.TrackLine.EffectArg != 0)
				channelInfo.SampleOffset = channelInfo.TrackLine.EffectArg;

			// Calculate the offset
			uint offset = channelInfo.SampleOffset * 256U * 2;		// Multiply by two, because we have length in bytes, while the original player have length in words

			if (offset < channelInfo.Length)
				channelInfo.Start = offset;
			else
			{
				channelInfo.Start = channelInfo.Loop;
				channelInfo.Length = channelInfo.LoopLength;
			}

			if (channelInfo.Length > 0)
				channel.PlaySample(channelInfo.TrackLine.Sample, channelInfo.SampleData, channelInfo.Start, channelInfo.Length);
			else
				channel.Mute();
		}



		/********************************************************************/
		/// <summary>
		/// 0x0A - Slides the volume
		/// </summary>
		/********************************************************************/
		private void DoVolumeSlide(ChannelInfo channelInfo, IChannel channel)
		{
			short newVolume = (short)channelInfo.Volume;

			byte volumeSpeed = (byte)(channelInfo.TrackLine.EffectArg >> 4);
			if (volumeSpeed != 0)
			{
				newVolume += volumeSpeed;
				if (newVolume > 64)
					newVolume = 64;
			}
			else
			{
				newVolume -= (short)(channelInfo.TrackLine.EffectArg & 0x0f);
				if (newVolume < 0)
					newVolume = 0;
			}

			channelInfo.Volume = (ushort)newVolume;
			channel.SetAmigaVolume(channelInfo.Volume);
		}



		/********************************************************************/
		/// <summary>
		/// 0x0B - Jumps to another position
		/// </summary>
		/********************************************************************/
		private void DoPositionJump(ChannelInfo channelInfo)
		{
			newPosition = channelInfo.TrackLine.EffectArg;
			newPositionFlag = true;
			newRow = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 0x0C - Sets the channel volume
		/// </summary>
		/********************************************************************/
		private void DoSetVolume(ChannelInfo channelInfo, IChannel channel)
		{
			channelInfo.Volume = channelInfo.TrackLine.EffectArg;
			if (channelInfo.Volume > 64)
				channelInfo.Volume = 64;

			channel.SetAmigaVolume(channelInfo.Volume);
		}



		/********************************************************************/
		/// <summary>
		/// 0x0D - Breaks the pattern and jump to the next position
		/// </summary>
		/********************************************************************/
		private void DoPatternBreak(ChannelInfo channelInfo)
		{
			newPosition = (ushort)(currentPosition + 1);
			newRow = channelInfo.TrackLine.EffectArg;
			newPositionFlag = true;
		}



		/********************************************************************/
		/// <summary>
		/// 0x0F - Changes the speed of the module
		/// </summary>
		/********************************************************************/
		private void DoSetSpeed(ChannelInfo channelInfo)
		{
			if (channelInfo.TrackLine.EffectArg > 31)
			{
				tempo = channelInfo.TrackLine.EffectArg;
				setTempo = true;
			}
			else
			{
				ushort newSpeed = channelInfo.TrackLine.EffectArg;
				if (newSpeed == 0)
					newSpeed = 1;

				ChangeSpeed(newSpeed);
				speedCount = 0;
			}
		}
		#endregion

		#region Methods to all the extended effects
		/********************************************************************/
		/// <summary>
		/// 0xE0 - Changes the filter
		/// </summary>
		/********************************************************************/
		private void DoSetFilter(ChannelInfo channelInfo)
		{
			AmigaFilter = (channelInfo.TrackLine.EffectArg & 0x01) == 0;
		}



		/********************************************************************/
		/// <summary>
		/// 0xE1 - Fine slide the frequency up
		/// </summary>
		/********************************************************************/
		private void DoFineSlideUp(ChannelInfo channelInfo, IChannel channel)
		{
			channelInfo.Period -= (ushort)(channelInfo.TrackLine.EffectArg & 0x0f);
			if (channelInfo.Period < 113)
				channelInfo.Period = 113;

			channel.SetAmigaPeriod(channelInfo.Period);
		}



		/********************************************************************/
		/// <summary>
		/// 0xE2 - Fine slide the frequency down
		/// </summary>
		/********************************************************************/
		private void DoFineSlideDown(ChannelInfo channelInfo, IChannel channel)
		{
			channelInfo.Period += (ushort)(channelInfo.TrackLine.EffectArg & 0x0f);
			if (channelInfo.Period > 856)
				channelInfo.Period = 856;

			channel.SetAmigaPeriod(channelInfo.Period);
		}



		/********************************************************************/
		/// <summary>
		/// 0xE3 - Sets a new glissando control
		/// </summary>
		/********************************************************************/
		private void DoSetGlissando(ChannelInfo channelInfo)
		{
			channelInfo.GlissandoControl = (byte)(channelInfo.TrackLine.EffectArg & 0x0f);
		}



		/********************************************************************/
		/// <summary>
		/// 0xE4 - Sets a new vibrato waveform
		/// </summary>
		/********************************************************************/
		private void DoSetVibratoWaveform(ChannelInfo channelInfo)
		{
			channelInfo.VibratoWave = (byte)(channelInfo.TrackLine.EffectArg & 0x0f);
		}



		/********************************************************************/
		/// <summary>
		/// 0xE5 - Changes the fine tune
		/// </summary>
		/********************************************************************/
		private void DoSetFineTune(ChannelInfo channelInfo)
		{
			channelInfo.FineTune = (byte)(channelInfo.TrackLine.EffectArg & 0x0f);
		}



		/********************************************************************/
		/// <summary>
		/// 0xE6 - Jump to pattern loop position
		/// </summary>
		/********************************************************************/
		private void DoPatternLoop(ChannelInfo channelInfo)
		{
			byte arg = (byte)(channelInfo.TrackLine.EffectArg & 0x0f);

			if (arg == 0)
				loopRow = rowCount;
			else
			{
				if (loopCount == 0)
				{
					loopCount = arg;
					jumpBreakFlag = true;
				}
				else
				{
					loopCount--;
					if (loopCount != 0)
						jumpBreakFlag = true;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0xE7 - Sets a new tremolo waveform
		/// </summary>
		/********************************************************************/
		private void DoSetTremoloWaveform(ChannelInfo channelInfo)
		{
			channelInfo.TremoloWave = (byte)(channelInfo.TrackLine.EffectArg & 0x0f);
		}



		/********************************************************************/
		/// <summary>
		/// 0xE9 - Retrigs the current note (initialize)
		/// </summary>
		/********************************************************************/
		private void DoInitRetrig(ChannelInfo channelInfo, IChannel channel)
		{
			channelInfo.Retrig = 0;
			DoRetrigNote(channelInfo, channel);
		}



		/********************************************************************/
		/// <summary>
		/// 0xE9 - Retrigs the current note
		/// </summary>
		/********************************************************************/
		private void DoRetrigNote(ChannelInfo channelInfo, IChannel channel)
		{
			byte arg = (byte)(channelInfo.TrackLine.EffectArg & 0x0f);

			channelInfo.Retrig++;
			if (channelInfo.Retrig >= arg)
			{
				channelInfo.Retrig = 0;

				channel.PlaySample(channelInfo.TrackLine.Sample, channelInfo.SampleData, channelInfo.Start, channelInfo.Length);
				channel.SetAmigaPeriod(channelInfo.Period);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0xEA - Fine slide the volume up
		/// </summary>
		/********************************************************************/
		private void DoFineVolumeSlideUp(ChannelInfo channelInfo, IChannel channel)
		{
			channelInfo.Volume += (ushort)(channelInfo.TrackLine.EffectArg & 0x0f);
			if (channelInfo.Volume > 64)
				channelInfo.Volume = 64;

			channel.SetAmigaVolume(channelInfo.Volume);
		}



		/********************************************************************/
		/// <summary>
		/// 0xEB - Fine slide the volume down
		/// </summary>
		/********************************************************************/
		private void DoFineVolumeSlideDown(ChannelInfo channelInfo, IChannel channel)
		{
			short newVolume = (short)(channelInfo.Volume - (channelInfo.TrackLine.EffectArg & 0x0f));
			if (newVolume < 0)
				newVolume = 0;

			channelInfo.Volume = (ushort)newVolume;
			channel.SetAmigaVolume(channelInfo.Volume);
		}



		/********************************************************************/
		/// <summary>
		/// 0xEC - Stops the current note from playing
		/// </summary>
		/********************************************************************/
		private void DoNoteCut(ChannelInfo channelInfo, IChannel channel)
		{
			if ((channelInfo.TrackLine.EffectArg & 0x0f) <= speedCount)
			{
				channelInfo.Volume = 0;
				channel.SetVolume(0);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0xED - Waits a little while before playing
		/// </summary>
		/********************************************************************/
		private void DoNoteDelay(ChannelInfo channelInfo, IChannel channel)
		{
			if (channelInfo.NoteNr >= 0)
			{
				if ((channelInfo.TrackLine.EffectArg & 0x0f) == speedCount)
				{
					channel.PlaySample(channelInfo.TrackLine.Sample, channelInfo.SampleData, channelInfo.Start, channelInfo.Length);
					channel.SetAmigaPeriod(channelInfo.Period);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0xEE - Pauses the pattern for a little while
		/// </summary>
		/********************************************************************/
		private void DoPatternDelay(ChannelInfo channelInfo)
		{
			patternWait = (byte)(channelInfo.TrackLine.EffectArg & 0x0f);
		}
		#endregion

		#endregion
	}
}
