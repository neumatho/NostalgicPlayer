/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Theme.Interfaces
{
	/// <summary>
	/// Holds all colors used by a themed list item
	/// </summary>
	public interface IListItemColors
	{
		/// <summary></summary>
		Color NormalBackgroundStartColor { get; }
		/// <summary></summary>
		Color NormalBackgroundMiddleColor { get; }
		/// <summary></summary>
		Color NormalBackgroundStopColor { get; }
		/// <summary></summary>
		Color NormalTextColor { get; }

		/// <summary></summary>
		Color SelectedBackgroundStartColor { get; }
		/// <summary></summary>
		Color SelectedBackgroundMiddleColor { get; }
		/// <summary></summary>
		Color SelectedBackgroundStopColor { get; }
		/// <summary></summary>
		Color SelectedTextColor { get; }
	}
}
