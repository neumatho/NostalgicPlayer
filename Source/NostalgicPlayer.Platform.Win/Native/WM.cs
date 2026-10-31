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
		SETREDRAW = 0x000B,

		/// <summary></summary>
		NCMOUSEFIRST = 0x00A0,

		/// <summary></summary>
		NCMOUSELAST = 0x00AD,

		/// <summary></summary>
		KEYFIRST = 0x0100,

		/// <summary></summary>
		KEYLAST = 0x0109,

		/// <summary></summary>
		MOUSEFIRST = 0x0200,

		/// <summary></summary>
		MOUSEWHEEL = 0x020A,

		/// <summary></summary>
		MOUSELAST = 0x020E
	}
	// ReSharper restore InconsistentNaming
}
