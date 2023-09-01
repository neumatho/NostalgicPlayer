/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp
{
	/// <summary>
	/// Sample format flags
	/// </summary>
	[Flags]
	public enum Xmp_Format
	{
		/// <summary>
		/// Default setup
		/// </summary>
		Default = 0,

		/// <summary>
		/// Mix to 8-bit instead of 16
		/// </summary>
		_8Bit = (1 << 0),

		/// <summary>
		/// Mix to unsigned samples
		/// </summary>
		Unsigned = (1 << 1),

		/// <summary>
		/// Mix to mono instead of stereo
		/// </summary>
		Mono = (1 << 2)
	}
}
