/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Theme.Interfaces.Images
{
	/// <summary>
	/// Holds the Main window image colors
	/// </summary>
	public interface IImageMainColors
	{
		/// <summary></summary>
		Color InformationColor { get; }
		/// <summary></summary>
		Color MuteColor { get; }
		/// <summary></summary>
		Color AddColor { get; }
		/// <summary></summary>
		Color RemoveColor { get; }
		/// <summary></summary>
		Color SwapColor { get; }
		/// <summary></summary>
		Color SortColor { get; }
		/// <summary></summary>
		Color MoveUpColor { get; }
		/// <summary></summary>
		Color MoveDownColor { get; }
		/// <summary></summary>
		Color ListColor { get; }
		/// <summary></summary>
		Color DiskColor { get; }
		/// <summary></summary>
		Color PreviousModuleColor { get; }
		/// <summary></summary>
		Color NextModuleColor { get; }
		/// <summary></summary>
		Color PreviousSongColor { get; }
		/// <summary></summary>
		Color NextSongColor { get; }
		/// <summary></summary>
		Color FastRewindColor { get; }
		/// <summary></summary>
		Color FastForwardColor { get; }
		/// <summary></summary>
		Color PlayColor { get; }
		/// <summary></summary>
		Color EjectColor { get; }
		/// <summary></summary>
		Color PauseColor { get; }
		/// <summary></summary>
		Color LoopColor { get; }
		/// <summary></summary>
		Color FavoritesColor { get; }
		/// <summary></summary>
		Color EqualizerColor { get; }
		/// <summary></summary>
		Color SamplesColor { get; }
	}
}
