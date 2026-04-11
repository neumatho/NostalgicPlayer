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
	/// 
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	// ReSharper disable InconsistentNaming
	public struct TRACKMOUSEEVENT
	{
		/// <summary></summary>
		public uint cbSize;
		/// <summary></summary>
		public uint dwFlags;
		/// <summary></summary>
		public IntPtr hwndTrack;
		/// <summary></summary>
		public uint dwHoverTime;
	}
	// ReSharper restore InconsistentNaming
}
