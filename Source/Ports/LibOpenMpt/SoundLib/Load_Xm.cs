/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.String;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Exceptions;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;
using Algorithm = Polycode.NostalgicPlayer.Kit.C.Std.Algorithm;
using Utility = Polycode.NostalgicPlayer.Kit.C.Std.Utility;
using Version = Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.Version;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// XM (FastTracker II) module loader
	/// </summary>
	internal partial class CSoundFile
	{
		[Flags]
		private enum TrackerVersions
		{
			/// <summary>
			/// Probably not made with MPT
			/// </summary>
			Unknown = 0x00,

			/// <summary>
			/// Made with MPT alpha / beta
			/// </summary>
			OldModPlug = 0x01,

			/// <summary>
			/// Made with MPT (not alpha / beta)
			/// </summary>
			NewModPlug = 0x02,

			/// <summary>
			/// MPT up to v1.11 sets both the normal loop and the ping pong loop flag
			/// </summary>
			ModPlugBidiFlag = 0x04,

			/// <summary>
			/// Made with OpenMPT
			/// </summary>
			OpenMpt = 0x08,

			/// <summary>
			/// We are very sure that we found the correct tracker version
			/// </summary>
			Confirmed = 0x10,

			/// <summary>
			/// "FastTracker v2.00", but FastTracker has not been ruled out
			/// </summary>
			Ft2Generic = 0x20,

			/// <summary>
			/// Not FastTracker 2: The instrument type changed between two
			/// instruments, or a null character was found in the song title
			/// </summary>
			Ft2Clone = 0x80,

			/// <summary>
			/// Could be PlayerPRO
			/// </summary>
			PlayerPro = 0x100,

			/// <summary>
			/// Probably DigiTrakker
			/// </summary>
			DigiTrakker = 0x200,

			/// <summary>
			/// Allow an empty order list like in OpenMPT
			/// (FT2 just plays pattern 0 if the order list is empty according to the header)
			/// </summary>
			EmptyOrders = 0x800
		}

		private const uint32 XmInstrumentHeaderSize = 263;
		private const uint32 XmSampleHeaderSize = 40;

		private const uint8 XmEnvelopeLoop = 0x04;

		private const uint8 XmSample16Bit = 0x10;
		private const uint8 XmSampleStereo = 0x20;
		private const uint8 XmSampleAdpcm = 0xad;

		private static ModCommandVolCmd[] volEffTrans =
		[
			VolumeCommand.VolSlideDown, VolumeCommand.VolSlideUp, VolumeCommand.FineVolDown, VolumeCommand.FineVolUp,
			VolumeCommand.VibratoSpeed, VolumeCommand.VibratoDepth, VolumeCommand.Panning, VolumeCommand.PanSlideLeft,
			VolumeCommand.PanSlideRight, VolumeCommand.TonePortamento
		];

		#region OpenMPT loader class
		public class XmLoader : IFormatLoader
		{
			public static readonly FileFormatLoader Format = new FileFormatLoader
			{
				Id = Guid.Parse("81C4F9B7-7B8B-4FD9-B5BB-8E445C3684F5"),
				Name = Resources.IDS_MPT_XM_NAME,
				Description = Resources.IDS_MPT_XM_DESCRIPTION,
				Prober = Probe_OpenMpt,
				Create = Create
			};

			private readonly CSoundFile sndFile;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			private XmLoader(CSoundFile soundFile)
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
				return ProbeFileHeaderXm(file, pFileSize);
			}



			/********************************************************************/
			/// <summary>
			///
			/// </summary>
			/********************************************************************/
			public static IFormatLoader Create(CSoundFile soundFile)
			{
				return new XmLoader(soundFile);
			}



			/********************************************************************/
			/// <summary>
			///
			/// </summary>
			/********************************************************************/
			public bool Read(FileReader file, ModLoadingFlags loadFlags)
			{
				return sndFile.ReadXm(file, loadFlags);
			}
		}
		#endregion

		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		private static ProbeResult ProbeFileHeaderXm(FileReader file, uint64? pFileSize)
		{
			XmFileHeader fileHeader = new XmFileHeader();

			if (!file.ReadStruct(ref fileHeader))
				return ProbeResult.WantMoreData;

			if (!ValidateHeader(ref fileHeader))
				return ProbeResult.Failure;

			ProbeResult result = ProbeAdditionalSize(file, pFileSize, GetHeaderMinimumAdditionalSize(ref fileHeader));
			if (result != ProbeResult.Success)
				return result;

			return ExtendedProbeXm(file, ref fileHeader);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public bool ReadXm(FileReader file, ModLoadingFlags loadFlags)
		{
			file.Rewind();

			XmFileHeader fileHeader = new XmFileHeader();

			if (!file.ReadStruct(ref fileHeader))
				return false;

			if (!ValidateHeader(ref fileHeader))
				return false;

			if (!file.CanRead(GetHeaderMinimumAdditionalSize(ref fileHeader)))
				return false;
			else if (loadFlags == ModLoadingFlags.OnlyVerifyHeader)
				return true;

			InitializeGlobals(ModType.Xm, fileHeader.Channels);
			m_nMixLevels = MixLevels.Compatible;

			TrackerVersions madeWith = TrackerVersions.Unknown;
			string madeWithTracker = string.Empty;
			bool isMadTracker = false;
			CPointer<uint8> trackerName = fileHeader.TrackerName.ToArray().begin();

			if ((CMemory.memcmp(trackerName, "FastTracker v2.00   ", 20) == 0) && (fileHeader.Size == 276))
			{
				StdString songName = new StdString(fileHeader.SongName.ToArray().begin(), (size_t)Marshal.SizeOf(fileHeader.SongName));

				if (fileHeader.Version < 0x0104)
					madeWith = TrackerVersions.Ft2Generic | TrackerVersions.Confirmed;
				else
				{
					size_t firstNull = songName.find("\0");

					if (firstNull != StdString.npos)
					{
						// FT2 pads the song title with spaces, some other trackers use null chars
						// PlayerPRO filles the remaining buffer after the null terminator with space characters.
						// PlayerPRO does not support song restart position
						if (fileHeader.RestartPos != 0)
							madeWith = TrackerVersions.Ft2Clone | TrackerVersions.NewModPlug | TrackerVersions.EmptyOrders;
						else if (firstNull == (songName.size() - 1))
							madeWith = TrackerVersions.Ft2Clone | TrackerVersions.NewModPlug | TrackerVersions.PlayerPro | TrackerVersions.EmptyOrders;
						else if (songName.find_first_not_of(" ", firstNull + 1) == StdString.npos)
							madeWith = TrackerVersions.PlayerPro | TrackerVersions.Confirmed;
						else
							madeWith = TrackerVersions.Ft2Clone | TrackerVersions.NewModPlug | TrackerVersions.EmptyOrders;
					}
					else
					{
						if (fileHeader.RestartPos != 0)
							madeWith = TrackerVersions.Ft2Generic | TrackerVersions.NewModPlug;
						else
							madeWith = TrackerVersions.Ft2Generic | TrackerVersions.NewModPlug | TrackerVersions.PlayerPro;
					}
				}
			}
			else if (CMemory.memcmp(trackerName, "FastTracker v 2.00  ", 20) == 0)
			{
				// MPT 1.0 (exact version to be determined later)
				madeWith = TrackerVersions.OldModPlug;
			}
			else
			{
				// Something else!
				madeWith = TrackerVersions.Unknown | TrackerVersions.Confirmed;

				madeWithTracker = EncoderCollection.Dos.GetString(new StdString(MptString.ReadBuf(ReadWriteMode.SpacePadded, fileHeader.TrackerName.ToArray())).Span());

				if (CMemory.memcmp(trackerName, "OpenMPT ", 8) == 0)
					madeWith = TrackerVersions.OpenMpt | TrackerVersions.Confirmed | TrackerVersions.EmptyOrders;
				else if (CMemory.memcmp(trackerName, "MilkyTracker ", 12) == 0)
				{
					// MilkyTracker prior to version 0.90.87 doesn't set a version string.
					// Luckily, starting with v0.90.87, MilkyTracker also implements the FT2 panning scheme
					if (CMemory.memcmp(trackerName + 12, "        ", 8) != 0)
						m_nMixLevels = MixLevels.CompatibleFT2;
				}
				else if (CMemory.memcmp(trackerName, "Fasttracker II clone", 20) == 0)
				{
					// 8bitbubsy's FT2 clone should be treated exactly like FT2
					madeWith = TrackerVersions.Ft2Generic | TrackerVersions.Confirmed;
				}
				else if (CMemory.memcmp(trackerName, "MadTracker 2.0\0", 15) == 0)
				{
					// Fix channel 2 in m3_cha.xm
					m_PlayBehaviour.reset(PlayBehaviour.Ft2PortaNoNote);

					// Fix arpeggios in kragle_-_happy_day.xm
					m_PlayBehaviour.reset(PlayBehaviour.Ft2Arpeggio);
					isMadTracker = true;

					if (CMemory.memcmp(trackerName + 15, "\0\0\0\0", 4) == 0)
						madeWithTracker = "MadTracker 2 (registered)";
					else
						madeWithTracker = "MadTracker 2";
				}
				else if ((CMemory.memcmp(trackerName, "Skale Tracker\0", 14) == 0) || (CMemory.memcmp(trackerName, "Sk@le Tracker\0", 14) == 0))
				{
					m_PlayBehaviour.reset(PlayBehaviour.Ft2St3OffsetOutOfRange);

					// Fix arpeggios in KAPTENFL.XM
					m_PlayBehaviour.reset(PlayBehaviour.Ft2Arpeggio);
				}
				else if ((CMemory.memcmp(trackerName, "*Converted ", 11) == 0) && (CMemory.memcmp(trackerName + 14, "-File*", 6) == 0))
				{
					madeWith = TrackerVersions.DigiTrakker | TrackerVersions.Confirmed;
					madeWithTracker = "Digitrakker";
				}
			}

			m_SongName = MptString.ReadBuf(ReadWriteMode.SpacePadded, fileHeader.SongName.ToArray());

			m_nMinPeriod = 1;
			m_nMaxPeriod = 31999;

			Order.Current.SetRestartPos(fileHeader.RestartPos);
			m_nInstruments = Math.Min((uint16)fileHeader.Instruments, (uint16)(Snd_Def.Max_Instruments - 1));

			if (fileHeader.Speed != 0)
				Order.Current.SetDefaultSpeed(fileHeader.Speed);

			if (fileHeader.Tempo != 0)
				Order.Current.SetDefaultTempo(OpenMpt.Clamp(new Tempo(fileHeader.Tempo, 0), ModSpecs.xmEx.GetTempoMin(), ModSpecs.xmEx.GetTempoMax()));

			m_SongFlags.Reset();
			m_SongFlags.Set(SongFlags.LinearSlides, (fileHeader.Flags & XmFileHeader.LinearSlides) != 0);
			m_SongFlags.Set(SongFlags.ExFilterRange, (fileHeader.Flags & XmFileHeader.ExtendedFilterRange) != 0);

			if (m_SongFlags.Test(SongFlags.ExFilterRange) && madeWith.Test(TrackerVersions.NewModPlug))
				madeWith = TrackerVersions.Ft2Clone | TrackerVersions.NewModPlug | TrackerVersions.Confirmed | TrackerVersions.EmptyOrders;

			Loaders.ReadOrderFromFile<uint8>(Order.Current, file, fileHeader.Orders);

			if ((fileHeader.Orders == 0) && !madeWith.Test(TrackerVersions.EmptyOrders))
			{
				// Fix lamb_-_dark_lighthouse.xm, which only contains one pattern and an empty order list
				Order.Current.assign(1, 0);
			}

			file.Seek(fileHeader.Size + 60);

			if (fileHeader.Version >= 0x0104)
				ReadXmPatterns(file, ref fileHeader, this);

			bool isOxm = false;

			// In case of XM versions < 1.04, we need to memorize the sample flags for all samples, as they are not stored immediately after the sample headers
			vector<SampleIO> sampleFlags = new vector<SampleIO>();
			uint8 sampleReserved = 0;
			int16 lastInstrType = -1, lastSampleReserved = -1;
			int64 lastSampleHeaderSize = -1;
			bool unsupportedSamples = false;
			bool anyAdpcm = false;
			bool instrumentWithSamplesEncountered = false;

			// Reading instruments
			for (InstrumentIndex instr = 1; instr <= m_nInstruments; instr++)
			{
				if (AllocateInstrument(instr) == null)
					return false;

				if (!file.CanRead(4))
					continue;

				// First, try to read instrument header length...
				uint32 headerSize = file.ReadUInt32LE();

				if (headerSize == 0)
					headerSize = (uint32)Marshal.SizeOf<XmInstrumentHeader>();

				// Now, read the complete struct
				file.SkipBack(4);

				XmInstrumentHeader instrHeader = new XmInstrumentHeader();
				file.ReadStructPartial(ref instrHeader, headerSize);

				// Time for some version detection stuff
				if (madeWith == TrackerVersions.OldModPlug)
				{
					madeWith.Set(TrackerVersions.Confirmed);

					if (instrHeader.Size == 245)
					{
						// ModPlug Tracker Alpha
						m_dwLastSavedWithVersion = Version.MPT_V._1_00_00_A5;
						madeWithTracker = "ModPlug Tracker 1.0 alpha";
					}
					else if (instrHeader.Size == 263)
					{
						// ModPlug Tracker Beta (Beta 1 still behaves like Alpha, but Beta 3.3 does it this way)
						m_dwLastSavedWithVersion = Version.MPT_V._1_00_00_B3;
						madeWithTracker = "ModPlug Tracker 1.0 beta";
					}
					else
					{
						// WTF?
						madeWith = TrackerVersions.Unknown | TrackerVersions.Confirmed;
					}
				}
				else if (instrHeader.NumSamples == 0)
				{
					// Empty instruments make tracker identification pretty easy
					if ((instrHeader.Size == 263) && (instrHeader.SampleHeaderSize == 0) && madeWith.Test(TrackerVersions.NewModPlug))
						madeWith.Set(TrackerVersions.Confirmed);
					else if ((instrHeader.Size != 29) && madeWith.Test(TrackerVersions.DigiTrakker))
						madeWith.Reset(TrackerVersions.DigiTrakker);
					else if (madeWith.Test(TrackerVersions.Ft2Clone | TrackerVersions.Ft2Generic) && (instrHeader.Size != 33))
					{
						// Sure isn't FT2.
						// 4-mat's eternity.xm has an empty instruments with a header size of 29.
						// Another module using that size is funky_dumbass.xm. Mysterious!
						// Note: This may happen when the XM Commenter by Aka (XMC.EXE) adds empty instruments at the end of the list,
						// which would explain the latter case, but in eternity.xm the empty slots are not at the end of the list
						madeWith = TrackerVersions.Unknown;
					}

					if (instrHeader.Size != 33)
						madeWith.Reset(TrackerVersions.PlayerPro);
					else if ((instrHeader.SampleHeaderSize > Marshal.SizeOf<XmSample>()) && madeWith.Test(TrackerVersions.PlayerPro))
					{
						// Older PlayerPRO versions appear to write garbage in the sampleHeaderSize field, and it's different for each sample.
						// Note: FT2 NORMALLY writes sampleHeaderSize=40 for all samples, but for any instruments before the first
						// instrument that has numSamples != 0, sampleHeaderSize will be uninitialized. It will always be the same
						// value, though
						if (instrumentWithSamplesEncountered || (lastSampleHeaderSize != -1) && (instrHeader.SampleHeaderSize != lastSampleHeaderSize))
							madeWith = TrackerVersions.PlayerPro | TrackerVersions.Confirmed;

						lastSampleHeaderSize = instrHeader.SampleHeaderSize;
					}
				}

				instrHeader.ConvertToMpt(Instruments[instr]);

				if (lastInstrType == -1)
					lastInstrType = instrHeader.Type;
				else if ((lastInstrType != instrHeader.Type) && madeWith.Test(TrackerVersions.Ft2Generic))
				{
					// FT2 writes some random junk for the instrument type field,
					// but it's always the SAME junk for every instrument saved.
					// Note: This may happen when running an FT2-made XM through PutInst and adding new instrument slots
					madeWith.Reset(TrackerVersions.Ft2Generic);
					madeWith.Set(TrackerVersions.Ft2Clone);
				}

				if (instrHeader.NumSamples > 0)
				{
					instrumentWithSamplesEncountered = true;

					// Yep, there are some samples associated with this instrument

					// If MIDI settings are present, this is definitely not an old MPT or PlayerPRO
					if ((instrHeader.Instrument.MidiEnabled | instrHeader.Instrument.MidiChannel | instrHeader.Instrument.MidiProgram | instrHeader.Instrument.MuteComputer) != 0)
						madeWith.Reset(TrackerVersions.OldModPlug | TrackerVersions.NewModPlug | TrackerVersions.PlayerPro);

					if ((instrHeader.Size != 263) || (instrHeader.Type != 0))
						madeWith.Reset(TrackerVersions.PlayerPro);

					if (!madeWith.Test(TrackerVersions.Confirmed) && madeWith.Test(TrackerVersions.PlayerPro))
					{
						// Note: Earlier (?) PlayerPRO versions do not seem to set the loop points to 0xFF (george_megas_-_q.xm)
						if ((((instrHeader.Instrument.VolFlags & XmInstrument.EnvLoop) == 0) && (instrHeader.Instrument.VolLoopStart == 0xff) && (instrHeader.Instrument.VolLoopEnd == 0xff)) ||
							(((instrHeader.Instrument.PanFlags & XmInstrument.EnvLoop) == 0) && (instrHeader.Instrument.PanLoopStart == 0xff) && (instrHeader.Instrument.PanLoopEnd == 0xff)))
						{
							madeWith.Set(TrackerVersions.Confirmed);
							madeWith.Reset(TrackerVersions.NewModPlug);
						}
					}

					// Read sample headers
					vector<SampleIndex> sampleSlots = AllocateXmSamples(this, instrHeader.NumSamples);

					// Update sample assignment map
					for (size_t k = 0 + 12; k < 96 + 12; k++)
					{
						if (Instruments[instr].Keyboard[k] < sampleSlots.size())
							Instruments[instr].Keyboard[k] = sampleSlots[Instruments[instr].Keyboard[k]];
					}

					if (fileHeader.Version >= 0x0104)
						sampleFlags.clear();

					// Need to memorize those if we're going to skip any samples...
					vector<uint32> sampleSize = new vector<uint32>(instrHeader.NumSamples);

					for (SampleIndex sample = 0; sample < instrHeader.NumSamples; sample++)
					{
						XmSample sampleHeader = new XmSample();
						file.ReadStruct(ref sampleHeader);

						sampleFlags.push_back(sampleHeader.GetSampleFormat());
						sampleSize[sample] = sampleHeader.Length;
						sampleReserved |= sampleHeader.Reserved;

						if ((sampleHeader.Reserved != 0) && (sampleHeader.Reserved != 0xad))
							madeWith.Reset(TrackerVersions.OldModPlug | TrackerVersions.NewModPlug | TrackerVersions.OpenMpt);

						if (lastSampleReserved == -1)
							lastSampleReserved = sampleHeader.Reserved;
						else if (lastSampleReserved != sampleHeader.Reserved)
							madeWith.Reset(TrackerVersions.PlayerPro);

						if (sampleHeader.Pan != 128)
							madeWith.Reset(TrackerVersions.PlayerPro);

						if (((sampleHeader.FineTune & 0x0f) != 0) && (sampleHeader.FineTune != 127))
							madeWith.Reset(TrackerVersions.PlayerPro);

						if (sample < sampleSlots.size())
						{
							SampleIndex mptSample = sampleSlots[sample];

							sampleHeader.ConvertToMpt(Samples[mptSample]);
							instrHeader.Instrument.ApplyAutoVibratoToMpt(Samples[mptSample]);

							m_szNames[mptSample].Assign(MptString.ReadBuf(ReadWriteMode.SpacePadded, sampleHeader.Name.ToArray()));

							array<uint8> sampleName = sampleHeader.Name.ToArray();
							if (madeWith.Test(TrackerVersions.Ft2Generic | TrackerVersions.Ft2Clone) && madeWith.Test(TrackerVersions.NewModPlug | TrackerVersions.PlayerPro) && !madeWith.Test(TrackerVersions.Confirmed) &&
							    ((sampleHeader.Reserved > 22) || (Algorithm.find_if(sampleName.begin() + sampleHeader.Reserved, sampleName.end(), (uint8 c) => c != (uint8)' ')) != sampleName.end()))
							{
								// FT2 stores the sample name length here (it just copies the entire Pascal string, but that string might have ended with spaces even before space-padding it in the file, so we cannot do an exact length comparison)
								madeWith.Reset(TrackerVersions.Ft2Generic);
								madeWith.Set(TrackerVersions.Ft2Clone | TrackerVersions.Confirmed);
							}

							if (((sampleHeader.Flags & 3) == 3) && madeWith.Test(TrackerVersions.NewModPlug))
								madeWith.Set(TrackerVersions.ModPlugBidiFlag);
						}

						if (sampleFlags.back().GetEncoding() == SampleIO.Encoding.Adpcm)
							anyAdpcm = true;
					}

					// Read samples
					if (fileHeader.Version >= 0x0104)
					{
						for (SampleIndex sample = 0; sample < instrHeader.NumSamples; sample++)
						{
							// Sample 15 in dirtysex.xm by J/M/T/M is a 16-bit sample with an odd size of 0x18B according to the header, while the real sample size would be 0x18A.
							// Always read as many bytes as specified in the header, even if the sample reader would probably read less bytes
							FileReader sampleChunk = new FileReader(file.ReadChunk(sampleFlags[sample].GetEncoding() != SampleIO.Encoding.Adpcm ? sampleSize[sample] : (16 + ((sampleSize[sample] + 1) / 2))));

							if ((sample < sampleSlots.size()) && ((loadFlags & ModLoadingFlags.LoadSampleData) != 0))
							{
								if (!ReadSampleData(Samples[sampleSlots[sample]], sampleFlags[sample], sampleChunk, ref isOxm))
									unsupportedSamples = true;
							}
						}
					}
				}
			}

			if ((sampleReserved == 0) && madeWith.Test(TrackerVersions.NewModPlug) && CMemory.memchr<uint8>(fileHeader.SongName.ToArray().data(), 0x00, (size_t)Marshal.SizeOf(fileHeader.SongName)).IsNotNull)
			{
				// Null-terminated song name: Quite possibly MPT. (could really be an MPT-made file resaved in FT2, though)
				madeWith.Set(TrackerVersions.Confirmed);
			}

			if (fileHeader.Version < 0x0104)
			{
				// Load Patterns and Samples (Version 1.02 and 1.03)
				if ((loadFlags & (ModLoadingFlags.LoadPatternData | ModLoadingFlags.LoadSampleData)) != 0)
					ReadXmPatterns(file, ref fileHeader, this);

				if ((loadFlags & ModLoadingFlags.LoadSampleData) != 0)
				{
					for (SampleIndex sample = 1; sample <= GetNumSamples(); sample++)
						sampleFlags[sample - 1].ReadSample(Samples[sample], file);
				}
			}

			if (unsupportedSamples)
				throw new OpenMptException(Resources.IDS_MPT_ERR_UNSUPPORTED_SAMPLE);

			// Read song comments: "text"
			if (file.ReadMagic("text"))
			{
				m_SongMessage.Read(file, file.ReadUInt32LE(), SongMessage.LineEnding.Cr);
				madeWith.Set(TrackerVersions.Confirmed);
				madeWith.Reset(TrackerVersions.PlayerPro);
			}

			// Read midi config: "MIDI"
			bool hasMidiConfig = false;

			if (file.ReadMagic("MIDI"))
			{
				m_MidiCfg.Load(file, file.ReadUInt32LE());
				m_MidiCfg.Sanitize();

				hasMidiConfig = true;
				madeWith.Set(TrackerVersions.Confirmed);
				madeWith.Reset(TrackerVersions.PlayerPro);
			}

			// Read pattern names: "PNAM"
			if (file.ReadMagic("PNAM"))
			{
				PatternIndex namedPats = Math.Min((PatternIndex)(file.ReadUInt32LE() / Snd_Def.Max_PatternName), Patterns.Size());

				for (PatternIndex pat = 0; pat < namedPats; pat++)
				{
					uint8[] patName = new uint8[Snd_Def.Max_PatternName];

					file.ReadString(ReadWriteMode.MaybeNullTerminated, patName, Snd_Def.Max_PatternName);
					Patterns[pat].SetName(patName);
				}

				madeWith.Set(TrackerVersions.Confirmed);
				madeWith.Reset(TrackerVersions.PlayerPro);
			}

			// Read channel names: "CNAM"
			if (file.ReadMagic("CNAM"))
			{
				ChannelIndex namedChans = Math.Min((ChannelIndex)(file.ReadUInt32LE() / Snd_Def.Max_ChannelName), GetNumChannels());

				for (ChannelIndex chn = 0; chn < namedChans; chn++)
					file.ReadString(ReadWriteMode.MaybeNullTerminated, ChnSettings[chn].szName, Snd_Def.Max_ChannelName);

				madeWith.Set(TrackerVersions.Confirmed);
				madeWith.Reset(TrackerVersions.PlayerPro);
			}

			// Read mix plugins information
			if (file.CanRead(8))
			{
				size_t oldPos = file.GetPosition();

				LoadMixPlugins(file);

				if (file.GetPosition() != oldPos)
				{
					madeWith.Set(TrackerVersions.Confirmed);
					madeWith.Reset(TrackerVersions.PlayerPro);
				}
			}

			if (madeWith.Test(TrackerVersions.Confirmed))
			{
				if (madeWith.Test(TrackerVersions.ModPlugBidiFlag))
				{
					m_dwLastSavedWithVersion = Version.MPT_V._1_11;
					madeWithTracker = "ModPlug Tracker 1.0 - 1.11";
				}
				else if (madeWith.Test(TrackerVersions.NewModPlug) && !madeWith.Test(TrackerVersions.PlayerPro))
				{
					m_dwLastSavedWithVersion = Version.MPT_V._1_16;
					madeWithTracker = "ModPlug Tracker 1.0 - 1.16";
				}
				else if (madeWith.Test(TrackerVersions.NewModPlug) && madeWith.Test(TrackerVersions.PlayerPro))
				{
					m_dwLastSavedWithVersion = Version.MPT_V._1_16;
					madeWithTracker = "ModPlug Tracker 1.0 - 1.16 / PlayerPRO";
				}
				else if (!madeWith.Test(TrackerVersions.NewModPlug) && madeWith.Test(TrackerVersions.PlayerPro))
					madeWithTracker = "PlayerPRO";
			}

			if (CMemory.memcmp(trackerName, "OpenMPT ", 8) == 0)
			{
				// Hey, I know this tracker!
				StdString mptVersion = new StdString(trackerName + 8, 12);
				m_dwLastSavedWithVersion = Version.Parse(mptVersion.ToString());
				madeWith = TrackerVersions.OpenMpt | TrackerVersions.Confirmed;

				if (m_dwLastSavedWithVersion < Version.MPT_V._1_22_07_19)
					m_nMixLevels = MixLevels.Compatible;
				else
					m_nMixLevels = MixLevels.CompatibleFT2;
			}

			if ((m_dwLastSavedWithVersion.GetRawVersion() != 0) && !madeWith.Test(TrackerVersions.OpenMpt))
			{
				m_nMixLevels = MixLevels.Original;
				m_PlayBehaviour.reset();
			}

			if (madeWith.Test(TrackerVersions.Ft2Generic))
			{
				m_nMixLevels = MixLevels.CompatibleFT2;

				if (!hasMidiConfig)
				{
					// FT2 allows typing in arbitrary unsupported effect letters such as Zxx.
					// Prevent these commands from being interpreted as filter commands by erasing the default MIDI Config
					m_MidiCfg.ClearZxxMacros();
				}

				if (fileHeader.Version >= 0x0104)		// Old versions of FT2 didn't have (smooth) ramping. Disable it for those versions where we can be sure that there should be no ramping
				{
					// Apply FT2-style super-soft volume ramping
					m_PlayBehaviour.set(PlayBehaviour.Ft2VolumeRamping);
				}
			}

			if (string.IsNullOrEmpty(madeWithTracker))
			{
				if (madeWith.Test(TrackerVersions.DigiTrakker) && (sampleReserved == 0) && ((lastInstrType != 0 ? lastInstrType : -1) == -1))
					madeWithTracker = "DigiTrakker";
				else if (madeWith.Test(TrackerVersions.Ft2Generic))
					madeWithTracker = "FastTracker 2 or compatible";
				else
					madeWithTracker = "Unknown";
			}

			bool isOpenMptMade = false;	// Specific for OpenMPT 1.17+

			if (GetNumInstruments() != 0)
				isOpenMptMade = LoadExtendedInstrumentProperties(file);

			LoadExtendedSongProperties(file, true, ref isOpenMptMade);

			if (isOpenMptMade && (m_dwLastSavedWithVersion < Version.MPT_V._1_17))
			{
				// Up to OpenMPT 1.17.02.45 (r165), it was possible that the "last saved with" field was 0
				// when saving a file in OpenMPT for the first time
				m_dwLastSavedWithVersion = Version.MPT_V._1_17;
			}

			if (m_dwLastSavedWithVersion >= Version.MPT_V._1_17)
				madeWithTracker = "OpenMPT " + m_dwLastSavedWithVersion;

			// We no longer allow any --- or +++ items in the order list now
			if ((m_dwLastSavedWithVersion.GetRawVersion() != 0) && (m_dwLastSavedWithVersion < Version.MPT_V._1_22_02_02))
			{
				if (!Patterns.IsValidPat(0xfe))
					Order.Current.RemovePattern(0xfe);

				if (!Patterns.IsValidPat(0xff))
					Order.Current.Replace(0xff, Snd_Def.PatternIndex_Invalid);
			}

			m_ModFormat.FormatName = string.Format("FastTracker 2 v{0}.{1:x2}", fileHeader.Version >> 8, fileHeader.Version & 0xff);
			m_ModFormat.MadeWithTracker = Utility.move(madeWithTracker);
			m_ModFormat.CharSet = isMadTracker ? EncoderCollection.Win1252 : FindCharSet();

			if (isOxm)
			{
				m_ModFormat.OriginalFormatName = Utility.move(m_ModFormat.FormatName);
				m_ModFormat.FormatName = "OggMod FastTracker 2";
				m_ModFormat.Type = "oxm";
				m_ModFormat.OriginalType = "xm";
			}
			else
				m_ModFormat.Type = "xm";

			if (anyAdpcm)
				m_ModFormat.MadeWithTracker += Resources.IDS_MPT_ADPCM;

			m_ModFormat.ExtraInformation = m_ModFormat.MadeWithTracker;

			return true;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Allocate samples for an instrument
		/// </summary>
		/********************************************************************/
		private static vector<SampleIndex> AllocateXmSamples(CSoundFile sndFile, SampleIndex numSamples)
		{
			OpenMpt.LimitMax(ref numSamples, (SampleIndex)32);

			vector<SampleIndex> foundSlots = new vector<SampleIndex>();
			foundSlots.reserve(numSamples);

			for (SampleIndex i = 0; i < numSamples; i++)
			{
				SampleIndex candidateSlot = (SampleIndex)(sndFile.GetNumSamples() + 1);

				if (candidateSlot >= Snd_Def.Max_Samples)
				{
					// If too many sample slots are needed, try to fill some empty slots first
					for (SampleIndex j = 1; j <= sndFile.GetNumSamples(); j++)
					{
						if (sndFile.GetSample(j).HasSampleData())
							continue;

						if (!Mpt.Base.Algorithm.Contains(foundSlots, j))
						{
							// Empty sample slot that is not occupied by the current instrument. Yay!
							candidateSlot = j;

							// Remove unused sample from instrument sample assignments
							for (InstrumentIndex ins = 1; ins <= sndFile.GetNumInstruments(); ins++)
							{
								if (sndFile.Instruments[ins] == null)
									continue;

								for (size_t smp = 0; smp < sndFile.Instruments[ins].Keyboard.size(); smp++)
								{
									SampleIndex sample = sndFile.Instruments[ins].Keyboard[smp];

									if (sample == candidateSlot)
										sndFile.Instruments[ins].Keyboard[smp] = 0;
								}
							}

							break;
						}
					}
				}

				if (candidateSlot >= Snd_Def.Max_Samples)
				{
					// Still couldn't find any empty sample slots, so look out for existing but unused samples
					vector<bool> usedSamples = new vector<bool>();
					SampleIndex unusedSampleCount = sndFile.DetectUnusedSamples(usedSamples);

					if (unusedSampleCount > 0)
					{
						sndFile.RemoveSelectedSamples(usedSamples);

						// Remove unused samples from instrument sample assignments
						for (InstrumentIndex ins = 1; ins <= sndFile.GetNumInstruments(); ins++)
						{
							if (sndFile.Instruments[ins] == null)
								continue;

							for (size_t smp = 0; smp < sndFile.Instruments[ins].Keyboard.size(); smp++)
							{
								SampleIndex sample = sndFile.Instruments[ins].Keyboard[smp];

								if ((sample < usedSamples.size()) && !usedSamples[sample])
									sndFile.Instruments[ins].Keyboard[smp] = 0;
							}
						}

						// New candidate slot is first unused sample slot
						candidateSlot = (SampleIndex)(Algorithm.find(usedSamples.begin() + 1, usedSamples.end(), false) - usedSamples.begin());
					}
					else
					{
						// No unused sample slots: Give up :(
						break;
					}
				}

				if (candidateSlot < Snd_Def.Max_Samples)
				{
					foundSlots.push_back(candidateSlot);

					if (candidateSlot > sndFile.GetNumSamples())
						sndFile.m_nSamples = candidateSlot;
				}
			}

			return foundSlots;
		}



		/********************************************************************/
		/// <summary>
		/// Read .XM patterns
		/// </summary>
		/********************************************************************/
		private static void ReadXmPatterns(FileReader file, ref XmFileHeader fileHeader, CSoundFile sndFile)
		{
			// Reading patterns
			sndFile.Patterns.ResizeArray(fileHeader.Patterns);

			for (PatternIndex pat = 0; pat < fileHeader.Patterns; pat++)
			{
				size_t curPos = file.GetPosition();
				uint32 headerSize = file.ReadUInt32LE();

				if ((headerSize < 8) || !file.CanRead(headerSize - 4))
					break;

				file.Skip(1);	// Pack method (= 0)

				RowIndex numRows;

				if (fileHeader.Version == 0x0102)
					numRows = file.ReadUInt8() + 1U;
				else
					numRows = file.ReadUInt16LE();

				// A packed size of 0 indicates a completely empty pattern
				uint16 packedSize = file.ReadUInt16LE();

				if (numRows == 0)
					numRows = 64;
				else if (numRows > Snd_Def.Max_Pattern_Rows)
					numRows = Snd_Def.Max_Pattern_Rows;

				file.Seek(curPos + headerSize);

				FileReader patternChunk = new FileReader(file.ReadChunk(packedSize));

				if ((pat >= Snd_Def.Max_Patterns) || !sndFile.Patterns.Insert(pat, numRows) || (packedSize == 0))
					continue;

				const uint8 IsPackByte = 0x80;
				const uint8 AllFlags = 0xff;

				const uint8 NotePresent = 0x01;
				const uint8 InstrPresent = 0x02;
				const uint8 VolPresent = 0x04;
				const uint8 CommandPresent = 0x08;
				const uint8 ParamPresent = 0x10;

				foreach (ModCommand m in sndFile.Patterns[pat])
				{
					if (!file.CanRead(1))
						break;

					uint8 info = patternChunk.ReadUInt8();

					uint8 vol = 0, command = 0;

					if ((info & IsPackByte) != 0)
					{
						// Interpret byte as flag set
						if ((info & NotePresent) != 0)
							m.Note = patternChunk.ReadUInt8();
					}
					else
					{
						// Interpret byte as note, read all other pattern fields as well
						m.Note = info;
						info = AllFlags;
					}

					if ((info & InstrPresent) != 0)
						m.Instr = patternChunk.ReadUInt8();

					if ((info & VolPresent) != 0)
						vol = patternChunk.ReadUInt8();

					if ((info & CommandPresent) != 0)
						command = patternChunk.ReadUInt8();

					if ((info & ParamPresent) != 0)
						m.Param = patternChunk.ReadUInt8();

					if (m.Note == 97)
						m.Note = ModCommand.Note_KeyOff;
					else if ((m.Note > 0) && (m.Note < 97))
						m.Note += 12;
					else
						m.Note = ModCommand.Note_None;

					if ((command | m.Param) != 0)
						ModTools.ConvertModCommand(m, command, m.Param);
					else
						m.Command = EffectCommand.None;

					if (m.Instr == 0xff)
						m.Instr = 0;

					if ((vol >= 0x10) && (vol <= 0x50))
					{
						m.VolCmd = VolumeCommand.Volume;
						m.Vol = (uint8)(vol - 0x10);
					}
					else if (vol >= 0x60)
					{
						// Volume commands 6-F translation
						m.VolCmd = volEffTrans[(vol - 0x60) >> 4];
						m.Vol = (uint8)(vol & 0x0f);

						if (m.VolCmd == VolumeCommand.Panning)
							m.Vol *= 4;		// FT2 does indeed not scale panning symmetrically
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		private static bool ValidateHeader(ref XmFileHeader fileHeader)
		{
			if ((fileHeader.Channels == 0) || (fileHeader.Channels > Snd_Def.Max_BaseChannels) || (CMemory.memcmp(fileHeader.Signature.ToArray().begin(), "Extended Module: ", 17) != 0))
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		private static uint64 GetHeaderMinimumAdditionalSize(ref XmFileHeader fileHeader)
		{
			return (uint64)(fileHeader.Orders + (4 * (fileHeader.Patterns + fileHeader.Instruments)));
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		private static bool ReadSampleData(ModSample sample, SampleIO sampleFlags, FileReader sampleChunk, ref bool isOxm)
		{
			bool unsupportedSample = false;

			bool isOgg = false;
			isOxm = isOxm || isOgg;

			sampleChunk.Rewind();

			sampleFlags.ReadSample(sample, sampleChunk);

			return !unsupportedSample;
		}



		/********************************************************************/
		/// <summary>
		/// TNE: Added extra checks so only OpenMPT modules are recognized.
		/// The tracker detection of the original loader is ported, but the
		/// file is only walked through once and the probe returns as soon
		/// as the verdict is settled
		/// </summary>
		/********************************************************************/
		private static ProbeResult ExtendedProbeXm(FileReader file, ref XmFileHeader fileHeader)
		{
			CPointer<uint8> trackerName = fileHeader.TrackerName.ToArray().begin();
			CPointer<uint8> songName = fileHeader.SongName.ToArray().begin();

			bool isOpenMptName = CMemory.memcmp(trackerName, "OpenMPT ", 8) == 0;

			// ModPlugin is the web browser plugin which later became
			// ModPlug Tracker, and it is the only thing ever writing this
			// name. Neither the original loader nor LibXmp knows it, and
			// only some of the modules hold ADPCM packed samples, so the
			// name has to be taken as a signature on its own
			bool isModPlugName = isOpenMptName || (CMemory.memcmp(trackerName, "MOD Plugin packed", 17) == 0);

			// FastTracker 2 pads the song title with spaces, while some
			// other trackers terminate it with a null character
			c_int firstNull = FindNullCharacter(songName, 20);

			TrackerVersions madeWith;

			if ((CMemory.memcmp(trackerName, "FastTracker v2.00   ", 20) == 0) && (fileHeader.Size == 276))
			{
				if (fileHeader.Version < 0x0104)
					madeWith = TrackerVersions.Ft2Generic | TrackerVersions.Confirmed;
				else if (firstNull >= 0)
				{
					// PlayerPRO fills the rest of the buffer after the null
					// terminator with spaces and does not support a song
					// restart position
					if (fileHeader.RestartPos != 0)
						madeWith = TrackerVersions.Ft2Clone | TrackerVersions.NewModPlug | TrackerVersions.EmptyOrders;
					else if (firstNull == 19)
						madeWith = TrackerVersions.Ft2Clone | TrackerVersions.NewModPlug | TrackerVersions.PlayerPro | TrackerVersions.EmptyOrders;
					else if (IsOnlySpaces(songName + (firstNull + 1), 20 - (firstNull + 1)))
						madeWith = TrackerVersions.PlayerPro | TrackerVersions.Confirmed;
					else
						madeWith = TrackerVersions.Ft2Clone | TrackerVersions.NewModPlug | TrackerVersions.EmptyOrders;
				}
				else
				{
					if (fileHeader.RestartPos != 0)
						madeWith = TrackerVersions.Ft2Generic | TrackerVersions.NewModPlug;
					else
						madeWith = TrackerVersions.Ft2Generic | TrackerVersions.NewModPlug | TrackerVersions.PlayerPro;
				}
			}
			else if (CMemory.memcmp(trackerName, "FastTracker v 2.00  ", 20) == 0)
			{
				// ModPlug Tracker 1.0, the exact version is found below
				madeWith = TrackerVersions.OldModPlug;
			}
			else
			{
				// Something else. Only the trackers telling something about
				// ModPlug Tracker / OpenMPT are checked here. The remaining
				// ones handled by the original loader only change playback
				// quirks and all end up as unknown anyway
				madeWith = TrackerVersions.Unknown | TrackerVersions.Confirmed;

				if (isOpenMptName)
					madeWith = TrackerVersions.OpenMpt | TrackerVersions.Confirmed | TrackerVersions.EmptyOrders;
				else if (CMemory.memcmp(trackerName, "Fasttracker II clone", 20) == 0)
				{
					// 8bitbubsy's FastTracker 2 clone
					madeWith = TrackerVersions.Ft2Generic | TrackerVersions.Confirmed;
				}
				else if ((CMemory.memcmp(trackerName, "*Converted ", 11) == 0) && (CMemory.memcmp(trackerName + 14, "-File*", 6) == 0))
					madeWith = TrackerVersions.DigiTrakker | TrackerVersions.Confirmed;
			}

			// The extended filter range is only ever written by
			// ModPlug Tracker / OpenMPT. The original loader only
			// uses the flag to confirm an already suspected ModPlug module
			if ((fileHeader.Flags & XmFileHeader.ExtendedFilterRange) != 0)
				return ProbeResult.Success;

			// Skip the order list and jump to the pattern data. A file
			// which is cut short is not rejected right away, because the
			// tracker name alone may already have told enough
			bool truncated = !file.Seek(fileHeader.Size + 60U);

			if (!truncated && (fileHeader.Version >= 0x0104))
				SkipXmPatterns(file, ref fileHeader);

			bool anyAdpcm = false;
			bool oggResolved = false;
			uint8 sampleReserved = 0;
			c_int lastInstrType = -1;
			c_int lastSampleReserved = -1;
			int64 lastSampleHeaderSize = -1;
			bool instrumentWithSamplesEncountered = false;
			size_t totalSampleBytes = 0;

			InstrumentIndex numInstruments = (InstrumentIndex)Math.Min((c_int)fileHeader.Instruments, Snd_Def.Max_Instruments - 1);

			for (InstrumentIndex instr = 1; !truncated && (instr <= numInstruments); instr++)
			{
				if (!file.CanRead(4))
					break;

				size_t curPos = file.GetPosition();

				// The stored size tells how much of the instrument header
				// is really present in the file. Everything behind it is
				// taken as zero, just like the partial structure read done
				// by the original loader
				uint32 storedSize = file.ReadUInt32LE();
				uint32 headerSize = storedSize == 0 ? XmInstrumentHeaderSize : storedSize;
				uint32 presentSize = Math.Min(headerSize, XmInstrumentHeaderSize);

				uint8 insType = 0;
				uint16 numSamples = 0;
				uint32 sampleHeaderSize = 0;

				if ((presentSize >= 33) && file.Seek(curPos + 26UL))
				{
					insType = file.ReadUInt8();
					numSamples = file.ReadUInt16LE();
					sampleHeaderSize = file.ReadUInt32LE();
				}

				uint8 volLoopStart = 0, volLoopEnd = 0, volFlags = 0;
				uint8 panLoopStart = 0, panLoopEnd = 0, panFlags = 0;
				uint8 midiEnabled = 0, midiChannel = 0, muteComputer = 0;
				uint16 midiProgram = 0;

				if ((presentSize >= 248) && file.Seek(curPos + 228UL))
				{
					volLoopStart = file.ReadUInt8();
					volLoopEnd = file.ReadUInt8();
					file.Skip(1);							// Panning sustain point
					panLoopStart = file.ReadUInt8();
					panLoopEnd = file.ReadUInt8();
					volFlags = file.ReadUInt8();
					panFlags = file.ReadUInt8();
					file.Skip(6);							// Auto vibrato and fade out
					midiEnabled = file.ReadUInt8();
					midiChannel = file.ReadUInt8();
					midiProgram = file.ReadUInt16LE();
					file.Skip(2);							// Pitch wheel range
					muteComputer = file.ReadUInt8();
				}

				if (!file.Seek(curPos + headerSize))
				{
					truncated = true;
					break;
				}

				// Time for some version detection stuff
				if (madeWith == TrackerVersions.OldModPlug)
				{
					// ModPlug Tracker 1.0 alpha stores 245 and beta 263
					if ((storedSize == 245) || (storedSize == 263))
						madeWith |= TrackerVersions.Confirmed;
					else
						madeWith = TrackerVersions.Unknown | TrackerVersions.Confirmed;
				}
				else if (numSamples == 0)
				{
					// Empty instruments make tracker identification pretty easy
					if ((storedSize == 263) && (sampleHeaderSize == 0) && ((madeWith & TrackerVersions.NewModPlug) != 0))
						madeWith |= TrackerVersions.Confirmed;
					else if ((storedSize != 29) && ((madeWith & TrackerVersions.DigiTrakker) != 0))
						madeWith &= ~TrackerVersions.DigiTrakker;
					else if (((madeWith & (TrackerVersions.Ft2Clone | TrackerVersions.Ft2Generic)) != 0) && (storedSize != 33))
					{
						// Sure isn't FastTracker 2
						madeWith = TrackerVersions.Unknown;
					}

					if (storedSize != 33)
						madeWith &= ~TrackerVersions.PlayerPro;
					else if ((sampleHeaderSize > XmSampleHeaderSize) && ((madeWith & TrackerVersions.PlayerPro) != 0))
					{
						// Older PlayerPRO versions write garbage into the
						// sample header size field, and it is different
						// for each sample
						if (instrumentWithSamplesEncountered || ((lastSampleHeaderSize != -1) && (sampleHeaderSize != lastSampleHeaderSize)))
							madeWith = TrackerVersions.PlayerPro | TrackerVersions.Confirmed;

						lastSampleHeaderSize = sampleHeaderSize;
					}
				}

				if (lastInstrType == -1)
					lastInstrType = insType;
				else if ((lastInstrType != insType) && ((madeWith & TrackerVersions.Ft2Generic) != 0))
				{
					// FastTracker 2 writes some random junk into the
					// instrument type field, but it is always the same
					// junk for every instrument saved
					madeWith &= ~TrackerVersions.Ft2Generic;
					madeWith |= TrackerVersions.Ft2Clone;
				}

				if (numSamples > 0)
				{
					instrumentWithSamplesEncountered = true;

					// If MIDI settings are present, this is definitely not
					// an old ModPlug Tracker or PlayerPRO
					if ((midiEnabled | midiChannel | midiProgram | muteComputer) != 0)
						madeWith &= ~(TrackerVersions.OldModPlug | TrackerVersions.NewModPlug | TrackerVersions.PlayerPro);

					if ((storedSize != 263) || (insType != 0))
						madeWith &= ~TrackerVersions.PlayerPro;

					if (((madeWith & TrackerVersions.Confirmed) == 0) && ((madeWith & TrackerVersions.PlayerPro) != 0))
					{
						// Earlier PlayerPRO versions do not seem to set the loop points to 0xff
						if ((((volFlags & XmEnvelopeLoop) == 0) && (volLoopStart == 0xff) && (volLoopEnd == 0xff)) ||
						    (((panFlags & XmEnvelopeLoop) == 0) && (panLoopStart == 0xff) && (panLoopEnd == 0xff)))
						{
							madeWith |= TrackerVersions.Confirmed;
							madeWith &= ~TrackerVersions.NewModPlug;
						}
					}

					// Read the sample headers
					uint32[] sampleSizes = new uint32[numSamples];
					bool[] adpcmSamples = new bool[numSamples];

					for (SampleIndex smp = 0; smp < numSamples; smp++)
					{
						if (!file.CanRead(XmSampleHeaderSize))
						{
							truncated = true;
							break;
						}

						uint32 length = file.ReadUInt32LE();
						file.Skip(9);								// Loop points and volume
						int8 fineTune = (int8)file.ReadUInt8();
						uint8 sampleFlags = file.ReadUInt8();
						uint8 pan = file.ReadUInt8();
						file.Skip(1);								// Relative tone
						uint8 reserved = file.ReadUInt8();
						array<uint8> sampleName = file.ReadArray<uint8>(22);

						sampleReserved |= reserved;

						if ((reserved != 0) && (reserved != XmSampleAdpcm))
							madeWith &= ~(TrackerVersions.OldModPlug | TrackerVersions.NewModPlug | TrackerVersions.OpenMpt);

						if (lastSampleReserved == -1)
							lastSampleReserved = reserved;
						else if (lastSampleReserved != reserved)
							madeWith &= ~TrackerVersions.PlayerPro;

						if (pan != 128)
							madeWith &= ~TrackerVersions.PlayerPro;

						if (((fineTune & 0x0f) != 0) && (fineTune != 127))
							madeWith &= ~TrackerVersions.PlayerPro;

						// FastTracker 2 stores the sample name length here.
						// It just copies the whole Pascal string, and that
						// string might have ended with spaces even before
						// being space padded in the file, so an exact
						// length comparison cannot be made
						if (((madeWith & (TrackerVersions.Ft2Generic | TrackerVersions.Ft2Clone)) != 0) &&
						    ((madeWith & (TrackerVersions.NewModPlug | TrackerVersions.PlayerPro)) != 0) &&
						    ((madeWith & TrackerVersions.Confirmed) == 0) &&
						    ((reserved > 22) || !IsOnlySpaces(sampleName.begin() + reserved, 22 - reserved)))
						{
							madeWith &= ~TrackerVersions.Ft2Generic;
							madeWith |= TrackerVersions.Ft2Clone | TrackerVersions.Confirmed;
						}

						if (((sampleFlags & 3) == 3) && ((madeWith & TrackerVersions.NewModPlug) != 0))
							madeWith |= TrackerVersions.ModPlugBidiFlag;

						bool isAdpcm = (reserved == XmSampleAdpcm) && ((sampleFlags & (XmSample16Bit | XmSampleStereo)) == 0);
						if (isAdpcm)
							anyAdpcm = true;

						sampleSizes[smp] = length;
						adpcmSamples[smp] = isAdpcm;
					}

					if (truncated)
						break;

					// Read the sample data
					for (SampleIndex smp = 0; smp < numSamples; smp++)
					{
						size_t chunkSize = adpcmSamples[smp] ? 16UL + ((sampleSizes[smp] + 1UL) / 2UL) : sampleSizes[smp];

						if (fileHeader.Version < 0x0104)
						{
							// Version 1.02 and 1.03 store the sample data after the patterns instead
							totalSampleBytes += chunkSize;
							continue;
						}

						size_t chunkPos = file.GetPosition();

						// Only the first sample holding any data is
						// checked for Ogg Vorbis. OggMod encodes every
						// single sample, so one look is enough to tell an
						// .oxm apart. The original loader verifies all Ogg
						// pages of every single sample instead. OggMod
						// stores the length of the decoded sample in front
						// of the stream, so the magic sits 4 bytes in
						if (!oggResolved && (chunkSize >= 8))
						{
							oggResolved = true;

							file.Skip(4);

							if (file.ReadMagic("OggS"))
								return ProbeResult.Failure;
						}

						if (!file.Seek(chunkPos + chunkSize))
						{
							truncated = true;
							break;
						}
					}

					if (truncated)
						break;
				}

				// Only when it is known that OggMod has not been at work,
				// a success may be returned
				if (oggResolved && IsMadeWithModPlug(isModPlugName, madeWith, anyAdpcm))
					return ProbeResult.Success;
			}

			if (!truncated && (fileHeader.Version < 0x0104))
			{
				// Patterns and sample data are stored after the
				// instruments in version 1.02 and 1.03
				SkipXmPatterns(file, ref fileHeader);

				file.Skip(totalSampleBytes);
			}

			// A null terminated song name is quite possibly ModPlug
			// Tracker. It could really be a ModPlug made file which has
			// been resaved in FastTracker 2, though
			if ((sampleReserved == 0) && ((madeWith & TrackerVersions.NewModPlug) != 0) && (firstNull >= 0))
				madeWith |= TrackerVersions.Confirmed;

			// All the sample data has been passed by now, so it is known
			// whether OggMod has been at work or not and a success may be
			// returned at any time from here on
			if (IsMadeWithModPlug(isModPlugName, madeWith, anyAdpcm))
				return ProbeResult.Success;

			// The song extensions holding the song comments, the MIDI
			// configuration, the pattern names and the channel names are
			// only ever written by ModPlug Tracker / OpenMPT, so a single
			// one of them settles it no matter what the tracker detection
			// above has come up with
			if (file.ReadMagic("text") ||		// Song comments
			    file.ReadMagic("MIDI") ||		// MIDI configuration
			    file.ReadMagic("PNAM") ||		// Pattern names
			    file.ReadMagic("CNAM"))			// Channel names
			{
				return ProbeResult.Success;
			}

			// Mix plugins. LibXmp has no support for those at all, so it
			// does not matter which tracker wrote them
			if (file.CanRead(8) && SkipXmMixPlugins(file))
				return ProbeResult.Success;

			// Extended instrument and song properties are only written by
			// OpenMPT 1.17 and later, so finding either of them is enough
			if ((numInstruments > 0) && file.ReadMagic("XTPM"))
				return ProbeResult.Success;

			if (file.ReadMagic("STPM"))
				return ProbeResult.Success;

			return ProbeResult.Failure;
		}



		/********************************************************************/
		/// <summary>
		/// Tell if the collected flags point at a module made with ModPlug
		/// Tracker or OpenMPT. Only the two cases where the original loader
		/// itself reports ModPlug Tracker as the creating tracker are taken,
		/// so nothing is claimed which OpenMPT would not call ModPlug made
		/// </summary>
		/********************************************************************/
		private static bool IsMadeWithModPlug(bool isModPlugName, TrackerVersions madeWith, bool anyAdpcm)
		{
			// OpenMPT 1.17 and later and ModPlugin both write their own
			// name into the header, and only ModPlug ever packed samples
			// with its own ADPCM compression
			if (isModPlugName || anyAdpcm)
				return true;

			if ((madeWith & TrackerVersions.Confirmed) == 0)
				return false;

			// ModPlug Tracker 1.0 alpha / beta
			if ((madeWith & TrackerVersions.OldModPlug) != 0)
				return true;

			// ModPlug Tracker 1.0 - 1.16. PlayerPRO writes files which
			// look almost the same, so those are left alone
			return ((madeWith & TrackerVersions.NewModPlug) != 0) && ((madeWith & TrackerVersions.PlayerPro) == 0);
		}



		/********************************************************************/
		/// <summary>
		/// Skip over all the patterns without unpacking them
		/// </summary>
		/********************************************************************/
		private static void SkipXmPatterns(FileReader file, ref XmFileHeader fileHeader)
		{
			for (PatternIndex pat = 0; pat < fileHeader.Patterns; pat++)
			{
				size_t curPos = file.GetPosition();

				uint32 headerSize = file.ReadUInt32LE();
				if ((headerSize < 8) || !file.CanRead(headerSize - 4))
					break;

				file.Skip(1);		// Pack method (= 0)

				// Number of rows
				file.Skip(fileHeader.Version == 0x0102 ? 1UL : 2UL);

				// A packed size of 0 indicates a completely empty pattern
				uint16 packedSize = file.ReadUInt16LE();

				file.Seek(curPos + headerSize);
				file.Skip(packedSize);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Skip over all the mix plugin chunks without parsing them. Tells
		/// whether any real mix plugin chunk was found
		/// </summary>
		/********************************************************************/
		private static bool SkipXmMixPlugins(FileReader file)
		{
			bool hasPluginChunks = false;

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

				if ((CMemory.memcmp(code, "CHFX", 4) == 0) || IsXmPluginChunk(code))
					hasPluginChunks = true;

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
		private static bool IsXmPluginChunk(CPointer<uint8> code)
		{
			return (code[0] == 'F') && (code[1] == 'X') && (code[2] >= '0') && (code[2] <= '9') && (code[3] >= '0') && (code[3] <= '9');
		}



		/********************************************************************/
		/// <summary>
		/// Return the index of the first null character or -1 if the string
		/// does not hold any
		/// </summary>
		/********************************************************************/
		private static c_int FindNullCharacter(CPointer<uint8> str, c_int length)
		{
			for (c_int i = 0; i < length; i++)
			{
				if (str[i] == 0x00)
					return i;
			}

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// Tell if the given string only holds space characters
		/// </summary>
		/********************************************************************/
		private static bool IsOnlySpaces(CPointer<uint8> str, c_int length)
		{
			for (c_int i = 0; i < length; i++)
			{
				if (str[i] != 0x20)
					return false;
			}

			return true;
		}
		#endregion
	}
}
