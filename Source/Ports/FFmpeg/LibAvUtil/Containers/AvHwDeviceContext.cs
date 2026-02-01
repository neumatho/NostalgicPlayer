/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// This struct aggregates all the (hardware/vendor-specific) "high-level" state,
	/// i.e. state that is not tied to a concrete processing configuration.
	/// E.g., in an API that supports hardware-accelerated encoding and decoding,
	/// this struct will (if possible) wrap the state that is common to both encoding
	/// and decoding and from which specific instances of encoders or decoders can be
	/// derived.
	///
	/// This struct is reference-counted with the AVBuffer mechanism. The
	/// av_hwdevice_ctx_alloc() constructor yields a reference, whose data field
	/// points to the actual AVHWDeviceContext. Further objects derived from
	/// AVHWDeviceContext (such as AVHWFramesContext, describing a frame pool with
	/// specific properties) will hold an internal reference to it. After all the
	/// references are released, the AVHWDeviceContext itself will be freed,
	/// optionally invoking a user-specified callback for uninitializing the hardware
	/// state
	/// </summary>
	public class AvHwDeviceContext : AvClass, IDataContext
	{
		/// <summary>
		/// A class for logging. Set by av_hwdevice_ctx_alloc()
		/// </summary>
		public AvClass Av_Class => this;

		/// <summary>
		/// This field identifies the underlying API used for hardware access.
		///
		/// This field is set when this struct is allocated and never changed
		/// afterwards
		/// </summary>
		public AvHwDeviceType Type;

		/// <summary>
		/// The format-specific data, allocated and freed by libavutil along with
		/// this context.
		///
		/// Should be cast by the user to the format-specific context defined in the
		/// corresponding header (hwcontext_*.h) and filled as described in the
		/// documentation before calling av_hwdevice_ctx_init().
		///
		/// After calling av_hwdevice_ctx_init() this struct should not be modified
		/// by the caller
		/// </summary>
		public IContext HwCtx;

		/// <summary>
		/// This field may be set by the caller before calling av_hwdevice_ctx_init().
		///
		/// If non-NULL, this callback will be called when the last reference to
		/// this context is unreferenced, immediately before it is freed.
		///
		/// Note: when other objects (e.g an AVHWFramesContext) are derived from this
		///       struct, this callback will be invoked after all such child objects
		///       are fully uninitialized and their respective destructors invoked
		/// </summary>
		public UtilFunc.HwFreeDevice_Delegate Free;

		/// <summary>
		/// Arbitrary user data, to be used e.g. by the free() callback
		/// </summary>
		public IOpaque User_Opaque;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public IDataContext MakeDeepClone()
		{
			return (AvHwDeviceContext)MemberwiseClone();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/

		public void CopyTo(IDataContext destination)
		{
			base.CopyTo((AvClass)destination);

			AvHwDeviceContext dst = (AvHwDeviceContext)destination;

			dst.Type = Type;
			dst.HwCtx = HwCtx;
			dst.Free = Free;
			dst.User_Opaque = User_Opaque;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public UtilFunc.Alloc_DataContext_Delegate Allocator => CreateInstance;

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Allocate a new instance of this type
		/// </summary>
		/********************************************************************/
		private static IDataContext CreateInstance()
		{
			return new AvHwDeviceContext();
		}
		#endregion
	}
}
