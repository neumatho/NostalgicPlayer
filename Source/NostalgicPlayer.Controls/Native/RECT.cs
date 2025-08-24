/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.InteropServices;

namespace Polycode.NostalgicPlayer.Controls.Native
{
	/// <summary>
	/// 
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	// ReSharper disable InconsistentNaming
	internal struct RECT
	{
		public int Left;
		public int Top;
		public int Right;
		public int Bottom;
	}
	// ReSharper restore InconsistentNaming
}
