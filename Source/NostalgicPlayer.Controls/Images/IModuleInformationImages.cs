/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Images
{
	/// <summary>
	/// Holds all the images needed by the Module Information window
	/// </summary>
	public interface IModuleInformationImages
	{
		/// <summary>
		/// Gets the previous picture image
		/// </summary>
		Bitmap PreviousPicture { get; }

		/// <summary>
		/// Gets the next picture image
		/// </summary>
		Bitmap NextPicture { get; }
	}
}
