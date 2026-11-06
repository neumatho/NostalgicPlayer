/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Theme.Interfaces.Images
{
	/// <summary>
	/// Holds the ModLibrary window image colors
	/// </summary>
	public interface IImageModLibraryColors
	{
		/// <summary></summary>
		Color DirectoryColor { get; }
		/// <summary></summary>
		Color FileColor { get; }
		/// <summary></summary>
		Color FavoriteColor { get; }
	}
}
