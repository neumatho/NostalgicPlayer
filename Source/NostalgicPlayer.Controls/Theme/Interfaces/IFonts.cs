/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Theme.Interfaces
{
	/// <summary>
	/// Holds all the fonts
	/// </summary>
	public interface IFonts
	{
		/// <summary></summary>
		Font FormTitleFont { get; }

		/// <summary></summary>
		Font RegularFont { get; }

		/// <summary></summary>
		Font MonospaceFont { get; }
	}
}
