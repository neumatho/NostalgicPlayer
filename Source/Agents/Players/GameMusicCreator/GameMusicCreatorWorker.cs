/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Polycode.NostalgicPlayer.Agent.Player.GameMusicCreator.Containers;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.GameMusicCreator
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class GameMusicCreatorWorker : ModulePlayerWithPositionDurationAgentBase
	{
		private int numberOfPositions;
		private byte[] positionList;

		private Sample[] samples;
		private Pattern[] patterns;

		private GlobalPlayingInfo playingInfo;
		private ChannelInfo[] channelInfo;

		private bool endReached;

		private const int InfoPositionLine = 3;
		private const int InfoPatternLine = 4;
		private const int InfoSpeedLine = 5;

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => new [] { "gmc" };



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			if (moduleStream.Length < 444)
				return AgentResult.Unknown;

			// Check size of position table
			moduleStream.Seek(240, SeekOrigin.Begin);

			uint positionListLength = moduleStream.Read_B_UINT32();
			if (positionListLength > 100)
				return AgentResult.Unknown;

			// Check the first pattern offsets
			uint temp = moduleStream.Read_B_UINT32();
			temp |= moduleStream.Read_B_UINT32();
			temp |= moduleStream.Read_B_UINT32();
			temp |= moduleStream.Read_B_UINT32();
			temp &= 0x03ff03ff;
			if (temp != 0)
				return AgentResult.Unknown;

			// Check sample information
			moduleStream.Seek(0, SeekOrigin.Begin);

			uint sampleSize = 0;
			ushort temp1;

			for (int i = 0; i < 15; i++)
			{
				// Check start address
				temp = moduleStream.Read_B_UINT32();
				if (temp > 0xf8000)
					return AgentResult.Unknown;

				// Check sample length
				temp1 = moduleStream.Read_B_UINT16();
				if (temp1 >= 0x8000)
					return AgentResult.Unknown;

				if ((temp1 != 0) && (temp == 0))
					return AgentResult.Unknown;

				sampleSize += temp1 * 2U;

				// Check volume
				if (moduleStream.Read_B_UINT16() > 64)
					return AgentResult.Unknown;

				// Get loop address
				uint temp2 = moduleStream.Read_B_UINT32();
				if (temp2 != 0)
				{
					if ((temp2 > 0xf8000) || (temp == 0) || (temp1 == 0))
						return AgentResult.Unknown;
				}

				moduleStream.Seek(4, SeekOrigin.Current);
			}

			// Check position table and find number of patterns
			moduleStream.Seek(244, SeekOrigin.Begin);

			temp1 = 0;

			for (int i = 0; i < positionListLength; i++)
			{
				ushort temp3 = moduleStream.Read_B_UINT16();
				if ((temp3 % 1024) != 0)
					return AgentResult.Unknown;

				if ((temp3 < 0x8000) && (temp3 > temp1))
					temp1 = temp3;
			}

			ushort patternNum = (ushort)((temp1 / 1024) + 1);

			// Check module length
			if ((444 + patternNum * 1024 + sampleSize) > (moduleStream.Length + 256))
				return AgentResult.Unknown;

			// Check periods in the first pattern
			moduleStream.Seek(444, SeekOrigin.Begin);

			int periodCount = 0;

			for (int i = 0; i < 256; i++)
			{
				temp = moduleStream.Read_B_UINT32();
				temp = (temp & 0x0fff0000) >> 16;

				if ((temp != 0) && (temp != 0xffe))
				{
					if (!Tables.Periods.Contains((short)temp))
						return AgentResult.Unknown;

					periodCount++;
				}
			}

			if (periodCount == 0)
				return AgentResult.Unknown;

			return AgentResult.Ok;
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
					description = Resources.IDS_GMC_INFODESCLINE0;
					value = numberOfPositions.ToString();
					break;
				}

				// Used patterns
				case 1:
				{
					description = Resources.IDS_GMC_INFODESCLINE1;
					value = patterns.Length.ToString();
					break;
				}

				// Supported / used samples
				case 2:
				{
					description = Resources.IDS_GMC_INFODESCLINE2;
					value = "15";
					break;
				}

				// Playing position
				case 3:
				{
					description = Resources.IDS_GMC_INFODESCLINE3;
					value = playingInfo.CurrentPosition.ToString();
					break;
				}

				// Playing pattern
				case 4:
				{
					description = Resources.IDS_GMC_INFODESCLINE4;
					value = playingInfo.CurrentPattern.ToString();
					break;
				}

				// Current speed
				case 5:
				{
					description = Resources.IDS_GMC_INFODESCLINE6;
					value = playingInfo.SongStep.ToString();
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

				// Read sample information
				samples = new Sample[15];

				for (int i = 0; i < 15; i++)
				{
					Sample sample = new Sample();

					uint start = moduleStream.Read_B_UINT32();
					sample.Length = (ushort)(moduleStream.Read_B_UINT16() * 2);
					sample.Volume = moduleStream.Read_B_UINT16();
					uint loopStart = moduleStream.Read_B_UINT32();
					sample.LoopLength = moduleStream.Read_B_UINT16();

					if ((sample.LoopLength != 0) && (sample.LoopLength != 2))
					{
						sample.LoopStart = (ushort)(loopStart - start);
						sample.LoopLength *= 2;
					}
					else
					{
						sample.LoopStart = 0;
						sample.LoopLength = 0;
					}

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_GMC_ERR_LOADING_SAMPLEINFO;
						Cleanup();

						return AgentResult.Error;
					}

					moduleStream.Seek(2, SeekOrigin.Current);

					samples[i] = sample;
				}

				// Read position list
				numberOfPositions = moduleStream.Read_B_INT32();
				positionList = new byte[numberOfPositions];

				int numberOfPatterns = 0;

				for (int i = 0, cnt = numberOfPositions; i < cnt; i++)
				{
					ushort temp = moduleStream.Read_B_UINT16();
					if (temp < 0x8000)
					{
						positionList[i] = (byte)(temp / 1024);

						if (positionList[i] > numberOfPatterns)
							numberOfPatterns = positionList[i];
					}
					else
						numberOfPositions--;
				}

				numberOfPatterns++;

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_GMC_ERR_LOADING_HEADER;
					Cleanup();

					return AgentResult.Error;
				}

				// Read patterns
				moduleStream.Seek(444, SeekOrigin.Begin);

				patterns = new Pattern[numberOfPatterns];

				for (int i = 0; i < numberOfPatterns; i++)
				{
					Pattern pattern = new Pattern();

					for (int j = 0; j < 64; j++)
					{
						for (int k = 0; k < 4; k++)
						{
							ushort period = moduleStream.Read_B_UINT16();
							byte byt3 = moduleStream.Read_UINT8();
							byte byt4 = moduleStream.Read_UINT8();

							pattern.Tracks[k, j] = new TrackLine
							{
								Period = period,
								Sample = (byte)(byt3 >> 4),
								Effect = (Effect)(byt3 & 0x0f),
								EffectArg = byt4
							};
						}
					}

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_GMC_ERR_LOADING_PATTERNS;
						Cleanup();

						return AgentResult.Error;
					}

					patterns[i] = pattern;
				}

				// Read sample data
				for (int i = 0; i < 15; i++)
				{
					Sample sample = samples[i];

					if (sample.Length > 0)
					{
						sample.Data = moduleStream.ReadSampleData(i, sample.Length, out _);

						if (moduleStream.EndOfStream)
						{
							errorMessage = Resources.IDS_GMC_ERR_LOADING_SAMPLES;
							Cleanup();

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
			EveryTick();

			playingInfo.SongSpeed++;
			if (playingInfo.SongSpeed >= playingInfo.SongStep)
			{
				playingInfo.SongSpeed = 0;

				UpdatePatternCounters();
				PlayPatternRow();
			}

			if (endReached)
			{
				OnEndReached(playingInfo.CurrentPosition);
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

				for (int j = 0; j < 3 * 12; j++)
					frequencies[4 * 12 + j] = 3546895U / (ushort)Tables.Periods[j];

				foreach (Sample sample in samples)
				{
					SampleInfo sampleInfo = new SampleInfo
					{
						Name = string.Empty,
						Type = SampleInfo.SampleType.Sample,
						BitSize = SampleInfo.SampleSize._8Bit,
						Volume = (ushort)(sample.Volume * 4),
						Panning = -1,
						Sample = sample.Data,
						Length = sample.Length,
						NoteFrequencies = frequencies
					};

					if (sample.LoopLength == 0)
					{
						// No loop
						sampleInfo.Flags = SampleInfo.SampleFlag.None;
						sampleInfo.LoopStart = 0;
						sampleInfo.LoopLength = 0;
					}
					else
					{
						// Sample loops
						sampleInfo.Flags = SampleInfo.SampleFlag.Loop;
						sampleInfo.LoopStart = sample.LoopStart;
						sampleInfo.LoopLength = sample.LoopLength;
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
			InitializeSound(startPosition);

			return startPosition;
		}



		/********************************************************************/
		/// <summary>
		/// Return the total number of positions
		/// </summary>
		/********************************************************************/
		protected override int GetTotalNumberOfPositions()
		{
			return numberOfPositions;
		}



		/********************************************************************/
		/// <summary>
		/// Create a snapshot of all the internal structures and return it
		/// </summary>
		/********************************************************************/
		protected override ISnapshot CreateSnapshot()
		{
			return new Snapshot(playingInfo, channelInfo);
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
			channelInfo = clonedSnapshot.Channels;

			UpdateModuleInformation();

			return true;
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
			playingInfo = new GlobalPlayingInfo
			{
				CurrentPosition = (short)(startPosition - 1),
				CurrentPattern = 0,

				SongSpeed = 0,
				SongStep = 6,
				PatternCount = 63
			};

			endReached = false;

			// Initialize channel structure
			channelInfo = new ChannelInfo[4];

			for (int i = 0; i < 4; i++)
			{
				ChannelInfo chanInfo = new ChannelInfo();

				chanInfo.Slide = 0;
				chanInfo.Period = 0;
				chanInfo.Volume = 0;

				channelInfo[i] = chanInfo;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player have allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			samples = null;
			patterns = null;
			positionList = null;

			playingInfo = null;
			channelInfo = null;
		}



		/********************************************************************/
		/// <summary>
		/// Set a new pattern to play
		/// </summary>
		/********************************************************************/
		private void UpdatePatternCounters()
		{
			playingInfo.PatternCount++;
			if (playingInfo.PatternCount == 64)
			{
				playingInfo.CurrentPosition++;
				if (playingInfo.CurrentPosition >= numberOfPositions)
					playingInfo.CurrentPosition = 0;

				if (HasPositionBeenVisited(playingInfo.CurrentPosition))
					endReached = true;

				MarkPositionAsVisited(playingInfo.CurrentPosition);

				playingInfo.PatternCount = 0;
				playingInfo.CurrentPattern = positionList[playingInfo.CurrentPosition];

				ShowSongPosition();
				ShowPattern();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Play all voices for a single pattern row
		/// </summary>
		/********************************************************************/
		private void PlayPatternRow()
		{
			for (int i = 0; i < 4; i++)
			{
				ChannelInfo chanInfo = channelInfo[i];
				IChannel channel = VirtualChannels[i];

				TrackLine trackLine = patterns[playingInfo.CurrentPattern].Tracks[i, playingInfo.PatternCount];

				SetInstrument(channel, chanInfo, trackLine);
				SetEffect(chanInfo, trackLine);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Set new instrument if required
		/// </summary>
		/********************************************************************/
		private void SetInstrument(IChannel channel, ChannelInfo chanInfo, TrackLine trackLine)
		{
			if (trackLine.Sample != 0)
			{
				Sample sample = samples[trackLine.Sample - 1];

				chanInfo.Period = trackLine.Period;
				chanInfo.Volume = sample.Volume;
				chanInfo.Slide = 0;

				if (sample.Data != null)
				{
					channel.PlaySample((short)(trackLine.Sample - 1), sample.Data, 0, sample.Length);

					if (sample.LoopLength > 0)
						channel.SetLoop(sample.LoopStart, sample.LoopLength);

					channel.SetVolume(0);
					channel.SetAmigaPeriod(chanInfo.Period);
				}
				else
					channel.Mute();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Set new effect if required
		/// </summary>
		/********************************************************************/
		private void SetEffect(ChannelInfo chanInfo, TrackLine trackLine)
		{
			byte effectArg = trackLine.EffectArg;

			switch (trackLine.Effect)
			{
				case Effect.SlideUp:
				{
					chanInfo.Slide = -effectArg;
					break;
				}

				case Effect.SlideDown:
				{
					chanInfo.Slide = effectArg;
					break;
				}

				case Effect.SetVolume:
				{
					if (effectArg > 64)
						effectArg = 64;

					chanInfo.Volume = effectArg;
					break;
				}

				case Effect.PatternBreak:
				{
					playingInfo.PatternCount = 63;
					break;
				}

				case Effect.PositionJump:
				{
					playingInfo.CurrentPosition = (short)(effectArg - 1);
					playingInfo.PatternCount = 63;
					break;
				}

				case Effect.EnableFilter:
				{
					AmigaFilter = true;
					break;
				}

				case Effect.DisableFilter:
				{
					AmigaFilter = false;
					break;
				}

				case Effect.SetSpeed:
				{
					playingInfo.SongStep = effectArg;

					ShowSpeed();
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle everything that need to be done on every tick
		/// </summary>
		/********************************************************************/
		private void EveryTick()
		{
			for (int i = 0; i < 4; i++)
			{
				ChannelInfo chanInfo = channelInfo[i];
				IChannel channel = VirtualChannels[i];

				chanInfo.Period = (ushort)(chanInfo.Period + chanInfo.Slide);

				channel.SetAmigaPeriod(chanInfo.Period);
				channel.SetAmigaVolume(chanInfo.Volume);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current song position
		/// </summary>
		/********************************************************************/
		private void ShowSongPosition()
		{
			OnModuleInfoChanged(InfoPositionLine, playingInfo.CurrentPosition.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with pattern number
		/// </summary>
		/********************************************************************/
		private void ShowPattern()
		{
			OnModuleInfoChanged(InfoPatternLine, playingInfo.CurrentPattern.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current speed
		/// </summary>
		/********************************************************************/
		private void ShowSpeed()
		{
			OnModuleInfoChanged(InfoSpeedLine, playingInfo.SongStep.ToString());
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
		}
		#endregion
	}
}
