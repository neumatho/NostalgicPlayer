/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;

namespace Polycode.NostalgicPlayer.Controls.Native
{
	/// <summary>
	/// Holds needed dll calls to gdi32.dll
	/// </summary>
	internal static class Gdi32
	{
		[DllImport("gdi32.dll")]
		public static extern IntPtr CreateRoundRectRgn(int x1, int y1, int x2, int y2, int w, int h);
	}
}
