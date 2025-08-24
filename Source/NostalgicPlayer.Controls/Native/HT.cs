/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Controls.Native
{
	/// <summary>
	/// Different hittest results
	/// </summary>
	// ReSharper disable InconsistentNaming
	internal enum HT : uint
	{
		NOWHERE = 0,
		CAPTION = 2,
		SYSMENU = 3,
		MINBUTTON = 8,
		MAXBUTTON = 9,
		LEFT = 10,
		RIGHT = 11,
		TOP = 12,
		TOPLEFT = 13,
		TOPRIGHT = 14,
		BOTTOM = 15,
		BOTTOMLEFT = 16,
		BOTTOMRIGHT = 17,
		CLOSE = 20
	}
	// ReSharper restore InconsistentNaming
}
