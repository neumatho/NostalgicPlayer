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
	public enum AvCodecHwConfigMethod
	{
		/// <summary>
		/// The codec supports this format via the hw_device_ctx interface.
		///
		/// When selecting this format, AVCodecContext.hw_device_ctx should
		/// have been set to a device of the specified type before calling
		/// avcodec_open2()
		/// </summary>
		Hw_Device_Ctx = 0x01,

		/// <summary>
		/// The codec supports this format via the hw_frames_ctx interface.
		///
		/// When selecting this format for a decoder,
		/// AVCodecContext.hw_frames_ctx should be set to a suitable frames
		/// context inside the get_format() callback.  The frames context
		/// must have been created on a device of the specified type.
		///
		/// When selecting this format for an encoder,
		/// AVCodecContext.hw_frames_ctx should be set to the context which
		/// will be used for the input frames before calling avcodec_open2()
		/// </summary>
		Hw_Frame_Ctx = 0x02,

		/// <summary>
		/// The codec supports this format by some internal method.
		///
		/// This format can be selected without any additional configuration -
		/// no device or frames context is required
		/// </summary>
		Internal = 0x04,

		/// <summary>
		/// The codec supports this format by some ad-hoc method.
		///
		/// Additional settings and/or function calls are required.  See the
		/// codec-specific documentation for details. (Methods requiring
		/// this sort of configuration are deprecated and others should be
		/// used in preference)
		/// </summary>
		Ad_Hoc = 0x08
	}
}
