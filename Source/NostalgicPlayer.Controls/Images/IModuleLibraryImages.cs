/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Images
{
	/// <summary>
	/// Holds all the images needed by the Module Library window
	/// </summary>
	public interface IModuleLibraryImages
	{
		/// <summary>
		/// Gets the directory image
		/// </summary>
		Bitmap Directory { get; }

		/// <summary>
		/// Gets the filer image
		/// </summary>
		Bitmap File { get; }

		/// <summary>
		/// Gets the favorite image
		/// </summary>
		Bitmap Favorite { get; }
	}
}
