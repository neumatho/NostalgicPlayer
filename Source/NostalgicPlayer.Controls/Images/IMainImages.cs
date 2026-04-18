/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Images
{
	/// <summary>
	/// Holds all the images needed by the Main window
	/// </summary>
	public interface IMainImages
	{
		/// <summary>
		/// Gets the playing item image
		/// </summary>
		Bitmap GetPlayingItem(Color color);
	}
}
