/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Theme.Interfaces
{
	/// <summary>
	/// Holds all the colors used by a DataGridView
	/// </summary>
	public interface IDataGridViewColors
	{
		/// <summary></summary>
		Color BackgroundColor { get; }

		/// <summary></summary>
		Color NormalHeaderBorderColor { get; }
		/// <summary></summary>
		Color NormalHeaderBackgroundStartColor { get; }
		/// <summary></summary>
		Color NormalHeaderBackgroundStopColor { get; }
		/// <summary></summary>
		Color NormalHeaderTextColor { get; }

		/// <summary></summary>
		Color PressedHeaderBorderColor { get; }
		/// <summary></summary>
		Color PressedHeaderBackgroundStartColor { get; }
		/// <summary></summary>
		Color PressedHeaderBackgroundStopColor { get; }
		/// <summary></summary>
		Color PressedHeaderTextColor { get; }

		/// <summary></summary>
		Color NormalCellBackgroundStartColor { get; }
		/// <summary></summary>
		Color NormalCellBackgroundMiddleColor { get; }
		/// <summary></summary>
		Color NormalCellBackgroundStopColor { get; }
		/// <summary></summary>
		Color NormalCellTextColor { get; }

		/// <summary></summary>
		Color SelectedCellBackgroundStartColor { get; }
		/// <summary></summary>
		Color SelectedCellBackgroundMiddleColor { get; }
		/// <summary></summary>
		Color SelectedCellBackgroundStopColor { get; }
		/// <summary></summary>
		Color SelectedCellTextColor { get; }
	}
}
