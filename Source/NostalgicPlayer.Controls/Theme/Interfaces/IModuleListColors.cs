/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Theme.Interfaces
{
	/// <summary>
	/// Holds all colors used by a themed module list
	/// </summary>
	public interface IModuleListColors
	{
		/// <summary></summary>
		Color BackgroundColor { get; }
		/// <summary></summary>
		Color DropLineColor { get; }

		/// <summary></summary>
		Color NormalItemBackgroundStartColor { get; }
		/// <summary></summary>
		Color NormalItemBackgroundMiddleColor { get; }
		/// <summary></summary>
		Color NormalItemBackgroundStopColor { get; }
		/// <summary></summary>
		Color NormalItemTextColor { get; }
		/// <summary></summary>
		Color NormalItemSubSongColor { get; }

		/// <summary></summary>
		Color SelectedItemBackgroundStartColor { get; }
		/// <summary></summary>
		Color SelectedItemBackgroundMiddleColor { get; }
		/// <summary></summary>
		Color SelectedItemBackgroundStopColor { get; }
		/// <summary></summary>
		Color SelectedItemTextColor { get; }
		/// <summary></summary>
		Color SelectedItemSubSongColor { get; }
	}
}
