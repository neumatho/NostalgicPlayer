/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class HwContextType
	{
		/// <summary>
		/// 
		/// </summary>
		public AvHwDeviceType Type;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<char> Name;

		/// <summary>
		/// An array of pixel formats supported by the AVHWFramesContext instances
		/// Terminated by AV_PIX_FMT_NONE
		/// </summary>
		public CPointer<AvPixelFormat> Pix_Fmts;

		/// <summary>
		/// size of the public hardware-specific context,
		/// i.e. AVHWDeviceContext.hwctx
		/// </summary>
		public size_t Device_HwCtx_Size;

		/// <summary>
		/// Size of the hardware-specific device configuration.
		/// (Used to query hwframe constraints)
		/// </summary>
		public size_t Device_HwConfig_Size;

		/// <summary>
		/// size of the public frame pool hardware-specific context,
		/// i.e. AVHWFramesContext.hwctx
		/// </summary>
		public size_t Frames_HwCtx_Size;

		/// <summary>
		/// 
		/// </summary>
		public UtilFunc.DeviceCreate_Delegate Device_Create;

		/// <summary>
		/// 
		/// </summary>
		public UtilFunc.DeviceDerive_Delegate Device_Derive;

		/// <summary>
		/// 
		/// </summary>
		public UtilFunc.DeviceInit_Delegate Device_Init;

		/// <summary>
		/// 
		/// </summary>
		public UtilFunc.DeviceUninit_Delegate Device_Uninit;

		/// <summary>
		/// 
		/// </summary>
		public UtilFunc.FramesGetConstraints_Delegate Frames_Get_Constraints;

		/// <summary>
		/// 
		/// </summary>
		public UtilFunc.FramesInit_Delegate Frames_Init;

		/// <summary>
		/// 
		/// </summary>
		public UtilFunc.FramesUninit_Delegate Frames_Uninit;

		/// <summary>
		/// 
		/// </summary>
		public UtilFunc.FramesGetBuffer_Delegate Frames_Get_Buffer;

		/// <summary>
		/// 
		/// </summary>
		public UtilFunc.TransferGetFormats_Delegate Transfer_Get_Formats;

		/// <summary>
		/// 
		/// </summary>
		public UtilFunc.TransferDataTo_Delegate Transfer_Data_To;

		/// <summary>
		/// 
		/// </summary>
		public UtilFunc.TransferDataFrom_Delegate Transfer_Data_From;

		/// <summary>
		/// 
		/// </summary>
		public UtilFunc.MapTo_Delegate Map_To;

		/// <summary>
		/// 
		/// </summary>
		public UtilFunc.MapFrom_Delegate Map_From;

		/// <summary>
		/// 
		/// </summary>
		public UtilFunc.FramesDeriveTo_Delegate Frames_Derive_To;

		/// <summary>
		/// 
		/// </summary>
		public UtilFunc.FramesDeriveFrom_Delegate Frames_Derive_From;
	}
}
