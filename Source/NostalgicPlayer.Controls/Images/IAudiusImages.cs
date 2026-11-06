/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Images
{
	/// <summary>
	/// Holds all the images needed by the Audius window
	/// </summary>
	public interface IAudiusImages
	{
		/// <summary>
		/// Gets the repost image
		/// </summary>
		Bitmap Repost { get; }

		/// <summary>
		/// Gets the favorite image
		/// </summary>
		Bitmap Favorite { get; }

		/// <summary>
		/// Gets the show profile information image
		/// </summary>
		Bitmap ShowProfileInfo { get; }

		/// <summary>
		/// Gets the close profile image
		/// </summary>
		Bitmap Close { get; }

		/// <summary>
		/// Gets the unknown album cover image
		/// </summary>
		Bitmap UnknownAlbumCover { get; }

		/// <summary>
		/// Gets the unknown profile image
		/// </summary>
		Bitmap UnknownProfile { get; }
	}
}
