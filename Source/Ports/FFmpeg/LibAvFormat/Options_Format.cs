/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat
{
	/// <summary>
	/// Options definition for AVFormatContext
	/// </summary>
	public static class Options_Format
	{
		private static readonly AvClass av_Format_Context_Class = new AvClass//XX 128
		{
			Class_Name = "AVFormatContext".ToCharPointer(),
			Item_Name = Format_To_Name,
			Option = Options_Table.AvFormat_Options,
			Version = Version.Version_Int,
			Child_Next = Format_Next_Child,
			Child_Class_Iterate = Format_Child_Class_Iterate,
			Category = AvClassCategory.Muxer,
			Get_Category = Get_Category
		};

		private static readonly AvOption[] stream_Options =
		[
			// Disposition Opt
			new AvOption("disposition", null, nameof(AvStream.Disposition), AvOptionType.Flags, new AvOption.DefaultValueUnion { I64 = (int64_t)AvDisposition.None }, AvOptFlag.Encoding_Param, "disposition"),
			new AvOption("default", AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvDisposition.Default }, "disposition"),
			new AvOption("dub", AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvDisposition.Dub }, "disposition"),
			new AvOption("original", AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvDisposition.Original }, "disposition"),
			new AvOption("comment", AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvDisposition.Comment }, "disposition"),
			new AvOption("lyrics", AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvDisposition.Lyrics }, "disposition"),
			new AvOption("karaoke", AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvDisposition.Karaoke }, "disposition"),
			new AvOption("forced", AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvDisposition.Forced }, "disposition"),
			new AvOption("hearing_impaired", AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvDisposition.Hearing_Impaired }, "disposition"),
			new AvOption("visual_impaired", AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvDisposition.Visual_Impaired }, "disposition"),
			new AvOption("clean_effects", AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvDisposition.Clean_Effects }, "disposition"),
			new AvOption("attached_pic", AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvDisposition.Attached_Pic }, "disposition"),
			new AvOption("timed_thumbnails", AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvDisposition.Timed_Thumbnails }, "disposition"),
			new AvOption("non_diegetic", AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvDisposition.Non_Diegetic }, "disposition"),
			new AvOption("captions", AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvDisposition.Captions }, "disposition"),
			new AvOption("descriptions", AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvDisposition.Descriptions }, "disposition"),
			new AvOption("metadata", AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvDisposition.Metadata }, "disposition"),
			new AvOption("dependent", AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvDisposition.Dependent }, "disposition"),
			new AvOption("still_image", AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvDisposition.Still_Image }, "disposition"),
			new AvOption("multilayer", AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvDisposition.Multilayer }, "disposition"),

			new AvOption("discard", null, nameof(AvStream.Discard), AvOptionType.Int, new AvOption.DefaultValueUnion { I64 = (int64_t)AvDiscard.Default }, c_int.MinValue, c_int.MaxValue, AvOptFlag.Decoding_Param, "avdiscard"),
			new AvOption("none", AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvDiscard.None }, "avdiscard"),
			new AvOption("default", AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvDiscard.Default }, "avdiscard"),
			new AvOption("noref", AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvDiscard.NonRef }, "avdiscard"),
			new AvOption("bidir", AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvDiscard.Bidir }, "avdiscard"),
			new AvOption("nointra", AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvDiscard.NonIntra }, "avdiscard"),
			new AvOption("nokey", AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvDiscard.NonKey }, "avdiscard"),
			new AvOption("all", AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvDiscard.All }, "avdiscard"),
			new AvOption()
		];

		private static readonly AvClass stream_Class = new AvClass()//XX 232
		{
			Class_Name = "AVStream".ToCharPointer(),
			Item_Name = Log.Av_Default_Item_Name,
			Version = Version.Version_Int,
			Option = stream_Options
		};

		/********************************************************************/
		/// <summary>
		/// Allocate an AVFormatContext
		/// </summary>
		/********************************************************************/
		public static AvFormatContext AvFormat_Alloc_Context()//XX 162
		{
			FormatContextInternal fci = Mem.Av_MAlloczObj<FormatContextInternal>();
			if (fci == null)
				return null;

			FFFormatContext si = fci.Fc;
			AvFormatContext s = si.Pub;
			av_Format_Context_Class.CopyTo(s.Av_Class);
			s.Io_Open = Io_Open_Default;
			s.Io_Close2 = Io_Close2_Default;

			Opt.Av_Opt_Set_Defaults(s);

			si.Pkt = Packet.Av_Packet_Alloc();
			si.Parse_Pkt = Packet.Av_Packet_Alloc();

			if ((si.Pkt == null) || (si.Parse_Pkt == null))
			{
				AvFormat.AvFormat_Free_Context(s);

				return null;
			}

			return s;
		}



		/********************************************************************/
		/// <summary>
		/// Add a new stream to a media file.
		///
		/// When demuxing, it is called by the demuxer in read_header(). If
		/// the flag AVFMTCTX_NOHEADER is set in s.ctx_flags, then it may
		/// also be called in read_packet().
		///
		/// When muxing, should be called by the user before
		/// avformat_write_header().
		/// User is required to call avformat_free_context() to clean up the
		/// allocation by avformat_new_stream()
		/// </summary>
		/********************************************************************/
		public static AvStream AvFormat_New_Stream(AvFormatContext s, AvCodec c)//XX 244
		{
			if (s.Nb_Streams >= s.Max_Streams)
			{
				Log.Av_Log(s, Log.Av_Log_Error, "Number of streams exceeds max_streams parameter (%d), see the documentation if you wish to increase it\n", s.Max_Streams);

				return null;
			}

			CPointer<AvStream> streams = Mem.Av_Realloc_ArrayObj(s.Streams, s.Nb_Streams + 1);

			if (streams.IsNull)
				return null;

			s.Streams = streams;

			FFStream sti = Mem.Av_MAlloczObj<FFStream>();

			if (sti == null)
				return null;

			AvStream st = sti.Pub;

			stream_Class.CopyTo(st.Av_Class);
			st.CodecPar = Codec_Par.AvCodec_Parameters_Alloc();

			if (st.CodecPar == null)
				goto Fail;

			sti.FmtCtx = s;

			if (s.IFormat != null)
			{
				sti.AvCtx = Options_Codec.AvCodec_Alloc_Context3(null);

				if (sti.AvCtx == null)
					goto Fail;

				sti.Info = Mem.Av_MAlloczObj<FFStreamInfo>();

				if (sti.Info == null)
					goto Fail;

				sti.Info.Fps_First_Dts = UtilConstants.Av_NoPts_Value;
				sti.Info.Fps_Last_Dts = UtilConstants.Av_NoPts_Value;

				// Default pts setting is MPEG-like
				AvFormat.AvPriv_Set_Pts_Info(st, 33, 1, 90000);

				// We set the current DTS to 0 so that formats without any timestamps
				// but durations get some timestamps, formats with some unknown
				// timestamps have their first few packets buffered and the
				// timestamps corrected before they are returned to the user
				sti.Cur_Dts = AvFormatInternal.Relative_Ts_Base;
			}
			else
				sti.Cur_Dts = UtilConstants.Av_NoPts_Value;

			st.Index = (c_int)s.Nb_Streams;
			st.Start_Time = UtilConstants.Av_NoPts_Value;
			st.Duration = UtilConstants.Av_NoPts_Value;
			sti.First_Dts = UtilConstants.Av_NoPts_Value;
			sti.Probe_Packets = s.Max_Probe_Packets;
			sti.Pts_Wrap_Reference = UtilConstants.Av_NoPts_Value;
			sti.Pts_Wrap_Behavior = AvPtsWrap.Ignore;

			sti.Last_IP_Pts = UtilConstants.Av_NoPts_Value;
			sti.Last_Dts_For_Order_Check = UtilConstants.Av_NoPts_Value;

			for (c_int i = 0; i < (FFStream.Max_Reorder_Delay + 1); i++)
				sti.Pts_Buffer[i] = UtilConstants.Av_NoPts_Value;

			st.Sample_Aspects_Ratio = new AvRational(0, 1);

			sti.Need_Context_Update = 1;

			s.Streams[s.Nb_Streams++] = st;

			return st;

			Fail:
			AvFormat.FF_Free_Stream(ref st);

			return null;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static CPointer<char> Format_To_Name(IClass ptr)//XX 45
		{
			AvFormatContext fc = (AvFormatContext)ptr;

			if (fc.IFormat != null)
				return fc.IFormat.Name;
			else if (fc.OFormat != null)
				return fc.OFormat.Name;
			else
				return fc.Av_Class.Class_Name;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static IEnumerable<IOptionContext> Format_Next_Child(IOptionContext obj)//XX 53
		{
			AvFormatContext s = (AvFormatContext)obj;

			if ((s.Priv_Data != null) &&
			    (((s.IFormat != null) && (s.IFormat.Priv_Class != null)) ||
				((s.OFormat != null) && (s.OFormat.Priv_Class != null))))
			{
				yield return s.Priv_Data;
			}

			if ((s.Pb != null) && (s.Pb.Av_Class != null))
				yield return s.Pb;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static IEnumerable<AvClass> Format_Child_Class_Iterate()//XX 75
		{
			// AvIO
			yield return AvIo.FF_AvIo_Class;

			// Mux
			foreach (AvOutputFormat ofmt in AllFormats.Av_Muxer_Iterate())
			{
				AvClass ret = ofmt.Priv_Class;
				if (ret != null)
					yield return ret;
			}

			// Demux
			foreach (AvInputFormat ifmt in AllFormats.Av_Demuxer_Iterate())
			{
				AvClass ret = ifmt.Priv_Class;
				if (ret != null)
					yield return ret;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static AvClassCategory Get_Category(IClass ptr)//XX 121
		{
			AvFormatContext s = (AvFormatContext)ptr;

			if (s.IFormat != null)
				return AvClassCategory.Demuxer;
			else
				return AvClassCategory.Muxer;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Io_Open_Default(AvFormatContext s, out AvIoContext pb, CPointer<char> url, AvIoFlag flags, ref AvDictionary options)//XX 139
		{
			return AvIo.FFIo_Open_Whitelist(out pb, url, flags, s.Interrupt_Callback, ref options, s.Protocol_Whitelist, s.Protocol_Blacklist);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Io_Close2_Default(AvFormatContext s, AvIoContext pb)//XX 157
		{
			return AvIo.AvIo_Close(pb);
		}
		#endregion
	}
}
