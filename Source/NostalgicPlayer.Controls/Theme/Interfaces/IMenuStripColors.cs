/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Theme.Interfaces
{
	/// <summary>
	/// Holds all colors used by a themed menu strip
	/// </summary>
	public interface IMenuStripColors
	{
		/// <summary></summary>
		Color MenuBarBackgroundColor { get; }

		/// <summary></summary>
		Color NormalMenuBarItemBorderColor { get; }
		/// <summary></summary>
		Color NormalMenuBarItemBackgroundStartColor { get; }
		/// <summary></summary>
		Color NormalMenuBarItemBackgroundStopColor { get; }
		/// <summary></summary>
		Color NormalMenuBarItemTextColor { get; }

		/// <summary></summary>
		Color HoverMenuBarItemBorderColor { get; }
		/// <summary></summary>
		Color HoverMenuBarItemBackgroundStartColor { get; }
		/// <summary></summary>
		Color HoverMenuBarItemBackgroundStopColor { get; }
		/// <summary></summary>
		Color HoverMenuBarItemTextColor { get; }

		/// <summary></summary>
		Color OpenMenuBarItemBorderColor { get; }
		/// <summary></summary>
		Color OpenMenuBarItemBackgroundStartColor { get; }
		/// <summary></summary>
		Color OpenMenuBarItemBackgroundStopColor { get; }
		/// <summary></summary>
		Color OpenMenuBarItemTextColor { get; }

		/// <summary></summary>
		Color DropDownBorderColor { get; }
		/// <summary></summary>
		Color DropDownSeparatorColor { get; }

		/// <summary></summary>
		Color NormalDropDownItemBackgroundStartColor { get; }
		/// <summary></summary>
		Color NormalDropDownItemBackgroundMiddleColor { get; }
		/// <summary></summary>
		Color NormalDropDownItemBackgroundStopColor { get; }
		/// <summary></summary>
		Color NormalDropDownItemTextColor { get; }

		/// <summary></summary>
		Color HoverDropDownItemBackgroundStartColor { get; }
		/// <summary></summary>
		Color HoverDropDownItemBackgroundMiddleColor { get; }
		/// <summary></summary>
		Color HoverDropDownItemBackgroundStopColor { get; }
		/// <summary></summary>
		Color HoverDropDownItemTextColor { get; }

		/// <summary></summary>
		Color DisabledDropDownItemBackgroundStartColor { get; }
		/// <summary></summary>
		Color DisabledDropDownItemBackgroundMiddleColor { get; }
		/// <summary></summary>
		Color DisabledDropDownItemBackgroundStopColor { get; }
		/// <summary></summary>
		Color DisabledDropDownItemTextColor { get; }
	}
}
