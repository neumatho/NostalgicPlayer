/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.Containers.Flags
{
	/// <summary>
	/// Indicate the format of the sample and how to play it
	/// </summary>
	[Flags]
	public enum PlayBufferFlag
	{
		/// <summary>
		/// No flags
		/// </summary>
		None = 0x0000,

		/// <summary>
		/// Sample is in 16-bit
		/// </summary>
		_16Bit = 0x0001
	}
}
