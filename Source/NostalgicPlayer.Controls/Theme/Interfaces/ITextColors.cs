/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Theme.Interfaces
{
	/// <summary>
	/// Holds the standard text colors used by controls drawing text
	/// directly on the background of their parent
	/// </summary>
	internal interface ITextColors
	{
		/// <summary></summary>
		Color NormalTextColor { get; }
		/// <summary></summary>
		Color HoverTextColor { get; }
		/// <summary></summary>
		Color PressedTextColor { get; }
		/// <summary></summary>
		Color DisabledTextColor { get; }
	}
}
