/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	public enum AvCodecCap
	{
		/// <summary>
		/// Decoder can use draw_horiz_band callback
		/// </summary>
		Draw_Horiz_Band = 1 << 0,

		/// <summary>
		/// Codec uses get_buffer() or get_encode_buffer() for allocating buffers and supports custom allocators.
		/// If not set, it might not use those at all, or use operations that assume the buffer was allocated by
		/// avcodec_default_get_buffer2 or avcodec_default_get_encode_buffer.
		/// </summary>
		Dr1 = 1 << 1,

		/// <summary>
		/// Encoder or decoder requires flushing with NULL input at the end in order to give the complete and correct output.
		/// Decoders: feed with empty packet at end to obtain delayed frames. Encoders: feed with NULL until no more data.
		/// </summary>
		Delay = 1 << 5,

		/// <summary>
		/// Codec can be fed a final frame with a smaller size to avoid truncation of the last audio samples.
		/// </summary>
		Small_Last_Frame = 1 << 6,

		/// <summary>
		/// Codec is experimental and avoided in favor of non-experimental encoders.
		/// </summary>
		Experimental = 1 << 9,

		/// <summary>
		/// Codec should fill in channel configuration and sample rate instead of container.
		/// </summary>
		Channel_Conf = 1 << 10,

		/// <summary>
		/// Codec supports frame-level multithreading.
		/// </summary>
		Frame_Threads = 1 << 12,

		/// <summary>
		/// Codec supports slice/partition-based multithreading.
		/// </summary>
		Slice_Threads = 1 << 13,

		/// <summary>
		/// Codec supports changed parameters at any point.
		/// </summary>
		Param_Change = 1 << 14,

		/// <summary>
		/// Codec supports multithreading via other methods (e.g. underlying lib) than slice/frame threads.
		/// </summary>
		Other_Threads = 1 << 15,

		/// <summary>
		/// Audio encoder supports receiving a different number of samples in each call.
		/// </summary>
		Variable_Frame_Size = 1 << 16,

		/// <summary>
		/// Decoder is not a preferred choice for probing (expensive or low information). Use only as last resort.
		/// </summary>
		Avoid_Probing = 1 << 17,

		/// <summary>
		/// Codec is backed by a hardware implementation (non-hwaccel). For hwaccels use avcodec_get_hw_config().
		/// </summary>
		Hardware = 1 << 18,

		/// <summary>
		/// Codec may be backed by hardware but can fallback to software.
		/// </summary>
		Hybrid = 1 << 19,

		/// <summary>
		/// Encoder can reorder and return user opaque values (see AV_CODEC_FLAG_COPY_OPAQUE).
		/// </summary>
		Encoder_Reordered_Opaque = 1 << 20,

		/// <summary>
		/// Encoder can be flushed using avcodec_flush_buffers(); otherwise must be closed and reopened.
		/// </summary>
		Encoder_Flush = 1 << 21,

		/// <summary>
		/// Encoder can output reconstructed frame data (raw frames from decoding its own bitstream).
		/// Enabled by AV_CODEC_FLAG_RECON_FRAME.
		/// </summary>
		Encoder_Recon_Frame = 1 << 22
	}
}
