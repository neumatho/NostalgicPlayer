/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	public enum AvHwDeviceType
	{
		/// <summary>
		/// 
		/// </summary>
		None,

		/// <summary>
		/// 
		/// </summary>
		Vdpau,

		/// <summary>
		/// 
		/// </summary>
		Cuda,

		/// <summary>
		/// 
		/// </summary>
		VaApi,

		/// <summary>
		/// 
		/// </summary>
		Dxva2,

		/// <summary>
		/// 
		/// </summary>
		Qsv,

		/// <summary>
		/// 
		/// </summary>
		VideoToolbox,

		/// <summary>
		/// 
		/// </summary>
		D3D11Va,

		/// <summary>
		/// 
		/// </summary>
		Drm,

		/// <summary>
		/// 
		/// </summary>
		OpenCl,

		/// <summary>
		/// 
		/// </summary>
		MediaCodec,

		/// <summary>
		/// 
		/// </summary>
		Vulkan,

		/// <summary>
		/// 
		/// </summary>
		D3D12Va,

		/// <summary>
		/// 
		/// </summary>
		Amf,

		/// <summary>
		/// OpenHarmony Codec device
		/// </summary>
		OhCodec
	}
}
