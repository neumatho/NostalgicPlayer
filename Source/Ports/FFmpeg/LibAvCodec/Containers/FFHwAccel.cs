/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class FFHwAccel : AvHwAccel
	{
		/// <summary>
		/// The public AVHWAccel. See avcodec.h for it
		/// </summary>
		public AvHwAccel P => this;

		/// <summary>
		/// Allocate a custom buffer
		/// </summary>
		public readonly CodecFunc.Alloc_Frame_Delegate Alloc_Frame = null;

		// <summary>
		// Called at the beginning of each frame or field picture.
		//
		// Meaningful frame information (codec specific) is guaranteed to
		// be parsed at this point. This function is mandatory.
		//
		// Note that buf can be NULL along with buf_size set to 0.
		// Otherwise, this means the whole frame is available at this point
		// </summary>
//		public CodecFunc.Start_Frame_Delegate Start_Frame;

		// <summary>
		// Callback for parameter data (SPS/PPS/VPS etc).
		//
		// Useful for hardware decoders which keep persistent state about the
		// video parameters, and need to receive any changes to update that state
		// </summary>
//		public CodecFunc.Decode_Params_Delegate Decode_Params;

		// <summary>
		// Callback for each slice.
		//
		// Meaningful slice information (codec specific) is guaranteed to
		// be parsed at this point. This function is mandatory
		// </summary>
//		public CodecFunc.Decode_Slice_Delegate Decode_Slice;

		// <summary>
		// Called at the end of each frame or field picture.
		//
		// The whole picture is parsed at this point and can now be sent
		// to the hardware accelerator. This function is mandatory
		// </summary>
//		public CodecFunc.End_Frame_Delegate End_Frame;

		// <summary>
		// Size of per-frame hardware accelerator private data.
		//
		// Private data is allocated with av_mallocz() before
		// AVCodecContext.get_buffer() and deallocated after
		// AVCodecContext.release_buffer()
		// </summary>
//		public c_int Frame_Priv_Data_Size;

		/// <summary>
		/// Size of the private data to allocate in
		/// AVCodecInternal.hwaccel_priv_data
		/// </summary>
//		public c_int Priv_Data_Size;
		public readonly CodecFunc.Private_Data_Alloc_Delegate Priv_Data_Alloc = null;

		/// <summary>
		/// Internal hwaccel capabilities
		/// </summary>
		public readonly HwAccelCap Caps_Internal = HwAccelCap.None;

		// <summary>
		// Initialize the hwaccel private data.
		//
		// This will be called from ff_get_format(), after hwaccel and
		// hwaccel_context are set and the hwaccel private data in AVCodecInternal
		// is allocated
		// </summary>
//		public CodecFunc.Init_Delegate Init;

		/// <summary>
		/// Uninitialize the hwaccel private data.
		///
		/// This will be called from get_format() or ff_codec_close(), after hwaccel
		/// and hwaccel_context are already uninitialized
		/// </summary>
		public readonly CodecFunc.Uninit_Delegate Uninit = null;

		// <summary>
		// Fill the given hw_frames context with current codec parameters. Called
		// from get_format. Refer to avcodec_get_hw_frames_parameters() for
		// details.
		//
		// This CAN be called before AVHWAccel.init is called, and you must assume
		// that avctx->hwaccel_priv_data is invalid
		// </summary>
//		public CodecFunc.Frame_Params_Delegate Frame_Params;

		/// <summary>
		/// Copy necessary context variables from a previous thread context to the current one.
		/// For thread-safe hwaccels only
		/// </summary>
		public readonly CodecFunc.Update_Thread_Context_Delegate Update_Thread_Context = null;

		// <summary>
		// Callback to free the hwaccel-specific frame data
		// </summary>
//		public CodecFunc.Free_Frame_Priv_Delegate Free_Frame_Priv;

		// <summary>
		// Callback to flush the hwaccel state
		// </summary>
//		public CodecFunc.Flush_Delegate Flush;
	}
}
