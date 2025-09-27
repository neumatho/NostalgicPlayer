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
		/// <summary></summary>
		Color NormalBorderColor { get; }
		/// <summary></summary>
		Color NormalBackgroundStartColor { get; }
		/// <summary></summary>
		Color NormalBackgroundStopColor { get; }
		/// <summary></summary>
		Color NormalTextColor { get; }

		/// <summary></summary>
		Color HoverBorderColor { get; }
		/// <summary></summary>
		Color HoverBackgroundStartColor { get; }
		/// <summary></summary>
		Color HoverBackgroundStopColor { get; }
		/// <summary></summary>
		Color HoverTextColor { get; }

		/// <summary></summary>
		Color PressedBorderColor { get; }
		/// <summary></summary>
		Color PressedBackgroundStartColor { get; }
		/// <summary></summary>
		Color PressedBackgroundStopColor { get; }
		/// <summary></summary>
		Color PressedTextColor { get; }

		/// <summary></summary>
		Color FocusedBorderColor { get; }
		/// <summary></summary>
		Color FocusedBackgroundStartColor { get; }
		/// <summary></summary>
		Color FocusedBackgroundStopColor { get; }
		/// <summary></summary>
		Color FocusedTextColor { get; }

		/// <summary></summary>
		Color DisabledBorderColor { get; }
		/// <summary></summary>
		Color DisabledBackgroundStartColor { get; }
		/// <summary></summary>
		Color DisabledBackgroundStopColor { get; }
		/// <summary></summary>
		Color DisabledTextColor { get; }
	}
}
