/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Images
{
	/// <summary>
	/// Holds all the images needed by the form
	/// </summary>
	internal interface IFormImages
	{
		/// <summary>
		/// Gets the close button image
		/// </summary>
		Bitmap CaptionClose { get; }

		/// <summary>
		/// Gets the maximize button image
		/// </summary>
		Bitmap CaptionMaximize { get; }

		/// <summary>
		/// Gets the minimize button image
		/// </summary>
		Bitmap CaptionMinimize { get; }

		/// <summary>
		/// Gets the normalize button image
		/// </summary>
		Bitmap CaptionNormalize { get; }
	}
}
