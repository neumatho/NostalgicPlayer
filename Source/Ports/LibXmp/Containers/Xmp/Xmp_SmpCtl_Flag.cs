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
	public enum Xmp_SmpCtl_Flag
	{
		/// <summary>
		/// Don't load sample
		/// </summary>
		Skip = (1 << 0)
	}
}
