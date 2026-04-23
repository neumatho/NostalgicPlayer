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
	public enum RDW : uint
	{
		/// <summary></summary>
		INVALIDATE = 0x0001,
		/// <summary></summary>
		NOCHILDREN = 0x0040,
		/// <summary></summary>
		FRAME = 0x0400,
	}
	// ReSharper restore InconsistentNaming
}
