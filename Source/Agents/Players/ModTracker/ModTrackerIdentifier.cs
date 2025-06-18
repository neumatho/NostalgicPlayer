/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Linq;
using Polycode.NostalgicPlayer.Agent.Player.ModTracker.Containers;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.ModTracker
{
	/// <summary>
	/// Helper class to identify the different formats
	/// </summary>
	internal static class ModTrackerIdentifier
	{
		/// <summary></summary>
		public static readonly byte[] StSynthId1 = [ 0x53, 0x54, 0x31, 0x2e, 0x33, 0x20, 0x4d, 0x6f, 0x64, 0x75, 0x6c, 0x65, 0x49, 0x4e, 0x46, 0x4f ];		// ST1.3 ModuleINFO
		/// <summary></summary>
		public static readonly byte[] StSynthId2 = [ 0x53, 0x54, 0x31, 0x2e, 0x32, 0x20, 0x4d, 0x6f, 0x64, 0x75, 0x6c, 0x65, 0x49, 0x4e, 0x46, 0x4f ];		// ST1.2 ModuleINFO

		/// <summary></summary>
		public static readonly byte[] AsSynthId1 = [ 0x41, 0x75, 0x64, 0x69, 0x6f, 0x53, 0x63, 0x75, 0x6c, 0x70, 0x74, 0x75, 0x72, 0x65, 0x31, 0x30 ];		// AudioSculpture10

		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public static readonly string[] FileExtensions = [ "mod", "adsc", "st26", "ice", "ptm" ];



		/********************************************************************/
		/// <summary>
		/// Tests the module to see which type of module it is
		/// </summary>
		/********************************************************************/
		public static ModuleType TestModule(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			if (moduleStream.Length < 1468)
				return ModuleType.Unknown;

			if (CheckForProTrackerIff(fileInfo.ModuleStream))
				return ModuleType.ProTrackerIff;

			// Check mark
			moduleStream.Seek(1464, SeekOrigin.Begin);
			string mark = moduleStream.ReadMark(4, false);

			if (mark == "MTN\0")
				return ModuleType.SoundTracker26;

			if (mark == "IT10")
				return ModuleType.IceTracker;

			moduleStream.Seek(1080, SeekOrigin.Begin);
			mark = moduleStream.ReadMark();

			// Check the mark for valid characters
			bool valid = (mark.Length == 4) && mark.Select(t => (byte)t).All(byt => (byt >= 32) && (byt <= 127));

			ModuleType retVal;

			if (valid)
				retVal = Check31SamplesModule(fileInfo, mark);
			else
				retVal = Check15SamplesModule(moduleStream);

			return retVal;
		}



		/********************************************************************/
		/// <summary>
		/// Will try to open the synth file
		/// </summary>
		/********************************************************************/
		public static ModuleStream OpenSynthFile(PlayerFileInfo fileInfo, bool addSize)
		{
			ModuleStream moduleStream = fileInfo.Loader?.OpenExtraFileByExtension("nt", addSize);
			if (moduleStream == null)
				moduleStream = fileInfo.Loader?.OpenExtraFileByExtension("as", addSize);

			return moduleStream;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Checks for ProTracker IFF format
		/// </summary>
		/********************************************************************/
		private static bool CheckForProTrackerIff(ModuleStream moduleStream)
		{
			moduleStream.Seek(0, SeekOrigin.Begin);
			if (moduleStream.ReadMark() != "FORM")
				return false;

			moduleStream.Seek(8, SeekOrigin.Begin);
			if (moduleStream.ReadMark() != "MODL")
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Checks for foreign module formats, which should be ignored
		/// </summary>
		/********************************************************************/
		private static bool CheckForeignModuleFormats(ModuleStream moduleStream)
		{
			moduleStream.Seek(0, SeekOrigin.Begin);

			if (moduleStream.ReadMark(12) == "FamiTracker ")
				return true;

			return false;
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
		private static ModuleType Check15SamplesModule(ModuleStream moduleStream)
		{
			// First check for any other module types that may be detected as
			// a 15-samples module in some cases. We want to skip those
			if (CheckForeignModuleFormats(moduleStream))
				return ModuleType.Unknown;

			ModuleType minimumVersion = ModuleType.UltimateSoundTracker10;

			byte[] buf = new byte[22];
			int diskPrefixCount = 0;
			bool hasBigSamples = false;
			int sampleLength = 0;

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
				moduleStream.ReadInto(buf, 0, 22);

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
				int len = moduleStream.Read_B_UINT16() * 2;
				if (len > 9999)
				{
					// 32 KB samples was introduced in Master SoundTracker
					minimumVersion = GetMinimumVersion(minimumVersion, ModuleType.MasterSoundTracker10);
					hasBigSamples = true;
				}

				sampleLength += len;
			}

			if (diskPrefixCount > 0)
				minimumVersion = GetMinimumVersion(minimumVersion, diskPrefixCount == 15 ? ModuleType.UltimateSoundTracker18 : ModuleType.SoundTrackerII);

			// Read the global song speed
			moduleStream.Seek(470, SeekOrigin.Begin);
			byte songLen = moduleStream.Read_UINT8();
			byte initTemp = moduleStream.Read_UINT8();

			if ((songLen == 0) || (songLen > 128))
				return ModuleType.Unknown;

			// Find the patterns used
			byte[] pos = new byte[128];
			moduleStream.ReadInto(pos, 0, 128);

			byte[] usedPatterns = FindUsedPatterns(pos, 128);
			if (usedPatterns.FirstOrDefault(p => p >= 64) != 0)
				return ModuleType.Unknown;

			long calculatedLength = (usedPatterns.Max(p => p) + 1) * 1024 + 0x258 + sampleLength;
			if ((calculatedLength < (moduleStream.Length - 0x640)) || (calculatedLength > (moduleStream.Length + 0x200)))
			{
				// The calculated module length should match almost the actual length
				return ModuleType.Unknown;
			}

			// Scan all patterns to be more precise which version of tracker that has been used
			bool useOldArpeggioEffect = false;
			bool useNewArpeggioEffect = false;
			bool useVolumeSlide = false;
			bool haveEffectD00 = false;
			bool useSpeed = false;
			bool useFilter = false;
			bool gotEmptyColumn = false;

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

						if ((a == 0) && (b == 0) && (c == 0) && (d == 0))
							gotEmptyColumn = true;

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

			if (!gotEmptyColumn)
				return ModuleType.Unknown;

			// Check the global speed
			if ((initTemp != 0) && (initTemp != 0x78) && (diskPrefixCount == 15))
			{
				// Hardcoded for specific modules, which runs too fast and therefore should not be upgraded. I know, it is not a great solution, but it works
				//
				// 1. jjk55
				// 2. Cut it
				if (((songLen != 0x4a) || (initTemp != 0xb3)) && ((songLen != 0x1e) || (initTemp != 0xa0)))
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

			if ((initTemp != 0) && (initTemp < 60) && ((minimumVersion == ModuleType.UltimateSoundTracker10) || (minimumVersion == ModuleType.SoundTrackerII) || (minimumVersion == ModuleType.SoundTrackerVI) || (minimumVersion == ModuleType.MasterSoundTracker10) || (minimumVersion == ModuleType.SoundTracker2x)))
				return ModuleType.Unknown;

			return minimumVersion;
		}



		/********************************************************************/
		/// <summary>
		/// Checks the module to see if it's a 31 samples module
		/// </summary>
		/********************************************************************/
		private static ModuleType Check31SamplesModule(PlayerFileInfo fileInfo, string mark)
		{
			ModuleType retVal = ModuleType.Unknown;

			ModuleStream moduleStream = fileInfo.ModuleStream;

			if ((mark == "M.K.") || (mark == "M!K!") ||
				(mark == "M&K!") ||								// Echobea3.mod and some His Master's Noise modules
				(mark == "LARD") ||								// judgement_day_gvine.mod
				(mark == "NSMS"))								// kingdomofpleasure.mod
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

					if (restartByte == 0x7f)
						retVal = ModuleType.ProTracker;
				}

				// Find the patterns used
				byte[] pos = new byte[128];
				moduleStream.ReadInto(pos, 0, 128);

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

				// Mod's Grave .WOW files have an M.K. signature, but they're actually 8 channel.
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
						return ModuleType.Unknown;
				}

				if (mark != "M&K!")		// Skip check for most likely His Master's Noise format
				{
					// Check the patterns for any BPM speed effects or ExtraEffect effects
					// just to be sure it's not a NoiseTracker module.
					//
					// Also check to see if it's an Unic Tracker module. If so, don't
					// recognize it
					byte[] usedPatterns = FindUsedPatterns(pos, songLen);

					for (int i = 0; i < usedPatterns.Length; i++)
					{
						moduleStream.Seek(1084 + usedPatterns[i] * 1024, SeekOrigin.Begin);

						for (int j = 0; j < 4 * 64; j++)
						{
							byte a = moduleStream.Read_UINT8();
							byte b = moduleStream.Read_UINT8();
							byte c = moduleStream.Read_UINT8();
							byte d = moduleStream.Read_UINT8();

							// Check the data to see if it's not an Unic Tracker module
							//
							// Is sample > 31
							byte s = (byte)((a & 0xf0) | ((c & 0xf0) >> 4));
							if (s > 31)
							{
								retVal = ModuleType.Unknown;
								goto stopLoop;
							}

							// Is pitch between 28 and 856
							uint temp = (((uint)a & 0x0f) << 8) | b;
							if ((temp != 0) && ((temp < 113) || (temp > 856)))
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
									break;
								}

								// This check has been uncommented, because it is not very
								// secure way, e.g. Klisje Paa Klisje was wrongly detected
								// as ProTracker, which it isn't
								//
								// Note 2: Reintroduced this check again, but changed the
								// speed check is over 120 instead of 31. This will fix
								// Santa's Workchip, but some modules which was detected
								// as NoiseTracker will now become ProTracker
								case Effect.SetSpeed:
								{
									if ((d >= 120) && (d < 255))
									{
										retVal = ModuleType.ProTracker;
										goto stopLoop;
									}
									break;
								}

								case Effect.ExtraEffect:
								{
									if (d >= 16)
										retVal = ModuleType.ProTracker;

									break;
								}
							}
						}
					}
stopLoop:
					;
				}

				if ((retVal != ModuleType.Unknown) && (retVal != ModuleType.ProTracker))
				{
					// Well, now we want to be really really sure it's
					// not a NoiseTracker module, so we check the sample
					// information to see if some samples has a fine tune.
					// Also check if ST-xx: is used in any of the sample names
					byte[] buf = new byte[30];
					bool hasDiskPrefix = false;

					moduleStream.Seek(20, SeekOrigin.Begin);

					for (int i = 0; i < 31; i++)
					{
						moduleStream.ReadInto(buf, 0, 30);

						// Check for disk prefix
						if (((buf[0] == 'S') || (buf[0] == 's')) && ((buf[1] == 'T') || (buf[1] == 't')) && (buf[2] == '-') && (buf[5] == ':'))
						{
							// ProTracker does not have ST-xx: prefixes
							hasDiskPrefix = true;
						}

						byte fineTune = buf[24];

						// Some His Master's Noise modules uses the "M&K!" mark, so check
						// for that (this is not the real format, which is "FEST", but I do
						// not know why some download web pages has modules in "M&K!" format)
						if (((fineTune & 0xf0) != 0) && (mark == "M&K!"))
						{
							retVal = ModuleType.HisMastersNoise;
							break;
						}

						if ((fineTune & 0x0f) != 0)
						{
							retVal = ModuleType.ProTracker;
							break;
						}
					}

					if (!hasDiskPrefix && (restartByte >= 120))
						retVal = ModuleType.ProTracker;
				}
			}
			else
			{
				if ((mark == "FLT4") || (mark == "EXO4"))
				{
					// Find out if the file is an Audio Sculpture module
					retVal = CheckSynthFile(fileInfo);
				}
				else if (mark == "FLT8")
					retVal = ModuleType.StarTrekker8;
				else if (mark == "FEST")
					retVal = ModuleType.HisMastersNoise;
			}

			return retVal;
		}



		/********************************************************************/
		/// <summary>
		/// Checks the synth file if any to see what kind of module it is
		/// </summary>
		/********************************************************************/
		private static ModuleType CheckSynthFile(PlayerFileInfo fileInfo)
		{
			using (ModuleStream moduleStream = OpenSynthFile(fileInfo, false))
			{
				// Did we get any file at all
				if (moduleStream != null)
				{
					byte[] id = new byte[16];
					if (moduleStream.Read(id, 0, 16) == 16)
					{
						if (id.SequenceEqual(AsSynthId1))
							return ModuleType.AudioSculpture;
					}
				}
			}

			return ModuleType.StarTrekker;
		}



		/********************************************************************/
		/// <summary>
		/// Find the minimum version
		/// </summary>
		/********************************************************************/
		private static ModuleType GetMinimumVersion(ModuleType type1, ModuleType type2)
		{
			return (ModuleType)Math.Max((int)type1, (int)type2);
		}



		/********************************************************************/
		/// <summary>
		/// Find the patterns to search. Return only the patterns that are
		/// actual played
		/// </summary>
		/********************************************************************/
		private static byte[] FindUsedPatterns(byte[] pos, byte songLen)
		{
			return pos.Take(songLen).Distinct().OrderBy(b => b).ToArray();
		}
		#endregion
	}
}
