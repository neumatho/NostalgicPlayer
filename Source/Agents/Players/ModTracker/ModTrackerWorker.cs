/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.ModTracker.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Extensions;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.ModTracker
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class ModTrackerWorker : ModulePlayerAgentBase
	{
		private static readonly Dictionary<Guid, ModuleType> moduleTypeLookup = new Dictionary<Guid, ModuleType>
		{
			{ ModTracker.Agent1Id, ModuleType.UltimateSoundTracker10 },
			{ ModTracker.Agent2Id, ModuleType.UltimateSoundTracker18 },
			{ ModTracker.Agent3Id, ModuleType.SoundTrackerII },
			{ ModTracker.Agent4Id, ModuleType.SoundTrackerVI },
			{ ModTracker.Agent5Id, ModuleType.SoundTrackerIX },
			{ ModTracker.Agent6Id, ModuleType.MasterSoundTracker10 },
			{ ModTracker.Agent7Id, ModuleType.SoundTracker2x },
			{ ModTracker.Agent8Id, ModuleType.NoiseTracker },
			{ ModTracker.Agent9Id, ModuleType.StarTrekker },
			{ ModTracker.Agent10Id, ModuleType.StarTrekker8 },
			{ ModTracker.Agent11Id, ModuleType.ProTracker },
			{ ModTracker.Agent12Id, ModuleType.FastTracker },
			{ ModTracker.Agent13Id, ModuleType.MultiTracker },
			{ ModTracker.Agent14Id, ModuleType.Octalyser },
			{ ModTracker.Agent15Id, ModuleType.ModsGrave }
		};

		private const int NumberOfNotes = 7 * 12;

		private static readonly byte[] synthId1 = { 0x53, 0x54, 0x31, 0x2e, 0x33, 0x20, 0x4d, 0x6f, 0x64, 0x75, 0x6c, 0x65, 0x49, 0x4e, 0x46, 0x4f };		// ST1.3 ModuleINFO
		private static readonly byte[] synthId2 = { 0x53, 0x54, 0x31, 0x2e, 0x32, 0x20, 0x4d, 0x6f, 0x64, 0x75, 0x6c, 0x65, 0x49, 0x4e, 0x46, 0x4f };		// ST1.2 ModuleINFO
		private static readonly byte[] synthId3 = { 0x41, 0x75, 0x64, 0x69, 0x6f, 0x53, 0x63, 0x75, 0x6c, 0x70, 0x74, 0x75, 0x72, 0x65, 0x31, 0x30 };		// AudioSculpture10

		private readonly ModuleType currentModuleType;
		private bool packed;

		private DurationInfo durInfo;

		private ushort subSongCount;
		private bool endReached;

		private string songName;
		private string[] comment;
		private ushort maxPattern;
		private ushort channelNum;
		private ushort sampleNum;
		private ushort songLength;
		private ushort trackNum;
		private ushort patternLength;
		private ushort restartPos;
		private byte initTempo;

		private ushort minPeriod;
		private ushort maxPeriod;

		private byte[] panning;
		private byte[] positions;

		private Sample[] samples;
		private TrackLine[][] tracks;
		private ushort[] sequences;

		private ModChannel[] channels;

		private ushort songPos;
		private ushort patternPos;
		private ushort breakPos;
		private bool posJumpFlag;
		private bool breakFlag;
		private bool gotJump;
		private bool gotBreak;
		private byte tempo;
		private byte speed;
		private byte counter;
		private byte lowMask;
		private byte pattDelayTime;
		private byte pattDelayTime2;
		private bool patternLoopHandled;	// Indicate if an E6x effect has been handled on the same line. Only used for Atari Octalyser modules

		private AmSample[] amData;

		private readonly Random rnd;

		private const int InfoSpeedLine = 3;
		private const int InfoTempoLine = 4;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModTrackerWorker(Guid typeId)
		{
			if (!moduleTypeLookup.TryGetValue(typeId, out currentModuleType))
				currentModuleType = ModuleType.Unknown;

			rnd = new Random();
		}

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => new [] { "mod", "mtm", "wow" };



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			// Check the module
			ModuleType checkType = TestModule(fileInfo);
			if (checkType == currentModuleType)
				return AgentResult.Ok;

			return AgentResult.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Return some extra information about the format. If it returns
		/// null or an empty string, nothing extra is shown
		/// </summary>
		/********************************************************************/
		public override string ExtraFormatInfo
		{
			get
			{
				if (!packed)
					return null;

				return Resources.IDS_MOD_PACKED;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the name of the module
		/// </summary>
		/********************************************************************/
		public override string ModuleName => songName;



		/********************************************************************/
		/// <summary>
		/// Return the comment separated in lines
		/// </summary>
		/********************************************************************/
		public override string[] Comment => comment;



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
					description = Resources.IDS_MOD_INFODESCLINE0;
					value = songLength.ToString();
					break;
				}

				// Used patterns
				case 1:
				{
					description = Resources.IDS_MOD_INFODESCLINE1;
					value = maxPattern.ToString();
					break;
				}

				// Supported / used samples
				case 2:
				{
					description = Resources.IDS_MOD_INFODESCLINE2;
					value = sampleNum.ToString();
					break;
				}

				// Current speed
				case 3:
				{
					description = Resources.IDS_MOD_INFODESCLINE3;
					value = speed.ToString();
					break;
				}

				// BPM
				case 4:
				{
					description = Resources.IDS_MOD_INFODESCLINE4;
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
			AgentResult retVal;

			// Load the module
			packed = false;

			if (currentModuleType == ModuleType.MultiTracker)
				retVal = LoadMultiTracker(fileInfo, out errorMessage);
			else
				retVal = LoadTracker(fileInfo, out errorMessage);

			return retVal;
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the player
		/// </summary>
		/********************************************************************/
		public override bool InitPlayer(out string errorMessage)
		{
			// Initialize structures
			channels = new ModChannel[channelNum];

			return base.InitPlayer(out errorMessage);
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
			durInfo = durationInfo;

			InitializeSound(durationInfo.StartPosition);

			return base.InitSound(songNumber, durationInfo, out errorMessage);
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the duration for all sub-songs
		/// </summary>
		/********************************************************************/
		public override DurationInfo[] CalculateDuration()
		{
			DurationInfo[] durations = CalculateDurationBySongPosition();

			subSongCount = (ushort)durations.Length;

			return durations;
		}



		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public override void Play()
		{
			patternLoopHandled = false;

			if (speed != 0)				// Only play if speed <> 0
			{
				counter++;				// Count speed counter
				if (counter >= speed)   // Do we have to change pattern line?
				{
					counter = 0;

					if (pattDelayTime2 != 0)	// Pattern delay active
						NoNewAllChannels();
					else
						GetNewNote();

					// Get next pattern line
					patternPos++;
					if (pattDelayTime != 0)   // New pattern delay time
					{
						// Activate the pattern delay
						pattDelayTime2 = pattDelayTime;
						pattDelayTime = 0;
					}

					// Pattern delay routine, jump one line back again
					if (pattDelayTime2 != 0)
					{
						if (--pattDelayTime2 != 0)
							patternPos--;
					}

					// Has the module ended (from a Bxx effect on same position)?
					if (gotJump)
					{
						// If we got both a Bxx and Dxx command
						// on the same line, don't end the module
						// (unless we jump to position 0)
						endReached = !gotBreak || ((songPos == 0xffff) && (breakPos == 0));

						gotJump = false;
					}

					// Make sure that the break flag is always cleared
					gotBreak = false;

					// Pattern break
					if (breakFlag)
					{
						// Calculate new position in the next pattern
						breakFlag = false;
						patternPos = breakPos;
						breakPos = 0;
					}

					// Have we played the whole pattern?
					if (patternPos >= patternLength)
						NextPosition();
				}
				else
					NoNewAllChannels();

				if (posJumpFlag)
					NextPosition();
			}
			else
			{
				NoNewAllChannels();

				if (posJumpFlag)
					NextPosition();
			}

			if (currentModuleType == ModuleType.StarTrekker)
				AmHandler();

			// If we have reached the end of the module, reset speed and tempo
			if (endReached)
			{
				if ((durInfo == null) || (songPos < durInfo.StartPosition) || (songPos >= durInfo.PositionInfo.Length))
				{
					speed = 6;
					ChangeTempo(initTempo);
				}
				else
				{
					PositionInfo posInfo = durInfo.PositionInfo[songPos - durInfo.StartPosition];
					speed = posInfo.Speed;
					ChangeTempo((byte)posInfo.Bpm);
				}

				OnEndReached();
				endReached = false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the module use
		/// </summary>
		/********************************************************************/
		public override int ModuleChannelCount => channelNum;



		/********************************************************************/
		/// <summary>
		/// Return information about sub-songs
		/// </summary>
		/********************************************************************/
		public override SubSongInfo SubSongs => new SubSongInfo(subSongCount, 0);



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
			songPos = (ushort)position;
			patternPos = 0;
			pattDelayTime = 0;
			pattDelayTime2 = 0;

			// Change the speed
			speed = positionInfo.Speed;
			ChangeTempo((byte)positionInfo.Bpm);

			// Change the module info
			OnModuleInfoChanged(InfoSpeedLine, speed.ToString());
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

				for (int i = 0, cnt = samples.Length; i < cnt; i++)
				{
					Sample sample = samples[i];

					SampleInfo sampleInfo = new SampleInfo
					{
						Name = sample.SampleName,
						Flags = sample.LoopLength <= 1 ? SampleInfo.SampleFlags.None : SampleInfo.SampleFlags.Loop,
						Type = (amData?[i] != null) && (amData[i].Mark == 0x414d) ? SampleInfo.SampleType.Synth : SampleInfo.SampleType.Sample,
						BitSize = 8,
						MiddleC = (int)(7093789.2 / (Tables.Periods[sample.FineTune, 3 * 12] * 2)),
						Volume = sample.Volume * 4,
						Panning = -1,
						Sample = sample.Data,
						Length = sample.Length * 2,
						LoopStart = sample.LoopStart * 2,
						LoopLength = sample.LoopLength * 2
					};

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
		protected override void InitDurationCalculation(int startPosition)
		{
			InitializeSound(startPosition);
		}



		/********************************************************************/
		/// <summary>
		/// Return the current speed
		/// </summary>
		/********************************************************************/
		protected override byte GetCurrentSpeed()
		{
			return speed;
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
		/// Tests the current module to see if it's in sound tracker format
		/// </summary>
		/********************************************************************/
		private bool IsSoundTracker()
		{
			return currentModuleType <= ModuleType.SoundTracker2x;
		}



		/********************************************************************/
		/// <summary>
		/// Tests the current module to see if it's in noise tracker or
		/// similar format
		/// </summary>
		/********************************************************************/
		private bool IsNoiseTracker()
		{
			return (currentModuleType >= ModuleType.NoiseTracker) && (currentModuleType <= ModuleType.StarTrekker8);
		}



		/********************************************************************/
		/// <summary>
		/// Tests the current module to see if it's one of the PC trackers
		/// </summary>
		/********************************************************************/
		private bool IsPcTracker()
		{
			return (currentModuleType >= ModuleType.FastTracker) && (currentModuleType <= ModuleType.MultiTracker);
		}



		/********************************************************************/
		/// <summary>
		/// Tests the module to see which type of module it is
		/// </summary>
		/********************************************************************/
		private ModuleType TestModule(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// First check to see if it's a MultiTracker module
			if (moduleStream.Length < 36)
				return ModuleType.Unknown;

			byte[] buf = new byte[22];

			// Check the signature
			moduleStream.Seek(0, SeekOrigin.Begin);
			moduleStream.Read(buf, 0, 4);

			if ((buf[0] == 'M') && (buf[1] == 'T') && (buf[2] == 'M') && (buf[3] == 0x10))
				return ModuleType.MultiTracker;

			// Now check to see if it's a Noise- or ProTracker module
			if (moduleStream.Length < 1084)
				return ModuleType.Unknown;

			// Check mark
			moduleStream.Seek(1080, SeekOrigin.Begin);
			uint mark = moduleStream.Read_B_UINT32();

			// Check the mark for valid characters
			bool nonValid = false;
			uint markCheck = mark;
			for (int i = 0; i < 4; i++)
			{
				byte byt = (byte)(markCheck & 0xff);
				if ((byt < 32) || (byt > 127))
				{
					nonValid = true;
					break;
				}

				markCheck = markCheck >> 8;
			}

			ModuleType retVal;

			if (nonValid)
				retVal = Check15SamplesModule(moduleStream);
			else
				retVal = Check31SamplesModule(moduleStream, mark);

			return retVal;
		}



		/********************************************************************/
		/// <summary>
		/// Checks the module to see if it's a 15 samples SoundTracker
		///
		/// These checks are based on the information found at:
		/// https://resources.openmpt.org/documents/soundtracker_versions.html
		///
		/// Also some checks are inspired by the load function in OpenMPT:
		/// https://source.openmpt.org/svn/openmpt/tags/libopenmpt-0.2.3746-beta3/soundlib/Load_mod.cpp
		/// </summary>
		/********************************************************************/
		private ModuleType Check15SamplesModule(ModuleStream moduleStream)
		{
			// Check for some other formats, that has been recognized wrongly as 15 samples modules
			moduleStream.Seek(0, SeekOrigin.Begin);
			uint mark = moduleStream.Read_B_UINT32();

			if ((mark == 0x52494646) || (mark == 0x464f524d) || (mark == 0x44444d46) || (mark == 0x54464d58))
				return ModuleType.Unknown;

			ModuleType minimumVersion = ModuleType.UltimateSoundTracker10;

			byte[] buf = new byte[22];
			int diskPrefixCount = 0;
			bool hasBigSamples = false;

			// Check all sample names
			//
			// Some modules have non-ascii (invalid) characters in their sample
			// names, so to be able to play those, we only partly check the
			// sample names. Before it has been disabled, but that made some
			// other files to be recognized as SoundTracker modules. That's why
			// I has introduced it again
			for (int i = 0; i < 15; i++)
			{
				moduleStream.Seek(20 + i * 30, SeekOrigin.Begin);
				moduleStream.Read(buf, 0, 22);

				// Now check the name (but only for the first 7)
				if (i < 7)
				{
					for (int j = 2; j < 10; j++)
					{
						if (buf[j] == 0x00)
							break;

						if ((buf[j] < 32) || (buf[j] > 127))
						{
							// Invalid sample name, so not a SoundTracker module
							return ModuleType.Unknown;
						}
					}
				}

				// Check for disk prefix
				if (((buf[0] == 'S') || (buf[0] == 's')) && ((buf[1] == 'T') || (buf[1] == 't')) && (buf[2] == '-') && (buf[5] == ':'))
				{
					// Ultimate SoundTracker 1.8 and D.O.C. SoundTracker IX always have disk number in sample names
					diskPrefixCount++;
				}

				// Check the sample length
				ushort len = moduleStream.Read_B_UINT16();
				if ((len * 2) > 9999)
				{
					// 32 KB samples was introduced in Master SoundTracker
					minimumVersion = GetMinimumVersion(minimumVersion, ModuleType.MasterSoundTracker10);
					hasBigSamples = true;
				}
			}

			if (diskPrefixCount > 0)
				minimumVersion = GetMinimumVersion(minimumVersion, diskPrefixCount == 15 ? ModuleType.UltimateSoundTracker18 : ModuleType.SoundTrackerII);

			// Read the global song speed
			moduleStream.Seek(470, SeekOrigin.Begin);
			byte songLen = moduleStream.Read_UINT8();
			byte temp = moduleStream.Read_UINT8();

			if ((songLen == 0) || (songLen > 128))
				return ModuleType.Unknown;

			// Find the patterns used
			byte[] pos = new byte[128];
			moduleStream.Read(pos, 0, 128);

			byte[] usedPatterns = FindUsedPatterns(pos, songLen);
			if (usedPatterns.FirstOrDefault(p => p > 64) != 0)
				return ModuleType.Unknown;

			// Scan all patterns to be more precise which version of tracker that has been used
			bool useOldArpeggioEffect = false;
			bool useNewArpeggioEffect = false;
			bool useVolumeSlide = false;
			bool haveEffectD00 = false;
			bool useSpeed = false;
			bool useFilter = false;

			for (int i = 0, p = 0; i < usedPatterns.Length; p++)
			{
				if (p == usedPatterns[i])
				{
					int effectDChannel = -1, effectDChannelCount = 0, effectDChannelCount2 = 0;
					int effectEChannel = -1, effectEChannelCount = 0;

					for (int j = 0; j < 4 * 64; j++)
					{
						byte a = moduleStream.Read_UINT8();
						byte b = moduleStream.Read_UINT8();
						byte c = moduleStream.Read_UINT8();
						byte d = moduleStream.Read_UINT8();

						byte effect = (byte)(c & 0x0f);

						if ((j % 4) == effectDChannel)
						{
							if ((a == 0) && (b == 0) && (((c == 0) && (d == 0)) || (effect == 13)))
							{
								effectDChannelCount++;
								if ((effectDChannelCount >= 5) && (effectDChannelCount2 == 0))
								{
									// Since there is a lot of empty space after the last Dxx command,
									// we assume it's supposed to be a pattern break effect.
									//
									// SoundTracker 2.0 is the only one that support this command
									minimumVersion = ModuleType.SoundTracker2x;
									useVolumeSlide = false;
								}
							}
							else
							{
								if ((effect != 13) && (effect != 12))		// This fixes the Loader1 module
								{
									effectDChannel = -1;

									if (!useVolumeSlide)
									{
										// Ok, it may be a pattern break, so upgrade (Italo.Dance has a lot of pattern break)
										minimumVersion = ModuleType.SoundTracker2x;
									}
								}
							}
						}

						if ((j % 4) == effectEChannel)
							effectEChannelCount++;

						switch (effect)
						{
							case 0:
							{
								if (d != 0)
								{
									if (!useOldArpeggioEffect)		// "N.S. Quiz" has a lot of 007 effects, but it is Ultimate version, because it also uses 1xy as arpeggio
									{
										// Seems like it uses arpeggio in command 0xx
										minimumVersion = GetMinimumVersion(minimumVersion, ModuleType.SoundTrackerII);
										useNewArpeggioEffect = true;
									}
								}
								else
								{
									if (((j % 4) == effectEChannel) && (effectEChannelCount == 1) && !useVolumeSlide)
									{
										// Seems like the auto-slide isn't in use, so it could be a "Set filter" instead
										minimumVersion = GetMinimumVersion(minimumVersion, ModuleType.SoundTrackerIX);
										useFilter = true;
									}
								}
								break;
							}

							case 0x1:
							case 0x2:
							{
								if ((d > 0x1f) && !hasBigSamples && !useNewArpeggioEffect)
								{
									// If a 1xx or 2xx effect has a parameter greater than 0x1f, it is
									// assumed to be a Ultimate SoundTracker, except if it has big samples
									minimumVersion = diskPrefixCount > 0 ? ModuleType.UltimateSoundTracker18 : ModuleType.UltimateSoundTracker10;
									useOldArpeggioEffect = true;
								}
								else if ((effect == 1) && (d < 0x03))
								{
									// This doesn't look like an arpeggio
									minimumVersion = GetMinimumVersion(minimumVersion, ModuleType.SoundTrackerII);
								}
								break;
							}

							case 0xb:
							{
								minimumVersion = ModuleType.SoundTracker2x;
								break;
							}

							case 0xc:
							{
								minimumVersion = GetMinimumVersion(minimumVersion, ModuleType.SoundTrackerII);
								break;
							}

							case 0xd:
							{
								if (effectDChannel == -1)
								{
									if (minimumVersion >= ModuleType.MasterSoundTracker10)
									{
										if (d == 0)
										{
											minimumVersion = ModuleType.SoundTracker2x;
											haveEffectD00 = true;
										}
										else
										{
											if (!haveEffectD00)
											{
												if (!useFilter)
												{
													// Looks like a volume slide. Technobob2, Bomberpilot and Noise uses big samples and volume slide, so we prioritize the volume slide (it could be some other SoundTracker clone that is used)
													useVolumeSlide = true;

													minimumVersion = useSpeed ? ModuleType.SoundTrackerVI : ModuleType.SoundTrackerII;
												}
												else
													minimumVersion = ModuleType.SoundTracker2x;
											}
										}
									}
									else
									{
										minimumVersion = GetMinimumVersion(minimumVersion, ModuleType.SoundTrackerII);

										if ((d == 0) && !useVolumeSlide)
										{
											effectDChannel = j % 4;
											effectDChannelCount = 0;
											effectDChannelCount2 = 0;
										}
										else
										{
											// Could be a volume slide
											useVolumeSlide = true;

											effectDChannel = j % 4;
											effectDChannelCount = 0;

											if (minimumVersion > ModuleType.SoundTrackerVI)
												minimumVersion = useSpeed ? ModuleType.SoundTrackerVI : ModuleType.SoundTrackerII;
										}
									}
								}
								else
								{
									if ((j % 4) == effectDChannel)
									{
										effectDChannelCount2++;
										if (effectDChannelCount2 > 2)
										{
											// More than one effect D right after each other, so it is a volume slide
											useVolumeSlide = true;

											effectDChannel = -1;
											effectDChannelCount = 0;

											// Do not clear count2 here
										}
									}
								}
								break;
							}

							case 0xe:
							{
								minimumVersion = GetMinimumVersion(minimumVersion, ModuleType.SoundTrackerII);
								if (d == 1)
								{
									effectEChannel = j % 4;
									effectEChannelCount = 0;
								}
								break;
							}

							case 0xf:
							{
								minimumVersion = GetMinimumVersion(minimumVersion, ModuleType.SoundTrackerVI);
								useSpeed = true;
								break;
							}
						}
					}

					i++;
				}
				else
				{
					// Skip the pattern
					moduleStream.Seek(1024, SeekOrigin.Current);
				}
			}

			// Check the global speed
			if ((temp != 0x78) && (temp != 0) && (diskPrefixCount == 15))
			{
				// Hardcoded for specific modules, which runs too fast and therefore should not be upgraded. I know, it is not a great solution, but it works
				//
				// 1. jjk55
				// 2. Cut it
				if (((songLen != 0x4a) || (temp != 0xb3)) && ((songLen != 0x1e) || (temp != 0xa0)))
				{
					if (minimumVersion > ModuleType.UltimateSoundTracker18)
					{
						// D.O.C. SoundTracker IX reintroduced the variable tempo
						minimumVersion = GetMinimumVersion(minimumVersion, ModuleType.SoundTrackerIX);
					}
					else
					{
						// Ultimate SoundTracker 1.8 adds variable tempo
						minimumVersion = GetMinimumVersion(minimumVersion, ModuleType.UltimateSoundTracker18);
					}
				}
			}

			return minimumVersion;
		}



		/********************************************************************/
		/// <summary>
		/// Checks the module to see if it's a 31 samples module
		/// </summary>
		/********************************************************************/
		private ModuleType Check31SamplesModule(ModuleStream moduleStream, uint mark)
		{
			ModuleType retVal = ModuleType.Unknown;

			if ((mark == 0x4d2e4b2e) || (mark == 0x4d214b21) || (mark == 0x4d264b21))       // M.K. || M!K! || M&K!
			{
				bool maybeWow = true;
				uint totalSampleLength = 0;

				// Now we know it's either a Noise- or ProTracker module, but we
				// need to know exactly which type it is
				retVal = ModuleType.NoiseTracker;

				// Get the length byte
				moduleStream.Seek(950, SeekOrigin.Begin);
				byte songLen = moduleStream.Read_UINT8();

				// Check restart byte
				byte restartByte = moduleStream.Read_UINT8();
				if (restartByte != 0)
				{
					// Mod's Grave .WOW files always use 0x00 for the "restart" byte
					maybeWow = false;

					if (restartByte > 126)
						retVal = ModuleType.ProTracker;
				}

				// Find the patterns used
				byte[] pos = new byte[128];
				moduleStream.Read(pos, 0, 128);

				if (maybeWow)
				{
					// Check the sample lengths and accumulate them
					for (int i = 0; i < 31; i++)
					{
						moduleStream.Seek(20 + i * 30 + 22, SeekOrigin.Begin);

						ushort sampleLength = moduleStream.Read_B_UINT16();
						ushort fineTuneVolume = moduleStream.Read_B_UINT16();

						totalSampleLength += (uint)sampleLength << 1;
						if ((sampleLength != 0) && (fineTuneVolume != 0x0040))
						{
							// Mod's Grave .WOW files are converted from .669 and thus
							// do not have sample fine tune or volume
							maybeWow = false;
							break;
						}
					}
				}

				// Mod's Grave .WOW files have an M.K. signature but they're actually 8 channel.
				// The only way to distinguish them from a 4-channel M.K. file is to check the
				// length of the .MOD against the expected length of a .WOW file with the same
				// number of patterns as this file. To make things harder, Mod's Grave occasionally
				// adds an extra byte to .WOW files and sometimes .MOD authors pad their files.
				// Prior checks for WOW behavior should help eliminate false positives here.
				//
				// Also note the length check relies on counting samples with a length word=1 to work.
				if (maybeWow)
				{
					int maxPat = pos.Max() + 1;

					uint wowLength = (uint)(1084 + totalSampleLength + (maxPat * 64 * 4 * 8));
					if ((moduleStream.Length & ~1) == wowLength)
						retVal = ModuleType.ModsGrave;
				}

				if (retVal != ModuleType.ModsGrave)
				{
					// Check the patterns for any BPM speed effects or ExtraEffect effects
					// just to be sure it's not a NoiseTracker module.
					//
					// Also check to see if it's a UNIC Tracker module. If so, don't
					// recognize it
					moduleStream.Seek(1084, SeekOrigin.Begin);

					byte[] usedPatterns = FindUsedPatterns(pos, songLen);

					for (int i = 0, p = 0; i < usedPatterns.Length; p++)
					{
						if (p == usedPatterns[i])
						{
							for (int j = 0; j < 4 * 64; j++)
							{
								byte a = moduleStream.Read_UINT8();
								byte b = moduleStream.Read_UINT8();
								byte c = moduleStream.Read_UINT8();
								byte d = moduleStream.Read_UINT8();

								// Check the data to see if it's not a UNIC Tracker module
								//
								// Is sample > 31
								byte s = (byte)((a & 0xf0) | ((c & 0xf0) >> 4));
								if (s > 31)
								{
									retVal = ModuleType.Unknown;
									goto stopLoop;
								}

								// Is pitch between 28 and 856 (increased to 1750, because of Oh Yeah.mod)
								uint temp = (((uint)a & 0x0f) << 8) | b;
								if ((temp != 0) && ((temp < 28) || (temp > 1750)))
								{
									retVal = ModuleType.Unknown;
									goto stopLoop;
								}

								Effect effect = (Effect)(c & 0x0f);

								switch (effect)
								{
									case Effect.Tremolo:
									case Effect.SampleOffset:
									{
										retVal = ModuleType.ProTracker;
										goto stopLoop;
									}

									// This check has been uncommented, because it is not very
									// secure way, e.g. Klisje Paa Klisje was wrongly detected
									// as ProTracker, which it isn't
/*									case Effect.SetSpeed:
									{
										if (d > 31)
										{
											retVal = ModuleType.ProTracker;
											goto stopLoop;
										}
										break;
									}
*/
									case Effect.ExtraEffect:
									{
										if (d >= 16)
										{
											retVal = ModuleType.ProTracker;
											goto stopLoop;
										}
										break;
									}
								}
							}

							i++;
						}
					}

stopLoop:
					if ((retVal != ModuleType.Unknown) && (retVal != ModuleType.ProTracker))
					{
						// Well, now we want to be really really sure it's
						// not a NoiseTracker module, so we check the sample
						// information to see if some samples has a fine tune
						moduleStream.Seek(44, SeekOrigin.Begin);

						for (int i = 0; i < 31; i++)
						{
							if ((moduleStream.Read_UINT8() & 0x0f) != 0)
							{
								retVal = ModuleType.ProTracker;
								break;
							}

							// Seek to next sample
							moduleStream.Seek(30 - 1, SeekOrigin.Current);
						}
					}
				}
			}
			else
			{
				if ((mark == 0x464c5434) || (mark == 0x45584f34))			// FLT4 || EXO4
					retVal = ModuleType.StarTrekker;
				else if (mark == 0x464c5438)								// FLT8
					retVal = ModuleType.StarTrekker8;
				else if ((mark == 0x43443831) || (mark == 0x43443631))		// CD81 || CD61
					retVal = ModuleType.Octalyser;
				else if (((mark & 0x00ffffff) == 0x0043484e) || ((mark & 0x0000ffff) == 0x00004348) || ((mark & 0xffffff00) == 0x54445a00))		// \0CHN || \0\0CH || TDZ\0 (this is TakeTracker only)
					retVal = ModuleType.FastTracker;
			}

			return retVal;
		}



		/********************************************************************/
		/// <summary>
		/// Find the minimum version
		/// </summary>
		/********************************************************************/
		private ModuleType GetMinimumVersion(ModuleType type1, ModuleType type2)
		{
			return (ModuleType)Math.Max((int)type1, (int)type2);
		}



		/********************************************************************/
		/// <summary>
		/// Find the patterns to search. Returns only the patterns the are
		/// actual played
		/// </summary>
		/********************************************************************/
		private byte[] FindUsedPatterns(byte[] pos, byte songLen)
		{
			return pos.Take(songLen).Distinct().OrderBy(b => b).ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Will load a tracker module into memory
		/// </summary>
		/********************************************************************/
		private AgentResult LoadTracker(PlayerFileInfo fileInfo, out string errorMessage)
		{
			try
			{
				byte[] buf = new byte[23];

				ModuleStream moduleStream = fileInfo.ModuleStream;
				Encoding encoder = IsPcTracker() || (currentModuleType == ModuleType.ModsGrave) ? EncoderCollection.Dos : currentModuleType == ModuleType.Octalyser ? EncoderCollection.Atari : EncoderCollection.Amiga;

				// Find out the number of samples
				sampleNum = (ushort)((currentModuleType <= ModuleType.SoundTracker2x) ? 15 : 31);

				// Read the song name
				buf[20] = 0x00;
				moduleStream.Read(buf, 0, 20);

				songName = encoder.GetString(buf);
				comment = new string[0];

				// Allocate space to the samples
				samples = new Sample[sampleNum];

				// Read the samples
				for (int i = 0; i < sampleNum; i++)
				{
					Sample sample = samples[i] = new Sample();

					// Read the sample info
					buf[22] = 0x00;
					moduleStream.Read(buf, 0, 22);				// Name of the sample

					ushort length = moduleStream.Read_B_UINT16();			// Length in words
					byte fineTune = moduleStream.Read_UINT8();				// Only the low nibble is used (mask it out and extend the sign)
					byte volume = moduleStream.Read_UINT8();				// The volume
					ushort repeatStart = moduleStream.Read_B_UINT16();		// Number of words from the beginning where the loop starts
					ushort repeatLength = moduleStream.Read_B_UINT16();		// The loop length in words

					if (sampleNum == 15)
					{
						// For all 15 samples modules, the repeat start is in bytes, so convert it to words
						repeatStart /= 2;
					}

					// If repeat length is 1, it is the same as no loop, so reset it
					if (repeatLength == 1)
						repeatLength = 0;

					// Correct "funny" modules
					if (repeatStart > length)
					{
						repeatStart = 0;
						repeatLength = 0;
					}

					if ((repeatStart + repeatLength) > length)
						repeatLength = (ushort)(length - repeatStart);

					// Do the recognized format support fine tune?
					if (IsSoundTracker() || IsNoiseTracker() || (currentModuleType == ModuleType.ModsGrave))
						fineTune = 0;

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_MOD_ERR_LOADING_SAMPLEINFO;
						Cleanup();

						return AgentResult.Error;
					}

					// Put the information into the sample structure
					sample.SampleName = encoder.GetString(buf).RemoveInvalidChars();
					sample.Data = null;
					sample.Length = length;
					sample.LoopStart = repeatStart;
					sample.LoopLength = repeatLength;
					sample.FineTune = (byte)(fineTune & 0x0f);
					sample.Volume = volume;
				}

				// Read more header information
				songLength = moduleStream.Read_UINT8();

				// Make beatwave.mod to work
				if (songLength > 128)
					songLength = 128;

				if ((currentModuleType == ModuleType.NoiseTracker) || (currentModuleType == ModuleType.StarTrekker) || (currentModuleType == ModuleType.StarTrekker8) || (currentModuleType == ModuleType.FastTracker) || (currentModuleType == ModuleType.Octalyser))
				{
					initTempo = 125;
					restartPos = (ushort)(moduleStream.Read_UINT8() & 0x7f);

					if (restartPos >= songLength)
						restartPos = 0;
				}
				else
				{
					if (sampleNum == 15)
					{
						restartPos = 0;
						byte temp = moduleStream.Read_UINT8();

						if (temp < 100)			// 100 is just a guess. Starwars.Remix is playing too slow if used the tempo in the module
							initTempo = 125;
						else
						{
							switch (currentModuleType)
							{
								case ModuleType.UltimateSoundTracker10:
								case ModuleType.SoundTrackerII:
								case ModuleType.SoundTrackerVI:
								case ModuleType.MasterSoundTracker10:
								case ModuleType.SoundTracker2x:
								{
									initTempo = 125;
									break;
								}

								case ModuleType.UltimateSoundTracker18:
								case ModuleType.SoundTrackerIX:
								{
									if (temp != 0x78)
										initTempo = (byte)ConvertSoundTrackerBpm(temp);
									else
										initTempo = (byte)(temp * 25 / 24);			// Some modules won't play correctly if the tempo isn't converted

									break;
								}
							}
						}
					}
					else
					{
						moduleStream.Read_UINT8();
						initTempo = 125;
						restartPos = 0;
					}
				}

				positions = new byte[128];
				moduleStream.Read(positions, 0, 128);

				// All 31 samples modules have a mark
				uint mark = sampleNum == 31 ? moduleStream.Read_B_UINT32() : 0;

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_MOD_ERR_LOADING_HEADER;
					Cleanup();

					return AgentResult.Error;
				}

				// Find the missing information
				patternLength = 64;

				// Find the number of channels used
				if ((currentModuleType == ModuleType.StarTrekker8) || (currentModuleType == ModuleType.ModsGrave))
					channelNum = 8;
				else
				{
					if ((mark & 0xffff00ff) == 0x43440031)			// CD\01
						channelNum = (ushort)(((mark & 0x0000ff00) >> 8) - 0x30);
					else
					{
						if ((mark & 0x00ffffff) == 0x0043484e)			// \0CHN
							channelNum = (ushort)(((mark & 0xff000000) >> 24) - 0x30);
						else
						{
							if ((mark & 0x0000ffff) == 0x00004348)		// \0\0CH
								channelNum = (ushort)((((mark & 0xff000000) >> 24) - 0x30) * 10 + ((mark & 0x00ff0000) >> 16) - 0x30);
							else
							{
								if ((mark & 0xffffff00) == 0x54445a00)	// TDZ\0
									channelNum = (ushort)((mark & 0x000000ff) - 0x30);
								else
									channelNum = 4;
							}
						}
					}
				}

				// If we load a StarTrekker 8-voices module, divide all the
				// pattern numbers by 2
				if (currentModuleType == ModuleType.StarTrekker8)
				{
					for (int i = 0; i < 128; i++)
						positions[i] /= 2;
				}

				// Find the highest pattern number
				maxPattern = 0;

				for (int i = 0; i < 128; i++)
				{
					if (positions[i] > maxPattern)
						maxPattern = positions[i];
				}

				maxPattern++;
				trackNum = (ushort)(maxPattern * channelNum);

				// Find the min and max periods
				if (IsPcTracker())
				{
					minPeriod = 28;
					maxPeriod = 3424;
				}
				else
				{
					minPeriod = 113;
					maxPeriod = 856;
				}

				// Allocate space for the patterns
				tracks = new TrackLine[trackNum][];

				// Read the tracks
				TrackLine[][] line = new TrackLine[channelNum][];

				for (int i = 0; i < trackNum / channelNum; i++)
				{
					// Allocate memory to hold the tracks
					for (int j = 0; j < channelNum; j++)
						line[j] = tracks[i * channelNum + j] = new TrackLine[patternLength];

					if (currentModuleType == ModuleType.StarTrekker8)
					{
						LoadModTracks(moduleStream, line, 0, 4);
						LoadModTracks(moduleStream, line, 4, 4);
					}
					else
						LoadModTracks(moduleStream, line, 0, channelNum);

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_MOD_ERR_LOADING_PATTERNS;
						Cleanup();

						return AgentResult.Error;
					}
				}

				// Allocate memory to hold the sequences
				sequences = new ushort[32 * maxPattern];

				// Calculate the sequence numbers
				for (int i = 0; i < maxPattern; i++)
				{
					for (int j = 0; j < channelNum; j++)
						sequences[i * 32 + j] = (ushort)(i * channelNum + j);
				}

				byte[] decodeTable = null;

				// Read the samples
				for (int i = 0; i < sampleNum; i++)
				{
					// Allocate space to hold the sample
					int length = samples[i].Length * 2;
					if (length != 0)
					{
						sbyte[] sampleBuffer = new sbyte[length];
						samples[i].Data = sampleBuffer;

						using (ModuleStream sampleDataStream = moduleStream.GetSampleDataStream(i, length))
						{
							// Check for Mod Plugin packed samples
							sampleDataStream.Read(buf, 0, 5);

							if ((buf[0] == 0x41) && (buf[1] == 0x44) && (buf[2] == 0x50) && (buf[3] == 0x43) && (buf[4] == 0x4d))
							{
								// It is, so read and depack it
								packed = true;

								// Read a 16 byte buffer with delta values
								sbyte[] compressionTable = new sbyte[16];
								sampleDataStream.ReadSigned(compressionTable, 0, 16);

								if (decodeTable == null)
									decodeTable = new byte[8192];

								sbyte adpcmDelta = 0;
								int offset = 0;
								length /= 2;

								while (length > 0)
								{
									int todo = Math.Min(decodeTable.Length, length);
									int read = sampleDataStream.Read(decodeTable, 0, todo);
									if (read != todo)
									{
										errorMessage = Resources.IDS_MOD_ERR_LOADING_SAMPLES;
										Cleanup();

										return AgentResult.Error;
									}

									for (int j = 0; j < todo; j++)
									{
										byte b = decodeTable[j];

										adpcmDelta += compressionTable[b & 0x0f];
										sampleBuffer[offset++] = adpcmDelta;
										adpcmDelta += compressionTable[(b >> 4) & 0x0f];
										sampleBuffer[offset++] = adpcmDelta;
									}

									length -= todo;
								}

								// Continue with next sample
								continue;
							}

							// It is not, so seek back and read the sample
							sampleDataStream.Seek(-5, SeekOrigin.Current);

							// Check to see if we miss too much from the last sample
							if (sampleDataStream.Length - sampleDataStream.Position < (length - 512))
							{
								errorMessage = Resources.IDS_MOD_ERR_LOADING_SAMPLES;
								Cleanup();

								return AgentResult.Error;
							}

							// Read the sample
							sampleDataStream.ReadSigned(sampleBuffer, 0, length);
						}
					}
				}

				// Ok, we're done, load any extra files if needed
				if (currentModuleType == ModuleType.StarTrekker)
					LoadSynthSamples(fileInfo);

				errorMessage = string.Empty;
				return AgentResult.Ok;
			}
			catch (Exception)
			{
				Cleanup();
				throw;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will load a MultiTracker module into memory
		/// </summary>
		/********************************************************************/
		private AgentResult LoadMultiTracker(PlayerFileInfo fileInfo, out string errorMessage)
		{
			try
			{
				byte[] buf = new byte[23];

				ModuleStream moduleStream = fileInfo.ModuleStream;
				Encoding encoder = EncoderCollection.Dos;

				// Read the header
				moduleStream.Seek(3, SeekOrigin.Begin);

				moduleStream.Read_UINT8();									// File version

				buf[20] = 0x00;
				moduleStream.Read(buf, 0, 20);					// Name of the module
				songName = encoder.GetString(buf);

				trackNum = (ushort)(moduleStream.Read_L_UINT16() + 1);		// Number of tracks. Add one because track 0 is not written but considered empty
				maxPattern = (ushort)(moduleStream.Read_UINT8() + 1);		// Number of patterns
				songLength = (ushort)(moduleStream.Read_UINT8() + 1);		// Length of the song
				ushort commentLength = moduleStream.Read_L_UINT16();		// Length of the comment field
				sampleNum = moduleStream.Read_UINT8();						// Number of samples
				moduleStream.Read_UINT8();									// Attributes -> not used yet
				patternLength = moduleStream.Read_UINT8();					// Number of lines in each pattern
				channelNum = moduleStream.Read_UINT8();						// Number of channels

				panning = new byte[32];
				moduleStream.Read(panning, 0, 32);				// Panning table

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_MOD_ERR_LOADING_HEADER;
					Cleanup();

					return AgentResult.Error;
				}

				// Allocate the sample information and read them
				samples = new Sample[sampleNum];

				for (int i = 0; i < sampleNum; i++)
				{
					Sample sample = samples[i] = new Sample();

					// Read the sample info
					buf[22] = 0x00;
					moduleStream.Read(buf, 0, 22);				// Name of the sample

					uint length = moduleStream.Read_L_UINT32();				// Length in bytes
					uint repeatStart = moduleStream.Read_L_UINT32();		// Number of bytes from the beginning where the loop starts
					uint repeatEnd = moduleStream.Read_L_UINT32();			// Number of bytes from the beginning where the loop ends
					byte fineTune = moduleStream.Read_UINT8();				// Only the low nibble is used (mask it out and extend the sign)
					byte volume = moduleStream.Read_UINT8();				// The volume (0-64)
					moduleStream.Read_UINT8();								// Attributes -> not used

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_MOD_ERR_LOADING_SAMPLEINFO;
						Cleanup();

						return AgentResult.Error;
					}

					// Put the information into the sample structure
					sample.SampleName = encoder.GetString(buf).RemoveInvalidChars();
					sample.Data = null;
					sample.Length = (ushort)(length / 2);
					sample.LoopStart = (ushort)(repeatStart / 2);
					sample.LoopLength = (ushort)(repeatEnd / 2 - sample.LoopStart);
					sample.FineTune = (byte)(fineTune & 0x0f);
					sample.Volume = volume;
				}

				// Read the positions
				positions = new byte[128];
				moduleStream.Read(positions, 0, 128);

				// Allocate memory to hold all the tracks
				tracks = new TrackLine[trackNum][];

				// Generate an empty track
				tracks[0] = new TrackLine[patternLength];
				for (int j = 0; j < patternLength; j++)
					tracks[0][j] = new TrackLine();

				// Read the tracks
				for (int i = 0, cnt = trackNum - 1; i < cnt; i++)
				{
					// Allocate memory to hold the track
					TrackLine[] line = tracks[i + 1] = new TrackLine[patternLength];

					// Now read the track
					for (int j = 0; j < patternLength; j++)
					{
						TrackLine workLine = line[j] = new TrackLine();

						byte a = moduleStream.Read_UINT8();
						byte b = moduleStream.Read_UINT8();
						byte c = moduleStream.Read_UINT8();

						workLine.Note = (byte)(a >> 2);
						if (workLine.Note != 0)
							workLine.Note += 13;

						workLine.Sample = (byte)((a & 0x03) << 4 | ((b & 0xf0) >> 4));
						workLine.Effect = (Effect)(b & 0x0f);
						workLine.EffectArg = c;
					}

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_MOD_ERR_LOADING_PATTERNS;
						Cleanup();

						return AgentResult.Error;
					}
				}

				// Allocate memory to hold the sequences
				sequences = new ushort[32 * maxPattern];

				// Read the sequence data
				moduleStream.ReadArray_L_UINT16s(sequences, 0, sequences.Length);

				// Read the comment field
				comment = moduleStream.ReadCommentBlock(commentLength, 40, encoder);

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_MOD_ERR_LOADING_PATTERNS;
					Cleanup();

					return AgentResult.Error;
				}

				// Read the samples
				for (int i = 0; i < sampleNum; i++)
				{
					// Allocate space to hold the sample
					int length = samples[i].Length * 2;
					if (length != 0)
					{
						// Read the sample
						samples[i].Data = moduleStream.ReadSampleData(i, length, out _);

						if (moduleStream.EndOfStream)
						{
							errorMessage = Resources.IDS_MOD_ERR_LOADING_SAMPLES;
							Cleanup();

							return AgentResult.Error;
						}

						// Convert the sample to signed
						sbyte[] sampleData = samples[i].Data;
						for (int j = 0; j < length; j++)
							sampleData[j] = (sbyte)((byte)sampleData[j] + 0x80);
					}
				}

				// Initialize the rest of the variables used
				minPeriod = 45;
				maxPeriod = 1616;
				restartPos = 0;
				initTempo = 125;

				// Ok, we're done
				errorMessage = string.Empty;
				return AgentResult.Ok;
			}
			catch (Exception)
			{
				Cleanup();
				throw;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will load x number of tracks in MOD format
		/// </summary>
		/********************************************************************/
		private void LoadModTracks(ModuleStream moduleStream, TrackLine[][] tracks, int offset, int channels)
		{
			for (int i = 0; i < patternLength; i++)
			{
				for (int j = 0; j < channels; j++)
				{
					TrackLine workLine = tracks[offset + j][i] = new TrackLine();

					byte a = moduleStream.Read_UINT8();
					byte b = moduleStream.Read_UINT8();
					byte c = moduleStream.Read_UINT8();
					byte d = moduleStream.Read_UINT8();

					ushort note = (ushort)(((a & 0x0f) << 8) | b);

					// Is there any note?
					if (note != 0)
					{
						int n;
						for (n = 0; n < NumberOfNotes; n++)
						{
							if (note >= Tables.Periods[0, n])
								break;		// Found the note number
						}

						workLine.Note = (byte)(n == NumberOfNotes ? n : n + 1);
					}
					else
						workLine.Note = 0;

					workLine.Sample = (byte)((a & 0xf0) | ((c & 0xf0) >> 4));
					workLine.Effect = (Effect)(c & 0x0f);
					workLine.EffectArg = d;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will load the extra synth file used by StarTrekker
		/// </summary>
		/********************************************************************/
		private void LoadSynthSamples(PlayerFileInfo fileInfo)
		{
			using (ModuleStream moduleStream = fileInfo.Loader?.OpenExtraFile("nt"))
			{
				// Did we get any file at all
				if (moduleStream != null)
				{
					byte[] id = new byte[16];
					if ((moduleStream.Read(id, 0, 16) == 16) && (id.SequenceEqual(synthId1) || id.SequenceEqual(synthId2) || id.SequenceEqual(synthId3)))
					{
						amData = new AmSample[31];

						// Skip header
						moduleStream.Seek(144, SeekOrigin.Begin);

						// Load the AM data
						for (int i = 0; i < 31; i++)
						{
							AmSample amSample = new AmSample();

							amSample.Mark = moduleStream.Read_B_UINT16();
							moduleStream.Seek(4, SeekOrigin.Current);
							amSample.StartAmp = moduleStream.Read_B_UINT16();
							amSample.Attack1Level = moduleStream.Read_B_UINT16();
							amSample.Attack1Speed = moduleStream.Read_B_UINT16();
							amSample.Attack2Level = moduleStream.Read_B_UINT16();
							amSample.Attack2Speed = moduleStream.Read_B_UINT16();
							amSample.SustainLevel = moduleStream.Read_B_UINT16();
							amSample.DecaySpeed = moduleStream.Read_B_UINT16();
							amSample.SustainTime = moduleStream.Read_B_UINT16();
							moduleStream.Seek(2, SeekOrigin.Current);
							amSample.ReleaseSpeed = moduleStream.Read_B_UINT16();
							amSample.Waveform = moduleStream.Read_B_UINT16();
							amSample.PitchFall = moduleStream.Read_B_UINT16();
							amSample.VibAmp = (short)moduleStream.Read_B_UINT16();
							amSample.VibSpeed = moduleStream.Read_B_UINT16();
							amSample.baseFreq = moduleStream.Read_B_UINT16();

							moduleStream.Seek(84, SeekOrigin.Current);

							amData[i] = amSample;

							if (moduleStream.EndOfStream)
								break;
						}
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Initialize sound structures
		/// </summary>
		/********************************************************************/
		private void InitializeSound(int startPosition)
		{
			// Initialize all the variables
			endReached = false;

			speed = 6;
			tempo = initTempo;
			patternPos = 0;
			counter = 0;
			songPos = (ushort)startPosition;
			breakPos = 0;
			posJumpFlag = false;
			breakFlag = false;
			gotJump = false;
			gotBreak = false;
			lowMask = 0xff;
			pattDelayTime = 0;
			pattDelayTime2 = 0;

			for (int i = 0; i < channelNum; i++)
			{
				ModChannel modChan = channels[i] = new ModChannel();

				modChan.TrackLine.Note = 0;
				modChan.TrackLine.Sample = 0;
				modChan.TrackLine.Effect = 0;
				modChan.TrackLine.EffectArg = 0;
				modChan.SampleData = null;
				modChan.Offset = 0;
				modChan.Length = 0;
				modChan.LoopStart = 0;
				modChan.LoopLength = 0;
				modChan.StartOffset = 0;
				modChan.Period = 0;
				modChan.FineTune = 0;
				modChan.Volume = 0;
				modChan.TonePortDirec = 0;
				modChan.TonePortSpeed = 0;
				modChan.WantedPeriod = 0;
				modChan.VibratoCmd = 0;
				modChan.VibratoPos = 0;
				modChan.TremoloCmd = 0;
				modChan.TremoloPos = 0;
				modChan.WaveControl = 0;
				modChan.GlissFunk = 0;
				modChan.SampleOffset = 0;
				modChan.PattPos = 0;
				modChan.LoopCount = 0;
				modChan.FunkOffset = 0;
				modChan.WaveStart = 0;
				modChan.AutoSlide = false;
				modChan.AutoSlideArg = 0;
				modChan.AmSample = false;
				modChan.AmToDo = AmToDo.None;
				modChan.SampleNum = 0;
				modChan.CurLevel = 0;
				modChan.VibDegree = 0;
				modChan.SustainCounter = 0;

				if (IsPcTracker())
				{
					if (currentModuleType == ModuleType.MultiTracker)
						modChan.Panning = (ushort)(panning[i] * 16);
					else
						modChan.Panning = (ushort)((((i & 3) == 0) || ((i & 3) == 3)) ? ChannelPanning.Left : ChannelPanning.Right);
				}
				else
					modChan.Panning = 0;
			}

			SetBpmTempo(initTempo);
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player have allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			panning = null;
			positions = null;

			samples = null;
			tracks = null;
			sequences = null;
			channels = null;
		}



		/********************************************************************/
		/// <summary>
		/// Convert a SoundTracker BPM to a real BPM
		/// </summary>
		/********************************************************************/
		private ushort ConvertSoundTrackerBpm(ushort bpm)
		{
			bpm = (ushort)(((709378.92 / 50) * 125) / ((240 - bpm) * 122));
			if (bpm > 255)
				bpm = 255;

			return bpm;
		}



		/********************************************************************/
		/// <summary>
		/// Jumps to the next song position
		/// </summary>
		/********************************************************************/
		private void NextPosition()
		{
			// Initialize the position variables
			patternPos = breakPos;
			breakPos = 0;
			posJumpFlag = false;
			songPos += 1;

			if (songPos >= songLength)
			{
				songPos = restartPos;

				// And the module has repeated
				endReached = true;
			}

			// Position has changed
			OnPositionChanged();
		}



		/********************************************************************/
		/// <summary>
		/// Checks all channels to see if any commands should run
		/// </summary>
		/********************************************************************/
		private void NoNewAllChannels()
		{
			for (int i = 0; i < channelNum; i++)
				CheckEffects(VirtualChannels[i], channels[i]);
		}



		/********************************************************************/
		/// <summary>
		/// Parses the next pattern line
		/// </summary>
		/********************************************************************/
		private void GetNewNote()
		{
			// Get position information into temporary variables
			ushort curSongPos = songPos;
			ushort curPattPos = patternPos;

			for (int i = 0; i < channelNum; i++)
			{
				// Find the track to use
				ushort trkNum = sequences[positions[curSongPos] * 32 + i];
				TrackLine trackData = tracks[trkNum][curPattPos];

				IChannel chan = VirtualChannels[i];
				ModChannel modChan = channels[i];

				PlayVoice(trackData, chan, modChan);

				// Set volume
				if (!modChan.AmSample)
					chan.SetVolume((ushort)(modChan.Volume * 4));
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parses one pattern line for one channel
		/// </summary>
		/********************************************************************/
		private void PlayVoice(TrackLine trackData, IChannel chan, ModChannel modChan)
		{
			// Check for any note or effect running
			if ((modChan.TrackLine.Note == 0) && (modChan.TrackLine.Sample == 0) && (modChan.TrackLine.Effect == 0) && (modChan.TrackLine.EffectArg == 0))
			{
				// Nothing runs, so set the period
				chan.SetAmigaPeriod(modChan.Period);
			}

			// Copy pattern line to fields in our channel structure
			modChan.TrackLine.Note = trackData.Note;
			modChan.TrackLine.Sample = trackData.Sample;
			modChan.TrackLine.Effect = trackData.Effect;
			modChan.TrackLine.EffectArg = trackData.EffectArg;

			byte sampNum = modChan.TrackLine.Sample;
			if ((sampNum != 0) && (sampNum <= sampleNum))
			{
				// New sample
				Sample sample = samples[sampNum - 1];

				modChan.SampleNum = sampNum;
				modChan.AmSample = false;

				if (currentModuleType == ModuleType.StarTrekker)
				{
					AmSample amSamp = amData?[sampNum - 1];
					if ((amSamp != null) && (amSamp.Mark == 0x414d))	// AM
					{
						modChan.Volume = (sbyte)(amSamp.StartAmp / 4);
						modChan.AmSample = true;
					}
				}

				modChan.SampleData = sample.Data;
				modChan.Offset = 0;
				modChan.Length = sample.Length;
				modChan.StartOffset = 0;
				modChan.FineTune = sample.FineTune;

				if (!modChan.AmSample)
					modChan.Volume = (sbyte)sample.Volume;

				// Check to see if we got a loop
				if ((sample.LoopStart != 0) && (sample.LoopLength > 1))
				{
					// We have, now check the player mode
					if (currentModuleType <= ModuleType.SoundTracker2x)
					{
						// Only plays the loop part. Loop start has been converted to words in loader
						modChan.Offset = (ushort)(sample.LoopStart * 2);
						modChan.LoopStart = modChan.Offset;
						modChan.WaveStart = modChan.Offset;
						modChan.Length = sample.LoopLength;
						modChan.LoopLength = modChan.Length;
					}
					else
					{
						modChan.LoopStart = (ushort)(sample.LoopStart * 2);
						modChan.WaveStart = modChan.LoopStart;

						modChan.Length = (ushort)(sample.LoopStart + sample.LoopLength);
						modChan.LoopLength = sample.LoopLength;
					}
				}
				else
				{
					// No loop
					modChan.LoopStart = sample.LoopStart;
					modChan.WaveStart = modChan.LoopStart;
					modChan.LoopLength = sample.LoopLength;
				}

				// Set panning
				if (IsPcTracker())
					chan.SetPanning(modChan.Panning);
			}

			// Check for some commands
			if (modChan.TrackLine.Note != 0)
			{
				// There is a new note to play
				Effect cmd = modChan.TrackLine.Effect;

				if (!modChan.AmSample)
				{
					if ((currentModuleType == ModuleType.SoundTrackerII) || (currentModuleType == ModuleType.SoundTrackerVI))
					{
						if ((Effect2)cmd == Effect2.AutoVolumeSlide)
						{
							modChan.AutoSlide = true;
							modChan.AutoSlideArg = modChan.TrackLine.EffectArg;
						}
						else
						{
							if (modChan.TrackLine.EffectArg == 0)
							{
								modChan.AutoSlide = false;
								modChan.AutoSlideArg = 0;
							}
						}
					}
					else
					{
						if (currentModuleType >= ModuleType.NoiseTracker)
						{
							// Check for SetFineTune
							if ((currentModuleType >= ModuleType.ProTracker) && (cmd == Effect.ExtraEffect) && ((ExtraEffect)(modChan.TrackLine.EffectArg & 0xf0) == ExtraEffect.SetFineTune))
								SetFineTune(modChan);
							else
							{
								switch (cmd)
								{
									case Effect.TonePortamento:
									case Effect.TonePort_VolSlide:
									{
										SetTonePorta(modChan);
										CheckMoreEffects(chan, modChan);
										return;
									}

									case Effect.SampleOffset:
									{
										CheckMoreEffects(chan, modChan);
										break;
									}
								}
							}
						}
					}
				}

				// Set the period
				modChan.Period = Tables.Periods[modChan.FineTune, modChan.TrackLine.Note - 1];

				if (!((currentModuleType >= ModuleType.ProTracker) && (cmd == Effect.ExtraEffect) && ((ExtraEffect)(modChan.TrackLine.EffectArg & 0xf0) == ExtraEffect.NoteDelay)))
				{
					if ((modChan.WaveControl & 4) == 0)
						modChan.VibratoPos = 0;

					if ((modChan.WaveControl & 64) == 0)
						modChan.TremoloPos = 0;

					if (modChan.AmSample)
					{
						// Setup AM sample
						AmSample amSample = amData?[modChan.SampleNum - 1];
						if (amSample != null)
						{
							modChan.SampleData = Tables.AmWaveforms[amSample.Waveform];
							modChan.Offset = 0;
							modChan.StartOffset = 0;
							modChan.Length = 16;
							modChan.LoopStart = 0;
							modChan.LoopLength = 16;

							modChan.AmToDo = AmToDo.Attack1;
							modChan.CurLevel = (short)amSample.StartAmp;
							modChan.VibDegree = 0;
							modChan.Period = (ushort)(modChan.Period << amSample.baseFreq);
						}
					}

					// Fill out the channel
					if (modChan.Length > 0)
					{
						chan.PlaySample(modChan.SampleData, (uint)(modChan.Offset + modChan.StartOffset * 2), (uint)modChan.Length * 2);
						chan.SetAmigaPeriod(modChan.Period);

						// Setup loop
						if (modChan.LoopLength > 0)
							chan.SetLoop(modChan.LoopStart, (uint)modChan.LoopLength * 2);
					}
					else
						chan.Mute();
				}
			}

			CheckMoreEffects(chan, modChan);
		}



		/********************************************************************/
		/// <summary>
		/// Check one channel to see if there are some commands to run
		/// </summary>
		/********************************************************************/
		private void CheckEffects(IChannel chan, ModChannel modChan)
		{
			UpdateFunk(modChan);

			Effect cmd = modChan.TrackLine.Effect;
			if ((cmd != 0) || (modChan.TrackLine.EffectArg != 0))
			{
				if ((currentModuleType == ModuleType.UltimateSoundTracker10) || (currentModuleType == ModuleType.UltimateSoundTracker18))
				{
					switch ((UltraEffect)cmd)
					{
						case UltraEffect.Arpreggio:
						{
							Arpeggio(chan, modChan);
							break;
						}

						case UltraEffect.PitchBend:
						{
							PitchBend(chan, modChan);
							break;
						}
					}
				}
				else if ((currentModuleType == ModuleType.SoundTrackerII) || (currentModuleType == ModuleType.SoundTrackerVI))
				{
					if (modChan.AutoSlide)
						VolumeSlide(modChan, modChan.AutoSlideArg);

					switch ((Effect2)cmd)
					{
						case Effect2.Arpeggio:
//						case Effect2.ModulateVolume:
//						case Effect2.ModulatePeriod:
//						case Effect2.ModulateVolumePeriod:
						{
							// Arpeggio or normal note
							Arpeggio(chan, modChan);
							break;
						}

						case Effect2.SlideUp:
//						case Effect2.ModulateVolumeSlideUp:
//						case Effect2.ModulatePeriodSlideUp:
//						case Effect2.ModulateVolumePeriodSlideUp:
						{
							PortaUp(chan, modChan);
							break;
						}

						case Effect2.SlideDown:
//						case Effect2.ModulateVolumeSlideDown:
//						case Effect2.ModulatePeriodSlideDown:
//						case Effect2.ModulateVolumePeriodSlideDown:
						{
							PortaDown(chan, modChan);
							break;
						}

						case Effect2.VolumeSlide:
						{
							VolumeSlide(modChan, modChan.TrackLine.EffectArg);
							break;
						}
					}
				}
				else
				{
					switch (cmd)
					{
						case Effect.Arpeggio:
						{
							// Arpeggio or normal note
							Arpeggio(chan, modChan);
							break;
						}

						case Effect.SlideUp:
						{
							PortaUp(chan, modChan);
							break;
						}

						case Effect.SlideDown:
						{
							PortaDown(chan, modChan);
							break;
						}

						case Effect.TonePortamento:
						{
							TonePortamento(chan, modChan);
							break;
						}

						case Effect.Vibrato:
						{
							Vibrato(chan, modChan);
							break;
						}

						case Effect.TonePort_VolSlide:
						{
							TonePlusVolSlide(chan, modChan);
							break;
						}

						case Effect.Vibrato_VolSlide:
						{
							VibratoPlusVolSlide(chan, modChan);
							break;
						}

						case Effect.ExtraEffect:
						{
							ECommands(chan, modChan);
							break;
						}

						default:
						{
							chan.SetAmigaPeriod(modChan.Period);

							if (cmd == Effect.Tremolo)
								Tremolo(chan, modChan);
							else
							{
								if (cmd == Effect.VolumeSlide)
									VolumeSlide(modChan, modChan.TrackLine.EffectArg);
							}
							break;
						}
					}
				}
			}

			// Set volume
			if (!modChan.AmSample)
				chan.SetVolume((ushort)(modChan.Volume * 4));
		}



		/********************************************************************/
		/// <summary>
		/// Check one channel to see if there are some commands to run
		/// </summary>
		/********************************************************************/
		private void CheckMoreEffects(IChannel chan, ModChannel modChan)
		{
			switch (currentModuleType)
			{
				case ModuleType.UltimateSoundTracker10:
				case ModuleType.UltimateSoundTracker18:
				{
					chan.SetAmigaPeriod(modChan.Period);
					break;
				}

				case ModuleType.SoundTrackerII:
				case ModuleType.SoundTrackerVI:
				{
					switch ((Effect2)modChan.TrackLine.Effect)
					{
						case Effect2.SetVolume:
						{
							VolumeChange(modChan);
							break;
						}

						case Effect2.SetSpeed:
						{
							SetSpeed(modChan);
							break;
						}
					}
					break;
				}

				case ModuleType.MultiTracker:
				{
					switch (modChan.TrackLine.Effect)
					{
						case Effect.SampleOffset:
						{
							SampleOffset(modChan);
							break;
						}

						case Effect.PosJump:
						{
							PositionJump(modChan);
							break;
						}

						case Effect.SetVolume:
						{
							VolumeChange(modChan);
							break;
						}

						case Effect.PatternBreak:
						{
							PatternBreak(modChan);
							break;
						}

						case Effect.ExtraEffect:
						{
							ECommands(chan, modChan);
							break;
						}

						case Effect.SetSpeed:
						{
							SetSpeed(modChan);
							break;
						}

						case Effect.SlideUp:
						{
							PortaUp(chan, modChan);
							break;
						}

						case Effect.SlideDown:
						{
							PortaDown(chan, modChan);
							break;
						}

						case Effect.TonePortamento:
						{
							TonePortamento(chan, modChan);
							break;
						}

						case Effect.Vibrato:
						{
							Vibrato(chan, modChan);
							break;
						}

						case Effect.TonePort_VolSlide:
						{
							TonePlusVolSlide(chan, modChan);
							break;
						}

						case Effect.Vibrato_VolSlide:
						{
							VibratoPlusVolSlide(chan, modChan);
							break;
						}

						default:
						{
							chan.SetAmigaPeriod(modChan.Period);
							break;
						}
					}
					break;
				}

				case ModuleType.SoundTrackerIX:
				case ModuleType.MasterSoundTracker10:
				{
					switch (modChan.TrackLine.Effect)
					{
						case Effect.SetVolume:
						{
							VolumeChange(modChan);
							break;
						}

						case Effect.ExtraEffect:
						{
							ECommands(chan, modChan);
							break;
						}

						case Effect.SetSpeed:
						{
							SetSpeed(modChan);
							break;
						}

						default:
						{
							chan.SetAmigaPeriod(modChan.Period);
							break;
						}
					}
					break;
				}

				default:
				{
					switch (modChan.TrackLine.Effect)
					{
						case Effect.SampleOffset:
						{
							SampleOffset(modChan);
							break;
						}

						case Effect.PosJump:
						{
							PositionJump(modChan);
							break;
						}

						case Effect.SetVolume:
						{
							VolumeChange(modChan);
							break;
						}

						case Effect.PatternBreak:
						{
							PatternBreak(modChan);
							break;
						}

						case Effect.SetPanning:
						{
							if (IsPcTracker())
								SetPanning(chan, modChan, modChan.TrackLine.EffectArg);

							break;
						}

						case Effect.ExtraEffect:
						{
							ECommands(chan, modChan);
							break;
						}

						case Effect.SetSpeed:
						{
							SetSpeed(modChan);
							break;
						}

						default:
						{
							chan.SetAmigaPeriod(modChan.Period);
							break;
						}
					}
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Check one channel to see if there are some of the extra commands
		/// to run
		/// </summary>
		/********************************************************************/
		private void ECommands(IChannel chan, ModChannel modChan)
		{
			if (currentModuleType >= ModuleType.SoundTrackerIX)
			{
				if (IsSoundTracker() || IsNoiseTracker())
					FilterOnOff(modChan);
				else
				{
					switch ((ExtraEffect)(modChan.TrackLine.EffectArg & 0xf0))
					{
						case ExtraEffect.SetFilter:
						{
							if (!IsPcTracker() && (currentModuleType != ModuleType.Octalyser) && (currentModuleType != ModuleType.ModsGrave))
								FilterOnOff(modChan);

							break;
						}

						case ExtraEffect.FineSlideUp:
						{
							FinePortaUp(chan, modChan);
							break;
						}

						case ExtraEffect.FineSlideDown:
						{
							FinePortaDown(chan, modChan);
							break;
						}

						case ExtraEffect.GlissandoCtrl:
						{
							SetGlissControl(modChan);
							break;
						}

						case ExtraEffect.VibratoWaveform:
						{
							SetVibratoControl(modChan);
							break;
						}

						case ExtraEffect.SetFineTune:
						{
							SetFineTune(modChan);
							break;
						}

						case ExtraEffect.JumpToLoop:
						{
							JumpLoop(modChan);
							break;
						}

						case ExtraEffect.TremoloWaveform:
						{
							SetTremoloControl(modChan);
							break;
						}

						case ExtraEffect.KarplusStrong:
						{
							if ((currentModuleType != ModuleType.Octalyser) && (currentModuleType != ModuleType.ModsGrave))
							{
								if (IsPcTracker())
									SetPanning(chan, modChan, (ushort)((modChan.TrackLine.EffectArg & 0x0f) << 4));
								else
									KarplusStrong(modChan);
							}
							break;
						}

						case ExtraEffect.Retrig:
						{
							RetrigNote(chan, modChan);
							break;
						}

						case ExtraEffect.FineVolSlideUp:
						{
							VolumeFineUp(modChan);
							break;
						}

						case ExtraEffect.FineVolSlideDown:
						{
							VolumeFineDown(modChan);
							break;
						}

						case ExtraEffect.NoteCut:
						{
							NoteCut(modChan);
							break;
						}

						case ExtraEffect.NoteDelay:
						{
							NoteDelay(chan, modChan);
							break;
						}

						case ExtraEffect.PatternDelay:
						{
							PatternDelay(modChan);
							break;
						}

						case ExtraEffect.InvertLoop:
						{
							FunkIt(modChan);
							break;
						}
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Sets the portamento frequency
		/// </summary>
		/********************************************************************/
		private void SetTonePorta(ModChannel modChan)
		{
			ushort period = Tables.Periods[0, modChan.TrackLine.Note - 1];

			int i;
			for (i = 0; i < NumberOfNotes; i++)
			{
				if (Tables.Periods[modChan.FineTune, i] <= period)
				{
					i++;
					break;
				}
			}

			// Decrement counter so it have the right value.
			// This is because if the loop goes all the way through
			i--;

			if ((modChan.FineTune > 7) && (i != 0))
				i--;

			period = Tables.Periods[modChan.FineTune, i];

			modChan.WantedPeriod = period;
			modChan.TonePortDirec = 0;

			if (modChan.Period == period)
				modChan.WantedPeriod = 0;
			else
			{
				if (modChan.Period > period)
					modChan.TonePortDirec = 1;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Updates funk?
		/// </summary>
		/********************************************************************/
		private void UpdateFunk(ModChannel modChan)
		{
			byte glissFunk = (byte)(modChan.GlissFunk >> 4);
			if (glissFunk != 0)
			{
				modChan.FunkOffset += Tables.FunkTable[glissFunk];
				if (modChan.FunkOffset >= 128)
				{
					modChan.FunkOffset = 0;

					ushort waveStart = (ushort)(modChan.WaveStart + 1);
					if (waveStart >= (modChan.LoopStart + modChan.LoopLength * 2))
						waveStart = modChan.LoopStart;

					modChan.WaveStart = waveStart;

					// Invert the sample data
					if (modChan.SampleData != null)
						modChan.SampleData[waveStart] = (sbyte)~modChan.SampleData[waveStart];
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will change the tempo on the module
		/// </summary>
		/********************************************************************/
		private void ChangeTempo(byte newTempo)
		{
			if (newTempo != tempo)
			{
				// BPM speed
				SetBpmTempo(newTempo);

				// Change the module info
				OnModuleInfoChanged(InfoTempoLine, newTempo.ToString());

				// Remember the tempo
				tempo = newTempo;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handles StarTrekker AM samples
		/// </summary>
		/********************************************************************/
		private void AmHandler()
		{
			for (int i = 0; i < channelNum; i++)
			{
				ModChannel modChan = channels[i];

				if (modChan.AmSample)
				{
					AmSample amSamp = amData?[modChan.SampleNum - 1];
					if (amSamp != null)
					{
						IChannel chan = VirtualChannels[i];

						switch (modChan.AmToDo)
						{
							case AmToDo.Attack1:
							{
								if (modChan.CurLevel == amSamp.Attack1Level)
									modChan.AmToDo = AmToDo.Attack2;
								else
								{
									if (modChan.CurLevel < amSamp.Attack1Level)
									{
										modChan.CurLevel += (short)amSamp.Attack1Speed;
										if (modChan.CurLevel >= amSamp.Attack1Level)
										{
											modChan.CurLevel = (short)amSamp.Attack1Level;
											modChan.AmToDo = AmToDo.Attack2;
										}
									}
									else
									{
										modChan.CurLevel -= (short)amSamp.Attack1Speed;
										if (modChan.CurLevel <= amSamp.Attack1Level)
										{
											modChan.CurLevel = (short)amSamp.Attack1Level;
											modChan.AmToDo = AmToDo.Attack2;
										}
									}
								}
								break;
							}

							case AmToDo.Attack2:
							{
								if (modChan.CurLevel == amSamp.Attack2Level)
									modChan.AmToDo = AmToDo.Sustain;
								else
								{
									if (modChan.CurLevel < amSamp.Attack2Level)
									{
										modChan.CurLevel += (short)amSamp.Attack2Speed;
										if (modChan.CurLevel >= amSamp.Attack2Level)
										{
											modChan.CurLevel = (short)amSamp.Attack2Level;
											modChan.AmToDo = AmToDo.Sustain;
										}
									}
									else
									{
										modChan.CurLevel -= (short)amSamp.Attack2Speed;
										if (modChan.CurLevel <= amSamp.Attack2Level)
										{
											modChan.CurLevel = (short)amSamp.Attack2Level;
											modChan.AmToDo = AmToDo.Sustain;
										}
									}
								}
								break;
							}

							case AmToDo.Sustain:
							{
								if (modChan.CurLevel == amSamp.SustainLevel)
								{
									modChan.SustainCounter = (short)amSamp.SustainTime;
									modChan.AmToDo = AmToDo.SustainDecay;
								}
								else
								{
									if (modChan.CurLevel < amSamp.SustainLevel)
									{
										modChan.CurLevel += (short)amSamp.DecaySpeed;
										if (modChan.CurLevel >= amSamp.SustainLevel)
										{
											modChan.CurLevel = (short)amSamp.SustainLevel;
											modChan.SustainCounter = (short)amSamp.SustainTime;
											modChan.AmToDo = AmToDo.SustainDecay;
										}
									}
									else
									{
										modChan.CurLevel -= (short)amSamp.DecaySpeed;
										if (modChan.CurLevel <= amSamp.SustainLevel)
										{
											modChan.CurLevel = (short)amSamp.SustainLevel;
											modChan.SustainCounter = (short)amSamp.SustainTime;
											modChan.AmToDo = AmToDo.SustainDecay;
										}
									}
								}
								break;
							}

							case AmToDo.SustainDecay:
							{
								modChan.SustainCounter--;
								if (modChan.SustainCounter < 0)
									modChan.AmToDo = AmToDo.Release;

								break;
							}

							case AmToDo.Release:
							{
								modChan.CurLevel -= (short)amSamp.ReleaseSpeed;
								if (modChan.CurLevel <= 0)
								{
									modChan.AmToDo = AmToDo.None;
									modChan.CurLevel = 0;
									modChan.AmSample = false;

									chan.Mute();
								}
								break;
							}
						}

						// Set the volume
						chan.SetVolume((ushort)modChan.CurLevel);

						// Do the pitch fall
						modChan.Period += amSamp.PitchFall;

						// Do vibrato
						int vibVal = amSamp.VibAmp;
						if (vibVal != 0)
						{
							bool flag = false;
							ushort degree = modChan.VibDegree;

							if (degree >= 180)
							{
								degree -= 180;
								flag = true;
							}

							vibVal = Tables.AmSinus[degree] * amSamp.VibAmp / 128;
							if (flag)
								vibVal = -vibVal;
						}

						// Set new frequency
						chan.SetAmigaPeriod((uint)(modChan.Period + vibVal));

						modChan.VibDegree += amSamp.VibSpeed;
						if (modChan.VibDegree >= 360)
							modChan.VibDegree -= 360;
					}
				}
			}

			// Generate noise waveform
			for (int i = 0; i < 32; i++)
				Tables.AmWaveforms[3][i] = (sbyte)rnd.Next(255);
		}

		#region Functions to all the normal effects
		/********************************************************************/
		/// <summary>
		/// 0x00 - Plays arpeggio or normal note
		/// 0x01 for Ultimate SoundTracker
		/// </summary>
		/********************************************************************/
		private void Arpeggio(IChannel chan, ModChannel modChan)
		{
			ushort period;

			if (modChan.TrackLine.EffectArg != 0)
			{
				byte arp;
				byte modulus = (byte)(counter % 3);

				switch (modulus)
				{
					case 1:
					{
						arp = (byte)(modChan.TrackLine.EffectArg >> 4);
						break;
					}

					case 2:
					{
						arp = (byte)(modChan.TrackLine.EffectArg & 0x0f);
						break;
					}

					default:
					{
						arp = 0;
						break;
					}
				}

				// Find the index into the period tables
				int i;
				for (i = 0; i < NumberOfNotes; i++)
				{
					if (Tables.Periods[modChan.FineTune, i] <= modChan.Period)
						break;
				}

				// Get the period
				period = Tables.Periods[modChan.FineTune, i + arp >= NumberOfNotes ? NumberOfNotes - 1 : i + arp];
			}
			else
			{
				// Normal note
				period = modChan.Period;
			}

			chan.SetAmigaPeriod(period);
		}



		/********************************************************************/
		/// <summary>
		/// 0x01 - Slides the frequency up or down (Ultimate SoundTracker)
		/// </summary>
		/********************************************************************/
		private void PitchBend(IChannel chan, ModChannel modChan)
		{
			if ((modChan.TrackLine.EffectArg & 0xf0) == 0)
			{
				modChan.Period -= (ushort)(modChan.TrackLine.EffectArg & 0x0f);
				if (modChan.Period < minPeriod)
					modChan.Period = minPeriod;
			}
			else
			{
				modChan.Period += (ushort)((modChan.TrackLine.EffectArg & 0xf0) >> 4);
				if (modChan.Period > maxPeriod)
					modChan.Period = maxPeriod;
			}

			chan.SetAmigaPeriod(modChan.Period);
		}



		/********************************************************************/
		/// <summary>
		/// 0x01 - Slides the frequency up
		/// </summary>
		/********************************************************************/
		private void PortaUp(IChannel chan, ModChannel modChan)
		{
			modChan.Period -= (ushort)(modChan.TrackLine.EffectArg & lowMask);
			if (modChan.Period < minPeriod)
				modChan.Period = minPeriod;

			lowMask = 0xff;

			chan.SetAmigaPeriod(modChan.Period);
		}



		/********************************************************************/
		/// <summary>
		/// 0x02 - Slides the frequency down
		/// </summary>
		/********************************************************************/
		private void PortaDown(IChannel chan, ModChannel modChan)
		{
			modChan.Period += (ushort)(modChan.TrackLine.EffectArg & lowMask);
			if (modChan.Period > maxPeriod)
				modChan.Period = maxPeriod;

			lowMask = 0xff;

			chan.SetAmigaPeriod(modChan.Period);
		}



		/********************************************************************/
		/// <summary>
		/// 0x03 - Slides the frequency to the current note
		/// </summary>
		/********************************************************************/
		private void TonePortamento(IChannel chan, ModChannel modChan, bool skip = false)
		{
			if (!skip)
			{
				if (modChan.TrackLine.EffectArg != 0)
				{
					// Set the slide speed
					modChan.TonePortSpeed = modChan.TrackLine.EffectArg;
					modChan.TrackLine.EffectArg = 0;
				}
			}

			// Is slide mode enabled?
			if (modChan.WantedPeriod != 0)
			{
				int period;

				if (modChan.TonePortDirec != 0)
				{
					// Slide up
					period = modChan.Period - modChan.TonePortSpeed;
					if (modChan.WantedPeriod >= period)
					{
						// Set to the final period and disable slide
						period = modChan.WantedPeriod;
						modChan.WantedPeriod = 0;
					}

					modChan.Period = (ushort)period;
				}
				else
				{
					// Slide down
					period = modChan.Period + modChan.TonePortSpeed;
					if (modChan.WantedPeriod <= period)
					{
						// Set to final period and disable slide
						period = modChan.WantedPeriod;
						modChan.WantedPeriod = 0;
					}

					modChan.Period = (ushort)period;
				}

				// Is glissando enabled?
				if ((modChan.GlissFunk & 0x0f) != 0)
				{
					int i;
					for (i = 0; i < NumberOfNotes; i++)
					{
						if (Tables.Periods[modChan.FineTune, i] <= period)
						{
							i++;
							break;
						}
					}

					period = Tables.Periods[modChan.FineTune, i - 1];
				}

				chan.SetAmigaPeriod((uint)period);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0x04 - Vibrates the frequency
		/// </summary>
		/********************************************************************/
		private void Vibrato(IChannel chan, ModChannel modChan, bool skip = false)
		{
			// Get the effect argument
			byte effArg = modChan.TrackLine.EffectArg;

			// Setup vibrato command
			if (!skip && (effArg != 0))
			{
				byte vibCmd = modChan.VibratoCmd;

				if ((effArg & 0x0f) != 0)
					vibCmd = (byte)((vibCmd & 0xf0) | (effArg & 0x0f));

				if ((effArg & 0xf0) != 0)
					vibCmd = (byte)((vibCmd & 0x0f) | (effArg & 0xf0));

				modChan.VibratoCmd = vibCmd;
			}

			// Calculate new position
			byte vibPos = (byte)((modChan.VibratoPos / 4) & 0x1f);
			byte waveCtrl = (byte)(modChan.WaveControl & 0x03);

			byte addVal;

			if (waveCtrl != 0)
			{
				vibPos *= 8;
				if (waveCtrl != 1)
					addVal = 255;		// Square vibrato
				else
				{
					// Ramp down vibrato
					if (modChan.VibratoPos < 0)
						addVal = (byte)(255 - vibPos);
					else
						addVal = vibPos;
				}
			}
			else
			{
				// Sine vibrato
				addVal = Tables.VibratoTable[vibPos];
			}

			// Set the vibrato
			addVal = (byte)(addVal * (modChan.VibratoCmd & 0x0f) / 128);
			ushort period = modChan.Period;

			if (modChan.VibratoPos < 0)
				period -= addVal;
			else
				period += addVal;

			chan.SetAmigaPeriod(period);

			modChan.VibratoPos += (sbyte)((modChan.VibratoCmd / 4) & 0x3c);
		}



		/********************************************************************/
		/// <summary>
		/// 0x05 - Is both effect 0x03 and 0x0a
		/// </summary>
		/********************************************************************/
		private void TonePlusVolSlide(IChannel chan, ModChannel modChan)
		{
			TonePortamento(chan, modChan, true);
			VolumeSlide(modChan, modChan.TrackLine.EffectArg);
		}



		/********************************************************************/
		/// <summary>
		/// 0x06 - Is both effect 0x04 and 0x0a
		/// </summary>
		/********************************************************************/
		private void VibratoPlusVolSlide(IChannel chan, ModChannel modChan)
		{
			Vibrato(chan, modChan, true);
			VolumeSlide(modChan, modChan.TrackLine.EffectArg);
		}



		/********************************************************************/
		/// <summary>
		/// 0x07 - Makes vibrato on the volume
		/// </summary>
		/********************************************************************/
		private void Tremolo(IChannel chan, ModChannel modChan)
		{
			// Get the effect argument
			byte effArg = modChan.TrackLine.EffectArg;

			// Setup tremolo command
			if (effArg != 0)
			{
				byte treCmd = modChan.TremoloCmd;

				if ((effArg & 0x0f) != 0)
					treCmd = (byte)((treCmd & 0xf0) | (effArg & 0x0f));

				if ((effArg & 0xf0) != 0)
					treCmd = (byte)((treCmd & 0x0f) | (effArg & 0xf0));

				modChan.TremoloCmd = treCmd;
			}

			// Calculate new position
			byte trePos = (byte)((modChan.TremoloPos / 4) & 0x1f);
			byte waveCtrl = (byte)((modChan.WaveControl >> 4) & 0x03);

			byte addVal;

			if (waveCtrl != 0)
			{
				trePos *= 8;
				if (waveCtrl != 1)
					addVal = 255;		// Square vibrato
				else
				{
					// Ramp down tremolo
					if (modChan.TremoloPos < 0)
						addVal = (byte)(255 - trePos);
					else
						addVal = trePos;
				}
			}
			else
			{
				// Sine vibrato
				addVal = Tables.VibratoTable[trePos];
			}

			// Set the tremolo
			addVal = (byte)(addVal * (modChan.TremoloCmd & 0x0f) / 64);
			short volume = modChan.Volume;

			if (modChan.TremoloPos < 0)
			{
				volume -= addVal;
				if (volume < 0)
					volume = 0;
			}
			else
			{
				volume += addVal;
				if (volume > 64)
					volume = 64;
			}

			chan.SetVolume((ushort)(volume * 4));

			modChan.TremoloPos += (sbyte)((modChan.TremoloCmd / 4) & 0x3c);
		}



		/********************************************************************/
		/// <summary>
		/// 0x08 - Set a new panning
		/// </summary>
		/********************************************************************/
		private void SetPanning(IChannel chan, ModChannel modChan, ushort pan)
		{
			modChan.Panning = pan;
			chan.SetPanning(pan);
		}



		/********************************************************************/
		/// <summary>
		/// 0x09 - Starts the sample somewhere else, but the start
		/// </summary>
		/********************************************************************/
		private void SampleOffset(ModChannel modChan)
		{
			// Check for initialize value
			if (modChan.TrackLine.EffectArg != 0)
				modChan.SampleOffset = modChan.TrackLine.EffectArg;

			// Calculate the offset
			ushort offset = (ushort)(modChan.SampleOffset * 128);
			if (offset < modChan.Length)
				modChan.StartOffset = offset;
			else
			{
				modChan.Length = modChan.LoopLength;
				modChan.Offset = modChan.LoopStart;
				modChan.StartOffset = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0x0A - Slides the volume
		/// </summary>
		/********************************************************************/
		private void VolumeSlide(ModChannel modChan, byte effectArg)
		{
			byte spd = (byte)(effectArg >> 4);
			if (spd != 0)
			{
				// Slide up
				modChan.Volume += (sbyte)spd;
				if (modChan.Volume > 64)
					modChan.Volume = 64;
			}
			else
			{
				// Slide down
				modChan.Volume -= (sbyte)(effectArg & 0x0f);
				if (modChan.Volume < 0)
					modChan.Volume = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0x0B - Jumps to another position
		/// </summary>
		/********************************************************************/
		private void PositionJump(ModChannel modChan)
		{
			byte pos = modChan.TrackLine.EffectArg;
			if (pos < songPos)
				endReached = true;		// Module has repeated

			if (pos == songPos)
				gotJump = true;			// Module jump to the same position, maybe end

			// Set the new position
			songPos = (ushort)(pos - 1);
			breakPos = 0;
			posJumpFlag = true;
		}



		/********************************************************************/
		/// <summary>
		/// 0x0C - Sets the channel volume
		/// </summary>
		/********************************************************************/
		private void VolumeChange(ModChannel modChan)
		{
			byte vol = modChan.TrackLine.EffectArg;
			if (vol > 64)
				vol = 64;

			modChan.Volume = (sbyte)vol;
		}



		/********************************************************************/
		/// <summary>
		/// 0x0D - Breaks the pattern and jump to the next position
		/// </summary>
		/********************************************************************/
		private void PatternBreak(ModChannel modChan)
		{
			byte arg = modChan.TrackLine.EffectArg;

			breakPos = (ushort)(((arg >> 4) & 0x0f) * 10 + (arg & 0x0f));
			if (breakPos > 63)
				breakPos = 0;

			posJumpFlag = true;
			gotBreak = true;
		}



		/********************************************************************/
		/// <summary>
		/// 0x0F - Changes the speed of the module
		/// </summary>
		/********************************************************************/
		private void SetSpeed(ModChannel modChan)
		{
			// Get the new speed
			byte newSpeed = modChan.TrackLine.EffectArg;

			if (IsSoundTracker())
			{
				newSpeed &= 0x0f;
				if (newSpeed != 0)
				{
					// Set the new speed
					speed = newSpeed;

					// Change the module info
					OnModuleInfoChanged(InfoSpeedLine, speed.ToString());
				}
			}
			else if (IsNoiseTracker())
			{
				if (newSpeed > 31)
					newSpeed = 31;
				else if (newSpeed == 0)
					newSpeed = 1;

				// Set the new speed
				speed = newSpeed;

				// Change the module info
				OnModuleInfoChanged(InfoSpeedLine, speed.ToString());
			}
			else
			{
				// New trackers
				if (newSpeed > 0)
				{
					if (newSpeed >= 32)
						ChangeTempo(newSpeed);
					else
					{
						// Set the new speed
						speed = newSpeed;
						counter = 0;

						// Change the module info
						OnModuleInfoChanged(InfoSpeedLine, speed.ToString());
					}
				}
				else
				{
					// If speed is 0, we assume the module has ended (this will fix Kenmare River.mod)
					endReached = true;
				}
			}
		}
		#endregion

		#region Functions to all the extended effects
		/********************************************************************/
		/// <summary>
		/// 0xE0 - Changes the filter
		/// </summary>
		/********************************************************************/
		private void FilterOnOff(ModChannel modChan)
		{
			AmigaFilter = (modChan.TrackLine.EffectArg & 0x01) == 0;
		}



		/********************************************************************/
		/// <summary>
		/// 0xE1 - Fine slide the frequency up
		/// </summary>
		/********************************************************************/
		private void FinePortaUp(IChannel chan, ModChannel modChan)
		{
			if (counter == 0)
			{
				lowMask = 0x0f;
				PortaUp(chan, modChan);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0xE2 - Fine slide the frequency down
		/// </summary>
		/********************************************************************/
		private void FinePortaDown(IChannel chan, ModChannel modChan)
		{
			if (counter == 0)
			{
				lowMask = 0x0f;
				PortaDown(chan, modChan);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0xE3 - Sets a new glissando control
		/// </summary>
		/********************************************************************/
		private void SetGlissControl(ModChannel modChan)
		{
			modChan.GlissFunk &= 0xf0;
			modChan.GlissFunk |= (byte)(modChan.TrackLine.EffectArg & 0x0f);
		}



		/********************************************************************/
		/// <summary>
		/// 0xE4 - Sets a new vibrato waveform
		/// </summary>
		/********************************************************************/
		private void SetVibratoControl(ModChannel modChan)
		{
			modChan.WaveControl &= 0xf0;
			modChan.WaveControl |= (byte)(modChan.TrackLine.EffectArg & 0x0f);
		}



		/********************************************************************/
		/// <summary>
		/// 0xE5 - Changes the fine tune
		/// </summary>
		/********************************************************************/
		private void SetFineTune(ModChannel modChan)
		{
			modChan.FineTune = (byte)(modChan.TrackLine.EffectArg & 0x0f);
		}



		/********************************************************************/
		/// <summary>
		/// 0xE6 - Jump to pattern loop position
		/// </summary>
		/********************************************************************/
		private void JumpLoop(ModChannel modChan)
		{
			if (counter == 0)
			{
				byte arg = (byte)(modChan.TrackLine.EffectArg & 0x0f);

				// Atari Octalyser seems to have the loop arguments as global instead
				// of channel separated. At least what I can see from 8er-mod.
				//
				// The replay sources that I got my hand on, does not support E6x, so
				// I can not verify it. However, Dammed Illusion have the same E6x on
				// multiple channels, so that's why the "patternLoopHandled" has been
				// introduced
				if (currentModuleType == ModuleType.Octalyser)
					modChan = channels[0];

				if (arg != 0)
				{
					if (pattDelayTime2 == 0)
					{
						// Jump to the loop currently set
						if (modChan.LoopCount == 0)
						{
							if (!patternLoopHandled)
							{
								modChan.LoopCount = arg;
								patternLoopHandled = true;
							}
						}
						else
						{
							if ((currentModuleType != ModuleType.Octalyser) || !patternLoopHandled)
							{
								modChan.LoopCount--;
								patternLoopHandled = true;
							}
						}

						if (modChan.LoopCount != 0)
						{
							breakPos = modChan.PattPos;
							breakFlag = true;
						}
					}
				}
				else
				{
					// Set the loop start point
					modChan.PattPos = (byte)patternPos;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0xE7 - Sets a new tremolo waveform
		/// </summary>
		/********************************************************************/
		private void SetTremoloControl(ModChannel modChan)
		{
			modChan.WaveControl &= 0x0f;
			modChan.WaveControl |= (byte)((modChan.TrackLine.EffectArg & 0x0f) << 4);
		}



		/********************************************************************/
		/// <summary>
		/// 0xE8 - Karplus strong
		/// </summary>
		/********************************************************************/
		private void KarplusStrong(ModChannel modChan)
		{
			int index = modChan.LoopStart;

			for (int i = modChan.LoopLength * 2 - 2; i >= 0; i--)
			{
				modChan.SampleData[index] = (sbyte)((modChan.SampleData[index] + modChan.SampleData[index + 1]) / 2);
				index++;
			}

			modChan.SampleData[index] = (sbyte)((modChan.SampleData[index] + modChan.SampleData[modChan.LoopStart]) / 2);
		}



		/********************************************************************/
		/// <summary>
		/// 0xE9 - Retrigs the current note
		/// </summary>
		/********************************************************************/
		private void RetrigNote(IChannel chan, ModChannel modChan)
		{
			byte arg = (byte)(modChan.TrackLine.EffectArg & 0x0f);

			if (arg != 0)
			{
				if ((counter != 0) && (modChan.TrackLine.Note == 0))
				{
					if ((counter % arg) == 0)
					{
						// Retrig the sample
						if (modChan.Length != 0)
						{
							chan.PlaySample(modChan.SampleData, 0, (uint)modChan.Length * 2);

							if (modChan.LoopLength != 0)
								chan.SetLoop(modChan.LoopStart, (uint)modChan.LoopLength * 2);

							chan.SetAmigaPeriod(modChan.Period);
						}
						else
							chan.Mute();
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0xEA - Fine slide the volume up
		/// </summary>
		/********************************************************************/
		private void VolumeFineUp(ModChannel modChan)
		{
			if (counter == 0)
			{
				modChan.Volume += (sbyte)(modChan.TrackLine.EffectArg & 0x0f);
				if (modChan.Volume > 64)
					modChan.Volume = 64;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0xEB - Fine slide the volume down
		/// </summary>
		/********************************************************************/
		private void VolumeFineDown(ModChannel modChan)
		{
			if (counter == 0)
			{
				modChan.Volume -= (sbyte)(modChan.TrackLine.EffectArg & 0x0f);
				if (modChan.Volume < 0)
					modChan.Volume = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0xEC - Stops the current note from playing
		/// </summary>
		/********************************************************************/
		private void NoteCut(ModChannel modChan)
		{
			if ((modChan.TrackLine.EffectArg & 0x0f) == counter)
				modChan.Volume = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 0xED - Waits a little while before playing
		/// </summary>
		/********************************************************************/
		private void NoteDelay(IChannel chan, ModChannel modChan)
		{
			if (((modChan.TrackLine.EffectArg & 0x0f) == counter) && (modChan.TrackLine.Note != 0))
			{
				// Retrig the sample
				if (modChan.Length != 0)
				{
					chan.PlaySample(modChan.SampleData, 0, (uint)modChan.Length * 2);

					if (modChan.LoopLength != 0)
						chan.SetLoop(modChan.LoopStart, (uint)modChan.LoopLength * 2);

					chan.SetAmigaPeriod(modChan.Period);
				}
				else
					chan.Mute();
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0xEE - Pauses the pattern for a little while
		/// </summary>
		/********************************************************************/
		private void PatternDelay(ModChannel modChan)
		{
			if ((counter == 0) && (pattDelayTime2 == 0))
				pattDelayTime = (byte)((modChan.TrackLine.EffectArg & 0x0f) + 1);
		}



		/********************************************************************/
		/// <summary>
		/// 0xEF - Inverts the loop
		/// </summary>
		/********************************************************************/
		private void FunkIt(ModChannel modChan)
		{
			if (counter == 0)
			{
				byte arg = (byte)((modChan.TrackLine.EffectArg & 0x0f) << 4);

				modChan.GlissFunk &= 0x0f;
				modChan.GlissFunk |= arg;

				if (arg != 0)
					UpdateFunk(modChan);
			}
		}
		#endregion

		#endregion
	}
}
