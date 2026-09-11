/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using System.Text;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.String;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers.Arrays;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers.Strings;
using Algorithm = Polycode.NostalgicPlayer.Kit.C.Std.Algorithm;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// MOD / NST (ProTracker / NoiseTracker / Startrekker) module loader
	/// </summary>
	internal partial class CSoundFile
	{
		#region OpenMPT loader class
		public class ModLoader : IFormatLoader
		{
			public static readonly FileFormatLoader Format = new FileFormatLoader
			{
				Id = Guid.Parse("0FE3B659-4DF0-4192-AEA0-96376F20296C"),
				Name = Resources.IDS_MPT_MOD_NAME,
				Description = Resources.IDS_MPT_MOD_DESCRIPTION,
				Prober = Probe_OpenMpt,
				Create = Create
			};

			private readonly CSoundFile sndFile;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			private ModLoader(CSoundFile soundFile)
			{
				sndFile = soundFile;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public static ProbeResult Probe_OpenMpt(FileReader file, uint64? pFileSize)
			{
				return ProbeFileHeaderMod(file, InternalFormat.OpenMpt);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public static IFormatLoader Create(CSoundFile soundFile)
			{
				return new ModLoader(soundFile);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public bool Read(FileReader file, ModLoadingFlags loadFlags)
			{
				return sndFile.ReadMod(file, loadFlags);
			}
		}
		#endregion

		#region Inconexia loader class
		public class InconexiaLoader : IFormatLoader
		{
			public static readonly FileFormatLoader Format = new FileFormatLoader
			{
				Id = Guid.Parse("004632FA-B919-4D38-B548-9AD47B21DEA2"),
				Name = Resources.IDS_MPT_INCONEXIA_NAME,
				Description = Resources.IDS_MPT_INCONEXIA_DESCRIPTION,
				Prober = Probe_Inconexia,
				Create = Create
			};

			private readonly CSoundFile sndFile;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			private InconexiaLoader(CSoundFile soundFile)
			{
				sndFile = soundFile;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public static ProbeResult Probe_Inconexia(FileReader file, uint64? pFileSize)
			{
				return ProbeFileHeaderMod(file, InternalFormat.Inconexia);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public static IFormatLoader Create(CSoundFile soundFile)
			{
				return new InconexiaLoader(soundFile);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public bool Read(FileReader file, ModLoadingFlags loadFlags)
			{
				return sndFile.ReadMod(file, loadFlags);
			}
		}
		#endregion

		#region Aleshar loader class
		public class AlesharLoader : IFormatLoader
		{
			public static readonly FileFormatLoader Format = new FileFormatLoader
			{
				Id = Guid.Parse("AD67F62B-C879-4AB0-AAE3-0411BF3A99BB"),
				Name = Resources.IDS_MPT_ALESHAR_NAME,
				Description = Resources.IDS_MPT_ALESHAR_DESCRIPTION,
				Prober = Probe_Aleshar,
				Create = Create
			};

			private readonly CSoundFile sndFile;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			private AlesharLoader(CSoundFile soundFile)
			{
				sndFile = soundFile;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public static ProbeResult Probe_Aleshar(FileReader file, uint64? pFileSize)
			{
				return ProbeFileHeaderMod(file, InternalFormat.Aleshar);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public static IFormatLoader Create(CSoundFile soundFile)
			{
				return new AlesharLoader(soundFile);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public bool Read(FileReader file, ModLoadingFlags loadFlags)
			{
				return sndFile.ReadMod(file, loadFlags);
			}
		}
		#endregion

		/// <summary>
		/// Arbitrary threshold for deciding that 8xx effects are meant as
		/// panning and not just as sync markers
		/// </summary>
		private const uint8 Enable_Mod_Panning_Threshold = 0x38;

		/// <summary>
		/// Minimum number of panning effects needed, before they are
		/// considered as real panning
		/// </summary>
		private const int Minimum_Mod_Panning_Effects = 4;

		/// <summary>
		/// Minimum number of sample names that all have to fill out the
		/// whole name field, before it is taken as a sign of a fixed width
		/// editor field and thus an Octalyser module
		/// </summary>
		private const int Minimum_Filled_Sample_Names = 8;

		private enum InternalFormat
		{
			Unknown,
			OpenMpt,
			Inconexia,
			Aleshar
		}

		private class ModMagicResult
		{
			public string MadeWithTracker;
			public uint32 InvalidByteThreshold = ModSampleHeader.Invalid_Byte_Threshold;
			public uint16 PatternDataOffset = 1084;
			public ChannelIndex NumChannels = 0;
			public bool IsNoiseTracker = false;
			public bool IsStartrekker = false;
			public bool IsGenericMultiChannel = false;
			public bool SetModVBlankTiming = false;
			public bool SwapBytes = false;

			// TNE: Added extra probe flags
			public bool MaybeOpenMpt = false;
			public InternalFormat Format = InternalFormat.Unknown;
		}

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static ProbeResult ProbeFileHeaderMod(FileReader file, InternalFormat format)//XX 247
		{
			if (!file.LengthIsAtLeast(1080 + 4))
				return ProbeResult.WantMoreData;

			file.Seek(1080);

			byte[] magic = new byte[4];
			file.ReadArray(magic);

			ModMagicResult modMagicResult = new ModMagicResult();

			if (!CheckModMagic(magic, modMagicResult))
				return ProbeResult.Failure;

			file.Seek(20);

			uint32 invalidBytes = 0;
			for (SampleIndex smp = 1; smp <= 31; smp++)
			{
				ModSampleHeader sampleHeader = ModTools.ReadAndSwap<ModSampleHeader>.From(file, modMagicResult.SwapBytes);
				invalidBytes += sampleHeader.GetInvalidByteScore();
			}

			if (invalidBytes > modMagicResult.InvalidByteThreshold)
				return ProbeResult.Failure;

			if (modMagicResult.Format == format)
				return ProbeResult.Success;

			if (!modMagicResult.MaybeOpenMpt)
				return ProbeResult.Failure;

			ProbeResult result = ExtendedProbe(file, magic, modMagicResult);

			if ((result == ProbeResult.Success) && (format == InternalFormat.OpenMpt))
				return ProbeResult.Success;

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool ReadMod(FileReader file, ModLoadingFlags loadFlags)
		{
			CPointer<uint8> magic = new CPointer<uint8>(4);

			if (!file.Seek(1080) || !file.ReadArray(magic))
				return false;

			ModMagicResult modMagicResult = new ModMagicResult();

			if (!CheckModMagic(magic, modMagicResult) || (modMagicResult.NumChannels < 1) || (modMagicResult.NumChannels > Snd_Def.Max_BaseChannels))
				return false;

			if (loadFlags == ModLoadingFlags.OnlyVerifyHeader)
				return true;

			InitializeGlobals(ModType.Mod, modMagicResult.NumChannels);

			bool isNoiseTracker = modMagicResult.IsNoiseTracker;
			bool isStartrekker = modMagicResult.IsStartrekker;
			bool isGenericMultiChannel = modMagicResult.IsGenericMultiChannel;
			bool isInconexia = ModTools.IsMagic(magic, "M\0\0\0") || ModTools.IsMagic(magic, "8\0\0\0");

			// A loop length of zero will freeze ProTracker, so assume that modules having such a value were not meant to be played on Amiga. Fixes LHS_MI.MOD
			bool hasRepLen0 = false;

			// Empty sample slots typically should have a default volume of 0 in ProTracker
			bool hasEmptySampleWithVolume = false;

			if (modMagicResult.SetModVBlankTiming)
				m_PlayBehaviour.set(PlayBehaviour.ModVBlankTiming);

			// Startrekker 8 channel mod (needs special treatment, see below)
			bool isFlt8 = isStartrekker && (GetNumChannels() == 8);
			bool isMdKd = ModTools.IsMagic(magic, "M.K.");

			// Adjust finetune values for modules saved with "His Master's Noisetracker"
			bool isHmnt = ModTools.IsMagic(magic, "M&K!") || ModTools.IsMagic(magic, "FEST");
			bool maybeWow = isMdKd;

			// Read song title
			file.Seek(0);
			array<uint8> songTitle = ModTools.ReadAndSwap<String20>.From(file, modMagicResult.SwapBytes).ToArray();
			m_SongName = MptString.ReadBuf(ReadWriteMode.SpacePadded, songTitle);

			// Load Sample Headers
			SmpLength totalSampleLen = 0, wowSampleLen = 0;
			m_nSamples = 31;
			uint32 invalidBytes = 0;
			bool hasLongSamples = false;

			for (SampleIndex smp = 1; smp <= 31; smp++)
			{
				ModSampleHeader sampleHeader = ModTools.ReadAndSwap<ModSampleHeader>.From(file, modMagicResult.SwapBytes);

				invalidBytes += ModTools.ReadModSample(sampleHeader, Samples[smp], m_szNames[smp], GetNumChannels() == 4);
				totalSampleLen += Samples[smp].nLength;

				if (isHmnt)
					Samples[smp].nFineTune = (int8)(-(sampleHeader.FineTune << 3));
				else if (Samples[smp].nLength > 65535)
					hasLongSamples = true;

				if ((sampleHeader.Length != 0) && (sampleHeader.LoopLength == 0))
					hasRepLen0 = true;
				else if ((sampleHeader.Length == 0) && (sampleHeader.Volume == 64))
					hasEmptySampleWithVolume = true;

				if (maybeWow)
				{
					// Some WOW files rely on sample length 1 being counted as well
					wowSampleLen += sampleHeader.Length * 2U;

					// WOW files are converted 669 files, which don't support finetune or default volume
					if (sampleHeader.FineTune != 0)
						maybeWow = false;
					else if ((sampleHeader.Length > 0) && (sampleHeader.Volume != 64))
						maybeWow = false;
				}
			}

			// If there is too much binary garbage in the sample headers, reject the file
			if (invalidBytes > modMagicResult.InvalidByteThreshold)
				return false;

			// Read order information
			ModFileHeader fileHeader = ModTools.ReadAndSwap<ModFileHeader>.From(file, modMagicResult.SwapBytes);

			file.Seek(modMagicResult.PatternDataOffset);

			if (fileHeader.RestartPos > 0)
				maybeWow = false;

			if (!maybeWow)
				wowSampleLen = 0;

			Loaders.ReadOrderFromArray(Order.Current, fileHeader.OrderList.ToArray());

			OrderIndex realOrders = fileHeader.NumOrders;

			if (realOrders > 128)
			{
				// beatwave.mod by Sidewinder claims to have 129 orders. (MD5: 8a029ac498d453beb929db9a73c3c6b4, SHA1: f7b76fb9f477b07a2e78eb10d8624f0df262cde7 - the version from ModArchive, not ModLand)
				realOrders = 128;
			}
			else if (realOrders == 0)
			{
				// Is this necessary?
				realOrders = 128;

				while ((realOrders > 1) && (Order.Current[realOrders - 1] == 0))
					realOrders--;
			}

			// Get number of patterns (including some order list sanity checks)
			PatternIndex numPatterns = ModTools.GetNumPatterns(file, this, realOrders, totalSampleLen, wowSampleLen, false);

			if (maybeWow && (GetNumChannels() == 8))
			{
				// M.K. with 8 channels = Mod's Grave
				modMagicResult.MadeWithTracker = "Mod's Grave";
				isGenericMultiChannel = true;
			}

			if (isFlt8)
			{
				// FLT8 has only even order items, so divide by two
				for (size_t i = 0; i < Order.Current.size(); i++)
					Order.Current[i] /= 2;
			}

			// Restart position sanity checks
			realOrders--;
			Order.Current.SetRestartPos(fileHeader.RestartPos);

			// (Ultimate) Soundtracker didn't have a restart position, but instead stored a default tempo in this value.
			// The default value for this is 0x78 (120 BPM). This is probably the reason why some M.K. modules
			// have this weird restart position. I think I've read somewhere that NoiseTracker actually writes 0x78 there.
			// M.K. files that have restart pos == 0x78: action's batman by DJ Uno, VALLEY.MOD, WormsTDC.MOD, ZWARTZ.MOD
			// Files that have an order list longer than 0x78 with restart pos = 0x78: my_shoe_is_barking.mod, papermix.mod
			// - in both cases it does not appear like the restart position should be used
			if ((fileHeader.RestartPos > realOrders) || ((fileHeader.RestartPos == 0x78) && (GetNumChannels() == 4)))
				Order.Current.SetRestartPos(0);

			Order.Current.SetDefaultSpeed(6);
			Order.Current.SetDefaultTempo(125);
			m_nMinPeriod = 14 * 4;
			m_nMaxPeriod = 3424 * 4;

			// Prevent clipping based on number of channels... If all channels are playing at full volume, "256 / #channels"
			// is the maximum possible sample pre-amp without getting distortion (Compatible mix levels given).
			// The more channels we have, the less likely it is that all of them are used at the same time, though, so cap at 32...
			m_nSamplePreAmp = OpenMpt.Clamp(256U / GetNumChannels(), 32U, 128U);
			m_SongFlags = SongFlags.Format_No_VolCol;	// SONG_ISAMIGA will be set conditionally

			// Setup channel pan positions and volume
			SetupModPanning();

			// Before loading patterns, apply some heuristics:
			// - Scan patterns to check if file could be a NoiseTracker file in disguise.
			//   In this case, the parameter of Dxx commands needs to be ignored (see 1.11song2.mod, 2-3song6.mod).
			// - Use the same code to find notes that would be out-of-range on Amiga.
			// - Detect 7-bit panning and whether 8xx / E8x commands should be interpreted as panning at all.
			bool onlyAmigaNotes = true;
			bool fix7BitPanning = false;
			uint8 maxPanning = 0;		// For detecting 8xx-as-sync

			if (!isNoiseTracker)
			{
				uint32 patternLength = GetNumChannels() * 64U;
				bool leftPanning = false, extendedPanning = false;	// For detecting 800-880 panning
				isNoiseTracker = isMdKd && !hasEmptySampleWithVolume && !hasLongSamples;

				for (PatternIndex pat = 0; pat < numPatterns; pat++)
				{
					uint16 patternBreaks = 0;

					for (uint32 i = 0; i < patternLength; i++)
					{
						ModCommand m = new ModCommand();

						array<uint8> data = ModTools.ReadAndSwap<Array4>.From(file, modMagicResult.SwapBytes && (pat == 0)).ToArray();
						(uint8 command, uint8 param) = ModTools.ReadModPatternEntry(data, m);

						if (!m.IsAmigaNote())
							isNoiseTracker = onlyAmigaNotes = false;

						if (((command > 0x06) && (command < 0x0a)) || ((command == 0x0e) && (param > 0x01)) || ((command == 0x0f) && (param > 0x1f)) || ((command == 0x0d) && (++patternBreaks > 1)))
							isNoiseTracker = false;

						if (command == 0x08)
						{
							// Note: commands 880...88F are not considered for determining the panning style, as some modules use 7-bit panning but slightly overshoot:
							// LOOKATME.MOD (MD5: dedcec1a2a135aeb1a311841cea2c60c, SHA1: 42bf92bf824ef9fb904704b8ee7e3a30df60038d) has an 88A command as its rightmost panning
							maxPanning = Math.Max(maxPanning, param);

							if (param < 0x80)
								leftPanning = true;
							else if ((param > 0x8f) && (param != 0xa4))
								extendedPanning = true;
						}
						else if ((command == 0x0e) && ((param & 0xf0) == 0x80))
							maxPanning = Math.Max(maxPanning, (uint8)((param & 0x0f) << 4));
					}
				}

				fix7BitPanning = leftPanning && !extendedPanning && (maxPanning >= Enable_Mod_Panning_Threshold);
			}

			file.Seek(modMagicResult.PatternDataOffset);

			ChannelIndex readChannels = (ChannelIndex)(isFlt8 ? 4 : GetNumChannels());	// 4 channels per pattern in FLT8 format

			if (isFlt8)
				numPatterns++;	// As one logical pattern consists of two real patterns in FLT8 format, the highest pattern number has to be increased by one

			bool hasTempoCommands = false, definitelyCia = hasLongSamples;	// For detecting VBlank MODs

			// Heuristic for rejecting E0x commands that are most likely not intended to actually toggle the Amiga LED filter, like in naen_leijasi_ptk.mod by ilmarque
			bool filterState = false;
			c_int filterTransitions = 0;

			// Reading patterns
			Patterns.ResizeArray(numPatterns);
			bitset referencedSamples = new bitset(32);

			for (PatternIndex pat = 0; pat < numPatterns; pat++)
			{
				CPointer<ModCommand> rowBase = null;

				if (isFlt8)
				{
					// FLT8: Only create "even" patterns and either write to channel 1 to 4 (even patterns) or 5 to 8 (odd patterns)
					PatternIndex actualPattern = (PatternIndex)(pat / 2U);

					if (((pat % 2U) == 0) && !Patterns.Insert(actualPattern, 64))
						break;

					rowBase = Patterns[actualPattern].GetpModCommand(0, (ChannelIndex)((pat % 2U) == 0 ? 0 : 4));
				}
				else
				{
					if (!Patterns.Insert(pat, 64))
						break;

					rowBase = Patterns[pat].GetpModCommand(0, 0);
				}

				if (rowBase.IsNull || ((loadFlags & ModLoadingFlags.LoadPatternData) == 0))
					break;

				// For detecting PT1x mode
				vector<ModCommandInstr> lastInstrument = new vector<ModCommandInstr>(GetNumChannels(), 0);
				vector<uint8> instrWithoutNoteCount = new vector<uint8>(GetNumChannels(), 0);

				for (RowIndex row = 0; row < 64; row++, rowBase += GetNumChannels())
				{
					// If we have more than one Fxx command on this row and one can be interpreted as speed
					// and the other as tempo, we can be rather sure that it is not a VBlank mod
					bool hasSpeedOnRow = false, hasTempoOnRow = false;

					for (ChannelIndex chn = 0; chn < readChannels; chn++)
					{
						ModCommand m = rowBase[chn];
						array<uint8> data = ModTools.ReadAndSwap<Array4>.From(file, modMagicResult.SwapBytes && (pat == 0)).ToArray();
						(uint8 command, uint8 param) = ModTools.ReadModPatternEntry(data, m);

						if ((command != 0) || (param != 0))
						{
							if (isStartrekker && (command == 0x0e))
							{
								// No support for Startrekker assembly macros
								command = param = 0;
							}
							else if (isStartrekker && (command == 0x0f) && (param > 0x1f))
							{
								// Startrekker caps speed at 31 ticks per row
								param = 0x1f;
							}

							ModTools.ConvertModCommand(m, command, param);
						}

						// Perform some checks for our heuristics...
						if (m.Command == EffectCommand.Tempo)
						{
							hasTempoOnRow = true;

							if (m.Param < 100)
								hasTempoCommands = true;
						}
						else if (m.Command == EffectCommand.Speed)
							hasSpeedOnRow = true;
						else if ((m.Command == EffectCommand.PatternBreak) && isNoiseTracker)
							m.Param = 0;
						else if ((m.Command == EffectCommand.Tremolo) && isHmnt)
							m.Command = EffectCommand.Hmn_Mega_Arp;
						else if ((m.Command == EffectCommand.Panning8) && fix7BitPanning)
						{
							// Fix MODs with 7-bit + surround panning
							if (m.Param == 0xa4)
							{
								m.Command = EffectCommand.S3MCmdEx;
								m.Param = 0x91;
							}
							else
								m.Param = ModCommandParam.CreateSaturating(m.Param * 2);
						}
						else if ((m.Command == EffectCommand.ModCmdEx) && (m.Param < 0x10))
						{
							// Count LED filter transitions
							bool newState = (m.Param & 0x01) == 0;

							if (newState != filterState)
							{
								filterState = newState;
								filterTransitions++;
							}
						}

						if ((m.Note == ModCommand.Note_None) && (m.Instr > 0) && !isFlt8)
						{
							if ((lastInstrument[chn] > 0) && (lastInstrument[chn] != m.Instr))
							{
								// Arbitrary threshold for enabling sample swapping: 4 consecutive "sample swaps" in one pattern
								if (++instrWithoutNoteCount[chn] >= 4)
									m_PlayBehaviour.set(PlayBehaviour.ModSampleSwap);
							}
						}
						else if (m.Note != ModCommand.Note_None)
							instrWithoutNoteCount[chn] = 0;

						if (m.Instr != 0)
						{
							lastInstrument[chn] = m.Instr;

							if (isStartrekker)
								referencedSamples.set((size_t)m.Instr & 0x1f);
						}
					}

					if (hasSpeedOnRow && hasTempoOnRow)
						definitelyCia = true;
				}
			}

			if (onlyAmigaNotes && !hasRepLen0 && (ModTools.IsMagic(magic, "M.K.") || ModTools.IsMagic(magic, "M!K!") || ModTools.IsMagic(magic, "PATT")))
			{
				// M.K. files that don't exceed the Amiga note limit (fixes mod.mothergoose)
				m_SongFlags.Set(SongFlags.AmigaLimits);

				// Need this for professionaltracker.mod by h0ffman (SHA1: 9a7c52cbad73ed2a198ee3fa18d3704ea9f546ff)
				m_SongFlags.Set(SongFlags.Pt_Mode);
				m_PlayBehaviour.set(PlayBehaviour.ModSampleSwap);
				m_PlayBehaviour.set(PlayBehaviour.ModOutOfRangeNoteDelay);
				m_PlayBehaviour.set(PlayBehaviour.ModTempoOnSecondTick);

				// Arbitrary threshold for deciding that 8xx effects are only used as sync markers
				if (maxPanning < Enable_Mod_Panning_Threshold)
				{
					m_PlayBehaviour.set(PlayBehaviour.ModIgnorePanning);

					if (fileHeader.RestartPos != 0x7f)
					{
						// Don't enable these hacks for ScreamTracker modules (restart position = 0x7F), to fix e.g. sample 10 in BASIC001.MOD (SHA1: 11298a5620e677beaa50bd4ed00c3710b75c81af)
						// Note: restart position = 0x7F can also be found in ProTracker modules, e.g. professionaltracker.mod by h0ffman
						m_PlayBehaviour.set(PlayBehaviour.ModOneShotLoops);
					}
				}
			}
			else if (!onlyAmigaNotes && (fileHeader.RestartPos == 0x7f) && isMdKd && ((fileHeader.RestartPos + 1U) >= realOrders))
				modMagicResult.MadeWithTracker = "Scream Tracker";

			if (onlyAmigaNotes && !isGenericMultiChannel && (filterTransitions < 7))
				m_SongFlags.Set(SongFlags.IsAmiga);

			if (isGenericMultiChannel || isMdKd || ModTools.IsMagic(magic, "M!K!"))
				m_PlayBehaviour.set(PlayBehaviour.Ft2ModTremoloRampWaveform);

			if (isInconexia)
				m_PlayBehaviour.set(PlayBehaviour.ModIgnorePanning);

			// Reading samples
			bool anyAdpcm = false;

			if ((loadFlags & ModLoadingFlags.LoadSampleData) != 0)
			{
				file.Seek((size_t)(modMagicResult.PatternDataOffset + ((readChannels * 64 * 4) * numPatterns)));

				for (SampleIndex smp = 1; smp <= 31; smp++)
				{
					ModSample sample = Samples[smp];

					if (sample.nLength != 0)
					{
						SampleIO.Encoding encoding = SampleIO.Encoding.SignedPcm;

						if (isInconexia)
							encoding = SampleIO.Encoding.DeltaPcm;
						else if (file.ReadMagic("ADPCM"))
						{
							encoding = SampleIO.Encoding.Adpcm;
							anyAdpcm = true;
						}

						SampleIO sampleIO = new SampleIO(SampleIO.BitDepth._8Bit, SampleIO.Channels.Mono, SampleIO.Endianness.LittleEndian, encoding);

						// Fix sample 6 in MOD.shorttune2, which has a replen longer than the sample itself.
						// ProTracker reads beyond the end of the sample when playing. Normally samples are
						// adjacent in PT's memory, so we simply read into the next sample in the file.
						// On the other hand, the loop points in Purple Motions's SOUL-O-M.MOD are completely broken and shouldn't be treated like this.
						// As it was most likely written in Scream Tracker, it has empty sample slots with a default volume of 64, which we use for
						// rejecting this quirk for that file
						size_t nextSample = file.GetPosition() + sampleIO.CalculateEncodedSize(sample.nLength);

						if (isMdKd && onlyAmigaNotes && !hasEmptySampleWithVolume)
							sample.nLength = Math.Max(sample.nLength, sample.nLoopEnd);

						sampleIO.ReadSample(sample, file, smp, sample.nLength);
						file.Seek(nextSample);
					}
				}

				// XOR with 0xDF gives the message "TakeTrackered with version 0.9E!!!!!"
				if ((GetNumChannels() <= 16) && (file.ReadMagic("\x8B\xBE\xB4\xBA\x8B\xAD\xBE\xBC\xB4\xBA\xAD\xBA\xBB\xFF\xA8\xB6\xAB\xB7\xFF\xA9\xBA\xAD\xAC\xB6\xB0\xB1\xFF\xEF\xF1\xE6\xBA\xFE\xFE\xFE\xFE\xFE")))
					modMagicResult.MadeWithTracker = "TakeTracker";
				else if (isMdKd && (file.ReadArray<byte>(6) == new array<byte>([ 0x00, 0x11, 0x55, 0x33, 0x22, 0x11 ])) && file.CanRead(3))	// 3 more bytes that differ between modules and Tetramed version, purpose unknown
					modMagicResult.MadeWithTracker = "Tetramed";
			}

			if (((loadFlags & ModLoadingFlags.LoadSampleData) != 0) && isStartrekker && (m_nInstruments == 0))
			{
				uint8 emptySampleReferences = 0;

				for (SampleIndex smp = 1; smp <= 31; smp++)
				{
					if (referencedSamples[smp] && (Samples[smp].nLength == 0))
					{
						if (++emptySampleReferences > 1)
						{
							break;
						}
					}
				}
			}

			// His Master's Noise "Mupp" instrument extensions
			if (((loadFlags & ModLoadingFlags.LoadSampleData) != 0) && isHmnt)
			{
				uint8 muppCount = 0;

				for (SampleIndex smp = 1; smp <= 31; smp++)
				{
					file.Seek((size_t)(20 + ((smp - 1) * Marshal.SizeOf<ModSampleHeader>())));

					if (!file.ReadMagic("Mupp") || !CanAddMoreSamples(28))
						continue;

					if (m_nInstruments == 0)
					{
						m_PlayBehaviour.set(PlayBehaviour.ModSampleSwap);
						m_nInstruments = 31;

						for (InstrumentIndex ins = 1; ins <= 31; ins++)
						{
							ModInstrument instr = AllocateInstrument(ins, ins);

							if (instr != null)
								instr.Name.Assign(m_szNames[ins]);
						}
					}

					ModInstrument instr_ = Instruments[smp];

					if (instr_ == null)
						continue;

					array<uint8> muppHeader = file.ReadArray<uint8>(3);
					uint8 muppPattern = muppHeader[0];
					uint8 loopStart = muppHeader[1];
					uint8 loopEnd = muppHeader[2];
					file.Seek((size_t)(1084 + (1024 * muppPattern)));
					SampleIndex startSmp = (SampleIndex)(m_nSamples + 1);
					m_nSamples += 28;
					instr_.AssignSample(startSmp);

					SampleIO sampleIO = new SampleIO(SampleIO.BitDepth._8Bit, SampleIO.Channels.Mono, SampleIO.Endianness.LittleEndian, SampleIO.Encoding.SignedPcm);

					for (SampleIndex muppSmp = startSmp; muppSmp <= m_nSamples; muppSmp++)
					{
						ModSample mptSmp = Samples[muppSmp];

						mptSmp.Initialize(ModType.Mod);
						mptSmp.nLength = 32;
						mptSmp.nLoopStart = 0;
						mptSmp.nLoopEnd = 32;
						mptSmp.nFineTune = Samples[smp].nFineTune;
						mptSmp.nVolume = Samples[smp].nVolume;
						mptSmp.uFlags.Set(ChannelFlags.Chn_Loop);

						sampleIO.ReadSample(mptSmp, file, muppSmp, mptSmp.nLength);
					}

					InstrumentSynthEvents events = instr_.Synth.m_Scripts.emplace_back(new InstrumentSynthEvents());
					events.reserve((size_t)Math.Min(loopEnd + 2, 65));
					array<uint8> waveforms = file.ReadArray<uint8>(64);
					array<uint8> volumes = file.ReadArray<uint8>(64);

					for (uint8 i = 0; i < 64; i++)
					{
						events.push_back(InstrumentSynth.Event.Mupp_SetWaveform(muppCount, waveforms[i], volumes[i]));

						if ((i == loopEnd) && (loopStart <= loopEnd))
						{
							events.push_back(InstrumentSynth.Event.Jump(loopStart));
							break;
						}
					}

					muppCount++;
				}
			}

			// For "the ultimate beeper.mod"
			{
				ModSample sample = Samples[0];

				sample.Initialize(ModType.Mod);
				sample.nLength = 2;
				sample.nLoopStart = 0;
				sample.nLoopEnd = 2;
				sample.nVolume = 0;
				sample.uFlags.Set(ChannelFlags.Chn_Loop);
				sample.AllocateSample();
			}

			// Fix VBlank MODs. Arbitrary threshold: 8 minutes (enough for "frame of mind" by Dascon...).
			// Basically, this just converts all tempo commands into speed commands
			// for MODs which are supposed to have VBlank timing (instead of CIA timing).
			// There is no perfect way to do this, since both MOD types look the same,
			// but the most reliable way is to simply check for extremely long songs
			// (as this would indicate that e.g. a F30 command was really meant to set
			// the ticks per row to 48, and not the tempo to 48 BPM).
			// In the pattern loader above, a second condition is used: Only tempo commands
			// below 100 BPM are taken into account. Furthermore, only ProTracker (M.K. / M!K!)
			// modules are checked
			if ((isMdKd || ModTools.IsMagic(magic, "M!K!")) && hasTempoCommands && !definitelyCia)
			{
				c_double songTime = GetLength(EnmGetLengthResetMode.eNoAdjust).front().Duration;

				if (songTime >= 480.0)
				{
					m_PlayBehaviour.set(PlayBehaviour.ModVBlankTiming);

					if (GetLength(EnmGetLengthResetMode.eNoAdjust, new GetLengthTarget(songTime)).front().TargetReached)
					{
						// This just makes things worse, song is at least as long as in CIA mode
						// Obviously we should keep using CIA timing then...
						m_PlayBehaviour.reset(PlayBehaviour.ModVBlankTiming);
					}
					else
						modMagicResult.MadeWithTracker = "ProTracker (VBlank)";
				}
			}

			Algorithm.transform(magic.Begin(), magic.End(), magic.Begin(), (uint8 c) => c < ' ' ? (uint8)' ' : c);

			m_ModFormat.FormatName = string.Format("ProTracker MOD ({0})", Encoding.ASCII.GetString(magic.AsSpan()));
			m_ModFormat.Type = "mod";

			if (!string.IsNullOrEmpty(modMagicResult.MadeWithTracker))
				m_ModFormat.MadeWithTracker = modMagicResult.MadeWithTracker;

			m_ModFormat.CharSet = EncoderCollection.Dos;	// TNE: Changed charset from Amiga to DOS, since this player won't play Amiga modules

			if (anyAdpcm)
			{
				m_ModFormat.MadeWithTracker += " (ADPCM packed)";
				m_ModFormat.ExtraInformation = Resources.IDS_MPT_MOD_ADPCM;
			}

			return true;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static bool CheckModMagic(CPointer<byte> magic, ModMagicResult result)//XX 160
		{
			result.MaybeOpenMpt = false;
			result.Format = InternalFormat.Unknown;

			if (ModTools.IsMagic(magic, "M.K.")			// ProTracker and compatible
			    || ModTools.IsMagic(magic, "M!K!")		// ProTracker (> 64 patterns)
			    || ModTools.IsMagic(magic, "PATT")		// ProTracker 3.6
			    || ModTools.IsMagic(magic, "NSMS")		// kingdomofpleasure.mod by bee hunter
			    || ModTools.IsMagic(magic, "LARD"))		// judgement_day_gvine.mod by 4-mat
			{
				result.MadeWithTracker = "Generic ProTracker or compatible";
				result.NumChannels = 4;

				result.MaybeOpenMpt = ModTools.IsMagic(magic, "M.K.") || ModTools.IsMagic(magic, "M!K!");
			}
			else if (ModTools.IsMagic(magic, "M&K!")	// "His Master's Noise" musicdisk
			    || ModTools.IsMagic(magic, "FEST")		// "His Master's Noise" musicdisk
			    || ModTools.IsMagic(magic, "N.T."))
			{
				result.MadeWithTracker = ModTools.IsMagic(magic, "N.T.") ? "NoiseTracker" : "His Master's NoiseTracker";
				result.IsNoiseTracker = true;
				result.SetModVBlankTiming = true;
				result.NumChannels = 4;
			}
			else if (ModTools.IsMagic(magic, "OKTA") || ModTools.IsMagic(magic, "OCTA"))
			{
				// Oktalyzer
				result.MadeWithTracker = "Oktalyzer";
				result.NumChannels = 8;
			}
			else if (ModTools.IsMagic(magic, "CD81") || ModTools.IsMagic(magic, "CD61"))
			{
				// Octalyser on Atari STe/Falcon
				result.MadeWithTracker = "Octalyser (Atari)";
				result.NumChannels = (ChannelIndex)(magic[2] - '0');
			}
			else if (ModTools.IsMagic(magic, "M\0\0\0") || ModTools.IsMagic(magic, "8\0\0\0"))
			{
				// Inconexia demo by Iguana, delta samples (https://www.pouet.net/prod.php?which=830)
				result.MadeWithTracker = "Inconexia demo (delta samples)";
				result.InvalidByteThreshold = ModSampleHeader.Invalid_Byte_Fragile_Threshold;
				result.NumChannels = (ushort)((magic[0] == '8') ? 8 : 4);
				result.Format = InternalFormat.Inconexia;
			}
			else if ((CMemory.memcmp(magic, "FA0", 3) == 0) && (magic[3] >= '4') && (magic[3] <= '8'))
			{
				// Digital Tracker on Atari Falcon
				result.MadeWithTracker = "Digital Tracker";
				result.NumChannels = (ChannelIndex)(magic[3] - '0');

				// Digital Tracker MODs contain four bytes (00 40 00 00) right after the magic bytes which don't seem to do anything special
				result.PatternDataOffset = 1088;
			}
			else if (((CMemory.memcmp(magic, "FLT", 3) == 0) || (CMemory.memcmp(magic, "EXO", 3) == 0)) && ((magic[3] == '4') || (magic[3] == '8')))
			{
				// FLTx / EXOx - Startrekker by Exolon / Fairlight
				result.MadeWithTracker = "Startrekker";
				result.IsStartrekker = true;
				result.SetModVBlankTiming = true;
				result.NumChannels = (ChannelIndex)(magic[3] - '0');
			}
			else if ((magic[0] >= '1') && (magic[0] <= '9') && (CMemory.memcmp(magic + 1, "CHN", 3) == 0))
			{
				// xCHN - Many trackers
				result.MadeWithTracker = "Generic MOD-compatible Tracker";
				result.IsGenericMultiChannel = true;
				result.NumChannels = (ChannelIndex)(magic[0] - '0');
				result.MaybeOpenMpt = true;
			}
			else if ((magic[0] >= '1') && (magic[0] <= '9') && (magic[1] >= '0') && (magic[1] <= '9') && ((CMemory.memcmp(magic + 2, "CH", 2) == 0) || (CMemory.memcmp(magic + 2, "CN", 2) == 0)))
			{
				// xxCN / xxCH - Many trackers
				result.MadeWithTracker = "Generic MOD-compatible Tracker";
				result.IsGenericMultiChannel = true;
				result.NumChannels = (ChannelIndex)(((magic[0] - '0') * 10) + (magic[1] - '0'));
				result.MaybeOpenMpt = true;
			}
			else if ((CMemory.memcmp(magic, "TDZ", 3) == 0) && (magic[3] >= '1') && (magic[3] <= '9'))
			{
				// TDZx - TakeTracker (only TDZ1-TDZ3 should exist, but historically this code only supported 4-9 channels, so we keep those for the unlikely case that they were actually used for something)
				result.MadeWithTracker = "TakeTracker";
				result.NumChannels = (ChannelIndex)(magic[3] - '0');
			}
			else if (ModTools.IsMagic(magic, ".M.K"))
			{
				// Hacked .DMF files from the game "Apocalypse Abyss"
				result.NumChannels = 4;
				result.SwapBytes = true;
			}
			else if (ModTools.IsMagic(magic, "WARD"))
			{
				// MUSIC*.DTA files from the DOS game Aleshar - The World Of Ice
				result.MadeWithTracker = "Generic MOD-compatible Tracker";
				result.IsGenericMultiChannel = true;
				result.NumChannels = 8;
				result.Format = InternalFormat.Aleshar;
			}
			else
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// TNE: Added extra checks so only OpenMPT modules are recognized.
		/// The first two checks are ported from libxmp, which are the only
		/// two things it uses to tell that a MOD file was made by OpenMPT.
		/// The last two scan the pattern data for the panning style used by
		/// ModPlug Tracker / OpenMPT and look for ADPCM packed samples
		/// </summary>
		/********************************************************************/
		private static ProbeResult ExtendedProbe(FileReader file, CPointer<byte> magic, ModMagicResult modMagicResult)
		{
			bool hasBigSample = false;
			bool hasLoop0 = false;
			bool hasVolInEmptyIns = false;
			bool hasConvertedSample = false;
			bool hasEmptySampleWithLoop1 = false;
			bool hasStIns = false;
			bool hasSpaceNames = false;
			bool hasNonDefaultEmptyIns = false;
			int namedSamples = 0;
			int filledSampleNames = 0;
			size_t totalSampleBytes = 0;
			SmpLength[] sampleLengths = new SmpLength[31];

			file.Seek(20);

			for (SampleIndex smp = 1; smp <= 31; smp++)
			{
				ModSampleHeader sampleHeader = ModTools.ReadAndSwap<ModSampleHeader>.From(file, modMagicResult.SwapBytes);

				if (sampleHeader.Length >= 0x8000)
					hasBigSample = true;

				if (sampleHeader.LoopLength == 0)
					hasLoop0 = true;

				if (sampleHeader.Length == 0)
				{
					if (sampleHeader.Volume > 0)
						hasVolInEmptyIns = true;

					if (sampleHeader.LoopLength == 1)
						hasEmptySampleWithLoop1 = true;

					// Octalyser initializes every unused sample slot in the same
					// way, so a slot holding anything else rules it out
					if ((sampleHeader.Volume != 0x40) || (sampleHeader.LoopStart != 0) || (sampleHeader.LoopLength != 1))
						hasNonDefaultEmptyIns = true;
				}
				else if ((sampleHeader.Length == 1) && (sampleHeader.Volume == 0))
					hasConvertedSample = true;

				string sampleName = sampleHeader.Name.ToString();

				if (IsSoundTrackerSampleName(sampleName))
					hasStIns = true;
				else if (sampleHeader.Name[20] == ' ')
					hasSpaceNames = true;

				if (HasSampleName(sampleName))
				{
					namedSamples++;

					// The last byte of the field is always left as a zero
					// terminator, so a name of 21 characters fills it out
					if (sampleName.Length >= 21)
						filledSampleNames++;
				}

				sampleLengths[smp - 1] = sampleHeader.Length * 2U;
				totalSampleBytes += sampleLengths[smp - 1];
			}

			// A sample bigger than 64 KB cannot have been written by
			// ProTracker, so the module has to be made by OpenMPT
			if (hasBigSample)
				return ProbeResult.Success;

			// The sample headers are followed by the order list
			ModFileHeader fileHeader = ModTools.ReadAndSwap<ModFileHeader>.From(file, modMagicResult.SwapBytes);

			// Octalyser (Atari) writes the sample names into a fixed width field,
			// padding them with spaces, and initializes every unused sample slot
			// with a volume of 64 and an empty loop. ProTracker leaves the unused
			// slots all zero, so that is what tells the two apart. A restart
			// position of 0x7f is written by ProTracker itself and is never seen
			// in an Octalyser module. Without these, any module using the sample
			// names as a scroll text would be taken for an Octalyser module
			if (!hasStIns && hasVolInEmptyIns && !hasNonDefaultEmptyIns && (fileHeader.RestartPos != 0x7f) && (hasSpaceNames || ((namedSamples >= Minimum_Filled_Sample_Names) && (filledSampleNames == namedSamples))))
				return ProbeResult.Failure;		// Probably an Octalyser module

			// Find the number of patterns stored in the file
			array<uint8> orderList = fileHeader.OrderList.ToArray();
			PatternIndex numPatterns = 0;

			for (OrderIndex ord = 0; ord < 128; ord++)
			{
				uint8 pat = orderList[ord];

				if ((pat < 128) && (numPatterns <= pat))
					numPatterns = (PatternIndex)(pat + 1);
			}

			if (HasOpenMptSampleLayout(magic, modMagicResult, fileHeader, hasLoop0, hasVolInEmptyIns, hasConvertedSample, hasEmptySampleWithLoop1, hasStIns, numPatterns))
				return ProbeResult.Success;

			if (ScanPatternsForOpenMpt(file, modMagicResult, numPatterns, totalSampleBytes) == ProbeResult.Success)
				return ProbeResult.Success;

			if (HasAdpcmSamples(file, modMagicResult, numPatterns, sampleLengths))
				return ProbeResult.Success;

			return ProbeResult.Failure;
		}



		/********************************************************************/
		/// <summary>
		/// Checks if the sample name holds any real text at all
		/// </summary>
		/********************************************************************/
		private static bool HasSampleName(string name)
		{
			foreach (char chr in name)
			{
				if (chr > ' ')
					return true;
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Return true if the sample name looks like one of the instruments
		/// shipped with Ultimate Soundtracker, e.g. "st-01:"
		/// </summary>
		/********************************************************************/
		private static bool IsSoundTrackerSampleName(string name)
		{
			if (name.Length < 6)
				return false;

			if ((name[0] != 's') && (name[0] != 'S'))
				return false;

			if ((name[1] != 't') && (name[1] != 'T'))
				return false;

			if ((name[2] != '-') || (name[5] != ':'))
				return false;

			return char.IsDigit(name[3]) && char.IsDigit(name[4]);
		}



		/********************************************************************/
		/// <summary>
		/// Return true if the sample headers have the layout libxmp uses to
		/// tell that a module was made by OpenMPT. An empty sample having
		/// both a volume and a loop length of one is what gives it away,
		/// but only when nothing else points at another tracker
		/// </summary>
		/********************************************************************/
		private static bool HasOpenMptSampleLayout(CPointer<byte> magic, ModMagicResult modMagicResult, ModFileHeader fileHeader, bool hasLoop0, bool hasVolInEmptyIns, bool hasConvertedSample, bool hasEmptySampleWithLoop1, bool hasStIns, PatternIndex numPatterns)
		{
			// libxmp only reaches this test for M.K. modules. All the other
			// magics either identify the tracker on their own or end up
			// with a channel count that is rejected right below
			if (!ModTools.IsMagic(magic, "M.K.") || (modMagicResult.NumChannels != 4))
				return false;

			// 0x78 is not a real restart position, but the default tempo of
			// 120 BPM that (Ultimate) Soundtracker stored in this byte and
			// which a lot of old modules kept. NoiseTracker itself stores a
			// real restart position below 0x7f. 0x7f is ScreamTracker or a
			// ProTracker clone and anything above that is unknown. None of
			// them can be OpenMPT
			uint8 restart = fileHeader.RestartPos;

			if ((restart != numPatterns) && ((restart == 0x78) || (restart >= 0x7f)))
				return false;

			// A loop length of zero, a one word long silent sample or the
			// Ultimate Soundtracker instruments all point somewhere else
			if (hasLoop0 || hasConvertedSample || hasStIns)
				return false;

			return hasEmptySampleWithLoop1 && hasVolInEmptyIns;
		}



		/********************************************************************/
		/// <summary>
		/// Scan the pattern data for the panning style only ModPlug
		/// Tracker / OpenMPT writes into MOD files. 8A4 is 7-bit panning +
		/// surround, which the loader converts into a real surround command,
		/// and 8xx panning in the 00-80 range is how OpenMPT stores panning
		/// at all
		/// </summary>
		/********************************************************************/
		private static ProbeResult ScanPatternsForOpenMpt(FileReader file, ModMagicResult modMagicResult, PatternIndex numPatterns, size_t totalSampleBytes)
		{
			ProbeResult noMatchResult = ProbeResult.Failure;

			if (modMagicResult.NumChannels == 0)
				return noMatchResult;

			size_t patternSize = modMagicResult.NumChannels * 256U;
			size_t sizeWithoutPatterns = modMagicResult.PatternDataOffset + totalSampleBytes;

			file.Seek(modMagicResult.PatternDataOffset);

			uint8 maxPanning = 0;
			bool leftPanning = false, extendedPanning = false;
			int panningEffects = 0;

			for (PatternIndex pat = 0; pat < numPatterns; pat++)
			{
				// Never scan into the sample data, in case the order list claims
				// more patterns than the file really holds
				if (!file.LengthIsAtLeast(sizeWithoutPatterns + ((pat + 1U) * patternSize)))
					break;

				for (uint32 i = 0; i < (patternSize / 4); i++)
				{
					if (!file.CanRead(4))
						return noMatchResult;

					array<uint8> data = ModTools.ReadAndSwap<Array4>.From(file, modMagicResult.SwapBytes && (pat == 0)).ToArray();

					uint8 command = (uint8)(data[2] & 0x0f), param = data[3];

					if (command == 0x08)
					{
						panningEffects++;

						// 8A4 is 7-bit panning + surround
						if (param == 0xa4)
							return ProbeResult.Success;

						maxPanning = Math.Max(maxPanning, param);

						if (param < 0x80)
							leftPanning = true;
						else if (param > 0x8f)
							extendedPanning = true;
					}
					else if ((command == 0x0e) && ((param & 0xf0) == 0x80))
					{
						panningEffects++;

						maxPanning = Math.Max(maxPanning, (uint8)((param & 0x0f) << 4));
					}
				}
			}

			// Only trust the panning, if enough panning effects are used,
			// since a few of them are most likely just sync markers
			if (panningEffects < Minimum_Mod_Panning_Effects)
				return noMatchResult;

			// Same heuristic as the loader uses to detect modules with
			// 7-bit panning, which is how OpenMPT stores panning in MOD
			// files. Other trackers either use the full 8-bit range or do
			// not write 8xx commands at all
			if (leftPanning && !extendedPanning && (maxPanning >= Enable_Mod_Panning_Threshold))
				return ProbeResult.Success;

			return noMatchResult;
		}



		/********************************************************************/
		/// <summary>
		/// Return true if any of the samples are stored in ADPCM format.
		/// Only ModPlug Tracker can save samples that way and since OpenMPT
		/// is based on ModPlug Tracker, we want to play these modules
		/// </summary>
		/********************************************************************/
		private static bool HasAdpcmSamples(FileReader file, ModMagicResult modMagicResult, PatternIndex numPatterns, SmpLength[] sampleLengths)
		{
			if (modMagicResult.NumChannels == 0)
				return false;

			// Seek to the first sample
			size_t patternSize = modMagicResult.NumChannels * 256U;

			if (!file.Seek(modMagicResult.PatternDataOffset + (numPatterns * patternSize)))
				return false;

			for (SampleIndex smp = 0; smp < 31; smp++)
			{
				if (sampleLengths[smp] == 0)
					continue;

				// ReadMagic() only moves the position forward on a match, so
				// the whole sample is skipped when it is not packed
				if (file.ReadMagic("ADPCM"))
					return true;

				if (!file.Skip(sampleLengths[smp]))
					break;
			}

			return false;
		}
		#endregion
	}
}
