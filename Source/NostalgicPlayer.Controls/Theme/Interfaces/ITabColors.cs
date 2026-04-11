/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Theme.Interfaces
{
	/// <summary>
	/// Holds all colors used by a themed tab control
	/// </summary>
	public interface ITabColors
	{
		/// <summary></summary>
		Color BorderColor { get; }
		/// <summary></summary>
		Color BackgroundColor { get; }

		/// <summary></summary>
		Color NormalTabBackgroundStartColor { get; }
		/// <summary></summary>
		Color NormalTabBackgroundStopColor { get; }
		/// <summary></summary>
		Color NormalTabTextColor { get; }

		/// <summary></summary>
		Color HoverTabBackgroundStartColor { get; }
		/// <summary></summary>
		Color HoverTabBackgroundStopColor { get; }
		/// <summary></summary>
		Color HoverTabTextColor { get; }

		/// <summary></summary>
		Color SelectedTabBackgroundStartColor { get; }
		/// <summary></summary>
		Color SelectedTabBackgroundStopColor { get; }
		/// <summary></summary>
		Color SelectedTabTextColor { get; }
	}
}
