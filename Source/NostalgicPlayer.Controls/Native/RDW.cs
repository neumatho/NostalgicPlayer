/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Controls.Native
{
	/// <summary>
	/// 
	/// </summary>
	// ReSharper disable InconsistentNaming
	[Flags]
	internal enum RDW : uint
	{
		INVALIDATE = 0x0001,
		NOCHILDREN = 0x0040,
		FRAME = 0x0400,
	}
	// ReSharper restore InconsistentNaming
}
