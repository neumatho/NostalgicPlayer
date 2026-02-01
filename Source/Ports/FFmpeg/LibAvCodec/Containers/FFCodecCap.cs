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
	internal enum FFCodecCap
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// The codec is not known to be init-threadsafe (i.e. it might be unsafe
		/// to initialize this codec and another codec concurrently, typically because
		/// the codec calls external APIs that are not known to be thread-safe).
		/// Therefore calling the codec's init function needs to be guarded with a lock
		/// </summary>
		Not_Init_Threadsafe = 1 << 0,

		/// <summary>
		/// The codec allows calling the close function for deallocation even if
		/// the init function returned a failure. Without this capability flag, a
		/// codec does such cleanup internally when returning failures from the
		/// init function and does not expect the close function to be called at
		/// all
		/// </summary>
		Init_Cleanup = 1 << 1,

		/// <summary>
		/// Decoders marked with FF_CODEC_CAP_SETS_PKT_DTS want to set
		/// AVFrame.pkt_dts manually. If the flag is set, decode.c won't overwrite
		/// this field. If it's unset, decode.c tries to guess the pkt_dts field
		/// from the input AVPacket
		/// </summary>
		Sets_Pkt_Dts = 1 << 2,

		/// <summary>
		/// The decoder extracts and fills its parameters even if the frame is
		/// skipped due to the skip_frame setting
		/// </summary>
		Skip_Frame_Fill_Param = 1 << 3,

		/// <summary>
		/// The decoder sets the cropping fields in the output frames manually.
		/// If this cap is set, the generic code will initialize output frame
		/// dimensions to coded rather than display values
		/// </summary>
		Exports_Cropping = 1 << 4,

		/// <summary>
		/// Codec initializes slice-based threading with a main function
		/// </summary>
		Slice_Thread_Has_Mf = 1 << 5,

		/// <summary>
		/// The decoder might make use of the ProgressFrame API
		/// </summary>
		Uses_ProgressFrames = 1 << 6,

		/// <summary>
		/// Codec handles avctx->thread_count == 0 (auto) internally
		/// </summary>
		Auto_Threads = 1 << 7,

		/// <summary>
		/// Codec handles output frame properties internally instead of letting the
		/// internal logic derive them from AVCodecInternal.last_pkt_props
		/// </summary>
		Sets_Frame_Props = 1 << 8,

		/// <summary>
		/// Codec supports embedded ICC profiles (AV_FRAME_DATA_ICC_PROFILE)
		/// </summary>
		Icc_Profiles = 1 << 9,

		/// <summary>
		/// The encoder has AV_CODEC_CAP_DELAY set, but does not actually have delay - it
		/// only wants to be flushed at the end to update some context variables (e.g.
		/// 2pass stats) or produce a trailing packet. Besides that it immediately
		/// produces exactly one output packet per each input frame, just as no-delay
		/// encoders do
		/// </summary>
		Eof_Flush = 1 << 10
	}
}
