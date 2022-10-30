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
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.SoundFx.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.SoundFx
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class SoundFxWorker : ModulePlayerAgentBase
	{
		private Sample[] samples;
		private byte[] orders;
		private uint[][] patterns;

		private int numberOfSubSongs;

		private ushort delay;
		private ushort maxPattern;
		private uint songLength;

		private Channel[] channelInfo;

		private ushort timer;
		private uint trackPos;
		private uint posCounter;
		private bool breakFlag;

		// Period table
		private static readonly short[] noteTable =
		{
			1076, 1076, 1076, 1076, 1076, 1076, 1076, 1076, 1076, 1076, 1076, 1076,
			1076, 1076, 1076, 1076, 1076, 1076, 1076, 1076,	1076, 1016,  960,  906,
			 856,  808,  762,  720,  678,  640,  604,  570,  538,  508,  480,  453,
			 428,  404,  381,  360,  339,  320,  302,  285,  269,  254,  240,  226,
			 214,  202,  190,  180,  170,  160,  151,  143,  135,  127,  120,  113,
			 113,  113,  113,  113,  113,  113,  113,  113,  113,  113,  113,  113,
			 113,  113,  113,  113,  113,  113,  113,  113,  113,  113,  113,  113,
			 113,  113,   -1
		};

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => new [] { "sfx", "sfx2" };



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
			if (fileSize < 144)
				return AgentResult.Unknown;

			// Read the sample size table
			uint[] sampleSizes = new uint[31];

			moduleStream.Seek(0, SeekOrigin.Begin);
			moduleStream.ReadArray_B_UINT32s(sampleSizes, 0, 31);

			// Check the mark
			uint mark = moduleStream.Read_B_UINT32();
			if (mark != 0x534f3331)					// SO31
				return AgentResult.Unknown;

			// Check the sample sizes
			uint total = 0;
			for (int i = 0; i < 31; i++)
				total += sampleSizes[i];

			if (total > fileSize)
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
				// Song length
				case 0:
				{
					description = Resources.IDS_SFX_INFODESCLINE0;
					value = songLength.ToString();
					break;
				}

				// Used patterns
				case 1:
				{
					description = Resources.IDS_SFX_INFODESCLINE1;
					value = maxPattern.ToString();
					break;
				}

				// Supported / used samples
				case 2:
				{
					description = Resources.IDS_SFX_INFODESCLINE2;
					value = "31";
					break;
				}

				// BPM
				case 3:
				{
					description = Resources.IDS_SFX_INFODESCLINE3;
					value = GetCurrentBpm().ToString();
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
				Encoding encoder = EncoderCollection.Amiga;

				// Read the sample size table
				uint[] sampleSizes = new uint[31];

				moduleStream.ReadArray_B_UINT32s(sampleSizes, 0, 31);

				// Skip the module mark
				moduleStream.Seek(4, SeekOrigin.Current);

				// Read the delay value
				delay = moduleStream.Read_B_UINT16();

				// Skip the pads
				moduleStream.Seek(14, SeekOrigin.Current);

				// Initialize sample array
				samples = new Sample[31];

				// Read the sample information
				for (int i = 0; i < 31; i++)
				{
					Sample sample = new Sample();

					sample.Name = moduleStream.ReadString(encoder, 22);
					sample.Length = (ushort)(moduleStream.Read_B_UINT16() * 2);
					sample.Volume = moduleStream.Read_B_UINT16();
					sample.LoopStart = moduleStream.Read_B_UINT16();
					sample.LoopLength = (ushort)(moduleStream.Read_B_UINT16() * 2);

					// Sample loop fix
					if ((sample.Length != 0) && (sample.LoopStart == sample.Length))
						sample.Length += sample.LoopLength;

					// Adjust the sample length
					if (sample.Length > 2)
						sample.Length = Math.Max(sample.Length, sample.LoopStart + sample.LoopLength);

					// September by Allister Brimble uses a sample with 0 length, but contains some data. So check for that
					if ((sample.Length == 0 && sampleSizes[i] != 0))
						sample.Length = sampleSizes[i];

					// Volume fix
					if (sample.Volume > 64)
						sample.Volume = 64;

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_SFX_ERR_LOADING_SAMPLEINFO;
						return AgentResult.Error;
					}

					samples[i] = sample;
				}

				// Read the song length
				songLength = moduleStream.Read_UINT8();
				moduleStream.Seek(1, SeekOrigin.Current);

				// Read the orders
				orders = new byte[128];
				moduleStream.Read(orders, 0, 128);
				moduleStream.Seek(4, SeekOrigin.Current);

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_SFX_ERR_LOADING_HEADER;
					return AgentResult.Error;
				}

				// Find highest pattern number
				maxPattern = 0;

				for (int i = 0; i < songLength; i++)
				{
					if (orders[i] > maxPattern)
						maxPattern = orders[i];
				}

				maxPattern++;

				// Allocate pattern array
				patterns = new uint[maxPattern][];

				// Allocate and load patterns
				for (int i = 0; i < maxPattern; i++)
				{
					// Allocate space to hold the pattern
					patterns[i] = new uint[4 * 64];

					// Read the pattern data
					moduleStream.ReadArray_B_UINT32s(patterns[i], 0, 4 * 64);

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_SFX_ERR_LOADING_PATTERNS;
						return AgentResult.Error;
					}
				}

				// Now it's time to read the sample data
				for (int i = 0; i < 31; i++)
				{
					int length = (int)sampleSizes[i];
					if (length != 0)
					{
						samples[i].SampleAddr = moduleStream.ReadSampleData(i, length, out int readBytes);

						// Check to see if we miss too much from the last sample
						if (readBytes < (length - 512))
						{
							errorMessage = Resources.IDS_SFX_ERR_LOADING_SAMPLES;
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
		public override bool InitSound(int songNumber, DurationInfo durationInfo, out string errorMessage)
		{
			if (!base.InitSound(songNumber, durationInfo, out errorMessage))
				return false;

			InitializeSound(durationInfo.StartPosition);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the current song
		/// </summary>
		/********************************************************************/
		public override void CleanupSound()
		{
			channelInfo = null;
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the duration for all sub-songs
		/// </summary>
		/********************************************************************/
		public override DurationInfo[] CalculateDuration()
		{
			DurationInfo[] durations = CalculateDurationBySongPosition();
			numberOfSubSongs = durations.Length;

			return durations;
		}



		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public override void Play()
		{
			timer++;
			if (timer == 6)
			{
				// Get the next pattern line
				timer = 0;
				PlaySound();
			}
			else
			{
				// Run any realtime effects
				MakeEffects(channelInfo[0], VirtualChannels[0]);
				MakeEffects(channelInfo[1], VirtualChannels[1]);
				MakeEffects(channelInfo[2], VirtualChannels[2]);
				MakeEffects(channelInfo[3], VirtualChannels[3]);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return information about sub-songs
		/// </summary>
		/********************************************************************/
		public override SubSongInfo SubSongs => new SubSongInfo(numberOfSubSongs, 0);



		/********************************************************************/
		/// <summary>
		/// Return the length of the current song
		/// </summary>
		/********************************************************************/
		public override int SongLength => (int)songLength;



		/********************************************************************/
		/// <summary>
		/// Return the current position of the song
		/// </summary>
		/********************************************************************/
		public override int GetSongPosition()
		{
			return (int)trackPos;
		}



		/********************************************************************/
		/// <summary>
		/// Set a new position of the song
		/// </summary>
		/********************************************************************/
		public override void SetSongPosition(int position, PositionInfo positionInfo)
		{
			// Change the position
			breakFlag = false;
			posCounter = 0;
			trackPos = (uint)position;

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
					// Build frequency table
					uint[] frequencies = new uint[10 * 12];

					for (int j = 0; j < 7 * 12; j++)
						frequencies[12 + j] = 3546895U / (ushort)noteTable[j];

					SampleInfo sampleInfo = new SampleInfo
					{
						Name = sample.Name,
						Type = SampleInfo.SampleType.Sample,
						BitSize = 8,
						MiddleC = frequencies[12 + 3 * 12],
						Volume = (ushort)(sample.Volume * 4),
						Panning = -1,
						Sample = sample.SampleAddr,
						Length = sample.Length > 2 ? sample.Length : 0,
						NoteFrequencies = frequencies
					};

					if (sample.LoopLength <= 2)
					{
						// No loop
						sampleInfo.Flags = SampleInfo.SampleFlags.None;
						sampleInfo.LoopStart = 0;
						sampleInfo.LoopLength = 0;
					}
					else
					{
						// Sample loops
						sampleInfo.Flags = SampleInfo.SampleFlags.Loop;
						sampleInfo.LoopStart = sample.LoopStart;
						sampleInfo.LoopLength = sample.LoopLength;
					}

					result.Add(sampleInfo);
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
		/// Return the current BPM
		/// </summary>
		/********************************************************************/
		protected override ushort GetCurrentBpm()
		{
			return (ushort)(PlayingFrequency * 2.5f);
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
			channelInfo = Helpers.InitializeArray<Channel>(4);

			timer = 0;
			trackPos = (uint)startPosition;
			posCounter = 0;
			breakFlag = false;

			// Calculate the frequency to play with
			PlayingFrequency = 1773447.0f / delay / 2.5f;

			MarkPositionAsVisited(startPosition);
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player has allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			patterns = null;
			orders = null;
			samples = null;
		}



		/********************************************************************/
		/// <summary>
		/// Get the next pattern line and parse it
		/// </summary>
		/********************************************************************/
		private void PlaySound()
		{
			// Find the pattern address
			uint[] patternAdr = patterns[orders[trackPos]];

			// Parse the pattern data
			PlayNote(channelInfo[0], VirtualChannels[0], patternAdr[posCounter]);
			PlayNote(channelInfo[1], VirtualChannels[1], patternAdr[posCounter + 1]);
			PlayNote(channelInfo[2], VirtualChannels[2], patternAdr[posCounter + 2]);
			PlayNote(channelInfo[3], VirtualChannels[3], patternAdr[posCounter + 3]);

			// Do we need to break the current pattern?
			if (breakFlag)
			{
				breakFlag = false;
				posCounter = 4 * 63;
			}

			// Go to the next pattern line
			posCounter += 4;
			if (posCounter == 4 * 64)
			{
				// Okay, the pattern is done, go to the next pattern
				posCounter = 0;
				trackPos++;

				if (trackPos == songLength)
				{
					// Module is done, loop it
					trackPos = (uint)(currentDurationInfo?.StartPosition ?? 0);
				}

				if (HasPositionBeenVisited((int)trackPos))
				{
					// Next module
					OnEndReached();
				}

				// Tell NostalgicPlayer we have changed the position
				OnPositionChanged();

				MarkPositionAsVisited((int)trackPos);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse one channel pattern line
		/// </summary>
		/********************************************************************/
		private void PlayNote(Channel channel, IChannel virtChannel, uint patternData)
		{
			// Remember the pattern data
			channel.PatternData = patternData;

			// If there is a PIC command, don't parse the pattern data
			if ((patternData & 0xffff0000) != 0xfffd0000)
			{
				// Get the sample number
				byte sampleNum = (byte)((patternData & 0x0000f000) >> 12);
				if ((patternData & 0x10000000) != 0)
					sampleNum += 16;

				if (sampleNum != 0)
				{
					Sample curSample = samples[sampleNum - 1];

					// Get the sample information
					channel.SampleNumber = (short)(sampleNum - 1);
					channel.Sample = curSample.SampleAddr;
					channel.SampleLen = curSample.Length;
					channel.Volume = curSample.Volume;
					channel.LoopStart = curSample.LoopStart;
					channel.LoopLength = curSample.LoopLength;

					// Get current volume
					short volume = (short)channel.Volume;

					// Any volume effects
					switch ((patternData & 0x00000f00) >> 8)
					{
						// Change volume up
						case 5:
						{
							volume += (short)(patternData & 0x000000ff);
							if (volume > 64)
								volume = 64;

							break;
						}

						// Change volume down
						case 6:
						{
							volume -= (short)(patternData & 0x000000ff);
							if (volume < 0)
								volume = 0;

							break;
						}
					}

					// Change the volume on the channel
					virtChannel.SetVolume((ushort)(volume * 4));
				}
			}

			// Do we have a pattern PIC command?
			if ((patternData & 0xffff0000) == 0xfffd0000)
			{
				channel.PatternData &= 0xffff0000;
				return;
			}

			if ((patternData & 0xffff0000) == 0)
				return;

			// Stop sliding and stepping
			channel.SlideSpeed = 0;
			channel.StepValue = 0;

			// Get the period
			channel.CurrentNote = (ushort)(((patternData & 0xffff0000) >> 16) & 0xefff);

			// Do we have a pattern STP command?
			if ((patternData & 0xffff0000) == 0xfffe0000)
			{
				virtChannel.Mute();
				return;
			}

			// Do we have a pattern BRK command?
			if ((patternData & 0xffff0000) == 0xfffc0000)
			{
				breakFlag = true;
				channel.PatternData &= 0xefffffff;
				return;
			}

			// Do we have a pattern ??? command?
			if ((patternData & 0xffff0000) == 0xfffb0000)
			{
				channel.PatternData &= 0xefffffff;
				return;
			}

			// Play the note
			if (channel.Sample != null)
			{
				virtChannel.PlaySample(channel.SampleNumber, channel.Sample, 0, channel.SampleLen);
				virtChannel.SetAmigaPeriod(channel.CurrentNote);

				if (channel.LoopLength > 2)
					virtChannel.SetLoop(channel.LoopStart, channel.LoopLength);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Runs all the realtime effects
		/// </summary>
		/********************************************************************/
		private void MakeEffects(Channel channel, IChannel virtChannel)
		{
			if (channel.StepValue != 0)
			{
				// Well, we need to pitch step the note
				if (channel.StepValue < 0)
				{
					// Step it up
					channel.StepNote = (ushort)(channel.StepNote + channel.StepValue);

					if (channel.StepNote <= channel.StepEndNote)
					{
						channel.StepValue = 0;
						channel.StepNote = channel.StepEndNote;
					}
				}
				else
				{
					// Step it down
					channel.StepNote = (ushort)(channel.StepNote + channel.StepValue);

					if (channel.StepNote >= channel.StepEndNote)
					{
						channel.StepValue = 0;
						channel.StepNote = channel.StepEndNote;
					}
				}

				// Set the new period on the channel
				channel.CurrentNote = channel.StepNote;
				virtChannel.SetAmigaPeriod(channel.CurrentNote);
			}
			else
			{
				if (channel.SlideSpeed != 0)
				{
					ushort value = (ushort)(channel.SlideParam & 0x0f);

					if (value != 0)
					{
						if (++channel.SlideControl == value)
						{
							channel.SlideControl = 0;
							value = (ushort)((channel.SlideParam << 4) << 3);

							if (!channel.SlideDirection)
							{
								channel.SlidePeriod += 8;
								value += channel.SlideSpeed;

								if (value == channel.SlidePeriod)
									channel.SlideDirection = true;
							}
							else
							{
								channel.SlidePeriod -= 8;
								value -= channel.SlideSpeed;

								if (value == channel.SlidePeriod)
									channel.SlideDirection = false;
							}

							// Set the new period on the channel
							channel.CurrentNote = channel.SlidePeriod;
							virtChannel.SetAmigaPeriod(channel.SlidePeriod);
						}
					}
				}

				// Well, do we have any effects we need to run
				switch ((channel.PatternData & 0x00000f00) >> 8)
				{
					// Arpeggio
					case 1:
					{
						Arpeggio(channel, virtChannel);
						break;
					}

					// Pitchbend
					case 2:
					{
						ushort newPeriod;

						short bendValue = (short)((channel.PatternData & 0x000000f0) >> 4);
						if (bendValue != 0)
							newPeriod = (ushort)(((channel.PatternData & 0xefff0000) >> 16) + bendValue);
						else
						{
							bendValue = (short)(channel.PatternData & 0x0000000f);
							if (bendValue != 0)
								newPeriod = (ushort)(((channel.PatternData & 0xefff0000) >> 16) - bendValue);
							else
								break;
						}

						// Play the new period
						virtChannel.SetAmigaPeriod(newPeriod);

						// Put the new period into the pattern data
						channel.PatternData = (channel.PatternData & 0x1000ffff) | ((uint)newPeriod << 16);
						break;
					}

					// LedOn (filter off!)
					//
					// There is a little bug in the original player. They has
					// exchanged the on/off, so the effect names does not
					// match what they actually do :)
					case 3:
					{
						AmigaFilter = false;
						break;
					}

					// LedOff (filter on!)
					case 4:
					{
						AmigaFilter = true;
						break;
					}

					// SetStepUp
					case 7:
					{
						StepFinder(channel, false);
						break;
					}

					// SetStepDown
					case 8:
					{
						StepFinder(channel, true);
						break;
					}

					// Auto slide
					//
					// There is none of the official player sources that implement this
					// effect, but "Alcatrash" by Kerni uses it. Found a implementation
					// of it in Flod 4.1, which this is based on
					case 9:
					{
						channel.SlideSpeed = channel.SlidePeriod = channel.CurrentNote;
						channel.SlideParam = (ushort)(channel.PatternData & 0x000000ff);
						channel.SlideDirection = false;
						channel.SlideControl = 0;
						break;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse and run the arpeggio effect
		/// </summary>
		/********************************************************************/
		private void Arpeggio(Channel channel, IChannel virtChannel)
		{
			short index;

			// Find out which period to play
			switch (timer)
			{
				case 1:
				case 5:
				{
					index = (short)((channel.PatternData & 0x000000f0) >> 4);
					break;
				}

				case 2:
				case 4:
				{
					index = (short)(channel.PatternData & 0x0000000f);
					break;
				}

				default:
				{
					virtChannel.SetAmigaPeriod(channel.CurrentNote);
					return;
				}
			}

			// Got the index, now find the period
			int note = 20;
			for (;;)
			{
				// Couldn't find the period, so no arpeggio :(
				if (noteTable[note] == -1)
					return;

				// Found the period, break the loop
				if (noteTable[note] == channel.CurrentNote)
					break;

				// Get the next period
				note++;
			}

			// Set the period
			virtChannel.SetAmigaPeriod((ushort)noteTable[note + index]);
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the step values
		/// </summary>
		/********************************************************************/
		private void StepFinder(Channel channel, bool stepDown)
		{
			// Find the step value and the end index
			short stepValue = (short)(channel.PatternData & 0x0000000f);
			short endIndex = (short)((channel.PatternData & 0x000000f0) >> 4);

			if (stepDown)
				stepValue = (short)-stepValue;
			else
				endIndex = (short)-endIndex;

			channel.StepNote = channel.CurrentNote;
			channel.StepValue = stepValue;

			// Find the period in the period table
			int note = 20;
			for (;;)
			{
				// Couldn't find the period, so don't really step anywhere
				if (noteTable[note] == -1)
				{
					channel.StepEndNote = channel.CurrentNote;
					return;
				}

				// Found the period, break the loop
				if (noteTable[note] == channel.CurrentNote)
					break;

				// Get the next period
				note++;
			}

			// Get the end note
			channel.StepEndNote = (ushort)noteTable[note + endIndex];
		}
		#endregion
	}
}
