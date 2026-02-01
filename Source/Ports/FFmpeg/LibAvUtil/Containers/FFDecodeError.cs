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
	public enum FFDecodeError
	{
		/// <summary>
		/// 
		/// </summary>
		None,

		/// <summary>
		/// 
		/// </summary>
		Invalid_Bitstream = 1,

		/// <summary>
		/// 
		/// </summary>
		Missing_Reference = 2,

		/// <summary>
		/// 
		/// </summary>
		Consealment_Active = 4,

		/// <summary>
		/// 
		/// </summary>
		Decode_Slices = 8
	}
}
