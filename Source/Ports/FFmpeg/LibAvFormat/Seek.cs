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
	/// Seeking and index-related functions
	/// </summary>
	public static class Seek
	{
		/********************************************************************/
		/// <summary>
		/// Update cur_dts of all streams based on the given timestamp and
		/// AVStream.
		///
		/// Stream ref_st unchanged, others set cur_dts in their native
		/// time base. Only needed for timestamp wrapping or if (dts not set
		/// and pts!=dts)
		/// </summary>
		/********************************************************************/
		public static void AvPriv_Update_Cur_Dts(AvFormatContext s, AvStream ref_St, int64_t timestamp)//XX 37
		{
			for (c_uint i = 0; i < s.Nb_Streams; i++)
			{
				AvStream st = s.Streams[i];
				FFStream sti = Internal.FFStream(st);

				sti.Cur_Dts = Mathematics.Av_Rescale(timestamp, st.Time_Base.Den * (int64_t)ref_St.Time_Base.Num, st.Time_Base.Num * (int64_t)ref_St.Time_Base.Den);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Ensure the index uses less memory than the maximum specified in
		/// AVFormatContext.max_index_size by discarding entries if it grows
		/// too large
		/// </summary>
		/********************************************************************/
		public static void FF_Reduce_Index(AvFormatContext s, c_int stream_Index)//XX 50
		{
			AvStream st = s.Streams[stream_Index];
			FFStream sti = Internal.FFStream(st);
			c_uint max_Entries = s.Max_Index_Size / 28/*sizeof(AvIndexEntry)*/;

			if ((c_uint)sti.Nb_Index_Entries >= max_Entries)
			{
				c_int i;

				for (i = 0; 2 * i < sti.Nb_Index_Entries; i++)
					sti.Index_Entries[i] = sti.Index_Entries[2 * i];

				sti.Nb_Index_Entries = i;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int FF_Add_Index_Entry(ref CPointer<AvIndexEntry> index_Entries, ref c_int nb_Index_Entries, ref c_uint index_Entries_Allocated_Size, int64_t pos, int64_t timestamp, c_int size, c_int distance, AvIndex flags)//XX 64
		{
			AvIndexEntry ie;

			if (((c_uint)nb_Index_Entries + 1) >= (uint.MaxValue / 28/*sizeof(AvIndexEntry)*/))
				return -1;

			if (timestamp == UtilConstants.Av_NoPts_Value)
				return Error.EINVAL;

			if ((size < 0) || (size > 0x3fffffff))
				return Error.EINVAL;

			if (AvFormatInternal.Is_Relative(timestamp))	// FIXME this maintains previous behavior but we should shift by the correct offset once known
				timestamp -= AvFormatInternal.Relative_Ts_Base;

			CPointer<AvIndexEntry> entries = Mem.Av_Fast_ReallocObj(index_Entries, ref index_Entries_Allocated_Size, (size_t)nb_Index_Entries + 1);

			if (entries.IsNull)
				return -1;

			index_Entries = entries;

			c_int index = FF_Index_Search_Timestamp(index_Entries, nb_Index_Entries, timestamp, AvSeekFlag.Any);

			if (index < 0)
			{
				index = nb_Index_Entries++;
				ie = entries[index];
			}
			else
			{
				ie = entries[index];

				if (ie.Timestamp != timestamp)
				{
					if (ie.Timestamp <= timestamp)
						return -1;

					CMemory.memmove(entries + index + 1, entries + index, (size_t)(nb_Index_Entries - index));
					nb_Index_Entries++;
				}
				else if ((ie.Pos == pos) && (distance < ie.Min_Distance))
				{
					// Do not reduce the distance
					distance = ie.Min_Distance;
				}
			}

			ie.Pos = pos;
			ie.Timestamp = timestamp;
			ie.Min_Distance = distance;
			ie.Size = size;
			ie.Flags = flags;

			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Add an index entry into a sorted list. Update the entry if the
		/// list already contains it
		/// </summary>
		/********************************************************************/
		public static c_int Av_Add_Index_Entry(AvStream st, int64_t pos, int64_t timestamp, c_int size, c_int distance, AvIndex flags)//XX 122
		{
			FFStream sti = Internal.FFStream(st);
			timestamp = Demux.FF_Wrap_Timestamp(st, timestamp);

			return FF_Add_Index_Entry(ref sti.Index_Entries, ref sti.Nb_Index_Entries, ref sti.Index_Entries_Allocated_Size, pos, timestamp, size, distance, flags);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int FF_Index_Search_Timestamp(CPointer<AvIndexEntry> entries, c_int nb_Entries, int64_t wanted_Timestamp, AvSeekFlag flags)//XX 132
		{
			c_int m;

			c_int a = -1;
			c_int b = nb_Entries;

			// Optimize appending index entries at the end
			if ((b != 0) && (entries[b - 1].Timestamp < wanted_Timestamp))
				a = b - 1;

			while ((b - a) > 1)
			{
				m = (a + b) >> 1;

				// Search for next non-discarded packet
				while (((entries[m].Flags & AvIndex.Discard_Frame) != 0) && (m < b) && (m < (nb_Entries - 1)))
				{
					m++;

					if ((m == b) && (entries[m].Timestamp >= wanted_Timestamp))
					{
						m = b - 1;
						break;
					}
				}

				int64_t timestamp = entries[m].Timestamp;

				if (timestamp >= wanted_Timestamp)
					b = m;

				if (timestamp <= wanted_Timestamp)
					a = m;
			}

			m = (flags & AvSeekFlag.Backward) != 0 ? a : b;

			if ((flags & AvSeekFlag.Any) == 0)
			{
				while ((m >= 0) && (m < nb_Entries) && ((entries[m].Flags & AvIndex.KeyFrame) == 0))
					m += (flags & AvSeekFlag.Backward) != 0 ? -1 : 1;
			}

			if (m == nb_Entries)
				return -1;

			return m;
		}



		/********************************************************************/
		/// <summary>
		/// Get the index for a specific timestamp
		/// </summary>
		/********************************************************************/
		public static c_int Av_Index_Search_Timestamp(AvStream st, int64_t wanted_Timestamp, AvSeekFlag flags)//XX 245
		{
			FFStream sti = Internal.FFStream(st);

			return FF_Index_Search_Timestamp(sti.Index_Entries, sti.Nb_Index_Entries, wanted_Timestamp, flags);
		}



		/********************************************************************/
		/// <summary>
		/// Perform a binary search using av_index_search_timestamp() and
		/// FFInputFormat.read_timestamp()
		/// </summary>
		/********************************************************************/
		public static c_int FF_Seek_Frame_Binary(AvFormatContext s, c_int stream_Index, int64_t target_Ts, AvSeekFlag flags)//XX 290
		{
			FFInputFormat avif = Demux.FFIFmt(s.IFormat);
			int64_t pos_Min = 0, pos_Max = 0;
			int64_t ts_Min, ts_Max;

			if (stream_Index < 0)
				return -1;

			Log.Av_Log(s, Log.Av_Log_Trace, "read_seek: %d %s\n", stream_Index, Timestamp.Av_Ts2Str(target_Ts));

			ts_Max = ts_Min = UtilConstants.Av_NoPts_Value;
			int64_t pos_Limit = -1;

			AvStream st = s.Streams[stream_Index];
			FFStream sti = Internal.FFStream(st);

			if (sti.Index_Entries.IsNotNull)
			{
				// FIXME: Whole function must be checked for non-keyframe entries in
				// index case, especially read_timestamp()
				c_int index = Av_Index_Search_Timestamp(st, target_Ts, flags | AvSeekFlag.Backward);
				index = Macros.FFMax(index, 0);

				AvIndexEntry e = sti.Index_Entries[index];

				if ((e.Timestamp <= target_Ts) || (e.Pos == e.Min_Distance))
				{
					pos_Min = e.Pos;
					ts_Min = e.Timestamp;

					Log.Av_Log(s, Log.Av_Log_Trace, "using cached pos_min=0x%llx dts_min=%s\n", pos_Min, Timestamp.Av_Ts2Str(ts_Min));
				}

				index = Av_Index_Search_Timestamp(st, target_Ts, flags & ~AvSeekFlag.Backward);

				if (index >= 0)
				{
					e = sti.Index_Entries[index];

					pos_Max = e.Pos;
					ts_Max = e.Timestamp;
					pos_Limit = pos_Max - e.Min_Distance;

					Log.Av_Log(s, Log.Av_Log_Trace, "using cached pos_max=0x%llx pos_limit=0x%llx dts_max=%s\n", pos_Max, pos_Limit, Timestamp.Av_Ts2Str(ts_Max));
				}
			}

			int64_t pos = FF_Gen_Search(s, stream_Index, target_Ts, pos_Min, pos_Max, pos_Limit, ts_Min, ts_Max, flags, out int64_t ts, avif.Read_Timestamp);

			if (pos < 0)
				return -1;

			// Do the seek
			c_int ret = (c_int)AvIoBuf.AvIo_Seek(s.Pb, pos, AvSeek.Set);

			if (ret < 0)
				return ret;

			FF_Read_Frame_Flush(s);
			AvPriv_Update_Cur_Dts(s, st, ts);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Update cur_dts of all streams based on the given timestamp and
		/// AVStream.
		///
		/// Stream ref_st unchanged, others set cur_dts in their native
		/// time base. Only needed for timestamp wrapping or if (dts not set
		/// and pts!=dts)
		/// </summary>
		/********************************************************************/
		public static c_int FF_Find_Last_Ts(AvFormatContext s, c_int stream_Index, out int64_t ts, out int64_t pos, FormatFunc.Read_Timestamp_Delegate read_Timestamp_Func)//XX 360
		{
			ts = 0;
			pos = 0;

			int64_t step = 1024;
			int64_t fileSize = AvIoBuf.AvIo_Size(s.Pb);
			int64_t pos_Max = fileSize - 1;
			int64_t limit, ts_Max;

			do
			{
				limit = pos_Max;
				pos_Max = Macros.FFMax(0, pos_Max - step);

				ts_Max = Read_Timestamp(s, stream_Index, ref pos_Max, limit, read_Timestamp_Func);

				step += step;
			}
			while ((ts_Max == UtilConstants.Av_NoPts_Value) && ((2 * limit) > step));

			if (ts_Max == UtilConstants.Av_NoPts_Value)
				return -1;

			for (;;)
			{
				int64_t tmp_Pos = pos_Max + 1;
				int64_t tmp_Ts = Read_Timestamp(s, stream_Index, ref tmp_Pos, int64_t.MaxValue, read_Timestamp_Func);

				if (tmp_Ts == UtilConstants.Av_NoPts_Value)
					break;

				ts_Max = tmp_Ts;
				pos_Max = tmp_Pos;

				if (tmp_Pos >= fileSize)
					break;
			}

			ts = ts_Max;
			pos = pos_Max;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Perform a binary search using read_timestamp()
		/// </summary>
		/********************************************************************/
		public static int64_t FF_Gen_Search(AvFormatContext s, c_int stream_Index, int64_t target_Ts, int64_t pos_Min, int64_t pos_Max, int64_t pos_Limit, int64_t ts_Min, int64_t ts_Max, AvSeekFlag flags, out int64_t ts_Ret, FormatFunc.Read_Timestamp_Delegate read_Timestamp_Func)//XX 398
		{
			ts_Ret = 0;

			FFFormatContext si = Internal.FFFormatContext(s);
			int64_t pos, ts;

			Log.Av_Log(s, Log.Av_Log_Trace, "gen_seek: %d %s\n", stream_Index, Timestamp.Av_Ts2Str(target_Ts));

			if (ts_Min == UtilConstants.Av_NoPts_Value)
			{
				pos_Min = si.Data_Offset;
				ts_Min = Read_Timestamp(s, stream_Index, ref pos_Min, int64_t.MaxValue, read_Timestamp_Func);

				if (ts_Min == UtilConstants.Av_NoPts_Value)
					return -1;
			}

			if (ts_Min >= target_Ts)
			{
				ts_Ret = ts_Min;

				return pos_Min;
			}

			if (ts_Max == UtilConstants.Av_NoPts_Value)
			{
				c_int ret = FF_Find_Last_Ts(s, stream_Index, out ts_Max, out pos_Max, read_Timestamp_Func);

				if (ret < 0)
					return ret;

				pos_Limit = pos_Max;
			}

			if (ts_Max <= target_Ts)
			{
				ts_Ret = ts_Max;

				return pos_Max;
			}

			c_int no_Change = 0;

			while (pos_Min < pos_Limit)
			{
				Log.Av_Log(s, Log.Av_Log_Trace, "pos_min=0x%llx pos_max=0x%llx dts_min=%s dts_max=%s\n", pos_Min, pos_Max, Timestamp.Av_Ts2Str(ts_Min), Timestamp.Av_Ts2Str(ts_Max));

				if (no_Change == 0)
				{
					int64_t approximate_Keyframe_Distance = pos_Max - pos_Limit;

					// Interpolate position (better than dichotomy)
					pos = Mathematics.Av_Rescale(target_Ts - ts_Min, pos_Max - pos_Min, ts_Max - ts_Min) + pos_Min - approximate_Keyframe_Distance;
				}
				else if (no_Change == 1)
				{
					// Bisection if interpolation did not change min / max pos last time
					pos = (pos_Min + pos_Limit) >> 1;
				}
				else
				{
					// Linear search if bisection failed, can only happen if there
					// are very few or no keyframes between min/max
					pos = pos_Min;
				}

				if (pos <= pos_Min)
					pos = pos_Min + 1;
				else if (pos > pos_Limit)
					pos = pos_Limit;

				int64_t start_Pos = pos;

				// May pass pos_limit instead of -1
				ts = Read_Timestamp(s, stream_Index, ref pos, int64_t.MaxValue, read_Timestamp_Func);

				if (pos == pos_Max)
					no_Change++;
				else
					no_Change = 0;

				Log.Av_Log(s, Log.Av_Log_Trace, "%ld %ld %ld / %s %s %s target:%s limit:%ld start:%ld noc:%d\n", pos_Min, pos, pos_Max, Timestamp.Av_Ts2Str(ts_Min), Timestamp.Av_Ts2Str(ts), Timestamp.Av_Ts2Str(ts_Max), Timestamp.Av_Ts2Str(target_Ts), pos_Limit, start_Pos, no_Change);

				if (ts == UtilConstants.Av_NoPts_Value)
				{
					Log.Av_Log(s, Log.Av_Log_Error, "read_timestamp() failed in the middle\n");

					return -1;
				}

				if (target_Ts <= ts)
				{
					pos_Limit = stream_Index - 1;
					pos_Max = pos;
					ts_Max = ts;
				}

				if (target_Ts >= ts)
				{
					pos_Min = pos;
					ts_Min = ts;
				}
			}

			pos = (flags & AvSeekFlag.Backward) != 0 ? pos_Min : pos_Max;
			ts = (flags & AvSeekFlag.Backward) != 0 ? ts_Min : ts_Max;

			ts_Ret = ts;

			return pos;
		}



		/********************************************************************/
		/// <summary>
		/// Seek to the keyframe at timestamp
		/// </summary>
		/********************************************************************/
		public static c_int Av_Seek_Frame(AvFormatContext s, c_int stream_Index, int64_t timestamp, AvSeekFlag flags)//XX 641
		{
			if ((Demux.FFIFmt(s.IFormat).Read_Seek2 != null) && (Demux.FFIFmt(s.IFormat).Read_Seek == null))
			{
				int64_t min_Ts = int64_t.MinValue, max_Ts = int64_t.MaxValue;

				if ((flags & AvSeekFlag.Backward) != 0)
					max_Ts = timestamp;
				else
					min_Ts = timestamp;

				return AvFormat_Seek_File(s, stream_Index, min_Ts, timestamp, max_Ts, flags & AvSeekFlag.Backward);
			}

			c_int ret = Seek_Frame_Internal(s, stream_Index, timestamp, flags);

			if (ret >= 0)
				ret = Demux_Utils.AvFormat_Queue_Attached_Pictures(s);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Seek to timestamp ts.
		/// Seeking will be done so that the point from which all active
		/// streams can be presented successfully will be closest to ts and
		/// within min/max_ts. Active streams are all streams that have
		/// AVStream.discard ‹ AVDISCARD_ALL
		///
		/// If flags contain AVSEEK_FLAG_BYTE, then all timestamps are in
		/// bytes and are the file position (this may not be supported by all
		/// demuxers).
		/// If flags contain AVSEEK_FLAG_FRAME, then all timestamps are in
		/// frames in the stream with stream_index (this may not be supported
		/// by all demuxers).
		/// Otherwise all timestamps are in units of the stream selected by
		/// stream_index or if stream_index is -1, in AV_TIME_BASE units.
		/// If flags contain AVSEEK_FLAG_ANY, then non-keyframes are treated
		/// as keyframes (this may not be supported by all demuxers).
		/// If flags contain AVSEEK_FLAG_BACKWARD, it is ignored
		/// </summary>
		/********************************************************************/
		public static c_int AvFormat_Seek_File(AvFormatContext s, c_int stream_Index, int64_t min_Ts, int64_t ts, int64_t max_Ts, AvSeekFlag flags)//XX 664
		{
			c_int ret;

			if ((min_Ts > ts) || (max_Ts < ts))
				return -1;

			if ((stream_Index < -1) || (stream_Index >= (c_int)s.Nb_Streams))
				return Error.EINVAL;

			if (s.Seek2Any > 0)
				flags |= AvSeekFlag.Any;

			flags &= ~AvSeekFlag.Backward;

			if (Demux.FFIFmt(s.IFormat).Read_Seek2 != null)
			{
				FF_Read_Frame_Flush(s);

				if ((stream_Index == -1) && (s.Nb_Streams == 1))
				{
					AvRational time_Base = s.Streams[0].Time_Base;

					ts = Mathematics.Av_Rescale_Q(ts, UtilConstants.Av_Time_Base_Q, time_Base);
					min_Ts = Mathematics.Av_Rescale_Rnd(min_Ts, time_Base.Den, time_Base.Num * (int64_t)UtilConstants.Av_Time_Base, AvRounding.Up | AvRounding.Pass_MinMax);
					max_Ts = Mathematics.Av_Rescale_Rnd(max_Ts, time_Base.Den, time_Base.Num * (int64_t)UtilConstants.Av_Time_Base, AvRounding.Down | AvRounding.Pass_MinMax);

					stream_Index = 0;
				}

				ret = Demux.FFIFmt(s.IFormat).Read_Seek2(s, stream_Index, min_Ts, ts, max_Ts, flags);

				if (ret >= 0)
					ret = Demux_Utils.AvFormat_Queue_Attached_Pictures(s);

				return ret;
			}

			// Fall back on old API if new is not implemented but old is.
			// Note the old API has somewhat different semantics
			AvSeekFlag dir = (uint64_t)(ts - min_Ts) > (uint64_t)(max_Ts - ts) ? AvSeekFlag.Backward : AvSeekFlag.None;

			ret = Av_Seek_Frame(s, stream_Index, ts, flags | dir);

			if ((ret < 0) && (ts != min_Ts) && (max_Ts != ts))
			{
				ret = Av_Seek_Frame(s, stream_Index, dir != AvSeekFlag.None ? max_Ts : min_Ts, flags | dir);

				if (ret >= 0)
					ret = Av_Seek_Frame(s, stream_Index, ts, flags | (dir ^ AvSeekFlag.Backward));
			}

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Flush the frame reader
		/// </summary>
		/********************************************************************/
		public static void FF_Read_Frame_Flush(AvFormatContext s)// 716
		{
			AvFormat.FF_Flush_Packet_Queue(s);

			// Reset read state for each stream
			for (c_uint i = 0; i < s.Nb_Streams; i++)
			{
				AvStream st = s.Streams[i];
				FFStream sti = Internal.FFStream(st);

				if (sti.Parser != null)
				{
					Parser.Av_Parser_Close(sti.Parser);

					sti.Parser = null;
				}

				sti.Last_IP_Pts = UtilConstants.Av_NoPts_Value;
				sti.Last_Dts_For_Order_Check = UtilConstants.Av_NoPts_Value;

				if (sti.First_Dts == UtilConstants.Av_NoPts_Value)
					sti.Cur_Dts = AvFormatInternal.Relative_Ts_Base;
				else
				{
					// We set the current DTS to an unspecified origin
					sti.Cur_Dts = UtilConstants.Av_NoPts_Value;
				}

				sti.Probe_Packets = s.Max_Probe_Packets;

				for (c_int j = 0; j < (FFStream.Max_Reorder_Delay + 1); j++)
					sti.Pts_Buffer[j] = UtilConstants.Av_NoPts_Value;

				sti.Skip_Samples = 0;
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static int64_t Read_Timestamp(AvFormatContext s, c_int stream_Index, ref int64_t pPos, int64_t pos_Limit, FormatFunc.Read_Timestamp_Delegate read_Timestamp)//XX 281
		{
			int64_t ts = read_Timestamp(s, stream_Index, ref pPos, pos_Limit);

			if (stream_Index >= 0)
				ts = Demux.FF_Wrap_Timestamp(s.Streams[stream_Index], ts);

			return ts;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Seek_Frame_Byte(AvFormatContext s, c_int stream_Index, int64_t pos, AvSeekFlag flags)//XX 505
		{
			FFFormatContext si = Internal.FFFormatContext(s);

			int64_t pos_Min = si.Data_Offset;
			int64_t pos_Max = AvIoBuf.AvIo_Size(s.Pb) - 1;

			if (pos < pos_Min)
				pos = pos_Min;
			else if (pos > pos_Max)
				pos = pos_Max;

			AvIoBuf.AvIo_Seek(s.Pb, pos, AvSeek.Set);

			s.Io_Repositioned = 1;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Seek_Frame_Generic(AvFormatContext s, c_int stream_Index, int64_t timestamp, AvSeekFlag flags)//XX 526
		{
			FFFormatContext si = Internal.FFFormatContext(s);
			AvStream st = s.Streams[stream_Index];
			FFStream sti = Internal.FFStream(st);
			AvIndexEntry ie;
			int64_t ret;

			c_int index = Av_Index_Search_Timestamp(st, timestamp, flags);

			if ((index < 0) && (sti.Nb_Index_Entries != 0) && (timestamp < sti.Index_Entries[0].Timestamp))
				return -1;

			if ((index < 0) || (index == (sti.Nb_Index_Entries - 1)))
			{
				AvPacket pkt = si.Pkt;
				c_int nonKey = 0;

				if (sti.Nb_Index_Entries != 0)
				{
					ie = sti.Index_Entries[sti.Nb_Index_Entries - 1];

					ret = AvIoBuf.AvIo_Seek(s.Pb, ie.Pos, AvSeek.Set);

					if (ret < 0)
						return (c_int)ret;

					s.Io_Repositioned = 1;

					AvPriv_Update_Cur_Dts(s, st, ie.Timestamp);
				}
				else
				{
					ret = AvIoBuf.AvIo_Seek(s.Pb, si.Data_Offset, AvSeek.Set);

					if (ret < 0)
						return (c_int)ret;

					s.Io_Repositioned = 1;
				}

				Packet.Av_Packet_Unref(pkt);

				for (;;)
				{
					c_int read_Status;

					do
					{
						read_Status = Demux.Av_Read_Frame(s, pkt);
					}
					while (read_Status == Error.EAGAIN);

					if (read_Status < 0)
						break;

					if ((stream_Index == pkt.Stream_Index) && (pkt.Dts > timestamp))
					{
						if ((pkt.Flags & AvPktFlag.Key) != 0)
						{
							Packet.Av_Packet_Unref(pkt);
							break;
						}

						if ((nonKey++ > 1000) && (st.CodecPar.Codec_Id != AvCodecId.CdGraphics))
						{
							Log.Av_Log(s, Log.Av_Log_Error, "seek_frame_generic failed as this stream seems to contain no keyframes after the target timestamp, %d non keyframes found\n", nonKey);

							Packet.Av_Packet_Unref(pkt);
							break;
						}
					}

					Packet.Av_Packet_Unref(pkt);
				}

				index = Av_Index_Search_Timestamp(st, timestamp, flags);
			}

			if (index < 0)
				return -1;

			FF_Read_Frame_Flush(s);

			if (Demux.FFIFmt(s.IFormat).Read_Seek != null)
			{
				if (Demux.FFIFmt(s.IFormat).Read_Seek(s, stream_Index, timestamp, flags) >= 0)
					return 0;
			}

			ie = sti.Index_Entries[index];

			ret = AvIoBuf.AvIo_Seek(s.Pb, ie.Pos, AvSeek.Set);

			if (ret < 0)
				return (c_int)ret;

			s.Io_Repositioned = 1;

			AvPriv_Update_Cur_Dts(s, st, ie.Timestamp);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Seek_Frame_Internal(AvFormatContext s, c_int stream_Index, int64_t timestamp, AvSeekFlag flags)//XX 597
		{
			c_int ret;

			if ((flags & AvSeekFlag.Byte) != 0)
			{
				if ((s.IFormat.Flags & AvFmt.No_Byte_Seek) != 0)
					return -1;

				FF_Read_Frame_Flush(s);

				return Seek_Frame_Byte(s, stream_Index, timestamp, flags);
			}

			if (stream_Index < 0)
			{
				stream_Index = AvFormat.Av_Find_Default_Stream_Index(s);

				if (stream_Index < 0)
					return -1;

				AvStream st = s.Streams[stream_Index];

				// timestamp for default must be expressed in AV_TIME_BASE units
				timestamp = Mathematics.Av_Rescale(timestamp, st.Time_Base.Den, UtilConstants.Av_Time_Base * (int64_t)st.Time_Base.Num);
			}

			// First, we try the format specific seek
			if (Demux.FFIFmt(s.IFormat).Read_Seek != null)
			{
				FF_Read_Frame_Flush(s);

				ret = Demux.FFIFmt(s.IFormat).Read_Seek(s, stream_Index, timestamp, flags);
			}
			else
				ret = -1;

			if (ret >= 0)
				return 0;

			if ((Demux.FFIFmt(s.IFormat).Read_Timestamp != null) && ((s.IFormat.Flags & AvFmt.NoBinSearch) == 0))
			{
				FF_Read_Frame_Flush(s);

				return FF_Seek_Frame_Binary(s, stream_Index, timestamp, flags);
			}
			else if ((s.IFormat.Flags & AvFmt.NoGenSearch) == 0)
			{
				FF_Read_Frame_Flush(s);

				return Seek_Frame_Generic(s, stream_Index, timestamp, flags);
			}
			else
				return -1;
		}
		#endregion
	}
}
