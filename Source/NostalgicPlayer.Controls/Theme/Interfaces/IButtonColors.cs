/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Theme.Interfaces
{
	/// <summary>
	/// Holds all colors used by a themed button
	/// </summary>
	public interface IButtonColors
	{
		Color NormalBorderColor { get; }
		Color NormalBackgroundStartColor { get; }
		Color NormalBackgroundStopColor { get; }
		Color NormalTextColor { get; }

		Color HoverBorderColor { get; }
		Color HoverBackgroundStartColor { get; }
		Color HoverBackgroundStopColor { get; }
		Color HoverTextColor { get; }

		Color PressedBorderColor { get; }
		Color PressedBackgroundStartColor { get; }
		Color PressedBackgroundStopColor { get; }
		Color PressedTextColor { get; }

		Color FocusedBorderColor { get; }
		Color FocusedBackgroundStartColor { get; }
		Color FocusedBackgroundStopColor { get; }
		Color FocusedTextColor { get; }

		Color DisabledBorderColor { get; }
		Color DisabledBackgroundStartColor { get; }
		Color DisabledBackgroundStopColor { get; }
		Color DisabledTextColor { get; }
	}
}
