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
	public enum FFBug
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// Autodetection
		/// </summary>
		AutoDetect = 1,

		/// <summary>
		/// 
		/// </summary>
		Xvid_Ilace = 4,

		/// <summary>
		/// 
		/// </summary>
		Ump4 = 8,

		/// <summary>
		/// 
		/// </summary>
		No_Padding = 16,

		/// <summary>
		/// 
		/// </summary>
		Amv = 32,

		/// <summary>
		/// 
		/// </summary>
		Qpel_Chroma = 64,

		/// <summary>
		/// 
		/// </summary>
		Std_Qpel = 128,

		/// <summary>
		/// 
		/// </summary>
		Qpel_Chroma2 = 256,

		/// <summary>
		/// 
		/// </summary>
		Direct_BlockSize = 512,

		/// <summary>
		/// 
		/// </summary>
		Edge = 1024,

		/// <summary>
		/// 
		/// </summary>
		Hpel_Chroma = 2048,

		/// <summary>
		/// 
		/// </summary>
		Dc_Clip = 4096,

		/// <summary>
		/// Work around various bugs in Microsoft's broken decoders
		/// </summary>
		Ms = 8192,

		/// <summary>
		/// 
		/// </summary>
		Truncated = 16384,

		/// <summary>
		/// 
		/// </summary>
		IEdge = 32768
	}
}
