/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.String;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Containers;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Exceptions;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Audio;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io_Read;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundDsp;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;
using FileReader = Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.FileReader;
using Version = Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.Version;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Core class of the playback engine. Every song is represented by a CSoundFile object
	/// </summary>
	internal partial class CSoundFile
	{
		/// <summary>
		/// Type of panning command
		/// </summary>
		public enum PanningType
		{
			Pan4Bit = 4,
			Pan6Bit = 6,
			Pan8Bit = 8
		}

		public const size_t ProbeRecommendedSize = 2048;

		public const uint32 Ticks_Row_Finished = uint32.MaxValue - 1U;

		public readonly Lock SndLock = new Lock();

		private readonly string[] m_NoteNames;//XX 429

		private readonly CTuningCollection m_pTuningsTuneSpecific;

		// Misc data
		private CModSpecifications m_pModSpecs;//XX 436

		// Interleaved Front Mix Buffer (Also room for interleaved rear mix)
		private readonly mixsample_t[] MixSoundBuffer = new mixsample_t[Mixer.MixBufferSize * 4];//XX 440
		private readonly mixsample_t[] MixRearBuffer = new mixsample_t[Mixer.MixBufferSize * 2];//XX 441

		// Non-interleaved plugin processing buffer
		private readonly c_float[][] MixFloatBuffer = ArrayHelper.Initialize2Arrays<c_float>(2, Mixer.MixBufferSize);//XX 443
		private readonly mixsample_t[][] MixInputBuffer = ArrayHelper.Initialize2Arrays<mixsample_t>(Mixer.NumMixInputBuffers, Mixer.MixBufferSize);//XX 444

		// End-of-sample pop reduction tail level
		private mixsample_t m_DryLOfsVol = 0;//XX 447
		private mixsample_t m_DryROfsVol = 0;
		private mixsample_t m_SurroundLOfsVol = 0;
		private mixsample_t m_SurroundROfsVol = 0;

		public MixerSettings m_MixerSettings = new MixerSettings();//XX 451
		public readonly CResampler m_Resampler = new CResampler();//XX 452

		public readonly mixsample_t[] ReverbSendBuffer = new mixsample_t[Mixer.MixBufferSize * 2];//XX 454
		public mixsample_t m_RvbROfsVol = 0;//XX 455
		public mixsample_t m_RvbLOfsVol = 0;
		public readonly CReverb m_Reverb = new CReverb();//XX 456

		private ModType m_nType;//XX 478
		private ModContainerType m_ContainerType = ModContainerType.None;//XX 479

		public SampleIndex m_nSamples = 0;//XX 481
		public InstrumentIndex m_nInstruments = 0;//XX 482
		public uint32 m_nDefaultGlobalVolume;//XX 483
		public SongFlags m_SongFlags;//XX 484
		public ChannelIndex m_nMixChannels = 0;//XX 485
		private ChannelIndex m_nMixStat;//XX 487

		// Interface to NostalgicPlayer visualizations. Tells which channels
		// that have been triggered since the last time the visualizer
		// channels were read
		public readonly bool[] m_VisualNoteKicked = new bool[Snd_Def.Max_Channels];

		// Default rows per beat and measure for this module
		public RowIndex m_nDefaultRowsPerBeat;//XX 489
		public RowIndex m_nDefaultRowsPerMeasure;
		public TempoMode m_nTempoMode = TempoMode.Classic;//XX 490

		public uint32 m_nSamplePreAmp;//XX 499
		public uint32 m_nVstiVolume;
		public uint32 m_OplVolumeFactor;	// 16,16//XX 500
		public const uint32 m_OplVolumeFactorScale = 1 << 16;//XX 501

		/// <summary>
		/// Pitch shift factor (65536 = no pitch shifting)
		/// </summary>
		public uint32 m_nFreqFactor = 65536;//XX 505

		/// <summary>
		/// Tempo factor (65536 = no tempo adjustment)
		/// </summary>
		public uint32 m_nTempoFactor = 65536;//XX 506

		/// <summary>
		/// Row swing factors for modern tempo mode
		/// </summary>
		public readonly TempoSwing m_TempoSwing = new TempoSwing();//XX 510

		/// <summary>
		/// Min Period = highest possible frequency, Max Period = lowest possible frequency for current format
		/// Note: Period is an Amiga metric that is inverse to frequency.
		/// Periods in MPT are 4 times as fine as Amiga periods because of extra fine frequency slides (introduced in the S3M format)
		/// </summary>
		public int32 m_nMinPeriod;//XX 515
		public int32 m_nMaxPeriod;

		/// <summary>
		/// Resampling mode (if overriding the globally set resampling)
		/// </summary>
		public ResamplingMode m_nResampling;//XX 517
		public int32 m_nRepeatCount = 0;	// -1 means repeat infinitely//XX 518
		public OrderIndex m_RestartOverridePos = 0;//XX 519
		public OrderIndex m_MaxOrderPosition = 0;
		public readonly vector<ModChannelSettings> ChnSettings = new vector<ModChannelSettings>();	// Initial channels settings//XX 520
		public readonly CPatternContainer Patterns;//XX 521
		public readonly ModSequenceSet Order;	// Pattern sequences (order list)//XX 522
		protected readonly ModSample[] Samples = ArrayHelper.InitializeArray<ModSample>(Snd_Def.Max_Samples);//XX 524

		public readonly ModInstrument[] Instruments = new ModInstrument[Snd_Def.Max_Instruments];	// Instrument headers//XX 526
		public readonly InstrumentSynthEvents m_GlobalScript = new InstrumentSynthEvents();//XX 527
		public readonly MidiMacroConfig m_MidiCfg = new MidiMacroConfig();//XX 528

		public readonly CharBuf[] m_szNames = ArrayHelper.InitializeArray(Snd_Def.Max_Samples, () => new CharBuf(Snd_Def.Max_SampleName));//XX 533

		public Version m_dwCreatedWithVersion = new Version();//XX 535
		public Version m_dwLastSavedWithVersion = new Version();//XX 536

		public PlayBehaviourSet m_PlayBehaviour = new PlayBehaviourSet();//XX 538

		protected Fast_Prng m_Prng;//XX 542

		protected CSoundFilePlayConfig m_PlayConfig = new CSoundFilePlayConfig();//XX 548
		protected MixLevels m_nMixLevels;//XX 549

		public PlayState m_PlayState = new PlayState();//XX 552

		/// <summary>
		/// For handling backwards jumps and stuff to prevent infinite loops when counting the mod length or rendering to wav
		/// </summary>
		protected RowVisitor m_VisitedRows;//XX 556

		public StdString m_SongName = new StdString();//XX 583
		public string m_SongArtist;//XX 584
		public readonly SongMessage m_SongMessage = new SongMessage();//XX 585
		public ModFormatDetails m_ModFormat = new ModFormatDetails();//XX 586

		protected readonly vector<FileHistory> m_FileHistory = new vector<FileHistory>();//XX 589

		public bool m_bIsRendering = false;//XX 609

		public const uint32 FadeSongDelay = 100;//XX 1309

		public class FileFormatLoader
		{
			public delegate ProbeResult Prober_Delegate(FileReader file, uint64? pFileSize);
			public delegate IFormatLoader Create_Delegate(CSoundFile soundFile);

			public Guid Id { get; init; }
			public string Name { get; init; }
			public string Description { get; init; }
			public Prober_Delegate Prober { get; init; }
			public Create_Delegate Create { get; init; }
		}

		private static readonly FileFormatLoader[] moduleFormatLoaders =
		[
			ModLoader.Format
		];

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public CSoundFile()//XX 113
		{
			m_NoteNames = Tables.NoteNamesSharp;
			m_pModSpecs = ModSpecs.itEx;
			m_nType = ModType.None;

			Patterns = new CPatternContainer(this);
			Order = new ModSequenceSet(this);

			m_Prng = Seed.Make_Prng<Fast_Prng, uint64>(MptRandom.Global_Prng());
			m_VisitedRows = new RowVisitor(this);

			OpenMpt.MemsetZero(MixSoundBuffer);
			OpenMpt.MemsetZero(MixRearBuffer);
			OpenMpt.MemsetZero(MixFloatBuffer[0]);
			OpenMpt.MemsetZero(MixFloatBuffer[1]);

			m_nDefaultRowsPerBeat = m_PlayState.m_nCurrentRowsPerBeat = Snd_Def.Default_Rows_Per_Beat;
			m_nDefaultRowsPerMeasure = m_PlayState.m_nCurrentRowsPerMeasure = Snd_Def.Default_Rows_Per_Measure;

			OpenMpt.MemsetZero(Instruments);
			OpenMpt.Clear(m_szNames);

			m_pTuningsTuneSpecific = new CTuningCollection();
		}



		/********************************************************************/
		/// <summary>
		/// Return a list of all available formats
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<FileFormatLoader> GetFormats()
		{
			return moduleFormatLoaders;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsGlobalVolumeUnset()
		{
			return IsFirstTick();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Fast_Prng AccessPrng()
		{
			return m_Prng;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public vector<FileHistory> GetFileHistory()
		{
			return m_FileHistory;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ModType GetType_()
		{
			return m_nType;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ModContainerType GetContainerType()
		{
			return m_ContainerType;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Encoding GetCharsetFile()
		{
			return m_ModFormat.CharSet;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Encoding GetCharsetInternal()
		{
			return GetCharsetFile();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public LogicalTimezone GetTimezoneInternal()
		{
			return m_ModFormat.Timezone;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MixLevels GetMixLevels()
		{
			return m_nMixLevels;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public InstrumentIndex GetNumInstruments()
		{
			return m_nInstruments;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SampleIndex GetNumSamples()
		{
			return m_nSamples;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ChannelIndex GetNumChannels()
		{
			return (ChannelIndex)ChnSettings.size();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public PatternIndex GetCurrentPattern()
		{
			return m_PlayState.m_nPattern;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public OrderIndex GetCurrentOrder()
		{
			return m_PlayState.m_nCurrentOrder;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool CanAddMoreSamples(SampleIndex amount = 1)
		{
			return (amount < Snd_Def.Max_Samples) && (m_nSamples < (Snd_Def.Max_Samples - amount));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CModSpecifications GetModSpecifications()
		{
			return m_pModSpecs;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ResetMixStat()
		{
			m_nMixStat = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public StdString GetTitle()
		{
			return m_SongName;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsFirstTick()
		{
			return m_PlayState.m_lTotalSampleCount == 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public samplecount_t Read(samplecount_t count, IAudioTarget target)
		{
			AudioSourceNone source = new AudioSourceNone();

			return Read(count, target, source);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsRenderingToDisc()
		{
			return m_bIsRendering;
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if periods are actually plain frequency values in Hz
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool PeriodsAreFrequencies()
		{
			return m_PlayBehaviour[PlayBehaviour.PeriodsAreHertz] && !UseFineTuneAndTranspose();
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if the format uses transpose+finetune rather than
		/// frequency in Hz to specify middle-C
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool UseFineTuneAndTranspose(ModType type)
		{
			return (type & (ModType.Amf0 | ModType.Digi | ModType.Med | ModType.Mod | ModType.Mtm | ModType.Okt | ModType.Sfx | ModType.Stp | ModType.Xm)) != 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool UseFineTuneAndTranspose()
		{
			return UseFineTuneAndTranspose(GetType_());
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if the format uses combined commands for fine and
		/// regular portamento slides
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool UseCombinedPortamentoCommands(ModType type)
		{
			return (type & (ModType.Mod | ModType.Xm | ModType.Mt2 | ModType.Med | ModType.Amf0 | ModType.Digi | ModType.Stp | ModType.Dtm)) == 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool UseCombinedPortamentoCommands()
		{
			return UseCombinedPortamentoCommands(GetType_());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ModSample GetSample(SampleIndex sample)
		{
			return Samples[sample];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int8 Mod2XmFineTune(c_int v)
		{
			return (int8)((uint8)v << 4);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int8 Xm2ModFineTune(c_int v)
		{
			return (int8)((uint8)v >> 4);
		}



		/********************************************************************/
		/// <summary>
		/// Global variable initializer for loader functions
		/// </summary>
		/********************************************************************/
		public void InitializeGlobals(ModType type, ChannelIndex numChannels)//XX 173
		{
			// Do not add or change any of these values! And if you do, review each and every loader to check if they require these defaults!
			m_nType = type;
			OpenMpt.LimitMax(ref numChannels, Snd_Def.Max_BaseChannels);

			ModType bestType = GetBestSaveFormat();
			m_PlayBehaviour = GetDefaultPlaybackBehaviour(bestType);

			if ((bestType == ModType.It) && (type != bestType))
			{
				// This is such an odd behaviour that it's unlikely that any of the other formats will need it by default. Re-enable as needed
				m_PlayBehaviour.reset(PlayBehaviour.ItInitialNoteMemory);
			}

			m_pModSpecs = GetModSpecifications(bestType);

			// Delete instruments in case some previously called loader already created them
			for (InstrumentIndex i = 1; i <= m_nInstruments; i++)
				Instruments[i] = null;

			m_ContainerType = ModContainerType.None;
			m_nInstruments = 0;
			m_nSamples = 0;
			m_nSamplePreAmp = 48;
			m_nVstiVolume = 48;
			m_OplVolumeFactor = m_OplVolumeFactorScale;
			m_nDefaultGlobalVolume = Snd_Def.Max_Global_Volume;
			m_SongFlags.Reset();
			m_nMinPeriod = 16;
			m_nMaxPeriod = 32767;
			m_nResampling = ResamplingMode.Default;
			m_dwLastSavedWithVersion = new Version(0);
			m_dwCreatedWithVersion = new Version(0);
			m_nTempoMode = TempoMode.Classic;

			SetMixLevels(MixLevels.Compatible);

			Patterns.DestroyPatterns();
			Order.Initialize();

			m_GlobalScript.clear();
			m_SongName.clear();
			m_SongArtist = string.Empty;
			m_SongMessage.clear();
			m_ModFormat = new ModFormatDetails();
			m_FileHistory.clear();
			m_TempoSwing.clear();

			// Note: We do not use the Amiga resampler for DBM as it's a multichannel format and can make use of higher-quality Amiga soundcards instead of Paula
			if ((GetType_() & (ModType.Digi | ModType.Med | ModType.Mod | ModType.Okt | ModType.Sfx | ModType.Stp)) != 0)
				m_SongFlags.Set(SongFlags.IsAmiga);

			if ((GetType_() & (ModType.Amf0 | ModType.Digi | ModType.Mtm)) != 0)
				m_SongFlags.Set(SongFlags.Format_No_VolCol);

			ChnSettings.assign(numChannels, new ModChannelSettings());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static ProbeResult Probe(ProbeFlags flags, Stream stream, uint64? pFileSize, out Probe_File_Header_Info info)//XX 366
		{
			info = null;

			ProbeResult result = ProbeResult.Failure;

			// TNE: Change to FileReader instead of MemoryFileReader, because I want to use the stream directly
			FileReader file = new FileReader(FileCursor_StdStream.Make_FileCursor<PathString>(stream));

			if ((flags & ProbeFlags.Containers) != 0)
			{
				// TNE: No containers are ported in this version, since all
				// supported containers are processed elsewhere (converter agents)
			}

			if ((flags & ProbeFlags.Modules) != 0)
			{
				foreach (FileFormatLoader format in moduleFormatLoaders)
				{
					if (format.Prober != null)
					{
						ProbeResult lastResult = format.Prober(new FileReader(file), pFileSize);

						if (lastResult == ProbeResult.Success)
						{
							info = new Probe_File_Header_Info
							{
								Id = format.Id
							};

							return ProbeResult.Success;
						}
						else if (lastResult == ProbeResult.WantMoreData)
							result = ProbeResult.WantMoreData;
					}
				}
			}

			if (pFileSize.HasValue)
			{
				if ((result == ProbeResult.WantMoreData) && (size_t.CreateSaturating(pFileSize.Value) <= file.GetLength()))
				{
					// If the prober wants more data but we already reached EOF,
					// probing must fail
					result = ProbeResult.Failure;
				}
			}
			else
			{
				if ((result == ProbeResult.WantMoreData) && file.LengthIsAtLeast(ProbeRecommendedSize))
				{
					// If the prober wants more data, but we already provided the recommended required maximum,
					// just return success as this is the best we can do for the suggested probing size
					result = ProbeResult.Success;
				}
			}

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Create(ModType type, ChannelIndex numChannels)//XX 418
		{
			Create(new FileReader(), ModLoadingFlags.LoadCompleteModule);
			SetType(type);
			ChnSettings.resize(numChannels);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool Create(FileReader file, ModLoadingFlags loadFlags, Guid? formatId = null)//XX 426
		{
			m_nMixChannels = 0;
			m_nFreqFactor = m_nTempoFactor = 65536;

			OpenMpt.Clear(m_szNames);

			if (CreateInternal(file, loadFlags, formatId))
				return true;

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool CreateInternal(FileReader file, ModLoadingFlags loadFlags, Guid? formatId)//XX 479
		{
			if (file.IsValid())
			{
				vector<ContainerItem> containerItems = new vector<ContainerItem>();
				ModContainerType packedContainerType = ModContainerType.None;

				if ((loadFlags & ModLoadingFlags.SkipContainer) == 0)
				{
					ContainerLoadingFlags containerLoadFlags = loadFlags == ModLoadingFlags.OnlyVerifyHeader ? ContainerLoadingFlags.OnlyVerifyHeader : ContainerLoadingFlags.UnwrapData;

					// TNE: Removed different unpack calls, since this is done
					// elsewhere in NostalgicPlayer (decruncher agents)

					if (packedContainerType != ModContainerType.None)
					{
						if (loadFlags == ModLoadingFlags.OnlyVerifyHeader)
							return true;

						if (!containerItems.empty())
							file = containerItems[0].File;
					}
				}

				if ((loadFlags & ModLoadingFlags.SkipModules) != 0)
					return false;

				// Try all module format loaders
				bool loaderSuccess = false;

				foreach (FileFormatLoader format in moduleFormatLoaders)
				{
					if (formatId.HasValue && (format.Id != formatId.Value))
						continue;

					loaderSuccess = format.Create(this).Read(file, loadFlags);

					if (loaderSuccess)
						break;
				}

				if (!loaderSuccess)
				{
					m_nType = ModType.None;
					m_ContainerType = ModContainerType.None;
				}

				if (loadFlags == ModLoadingFlags.OnlyVerifyHeader)
					return loaderSuccess;

				if ((packedContainerType != ModContainerType.None) && (m_ContainerType == ModContainerType.None))
					m_ContainerType = packedContainerType;

				m_VisitedRows.Initialize(true);
			}
			else
			{
				// New song
				InitializeGlobals(ModType.None, 0);
				m_VisitedRows.Initialize(true);
				m_dwCreatedWithVersion = Version.Current();
			}

			// Adjust channels
			ChannelFlags muteFlag = GetChannelMuteFlag();

			for (ChannelIndex chn = 0; chn < ChnSettings.size(); chn++)
			{
				OpenMpt.LimitMax(ref ChnSettings[chn].nVolume, (uint8)64);

				if (ChnSettings[chn].nPan > 256)
					ChnSettings[chn].nPan = 128;

				if (ChnSettings[chn].nMixPlugin > Snd_Def.Max_MixPlugins)
					ChnSettings[chn].nMixPlugin = 0;

				m_PlayState.Chn[chn].Reset(ModChannel.ResetFlags.Total, this, chn, muteFlag);
			}

			// Checking samples, load external samples
			for (SampleIndex nSmp = 1; nSmp <= m_nSamples; nSmp++)
			{
				ModSample sample = Samples[nSmp];
				OpenMpt.LimitMax(ref sample.nLength, Snd_Def.Max_Sample_Length);
				sample.SanitizeLoops();

				if (sample.HasSampleData())
					sample.PrecomputeLoops(this, false);
				else if (!sample.uFlags.Test(ChannelFlags.Smp_KeepOnDisk))
				{
					sample.nLength = 0;
					sample.nLoopStart = 0;
					sample.nLoopEnd = 0;
					sample.nSustainStart = 0;
					sample.nSustainEnd = 0;
					sample.uFlags.Reset(ChannelFlags.Chn_Loop | ChannelFlags.Chn_PingPongLoop | ChannelFlags.Chn_SustainLoop | ChannelFlags.Chn_PingPongSustain);
				}

				if (sample.nGlobalVol > 64)
					sample.nGlobalVol = 64;

/*				if (sample.uFlags.Test(ChannelFlags.Chn_Adlib) && (m_Opl == null))
					InitOpl();*///XX
			}

			// Check invalid instruments
			InstrumentIndex maxInstr = 0;

			for (InstrumentIndex i = 0; i <= m_nInstruments; i++)
			{
				if (Instruments[i] != null)
				{
					maxInstr = i;
					Instruments[i].Sanitize(GetType_());
				}
			}

			m_nInstruments = maxInstr;

			// Set default play state values
			if ((m_nDefaultRowsPerBeat == 0) && (m_nTempoMode == TempoMode.Modern))
				m_nDefaultRowsPerBeat = 1;

			if (m_nDefaultRowsPerMeasure < m_nDefaultRowsPerBeat)
				m_nDefaultRowsPerMeasure = m_nDefaultRowsPerBeat;

			OpenMpt.LimitMax(ref m_nDefaultRowsPerBeat, Snd_Def.Max_Rows_Per_Beat);
			OpenMpt.LimitMax(ref m_nDefaultRowsPerMeasure, Snd_Def.Max_Rows_Per_Beat);
			OpenMpt.LimitMax(ref m_nDefaultGlobalVolume, Snd_Def.Max_Global_Volume);
			OpenMpt.LimitMax(ref m_nSamplePreAmp, Snd_Def.Max_PreAmp);
			OpenMpt.LimitMax(ref m_nVstiVolume, Snd_Def.Max_PreAmp);

			if (!m_TempoSwing.empty())
				m_TempoSwing.resize(m_nDefaultRowsPerBeat);

			m_PlayState.m_nMusicSpeed = Order.Current.GetDefaultSpeed();
			m_PlayState.m_nMusicTempo = Order.Current.GetDefaultTempo();
			m_PlayState.m_nCurrentRowsPerBeat = m_nDefaultRowsPerBeat;
			m_PlayState.m_nCurrentRowsPerMeasure = m_nDefaultRowsPerMeasure;
			m_PlayState.m_nGlobalVolume = (int32)m_nDefaultGlobalVolume;
			m_PlayState.ResetGlobalVolumeRamping();

			m_PlayState.m_nNextOrder = 0;
			m_PlayState.m_nCurrentOrder = 0;
			m_PlayState.m_nPattern = 0;
			m_PlayState.m_nBufferCount = 0;
			m_PlayState.m_dBufferDiff = 0;
			m_PlayState.m_nTickCount = Ticks_Row_Finished;
			m_PlayState.m_nNextRow = 0;
			m_PlayState.m_nRow = 0;
			m_PlayState.m_nPatternDelay = 0;
			m_PlayState.m_nFrameDelay = 0;
			m_PlayState.m_NextPatStartRow = 0;
			m_PlayState.m_nSeqOverride = Snd_Def.OrderIndex_Invalid;

			if (UseFineTuneAndTranspose())
				m_PlayBehaviour.reset(PlayBehaviour.PeriodsAreHertz);

			m_RestartOverridePos = m_MaxOrderPosition = 0;

			RecalculateSamplesPerTick();

			foreach (ModSequence order in Order)
			{
				order.Shrink();

				if (order.GetRestartPos() >= order.size())
					order.SetRestartPos(0);
			}

			if (GetType_() == ModType.None)
				return false;

			m_pModSpecs = GetModSpecifications(GetBestSaveFormat());

			// When reading a file made with an older version of MPT, it might be necessary to upgrade some settings automatically
			if (m_dwLastSavedWithVersion)
				UpgradeModule();

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetPreAmp(uint32 nVol)//XX 877
		{
			if (nVol < 1)
				nVol = 1;

			if (nVol > 0x200)
				nVol = 0x200;	// x4 maximum

			m_MixerSettings.m_nPreAmp = nVol;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_double GetCurrentBpm()
		{
			c_double bpm;

			if (m_nTempoMode == TempoMode.Modern)
			{
				// With modern mode, we trust that true bpm is close enough to what user chose.
				// This avoids oscillation due to tick-to-tick corrections
				bpm = m_PlayState.m_nMusicTempo.ToDouble();
			}
			else
			{
				// With other modes, we calculate it:
				RowIndex rowsPerBeat = m_PlayState.m_nCurrentRowsPerBeat != 0 ? m_PlayState.m_nCurrentRowsPerBeat : Snd_Def.Default_Rows_Per_Beat;
				c_double ticksPerBeat = m_PlayState.m_nMusicSpeed * rowsPerBeat;		// Ticks/beat = ticks/row * rows/beat
				c_double samplesPerBeat = m_PlayState.m_nSamplesPerTick * ticksPerBeat;	// Samps/beat = samps/tick * ticks/beat
				bpm = m_MixerSettings.gdwMixingFreq / samplesPerBeat * 60;				// Beats/sec  = samps/sec  / samps/beat
			}																			// Beats/min  =  beats/sec * 60

			return bpm;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void ResetPlayPos()//XX 913
		{
			ChannelFlags muteFlag = GetChannelMuteFlag();

			for (ChannelIndex i = 0; i < m_PlayState.Chn.size(); i++)
				m_PlayState.Chn[i].Reset(ModChannel.ResetFlags.SetPosFull, this, i, muteFlag);

			Array.Clear(m_VisualNoteKicked);	// NostalgicPlayer specific

			m_VisitedRows.Initialize(true);
			m_PlayState.m_Flags.Reset(PlayFlags.Song_FadingSong | PlayFlags.Song_EndReached);

			m_PlayState.m_nGlobalVolume = (int32)m_nDefaultGlobalVolume;
			m_PlayState.m_nMusicSpeed = Order.Current.GetDefaultSpeed();
			m_PlayState.m_nMusicTempo = Order.Current.GetDefaultTempo();

			// Do not ramp global volume when starting playback
			m_PlayState.ResetGlobalVolumeRamping();

			m_PlayState.m_nNextOrder = 0;
			m_PlayState.m_nNextRow = 0;
			m_PlayState.m_nTickCount = Ticks_Row_Finished;
			m_PlayState.m_nBufferCount = 0;
			m_PlayState.m_dBufferDiff = 0;
			m_PlayState.m_nPatternDelay = 0;
			m_PlayState.m_nFrameDelay = 0;
			m_PlayState.m_NextPatStartRow = 0;
			m_PlayState.m_lTotalSampleCount = 0;
			m_PlayState.m_ppqPosFract = 0.0;
			m_PlayState.m_ppqPosBeat = 0;
			m_PlayState.m_GlobalScriptState.Initialize(this);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetCurrentOrder(OrderIndex nOrder)//XX 945
		{
			while ((nOrder < Order.Current.size()) && !Order.Current.IsValidPat(nOrder))
				nOrder++;

			if (nOrder >= Order.Current.size())
				return;

			foreach (ModChannel chn in m_PlayState.Chn)
			{
				chn.nPeriod = 0;
				chn.nNote = ModCommand.Note_None;
				chn.nPortamentoDest = 0;
				chn.nCommand = EffectCommand.None;
				chn.nPatternLoopCount = 0;
				chn.nPatternLoop = 0;
				chn.nVibratoPos = chn.nTremoloPos = chn.nPanbrelloPos = 0;

				// IT compatibility 15. Retrigger
				if (m_PlayBehaviour[PlayBehaviour.ItRetrigger])
				{
					chn.nRetrigCount = 0;
					chn.nRetrigParam = 1;
				}

				chn.nTremorCount = 0;
			}

			if (nOrder == 0)
				ResetPlayPos();
			else
			{
				m_PlayState.m_nNextOrder = nOrder;
				m_PlayState.m_nRow = m_PlayState.m_nNextRow = 0;
				m_PlayState.m_nPattern = 0;
				m_PlayState.m_nTickCount = Ticks_Row_Finished;
				m_PlayState.m_nBufferCount = 0;
				m_PlayState.m_dBufferDiff = 0;
				m_PlayState.m_nPatternDelay = 0;
				m_PlayState.m_nFrameDelay = 0;
				m_PlayState.m_NextPatStartRow = 0;
				m_PlayState.m_GlobalScriptState.Initialize(this);
			}

			m_PlayState.m_Flags.Reset(PlayFlags.Song_FadingSong | PlayFlags.Song_EndReached);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SuspendPlugins()//XX 995
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void ResumePlugins()//XX 1011
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void StopAllVsti()//XX 1042
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetMixLevels(MixLevels levels)//XX 1057
		{
			m_nMixLevels = levels;
			m_PlayConfig.SetMixLevels(m_nMixLevels);
			RecalculateGainForAllPlugs();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void RecalculateGainForAllPlugs()//XX 1065
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public PlayBehaviourSet GetSupportedPlaybackBehaviour(ModType type)//XX 1163
		{
			PlayBehaviourSet playBehaviour = new PlayBehaviourSet();

			switch (type)
			{
				case ModType.Mpt:
				{
					playBehaviour.set(PlayBehaviour.OplFlexibleNoteOff);
					playBehaviour.set(PlayBehaviour.OplWithNna);
					playBehaviour.set(PlayBehaviour.OplNoteOffOnNoteChange);

					goto case ModType.It;
				}

				case ModType.It:
				{
					playBehaviour.set(PlayBehaviour.Msf_Compatible_Play);
					playBehaviour.set(PlayBehaviour.PeriodsAreHertz);
					playBehaviour.set(PlayBehaviour.TempoClamp);
					playBehaviour.set(PlayBehaviour.PerChannelGlobalVolSlide);
					playBehaviour.set(PlayBehaviour.PanOverride);
					playBehaviour.set(PlayBehaviour.ItInstrWithoutNote);
					playBehaviour.set(PlayBehaviour.ItVolColFinePortamento);
					playBehaviour.set(PlayBehaviour.ItArpeggio);
					playBehaviour.set(PlayBehaviour.ItOutOfRangeDelay);
					playBehaviour.set(PlayBehaviour.ItPortaMemoryShare);
					playBehaviour.set(PlayBehaviour.ItPatternLoopTargetReset);
					playBehaviour.set(PlayBehaviour.ItFt2PatternLoop);
					playBehaviour.set(PlayBehaviour.ItPingPongNoReset);
					playBehaviour.set(PlayBehaviour.ItEnvelopeReset);
					playBehaviour.set(PlayBehaviour.ItClearOldNoteAfterCut);
					playBehaviour.set(PlayBehaviour.ItVibratoTremoloPanbrello);
					playBehaviour.set(PlayBehaviour.ItTremor);
					playBehaviour.set(PlayBehaviour.ItRetrigger);
					playBehaviour.set(PlayBehaviour.ItMultiSampleBehaviour);
					playBehaviour.set(PlayBehaviour.ItPortaTargetReached);
					playBehaviour.set(PlayBehaviour.ItPatternLoopBreak);
					playBehaviour.set(PlayBehaviour.ItOffset);
					playBehaviour.set(PlayBehaviour.ItSwingBehaviour);
					playBehaviour.set(PlayBehaviour.ItNnaReset);
					playBehaviour.set(PlayBehaviour.ItSCxStopsSample);
					playBehaviour.set(PlayBehaviour.ItEnvelopePositionHandling);
					playBehaviour.set(PlayBehaviour.ItPortamentoInstrument);
					playBehaviour.set(PlayBehaviour.ItPingPongMode);
					playBehaviour.set(PlayBehaviour.ItRealNoteMapping);
					playBehaviour.set(PlayBehaviour.ItHighOffsetNoRetrig);
					playBehaviour.set(PlayBehaviour.ItFilterBehaviour);
					playBehaviour.set(PlayBehaviour.ItNoSurroundPan);
					playBehaviour.set(PlayBehaviour.ItShortSampleRetrig);
					playBehaviour.set(PlayBehaviour.ItPortaNoNote);
					playBehaviour.set(PlayBehaviour.ItFt2DontResetNoteOffOnPorta);
					playBehaviour.set(PlayBehaviour.ItVolColMemory);
					playBehaviour.set(PlayBehaviour.ItPortamentoSwapResetsPos);
					playBehaviour.set(PlayBehaviour.ItEmptyNoteMapSlot);
					playBehaviour.set(PlayBehaviour.ItFirstTickHandling);
					playBehaviour.set(PlayBehaviour.ItSampleAndHoldPanbrello);
					playBehaviour.set(PlayBehaviour.ItClearPortaTarget);
					playBehaviour.set(PlayBehaviour.ItPanbrelloHold);
					playBehaviour.set(PlayBehaviour.ItPanningReset);
					playBehaviour.set(PlayBehaviour.ItPatternLoopWithJumps);
					playBehaviour.set(PlayBehaviour.ItInstrWithNoteOff);
					playBehaviour.set(PlayBehaviour.ItMultiSampleInstrumentNumber);
					playBehaviour.set(PlayBehaviour.RowDelayWithNoteDelay);
					playBehaviour.set(PlayBehaviour.ItInstrWithNoteOffOldEffects);
					playBehaviour.set(PlayBehaviour.ItDoNotOverrideChannelPan);
					playBehaviour.set(PlayBehaviour.ItDctBehaviour);
					playBehaviour.set(PlayBehaviour.ItPitchPanSeparation);
					playBehaviour.set(PlayBehaviour.ItResetFilterOnPortaSmpChange);
					playBehaviour.set(PlayBehaviour.ItInitialNoteMemory);
					playBehaviour.set(PlayBehaviour.ItNoSustainOnPortamento);
					playBehaviour.set(PlayBehaviour.ItEmptyNoteMapSlotIgnoreCell);
					playBehaviour.set(PlayBehaviour.ItOffsetWithInstrNumber);
					playBehaviour.set(PlayBehaviour.ItDoublePortamentoSlides);
					playBehaviour.set(PlayBehaviour.ItCarryAfterNoteOff);
					playBehaviour.set(PlayBehaviour.ItNoteCutWithPorta);
					playBehaviour.set(PlayBehaviour.ItVolColNoSlidePropagation);
					playBehaviour.set(PlayBehaviour.ItStoppedFilterEnvAtStart);
					break;
				}

				case ModType.Xm:
				{
					playBehaviour.set(PlayBehaviour.Msf_Compatible_Play);
					playBehaviour.set(PlayBehaviour.Ft2VolumeRamping);
					playBehaviour.set(PlayBehaviour.TempoClamp);
					playBehaviour.set(PlayBehaviour.PerChannelGlobalVolSlide);
					playBehaviour.set(PlayBehaviour.PanOverride);
					playBehaviour.set(PlayBehaviour.ItFt2PatternLoop);
					playBehaviour.set(PlayBehaviour.ItFt2DontResetNoteOffOnPorta);
					playBehaviour.set(PlayBehaviour.Ft2Arpeggio);
					playBehaviour.set(PlayBehaviour.Ft2Retrigger);
					playBehaviour.set(PlayBehaviour.Ft2VolColVibrato);
					playBehaviour.set(PlayBehaviour.Ft2PortaNoNote);
					playBehaviour.set(PlayBehaviour.Ft2KeyOff);
					playBehaviour.set(PlayBehaviour.Ft2PanSlide);
					playBehaviour.set(PlayBehaviour.Ft2St3OffsetOutOfRange);
					playBehaviour.set(PlayBehaviour.Ft2RestrictXCommand);
					playBehaviour.set(PlayBehaviour.Ft2RetrigWithNoteDelay);
					playBehaviour.set(PlayBehaviour.Ft2SetPanEnvPos);
					playBehaviour.set(PlayBehaviour.Ft2PortaIgnoreInstr);
					playBehaviour.set(PlayBehaviour.Ft2VolColMemory);
					playBehaviour.set(PlayBehaviour.Ft2LoopE60Restart);
					playBehaviour.set(PlayBehaviour.Ft2ProcessSilentChannels);
					playBehaviour.set(PlayBehaviour.Ft2ReloadSampleSettings);
					playBehaviour.set(PlayBehaviour.Ft2PortaDelay);
					playBehaviour.set(PlayBehaviour.Ft2Transpose);
					playBehaviour.set(PlayBehaviour.Ft2PatternLoopWithJumps);
					playBehaviour.set(PlayBehaviour.Ft2PortaTargetNoReset);
					playBehaviour.set(PlayBehaviour.Ft2EnvelopeEscape);
					playBehaviour.set(PlayBehaviour.Ft2Tremor);
					playBehaviour.set(PlayBehaviour.Ft2OutOfRangeDelay);
					playBehaviour.set(PlayBehaviour.Ft2Periods);
					playBehaviour.set(PlayBehaviour.Ft2PanWithDelayedNoteOff);
					playBehaviour.set(PlayBehaviour.Ft2VolColDelay);
					playBehaviour.set(PlayBehaviour.Ft2FineTunePrecision);
					playBehaviour.set(PlayBehaviour.Ft2NoteOffFlags);
					playBehaviour.set(PlayBehaviour.RowDelayWithNoteDelay);
					playBehaviour.set(PlayBehaviour.Ft2ModTremoloRampWaveform);
					playBehaviour.set(PlayBehaviour.Ft2PortaUpDownMemory);
					playBehaviour.set(PlayBehaviour.Ft2PanSustainRelease);
					playBehaviour.set(PlayBehaviour.Ft2NoteDelayWithoutInstr);
					playBehaviour.set(PlayBehaviour.Ft2PortaResetDirection);
					playBehaviour.set(PlayBehaviour.Ft2AutoVibratoAbortSweep);
					playBehaviour.set(PlayBehaviour.Ft2OffsetMemoryRequiresNote);
					break;
				}

				case ModType.S3M:
				{
					playBehaviour.set(PlayBehaviour.Msf_Compatible_Play);
					playBehaviour.set(PlayBehaviour.TempoClamp);
					playBehaviour.set(PlayBehaviour.PanOverride);
					playBehaviour.set(PlayBehaviour.ItPanbrelloHold);
					playBehaviour.set(PlayBehaviour.Ft2St3OffsetOutOfRange);
					playBehaviour.set(PlayBehaviour.St3NoMutedChannels);
					playBehaviour.set(PlayBehaviour.St3PortaSampleChange);
					playBehaviour.set(PlayBehaviour.St3EffectMemory);
					playBehaviour.set(PlayBehaviour.St3VibratoMemory);
					playBehaviour.set(PlayBehaviour.St3PortaAfterArpeggio);
					playBehaviour.set(PlayBehaviour.RowDelayWithNoteDelay);
					playBehaviour.set(PlayBehaviour.St3OffsetWithoutInstrument);
					playBehaviour.set(PlayBehaviour.St3RetrigAfterNoteCut);
					playBehaviour.set(PlayBehaviour.St3SampleSwap);
					playBehaviour.set(PlayBehaviour.OplNoteOffOnNoteChange);
					playBehaviour.set(PlayBehaviour.ApplyUpperPeriodLimit);
					playBehaviour.set(PlayBehaviour.St3TonePortaWithAdlibNote);
					playBehaviour.set(PlayBehaviour.S3MIgnoreCombinedFineSlides);
					break;
				}

				case ModType.Mod:
				{
					playBehaviour.set(PlayBehaviour.ModVBlankTiming);
					playBehaviour.set(PlayBehaviour.ModOneShotLoops);
					playBehaviour.set(PlayBehaviour.ModIgnorePanning);
					playBehaviour.set(PlayBehaviour.ModSampleSwap);
					playBehaviour.set(PlayBehaviour.ModOutOfRangeNoteDelay);
					playBehaviour.set(PlayBehaviour.ModTempoOnSecondTick);
					playBehaviour.set(PlayBehaviour.RowDelayWithNoteDelay);
					playBehaviour.set(PlayBehaviour.Ft2ModTremoloRampWaveform);
					break;
				}

				default:
				{
					playBehaviour.set(PlayBehaviour.Msf_Compatible_Play);
					playBehaviour.set(PlayBehaviour.PeriodsAreHertz);
					playBehaviour.set(PlayBehaviour.TempoClamp);
					playBehaviour.set(PlayBehaviour.PanOverride);
					break;
				}
			}

			return playBehaviour;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public PlayBehaviourSet GetDefaultPlaybackBehaviour(ModType type)//XX 1325
		{
			PlayBehaviourSet playBehaviour;

			switch (type)
			{
				case ModType.Mpt:
				{
					playBehaviour = new PlayBehaviourSet();

					playBehaviour.set(PlayBehaviour.PeriodsAreHertz);
					playBehaviour.set(PlayBehaviour.PerChannelGlobalVolSlide);
					playBehaviour.set(PlayBehaviour.PanOverride);
					playBehaviour.set(PlayBehaviour.ItArpeggio);
					playBehaviour.set(PlayBehaviour.ItPortaMemoryShare);
					playBehaviour.set(PlayBehaviour.ItPatternLoopTargetReset);
					playBehaviour.set(PlayBehaviour.ItFt2PatternLoop);
					playBehaviour.set(PlayBehaviour.ItPingPongNoReset);
					playBehaviour.set(PlayBehaviour.ItClearOldNoteAfterCut);
					playBehaviour.set(PlayBehaviour.ItVibratoTremoloPanbrello);
					playBehaviour.set(PlayBehaviour.ItMultiSampleBehaviour);
					playBehaviour.set(PlayBehaviour.ItPortaTargetReached);
					playBehaviour.set(PlayBehaviour.ItPatternLoopBreak);
					playBehaviour.set(PlayBehaviour.ItSwingBehaviour);
					playBehaviour.set(PlayBehaviour.ItSCxStopsSample);
					playBehaviour.set(PlayBehaviour.ItEnvelopePositionHandling);
					playBehaviour.set(PlayBehaviour.ItPingPongMode);
					playBehaviour.set(PlayBehaviour.ItRealNoteMapping);
					playBehaviour.set(PlayBehaviour.ItPortaNoNote);
					playBehaviour.set(PlayBehaviour.ItVolColMemory);
					playBehaviour.set(PlayBehaviour.ItFirstTickHandling);
					playBehaviour.set(PlayBehaviour.ItClearPortaTarget);
					playBehaviour.set(PlayBehaviour.ItSampleAndHoldPanbrello);
					playBehaviour.set(PlayBehaviour.ItPanbrelloHold);
					playBehaviour.set(PlayBehaviour.ItPanningReset);
					playBehaviour.set(PlayBehaviour.ItInstrWithNoteOff);
					playBehaviour.set(PlayBehaviour.OplFlexibleNoteOff);
					playBehaviour.set(PlayBehaviour.ItDoNotOverrideChannelPan);
					playBehaviour.set(PlayBehaviour.ItDctBehaviour);
					playBehaviour.set(PlayBehaviour.OplWithNna);
					playBehaviour.set(PlayBehaviour.ItPitchPanSeparation);
					break;
				}

				case ModType.S3M:
				{
					playBehaviour = GetSupportedPlaybackBehaviour(type);

					// Default behaviour was chosen to follow GUS, so kST3PortaSampleChange is enabled and kST3SampleSwap is disabled.
					// For SoundBlaster behaviour, those two flags would need to be swapped
					playBehaviour.reset(PlayBehaviour.St3SampleSwap);

					// Most trackers supporting the S3M format, including all OpenMPT versions up to now, support fine slides with Kxy / Lxy, so only enable this quirk for files made with ST3
					playBehaviour.reset(PlayBehaviour.S3MIgnoreCombinedFineSlides);
					break;
				}

				case ModType.Xm:
				{
					playBehaviour = GetSupportedPlaybackBehaviour(type);

					// Only set this explicitely for FT2-made XMs
					playBehaviour.reset(PlayBehaviour.Ft2VolumeRamping);
					break;
				}

				case ModType.Mod:
				{
					playBehaviour = new PlayBehaviourSet();

					playBehaviour.set(PlayBehaviour.RowDelayWithNoteDelay);
					break;
				}

				default:
				{
					playBehaviour = GetSupportedPlaybackBehaviour(type);
					break;
				}
			}

			return playBehaviour;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ModType GetBestSaveFormat()//XX 1391
		{
			switch (GetType_())
			{
				case ModType.Mod:
				case ModType.S3M:
				case ModType.Xm:
				case ModType.It:
				case ModType.Mpt:
					return GetType_();

				case ModType.Amf0:
				case ModType.Digi:
				case ModType.Sfx:
				case ModType.Stp:
					return ModType.Mod;

				case ModType.Med:
				{
					if (m_nInstruments == 0)
					{
						foreach (CPattern pat in Patterns)
						{
							if (pat.IsValid() && (pat.GetNumRows() != 64))
								return ModType.Xm;
						}

						return ModType.Mod;
					}

					return ModType.Xm;
				}

				case ModType.Psm:
				{
					if (Order.GetNumSequences() > 1)
						return ModType.Mpt;

					if (GetNumChannels() > 16)
						return ModType.It;

					for (ChannelIndex i = 0; i < GetNumChannels(); i++)
					{
						if (ChnSettings[i].dwFlags.Test(ChannelFlags.Chn_Surround) || (ChnSettings[i].nVolume != 64))
							return ModType.It;
					}

					return ModType.S3M;
				}

				case ModType._669:
				case ModType.Far:
				case ModType.Stm:
				case ModType.Dsm:
				case ModType.Amf:
				case ModType.Mtm:
					return ModType.S3M;

				case ModType.Ams:
				case ModType.Dmf:
				case ModType.Dbm:
				case ModType.Imf:
				case ModType.J2B:
				case ModType.Ult:
				case ModType.Okt:
				case ModType.Mt2:
				case ModType.Mdl:
				case ModType.Ptm:
				case ModType.Dtm:
				default:
					return ModType.It;

				case ModType.Mid:
					return ModType.Mpt;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public CPointer<uint8> GetSampleName(SampleIndex nSample)//XX 1457
		{
			if (nSample < Snd_Def.Max_Samples)
				return m_szNames[nSample].Buf;
			else
				return new CPointer<uint8>(0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public CPointer<uint8> GetInstrumentName(InstrumentIndex nInstr)//XX 1470
		{
			if ((nInstr >= Snd_Def.Max_Instruments) || (Instruments[nInstr] == null))
				return new CPointer<uint8>(0);

			return Instruments[nInstr].Name.Buf;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void InitAmigaResampler()//XX 1480
		{
			if (m_SongFlags.Test(SongFlags.IsAmiga) && (m_Resampler.m_Settings.EmulateAmiga != Resampling.AmigaFilter.Off))
				throw new OpenMptException("Amiga filter not implemented");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public CModSpecifications GetModSpecifications(ModType type)//XX 1705
		{
			switch (type)
			{
				case ModType.Mpt:
					return ModSpecs.mptm;

				case ModType.It:
					return ModSpecs.itEx;

				case ModType.Xm:
					return ModSpecs.xmEx;

				case ModType.S3M:
					return ModSpecs.s3mEx;

				case ModType.Mod:
				default:
					return ModSpecs.mod;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetType(ModType type)//XX 1733
		{
			m_nType = type;
			m_PlayBehaviour = GetDefaultPlaybackBehaviour(GetBestSaveFormat());
			m_pModSpecs = GetModSpecifications(GetBestSaveFormat());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ModMessageHeuristicOrder GetMessageHeuristic()//XX 1778
		{
			ModMessageHeuristicOrder result = ModMessageHeuristicOrder.Default;

			switch (GetType_())
			{
				case ModType.Mpt:
				{
					result = ModMessageHeuristicOrder.Samples;
					break;
				}

				case ModType.It:
				{
					result = ModMessageHeuristicOrder.Samples;
					break;
				}

				case ModType.Xm:
				{
					result = ModMessageHeuristicOrder.InstrumentsSamples;
					break;
				}

				case ModType.Mdl:
				{
					result = ModMessageHeuristicOrder.InstrumentsSamples;
					break;
				}

				case ModType.Imf:
				{
					result = ModMessageHeuristicOrder.InstrumentsSamples;
					break;
				}

				default:
				{
					result = ModMessageHeuristicOrder.Default;
					break;
				}
			}

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the length of a tick, depending on the tempo mode.
		/// This differs from GetTickDuration() by not accumulating errors
		/// because this is not called once per tick but in unrelated
		/// circumstances. So this should not update error accumulation
		/// </summary>
		/********************************************************************/
		public void RecalculateSamplesPerTick()//XX 1845
		{
			switch (m_nTempoMode)
			{
				case TempoMode.Classic:
				default:
				{
					m_PlayState.m_nSamplesPerTick = (uint32)Util.MulDiv((int32)m_MixerSettings.gdwMixingFreq, (int32)(5 * Tempo.FractFact), (int32)Math.Max(1, m_PlayState.m_nMusicTempo.GetRaw() << 1));
					break;
				}

				case TempoMode.Modern:
				{
					m_PlayState.m_nSamplesPerTick = (uint32)((Util.Mul32To64_Unsigned(m_MixerSettings.gdwMixingFreq, 60 * Tempo.FractFact) / Math.Max(1UL, Util.Mul32To64_Unsigned(m_PlayState.m_nMusicSpeed, m_PlayState.m_nCurrentRowsPerBeat) * m_PlayState.m_nMusicTempo.GetRaw())));
					break;
				}

				case TempoMode.Alternative:
				{
					m_PlayState.m_nSamplesPerTick = (uint32)Util.MulDiv((int32)m_MixerSettings.gdwMixingFreq, (int32)Tempo.FractFact, (int32)Math.Max(1, m_PlayState.m_nMusicTempo.GetRaw()));
					break;
				}
			}

			m_PlayState.m_nSamplesPerTick = (uint32)Util.MulDivR((int32)m_PlayState.m_nSamplesPerTick, (int32)m_nTempoFactor, 65536);

			if (m_PlayState.m_nSamplesPerTick == 0)
				m_PlayState.m_nSamplesPerTick = 1;
		}



		/********************************************************************/
		/// <summary>
		/// Get length of a tick in sample, with tick-to-tick tempo
		/// correction in modern tempo mode. This has to be called exactly
		/// once per tick because otherwise the error accumulation goes wrong
		/// </summary>
		/********************************************************************/
		public uint32 GetTickDuration(PlayState playState)//XX 1873
		{
			uint32 retVal = 0;

			switch (m_nTempoMode)
			{
				case TempoMode.Classic:
				default:
				{
					retVal = (uint32)Util.MulDiv((int32)m_MixerSettings.gdwMixingFreq, (int32)(5 * Tempo.FractFact), (int32)Math.Max(1, playState.m_nMusicTempo.GetRaw() << 1));
					break;
				}

				case TempoMode.Alternative:
				{
					retVal = (uint32)Util.MulDiv((int32)m_MixerSettings.gdwMixingFreq, (int32)Tempo.FractFact, (int32)Math.Max(1, playState.m_nMusicTempo.GetRaw()));
					break;
				}

				case TempoMode.Modern:
				{
					c_double accurateBufferCount = m_MixerSettings.gdwMixingFreq * (60.0 / (playState.m_nMusicTempo.ToDouble() * Util.Mul32To64_Unsigned(playState.m_nMusicSpeed, playState.m_nCurrentRowsPerBeat)));
					TempoSwing swing = (Patterns.IsValidPat(playState.m_nPattern) && Patterns[playState.m_nPattern].HasTempoSwing()) ? Patterns[playState.m_nPattern].GetTempoSwing() : m_TempoSwing;

					if (!swing.empty())
					{
						// Apply current row's tempo swing factor
						uint32 swingFactor = swing[playState.m_nRow % swing.size()];
						accurateBufferCount = accurateBufferCount * swingFactor / (c_double)TempoSwing.Unity;
					}

					uint32 bufferCount = (uint32)accurateBufferCount;
					playState.m_dBufferDiff += accurateBufferCount - bufferCount;

					// tick-to-tick tempo correction:
					if (playState.m_dBufferDiff >= 1)
					{
						bufferCount++;
						playState.m_dBufferDiff--;
					}
					else if (m_PlayState.m_dBufferDiff <= -1)
					{
						bufferCount--;
						playState.m_dBufferDiff++;
					}

					retVal = bufferCount;
					break;
				}
			}

			// When the user modifies the tempo, we do not really care about accurate tempo error accumulation
			retVal = Util.MulDivR_Unsigned(retVal, m_nTempoFactor, 65536);

			if (retVal == 0)
				retVal = 1;

			return retVal;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public static ChannelFlags GetChannelMuteFlag()//XX 1946
		{
			return ChannelFlags.Chn_SyncMute;
		}



		/********************************************************************/
		/// <summary>
		/// Resolve note/instrument combination to real sample index. Return
		/// value is guaranteed to be in [0, GetNumSamples()]
		/// </summary>
		/********************************************************************/
		public SampleIndex GetSampleIndex(ModCommandNote note, uint32 instr)//XX 1957
		{
			SampleIndex smp = 0;

			if (GetNumInstruments() != 0)
			{
				if (ModCommand.IsNote(note) && (instr <= GetNumInstruments()) && (Instruments[instr] != null))
					smp = Instruments[instr].Keyboard[note - ModCommand.Note_Min];
			}
			else
				smp = (SampleIndex)instr;

			if (smp <= GetNumSamples())
				return smp;
			else
				return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ModInstrument AllocateInstrument(InstrumentIndex instr, SampleIndex assignedSample)//XX 2053
		{
			if ((instr == 0) || (instr >= Snd_Def.Max_Instruments))
				return null;

			ModInstrument ins = Instruments[instr];

			if (ins != null)
			{
				// Re-initialize instrument
				new ModInstrument(assignedSample).CopyTo(ins);
			}
			else
			{
				// Create new instrument
				Instruments[instr] = ins = new ModInstrument(assignedSample);
			}

			if (ins != null)
				m_nInstruments = Math.Max(m_nInstruments, instr);

			return ins;
		}



		/********************************************************************/
		/// <summary>
		/// Set up channel panning suitable for MOD + similar files. If the
		/// current mod type is not MOD, forceSetup has to be set to true for
		/// this function to take effect
		/// </summary>
		/********************************************************************/
		public void SetupModPanning(bool forceSetup = false)//XX 2128
		{
			// Setup LRRL panning, max channel volume
			if (((GetType_() & ModType.Mod) == 0) && !forceSetup)
				return;

			for (ChannelIndex chn = 0; chn < GetNumChannels(); chn++)
			{
				ChnSettings[chn].dwFlags.Reset(ChannelFlags.Chn_Surround);

				if ((m_MixerSettings.MixerFlags & MixerFlags.MaxDefaultPan) != 0)
					ChnSettings[chn].nPan = (uint16)(((chn & 3) == 1) || ((chn & 3) == 2) ? 256 : 0);
				else
					ChnSettings[chn].nPan = (uint16)(((chn & 3) == 1) || ((chn & 3) == 2) ? 0xc0 : 0x40);
			}
		}
	}
}
