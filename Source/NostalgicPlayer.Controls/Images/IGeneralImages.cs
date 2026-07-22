/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Images
{
	/// <summary>
	/// Holds all the general usable images
	/// </summary>
	public interface IGeneralImages
	{
		/// <summary>
		/// Gets the logo image
		/// </summary>
		Bitmap Logo { get; }

		/// <summary>
		/// Gets the message box error icon
		/// </summary>
		Bitmap Error { get; }

		/// <summary>
		/// Gets the message box warning icon
		/// </summary>
		Bitmap Warning { get; }

		/// <summary>
		/// Gets the message box information icon
		/// </summary>
		Bitmap Information { get; }

		/// <summary>
		/// Gets the message box question icon
		/// </summary>
		Bitmap Question { get; }

		/// <summary>
		/// Gets the search button image
		/// </summary>
		Bitmap Search { get; }
	}
}
