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
	internal enum TME : uint
	{
		LEAVE = 0x00000002,
		NONCLIENT = 0x00000010
	}
	// ReSharper restore InconsistentNaming
}
