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
	public static class Utils_Format
	{
		/// <summary>
		/// An arbitrarily chosen "sane" max packet size -- 50M
		/// </summary>
		private const c_int Sane_Chunk_Size = 50000000;

		/********************************************************************/
		/// <summary>
		/// Allocate and read the payload of a packet and initialize its
		/// fields with default values
		/// </summary>
		/********************************************************************/
		public static c_int Av_Get_Packet(AvIoContext s, AvPacket pkt, c_int size)//XX 94
		{
			Packet.Av_Packet_Unref(pkt);

			pkt.Pos = AvIoBuf.AvIo_Tell(s);

			return Append_Packet_Chunked(s, pkt, size);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal static AvCodecId FF_Codec_Get_Id(CPointer<AvCodecTag> tags, c_uint tag)//XX 139
		{
			for (c_int i = 0; i < tags.Length; i++)
			{
				if (tag == tags[i].Tag)
					return tags[i].Id;
			}

			for (c_int i = 0; i < tags.Length; i++)
			{
				if (To_Upper4.FF_ToUpper4(tag) == To_Upper4.FF_ToUpper4(tags[i].Tag))
					return tags[i].Id;
			}

			return AvCodecId.None;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal static AvCodecId FF_Get_Pcm_Codec_Id(c_int bps, c_int flt, c_int be, c_int sFlags)//XX 150
		{
			if ((bps <= 0) || (bps > 64))
				return AvCodecId.None;

			if (flt != 0)
			{
				switch (bps)
				{
					case 32:
						return be != 0 ? AvCodecId.Pcm_F32Be : AvCodecId.Pcm_F32Le;

					case 64:
						return be != 0 ? AvCodecId.Pcm_F64Be : AvCodecId.Pcm_F64Le;

					default:
						return AvCodecId.None;
				}
			}
			else
			{
				bps += 7;
				bps >>= 3;

				if ((sFlags & (1 << (bps - 1))) != 0)
				{
					switch (bps)
					{
						case 1:
							return AvCodecId.Pcm_S8;

						case 2:
							return be != 0 ? AvCodecId.Pcm_S16Be : AvCodecId.Pcm_S16Le;

						case 3:
							return be != 0 ? AvCodecId.Pcm_S24Be : AvCodecId.Pcm_S24Le;

						case 4:
							return be != 0 ? AvCodecId.Pcm_S32Be : AvCodecId.Pcm_S32Le;

						case 8:
							return be != 0 ? AvCodecId.Pcm_S64Be : AvCodecId.Pcm_S64Le;

						default:
							return AvCodecId.None;
					}
				}
				else
				{
					switch (bps)
					{
						case 1:
							return AvCodecId.Pcm_U8;

						case 2:
							return be != 0 ? AvCodecId.Pcm_U16Be : AvCodecId.Pcm_U16Le;

						case 3:
							return be != 0 ? AvCodecId.Pcm_U24Be : AvCodecId.Pcm_U24Le;

						case 4:
							return be != 0 ? AvCodecId.Pcm_U32Be : AvCodecId.Pcm_U32Le;

						default:
							return AvCodecId.None;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Allocate extradata with additional AV_INPUT_BUFFER_PADDING_SIZE
		/// at end which is always set to 0.
		///
		/// Previously allocated extradata in par will be freed
		/// </summary>
		/********************************************************************/
		internal static c_int FF_Alloc_ExtraData(AvCodecParameters par, c_int size)//XX 233
		{
			Mem.Av_FreeP(ref par.ExtraData);

			if ((size < 0) || (size >= (int32_t.MaxValue - Defs.Av_Input_Buffer_Padding_Size)))
				return Error.EINVAL;

			CPointer<uint8_t> data = Mem.Av_MAlloc<uint8_t>((size_t)(size + Defs.Av_Input_Buffer_Padding_Size));

			if (data.IsNull)
				return Error.ENOMEM;

			par.ExtraData = new DataBufferContext(data, (size_t)size);

			CMemory.memset<uint8_t>(data + size, 0, Defs.Av_Input_Buffer_Padding_Size);

			return 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Read the data in sane-sized chunks and append to pkt.
		/// Return the number of bytes read or an error
		/// </summary>
		/********************************************************************/
		private static c_int Append_Packet_Chunked(AvIoContext s, AvPacket pkt, c_int size)//XX 55
		{
			c_int orig_Size = pkt.Size;
			c_int ret;

			do
			{
				c_int prev_Size = pkt.Size;

				// When the caller requests a lot of data, limit it to the amount
				// left in file or SANE_CHUNK_SIZE when it is not known
				c_int read_Size = size;

				if (read_Size > (Sane_Chunk_Size / 10))
				{
					read_Size = AvIoBuf.FFIo_Limit(s, read_Size);

					// If filesize/maxsize is unknown, limit to SANE_CHUNK_SIZE
					if (AvIo_Internal.FFIoContext(s).MaxSize < 0)
						read_Size = Macros.FFMin(read_Size, Sane_Chunk_Size);
				}

				ret = Packet.Av_Grow_Packet(pkt, read_Size);

				if (ret < 0)
					break;

				ret = AvIoBuf.AvIo_Read(s, pkt.Data + prev_Size, read_Size);

				if (ret != read_Size)
				{
					Packet.Av_Shrink_Packet(pkt, prev_Size + Macros.FFMax(ret, 0));
					break;
				}

				size -= read_Size;
			}
			while (size > 0);

			if (size > 0)
				pkt.Flags |= AvPktFlag.Corrupt;

			if (pkt.Size == 0)
				Packet.Av_Packet_Unref(pkt);

			return pkt.Size > orig_Size ? pkt.Size - orig_Size : ret;
		}
		#endregion
	}
}
