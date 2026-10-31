/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Theme.Interfaces
{
	/// <summary>
	/// Holds all the colors used by a GroupBox container
	/// </summary>
	public interface IGroupBoxColors
	{
		/// <summary></summary>
		Color BorderColor { get; }
		/// <summary></summary>
		Color HeaderColor { get; }
	}
}
