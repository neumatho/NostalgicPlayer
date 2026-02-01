/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class FFCodec : AvCodec
	{
		/// <summary>
		/// The public AVCodec. See codec.h for it
		/// </summary>
		public AvCodec P => this;

		/// <summary>
		/// Internal codec capabilities FF_CODEC_CAP_*.
		/// </summary>
		public FFCodecCap Caps_Internal;

		/// <summary>
		/// Is this a decoder?
		/// </summary>
		public bool Is_Decoder;

		// <summary>
		// This field determines the video color ranges supported by an encoder.
		// Should be set to a bitmask of AVCOL_RANGE_MPEG and AVCOL_RANGE_JPEG.
		// </summary>
//		public AvColorRange Color_Ranges;

		/// <summary>
		/// This field determines the type of the codec (decoder/encoder)
		/// and also the exact callback cb implemented by the codec.
		/// cb_type uses enum FFCodecType values
		/// </summary>
		public FFCodecType Cb_Type;

		// <summary>
		// This field determines the alpha modes supported by an encoder
		// </summary>
//		public CPointer<AvAlphaMode> Alpha_Modes;

		/// <summary>
		/// 
		/// </summary>
//		public c_int Priv_Data_Size;
		public CodecFunc.Private_Data_Alloc_Delegate Priv_Data_Alloc;

		/// <summary>
		/// Copy necessary context variables from a previous thread context to the current one.
		/// If not defined, the next thread will start automatically; otherwise, the codec
		/// must call ff_thread_finish_setup().
		///
		/// dst and src will (rarely) point to the same context, in which case memcpy should be skipped
		/// </summary>
		public readonly CodecFunc.Update_Thread_Context_Delegate Update_Thread_Context = null;

		/// <summary>
		/// Copy variables back to the user-facing context
		/// </summary>
		public readonly CodecFunc.Update_Thread_Context_For_User_Delegate Update_Thread_Context_For_User = null;

		/// <summary>
		/// Private codec-specific defaults
		/// </summary>
		public readonly IEnumerable<FFCodecDefault> Defaults = null;

		/// <summary>
		/// 
		/// </summary>
		public CodecFunc.Init_Delegate Init;

		/// <summary>
		/// 
		/// </summary>
		public 
		(
			// Decode to an AVFrame.
			// cb is in this state if cb_type is FF_CODEC_CB_TYPE_DECODE
			CodecFunc.Decode_Delegate Decode,

			// Decode subtitle data to an AVSubtitle.
			// cb is in this state if cb_type is FF_CODEC_CB_TYPE_DECODE_SUB.
			// 
			// Apart from that this is like the decode callback
			CodecFunc.Decode_Sub_Delegate Decode_Sub,

			// Decode API with decoupled packet/frame dataflow.
			// cb is in this state if cb_type is FF_CODEC_CB_TYPE_RECEIVE_FRAME.
			// 
			// This function is called to get one output frame. It should call
			// ff_decode_get_packet() to obtain input data
			CodecFunc.Receive_Frame_Delegate Receive_Frame,

			// Encode data to an AVPacket.
			// cb is in this state if cb_type is FF_CODEC_CB_TYPE_ENCODE
			CodecFunc.Encode_Delegate Encode,

			// Encode subtitles to a raw buffer.
			// cb is in this state if cb_type is FF_CODEC_CB_TYPE_ENCODE_SUB
			CodecFunc.Encode_Sub_Delegate Encode_Sub,

			// Encode API with decoupled frame/packet dataflow.
			// cb is in this state if cb_type is FF_CODEC_CB_TYPE_RECEIVE_PACKET.
			// 
			// This function is called to get one output packet.
			// It should call ff_encode_get_frame() to obtain input data
			CodecFunc.Receive_Packet_Delegate Receive_Packet
		) Cb;

		/// <summary>
		/// 
		/// </summary>
		public CodecFunc.Close_Delegate Close;

		/// <summary>
		/// Flush buffers.
		/// Will be called when seeking
		/// </summary>
		public CodecFunc.Flush_Delegate Flush;

		/// <summary>
		/// Decoding only, a comma-separated list of bitstream filters to apply to
		/// packets before decoding
		/// </summary>
		public readonly CPointer<char> Bsfs = null;

		/// <summary>
		/// Array of pointers to hardware configurations supported by the codec,
		/// or NULL if no hardware supported.  The array is terminated by a NULL
		/// pointer.
		///
		/// The user can only access this field via avcodec_get_hw_config()
		/// </summary>
		public CPointer<AvCodecHwConfigInternal> Hw_Configs;

		/// <summary>
		/// List of supported codec_tags, terminated by FF_CODEC_TAGS_END
		/// </summary>
		public readonly CPointer<uint32_t> Codec_Tags = null;

		/// <summary>
		/// Custom callback for avcodec_get_supported_config(). If absent,
		/// ff_default_get_supported_config() will be used. `out_num_configs` will
		/// always be set to a valid pointer
		/// </summary>
		public readonly CodecFunc.Get_Supported_Config_Delegate Get_Supported_Config = null;
	}
}
