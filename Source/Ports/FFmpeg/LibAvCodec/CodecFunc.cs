/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Interfaces;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec
{
	/// <summary>
	/// All function delegates
	/// </summary>
	public static class CodecFunc
	{
		/// <summary></summary>
		public delegate void Draw_Horiz_Band_Delegate(AvCodecContext s, AvFrame src, c_int[] offset, c_int y, c_int type, c_int height);
		/// <summary></summary>
		public delegate AvPixelFormat Get_Format_Delegate(AvCodecContext s, CPointer<AvPixelFormat> fmt);
		/// <summary></summary>
		public delegate c_int Get_Buffer2_Delegate(AvCodecContext s, AvFrame frame, c_int flags);
		/// <summary></summary>
		public delegate c_int Execute_Func_Delegate(AvCodecContext c2, IExecuteArg arg);
		/// <summary></summary>
		public delegate c_int Execute_Delegate(AvCodecContext s, Execute_Func_Delegate func, CPointer<IExecuteArg> arg2, CPointer<c_int> ret, c_int count);
		/// <summary></summary>
		public delegate c_int Execute2_Func_Delegate(AvCodecContext c2, IExecuteArg arg, c_int jobNr, c_int threadNr);
		/// <summary></summary>
		public delegate c_int Execute2_Delegate(AvCodecContext s, Execute2_Func_Delegate func, CPointer<IExecuteArg> arg2, CPointer<c_int> ret, c_int count);
		/// <summary></summary>
		public delegate c_int Main_Func_Delegate(AvCodecContext c);
		/// <summary></summary>
		public delegate c_int Get_Encode_Buffer_Delegate(AvCodecContext s, AvPacket pkt, c_int flags);

		/// <summary></summary>
		public delegate c_int Parser_Init_Delegate(AvCodecParserContext s);
		/// <summary></summary>
		public delegate c_int Parser_Parse_Delegate(AvCodecParserContext s, AvCodecContext avCtx, out CPointer<uint8_t> pOutBuf, out c_int pOutBuf_Size, CPointer<uint8_t> buf, c_int buf_Size);
		/// <summary></summary>
		public delegate void Parser_Close_Delegate(AvCodecParserContext s);

		/// <summary></summary>
		public delegate c_int Init_Bsf_Delegate(AvBsfContext ctx);
		/// <summary></summary>
		public delegate c_int Filter_Bsf_Delegate(AvBsfContext ctx, AvPacket pkt);
		/// <summary></summary>
		public delegate void Close_Bsf_Delegate(AvBsfContext ctx);
		/// <summary></summary>
		public delegate void Flush_Bsf_Delegate(AvBsfContext ctx);

		/// <summary></summary>
		public delegate c_int Update_Thread_Context_Delegate(AvCodecContext dst, AvCodecContext src);
		/// <summary></summary>
		public delegate c_int Update_Thread_Context_For_User_Delegate(AvCodecContext dst, AvCodecContext src);

		/// <summary></summary>
		public delegate c_int Init_Delegate(AvCodecContext avCtx);
		/// <summary></summary>
		public delegate c_int Uninit_Delegate(AvCodecContext avCtx);
		/// <summary></summary>
		public delegate c_int Close_Delegate(AvCodecContext avCtx);
		/// <summary></summary>
		public delegate void Flush_Delegate(AvCodecContext avCtx);
		/// <summary></summary>
		public delegate c_int Get_Supported_Config_Delegate(AvCodecContext avCtx, AvCodec codec, AvCodecConfig config, c_uint flags, out object out_Configs, out c_int out_Num_Configs);//XX

		/// <summary></summary>
		public delegate c_int Decode_Delegate(AvCodecContext avCtx, AvFrame frame, out c_int got_Frame_Ptr, AvPacket avPkt);
		/// <summary></summary>
		public delegate c_int Decode_Sub_Delegate(AvCodecContext avCtx, AvSubtitle sub, out c_int got_Frame_Ptr, AvPacket avPkt);
		/// <summary></summary>
		public delegate c_int Receive_Frame_Delegate(AvCodecContext avCtx, AvFrame frame);
		/// <summary></summary>
		public delegate c_int Encode_Delegate(AvCodecContext avCtx, AvPacket avPkt, AvFrame frame, out c_int got_Packet_Ptr);
		/// <summary></summary>
		public delegate c_int Encode_Sub_Delegate(AvCodecContext avCtx, CPointer<uint8_t> buf, c_int buf_Size, AvSubtitle sub);
		/// <summary></summary>
		public delegate c_int Receive_Packet_Delegate(AvCodecContext avCtx, AvPacket avPkt);

		/// <summary></summary>
		public delegate c_int Alloc_Frame_Delegate(AvCodecContext avCtx, AvFrame frame);
		/// <summary></summary>
		public delegate c_int Start_Frame_Delegate(AvCodecContext avCtx, AvBufferRef buf_Ref, CPointer<uint8_t> buf, uint32_t buf_Size);
		/// <summary></summary>
		public delegate c_int Decode_Params_Delegate(AvCodecContext avCtx, c_int type, CPointer<uint8_t> buf, uint32_t buf_Size);
		/// <summary></summary>
		public delegate c_int Decode_Slice_Delegate(AvCodecContext avCtx, CPointer<uint8_t> buf, uint32_t buf_Size);
		/// <summary></summary>
		public delegate c_int End_Frame_Delegate(AvCodecContext avCtx);

		/// <summary></summary>
		public delegate c_int Frame_Params_Delegate(AvCodecContext avCtx, AvBufferRef hw_Frames_Ctx);
		/// <summary></summary>
		public delegate void Free_Frame_Priv_Delegate(AvRefStructOpaque hwCtx, IOpaque priv_Data);

		/// <summary></summary>
		public delegate IPrivateData Private_Data_Alloc_Delegate();

		/// <summary></summary>
		public delegate c_int Packet_Copy_Delegate(AvPacket dst, AvPacket src);

		/// <summary></summary>
		public delegate c_int Post_Process_Delegate(IClass logCtx, AvFrame frame);
		/// <summary></summary>
		public delegate void Post_Process_Opaque_Free_Delegate(IOpaque opaque);

		/// <summary></summary>
		public delegate void HwAccel_Priv_Free_Delegate(IPrivateData priv);
	}
}
