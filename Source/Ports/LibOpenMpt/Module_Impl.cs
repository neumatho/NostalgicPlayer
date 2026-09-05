/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Containers;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Exceptions;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Audio;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io_Read;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Parse;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundBase.SampleConversion;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Dithers;
using Algorithm = Polycode.NostalgicPlayer.Kit.C.Std.Algorithm;
using FileReader = Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.FileReader;
using SubSongs_Type = Polycode.NostalgicPlayer.Kit.C.Std.vector<Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers.SubSong_Data>;
using Utility = Polycode.NostalgicPlayer.Kit.C.Std.Utility;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt
{
	/// <summary>
	/// 
	/// </summary>
	internal class Module_Impl
	{
		public enum Amiga_Filter_Type
		{
			A500,
			A1200,
			Unfiltered,
			Auto_Filter
		}

		protected enum Song_End_Action
		{
			Fadeout_Song,
			Continue_Song,
			Stop_Song
		}

		public enum Ctl_Type
		{
			Boolean,
			Integer,
			FloatingPoint,
			Text
		}

		public class Ctl_Info
		{
			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public Ctl_Info(string name, Ctl_Type type)
			{
				Name = name;
				Type = type;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public string Name { get; }



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public Ctl_Type Type { get; }
		}

		private static readonly Ctl_Info[] ctl_Infos =
		[
			new Ctl_Info("load.skip_samples", Ctl_Type.Boolean),
			new Ctl_Info("load.skip_patterns", Ctl_Type.Boolean),
			new Ctl_Info("load.skip_plugins", Ctl_Type.Boolean),
			new Ctl_Info("load.skip_subsongs_init", Ctl_Type.Boolean),
			new Ctl_Info("seek.sync_samples", Ctl_Type.Boolean),
			new Ctl_Info("subsong", Ctl_Type.Integer),
			new Ctl_Info("play.tempo_factor", Ctl_Type.FloatingPoint),
			new Ctl_Info("play.pitch_factor", Ctl_Type.FloatingPoint),
			new Ctl_Info("play.at_end", Ctl_Type.Text),
			new Ctl_Info("render.resampler.emulate_amiga", Ctl_Type.Boolean),
			new Ctl_Info("render.resampler.emulate_amiga_type", Ctl_Type.Text),
			new Ctl_Info("render.opl.volume_factor", Ctl_Type.FloatingPoint),
			new Ctl_Info("dither", Ctl_Type.Integer)
		];

		protected const int32_t All_SubSongs = -1;

		protected int32_t m_Current_SubSong;
		protected c_double m_CurrentPositionSeconds;
		protected readonly CSoundFile m_SndFile;
		protected bool m_Loaded;
		protected bool m_Mixer_Initialized;
		protected readonly DithersWrapperOpenMpt m_Dithers;
		protected SubSongs_Type m_SubSongs = new SubSongs_Type();
		protected c_float m_Gain;
		protected Song_End_Action m_Ctl_Play_At_End;
		protected Amiga_Filter_Type m_Ctl_Render_Resampler_Emulate_Amiga_Type;
		protected bool m_Ctl_Load_Skip_Samples;
		protected bool m_Ctl_Load_Skip_Patterns;
		protected bool m_Ctl_Load_Skip_Plugins;
		protected bool m_Ctl_Load_Skip_SubSongs_Init;
		protected bool m_Ctl_Seek_Sync_Samples;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Module_Impl(map<string, string> ctls)//XX 466
		{
			m_SndFile = new CSoundFile();
			m_Loaded = false;
			m_Mixer_Initialized = false;
			m_Dithers = DithersWrapperOpenMpt.Create(MptRandom.Global_Prng(), DithersWrapperOpenMpt.DefaultDither, 4);
			m_Current_SubSong = 0;
			m_CurrentPositionSeconds = 0.0;
			m_Gain = 1.0f;
			m_Ctl_Play_At_End = Song_End_Action.Fadeout_Song;
			m_Ctl_Load_Skip_Samples = false;
			m_Ctl_Load_Skip_Patterns = false;
			m_Ctl_Load_Skip_Plugins = false;
			m_Ctl_Load_Skip_SubSongs_Init = false;
			m_Ctl_Seek_Sync_Samples = true;

			// Init member variables that correspond to this
			foreach (var ctl in ctls)
				Ctl_Set(ctl.first, ctl.second, false);
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Module_Impl(Stream stream, Guid? formatId, map<string, string> ctls) : this(ctls)//XX 893
		{
			Load(FileCursor_StdStream.Make_FileCursor<PathString>(stream), formatId, ctls);
			Apply_LibOpenMpt_Defaults();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected void Apply_Mixer_Settings(int32_t sampleRate, c_int channels)//XX 422
		{
			bool sampleRate_Changed = (int32_t)m_SndFile.m_MixerSettings.gdwMixingFreq != sampleRate;
			bool channels_Changed = (c_int)m_SndFile.m_MixerSettings.gnChannels != channels;

			if (sampleRate_Changed || channels_Changed)
			{
				MixerSettings mixerSettings = m_SndFile.m_MixerSettings;

				int32_t volRampIn_Us = mixerSettings.GetVolumeRampUpMicroseconds();
				int32_t volRampOut_Us = mixerSettings.GetVolumeRampDownMicroseconds();

				mixerSettings.gdwMixingFreq = (uint32)sampleRate;
				mixerSettings.gnChannels = (uint32)channels;

				mixerSettings.SetVolumeRampUpMicroseconds(volRampIn_Us);
				mixerSettings.SetVolumeRampDownMicroseconds(volRampOut_Us);

				m_SndFile.SetMixerSettings(ref mixerSettings);
			}
			else if (!m_Mixer_Initialized)
				m_SndFile.InitPlayer(true);

			if (sampleRate_Changed)
			{
				m_SndFile.SuspendPlugins();
				m_SndFile.ResumePlugins();
			}

			m_Mixer_Initialized = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected void Apply_LibOpenMpt_Defaults()//XX 443
		{
			Set_Render_Param(Render_Param.StereoSeparation_Percent, 100);
			m_SndFile.Order.SetSequence(0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected SubSongs_Type Get_SubSongs()//XX 447
		{
			vector<SubSong_Data> subSongs = new vector<SubSong_Data>();

			if (m_SndFile.Order.GetNumSequences() == 0)
				throw new OpenMptException("Module contains no songs");

			for (SequenceIndex seq = 0; seq < m_SndFile.Order.GetNumSequences(); ++seq)
			{
				vector<GetLengthType> lengths = m_SndFile.GetLength(EnmGetLengthResetMode.eNoAdjust, new GetLengthTarget(true).StartPos(seq, 0, 0));

				foreach (GetLengthType l in lengths)
					subSongs.push_back(new SubSong_Data(l.Duration, (int32_t)l.StartRow, l.StartOrder, seq, (int32_t)l.RestartRow, l.RestartOrder));
			}

			return subSongs;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected void Init_SubSongs(out SubSongs_Type subSongs)//XX 460
		{
			subSongs = Get_SubSongs();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected bool Has_SubSongs_Inited()//XX 463
		{
			return !m_SubSongs.empty();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected void Load(FileCursor file, Guid? formatId, map<string, string> ctls)//XX 487
		{
			{
				ModLoadingFlags load_Flags = ModLoadingFlags.LoadCompleteModule;

				if (m_Ctl_Load_Skip_Samples)
					load_Flags &= ~ModLoadingFlags.LoadSampleData;

				if (m_Ctl_Load_Skip_Patterns)
					load_Flags &= ~ModLoadingFlags.LoadPatternData;

				if (m_Ctl_Load_Skip_Plugins)
					load_Flags &= ~(ModLoadingFlags.LoadPluginData | ModLoadingFlags.LoadPluginInstance);

				if (!m_SndFile.Create(new FileReader(file), load_Flags, formatId))
					throw new OpenMptException("Error loading file");

				if (!m_Ctl_Load_Skip_SubSongs_Init)
					Init_SubSongs(out m_SubSongs);

				m_Loaded = true;
			}

			// Init CSoundFile state that correspond to ctls
			foreach (var ctl in ctls)
				Ctl_Set(ctl.first, ctl.second, false);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected bool Is_Loaded()//XX 520
		{
			return m_Loaded;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected size_t Read_Wrapper(size_t count, CPointer<int16_t> left, CPointer<int16_t> right, CPointer<int16_t> rear_Left, CPointer<int16_t> rear_Right)//XX 523
		{
			m_SndFile.ResetMixStat();
			m_SndFile.m_bIsRendering = m_Ctl_Play_At_End != Song_End_Action.Fadeout_Song;
			size_t count_Read = 0;
			CPointer<int16_t>[] buffers = [ left, right, rear_Left, rear_Right ];
			AudioTargetBufferWithGain<Audio_Span_Planar<int16_t>, int16_t, ConvertFixedPointInt16<MixFractionalBits>> target = new AudioTargetBufferWithGain<Audio_Span_Planar<int16_t>, int16_t, ConvertFixedPointInt16<MixFractionalBits>>(new Audio_Span_Planar<int16_t>(buffers, Valid_Channels(buffers, (size_t)buffers.Length), count), m_Dithers, m_Gain);

			while (count > 0)
			{
				size_t count_Chunk = m_SndFile.Read((samplecount_t)Math.Min(count, (uint64_t)samplecount_t.MaxValue / 2 / 4 / 4), target);	// Safety margin / sampleSize / channels

				if (count_Chunk == 0)
					break;

				count -= count_Chunk;
				count_Read += count_Chunk;
			}

			if ((count_Read == 0) && (m_Ctl_Play_At_End == Song_End_Action.Continue_Song))
			{
				// This is the song end, but allow the song or loop to restart on the next call
				m_SndFile.m_PlayState.m_Flags.Reset(PlayFlags.Song_EndReached);
			}

			return count_Read;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static Probe_File_Header_Result Probe_File_Header(Probe_File_Header_Flags flags, Stream stream, out Probe_File_Header_Info info)//XX 801
		{
			Probe_File_Header_Result result = Probe_File_Header_Result.Failure;

			bool seekable = FileDataStdStream.IsSeekable(stream);
			uint64_t fileSize = (seekable ? FileDataStdStream.GetLength(stream) : 0);

			// TNE: Removed reading into memory and changed the Probe method to take the stream directly instead

			switch (CSoundFile.Probe((ProbeFlags)flags, stream, seekable ? fileSize : null, out info))
			{
				case ProbeResult.Success:
				{
					result = Probe_File_Header_Result.Success;
					break;
				}

				case ProbeResult.Failure:
				{
					result = Probe_File_Header_Result.Failure;
					break;
				}

				case ProbeResult.WantMoreData:
				{
					result = Probe_File_Header_Result.WantMoreData;
					break;
				}

				default:
					throw new OpenMptException("Internal error");
			}

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Set_Render_Param(Render_Param @param, int32_t value)//XX 958
		{
			switch (param)
			{
				case Render_Param.MasterGain_MilliBel:
				{
					m_Gain = CMath.pow(10.0f, value * 0.001f * 0.5f);
					break;
				}

				case Render_Param.StereoSeparation_Percent:
				{
					int32_t newValue = value * MixerSettings.StereoSeparationScale / 100;

					if (newValue != m_SndFile.m_MixerSettings.m_nStereoSeparation)
					{
						MixerSettings settings = m_SndFile.m_MixerSettings;
						settings.m_nStereoSeparation = newValue;
						m_SndFile.SetMixerSettings(ref settings);
					}

					break;
				}

				case Render_Param.InterpolationFilter_Length:
				{
					CResamplerSettings newSettings = m_SndFile.m_Resampler.m_Settings;
					newSettings.SrcMode = FilterLength_To_ResamplingMode(value);

					if (newSettings != m_SndFile.m_Resampler.m_Settings)
						m_SndFile.SetResamplerSettings(ref newSettings);

					break;
				}

				case Render_Param.VolumeRamping_Strength:
				{
					MixerSettings newSettings = m_SndFile.m_MixerSettings;
					Ramping_To_MixerSettings(ref newSettings, value);

					if ((m_SndFile.m_MixerSettings.VolumeRampUpMicroseconds != newSettings.VolumeRampUpMicroseconds) || (m_SndFile.m_MixerSettings.VolumeRampDownMicroseconds != newSettings.VolumeRampDownMicroseconds))
						m_SndFile.SetMixerSettings(ref newSettings);

					break;
				}

				default:
					throw new OpenMptException("Unknown render param");
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public size_t Read(int32_t sampleRate, size_t count, CPointer<int16_t> left, CPointer<int16_t> right)//XX 998
		{
			if (left.IsNull || right.IsNull)
				throw new OpenMptException("null pointer");

			Apply_Mixer_Settings(sampleRate, 2);
			count = Read_Wrapper(count, left, right, null, null);

			m_CurrentPositionSeconds += (c_double)count / sampleRate;

			return count;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public size_t Read(int32_t sampleRate, size_t count, CPointer<int16_t> left, CPointer<int16_t> right, CPointer<int16_t> rear_Left, CPointer<int16_t> rear_Right)//XX 1007
		{
			if (left.IsNull || right.IsNull || rear_Left.IsNull || rear_Right.IsNull)
				throw new OpenMptException("null pointer");

			Apply_Mixer_Settings(sampleRate, 4);
			count = Read_Wrapper(count, left, right, rear_Left, rear_Right);

			m_CurrentPositionSeconds += (c_double)count / sampleRate;

			return count;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_double Get_Duration_Seconds()//XX 1081
		{
			SubSongs_Type subSongs_Temp = Has_SubSongs_Inited() ? new SubSongs_Type() : Get_SubSongs();
			SubSongs_Type subSongs = Has_SubSongs_Inited() ? m_SubSongs : subSongs_Temp;

			if (m_Current_SubSong == All_SubSongs)
			{
				// Play all subsongs consecutively
				c_double total_Duration = 0.0;

				foreach (SubSong_Data subSong in subSongs)
					total_Duration += subSong.Duration;

				return total_Duration;
			}

			return subSongs[m_Current_SubSong].Duration;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_double Get_Time_At_Position(int32_t order, int32_t row)//XX 1095
		{
			GetLengthType t = m_SndFile.GetLength(EnmGetLengthResetMode.eNoAdjust, new GetLengthTarget((OrderIndex)order, (RowIndex)row)).back();

			if (t.TargetReached)
				return t.Duration;
			else
				return -1.0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Select_SubSong(int32_t subSong)//XX 1103
		{
			SubSongs_Type subSongs_Temp = Has_SubSongs_Inited() ? new SubSongs_Type() : Get_SubSongs();
			SubSongs_Type subSongs = Has_SubSongs_Inited() ? m_SubSongs : subSongs_Temp;

			if ((subSong != All_SubSongs) && ((subSong < 0) || (subSong >= (int32_t)subSongs.size())))
				throw new OpenMptException("Invalid subsong");

			m_Current_SubSong = subSong;
			m_SndFile.m_SongFlags.Set(SongFlags.PlayAllSongs, subSong == All_SubSongs);

			if (subSong == All_SubSongs)
				subSong = 0;

			m_SndFile.Order.SetSequence((SequenceIndex)subSongs[subSong].Sequence);
			Set_Position_Order_Row(subSongs[subSong].Start_Order, subSongs[subSong].Start_Row);

			m_CurrentPositionSeconds = 0.0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int32_t Get_Restart_Order(int32_t subSong)//XX 1122
		{
			SubSongs_Type subSongs_Temp = Has_SubSongs_Inited() ? new SubSongs_Type() : Get_SubSongs();
			SubSongs_Type subSongs = Has_SubSongs_Inited() ? m_SubSongs : subSongs_Temp;

			if ((subSong < 0) || (subSong >= (int32_t)subSongs.size()))
				throw new OpenMptException("Invalid subsong");

			return subSongs[subSong].Restart_Order;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int32_t Get_Restart_Row(int32_t subSong)//XX 1130
		{
			SubSongs_Type subSongs_Temp = Has_SubSongs_Inited() ? new SubSongs_Type() : Get_SubSongs();
			SubSongs_Type subSongs = Has_SubSongs_Inited() ? m_SubSongs : subSongs_Temp;

			if ((subSong < 0) || (subSong >= (int32_t)subSongs.size()))
				throw new OpenMptException("Invalid subsong");

			return subSongs[subSong].Restart_Row;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_double Set_Position_Seconds(c_double seconds)//XX 1148
		{
			SubSongs_Type subSongs_Temp = Has_SubSongs_Inited() ? new SubSongs_Type() : Get_SubSongs();
			SubSongs_Type subSongs = Has_SubSongs_Inited() ? m_SubSongs : subSongs_Temp;

			SubSong_Data subSong = null;
			c_double base_Seconds = 0.0;

			if (m_Current_SubSong == All_SubSongs)
			{
				// When playing all subsongs, find out which subsong this time would belong to
				subSong = subSongs.back();

				for (size_t i = 0; i < subSongs.size(); ++i)
				{
					if ((base_Seconds + subSongs[i].Duration) > seconds)
					{
						subSong = subSongs[i];
						break;
					}

					base_Seconds += subSongs[i].Duration;
				}

				seconds -= base_Seconds;
			}
			else
				subSong = subSongs[m_Current_SubSong];

			m_SndFile.SetCurrentOrder((OrderIndex)subSong.Start_Order);
			GetLengthType t = m_SndFile.GetLength(m_Ctl_Seek_Sync_Samples ? EnmGetLengthResetMode.eAdjustSamplePositions : EnmGetLengthResetMode.eAdjust, new GetLengthTarget(seconds).StartPos((SequenceIndex)subSong.Sequence, (OrderIndex)subSong.Start_Order, (RowIndex)subSong.Start_Row)).back();
			m_SndFile.m_PlayState.m_nNextOrder = m_SndFile.m_PlayState.m_nCurrentOrder = t.TargetReached ? t.RestartOrder : t.EndOrder;
			m_SndFile.m_PlayState.m_nNextRow = t.TargetReached ? t.RestartRow : t.EndRow;
			m_SndFile.m_PlayState.m_nTickCount = CSoundFile.Ticks_Row_Finished;
			m_CurrentPositionSeconds = base_Seconds + t.Duration;

			return m_CurrentPositionSeconds;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_double Set_Position_Order_Row(int32_t order, int32_t row)//XX 1175
		{
			if ((order < 0) || (order >= m_SndFile.Order.Current.GetLengthTailTrimmed()))
				return m_CurrentPositionSeconds;

			PatternIndex pattern = m_SndFile.Order.Current[order];

			if (m_SndFile.Patterns.IsValidIndex(pattern))
			{
				if ((row < 0) || (row >= (int32_t)m_SndFile.Patterns[pattern].GetNumRows()))
					return m_CurrentPositionSeconds;
			}
			else
				row = 0;

			m_SndFile.m_PlayState.m_nCurrentOrder = (OrderIndex)order;
			m_SndFile.SetCurrentOrder((OrderIndex)order);
			m_SndFile.m_PlayState.m_nNextRow = (RowIndex)row;
			m_SndFile.m_PlayState.m_nTickCount = CSoundFile.Ticks_Row_Finished;

			m_CurrentPositionSeconds = m_SndFile.GetLength(m_Ctl_Seek_Sync_Samples ? EnmGetLengthResetMode.eAdjustSamplePositions : EnmGetLengthResetMode.eAdjust, new GetLengthTarget((OrderIndex)order, (RowIndex)row)).back().Duration;

			return m_CurrentPositionSeconds;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public StdString Get_Message_Instruments()//XX 1212
		{
			StdString retVal = new StdString();
			StdString tmp = new StdString();
			bool valid = false;

			for (InstrumentIndex i = 1; i <= m_SndFile.GetNumInstruments(); ++i)
			{
				StdString instName = m_SndFile.GetInstrumentName(i);

				if (!instName.empty())
					valid = true;

				tmp += instName;
				tmp += "\n";
			}

			if (valid)
				retVal = tmp;

			return retVal;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public StdString Get_Message_Samples()//XX 1229
		{
			StdString retVal = new StdString();
			StdString tmp = new StdString();
			bool valid = false;

			for (SampleIndex i = 1; i <= m_SndFile.GetNumSamples(); ++i)
			{
				StdString sampleName = m_SndFile.GetSampleName(i);

				if (!sampleName.empty())
					valid = true;

				tmp += sampleName;
				tmp += "\n";
			}

			if (valid)
				retVal = tmp;

			return retVal;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public string Get_Metadata(string key)//XX 1246
		{
			if (key == "type")
				return m_SndFile.m_ModFormat.Type;
			else if (key == "type_long")
				return m_SndFile.m_ModFormat.FormatName;
			else if (key == "originaltype")
				return m_SndFile.m_ModFormat.OriginalType;
			else if (key == "originaltype_long")
				return m_SndFile.m_ModFormat.OriginalFormatName;
			else if (key == "container")
				return Tables.ModContainerTypeToString(m_SndFile.GetContainerType());
			else if (key == "container_long")
				return Tables.ModContainerTypeToTracker(m_SndFile.GetContainerType());
			else if (key == "tracker")
				return m_SndFile.m_ModFormat.MadeWithTracker;
			else if (key == "artist")
				return m_SndFile.m_SongArtist;
			else if (key == "title")
				return m_SndFile.m_ModFormat.CharSet.GetString(m_SndFile.GetTitle().data().AsSpan());
			else if (key == "date")
			{
				if (m_SndFile.GetFileHistory().empty() || !m_SndFile.GetFileHistory().back().HasValidDate())
					return string.Empty;

				return m_SndFile.GetFileHistory().back().AsIso8601(m_SndFile.GetTimezoneInternal());
			}
			else if (key == "message")
			{
				StdString retVal = m_SndFile.m_SongMessage.GetFormatted(SongMessage.LineEnding.leLf);

				if (retVal.empty())
				{
					switch (m_SndFile.GetMessageHeuristic())
					{
						case ModMessageHeuristicOrder.Instruments:
						{
							retVal = Get_Message_Instruments();
							break;
						}

						case ModMessageHeuristicOrder.Samples:
						{
							retVal = Get_Message_Samples();
							break;
						}

						case ModMessageHeuristicOrder.InstrumentsSamples:
						{
							if (retVal.empty())
								retVal = Get_Message_Instruments();

							if (retVal.empty())
								retVal = Get_Message_Samples();

							break;
						}

						case ModMessageHeuristicOrder.SamplesInstruments:
						{
							if (retVal.empty())
								retVal = Get_Message_Samples();

							if (retVal.empty())
								retVal = Get_Message_Instruments();

							break;
						}

						case ModMessageHeuristicOrder.BothInstrumentsSamples:
						{
							StdString message_Instruments = Get_Message_Instruments();
							StdString message_Samples = Get_Message_Samples();

							if (!message_Instruments.empty())
								retVal += Utility.move(message_Instruments);

							if (!message_Samples.empty())
								retVal += Utility.move(message_Samples);

							break;
						}

						case ModMessageHeuristicOrder.BothSamplesInstruments:
						{
							StdString message_Instruments = Get_Message_Instruments();
							StdString message_Samples = Get_Message_Samples();

							if (!message_Samples.empty())
								retVal += Utility.move(message_Samples);

							if (!message_Instruments.empty())
								retVal += Utility.move(message_Instruments);

							break;
						}
					}
				}

				return m_SndFile.m_ModFormat.CharSet.GetString(retVal.data().AsSpan());
			}
			else if (key == "message_raw")
			{
				StdString retVal = m_SndFile.m_SongMessage.GetFormatted(SongMessage.LineEnding.leLf);

				return m_SndFile.m_ModFormat.CharSet.GetString(retVal.data().AsSpan());
			}
			else if (key == "warnings")
				throw new NotImplementedException("warnings");

			return string.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_double Get_Current_Estimated_Bpm()//XX 1342
		{
			return m_SndFile.GetCurrentBpm();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int32_t Get_Current_Speed()//XX 1345
		{
			return (int32_t)m_SndFile.m_PlayState.m_nMusicSpeed;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_double Get_Current_Tempo2()//XX 1351
		{
			return m_SndFile.m_PlayState.m_nMusicTempo.ToDouble();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int32_t Get_Current_Order()//XX 1354
		{
			return m_SndFile.GetCurrentOrder();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int32_t Get_Current_Pattern()//XX 1357
		{
			int32_t order = m_SndFile.GetCurrentOrder();

			if ((order < 0) || (order >= m_SndFile.Order.Current.GetLengthTailTrimmed()))
				return m_SndFile.GetCurrentPattern();

			int32_t pattern = m_SndFile.Order.Current[order];

			if (!m_SndFile.Patterns.IsValidIndex((PatternIndex)pattern))
				return -1;

			return pattern;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int32_t Get_Num_SubSongs()//XX 1408
		{
			SubSongs_Type subSongs_Temp = Has_SubSongs_Inited() ? new SubSongs_Type() : Get_SubSongs();
			SubSongs_Type subSongs = Has_SubSongs_Inited() ? m_SubSongs : subSongs_Temp;

			return (int32_t)subSongs.size();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int32_t Get_Num_Channels()//XX 1413
		{
			return m_SndFile.GetNumChannels();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int32_t Get_Num_Orders()//XX 1416
		{
			return m_SndFile.Order.Current.GetLengthTailTrimmed();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int32_t Get_Num_Patterns()//XX 1419
		{
			return m_SndFile.Patterns.GetNumPatterns();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int32_t Get_Num_Instruments()//XX 1422
		{
			return m_SndFile.GetNumInstruments();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int32_t Get_Num_Samples()//XX 1425
		{
			return m_SndFile.GetNumSamples();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public vector<string> Get_SubSong_Names()//XX 1429
		{
			vector<string> retVal = new vector<string>();

			SubSongs_Type subSongs_Temp = Has_SubSongs_Inited() ? new SubSongs_Type() : Get_SubSongs();
			SubSongs_Type subSongs = Has_SubSongs_Inited() ? m_SubSongs : subSongs_Temp;
			retVal.reserve(subSongs.size());

			foreach (SubSong_Data subSong in subSongs)
			{
				ModSequence order = m_SndFile.Order[(SequenceIndex)subSong.Sequence];
				retVal.push_back(order.GetName());

				if (string.IsNullOrEmpty(retVal.back()))
				{
					// Use first pattern name instead
					if (order.IsValidPat((OrderIndex)subSong.Start_Order))
						retVal.back() = m_SndFile.GetCharsetInternal().GetString(m_SndFile.Patterns[order[subSong.Start_Order]].GetName().data().AsSpan());
				}
			}

			return retVal;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public pair<CPointer<Ctl_Info>, CPointer<Ctl_Info>> Get_Ctl_Infos()//XX 1720
		{
			return pair.make_pair(Iterator.begin(ctl_Infos), Iterator.end(ctl_Infos));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Ctl_Set(string ctl, string value, bool throw_If_Unknown = true)//XX 1977
		{
			if (!string.IsNullOrEmpty(ctl))
			{
				char rightMost = ctl[^1];

				if ((rightMost == '!') || (rightMost == '?'))
				{
					if (rightMost == '!')
						throw_If_Unknown = true;
					else if (rightMost == '?')
						throw_If_Unknown = false;

					ctl = ctl.Substring(0, ctl.Length - 1);
				}
			}

			CPointer<Ctl_Info> found_Ctl = Algorithm.find_if(Get_Ctl_Infos().first, Get_Ctl_Infos().second, (Ctl_Info info) => info.Name == ctl);

			if (found_Ctl == Get_Ctl_Infos().second)
			{
				if (string.IsNullOrEmpty(ctl))
					throw new OpenMptException("Empty ctl: := " + value);
				else if (throw_If_Unknown)
					throw new OpenMptException("Unknown ctl: " + ctl + " := " + value);
				else
					return;
			}

			switch (found_Ctl[0].Type)
			{
				case Ctl_Type.Boolean:
				{
					Ctl_Set_Boolean(ctl, Parse_.Parse<bool>(value), throw_If_Unknown);
					break;
				}

				case Ctl_Type.Integer:
				{
					Ctl_Set_Integer(ctl, Parse_.Parse<int64_t>(value), throw_If_Unknown);
					break;
				}

				case Ctl_Type.FloatingPoint:
				{
					Ctl_Set_FloatingPoint(ctl, Parse_.Parse<c_double>(value), throw_If_Unknown);
					break;
				}

				case Ctl_Type.Text:
				{
					Ctl_Set_Text(ctl, value, throw_If_Unknown);
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Ctl_Set_Boolean(string ctl, bool value, bool throw_If_Unknown = true)//XX 2016
		{
			if (!string.IsNullOrEmpty(ctl))
			{
				char rightMost = ctl[^1];

				if ((rightMost == '!') || (rightMost == '?'))
				{
					if (rightMost == '!')
						throw_If_Unknown = true;
					else if (rightMost == '?')
						throw_If_Unknown = false;

					ctl = ctl.Substring(0, ctl.Length - 1);
				}
			}

			CPointer<Ctl_Info> found_Ctl = Algorithm.find_if(Get_Ctl_Infos().first, Get_Ctl_Infos().second, (Ctl_Info info) => info.Name == ctl);

			if (found_Ctl == Get_Ctl_Infos().second)
			{
				if (string.IsNullOrEmpty(ctl))
					throw new OpenMptException("Empty ctl: := " + value);
				else if (throw_If_Unknown)
					throw new OpenMptException("Unknown ctl: " + ctl + " := " + value);
				else
					return;
			}

			if (string.IsNullOrEmpty(ctl))
				throw new OpenMptException("Empty ctl: := " + value);
			else if ((ctl == "load.skip_samples") || (ctl == "load_skip_samples"))
				m_Ctl_Load_Skip_Samples = value;
			else if ((ctl == "load.skip_patterns") || (ctl == "load_skip_patterns"))
				m_Ctl_Load_Skip_Patterns = value;
			else if (ctl == "load.skip_plugins")
				m_Ctl_Load_Skip_Plugins = value;
			else if (ctl == "load.skip_subsongs_init")
				m_Ctl_Load_Skip_SubSongs_Init = value;
			else if (ctl == "seek.sync_samples")
				m_Ctl_Seek_Sync_Samples = value;
			else if (ctl == "render.resampler.emulate_amiga")
			{
				CResamplerSettings newSettings = m_SndFile.m_Resampler.m_Settings;
				bool enabled = value;

				if (enabled)
					newSettings.EmulateAmiga = Translate_Amiga_Filter_Type(m_Ctl_Render_Resampler_Emulate_Amiga_Type);
				else
					newSettings.EmulateAmiga = Resampling.AmigaFilter.Off;

				if (newSettings != m_SndFile.m_Resampler.m_Settings)
					m_SndFile.SetResamplerSettings(ref newSettings);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Ctl_Set_Integer(string ctl, int64_t value, bool throw_If_Unknown = true)//XX 2066
		{
			if (!string.IsNullOrEmpty(ctl))
			{
				char rightMost = ctl[^1];

				if ((rightMost == '!') || (rightMost == '?'))
				{
					if (rightMost == '!')
						throw_If_Unknown = true;
					else if (rightMost == '?')
						throw_If_Unknown = false;

					ctl = ctl.Substring(0, ctl.Length - 1);
				}
			}

			CPointer<Ctl_Info> found_Ctl = Algorithm.find_if(Get_Ctl_Infos().first, Get_Ctl_Infos().second, (Ctl_Info info) => info.Name == ctl);

			if (found_Ctl == Get_Ctl_Infos().second)
			{
				if (string.IsNullOrEmpty(ctl))
					throw new OpenMptException("Empty ctl: := " + value);
				else if (throw_If_Unknown)
					throw new OpenMptException("Unknown ctl: " + ctl + " := " + value);
				else
					return;
			}

			if (string.IsNullOrEmpty(ctl))
				throw new OpenMptException("Empty ctl: := " + value);
			else if (ctl == "subsong")
				Select_SubSong(int32_t.CreateSaturating(value));
			else if (ctl == "dither")
			{
				size_t dither = size_t.CreateSaturating(value);

				if (dither >= DithersWrapperOpenMpt.GetNumDithers())
					dither = DithersWrapperOpenMpt.GetDefaultDither();

				m_Dithers.SetMode(dither);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Ctl_Set_FloatingPoint(string ctl, c_double value, bool throw_If_Unknown = true)//XX 2105
		{
			if (!string.IsNullOrEmpty(ctl))
			{
				char rightMost = ctl[^1];

				if ((rightMost == '!') || (rightMost == '?'))
				{
					if (rightMost == '!')
						throw_If_Unknown = true;
					else if (rightMost == '?')
						throw_If_Unknown = false;

					ctl = ctl.Substring(0, ctl.Length - 1);
				}
			}

			CPointer<Ctl_Info> found_Ctl = Algorithm.find_if(Get_Ctl_Infos().first, Get_Ctl_Infos().second, (Ctl_Info info) => info.Name == ctl);

			if (found_Ctl == Get_Ctl_Infos().second)
			{
				if (string.IsNullOrEmpty(ctl))
					throw new OpenMptException("Empty ctl: := " + value);
				else if (throw_If_Unknown)
					throw new OpenMptException("Unknown ctl: " + ctl + " := " + value);
				else
					return;
			}

			if (string.IsNullOrEmpty(ctl))
				throw new OpenMptException("Empty ctl: := " + value);
			else if (ctl == "play.tempo_factor")
			{
				if (!Is_Loaded())
					return;

				c_double factor = value;

				if ((factor <= 0.0) || (factor > 4.0))
					throw new OpenMptException("Invalid tempo factor");

				m_SndFile.m_nTempoFactor = SaturateRound.Saturate_Round<uint32_t, c_double>(65536.0 / factor);
				m_SndFile.RecalculateSamplesPerTick();
			}
			else if (ctl == "play.pitch_factor")
			{
				if (!Is_Loaded())
					return;

				c_double factor = value;

				if ((factor <= 0.0) || (factor > 4.0))
					throw new OpenMptException("Invalid pitch factor");

				m_SndFile.m_nFreqFactor = SaturateRound.Saturate_Round<uint32_t, c_double>(65536.0 * factor);
				m_SndFile.RecalculateSamplesPerTick();
			}
			else if (ctl == "render.opl.volume_factor")
				m_SndFile.m_OplVolumeFactor = (uint32)SaturateRound.Saturate_Round<int32_t, c_double>(value * CSoundFile.m_OplVolumeFactorScale);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Ctl_Set_Text(string ctl, string value, bool throw_If_Unknown = true)//XX 2158
		{
			if (!string.IsNullOrEmpty(ctl))
			{
				char rightMost = ctl[^1];

				if ((rightMost == '!') || (rightMost == '?'))
				{
					if (rightMost == '!')
						throw_If_Unknown = true;
					else if (rightMost == '?')
						throw_If_Unknown = false;

					ctl = ctl.Substring(0, ctl.Length - 1);
				}
			}

			CPointer<Ctl_Info> found_Ctl = Algorithm.find_if(Get_Ctl_Infos().first, Get_Ctl_Infos().second, (Ctl_Info info) => info.Name == ctl);

			if (found_Ctl == Get_Ctl_Infos().second)
			{
				if (string.IsNullOrEmpty(ctl))
					throw new OpenMptException("Empty ctl: := " + value);
				else if (throw_If_Unknown)
					throw new OpenMptException("Unknown ctl: " + ctl + " := " + value);
				else
					return;
			}

			if (string.IsNullOrEmpty(ctl))
				throw new OpenMptException("Empty ctl: := " + value);
			else if (ctl == "play.at_end")
			{
				if (value == "fadeout")
					m_Ctl_Play_At_End = Song_End_Action.Fadeout_Song;
				else if (value == "continue")
					m_Ctl_Play_At_End = Song_End_Action.Continue_Song;
				else if (value == "stop")
					m_Ctl_Play_At_End = Song_End_Action.Stop_Song;
				else
					throw new OpenMptException("Unknown song end action: " + value);
			}
			else if (ctl == "render.resampler.emulate_amiga_type")
			{
				if (value == "a500")
					m_Ctl_Render_Resampler_Emulate_Amiga_Type = Amiga_Filter_Type.A500;
				else if (value == "a1200")
					m_Ctl_Render_Resampler_Emulate_Amiga_Type = Amiga_Filter_Type.A1200;
				else if (value == "unfiltered")
					m_Ctl_Render_Resampler_Emulate_Amiga_Type = Amiga_Filter_Type.Unfiltered;
				else if (value == "auto")
					m_Ctl_Render_Resampler_Emulate_Amiga_Type = Amiga_Filter_Type.Auto_Filter;
				else
					throw new OpenMptException("Invalid Amiga filter type: " + value);

				if (m_SndFile.m_Resampler.m_Settings.EmulateAmiga != Resampling.AmigaFilter.Off)
				{
					CResamplerSettings newSettings = m_SndFile.m_Resampler.m_Settings;
					newSettings.EmulateAmiga = Translate_Amiga_Filter_Type(m_Ctl_Render_Resampler_Emulate_Amiga_Type);

					if (newSettings != m_SndFile.m_Resampler.m_Settings)
						m_SndFile.SetResamplerSettings(ref newSettings);
				}
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private ResamplingMode FilterLength_To_ResamplingMode(int32_t length)//XX 334
		{
			ResamplingMode result = ResamplingMode.Sinc8Lp;

			if (length == 0)
				result = ResamplingMode.Sinc8Lp;
			else if (length >= 8)
				result = ResamplingMode.Sinc8Lp;
			else if (length >= 3)
				result = ResamplingMode.Cubic;
			else if (length >= 2)
				result = ResamplingMode.Linear;
			else if (length >= 1)
				result = ResamplingMode.Nearest;
			else
				throw new OpenMptException("Negative filter length");

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private size_t Valid_Channels<TSampleType>(CPointer<TSampleType>[] buffers, size_t max_Channels)//XX 373
		{
			size_t channel;

			for (channel = 0; channel < max_Channels; ++channel)
			{
				if (buffers[channel].IsNull)
					break;
			}

			return channel;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Resampling.AmigaFilter Translate_Amiga_Filter_Type(Amiga_Filter_Type amiga_Type)//XX 383
		{
			switch (amiga_Type)
			{
				case Amiga_Filter_Type.A500:
					return Resampling.AmigaFilter.A500;

				case Amiga_Filter_Type.A1200:
				case Amiga_Filter_Type.Auto_Filter:
				default:
					return Resampling.AmigaFilter.A1200;

				case Amiga_Filter_Type.Unfiltered:
					return Resampling.AmigaFilter.Unfiltered;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Ramping_To_MixerSettings(ref MixerSettings settings, c_int ramping)//XX 396
		{
			if (ramping == -1)
			{
				settings.SetVolumeRampUpMicroseconds(new MixerSettings().GetVolumeRampUpMicroseconds());
				settings.SetVolumeRampDownMicroseconds(new MixerSettings().GetVolumeRampDownMicroseconds());
			}
			else if (ramping <= 0)
			{
				settings.SetVolumeRampUpMicroseconds(0);
				settings.SetVolumeRampDownMicroseconds(0);
			}
			else
			{
				settings.SetVolumeRampUpMicroseconds(ramping * 1000);
				settings.SetVolumeRampDownMicroseconds(ramping * 1000);
			}
		}
		#endregion
	}
}
