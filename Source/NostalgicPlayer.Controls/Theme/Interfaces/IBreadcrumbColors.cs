/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Theme.Interfaces
{
	/// <summary>
	/// Holds all colors used by a themed breadcrumb control
	/// </summary>
	public interface IBreadcrumbColors
	{
		/// <summary></summary>
		Color TextColor { get; }
		/// <summary></summary>
		Color HoverTextColor { get; }
		/// <summary></summary>
		Color PressedTextColor { get; }
		/// <summary></summary>
		Color CurrentTextColor { get; }
		/// <summary></summary>
		Color DisabledTextColor { get; }

		/// <summary></summary>
		Color SeparatorColor { get; }
	}
}
