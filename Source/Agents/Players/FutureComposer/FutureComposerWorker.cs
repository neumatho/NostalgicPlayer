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
using Polycode.NostalgicPlayer.Agent.Player.FutureComposer.Containers;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.FutureComposer
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class FutureComposerWorker : ModulePlayerAgentBase
	{
		private static readonly byte[] silent =
		{
			0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xe1
		};

		private static readonly ushort[] periods =
		{
			1712, 1616, 1524, 1440, 1356, 1280, 1208, 1140, 1076, 1016,  960,  906,
			 856,  808,  762,  720,  678,  640,  604,  570,  538,  508,  480,  453,
			 428,  404,  381,  360,  339,  320,  302,  285,  269,  254,  240,  226,
			 214,  202,  190,  180,  170,  160,  151,  143,  135,  127,  120,  113,
			 113,  113,  113,  113,  113,  113,  113,  113,  113,  113,  113,  113,
			3424, 3232, 3048, 2880, 2712, 2560, 2416, 2280, 2152, 2032, 1920, 1812,
			1712, 1616, 1524, 1440, 1356, 1280, 1208, 1140, 1076, 1016,  960,  906,
			 856,  808,  762,  720,  678,  640,  604,  570,  538,  508,  480,  453,
			 428,  404,  381,  360,  339,  320,  302,  285,  269,  254,  240,  226,
			 214,  202,  190,  180,  170,  160,  151,  143,  135,  127,  120,  113,
			 113,  113,  113,  113,  113,  113,  113,  113,  113,  113,  113,  113
		};

		private Sample[] sampInfo;

		private Sequence[] sequences;
		private Pattern[] patterns;
		private byte[] frqSequences;
		private VolSequence[] volSequences;

		private short seqNum;
		private short patNum;
		private short volNum;
		private short wavNum;
		private short sampNum;

		private ushort reSpCnt;
		private ushort repSpd;
		private ushort spdTemp;

		private bool[] audTemp;
		private VoiceInfo[] voiceData;

		private const int InfoSpeedLine = 4;

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => new [] { "fc", "fc14" };



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
			if (fileSize < 180)
				return AgentResult.Unknown;

			// Check the mark
			moduleStream.Seek(0, SeekOrigin.Begin);

			uint mark = moduleStream.Read_B_UINT32();
			if (mark != 0x46433134)					// FC14
				return AgentResult.Unknown;

			// Skip the song length
			moduleStream.Seek(4, SeekOrigin.Current);

			// Check the offset pointers
			for (int i = 0; i < 8; i++)
			{
				if (moduleStream.Read_B_UINT32() > fileSize)
					return AgentResult.Unknown;
			}

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
					description = Resources.IDS_FC_INFODESCLINE0;
					value = seqNum.ToString();
					break;
				}

				// Used patterns
				case 1:
				{
					description = Resources.IDS_FC_INFODESCLINE1;
					value = patNum.ToString();
					break;
				}

				// Supported / used samples
				case 2:
				{
					description = Resources.IDS_FC_INFODESCLINE2;
					value = sampNum.ToString();
					break;
				}

				// Used wave tables
				case 3:
				{
					description = Resources.IDS_FC_INFODESCLINE3;
					value = wavNum.ToString();
					break;
				}

				// Current speed
				case 4:
				{
					description = Resources.IDS_FC_INFODESCLINE4;
					value = repSpd.ToString();
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

				// Skip mark
				moduleStream.Seek(4, SeekOrigin.Begin);

				// Get the length of the sequences
				int seqLength = (int)moduleStream.Read_B_UINT32();

				// Get the offsets into the file
				int patOffset = (int)moduleStream.Read_B_UINT32();
				int patLength = (int)moduleStream.Read_B_UINT32();

				int frqOffset = (int)moduleStream.Read_B_UINT32();
				int frqLength = (int)moduleStream.Read_B_UINT32();

				int volOffset = (int)moduleStream.Read_B_UINT32();
				int volLength = (int)moduleStream.Read_B_UINT32();

				int smpOffset = (int)moduleStream.Read_B_UINT32();
				int wavOffset = (int)moduleStream.Read_B_UINT32();

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_FC_ERR_LOADING_HEADER;
					Cleanup();

					return AgentResult.Error;
				}

				// Read the sample information
				sampInfo = new Sample[10 + 80];
				sampNum = 10;

				int i;
				for (i = 0; i < 10; i++)
				{
					Sample samp = new Sample();

					samp.Address = null;
					samp.Length = (ushort)(moduleStream.Read_B_UINT16() * 2);
					samp.LoopStart = moduleStream.Read_B_UINT16();
					samp.LoopLength = (ushort)(moduleStream.Read_B_UINT16() * 2);
					samp.Multi = null;

					sampInfo[i] = samp;
				}

				// Read the wave table lengths
				for (i = 10; i < (10 + 80); i++)
				{
					Sample samp = new Sample();

					samp.Address = null;
					samp.Length = (ushort)(moduleStream.Read_UINT8() * 2);
					samp.LoopStart = 0;
					samp.LoopLength = samp.Length;
					samp.Multi = null;

					sampInfo[i] = samp;
				}

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_FC_ERR_LOADING_SAMPLEINFO;
					Cleanup();

					return AgentResult.Error;
				}

				// Find out how many wave tables that are used
				for (i = 89; i >= 10; i--)
				{
					if (sampInfo[i].Length != 0)
						break;
				}

				wavNum = (short)(i - 9);

				// Allocate memory to hold the sequences
				seqNum = (short)(seqLength / 13);
				sequences = new Sequence[seqNum];

				// Read the sequences
				for (i = 0; i < seqNum; i++)
				{
					Sequence seq = new Sequence();

					seq.VoiceSeq[0] = new VoiceSeq();
					seq.VoiceSeq[0].Pattern = moduleStream.Read_UINT8();
					seq.VoiceSeq[0].Transpose = (sbyte)moduleStream.Read_UINT8();
					seq.VoiceSeq[0].SoundTranspose = (sbyte)moduleStream.Read_UINT8();

					seq.VoiceSeq[1] = new VoiceSeq();
					seq.VoiceSeq[1].Pattern = moduleStream.Read_UINT8();
					seq.VoiceSeq[1].Transpose = (sbyte)moduleStream.Read_UINT8();
					seq.VoiceSeq[1].SoundTranspose = (sbyte)moduleStream.Read_UINT8();

					seq.VoiceSeq[2] = new VoiceSeq();
					seq.VoiceSeq[2].Pattern = moduleStream.Read_UINT8();
					seq.VoiceSeq[2].Transpose = (sbyte)moduleStream.Read_UINT8();
					seq.VoiceSeq[2].SoundTranspose = (sbyte)moduleStream.Read_UINT8();

					seq.VoiceSeq[3] = new VoiceSeq();
					seq.VoiceSeq[3].Pattern = moduleStream.Read_UINT8();
					seq.VoiceSeq[3].Transpose = (sbyte)moduleStream.Read_UINT8();
					seq.VoiceSeq[3].SoundTranspose = (sbyte)moduleStream.Read_UINT8();

					seq.Speed = moduleStream.Read_UINT8();

					sequences[i] = seq;
				}

				// Allocate memory to hold the patterns
				patNum = (short)(patLength / 64);
				patterns = new Pattern[patNum];

				// Read the patterns
				moduleStream.Seek(patOffset, SeekOrigin.Begin);

				for (i = 0; i < patNum; i++)
				{
					Pattern patt = new Pattern();

					for (int j = 0; j < 32; j++)
					{
						PatternRow row = new PatternRow();

						row.Note = moduleStream.Read_UINT8();
						row.Info = moduleStream.Read_UINT8();

						patt.PatternRows[j] = row;
					}

					patterns[i] = patt;
				}

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_FC_ERR_LOADING_PATTERNS;
					Cleanup();

					return AgentResult.Error;
				}

				// Allocate memory to hold the frequency sequences
				frqSequences = new byte[silent.Length + frqLength + 1];

				// Copy silent sequence to the first block
				Array.Copy(silent, 0, frqSequences, 0, silent.Length);

				// Read the frequency sequences
				moduleStream.Seek(frqOffset, SeekOrigin.Begin);
				moduleStream.Read(frqSequences, silent.Length, frqLength);

				// Set "end of sequence" mark
				frqSequences[frqSequences.Length - 1] = 0xe1;

				// Allocate memory to hold the volume sequences
				volNum = (short)(volLength / 64);
				volSequences = new VolSequence[1 + volNum];

				volSequences[0] = new VolSequence
				{
					Speed = silent[0],
					FrqNumber = silent[1],
					VibSpeed = (sbyte)silent[2],
					VibDepth = (sbyte)silent[3],
					VibDelay = silent[4]
				};

				Array.Copy(silent, 4, volSequences[0].Values, 0, silent.Length - 4);

				// Read the volume sequences
				moduleStream.Seek(volOffset, SeekOrigin.Begin);

				for (i = 1; i <= volNum; i++)
				{
					VolSequence volSeq = new VolSequence();

					volSeq.Speed = moduleStream.Read_UINT8();
					volSeq.FrqNumber = moduleStream.Read_UINT8();
					volSeq.VibSpeed = (sbyte)moduleStream.Read_UINT8();
					volSeq.VibDepth = (sbyte)moduleStream.Read_UINT8();
					volSeq.VibDelay = moduleStream.Read_UINT8();

					moduleStream.Read(volSeq.Values, 0, 59);

					volSequences[i] = volSeq;
				}

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_FC_ERR_LOADING_PATTERNS;
					Cleanup();

					return AgentResult.Error;
				}

				// Load the samples
				moduleStream.Seek(smpOffset, SeekOrigin.Begin);

				short realSampleNumber = 0;

				for (i = 0; i < 10; i++)
				{
					sampInfo[i].SampleNumber = realSampleNumber++;

					if (sampInfo[i].Length != 0)
					{
						// Read the first 4 bytes to see if it's a multi sample
						if (moduleStream.Read_B_UINT32() == 0x53534d50)           // SSMP
						{
							// It is, so allocate the multi sample structure
							// and fill in the information
							MultiSample multiSample = new MultiSample();
							uint[] multiOffsets = new uint[20];

							// Read the sample information
							for (int j = 0; j < 20; j++)
							{
								multiOffsets[j] = moduleStream.Read_B_UINT32();

								Sample samp = new Sample();

								samp.Length = (ushort)(moduleStream.Read_B_UINT16() * 2);
								samp.LoopStart = moduleStream.Read_B_UINT16();
								samp.LoopLength = (ushort)(moduleStream.Read_B_UINT16() * 2);

								multiSample.Sample[j] = samp;

								// Skip pad bytes
								moduleStream.Seek(6, SeekOrigin.Current);
							}

							// The sample structure holding the multi samples should not be included
							// as a sample, so decrement the count
							sampNum--;
							realSampleNumber--;

							// Read the sample data
							long sampStartOffset = moduleStream.Position;

							for (int j = 0; j < 20; j++)
							{
								if (multiSample.Sample[j].Length != 0)
								{
									// Read the sample data
									moduleStream.Seek(sampStartOffset + multiOffsets[j], SeekOrigin.Begin);
									multiSample.Sample[j].Address = moduleStream.ReadSampleData(10 + j, multiSample.Sample[j].Length, out _);

									// Skip pad bytes
									moduleStream.Read_B_UINT16();

									if (moduleStream.EndOfStream)
									{
										errorMessage = Resources.IDS_FC_ERR_LOADING_SAMPLES;
										Cleanup();

										return AgentResult.Error;
									}

									// Assign sample number
									multiSample.Sample[j].SampleNumber = realSampleNumber++;
									sampNum++;
								}
							}

							// Done, remember the pointer
							sampInfo[i].Multi = multiSample;
						}
						else
						{
							// It's just a normal sample, so seek back to the
							// start of the sample
							moduleStream.Seek(-4, SeekOrigin.Current);

							// Read the sample data
							sampInfo[i].Address = moduleStream.ReadSampleData(i, sampInfo[i].Length, out _);
						}
					}

					// Skip pad bytes
					moduleStream.Read_B_UINT16();

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_FC_ERR_LOADING_SAMPLES;
						Cleanup();

						return AgentResult.Error;
					}
				}

				// Load the wave tables
				moduleStream.Seek(wavOffset, SeekOrigin.Begin);

				for (i = 10; i < (10 + 80); i++)
				{
					if (sampInfo[i].Length != 0)
					{
						// Allocate memory to hold the wave table data
						sampInfo[i].Address = new sbyte[sampInfo[i].Length];

						// Read the wave table
						moduleStream.ReadSigned(sampInfo[i].Address, 0, sampInfo[i].Length);

						if (moduleStream.EndOfStream)
						{
							errorMessage = Resources.IDS_FC_ERR_LOADING_SAMPLES;
							Cleanup();

							return AgentResult.Error;
						}

						// Assign sample number
						sampInfo[i].SampleNumber = realSampleNumber++;
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

			// Initialize speed
			ushort spd = sequences[0].Speed;
			if (spd == 0)
				spd = 3;

			reSpCnt = spd;
			repSpd = spd;
			spdTemp = 1;

			// Initialize other variables
			audTemp = new bool[4];
			audTemp[0] = false;
			audTemp[1] = false;
			audTemp[2] = false;
			audTemp[3] = false;
			
			// Initialize channel variables
			voiceData = new VoiceInfo[4];

			for (int i = 0; i < 4; i++)
			{
				VoiceInfo voiceInfo = new VoiceInfo();

				voiceInfo.PitchBendSpeed = 0;
				voiceInfo.PitchBendTime = 0;
				voiceInfo.SongPos = 0;
				voiceInfo.CurNote = 0;
				voiceInfo.VolumeSeq = volSequences[0].Values;
				voiceInfo.VolumeBendSpeed = 0;
				voiceInfo.VolumeBendTime = 0;
				voiceInfo.VolumeSeqPos = 0;
				voiceInfo.VolumeCounter = 1;
				voiceInfo.VolumeSpeed = 1;
				voiceInfo.VolSusCounter = 0;
				voiceInfo.SusCounter = 0;
				voiceInfo.VibSpeed = 0;
				voiceInfo.VibDepth = 0;
				voiceInfo.VibValue = 0;
				voiceInfo.VibDelay = 0;
				voiceInfo.VolBendFlag = false;
				voiceInfo.PortFlag = false;
				voiceInfo.PatternPos = 0;
				voiceInfo.PitchBendFlag = false;
				voiceInfo.PattTranspose = 0;
				voiceInfo.Volume = 0;
				voiceInfo.VibFlag = 0;
				voiceInfo.Portamento = 0;
				voiceInfo.FrequencySeqStartOffset = 0;
				voiceInfo.FrequencySeqPos = 0;
				voiceInfo.Pitch = 0;
				voiceInfo.Channel = VirtualChannels[i];
				voiceInfo.CurPattern = patterns[sequences[0].VoiceSeq[i].Pattern];
				voiceInfo.Transpose = sequences[0].VoiceSeq[i].Transpose;
				voiceInfo.SoundTranspose = sequences[0].VoiceSeq[i].SoundTranspose;

				voiceData[i] = voiceInfo;
			}

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
			// Decrease replay speed counter
			reSpCnt--;
			if (reSpCnt == 0)
			{
				// Restore replay speed counter
				reSpCnt = repSpd;

				// Get new note for each channel
				NewNote(0);
				NewNote(1);
				NewNote(2);
				NewNote(3);
			}

			// Calculate effects for each channel
			Effect(0);
			Effect(1);
			Effect(2);
			Effect(3);
		}



		/********************************************************************/
		/// <summary>
		/// Return the length of the current song
		/// </summary>
		/********************************************************************/
		public override int SongLength => seqNum;



		/********************************************************************/
		/// <summary>
		/// Return the current position of the song
		/// </summary>
		/********************************************************************/
		public override int GetSongPosition()
		{
			return voiceData[0].SongPos;
		}



		/********************************************************************/
		/// <summary>
		/// Set a new position of the song
		/// </summary>
		/********************************************************************/
		public override void SetSongPosition(int position, PositionInfo positionInfo)
		{
			// Change the position
			voiceData[0].SongPos = (ushort)position;
			voiceData[0].PatternPos = 0;
			voiceData[0].Transpose = sequences[position].VoiceSeq[0].Transpose;
			voiceData[0].SoundTranspose = sequences[position].VoiceSeq[0].SoundTranspose;
			byte pattNum = sequences[position].VoiceSeq[0].Pattern;
			voiceData[0].CurPattern = patterns[pattNum >= patterns.Length ? 0 : pattNum];

			voiceData[1].SongPos = (ushort)position;
			voiceData[1].PatternPos = 0;
			voiceData[1].Transpose = sequences[position].VoiceSeq[1].Transpose;
			voiceData[1].SoundTranspose = sequences[position].VoiceSeq[1].SoundTranspose;
			pattNum = sequences[position].VoiceSeq[1].Pattern;
			voiceData[0].CurPattern = patterns[pattNum >= patterns.Length ? 0 : pattNum];

			voiceData[2].SongPos = (ushort)position;
			voiceData[2].PatternPos = 0;
			voiceData[2].Transpose = sequences[position].VoiceSeq[2].Transpose;
			voiceData[2].SoundTranspose = sequences[position].VoiceSeq[2].SoundTranspose;
			pattNum = sequences[position].VoiceSeq[2].Pattern;
			voiceData[0].CurPattern = patterns[pattNum >= patterns.Length ? 0 : pattNum];

			voiceData[3].SongPos = (ushort)position;
			voiceData[3].PatternPos = 0;
			voiceData[3].Transpose = sequences[position].VoiceSeq[3].Transpose;
			voiceData[3].SoundTranspose = sequences[position].VoiceSeq[3].SoundTranspose;
			pattNum = sequences[position].VoiceSeq[3].Pattern;
			voiceData[0].CurPattern = patterns[pattNum >= patterns.Length ? 0 : pattNum];

			// Set the speed
			reSpCnt = positionInfo.Speed;
			repSpd = reSpCnt;
			spdTemp = 1;

			OnModuleInfoChanged(InfoSpeedLine, repSpd.ToString());

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

				for (int i = 0; i < 10; i++)
				{
					Sample sample = sampInfo[i];

					if (sample.Multi != null)
					{
						for (int j = 0; j < 20; j++)
						{
							if (sample.Multi.Sample[j].Length != 0)
								result.Add(CreateSampleInfo(sample.Multi.Sample[j]));
						}
					}
					else
						result.Add(CreateSampleInfo(sample));
				}

				for (int i = 0; i < wavNum; i++)
				{
					Sample sample = sampInfo[10 + i];

					SampleInfo sampleInfo = CreateSampleInfo(sample);
					sampleInfo.Type = SampleInfo.SampleType.Synth;
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
			return (byte)repSpd;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player have allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			volSequences = null;
			frqSequences = null;
			patterns = null;
			sequences = null;
			sampInfo = null;
		}



		/********************************************************************/
		/// <summary>
		/// Create a sample info structure from the given sample
		/// </summary>
		/********************************************************************/
		private SampleInfo CreateSampleInfo(Sample sample)
		{
			// Build frequency table
			uint[] frequencies = new uint[10 * 12];

			for (int j = 0; j < 6 * 12; j++)
				frequencies[12 + j] = 3546895U / periods[5 * 12 + j];

			SampleInfo sampleInfo = new SampleInfo
			{
				Name = string.Empty,
				Type = SampleInfo.SampleType.Sample,
				BitSize = 8,
				MiddleC = frequencies[12 + 3 * 12],
				Volume = 256,
				Panning = -1,
				NoteFrequencies = frequencies
			};

			// Fill out the rest of the information
			sampleInfo.Sample = sample.Address;
			sampleInfo.Length = sample.Length;

			if (sampleInfo.LoopLength > 2)
			{
				sampleInfo.LoopStart = sample.LoopStart;
				sampleInfo.LoopLength = sample.LoopLength;
				sampleInfo.Flags = SampleInfo.SampleFlags.Loop;
			}
			else
			{
				sampleInfo.LoopStart = 0;
				sampleInfo.LoopLength = 0;
				sampleInfo.Flags = SampleInfo.SampleFlags.None;
			}

			return sampleInfo;
		}



		/********************************************************************/
		/// <summary>
		/// Will play the next row
		/// </summary>
		/********************************************************************/
		private void NewNote(uint chan)
		{
			// Get the voice data
			VoiceInfo voiData = voiceData[chan];

			// Check for end of pattern or "END" mark in pattern
			if ((voiData.PatternPos == 32) || (voiData.CurPattern.PatternRows[voiData.PatternPos].Note == 0x49))
			{
				// New position
				voiData.SongPos++;
				voiData.PatternPos = 0;

				// Have we reached the end of the module
				if (voiData.SongPos == seqNum)
				{
					// We have, wrap around the module
					voiData.SongPos = 0;

					// Tell NostalgicPlayer the module have ended
					OnEndReached();
				}

				// Count the speed counter
				spdTemp++;
				if (spdTemp == 5)
				{
					spdTemp = 1;

					// Get new replay speed
					if (sequences[voiData.SongPos].Speed != 0)
					{
						reSpCnt = sequences[voiData.SongPos].Speed;
						repSpd = reSpCnt;

						// Tell NostalgicPlayer about the speed change
						OnModuleInfoChanged(InfoSpeedLine, repSpd.ToString());
					}

					// Tell NostalgicPlayer we have changed position
					OnPositionChanged();
				}

				// Get pattern information
				voiData.Transpose = sequences[voiData.SongPos].VoiceSeq[chan].Transpose;
				voiData.SoundTranspose = sequences[voiData.SongPos].VoiceSeq[chan].SoundTranspose;

				byte pattNum = sequences[voiData.SongPos].VoiceSeq[chan].Pattern;
				if (pattNum >= patterns.Length)
					pattNum = 0;

				voiData.CurPattern = patterns[pattNum];
			}

			// Get the pattern row
			byte note = voiData.CurPattern.PatternRows[voiData.PatternPos].Note;
			byte info = voiData.CurPattern.PatternRows[voiData.PatternPos].Info;

			// Check to see if we need to make portamento
			//
			// Info = Portamento/Instrument info
			//        Bit 7   = Portamento on
			//        Bit 6   = Portamento off
			//        Bit 5-0 = Instrument number
			//
			// Info in the next row = Portamento value
			//        Bit 7-5 = Always zero
			//        Bit 4   = Up/down
			//        Bit 3-0 = Value
			if ((note != 0) || ((info & 0xc0) != 0))
			{
				if (note != 0)
					voiData.Pitch = 0;

				if ((info & 0x80) != 0)
					voiData.Portamento = voiData.PatternPos < 31 ? voiData.CurPattern.PatternRows[voiData.PatternPos + 1].Info : (byte)0;
				else
					voiData.Portamento = 0;
			}

			// Got any note
			note &= 0x7f;
			if (note != 0)
			{
				voiData.CurNote = (sbyte)note;

				// Mute the channel
				audTemp[chan] = false;
				voiData.Channel.Mute();

				// Find the volume sequence
				byte inst = (byte)((info & 0x3f) + voiData.SoundTranspose);
				if (inst >= volNum)
					inst = 0;
				else
					inst++;

				voiData.VolumeSeqPos = 0;
				voiData.VolumeCounter = volSequences[inst].Speed;
				voiData.VolumeSpeed = volSequences[inst].Speed;
				voiData.VolSusCounter = 0;

				voiData.VibSpeed = volSequences[inst].VibSpeed;
				voiData.VibFlag = 0x40;
				voiData.VibDepth = volSequences[inst].VibDepth;
				voiData.VibValue = volSequences[inst].VibDepth;
				voiData.VibDelay = (sbyte)volSequences[inst].VibDelay;
				voiData.VolumeSeq = volSequences[inst].Values;

				// Find the frequency sequence
				voiData.FrequencySeqStartOffset = (ushort)(silent.Length + volSequences[inst].FrqNumber * 64);
				voiData.FrequencySeqPos = 0;
				voiData.SusCounter = 0;
			}

			// Go to the next pattern row
			voiData.PatternPos++;
		}



		/********************************************************************/
		/// <summary>
		/// Will calculate the effects for one channel and play them
		/// </summary>
		/********************************************************************/
		private void Effect(uint chan)
		{
			// Get the voice data
			VoiceInfo voiData = voiceData[chan];

			// Parse the frequency sequence commands
			bool oneMore;
			do
			{
				// Only loop one time, except if this flag is set later on
				oneMore = false;

				if (voiData.SusCounter != 0)
				{
					voiData.SusCounter--;
					break;
				}

				// Sustain counter is zero, run the next part of the sequence
				ushort seqPoi = (ushort)(voiData.FrequencySeqStartOffset + voiData.FrequencySeqPos);
				if (seqPoi >= frqSequences.Length)
					break;

				bool parseEffect;
				do
				{
					byte dat;

					// Only loop one time, except if this flag is set later on
					parseEffect = false;

					// Get the next command in the sequence
					byte cmd = frqSequences[seqPoi++];

					// Check for end of sequence
					if (cmd == 0xe1)
						break;

					// Check for "loop to other part of sequence" command
					if (cmd == 0xe0)
					{
						dat = (byte)(frqSequences[seqPoi] & 0x3f);

						voiData.FrequencySeqPos = dat;
						seqPoi = (ushort)(voiData.FrequencySeqStartOffset + dat);

						cmd = frqSequences[seqPoi++];
					}

					// Check for all the effects
					switch (cmd)
					{
						// Set wave form
						case 0xe2:
						{
							// Get instrument number
							dat = frqSequences[seqPoi++];

							if (dat < 90)
							{
								if (sampInfo[dat].Address != null)
								{
									voiData.Channel.PlaySample(sampInfo[dat].SampleNumber, sampInfo[dat].Address, 0, sampInfo[dat].Length);
									if (sampInfo[dat].LoopLength > 2)
									{
										if ((sampInfo[dat].LoopStart + sampInfo[dat].LoopLength) > sampInfo[dat].Length)
											voiData.Channel.SetLoop(sampInfo[dat].LoopStart, (uint)(sampInfo[dat].Length - sampInfo[dat].LoopStart));
										else
											voiData.Channel.SetLoop(sampInfo[dat].LoopStart, sampInfo[dat].LoopLength);
									}
								}
							}

							voiData.VolumeSeqPos = 0;
							voiData.VolumeCounter = 1;
							voiData.FrequencySeqPos += 2;
							audTemp[chan] = true;
							break;
						}

						// Set loop
						case 0xe4:
						{
							// Check to see if the channel is active
							if (audTemp[chan])
							{
								// Get instrument number
								dat = frqSequences[seqPoi++];

								if (dat < 90)
									voiData.Channel.SetLoop(sampInfo[dat].Address, sampInfo[dat].LoopStart, sampInfo[dat].LoopLength);

								voiData.FrequencySeqPos += 2;
							}
							break;
						}

						// Set sample
						case 0xe9:
						{
							audTemp[chan] = true;

							// Get instrument number
							dat = frqSequences[seqPoi++];

							if ((dat < 90) && (sampInfo[dat].Multi != null))
							{
								MultiSample mulSamp = sampInfo[dat].Multi;

								// Get multi sample number
								dat = frqSequences[seqPoi++];

								if (dat < 20)
								{
									if (mulSamp.Sample[dat].Address != null)
									{
										voiData.Channel.PlaySample(mulSamp.Sample[dat].SampleNumber, mulSamp.Sample[dat].Address, 0, mulSamp.Sample[dat].Length);
										if (mulSamp.Sample[dat].LoopLength > 2)
										{
											if ((mulSamp.Sample[dat].LoopStart + mulSamp.Sample[dat].LoopLength) > mulSamp.Sample[dat].Length)
												voiData.Channel.SetLoop(mulSamp.Sample[dat].LoopStart, (uint)(mulSamp.Sample[dat].Length - mulSamp.Sample[dat].LoopStart));
											else
												voiData.Channel.SetLoop(mulSamp.Sample[dat].LoopStart, mulSamp.Sample[dat].LoopLength);
										}
									}
								}

								voiData.VolumeSeqPos = 0;
								voiData.VolumeCounter = 1;
							}

							voiData.FrequencySeqPos += 3;
							break;
						}

						// Pattern jump
						case 0xe7:
						{
							parseEffect = true;

							// Get new position
							dat = frqSequences[seqPoi];

							seqPoi = (ushort)(silent.Length + dat * 64);
							if (seqPoi >= frqSequences.Length)
								seqPoi = 0;

							voiData.FrequencySeqStartOffset = seqPoi;
							voiData.FrequencySeqPos = 0;
							break;
						}

						// Pitch bend
						case 0xea:
						{
							voiData.PitchBendSpeed = (sbyte)frqSequences[seqPoi++];
							voiData.PitchBendTime = frqSequences[seqPoi++];
							voiData.FrequencySeqPos += 3;
							break;
						}

						// New sustain
						case 0xe8:
						{
							voiData.SusCounter = frqSequences[seqPoi++];
							voiData.FrequencySeqPos += 2;

							oneMore = true;
							break;
						}

						// New vibrato
						case 0xe3:
						{
							voiData.VibSpeed = (sbyte)frqSequences[seqPoi++];
							voiData.VibDepth = (sbyte)frqSequences[seqPoi++];
							voiData.FrequencySeqPos += 3;
							break;
						}
					}

					if (!parseEffect && !oneMore)
					{
						// Get transpose value
						seqPoi = (ushort)(voiData.FrequencySeqStartOffset + voiData.FrequencySeqPos);
						voiData.PattTranspose = (sbyte)frqSequences[seqPoi];
						voiData.FrequencySeqPos++;
					}
				}
				while (parseEffect);
			}
			while (oneMore);

			// Parse the volume sequence commands
			if (voiData.VolSusCounter != 0)
				voiData.VolSusCounter--;
			else
			{
				if (voiData.VolumeBendTime != 0)
					DoVolBend(voiData);
				else
				{
					voiData.VolumeCounter--;
					if (voiData.VolumeCounter == 0)
					{
						voiData.VolumeCounter = voiData.VolumeSpeed;

						bool parseEffect;
						do
						{
							// Only loop one time, except if this flag is set later on
							parseEffect = false;

							// Check for end of sequence
							if ((voiData.VolumeSeqPos >= voiData.VolumeSeq.Length) || (voiData.VolumeSeq[voiData.VolumeSeqPos] == 0xe1))
								break;

							// Check for all the effects
							switch (voiData.VolumeSeq[voiData.VolumeSeqPos])
							{
								// Volume bend
								case 0xea:
								{
									voiData.VolumeBendSpeed = voiData.VolumeSeq[voiData.VolumeSeqPos + 1];
									voiData.VolumeBendTime = voiData.VolumeSeq[voiData.VolumeSeqPos + 2];
									voiData.VolumeSeqPos += 3;

									DoVolBend(voiData);
									break;
								}

								// New volume sustain
								case 0xe8:
								{
									voiData.VolSusCounter = voiData.VolumeSeq[voiData.VolumeSeqPos + 1];
									voiData.VolumeSeqPos += 2;
									break;
								}

								// Set new position
								case 0xe0:
								{
									voiData.VolumeSeqPos = (ushort)((voiData.VolumeSeq[voiData.VolumeSeqPos + 1] & 0x3f) - 5);

									parseEffect = true;
									break;
								}

								// Set volume
								default:
								{
									voiData.Volume = (sbyte)(voiData.VolumeSeq[voiData.VolumeSeqPos] & 0x7f);		// Revenge of Fracula plays a sample in the beginning with a volume of 0xff
									voiData.VolumeSeqPos++;
									break;
								}
							}
						}
						while (parseEffect);
					}
				}
			}

			// Calculate the period
			sbyte note = voiData.PattTranspose;
			if (note >= 0)
				note += (sbyte)(voiData.CurNote + voiData.Transpose);

			note &= 0x7f;

			// Get the period
			ushort period = note < periods.Length ? periods[note] : (ushort)0;
			byte vibFlag = voiData.VibFlag;

			// Shall we vibrate?
			if (voiData.VibDelay != 0)
				voiData.VibDelay--;
			else
			{
				ushort vibBase = (ushort)(note * 2);
				sbyte vibDep = (sbyte)(voiData.VibDepth * 2);
				sbyte vibVal = voiData.VibValue;

				if (((vibFlag & 0x80) == 0) || ((vibFlag & 0x01) == 0))
				{
					if ((vibFlag & 0x20) != 0)
					{
						vibVal += voiData.VibSpeed;
						if (vibVal >= vibDep)
						{
							vibFlag &= 0b11011111;	// ~0x20
							vibVal = vibDep;
						}
					}
					else
					{
						vibVal -= voiData.VibSpeed;
						if (vibVal < 0)
						{
							vibFlag |= 0x20;
							vibVal = 0;
						}
					}

					voiData.VibValue = vibVal;
				}

				vibDep /= 2;
				vibVal -= vibDep;
				vibBase += 160;

				while (vibBase < 256)
				{
					vibVal *= 2;
					vibBase += 24;
				}

				period += (ushort)vibVal;
			}

			voiData.VibFlag = (byte)(vibFlag ^ 0x01);

			// Do the portamento thing
			voiData.PortFlag = !voiData.PortFlag;
			if (voiData.PortFlag && (voiData.Portamento != 0))
			{
				if (voiData.Portamento <= 31)
					voiData.Pitch -= voiData.Portamento;
				else
					voiData.Pitch += (ushort)(voiData.Portamento & 0x1f);
			}

			// Pitch bend
			voiData.PitchBendFlag = !voiData.PitchBendFlag;
			if (voiData.PitchBendFlag && (voiData.PitchBendTime != 0))
			{
				voiData.PitchBendTime--;
				voiData.Pitch -= (ushort)voiData.PitchBendSpeed;
			}

			period += voiData.Pitch;

			// Check for bounds
			if (period < 113)
				period = 113;
			else
			{
				if (period > 3424)
					period = 3424;
			}

			if (voiData.Volume < 0)
				voiData.Volume = 0;
			else
			{
				if (voiData.Volume > 64)
					voiData.Volume = 64;
			}

			// Play the period
			voiData.Channel.SetAmigaPeriod(period);
			voiData.Channel.SetAmigaVolume((ushort)voiData.Volume);
		}



		/********************************************************************/
		/// <summary>
		/// Will make a volume bend
		/// </summary>
		/********************************************************************/
		private void DoVolBend(VoiceInfo voiData)
		{
			voiData.VolBendFlag = !voiData.VolBendFlag;
			if (voiData.VolBendFlag)
			{
				voiData.VolumeBendTime--;
				voiData.Volume += (sbyte)voiData.VolumeBendSpeed;

				if (voiData.Volume > 64)
				{
					voiData.Volume = 64;
					voiData.VolumeBendTime = 0;
				}
				else
				{
					if (voiData.Volume < 0)
					{
						voiData.Volume = 0;
						voiData.VolumeBendTime = 0;
					}
				}
			}
		}
		#endregion
	}
}
