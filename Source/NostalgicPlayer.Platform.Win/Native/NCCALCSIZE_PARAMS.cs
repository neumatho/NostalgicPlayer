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
	public struct NCCALCSIZE_PARAMS
	{
		/// <summary></summary>
		public RECT rgrc0;
		/// <summary></summary>
		public RECT rgrc1;
		/// <summary></summary>
		public RECT rgrc2;
		/// <summary></summary>
		public IntPtr lppos;
	}
	// ReSharper restore InconsistentNaming
}
