/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Theme.Interfaces
{
	/// <summary>
	/// Holds all colors used by a themed combo box
	/// </summary>
	public interface IComboBoxColors
	{
		/// <summary></summary>
		Color NormalBorderColor { get; }
		/// <summary></summary>
		Color NormalBackgroundColor { get; }
		/// <summary></summary>
		Color NormalTextColor { get; }
		/// <summary></summary>
		Color NormalArrowButtonBorderColor { get; }
		/// <summary></summary>
		Color NormalArrowButtonBackgroundStartColor { get; }
		/// <summary></summary>
		Color NormalArrowButtonBackgroundStopColor { get; }
		/// <summary></summary>
		Color NormalArrowColor { get; }

		/// <summary></summary>
		Color HoverBorderColor { get; }
		/// <summary></summary>
		Color HoverBackgroundColor { get; }
		/// <summary></summary>
		Color HoverTextColor { get; }
		/// <summary></summary>
		Color HoverArrowButtonBorderColor { get; }
		/// <summary></summary>
		Color HoverArrowButtonBackgroundStartColor { get; }
		/// <summary></summary>
		Color HoverArrowButtonBackgroundStopColor { get; }
		/// <summary></summary>
		Color HoverArrowColor { get; }

		/// <summary></summary>
		Color PressedBorderColor { get; }
		/// <summary></summary>
		Color PressedBackgroundColor { get; }
		/// <summary></summary>
		Color PressedTextColor { get; }
		/// <summary></summary>
		Color PressedArrowButtonBorderColor { get; }
		/// <summary></summary>
		Color PressedArrowButtonBackgroundStartColor { get; }
		/// <summary></summary>
		Color PressedArrowButtonBackgroundStopColor { get; }
		/// <summary></summary>
		Color PressedArrowColor { get; }

		/// <summary></summary>
		Color FocusedBorderColor { get; }
		/// <summary></summary>
		Color FocusedBackgroundColor { get; }
		/// <summary></summary>
		Color FocusedTextColor { get; }
		/// <summary></summary>
		Color FocusedArrowButtonBorderColor { get; }
		/// <summary></summary>
		Color FocusedArrowButtonBackgroundStartColor { get; }
		/// <summary></summary>
		Color FocusedArrowButtonBackgroundStopColor { get; }
		/// <summary></summary>
		Color FocusedArrowColor { get; }

		/// <summary></summary>
		Color DisabledBorderColor { get; }
		/// <summary></summary>
		Color DisabledBackgroundColor { get; }
		/// <summary></summary>
		Color DisabledTextColor { get; }
		/// <summary></summary>
		Color DisabledArrowButtonBorderColor { get; }
		/// <summary></summary>
		Color DisabledArrowButtonBackgroundStartColor { get; }
		/// <summary></summary>
		Color DisabledArrowButtonBackgroundStopColor { get; }
		/// <summary></summary>
		Color DisabledArrowColor { get; }

		/// <summary></summary>
		Color NormalDropDownBackgroundStartColor { get; }
		/// <summary></summary>
		Color NormalDropDownBackgroundMiddleColor { get; }
		/// <summary></summary>
		Color NormalDropDownBackgroundStopColor { get; }
		/// <summary></summary>
		Color NormalDropDownTextColor { get; }

		/// <summary></summary>
		Color SelectedDropDownBackgroundStartColor { get; }
		/// <summary></summary>
		Color SelectedDropDownBackgroundMiddleColor { get; }
		/// <summary></summary>
		Color SelectedDropDownBackgroundStopColor { get; }
		/// <summary></summary>
		Color SelectedDropDownTextColor { get; }
	}
}
