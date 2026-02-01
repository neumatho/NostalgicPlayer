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
	public enum FFDebug
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// 
		/// </summary>
		Pict_Info = 1,

		/// <summary>
		/// 
		/// </summary>
		Rc = 2,

		/// <summary>
		/// 
		/// </summary>
		Bitstream = 4,

		/// <summary>
		/// 
		/// </summary>
		Mb_Type = 8,

		/// <summary>
		/// 
		/// </summary>
		Qp = 16,

		/// <summary>
		/// 
		/// </summary>
		Dct_Coeff = 0x00000040,

		/// <summary>
		/// 
		/// </summary>
		Skip = 0x00000080,

		/// <summary>
		/// 
		/// </summary>
		StartCode = 0x00000100,

		/// <summary>
		/// 
		/// </summary>
		Er = 0x00000400,

		/// <summary>
		/// 
		/// </summary>
		Mmco = 0x00000800,

		/// <summary>
		/// 
		/// </summary>
		Bugs = 0x00001000,

		/// <summary>
		/// 
		/// </summary>
		Buffers = 0x00008000,

		/// <summary>
		/// 
		/// </summary>
		Threads = 0x00010000,

		/// <summary>
		/// 
		/// </summary>
		Green_Md = 0x00800000,

		/// <summary>
		/// 
		/// </summary>
		Debug_NoMc = 0x01000000,
	}
}
