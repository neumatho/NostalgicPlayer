/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class AvCodecInternal
	{
		/// <summary>
		/// When using frame-threaded decoding, this field is set for the first
		/// worker thread (e.g. to decode extradata just once)
		/// </summary>
		public c_int Is_Copy;

		/// <summary>
		/// This field is set to 1 when frame threading is being used and the parent
		/// AVCodecContext of this AVCodecInternal is a worker-thread context (i.e.
		/// one of those actually doing the decoding), 0 otherwise
		/// </summary>
		public c_int Is_Frame_Mt;

		// <summary>
		// Audio encoders can set this flag during init to indicate that they
		// want the small last frame to be padded to a multiple of pad_samples
		// </summary>
//		public c_int Pad_Samples;

		/// <summary>
		/// 
		/// </summary>
		public FramePool Pool;

		/// <summary>
		/// 
		/// </summary>
		public AvRefStructPool Progress_Frame_Pool;

		/// <summary>
		/// 
		/// </summary>
		public IContext Thread_Ctx;

		/// <summary>
		/// This packet is used to hold the packet given to decoders
		/// implementing the .decode API; it is unused by the generic
		/// code for decoders implementing the .receive_frame API and
		/// may be freely used (but not freed) by them with the caveat
		/// that the packet will be unreferenced generically in
		/// avcodec_flush_buffers()
		/// </summary>
		public AvPacket In_Pkt;

		/// <summary>
		/// 
		/// </summary>
		public AvBsfContext Bsf;

		/// <summary>
		/// Properties (timestamps+side data) extracted from the last packet passed
		/// for decoding
		/// </summary>
		public AvPacket Last_Pkt_Props;

		/// <summary>
		/// Temporary buffer used for encoders to store their bitstream
		/// </summary>
		public CPointer<uint8_t> Byte_Buffer;

		/// <summary>
		/// 
		/// </summary>
		public c_uint Byte_Buffer_Size;

		/// <summary>
		/// 
		/// </summary>
		public readonly object Frame_Thread_Encoder = null;

		/// <summary>
		/// The input frame is stored here for encoders implementing the simple
		/// encode API.
		///
		/// Not allocated in other cases
		/// </summary>
		public AvFrame In_Frame;

		/// <summary>
		/// When the AV_CODEC_FLAG_RECON_FRAME flag is used. the encoder should store
		/// here the reconstructed frame corresponding to the last returned packet.
		///
		/// Not allocated in other cases
		/// </summary>
		public AvFrame Recon_Frame;

		/// <summary>
		/// If this is set, then FFCodec->close (if existing) needs to be called
		/// for the parent AVCodecContext
		/// </summary>
		public c_int Needs_Close;

		/// <summary>
		/// Number of audio samples to skip at the start of the next decoded frame
		/// </summary>
		public c_int Skip_Samples;

		/// <summary>
		/// hwaccel-specific private data
		/// </summary>
		public IPrivateData HwAccel_Priv_Data;

		/// <summary>
		/// decoding: AVERROR_EOF has been returned from ff_decode_get_packet(); must
		///           not be used by decoders that use the decode() callback, as they
		///           do not call ff_decode_get_packet() directly
		///
		/// encoding: a flush frame has been submitted to avcodec_send_frame()
		/// </summary>
		public c_int Draining;

		/// <summary>
		/// Temporary buffers for newly received or not yet output packets/frames
		/// </summary>
		public AvPacket Buffer_Pkt;

		/// <summary>
		/// 
		/// </summary>
		public AvFrame Buffer_Frame;

		/// <summary>
		/// 
		/// </summary>
		public c_int Draining_Done;

		/// <summary>
		/// Set when the user has been warned about a failed allocation from
		/// a fixed frame pool
		/// </summary>
		public c_int Warned_On_Failed_Allocation_From_Fixed_Pool;
	}
}
