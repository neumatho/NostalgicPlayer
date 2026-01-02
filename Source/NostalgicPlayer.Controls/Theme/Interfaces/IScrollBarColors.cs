/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Theme.Interfaces
{
	/// <summary>
	/// Holds all colors used by a themed scrollbar
	/// </summary>
	public interface IScrollBarColors
	{
		/// <summary></summary>
		Color BackgroundColor { get; }

		/// <summary></summary>
		Color NormalArrowColor { get; }

		/// <summary></summary>
		Color HoverArrowColor { get; }

		/// <summary></summary>
		Color PressedArrowColor { get; }

		/// <summary></summary>
		Color DisabledArrowColor { get; }

		/// <summary></summary>
		Color NormalThumbColor { get; }

		/// <summary></summary>
		Color HoverThumbColor { get; }

		/// <summary></summary>
		Color PressedThumbColor { get; }
	}
}
