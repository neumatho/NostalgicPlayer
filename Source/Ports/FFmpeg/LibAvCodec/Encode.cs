/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Buffer = Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Buffer;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec
{
	/// <summary>
	/// Generic encoding-related code
	/// </summary>
	public static class Encode
	{
		/********************************************************************/
		/// <summary>
		/// The default callback for AVCodecContext.get_encode_buffer(). It
		/// is made public so it can be called by custom get_encode_buffer()
		/// implementations for encoders without AV_CODEC_CAP_DR1 set
		/// </summary>
		/********************************************************************/
		public static c_int AvCodec_Default_Get_Encode_Buffer(AvCodecContext avCtx, AvPacket avPkt, c_int flags)//XX 82
		{
			if ((avPkt.Size < 0) || (avPkt.Size > (c_int.MaxValue - Defs.Av_Input_Buffer_Padding_Size)))
				return Error.EINVAL;

			if (avPkt.Data.IsNotNull || (avPkt.Buf != null))
			{
				Log.Av_Log(avCtx, Log.Av_Log_Error, "avpkt->{data,buf} != NULL in avcodec_default_get_encode_buffer()\n");

				return Error.EINVAL;
			}

			c_int ret = Buffer.Av_Buffer_Realloc(ref avPkt.Buf, (size_t)avPkt.Size + Defs.Av_Input_Buffer_Padding_Size);

			if (ret < 0)
			{
				Log.Av_Log(avCtx, Log.Av_Log_Error, "Failed to allocate packet of size %d\n", avPkt.Size);

				return ret;
			}

			avPkt.Data = ((DataBufferContext)avPkt.Buf.Data).Data;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Perform encoder initialization and validation.
		/// Called when opening the encoder, before the FFCodec.init() call
		/// </summary>
		/********************************************************************/
		internal static c_int FF_Encode_Preinit(AvCodecContext avCtx)//XX 745
		{
			throw new NotImplementedException("FF_Encode_Receive_Frame");
		}



		/********************************************************************/
		/// <summary>
		/// avcodec_receive_frame() implementation for encoders
		/// </summary>
		/********************************************************************/
		public static c_int FF_Encode_Receive_Frame(AvCodecContext avCtx, AvFrame frame)//XX 858
		{
			throw new NotImplementedException("FF_Encode_Receive_Frame");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal static AvCodecInternal FF_Encode_Internal_Alloc()//XX 881
		{
			return Mem.Av_MAlloczObj<EncodeContext>();
		}
	}
}
