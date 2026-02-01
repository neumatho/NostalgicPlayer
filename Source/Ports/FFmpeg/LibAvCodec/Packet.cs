/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec
{
	/// <summary>
	/// 
	/// </summary>
	public static class Packet
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool AvPacket_Is_Empty(AvPacket pkt)
		{
			return pkt.Data.IsNull && (pkt.Side_Data_Elems == 0);
		}



		/********************************************************************/
		/// <summary>
		/// Allocate an AVPacket and set its fields to default values.
		///
		/// Note this only allocates the AVPacket itself, not the data
		/// buffers. Those must be allocated through other means such as
		/// av_new_packet
		/// </summary>
		/********************************************************************/
		public static AvPacket Av_Packet_Alloc()//XX 64
		{
			AvPacket pkt = Mem.Av_MAllocObj<AvPacket>();
			if (pkt == null)
				return null;

			Get_Packet_Defaults(pkt);

			return pkt;
		}



		/********************************************************************/
		/// <summary>
		/// Free the packet, if the packet is reference counted, it will be
		/// unreferenced first
		/// </summary>
		/********************************************************************/
		public static void Av_Packet_Free(ref AvPacket pkt)//XX 75
		{
			if (pkt == null)
				return;

			Av_Packet_Unref(pkt);
			Mem.Av_FreeP(ref pkt);
		}



		/********************************************************************/
		/// <summary>
		/// Allocate the payload of a packet and initialize its fields with
		/// default values
		/// </summary>
		/********************************************************************/
		public static c_int Av_New_Packet(AvPacket pkt, c_int size)//XX 99
		{
			AvBufferRef buf = null;

			c_int ret = Packet_Alloc(ref buf, size);

			if (ret < 0)
				return ret;

			Get_Packet_Defaults(pkt);

			pkt.Buf = buf;
			pkt.Data = ((DataBufferContext)buf.Data).Data;
			pkt.Size = size;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Reduce packet size, correctly zeroing padding
		/// </summary>
		/********************************************************************/
		public static void Av_Shrink_Packet(AvPacket pkt, c_int size)//XX 114
		{
			if (pkt.Size <= size)
				return;

			pkt.Size = size;

			CMemory.memset<uint8_t>(pkt.Data + size, 0, Defs.Av_Input_Buffer_Padding_Size);
		}



		/********************************************************************/
		/// <summary>
		/// Increase packet size, correctly zeroing padding
		/// </summary>
		/********************************************************************/
		public static c_int Av_Grow_Packet(AvPacket pkt, c_int grow_By)//XX 122
		{
			if ((c_uint)grow_By > (c_int.MaxValue - (pkt.Size + Defs.Av_Input_Buffer_Padding_Size)))
				return Error.ENOMEM;

			c_int new_Size = pkt.Size + grow_By + Defs.Av_Input_Buffer_Padding_Size;

			if (pkt.Buf != null)
			{
				size_t data_Offset;
				CPointer<uint8_t> old_Data = pkt.Data;

				if (pkt.Data.IsNull)
				{
					data_Offset = 0;
					pkt.Data = ((DataBufferContext)pkt.Buf.Data).Data;
				}
				else
				{
					data_Offset = (size_t)(pkt.Data - ((DataBufferContext)pkt.Buf.Data).Data);

					if (data_Offset > (size_t)(c_int.MaxValue - new_Size))
						return Error.ENOMEM;
				}

				if ((((size_t)new_Size + data_Offset) > ((DataBufferContext)pkt.Buf.Data).Size) || (Buffer.Av_Buffer_Is_Writable(pkt.Buf) == 0))
				{
					// Allocate slightly more than requested to avoid excessive reallocations
					if (((size_t)new_Size + data_Offset) < (size_t)(c_int.MaxValue - (new_Size / 16)))
						new_Size += new_Size / 16;

					c_int ret = Buffer.Av_Buffer_Realloc(ref pkt.Buf, (size_t)new_Size + data_Offset);

					if (ret < 0)
					{
						pkt.Data = old_Data;

						return ret;
					}

					pkt.Data = ((DataBufferContext)pkt.Buf.Data).Data + data_Offset;
				}
			}
			else
			{
				pkt.Buf = Buffer.Av_Buffer_Alloc((size_t)new_Size);

				if (pkt.Buf == null)
					return Error.ENOMEM;

				if (pkt.Size > 0)
					CMemory.memcpy(((DataBufferContext)pkt.Buf.Data).Data, pkt.Data, (size_t)pkt.Size);

				pkt.Data = ((DataBufferContext)pkt.Buf.Data).Data;
			}

			pkt.Size += grow_By;

			CMemory.memset<uint8_t>(pkt.Data + pkt.Size, 0, Defs.Av_Input_Buffer_Padding_Size);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Convenience function to free all the side data stored.
		/// All the other fields stay untouched
		/// </summary>
		/********************************************************************/
		public static void Av_Packet_Free_Side_Data(AvPacket pkt)//XX 189
		{
			for (c_int i = 0; i < pkt.Side_Data_Elems; i++)
				Mem.Av_FreeP(ref pkt.Side_Data[i].Data);

			Mem.Av_FreeP(ref pkt.Side_Data);
			pkt.Side_Data_Elems = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Wrap an existing array as a packet side data
		/// </summary>
		/********************************************************************/
		public static c_int Av_Packet_Add_Side_Data(AvPacket pkt, AvPacketSideDataType type, IDataContext data)//XX 198
		{
			c_int elems = pkt.Side_Data_Elems;

			for (c_int i = 0; i < elems; i++)
			{
				AvPacketSideData sd = pkt.Side_Data[i];

				if (sd.Type == type)
				{
					Mem.Av_Free(sd.Data);

					sd.Data = data;

					return 0;
				}
			}

			if (((c_uint)elems + 1) > (c_uint)AvPacketSideDataType.Nb)
				return Error.ERANGE;

			CPointer<AvPacketSideData> tmp = Mem.Av_ReallocObj(pkt.Side_Data, (size_t)elems + 1);

			if (tmp.IsNull)
				return Error.ENOMEM;

			pkt.Side_Data = tmp;
			pkt.Side_Data[elems].Data = data;
			pkt.Side_Data[elems].Type = type;
			pkt.Side_Data_Elems++;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Allocate new information of a packet
		/// </summary>
		/********************************************************************/
		public static DataBufferContext Av_Packet_New_Side_Data(AvPacket pkt, AvPacketSideDataType type, size_t size)//XX 232
		{
			if (size > (size_t.MaxValue - Defs.Av_Input_Buffer_Padding_Size))
				return null;

			CPointer<uint8_t> data = Mem.Av_MAllocz<uint8_t>(size + Defs.Av_Input_Buffer_Padding_Size);

			if (data.IsNull)
				return null;

			return (DataBufferContext)Av_Packet_New_Side_Data(pkt, type, new DataBufferContext(data, size));
		}



		/********************************************************************/
		/// <summary>
		/// Allocate new information of a packet
		/// </summary>
		/********************************************************************/
		public static IDataContext Av_Packet_New_Side_Data(AvPacket pkt, AvPacketSideDataType type, IDataContext data)//XX 232
		{
			if (data == null)
				return null;

			c_int ret = Av_Packet_Add_Side_Data(pkt, type, data);

			if (ret < 0)
			{
				Mem.Av_FreeP(ref data);

				return null;
			}

			return data;
		}



		/********************************************************************/
		/// <summary>
		/// Get side information from packet
		/// </summary>
		/********************************************************************/
		public static IDataContext Av_Packet_Get_Side_Data(AvPacket pkt, AvPacketSideDataType type)//XX 253
		{
			for (c_int i = 0; i < pkt.Side_Data_Elems; i++)
			{
				if (pkt.Side_Data[i].Type == type)
					return pkt.Side_Data[i].Data;
			}

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Unpack a dictionary from side_data
		/// </summary>
		/********************************************************************/
		public static c_int Av_Packet_Unpack_Dictionary(CPointer<char> data, size_t size, ref AvDictionary dict)//XX 353
		{
			if ((dict == null) || data.IsNull || (size == 0))
				return 0;

			CPointer<char> end = data + size;

			if ((size != 0) && (end[-1] != 0))
				return Error.InvalidData;

			while (data < end)
			{
				CPointer<char> key = data;
				CPointer<char> val = data + CString.strlen(key) + 1;

				if ((val >= end) || (key[0] == 0))
					return Error.InvalidData;

				c_int ret = Dict.Av_Dict_Set(ref dict, key, val, 0);

				if (ret < 0)
					return ret;

				data = val + CString.strlen(val) + 1;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Copy only "properties" fields from src to dst.
		///
		/// Properties for the purpose of this function are all the fields
		/// beside those related to the packet data (buf, data, size)
		/// </summary>
		/********************************************************************/
		public static c_int Av_Packet_Copy_Props(AvPacket dst, AvPacket src)//XX 396
		{
			dst.Pts = src.Pts;
			dst.Dts = src.Dts;
			dst.Pos = src.Pos;
			dst.Duration = src.Duration;
			dst.Flags = src.Flags;
			dst.Stream_Index = src.Stream_Index;
			dst.Opaque = src.Opaque;
			dst.Time_Base = src.Time_Base;
			dst.Opaque_Ref = null;
			dst.Side_Data = null;
			dst.Side_Data_Elems = 0;

			c_int ret = Buffer.Av_Buffer_Replace(ref dst.Opaque_Ref, src.Opaque_Ref);

			if (ret < 0)
				return ret;

			for (c_int i = 0; i < src.Side_Data_Elems; i++)
			{
				AvPacketSideDataType type = src.Side_Data[i].Type;
				IDataContext src_Data = src.Side_Data[i].Data;

				IDataContext dst_Data = Av_Packet_New_Side_Data(dst, type, src_Data.Allocator());

				if (dst_Data == null)
				{
					Buffer.Av_Buffer_Unref(ref dst.Opaque_Ref);
					Av_Packet_Free_Side_Data(dst);

					return Error.ENOMEM;
				}

				src_Data.CopyTo(dst_Data);
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Wipe the packet.
		///
		/// Unreference the buffer referenced by the packet and reset the
		/// remaining packet fields to their default values
		/// </summary>
		/********************************************************************/
		public static void Av_Packet_Unref(AvPacket pkt)//XX 433
		{
			Av_Packet_Free_Side_Data(pkt);
			Buffer.Av_Buffer_Unref(ref pkt.Opaque_Ref);
			Buffer.Av_Buffer_Unref(ref pkt.Buf);

			Get_Packet_Defaults(pkt);
		}



		/********************************************************************/
		/// <summary>
		/// Setup a new reference to the data described by a given packet
		///
		/// If src is reference-counted, setup dst as a new reference to the
		/// buffer in src. Otherwise allocate a new buffer in dst and copy
		/// the data from src into it.
		///
		/// All the other fields are copied from src.
		///
		/// See av_packet_unref
		/// </summary>
		/********************************************************************/
		public static c_int Av_Packet_Ref(AvPacket dst, AvPacket src)//XX 441
		{
			dst.Buf = null;

			c_int ret = Av_Packet_Copy_Props(dst, src);

			if (ret < 0)
				goto Fail;

			if (src.Buf == null)
			{
				ret = Packet_Alloc(ref dst.Buf, src.Size);

				if (ret < 0)
					goto Fail;

				if (src.Size != 0)
					CMemory.memcpy(((DataBufferContext)dst.Buf.Data).Data, src.Data, (size_t)src.Size);

				dst.Data = ((DataBufferContext)dst.Buf.Data).Data;
			}
			else
			{
				dst.Buf = Buffer.Av_Buffer_Ref(src.Buf);

				if (dst.Buf == null)
				{
					ret = Error.ENOMEM;

					goto Fail;
				}

				dst.Data = src.Data;
			}

			dst.Size = src.Size;

			return 0;

			Fail:
			Av_Packet_Unref(dst);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Move every field in src to dst and reset src.
		///
		/// See av_packet_unref
		/// </summary>
		/********************************************************************/
		public static void Av_Packet_Move_Ref(AvPacket dst, AvPacket src)//XX 490
		{
			src.CopyTo(dst);

			Get_Packet_Defaults(src);
		}



		/********************************************************************/
		/// <summary>
		/// Ensure the data described by a given packet is reference counted.
		///
		/// Note: This function does not ensure that the reference will be
		/// writable. Use av_packet_make_writable instead for that purpose
		/// </summary>
		/********************************************************************/
		public static c_int Av_Packet_Make_RefCounted(AvPacket pkt)//XX 496
		{
			if (pkt.Buf != null)
				return 0;

			c_int ret = Packet_Alloc(ref pkt.Buf, pkt.Size);

			if (ret < 0)
				return ret;

			if (pkt.Size != 0)
				CMemory.memcpy(((DataBufferContext)pkt.Buf.Data).Data, pkt.Data, (size_t)pkt.Size);

			pkt.Data = ((DataBufferContext)pkt.Buf.Data).Data;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Append an AVPacket to the list
		/// </summary>
		/********************************************************************/
		public static c_int AvPriv_Packet_List_Put(PacketList packet_Buffer, AvPacket pkt, CodecFunc.Packet_Copy_Delegate copy, FFPacketListFlag flags)//XX 547
		{
			PacketListEntry pktl = Mem.Av_MAllocObj<PacketListEntry>();
			c_uint update_End_Point = 1;
			c_int ret;

			if (pktl == null)
				return Error.ENOMEM;

			if (copy != null)
			{
				Get_Packet_Defaults(pktl.Pkt);

				ret = copy(pktl.Pkt, pkt);

				if (ret < 0)
				{
					Mem.Av_Free(pktl);

					return ret;
				}
			}
			else
			{
				ret = Av_Packet_Make_RefCounted(pkt);

				if (ret < 0)
				{
					Mem.Av_Free(pktl);

					return ret;
				}

				Av_Packet_Move_Ref(pktl.Pkt, pkt);
			}

			pktl.Next = null;

			if (packet_Buffer.Head != null)
			{
				if ((flags & FFPacketListFlag.Prepend) != 0)
				{
					pktl.Next = packet_Buffer.Head;
					packet_Buffer.Head = pktl;
					update_End_Point = 0;
				}
				else
					packet_Buffer.Tail.Next = pktl;
			}
			else
				packet_Buffer.Head = pktl;

			if (update_End_Point != 0)
			{
				// Add the packet in the buffered packet list
				packet_Buffer.Tail = pktl;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Remove the oldest AVPacket in the list and return it.
		///
		/// Note: The pkt will be overwritten completely on success. The
		///       caller owns the packet and must unref it by itself
		/// </summary>
		/********************************************************************/
		public static c_int AvPriv_Packet_List_Get(PacketList pkt_Buffer, AvPacket pkt)//XX 596
		{
			PacketListEntry pktl = pkt_Buffer.Head;

			if (pktl == null)
				return Error.EAGAIN;

			pktl.Pkt.CopyTo(pkt);
			pkt_Buffer.Head = pktl.Next;

			if (pkt_Buffer.Head == null)
				pkt_Buffer.Tail = null;

			Mem.Av_FreeP(ref pktl);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Wipe the list and unref all the packets in it
		/// </summary>
		/********************************************************************/
		public static void AvPriv_Packet_List_Free(PacketList pkt_Buf)//XX 610
		{
			PacketListEntry tmp = pkt_Buf.Head;

			while (tmp != null)
			{
				PacketListEntry pktl = tmp;
				tmp = pktl.Next;
				Av_Packet_Unref(pktl.Pkt);
				Mem.Av_FreeP(ref pktl);
			}

			pkt_Buf.Head = pkt_Buf.Tail = null;
		}



		/********************************************************************/
		/// <summary>
		/// Wrap existing data as packet side data
		/// </summary>
		/********************************************************************/
		public static AvPacketSideData Av_Packet_Side_Data_Add(ref CPointer<AvPacketSideData> pSd, ref c_int pNb_Sd, AvPacketSideDataType type, IDataContext data, c_int flags)//XX 713
		{
			return Packet_Side_Data_Add(ref pSd, ref pNb_Sd, type, data);
		}



		/********************************************************************/
		/// <summary>
		/// Convenience function to free all the side data stored in an
		/// array, and the array itself
		/// </summary>
		/********************************************************************/
		public static void Av_Packet_Side_Data_Free(ref CPointer<AvPacketSideData> pSd, ref c_int pNb_Sd)//XX 758
		{
			CPointer<AvPacketSideData> sd = pSd;
			c_int nb_Sd = pNb_Sd;

			for (c_int i = 0; i < nb_Sd; i++)
				Mem.Av_Free(sd[i].Data);

			Mem.Av_FreeP(ref pSd);
			pNb_Sd = 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Get_Packet_Defaults(AvPacket pkt)//XX 54
		{
			pkt.Clear();

			pkt.Pts = UtilConstants.Av_NoPts_Value;
			pkt.Dts = UtilConstants.Av_NoPts_Value;
			pkt.Pos = -1;
			pkt.Time_Base = Rational.Av_Make_Q(0, 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Packet_Alloc(ref AvBufferRef buf, c_int size)//XX 84
		{
			if ((size < 0) || (size >= (c_int.MaxValue - Defs.Av_Input_Buffer_Padding_Size)))
				return Error.EINVAL;

			c_int ret = Buffer.Av_Buffer_Realloc(ref buf, (size_t)size + Defs.Av_Input_Buffer_Padding_Size);

			if (ret < 0)
				return ret;

			CMemory.memset<uint8_t>(((DataBufferContext)buf.Data).Data + size, 0, Defs.Av_Input_Buffer_Padding_Size);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static AvPacketSideData Packet_Side_Data_Add(ref CPointer<AvPacketSideData> pSd, ref c_int pNb_Sd, AvPacketSideDataType type, IDataContext data)//XX 680
		{
			CPointer<AvPacketSideData> sd = pSd;
			c_int nb_sd = pNb_Sd;

			for (c_int i = 0; i < nb_sd; i++)
			{
				if (sd[i].Type != type)
					continue;

				Mem.Av_Free(sd[i].Data);

				sd[i].Data = data;

				return sd[i];
			}

			if (nb_sd == c_int.MaxValue)
				return null;

			CPointer<AvPacketSideData> tmp = Mem.Av_Realloc_ArrayObj(sd, (size_t)nb_sd + 1);

			if (tmp.IsNull)
				return null;

			pSd = sd = tmp;

			sd[nb_sd].Type = type;
			sd[nb_sd].Data = data;

			pNb_Sd = nb_sd + 1;

			return sd[nb_sd];
		}
		#endregion
	}
}
