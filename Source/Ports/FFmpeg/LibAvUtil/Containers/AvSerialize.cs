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
	public enum AvSerialize
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// Serialize options that are not set to default values only
		/// </summary>
		Skip_Defaults = 0x00000001,

		/// <summary>
		/// Serialize options that exactly match opt_flags only
		/// </summary>
		Opt_Flags_Exact = 0x00000002,

		/// <summary>
		/// Serialize options in possible children of the given object
		/// </summary>
		Search_Children = 0x00000004
	}
}
