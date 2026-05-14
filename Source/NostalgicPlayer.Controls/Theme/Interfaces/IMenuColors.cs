/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Theme.Interfaces
{
	/// <summary>
	/// Holds all colors used by a themed menu
	/// </summary>
	internal interface IMenuColors
	{
		/// <summary></summary>
		Color BorderColor { get; }
		/// <summary></summary>
		Color DropDownSeparatorColor { get; }

		/// <summary></summary>
		Color NormalItemBackgroundStartColor { get; }
		/// <summary></summary>
		Color NormalItemBackgroundMiddleColor { get; }
		/// <summary></summary>
		Color NormalItemBackgroundStopColor { get; }
		/// <summary></summary>
		Color NormalItemTextColor { get; }

		/// <summary></summary>
		Color HoverItemBackgroundStartColor { get; }
		/// <summary></summary>
		Color HoverItemBackgroundMiddleColor { get; }
		/// <summary></summary>
		Color HoverItemBackgroundStopColor { get; }
		/// <summary></summary>
		Color HoverItemTextColor { get; }

		/// <summary></summary>
		Color DisabledItemBackgroundStartColor { get; }
		/// <summary></summary>
		Color DisabledItemBackgroundMiddleColor { get; }
		/// <summary></summary>
		Color DisabledItemBackgroundStopColor { get; }
		/// <summary></summary>
		Color DisabledItemTextColor { get; }
	}
}
