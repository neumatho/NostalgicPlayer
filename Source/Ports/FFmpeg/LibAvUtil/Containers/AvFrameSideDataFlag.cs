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
	public enum AvFrameSideDataFlag
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// Remove existing entries before adding new ones
		/// </summary>
		Unique = 1 << 0,

		/// <summary>
		/// Don't add a new entry if another of the same type exists.
		/// Applies only for side data types without the AV_SIDE_DATA_PROP_MULTI prop
		/// </summary>
		Replace = 1 << 1,

		/// <summary>
		/// Create a new reference to the passed in buffer instead of taking ownership
		/// of it
		/// </summary>
		New_Ref = 1 << 2
	}
}
