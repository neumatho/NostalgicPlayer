/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
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
	}
}
