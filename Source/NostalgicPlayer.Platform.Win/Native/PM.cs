/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Platform.Native
{
	/// <summary>
	/// Flags for PeekMessage
	/// </summary>
	// ReSharper disable InconsistentNaming
	[Flags]
	public enum PM : uint
	{
		/// <summary></summary>
		NOREMOVE = 0x0000,

		/// <summary></summary>
		REMOVE = 0x0001,

		/// <summary></summary>
		NOYIELD = 0x0002
	}
	// ReSharper restore InconsistentNaming
}
