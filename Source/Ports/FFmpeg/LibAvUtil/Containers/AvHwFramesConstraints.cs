/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// This struct describes the constraints on hardware frames attached to
	/// a given device with a hardware-specific configuration. This is returned
	/// by av_hwdevice_get_hwframe_constraints() and must be freed by
	/// av_hwframe_constraints_free() after use
	/// </summary>
	public class AvHwFramesConstraints
	{
		/// <summary>
		/// A list of possible values for format in the hw_frames_ctx,
		/// terminated by AV_PIX_FMT_NONE. This member will always be filled
		/// </summary>
		public CPointer<AvPixelFormat> Valid_Hw_Formats;

		/// <summary>
		/// A list of possible values for sw_format in the hw_frames_ctx,
		/// terminated by AV_PIX_FMT_NONE. Can be NULL if this information is
		/// not known
		/// </summary>
		public CPointer<AvPixelFormat> Valid_Sw_Formats;

		/// <summary>
		/// The minimum size of frames in this hw_frames_ctx.
		/// (Zero if not known)
		/// </summary>
		public c_int Min_Width;

		/// <summary>
		/// 
		/// </summary>
		public c_int Min_Height;

		/// <summary>
		/// The maximum size of frames in this hw_frames_ctx.
		/// (INT_MAX if not known / no limit)
		/// </summary>
		public c_int Max_Width;

		/// <summary>
		/// 
		/// </summary>
		public c_int Max_Height;
	}
}
