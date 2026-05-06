/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Theme.Interfaces
{
	/// <summary>
	/// Holds all colors used by a themed track bar
	/// </summary>
	public interface ITrackBarColors
	{
		/// <summary></summary>
		Color BackgroundColor { get; }

		/// <summary></summary>
		Color NormalTrackBorderColor { get; }
		/// <summary></summary>
		Color NormalTrackBackgroundColor { get; }
		/// <summary></summary>
		Color NormalTrackFillStartColor { get; }
		/// <summary></summary>
		Color NormalTrackFillStopColor { get; }
		/// <summary></summary>
		Color NormalTickColor { get; }
		/// <summary></summary>
		Color NormalThumbBorderColor { get; }
		/// <summary></summary>
		Color NormalThumbBackgroundStartColor { get; }
		/// <summary></summary>
		Color NormalThumbBackgroundStopColor { get; }

		/// <summary></summary>
		Color HoverThumbBorderColor { get; }
		/// <summary></summary>
		Color HoverThumbBackgroundStartColor { get; }
		/// <summary></summary>
		Color HoverThumbBackgroundStopColor { get; }

		/// <summary></summary>
		Color PressedThumbBorderColor { get; }
		/// <summary></summary>
		Color PressedThumbBackgroundStartColor { get; }
		/// <summary></summary>
		Color PressedThumbBackgroundStopColor { get; }

		/// <summary></summary>
		Color FocusedThumbBorderColor { get; }
		/// <summary></summary>
		Color FocusedThumbBackgroundStartColor { get; }
		/// <summary></summary>
		Color FocusedThumbBackgroundStopColor { get; }

		/// <summary></summary>
		Color DisabledTrackBorderColor { get; }
		/// <summary></summary>
		Color DisabledTrackBackgroundColor { get; }
		/// <summary></summary>
		Color DisabledTrackFillStartColor { get; }
		/// <summary></summary>
		Color DisabledTrackFillStopColor { get; }
		/// <summary></summary>
		Color DisabledTickColor { get; }
		/// <summary></summary>
		Color DisabledThumbBorderColor { get; }
		/// <summary></summary>
		Color DisabledThumbBackgroundStartColor { get; }
		/// <summary></summary>
		Color DisabledThumbBackgroundStopColor { get; }
	}
}
