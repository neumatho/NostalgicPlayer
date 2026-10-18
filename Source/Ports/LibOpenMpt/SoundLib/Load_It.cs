/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.C.Std.Iterators;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.Srlztn;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.String;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Exceptions;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.FileFormat_Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Endian;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;
using Algorithm = Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base.Algorithm;
using Utility = Polycode.NostalgicPlayer.Kit.C.Std.Utility;
using Version = Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.Version;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// IT (Impulse Tracker) module loader
	/// Notes: Also handles MPTM loading, as the formats are almost identical
	/// </summary>
	internal partial class CSoundFile
	{
		#region OpenMPT IT loader class
		public class ItLoader : IFormatLoader
		{
			public static readonly FileFormatLoader Format = new FileFormatLoader
			{
				Id = Guid.Parse("DE400ED9-E17C-4224-AB08-3533227B6A48"),
				Name = Resources.IDS_MPT_IT_NAME,
				Description = Resources.IDS_MPT_IT_DESCRIPTION,
				Prober = Probe_OpenMpt,
				Create = Create
			};

			private readonly CSoundFile sndFile;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			private ItLoader(CSoundFile soundFile)
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
				return ProbeFileHeaderIt(file, pFileSize, InternalItFormat.It);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public static IFormatLoader Create(CSoundFile soundFile)
			{
				return new ItLoader(soundFile);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public bool Read(FileReader file, ModLoadingFlags loadFlags)
			{
				return sndFile.ReadIt(file, loadFlags);
			}
		}
		#endregion

		private enum InternalItFormat
		{
			Unknown,
			It,
			Mptm
		}

		/// <summary>
		/// If cwtv-field is greater or equal to this value,
		/// the MPTM file will not be loaded
		/// </summary>
		private const uint16 VerMptFileVerLoadLimit = 0x1000;

		private static readonly uint8[] maskToSkips = [ 0, 1, 1, 2, 1, 2, 2, 3, 2, 3, 3, 4, 3, 4, 4, 5 ];

		private static readonly pair<int32, PlayBehaviour>[] schismQuirks =
		[
			new pair<int32, PlayBehaviour>(new SchismVersionFromDate(2015,  1, 29).Date, PlayBehaviour.PeriodsAreHertz),					// https://github.com/schismtracker/schismtracker/commit/671b30311082a0e7df041fca25f989b5d2478f69
			new pair<int32, PlayBehaviour>(new SchismVersionFromDate(2016,  5, 13).Date, PlayBehaviour.ItShortSampleRetrig),				// https://github.com/schismtracker/schismtracker/commit/e7b1461fe751554309fd403713c2a1ef322105ca
			new pair<int32, PlayBehaviour>(new SchismVersionFromDate(2021,  5,  2).Date, PlayBehaviour.ItDoNotOverrideChannelPan),		// https://github.com/schismtracker/schismtracker/commit/a34ec86dc819915debc9e06f4727b77bf2dd29ee
			new pair<int32, PlayBehaviour>(new SchismVersionFromDate(2021,  5,  2).Date, PlayBehaviour.ItPanningReset),					// https://github.com/schismtracker/schismtracker/commit/648f5116f984815c69e11d018b32dfec53c6b97a
			new pair<int32, PlayBehaviour>(new SchismVersionFromDate(2021, 11,  1).Date, PlayBehaviour.ItPitchPanSeparation),			// https://github.com/schismtracker/schismtracker/commit/6e9f1207015cae0fe1b829fff7bb867e02ec6dea
			new pair<int32, PlayBehaviour>(new SchismVersionFromDate(2022,  4, 30).Date, PlayBehaviour.ItEmptyNoteMapSlot),				// https://github.com/schismtracker/schismtracker/commit/1b2f7d5522fcb971f134a6664182ca569f7c8008
			new pair<int32, PlayBehaviour>(new SchismVersionFromDate(2022,  4, 30).Date, PlayBehaviour.ItPortamentoSwapResetsPos),		// https://github.com/schismtracker/schismtracker/commit/1b2f7d5522fcb971f134a6664182ca569f7c8008
			new pair<int32, PlayBehaviour>(new SchismVersionFromDate(2022,  4, 30).Date, PlayBehaviour.ItMultiSampleInstrumentNumber),	// https://github.com/schismtracker/schismtracker/commit/1b2f7d5522fcb971f134a6664182ca569f7c8008
			new pair<int32, PlayBehaviour>(new SchismVersionFromDate(2023,  3,  9).Date, PlayBehaviour.ItInitialNoteMemory),				// https://github.com/schismtracker/schismtracker/commit/73e9d60676c2b48c8e94e582373e29517105b2b1
			new pair<int32, PlayBehaviour>(new SchismVersionFromDate(2023, 10, 17).Date, PlayBehaviour.ItDctBehaviour),					// https://github.com/schismtracker/schismtracker/commit/31d36dc00013fc5ab0efa20c782af18e8b006e07
			new pair<int32, PlayBehaviour>(new SchismVersionFromDate(2023, 10, 19).Date, PlayBehaviour.ItSampleAndHoldPanbrello),		// https://github.com/schismtracker/schismtracker/commit/411ec16b190ba1a486d8b0907ad8d74f8fdc2840
			new pair<int32, PlayBehaviour>(new SchismVersionFromDate(2023, 10, 19).Date, PlayBehaviour.ItPortaNoNote),					// https://github.com/schismtracker/schismtracker/commit/8ff0a86a715efb50c89770fb9095d4c4089ff187
			new pair<int32, PlayBehaviour>(new SchismVersionFromDate(2023, 10, 22).Date, PlayBehaviour.ItFirstTickHandling),				// https://github.com/schismtracker/schismtracker/commit/b9609e4f827e1b6ce9ebe6573b85e69388ca0ea0
			new pair<int32, PlayBehaviour>(new SchismVersionFromDate(2023, 10, 22).Date, PlayBehaviour.ItMultiSampleInstrumentNumber),	// https://github.com/schismtracker/schismtracker/commit/a9e5df533ab52c35190fcc1cbfed4f0347b660bb
			new pair<int32, PlayBehaviour>(new SchismVersionFromDate(2024,  3,  9).Date, PlayBehaviour.ItPanbrelloHold),					// https://github.com/schismtracker/schismtracker/commit/ebdebaa8c8a735a7bf49df55debded1b7aac3605
			new pair<int32, PlayBehaviour>(new SchismVersionFromDate(2024,  5, 12).Date, PlayBehaviour.ItNoSustainOnPortamento),			// https://github.com/schismtracker/schismtracker/commit/6f68f2855a7e5e4ffe825869244e631e15741037
			new pair<int32, PlayBehaviour>(new SchismVersionFromDate(2024,  5, 12).Date, PlayBehaviour.ItEmptyNoteMapSlotIgnoreCell),	// https://github.com/schismtracker/schismtracker/commit/aa84148e019a65f3d52ecd33fd84bfecfdb87bf4
			new pair<int32, PlayBehaviour>(new SchismVersionFromDate(2024,  5, 27).Date, PlayBehaviour.ItOffsetWithInstrNumber),			// https://github.com/schismtracker/schismtracker/commit/9237960d45079a54ad73f87bacfe5dd8ae82e273
			new pair<int32, PlayBehaviour>(new SchismVersionFromDate(2024, 10, 13).Date, PlayBehaviour.ItDoublePortamentoSlides),		// https://github.com/schismtracker/schismtracker/commit/223e327d9448561931b8cac8a55180286b17276c
			new pair<int32, PlayBehaviour>(new SchismVersionFromDate(2025,  1,  8).Date, PlayBehaviour.ItCarryAfterNoteOff),				// https://github.com/schismtracker/schismtracker/commit/ff7a817df327c8f13d97b8c6546a9329f59edff8
			new pair<int32, PlayBehaviour>(new SchismVersionFromDate(2026,  7, 13).Date, PlayBehaviour.ItCompatGxxCarryPortaWithIns)		// https://github.com/schismtracker/schismtracker/commit/21bbd9a74df8266efe10c1c7c8aaccfc7512ede5
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static ProbeResult ProbeFileHeaderIt(FileReader file, uint64? pFileSize, InternalItFormat format)
		{
			ItFileHeader fileHeader = new ItFileHeader();

			if (!file.ReadStruct(ref fileHeader))
				return ProbeResult.WantMoreData;

			if (!ValidateHeader(ref fileHeader))
				return ProbeResult.Failure;

			ProbeResult result = ProbeAdditionalSize(file, pFileSize, GetHeaderMinimumAdditionalSize(ref fileHeader));
			if (result != ProbeResult.Success)
				return result;

			result = ExtendedProbeIt(file, ref fileHeader, out InternalItFormat probedFormat);

			if ((result == ProbeResult.Success) && (probedFormat == format))
				return ProbeResult.Success;

			return ProbeResult.Failure;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool ReadIt(FileReader file, ModLoadingFlags loadFlags)
		{
			file.Rewind();

			ItFileHeader fileHeader = new ItFileHeader();

			if (!file.ReadStruct(ref fileHeader))
				return false;

			if (!ValidateHeader(ref fileHeader))
				return false;

			if (!file.CanRead(GetHeaderMinimumAdditionalSize(ref fileHeader)))
				return false;

			if (loadFlags == ModLoadingFlags.OnlyVerifyHeader)
				return true;

			InitializeGlobals(ModType.It, 0);

			bool interpretModPlugMade = false;
			string madeWithTracker = string.Empty;

			// OpenMPT crap at the end of file
			size_t mptStartPos = 0;

			if (CMemory.memcmp(fileHeader.Id.ToArray().begin(), "tpm.", 4) == 0)
			{
				// Legacy MPTM files (old 1.17.02.4x releases)
				SetType(ModType.Mpt);

				file.Seek(file.GetLength() - 4);
				mptStartPos = file.ReadUInt32LE();
			}
			else
			{
				if ((fileHeader.Cwtv > 0x888) && (fileHeader.Cwtv <= 0xfff))
				{
					file.Seek(file.GetLength() - 4);
					mptStartPos = file.ReadUInt32LE();

					if ((mptStartPos >= 0x100) && (mptStartPos < file.GetLength()))
					{
						if (file.Seek(mptStartPos) && file.ReadMagic("228"))
						{
							SetType(ModType.Mpt);

							if (fileHeader.Cwtv >= VerMptFileVerLoadLimit)
								throw new OpenMptException(Resources.IDS_MPT_ERR_INCOMPATIBLE_VERSION);
						}
					}
				}

				if (GetType_() == ModType.It)
				{
					// Which tracker was used to make this?
					if ((fileHeader.Cwtv & 0xf000) == 0x5000)
					{
						// OpenMPT Version number (Major.Minor)
						// This will only be interpreted as "made with ModPlug" (i.e. disable compatible playback etc) if the "reserved" field is set to "OMPT" - else, compatibility was used
						uint32 mptVersion = (uint32)((fileHeader.Cwtv & 0x0fff) << 16);

						if (CMemory.memcmp(Magic.MagicToStringLE(fileHeader.Reserved).ToPointer(), "OMPT", 4) == 0)
							interpretModPlugMade = true;
						else if (mptVersion >= 0x01_29_00_00)
							mptVersion |= fileHeader.Reserved & 0xffff;

						m_dwLastSavedWithVersion = new Version(mptVersion);
					}
					else if ((fileHeader.Cmwt == 0x888) || (fileHeader.Cwtv == 0x888))
					{
						// OpenMPT 1.17.02.26 (r122) to 1.18
						// Exact version number will be determined later
						interpretModPlugMade = true;
						m_dwLastSavedWithVersion = Version.MPT_V._1_17_00_00;
					}
					else if ((fileHeader.Cwtv == 0x0214) && (fileHeader.Cmwt == 0x0202) && (fileHeader.Reserved == 0))
					{
						// ModPlug Tracker b3.2 - 1.09, instruments 557 bytes apart
						m_dwLastSavedWithVersion = Version.MPT_V._1_09_00_00;
						madeWithTracker = "ModPlug Tracker b3.2 - 1.09";
						interpretModPlugMade = true;
					}
					else if ((fileHeader.Cwtv == 0x0300) && (fileHeader.Cmwt == 0x0300) && (fileHeader.Reserved == 0) && (fileHeader.OrdNum == 256) && (fileHeader.Sep == 128) && (fileHeader.Pwd == 0))
					{
						// A rare variant used from OpenMPT 1.17.02.20 (r113) to 1.17.02.25 (r121), found e.g. in xTr1m-SD.it
						m_dwLastSavedWithVersion = Version.MPT_V._1_17_02_20;
						interpretModPlugMade = true;
					}
				}
			}

			m_SongFlags.Set(SongFlags.LinearSlides, (fileHeader.Flags & ItFileHeader.LinearSlides) != 0);
			m_SongFlags.Set(SongFlags.ItOldEffects, (fileHeader.Flags & ItFileHeader.ItOldEffects) != 0);
			m_SongFlags.Set(SongFlags.ItCompatGxx, (fileHeader.Flags & ItFileHeader.ItCompatGxx) != 0);
			m_SongFlags.Set(SongFlags.ExFilterRange, (fileHeader.Flags & ItFileHeader.ExtendedFilterRange) != 0);

			m_SongName = MptString.ReadBuf(ReadWriteMode.SpacePadded, fileHeader.SongName.ToArray());

			// Read row highlights
			if ((fileHeader.Special & ItFileHeader.EmbedPatternHighlights) != 0)
			{
				// MPT 1.09 and older (and maybe also newer) versions leave this blank (0/0), but have the "special" flag set.
				// Newer versions of MPT and OpenMPT 1.17 *always* write 4/16 here.
				// Thus, we will just ignore those old versions.
				// Note: OpenMPT 1.17.03.02 was the first version to properly make use of the time signature in the IT header.
				// This poses a small unsolvable problem:
				// - In compatible mode, we cannot distinguish this version from earlier 1.17 releases.
				//   Thus we cannot know when to read this field or not (m_dwLastSavedWithVersion will always be 1.17.00.00).
				//   Luckily OpenMPT 1.17.03.02 should not be very wide-spread.
				// - In normal mode the time signature is always present in the song extensions anyway. So it's okay if we read
				//   the signature here and maybe overwrite it later when parsing the song extensions
				if (!m_dwLastSavedWithVersion || (m_dwLastSavedWithVersion >= Version.MPT_V._1_17_03_02))
				{
					m_nDefaultRowsPerBeat = fileHeader.Highlight_Minor;
					m_nDefaultRowsPerMeasure = fileHeader.Highlight_Major;
				}
			}

			// Global volume
			m_nDefaultGlobalVolume = (uint32)(fileHeader.GlobalVol << 1);

			if (m_nDefaultGlobalVolume > Snd_Def.Max_Global_Volume)
				m_nDefaultGlobalVolume = Snd_Def.Max_Global_Volume;

			if (fileHeader.Speed != 0)
				Order.Current.SetDefaultSpeed(fileHeader.Speed);

			Order.Current.SetDefaultTempoInt(Math.Max((uint8)31, fileHeader.Tempo));
			m_nSamplePreAmp = Math.Min(fileHeader.Mv, (uint8)128);

			// Reading orders
			file.Seek((size_t)Marshal.SizeOf<ItFileHeader>());

			if ((GetType_() == ModType.Mpt) && (fileHeader.Cwtv > 0x88a) && (fileHeader.Cwtv <= 0x88d))
			{
				// Deprecated format used for MPTm files created with OpenMPT 1.17.02.46 - 1.17.02.48
				uint16 version = file.ReadUInt16LE();

				if (version != 0)
					return false;

				uint32 numOrd = file.ReadUInt32LE();

				if ((numOrd > ModSpecs.Mptm.OrdersMax) || !Loaders.ReadOrderFromFile<uint32le, uint32>(Order.Current, file, numOrd))
					return false;
			}
			else
				Loaders.ReadOrderFromFile<uint8>(Order.Current, file, fileHeader.OrdNum, 0xff, 0xfe);

			// Reading instrument, sample and pattern offsets
			vector<uint32le> insPos = new vector<uint32le>();
			vector<uint32le> smpPos = new vector<uint32le>();
			vector<uint32le> patPos = new vector<uint32le>();

			if (!file.ReadVector(insPos, fileHeader.InsNum) || !file.ReadVector(smpPos, fileHeader.SmpNum) || !file.ReadVector(patPos, fileHeader.PatNum))
				return false;

			// Find the first parapointer.
			// This is used for finding out whether the edit history is actually stored in the file or not,
			// as some early versions of Schism Tracker set the history flag, but didn't save anything.
			// We will consider the history invalid if it ends after the first parapointer
			uint32 minPtr = uint32.MaxValue;

			foreach (uint32 pos in insPos)
			{
				if ((pos > 0) && (pos < minPtr))
					minPtr = pos;
			}

			foreach (uint32 pos in smpPos)
			{
				if ((pos > 0) && (pos < minPtr))
					minPtr = pos;
			}

			foreach (uint32 pos in patPos)
			{
				if ((pos > 0) && (pos < minPtr))
					minPtr = pos;
			}

			if ((fileHeader.Special & ItFileHeader.EmbedSongMessage) != 0)
				minPtr = Math.Min(minPtr, fileHeader.MsgOffset.Value);

			bool possiblyUnmo3 = (fileHeader.Cmwt == 0x0214) && ((fileHeader.Cwtv == 0x0214) || (fileHeader.Cwtv == 0)) &&
								 (fileHeader.Highlight_Major == 0) && (fileHeader.Highlight_Minor == 0) &&
								 (fileHeader.Pwd == 0) && (fileHeader.Reserved == 0) &&
								 ((fileHeader.Flags & (ItFileHeader.UseMidiPitchController | ItFileHeader.ReqEmbeddedMidiConfig)) == 0);

			if (possiblyUnmo3 && (fileHeader.InsNum == 0) && (fileHeader.SmpNum > 0) && ((file.GetPosition() + (4 * smpPos.size()) + 2) <= minPtr))
			{
				// UNMO3 < v2.4.0.1 reserves some space for instrument parapointers even in sample mode.
				// This makes reading MIDI macros and plugin information impossible.
				// Note: While UNMO3 and CheeseTracker header fingerprints are almost identical, we cannot mis-detect CheeseTracker here,
				// as it always sets the instrument mode flag and writes non-zero row highlights
				bool oldUnmo3 = true;

				for (uint16 i = 0; i < fileHeader.SmpNum; i++)
				{
					if (file.ReadUInt32LE() != 0)
					{
						oldUnmo3 = false;
						file.SkipBack((size_t)(4 + (i * 4)));
						break;
					}
				}

				if (oldUnmo3)
					madeWithTracker = "UNMO3 <= 2.4";
			}

			if (possiblyUnmo3 && (fileHeader.Cwtv == 0))
				madeWithTracker = "UNMO3 v0/1";

			// Reading IT Edit History Info
			// This is only supposed to be present if bit 1 of the special flags is set.
			// However, old versions of Schism and probably other trackers always set this bit
			// even if they don't write the edit history count. So we have to filter this out...
			// This is done by looking at the parapointers. If the history data ends after
			// the first parapointer, we assume that it's actually no history data
			if ((fileHeader.Special & ItFileHeader.EmbedEditHistory) != 0)
			{
				uint16 nFlt = file.ReadUInt16LE();

				if (file.CanRead((size_t)(nFlt * Marshal.SizeOf<ItHistoryStruct>())) && ((file.GetPosition() + (size_t)(nFlt * Marshal.SizeOf<ItHistoryStruct>())) <= minPtr))
				{
					m_FileHistory.resize(nFlt);

					foreach (FileHistory mptHistory in m_FileHistory)
					{
						ItHistoryStruct itHistory = new ItHistoryStruct();

						file.ReadStruct(ref itHistory);
						itHistory.ConvertToMpt(mptHistory);
					}

					if (possiblyUnmo3 && (nFlt == 0))
					{
						if ((fileHeader.Special & ItFileHeader.EmbedPatternHighlights) != 0)
							madeWithTracker = "UNMO3 <= 2.4.0.1";	// Set together with MIDI macro embed flag
						else
							madeWithTracker = "UNMO3";		// Either 2.4.0.2+ or no MIDI macros embedded
					}
				}
				else
				{
					// Oops, we were not supposed to read this
					file.SkipBack(2);
				}
			}
			else if (possiblyUnmo3 && (fileHeader.Special <= 1))
			{
				// UNMO3 < v2.4.0.1 will set the edit history special bit if the MIDI macro embed bit is also set,
				// but it always writes the two extra bytes for the edit history length (zeros).
				// If MIDI macros are embedded, we are fine and end up in the first case of the if statement (read edit history).
				// Otherwise we end up here and might have to read the edit history length
				if (file.ReadUInt16LE() == 0)
					madeWithTracker = "UNMO3 <= 2.4";
				else
				{
					// These were not zero bytes, but potentially belong to the upcoming MIDI config - need to skip back.
					// I think the only application that could end up here is CheeseTracker, if it allows to write 0 for both row highlight values.
					// IT 2.14 itself will always write the edit history
					file.SkipBack(2);
				}
			}

			// Reading MIDI Output & Macros
			bool hasMidiConfig = ((fileHeader.Flags & ItFileHeader.ReqEmbeddedMidiConfig) != 0) || ((fileHeader.Special & ItFileHeader.EmbedMidiConfiguration) != 0);

			if (hasMidiConfig && m_MidiCfg.Load(file))
				m_MidiCfg.Sanitize();

			bool hasModPlugExtensions = false;

			// Read pattern names: "PNAM"
			FileReader patNames;

			if (file.ReadMagic("PNAM"))
			{
				patNames = new FileReader(file.ReadChunk(file.ReadUInt32LE()));
				hasModPlugExtensions = true;
			}
			else
				patNames = new FileReader();

			// Read channel names: "CNAM"
			if (file.ReadMagic("CNAM"))
			{
				FileReader chnNames = new FileReader(file.ReadChunk(file.ReadUInt32LE()));
				ChnSettings.resize(Math.Min(Snd_Def.Max_BaseChannels, (ChannelIndex)(chnNames.GetLength() / Snd_Def.Max_ChannelName)));
				hasModPlugExtensions = true;

				foreach (ModChannelSettings chn in ChnSettings)
					chnNames.ReadString(ReadWriteMode.MaybeNullTerminated, chn.szName, Snd_Def.Max_ChannelName);
			}

			// Read mix plugins information
			FileReader pluginChunk = new FileReader(file.ReadChunk((minPtr >= file.GetPosition()) ? (minPtr - file.GetPosition()) : file.BytesLeft()));
			var mixResult = LoadMixPlugins(pluginChunk, false);
			bool hasPluginChunk = mixResult.hasPluginChunks;
			bool isBeRoTracker = mixResult.isBeRoTracker;

			if (hasPluginChunk)
				hasModPlugExtensions = true;

			if ((fileHeader.Cwtv == 0x0217) && (fileHeader.Cmwt == 0x0200) && (fileHeader.Reserved == 0) && !isBeRoTracker)
			{
				if (hasModPlugExtensions ||
				    (!Order.Current.empty() && (Order.Current.back() == Snd_Def.PatternIndex_Invalid)) ||
				    CMemory.memchr(fileHeader.ChnPan.ToArray().begin(), (uint8)0xff, (size_t)Marshal.SizeOf(fileHeader.ChnPan)).IsNotNull)
				{
					m_dwLastSavedWithVersion = Version.MPT_V._1_16;
					madeWithTracker = "ModPlug Tracker 1.09 - 1.16";
				}
				else
				{
					// OpenMPT 1.17 disguised as this in compatible mode,
					// but never writes 0xFF in the pan map for unused channels (which is an invalid value).
					// It also doesn't write a final "---" pattern in the order list.
					// Could also be original ModPlug Tracker though if all 64 channels and no ModPlug extensions are used
					m_dwLastSavedWithVersion = Version.MPT_V._1_17;
					madeWithTracker = "OpenMPT 1.17 (compatibility export)";
				}

				interpretModPlugMade = true;
			}

			// Read Song Message
			if (((fileHeader.Special & ItFileHeader.EmbedSongMessage) != 0) && (fileHeader.MsgLength > 0) && file.Seek(fileHeader.MsgOffset))
			{
				// Generally, IT files should use CR for line endings. However, ChibiTracker uses LF. One could do...
				// if(itHeader.cwtv == 0x0214 && itHeader.cmwt == 0x0214 && itHeader.reserved == ITFileHeader::chibiMagic) --> Chibi detected.
				// But we'll just use autodetection here:
				m_SongMessage.Read(file, fileHeader.MsgLength, SongMessage.LineEnding.Autodetect);
			}

			// Reading Instruments
			m_nInstruments = 0;

			if ((fileHeader.Flags & ItFileHeader.InstrumentMode) != 0)
				m_nInstruments = Math.Min((InstrumentIndex)fileHeader.InsNum, (InstrumentIndex)(Snd_Def.Max_Instruments - 1));

			for (InstrumentIndex i = 0; i < GetNumInstruments(); i++)
			{
				if ((insPos[i] > 0) && file.Seek(insPos[i]) && file.CanRead((size_t)(fileHeader.Cmwt < 0x200 ? Marshal.SizeOf<ItOldInstrument>() : Marshal.SizeOf<ItInstrument>())))
				{
					ModInstrument instrument = AllocateInstrument((InstrumentIndex)(i + 1));

					if (instrument != null)
					{
						ItInstrToMpt(file, instrument, fileHeader.Cmwt);

						// MIDI Pitch Wheel Depth is a global setting in IT. Apply it to all instruments
						instrument.MidiPwd = (int8)fileHeader.Pwd;
					}
				}
			}

			// In order to properly compute the position, in file, of eventual extended settings
			// such as "attack" we need to keep the "real" size of the last sample as those extra
			// setting will follow this sample in the file
			size_t lastSampleOffset = 0;

			if (fileHeader.SmpNum > 0)
				lastSampleOffset = (size_t)(smpPos[fileHeader.SmpNum - 1] + Marshal.SizeOf<ItSample>());

			bool possibleXmConversion = false;

			// There's a bug in IT somewhere that resets the "sample data present" flag in sample headers, but keeps the sample length
			// of a previously deleted sample (presumably).
			// As old ModPlug versions didn't set this flag under some circumstances (if a sample wasn't referenced by any instruments in instrument mode),
			// and because there appear to be some external tools that forget to set this flag at all, we only respect the flag if the file
			// vaguely looks like it was saved with IT. Some files that play garbage data if we don't do this:
			// astral projection.it by Lord Jon Ray
			// classic illusions.it by Blackstar
			// deep in dance.it by Simply DJ
			// There are many more such files but they don't reference the broken samples in their pattern data, or the sample data pointer
			// points right to the end of the file, so in both cases no audible problem can be observed
			bool muteBuggySamples = !interpretModPlugMade && (fileHeader.Cwtv >= 0x0100) && (fileHeader.Cwtv <= 0x0217) && ((fileHeader.Cwtv < 0x0207) || (fileHeader.Reserved != 0));

			// Reading Samples
			m_nSamples = Math.Min((SampleIndex)fileHeader.SmpNum, (SampleIndex)(Snd_Def.Max_Samples - 1));
			bool lastSampleCompressed = false, anyAdpcm = false, anyItCompressed = false;

			for (SampleIndex i = 0; i < GetNumSamples(); i++)
			{
				ItSample sampleHeader = new ItSample();

				if ((smpPos[i] > 0) && file.Seek(smpPos[i]) && file.ReadStruct(ref sampleHeader))
				{
					ModSample sample = Samples[i + 1];

					size_t sampleOffset = sampleHeader.ConvertToMpt(sample);

					if (muteBuggySamples && ((sampleHeader.Flags & ItSample.SampleDataPresent) == 0))
						sample.nLength = 0;

					m_szNames[i + 1].Assign(MptString.ReadBuf(ReadWriteMode.SpacePadded, sampleHeader.Name.ToArray()));

					if (!file.Seek(sampleOffset))
						continue;

					lastSampleCompressed = false;

					if (sample.uFlags.Test(ChannelFlags.Chn_Adlib))
					{
						// FM instrument in MPTM
						throw new NotImplementedException("ADLIB");//XX
					}
					else if (!sample.uFlags.Test(ChannelFlags.Smp_KeepOnDisk))
					{
						SampleIO sampleIo = sampleHeader.GetSampleFormat(fileHeader.Cwtv);

						if ((loadFlags & ModLoadingFlags.LoadSampleData) != 0)
							sampleIo.ReadSample(sample, file);
						else
						{
							if (sampleIo.IsVariableLengthEncoded())
								lastSampleCompressed = true;
							else
								file.Skip(sampleIo.CalculateEncodedSize(sample.nLength));
						}

						if ((sampleIo.GetEncoding() == SampleIO.Encoding.UnsignedPcm) && (sample.nLength != 0))
						{
							// There is some XM to IT converter (don't know which one) and it identifies as IT 2.04.
							// The only safe way to distinguish it from an IT-saved file are the unsigned samples
							possibleXmConversion = true;
						}
						else if (sampleIo.GetEncoding() == SampleIO.Encoding.Adpcm)
							anyAdpcm = true;
						else if ((sampleIo.GetEncoding() == SampleIO.Encoding.It214) || (sampleIo.GetEncoding() == SampleIO.Encoding.It215))
							anyItCompressed = true;
					}
					else
					{
						// External sample in MPTM file
						file.ReadVarInt(out size_t strLen);

						if (((loadFlags & ModLoadingFlags.LoadSampleData) != 0) && (strLen != 0))
						{
							StdString fileNameU8 = new StdString();

							file.ReadString(ReadWriteMode.MaybeNullTerminated, fileNameU8, strLen);
						}
						else
							file.Skip(strLen);
					}

					lastSampleOffset = Math.Max(lastSampleOffset, file.GetPosition());
				}
			}

			m_nSamples = Math.Max((SampleIndex)1, GetNumSamples());

			if (possibleXmConversion && (fileHeader.Cwtv == 0x0204) && (fileHeader.Cmwt == 0x0200) && (fileHeader.Special == 0) && (fileHeader.Reserved == 0) &&
			    ((fileHeader.Flags & ~ItFileHeader.LinearSlides) == (ItFileHeader.UseStereoPlayback | ItFileHeader.InstrumentMode | ItFileHeader.ItOldEffects)) &&
				(fileHeader.GlobalVol == 128) && (fileHeader.Mv == 48) && (fileHeader.Sep == 128) && (fileHeader.Pwd == 0) && (fileHeader.MsgLength == 0))
			{
				foreach (uint8 pan in fileHeader.ChnPan.ToArray())
				{
					if ((pan != 0x20) && (pan != 0xa0))
						possibleXmConversion = false;
				}

				foreach (uint8 vol in fileHeader.ChnVol.ToArray())
				{
					if (vol != 0x40)
						possibleXmConversion = false;
				}

				array<uint8> _songName = fileHeader.SongName.ToArray();
				for (size_t i = 20; i < _songName.size(); i++)
				{
					if (_songName[i] != 0)
						possibleXmConversion = false;
				}

				if (possibleXmConversion)
					madeWithTracker = "XM Conversion";
			}

			m_nMinPeriod = 0;
			m_nMaxPeriod = int32.MaxValue;

			PatternIndex numPats = Math.Min((PatternIndex)patPos.size(), GetModSpecifications().PatternsMax);

			if (numPats != patPos.size())
			{
				;
			}

			if ((loadFlags & ModLoadingFlags.LoadPatternData) == 0)
				numPats = 0;

			// Checking for number of used channels, which is not explicitly specified in the file
			ChannelIndex numChannels = Math.Max(GetNumChannels(), (ChannelIndex)1);

			for (PatternIndex pat = 0; pat < numPats; pat++)
			{
				if ((patPos[pat] == 0) || !file.Seek(patPos[pat]))
					continue;

				uint16 len = file.ReadUInt16LE();
				RowIndex numRows = file.ReadUInt16LE();

				if ((numRows < 1) || (numRows > Snd_Def.Max_Pattern_Rows) || !file.Skip(4))
					continue;

				FileReader patternData = new FileReader(file.ReadChunk(len));
				RowIndex row = 0;

				vector<uint8> chnMask = new vector<uint8>(numChannels);

				while ((row < numRows) && patternData.CanRead(1))
				{
					uint8 b = patternData.ReadUInt8();

					if (b == 0)
					{
						row++;
						continue;
					}

					ChannelIndex ch = (ChannelIndex)(b & ItTools.It_BitMask_PatternChanField);	// 0x7f We have some data grab a byte keeping only 7 bits

					if (ch != 0)
						ch = (ChannelIndex)(ch - 1);	// 0x3f mask of the byte again, keeping only 6 bits

					if (ch >= chnMask.size())
						chnMask.resize(ch + 1U, 0);

					if ((b & ItTools.It_BitMask_PatternChanEnabled) != 0)   // 0x80 check if the upper bit is enabled
						chnMask[ch] = patternData.ReadUInt8();		// Set the channel mask for this channel

					// Channel used
					if ((chnMask[ch] & 0x0f) != 0)		// If this channel is used set m_nChannels
					{
						if ((ch >= numChannels) && (ch < Snd_Def.Max_BaseChannels))
							numChannels = (ChannelIndex)(ch + 1);

						// Skip a number of bytes depending on note, instrument, volume, effect being present
						patternData.Skip(maskToSkips[chnMask[ch] & 0x0f]);
					}
				}

				lastSampleOffset = Math.Max(lastSampleOffset, file.GetPosition());
			}

			ChnSettings.resize(numChannels);

			// Compute extra instruments settings position
			if (lastSampleOffset > 0)
			{
				file.Seek(lastSampleOffset);

				if (lastSampleCompressed)
				{
					// If the last sample was compressed, we do not know where it ends.
					// Hence, in case we decided not to decode the sample data, we now
					// have to emulate this until we reach EOF or some instrument / song properties
					while (file.CanRead(4))
					{
						if (file.ReadMagic("XTPM") || file.ReadMagic("STPM"))
						{
							uint32 id = file.ReadUInt32LE();
							file.SkipBack(8);

							// Our chunk IDs should only contain ASCII characters
							if (((id & 0x80808080) == 0) && ((id & 0x60606060) != 0))
								break;
						}

						file.Skip(file.ReadUInt16LE());
					}
				}
			}

			// Load instrument and song extensions
			bool hasExtendedInstrumentProperties = LoadExtendedInstrumentProperties(file);

			if (interpretModPlugMade && !isBeRoTracker)
			{
				m_PlayBehaviour.reset();
				m_nMixLevels = MixLevels.Original;
			}

			// Need to do this before reading the patterns because m_nChannels might be modified by LoadExtendedSongProperties. *sigh*
			bool hasExtendedSongProperties = LoadExtendedSongProperties(file, false, ref interpretModPlugMade);

			// Reading Channels Pan Positions
			ChannelIndex headerChannels = Math.Min(GetNumChannels(), (ChannelIndex)64);
			array<uint8> chnVol = fileHeader.ChnVol.ToArray();
			array<uint8> chnPan = fileHeader.ChnPan.ToArray();

			for (ChannelIndex i = 0; i < headerChannels; i++)
			{
				if (fileHeader.ChnPan[i] == 0xff)
					continue;

				ChnSettings[i].nVolume = OpenMpt.Clamp<uint8, uint8>(chnVol[i], 0, 64);

				if ((chnPan[i] & 0x80) != 0)
					ChnSettings[i].dwFlags.Set(ChannelFlags.Chn_Mute);

				uint8 n = (uint8)(chnPan[i] & 0x7f);

				if (n <= 64)
					ChnSettings[i].nPan = (uint16)(n * 4);

				if (n == 100)
					ChnSettings[i].dwFlags.Set(ChannelFlags.Chn_Surround);
			}

			// Reading Patterns
			Patterns.ResizeArray(numPats);
			bool hasVolColOffset = false;

			for (PatternIndex pat = 0; pat < numPats; pat++)
			{
				if ((patPos[pat] == 0) || !file.Seek(patPos[pat]))
				{
					// Empty 64-row pattern
					if (!Patterns.Insert(pat, 64))
						break;

					// Now (after the Insert() call), we can read the pattern name
					CopyPatternName(Patterns[pat], patNames);
					continue;
				}

				uint16 len = file.ReadUInt16LE();
				RowIndex numRows = file.ReadUInt16LE();

				if (!file.Skip(4) || !Patterns.Insert(pat, numRows))
					continue;

				FileReader patternData = new FileReader(file.ReadChunk(len));

				// Now (after the Insert() call), we can read the pattern name
				CopyPatternName(Patterns[pat], patNames);

				vector<uint8> chnMask = new vector<uint8>(GetNumChannels());
				vector<ModCommand> lastValue = new vector<ModCommand>(GetNumChannels(), new ModCommand());

				forward_iterator<ModCommand> patData = Patterns[pat].Begin();
				RowIndex row = 0;
				ModCommand dummy = new ModCommand();

				while ((row < numRows) && patternData.CanRead(1))
				{
					uint8 b = patternData.ReadUInt8();

					if (b == 0)
					{
						row++;
						patData += GetNumChannels();
						continue;
					}

					ChannelIndex ch = (ChannelIndex)(b & ItTools.It_BitMask_PatternChanField);	// 0x7f

					if (ch != 0)
						ch = (ChannelIndex)(ch - 1);	// 0x3f

					if (ch >= chnMask.size())
					{
						chnMask.resize(ch + 1U, 0);
						lastValue.resize(ch + 1U, new ModCommand());
					}

					if ((b & ItTools.It_BitMask_PatternChanEnabled) != 0)   // 0x80
						chnMask[ch] = patternData.ReadUInt8();

					// Now we grab the data for this particular row/channel
					ModCommand m = ch < GetNumChannels() ? patData[ch] : dummy;

					if ((chnMask[ch] & 0x10) != 0)
						m.Note = lastValue[ch].Note;

					if ((chnMask[ch] & 0x20) != 0)
						m.Instr = lastValue[ch].Instr;

					if ((chnMask[ch] & 0x40) != 0)
					{
						m.VolCmd = lastValue[ch].VolCmd;
						m.Vol = lastValue[ch].Vol;
					}

					if ((chnMask[ch] & 0x80) != 0)
					{
						m.Command = lastValue[ch].Command;
						m.Param = lastValue[ch].Param;
					}

					if ((chnMask[ch] & 1) != 0)	// Note
					{
						uint8 note = patternData.ReadUInt8();

						if (note < 0x80)
							note += ModCommand.Note_Min;
						else if (note == 0xff)
							note = ModCommand.Note_KeyOff;
						else if (note == 0xfe)
							note = ModCommand.Note_NoteCut;
						else if ((note == 0xfd) && (GetType_() != ModType.Mpt))
							note = ModCommand.Note_None;	// Note: in MPTM format, NOTE_FADE is written as 0xFD to preserve compatibility with older OpenMPT versions
						else
							note = ModCommand.Note_Fade;

						m.Note = lastValue[ch].Note = note;
					}

					if ((chnMask[ch] & 2) != 0)
						m.Instr = lastValue[ch].Instr = patternData.ReadUInt8();

					if ((chnMask[ch] & 4) != 0)
					{
						uint8 vol = patternData.ReadUInt8();

						// 0-64: Set Volume
						if (vol <= 64)
						{
							m.VolCmd = VolumeCommand.Volume;
							m.Vol = vol;
						}
						// 128-192: Set Panning
						else if ((vol >= 128) && (vol <= 192))
						{
							m.VolCmd = VolumeCommand.Panning;
							m.Vol = (uint8)(vol - 128);
						}
						// 65-74: Fine Volume Up
						else if (vol < 75)
						{
							m.VolCmd = VolumeCommand.FineVolUp;
							m.Vol = (uint8)(vol - 65);
						}
						// 75-84: Fine Volume Down
						else if (vol < 85)
						{
							m.VolCmd = VolumeCommand.FineVolDown;
							m.Vol = (uint8)(vol - 75);
						}
						// 85-94: Volume Slide Up
						else if (vol < 95)
						{
							m.VolCmd = VolumeCommand.VolSlideUp;
							m.Vol = (uint8)(vol - 85);
						}
						// 95-104: Volume Slide Down
						else if (vol < 105)
						{
							m.VolCmd = VolumeCommand.VolSlideDown;
							m.Vol = (uint8)(vol - 95);
						}
						// 105-114: Pitch Slide Up
						else if (vol < 115)
						{
							m.VolCmd = VolumeCommand.PortaDown;
							m.Vol = (uint8)(vol - 105);
						}
						// 115-124: Pitch Slide Down
						else if (vol < 125)
						{
							m.VolCmd = VolumeCommand.PortaUp;
							m.Vol = (uint8)(vol - 115);
						}
						// 193-202: Portamento To
						else if ((vol >= 193) && (vol <= 202))
						{
							m.VolCmd = VolumeCommand.TonePortamento;
							m.Vol = (uint8)(vol - 193);
						}
						// 203-212: Vibrato Depth
						else if ((vol >= 203) && (vol <= 212))
						{
							m.VolCmd = VolumeCommand.VibratoDepth;
							m.Vol = (uint8)(vol - 203);

							// Old versions of ModPlug saved this as vibrato speed instead, so let's fix that
							if ((m.Vol != 0) && m_dwLastSavedWithVersion && (m_dwLastSavedWithVersion <= Version.MPT_V._1_17_02_54))
								m.VolCmd = VolumeCommand.VibratoSpeed;
						}
						// 213-222: Unused (was velocity)
						// 223-232: Offset
						else if ((vol >= 223) && (vol <= 232))
						{
							m.VolCmd = VolumeCommand.Offset;
							m.Vol = (uint8)(vol - 223);

							hasVolColOffset = true;
						}

						lastValue[ch].VolCmd = m.VolCmd;
						lastValue[ch].Vol = m.Vol;
					}

					// Reading command/param
					if ((chnMask[ch] & 8) != 0)
					{
						uint8 command = patternData.ReadUInt8();
						uint8 param = patternData.ReadUInt8();

						S3MConvert(m, command, param, true);

						// IT 1.xx does not support high offset command
						if ((m.Command == EffectCommand.S3MCmdEx) && ((m.Param & 0xf0) == 0xa0) && (fileHeader.Cwtv < 0x0200))
							m.Command = EffectCommand.Dummy;
						// Fix handling of commands V81-VFF in ITs made with old Schism Tracker versions
						// (fixed in https://github.com/schismtracker/schismtracker/commit/ab5517d4730d4c717f7ebffb401445679bd30888 - one of the last versions to identify as v0.50)
						else if ((m.Command == EffectCommand.GlobalVolume) && (m.Param > 0x80) && (fileHeader.Cwtv >= 0x1000) && (fileHeader.Cwtv <= 0x1050))
							m.Param = 0x80;

						// In some IT-compatible trackers, it is possible to input a parameter without a command.
						// In this case, we still need to update the last value memory (so that we don't reuse a previous non-empty effect).
						// OpenMPT didn't do this until v1.25.01.07.
						// Example: ckbounce.it
						lastValue[ch].Command = m.Command;
						lastValue[ch].Param = m.Param;
					}
				}
			}

			// Remove (default) cue points if no volume column offset is found (unless it's a new enough MPTM file, which could contain intentionally-placed custom cue points that we don't want to lose)
			if (!hasVolColOffset && ((GetType_() != ModType.Mpt) || (m_dwLastSavedWithVersion < Version.MPT_V._1_24_02_06)))
			{
				for (SampleIndex smp = 1; smp <= GetNumSamples(); smp++)
					Samples[smp].RemoveAllCuePoints();
			}

			if (!m_dwLastSavedWithVersion && (fileHeader.Cwtv == 0x0888))
			{
				// Up to OpenMPT 1.17.02.45 (r165), it was possible that the "last saved with" field was 0
				// when saving a file in OpenMPT for the first time
				m_dwLastSavedWithVersion = Version.MPT_V._1_17_00_00;
			}

			if (m_dwLastSavedWithVersion && string.IsNullOrEmpty(madeWithTracker))
			{
				madeWithTracker = "OpenMPT " + m_dwLastSavedWithVersion;

				bool isCompatExport = (CMemory.memcmp(Magic.MagicToStringLE(fileHeader.Reserved).ToPointer(), "OMPT", 4) != 0) && ((fileHeader.Cwtv & 0xf000) == 0x5000);

				if (m_dwLastSavedWithVersion == Version.MPT_V._1_17_00_00)
					isCompatExport = !hasExtendedInstrumentProperties && !hasExtendedSongProperties && !hasModPlugExtensions;

				if (isCompatExport)
					madeWithTracker += " (compatibility export)";
				else if (m_dwLastSavedWithVersion.IsTestVersion())
					madeWithTracker += " (test build)";
			}
			else
			{
				int32 schismDateVersion = (int32)(ItTools.SchismTrackerEpoch + ((fileHeader.Cwtv == 0x1fff) ? fileHeader.Reserved : (fileHeader.Cwtv - 0x1050)));

				switch (fileHeader.Cwtv >> 12)
				{
					case 0:
					{
						if (isBeRoTracker)
						{
							// Old versions
							madeWithTracker = "BeRoTracker";
						}
						else if ((fileHeader.Cwtv == 0x0214) && (fileHeader.Cmwt == 0x0200) && (fileHeader.Flags == 9) && (fileHeader.Special == 0) &&
						         (fileHeader.Highlight_Major == 0) && (fileHeader.Highlight_Minor == 0) &&
								 (fileHeader.InsNum == 0) && ((fileHeader.PatNum + 1) == fileHeader.OrdNum) &&
						         (fileHeader.GlobalVol == 128) && (fileHeader.Mv == 100) && (fileHeader.Speed == 1) && (fileHeader.Sep == 128) && (fileHeader.Pwd == 0) &&
						         (fileHeader.MsgLength == 0) && (fileHeader.MsgOffset == 0) && (fileHeader.Reserved == 0))
						{
							madeWithTracker = "OpenSPC conversion";
						}
						else if ((fileHeader.Cwtv == 0x0202) && (fileHeader.Cmwt == 0x0200) && (fileHeader.Highlight_Major == 0) && (fileHeader.Highlight_Minor == 0) && (fileHeader.Reserved == 0) && !patPos.empty() && !smpPos.empty() && (patPos[0] != 0) && (patPos[0] < smpPos[0]))
						{
							// ModPlug Tracker 1.0 pre-alpha up to alpha 4, patterns located before instruments / samples
							m_dwLastSavedWithVersion = Version.MPT_V._1_00_00_A0;
							madeWithTracker = "ModPlug Tracker 1.0 pre-alpha / alpha";
							interpretModPlugMade = true;
						}
						else if ((fileHeader.Cwtv == 0x0214) && (fileHeader.Cmwt == 0x0200) && (fileHeader.Highlight_Major == 0) && (fileHeader.Highlight_Minor == 0) && (fileHeader.Reserved == 0))
						{
							if ((fileHeader.Special & (ItFileHeader.EmbedPatternHighlights | ItFileHeader.EmbedEditHistory)) != 0)
							{
								// ModPlug Tracker 1.0a6/b1/b2
								// Instruments are 557 bytes apart in beta 2.3, in beta 1 still 560 bytes like in earlier versions
								if ((insPos.size() >= 2) && ((insPos[1] - insPos[0]) == 557))
								{
									m_dwLastSavedWithVersion = Version.MPT_V._1_00_00_B2;
									madeWithTracker = "ModPlug Tracker 1.0b2";
								}
								else
								{
									m_dwLastSavedWithVersion = Version.MPT_V._1_00_00_B1;
									madeWithTracker = "ModPlug Tracker 1.0 alpha / beta";
								}
							}
							else
							{
								// ModPlug Tracker 1.0a5, instruments 560 bytes apart
								m_dwLastSavedWithVersion = Version.MPT_V._1_00_00_A5;
								madeWithTracker = "ModPlug Tracker 1.0a5";
							}

							interpretModPlugMade = true;
						}
						else if ((fileHeader.Cwtv == 0x0214) && (fileHeader.Cmwt == 0x0214) && (CMemory.memcmp(Magic.MagicToStringLE(fileHeader.Reserved).ToPointer(), "CHBI", 4) == 0))
						{
							madeWithTracker = "ChibiTracker";
							m_PlayBehaviour.reset(PlayBehaviour.ItShortSampleRetrig);
							m_nSamplePreAmp /= 2;
						}
						else if ((fileHeader.Cwtv == 0x0214) && (fileHeader.Cmwt == 0x0214) && (fileHeader.Special <= 1) && (fileHeader.Pwd == 0) && (fileHeader.Reserved == 0) &&
						         ((fileHeader.Flags & (ItFileHeader.Vol0Optimisations | ItFileHeader.InstrumentMode | ItFileHeader.UseMidiPitchController | ItFileHeader.ReqEmbeddedMidiConfig | ItFileHeader.ExtendedFilterRange)) == ItFileHeader.InstrumentMode) &&
						         (m_nSamples > 1) && (Samples[1].FileName.ToString() == "XXXXXXXX.YYY"))
						{
							madeWithTracker = "CheeseTracker";
						}
						else if ((fileHeader.Cwtv == 0) && string.IsNullOrEmpty(madeWithTracker))
							madeWithTracker = "Unknown";
						else if ((fileHeader.Cwtv >= 0x0208) && (fileHeader.Cwtv <= 0x0214) && (fileHeader.Reserved == 0) && m_FileHistory.empty() && string.IsNullOrEmpty(madeWithTracker))
						{
							// Any file made with IT starting from v2.07 onwards should have an edit history
							madeWithTracker = "Unknown";
						}
						else if ((fileHeader.Cmwt < 0x0300) && string.IsNullOrEmpty(madeWithTracker))
						{
							madeWithTracker = GetImpulseTrackerVersion(fileHeader.Cwtv, fileHeader.Cmwt);

							if (m_FileHistory.empty() && (fileHeader.Reserved != 0))
							{
								// Starting from version 2.07, IT stores the total edit time of a module in the "reserved" field
								uint32 editTime = ItTools.DecodeItEditTimer(fileHeader.Cwtv, fileHeader.Reserved);

								FileHistory hist = new FileHistory();
								hist.OpenTime = (uint32)(editTime * (FileHistory.History_Timer_Precision / 18.2));
								m_FileHistory.push_back(hist);
							}
						}
						break;
					}

					case 1:
					{
						madeWithTracker = GetSchismTrackerVersion(fileHeader.Cwtv, fileHeader.Reserved);

						{
							foreach (pair<int32, PlayBehaviour> quirk in schismQuirks)
							{
								if (schismDateVersion < quirk.first)
									m_PlayBehaviour.reset(quirk.second);
							}
						}

						// Hertz in Amiga mode: Added 2021-05-02, https://github.com/schismtracker/schismtracker/commit/c656a6cbd5aaf81198a7580faf81cb7960cb6afa
						if ((schismDateVersion < new SchismVersionFromDate(2021, 5, 2).Date) && !m_SongFlags.Test(SongFlags.LinearSlides))
							m_PlayBehaviour.reset(PlayBehaviour.PeriodsAreHertz);

						// Imprecise calculation of ping-pong loop wraparound: Added 2021-11-01, https://github.com/schismtracker/schismtracker/commit/22cbb9b676e9c2c9feb7a6a17deca7a17ac138cc
						if (schismDateVersion < new SchismVersionFromDate(2021, 11, 1).Date)
							m_PlayBehaviour.set(PlayBehaviour.ImprecisePingPongLoops);

						break;
					}

					case 4:
					{
						madeWithTracker = string.Format("pyIT {0}.{1:x2}", (fileHeader.Cwtv & 0x0f00) >> 8, fileHeader.Cwtv & 0xff);
						break;
					}

					case 6:
					{
						madeWithTracker = "BeRoTracker";
						break;
					}

					case 7:
					{
						if ((fileHeader.Cwtv == 0x7fff) && (fileHeader.Cmwt == 0x0215))
							madeWithTracker = "munch.py";
						else
							madeWithTracker = string.Format("ITMCK {0}.{1}.{2}", (fileHeader.Cwtv >> 8) & 0x0f, (fileHeader.Cwtv >> 4) & 0x0f, fileHeader.Cwtv & 0x0f);

						break;
					}

					case 0xd:
					{
						if (fileHeader.Cwtv == 0xdaeb)
							madeWithTracker = "spc2it";
						else if (fileHeader.Cwtv == 0xd1ce)
							madeWithTracker = "itwriter";
						else
							madeWithTracker = "Unknown";

						break;
					}

					case 0xe:
					{
						if (fileHeader.Cwtv == 0xefff)
							madeWithTracker = "rosetracker";
						else
							madeWithTracker = "Unknown";

						break;
					}

					default:
					{
						madeWithTracker = "Unknown";
						break;
					}
				}
			}

			if (anyItCompressed)
				madeWithTracker += Resources.IDS_MPT_ITCOMPRESSED;

			if (anyAdpcm)
				madeWithTracker += Resources.IDS_MPT_ADPCM;

			// Ignore MIDI data. Fixes some files like denonde.it that were made with old versions of Impulse Tracker (which didn't support Zxx filters) and have Zxx effects in the patterns.
			// Example: denonde.it by Mystical
			// Note: Only checking the cwtv "made with" version is not enough: spx-visionsofthepast.it has the strange combination of cwtv=2.00, cmwt=2.16
			// Hence to be sure, we check that both values are below 2.14.
			// Note that all ModPlug Tracker alpha versions do not support filters yet. Earlier alphas identify as cwtv=2.02, cmwt=2.00, but later alpha versions identify as IT 2.14.
			// Apart from that, there's an unknown XM conversion tool declaring a lower compatible version, which naturally also does not support filters, so it's okay that it is caught here
			if (((fileHeader.Cwtv < 0x0214) && (fileHeader.Cmwt < 0x0214)) || (m_dwLastSavedWithVersion && (m_dwLastSavedWithVersion <= Version.MPT_V._1_00_00_A6)))
				m_MidiCfg.ClearZxxMacros();

			if (GetType_() == ModType.Mpt)
			{
				// START - mpt specific:
				if ((fileHeader.Cwtv > 0x0889) && file.Seek(mptStartPos))
					LoadMptmProperties(file, fileHeader.Cwtv);
			}

			m_ModFormat.FormatName = GetType_() == ModType.Mpt ? "OpenMPT MPTM" : string.Format("Impulse Tracker {0}.{1:x2}", fileHeader.Cmwt >> 8, fileHeader.Cmwt & 0xff);
			m_ModFormat.Type = GetType_() == ModType.Mpt ? "mptm" : "it";
			m_ModFormat.MadeWithTracker = Utility.move(madeWithTracker);
			m_ModFormat.CharSet = FindCharSet();
			m_ModFormat.Timezone = LogicalTimezone.Unspecified;

			m_ModFormat.ExtraInformation = m_ModFormat.MadeWithTracker;

			return true;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void ReadTuningCollection(ReaderStream iStrm, CTuningCollection tc, size_t dummy, Encoding defaultCharset)
		{
			string name = null;
			tc.Deserialize(iStrm, ref name, defaultCharset);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static bool ReadTuningMapTemplate<TTunNumType, TStrSizeType>(ReaderStream iStrm, map<uint16, string> shortToTNameMap, Encoding charset, size_t maxNum = 500) where TTunNumType : IUnsignedNumber<TTunNumType>, IComparisonOperators<TTunNumType, TTunNumType, bool> where TStrSizeType : IUnsignedNumber<TStrSizeType>, IBinaryInteger<TStrSizeType>
		{
			Io_.ReadIntLE(iStrm, out TTunNumType numTuning);

			if (numTuning > TTunNumType.CreateTruncating(maxNum))
				return true;

			for (size_t i = 0; i < size_t.CreateTruncating(numTuning); i++)
			{
				if (!Io_.ReadSizedStringLE(iStrm, out StdString temp, TStrSizeType.CreateTruncating(255)))
					return true;

				Io_.ReadIntLE(iStrm, out uint16 ui);
				shortToTNameMap[ui] = charset.GetString(temp.Span());
			}

			return iStrm.EndOfStream;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void ReadTuningMapImpl(ReaderStream iStrm, CSoundFile csf, Encoding charset, size_t _ = 0, bool old = false)
		{
			map<uint16, string> shortToTNameMap = new map<uint16, string>();

			if (old)
				ReadTuningMapTemplate<uint32, uint32>(iStrm, shortToTNameMap, charset);
			else
				ReadTuningMapTemplate<uint16, uint8>(iStrm, shortToTNameMap, charset);

			// Read & set tunings for instruments
			vector<string> notFoundTunings = new vector<string>();

			for (InstrumentIndex i = 1; i <= csf.GetNumInstruments(); i++)
			{
				Io_.ReadIntLE(iStrm, out uint16 ui);
				var iter = shortToTNameMap.find(ui);

				if ((csf.Instruments[i] != null) && (iter != shortToTNameMap.end()))
				{
					string str = iter.second;

					if (str == "->MPT_ORIGINAL_IT<-")
					{
						csf.Instruments[i].pTuning = null;
						continue;
					}

					csf.Instruments[i].pTuning = csf.GetTuneSpecificTunings().GetTuning(str);

					if (csf.Instruments[i].pTuning != null)
						continue;

					if ((str == "12TET [[fs15 1.17.02.49]]") || (str == "12TET"))
					{
						CTuning pNewTuning = csf.CreateTuning12Tet(str);
						CTuning pT = csf.GetTuneSpecificTunings().AddTuning(Utility.move(pNewTuning));

						if (pT != null)
							continue;
					}

					// Checking if not found tuning already noticed
					if (!Algorithm.Contains(notFoundTunings, str))
						notFoundTunings.push_back(str);

					csf.Instruments[i].pTuning = null;
				}
				else
				{
					// This 'else' happens probably only in case of corrupted file
					if (csf.Instruments[i] != null)
						csf.Instruments[i].pTuning = null;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void ReadTuningMap(ReaderStream iStrm, CSoundFile csf, size_t dummy, Encoding charset)
		{
			ReadTuningMapImpl(iStrm, csf, charset, dummy, false);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private size_t ItInstrToMpt(FileReader file, ModInstrument ins, uint16 trkVers)
		{
			if (trkVers < 0x0200)
			{
				// Load old format (IT 1.xx) instrument (early IT 2.xx modules may have cmwt set to 1.00 for backwards compatibility)
				ItOldInstrument instrumentHeader = new ItOldInstrument();

				if (!file.ReadStruct(ref instrumentHeader))
					return 0;
				else
				{
					instrumentHeader.ConvertToMpt(ins);

					return (size_t)Marshal.SizeOf<ItOldInstrument>();
				}
			}
			else
			{
				size_t offset = file.GetPosition();

				// Try loading extended instrument... instSize will differ between normal and extended instruments
				ItInstrumentEx instrumentHeader = new ItInstrumentEx();

				file.ReadStructPartial(ref instrumentHeader);
				size_t instSize = instrumentHeader.ConvertToMpt(ins, GetType_());
				file.Seek(offset + instSize);

				// Try reading modular instrument data.
				// Yes, it is completely idiotic that we have both this and LoadExtendedInstrumentProperties.
				// This is only required for files saved with *really* old OpenMPT versions (pre-1.17-RC1).
				// This chunk was also written in later versions (probably to maintain compatibility with
				// those ancient versions), but this also means that redundant information is stored in the file.
				// Starting from OpenMPT 1.25.02.07, this chunk is no longer written
				if (file.ReadMagic("MSNI"))
				{
					// ...the next piece of data must be the total size of the modular data
					FileReader modularData = new FileReader(file.ReadChunk(file.ReadUInt32LE()));
					instSize += 8 + modularData.GetLength();

					if (modularData.ReadMagic("GULP"))
					{
						ins.nMixPlug = modularData.ReadUInt8();

						if (ins.nMixPlug > Snd_Def.Max_MixPlugins)
							ins.nMixPlug = 0;
					}
				}

				return instSize;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void CopyPatternName(CPattern pattern, FileReader file)
		{
			uint8[] name = new uint8[Snd_Def.Max_PatternName];

			file.ReadString(ReadWriteMode.MaybeNullTerminated, name, Snd_Def.Max_PatternName);
			pattern.SetName(name);
		}



		/********************************************************************/
		/// <summary>
		/// Get version of Impulse Tracker that was used to create an IT/S3M
		/// file
		/// </summary>
		/********************************************************************/
		private string GetImpulseTrackerVersion(uint16 cwtv, uint16 cmwt)
		{
			string version;

			cwtv &= 0xfff;

			if (cmwt > 0x0214)
				version = "Impulse Tracker 2.15";
			else if ((cwtv >= 0x0215) && (cwtv <= 0x0217))
			{
				string[] versions = [ "1-2", "3", "4-5" ];

				version = string.Format("Impulse Tracker 2.14p{0}", versions[cwtv - 0x0215]);
			}
			else
				version = string.Format("Impulse Tracker {0}.{1:x2}", (cwtv & 0x0f00) >> 8, cwtv & 0xff);

			return version;
		}



		/********************************************************************/
		/// <summary>
		/// Get version of Schism Tracker that was used to create an IT/S3M
		/// file
		/// </summary>
		/********************************************************************/
		private string GetSchismTrackerVersion(uint16 cwtv, uint32 reserved)
		{
			// Schism Tracker version information in a nutshell:
			// < 0x020: a proper version (files saved by such versions are likely very rare)
			// = 0x020: any version between the 0.2a release (2005-04-29?) and 2007-04-17
			// = 0x050: anywhere from 2007-04-17 to 2009-10-31
			// > 0x050: the number of days since 2009-10-31
			// = 0xFFF: any version starting from 2020-10-28 (exact version stored in reserved value)
			cwtv &= 0xfff;

			if (cwtv > 0x50)
			{
				int32 date = (int32)(ItTools.SchismTrackerEpoch + (cwtv < 0xfff ? cwtv - 0x050 : reserved));
				int32 y = (int32)((Util.Mul32To64(10000, date) + 14780) / 3652425);
				int32 ddd = date - ((365 * y) + (y / 4) - (y / 100) + (y / 400));

				if (ddd < 0)
				{
					y--;
					ddd = date - ((365 * y) + (y / 4) - (y / 100) + (y / 400));
				}

				int32 mi = ((100 * ddd) + 52) / 3060;

				return string.Format("Schism Tracker {0:D4}-{1:D2}-{2:D2}", y + ((mi + 2) / 12), ((mi + 2) % 12) + 1, ddd - (((mi * 306) + 5) / 10) + 1);
			}
			else
				return string.Format("Schism Tracker 0.{0:x2}", cwtv);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static bool ValidateHeader(ref ItFileHeader fileHeader)
		{
			if (((CMemory.memcmp(fileHeader.Id.ToArray().begin(), "IMPM", 4) != 0) && (CMemory.memcmp(fileHeader.Id.ToArray().begin(), "tpm.", 4) != 0)) ||
				(fileHeader.InsNum > 0xff) || (fileHeader.SmpNum >= Snd_Def.Max_Samples))
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
		private static uint64 GetHeaderMinimumAdditionalSize(ref ItFileHeader fileHeader)
		{
			return (uint64)(fileHeader.OrdNum + ((fileHeader.InsNum + fileHeader.SmpNum + fileHeader.PatNum) * 4));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void LoadMptmProperties(FileReader file, uint16 cwtv)
		{
			using (ReaderStream iStrm = new ReaderStream(new MemoryStream(file.GetRawDataAsByteVector().data().ToArray())))
			{
				if (cwtv >= 0x88d)
				{
					SsbRead ssb = new SsbRead(iStrm);
					ssb.BeginRead("mptm", Version.Current().GetRawVersion());

					int8 useUtf8Tuning = 0;
					ssb.ReadItem(ref useUtf8Tuning, "UTF8Tuning");

					Encoding tuningCharset = useUtf8Tuning != 0 ? Encoding.UTF8 : GetCharsetInternal();

					CTuningCollection tuningCollection = GetTuneSpecificTunings();
					ssb.ReadItem(ref tuningCollection, "0", (strm, ref tc, dummy) => ReadTuningCollection(strm, tc, dummy, tuningCharset));

					CSoundFile soundFile = this;
					ssb.ReadItem(ref soundFile, "1", (strm, ref csf, dummy) => ReadTuningMap(strm, csf, dummy, tuningCharset));

					ssb.ReadItem(ref Order, "2", ModSequence.ReadModSequenceOld);
					ssb.ReadItem(ref Patterns, CPatternContainer.FileIdPatterns, CPatternContainer.ReadModPatterns);

					Encoding sequenceDefaultCharset = GetCharsetInternal();
					ssb.ReadItem(ref Order, ModSequence.FileIdSequences, (strm, ref seq, nSize) => ModSequence.ReadModSequences(strm, seq, nSize, sequenceDefaultCharset));

					if (ssb.HasFailed())
					{
						;
					}
				}
				else
				{
					// Loading for older files
					string name = null;

					if (GetTuneSpecificTunings().Deserialize(iStrm, ref name, GetCharsetInternal()) != SerializationResult.Success)
					{
						;
					}
					else
						ReadTuningMapImpl(iStrm, this, GetCharsetInternal(), 0, cwtv < 0x88c);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private (bool hasPluginChunks, bool isBeRoTracker) LoadMixPlugins(FileReader file, bool ignoreChannelCount = true)
		{
			bool hasPluginChunks = false, isBeRoTracker = false;

			while (file.CanRead(9))
			{
				uint8[] code = new uint8[4];

				file.ReadArray(code);
				uint32 chunkSize = file.ReadUInt32LE();

				if ((CMemory.memcmp(code, "IMPI", 4) == 0) ||		// IT instrument, we definitely read too far
					(CMemory.memcmp(code, "IMPS", 4) == 0) ||		// IT sample, ditto
					(CMemory.memcmp(code, "XTPM", 4) == 0) ||		// Instrument extensions, ditto
					(CMemory.memcmp(code, "STPM", 4) == 0) ||		// Song extensions, ditto
					!file.CanRead(chunkSize))
				{
					file.SkipBack(8);

					return (hasPluginChunks, isBeRoTracker);
				}

				FileReader chunk = new FileReader(file.ReadChunk(chunkSize));

				// Channel FX
				if (CMemory.memcmp(code, "CHFX", 4) == 0)
				{
					if (!ignoreChannelCount)
						ChnSettings.resize(Math.Clamp((ChannelIndex)(chunkSize / 4), GetNumChannels(), Snd_Def.Max_BaseChannels));

					foreach (ModChannelSettings chn in ChnSettings)
						chn.nMixPlugin = (PlugIndex)chunk.ReadUInt32LE();

					hasPluginChunks = true;
				}
				else if ((code[0] == 'F') && ((code[1] == 'X') || char.IsAsciiDigit((char)code[1])) && char.IsAsciiDigit((char)code[2]) && char.IsAsciiDigit((char)code[3]))
				{
					uint16 fxPlug = (uint16)(((code[2] - '0') * 10) + (code[3] - '0'));	// Calculate plug-in number

					if (code[1] != 'X')
						fxPlug += (uint16)((code[1] - '0') * 100);

					if (fxPlug < Snd_Def.Max_MixPlugins)
					{
						PlugIndex plug = (PlugIndex)fxPlug;
						ReadMixPluginChunk(chunk/*, m_MixPlugins[plug]*/);//XX
					}

					hasPluginChunks = true;
				}
				else if (CMemory.memcmp(code, "MODU", 4) == 0)
				{
					isBeRoTracker = true;
					m_dwLastSavedWithVersion = new Version();	// Reset MPT detection for old files that have a similar fingerprint
				}
			}

			return (hasPluginChunks, isBeRoTracker);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void ReadMixPluginChunk(FileReader file)
		{
			//XX Not done yet. Just collect the plug-in names
			file.Skip(64);

			uint8[] buffer = new uint8[64];
			file.ReadArray(buffer);

			string pluginName = EncoderCollection.Win1252.GetString(buffer);
			unsupportedPlugins.Add(pluginName);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void ReadField<T>(FileReader chunk, size_t size, out T field)
		{
			field = chunk.ReadSizedIntLE<T>(size);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void ReadFieldCast<T>(FileReader chunk, size_t size, out T field) where T : Enum
		{
			field = (T)Enum.ToObject(typeof(T), chunk.ReadSizedIntLE<int32>(size));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool LoadExtendedSongProperties(FileReader file, bool ignoreChannelCount, ref bool pInterpretMptMade)
		{
			if (!file.ReadMagic("STPM"))	// 'MPTS'
				return false;

			// Found MPTS, interpret the file MPT made
			pInterpretMptMade = true;

			// HACK: Reset mod flags to default values here, as they are not always written
			m_PlayBehaviour.reset();

			while (file.CanRead(7))
			{
				uint32 code = file.ReadUInt32LE();
				uint16 size = file.ReadUInt16LE();

				// Start of MPTM extensions, non-ASCII ID or truncated field
				if (code == Magic.MagicLE("228\x04"))
				{
					file.SkipBack(6);
					break;
				}
				else if (((code & 0x80808080) != 0) || ((code & 0x60606060) == 0) || !file.CanRead(size))
					break;

				FileReader chunk = new FileReader(file.ReadChunk(size));

				switch (Magic.MagicToStringBE(code))	// Interpret field code
				{
					case "DT..":
					{
						ReadField(chunk, size, out uint32 tempo);
						Order.Current.SetDefaultTempo(new Tempo(tempo, Order.Current.GetDefaultTempo().GetFract()));
						break;
					}

					case "RPB.":
					{
						ReadField(chunk, size, out m_nDefaultRowsPerBeat);
						break;
					}

					case "RPM.":
					{
						ReadField(chunk, size, out m_nDefaultRowsPerMeasure);
						break;
					}

					case "C...":
					{
						if (!ignoreChannelCount)
						{
							ReadField(chunk, size, out ChannelIndex chn);
							ChnSettings.resize(OpenMpt.Clamp(chn, GetNumChannels(), Snd_Def.Max_BaseChannels));
						}

						break;
					}

					case "TM..":
					{
						ReadFieldCast(chunk, size, out m_nTempoMode);
						break;
					}

					case "PMM.":
					{
						ReadFieldCast(chunk, size, out m_nMixLevels);
						break;
					}

					case "CWV.":
					{
						ReadField(chunk, size, out uint32 ver);
						m_dwCreatedWithVersion = new Version(ver);
						break;
					}

					case "LSWV":
					{
						ReadField(chunk, size, out uint32 ver);

						if (ver != 0)
							m_dwLastSavedWithVersion = new Version(ver);

						break;
					}

					case "SPA.":
					{
						ReadField(chunk, size, out m_nSamplePreAmp);
						break;
					}

					case "VSTV":
					{
						ReadField(chunk, size, out m_nVstiVolume);
						break;
					}

					case "DGV.":
					{
						ReadField(chunk, size, out m_nDefaultGlobalVolume);
						break;
					}

					case "RP..":
					{
						if (GetType_() != ModType.Xm)
						{
							ReadField(chunk, size, out OrderIndex restartPos);
							Order.Current.SetRestartPos(restartPos);
						}

						break;
					}

					case "ChnS":
					{
						// Channel settings for channels 65+
						if ((size <= ((Snd_Def.Max_BaseChannels - 64) * 2)) && ((size % 2) == 0) && ((GetType_() & (ModType.It | ModType.Mpt)) != 0))
						{
							ChannelIndex channelsInFile = ChannelIndex.CreateSaturating(64 + (size / 2));

							if (!ignoreChannelCount)
								ChnSettings.resize(Math.Clamp(GetNumChannels(), channelsInFile, Snd_Def.Max_BaseChannels));

							ChannelIndex numChannels = Math.Min(channelsInFile, GetNumChannels());

							for (ChannelIndex chn = 64; chn < numChannels; chn++)
							{
								uint8 pan = chunk.ReadUInt8();
								uint8 vol = chunk.ReadUInt8();

								if (pan != 0xff)
								{
									ChnSettings[chn].nVolume = vol;
									ChnSettings[chn].nPan = 128;
									ChnSettings[chn].dwFlags.Reset();

									if ((pan & 0x80) != 0)
										ChnSettings[chn].dwFlags.Set(ChannelFlags.Chn_Mute);

									pan &= 0x7f;

									if (pan <= 64)
										ChnSettings[chn].nPan = (uint16)(pan << 2);

									if (pan == 100)
										ChnSettings[chn].dwFlags.Set(ChannelFlags.Chn_Surround);
								}
							}
						}

						break;
					}

					case "MSF.":
					{
						// Playback compatibility flags
						size_t bit = 0;
						m_PlayBehaviour.reset();

						while (chunk.CanRead(1) && (bit < m_PlayBehaviour.size()))
						{
							uint8 b = chunk.ReadUInt8();

							for (uint8 i = 0; i < 8; i++, bit++)
							{
								if (((b & (1 << i)) != 0) && (bit < m_PlayBehaviour.size()))
									m_PlayBehaviour.set((PlayBehaviour)bit);
							}
						}

						break;
					}
				}

				switch (Magic.MagicToStringLE(code))	// Interpret field code
				{
					case "DTFR":
					{
						ReadField(chunk, size, out uint32 tempoFract);
						Order.Current.SetDefaultTempo(new Tempo(Order.Current.GetDefaultTempo().GetInt(), tempoFract));
						break;
					}

					case "RSMP":
					{
						ReadFieldCast(chunk, size, out m_nResampling);

						if (!Resampling.IsKnownMode(m_nResampling))
							m_nResampling = ResamplingMode.Default;

						break;
					}

					case "AUTH":
					{
						StdString artist = new StdString();

						chunk.ReadString(ReadWriteMode.SpacePadded, artist, chunk.GetLength());
						m_SongArtist = Encoding.UTF8.GetString(artist.Span());
						break;
					}

					case "CUES":
					{
						// Sample cues
						if (size > 2)
						{
							SampleIndex smp = chunk.ReadUInt16LE();

							if ((smp > 0) && (smp <= GetNumSamples()))
							{
								ModSample sample = Samples[smp];

								for (size_t i = 0; i < sample.Cues.size(); i++)
								{
									SmpLength cue;

									if (chunk.CanRead(4))
										cue = chunk.ReadUInt32LE();
									else
										cue = Snd_Def.Max_Sample_Length;

									sample.Cues[i] = cue;
								}
							}
						}

						break;
					}

					case "SWNG":
					{
						// Tempo Swing Factors
						if (size > 2)
						{
							using (MemoryStream ms = new MemoryStream(chunk.ReadRawDataAsByteVector().data().ToArray()))
							{
								TempoSwing.Deserialize(ms, ref m_TempoSwing, chunk.GetLength());
							}
						}

						break;
					}
				}
			}

			// Validate read values
			Order.Current.SetDefaultTempo(OpenMpt.Clamp(Order.Current.GetDefaultTempo(), GetModSpecifications().GetTempoMin(), GetModSpecifications().GetTempoMax()));

			if (m_nTempoMode >= TempoMode.NumModes)
				m_nTempoMode = TempoMode.Classic;

			if (m_nMixLevels >= MixLevels.NumMixLevels)
				m_nMixLevels = MixLevels.Original;

			return true;
		}

		#region Extended test
		/********************************************************************/
		/// <summary>
		/// TNE: Added extra checks so only OpenMPT modules are recognized.
		/// The tracker detection of the original loader is ported, but only
		/// the parts telling about ModPlug Tracker / OpenMPT. The file is
		/// walked through once and the probe returns as soon as the verdict
		/// is settled
		/// </summary>
		/********************************************************************/
		private static ProbeResult ExtendedProbeIt(FileReader file, ref ItFileHeader fileHeader, out InternalItFormat probedFormat)
		{
			const size_t ItSampleHeaderSize = 80;
			const size_t ItHistorySize = 8;

			const uint8 SampleDataPresent = 0x01;
			const uint8 Sample16Bit = 0x02;
			const uint8 SampleStereo = 0x04;
			const uint8 SampleCompressed = 0x08;

			const uint8 CvtOplInstrument = 0x40;
			const uint8 CvtExternalSample = 0x80;
			const uint8 CvtAdpcmSample = 0xff;

			probedFormat = InternalItFormat.It;

			// Legacy MPTM files (old 1.17.02.4x releases) have their own
			// signature, while newer ones are marked by the "228" block
			// which the last 4 bytes of the file point at
			if (CMemory.memcmp(fileHeader.Id.ToArray().begin(), "tpm.", 4) == 0)
				probedFormat = InternalItFormat.Mptm;
			else if ((fileHeader.Cwtv > 0x888) && (fileHeader.Cwtv <= 0xfff) && (file.GetLength() >= 4))
			{
				file.Seek(file.GetLength() - 4);
				uint32 mptStartPos = file.ReadUInt32LE();

				if ((mptStartPos >= 0x100) && (mptStartPos < file.GetLength()) && file.Seek(mptStartPos) && file.ReadMagic("228"))
					probedFormat = InternalItFormat.Mptm;
			}

			// MPTM is OpenMPT's own format
			if (probedFormat == InternalItFormat.Mptm)
				return ProbeResult.Success;

			// OpenMPT 1.17 and later, both normal files and compatibility exports
			if ((fileHeader.Cwtv & 0xf000) == 0x5000)
				return ProbeResult.Success;

			// OpenMPT 1.17.02.26 (r122) to 1.18
			if ((fileHeader.Cmwt == 0x888) || (fileHeader.Cwtv == 0x888))
				return ProbeResult.Success;

			// ModPlug Tracker b3.2 - 1.09, instruments 557 bytes apart
			if ((fileHeader.Cwtv == 0x0214) && (fileHeader.Cmwt == 0x0202) && (fileHeader.Reserved == 0))
				return ProbeResult.Success;

			// A rare variant used from OpenMPT 1.17.02.20 (r113) to 1.17.02.25 (r121)
			if ((fileHeader.Cwtv == 0x0300) && (fileHeader.Cmwt == 0x0300) && (fileHeader.Reserved == 0) && (fileHeader.OrdNum == 256) && (fileHeader.Sep == 128) && (fileHeader.Pwd == 0))
				return ProbeResult.Success;

			// The extended filter range is only ever written by ModPlug Tracker / OpenMPT
			if ((fileHeader.Flags & ItFileHeader.ExtendedFilterRange) != 0)
				return ProbeResult.Success;

			// Skip the order list and read the instrument, sample and pattern offsets
			file.Seek(192U + fileHeader.OrdNum);

			vector<uint32le> insPos = new vector<uint32le>();
			vector<uint32le> smpPos = new vector<uint32le>();
			vector<uint32le> patPos = new vector<uint32le>();

			if (!file.ReadVector(insPos, fileHeader.InsNum) || !file.ReadVector(smpPos, fileHeader.SmpNum) || !file.ReadVector(patPos, fileHeader.PatNum))
				return ProbeResult.Failure;

			// Find the first parapointer. The edit history and the
			// extensions in front of the instruments may not pass it
			uint32 minPtr = uint32.MaxValue;
			minPtr = GetMinimumPointer(insPos, minPtr);
			minPtr = GetMinimumPointer(smpPos, minPtr);
			minPtr = GetMinimumPointer(patPos, minPtr);

			if ((fileHeader.Special & ItFileHeader.EmbedSongMessage) != 0)
				minPtr = Math.Min(minPtr, fileHeader.MsgOffset);

			// The next steps only move the file position the same way as
			// the original loader, so the extensions are found at the same place
			bool possiblyUnmo3 = (fileHeader.Cmwt == 0x0214) && ((fileHeader.Cwtv == 0x0214) || (fileHeader.Cwtv == 0)) &&
			                     (fileHeader.Highlight_Major == 0) && (fileHeader.Highlight_Minor == 0) &&
			                     (fileHeader.Pwd == 0) && (fileHeader.Reserved == 0) &&
			                     ((fileHeader.Flags & (ItFileHeader.UseMidiPitchController | ItFileHeader.ReqEmbeddedMidiConfig)) == 0);

			if (possiblyUnmo3 && (fileHeader.InsNum == 0) && (fileHeader.SmpNum > 0) && ((file.GetPosition() + (4 * smpPos.size()) + 2) <= minPtr))
			{
				// UNMO3 < v2.4.0.1 reserves some space for instrument
				// parapointers even in sample mode
				for (c_int i = 0; i < fileHeader.SmpNum; i++)
				{
					if (file.ReadUInt32LE() != 0)
					{
						file.SkipBack(4 + ((size_t)i * 4));
						break;
					}
				}
			}

			if ((fileHeader.Special & ItFileHeader.EmbedEditHistory) != 0)
			{
				// Some trackers set the flag without writing the history,
				// so it is only taken if it ends before the first parapointer
				uint16 nflt = file.ReadUInt16LE();
				size_t historySize = nflt * ItHistorySize;

				if (file.CanRead(historySize) && ((file.GetPosition() + historySize) <= minPtr))
					file.Skip(historySize);
				else
					file.SkipBack(2);
			}
			else if (possiblyUnmo3 && (fileHeader.Special <= 1))
			{
				// UNMO3 < v2.4.0.1 always writes the edit history length
				if (file.ReadUInt16LE() != 0)
					file.SkipBack(2);
			}

			if ((((fileHeader.Flags & ItFileHeader.ReqEmbeddedMidiConfig) != 0) || ((fileHeader.Special & ItFileHeader.EmbedMidiConfiguration) != 0)) && file.CanRead(MidiMacroConfigData.StructSize))
				file.Skip(MidiMacroConfigData.StructSize);

			// Pattern and channel names are only ever written by ModPlug Tracker / OpenMPT
			if (file.ReadMagic("PNAM") || file.ReadMagic("CNAM"))
				return ProbeResult.Success;

			// Mix plugins. LibXmp has no support for those at all, so it
			// does not matter which tracker wrote them
			FileReader pluginChunk = new FileReader(file.ReadChunk(minPtr >= file.GetPosition() ? minPtr - file.GetPosition() : file.BytesLeft()));

			if (SkipMixPlugins(pluginChunk, out bool isBeRoTracker))
				return ProbeResult.Success;

			// ModPlug Tracker 1.09 - 1.16 and OpenMPT 1.17 in compatible mode both identify as this
			if ((fileHeader.Cwtv == 0x0217) && (fileHeader.Cmwt == 0x0200) && (fileHeader.Reserved == 0) && !isBeRoTracker)
				return ProbeResult.Success;

			if (!isBeRoTracker && (fileHeader.Highlight_Major == 0) && (fileHeader.Highlight_Minor == 0) && (fileHeader.Reserved == 0))
			{
				// ModPlug Tracker 1.0 pre-alpha up to alpha 4, patterns
				// located before instruments / samples
				if ((fileHeader.Cwtv == 0x0202) && (fileHeader.Cmwt == 0x0200) && (patPos.size() > 0) && (smpPos.size() > 0) && (patPos[0] != 0) && (patPos[0] < smpPos[0]))
					return ProbeResult.Success;

				// ModPlug Tracker 1.0a5 - 1.0b2, but not an OpenSPC
				// conversion which looks almost the same
				bool isOpenSpcConversion = (fileHeader.Flags == 9) && (fileHeader.Special == 0) &&
				                           (fileHeader.InsNum == 0) && ((fileHeader.PatNum + 1) == fileHeader.OrdNum) &&
				                           (fileHeader.GlobalVol == 128) && (fileHeader.Mv == 100) && (fileHeader.Speed == 1) && (fileHeader.Sep == 128) && (fileHeader.Pwd == 0) &&
				                           (fileHeader.MsgLength == 0) && (fileHeader.MsgOffset == 0);

				if ((fileHeader.Cwtv == 0x0214) && (fileHeader.Cmwt == 0x0200) && !isOpenSpcConversion)
					return ProbeResult.Success;
			}

			// Walk through the samples to find the end of the sample data,
			// where the extended instrument and song properties are stored
			bool muteBuggySamples = (fileHeader.Cwtv >= 0x0100) && (fileHeader.Cwtv <= 0x0217) && ((fileHeader.Cwtv < 0x0207) || (fileHeader.Reserved != 0));

			size_t lastSampleOffset = 0;
			bool lastSampleCompressed = false;

			if (fileHeader.SmpNum > 0)
				lastSampleOffset = smpPos[fileHeader.SmpNum - 1] + ItSampleHeaderSize;

			for (SampleIndex smp = 0; smp < fileHeader.SmpNum; smp++)
			{
				if ((smpPos[smp] == 0) || !file.Seek(smpPos[smp]) || !file.CanRead(ItSampleHeaderSize))
					continue;

				size_t headerPos = file.GetPosition();

				file.Seek(headerPos + 18);
				uint8 flags = file.ReadUInt8();

				file.Seek(headerPos + 46);
				uint8 cvt = file.ReadUInt8();
				file.Skip(1);									// Default pan
				uint32 length = file.ReadUInt32LE();

				file.Seek(headerPos + 72);
				uint32 samplePointer = file.ReadUInt32LE();

				// OPL instruments and external samples are OpenMPT features
				if ((cvt == CvtOplInstrument) || (cvt == CvtExternalSample))
					return ProbeResult.Success;

				// Only ModPlug ever packed samples with its own ADPCM compression
				if (((flags & SampleCompressed) == 0) && ((flags & Sample16Bit) == 0) && (cvt == CvtAdpcmSample))
					return ProbeResult.Success;

				if (muteBuggySamples && ((flags & SampleDataPresent) == 0))
					length = 0;

				if (!file.Seek(samplePointer))
					continue;

				// The end of a compressed sample is not known without
				// unpacking it, so it is searched for later on
				lastSampleCompressed = (flags & SampleCompressed) != 0;

				if (!lastSampleCompressed)
				{
					size_t sampleBytes = length;

					if ((flags & Sample16Bit) != 0)
						sampleBytes *= 2;

					// Some old versions of Impulse Tracker did not clear the stereo flag
					if (((flags & SampleStereo) != 0) && (fileHeader.Cwtv >= 0x214))
						sampleBytes *= 2;

					file.Skip(sampleBytes);
				}

				lastSampleOffset = Math.Max(lastSampleOffset, file.GetPosition());
			}

			// Patterns may be stored after the sample data
			PatternIndex numPatterns = Math.Min((PatternIndex)fileHeader.PatNum, ModSpecs.ItEx.PatternsMax);

			for (PatternIndex pat = 0; pat < numPatterns; pat++)
			{
				if ((patPos[pat] == 0) || !file.Seek(patPos[pat]))
					continue;

				uint16 len = file.ReadUInt16LE();
				RowIndex numRows = file.ReadUInt16LE();

				if ((numRows < 1) || (numRows > Snd_Def.Max_Pattern_Rows) || !file.Skip(4))
					continue;

				file.Skip(len);

				lastSampleOffset = Math.Max(lastSampleOffset, file.GetPosition());
			}

			if (lastSampleOffset > 0)
			{
				file.Seek(lastSampleOffset);

				if (lastSampleCompressed)
				{
					// Emulate the original loader, which jumps from block
					// to block until some instrument / song properties or
					// the end of the file is reached
					while (file.CanRead(4))
					{
						if (file.ReadMagic("XTPM") || file.ReadMagic("STPM"))
						{
							uint32 id = file.ReadUInt32LE();
							file.SkipBack(8);

							// Our chunk IDs should only contain ASCII characters
							if (((id & 0x80808080) == 0) && ((id & 0x60606060) != 0))
								break;
						}

						file.Skip(file.ReadUInt16LE());
					}
				}
			}

			// Extended instrument and song properties are only written by
			// OpenMPT 1.17 and later, so finding either of them is enough
			if (file.ReadMagic("XTPM") || file.ReadMagic("STPM"))
				return ProbeResult.Success;

			return ProbeResult.Failure;
		}



		/********************************************************************/
		/// <summary>
		/// Return the lowest non-zero offset in the given list or the
		/// current minimum if it is lower
		/// </summary>
		/********************************************************************/
		private static uint32 GetMinimumPointer(vector<uint32le> positions, uint32 minPtr)
		{
			for (size_t i = 0; i < positions.size(); i++)
			{
				uint32 pos = positions[i];

				if ((pos > 0) && (pos < minPtr))
					minPtr = pos;
			}

			return minPtr;
		}



		/********************************************************************/
		/// <summary>
		/// Skip over all the mix plugin chunks without parsing them. Tells
		/// whether any real mix plugin chunk was found and whether a
		/// BeRoTracker chunk was seen. Used by the probe methods
		/// </summary>
		/********************************************************************/
		private static bool SkipMixPlugins(FileReader file, out bool isBeRoTracker)
		{
			bool hasPluginChunks = false;
			isBeRoTracker = false;

			while (file.CanRead(9))
			{
				CPointer<uint8> code = file.ReadArray<uint8>(4).begin();
				uint32 chunkSize = file.ReadUInt32LE();

				if ((CMemory.memcmp(code, "IMPI", 4) == 0) ||	// IT instrument, we definitely read too far
				    (CMemory.memcmp(code, "IMPS", 4) == 0) ||	// IT sample, ditto
				    (CMemory.memcmp(code, "XTPM", 4) == 0) ||	// Instrument extensions, ditto
				    (CMemory.memcmp(code, "STPM", 4) == 0) ||	// Song extensions, ditto
				    !file.CanRead(chunkSize))
				{
					file.SkipBack(8);
					break;
				}

				if ((CMemory.memcmp(code, "CHFX", 4) == 0) || IsPluginChunk(code))
					hasPluginChunks = true;
				else if (CMemory.memcmp(code, "MODU", 4) == 0)
					isBeRoTracker = true;

				file.Skip(chunkSize);
			}

			return hasPluginChunks;
		}



		/********************************************************************/
		/// <summary>
		/// Tell if the given chunk identifier is one of the FXnn chunks
		/// holding the settings of a single mix plugin
		/// </summary>
		/********************************************************************/
		private static bool IsPluginChunk(CPointer<uint8> code)
		{
			return (code[0] == 'F') && (code[1] == 'X') && (code[2] >= '0') && (code[2] <= '9') && (code[3] >= '0') && (code[3] <= '9');
		}
		#endregion

		#endregion
	}
}
