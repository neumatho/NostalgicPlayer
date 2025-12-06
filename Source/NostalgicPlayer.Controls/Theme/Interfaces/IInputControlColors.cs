/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Theme.Interfaces
{
	/// <summary>
	/// Holds all colors used by a themed input control
	/// </summary>
	internal interface IInputControlColors
	{
		/// <summary></summary>
		Color NormalBorderColor { get; }
		/// <summary></summary>
		Color NormalBackgroundColor { get; }
		/// <summary></summary>
		Color NormalTextColor { get; }

		/// <summary></summary>
		Color HoverBorderColor { get; }
		/// <summary></summary>
		Color HoverBackgroundColor { get; }
		/// <summary></summary>
		Color HoverTextColor { get; }

		/// <summary></summary>
		Color FocusedBorderColor { get; }
		/// <summary></summary>
		Color FocusedBackgroundColor { get; }
		/// <summary></summary>
		Color FocusedTextColor { get; }

		/// <summary></summary>
		Color DisabledBorderColor { get; }
		/// <summary></summary>
		Color DisabledBackgroundColor { get; }
		/// <summary></summary>
		Color DisabledTextColor { get; }
	}
}
