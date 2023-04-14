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
	/// Holds needed dll calls to kernel32.dll
	/// </summary>
	public static class Kernel32
	{
		/// <summary></summary>
		[DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = true)]
		public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);
	}
}
