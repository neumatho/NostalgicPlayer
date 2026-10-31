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
	/// Windows message structure
	/// </summary>
	// ReSharper disable InconsistentNaming
	[StructLayout(LayoutKind.Sequential)]
	public struct MSG
	{
		/// <summary></summary>
		public IntPtr hwnd;

		/// <summary></summary>
		public uint message;

		/// <summary></summary>
		public IntPtr wParam;

		/// <summary></summary>
		public IntPtr lParam;

		/// <summary></summary>
		public uint time;

		/// <summary></summary>
		public int ptX;

		/// <summary></summary>
		public int ptY;

		/// <summary></summary>
		public uint lPrivate;
	}
	// ReSharper restore InconsistentNaming
}
