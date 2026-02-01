/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// Rounding methods
	/// </summary>
	[Flags]
	public enum AvRounding
	{
		/// <summary>
		/// Round toward zero
		/// </summary>
		Zero = 0,

		/// <summary>
		/// Round away from zero
		/// </summary>
		Inf = 1,

		/// <summary>
		/// Round toward -infinity
		/// </summary>
		Down = 2,

		/// <summary>
		/// Round toward +infinity
		/// </summary>
		Up = 3,

		/// <summary>
		/// Round to nearest and halfway cases away from zero
		/// </summary>
		Near_Inf = 5,

		/// <summary>
		/// Flag telling rescaling functions to pass `INT64_MIN`/`MAX` through
		/// unchanged, avoiding special cases for #AV_NOPTS_VALUE.
		///
		/// Unlike other values of the enumeration AVRounding, this value is a
		/// bitmask that must be used in conjunction with another value of the
		/// enumeration through a bitwise OR, in order to set behavior for normal
		/// cases
		/// </summary>
		Pass_MinMax = 8192
	}
}
