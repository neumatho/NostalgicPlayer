/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using System.Text;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Codec;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Codec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Buffer = Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Buffer;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat
{
	/// <summary>
	/// Core demuxing component
	/// </summary>
	public static class Demux
	{
		/// <summary>
		/// 
		/// </summary>
		public const c_int Max_Std_Timebases = (30 * 12) + 30 + 3 + 6;

		private const int64_t Duration_Default_Max_Read_Size = 250000;
		private const c_int Duration_Default_Max_Retry = 6;
		private const c_int Duration_Max_Retry = 1;

		private class Fmt_Id_Type
		{
			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public Fmt_Id_Type(string name, AvCodecId id, AvMediaType type)
			{
				Name = name.ToCharPointer();
				Id = id;
				Type = type;
			}

			/// <summary></summary>
			public CPointer<char> Name { get; }
			/// <summary></summary>
			public AvCodecId Id { get; }
			/// <summary></summary>
			public AvMediaType Type { get; }
		}

		private static readonly Fmt_Id_Type[] fmt_Id_Type =
		[
			new Fmt_Id_Type("aac", AvCodecId.Aac, AvMediaType.Audio),
			new Fmt_Id_Type("ac3", AvCodecId.Ac3, AvMediaType.Audio),
			new Fmt_Id_Type("aptx", AvCodecId.Aptx, AvMediaType.Audio),
			new Fmt_Id_Type("av1", AvCodecId.Av1, AvMediaType.Video),
			new Fmt_Id_Type("dts", AvCodecId.Dts, AvMediaType.Audio),
			new Fmt_Id_Type("dvbsub", AvCodecId.Dvb_Subtitle, AvMediaType.Subtitle),
			new Fmt_Id_Type("dvbtxt", AvCodecId.Dvb_TeleText, AvMediaType.Subtitle),
			new Fmt_Id_Type("eac3", AvCodecId.Eac3, AvMediaType.Audio),
			new Fmt_Id_Type("h264", AvCodecId.H264, AvMediaType.Video),
			new Fmt_Id_Type("hevc", AvCodecId.Hevc, AvMediaType.Video),
			new Fmt_Id_Type("loas", AvCodecId.Aac_Latm, AvMediaType.Audio),
			new Fmt_Id_Type("m4v", AvCodecId.Mpeg4, AvMediaType.Video),
			new Fmt_Id_Type("mjpeg_2000", AvCodecId.Jpeg2000, AvMediaType.Video),
			new Fmt_Id_Type("mp3", AvCodecId.Mp3, AvMediaType.Audio),
			new Fmt_Id_Type("mpegvideo", AvCodecId.Mpeg2Video, AvMediaType.Video),
			new Fmt_Id_Type("truehd", AvCodecId.TrueHd, AvMediaType.Audio),
			new Fmt_Id_Type("evc", AvCodecId.Evc, AvMediaType.Video),
			new Fmt_Id_Type("vvc", AvCodecId.Vvc, AvMediaType.Video)
		];

		private static readonly string[] duration_Name = [ "pts", "stream", "bit rate" ];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FFInputFormat FFIFmt(AvInputFormat fmt)
		{
			return (FFInputFormat)fmt;
		}



		/********************************************************************/
		/// <summary>
		/// Wrap a given time stamp, if there is an indication for an
		/// overflow
		/// </summary>
		/********************************************************************/
		internal static int64_t FF_Wrap_Timestamp(AvStream st, int64_t timestamp)//XX 68
		{
			return Wrap_Timestamp(st, timestamp);
		}



		/********************************************************************/
		/// <summary>
		/// Open an input stream and read the header. The codecs are not
		/// opened. The stream must be closed with avformat_close_input()
		/// </summary>
		/********************************************************************/
		public static c_int AvFormat_Open_Input(ref AvFormatContext ps, CPointer<char> fileName, AvInputFormat fmt, AvDictionary options)//XX 221
		{
			AvFormatContext s = ps;
			AvDictionary tmp = null;
			Id3v2ExtraMeta id3v2_Extra_Meta = null;
			c_int ret = 0;

			if (s == null)
			{
				s = Options_Format.AvFormat_Alloc_Context();
				if (s == null)
					return Error.ENOMEM;
			}

			FormatContextInternal fci = AvFormatInternal.FF_FC_Internal(s);
			FFFormatContext si = fci.Fc;

			if (s.Av_Class == null)
			{
				Log.Av_Log(null, Log.Av_Log_Error, "Input context has not been properly allocated by avformat_alloc_context() and is not NULL either\n");

				return Error.EINVAL;
			}

			if (fmt != null)
				s.IFormat = fmt;

			if (options != null)
				Dict.Av_Dict_Copy(ref tmp, options, AvDict.None);

			if (s.Pb != null)	// Must be before any goto fail
				s.Flags |= AvFmtFlag.Custom_Io;

			ret = Opt.Av_Opt_Set_Dict(s, ref tmp);

			if (ret < 0)
				goto Fail;

			s.Url = CString.Empty;

			ret = Init_Input(s, fileName, ref tmp);

			if (ret < 0)
				goto Fail;

			s.Probe_Score = ret;

			if (s.Protocol_Whitelist.IsNull && (s.Pb != null) && s.Pb.Protocol_Whitelist.IsNotNull)
			{
				s.Protocol_Whitelist = Mem.Av_StrDup(s.Pb.Protocol_Whitelist);

				if (s.Protocol_Whitelist.IsNull)
				{
					ret = Error.ENOMEM;
					goto Fail;
				}
			}

			if (s.Protocol_Blacklist.IsNull && (s.Pb != null) && s.Pb.Protocol_Blacklist.IsNotNull)
			{
				s.Protocol_Blacklist = Mem.Av_StrDup(s.Pb.Protocol_Blacklist);

				if (s.Protocol_Blacklist.IsNull)
				{
					ret = Error.ENOMEM;
					goto Fail;
				}
			}

			if (s.Format_Whitelist.IsNotNull && AvString.Av_Match_List(s.IFormat.Name, s.Format_Whitelist, ',') <= 0)
			{
				Log.Av_Log(s, Log.Av_Log_Error, "Format not on whitelist \'%s\'\n", s.Format_Whitelist);

				ret = Error.EINVAL;
				goto Fail;
			}

			AvIoBuf.AvIo_Skip(s.Pb, s.Skip_Initial_Bytes);

			// Check filename in case an image number is expected
			if ((s.IFormat.Flags & AvFmt.NeedNumber) != 0)
			{
				ret = Error.EINVAL;
				goto Fail;
			}

			s.Duration = s.Start_Time = UtilConstants.Av_NoPts_Value;

			// Allocate private data
			if (FFIFmt(s.IFormat).Priv_Data_Alloc != null)
			{
				s.Priv_Data = FFIFmt(s.IFormat).Priv_Data_Alloc();

				if (s.Priv_Data == null)
				{
					ret = Error.ENOMEM;
					goto Fail;
				}

				if (s.IFormat.Priv_Class != null)
				{
					s.IFormat.Priv_Class.CopyTo(s.Priv_Data);

					Opt.Av_Opt_Set_Defaults(s.Priv_Data);

					ret = Opt.Av_Opt_Set_Dict(s.Priv_Data, ref tmp);

					if (ret < 0)
						goto Fail;
				}
			}

			// e.g. AVFMT_NOFILE formats will not have an AVIOContext
			if ((s.Pb != null) && (Is_Id3v2_Format(s.IFormat)))
				Id3v2.FF_Id3v2_Read_Dict(s.Pb, ref si.Id3v2_Meta, Id3v2.Default_Magic, out id3v2_Extra_Meta);

			if (FFIFmt(s.IFormat).Read_Header != null)
			{
				ret = FFIFmt(s.IFormat).Read_Header(s);

				if (ret < 0)
				{
					if ((FFIFmt(s.IFormat).Flags_Internal & FFInFmtFlag.Init_Cleanup) != 0)
						goto Close;

					goto Fail;
				}
			}

			if (s.Metadata == null)
			{
				s.Metadata = si.Id3v2_Meta;
				si.Id3v2_Meta = null;
			}
			else if (si.Id3v2_Meta != null)
			{
				Log.Av_Log(s, Log.Av_Log_Warning, "Discarding ID3 tags because more suitable tags were found.\n");
				Dict.Av_Dict_Free(ref si.Id3v2_Meta);
			}

			if (id3v2_Extra_Meta != null)
			{
				ret = Id3v2.FF_Id3v2_Parse_Apic(s, id3v2_Extra_Meta);
				if (ret < 0)
					goto Close;

				ret = Id3v2.FF_Id3v2_Parse_Chapters(s, id3v2_Extra_Meta);
				if (ret < 0)
					goto Close;

				ret = Id3v2.FF_Id3v2_Parse_Priv(s, id3v2_Extra_Meta);
				if (ret < 0)
					goto Close;

				Id3v2.FF_Id3v2_Free_Extra_Meta(ref id3v2_Extra_Meta);
			}

			ret = Demux_Utils.AvFormat_Queue_Attached_Pictures(s);

			if (ret < 0)
				goto Close;

			if ((s.Pb != null) && (si.Data_Offset == 0))
				si.Data_Offset = AvIoBuf.AvIo_Tell(s.Pb);

			fci.Demuxing.Raw_Packet_Buffer_Size = 0;

			Update_Stream_AvCtx(s);

			if (options != null)
			{
				Dict.Av_Dict_Free(ref options);
				options = tmp;
			}

			ps = s;

			return 0;

			Close:
			if (FFIFmt(s.IFormat).Read_Close != null)
				FFIFmt(s.IFormat).Read_Close(s);

			Fail:
			Id3v2.FF_Id3v2_Free_Extra_Meta(ref id3v2_Extra_Meta);
			Dict.Av_Dict_Free(ref tmp);

			if ((s.Pb != null) && ((s.Flags & AvFmtFlag.Custom_Io) == 0))
				AvIo.AvIo_Close(s.Pb);

			AvFormat.AvFormat_Free_Context(s);
			ps = null;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Close an opened input AVFormatContext. Free it and all its
		/// contents and set *s to NULL
		/// </summary>
		/********************************************************************/
		public static void AvFormat_Close_Input(ref AvFormatContext ps)//XX 367
		{
			if (ps == null)
				return;

			AvFormatContext s = ps;
			AvIoContext pb = s.Pb;

			if (((s.IFormat != null) && (CString.strcmp(s.IFormat.Name, "image2".ToCharPointer()) != 0) && ((s.IFormat.Flags & AvFmt.NoFile) != 0)) || ((s.Flags & AvFmtFlag.Custom_Io) != 0))
				pb = null;

			if (s.IFormat != null)
			{
				if (FFIFmt(s.IFormat).Read_Close != null)
					FFIFmt(s.IFormat).Read_Close(s);
			}

			AvFormat.FF_Format_Io_Close(s, ref pb);
			AvFormat.AvFormat_Free_Context(s);

			ps = null;
		}



		/********************************************************************/
		/// <summary>
		/// Read a transport packet from a media file
		/// </summary>
		/********************************************************************/
		public static c_int FF_Read_Packet(AvFormatContext s, AvPacket pkt)//XX 619
		{
			FormatContextInternal fci = AvFormatInternal.FF_FC_Internal(s);
			c_int err;

			Packet.Av_Packet_Unref(pkt);

			for (;;)
			{
				PacketListEntry pktl = fci.Demuxing.Raw_Packet_Buffer.Head;

				if (pktl != null)
				{
					AvStream st = s.Streams[pktl.Pkt.Stream_Index];

					if (fci.Demuxing.Raw_Packet_Buffer_Size >= s.ProbeSize)
					{
						err = Probe_Codec(s, st, null);

						if (err < 0)
							return err;
					}

					if (Internal.FFStream(st).Request_Probe <= 0)
					{
						Packet.AvPriv_Packet_List_Get(fci.Demuxing.Raw_Packet_Buffer, pkt);

						fci.Demuxing.Raw_Packet_Buffer_Size -= pkt.Size;

						return 0;
					}
				}

				err = FFIFmt(s.IFormat).Read_Packet(s, pkt);

				if (err < 0)
				{
					Packet.Av_Packet_Unref(pkt);

					// Some demuxers return FFERROR_REDO when they consume
					// data and discard it (ignored streams, junk, extradata).
					// We must re-call the demuxer to get the real packet
					if (err == Error.Redo)
						continue;

					if ((pktl == null) || (err == Error.EAGAIN))
						return err;

					for (c_uint i = 0; i < s.Nb_Streams; i++)
					{
						AvStream st = s.Streams[i];
						FFStream sti = Internal.FFStream(st);

						if ((sti.Probe_Packets != 0) || (sti.Request_Probe > 0))
						{
							err = Probe_Codec(s, st, null);

							if (err < 0)
								return err;
						}
					}

					continue;
				}

				err = Packet.Av_Packet_Make_RefCounted(pkt);

				if (err < 0)
				{
					Packet.Av_Packet_Unref(pkt);

					return err;
				}

				err = Handle_New_Packet(s, pkt, 1);

				if (err <= 0)	// Error or passthrough
					return err;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the next frame of a stream.
		/// This function returns what is stored in the file, and does not
		/// validate that what is there are valid frames for the decoder.
		/// It will split what is stored in the file into frames and return
		/// one for each call. It will not omit invalid data between valid
		/// frames so as to give the decoder the maximum information possible
		/// for decoding.
		///
		/// On success, the returned packet is reference-counted
		/// (pkt->buf is set) and valid indefinitely. The packet must be
		/// freed with av_packet_unref() when it is no longer needed. For
		/// video, the packet contains exactly one frame. For audio, it
		/// contains an integer number of frames if each frame has a known
		/// fixed size (e.g. PCM or ADPCM data). If the audio frames have a
		/// variable size (e.g. MPEG audio), then it contains one frame.
		///
		/// pkt->pts, pkt->dts and pkt->duration are always set to correct
		/// values in AVStream.time_base units (and guessed if the format
		/// cannot provide them). pkt->pts can be AV_NOPTS_VALUE if the
		/// video format has B-frames, so it is better to rely on pkt->dts if
		/// you do not decompress the payload
		/// </summary>
		/********************************************************************/
		public static c_int Av_Read_Frame(AvFormatContext s, AvPacket pkt)//XX 1534
		{
			FFFormatContext si = Internal.FFFormatContext(s);
			c_int genPts = (s.Flags & AvFmtFlag.GenPts) != 0 ? 1 : 0;
			c_int eof = 0;
			c_int ret;
			AvStream st;

			if (genPts == 0)
			{
				ret = si.Packet_Buffer.Head != null ? Packet.AvPriv_Packet_List_Get(si.Packet_Buffer, pkt) : Read_Frame_Internal(s, pkt);

				if (ret < 0)
					return ret;

				goto Return_Packet;
			}

			for (;;)
			{
				PacketListEntry pktl = si.Packet_Buffer.Head;

				if (pktl != null)
				{
					AvPacket next_Pkt = pktl.Pkt;

					if (next_Pkt.Dts != UtilConstants.Av_NoPts_Value)
					{
						c_int wrap_Bits = s.Streams[next_Pkt.Stream_Index].Pts_Wrap_Bits;

						// Last dts seen for this stream. If any of packets following
						// current one had no dts, we will set this to AV_NOPTS_VALUE
						int64_t last_Dts = next_Pkt.Dts;

						while ((pktl != null) && (next_Pkt.Pts == UtilConstants.Av_NoPts_Value))
						{
							if ((pktl.Pkt.Stream_Index == next_Pkt.Stream_Index) && (Mathematics.Av_Compare_Mod((uint64_t)next_Pkt.Dts, (uint64_t)pktl.Pkt.Dts, 2UL << (wrap_Bits - 1)) < 0))
							{
								if (Mathematics.Av_Compare_Mod((uint64_t)pktl.Pkt.Pts, (uint64_t)pktl.Pkt.Dts, 2UL << (wrap_Bits - 1)) != 0)
								{
									// Not B-frame
									next_Pkt.Pts = pktl.Pkt.Dts;
								}

								if (last_Dts != UtilConstants.Av_NoPts_Value)
								{
									// Once last dts was set to AV_NOPTS_VALUE, we don't change it
									last_Dts = pktl.Pkt.Dts;
								}
							}

							pktl = pktl.Next;
						}

						if ((eof != 0) && (next_Pkt.Pts == UtilConstants.Av_NoPts_Value) && (last_Dts != UtilConstants.Av_NoPts_Value))
						{
							// Fixing the last reference frame had none pts issue (For MXF etc).
							// We only do this when
							// 1. eof.
							// 2. we are not able to resolve a pts value for current packet.
							// 3. the packets for this stream at the end of the files had valid dts.
							next_Pkt.Pts = last_Dts + next_Pkt.Duration;
						}

						pktl = si.Packet_Buffer.Head;
					}

					// Read packet from packet buffer, if there is data
					st = s.Streams[next_Pkt.Stream_Index];

					if (!((next_Pkt.Pts == UtilConstants.Av_NoPts_Value) && (st.Discard < AvDiscard.All) && (next_Pkt.Dts != UtilConstants.Av_NoPts_Value) && (eof == 0)))
					{
						ret = Packet.AvPriv_Packet_List_Get(si.Packet_Buffer, pkt);

						goto Return_Packet;
					}
				}

				ret = Read_Frame_Internal(s, pkt);

				if (ret < 0)
				{
					if ((pktl != null) && (ret != Error.EAGAIN))
					{
						eof = 1;

						continue;
					}
					else
						return ret;
				}

				ret = Packet.AvPriv_Packet_List_Put(si.Packet_Buffer, pkt, null, FFPacketListFlag.None);

				if (ret < 0)
				{
					Packet.Av_Packet_Unref(pkt);

					return ret;
				}
			}

			Return_Packet:
			st = s.Streams[pkt.Stream_Index];

			if (((s.IFormat.Flags & AvFmt.Generic_Index) != 0) && ((pkt.Flags & AvPktFlag.Key) != 0))
			{
				Seek.FF_Reduce_Index(s, st.Index);
				Seek.Av_Add_Index_Entry(st, pkt.Pos, pkt.Dts, 0, 0, AvIndex.KeyFrame);
			}

			if (AvFormatInternal.Is_Relative(pkt.Dts))
				pkt.Dts -= AvFormatInternal.Relative_Ts_Base;

			if (AvFormatInternal.Is_Relative(pkt.Pts))
				pkt.Pts -= AvFormatInternal.Relative_Ts_Base;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Read packets of a media file to get stream information. This is
		/// useful for file formats with no headers such as MPEG. This
		/// function also computes the real framerate in case of MPEG-2
		/// repeat frame mode.
		/// The logical file position is not changed by this function;
		/// examined packets may be buffered for later processing
		///
		/// Note: This function isn't guaranteed to open all the codecs, so
		///       options being non-empty at return is a perfectly normal
		///       behavior
		///
		/// Todo: Let the user decide somehow what information is needed so
		///       that we do not waste time getting stuff the user does not
		///       need
		/// </summary>
		/********************************************************************/
		public static c_int AvFormat_Find_Stream_Info(AvFormatContext ic, CPointer<AvDictionary> options)//XX 2512
		{
			FFFormatContext si = Internal.FFFormatContext(ic);
			c_int count = 0, ret = 0, err;
			AvPacket pkt1 = si.Pkt;
			int64_t old_Offset = AvIoBuf.AvIo_Tell(ic.Pb);

			// New streams might appear, no options for those
			c_int orig_Nb_Streams = (c_int)ic.Nb_Streams;
			int64_t max_Analyze_Duration = ic.Max_Analyze_Duration;
			int64_t probeSize = ic.ProbeSize;
			c_int eof_Reached = 0;

			c_int flush_Codecs = probeSize > 0 ? 1 : 0;

			Opt.Av_Opt_Set_Int(ic, "skip_clear".ToCharPointer(), 1, AvOptSearch.Search_Children);

			int64_t max_Stream_Analyze_Duration = max_Analyze_Duration;
			int64_t max_SubTitle_Analyze_Duration = max_Analyze_Duration;

			if (max_Analyze_Duration == 0)
			{
				max_Stream_Analyze_Duration = max_Analyze_Duration = 5 * UtilConstants.Av_Time_Base;
				max_SubTitle_Analyze_Duration = 30 * UtilConstants.Av_Time_Base;

				if (CString.strcmp(ic.IFormat.Name, "flv") == 0)
					max_Stream_Analyze_Duration = 90 * UtilConstants.Av_Time_Base;

				if ((CString.strcmp(ic.IFormat.Name, "mpeg") == 0) || (CString.strcmp(ic.IFormat.Name, "mpegts") == 0))
					max_Stream_Analyze_Duration = 7 * UtilConstants.Av_Time_Base;
			}

			if (ic.Pb != null)
			{
				FFIoContext ctx = AvIo_Internal.FFIoContext(ic.Pb);

				Log.Av_Log(ic, Log.Av_Log_Debug, "Before avformat_find_stream_info() pos: %lld bytes read:%lld seeks:%d nb_streams:%d\n", AvIoBuf.AvIo_Tell(ic.Pb), ctx.Bytes_Read, ctx.Seek_Count, ic.Nb_Streams);
			}

			for (c_uint i = 0; i < ic.Nb_Streams; i++)
			{
				AvDictionary tmpOpt = options.IsNotNull ? options[i] : null;

				AvStream st = ic.Streams[i];
				FFStream sti = Internal.FFStream(st);
				AvCodecContext avCtx = sti.AvCtx;

				// Check if the caller has overridden the codec id
				// Only for the split stuff
				if ((sti.Parser == null) && ((ic.Flags & AvFmtFlag.NoParse) == 0) && (sti.Request_Probe <= 0))
				{
					sti.Parser = Parser.Av_Parser_Init(st.CodecPar.Codec_Id);

					if (sti.Parser != null)
					{
						if (sti.Need_Parsing == AvStreamParseType.Headers)
							sti.Parser.Flags |= ParserFlag.Complete_Frames;
						else if (sti.Need_Parsing == AvStreamParseType.Full_Raw)
							sti.Parser.Flags |= ParserFlag.Use_Codec_Ts;
					}
					else if (sti.Need_Parsing != AvStreamParseType.None)
						Log.Av_Log(ic, Log.Av_Log_Verbose, "parser not found for codec %s, packets or times may be invalid.\n", Utils_Codec.AvCodec_Get_Name(st.CodecPar.Codec_Id));
				}

				ret = Codec_Par.AvCodec_Parameters_To_Context(avCtx, st.CodecPar);

				if (ret < 0)
					goto Find_Stream_Info_Err;

				if (sti.Request_Probe <= 0)
					sti.AvCtx_Inited = 1;

				AvCodec codec = Find_Probe_Decoder(ic, st, st.CodecPar.Codec_Id);

				// Force thread count to 1 since the H.264 decoder will not extract
				// SPS and PPS to extradata during multi-threaded decoding
				Dict.Av_Dict_Set(ref tmpOpt, "threads", "1", AvDict.None);

				// Force lowres to 0. The decoder might reduce the video size by the
				// lowres factor, and we don't want that propagated to the stream's
				// codecpar
				Dict.Av_Dict_Set(ref tmpOpt, "lowres", "0", AvDict.None);

				if (ic.Codec_Whitelist.IsNotNull)
					Dict.Av_Dict_Set(ref tmpOpt, "codec_whitelist", ic.Codec_Whitelist, AvDict.None);

				// Try to just open decoders, in case this is enough to get parameters.
				// Also ensure that subtitle header is properly set
				if (!Has_Codec_Parameters(st, out _) && (sti.Request_Probe <= 0) || (st.CodecPar.Codec_Type == AvMediaType.Subtitle))
				{
					if ((codec != null) && (avCtx.Codec == null))
					{
						if (AvCodec_.AvCodec_Open2(avCtx, codec, ref tmpOpt) < 0)
							Log.Av_Log(ic, Log.Av_Log_Warning, "Failed to open codec in %s\n", "avformat_find_stream_info");
					}
				}

				if (options.IsNotNull)
					options[i] = tmpOpt;
				else
					Dict.Av_Dict_Free(ref tmpOpt);
			}

			int64_t read_Size = 0;

			for (;;)
			{
				AvPacket pkt;
				AvStream st;
				FFStream sti;
				AvCodecContext avCtx;
				c_uint i;

				if (AvIo.FF_Check_Interrupt(ic.Interrupt_Callback) != 0)
				{
					ret = Error.Exit;

					Log.Av_Log(ic, Log.Av_Log_Debug, "interrupted\n");
					break;
				}

				// Check if one codec still needs to be handled
				for (i = 0; i < ic.Nb_Streams; i++)
				{
					st = ic.Streams[i];
					sti = Internal.FFStream(st);
					c_int fps_Analyze_FrameCount = 20;

					if (!Has_Codec_Parameters(st, out _))
						break;

					// If the timebase is coarse (like the usual millisecond precision
					// of mkv), we need to analyze more frames to reliably arrive at
					// the correct fps
					if (Rational.Av_Q2D(st.Time_Base) > 0.0005)
						fps_Analyze_FrameCount *= 2;

					if (Tb_Unreliable(ic, st) != 0)
						fps_Analyze_FrameCount = 0;

					if (ic.Fps_Probe_Size >= 0)
						fps_Analyze_FrameCount = ic.Fps_Probe_Size;

					if ((st.Disposition & AvDisposition.Attached_Pic) != 0)
						fps_Analyze_FrameCount = 0;

					// Variable fps and no guess at the real fps
					c_int _count = (ic.IFormat.Flags & AvFmt.NoTimestamps) != 0 ? (c_int)(sti.Info.Codec_Info_Duration_Fields / 2) : sti.Info.Duration_Count;

					if (!((st.R_Frame_Rate.Num != 0) && (st.Avg_Frame_Rate.Num != 0)) && (st.CodecPar.Codec_Type == AvMediaType.Video))
					{
						if (_count < fps_Analyze_FrameCount)
							break;
					}

					// Look at the first 3 frames if there is evidence of frame delay
					// but the decoder delay is not set
					if ((sti.Info.Frame_Delay_Evidence != 0) && (_count < 2) && (sti.AvCtx.Has_B_Frames == 0))
						break;

					if ((sti.AvCtx.ExtraData == null) && ((sti.Extract_ExtraData.Inited == 0) || (sti.Extract_ExtraData.Bsf != null)) && (Extract_ExtraData_Check(st) != 0))
						break;

					if ((sti.First_Dts == UtilConstants.Av_NoPts_Value) && (((ic.IFormat.Flags & AvFmt.NoTimestamps) == 0) || (sti.Need_Parsing == AvStreamParseType.Full_Raw)) && (sti.Codec_Info_Nb_Frames < ((st.Disposition & AvDisposition.Attached_Pic) != 0 ? 1 : ic.Max_Ts_Probe)) && ((st.CodecPar.Codec_Type == AvMediaType.Video) || (st.CodecPar.Codec_Type == AvMediaType.Audio)))
						break;
				}

				c_int analyzed_All_Streams = 0;

				if ((i == ic.Nb_Streams) && (si.Missing_Streams == 0))
				{
					analyzed_All_Streams = 1;

					// NOTE: If the format has no header, then we need to read some
					// packets to get most of the streams, so we cannot stop here
					if ((ic.Ctx_Flags & AvFmtCtx.NoHeader) == 0)
					{
						// If we found the info for all the codecs, we can stop
						ret = count;
						Log.Av_Log(ic, Log.Av_Log_Debug, "All info found\n");
						flush_Codecs = 0;
						break;
					}
				}

				// We did not get all the codec info, but we read too much data
				if (read_Size >= probeSize)
				{
					ret = count;
					Log.Av_Log(ic, Log.Av_Log_Debug, "Probe buffer size limit of %lld bytes reached\n", probeSize);

					for (i = 0; i < ic.Nb_Streams; i++)
					{
						st = ic.Streams[i];
						sti = Internal.FFStream(st);

						if ((st.R_Frame_Rate.Num == 0) && (sti.Info.Duration_Count <= 1) && (st.CodecPar.Codec_Type == AvMediaType.Video) && (CString.strcmp(ic.IFormat.Name, "image2") == 0))
							Log.Av_Log(ic, Log.Av_Log_Warning, "Stream #%d: not enough frames to estimate rate; consider increasing probesize\n", i);
					}

					break;
				}

				// NOTE: A new stream can be added there if no header in file
				// (AVFMTCTX_NOHEADER)
				ret = Read_Frame_Internal(ic, pkt1);

				if (ret == Error.EAGAIN)
					continue;

				if (ret < 0)
				{
					// EOF or error
					eof_Reached = 1;
					break;
				}

				if ((ic.Flags & AvFmtFlag.NoBuffer) == 0)
				{
					ret = Packet.AvPriv_Packet_List_Put(si.Packet_Buffer, pkt1, null, FFPacketListFlag.None);

					if (ret < 0)
						goto Unref_Then_Goto_End;

					pkt = si.Packet_Buffer.Tail.Pkt;
				}
				else
					pkt = pkt1;

				st = ic.Streams[pkt.Stream_Index];
				sti = Internal.FFStream(st);

				if ((st.Disposition & AvDisposition.Attached_Pic) == 0)
					read_Size += pkt.Size;

				avCtx = sti.AvCtx;

				if (sti.AvCtx_Inited == 0)
				{
					ret = Codec_Par.AvCodec_Parameters_To_Context(avCtx, st.CodecPar);

					if (ret < 0)
						goto Unref_Then_Goto_End;

					sti.AvCtx_Inited = 1;
				}

				if ((pkt.Dts != UtilConstants.Av_NoPts_Value) && (sti.Codec_Info_Nb_Frames > 1))
				{
					// Check for non-increasing dts
					if ((sti.Info.Fps_First_Dts != UtilConstants.Av_NoPts_Value) && (sti.Info.Fps_Last_Dts >= pkt.Dts))
					{
						Log.Av_Log(ic, Log.Av_Log_Debug, "Non-increasing DTS in stream %d: packet %d with DTS %lld, packet %d with DTS %lld\n", st.Index, sti.Info.Fps_Last_Dts_Idx, sti.Info.Fps_Last_Dts, sti.Codec_Info_Nb_Frames, pkt.Dts);

						sti.Info.Fps_First_Dts = sti.Info.Fps_Last_Dts = UtilConstants.Av_NoPts_Value;
					}

					// Check for a discontinuity in dts. If the difference in dts
					// is more than 1000 times the average packet duration in the
					// sequence, we treat it as a discontinuity
					if ((sti.Info.Fps_Last_Dts != UtilConstants.Av_NoPts_Value) && (sti.Info.Fps_Last_Dts_Idx > sti.Info.Fps_First_Dts) && (((uint64_t)(pkt.Dts - sti.Info.Fps_Last_Dts) / 1000) > ((uint64_t)(sti.Info.Fps_Last_Dts - sti.Info.Fps_First_Dts) / (uint64_t)(sti.Info.Fps_Last_Dts_Idx - sti.Info.Fps_First_Dts_Idx))))
					{
						Log.Av_Log(ic, Log.Av_Log_Warning, "DTS discontinuity in stream %d: packet %d with DTS %lld, packet %d with DTS %lld\n", st.Index, sti.Info.Fps_Last_Dts_Idx, sti.Info.Fps_Last_Dts, sti.Codec_Info_Nb_Frames, pkt.Dts);

						sti.Info.Fps_First_Dts = sti.Info.Fps_Last_Dts = UtilConstants.Av_NoPts_Value;
					}

					// Update stored dts values
					if (sti.Info.Fps_First_Dts == UtilConstants.Av_NoPts_Value)
					{
						sti.Info.Fps_First_Dts = pkt.Dts;
						sti.Info.Fps_First_Dts_Idx = sti.Codec_Info_Nb_Frames;
					}

					sti.Info.Fps_Last_Dts = pkt.Dts;
					sti.Info.Fps_Last_Dts_Idx = sti.Codec_Info_Nb_Frames;
				}

				if (sti.Codec_Info_Nb_Frames > 1)
				{
					int64_t t = 0;
					int64_t limit;

					if (st.Time_Base.Den > 0)
						t = Mathematics.Av_Rescale_Q(sti.Info.Codec_Info_Duration, st.Time_Base, UtilConstants.Av_Time_Base_Q);

					if (st.Avg_Frame_Rate.Num > 0)
						t = Macros.FFMax(t, Mathematics.Av_Rescale_Q(sti.Codec_Info_Nb_Frames, Rational.Av_Inv_Q(st.Avg_Frame_Rate), UtilConstants.Av_Time_Base_Q));

					if ((t == 0) && (sti.Codec_Info_Nb_Frames > 30) && (sti.Info.Fps_First_Dts != UtilConstants.Av_NoPts_Value) && (sti.Info.Fps_Last_Dts != UtilConstants.Av_NoPts_Value))
					{
						int64_t dur = Common.Av_Sat_Sub64(sti.Info.Fps_Last_Dts, sti.Info.Fps_First_Dts);
						t = Macros.FFMax(t, Mathematics.Av_Rescale_Q(dur, st.Time_Base, UtilConstants.Av_Time_Base_Q));
					}

					if (analyzed_All_Streams != 0)
						limit = max_Analyze_Duration;
					else if (avCtx.Codec_Type == AvMediaType.Subtitle)
						limit = max_SubTitle_Analyze_Duration;
					else
						limit = max_Stream_Analyze_Duration;

					if (t >= limit)
					{
						Log.Av_Log(ic, Log.Av_Log_Verbose, "max_analyze duration %lld reached at %lld microseconds st:%d\n", limit, t, pkt.Stream_Index);

						if ((ic.Flags & AvFmtFlag.NoBuffer) != 0)
							Packet.Av_Packet_Unref(pkt1);

						break;
					}

					if ((pkt.Duration > 0) && (pkt.Duration < (int64_t.MaxValue - sti.Info.Codec_Info_Duration)))
					{
						c_int fields = (sti.Codec_Desc != null) && ((sti.Codec_Desc.Props & AvCodecProp.Fields) != 0) ? 1 : 0;

						if ((avCtx.Codec_Type == AvMediaType.Subtitle) && (pkt.Pts != UtilConstants.Av_NoPts_Value) && (st.Start_Time != UtilConstants.Av_NoPts_Value) && (pkt.Pts >= st.Start_Time) && ((uint64_t)(pkt.Pts - st.Start_Time) < int64_t.MaxValue))
							sti.Info.Codec_Info_Duration = Macros.FFMin(pkt.Pts - st.Start_Time, sti.Info.Codec_Info_Duration + pkt.Duration);
						else
							sti.Info.Codec_Info_Duration += pkt.Duration;

						sti.Info.Codec_Info_Duration_Fields += (sti.Parser != null) && (sti.Need_Parsing != AvStreamParseType.None) && (fields != 0) ? sti.Parser.Repeat_Pict + 1 : 2;
					}
				}

				if (st.CodecPar.Codec_Type == AvMediaType.Video)
				{
					if ((pkt.Dts != pkt.Pts) && (pkt.Dts != UtilConstants.Av_NoPts_Value) && (pkt.Pts != UtilConstants.Av_NoPts_Value))
						sti.Info.Frame_Delay_Evidence = 1;
				}

				if (sti.AvCtx.ExtraData == null)
				{
					ret = Extract_ExtraData(si, st, pkt);

					if (ret < 0)
						goto Unref_Then_Goto_End;
				}

				// If still no information, we try to open the codec and to
				// decompress the frame. We try to avoid that in most cases as
				// it takes longer and uses more memory. For MPEG-4, we need to
				// decompress for QuickTime.
				// 
				// If AV_CODEC_CAP_CHANNEL_CONF is set this will force decoding of at
				// least one frame of codec data, this makes sure the codec initializes
				// the channel configuration and does not only trust the values from
				// the container
				AvDictionary tmpOpts = options.IsNotNull && (i < orig_Nb_Streams) ? options[i] : null;

				Try_Decode_Frame(ic, st, pkt, ref tmpOpts);

				if (options.IsNotNull && (i < orig_Nb_Streams))
					options[i] = tmpOpts;

				if ((ic.Flags & AvFmtFlag.NoBuffer) != 0)
					Packet.Av_Packet_Unref(pkt1);

				sti.Codec_Info_Nb_Frames++;
				count++;
			}

			if (eof_Reached != 0)
			{
				for (c_uint stream_Index = 0; stream_Index < ic.Nb_Streams; stream_Index++)
				{
					AvStream st = ic.Streams[stream_Index];
					AvCodecContext avCtx = Internal.FFStream(st).AvCtx;

					if (Has_Codec_Parameters(st, out _))
					{
						AvCodec codec = Find_Probe_Decoder(ic, st, st.CodecPar.Codec_Id);

						if ((codec != null) && (avCtx.Codec == null))
						{
							AvDictionary opts = null;

							if (ic.Codec_Whitelist.IsNotNull)
								Dict.Av_Dict_Set(ref opts, "codec_whitelist", ic.Codec_Whitelist, AvDict.None);

							if (AvCodec_.AvCodec_Open2(avCtx, codec, ref (options.IsNotNull && (stream_Index < orig_Nb_Streams) ? ref options[stream_Index] : ref opts)) < 0)
								Log.Av_Log(ic, Log.Av_Log_Warning, "Failed to open codec in %s\n", "AvFormat_Find_Stream_Info");

							Dict.Av_Dict_Free(ref opts);
						}
					}

					// EOF already reached while reading the stream above.
					// So continue with reoordering DTS with whatever delay we have
					if ((si.Packet_Buffer.Head != null) && !Has_Decode_Delay_Been_Guessed(st))
						Update_Dts_From_Pts(ic, (c_int)stream_Index, si.Packet_Buffer.Head);
				}
			}

			if (flush_Codecs != 0)
			{
				AvPacket empty_Pkt = si.Pkt;

				Packet.Av_Packet_Unref(empty_Pkt);

				for (c_uint i = 0; i < ic.Nb_Streams; i++)
				{
					AvStream st = ic.Streams[i];
					FFStream sti = Internal.FFStream(st);

					// Flush the decoders
					if (sti.Info.Found_Decoder == 1)
					{
						AvDictionary tmpOpts = options.IsNotNull && (i < orig_Nb_Streams) ? options[i] : null;

						err = Try_Decode_Frame(ic, st, empty_Pkt, ref tmpOpts);

						if (err < 0)
							Log.Av_Log(ic, Log.Av_Log_Info, "decoding for stream %d failed\n", st.Index);

						if (options.IsNotNull && (i < orig_Nb_Streams))
							options[i] = tmpOpts;
					}
				}
			}

			FF_Rfps_Calculate(ic);

			for (c_uint i = 0; i < ic.Nb_Streams; i++)
			{
				AvStream st = ic.Streams[i];
				FFStream sti = Internal.FFStream(st);
				AvCodecContext avCtx = sti.AvCtx;

				if (avCtx.Codec_Type == AvMediaType.Video)
				{
					if ((avCtx.Codec_Id == AvCodecId.RawVideo) && (avCtx.Codec_Tag == 0) && (avCtx.Bits_Per_Coded_Sample == 0))
					{
						uint32_t tag = Raw.AvCodec_Pix_Fmt_To_Codec_Tag(avCtx.Pix_Fmt);

						if (Raw.AvPriv_Pix_Fmt_Find(PixelFormatTagLists.Raw, tag) == avCtx.Pix_Fmt)
							avCtx.Codec_Tag = tag;
					}

					// Estimate average framerate if not set by demuxer
					if ((sti.Info.Codec_Info_Duration_Fields != 0) && (st.Avg_Frame_Rate.Num == 0) && (sti.Info.Codec_Info_Duration != 0))
					{
						c_int best_Fps = 0;
						c_double best_Error = 0.01;
						AvRational codec_Frame_Rate = avCtx.FrameRate;

						if ((sti.Info.Codec_Info_Duration >= (int64_t.MaxValue / st.Time_Base.Num / 2)) || (sti.Info.Codec_Info_Duration_Fields >= (int64_t.MaxValue / st.Time_Base.Den)) || (sti.Info.Codec_Info_Duration < 0))
							continue;

						Rational.Av_Reduce(out st.Avg_Frame_Rate.Num, out st.Avg_Frame_Rate.Den, sti.Info.Codec_Info_Duration_Fields * (int64_t)st.Time_Base.Den, sti.Info.Codec_Info_Duration * 2 * (int64_t)st.Time_Base.Num, 60000);

						// Round guessed framerate to a "standard" framerate if it's
						// within 1% of the original estimate
						for (c_int j = 0; j < Max_Std_Timebases; j++)
						{
							AvRational std_Fps = new AvRational(Get_Std_FrameRate(j), 12 * 1001);
							c_double error = CMath.fabs(Rational.Av_Q2D(st.Avg_Frame_Rate) / Rational.Av_Q2D(std_Fps) - 1);

							if (error < best_Error)
							{
								best_Error = error;
								best_Fps = std_Fps.Num;
							}

							if (((FFIFmt(ic.IFormat).Flags_Internal & FFInFmtFlag.Prefer_Codec_FrameRate) != 0) && (codec_Frame_Rate.Num > 0) && (codec_Frame_Rate.Den > 0))
							{
								error = CMath.fabs(Rational.Av_Q2D(codec_Frame_Rate) / Rational.Av_Q2D(std_Fps) - 1);

								if (error < best_Error)
								{
									best_Error = error;
									best_Fps = std_Fps.Num;
								}
							}
						}

						if (best_Fps != 0)
							Rational.Av_Reduce(out st.Avg_Frame_Rate.Num, out st.Avg_Frame_Rate.Den, best_Fps, 12 * 1001, c_int.MaxValue);
					}

					if (st.R_Frame_Rate.Num == 0)
					{
						AvCodecDescriptor desc = sti.Codec_Desc;
						AvRational mul = new AvRational((desc != null) && ((desc.Props & AvCodecProp.Fields) != 0) ? 2 : 1, 1);
						AvRational fr = Rational.Av_Mul_Q(avCtx.FrameRate, mul);

						if ((fr.Num != 0) && (fr.Den != 0) && (Rational.Av_Cmp_Q(st.Time_Base, Rational.Av_Inv_Q(fr)) <= 0))
							st.R_Frame_Rate = fr;
						else
						{
							st.R_Frame_Rate.Num = st.Time_Base.Den;
							st.R_Frame_Rate.Den = st.Time_Base.Num;
						}
					}

					st.CodecPar.FrameRate = avCtx.FrameRate;

					if ((sti.Display_Aspect_Ratio.Num != 0) && (sti.Display_Aspect_Ratio.Den != 0))
					{
						AvRational hw_Ratio = new AvRational(avCtx.PictureSize.Height, avCtx.PictureSize.Width);

						st.Sample_Aspects_Ratio = Rational.Av_Mul_Q(sti.Display_Aspect_Ratio, hw_Ratio);
					}
				}
				else if (avCtx.Codec_Type == AvMediaType.Audio)
				{
					if (avCtx.Bits_Per_Coded_Sample == 0)
						avCtx.Bits_Per_Coded_Sample = Utils_Codec.Av_Get_Bits_Per_Sample(avCtx.Codec_Id);

					// Set stream disposition based on audio service type
					switch (avCtx.Audio_Service_Type)
					{
						case AvAudioServiceType.Effects:
						{
							st.Disposition = AvDisposition.Clean_Effects;
							break;
						}

						case AvAudioServiceType.Visually_Impaired:
						{
							st.Disposition = AvDisposition.Visual_Impaired;
							break;
						}

						case AvAudioServiceType.Hearing_Impaired:
						{
							st.Disposition = AvDisposition.Hearing_Impaired;
							break;
						}

						case AvAudioServiceType.Commentary:
						{
							st.Disposition = AvDisposition.Comment;
							break;
						}

						case AvAudioServiceType.Karaoke:
						{
							st.Disposition = AvDisposition.Karaoke;
							break;
						}
					}
				}
			}

			if (probeSize != 0)
				Estimate_Timings(ic, old_Offset);

			Opt.Av_Opt_Set_Int(ic, "skip_clear".ToCharPointer(), 0, AvOptSearch.Search_Children);

			if ((ret >= 0) && (ic.Nb_Streams != 0))
			{
				// We could not have all the codec parameters before EOF
				ret = -1;
			}

			for (c_uint i = 0; i < ic.Nb_Streams; i++)
			{
				AvStream st = ic.Streams[i];
				FFStream sti = Internal.FFStream(st);

				// If no packet was ever seen, update context now for has_codec_parameters
				if (sti.AvCtx_Inited == 0)
				{
					if ((st.CodecPar.Codec_Type == AvMediaType.Audio) && (st.CodecPar.Format.Sample == AvSampleFormat.None))
						st.CodecPar.Format.Sample = sti.AvCtx.Sample_Fmt;

					ret = Codec_Par.AvCodec_Parameters_To_Context(sti.AvCtx, st.CodecPar);

					if (ret < 0)
						goto Find_Stream_Info_Err;
				}

				if (!Has_Codec_Parameters(st, out CPointer<char> errMsg))
				{
					CPointer<char> buf = new CPointer<char>(256);

					AvCodec_.AvCodec_String(buf, buf.Length, sti.AvCtx, 0);
					Log.Av_Log(ic, Log.Av_Log_Warning, "Could not find codec parameters for stream %d (%s): %s\nConsider increasing the value for the 'analyzeduration' (%lld) and 'probesize' (%lld) options\n", i, buf, errMsg, ic.Max_Analyze_Duration, ic.ProbeSize);
				}
				else
					ret = 0;
			}

			err = Compute_Chapters_End(ic);

			if (err < 0)
			{
				ret = err;

				goto Find_Stream_Info_Err;
			}

			// Update the stream parameters from the internal codec contexts
			for (c_uint i =0; i < ic.Nb_Streams; i++)
			{
				AvStream st = ic.Streams[i];
				FFStream sti = Internal.FFStream(st);

				if (sti.AvCtx_Inited != 0)
				{
					ret = Codec_Par.AvCodec_Parameters_From_Context(st.CodecPar, sti.AvCtx);

					if (ret < 0)
						goto Find_Stream_Info_Err;

					if ((sti.AvCtx.Rc_Buffer_Size > 0) || (sti.AvCtx.Rc_Max_Rate > 0) || (sti.AvCtx.Rc_Min_Rate != 0))
					{
						AvCpbProperties props = Utils_Codec.Av_Cpb_Properties_Alloc();

						if (props != null)
						{
							if (sti.AvCtx.Rc_Buffer_Size > 0)
								props.Buffer_Size = sti.AvCtx.Rc_Buffer_Size;

							if (sti.AvCtx.Rc_Min_Rate > 0)
								props.Min_BitRate = sti.AvCtx.Rc_Min_Rate;

							if (sti.AvCtx.Rc_Max_Rate > 0)
								props.Max_BitRate = sti.AvCtx.Rc_Max_Rate;

							if (Packet.Av_Packet_Side_Data_Add(ref st.CodecPar.Coded_Side_Data, ref st.CodecPar.Nb_Coded_Side_Data, AvPacketSideDataType.Cpb_Properties, props, 0) == null)
								Mem.Av_Free(props);
						}
					}
				}

				sti.AvCtx_Inited = 0;
			}

			Find_Stream_Info_Err:
			for (c_uint i = 0; i < ic.Nb_Streams; i++)
			{
				AvStream st = ic.Streams[i];
				FFStream sti = Internal.FFStream(st);

				if (sti.Info != null)
				{
					Mem.Av_FreeP(ref sti.Info.Duration_Error);
					Mem.Av_FreeP(ref sti.Info);
				}

				if (AvCodec_.AvCodec_Is_Open(sti.AvCtx))
				{
					err = Codec_Close(sti);

					if ((err < 0) && (ret >= 0))
						ret = err;
				}

				Bsf.Av_Bsf_Free(ref sti.Extract_ExtraData.Bsf);
			}

			if (ic.Pb != null)
			{
				FFIoContext ctx = AvIo_Internal.FFIoContext(ic.Pb);
				Log.Av_Log(ic, Log.Av_Log_Debug, "After avformat_find_stream_info() pos: %lld bytes read:%lld seeks:%d frames:%d\n", AvIoBuf.AvIo_Tell(ic.Pb), ctx.Bytes_Read, ctx.Seek_Count, count);
			}

			return ret;

			Unref_Then_Goto_End:
			Packet.Av_Packet_Unref(pkt1);

			goto Find_Stream_Info_Err;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static int64_t Wrap_Timestamp(AvStream st, int64_t timestamp)//XX 53
		{
			FFStream sti = Internal.CFFStream(st);

			if ((sti.Pts_Wrap_Behavior != AvPtsWrap.Ignore) && (st.Pts_Wrap_Bits < 64) && (sti.Pts_Wrap_Reference != UtilConstants.Av_NoPts_Value) && (timestamp != UtilConstants.Av_NoPts_Value))
			{
				if ((sti.Pts_Wrap_Behavior == AvPtsWrap.Add_Offset) && (timestamp < sti.Pts_Wrap_Reference))
					return timestamp + (1L << st.Pts_Wrap_Bits);
				else if ((sti.Pts_Wrap_Behavior == AvPtsWrap.Sub_Offset) && (timestamp >= sti.Pts_Wrap_Reference))
					return timestamp - (1L << st.Pts_Wrap_Bits);
			}

			return timestamp;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static AvCodec Find_Probe_Decoder(AvFormatContext s, AvStream st, AvCodecId codec_Id)//XX 73
		{
			// Other parts of the code assume this decoder to be used for h264,
			// so force it if possible
			if (codec_Id == AvCodecId.H264)
				return AllCodec.AvCodec_Find_Decoder_By_Name("h264".ToCharPointer());

			AvCodec codec = AvFormat.FF_Find_Decoder(s, st, codec_Id);

			if (codec == null)
				return null;

			if ((codec.Capabilities & AvCodecCap.Avoid_Probing) != 0)
			{
				foreach (AvCodec probe_Codec in AllCodec.Av_Codec_Iterate())
				{
					if ((probe_Codec.Id == codec_Id) && Utils_Codec.Av_Codec_Is_Decoder(probe_Codec) && ((probe_Codec.Capabilities & (AvCodecCap.Avoid_Probing | AvCodecCap.Experimental)) == 0))
						return probe_Codec;
				}
			}

			return codec;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Set_Codec_From_Probe_Data(AvFormatContext s, AvStream st, AvProbeData pd)//XX 103
		{
			AvInputFormat fmt = Format.Av_Probe_Input_Format3(pd, true, out c_int score);
			FFStream sti = Internal.FFStream(st);

			if (fmt != null)
			{
				Log.Av_Log(s, Log.Av_Log_Debug, "Probe with size=%d, packets=%d detected %s with score=%d\n", pd.Buf_Size, s.Max_Probe_Packets - sti.Probe_Packets, fmt.Name, score);

				foreach (Fmt_Id_Type f in fmt_Id_Type)
				{
					if (CString.strcmp(fmt.Name, f.Name) == 0)
					{
						if ((f.Type != AvMediaType.Audio) && (st.CodecPar.Sample_Rate != 0))
							continue;

						if ((sti.Request_Probe > score) && (st.CodecPar.Codec_Id != f.Id))
							continue;

						st.CodecPar.Codec_Id = f.Id;
						st.CodecPar.Codec_Type = f.Type;
						sti.Need_Context_Update = 1;

						return score;
					}
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Init_Input(AvFormatContext s, CPointer<char> fileName, ref AvDictionary options)//XX 158
		{
			AvProbeData pd = new AvProbeData { FileName = fileName };
			c_int score = AvProbe.Score_Retry;

			if (s.Pb != null)
			{
				s.Flags |= AvFmtFlag.Custom_Io;

				if (s.IFormat == null)
					return Format.Av_Probe_Input_Buffer2(s.Pb, out s.IFormat, fileName, s, 0, (c_uint)s.Format_ProbeSize);
				else if ((s.IFormat.Flags & AvFmt.NoFile) != 0)
					Log.Av_Log(s, Log.Av_Log_Warning, "Custom AVIOContext makes no sense and will be ignored with AVFMT_NOFILE format.\n");

				return 0;
			}

			if (((s.IFormat != null) && ((s.IFormat.Flags & AvFmt.NoFile) != 0)) || ((s.IFormat == null) && ((s.IFormat = Format.Av_Probe_Input_Format2(pd, false, ref score))) != null))
				return score;

			c_int ret = s.Io_Open(s, out s.Pb, fileName, AvIoFlag.Read | s.AvIo_Flags, ref options);

			if (ret < 0)
				return ret;

			if (s.IFormat != null)
				return 0;

			return Format.Av_Probe_Input_Buffer2(s.Pb, out s.IFormat, fileName, s, 0, (c_uint)s.Format_ProbeSize);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Update_Stream_AvCtx(AvFormatContext s)//XX 189
		{
			for (c_uint i = 0; i < s.Nb_Streams; i++)
			{
				AvStream st = s.Streams[i];
				FFStream sti = Internal.FFStream(st);

				if (sti.Need_Context_Update == 0)
					continue;

				// Close parser, because it depends on the codec
				if ((sti.Parser != null) && (sti.AvCtx.Codec_Id != st.CodecPar.Codec_Id))
				{
					Parser.Av_Parser_Close(sti.Parser);
					sti.Parser = null;
				}

				// Update internal codec context, for the parser
				c_int ret = Codec_Par.AvCodec_Parameters_To_Context(sti.AvCtx, st.CodecPar);

				if (ret < 0)
					return ret;

				sti.Codec_Desc = Codec_Desc.AvCodec_Descriptor_Get(sti.AvCtx.Codec_Id);

				sti.Need_Context_Update = 0;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool Is_Id3v2_Format(AvInputFormat fmt)//XX 217
		{
			return (FFIFmt(fmt).Flags_Internal & FFInFmtFlag.Id3V2_Auto) != 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Force_Codec_Ids(AvFormatContext s, AvStream st)//XX 392
		{
			switch (st.CodecPar.Codec_Type)
			{
				case AvMediaType.Video:
				{
					if (s.Video_Codec_Id != AvCodecId.None)
						st.CodecPar.Codec_Id = s.Video_Codec_Id;

					break;
				}

				case AvMediaType.Audio:
				{
					if (s.Audio_Codec_Id != AvCodecId.None)
						st.CodecPar.Codec_Id = s.Audio_Codec_Id;

					break;
				}

				case AvMediaType.Subtitle:
				{
					if (s.Subtitle_Codec_Id != AvCodecId.None)
						st.CodecPar.Codec_Id = s.Subtitle_Codec_Id;

					break;
				}

				case AvMediaType.Data:
				{
					if (s.Data_Codec_Id != AvCodecId.None)
						st.CodecPar.Codec_Id = s.Data_Codec_Id;

					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Probe_Codec(AvFormatContext s, AvStream st, AvPacket pkt)//XX 414
		{
			FormatContextInternal fci = AvFormatInternal.FF_FC_Internal(s);
			FFStream sti = Internal.FFStream(st);

			if (sti.Request_Probe > 0)
			{
				AvProbeData pd = sti.Probe_Data;

				Log.Av_Log(s, Log.Av_Log_Debug, "probing stream %d pp:%d\n", st.Index, sti.Probe_Packets);

				--sti.Probe_Packets;

				void No_Packet()
				{
					sti.Probe_Packets = 0;

					if (pd.Buf_Size == 0)
						Log.Av_Log(s, Log.Av_Log_Warning, "nothing to probe for stream %d\n", st.Index);
				}

				if (pkt != null)
				{
					CPointer<uint8_t> new_Buf = Mem.Av_Realloc(pd.Buf, (size_t)(pd.Buf_Size + pkt.Size + Defs.Av_Input_Buffer_Padding_Size));

					if (new_Buf.IsNull)
					{
						Log.Av_Log(s, Log.Av_Log_Warning, "Failed to reallocate probe buffer for stream %d\n", st.Index);

						No_Packet();
					}
					else
					{
						pd.Buf = new_Buf;

						CMemory.memcpy(pd.Buf + pd.Buf_Size, pkt.Data, (size_t)pkt.Size);

						pd.Buf_Size += pkt.Size;

						CMemory.memset<uint8_t>(pd.Buf + pd.Buf_Size, 0, Defs.Av_Input_Buffer_Padding_Size);
					}
				}
				else
					No_Packet();

				c_int end = (fci.Demuxing.Raw_Packet_Buffer_Size >= s.ProbeSize) || (sti.Probe_Packets <= 0) ? 1 : 0;

				if ((end != 0) || (IntMath.Av_Log2((c_uint)pd.Buf_Size) != IntMath.Av_Log2((c_uint)(pd.Buf_Size - pkt.Size))))
				{
					c_int score = Set_Codec_From_Probe_Data(s, st, pd);

					if (((st.CodecPar.Codec_Id != AvCodecId.None) && (score > AvProbe.Score_Stream_Retry)) || (end != 0))
					{
						pd.Buf_Size = 0;

						Mem.Av_FreeP(ref pd.Buf);

						sti.Request_Probe = -1;

						if (st.CodecPar.Codec_Id != AvCodecId.None)
							Log.Av_Log(s, Log.Av_Log_Debug, "probed stream %d\n", st.Index);
						else
							Log.Av_Log(s, Log.Av_Log_Warning, "probed stream %d failed\n", st.Index);
					}

					Force_Codec_Ids(s, st);
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Update_Wrap_Reference(AvFormatContext s, AvStream st, c_int stream_Index, AvPacket pkt)//XX 467
		{
			FFStream sti = Internal.FFStream(st);
			int64_t @ref = pkt.Dts;

			if (@ref == UtilConstants.Av_NoPts_Value)
				@ref = pkt.Pts;

			if ((sti.Pts_Wrap_Reference != UtilConstants.Av_NoPts_Value) || (st.Pts_Wrap_Bits >= 63) || (@ref == UtilConstants.Av_NoPts_Value) || (s.Correct_Ts_Overflow == 0))
				return 0;

			@ref &= (1L << st.Pts_Wrap_Bits) - 1;

			// Reference time stamp should be 60 s before first time stamp
			c_int pts_Wrap_Reference = (c_int)(@ref - Mathematics.Av_Rescale(60, st.Time_Base.Den, st.Time_Base.Num));

			// If first time stamp is not more than 1/8 and 60s before the wrap point, subtract rather than add wrap offset
			AvPtsWrap pts_Wrap_Behavior = (@ref < ((1L << st.Pts_Wrap_Bits) - (1L << st.Pts_Wrap_Bits - 3))) || (@ref < ((1L << st.Pts_Wrap_Bits) - Mathematics.Av_Rescale(60, st.Time_Base.Den, st.Time_Base.Num))) ? AvPtsWrap.Add_Offset : AvPtsWrap.Sub_Offset;

			AvProgram first_Program = AvFormat.Av_Find_Program_From_Stream(s, null, stream_Index);

			if (first_Program == null)
			{
				c_int default_Stream_Index = AvFormat.Av_Find_Default_Stream_Index(s);
				FFStream default_Sti = Internal.FFStream(s.Streams[default_Stream_Index]);

				if (default_Sti.Pts_Wrap_Reference == UtilConstants.Av_NoPts_Value)
				{
					for (c_uint i = 0; i < s.Nb_Streams; i++)
					{
						FFStream _sti = Internal.FFStream(s.Streams[i]);

						if (AvFormat.Av_Find_Program_From_Stream(s, null, (c_int)i) != null)
							continue;

						_sti.Pts_Wrap_Reference = pts_Wrap_Reference;
						_sti.Pts_Wrap_Behavior = pts_Wrap_Behavior;
					}
				}
				else
				{
					sti.Pts_Wrap_Reference = default_Sti.Pts_Wrap_Reference;
					sti.Pts_Wrap_Behavior = default_Sti.Pts_Wrap_Behavior;
				}
			}
			else
			{
				AvProgram program = first_Program;

				while (program != null)
				{
					if (program.Pts_Wrap_Reference != UtilConstants.Av_NoPts_Value)
					{
						pts_Wrap_Reference = (c_int)program.Pts_Wrap_Reference;
						pts_Wrap_Behavior = program.Pts_Wrap_Behaviour;
						break;
					}

					program = AvFormat.Av_Find_Program_From_Stream(s, program, stream_Index);
				}

				// Update every program with differing pts_wrap_reference
				program = first_Program;

				while (program != null)
				{
					if (program.Pts_Wrap_Reference != pts_Wrap_Reference)
					{
						for (c_uint i = 0; i < program.Nb_Stream_Indexes; i++)
						{
							FFStream _sti = Internal.FFStream(s.Streams[program.Stream_Index[i]]);

							_sti.Pts_Wrap_Reference = pts_Wrap_Reference;
							_sti.Pts_Wrap_Behavior = pts_Wrap_Behavior;
						}

						program.Pts_Wrap_Reference = pts_Wrap_Reference;
						program.Pts_Wrap_Behaviour = pts_Wrap_Behavior;
					}

					program = AvFormat.Av_Find_Program_From_Stream(s, program, stream_Index);
				}
			}

			return 1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Update_Timestamps(AvFormatContext s, AvStream st, AvPacket pkt)//XX 535
		{
			FFStream sti = Internal.FFStream(st);

			if ((Update_Wrap_Reference(s, st, pkt.Stream_Index, pkt) != 0) && (sti.Pts_Wrap_Behavior == AvPtsWrap.Sub_Offset))
			{
				// Correct first time stamps to negative values
				if (!AvFormatInternal.Is_Relative(sti.First_Dts))
					sti.First_Dts = Wrap_Timestamp(st, sti.First_Dts);

				if (!AvFormatInternal.Is_Relative(st.Start_Time))
					st.Start_Time = Wrap_Timestamp(st, st.Start_Time);

				if (!AvFormatInternal.Is_Relative(sti.Cur_Dts))
					sti.Cur_Dts = Wrap_Timestamp(st, sti.Cur_Dts);
			}

			pkt.Dts = Wrap_Timestamp(st, pkt.Dts);
			pkt.Pts = Wrap_Timestamp(st, pkt.Pts);

			Force_Codec_Ids(s, st);

			// TODO: audio: time filter; video: frame reordering (pts != dts)
			if (s.Use_Wallclock_As_Timestamps != 0)
				pkt.Dts = pkt.Pts = Mathematics.Av_Rescale_Q(Time.Av_GetTime(), UtilConstants.Av_Time_Base_Q, st.Time_Base);
		}



		/********************************************************************/
		/// <summary>
		/// Handle a new packet and either return it directly if possible
		/// and allow_passthrough is true or queue the packet (or drop the
		/// packet if corrupt)
		/// </summary>
		/********************************************************************/
		private static c_int Handle_New_Packet(AvFormatContext s, AvPacket pkt, c_int allow_Passthrough)//XX 567
		{
			FormatContextInternal fci = AvFormatInternal.FF_FC_Internal(s);

			if ((pkt.Flags & AvPktFlag.Corrupt) != 0)
			{
				Log.Av_Log(s, Log.Av_Log_Warning, "Packet corrupt (stream = %d, dts = %s)%s.\n", pkt.Stream_Index, Timestamp.Av_Ts2Str(pkt.Dts), (s.Flags & AvFmtFlag.Discard_Corrupt) != 0 ? ", dropping it" : string.Empty);

				if ((s.Flags & AvFmtFlag.Discard_Corrupt) != 0)
				{
					Packet.Av_Packet_Unref(pkt);

					return 1;
				}
			}

			AvStream st = s.Streams[pkt.Stream_Index];
			FFStream sti = Internal.FFStream(st);

			Update_Timestamps(s, st, pkt);

			if ((sti.Request_Probe <= 0) && (allow_Passthrough != 0) && (fci.Demuxing.Raw_Packet_Buffer.Head == null))
				return 0;

			c_int err = Packet.AvPriv_Packet_List_Put(fci.Demuxing.Raw_Packet_Buffer, pkt, null, FFPacketListFlag.None);

			if (err < 0)
			{
				Packet.Av_Packet_Unref(pkt);

				return err;
			}

			pkt = fci.Demuxing.Raw_Packet_Buffer.Tail.Pkt;
			fci.Demuxing.Raw_Packet_Buffer_Size += pkt.Size;

			err = Probe_Codec(s, st, pkt);

			if (err < 0)
				return err;

			return 1;
		}



		/********************************************************************/
		/// <summary>
		/// Return the frame duration in seconds. Return 0 if not available
		/// </summary>
		/********************************************************************/
		private static void Compute_Frame_Duration(AvFormatContext s, out c_int pNum, out c_int pDen, AvStream st, AvCodecParserContext pc, AvPacket pkt)//XX 686
		{
			FFStream sti = Internal.FFStream(st);
			AvRational codec_FrameRate = sti.AvCtx.FrameRate;

			pNum = 0;
			pDen = 0;

			switch (st.CodecPar.Codec_Type)
			{
				case AvMediaType.Video:
				{
					if ((st.R_Frame_Rate.Num != 0) && ((pc == null) || (codec_FrameRate.Num == 0)))
					{
						pNum = st.R_Frame_Rate.Den;
						pDen = st.R_Frame_Rate.Num;
					}
					else if (((s.IFormat.Flags & AvFmt.NoTimestamps) != 0) && (codec_FrameRate.Num == 0) && (st.Avg_Frame_Rate.Num != 0) && (st.Avg_Frame_Rate.Den != 0))
					{
						pNum = st.Avg_Frame_Rate.Den;
						pDen = st.Avg_Frame_Rate.Num;
					}
					else if ((st.Time_Base.Num * 1000L) > st.Time_Base.Den)
					{
						pNum = st.Time_Base.Num;
						pDen = st.Time_Base.Den;
					}
					else if ((codec_FrameRate.Den * 1000L) > codec_FrameRate.Num)
					{
						c_int ticks_Per_Frame = (sti.Codec_Desc != null) && ((sti.Codec_Desc.Props & AvCodecProp.Fields) != 0) ? 2 : 1;

						Rational.Av_Reduce(out pNum, out pDen, codec_FrameRate.Den, codec_FrameRate.Num * (int64_t)ticks_Per_Frame, c_int.MaxValue);

						if ((pc != null) && (pc.Repeat_Pict != 0))
							Rational.Av_Reduce(out pNum, out pDen, pNum * (1L + pc.Repeat_Pict), pDen, c_int.MaxValue);

						// If this codec can be interlaced or progressive then we need
						// a parser to compute duration of a packet. Thus if we have
						// no parser in such case leave duration undefined
						if ((sti.Codec_Desc != null) && ((sti.Codec_Desc.Props & AvCodecProp.Fields) != 0) && (pc == null))
							pNum = pDen = 0;
					}

					break;
				}

				case AvMediaType.Audio:
				{
					c_int frame_Size, sample_Rate;

					if (sti.AvCtx_Inited != 0)
					{
						frame_Size = Utils_Codec.Av_Get_Audio_Frame_Duration(sti.AvCtx, pkt.Size);
						sample_Rate = sti.AvCtx.Sample_Rate;
					}
					else
					{
						frame_Size = Utils_Codec.Av_Get_Audio_Frame_Duration2(st.CodecPar, pkt.Size);
						sample_Rate = st.CodecPar.Sample_Rate;
					}

					if ((frame_Size <= 0) || (sample_Rate <= 0))
						break;

					pNum = frame_Size;
					pDen = sample_Rate;
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static bool Has_Decode_Delay_Been_Guessed(AvStream st)//XX 749
		{
			FFStream sti = Internal.FFStream(st);

			if (st.CodecPar.Codec_Id != AvCodecId.H264)
				return true;

			if (sti.Info == null)	// If we have left find_stream_info then nb_decoded_frames won't increase anymore for stream copy
				return true;

			if (sti.AvCtx.Has_B_Frames < 3)
				return sti.Nb_Decoded_Frames >= 7;
			else if (sti.AvCtx.Has_B_Frames < 4)
				return sti.Nb_Decoded_Frames >= 18;
			else
				return sti.Nb_Decoded_Frames >= 20;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static PacketListEntry Get_Next_Pkt(AvFormatContext s, AvStream st, PacketListEntry pktl)//XX 768
		{
			FormatContextInternal fci = AvFormatInternal.FF_FC_Internal(s);
			FFFormatContext si = fci.Fc;

			if (pktl.Next != null)
				return pktl.Next;

			if (pktl == si.Packet_Buffer.Tail)
				return fci.Demuxing.Parse_Queue.Head;

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static int64_t Select_From_Pts_Buffer(AvStream st, CPointer<int64_t> pts_Buffer, int64_t dts)//XX 780
		{
			FFStream sti = Internal.FFStream(st);

			c_int oneIn_OneOut = (st.CodecPar.Codec_Id != AvCodecId.H264) && (st.CodecPar.Codec_Id != AvCodecId.Hevc) && (st.CodecPar.Codec_Id != AvCodecId.Vvc) ? 1 : 0;

			if (oneIn_OneOut == 0)
			{
				c_int delay = sti.AvCtx.Has_B_Frames;

				if (dts == UtilConstants.Av_NoPts_Value)
				{
					int64_t best_Score = int64_t.MaxValue;

					for (c_int i = 0; i < delay; i++)
					{
						if (sti.Pts_Reorder_Error_Count[i] != 0)
						{
							int64_t score = sti.Pts_Reorder_Error[i] / sti.Pts_Reorder_Error_Count[i];

							if (score < best_Score)
							{
								best_Score = score;
								dts = pts_Buffer[i];
							}
						}
					}
				}
				else
				{
					for (c_int i = 0; i < delay; i++)
					{
						if (pts_Buffer[i] != UtilConstants.Av_NoPts_Value)
						{
							int64_t diff = (int64_t)((uint64_t)Common.FFAbs(pts_Buffer[i] - dts) + (uint64_t)sti.Pts_Reorder_Error[i]);
							diff = Macros.FFMax(diff, sti.Pts_Reorder_Error[i]);

							sti.Pts_Reorder_Error[i] = diff;
							sti.Pts_Reorder_Error_Count[i]++;

							if (sti.Pts_Reorder_Error_Count[i] > 250)
							{
								sti.Pts_Reorder_Error[i] >>= 1;
								sti.Pts_Reorder_Error_Count[i] >>= 1;
							}
						}
					}
				}
			}

			if (dts == UtilConstants.Av_NoPts_Value)
				dts = pts_Buffer[0];

			return dts;
		}



		/********************************************************************/
		/// <summary>
		/// Updates the dts of packets of a stream in pkt_buffer, by
		/// re-ordering the pts of the packets in a window
		/// </summary>
		/********************************************************************/
		private static void Update_Dts_From_Pts(AvFormatContext s, c_int stream_Index, PacketListEntry pkt_Buffer)//XX 828
		{
			AvStream st = s.Streams[stream_Index];
			c_int delay = Internal.FFStream(st).AvCtx.Has_B_Frames;

			CPointer<int64_t> pts_Buffer = new CPointer<int64_t>(FFStream.Max_Reorder_Delay + 1);

			for (c_int i = 0; i < (FFStream.Max_Reorder_Delay + 1); i++)
				pts_Buffer[i] = UtilConstants.Av_NoPts_Value;

			for (; pkt_Buffer != null; pkt_Buffer = Get_Next_Pkt(s, st, pkt_Buffer))
			{
				if (pkt_Buffer.Pkt.Stream_Index != stream_Index)
					continue;

				if ((pkt_Buffer.Pkt.Pts != UtilConstants.Av_NoPts_Value) && (delay <= FFStream.Max_Reorder_Delay))
				{
					pts_Buffer[0] = pkt_Buffer.Pkt.Pts;

					for (c_int i = 0; (i < delay) && (pts_Buffer[i] > pts_Buffer[i + 1]); i++)
						Macros.FFSwap(ref pts_Buffer[i], ref pts_Buffer[i + 1]);

					pkt_Buffer.Pkt.Dts = Select_From_Pts_Buffer(st, pts_Buffer, pkt_Buffer.Pkt.Dts);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Update_Initial_Timestamps(AvFormatContext s, c_int stream_Index, int64_t dts, int64_t pts, AvPacket pkt)//XX 853
		{
			FormatContextInternal fci = AvFormatInternal.FF_FC_Internal(s);
			FFFormatContext si = fci.Fc;
			AvStream st = s.Streams[stream_Index];
			FFStream sti = Internal.FFStream(st);
			PacketListEntry pktl = si.Packet_Buffer.Head != null ? si.Packet_Buffer.Head : fci.Demuxing.Parse_Queue.Head;

			if ((sti.First_Dts != UtilConstants.Av_NoPts_Value) || (dts == UtilConstants.Av_NoPts_Value) || (sti.Cur_Dts == UtilConstants.Av_NoPts_Value) || (sti.Cur_Dts < (c_int.MinValue + AvFormatInternal.Relative_Ts_Base)) || (dts < (c_int.MinValue + (sti.Cur_Dts - AvFormatInternal.Relative_Ts_Base))) || AvFormatInternal.Is_Relative(dts))
				return;

			sti.First_Dts = dts - (sti.Cur_Dts - AvFormatInternal.Relative_Ts_Base);
			sti.Cur_Dts = dts;
			uint64_t shift = (uint64_t)sti.First_Dts - AvFormatInternal.Relative_Ts_Base;

			if (AvFormatInternal.Is_Relative(pts))
				pts = (int64_t)((uint64_t)pts + shift);

			for (PacketListEntry pktl_it = pktl; pktl_it != null; pktl_it = Get_Next_Pkt(s, st, pktl_it))
			{
				if (pktl_it.Pkt.Stream_Index != stream_Index)
					continue;

				if (AvFormatInternal.Is_Relative(pktl_it.Pkt.Pts))
					pktl_it.Pkt.Pts = (int64_t)((uint64_t)pktl_it.Pkt.Pts + shift);

				if (AvFormatInternal.Is_Relative(pktl_it.Pkt.Dts))
					pktl_it.Pkt.Dts = (int64_t)((uint64_t)pktl_it.Pkt.Dts + shift);

				if ((st.Start_Time == UtilConstants.Av_NoPts_Value) && (pktl_it.Pkt.Pts != UtilConstants.Av_NoPts_Value))
				{
					st.Start_Time = pktl_it.Pkt.Pts;

					if ((st.CodecPar.Codec_Type == AvMediaType.Audio) && (st.CodecPar.Sample_Rate != 0))
						st.Start_Time = Common.Av_Sat_Add64(st.Start_Time, Mathematics.Av_Rescale_Q(sti.Skip_Samples, new AvRational(1, st.CodecPar.Sample_Rate), st.Time_Base));
				}
			}

			if (Has_Decode_Delay_Been_Guessed(st))
				Update_Dts_From_Pts(s, stream_Index, pktl);

			if (st.Start_Time == UtilConstants.Av_NoPts_Value)
			{
				if ((st.CodecPar.Codec_Type == AvMediaType.Audio) || ((pkt.Flags & AvPktFlag.Discard) == 0))
					st.Start_Time = pts;

				if ((st.CodecPar.Codec_Type == AvMediaType.Audio) && (st.CodecPar.Sample_Rate != 0))
					st.Start_Time = Common.Av_Sat_Add64(st.Start_Time, Mathematics.Av_Rescale_Q(sti.Skip_Samples, new AvRational(1, st.CodecPar.Sample_Rate), st.Time_Base));
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Update_Initial_Durations(AvFormatContext s, AvStream st, c_int stream_Index, int64_t duration)//XX 907
		{
			FormatContextInternal fci = AvFormatInternal.FF_FC_Internal(s);
			FFFormatContext si = fci.Fc;
			FFStream sti = Internal.FFStream(st);
			PacketListEntry pktl = si.Packet_Buffer.Head != null ? si.Packet_Buffer.Head : fci.Demuxing.Parse_Queue.Head;
			int64_t cur_Dts = AvFormatInternal.Relative_Ts_Base;

			if (sti.First_Dts != UtilConstants.Av_NoPts_Value)
			{
				if (sti.Update_Initial_Durations_Done != 0)
					return;

				sti.Update_Initial_Durations_Done = 1;
				cur_Dts = sti.First_Dts;

				for (; pktl != null; pktl = Get_Next_Pkt(s, st, pktl))
				{
					if (pktl.Pkt.Stream_Index == stream_Index)
					{
						if ((pktl.Pkt.Pts != pktl.Pkt.Dts) || (pktl.Pkt.Dts != UtilConstants.Av_NoPts_Value) || (pktl.Pkt.Duration != 0))
							break;

						cur_Dts -= duration;
					}
				}

				if ((pktl != null) && (pktl.Pkt.Dts != sti.First_Dts))
				{
					Log.Av_Log(s, Log.Av_Log_Debug, "first_dts %s not matching first dts %s (pts %s, duration %lld in the queue\n", Timestamp.Av_Ts2Str(sti.First_Dts), Timestamp.Av_Ts2Str(pktl.Pkt.Dts), Timestamp.Av_Ts2Str(pktl.Pkt.Pts), pktl.Pkt.Duration);

					return;
				}

				if (pktl == null)
				{
					Log.Av_Log(s, Log.Av_Log_Debug, "first_dts %s but no packet with dts in the queue\n", Timestamp.Av_Ts2Str(sti.First_Dts));

					return;
				}

				pktl = si.Packet_Buffer.Head != null ? si.Packet_Buffer.Head : fci.Demuxing.Parse_Queue.Head;
				sti.First_Dts = cur_Dts;
			}
			else if (sti.Cur_Dts != AvFormatInternal.Relative_Ts_Base)
				return;

			for (; pktl != null; pktl = Get_Next_Pkt(s, st, pktl))
			{
				if (pktl.Pkt.Stream_Index != stream_Index)
					continue;

				if (((pktl.Pkt.Pts == pktl.Pkt.Dts) || (pktl.Pkt.Pts == UtilConstants.Av_NoPts_Value)) && ((pktl.Pkt.Dts == UtilConstants.Av_NoPts_Value) || (pktl.Pkt.Dts == sti.First_Dts) || (pktl.Pkt.Dts == AvFormatInternal.Relative_Ts_Base)) && (pktl.Pkt.Duration == 0) && ((uint64_t)Common.Av_Sat_Add64(cur_Dts, duration) == ((uint64_t)cur_Dts + (uint64_t)duration)))
				{
					pktl.Pkt.Dts = cur_Dts;

					if (sti.AvCtx.Has_B_Frames == 0)
						pktl.Pkt.Pts = cur_Dts;

					pktl.Pkt.Duration = duration;
				}
				else
					break;

				cur_Dts = pktl.Pkt.Dts + pktl.Pkt.Duration;
			}

			if (pktl == null)
				sti.Cur_Dts = cur_Dts;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Compute_Pkt_Fields(AvFormatContext s, AvStream st, AvCodecParserContext pc, AvPacket pkt, int64_t next_Dts, int64_t next_Pts)//XX 967
		{
			FormatContextInternal fci = AvFormatInternal.FF_FC_Internal(s);
			FFFormatContext si = fci.Fc;
			FFStream sti = Internal.FFStream(st);

			c_int oneIn_OneOut = (st.CodecPar.Codec_Id != AvCodecId.H264) && (st.CodecPar.Codec_Id != AvCodecId.Hevc) && (st.CodecPar.Codec_Id != AvCodecId.Vvc) ? 1 : 0;

			if ((s.Flags & AvFmtFlag.NoFillIn) != 0)
				return;

			if ((st.CodecPar.Codec_Type == AvMediaType.Video) && (pkt.Dts != UtilConstants.Av_NoPts_Value))
			{
				if ((pkt.Dts == pkt.Pts) && (sti.Last_Dts_For_Order_Check != UtilConstants.Av_NoPts_Value))
				{
					if (sti.Last_Dts_For_Order_Check <= pkt.Pts)
						sti.Dts_Ordered++;
					else
					{
						Log.Av_Log(s, sti.Dts_Misordered != 0 ? Log.Av_Log_Debug : Log.Av_Log_Warning, "DTS %lld < %lld out of order\n", pkt.Dts, sti.Last_Dts_For_Order_Check);

						sti.Dts_Misordered++;
					}

					if ((sti.Dts_Ordered + sti.Dts_Misordered) > 250)
					{
						sti.Dts_Ordered >>= 1;
						sti.Dts_Misordered >>= 1;
					}
				}

				sti.Last_Dts_For_Order_Check = pkt.Dts;

				if ((sti.Dts_Ordered < (8 * sti.Dts_Misordered)) && (pkt.Dts == pkt.Pts))
					pkt.Dts = UtilConstants.Av_NoPts_Value;
			}

			if (((s.Flags & AvFmtFlag.IgnDts) != 0) && (pkt.Pts != UtilConstants.Av_NoPts_Value))
				pkt.Dts = UtilConstants.Av_NoPts_Value;

			if ((pc != null) && (pc.Pict_Type == AvPictureType.B) && (sti.AvCtx.Has_B_Frames == 0))
			{
				// FIXME Set low_delay = 0 when has_b_frames = 1
				sti.AvCtx.Has_B_Frames = 1;
			}

			// Do we have a video B-frame?
			c_int delay = sti.AvCtx.Has_B_Frames;
			c_int presentation_Delayed = 0;

			// XXX: need has_b_frame, but cannot get it if the codec is
			// not initialized
			if ((delay != 0) && (pc != null) && (pc.Pict_Type != AvPictureType.B))
				presentation_Delayed = 1;

			if ((pkt.Pts != UtilConstants.Av_NoPts_Value) && (pkt.Dts != UtilConstants.Av_NoPts_Value) && (st.Pts_Wrap_Bits < 63) && (pkt.Dts > (int64_t.MinValue + (1L << st.Pts_Wrap_Bits))) && ((pkt.Dts - (1L << (st.Pts_Wrap_Bits - 1))) > pkt.Dts))
			{
				if (AvFormatInternal.Is_Relative(sti.Cur_Dts) || ((pkt.Dts - (1L << (st.Pts_Wrap_Bits - 1))) > sti.Cur_Dts))
					pkt.Dts -= 1L << st.Pts_Wrap_Bits;
				else
					pkt.Dts += 1L << st.Pts_Wrap_Bits;
			}

			// Some MPEG-2 in MPEG-PS lack dts (issue #171 / input_file.mpg).
			// We take the conservative approach and discard both.
			// Note: If this is misbehaving for an H.264 file, then possibly
			// presentation_delayed is not set correctly
			if ((delay == 1) && (pkt.Dts == pkt.Pts) && (pkt.Dts != UtilConstants.Av_NoPts_Value) && (presentation_Delayed != 0))
			{
				Log.Av_Log(s, Log.Av_Log_Debug, "invalid dts/pts combination %lld\n", pkt.Dts);

				if ((CString.strcmp(s.IFormat.Name, "mov,mp4,m4a,3gp,3g2,mj2") != 0) && (CString.strcmp(s.IFormat.Name, "flv") != 0))	// Otherwise we discard correct timestamps for vc1-wmapro.ism
					pkt.Dts = UtilConstants.Av_NoPts_Value;
			}

			AvRational duration = Rational.Av_Mul_Q(new AvRational((c_int)pkt.Duration, 1), st.Time_Base);

			if (pkt.Duration <= 0)
			{
				Compute_Frame_Duration(s, out c_int num, out c_int den, st, pc, pkt);

				if ((den != 0) && (num != 0))
				{
					duration = new AvRational(num, den);
					pkt.Duration = Mathematics.Av_Rescale_Rnd(1, num * (int64_t)st.Time_Base.Den, den * (int64_t)st.Time_Base.Num, AvRounding.Down);
				}
			}

			if ((pkt.Duration > 0) && ((si.Packet_Buffer.Head != null) || (fci.Demuxing.Parse_Queue.Head != null)))
				Update_Initial_Durations(s, st, pkt.Stream_Index, pkt.Duration);

			// Correct timestamps with byte offset if demuxers only have timestamps
			// on packet boundaries
			if ((pc != null) && (sti.Need_Parsing == AvStreamParseType.Timestamps) && (pkt.Size != 0))
			{
				// This will estimate bitrate based on this frame's duration and size
				int64_t offset = Mathematics.Av_Rescale(pc.Offset, pkt.Duration, pkt.Size);

				if (pkt.Pts != UtilConstants.Av_NoPts_Value)
					pkt.Pts += offset;

				if (pkt.Dts != UtilConstants.Av_NoPts_Value)
					pkt.Dts += offset;
			}

			// This may be redundant, but it should not hurt
			if ((pkt.Dts != UtilConstants.Av_NoPts_Value) && (pkt.Pts != UtilConstants.Av_NoPts_Value) && (pkt.Pts > pkt.Dts))
				presentation_Delayed = 1;

			// Interpolate PTS and DTS if they are not present. We skip H264
			// currently because delay and has_b_frames are not reliably set
			if (((delay == 0) || ((delay == 1) && (pc != null))) && (oneIn_OneOut != 0))
			{
				if (presentation_Delayed != 0)
				{
					// DTS = decompression timestamp
					// PTS = presentation timestamp
					if (pkt.Dts == UtilConstants.Av_NoPts_Value)
						pkt.Dts = sti.Last_IP_Pts;

					Update_Initial_Timestamps(s, pkt.Stream_Index, pkt.Dts, pkt.Pts, pkt);

					if (pkt.Dts == UtilConstants.Av_NoPts_Value)
						pkt.Dts = sti.Cur_Dts;

					// This is tricky: the dts must be incremented by the duration
					// of the frame we are displaying, i.e. the last I- or P-frame
					if ((sti.Last_IP_Duration == 0) && ((uint64_t)pkt.Duration <= int32_t.MaxValue))
						sti.Last_IP_Duration = (c_int)pkt.Duration;

					if (pkt.Dts != UtilConstants.Av_NoPts_Value)
						sti.Cur_Dts = Common.Av_Sat_Add64(pkt.Dts, sti.Last_IP_Duration);

					if ((pkt.Dts != UtilConstants.Av_NoPts_Value) && (pkt.Pts != UtilConstants.Av_NoPts_Value) && (sti.Last_IP_Duration > 0) && (((uint64_t)sti.Cur_Dts - (uint64_t)next_Dts + 1) <= 2) && (next_Dts != next_Pts) && (next_Pts != UtilConstants.Av_NoPts_Value))
						pkt.Pts = next_Dts;

					if ((uint64_t)pkt.Duration <= int32_t.MaxValue)
						sti.Last_IP_Duration = (c_int)pkt.Duration;

					sti.Last_IP_Pts = pkt.Pts;

					// Cannot compute PTS if not present (we can compute it only
					// by knowing the future
				}
				else if ((pkt.Pts != UtilConstants.Av_NoPts_Value) || (pkt.Dts != UtilConstants.Av_NoPts_Value) || (pkt.Duration > 0))
				{
					// Presentation is not delayed : PTS and DTS are the same
					if (pkt.Pts == UtilConstants.Av_NoPts_Value)
						pkt.Pts = pkt.Dts;

					Update_Initial_Timestamps(s, pkt.Stream_Index, pkt.Pts, pkt.Pts, pkt);

					if (pkt.Pts == UtilConstants.Av_NoPts_Value)
						pkt.Pts = sti.Cur_Dts;

					pkt.Dts = pkt.Pts;

					if ((pkt.Pts != UtilConstants.Av_NoPts_Value) && (duration.Num >= 0))
						sti.Cur_Dts = Mathematics.Av_Add_Stable(st.Time_Base, pkt.Pts, duration, 1);
				}
			}

			if ((pkt.Pts != UtilConstants.Av_NoPts_Value) && (delay <= FFStream.Max_Reorder_Delay))
			{
				sti.Pts_Buffer[0] = pkt.Pts;

				for (c_int i = 0; (i < delay) && (sti.Pts_Buffer[i] > sti.Pts_Buffer[i + 1]); i++)
					Macros.FFSwap(ref sti.Pts_Buffer[i], ref sti.Pts_Buffer[i + 1]);

				if (Has_Decode_Delay_Been_Guessed(st))
					pkt.Dts = Select_From_Pts_Buffer(st, sti.Pts_Buffer, pkt.Dts);
			}

			// We skipped it above so we try here
			if (oneIn_OneOut == 0)
			{
				// This should happen on the first packet
				Update_Initial_Timestamps(s, pkt.Stream_Index, pkt.Dts, pkt.Pts, pkt);
			}

			if (pkt.Dts > sti.Cur_Dts)
				sti.Cur_Dts = pkt.Dts;

			// Update flags
			if ((st.CodecPar.Codec_Type == AvMediaType.Data) || AvFormat.FF_Is_Intra_Only(st.CodecPar.Codec_Id))
				pkt.Flags |= AvPktFlag.Key;
		}



		/********************************************************************/
		/// <summary>
		/// Parse a packet, add all split parts to parse_queue
		/// </summary>
		/********************************************************************/
		private static c_int Parse_Packet(AvFormatContext s, AvPacket pkt, c_int stream_Index, bool flush)//XX 1162
		{
			FormatContextInternal fci = AvFormatInternal.FF_FC_Internal(s);
			FFFormatContext si = fci.Fc;
			AvPacket out_Pkt = si.Parse_Pkt;
			AvStream st = s.Streams[stream_Index];
			FFStream sti = Internal.FFStream(st);
			CPointer<uint8_t> data = pkt.Data;
			c_int size = pkt.Size;
			c_int ret = 0;
			bool got_Output = flush;

			if ((size == 0) && !flush && ((sti.Parser.Flags & ParserFlag.Complete_Frames) != 0))
			{
				// Preserve 0-size sync packets
				Compute_Pkt_Fields(s, st, sti.Parser, pkt, pkt.Dts, pkt.Pts);

				// Theora has valid 0-sized packets that need to be output
				if (st.CodecPar.Codec_Id == AvCodecId.Theora)
				{
					ret = Packet.AvPriv_Packet_List_Put(fci.Demuxing.Parse_Queue, pkt, null, FFPacketListFlag.None);

					if (ret < 0)
						goto Fail;
				}
			}

			while ((size > 0) || (flush && got_Output))
			{
				int64_t next_Pts = pkt.Pts;
				int64_t next_Dts = pkt.Dts;

				c_int len = Parser.Av_Parser_Parse2(sti.Parser, sti.AvCtx, out out_Pkt.Data, out out_Pkt.Size, data, size, pkt.Pts, pkt.Dts, pkt.Pos);

				pkt.Pts = pkt.Dts = UtilConstants.Av_NoPts_Value;
				pkt.Pos = -1;

				// Increment read pointer
				data = len != 0 ? data + len : data;
				size -= len;

				got_Output = out_Pkt.Size != 0;

				if (out_Pkt.Size == 0)
					continue;

				if ((pkt.Buf != null) && (out_Pkt.Data == pkt.Data))
				{
					// Reference pkt->buf only when out_pkt->data is guaranteed to point
					// to data in it and not in the parser's internal buffer.
					// XXX: Ensure this is the case with all parsers when sti->parser->flags
					// is PARSER_FLAG_COMPLETE_FRAMES and check for that instead?
					out_Pkt.Buf = Buffer.Av_Buffer_Ref(pkt.Buf);

					if (out_Pkt.Buf == null)
					{
						ret = Error.ENOMEM;

						goto Fail;
					}
				}
				else
				{
					ret = Packet.Av_Packet_Make_RefCounted(out_Pkt);

					if (ret < 0)
						goto Fail;
				}

				if (pkt.Side_Data.IsNotNull)
				{
					out_Pkt.Side_Data = pkt.Side_Data;
					out_Pkt.Side_Data_Elems = pkt.Side_Data_Elems;

					pkt.Side_Data.SetToNull();
					pkt.Side_Data_Elems = 0;
				}

				// Set the duration
				out_Pkt.Duration = (sti.Parser.Flags & ParserFlag.Complete_Frames) != 0 ? pkt.Duration : 0;

				if (st.CodecPar.Codec_Type == AvMediaType.Audio)
				{
					if (sti.AvCtx.Sample_Rate > 0)
						out_Pkt.Duration = Mathematics.Av_Rescale_Q_Rnd(sti.Parser.Duration, new AvRational(1, sti.AvCtx.Sample_Rate), st.Time_Base, AvRounding.Down);
				}
				else if (st.CodecPar.Codec_Id == AvCodecId.Gif)
				{
					if ((st.Time_Base.Num > 0) && (st.Time_Base.Den > 0) && (sti.Parser.Duration != 0))
						out_Pkt.Duration = sti.Parser.Duration;
				}

				out_Pkt.Stream_Index = st.Index;
				out_Pkt.Pts = sti.Parser.Pts;
				out_Pkt.Dts = sti.Parser.Dts;
				out_Pkt.Pos = sti.Parser.Pos;
				out_Pkt.Flags |= pkt.Flags & (AvPktFlag.Discard | AvPktFlag.Corrupt);

				if (sti.Need_Parsing == AvStreamParseType.Full_Raw)
					out_Pkt.Pos = sti.Parser.Frame_Offset;

				if ((sti.Parser.Key_Frame == 1) || ((sti.Parser.Key_Frame == -1) && (sti.Parser.Pict_Type == AvPictureType.I)))
					out_Pkt.Flags |= AvPktFlag.Key;

				if ((sti.Parser.Key_Frame == -1) && (sti.Parser.Pict_Type == AvPictureType.None) && ((pkt.Flags & AvPktFlag.Key) != 0))
					out_Pkt.Flags |= AvPktFlag.Key;

				Compute_Pkt_Fields(s, st, sti.Parser, out_Pkt, next_Dts, next_Pts);

				ret = Packet.AvPriv_Packet_List_Put(fci.Demuxing.Parse_Queue, out_Pkt, null, FFPacketListFlag.None);

				if (ret < 0)
					goto Fail;
			}

			// End of the stream => close and free the parser
			if (flush)
			{
				Parser.Av_Parser_Close(sti.Parser);
				sti.Parser = null;
			}

			Fail:
			if (ret < 0)
				Packet.Av_Packet_Unref(out_Pkt);

			Packet.Av_Packet_Unref(pkt);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static int64_t Ts_To_Samples(AvStream st, int64_t ts)//XX 1286
		{
			return Mathematics.Av_Rescale(ts, st.Time_Base.Num * st.CodecPar.Sample_Rate, st.Time_Base.Den);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Codec_Close(FFStream sti)//XX 1291
		{
			AvCodecParameters par_Tmp = null;
			AvCodec new_Codec = sti.AvCtx.Codec_Id != sti.Pub.CodecPar.Codec_Id ? AllCodec.AvCodec_Find_Decoder(sti.Pub.CodecPar.Codec_Id) : sti.AvCtx.Codec;
			c_int ret;

			AvCodecContext avCtx_New = Options_Codec.AvCodec_Alloc_Context3(new_Codec);

			if (avCtx_New == null)
			{
				ret = Error.ENOMEM;

				goto Fail;
			}

			par_Tmp = Codec_Par.AvCodec_Parameters_Alloc();

			if (par_Tmp == null)
			{
				ret = Error.ENOMEM;

				goto Fail;
			}

			ret = Codec_Par.AvCodec_Parameters_From_Context(par_Tmp, sti.AvCtx);

			if (ret < 0)
				goto Fail;

			ret = Codec_Par.AvCodec_Parameters_To_Context(avCtx_New, par_Tmp);

			if (ret < 0)
				goto Fail;

			avCtx_New.Pkt_TimeBase = sti.AvCtx.Pkt_TimeBase;

			Options_Codec.AvCodec_Free_Context(ref sti.AvCtx);
			sti.AvCtx = avCtx_New;

			avCtx_New = null;
			ret = 0;

			Fail:
			Options_Codec.AvCodec_Free_Context(ref avCtx_New);
			Codec_Par.AvCodec_Parameters_Free(ref par_Tmp);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Read_Frame_Internal(AvFormatContext s, AvPacket pkt)//XX 1340
		{
			FormatContextInternal fci = AvFormatInternal.FF_FC_Internal(s);
			FFFormatContext si = fci.Fc;
			c_int got_Packet = 0;
			c_int ret = 0;
			AvDictionary metadata = null;

			while ((got_Packet == 0) && (fci.Demuxing.Parse_Queue.Head == null))
			{
				// Read next packet
				ret = FF_Read_Packet(s, pkt);

				if (ret < 0)
				{
					if (ret == Error.EAGAIN)
						return ret;

					// Flush the parsers
					for (c_uint i = 0; i < s.Nb_Streams; i++)
					{
						AvStream _st = s.Streams[i];
						FFStream _sti = Internal.FFStream(_st);

						if ((_sti.Parser != null) && (_sti.Need_Parsing != AvStreamParseType.None))
							Parse_Packet(s, pkt, _st.Index, true);
					}

					// All remaining packets are now in parse_queue =>
					// really terminate parsing
					break;
				}

				ret = 0;
				AvStream st = s.Streams[pkt.Stream_Index];
				FFStream sti = Internal.FFStream(st);

				st.Event_Flags |= AvStreamEventFlag.New_Packets;

				// Update context if required
				if (sti.Need_Context_Update != 0)
				{
					if (AvCodec_.AvCodec_Is_Open(sti.AvCtx))
					{
						Log.Av_Log(s, Log.Av_Log_Debug, "Demuxer context update while decoder is open, closing and trying to re-open\n");

						ret = Codec_Close(sti);

						sti.Info.Found_Decoder = 0;

						if (ret < 0)
							return ret;
					}

					// Close parser, because it depends on the codec
					if ((sti.Parser != null) && (sti.AvCtx.Codec_Id != st.CodecPar.Codec_Id))
					{
						Parser.Av_Parser_Close(sti.Parser);

						sti.Parser = null;
					}

					ret = Codec_Par.AvCodec_Parameters_To_Context(sti.AvCtx, st.CodecPar);

					if (ret < 0)
					{
						Packet.Av_Packet_Unref(pkt);

						return ret;
					}

					if (sti.AvCtx.ExtraData == null)
					{
						sti.Extract_ExtraData.Inited = 0;

						ret = Extract_ExtraData(si, st, pkt);

						if (ret < 0)
						{
							Packet.Av_Packet_Unref(pkt);

							return ret;
						}
					}

					sti.Codec_Desc = Codec_Desc.AvCodec_Descriptor_Get(sti.AvCtx.Codec_Id);

					sti.Need_Context_Update = 0;
				}

				if ((pkt.Pts != UtilConstants.Av_NoPts_Value) && (pkt.Dts != UtilConstants.Av_NoPts_Value) && (pkt.Pts < pkt.Dts))
					Log.Av_Log(s, Log.Av_Log_Warning, "Invalid timestamps stream=%d, pts=%s, dts=%s, size=%d\n", pkt.Stream_Index, Timestamp.Av_Ts2Str(pkt.Pts), Timestamp.Av_Ts2Str(pkt.Dts), pkt.Size);

				if ((sti.Need_Parsing != AvStreamParseType.None) && (sti.Parser != null) && ((s.Flags & AvFmtFlag.NoParse) == 0))
				{
					sti.Parser = Parser.Av_Parser_Init(st.CodecPar.Codec_Id);

					if (sti.Parser == null)
					{
						Log.Av_Log(s, Log.Av_Log_Verbose, "parser not found for codec %s, packets or times may be invalid.\n", Utils_Codec.AvCodec_Get_Name(st.CodecPar.Codec_Id));

						// No parser available: just output the raw packets
						sti.Need_Parsing = AvStreamParseType.None;
					}
					else if (sti.Need_Parsing == AvStreamParseType.Headers)
						sti.Parser.Flags |= ParserFlag.Complete_Frames;
					else if (sti.Need_Parsing == AvStreamParseType.Full_Once)
						sti.Parser.Flags |= ParserFlag.Once;
					else if (sti.Need_Parsing == AvStreamParseType.Full_Raw)
						sti.Parser.Flags |= ParserFlag.Use_Codec_Ts;
				}

				if ((sti.Need_Parsing != AvStreamParseType.None) || (sti.Parser == null))
				{
					// No parsing needed: we just output the packet as is
					Compute_Pkt_Fields(s, st, null, pkt, UtilConstants.Av_NoPts_Value, UtilConstants.Av_NoPts_Value);

					if (((s.IFormat.Flags & AvFmt.Generic_Index) != 0) && ((pkt.Flags & AvPktFlag.Key) != 0) && (pkt.Dts != UtilConstants.Av_NoPts_Value))
					{
						Seek.FF_Reduce_Index(s, st.Index);
						Seek.Av_Add_Index_Entry(st, pkt.Pos, pkt.Dts, 0, 0, AvIndex.KeyFrame);
					}

					got_Packet = 1;
				}
				else if (st.Discard < AvDiscard.All)
				{
					ret = Parse_Packet(s, pkt, pkt.Stream_Index, false);

					if (ret < 0)
						return ret;

					st.CodecPar.Sample_Rate = sti.AvCtx.Sample_Rate;
					st.CodecPar.Bit_Rate = sti.AvCtx.Bit_Rate;

					ret = Channel_Layout.Av_Channel_Layout_Copy(st.CodecPar.Ch_Layout, sti.AvCtx.Ch_Layout);

					if (ret < 0)
						return ret;

					st.CodecPar.Codec_Id = sti.AvCtx.Codec_Id;
				}
				else
				{
					// Free packet
					Packet.Av_Packet_Unref(pkt);
				}

				if ((pkt.Flags & AvPktFlag.Key) != 0)
					sti.Skip_To_Keyframe = 0;

				if (sti.Skip_To_Keyframe != 0)
				{
					Packet.Av_Packet_Unref(pkt);

					got_Packet = 0;
				}
			}

			if ((got_Packet == 0) && (fci.Demuxing.Parse_Queue.Head != null))
				ret = Packet.AvPriv_Packet_List_Get(fci.Demuxing.Parse_Queue, pkt);

			if (ret >= 0)
			{
				AvStream st = s.Streams[pkt.Stream_Index];
				FFStream sti = Internal.FFStream(st);
				c_int discard_Padding = 0;

				if ((sti.First_Discard_Sample != 0) && (pkt.Pts != UtilConstants.Av_NoPts_Value))
				{
					int64_t pts = pkt.Pts - (AvFormatInternal.Is_Relative(pkt.Pts) ? AvFormatInternal.Relative_Ts_Base : 0);
					int64_t sample = Ts_To_Samples(st, pts);
					int64_t duration = Ts_To_Samples(st, pkt.Duration);
					int64_t end_Sample = sample + duration;

					if ((duration > 0) && (end_Sample >= sti.First_Discard_Sample) && (sample < sti.Last_Discard_Sample))
						discard_Padding = (c_int)Macros.FFMin(end_Sample - sti.First_Discard_Sample, duration);
				}

				if ((sti.Start_Skip_Samples != 0) && ((pkt.Pts == 0) || (pkt.Pts == AvFormatInternal.Relative_Ts_Base)))
					sti.Skip_Samples = (c_int)sti.Start_Skip_Samples;

				sti.Skip_Samples = Macros.FFMax(0, sti.Skip_Samples);

				if ((sti.Skip_Samples != 0) || (discard_Padding != 0))
				{
					DataBufferContext p = Packet.Av_Packet_New_Side_Data(pkt, AvPacketSideDataType.Skip_Samples, 10);

					if (p == null)
					{
						IntReadWrite.Av_WL32(p.Data, (uint32_t)sti.Skip_Samples);
						IntReadWrite.Av_WL32(p.Data + 4, (uint32_t)discard_Padding);

						Log.Av_Log(s, Log.Av_Log_Debug, "demuxer injecting skip %u / discard %u\n", (c_uint)sti.Skip_Samples, (c_uint)discard_Padding);
					}

					sti.Skip_Samples = 0;
				}
			}

			if (fci.Demuxing.MetaFree == 0)
			{
				c_int metaRet = Opt.Av_Opt_Get_Dict_Val(s, "metadata".ToCharPointer(), AvOptSearch.Search_Children, ref metadata);

				if (metadata != null)
				{
					s.Event_Flags |= AvFmtEventFlag.Metadata_Updated;

					Dict.Av_Dict_Copy(ref s.Metadata, metadata, AvDict.None);
					Dict.Av_Dict_Free(ref metadata);

					Opt.Av_Opt_Set_Dict_Val(s, "metadata".ToCharPointer(), null, AvOptSearch.Search_Children);
				}

				fci.Demuxing.MetaFree = metaRet == Error.Option_Not_Found ? 1 : 0;
			}

			// A demuxer might have returned EOF because of an IO error, let's
			// propagate this back to the user
			if ((ret == Error.EOF) && (s.Pb != null) && (s.Pb.Error < 0) && (s.Pb.Error != Error.EAGAIN))
				ret = s.Pb.Error;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Return TRUE if the stream has accurate duration in any stream
		/// </summary>
		/********************************************************************/
		private static bool Has_Duration(AvFormatContext ic)//XX 1634
		{
			for (c_uint i = 0; i < ic.Nb_Streams; i++)
			{
				AvStream st = ic.Streams[i];

				if (st.Duration != UtilConstants.Av_NoPts_Value)
					return true;
			}

			if (ic.Duration != UtilConstants.Av_NoPts_Value)
				return true;

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Estimate the stream timings from the one of each components.
		///
		/// Also computes the global bitrate if possible
		/// </summary>
		/********************************************************************/
		private static void Update_Stream_Timings(AvFormatContext ic)//XX 1651
		{
			int64_t fileSize;

			int64_t start_Time = int64_t.MaxValue;
			int64_t start_Time_Text = int64_t.MaxValue;
			int64_t end_Time = int64_t.MinValue;
			int64_t end_Time_Text = int64_t.MinValue;
			int64_t duration = int64_t.MinValue;
			int64_t duration_Text = int64_t.MinValue;

			for (uint i = 0; i < ic.Nb_Streams; i++)
			{
				AvStream st = ic.Streams[i];

				bool is_Text = (st.CodecPar.Codec_Type == AvMediaType.Subtitle) || (st.CodecPar.Codec_Type == AvMediaType.Data);

				if ((st.Start_Time != UtilConstants.Av_NoPts_Value) && (st.Time_Base.Den != 0))
				{
					int64_t start_Time1 = Mathematics.Av_Rescale_Q(st.Start_Time, st.Time_Base, UtilConstants.Av_Time_Base_Q);

					if (is_Text)
						start_Time_Text = Macros.FFMin(start_Time_Text, start_Time1);
					else
						start_Time = Macros.FFMin(start_Time, start_Time1);

					int64_t end_Time1 = Mathematics.Av_Rescale_Q_Rnd(st.Duration, st.Time_Base, UtilConstants.Av_Time_Base_Q, AvRounding.Near_Inf | AvRounding.Pass_MinMax);

					if ((end_Time1 != UtilConstants.Av_NoPts_Value) && (end_Time1 > 0 ? start_Time1 <= (int64_t.MaxValue - end_Time1) : start_Time1 >= (int64_t.MinValue - end_Time1)))
					{
						end_Time1 += start_Time1;

						if (is_Text)
							end_Time_Text = Macros.FFMax(end_Time_Text, end_Time1);
						else
							end_Time = Macros.FFMax(end_Time, end_Time1);
					}

					for (AvProgram p = null; (p = AvFormat.Av_Find_Program_From_Stream(ic, p, (c_int)i)) != null;)
					{
						if ((p.Start_Time == UtilConstants.Av_NoPts_Value) || (p.Start_Time > start_Time1))
							p.Start_Time = start_Time1;

						if (p.End_Time < end_Time1)
							p.End_Time = end_Time1;
					}
				}

				if (st.Duration != UtilConstants.Av_NoPts_Value)
				{
					int64_t duration1 = Mathematics.Av_Rescale_Q(st.Duration, st.Time_Base, UtilConstants.Av_Time_Base_Q);

					if (is_Text)
						duration_Text = Macros.FFMax(duration_Text, duration1);
					else
						duration = Macros.FFMax(duration, duration1);
				}
			}

			if ((start_Time == int64_t.MaxValue) || ((start_Time > start_Time_Text) && ((uint64_t)(start_Time - start_Time_Text) < UtilConstants.Av_Time_Base)))
				start_Time = start_Time_Text;
			else if (start_Time > start_Time_Text)
				Log.Av_Log(ic, Log.Av_Log_Verbose, "Ignoring outlier non primary stream starttime %f\n", start_Time_Text / (c_float)UtilConstants.Av_Time_Base);

			if ((end_Time == int64_t.MinValue) || ((end_Time < end_Time_Text) && ((uint64_t)(end_Time_Text - end_Time) < UtilConstants.Av_Time_Base)))
				end_Time = end_Time_Text;
			else if (end_Time < end_Time_Text)
				Log.Av_Log(ic, Log.Av_Log_Verbose, "Ignoring outlier non primary stream endtime %f\n", end_Time_Text / (c_float)UtilConstants.Av_Time_Base);

			if ((duration == int64_t.MinValue) || ((duration < duration_Text) && ((uint64_t)(duration_Text - duration) < UtilConstants.Av_Time_Base)))
				duration = duration_Text;
			else if (duration < duration_Text)
				Log.Av_Log(ic, Log.Av_Log_Verbose, "Ignoring outlier non primary stream duration %f\n", duration_Text / (c_float)UtilConstants.Av_Time_Base);

			if (start_Time != int64_t.MaxValue)
			{
				ic.Start_Time = start_Time;

				if (end_Time != int64_t.MinValue)
				{
					if (ic.Nb_Programs > 1)
					{
						for (c_uint i = 0; i < ic.Nb_Programs; i++)
						{
							AvProgram p = ic.Programs[i];

							if ((p.Start_Time != UtilConstants.Av_NoPts_Value) && (p.End_Time > p.Start_Time) && ((uint64_t)(p.End_Time - p.Start_Time) <= int64_t.MaxValue))
								duration = Macros.FFMax(duration, p.End_Time - p.Start_Time);
						}
					}
					else if ((end_Time >= start_Time) && ((uint64_t)(end_Time - start_Time) <= int64_t.MaxValue))
						duration = Macros.FFMax(duration, end_Time - start_Time);
				}
			}

			if ((duration != int64_t.MinValue) && (duration > 0) && (ic.Duration == UtilConstants.Av_NoPts_Value))
				ic.Duration = duration;

			if ((ic.Pb != null) && ((fileSize = AvIoBuf.AvIo_Size(ic.Pb)) > 0) && (ic.Duration > 0))
			{
				// Compute the bitrate
				c_double bitRate = fileSize * 8.0 * UtilConstants.Av_Time_Base / ic.Duration;

				if ((bitRate >= 0) && (bitRate <= int64_t.MaxValue))
					ic.Bit_Rate = (int64_t)bitRate;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Fill_All_Stream_Timings(AvFormatContext ic)//XX 1745
		{
			Update_Stream_Timings(ic);

			for (c_uint i = 0; i < ic.Nb_Streams; i++)
			{
				AvStream st = ic.Streams[i];

				if (st.Start_Time == UtilConstants.Av_NoPts_Value)
				{
					if (ic.Start_Time != UtilConstants.Av_NoPts_Value)
						st.Start_Time = Mathematics.Av_Rescale_Q(ic.Start_Time, UtilConstants.Av_Time_Base_Q, st.Time_Base);

					if (ic.Duration != UtilConstants.Av_NoPts_Value)
						st.Duration = Mathematics.Av_Rescale_Q(ic.Duration, UtilConstants.Av_Time_Base_Q, st.Time_Base);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Estimate_Timings_From_Bit_Rate(AvFormatContext ic)//XX 1762
		{
			FFFormatContext si = Internal.FFFormatContext(ic);
			c_int show_Warning = 0;

			// If bit_rate is already set, we believe it
			if (ic.Bit_Rate <= 0)
			{
				int64_t bit_Rate = 0;

				for (c_uint i = 0; i < ic.Nb_Streams; i++)
				{
					AvStream st = ic.Streams[i];
					FFStream sti = Internal.CFFStream(st);

					if ((st.CodecPar.Bit_Rate <= 0) && (sti.AvCtx.Bit_Rate > 0))
						st.CodecPar.Bit_Rate = sti.AvCtx.Bit_Rate;

					if (st.CodecPar.Bit_Rate > 0)
					{
						if ((int64_t.MaxValue - st.CodecPar.Bit_Rate) < bit_Rate)
						{
							bit_Rate = 0;
							break;
						}

						bit_Rate += st.CodecPar.Bit_Rate;
					}
					else if ((st.CodecPar.Codec_Type == AvMediaType.Video) && (sti.Codec_Info_Nb_Frames > 1))
					{
						// If we have a videostream with packets but without a bitrate
						// then consider the sum not known
						bit_Rate = 0;
						break;
					}
				}

				ic.Bit_Rate = bit_Rate;
			}

			// If duration is already set, we believe it
			if ((ic.Duration == UtilConstants.Av_NoPts_Value) && (ic.Bit_Rate != 0))
			{
				int64_t fileSize = ic.Pb != null ? AvIoBuf.AvIo_Size(ic.Pb) : 0;

				if (fileSize > si.Data_Offset)
				{
					fileSize -= si.Data_Offset;

					for (c_uint i = 0; i < ic.Nb_Streams; i++)
					{
						AvStream st = ic.Streams[i];

						if ((st.Time_Base.Num <= (int64_t.MaxValue / ic.Bit_Rate)) && (st.Duration == UtilConstants.Av_NoPts_Value))
						{
							st.Duration = Mathematics.Av_Rescale(fileSize, 8L * st.Time_Base.Den, ic.Bit_Rate * st.Time_Base.Num);
							show_Warning = 1;
						}
					}
				}
			}

			if (show_Warning != 0)
				Log.Av_Log(ic, Log.Av_Log_Warning, "Estimating duration from bitrate, this may be inaccurate\n");
		}



		/********************************************************************/
		/// <summary>
		/// Only usable for MPEG-PS streams
		/// </summary>
		/********************************************************************/
		private static void Estimate_Timings_From_Pts(AvFormatContext ic, int64_t old_Offset)//XX 1820
		{
			FFFormatContext si = Internal.FFFormatContext(ic);
			AvPacket pkt = si.Pkt;
			int64_t duration_Max_Read_Size = ic.Duration_ProbeSize != 0 ? ic.Duration_ProbeSize >> Duration_Max_Retry : Duration_Default_Max_Read_Size;
			c_int duration_Max_Retry = ic.Duration_ProbeSize != 0 ? Duration_Max_Retry : Duration_Default_Max_Retry;
			c_int found_Duration = 0;
			c_int is_End;
			int64_t offset;
			c_int retry = 0;

			// Flush packet queue
			AvFormat.FF_Flush_Packet_Queue(ic);

			for (c_uint i = 0; i < ic.Nb_Streams; i++)
			{
				AvStream st = ic.Streams[i];
				FFStream sti = Internal.FFStream(st);

				if ((st.Start_Time == UtilConstants.Av_NoPts_Value) && (sti.First_Dts == UtilConstants.Av_NoPts_Value) && (st.CodecPar.Codec_Type != AvMediaType.Unknown))
					Log.Av_Log(ic, Log.Av_Log_Warning, "start time for stream %d is not set in estimate_timings_from_pts\n", i);

				if (sti.Parser != null)
				{
					Parser.Av_Parser_Close(sti.Parser);
					sti.Parser = null;
				}
			}

			if (ic.Skip_Estimate_Duration_From_Pts != 0)
			{
				Log.Av_Log(ic, Log.Av_Log_Info, "Skipping duration calculation in estimate_timings_from_pts\n");

				goto Skip_Duration_Calc;
			}

			Opt.Av_Opt_Set_Int(ic, "skip_changes".ToCharPointer(), 1, AvOptSearch.Search_Children);

			// Estimate the end time (duration)
			// XXX: may need to support wrapping
			int64_t fileSize = ic.Pb != null ? AvIoBuf.AvIo_Size(ic.Pb) : 0;

			do
			{
				c_int ret;

				is_End = found_Duration;
				offset = fileSize - (duration_Max_Read_Size << retry);

				if (offset < 0)
					offset = 0;

				AvIoBuf.AvIo_Seek(ic.Pb, offset, AvSeek.Set);

				c_int read_Size = 0;

				for (;;)
				{
					if (read_Size >= (duration_Max_Read_Size << (Macros.FFMax(retry - 1, 0))))
						break;

					do
					{
						ret = FF_Read_Packet(ic, pkt);
					}
					while (ret == Error.EAGAIN);

					if (ret != 0)
						break;

					read_Size += pkt.Size;

					AvStream st = ic.Streams[pkt.Stream_Index];
					FFStream sti = Internal.FFStream(st);

					if ((pkt.Pts != UtilConstants.Av_NoPts_Value) && ((st.Start_Time != UtilConstants.Av_NoPts_Value) || (sti.First_Dts != UtilConstants.Av_NoPts_Value)))
					{
						if (pkt.Duration == 0)
						{
							Compute_Frame_Duration(ic, out c_int num, out c_int den, st, sti.Parser, pkt);

							if ((den != 0) && (num != 0))
								pkt.Duration = Mathematics.Av_Rescale_Rnd(1, num * (int64_t)st.Time_Base.Den, den * (int64_t)st.Time_Base.Num, AvRounding.Down);
						}

						int64_t duration = pkt.Pts + pkt.Duration;
						found_Duration = 1;

						if (st.Start_Time != UtilConstants.Av_NoPts_Value)
							duration -= st.Start_Time;
						else
							duration -= sti.First_Dts;

						if (duration > 0)
						{
							if ((st.Duration == UtilConstants.Av_NoPts_Value) || (sti.Info.Last_Duration <= 0) || ((st.Duration < duration) && (Common.FFAbs(duration - sti.Info.Last_Duration) < (60L * st.Time_Base.Den / st.Time_Base.Num))))
								st.Duration = duration;

							sti.Info.Last_Duration = duration;
						}
					}

					Packet.Av_Packet_Unref(pkt);
				}

				// Check if all audio/video streams have valid duration
				if (is_End == 0)
				{
					is_End = 1;

					for (c_uint i = 0; i < ic.Nb_Streams; i++)
					{
						AvStream st = ic.Streams[i];

						switch (st.CodecPar.Codec_Type)
						{
							case AvMediaType.Video:
							case AvMediaType.Audio:
							{
								if (st.Duration == UtilConstants.Av_NoPts_Value)
									is_End = 0;

								break;
							}
						}
					}
				}
			}
			while ((is_End == 0) && (offset != 0) && (++retry <= duration_Max_Retry));

			Opt.Av_Opt_Set_Int(ic, "skip_changes".ToCharPointer(), 0, AvOptSearch.Search_Children);

			// Warn about audio/video streams which duration could not be estimated
			for (c_uint i = 0; i < ic.Nb_Streams; i++)
			{
				AvStream st = ic.Streams[i];
				FFStream sti = Internal.CFFStream(st);

				if (st.Duration == UtilConstants.Av_NoPts_Value)
				{
					switch (st.CodecPar.Codec_Type)
					{
						case AvMediaType.Video:
						case AvMediaType.Audio:
						{
							if ((st.Start_Time != UtilConstants.Av_NoPts_Value) || (sti.First_Dts != UtilConstants.Av_NoPts_Value))
								Log.Av_Log(ic, Log.Av_Log_Warning, "stream %d : no PTS found at end of file, duration not set\n", i);
							else
								Log.Av_Log(ic, Log.Av_Log_Warning, "stream %d : no TS found at start of file, duration not set\n", i);

							break;
						}
					}
				}
			}

			Skip_Duration_Calc:
			Fill_All_Stream_Timings(ic);

			AvIoBuf.AvIo_Seek(ic.Pb, old_Offset, AvSeek.Set);

			for (c_uint i = 0; i < ic.Nb_Streams; i++)
			{
				AvStream st = ic.Streams[i];
				FFStream sti = Internal.FFStream(st);

				sti.Cur_Dts = sti.First_Dts;
				sti.Last_IP_Pts = UtilConstants.Av_NoPts_Value;
				sti.Last_Dts_For_Order_Check = UtilConstants.Av_NoPts_Value;

				for (c_int j = 0; j < (FFStream.Max_Reorder_Delay + 1); j++)
					sti.Pts_Buffer[j] = UtilConstants.Av_NoPts_Value;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static CPointer<char> Duration_Estimate_Name(AvDurationEstimationMethod method)//XX 1968
		{
			return duration_Name[(c_int)method].ToCharPointer();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Estimate_Timings(AvFormatContext ic, int64_t old_Offset)//XX 1973
		{
			int64_t file_Size;

			// Get the file size, if possible
			if ((ic.IFormat.Flags & AvFmt.NoFile) != 0)
				file_Size = 0;
			else
			{
				file_Size = AvIoBuf.AvIo_Size(ic.Pb);
				file_Size = Macros.FFMax(0, file_Size);
			}

			if (((CString.strcmp(ic.IFormat.Name, "mpeg") == 0) || (CString.strcmp(ic.IFormat.Name, "mpegts") == 0)) && (file_Size != 0) && ((ic.Pb.Seekable & AvIoSeekable.Normal) != 0))
			{
				// Get accurate estimate from the PTSes
				Estimate_Timings_From_Pts(ic, old_Offset);

				ic.Duration_Estimation_Method = AvDurationEstimationMethod.From_Pts;
			}
			else if (Has_Duration(ic))
			{
				// At least one component has timings - we use them for all
				// the components
				Fill_All_Stream_Timings(ic);

				// Nut demuxer estimate the duration from PTS
				if (CString.strcmp(ic.IFormat.Name, "nut") == 0)
					ic.Duration_Estimation_Method = AvDurationEstimationMethod.From_Pts;
				else
					ic.Duration_Estimation_Method = AvDurationEstimationMethod.From_Stream;
			}
			else
			{
				// Less precise: use bitrate info
				Estimate_Timings_From_Bit_Rate(ic);

				ic.Duration_Estimation_Method = AvDurationEstimationMethod.From_Bitrate;
			}

			Update_Stream_Timings(ic);

			for (c_uint i = 0; i < ic.Nb_Streams; i++)
			{
				AvStream st = ic.Streams[i];

				if (st.Time_Base.Den != 0)
					Log.Av_Log(ic, Log.Av_Log_Trace, "stream %u: start time: %s duration: %s\n", i, Timestamp.Av_Ts2TimeStr(st.Start_Time, st.Time_Base), Timestamp.Av_Ts2TimeStr(st.Duration, st.Time_Base));
			}

			Log.Av_Log(ic, Log.Av_Log_Trace, "format: start_time: %s duration: %s (estimate from %s) bitrate=%lld kb/s\n", Timestamp.Av_Ts2TimeStr(ic.Start_Time, UtilConstants.Av_Time_Base_Q), Timestamp.Av_Ts2TimeStr(ic.Duration, UtilConstants.Av_Time_Base_Q), Duration_Estimate_Name(ic.Duration_Estimation_Method), (int64_t)ic.Bit_Rate / 1000);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Determinable_Frame_Size(AvCodecContext avCtx)//XX 2022
		{
			switch (avCtx.Codec_Id)
			{
				case AvCodecId.Mp1:
				case AvCodecId.Mp2:
				case AvCodecId.Mp3:
				case AvCodecId.Codec2:
					return 1;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static bool Has_Codec_Parameters(AvStream st, out CPointer<char> errMsg_Ptr)//XX 2035
		{
			FFStream sti = Internal.CFFStream(st);
			AvCodecContext avCtx = sti.AvCtx;

			if ((avCtx.Codec_Id == AvCodecId.None) && (avCtx.Codec_Type != AvMediaType.Data))
			{
				errMsg_Ptr = "unknown codec".ToCharPointer();
				return false;
			}

			switch (avCtx.Codec_Type)
			{
				case AvMediaType.Audio:
				{
					if ((avCtx.Frame_Size == 0) && (Determinable_Frame_Size(avCtx) != 0))
					{
						errMsg_Ptr = "unspecified frame size".ToCharPointer();
						return false;
					}

					if ((sti.Info.Found_Decoder >= 0) && (avCtx.Sample_Fmt == AvSampleFormat.None))
					{
						errMsg_Ptr = "unspecified sample format".ToCharPointer();
						return false;
					}

					if (avCtx.Sample_Rate == 0)
					{
						errMsg_Ptr = "unspecified sample rate".ToCharPointer();
						return false;
					}

					if (avCtx.Ch_Layout.Nb_Channels == 0)
					{
						errMsg_Ptr = "unspecified number of channels".ToCharPointer();
						return false;
					}

					if ((sti.Info.Found_Decoder >= 0) && (sti.Nb_Decoded_Frames == 0) && (avCtx.Codec_Id == AvCodecId.Dts))
					{
						errMsg_Ptr = "no decodable DTS frames".ToCharPointer();
						return false;
					}

					break;
				}

				case AvMediaType.Video:
				{
					if (avCtx.PictureSize.Width == 0)
					{
						errMsg_Ptr = "unspecified size".ToCharPointer();
						return false;
					}

					if ((sti.Info.Found_Decoder >= 0) && (avCtx.Pix_Fmt == AvPixelFormat.None))
					{
						errMsg_Ptr = "unspecified pixel format".ToCharPointer();
						return false;
					}

					if ((st.CodecPar.Codec_Id == AvCodecId.Rv30) || (st.CodecPar.Codec_Id == AvCodecId.Rv40))
					{
						if ((st.Sample_Aspects_Ratio.Num == 0) && (st.CodecPar.Sample_Aspect_Ratio.Num == 0) && (sti.Codec_Info_Nb_Frames == 0))
						{
							errMsg_Ptr = "no frame in rv30/40 and no sar".ToCharPointer();
							return false;
						}
					}

					break;
				}

				case AvMediaType.Subtitle:
				{
					if ((avCtx.Codec_Id == AvCodecId.Hdmv_Pgs_Subtitle) && (avCtx.PictureSize.Width == 0))
					{
						errMsg_Ptr = "unspecified size".ToCharPointer();
						return false;
					}

					break;
				}
			}

			errMsg_Ptr = null;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Returns 1 or 0 if or if not decoded data was returned, or a
		/// negative error
		/// </summary>
		/********************************************************************/
		private static c_int Try_Decode_Frame(AvFormatContext s, AvStream st, AvPacket pkt, ref AvDictionary options)//XX 2084
		{
			FFStream sti = Internal.FFStream(st);
			AvCodecContext avCtx = sti.AvCtx;
			c_int got_Picture = 1, ret = 0;
			AvFrame frame = Frame.Av_Frame_Alloc();
			c_int do_Skip_Frame = 0;
			AvDiscard skip_Frame = AvDiscard.Default;
			c_int pkt_To_Send = pkt.Size > 0 ? 1 : 0;

			if (frame == null)
				return Error.ENOMEM;

			if (!AvCodec_.AvCodec_Is_Open(avCtx) && (sti.Info.Found_Decoder <= 0) && (((c_int)st.CodecPar.Codec_Id != -sti.Info.Found_Decoder) || (st.CodecPar.Codec_Id != AvCodecId.None)))
			{
				AvDictionary tmpOpt = options != null ? options : null;

				AvCodec codec = Find_Probe_Decoder(s, st, st.CodecPar.Codec_Id);

				if (codec == null)
				{
					sti.Info.Found_Decoder = -(c_int)st.CodecPar.Codec_Id;
					ret = -1;

					goto Fail;
				}

				// Force thread count to 1 since the H.264 decoder will not extract
				// SPS and PPS to extradata during multi-threaded decoding
				Dict.Av_Dict_Set(ref tmpOpt, "threads", "1", AvDict.None);

				// Force lowres to 0. The decoder might reduce the video size by the
				// lowres factor, and we don't want that propagated to the stream's
				// codecpar
				Dict.Av_Dict_Set(ref tmpOpt, "lowres", "0", AvDict.None);

				if (s.Codec_Whitelist.IsNotNull)
					Dict.Av_Dict_Set(ref tmpOpt, "codec_whitelist", s.Codec_Whitelist, AvDict.None);

				ret = AvCodec_.AvCodec_Open2(avCtx, codec, ref tmpOpt);

				if (options != null)
					options = tmpOpt;
				else
					Dict.Av_Dict_Free(ref tmpOpt);

				if (ret < 0)
				{
					sti.Info.Found_Decoder = -(c_int)avCtx.Codec_Id;

					goto Fail;
				}

				sti.Info.Found_Decoder = 1;
			}
			else if (sti.Info.Found_Decoder == 0)
				sti.Info.Found_Decoder = 1;

			if (sti.Info.Found_Decoder < 0)
			{
				ret = -1;

				goto Fail;
			}

			if (Utils_Codec.AvPriv_Codec_Get_Cap_Skip_Frame_Fill_Param(avCtx.Codec) != 0)
			{
				do_Skip_Frame = 1;
				skip_Frame = avCtx.Skip_Frame;
				avCtx.Skip_Frame = AvDiscard.All;
			}

			while (((pkt_To_Send != 0) || (pkt.Data.IsNull && (got_Picture != 0))) && (ret >= 0) && (!Has_Codec_Parameters(st, out _) || !Has_Decode_Delay_Been_Guessed(st) || (((sti.Codec_Info_Nb_Frames == 0) && ((avCtx.Codec.Capabilities & AvCodecCap.Channel_Conf) != 0)))))
			{
				got_Picture = 0;

				if ((avCtx.Codec_Type == AvMediaType.Video) || (avCtx.Codec_Type == AvMediaType.Audio))
				{
					ret = Decode.AvCodec_Send_Packet(avCtx, pkt);

					if ((ret < 0) && (ret != Error.EAGAIN) && (ret != Error.EOF))
						break;

					if (ret >= 0)
						pkt_To_Send = 0;

					ret = AvCodec_.AvCodec_Receive_Frame(avCtx, frame);

					if (ret >= 0)
						got_Picture = 1;

					if ((ret == Error.EAGAIN) || (ret == Error.EOF))
						ret = 0;
				}
				else if (avCtx.Codec_Type == AvMediaType.Subtitle)
				{
					ret = Decode.AvCodec_Decode_Subtitle2(avCtx, out AvSubtitle subtitle, out got_Picture, pkt);

					if (got_Picture != 0)
						AvCodec_.AvSubtitle_Free(subtitle);

					if (ret >= 0)
						pkt_To_Send = 0;
				}

				if (ret >= 0)
				{
					if (got_Picture != 0)
						sti.Nb_Decoded_Frames++;

					ret = got_Picture;
				}
			}

			Fail:
			if (do_Skip_Frame != 0)
				avCtx.Skip_Frame = skip_Frame;

			Frame.Av_Frame_Free(ref frame);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Chapter_Start_Cmp(AvChapter p1, AvChapter p2)
		{
			AvChapter ch1 = p1;
			AvChapter ch2 = p2;

			c_int delta = Mathematics.Av_Compare_Ts(ch1.Start, ch1.Time_Base, ch2.Start, ch2.Time_Base);

			if (delta != 0)
				return delta;

			return (c_int)Macros.FFDiffSign(ch1.Id, ch2.Id);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Compute_Chapters_End(AvFormatContext s)//XX 2196
		{
			int64_t max_Time = 0;

			if (s.Nb_Chapters == 0)
				return 0;

			if ((s.Duration > 0) && (s.Start_Time < (int64_t.MaxValue - s.Duration)))
				max_Time = s.Duration + ((s.Start_Time == UtilConstants.Av_NoPts_Value) ? 0 : s.Start_Time);

			CPointer<AvChapter> timeTable = Mem.Av_MemDupObj(s.Chapters, s.Nb_Chapters);

			if (timeTable.IsNull)
				return Error.ENOMEM;

			CSort.qsort(timeTable, s.Nb_Chapters, Chapter_Start_Cmp);

			for (c_uint i = 0; i < s.Nb_Chapters; i++)
			{
				if (timeTable[i].End == UtilConstants.Av_NoPts_Value)
				{
					AvChapter ch = timeTable[i];

					int64_t end = max_Time != 0 ? Mathematics.Av_Rescale_Q(max_Time, UtilConstants.Av_Time_Base_Q, ch.Time_Base) : int64_t.MaxValue;

					if ((i + 1) < s.Nb_Chapters)
					{
						AvChapter ch1 = timeTable[i + 1];

						int64_t next_Start = Mathematics.Av_Rescale_Q(ch1.Start, ch.Time_Base, ch.Time_Base);

						if ((next_Start > ch.Start) && (next_Start < end))
							end = next_Start;
					}

					ch.End = (end == int64_t.MaxValue) || (end < ch.Start) ? ch.Start : end;
				}
			}

			Mem.Av_Free(timeTable);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Get_Std_FrameRate(c_int i)//XX 2233
		{
			if (i < (30 * 12))
				return (i + 1) * 1001;

			i -= 30 * 12;

			if (i < 30)
				return (i + 31) * 1001 * 12;

			i -= 30;

			if (i < 3)
			{
				int[] values1 = [ 80, 120, 240 ];

				return values1[i] * 1001 * 12;
			}

			i -= 3;

			int[] values2 = [ 24, 30, 60, 12, 15, 48 ];

			return values2[i] * 1000 * 12;
		}



		/********************************************************************/
		/// <summary>
		/// Is the time base unreliable?
		/// This is a heuristic to balance between quick acceptance of the
		/// values in the headers vs. some extra checks.
		/// Old DivX and Xvid often have nonsense timebases like 1fps or
		/// 2fps. MPEG-2 commonly misuses field repeat flags to store
		/// different framerates. And there are "variable" fps files this
		/// needs to detect as well
		/// </summary>
		/********************************************************************/
		private static c_int Tb_Unreliable(AvFormatContext ic, AvStream st)//XX 2257
		{
			FFStream sti = Internal.FFStream(st);
			AvCodecDescriptor desc = sti.Codec_Desc;
			AvCodecContext c = sti.AvCtx;
			AvRational mul = new AvRational((desc != null) && ((desc.Props & AvCodecProp.Fields) != 0) ? 2 : 1, 1);
			AvRational time_Base = c.FrameRate.Num != 0 ? Rational.Av_Inv_Q(Rational.Av_Mul_Q(c.FrameRate, mul)) :
					// NOHEADER check added to not break existing behavior
					((ic.Ctx_Flags & AvFmtCtx.NoHeader) != 0) || (st.CodecPar.Codec_Type == AvMediaType.Audio) ? new AvRational(0, 1) : st.Time_Base;

			if ((time_Base.Den >= (101L * time_Base.Num)) ||
				(time_Base.Den < (5L * time_Base.Num)) ||
				(c.Codec_Tag == IntReadWrite.Av_RL32(Encoding.UTF8.GetBytes("mp4v"))) ||
				(c.Codec_Id == AvCodecId.Mpeg2Video) || (c.Codec_Id == AvCodecId.Gif) ||
				(c.Codec_Id == AvCodecId.Hevc) || (c.Codec_Id == AvCodecId.H264))
			{
				return 1;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void FF_Rfps_Calculate(AvFormatContext ic)//XX 2343
		{
			for (c_uint i = 0; i < ic.Nb_Streams; i++)
			{
				AvStream st = ic.Streams[i];
				FFStream sti = Internal.FFStream(st);

				if (st.CodecPar.Codec_Type != AvMediaType.Video)
					continue;

				// The check for tb_unreliable() is not completely correct, since this is not about handling
				// an unreliable/inexact time base, but a time base that is finer than necessary, as e.g.
				// ipmovie.c produces
				if ((Tb_Unreliable(ic, st) != 0) && (sti.Info.Duration_Count > 15) && (sti.Info.Duration_Gcd > Macros.FFMax(1, st.Time_Base.Den / (500L * st.Time_Base.Num))) && (st.R_Frame_Rate.Num == 0) && (sti.Info.Duration_Gcd < (int64_t.MaxValue / st.Time_Base.Num)))
					Rational.Av_Reduce(out st.R_Frame_Rate.Num, out st.R_Frame_Rate.Den, st.Time_Base.Den, st.Time_Base.Num * sti.Info.Duration_Gcd, c_int.MaxValue);

				if ((sti.Info.Duration_Count > 1) && (st.R_Frame_Rate.Num == 0) && (Tb_Unreliable(ic, st) != 0))
				{
					c_int num = 0;
					c_double best_Error = 0.01;
					AvRational ref_Rate = st.R_Frame_Rate.Num != 0 ? st.R_Frame_Rate : Rational.Av_Inv_Q(st.Time_Base);

					for (c_int j = 0; j < Max_Std_Timebases; j++)
					{
						if ((sti.Info.Codec_Info_Duration != 0) && ((sti.Info.Codec_Info_Duration * Rational.Av_Q2D(st.Time_Base)) < ((1001 * 11.5) / Get_Std_FrameRate(j))))
							continue;

						if ((sti.Info.Codec_Info_Duration == 0) && (Get_Std_FrameRate(j) < (1001 * 12)))
							continue;

						if ((Rational.Av_Q2D(st.Time_Base) * sti.Info.Rfps_Duration_Sum / sti.Info.Duration_Count) < ((1001 * 12.0 * 0.8) / Get_Std_FrameRate(j)))
							continue;

						for (c_int k = 0; k < 2; k++)
						{
							c_int n = sti.Info.Duration_Count;
							c_double a = sti.Info.Duration_Error[k][0][j] / n;
							c_double error = (sti.Info.Duration_Error[k][1][j] / n) - (a * a);

							if ((error < best_Error) && (best_Error > 0.000000001))
							{
								best_Error = error;
								num = Get_Std_FrameRate(j);
							}

							if (error < 0.02)
								Log.Av_Log(ic, Log.Av_Log_Debug, "rfps: %f %f\n", Get_Std_FrameRate(j) / 12.0 / 1001, error);
						}
					}

					// Do not increase frame rate by more than 1 % in order to match a standard rate
					if ((num != 0) && ((ref_Rate.Num == 0) || (((c_double)num / (12 * 1001)) < (1.01 * Rational.Av_Q2D(ref_Rate)))))
						Rational.Av_Reduce(out st.R_Frame_Rate.Num, out st.R_Frame_Rate.Den, num, 12 * 1001, c_int.MaxValue);
				}

				if ((st.Avg_Frame_Rate.Num == 0) && (st.R_Frame_Rate.Num != 0) && (sti.Info.Rfps_Duration_Sum != 0) && (sti.Info.Codec_Info_Duration <= 0) && (sti.Info.Duration_Count > 2) && ((CMath.fabs((1.0 / (Rational.Av_Q2D(st.R_Frame_Rate) * Rational.Av_Q2D(st.Time_Base))) - (sti.Info.Rfps_Duration_Sum / (c_double)sti.Info.Duration_Count))) <= 1.0))
				{
					Log.Av_Log(ic, Log.Av_Log_Debug, "Setting avg frame rate based on r frame rate\n");

					st.Avg_Frame_Rate = st.R_Frame_Rate;
				}

				Mem.Av_FreeP(ref sti.Info.Duration_Error);

				sti.Info.Last_Dts = UtilConstants.Av_NoPts_Value;
				sti.Info.Duration_Count = 0;
				sti.Info.Rfps_Duration_Sum = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Extract_ExtraData_Check(AvStream st)//XX 2407
		{
			AvBitStreamFilter f = BitStream_Filters.Av_Bsf_Get_By_Name("extract_extradata".ToCharPointer());

			if (f == null)
				return 0;

			if (f.Codec_Ids.IsNotNull)
			{
				for (c_int i = 0; i < f.Codec_Ids.Length; i++)
				{
					if (f.Codec_Ids[i] == st.CodecPar.Codec_Id)
						return 1;
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Extract_ExtraData_Init(AvStream st)//XX 2423
		{
			FFStream sti = Internal.FFStream(st);

			AvBitStreamFilter f = BitStream_Filters.Av_Bsf_Get_By_Name("extract_extradata".ToCharPointer());

			if (f == null)
				goto Finish;

			// Check that the codec id is supported
			c_int ret = Extract_ExtraData_Check(st);

			if (ret != 0)
				goto Finish;

			Bsf.Av_Bsf_Free(ref sti.Extract_ExtraData.Bsf);

			ret = Bsf.Av_Bsf_Alloc(f, out sti.Extract_ExtraData.Bsf);

			if (ret < 0)
				return ret;

			ret = Codec_Par.AvCodec_Parameters_Copy(sti.Extract_ExtraData.Bsf.Par_In, sti.CodecPar);

			if (ret < 0)
				goto Fail;

			sti.Extract_ExtraData.Bsf.Time_Base_In = st.Time_Base;

			ret = Bsf.Av_Bsf_Init(sti.Extract_ExtraData.Bsf);

			if (ret < 0)
				goto Fail;

			Finish:
			sti.Extract_ExtraData.Inited = 1;

			return 0;

			Fail:
			Bsf.Av_Bsf_Free(ref sti.Extract_ExtraData.Bsf);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Extract_ExtraData(FFFormatContext si, AvStream st, AvPacket pkt)//XX 2463
		{
			FFStream sti = Internal.FFStream(st);
			AvPacket pkt_Ref = si.Parse_Pkt;
			c_int ret;

			if (sti.Extract_ExtraData.Inited == 0)
			{
				ret = Extract_ExtraData_Init(st);

				if (ret < 0)
					return ret;
			}

			if ((sti.Extract_ExtraData.Inited != 0) && (sti.Extract_ExtraData.Bsf == null))
				return 0;

			ret = Packet.Av_Packet_Ref(pkt_Ref, pkt);

			if (ret < 0)
				return ret;

			ret = Bsf.Av_Bsf_Send_Packet(sti.Extract_ExtraData.Bsf, pkt_Ref);

			if (ret < 0)
			{
				Packet.Av_Packet_Unref(pkt_Ref);

				return ret;
			}

			while ((ret >= 0) && (sti.AvCtx.ExtraData == null))
			{
				ret = Bsf.Av_Bsf_Receive_Packet(sti.Extract_ExtraData.Bsf, pkt_Ref);

				if (ret < 0)
				{
					if ((ret != Error.EAGAIN) && (ret != Error.EOF))
						return ret;

					continue;
				}

				for (c_int i = 0; i < pkt_Ref.Side_Data_Elems; i++)
				{
					AvPacketSideData side_Data = pkt_Ref.Side_Data[i];

					if (side_Data.Type == AvPacketSideDataType.New_ExtraData)
					{
						sti.AvCtx.ExtraData = side_Data.Data;

						side_Data.Data = null;
						break;
					}
				}

				Packet.Av_Packet_Unref(pkt_Ref);
			}

			return 0;
		}
		#endregion
	}
}
