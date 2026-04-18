/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Platform.Native
{
	/// <summary>
	/// Different hittest results
	/// </summary>
	// ReSharper disable InconsistentNaming
	public enum HT : uint
	{
		/// <summary></summary>
		NOWHERE = 0,
		/// <summary></summary>
		CLIENT = 1,
		/// <summary></summary>
		CAPTION = 2,
		/// <summary></summary>
		SYSMENU = 3,
		/// <summary></summary>
		MINBUTTON = 8,
		/// <summary></summary>
		MAXBUTTON = 9,
		/// <summary></summary>
		LEFT = 10,
		/// <summary></summary>
		RIGHT = 11,
		/// <summary></summary>
		TOP = 12,
		/// <summary></summary>
		TOPLEFT = 13,
		/// <summary></summary>
		TOPRIGHT = 14,
		/// <summary></summary>
		BOTTOM = 15,
		/// <summary></summary>
		BOTTOMLEFT = 16,
		/// <summary></summary>
		BOTTOMRIGHT = 17,
		/// <summary></summary>
		CLOSE = 20
	}
	// ReSharper restore InconsistentNaming
}
