/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Native
{
	/// <summary>
	/// Holds needed dll calls to user32.dll
	/// </summary>
	public static class User32
	{
		/// <summary></summary>
		[DllImport("user32.dll")]
		public static extern bool HideCaret(IntPtr hWnd);

		/// <summary></summary>
		[DllImport("user32.dll")]
		public static extern IntPtr SendMessageW(IntPtr hWnd, WM msg, IntPtr wParam, IntPtr lParam);
	}
}
