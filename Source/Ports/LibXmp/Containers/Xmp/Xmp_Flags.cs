/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp
{
	/// <summary>
	/// Player flags
	/// </summary>
	[Flags]
	public enum Xmp_Flags
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// Use vblank timing
		/// </summary>
		VBlank = (1 << 0),

		/// <summary>
		/// Emulate FX9 bug
		/// </summary>
		Fx9Bug = (1 << 1),

		/// <summary>
		/// Emulate sample loop bug
		/// </summary>
		FixLoop = (1 << 2),

		/// <summary>
		/// Use Paula mixer in Amiga modules
		/// </summary>
		A500 = (1 << 3)
	}
}
