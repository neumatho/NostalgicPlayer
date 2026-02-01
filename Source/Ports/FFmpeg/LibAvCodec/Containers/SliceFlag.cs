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
	public enum SliceFlag
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// draw_horiz_band() is called in coded order instead of display
		/// </summary>
		Coded_Order = 0x0001,

		/// <summary>
		/// Allow draw_horiz_band() with field slices (MPEG-2 field pics)
		/// </summary>
		Allow_Field = 0x0002,

		/// <summary>
		/// Allow draw_horiz_band() with 1 component at a time (SVQ1)
		/// </summary>
		Allow_Plane = 0x0004
	}
}
