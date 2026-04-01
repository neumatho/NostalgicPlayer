/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.InteropServices;

namespace Polycode.NostalgicPlayer.Platform.Native
{
	/// <summary>
	/// 
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	// ReSharper disable InconsistentNaming
	public struct RECT
	{
		/// <summary></summary>
		public int Left;
		/// <summary></summary>
		public int Top;
		/// <summary></summary>
		public int Right;
		/// <summary></summary>
		public int Bottom;
	}
	// ReSharper restore InconsistentNaming
}
