/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Platform.Native
{
	/// <summary>
	/// Different Windows messages
	/// </summary>
	// ReSharper disable InconsistentNaming
	public enum WM : uint
	{
		/// <summary></summary>
		SIZE = 0x0005,
		/// <summary></summary>
		ACTIVATE = 0x0006,
		/// <summary></summary>
		SETREDRAW = 0x000B,
		/// <summary></summary>
		SETTEXT = 0x000C,
		/// <summary></summary>
		SYSCOLORCHANGE = 0x0015,
		/// <summary></summary>
		SETICON = 0x0080,
		/// <summary></summary>
		NCCALCSIZE = 0x0083,
		/// <summary></summary>
		NCHITTEST = 0x0084,
		/// <summary></summary>
		NCPAINT = 0x0085,
		/// <summary></summary>
		NCACTIVATE = 0x0086,
		/// <summary></summary>
		NCMOUSEMOVE = 0x00a0,
		/// <summary></summary>
		NCLBUTTONDOWN = 0x00A1,
		/// <summary></summary>
		NCLBUTTONUP = 0x00A2,
		/// <summary></summary>
		LBUTTONDOWN = 0x0201,
		/// <summary></summary>
		MOUSEWHEEL = 0x020A,
		/// <summary></summary>
		NCMOUSELEAVE = 0x02A2,

		// Undocumented messages used by UxTheme to draw themed captions/frames

		/// <summary></summary>
		NCUAHDRAWCAPTION = 0x00AE,
		/// <summary></summary>
		NCUAHDRAWFRAME = 0x00AF
	}
	// ReSharper restore InconsistentNaming
}
