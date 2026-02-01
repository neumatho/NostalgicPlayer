/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// This struct describes a set or pool of "hardware" frames (i.e. those with
	/// data not located in normal system memory). All the frames in the pool are
	/// assumed to be allocated in the same way and interchangeable.
	///
	/// This struct is reference-counted with the AVBuffer mechanism and tied to a
	/// given AVHWDeviceContext instance. The av_hwframe_ctx_alloc() constructor
	/// yields a reference, whose data field points to the actual AVHWFramesContext
	/// struct
	/// </summary>
	public class AvHwFramesContext : AvClass, ICopyTo<AvHwFramesContext>
	{
		/// <summary>
		/// A class for logging
		/// </summary>
		public AvClass Av_Class => this;

		/// <summary>
		/// A reference to the parent AVHWDeviceContext. This reference is owned and
		/// managed by the enclosing AVHWFramesContext, but the caller may derive
		/// additional references from it
		/// </summary>
		public AvBufferRef Device_Ref;

		/// <summary>
		/// The parent AVHWDeviceContext. This is simply a pointer to
		/// device_ref->data provided for convenience.
		///
		/// Set by libavutil in av_hwframe_ctx_init()
		/// </summary>
		public AvHwDeviceContext Device_Ctx;

		/// <summary>
		/// The format-specific data, allocated and freed automatically along with
		/// this context.
		///
		/// The user shall ignore this field if the corresponding format-specific
		/// header (hwcontext_*.h) does not define a context to be used as
		/// AVHWFramesContext.hwctx.
		///
		/// Otherwise, it should be cast by the user to said context and filled
		/// as described in the documentation before calling av_hwframe_ctx_init().
		///
		/// After any frames using this context are created, the contents of this
		/// struct should not be modified by the caller
		/// </summary>
		public IHardwareContext HwCtx;

		/// <summary>
		/// This field may be set by the caller before calling av_hwframe_ctx_init().
		///
		/// If non-NULL, this callback will be called when the last reference to
		/// this context is unreferenced, immediately before it is freed
		/// </summary>
		public UtilFunc.HwFreeFrames_Delegate Free;

		/// <summary>
		/// Arbitrary user data, to be used e.g. by the free() callback.
		/// </summary>
		public IOpaque User_Opaque;

		/// <summary>
		/// A pool from which the frames are allocated by av_hwframe_get_buffer().
		/// This field may be set by the caller before calling av_hwframe_ctx_init().
		/// The buffers returned by calling av_buffer_pool_get() on this pool must
		/// have the properties described in the documentation in the corresponding hw
		/// type's header (hwcontext_*.h). The pool will be freed strictly before
		/// this struct's free() callback is invoked.
		///
		/// This field may be NULL, then libavutil will attempt to allocate a pool
		/// internally. Note that certain device types enforce pools allocated at
		/// fixed size (frame count), which cannot be extended dynamically. In such a
		/// case, initial_pool_size must be set appropriately
		/// </summary>
		public AvBufferPool Pool;

		/// <summary>
		/// Initial size of the frame pool. If a device type does not support
		/// dynamically resizing the pool, then this is also the maximum pool size.
		///
		/// May be set by the caller before calling av_hwframe_ctx_init(). Must be
		/// set if pool is NULL and the device type does not support dynamic pools
		/// </summary>
		public c_int Initial_Pool_Size;

		/// <summary>
		/// The pixel format identifying the underlying HW surface type.
		///
		/// Must be a hwaccel format, i.e. the corresponding descriptor must have the
		/// AV_PIX_FMT_FLAG_HWACCEL flag set.
		///
		/// Must be set by the user before calling av_hwframe_ctx_init()
		/// </summary>
		public AvPixelFormat Format;

		/// <summary>
		/// The pixel format identifying the actual data layout of the hardware
		/// frames.
		///
		/// Must be set by the caller before calling av_hwframe_ctx_init().
		///
		/// Note when the underlying API does not provide the exact data layout, but
		/// only the colorspace/bit depth, this field should be set to the fully
		/// planar version of that format (e.g. for 8-bit 420 YUV it should be
		/// AV_PIX_FMT_YUV420P, not AV_PIX_FMT_NV12 or anything else)
		/// </summary>
		public AvPixelFormat Sw_Format;

		/// <summary>
		/// The allocated dimensions of the frames in this pool.
		///
		/// Must be set by the user before calling av_hwframe_ctx_init()
		/// </summary>
		public c_int Width;

		/// <summary>
		/// 
		/// </summary>
		public c_int Height;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void CopyTo(AvHwFramesContext destination)
		{
			base.CopyTo(destination);

			destination.Device_Ref = Device_Ref;
			destination.Device_Ctx = Device_Ctx;
			destination.HwCtx = HwCtx;
			destination.Free = Free;
			destination.User_Opaque = User_Opaque;
			destination.Pool = Pool;
			destination.Initial_Pool_Size = Initial_Pool_Size;
			destination.Format = Format;
			destination.Sw_Format = Sw_Format;
			destination.Width = Width;
			destination.Height = Height;
		}
	}
}
