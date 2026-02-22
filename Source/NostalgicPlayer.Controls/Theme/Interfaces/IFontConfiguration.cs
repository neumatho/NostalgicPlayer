/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Controls.Components;

namespace Polycode.NostalgicPlayer.Controls.Theme.Interfaces
{
	/// <summary>
	/// Controls derive from this interface can be configured to use another font
	/// </summary>
	public interface IFontConfiguration
	{
		/// <summary>
		/// Set the FontConfiguration component to use for this control
		/// </summary>
		FontConfiguration UseFont { get; set; }
	}
}
