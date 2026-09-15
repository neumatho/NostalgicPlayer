/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.String;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Endian;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;
using Utility = Polycode.NostalgicPlayer.Kit.C.Std.Utility;
using Version = Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.Version;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// S3M (ScreamTracker 3) module loader
	/// </summary>
	internal partial class CSoundFile
	{
		#region OpenMPT loader class
		public class S3MLoader : IFormatLoader
		{
			public static readonly FileFormatLoader Format = new FileFormatLoader
			{
				Id = Guid.Parse("39274574-DD73-4D47-9B5F-466211F028BB"),
				Name = Resources.IDS_MPT_S3M_NAME,
				Description = Resources.IDS_MPT_S3M_DESCRIPTION,
				Prober = Probe_OpenMpt,
				Create = Create
			};

			private readonly CSoundFile sndFile;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			private S3MLoader(CSoundFile soundFile)
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
				return ProbeFileHeaderS3M(file, pFileSize);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public static IFormatLoader Create(CSoundFile soundFile)
			{
				return new S3MLoader(soundFile);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public bool Read(FileReader file, ModLoadingFlags loadFlags)
			{
				return sndFile.ReadS3M(file, loadFlags);
			}
		}
		#endregion

		#region S3MPattern
		private const uint8 S3MEndOfRow = 0x00;
		private const uint8 S3MChannelMask = 0x1f;
		private const uint8 S3MNotePresent = 0x20;
		private const uint8 S3MVolumePresent = 0x40;
		private const uint8 S3MEffectPresent = 0x80;
		private const uint8 S3MAnyPresent = 0xe0;

		private const uint8 S3MNoteOff = 0xfe;
		private const uint8 S3MNoteNone = 0xff;
		#endregion

		#region PixPlayPanning class
		private class PixPlayPanning : CPatternContainer.IForeach<PixPlayPanning, ModCommand>
		{
			/********************************************************************/
			/// <summary>
			/// Is called for every mod command in all patterns
			/// </summary>
			/********************************************************************/
			public void Invoke(CPointer<ModCommand> arr)
			{
				ModCommand m = arr[0];

				if (m.Command == EffectCommand.Midi)
				{
					m.Command = EffectCommand.S3MCmdEx;
					m.Param |= 0x80;
				}
			}



			/********************************************************************/
			/// <summary>
			/// Make a deep copy of the current object
			/// </summary>
			/********************************************************************/
			public PixPlayPanning MakeDeepClone()
			{
				return (PixPlayPanning)MemberwiseClone();
			}
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static ProbeResult ProbeFileHeaderS3M(FileReader file, uint64? pFileSize)
		{
			S3MFileHeader fileHeader = new S3MFileHeader();

			if (!file.ReadStruct(ref fileHeader))
				return ProbeResult.WantMoreData;

			if (!ValidateHeader(ref fileHeader))
				return ProbeResult.Failure;

			ProbeResult result = ProbeAdditionalSize(file, pFileSize, GetHeaderMinimumAdditionalSize(ref fileHeader));
			if (result != ProbeResult.Success)
				return result;

			return ExtendedProbeS3M(file, ref fileHeader);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool ReadS3M(FileReader file, ModLoadingFlags loadFlags)
		{
			file.Rewind();

			// Is it a valid S3M file?
			S3MFileHeader fileHeader = new S3MFileHeader();

			if (!file.ReadStruct(ref fileHeader))
				return false;

			if (!ValidateHeader(ref fileHeader))
				return false;

			if (!file.CanRead((size_t)GetHeaderMinimumAdditionalSize(ref fileHeader)))
				return false;

			if (loadFlags == ModLoadingFlags.OnlyVerifyHeader)
				return true;

			InitializeGlobals(ModType.S3M, fileHeader.GetNumChannels());
			m_nMinPeriod = 64;
			m_nMaxPeriod = 32767;

			Loaders.ReadOrderFromFile<uint8>(Order.Current, file, fileHeader.OrdNum, 0xff, 0xfe);

			// Read sample header offsets
			vector<uint16le> sampleOffsets = new vector<uint16le>();
			file.ReadVector(sampleOffsets, fileHeader.SmpNum);

			// Read pattern offsets
			vector<uint16le> patternOffsets = new vector<uint16le>();
			file.ReadVector(patternOffsets, fileHeader.PatNum);

			// ST3 ignored Zxx commands, so if we find that a file was made with ST3, we should erase all MIDI macros
			bool keepMidiMacros = false;

			string madeWithTracker = string.Empty;
			bool formatTrackerStr = false;
			bool nonCompatTracker = false;
			bool isSt3 = false;
			bool isSchism = false;
			bool usePanningTable = fileHeader.UsePanningTable == S3MFileHeader.IdPanning;
			bool offsetsAreCanonical = !patternOffsets.empty() && !sampleOffsets.empty() && (patternOffsets[0] > sampleOffsets[0]);
			int32 schismDateVersion = ItTools.SchismTrackerEpoch + ((fileHeader.Cwtv == 0x4fff) ? fileHeader.Reserved2 : (fileHeader.Cwtv - 0x4050));
			uint32 editTimer = 0;

			switch (fileHeader.Cwtv & S3MFileHeader.TrackerMask)
			{
				case S3MFileHeader.TrkAkord & S3MFileHeader.TrackerMask:
				{
					if (fileHeader.Cwtv == S3MFileHeader.TrkAkord)
						madeWithTracker = "Akord";

					break;
				}

				case S3MFileHeader.TrkScreamTracker:
				{
					if ((fileHeader.Reserved2 == 0x4353) && (fileHeader.Reserved3 == 0x3242554c) && (fileHeader.Reserved4 == 0x302e))	// SCLUB2.0
						madeWithTracker = "Sound Club 2";
					else if ((fileHeader.Cwtv == S3MFileHeader.TrkSt3_20) && (fileHeader.Special == 0) && ((fileHeader.OrdNum & 0x01) == 0) && (fileHeader.UltraClicks == 0) && ((fileHeader.Flags & ~0x50) == 0) && usePanningTable && offsetsAreCanonical)
					{
						// Canonical offset check avoids mis-detection of an automatic conversion of Vic's "Paper" demo track
						if ((fileHeader.OrdNum & 0x0f) == 0)
						{
							// MPT and OpenMPT before 1.17.03.02 - Simply keep default (filter) MIDI macros
							if ((fileHeader.MasterVolume & 0x80) != 0)
							{
								m_dwLastSavedWithVersion = Version.MPT_V._1_16;;
								madeWithTracker = "ModPlug Tracker / OpenMPT 1.17";
							}
							else
							{
								// MPT 1.0 alpha5 doesn't set the stereo flag, but MPT 1.0 alpha6 does
								m_dwLastSavedWithVersion = Version.MPT_V._1_00_00_A0;
								madeWithTracker = "ModPlug Tracker 1.0 alpha";
							}
						}
						else if ((fileHeader.MasterVolume & 0x80) != 0)
							madeWithTracker = "Schism Tracker";

						keepMidiMacros = true;
						nonCompatTracker = true;
						m_PlayBehaviour.set(PlayBehaviour.St3LimitPeriod);
					}
					else if ((fileHeader.Cwtv == S3MFileHeader.TrkSt3_20) && (fileHeader.Special == 0) && (fileHeader.UltraClicks == 0) && (fileHeader.Flags == 0) && !usePanningTable)
					{
						if ((fileHeader.GlobalVol == 64) && (fileHeader.MasterVolume == 48))
							madeWithTracker = "PlayerPRO";
						else	// Always stereo
							madeWithTracker = "Velvet Studio";
					}
					else if ((fileHeader.Cwtv == S3MFileHeader.TrkSt3_20) && (fileHeader.Special == 0) && (fileHeader.UltraClicks == 0) && (fileHeader.Flags == 8) && !usePanningTable)
						madeWithTracker = "Impulse Tracker < 1.03";		// Not sure if 1.02 saves like this as I don't have it
					else
					{
						// ST3.20 should only ever write ultra-click values 16, 24 and 32 (corresponding to 8, 12 and 16 in the GUI), ST3.01/3.03 should only write 0,
						// though several ST3.01/3.03 files with ultra-click values of 16 have been found as well.
						// However, we won't fingerprint these values here as it's unlikely that there is any other tracker out there disguising as ST3 and using a strange ultra-click value.
						// Also, re-saving a file with a strange ultra-click value in ST3 doesn't fix this value unless the user manually changes it, or if it's below 16
						isSt3 = true;

						if (fileHeader.Cwtv == S3MFileHeader.TrkSt3_20)
						{
							// 3.21 writes the version number as 3.20. There is no known way to differentiate between the two
							madeWithTracker = "Scream Tracker 3.20 - 3.21";
						}
						else
						{
							madeWithTracker = "Scream Tracker";
							formatTrackerStr = true;
						}
					}

					break;
				}

				case S3MFileHeader.TrkImagoOrpheus:
				{
					formatTrackerStr = fileHeader.Cwtv != S3MFileHeader.TrkPlayerPro;

					if (formatTrackerStr)
						madeWithTracker = "Imago Orpheus";
					else
						madeWithTracker = "PlayerPRO";

					nonCompatTracker = true;
					break;
				}

				case S3MFileHeader.TrkImpulseTracker:
				{
					if (fileHeader.Cwtv == S3MFileHeader.TrkIt1_Old)
						madeWithTracker = "Impulse Tracker 1.03";	// Could also be 1.02, maybe? I don't have that one
					else
						madeWithTracker = GetImpulseTrackerVersion(fileHeader.Cwtv, 0);

					if ((fileHeader.Cwtv >= S3MFileHeader.TrkIt2_07) && (fileHeader.Reserved3 != 0))
					{
						// Starting from version 2.07, IT stores the total edit time of a module in the "reserved" field
						editTimer = ItTools.DecodeItEditTimer(fileHeader.Cwtv, fileHeader.Reserved3);
					}

					nonCompatTracker = true;
					m_PlayBehaviour.set(PlayBehaviour.PeriodsAreHertz);
					m_PlayBehaviour.set(PlayBehaviour.ItRetrigger);
					m_PlayBehaviour.set(PlayBehaviour.ItShortSampleRetrig);
					m_PlayBehaviour.set(PlayBehaviour.St3SampleSwap);	// Not exactly like ST3, but close enough
					m_PlayBehaviour.set(PlayBehaviour.ItPortaNoNote);
					m_PlayBehaviour.set(PlayBehaviour.ItPortamentoSwapResetsPos);
					m_nMinPeriod = 1;
					break;
				}

				case S3MFileHeader.TrkSchismTracker:
				{
					if (fileHeader.Cwtv == S3MFileHeader.TrkBeRoTrackerOld)
					{
						madeWithTracker = "BeRoTracker";
						m_PlayBehaviour.set(PlayBehaviour.St3LimitPeriod);
					}
					else
					{
						madeWithTracker = GetSchismTrackerVersion(fileHeader.Cwtv, fileHeader.Reserved2);
						m_nMinPeriod = 1;
						isSchism = true;

						if (schismDateVersion >= new SchismVersionFromDate(2021, 05, 02).Date)
							m_PlayBehaviour.set(PlayBehaviour.PeriodsAreHertz);

						if (schismDateVersion >= new SchismVersionFromDate(2016, 05, 13).Date)
							m_PlayBehaviour.set(PlayBehaviour.ItShortSampleRetrig);

						m_PlayBehaviour.reset(PlayBehaviour.St3TonePortaWithAdlibNote);

						if ((fileHeader.Cwtv == (S3MFileHeader.TrkSchismTracker | 0xfff)) && (fileHeader.Reserved3 != 0))
							editTimer = fileHeader.Reserved3;
					}

					nonCompatTracker = true;
					break;
				}

				case S3MFileHeader.TrkOpenMpt:
				{
					if ((fileHeader.Cwtv & 0xff00) == S3MFileHeader.TrkNesMusa)
					{
						madeWithTracker = "NESMusa";
						formatTrackerStr = true;
					}
					else if ((fileHeader.Reserved2 == 0) && (fileHeader.UltraClicks == 16) && (fileHeader.Channels[1] != 1))
					{
						// Liquid Tracker's ID clashes with OpenMPT's.
						// OpenMPT started writing full version information with OpenMPT 1.29 and later changed the ultraClicks value from 8 to 16.
						// Liquid Tracker writes an ultraClicks value of 16.
						// So we assume that a file was saved with Liquid Tracker if the reserved fields are 0 and ultraClicks is 16
						madeWithTracker = "Liquid Tracker";
						formatTrackerStr = true;
					}
					else if (fileHeader.Cwtv != S3MFileHeader.TrkGraoumfTracker)
					{
						uint32 mptVersion = (uint32)((fileHeader.Cwtv & S3MFileHeader.VersionMask) << 16);

						if (mptVersion >= 0x01_29_00_00)
						{
							mptVersion |= fileHeader.Reserved2;

							// Added in OpenMPT 1.32.00.31
							if (fileHeader.Reserved3 != 0)
								editTimer = fileHeader.Reserved3;
						}

						m_dwLastSavedWithVersion = new Version(mptVersion);
						madeWithTracker = "OpenMPT " + m_dwLastSavedWithVersion;
					}
					else
						madeWithTracker = "Graoumf Tracker";

					break;
				}

				case S3MFileHeader.TrkBeRoTracker:
				{
					madeWithTracker = "BeRoTracker";
					m_PlayBehaviour.set(PlayBehaviour.St3LimitPeriod);
					break;
				}

				case S3MFileHeader.TrkCreamTracker:
				{
					madeWithTracker = "CreamTracker";
					break;
				}

				default:
				{
					if (fileHeader.Cwtv == S3MFileHeader.TrkCamoto)
						madeWithTracker = "Camoto";

					break;
				}
			}

			if (formatTrackerStr)
				madeWithTracker = string.Format("{0} {1}.{2:x2}", madeWithTracker, (fileHeader.Cwtv & 0xf00) >> 8, fileHeader.Cwtv & 0xff);

			// IT edit timer
			if (editTimer != 0)
			{
				FileHistory hist = new FileHistory();
				hist.OpenTime = (uint32)(editTimer * (FileHistory.History_Timer_Precision / 18.2));
				m_FileHistory.push_back(hist);
			}

			m_ModFormat.FormatName = "Scream Tracker 3";
			m_ModFormat.Type = "s3m";
			m_ModFormat.MadeWithTracker = Utility.move(madeWithTracker);
			m_ModFormat.CharSet = m_dwLastSavedWithVersion ? EncoderCollection.Win1252 : EncoderCollection.Dos;

			if (nonCompatTracker)
			{
				m_PlayBehaviour.reset(PlayBehaviour.St3NoMutedChannels);
				m_PlayBehaviour.reset(PlayBehaviour.St3EffectMemory);
				m_PlayBehaviour.reset(PlayBehaviour.St3PortaSampleChange);
				m_PlayBehaviour.reset(PlayBehaviour.St3VibratoMemory);
				m_PlayBehaviour.reset(PlayBehaviour.St3PortaAfterArpeggio);
				m_PlayBehaviour.reset(PlayBehaviour.St3OffsetWithoutInstrument);
				m_PlayBehaviour.reset(PlayBehaviour.ApplyUpperPeriodLimit);
			}

			if (fileHeader.Cwtv <= S3MFileHeader.TrkSt3_01)
			{
				// This broken behaviour is not present in ST3.01
				m_PlayBehaviour.reset(PlayBehaviour.St3TonePortaWithAdlibNote);
			}

			if ((fileHeader.Cwtv & S3MFileHeader.TrackerMask) > S3MFileHeader.TrkScreamTracker)
			{
				if (((fileHeader.Cwtv & S3MFileHeader.TrackerMask) != S3MFileHeader.TrkImpulseTracker) || (fileHeader.Cwtv >= S3MFileHeader.TrkIt2_14))
				{
					// Keep MIDI macros if this is not an old IT version (BABYLON.S3M by Necros has Zxx commands and was saved with IT 2.05)
					keepMidiMacros = true;
				}
			}

			m_MidiCfg.Reset();

			if (!keepMidiMacros)
			{
				// Remove macros so they don't interfere with tunes made in trackers that don't support Zxx
				m_MidiCfg.ClearZxxMacros();
			}

			m_SongName = MptString.ReadBuf(ReadWriteMode.NullTerminated, fileHeader.Name.ToArray());

			if ((fileHeader.Flags & S3MFileHeader.AmigaLimits) != 0)
				m_SongFlags |= SongFlags.AmigaLimits;

			if ((fileHeader.Flags & S3MFileHeader.St2Vibrato) != 0)
				m_SongFlags |= SongFlags.S3MOldVibrato;

			if ((fileHeader.Cwtv == S3MFileHeader.TrkSt3_00) || ((fileHeader.Flags & S3MFileHeader.FastVolumeSlides) != 0))
				m_SongFlags |= SongFlags.FastVolSlides;

			// Even though ST3 accepts the command AFF as expected, it mysteriously fails to load a default speed of 255...
			if ((fileHeader.Speed == 0) || ((fileHeader.Speed == 255) && isSt3))
				Order.Current.SetDefaultSpeed(6);
			else
				Order.Current.SetDefaultSpeed(fileHeader.Speed);

			// ST3 also fails to load an otherwise valid default tempo of 32...
			if (fileHeader.Tempo < 33)
				Order.Current.SetDefaultTempoInt(isSt3 ? 125U : 32U);
			else
				Order.Current.SetDefaultTempoInt(fileHeader.Tempo);

			// Global volume
			m_nDefaultGlobalVolume = Math.Min(fileHeader.GlobalVol, (uint8)64) * 4U;

			// The following check is probably not very reliable, but it fixes a few tunes, e.g.
			// DARKNESS.S3M by Purple Motion (ST 3.00) and "Image of Variance" by C.C.Catch (ST 3.01).
			// Note that even ST 3.01b imports these files with a global volume of 0,
			// so it's not clear if these files ever played "as intended" in any ST3 versions (I don't have any older ST3 versions)
			if ((m_nDefaultGlobalVolume == 0) && (fileHeader.Cwtv < S3MFileHeader.TrkSt3_20))
				m_nDefaultGlobalVolume = Snd_Def.Max_Global_Volume;

			if ((fileHeader.FormatVersion == S3MFileHeader.OldVersion) && (fileHeader.MasterVolume < 8))
				m_nSamplePreAmp = (uint32)Math.Min((fileHeader.MasterVolume + 1) * 0x10, 0x7f);

			// These changes were probably only supposed to be done for older format revisions, where supposedly 0x10 was the stereo flag.
			// However, this version check is missing in ST3, so any mono file with a master volume of 18 will be converted to a stereo file with master volume 32
			else if ((fileHeader.MasterVolume == 2) || (fileHeader.MasterVolume == (2 | 0x10)))
				m_nSamplePreAmp = 0x20;
			else if ((fileHeader.MasterVolume & 0x7f) == 0)
				m_nSamplePreAmp = 48;
			else
				m_nSamplePreAmp = (uint32)Math.Max(fileHeader.MasterVolume & 0x7f, 0x10);	// Bit 7 = Stereo (we always use stereo)

			// Approximately as loud as in DOSBox and a real SoundBlaster 16
			m_nVstiVolume = 36;

			if (isSchism && (schismDateVersion < new SchismVersionFromDate(2018, 11, 12).Date))
				m_nVstiVolume = 64;

			bool isStereo = ((fileHeader.MasterVolume & 0x80) != 0) || (m_dwLastSavedWithVersion.GetRawVersion() != 0);

			if (!isStereo)
			{
				m_nSamplePreAmp = Util.MulDivR_Unsigned(m_nSamplePreAmp, 8, 11);
				m_nVstiVolume = Util.MulDivR_Unsigned(m_nVstiVolume, 8, 11);
			}

			// Channel setup
			bitset isAdlibChannel = new bitset(32);

			for (ChannelIndex i = 0; i < GetNumChannels(); i++)
			{
				uint8 cType = (uint8)(fileHeader.Channels[i] & ~0x80);

				if ((fileHeader.Channels[i] != 0xff) && isStereo)
					ChnSettings[i].nPan = (uint16)((cType & 8) != 0 ? 0xcc: 0x33);	// 200 : 56

				if ((fileHeader.Channels[i] & 0x80) != 0)
					ChnSettings[i].dwFlags = ChannelFlags.Chn_Mute;

				if ((cType >= 16) && (cType <= 29))
				{
					// Adlib channel - except for OpenMPT 1.19 and older, which would write wrong channel types for PCM channels 16-32.
					// However, MPT/OpenMPT always wrote the extra panning table, so there is no need to consider this here
					ChnSettings[i].nPan = 128;
					isAdlibChannel[i] = true;
				}
			}

			// Read extended channel panning
			if (usePanningTable)
			{
				bool hasChannelsWithoutPanning = false;
				array<uint8> pan = file.ReadArray<uint8>(32);

				for (ChannelIndex i = 0; i < GetNumChannels(); i++)
				{
					if (((pan[i] & 0x20) != 0) && (!isSt3 || !isAdlibChannel[i]))
						ChnSettings[i].nPan = (uint16)((((uint16)(pan[i] & 0x0f) * 256) + 8) / 15U);
					else if (pan[i] < 0x10)
						hasChannelsWithoutPanning = true;
				}

				if ((GetNumChannels() < 32) && (m_dwLastSavedWithVersion == Version.MPT_V._1_16))
				{
					// MPT 1.0 alpha 6 up to 1.16.203 set the panning bit for all channels, regardless of whether they are used or not.
					// Note: Schism Tracker fixed the same bug in git commit f21fe8bcae8b6dde2df27ede4ac9fe563f91baff
					if (hasChannelsWithoutPanning)
						m_ModFormat.MadeWithTracker = "ModPlug Tracker 1.16 / OpenMPT 1.17";
					else
						m_ModFormat.MadeWithTracker = "ModPlug Tracker";
				}
			}

			// Reading sample headers
			m_nSamples = Math.Min((SampleIndex)fileHeader.SmpNum, (SampleIndex)(Snd_Def.Max_Samples - 1));
			bool anySamples = false, anyAdpcm = false;
			uint16 gusAddresses = 0;

			for (SampleIndex smp = 0; smp < m_nSamples; smp++)
			{
				S3MSampleHeader sampleHeader = new S3MSampleHeader();

				if (!file.Seek((size_t)(sampleOffsets[smp] * 16)) || !file.ReadStruct(ref sampleHeader))
					continue;

				sampleHeader.ConvertToMpt(Samples[smp + 1], isSt3);

				// Old ModPlug Tracker allowed to write into the last byte reserved for the null terminator
				m_szNames[smp + 1].Assign(MptString.ReadBuf(ReadWriteMode.MaybeNullTerminated, sampleHeader.Name.ToArray()));

				if (sampleHeader.SampleType < S3MSampleHeader.TypeAdMel)
				{
					if (sampleHeader.Length != 0)
					{
						SampleIO sampleIo = sampleHeader.GetSampleFormat(fileHeader.FormatVersion == S3MFileHeader.OldVersion);

						if (((loadFlags & ModLoadingFlags.LoadSampleData) != 0) && file.Seek(sampleHeader.GetSampleOffset()))
							sampleIo.ReadSample(Samples[smp + 1], file);

						anySamples = true;

						if (sampleIo.GetEncoding() == SampleIO.Encoding.Adpcm)
							anyAdpcm = true;
					}

					gusAddresses |= sampleHeader.GusAddress;
				}
			}

			bool useGus = gusAddresses > 1;

			if (isSt3 && anySamples && (gusAddresses == 0) && (fileHeader.Cwtv != S3MFileHeader.TrkSt3_00))
			{
				// All Scream Tracker versions except for some probably early revisions of Scream Tracker 3.00 write GUS addresses. GUS support might not have existed at that point (1992).
				// Hence if a file claims to be written with ST3 (but not ST3.00), but has no GUS addresses, we deduce that it must be written by some other software (e.g. some PSM -> S3M conversions)
				isSt3 = false;
				m_ModFormat.MadeWithTracker = "Unknown";

				// Check these only after we are certain that it can't be ST3.01 because that version doesn't sanitize the ultraClicks value yet
				if ((fileHeader.Cwtv == S3MFileHeader.TrkSt3_01) && (fileHeader.UltraClicks == 0))
				{
					if (((fileHeader.Flags & ~(S3MFileHeader.FastVolumeSlides | S3MFileHeader.AmigaLimits)) == 0) && ((fileHeader.MasterVolume & 0x80) != 0) && usePanningTable)
						m_ModFormat.MadeWithTracker = "UNMO3";
					else if ((fileHeader.Flags == 0) && (fileHeader.GlobalVol == 48) && (fileHeader.MasterVolume == 176) && (fileHeader.Tempo == 150) && !usePanningTable)
						m_ModFormat.MadeWithTracker = "deMODifier";	// SoundSmith to S3M converter
					else if ((fileHeader.Flags == 0) && (fileHeader.GlobalVol == 64) && ((fileHeader.MasterVolume & 0x7f) == 48) && (fileHeader.Speed == 6) && (fileHeader.Tempo == 125) && !usePanningTable)
						m_ModFormat.MadeWithTracker = "Kosmic To-S3M";	// MTM to S3M converter by Zab/Kosmic
				}
			}
			else if (isSt3)
			{
				// Saving an S3M file in ST3 with the Gravis Ultrasound driver loaded will write a unique GUS memory address for each non-empty sample slot (and 0 for unused slots).
				// Re-saving that file in ST3 with the SoundBlaster driver loaded will reset the GUS address for all samples to 0 (unused) or 1 (used).
				// The first used sample will also have an address of 1 with the GUS driver.
				// So this is a safe way of telling if the file was last saved with the GUS driver loaded or not if there's more than one sample.
				m_PlayBehaviour.set(PlayBehaviour.St3PortaSampleChange, useGus);
				m_PlayBehaviour.set(PlayBehaviour.St3SampleSwap, !useGus);
				m_PlayBehaviour.set(PlayBehaviour.ItShortSampleRetrig, !useGus);		// Only half the truth but close enough for now

				m_ModFormat.MadeWithTracker += useGus ? " (GUS)" : " (SB)";

				// ST3's GUS driver doesn't use this value. Ignoring it fixes the balance between FM and PCM samples (e.g. in Rotagilla by Manwe)
				if (useGus)
					m_nSamplePreAmp = 48;
			}

			if (isSt3)
				m_PlayBehaviour.set(PlayBehaviour.S3MIgnoreCombinedFineSlides);

			if (anyAdpcm)
			{
				m_ModFormat.MadeWithTracker += " (ADPCM packed)";
				m_ModFormat.ExtraInformation = Resources.IDS_MPT_ADPCM;
			}

			// Try to find out if Zxx commands are supposed to be panning commands (PixPlay).
			// Actually I am only aware of one module that uses this panning style, namely "Crawling Despair" by $volkraq
			// and I have no idea what PixPlay is, so this code is solely based on the sample text of that module.
			// We won't convert if there are not enough Zxx commands, too "high" Zxx commands
			// or there are only "left" or "right" pannings (we assume that stereo should be somewhat balanced),
			// and modules not made with an old version of ST3 were probably made in a tracker that supports panning anyway
			bool pixPlayPanning = fileHeader.Cwtv < S3MFileHeader.TrkSt3_20;
			c_int zxxCountRight = 0, zxxCountLeft = 0;

			// Reading patterns
			if ((loadFlags & ModLoadingFlags.LoadPatternData) == 0)
				return true;

			// Order list cannot contain pattern indices > 255, so do not even try to load higher patterns
			PatternIndex readPatterns = Math.Min(fileHeader.PatNum, uint8.MaxValue);
			Patterns.ResizeArray(readPatterns);

			for (PatternIndex pat = 0; pat < readPatterns; pat++)
			{
				// A zero parapointer indicates an empty pattern
				if (!Patterns.Insert(pat, 64) || (patternOffsets[pat] == 0) || !file.Seek(patternOffsets[pat] * 16U))
					continue;

				// Skip pattern length indication.
				// Some modules, for example http://aminet.net/mods/8voic/s3m_hunt.lha seem to have a wrong pattern length -
				// If you strictly adhere the pattern length, you won't read some patterns (e.g. 17) correctly in that module.
				// It's most likely a broken copy because there are other versions of the track which don't have this issue.
				// Still, we don't really need this information, so we just ignore it
				file.Skip(2);

				// Read pattern data
				RowIndex row = 0;
				MptSpan<ModCommand> rowBase = Patterns[pat].GetRow(0);

				ModCommand dummy = new ModCommand();

				while (row < 64)
				{
					uint8 info = file.ReadUInt8();

					if (info == S3MEndOfRow)
					{
						// End of row
						if (++row < 64)
							rowBase = Patterns[pat].GetRow(row);

						continue;
					}

					ChannelIndex channel = (ChannelIndex)(info & S3MChannelMask);
					ModCommand m = channel < GetNumChannels() ? rowBase[channel] : dummy;

					if ((info & S3MNotePresent) != 0)
					{
						uint8 note = file.ReadUInt8();
						uint8 instr = file.ReadUInt8();

						if (note < 0xf0)
							m.Note = OpenMpt.Clamp((ModCommandNote)((note & 0x0f) + (12 * (note >> 4)) + 12 + ModCommand.Note_Min), ModCommand.Note_Min, ModCommand.Note_Max);
						else if (note == S3MNoteOff)
							m.Note = ModCommand.Note_NoteCut;
						else if (note == S3MNoteNone)
							m.Note = ModCommand.Note_None;

						m.Instr = instr;
					}

					if ((info & S3MVolumePresent) != 0)
					{
						uint8 volume = file.ReadUInt8();

						if ((volume >= 128) && (volume <= 192))
						{
							m.VolCmd = VolumeCommand.Panning;
							m.Vol = (uint8)(volume - 128);
						}
						else
						{
							m.VolCmd = VolumeCommand.Volume;
							m.Vol = Math.Min(volume, (uint8)64);
						}
					}

					if ((info & S3MEffectPresent) != 0)
					{
						uint8 command = file.ReadUInt8();
						uint8 param = file.ReadUInt8();

						S3MConvert(m, command, param, false);

						if ((m.Command == EffectCommand.S3MCmdEx) && ((m.Param & 0xf0) == 0xa0) && (fileHeader.Cwtv < S3MFileHeader.TrkSt3_20))
						{
							// Convert the old messy SoundBlaster stereo control command (or an approximation of it, anyway)
							uint8 cType = (uint8)(fileHeader.Channels[channel] & 0x7f);

							if (useGus || (cType >= 0x10))
								m.Command = EffectCommand.Dummy;
							else if ((m.Param == 0xa0) || (m.Param == 0xa2))	// Normal panning
								m.Param = (uint8)((cType & 8) != 0 ? 0x8c : 0x83);
							else if ((m.Param == 0xa1) || (m.Param == 0xa3))	// Swap left / right channel
								m.Param = (uint8)((cType & 8) != 0 ? 0x83 : 0x8c);
							else if (m.Param <= 0xa7)	// Center
								m.Param = 0x88;
							else
								m.Command = EffectCommand.Dummy;
						}
						else if (m.Command == EffectCommand.Midi)
						{
							// PixPlay panning test
							if (m.Param > 0x0f)
							{
								// PixPlay has Z00 to Z0F panning, so we ignore this
								pixPlayPanning = false;
							}
							else
							{
								if (m.Param < 0x08)
									zxxCountLeft++;
								else if (m.Param > 0x08)
									zxxCountRight++;
							}
						}
						else if ((m.Command == EffectCommand.Offset) && (m.Param == 0) && isSt3 && (fileHeader.Cwtv <= S3MFileHeader.TrkSt3_01))
						{
							// Offset command didn't have effect memory in ST3.01; fixed in ST3.03
							m.Command = EffectCommand.Dummy;
						}
					}
				}
			}

			if (pixPlayPanning && ((zxxCountLeft + zxxCountRight) >= GetNumChannels()) && ((-zxxCountLeft + zxxCountRight) < GetNumChannels()))
			{
				// There are enough Zxx commands, so let's assume this was made to be played with PixPlay
				Patterns.ForEachModCommand(new PixPlayPanning());
			}

			return true;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void S3MConvert(ModCommand m, uint8 command, uint8 param, bool fromIt)
		{
			m.Param = param;

			switch (command | 0x40)
			{
				case '@':
				{
					m.Command = m.Param != 0 ? EffectCommand.Dummy : EffectCommand.None;
					break;
				}

				case 'A':
				{
					m.Command = EffectCommand.Speed;
					break;
				}

				case 'B':
				{
					m.Command = EffectCommand.PositionJump;
					break;
				}

				case 'C':
				{
					m.Command = EffectCommand.PatternBreak;

					if (!fromIt)
						m.Param = (uint8)(((m.Param >> 4) * 10) + (m.Param & 0x0f));

					break;
				}

				case 'D':
				{
					m.Command = EffectCommand.VolumeSlide;
					break;
				}

				case 'E':
				{
					m.Command = EffectCommand.PortamentoDown;
					break;
				}

				case 'F':
				{
					m.Command = EffectCommand.PortamentoUp;
					break;
				}

				case 'G':
				{
					m.Command = EffectCommand.TonePortamento;
					break;
				}

				case 'H':
				{
					m.Command = EffectCommand.Vibrato;
					break;
				}

				case 'I':
				{
					m.Command = EffectCommand.Tremor;
					break;
				}

				case 'J':
				{
					m.Command = EffectCommand.Arpeggio;
					break;
				}

				case 'K':
				{
					m.Command = EffectCommand.VibratoVol;
					break;
				}

				case 'L':
				{
					m.Command = EffectCommand.TonePortaVol;
					break;
				}

				case 'M':
				{
					m.Command = EffectCommand.ChannelVolume;
					break;
				}

				case 'N':
				{
					m.Command = EffectCommand.ChannelVolSlide;
					break;
				}

				case 'O':
				{
					m.Command = EffectCommand.Offset;
					break;
				}

				case 'P':
				{
					m.Command = EffectCommand.PanningSlide;
					break;
				}

				case 'Q':
				{
					m.Command = EffectCommand.Retrig;
					break;
				}

				case 'R':
				{
					m.Command = EffectCommand.Tremolo;
					break;
				}

				case 'S':
				{
					m.Command = EffectCommand.S3MCmdEx;
					break;
				}

				case 'T':
				{
					m.Command = EffectCommand.Tempo;
					break;
				}

				case 'U':
				{
					m.Command = EffectCommand.FineVibrato;
					break;
				}

				case 'V':
				{
					m.Command = EffectCommand.GlobalVolume;
					break;
				}

				case 'W':
				{
					m.Command = EffectCommand.GlobalVolSlide;
					break;
				}

				case 'X':
				{
					m.Command = EffectCommand.Panning8;
					break;
				}

				case 'Y':
				{
					m.Command = EffectCommand.Panbrello;
					break;
				}

				case 'Z':
				{
					m.Command = EffectCommand.Midi;
					break;
				}

				case '\\':
				{
					m.Command = fromIt ? EffectCommand.SmoothMidi : EffectCommand.Midi;
					break;
				}

				// Chars under 0x40 don't save properly, so the following commands don't map to their pattern editor representations

				case ']':
				{
					m.Command = fromIt ? EffectCommand.DelayCut : EffectCommand.None;
					break;
				}

				case '[':
				{
					m.Command = fromIt ? EffectCommand.XParam : EffectCommand.None;
					break;
				}

				case '^':
				{
					m.Command = fromIt ? EffectCommand.FineTune : EffectCommand.None;
					break;
				}

				case '_':
				{
					m.Command = fromIt ? EffectCommand.FineTune_Smooth : EffectCommand.None;
					break;
				}

				// BeRoTracker extensions

				case '1' + 0x41:
				{
					m.Command = fromIt ? EffectCommand.KeyOff : EffectCommand.None;
					break;
				}

				case '2' + 0x41:
				{
					m.Command = fromIt ? EffectCommand.SetEnvPosition : EffectCommand.None;
					break;
				}

				default:
				{
					m.Command = EffectCommand.None;
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static bool ValidateHeader(ref S3MFileHeader fileHeader)
		{
			if ((CMemory.memcmp(fileHeader.Magic.ToArray().begin(), "SCRM", 4) != 0) || (fileHeader.FileType != S3MFileHeader.IdS3MType) ||
			    ((fileHeader.FormatVersion != S3MFileHeader.OldVersion) && (fileHeader.FormatVersion != S3MFileHeader.NewVersion)))
			{
				return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static uint64 GetHeaderMinimumAdditionalSize(ref S3MFileHeader fileHeader)
		{
			return (uint64)(fileHeader.OrdNum + ((fileHeader.SmpNum + fileHeader.PatNum) * 2));
		}



		/********************************************************************/
		/// <summary>
		/// TNE: Added extra checks so only OpenMPT modules are recognized
		/// </summary>
		/********************************************************************/
		private static ProbeResult ExtendedProbeS3M(FileReader file, ref S3MFileHeader fileHeader)
		{
			if (fileHeader.SmpNum == 0)
				return ProbeResult.Failure;

			// Read sample header offsets
			file.Seek(0x60U + fileHeader.OrdNum);

			vector<uint16le> sampleOffsets = new vector<uint16le>();
			file.ReadVector(sampleOffsets, fileHeader.SmpNum);

			// Read first pattern offset
			uint16 firstPatternOffset = file.ReadUInt16LE();

			if (((fileHeader.Cwtv & S3MFileHeader.TrackerMask) == S3MFileHeader.TrkScreamTracker) && (fileHeader.Reserved2 == 0x4353) && (fileHeader.Reserved3 == 0x3242554c) && (fileHeader.Reserved4 == 0x302e))	// SCLUB2.0
				return ProbeResult.Failure;

			if ((fileHeader.Cwtv == S3MFileHeader.TrkSt3_20) && (fileHeader.Special == 0) && ((fileHeader.OrdNum & 0x0f) == 0) && (fileHeader.UltraClicks == 0) && ((fileHeader.Flags & ~0x50) == 0) && (fileHeader.UsePanningTable == S3MFileHeader.IdPanning) && (fileHeader.PatNum > 0) && (fileHeader.SmpNum > 0) && (firstPatternOffset > sampleOffsets[0]))
				return ProbeResult.Success;

			if (((fileHeader.Cwtv & S3MFileHeader.TrackerMask) == S3MFileHeader.TrkOpenMpt) && ((fileHeader.Cwtv & 0xff00) != S3MFileHeader.TrkNesMusa) && ((fileHeader.Reserved2 != 0) || (fileHeader.UltraClicks != 16) || (fileHeader.Channels[1] == 1)) && (fileHeader.Cwtv != S3MFileHeader.TrkGraoumfTracker))
				return ProbeResult.Success;

			// Check if any of the samples is in ADPCM format
			for (SampleIndex i = 0; i < fileHeader.SmpNum; i++)
			{
				if (!file.Seek(sampleOffsets[i] * 16U))
					return ProbeResult.Failure;

				uint8 type = file.ReadUInt8();
				if (type < S3MSampleHeader.TypeAdMel)
				{
					file.Skip(15);

					uint32 length = file.ReadUInt32LE();
					if (length == 0)
						continue;

					file.Skip(10);

					uint8 pack = file.ReadUInt8();
					uint8 flags = file.ReadUInt8();

					if ((pack == S3MSampleHeader.PAdpcm) && ((flags & S3MSampleHeader.Smp16Bit) == 0) && ((flags & S3MSampleHeader.SmpStereo) == 0))
						return ProbeResult.Success;
				}
			}

			return ProbeResult.Failure;
		}
		#endregion
	}
}
