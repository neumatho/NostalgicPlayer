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
		Color TrackBorderColor { get; }
		/// <summary></summary>
		Color TrackBackgroundColor { get; }
		/// <summary></summary>
		Color TrackFillStartColor { get; }
		/// <summary></summary>
		Color TrackFillStopColor { get; }
		/// <summary></summary>
		Color TrackDisabledFillColor { get; }

		/// <summary></summary>
		Color TickColor { get; }
		/// <summary></summary>
		Color DisabledTickColor { get; }

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
		Color DisabledThumbBorderColor { get; }
		/// <summary></summary>
		Color DisabledThumbBackgroundStartColor { get; }
		/// <summary></summary>
		Color DisabledThumbBackgroundStopColor { get; }
	}
}
