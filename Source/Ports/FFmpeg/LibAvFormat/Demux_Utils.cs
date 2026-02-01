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
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat
{
	/// <summary>
	/// 
	/// </summary>
	public static class Demux_Utils
	{
		/********************************************************************/
		/// <summary>
		/// Add a new chapter
		/// </summary>
		/********************************************************************/
		public static AvChapter AvPriv_New_Chapter(AvFormatContext s, int64_t id, AvRational time_Base, int64_t start, int64_t end, CPointer<char> title)//XX 43
		{
			FormatContextInternal fci = AvFormatInternal.FF_FC_Internal(s);
			AvChapter chapter = null;

			if ((end != UtilConstants.Av_NoPts_Value) && (start > end))
			{
				Log.Av_Log(s, Log.Av_Log_Error, "Chapter end time %lld before start %lld\n", end, start);

				return null;
			}

			if (s.Nb_Chapters == 0)
				fci.Demuxing.Chapter_Ids_Monotonic = 1;
			else if ((fci.Demuxing.Chapter_Ids_Monotonic == 0) || (s.Chapters[s.Nb_Streams - 1].Id >= id))
			{
				for (c_uint i = 0; i < s.Nb_Chapters; i++)
				{
					if (s.Chapters[i].Id == id)
						chapter = s.Chapters[i];
				}

				if (chapter == null)
					fci.Demuxing.Chapter_Ids_Monotonic = 0;
			}

			if (chapter == null)
			{
				chapter = Mem.Av_MAlloczObj<AvChapter>();

				if (chapter == null)
					return null;

				c_int refNbChapters = (c_int)s.Nb_Chapters;
				c_int ret = Mem.Av_DynArray_Add_NoFreeObj(ref s.Chapters, ref refNbChapters, chapter);
				s.Nb_Chapters = (c_uint)refNbChapters;

				if (ret < 0)
				{
					Mem.Av_Free(chapter);

					return null;
				}
			}

			Dict.Av_Dict_Set(ref chapter.Metadata, "title", title, AvDict.None);

			chapter.Id = id;
			chapter.Time_Base = time_Base;
			chapter.Start = start;
			chapter.End = end;

			return chapter;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int AvFormat_Queue_Attached_Pictures(AvFormatContext s)//XX 84
		{
			FormatContextInternal fci = AvFormatInternal.FF_FC_Internal(s);

			for (c_uint i = 0; i < s.Nb_Streams; i++)
			{
				if (((s.Streams[i].Disposition & AvDisposition.Attached_Pic) != 0) && (s.Streams[i].Discard < AvDiscard.All))
				{
					if (s.Streams[i].Attached_Pic.Size <= 0)
					{
						Log.Av_Log(s, Log.Av_Log_Warning, "Attached picture on stream %d has invalid size, ignoring\n", i);

						continue;
					}

					c_int ret = Packet.AvPriv_Packet_List_Put(fci.Demuxing.Raw_Packet_Buffer, s.Streams[i].Attached_Pic, Packet.Av_Packet_Ref, FFPacketListFlag.None);

					if (ret < 0)
						return ret;
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Add an attached pic to an AVStream
		/// </summary>
		/********************************************************************/
		public static c_int FF_Add_Attached_Pic(AvFormatContext s, AvStream st0, AvIoContext pb, c_int size)
		{
			AvBufferRef temp = null;

			return FF_Add_Attached_Pic(s, st0, pb, ref temp, size);
		}



		/********************************************************************/
		/// <summary>
		/// Add an attached pic to an AVStream
		/// </summary>
		/********************************************************************/
		public static c_int FF_Add_Attached_Pic(AvFormatContext s, AvStream st0, AvIoContext pb, ref AvBufferRef buf, c_int size)//XX 107
		{
			AvStream st = st0;
			c_int ret;

			if ((st == null) && ((st = Options_Format.AvFormat_New_Stream(s, null)) == null))
				return Error.ENOMEM;

			AvPacket pkt = st.Attached_Pic;

			if (buf != null)
			{
				Packet.Av_Packet_Unref(pkt);

				pkt.Buf = buf;
				pkt.Data = ((DataBufferContext)buf.Data).Data;
				pkt.Size = (c_int)(((DataBufferContext)buf.Data).Size - Defs.Av_Input_Buffer_Padding_Size);

				buf = null;
			}
			else
			{
				ret = Utils_Format.Av_Get_Packet(pb, pkt, size);

				if (ret < 0)
					goto Fail;
			}

			st.Disposition |= AvDisposition.Attached_Pic;
			st.CodecPar.Codec_Type = AvMediaType.Video;

			pkt.Stream_Index = st.Index;
			pkt.Flags |= AvPktFlag.Key;

			return 0;

			Fail:
			if (st0 == null)
				AvFormat.FF_Remove_Stream(s, st);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Allocate extradata with additional AV_INPUT_BUFFER_PADDING_SIZE
		/// at end which is always set to 0 and fill it from pb
		/// </summary>
		/********************************************************************/
		public static c_int FF_Get_ExtraData(IClass logCtx, AvCodecParameters par, AvIoContext pb, c_int size)//XX 326
		{
			c_int ret = Utils_Format.FF_Alloc_ExtraData(par, size);

			if (ret < 0)
				return ret;

			ret = AvIoBuf.FFIo_Read_Size(pb, ((DataBufferContext)par.ExtraData).Data, size);

			if (ret < 0)
			{
				Mem.Av_FreeP(ref par.ExtraData);

				Log.Av_Log(logCtx, Log.Av_Log_Error, "Failed to read extradata of size %d\n", size);

				return ret;
			}

			return ret;
		}
	}
}
