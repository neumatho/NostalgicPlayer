/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Theme.Interfaces.Images
{
	/// <summary>
	/// Holds the general image colors
	/// </summary>
	public interface IImageGeneralColors
	{
		/// <summary></summary>
		Color MessageBoxErrorColor { get; }
		/// <summary></summary>
		Color MessageBoxWarningColor { get; }
		/// <summary></summary>
		Color MessageBoxInformationColor { get; }
		/// <summary></summary>
		Color MessageBoxQuestionColor { get; }
		/// <summary></summary>
		Color SearchColor { get; }
	}
}
