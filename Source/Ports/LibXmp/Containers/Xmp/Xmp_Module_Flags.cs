/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	public enum Xmp_Module_Flags
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// Uses tracks instead of patterns
		/// </summary>
		Uses_Tracks = 1 << 0
	}
}
