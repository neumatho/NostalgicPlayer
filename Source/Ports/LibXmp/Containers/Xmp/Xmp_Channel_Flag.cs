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
	public enum Xmp_Channel_Flag
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// Channel is synthesized
		/// </summary>
		Synth = (1 << 0),

		/// <summary>
		/// Channel is muted
		/// </summary>
		Mute = (1 << 1),

		/// <summary>
		/// Split Amiga channel in bits 5-4
		/// </summary>
		Split = (1 << 2),

		/// <summary>
		/// Surround channel
		/// </summary>
		Surround = (1 << 4)
	}
}
