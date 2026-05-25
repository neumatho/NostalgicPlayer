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
	public enum SWP : uint
	{
		/// <summary></summary>
		NOSIZE = 0x0001,
		/// <summary></summary>
		NOMOVE = 0x0002,
		/// <summary></summary>
		NOZORDER = 0x0004,
		/// <summary></summary>
		NOACTIVATE = 0x0010,
		/// <summary></summary>
		FRAMECHANGED = 0x0020,
	}
	// ReSharper restore InconsistentNaming
}
