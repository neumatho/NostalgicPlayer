/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;

namespace Polycode.NostalgicPlayer.Platform.Native
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
		public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

		/// <summary></summary>
		[DllImport("user32.dll")]
		public static extern IntPtr GetWindowDC(IntPtr hWnd);

		/// <summary></summary>
		[DllImport("user32.dll")]
		public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

		/// <summary></summary>
		[DllImport("user32.dll")]
		public static extern int MapWindowPoints(IntPtr hWndFrom, IntPtr hWndTo, ref RECT lpPoints, int cPoints);

		/// <summary></summary>
		[DllImport("user32.dll")]
		public static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, uint flags);

		/// <summary></summary>
		[DllImport("user32.dll")]
		public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

		/// <summary></summary>
		[DllImport("user32.dll")]
		public static extern IntPtr SendMessageW(IntPtr hWnd, WM msg, IntPtr wParam, IntPtr lParam);

		/// <summary></summary>
		[DllImport("user32.dll")]
		public static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

		/// <summary></summary>
		[DllImport("user32.dll")]
		public static extern bool TrackMouseEvent(ref TRACKMOUSEEVENT lpEventTrack);
	}
}
