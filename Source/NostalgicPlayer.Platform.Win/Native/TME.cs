/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Platform.Native
{
	/// <summary>
	/// 
	/// </summary>
	// ReSharper disable InconsistentNaming
	[Flags]
	public enum TME : uint
	{
		/// <summary></summary>
		LEAVE = 0x00000002,
		/// <summary></summary>
		NONCLIENT = 0x00000010
	}
	// ReSharper restore InconsistentNaming
}
