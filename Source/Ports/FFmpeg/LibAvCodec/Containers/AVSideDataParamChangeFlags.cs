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
	public enum AVSideDataParamChangeFlags
	{
		/// <summary>
		/// 
		/// </summary>
		Sample_Rate = 0x0004,

		/// <summary>
		/// 
		/// </summary>
		Dimensions = 0x0008
	}
}
