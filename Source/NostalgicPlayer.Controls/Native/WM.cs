/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Controls.Native
{
	/// <summary>
	/// Different Windows messages
	/// </summary>
	// ReSharper disable InconsistentNaming
	internal enum WM : uint
	{
		SIZE = 0x0005,
		ACTIVATE = 0x0006,
		SETTEXT = 0x000C,
		SYSCOLORCHANGE = 0x0015,
		SETICON = 0x0080,
		NCCALCSIZE = 0x0083,
		NCHITTEST = 0x0084,
		NCPAINT = 0x0085,
		NCACTIVATE = 0x0086,
		NCMOUSEMOVE = 0x00a0,
		NCLBUTTONDOWN = 0x00A1,
		NCLBUTTONUP = 0x00A2,
		NCMOUSELEAVE = 0x02A2,

		// Undocumented messages used by UxTheme to draw themed captions/frames
		NCUAHDRAWCAPTION = 0x00AE,
		NCUAHDRAWFRAME = 0x00AF
	}
	// ReSharper restore InconsistentNaming
}
