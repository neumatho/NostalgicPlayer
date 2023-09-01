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
	public enum Xmp_Envelope_Flag
	{
		/// <summary>
		/// 
		/// </summary>
		None,

		/// <summary>
		/// Envelope is enabled
		/// </summary>
		On = (1 << 0),

		/// <summary>
		/// Envelope has sustain point
		/// </summary>
		Sus = (1 << 1),

		/// <summary>
		/// Envelope has loop
		/// </summary>
		Loop = (1 << 2),

		/// <summary>
		/// Envelope is used for filter
		/// </summary>
		Flt = (1 << 3),

		/// <summary>
		/// Envelope has sustain loop
		/// </summary>
		SLoop = (1 << 4),

		/// <summary>
		/// Don't reset envelope position
		/// </summary>
		Carry = (1 << 5)
	}
}
