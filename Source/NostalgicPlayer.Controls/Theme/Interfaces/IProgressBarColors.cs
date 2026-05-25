/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Theme.Interfaces
{
	/// <summary>
	/// Holds all colors used by a themed progress bar
	/// </summary>
	public interface IProgressBarColors
	{
		/// <summary></summary>
		Color NormalBorderColor { get; }
		/// <summary></summary>
		Color NormalBackgroundStartColor { get; }
		/// <summary></summary>
		Color NormalBackgroundStopColor { get; }
		/// <summary></summary>
		Color NormalFillStartColor { get; }
		/// <summary></summary>
		Color NormalFillStopColor { get; }

		/// <summary></summary>
		Color DisabledBorderColor { get; }
		/// <summary></summary>
		Color DisabledBackgroundStartColor { get; }
		/// <summary></summary>
		Color DisabledBackgroundStopColor { get; }
		/// <summary></summary>
		Color DisabledFillStartColor { get; }
		/// <summary></summary>
		Color DisabledFillStopColor { get; }
	}
}
