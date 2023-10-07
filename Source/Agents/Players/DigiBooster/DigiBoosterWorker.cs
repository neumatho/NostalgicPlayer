using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.DigiBooster.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Types;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.DigiBooster
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class DigiBoosterWorker : ModulePlayerWithPositionDurationAgentBase
	{
		private byte version;
		private byte numberOfChannels;
		private byte numberOfPatterns;
		private string songName;

		private byte songLength;
		private byte[] orders;

		private Sample[] samples;
		private Pattern[] patterns;

		private GlobalPlayingInfo playingInfo;
		private ChannelInfo[] channels;

		private bool endReached;

		private static readonly ChannelPanningType[] panPos =
		{
			ChannelPanningType.Left, ChannelPanningType.Left, ChannelPanningType.Right, ChannelPanningType.Right,
			ChannelPanningType.Right, ChannelPanningType.Right, ChannelPanningType.Left, ChannelPanningType.Left
		};

		private const int InfoPositionLine = 3;
		private const int InfoPatternLine = 4;
		private const int InfoSpeedLine = 5;
		private const int InfoTempoLine = 6;

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => new [] { "digi" };



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			if (moduleStream.Length < 1572)
				return AgentResult.Unknown;

			// Check the mark
			moduleStream.Seek(0, SeekOrigin.Begin);

			byte[] buf = new byte[20];
			moduleStream.Read(buf, 0, 20);

			if (Encoding.ASCII.GetString(buf, 0, 20) != "DIGI Booster module\0")
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
					description = Resources.IDS_DIGI_INFODESCLINE0;
					value = songLength.ToString();
					break;
				}

				// Used patterns
				case 1:
				{
					description = Resources.IDS_DIGI_INFODESCLINE1;
					value = numberOfPatterns.ToString();
					break;
				}

				// Supported / used samples
				case 2:
				{
					description = Resources.IDS_DIGI_INFODESCLINE2;
					value = "31";
					break;
				}

				// Playing position
				case 3:
				{
					description = Resources.IDS_DIGI_INFODESCLINE3;
					value = playingInfo.SongPosition.ToString();
					break;
				}

				// Playing pattern
				case 4:
				{
					description = Resources.IDS_DIGI_INFODESCLINE4;
					value = orders[playingInfo.SongPosition].ToString();
					break;
				}

				// Current speed
				case 5:
				{
					description = Resources.IDS_DIGI_INFODESCLINE5;
					value = playingInfo.Tempo.ToString();
					break;
				}

				// Current tempo (BPM)
				case 6:
				{
					description = Resources.IDS_DIGI_INFODESCLINE6;
					value = playingInfo.CiaTempo.ToString();
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

				// Read the header
				moduleStream.Seek(24, SeekOrigin.Begin);

				version = moduleStream.Read_UINT8();
				numberOfChannels = moduleStream.Read_UINT8();
				bool packed = moduleStream.Read_UINT8() != 0;

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_DIGI_ERR_LOADING_HEADER;
					return AgentResult.Error;
				}

				moduleStream.Seek(46, SeekOrigin.Begin);

				numberOfPatterns = (byte)(moduleStream.Read_UINT8() + 1);
				songLength = (byte)(moduleStream.Read_UINT8() + 1);

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_DIGI_ERR_LOADING_HEADER;
					return AgentResult.Error;
				}

				// Read the orders
				orders = new byte[128];
				moduleStream.Read(orders, 0, 128);

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_DIGI_ERR_LOADING_HEADER;
					return AgentResult.Error;
				}

				// Read sample information
				ReadSampleInformation(moduleStream);

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_DIGI_ERR_LOADING_SAMPLEINFO;
					return AgentResult.Error;
				}

				// Read song name
				Encoding encoder = EncoderCollection.Amiga;

				songName = moduleStream.ReadString(encoder, 32);

				// Read sample names
				for (int i = 0; i < 31; i++)
					samples[i].Name = moduleStream.ReadString(encoder, 30);

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_DIGI_ERR_LOADING_SAMPLEINFO;
					return AgentResult.Error;
				}

				// Read pattern data
				patterns = new Pattern[numberOfPatterns];
				bool loadFailed;

				if (packed)
					loadFailed = ReadPackedPatternData(moduleStream);
				else
					loadFailed = ReadUnpackedPatternData(moduleStream);

				if (loadFailed)
				{
					errorMessage = Resources.IDS_DIGI_ERR_LOADING_PATTERNS;
					return AgentResult.Error;
				}

				// Read sample data
				for (int i = 0; i < 31; i++)
				{
					if (samples[i].Length != 0)
					{
						samples[i].SampleData = moduleStream.ReadSampleData(i, (int)samples[i].Length, out int readBytes);

						if (readBytes != samples[i].Length)
						{
							errorMessage = Resources.IDS_DIGI_ERR_LOADING_SAMPLES;
							return AgentResult.Error;
						}
					}
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
			byte currentSongPosition = playingInfo.SongPosition;

			if (playingInfo.Count >= playingInfo.Tempo)
			{
				if ((playingInfo.Tempo != 0) && (playingInfo.PatternPosition == 64))
				{
					playingInfo.PatternPosition = 0;
					playingInfo.SongPosition++;
				}

				if (playingInfo.SongPosition >= songLength)
				{
					playingInfo.SongPosition = 0;
					playingInfo.PatternPosition = 0;
				}

				if (playingInfo.Tempo == 0)
				{
					playingInfo.SongPosition = 0;
					playingInfo.PatternPosition = 0;
					playingInfo.Tempo = 6;

					for (int i = 0; i < numberOfChannels; i++)
					{
						ChannelInfo channelInfo = channels[i];

						channelInfo.LoopPatternPosition = 0;
						channelInfo.LoopSongPosition = 0;
						channelInfo.LoopHowMany = 0;
					}
				}

				TrackLine[,] rows = patterns[orders[playingInfo.SongPosition]].Rows;

				for (int i = 0; i < 8; i++)
					playingInfo.CurrentRow[i] = rows[i, playingInfo.PatternPosition];
			}

			for (int i = 0; i < numberOfChannels; i++)
				PlayVoice(channels[i], VirtualChannels[i]);

			for (int i = 0; i < numberOfChannels; i++)
				ParseVoice(channels[i], playingInfo.CurrentRow[i]);

			if (playingInfo.PauseVbl > 0)
			{
				playingInfo.PauseEnabled = true;
				playingInfo.PauseVbl--;
			}

			if (playingInfo.Count >= playingInfo.Tempo)
			{
				playingInfo.Count = 0;

				if (playingInfo.PauseVbl == 0)
				{
					playingInfo.PatternPosition++;
					playingInfo.PauseEnabled = false;
				}
			}

			playingInfo.Count++;

			if (currentSongPosition != playingInfo.SongPosition)
			{
				ShowSongPosition();
				ShowPattern();

				if (HasPositionBeenVisited(playingInfo.SongPosition) && channels.All(x => x.LoopHowMany == 0))
					endReached = true;

				MarkPositionAsVisited(playingInfo.SongPosition);
			}

			// Have we reached the end of the module
			if (endReached)
			{
				OnEndReached(playingInfo.SongPosition);
				endReached = false;

				MarkPositionAsVisited(playingInfo.SongPosition);
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
				for (int i = 0; i < 31; i++)
				{
					Sample sample = samples[i];

					// Build frequency table
					uint[] frequencies = new uint[10 * 12];

					for (int j = 0; j < 3 * 12; j++)
					{
						ushort period;

						if ((sample.FineTune >= -8) && (sample.FineTune <= 7))
							period = Tables.Periods[sample.FineTune + 8, j];
						else
						{
							period = Tables.Periods[8, j];
							period = (ushort)(period - ((period * sample.FineTune) / 140));
						}

						frequencies[4 * 12 + j] = 3546895U / period;
					}

					yield return new SampleInfo
					{
						Name = sample.Name,
						Type = SampleInfo.SampleType.Sample,
						BitSize = SampleInfo.SampleSize._8Bit,
						Volume = (ushort)(sample.Volume * 4),
						Panning = -1,
						Sample = sample.SampleData,
						Length = sample.Length,
						LoopStart = sample.LoopStart,
						LoopLength = sample.LoopLength,
						NoteFrequencies = frequencies,
						Flags = sample.LoopLength != 0 ? SampleInfo.SampleFlag.Loop : SampleInfo.SampleFlag.None
					};
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
			return new Snapshot(playingInfo, channels);
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
			Snapshot clonedSnapshot = new Snapshot(currentSnapshot.PlayingInfo, currentSnapshot.Channels);

			playingInfo = clonedSnapshot.PlayingInfo;
			channels = clonedSnapshot.Channels;

			UpdateModuleInformation();

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Read the sample information
		/// </summary>
		/********************************************************************/
		private void ReadSampleInformation(ModuleStream moduleStream)
		{
			uint[] lengths = new uint[31];
			uint[] loopStarts = new uint[31];
			uint[] loopLengths = new uint[31];
			byte[] volumes = new byte[31];
			sbyte[] fineTunes = new sbyte[31];

			moduleStream.ReadArray_B_UINT32s(lengths, 0, 31);
			moduleStream.ReadArray_B_UINT32s(loopStarts, 0, 31);
			moduleStream.ReadArray_B_UINT32s(loopLengths, 0, 31);
			moduleStream.Read(volumes, 0, 31);
			moduleStream.ReadSigned(fineTunes, 0, 31);

			if ((version >= 0x10) && (version <= 0x13))
				Array.Clear(fineTunes);

			samples = new Sample[31];

			for (int i = 0; i < 31; i++)
			{
				uint length = lengths[i];
				uint loopStart = loopStarts[i];
				uint loopLength = loopLengths[i];

				if ((loopStart > length) || (loopLength == 0))
				{
					loopStart = 0;
					loopLength = 0;
				}
				else if ((loopStart + loopLength) >= length)
					loopLength = length - loopStart;

				samples[i] = new Sample
				{
					Length = length,
					LoopStart = loopStart,
					LoopLength = loopLength,
					Volume = volumes[i],
					FineTune = fineTunes[i]
				};
			}
		}



		/********************************************************************/
		/// <summary>
		/// Read all the pattern data as compressed
		/// </summary>
		/********************************************************************/
		private bool ReadPackedPatternData(ModuleStream moduleStream)
		{
			for (int i = 0; i < numberOfPatterns; i++)
			{
				Pattern pattern = new Pattern();

				int length = moduleStream.Read_B_UINT16();

				byte[] bitMasks = new byte[64];
				moduleStream.Read(bitMasks, 0, 64);
				length -= 64;

				for (int j = 0; j < 64; j++)
				{
					byte mask = bitMasks[j];

					for (int k = 0x80, m = 0; k > 0; k >>= 1, m++)
					{
						uint data = 0;

						if ((mask & k) != 0)
						{
							data = moduleStream.Read_B_UINT32();
							length -= 4;
						}

						pattern.Rows[m, j] = ParseTrackLine(data);
					}
				}

				if ((moduleStream.EndOfStream) || (length != 0))
					return true;

				patterns[i] = pattern;
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Read all the pattern data as plain data
		/// </summary>
		/********************************************************************/
		private bool ReadUnpackedPatternData(ModuleStream moduleStream)
		{
			for (int i = 0; i < numberOfPatterns; i++)
			{
				Pattern pattern = new Pattern();

				for (int j = 0; j < 4; j++)
				{
					for (int k = 0; k < 64; k++)
					{
						uint data = moduleStream.Read_B_UINT32();
						pattern.Rows[j, k] = ParseTrackLine(data);
					}
				}

				for (int j = 4; j < 8; j++)
				{
					for (int k = 0; k < 64; k++)
					{
						uint data = moduleStream.Read_B_UINT32();
						pattern.Rows[j, k] = ParseTrackLine(data);
					}
				}

				if (moduleStream.EndOfStream)
					return true;

				patterns[i] = pattern;
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player have allocated
		/// </summary>
		/********************************************************************/
		private TrackLine ParseTrackLine(uint data)
		{
			return new TrackLine
			{
				Period = (ushort)((data >> 16) & 0x0fff),
				SampleNumber = (byte)(((data >> 24) & 0xf0) | ((data >> 12) & 0x0f)),
				Effect = (Effect)((data >> 8) & 0x0f),
				EffectArg = (byte)(data & 0xff)
			};
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
				CiaTempo = 125,
				Tempo = 6,
				Count = 6,

				SongPosition = (byte)startPosition,
				PatternPosition = 0,

				CurrentRow = new TrackLine[8],

				PauseVbl = 0,
				PauseEnabled = false
			};

			endReached = false;

			channels = ArrayHelper.InitializeArray<ChannelInfo>(numberOfChannels);

			for (int i = 0; i < numberOfChannels; i++)
				VirtualChannels[i].SetPanning((ushort)panPos[i]);

			SetBpmTempo(playingInfo.CiaTempo);
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player have allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			orders = null;
			samples = null;
			patterns = null;

			playingInfo = null;
			channels = null;
		}



		/********************************************************************/
		/// <summary>
		/// Play the different voices with the right samples
		/// </summary>
		/********************************************************************/
		private void PlayVoice(ChannelInfo channelInfo, IChannel channel)
		{
			if (channelInfo.MainPeriod != 0)
			{
				bool retrig = false;

				if (channelInfo.OffEnable)
				{
					channelInfo.OffEnable = false;
					retrig = true;

					if (channelInfo.PlayPointer)
					{
						if ((channelInfo.LastRoundInfo.Effect == Effect.Glissando) || (channelInfo.LastRoundInfo.Effect == Effect.GlissandoVolumeSlide))
							retrig = false;
					}
				}

				if (channelInfo.MainPeriod == -1)
				{
					channel.Mute();

					channelInfo.PlayPointer = true;
					channelInfo.MainPeriod = 0;
				}
				else
				{
					channel.SetAmigaPeriod((uint)channelInfo.MainPeriod);

					byte vol = channelInfo.MainVolume;
					if (vol > 64)
						vol = 64;

					channel.SetAmigaVolume(vol);

					if (channelInfo.RobotEnable)
					{
						if (retrig)
							channel.PlaySample((short)(channelInfo.OldSampleNumber - 1), channelInfo.RobotBuffers[0], 0, (uint)channelInfo.RobotBytesToPlay);

						channel.SetLoop(channelInfo.RobotBuffers[0], 0, (uint)channelInfo.RobotBytesToPlay);

						(channelInfo.RobotBuffers[0], channelInfo.RobotBuffers[1]) = (channelInfo.RobotBuffers[1], channelInfo.RobotBuffers[0]);
					}
					else
					{
						if (retrig)
						{
							Sample sample = samples[channelInfo.OldSampleNumber - 1];

							if (channelInfo.BackwardEnabled != 0)
							{
								channel.PlaySample((short)(channelInfo.OldSampleNumber - 1), channelInfo.SampleData, 0, sample.Length, backwards: true);

								if ((channelInfo.BackwardEnabled == 2) && (sample.LoopLength > 0))
									channel.SetLoop(sample.LoopStart, sample.LoopLength);
							}
							else
							{
								channel.PlaySample((short)(channelInfo.OldSampleNumber - 1), channelInfo.SampleData, channelInfo.StartOffset, sample.Length);

								if (sample.LoopLength > 0)
									channel.SetLoop(sample.LoopStart, sample.LoopLength);
							}
						}
					}

					channelInfo.PlayPointer = true;
				}
			}

			channelInfo.IsActive = channel.IsActive;
		}



		/********************************************************************/
		/// <summary>
		/// Parses the next row of pattern data and prepare the voice
		/// </summary>
		/********************************************************************/
		private void ParseVoice(ChannelInfo channelInfo, TrackLine trackLine)
		{
			channelInfo.Volume = channelInfo.OldVolume;

			if ((playingInfo.Tempo != 0) && (playingInfo.Count >= playingInfo.Tempo))
			{
				ParseRowData(channelInfo, trackLine);
				DoTriggerEffects(channelInfo);
			}

			if ((playingInfo.Tempo - 1) == playingInfo.Count)
			{
				Effect effect = channelInfo.LastRoundInfo.Effect;

				if (effect == Effect.GlissandoVolumeSlide)
				{
					channelInfo.LastRoundInfo.Effect = Effect.Glissando;
					channelInfo.LastRoundInfo.EffectArg = 0;
				}
				else
				{
					if (effect == Effect.Extra)
					{
						ExtraEffect extraEffect = (ExtraEffect)(channelInfo.LastRoundInfo.EffectArg & 0xf0);

						if ((extraEffect != ExtraEffect.CutSample) && (extraEffect != ExtraEffect.Retrace))
						{
							channelInfo.LastRoundInfo.Effect = Effect.None;
							channelInfo.LastRoundInfo.EffectArg = 0;
						}
					}
					else if ((effect != Effect.Robot) && (effect != Effect.Glissando) && (effect != Effect.Vibrato) && (effect != Effect.Arpeggio))
					{
						channelInfo.LastRoundInfo.Effect = Effect.None;
						channelInfo.LastRoundInfo.EffectArg = 0;
					}
				}
			}

			TestPeriod(channelInfo);
			EffectCommands(channelInfo);
			TestPeriod(channelInfo);

			channelInfo.GlissandoOldPeriod = channelInfo.LastRoundInfo.Period;
			channelInfo.OldVolume = channelInfo.Volume;

			if (channelInfo.LastRoundInfo.Period == 0)
			{
				if (channelInfo.MainPeriod != 0)
					channelInfo.MainPeriod = -1;
			}
			else
			{
				channelInfo.MainPeriod = (short)channelInfo.LastRoundInfo.Period;
				channelInfo.MainVolume = channelInfo.Volume;

				if (channelInfo.BackwardEnabled == 0)
				{
					if ((channelInfo.LastRoundInfo.Effect == Effect.Extra) && ((ExtraEffect)(channelInfo.LastRoundInfo.EffectArg & 0xf0) == ExtraEffect.BackwardPlay))
						channelInfo.BackwardEnabled = (byte)((channelInfo.LastRoundInfo.EffectArg & 0x0f) == 0 ? 1 : 2);
					else
					{
						if (channelInfo.LastRoundInfo.Effect == Effect.Robot)
							RobotEffect(channelInfo, channelInfo.LastRoundInfo.EffectArg);
						else
						{
							if (channelInfo.RobotEnable)
							{
								channelInfo.OffEnable = true;
								channelInfo.RobotEnable = false;
							}
						}
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parses the next row of pattern data
		/// </summary>
		/********************************************************************/
		private void ParseRowData(ChannelInfo channelInfo, TrackLine trackLine)
		{
			bool oldPeriod = true;

			if (!playingInfo.PauseEnabled && !channelInfo.OnOffChannel && (trackLine.Period != 0))
			{
				if (trackLine.Effect == Effect.Glissando)
					channelInfo.GlissandoNewPeriod = 0;

				channelInfo.VibratoPeriod = 0;
				channelInfo.OffEnable = true;

				channelInfo.OriginalPeriod = FindPeriod(channelInfo, trackLine);
				channelInfo.LastRoundInfo.Period = channelInfo.OriginalPeriod;

				oldPeriod = false;
			}

			byte sampleNumber = channelInfo.LastRoundInfo.SampleNumber;
			Effect effect = Effect.None;
			byte effectArg = 0;

			try
			{
				if ((trackLine.SampleNumber != 0) || (trackLine.Effect != Effect.None) || (trackLine.EffectArg != 0))
				{
					effect = trackLine.Effect;
					effectArg = trackLine.EffectArg;

					if (trackLine.SampleNumber != 0)
					{
						byte number = sampleNumber == 0 ? channelInfo.OldSampleNumber : sampleNumber;

						if (oldPeriod && (number != 0))
							channelInfo.Volume = samples[number - 1].Volume;
						else
						{
							sampleNumber = trackLine.SampleNumber;

							if (effect == Effect.Glissando)
							{
								if (!channelInfo.IsActive)
								{
									sampleNumber = 0;
									effect = Effect.None;
									effectArg = 0;
									return;
								}
							}
							else
							{
								channelInfo.SampleData = samples[sampleNumber - 1].SampleData;
								channelInfo.StartOffset = 0;
							}

							channelInfo.Volume = samples[sampleNumber - 1].Volume;
							channelInfo.OldSampleNumber = sampleNumber;
							channelInfo.BackwardEnabled = 0;
							return;
						}
					}
				}

				if (trackLine.Period != 0)
				{
					sampleNumber = channelInfo.OldSampleNumber;

					if ((effect == Effect.Glissando) || (effect == Effect.GlissandoVolumeSlide))
					{
						if (!channelInfo.IsActive)
						{
							sampleNumber = 0;
							effect = Effect.None;
							effectArg = 0;
						}
					}
					else
					{
						if (sampleNumber == 0)
						{
							channelInfo.SampleData = null;
							channelInfo.StartOffset = 0;
						}
						else
						{
							channelInfo.SampleData = samples[sampleNumber - 1].SampleData;
							channelInfo.StartOffset = 0;
						}
					}
				}
			}
			finally
			{
				channelInfo.LastRoundInfo.SampleNumber = sampleNumber;
				channelInfo.LastRoundInfo.Effect = effect;
				channelInfo.LastRoundInfo.EffectArg = effectArg;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Find the period to play
		/// </summary>
		/********************************************************************/
		private ushort FindPeriod(ChannelInfo channelInfo, TrackLine trackLine)
		{
			byte sampleNumber = trackLine.SampleNumber;
			if (sampleNumber == 0)
				sampleNumber = channelInfo.OldSampleNumber;

			if (sampleNumber == 0)
				return 0;

			Sample sample = samples[sampleNumber - 1];
			if (sample.FineTune == 0)
				return trackLine.Period;

			sbyte fineTune = sample.FineTune;
			ushort period = trackLine.Period;

			if ((fineTune >= -8) && (fineTune <= 7))
			{
				for (int i = 0; i < 36; i++)
				{
					if (Tables.Periods[8, i] == period)
						return Tables.Periods[fineTune + 8, i];
				}
			}

			return (ushort)(period - ((period * fineTune) / 140));
		}



		/********************************************************************/
		/// <summary>
		/// Check given period for "out-of-bounds" values
		/// </summary>
		/********************************************************************/
		private void TestPeriod(ChannelInfo channelInfo)
		{
			if (channelInfo.LastRoundInfo.Period == 0)
			{
				channelInfo.LastRoundInfo.Period = 0;
				channelInfo.LastRoundInfo.SampleNumber = 0;
				channelInfo.LastRoundInfo.Effect = Effect.None;
				channelInfo.LastRoundInfo.EffectArg = 0;
			}
			else if (channelInfo.LastRoundInfo.Period < 113)
				channelInfo.LastRoundInfo.Period = 113;
		}



		/********************************************************************/
		/// <summary>
		/// Check for effects that only run once
		/// </summary>
		/********************************************************************/
		private void DoTriggerEffects(ChannelInfo channelInfo)
		{
			if ((channelInfo.LastRoundInfo.SampleNumber == 0) || (samples[channelInfo.LastRoundInfo.SampleNumber - 1].SampleData == null))
			{
				channelInfo.LastRoundInfo.Period = 0;
				channelInfo.LastRoundInfo.SampleNumber = 0;
			}

			EffectCommands2(channelInfo);

			if (channelInfo.OnOffChannel)
			{
				channelInfo.LastRoundInfo.Period = 0;
				channelInfo.LastRoundInfo.SampleNumber = 0;
				channelInfo.LastRoundInfo.Effect = Effect.None;
				channelInfo.LastRoundInfo.EffectArg = 0;
			}
			else
			{
				if ((channelInfo.LastRoundInfo.Effect == Effect.Extra) && ((ExtraEffect)channelInfo.LastRoundInfo.EffectArg == ExtraEffect.StopPlaying))
				{
					channelInfo.OffEnable = true;

					channelInfo.LastRoundInfo.Period = 0;
					channelInfo.LastRoundInfo.SampleNumber = 0;
					channelInfo.LastRoundInfo.Effect = Effect.None;
					channelInfo.LastRoundInfo.EffectArg = 0;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle effects which are only run once
		/// </summary>
		/********************************************************************/
		private void EffectCommands2(ChannelInfo channelInfo)
		{
			Effect effect = channelInfo.LastRoundInfo.Effect;
			byte effectArg = channelInfo.LastRoundInfo.EffectArg;

			if ((effect != Effect.None) || (effectArg != 0))
			{
				switch (effect)
				{
					case Effect.SampleOffset:
					{
						SampleOffsetMain(channelInfo, effectArg);
						break;
					}

					case Effect.SongRepeat:
					{
						SongRepeat(effectArg);
						break;
					}

					case Effect.SetVolume:
					{
						SetVolume(channelInfo, effectArg);
						break;
					}

					case Effect.PatternBreak:
					{
						PatternBreak(effectArg);
						break;
					}

					case Effect.SetSpeed:
					{
						SetTempo(effectArg);
						break;
					}

					case Effect.Extra:
					{
						ExtraEffect extraEffect = (ExtraEffect)(effectArg & 0xf0);
						effectArg &= 0x0f;

						switch (extraEffect)
						{
							case ExtraEffect.Filter:
							{
								Filter(effectArg);
								break;
							}

							case ExtraEffect.ChannelOnOff:
							{
								ChannelOnOff(channelInfo, effectArg);
								break;
							}

							case ExtraEffect.FineSlideUp:
							{
								FineSlideUp(channelInfo, effectArg);
								break;
							}

							case ExtraEffect.FineSlideDown:
							{
								FineSlideDown(channelInfo, effectArg);
								break;
							}

							case ExtraEffect.Loop:
							{
								Loop(channelInfo, effectArg);
								break;
							}

							case ExtraEffect.SampleOffset:
							{
								SampleOffsetExtra(channelInfo, effectArg);
								break;
							}

							case ExtraEffect.FineVolumeUp:
							{
								FineVolumeUp(channelInfo, effectArg);
								break;
							}

							case ExtraEffect.FineVolumeDown:
							{
								FineVolumeDown(channelInfo, effectArg);
								break;
							}

							case ExtraEffect.PatternDelay:
							{
								PatternDelay(effectArg);
								break;
							}
						}
						break;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle effects which are run for every tick
		/// </summary>
		/********************************************************************/
		private void EffectCommands(ChannelInfo channelInfo)
		{
			Effect effect = channelInfo.LastRoundInfo.Effect;
			byte effectArg = channelInfo.LastRoundInfo.EffectArg;

			if ((effect != Effect.None) || (effectArg != 0))
			{
				switch (effect)
				{
					case Effect.Arpeggio:
					{
						Arpeggio(channelInfo, effectArg);
						break;
					}

					case Effect.PortamentoUp:
					{
						PortamentoUp(channelInfo, effectArg);
						break;
					}

					case Effect.PortamentoDown:
					{
						PortamentoDown(channelInfo, effectArg);
						break;
					}

					case Effect.Glissando:
					{
						Glissando(channelInfo, effectArg);
						break;
					}

					case Effect.Vibrato:
					{
						Vibrato(channelInfo, effectArg);
						break;
					}

					case Effect.GlissandoVolumeSlide:
					{
						GlissandoVolumeSlide(channelInfo, effectArg);
						break;
					}

					case Effect.VibratoVolumeSlide:
					{
						VibratoVolumeSlide(channelInfo, effectArg);
						break;
					}

					case Effect.VolumeSlide:
					{
						VolumeSlide(channelInfo, effectArg);
						break;
					}

					case Effect.Extra:
					{
						ExtraEffect extraEffect = (ExtraEffect)(effectArg & 0xf0);
						effectArg &= 0x0f;

						switch (extraEffect)
						{
							case ExtraEffect.Retrace:
							{
								Retrace(channelInfo, effectArg);
								break;
							}

							case ExtraEffect.CutSample:
							{
								CutSample(channelInfo, effectArg);
								break;
							}
						}
						break;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Loop(ChannelInfo channelInfo, byte effectArg)
		{
			if (effectArg == 0)
			{
				if (channelInfo.LoopHowMany == 0)
				{
					channelInfo.LoopPatternPosition = (sbyte)(playingInfo.PatternPosition - 1);
					channelInfo.LoopSongPosition = playingInfo.SongPosition;
				}
			}
			else
			{
				if (channelInfo.LoopHowMany != 0)
				{
					channelInfo.LoopHowMany--;

					if (channelInfo.LoopHowMany == 0)
					{
						channelInfo.LoopPatternPosition = 0;
						channelInfo.LoopSongPosition = 0;
						channelInfo.LoopHowMany = 0;
					}
					else
					{
						playingInfo.PatternPosition = channelInfo.LoopPatternPosition;
						playingInfo.SongPosition = channelInfo.LoopSongPosition;
					}
				}
				else
				{
					channelInfo.LoopHowMany = effectArg;

					playingInfo.PatternPosition = channelInfo.LoopPatternPosition;
					playingInfo.SongPosition = channelInfo.LoopSongPosition;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PatternDelay(byte effectArg)
		{
			if (!playingInfo.PauseEnabled)
			{
				if (effectArg != 0)
					playingInfo.PauseVbl = (ushort)((playingInfo.Tempo * effectArg) + 1);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void SongRepeat(byte effectArg)
		{
			playingInfo.PatternPosition = -1;

			if (effectArg > 127)
				effectArg = 127;

			if (effectArg <= playingInfo.SongPosition)
				endReached = true;

			playingInfo.SongPosition = effectArg;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PatternBreak(byte effectArg)
		{
			if (effectArg > 0x63)
				effectArg = 0x63;

			if (playingInfo.PatternPosition != -1)
			{
				playingInfo.SongPosition++;
				if (playingInfo.SongPosition >= songLength)
					playingInfo.SongPosition = 0;
			}

			playingInfo.PatternPosition = (sbyte)(Tables.Hex[effectArg] - 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void SampleOffsetMain(ChannelInfo channelInfo, byte effectArg)
		{
			channelInfo.StartOffset += (channelInfo.SampleOffset * 65536U) + (effectArg * 256U);

			if (channelInfo.StartOffset > channelInfo.SampleData.Length)
				channelInfo.StartOffset = (uint)channelInfo.SampleData.Length;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void SampleOffsetExtra(ChannelInfo channelInfo, byte effectArg)
		{
			channelInfo.SampleOffset = effectArg;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void SetTempo(byte effectArg)
		{
			if (effectArg > 31)
				ChangeTempo(effectArg);
			else
			{
				playingInfo.Count = effectArg;
				ChangeSpeed(effectArg);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ChannelOnOff(ChannelInfo channelInfo, byte effectArg)
		{
			if (effectArg == 0)
				channelInfo.OnOffChannel = true;
			else if (effectArg == 1)
				channelInfo.OnOffChannel = false;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Filter(byte effectArg)
		{
			if (effectArg == 0)
				AmigaFilter = true;
			else if (effectArg == 1)
				AmigaFilter = false;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Retrace(ChannelInfo channelInfo, byte effectArg)
		{
			if (playingInfo.Count == 1)
				channelInfo.RetraceCount = 0;

			if (channelInfo.RetraceCount == (effectArg - 1))
			{
				channelInfo.OffEnable = true;
				channelInfo.StartOffset = 0;
				channelInfo.RetraceCount = 0;
			}
			else
				channelInfo.RetraceCount++;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void CutSample(ChannelInfo channelInfo, byte effectArg)
		{
			if (playingInfo.Count == effectArg)
				channelInfo.Volume = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Arpeggio(ChannelInfo channelInfo, byte effectArg)
		{
			byte arp = Tables.ArpeggioList[playingInfo.Count - 1];

			if (arp == 0)
			{
				channelInfo.LastRoundInfo.Period = channelInfo.OriginalPeriod;
				return;
			}

			if (arp == 2)
				effectArg &= 0x0f;
			else
				effectArg = (byte)((effectArg & 0xf0) >> 4);

			ushort period = channelInfo.OriginalPeriod;

			for (byte i = 0; i < 36; i++)
			{
				if (period >= Tables.Periods[8, i])
				{
					effectArg += i;
					if (effectArg >= 35)
						effectArg = 35;

					period = Tables.Periods[8, effectArg];
					break;
				}
			}

			channelInfo.LastRoundInfo.Period = period;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PortamentoUp(ChannelInfo channelInfo, byte effectArg)
		{
			if (effectArg == 0)
				effectArg = channelInfo.PortamentoUpOldValue;
			else
				channelInfo.PortamentoUpOldValue = effectArg;

			channelInfo.LastRoundInfo.Period -= effectArg;

			if (channelInfo.LastRoundInfo.Period < 113)
				channelInfo.LastRoundInfo.Period = 113;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PortamentoDown(ChannelInfo channelInfo, byte effectArg)
		{
			if (effectArg == 0)
				effectArg = channelInfo.PortamentoDownOldValue;
			else
				channelInfo.PortamentoDownOldValue = effectArg;

			channelInfo.LastRoundInfo.Period += effectArg;

			if (channelInfo.LastRoundInfo.Period > 856)
				channelInfo.LastRoundInfo.Period = 856;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void SetVolume(ChannelInfo channelInfo, byte effectArg)
		{
			channelInfo.Volume = effectArg;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void VolumeSlide(ChannelInfo channelInfo, byte effectArg)
		{
			if (effectArg == 0)
				effectArg = channelInfo.SlideVolumeOld;
			else
				channelInfo.SlideVolumeOld = effectArg;

			if (effectArg < 0x10)
			{
				channelInfo.Volume -= effectArg;
				if ((sbyte)channelInfo.Volume < 0)
					channelInfo.Volume = 0;
			}
			else
			{
				channelInfo.Volume += (byte)(effectArg >> 4);
				if (channelInfo.Volume > 64)
					channelInfo.Volume = 64;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FineSlideDown(ChannelInfo channelInfo, byte effectArg)
		{
			channelInfo.LastRoundInfo.Period += effectArg;

			if (channelInfo.LastRoundInfo.Period > 856)
				channelInfo.LastRoundInfo.Period = 856;

			channelInfo.LastRoundInfo.Effect = Effect.None;
			channelInfo.LastRoundInfo.EffectArg = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FineSlideUp(ChannelInfo channelInfo, byte effectArg)
		{
			channelInfo.LastRoundInfo.Period -= effectArg;

			if (channelInfo.LastRoundInfo.Period < 113)
				channelInfo.LastRoundInfo.Period = 113;

			channelInfo.LastRoundInfo.Effect = Effect.None;
			channelInfo.LastRoundInfo.EffectArg = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FineVolumeUp(ChannelInfo channelInfo, byte effectArg)
		{
			channelInfo.Volume += effectArg;

			if (channelInfo.Volume > 64)
				channelInfo.Volume = 64;

			channelInfo.LastRoundInfo.Effect = Effect.None;
			channelInfo.LastRoundInfo.EffectArg = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FineVolumeDown(ChannelInfo channelInfo, byte effectArg)
		{
			channelInfo.Volume -= effectArg;

			if ((sbyte)channelInfo.Volume < 0)
				channelInfo.Volume = 0;

			channelInfo.LastRoundInfo.Effect = Effect.None;
			channelInfo.LastRoundInfo.EffectArg = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Glissando(ChannelInfo channelInfo, byte effectArg)
		{
			if (effectArg == 0)
				effectArg = channelInfo.GlissandoOldValue;

			if (playingInfo.Count == 1)
				channelInfo.GlissandoOldValue = effectArg;

			if (channelInfo.GlissandoOldPeriod == 0)
				return;

			if (channelInfo.GlissandoNewPeriod == 0)
			{
				channelInfo.GlissandoNewPeriod = channelInfo.LastRoundInfo.Period;
				channelInfo.LastRoundInfo.Period = channelInfo.GlissandoOldPeriod;
				channelInfo.GlissandoEnable = false;

				if (channelInfo.GlissandoNewPeriod == channelInfo.LastRoundInfo.Period)
				{
					channelInfo.GlissandoNewPeriod = 0;
					return;
				}

				if (channelInfo.GlissandoNewPeriod < channelInfo.LastRoundInfo.Period)
					channelInfo.GlissandoEnable = true;
			}
			else
			{
				if (channelInfo.GlissandoEnable)
				{
					channelInfo.GlissandoOldPeriod -= effectArg;

					if (channelInfo.GlissandoOldPeriod <= channelInfo.GlissandoNewPeriod)
					{
						channelInfo.GlissandoOldPeriod = channelInfo.GlissandoNewPeriod;
						channelInfo.GlissandoNewPeriod = 0;
					}
				}
				else
				{
					channelInfo.GlissandoOldPeriod += effectArg;

					if (channelInfo.GlissandoOldPeriod >= channelInfo.GlissandoNewPeriod)
					{
						channelInfo.GlissandoOldPeriod = channelInfo.GlissandoNewPeriod;
						channelInfo.GlissandoNewPeriod = 0;
					}
				}

				channelInfo.LastRoundInfo.Period = channelInfo.GlissandoOldPeriod;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void GlissandoVolumeSlide(ChannelInfo channelInfo, byte effectArg)
		{
			VolumeSlide(channelInfo, effectArg);
			Glissando(channelInfo, 0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void VibratoVolumeSlide(ChannelInfo channelInfo, byte effectArg)
		{
			VolumeSlide(channelInfo, effectArg);
			Vibrato(channelInfo, 0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Vibrato(ChannelInfo channelInfo, byte effectArg)
		{
			if ((playingInfo.Count == playingInfo.Tempo) && (channelInfo.VibratoPeriod == 0))
				channelInfo.VibratoPeriod = channelInfo.LastRoundInfo.Period;

			ushort period = channelInfo.VibratoPeriod;

			try
			{
				if (playingInfo.Count == (playingInfo.Tempo - 1))
				{
					channelInfo.VibratoPeriod = 0;
					return;
				}

				if ((effectArg & 0x0f) == 0)
					effectArg |= (byte)(channelInfo.VibratoOldValue & 0x0f);

				if ((effectArg & 0xf0) == 0)
					effectArg |= (byte)(channelInfo.VibratoOldValue & 0xf0);

				channelInfo.VibratoOldValue = effectArg;

				ushort vibVal = Tables.VibratoSin[(channelInfo.VibratoValue >> 2) & 0x1f];
				vibVal = (ushort)(((effectArg & 0x0f) * vibVal) >> 7);

				if (channelInfo.VibratoValue < 0)
					period -= vibVal;
				else
					period += vibVal;

				channelInfo.VibratoValue += (sbyte)((effectArg >> 2) & 0x3c);
			}
			finally
			{
				channelInfo.LastRoundInfo.Period = period;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Add robot effect on the sample
		/// </summary>
		/********************************************************************/
		private void RobotEffect(ChannelInfo channelInfo, byte effectArg)
		{
			if (!channelInfo.RobotEnable)
			{
				channelInfo.OffEnable = true;
				channelInfo.RobotEnable = true;
				channelInfo.RobotCurrentPosition = 0;
			}

			// Calculate number of bytes to play from the sample in one tick
			int tickBytes = ((35468 * 125 / playingInfo.CiaTempo) / channelInfo.MainPeriod) * 2 + 2;

			MakeRobotBuffer(channelInfo, tickBytes);

			if (effectArg == 0)
				effectArg = channelInfo.RobotOldValue;
			else
				channelInfo.RobotOldValue = effectArg;

			// Calculate what to play from the sample
			int toPlayBytes = (tickBytes >> 6) * ((effectArg + 80) >> 2);

			if (tickBytes > toPlayBytes)
				toPlayBytes = tickBytes - toPlayBytes + 1;
			else
				toPlayBytes = 2;

			channelInfo.RobotBytesToPlay = toPlayBytes;
		}



		/********************************************************************/
		/// <summary>
		/// Copy part of the playing sampling into it's own buffer. This is
		/// done to simulate loop
		/// </summary>
		/********************************************************************/
		private void MakeRobotBuffer(ChannelInfo channelInfo, int tickBytes)
		{
			if (channelInfo.RobotBuffers == null)
			{
				channelInfo.RobotBuffers = new sbyte[][]
				{
					// 2500 is the max number of bytes when using BPM 32 and period 113.
					// Using double buffering, just in case the old buffer isn't done playing
					new sbyte[2500],
					new sbyte[2500]
				};
			}

			Sample sample = samples[channelInfo.OldSampleNumber - 1];

			if (sample.LoopLength == 0)
			{
				int todo = Math.Min(tickBytes, (int)sample.Length - channelInfo.RobotCurrentPosition);
				Array.Copy(sample.SampleData, channelInfo.RobotCurrentPosition, channelInfo.RobotBuffers[0], 0, todo);

				channelInfo.RobotCurrentPosition += todo;

				if (channelInfo.RobotCurrentPosition >= sample.Length)
				{
					tickBytes -= todo;
					if (tickBytes > 0)
						Array.Clear(channelInfo.RobotBuffers[0], todo, tickBytes);

					channelInfo.LastRoundInfo.Period = 0;
					channelInfo.LastRoundInfo.Effect = Effect.None;
					channelInfo.LastRoundInfo.EffectArg = 0;
				}
			}
			else
			{
				int loopEnd = (int)(sample.LoopStart + sample.LoopLength);
				int destPos = 0;

				while (tickBytes > 0)
				{
					int todo = Math.Min(tickBytes, loopEnd - channelInfo.RobotCurrentPosition);

					Array.Copy(sample.SampleData, channelInfo.RobotCurrentPosition, channelInfo.RobotBuffers[0], destPos, todo);

					channelInfo.RobotCurrentPosition += todo;
					tickBytes -= todo;
					destPos += todo;

					if (channelInfo.RobotCurrentPosition >= loopEnd)
						channelInfo.RobotCurrentPosition = (int)sample.LoopStart;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will change the speed on the module
		/// </summary>
		/********************************************************************/
		private void ChangeSpeed(byte newSpeed)
		{
			if (newSpeed != playingInfo.Tempo)
			{
				// Remember the speed
				playingInfo.Tempo = newSpeed;

				// Change the module info
				ShowSpeed();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will change the tempo on the module
		/// </summary>
		/********************************************************************/
		private void ChangeTempo(byte newTempo)
		{
			if (newTempo != playingInfo.CiaTempo)
			{
				// BPM tempo
				SetBpmTempo(newTempo);

				// Remember the tempo
				playingInfo.CiaTempo = newTempo;

				// Change the module info
				ShowTempo();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current song position
		/// </summary>
		/********************************************************************/
		private void ShowSongPosition()
		{
			OnModuleInfoChanged(InfoPositionLine, playingInfo.SongPosition.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with pattern number
		/// </summary>
		/********************************************************************/
		private void ShowPattern()
		{
			OnModuleInfoChanged(InfoPatternLine, orders[playingInfo.SongPosition].ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current speed
		/// </summary>
		/********************************************************************/
		private void ShowSpeed()
		{
			OnModuleInfoChanged(InfoSpeedLine, playingInfo.Tempo.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with tempo
		/// </summary>
		/********************************************************************/
		private void ShowTempo()
		{
			OnModuleInfoChanged(InfoTempoLine, playingInfo.CiaTempo.ToString());
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
		#endregion
	}
}
