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
using Polycode.NostalgicPlayer.Agent.Player.Oktalyzer.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.Oktalyzer
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class OktalyzerWorker : ModulePlayerAgentBase
	{
		private static readonly short[] periods =
		{
			856, 808, 762, 720, 678, 640, 604, 570, 538, 508, 480, 453,
			428, 404, 381, 360, 339, 320, 302, 285, 269, 254, 240, 226,
			214, 202, 190, 180, 170, 160, 151, 143, 135, 127, 120, 113
		};

		private static readonly ChannelPanning[] panPos =
		{
			ChannelPanning.Left, ChannelPanning.Left, ChannelPanning.Right, ChannelPanning.Right,
			ChannelPanning.Right, ChannelPanning.Right, ChannelPanning.Left, ChannelPanning.Left
		};

		private static readonly sbyte[] arp10 =
		{
			0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0
		};

		private static readonly sbyte[] arp12 =
		{
			0, 1, 2, 3, 1, 2, 3, 1, 2, 3, 1, 2, 3, 1, 2, 3
		};

		private uint sampNum;
		private ushort pattNum;
		private ushort songLength;
		private ushort chanNum;
		private ushort startSpeed;

		private ushort currentSpeed;
		private ushort speedCounter;
		private short songPos;
		private short newSongPos;
		private short pattPos;
		private bool filterStatus;

		private bool[] channelFlags;
		private byte[] patternTable;
		private Sample[] sampleInfo;
		private Pattern[] patterns;

		private PatternLine[] currLine;
		private ChannelInfo[] chanInfo;
		private sbyte[] chanVol;
		private byte[] chanIndex;

		private uint readPatt;
		private uint readSamp;
		private uint realUsedSampNum;

		private const int InfoSpeedLine = 3;

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => new [] { "okt", "okta" };



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			if (moduleStream.Length < 1368)
				return AgentResult.Unknown;

			// Check the mark
			moduleStream.Seek(0, SeekOrigin.Begin);

			if ((moduleStream.Read_B_UINT32() != 0x4f4b5441) || (moduleStream.Read_B_UINT32() != 0x534f4e47))	// OKTASONG
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
					description = Resources.IDS_OKT_INFODESCLINE0;
					value = songLength.ToString();
					break;
				}

				// Used patterns
				case 1:
				{
					description = Resources.IDS_OKT_INFODESCLINE1;
					value = pattNum.ToString();
					break;
				}

				// Supported / used samples
				case 2:
				{
					description = Resources.IDS_OKT_INFODESCLINE2;
					value = sampNum.ToString();
					break;
				}

				// Current speed
				case 3:
				{
					description = Resources.IDS_OKT_INFODESCLINE3;
					value = currentSpeed.ToString();
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

				// Skip the mark
				moduleStream.Seek(8, SeekOrigin.Begin);

				// Initialize variables
				sampNum = 0;
				pattNum = 0;
				songLength = 0;
				startSpeed = 6;

				readPatt = 0;
				readSamp = 0;
				realUsedSampNum = 0;

				// Okay, now read each chunk and parse them
				for (;;)
				{
					// Read the chunk name and length
					uint chunkName = moduleStream.Read_B_UINT32();
					uint chunkSize = moduleStream.Read_B_UINT32();

					// Do we have any chunks left?
					if (moduleStream.EndOfStream)
						break;			// No, stop the loading

					// Find out what the chunk is and begin to parse it
					switch (chunkName)
					{
						// Channel modes (CMOD)
						case 0x434d4f44:
						{
							ParseCmod(moduleStream, chunkSize, out errorMessage);
							break;
						}

						// Sample information (SAMP)
						case 0x53414d50:
						{
							ParseSamp(moduleStream, chunkSize, out errorMessage);
							break;
						}

						// Start speed (SPEE)
						case 0x53504545:
						{
							ParseSpee(moduleStream, chunkSize, out errorMessage);
							break;
						}

						// Song length (SLEN)
						case 0x534c454e:
						{
							ParseSlen(moduleStream, chunkSize, out errorMessage);
							break;
						}

						// Number of pattern positions (PLEN)
						case 0x504c454e:
						{
							ParsePlen(moduleStream, chunkSize, out errorMessage);
							break;
						}

						// Pattern table (PATT)
						case 0x50415454:
						{
							ParsePatt(moduleStream, chunkSize, out errorMessage);
							break;
						}

						// Pattern body (PBOD)
						case 0x50424f44:
						{
							if ((readPatt < pattNum) && (patterns != null))
							{
								ParsePbod(moduleStream, chunkSize, out errorMessage);
								readPatt++;
							}
							else
							{
								// Ignore the chunk
								moduleStream.Seek(chunkSize, SeekOrigin.Current);
							}
							break;
						}

						// Sample data (SBOD)
						case 0x53424f44:
						{
							if ((readSamp < sampNum) && (sampleInfo != null))
							{
								ParseSbod(moduleStream, chunkSize, out errorMessage);
								readSamp++;
							}
							else
							{
								// Ignore the chunk
								moduleStream.Seek(chunkSize, SeekOrigin.Current);
							}
							break;
						}

						// Unknown chunks
						default:
						{
							// Check to see if we had read all the samples
							if ((readSamp == 0) || (readSamp < realUsedSampNum))
								errorMessage = string.Format(Resources.IDS_OKT_ERR_UNKNOWN_CHUNK, (char)((chunkName >> 24) & 0xff), (char)((chunkName >> 16) & 0xff), (char)((chunkName >> 8) & 0xff), (char)(chunkName & 0xff));
							else
							{
								// Well, we had read all the samples, so if the file
								// has some extra data appended, we ignore it
								moduleStream.Seek(0, SeekOrigin.End);
							}
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
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the current song
		/// </summary>
		/********************************************************************/
		public override bool InitSound(int songNumber, DurationInfo durationInfo, out string errorMessage)
		{
			// Initialize the variables
			chanInfo = Helpers.InitializeArray<ChannelInfo>(8);
			currLine = Helpers.InitializeArray<PatternLine>(8);
			chanVol = new sbyte[8];
			chanIndex = new byte[8];

			for (int i = 0; i < 8; i++)
				chanVol[i] = 64;

			songPos = 0;
			newSongPos = -1;
			pattPos = -1;
			currentSpeed = startSpeed;
			speedCounter = 0;
			filterStatus = false;

			// Set the channel panning + create the channel index
			for (int i = 0, panNum = 0; i < chanNum; i++, panNum++)
			{
				VirtualChannels[i].SetPanning((ushort)panPos[panNum]);
				chanIndex[i] = (byte)(panNum / 2);

				if (!channelFlags[panNum / 2])
					panNum++;
			}

			return base.InitSound(songNumber, durationInfo, out errorMessage);
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the current song
		/// </summary>
		/********************************************************************/
		public override void CleanupSound()
		{
			chanInfo = null;
			currLine = null;
			chanVol = null;
			chanIndex = null;
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
			// Wait until we need to play another pattern line
			speedCounter++;
			if (speedCounter >= currentSpeed)
			{
				// Play next pattern line
				speedCounter = 0;

				FindNextPatternLine();
				PlayPatternLine();
			}

			// Do each frame stuff
			DoEffects();
			SetVolumes();

			// Do the filter stuff
			AmigaFilter = filterStatus;
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the module use
		/// </summary>
		/********************************************************************/
		public override int ModuleChannelCount => chanNum;



		/********************************************************************/
		/// <summary>
		/// Return the length of the current song
		/// </summary>
		/********************************************************************/
		public override int SongLength => songLength;



		/********************************************************************/
		/// <summary>
		/// Return the current position of the song
		/// </summary>
		/********************************************************************/
		public override int GetSongPosition()
		{
			return songPos;
		}



		/********************************************************************/
		/// <summary>
		/// Set a new position of the song
		/// </summary>
		/********************************************************************/
		public override void SetSongPosition(int position, PositionInfo positionInfo)
		{
			// Change the position
			songPos = (short)position;
			pattPos = 0;
			currentSpeed = positionInfo.Speed;
			speedCounter = currentSpeed;
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

				foreach (Sample sample in sampleInfo)
				{
					// Build frequency table
					uint[] frequencies = new uint[10 * 12];

					for (int j = 0; j < 3 * 12; j++)
						frequencies[3 * 12 + j] = 3546895U / (ushort)periods[j];

					SampleInfo sampleInfo = new SampleInfo
					{
						Name = sample.Name,
						Type = SampleInfo.SampleType.Sample,
						BitSize = 8,
						MiddleC = frequencies[3 * 12 + 12],
						Volume = (byte)(sample.Volume * 4),
						Panning = -1,
						Sample = sample.SampleData,
						Length = sample.Length,
						NoteFrequencies = frequencies
					};

					if (sample.RepeatLength == 0)
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
						sampleInfo.LoopStart = sample.RepeatStart;
						sampleInfo.LoopLength = sample.RepeatLength;
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
			InitSound(0, null, out _);

			return startPosition;
		}



		/********************************************************************/
		/// <summary>
		/// Return the current speed
		/// </summary>
		/********************************************************************/
		protected override byte GetCurrentSpeed()
		{
			return (byte)currentSpeed;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player has allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			patterns = null;
			sampleInfo = null;
			channelFlags = null;
		}



		/********************************************************************/
		/// <summary>
		/// Parses the CMOD chunk
		/// </summary>
		/********************************************************************/
		private void ParseCmod(ModuleStream moduleStream, uint chunkSize, out string errorMessage)
		{
			// Start to check the chunk size
			if (chunkSize != 8)
			{
				errorMessage = string.Format(Resources.IDS_OKT_ERR_INVALID_CHUNK_SIZE, "CMOD", chunkSize);
				return;
			}

			// Read the channel flags
			chanNum = 4;
			channelFlags = new bool[4];

			for (int i = 0; i < 4; i++)
			{
				if (moduleStream.Read_B_UINT16() == 0)
					channelFlags[i] = false;
				else
				{
					channelFlags[i] = true;
					chanNum++;
				}
			}

			errorMessage = string.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// Parses the SAMP chunk
		/// </summary>
		/********************************************************************/
		private void ParseSamp(ModuleStream moduleStream, uint chunkSize, out string errorMessage)
		{
			Encoding encoder = EncoderCollection.Amiga;
			byte[] buffer = new byte[21];

			// Calculate the number of samples
			sampNum = chunkSize / 32;

			// Allocate memory to hold the sample information
			sampleInfo = new Sample[sampNum];

			// Read the sample information
			buffer[20] = 0x00;

			for (int i = 0; i < sampNum; i++)
			{
				Sample sample = new Sample();

				// Sample name
				moduleStream.ReadString(buffer, 20);
				sample.Name = encoder.GetString(buffer);

				// Other information
				sample.Length = moduleStream.Read_B_UINT32();
				sample.RepeatStart = (ushort)(moduleStream.Read_B_UINT16() * 2);
				sample.RepeatLength = (ushort)(moduleStream.Read_B_UINT16() * 2);

				if (sample.RepeatLength <= 2)
				{
					sample.RepeatStart = 0;
					sample.RepeatLength = 0;
				}

				moduleStream.Seek(1, SeekOrigin.Current);
				sample.Volume = moduleStream.Read_UINT8();
				sample.Mode = moduleStream.Read_B_UINT16();

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_OKT_ERR_LOADING_HEADER;
					return;
				}

				if (sample.Length != 0)
					realUsedSampNum++;

				sampleInfo[i] = sample;
			}

			errorMessage = string.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// Parses the SPEE chunk
		/// </summary>
		/********************************************************************/
		private void ParseSpee(ModuleStream moduleStream, uint chunkSize, out string errorMessage)
		{
			// Start to check the chunk size
			if (chunkSize != 2)
			{
				errorMessage = string.Format(Resources.IDS_OKT_ERR_INVALID_CHUNK_SIZE, "SPEE", chunkSize);
				return;
			}

			// Read the start speed
			startSpeed = moduleStream.Read_B_UINT16();

			errorMessage = string.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// Parses the SLEN chunk
		/// </summary>
		/********************************************************************/
		private void ParseSlen(ModuleStream moduleStream, uint chunkSize, out string errorMessage)
		{
			// Start to check the chunk size
			if (chunkSize != 2)
			{
				errorMessage = string.Format(Resources.IDS_OKT_ERR_INVALID_CHUNK_SIZE, "SLEN", chunkSize);
				return;
			}

			// Read the number of patterns
			pattNum = moduleStream.Read_B_UINT16();

			// Allocate memory to hold the patterns
			patterns = new Pattern[pattNum];

			errorMessage = string.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// Parses the PLEN chunk
		/// </summary>
		/********************************************************************/
		private void ParsePlen(ModuleStream moduleStream, uint chunkSize, out string errorMessage)
		{
			// Start to check the chunk size
			if (chunkSize != 2)
			{
				errorMessage = string.Format(Resources.IDS_OKT_ERR_INVALID_CHUNK_SIZE, "PLEN", chunkSize);
				return;
			}

			// Read the song length
			songLength = moduleStream.Read_B_UINT16();

			errorMessage = string.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// Parses the PATT chunk
		/// </summary>
		/********************************************************************/
		private void ParsePatt(ModuleStream moduleStream, uint chunkSize, out string errorMessage)
		{
			// Start to check the chunk size
			if (chunkSize != 128)
			{
				errorMessage = string.Format(Resources.IDS_OKT_ERR_INVALID_CHUNK_SIZE, "PLEN", chunkSize);
				return;
			}

			// Read the positions
			patternTable = new byte[128];
			moduleStream.Read(patternTable, 0, 128);

			errorMessage = string.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// Parses the PBOD chunk
		/// </summary>
		/********************************************************************/
		private void ParsePbod(ModuleStream moduleStream, uint chunkSize, out string errorMessage)
		{
			// Allocate pattern
			Pattern pattern = new Pattern();

			// First read the number of pattern lines
			pattern.LineNum = (short)moduleStream.Read_B_UINT16();

			// Allocate lines
			pattern.Lines = new PatternLine[pattern.LineNum * chanNum];

			// Read the pattern data
			for (int i = 0; i < pattern.LineNum; i++)
			{
				for (int j = 0; j < chanNum; j++)
				{
					PatternLine line = new PatternLine();

					line.Note = moduleStream.Read_UINT8();
					line.SampleNum = moduleStream.Read_UINT8();
					line.Effect = moduleStream.Read_UINT8();
					line.EffectArg = moduleStream.Read_UINT8();

					pattern.Lines[i * chanNum + j] = line;
				}
			}

			if (moduleStream.EndOfStream)
			{
				errorMessage = Resources.IDS_OKT_ERR_LOADING_PATTERNS;
				return;
			}

			patterns[readPatt] = pattern;

			errorMessage = string.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// Parses the SBOD chunk
		/// </summary>
		/********************************************************************/
		private void ParseSbod(ModuleStream moduleStream, uint chunkSize, out string errorMessage)
		{
			errorMessage = string.Empty;

			// Find the next sample slot
			while (sampleInfo[readSamp].Length == 0)
			{
				readSamp++;
				if (readSamp >= sampNum)
					return;
			}

			// Allocate memory to hold the sample data
			uint allocLen = Math.Max(chunkSize, sampleInfo[readSamp].Length);
			sampleInfo[readSamp].SampleData = new sbyte[allocLen];

			int readBytes = moduleStream.ReadSampleData((int)readSamp, sampleInfo[readSamp].SampleData, (int)chunkSize);

			if (moduleStream.EndOfStream)
			{
				if (((readSamp + 1) < realUsedSampNum) || ((readBytes + 20) < chunkSize))
					errorMessage = Resources.IDS_OKT_ERR_LOADING_SAMPLES;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Find the next pattern to use and where in the pattern we need to
		/// extract the information we need
		/// </summary>
		/********************************************************************/
		private void FindNextPatternLine()
		{
			// Find the right pattern
			Pattern patt = patterns[patternTable[songPos]];

			// Go to next pattern line
			pattPos++;

			if ((pattPos >= patt.LineNum) || (newSongPos != -1))
			{
				// Okay, we're done with the current pattern. Find the next one
				pattPos = 0;

				if (newSongPos != -1)
				{
					if (newSongPos < songPos)
						OnEndReached();

					songPos = newSongPos;
					newSongPos = -1;
				}
				else
					songPos++;

				if (songPos == songLength)
				{
					songPos = 0;
					currentSpeed = startSpeed;

					// Song end
					OnEndReached();
				}

				// Tell NostalgicPlayer we have changed the song position
				OnPositionChanged();

				// Find the right pattern
				patt = patterns[patternTable[songPos]];
			}

			// Copy the current line data
			for (int i = 0; i < chanNum; i++)
				currLine[i] = patt.Lines[pattPos * chanNum + i];
		}



		/********************************************************************/
		/// <summary>
		/// Trig new samples in each channel
		/// </summary>
		/********************************************************************/
		private void PlayPatternLine()
		{
			for (uint i = 0, j = 0; i < 4; i++, j++)
			{
				PlayChannel(j);

				if (channelFlags[i])
					PlayChannel(++j);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will parse the pattern data for one channel
		/// </summary>
		/********************************************************************/
		private void PlayChannel(uint channelNum)
		{
			// Get the pattern and channel data
			PatternLine pattData = currLine[channelNum];
			ChannelInfo chanData = chanInfo[channelNum];

			// If we shouldn't play any note, well just return
			if (pattData.Note == 0)
				return;

			// Get the note number
			byte note = (byte)(pattData.Note - 1);

			// Does the instrument have a sample attached?
			Sample samp = sampleInfo[pattData.SampleNum];
			if ((samp.SampleData == null) || (samp.Length == 0))
				return;

			// Well, find out if we are playing in a mixed or normal channel
			if (channelFlags[chanIndex[channelNum]])
			{
				// Mixed
				//
				// If the sample is mode "4", it won't be played
				if (samp.Mode == 1)
					return;

				// Just play the sample. Samples doesn't loop in mixed channels
				VirtualChannels[channelNum].PlaySample(pattData.SampleNum, samp.SampleData, 0, samp.Length);

				chanData.ReleaseStart = 0;
				chanData.ReleaseLength = 0;
			}
			else
			{
				// Normal
				//
				// If the sample is mode "8", it won't be played
				if (samp.Mode == 0)
					return;

				// Set the channel volume
				chanVol[chanIndex[channelNum]] = (sbyte)samp.Volume;

				// Does the sample loop?
				if (samp.RepeatLength == 0)
				{
					// No
					VirtualChannels[channelNum].PlaySample(pattData.SampleNum, samp.SampleData, 0, samp.Length);

					chanData.ReleaseStart = 0;
					chanData.ReleaseLength = 0;
				}
				else
				{
					// Yes
					VirtualChannels[channelNum].PlaySample(pattData.SampleNum, samp.SampleData, 0, (uint)samp.RepeatStart + samp.RepeatLength);
					VirtualChannels[channelNum].SetLoop(samp.RepeatStart, samp.RepeatLength);

					chanData.ReleaseStart = (uint)samp.RepeatStart + samp.RepeatLength;
					chanData.ReleaseLength = samp.Length - chanData.ReleaseStart;
				}
			}

			// Find the period
			chanData.CurrNote = note;
			chanData.CurrPeriod = periods[note];

			VirtualChannels[channelNum].SetAmigaPeriod((uint)chanData.CurrPeriod);
		}



		/********************************************************************/
		/// <summary>
		/// Sets the volume for each channel
		/// </summary>
		/********************************************************************/
		private void SetVolumes()
		{
			// Start to copy the volumes
			chanVol[4] = chanVol[0];
			chanVol[5] = chanVol[1];
			chanVol[6] = chanVol[2];
			chanVol[7] = chanVol[3];

			// Now set the volume
			for (int i = 0, j = 0; i < 4; i++, j++)
			{
				VirtualChannels[j].SetVolume((ushort)(chanVol[i] * 4));

				if (channelFlags[i])
					VirtualChannels[++j].SetVolume((ushort)(chanVol[i] * 4));
			}
		}



		/********************************************************************/
		/// <summary>
		/// Run all the effects
		/// </summary>
		/********************************************************************/
		private void DoEffects()
		{
			for (uint i = 0, j = 0; i < 4; i++, j++)
			{
				DoChannelEffect(j);

				if (channelFlags[i])
					DoChannelEffect(++j);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Run the effects on one channel
		/// </summary>
		/********************************************************************/
		private void DoChannelEffect(uint channelNum)
		{
			// Get the pattern and channel data
			PatternLine pattData = currLine[channelNum];
			ChannelInfo chanData = chanInfo[channelNum];

			switch (pattData.Effect)
			{
				// Effect '1': Portamento down
				case 1:
				{
					chanData.CurrPeriod -= pattData.EffectArg;
					if (chanData.CurrPeriod < 113)
						chanData.CurrPeriod = 113;

					VirtualChannels[channelNum].SetAmigaPeriod((uint)chanData.CurrPeriod);
					break;
				}

				// Effect '2': Portamento up
				case 2:
				{
					chanData.CurrPeriod += pattData.EffectArg;
					if (chanData.CurrPeriod > 856)
						chanData.CurrPeriod = 856;

					VirtualChannels[channelNum].SetAmigaPeriod((uint)chanData.CurrPeriod);
					break;
				}

				// Effect 'A': Arpeggio type 1
				case 10:
				{
					sbyte workNote = (sbyte)chanData.CurrNote;
					sbyte arpNum = arp10[speedCounter];

					switch (arpNum)
					{
						// Note - upper 4 bits
						case 0:
						{
							workNote -= (sbyte)((pattData.EffectArg & 0xf0) >> 4);
							break;
						}

						// Note
						case 1:
							break;

						// Note + lower 4 bits
						case 2:
						{
							workNote += (sbyte)(pattData.EffectArg & 0x0f);
							break;
						}
					}

					PlayNote(channelNum, chanData, workNote);
					break;
				}

				// Effect 'B': Arpeggio type 2
				case 11:
				{
					sbyte workNote = (sbyte)chanData.CurrNote;

					switch (speedCounter & 0x3)
					{
						// Note
						case 0:
						case 2:
							break;

						// Note + lower 4 bits
						case 1:
						{
							workNote += (sbyte)(pattData.EffectArg & 0x0f);
							break;
						}

						// Note - upper 4 bits
						case 3:
						{
							workNote -= (sbyte)((pattData.EffectArg & 0xf0) >> 4);
							break;
						}
					}

					PlayNote(channelNum, chanData, workNote);
					break;
				}

				// Effect 'C': Arpeggio type 3
				case 12:
				{
					sbyte workNote = (sbyte)chanData.CurrNote;
					sbyte arpNum = arp12[speedCounter];

					if (arpNum == 0)
						break;

					switch (arpNum)
					{
						// Note - upper 4 bits
						case 1:
						{
							workNote -= (sbyte)((pattData.EffectArg & 0xf0) >> 4);
							break;
						}

						// Note + lower 4 bits
						case 2:
						{
							workNote += (sbyte)(pattData.EffectArg & 0x0f);
							break;
						}

						// Note
						case 3:
							break;
					}

					PlayNote(channelNum, chanData, workNote);
					break;
				}

				// Effect 'H': Increase note once per line
				case 17:
				{
					if (speedCounter != 0)
						break;

					goto case 30;
				}

				// Effect 'U': Increase note once per tick
				case 30:
				{
					chanData.CurrNote += pattData.EffectArg;
					PlayNote(channelNum, chanData, (sbyte)chanData.CurrNote);
					break;
				}

				// Effect 'L': Decrease note once per line
				case 21:
				{
					if (speedCounter != 0)
						break;

					goto case 13;
				}

				// Effect 'D': Decrease note once per tick
				case 13:
				{
					chanData.CurrNote -= pattData.EffectArg;
					PlayNote(channelNum, chanData, (sbyte)chanData.CurrNote);
					break;
				}

				// Effect 'F': Filter control
				case 15:
				{
					if (speedCounter == 0)
						filterStatus = pattData.EffectArg != 0;

					break;
				}

				// Effect 'P': Position jump
				case 25:
				{
					if (speedCounter == 0)
					{
						ushort newPos = (ushort)(((pattData.EffectArg & 0xf0) >> 4) * 10 + (pattData.EffectArg & 0x0f));

						if (newPos < songLength)
							newSongPos = (short)newPos;
					}
					break;
				}

				// Effect 'R': Release sample
				case 27:
				{
					if ((chanData.ReleaseStart != 0) && (chanData.ReleaseLength != 0))
						VirtualChannels[channelNum].PlayReleasePart(chanData.ReleaseStart, chanData.ReleaseLength);

					break;
				}

				// Effect 'S': Set speed
				case 28:
				{
					if ((speedCounter == 0) && ((pattData.EffectArg & 0xf) != 0))
					{
						currentSpeed = (ushort)(pattData.EffectArg & 0xf);

						// Change the module info
						OnModuleInfoChanged(InfoSpeedLine, currentSpeed.ToString());
					}
					break;
				}

				// Effect 'O': Volume control with retrig
				case 24:
				{
					chanVol[chanIndex[channelNum]] = chanVol[chanIndex[channelNum] + 4];

					goto case 31;
				}

				// Effect 'V': Volume control
				case 31:
				{
					int volIndex = chanIndex[channelNum];
					byte effArg = pattData.EffectArg;

					if (effArg <= 64)
					{
						// Set the volume
						chanVol[volIndex] = (sbyte)effArg;
						break;
					}

					effArg -= 64;
					if (effArg < 16)
					{
						// Decrease the volume for every tick
						chanVol[volIndex] -= (sbyte)effArg;
						if (chanVol[volIndex] < 0)
							chanVol[volIndex] = 0;

						break;
					}

					effArg -= 16;
					if (effArg < 16)
					{
						// Increase the volume for every tick
						chanVol[volIndex] += (sbyte)effArg;
						if (chanVol[volIndex] > 64)
							chanVol[volIndex] = 64;

						break;
					}

					effArg -= 16;
					if (effArg < 16)
					{
						// Decrease the volume for every line
						if (speedCounter == 0)
						{
							chanVol[volIndex] -= (sbyte)effArg;
							if (chanVol[volIndex] < 0)
								chanVol[volIndex] = 0;
						}
						break;
					}

					effArg -= 16;
					if (effArg < 16)
					{
						// Increase the volume for every line
						if (speedCounter == 0)
						{
							chanVol[volIndex] += (sbyte)effArg;
							if (chanVol[volIndex] > 64)
								chanVol[volIndex] = 64;
						}
						break;
					}
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Plays the note on the channel given
		/// </summary>
		/********************************************************************/
		private void PlayNote(uint channelNum, ChannelInfo chanData, sbyte note)
		{
			// Check for out of bounds
			if (note < 0)
				note = 0;

			if (note > 35)
				note = 35;

			// Play the note
			chanData.CurrPeriod = periods[note];
			VirtualChannels[channelNum].SetAmigaPeriod((uint)chanData.CurrPeriod);
		}
		#endregion
	}
}
