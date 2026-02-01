/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat
{
	/// <summary>
	/// 
	/// </summary>
	public static class AvFormat
	{
		/********************************************************************/
		/// <summary>
		/// Frees a stream without modifying the corresponding
		/// AVFormatContext. Must only be called if the latter doesn't matter
		/// or if the stream is not yet attached to an AVFormatContext
		/// </summary>
		/********************************************************************/
		internal static void FF_Free_Stream(ref AvStream pst)//XX 45
		{
			AvStream st = pst;
			FFStream sti = Internal.FFStream(st);

			if (st == null)
				return;

			if (st.Attached_Pic.Data != null)
				Packet.Av_Packet_Unref(st.Attached_Pic);

			Parser.Av_Parser_Close(sti.Parser);
			Options_Codec.AvCodec_Free_Context(ref sti.AvCtx);
			Bsf.Av_Bsf_Free(ref sti.BSfc);
			Mem.Av_FreeP(ref sti.Index_Entries);
			Mem.Av_FreeP(ref sti.Probe_Data.Buf);

			Bsf.Av_Bsf_Free(ref sti.Extract_ExtraData.Bsf);

			if (sti.Info != null)
			{
				Mem.Av_FreeP(ref sti.Info.Duration_Error);
				Mem.Av_FreeP(ref sti.Info);
			}

			Dict.Av_Dict_Free(ref st.Metadata);
			Codec_Par.AvCodec_Parameters_Free(ref st.CodecPar);
			Mem.Av_FreeP(ref st.Priv_Data);

			Mem.Av_FreeP(ref pst);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal static void FF_Free_Stream_Group(ref AvStreamGroup pStg)//XX 76
		{
			AvStreamGroup stg = pStg;

			if (stg == null)
				return;

			Mem.Av_FreeP(ref stg.Streams);
			Dict.Av_Dict_Free(ref stg.Metadata);
			Mem.Av_FreeP(ref stg.Priv_Data);

			switch (stg.Type)
			{
				case AvStreamGroupParamsType.Iamf_Audio_Element:
				{
					AvIamfAudioElement obj = (AvIamfAudioElement)stg.Params;
					Iamf.Av_Iamf_Audio_Element_Free(ref obj);
					stg.Params = obj;
					break;
				}

				case AvStreamGroupParamsType.Iamf_Mix_Presentation:
				{
					AvIamfMixPresentation obj = (AvIamfMixPresentation)stg.Params;
					Iamf.Av_Iamf_Mix_Presentation_Free(ref obj);
					stg.Params = obj;
					break;
				}

				case AvStreamGroupParamsType.Tile_Grid:
				{
					Opt.Av_Opt_Free(stg.Params);
					Mem.Av_FreeP(ref ((AvStreamGroupTileGrid)stg.Params).Offsets);
					Packet.Av_Packet_Side_Data_Free(ref ((AvStreamGroupTileGrid)stg.Params).Coded_Side_Data, ref ((AvStreamGroupTileGrid)stg.Params).Nb_Coded_Side_Data);
					Mem.Av_FreeP(ref stg.Params);
					break;
				}

				case AvStreamGroupParamsType.Lcevc:
				{
					Opt.Av_Opt_Free(stg.Params);
					Mem.Av_FreeP(ref stg.Params);
					break;
				}
			}

			Mem.Av_FreeP(ref pStg);
		}



		/********************************************************************/
		/// <summary>
		/// Remove a stream from its AVFormatContext and free it.
		/// The stream must be the last stream of the AVFormatContext
		/// </summary>
		/********************************************************************/
		internal static void FF_Remove_Stream(AvFormatContext s, AvStream st)//XX 113
		{
			FF_Free_Stream(ref s.Streams[--s.Nb_Streams]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal static void FF_Flush_Packet_Queue(AvFormatContext s)//XX 130
		{
			FormatContextInternal fci = AvFormatInternal.FF_FC_Internal(s);
			FFFormatContext si = fci.Fc;

			Packet.AvPriv_Packet_List_Free(fci.Demuxing.Parse_Queue);
			Packet.AvPriv_Packet_List_Free(si.Packet_Buffer);
			Packet.AvPriv_Packet_List_Free(fci.Demuxing.Raw_Packet_Buffer);

			fci.Demuxing.Raw_Packet_Buffer_Size = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Free an AVFormatContext and all its streams
		/// </summary>
		/********************************************************************/
		public static void AvFormat_Free_Context(AvFormatContext s)//XX 141
		{
			if (s == null)
				return;

			FormatContextInternal fci = AvFormatInternal.FF_FC_Internal(s);
			FFFormatContext si = fci.Fc;

			if ((s.OFormat != null) && (Mux.FFOFmt(s.OFormat).Deinit != null) && (fci.Muxing.Initialized != 0))
				Mux.FFOFmt(s.OFormat).Deinit(s);

			Opt.Av_Opt_Free(s);

			if ((s.IFormat != null) && (s.IFormat.Priv_Class != null) && (s.Priv_Data != null))
				Opt.Av_Opt_Free(s.Priv_Data);

			if ((s.OFormat != null) && (s.OFormat.Priv_Class != null) && (s.Priv_Data != null))
				Opt.Av_Opt_Free(s.Priv_Data);

			for (c_uint i = 0; i < s.Nb_Streams; i++)
				FF_Free_Stream(ref s.Streams[i]);

			for (c_uint i = 0; i < s.Nb_Stream_Groups; i++)
				FF_Free_Stream_Group(ref s.Stream_Groups[i]);

			s.Nb_Stream_Groups = 0;
			s.Nb_Streams = 0;

			for (c_uint i = 0; i < s.Nb_Programs; i++)
			{
				Dict.Av_Dict_Free(ref s.Programs[i].Metadata);
				Mem.Av_FreeP(ref s.Programs[i].Stream_Index);
				Mem.Av_FreeP(ref s.Programs[i]);
			}

			s.Nb_Programs = 0;

			Mem.Av_FreeP(ref s.Programs);
			Mem.Av_FreeP(ref s.Priv_Data);

			while (s.Nb_Chapters-- != 0)
			{
				Dict.Av_Dict_Free(ref s.Chapters[s.Nb_Chapters].Metadata);
				Mem.Av_FreeP(ref s.Chapters[s.Nb_Chapters]);
			}

			Mem.Av_FreeP(ref s.Chapters);
			Dict.Av_Dict_Free(ref s.Metadata);
			Dict.Av_Dict_Free(ref si.Id3v2_Meta);
			Packet.Av_Packet_Free(ref si.Pkt);
			Packet.Av_Packet_Free(ref si.Parse_Pkt);
			Packet.AvPriv_Packet_List_Free(si.Packet_Buffer);
			Mem.Av_FreeP(ref s.Streams);
			Mem.Av_FreeP(ref s.Stream_Groups);

			if (s.IFormat != null)
				FF_Flush_Packet_Queue(s);

			Mem.Av_FreeP(ref s.Url);
			Mem.Av_Free(s);
		}



		/********************************************************************/
		/// <summary>
		/// Find the programs which belong to a given stream
		/// </summary>
		/********************************************************************/
		public static AvProgram Av_Find_Program_From_Stream(AvFormatContext ic, AvProgram last, c_int s)//XX 325
		{
			for (c_uint i = 0; i < ic.Nb_Programs; i++)
			{
				if (ic.Programs[i] == last)
					last = null;
				else
				{
					if (last == null)
					{
						for (c_uint j = 0; j < ic.Programs[i].Nb_Stream_Indexes; j++)
						{
							if (ic.Programs[i].Stream_Index[j] == s)
								return ic.Programs[i];
						}
					}
				}
			}

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Get the codec tag for the given codec id
		/// </summary>
		/********************************************************************/
		public static c_int Av_Find_Default_Stream_Index(AvFormatContext s)//XX 340
		{
			c_int best_Stream = 0;
			c_int best_Score = c_int.MinValue;

			if (s.Nb_Streams <= 0)
				return -1;

			for (c_uint i = 0; i < s.Nb_Streams; i++)
			{
				AvStream st = s.Streams[i];
				FFStream sti = Internal.CFFStream(st);
				c_int score = 0;

				if (st.CodecPar.Codec_Type == AvMediaType.Video)
				{
					if ((st.Disposition & AvDisposition.Attached_Pic) != 0)
						score -= 400;

					if ((st.CodecPar.Width != 0) && (st.CodecPar.Height != 0))
						score += 50;

					score += 25;
				}

				if (st.CodecPar.Codec_Type == AvMediaType.Audio)
				{
					if (st.CodecPar.Sample_Rate != 0)
						score += 50;
				}

				if (sti.Codec_Info_Nb_Frames != 0)
					score += 12;

				if (st.Discard != AvDiscard.All)
					score += 200;

				if (score > best_Score)
				{
					best_Score = score;
					best_Stream = (c_int)i;
				}
			}

			return best_Stream;
		}



		/********************************************************************/
		/// <summary>
		/// Find the "best" stream in the file.
		/// The best stream is determined according to various heuristics as
		/// the most likely to be what the user expects.
		/// If the decoder parameter is non-NULL, av_find_best_stream will
		/// find the default decoder for the stream's codec; streams for
		/// which no decoder can be found are ignored
		/// </summary>
		/********************************************************************/
		public static c_int Av_Find_Best_Stream(AvFormatContext ic, AvMediaType type, c_int wanted_Stream_Nb, c_int related_Stream, out AvCodec decoder_Ret, c_int flags)//XX 376
		{
			c_int nb_Streams = (c_int)ic.Nb_Streams;
			c_int ret = Error.Stream_Not_Found;
			c_int best_Count = -1, best_Multiframe = -1, best_Disposition = -1;
			int64_t best_BitRate = -1;
			CPointer<c_uint> program = null;
			AvCodec decoder = null, best_Decoder = null;

			if ((related_Stream >= 0) && (wanted_Stream_Nb < 0))
			{
				AvProgram p = Av_Find_Program_From_Stream(ic, null, related_Stream);

				if (p != null)
				{
					program = p.Stream_Index;
					nb_Streams = (c_int)p.Nb_Stream_Indexes;
				}
			}

			for (c_uint i = 0; i < nb_Streams; i++)
			{
				c_int real_Stream_Index = (c_int)(program.IsNotNull ? program[i] : i);
				AvStream st = ic.Streams[real_Stream_Index];
				AvCodecParameters par = st.CodecPar;

				if (par.Codec_Type != type)
					continue;

				if ((wanted_Stream_Nb >= 0) && (real_Stream_Index != wanted_Stream_Nb))
					continue;

				if ((type == AvMediaType.Audio) && !((par.Ch_Layout.Nb_Channels != 0) && (par.Sample_Rate != 0)))
					continue;

				decoder = FF_Find_Decoder(ic, st, par.Codec_Id);

				if (decoder == null)
				{
					if (ret < 0)
						ret = Error.Decoder_Not_Found;

					continue;
				}

				c_int disposition = (((st.Disposition & (AvDisposition.Hearing_Impaired | AvDisposition.Visual_Impaired)) == 0) ? 1 : 0) + ((st.Disposition & AvDisposition.Default) != 0 ? 1 : 0);
				c_int count = Internal.FFStream(st).Codec_Info_Nb_Frames;
				int64_t bitRate = par.Bit_Rate;
				c_int multiframe = Macros.FFMin(5, count);

				if ((best_Disposition > disposition) || ((best_Disposition == disposition) && (best_Multiframe > multiframe)) || ((best_Disposition == disposition) && (best_Multiframe == multiframe) && (best_BitRate > bitRate)) || (((best_Disposition == disposition) && (best_Multiframe == multiframe) && (best_BitRate == bitRate) && (best_Count >= count))))
					continue;

				best_Disposition = disposition;
				best_Count = count;
				best_BitRate = bitRate;
				best_Multiframe = multiframe;

				ret = real_Stream_Index;
				best_Decoder = decoder;

				if (program.IsNotNull && (i == (nb_Streams - 1)) && (ret < 0))
				{
					program.SetToNull();
					nb_Streams = (c_int)ic.Nb_Streams;

					// No related stream found, try again with everything
					i = 0;
				}
			}

			decoder_Ret = best_Decoder;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Set the time base and wrapping info for a given stream. This will
		/// be used to interpret the stream's timestamps. If the new time
		/// base is invalid (numerator or denominator are non-positive), it
		/// leaves the stream unchanged
		/// </summary>
		/********************************************************************/
		internal static void AvPriv_Set_Pts_Info(AvStream st, c_int pts_Wrap_Bits, c_uint pts_Num, c_uint pts_Den)//XX 777
		{
			FFStream sti = Internal.FFStream(st);
			AvRational new_Tb;

			if (Rational.Av_Reduce(out new_Tb.Num, out new_Tb.Den, pts_Num, pts_Den, c_int.MaxValue) != 0)
			{
				if (new_Tb.Num != pts_Num)
					Log.Av_Log(null, Log.Av_Log_Debug, "st:%d removing common factor %d from timebase\n", st.Index, pts_Num / new_Tb.Num);
			}
			else
				Log.Av_Log(null, Log.Av_Log_Warning, "st:%d has too large timebase, reducing\n", st.Index);

			if ((new_Tb.Num <= 0) || (new_Tb.Den <= 0))
			{
				Log.Av_Log(null, Log.Av_Log_Error, "Ignoring attempt to set invalid timebase %d/%d for st:%d\n", new_Tb.Num, new_Tb.Den, st.Index);
				return;
			}

			st.Time_Base = new_Tb;

			if (sti.AvCtx != null)
				sti.AvCtx.Pkt_TimeBase = new_Tb;

			st.Pts_Wrap_Bits = pts_Wrap_Bits;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal static AvCodec FF_Find_Decoder(AvFormatContext s, AvStream st, AvCodecId codec_Id)//XX 804
		{
			switch (st.CodecPar.Codec_Type)
			{
				case AvMediaType.Video:
				{
					if (s.Video_Codec != null)
						return s.Video_Codec;

					break;
				}

				case AvMediaType.Audio:
				{
					if (s.Audio_Codec != null)
						return s.Audio_Codec;

					break;
				}

				case AvMediaType.Subtitle:
				{
					if (s.Subtitle_Codec != null)
						return s.Subtitle_Codec;

					break;
				}
			}

			return AllCodec.AvCodec_Find_Decoder(codec_Id);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal static bool FF_Is_Intra_Only(AvCodecId id)//XX 850
		{
			AvCodecDescriptor d = Codec_Desc.AvCodec_Descriptor_Get(id);

			if (d == null)
				return false;

			if (((d.Type == AvMediaType.Video) || (d.Type == AvMediaType.Audio)) && ((d.Props & AvCodecProp.Intra_Only) == 0))
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// A wrapper around AVFormatContext.io_close that should be used
		/// instead of calling the pointer directly
		/// </summary>
		/********************************************************************/
		internal static c_int FF_Format_Io_Close(AvFormatContext s, ref AvIoContext pb)//XX 868
		{
			c_int ret = 0;

			if (pb != null)
				ret = s.Io_Close2(s, pb);

			pb = null;

			return ret;
		}
	}
}
