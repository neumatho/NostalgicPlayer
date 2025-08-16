/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Kit.Gui.Interfaces
{
	/// <summary>
	/// Derive from this interface, if you want to show a window in your agent
	/// </summary>
	public interface IAgentGuiDisplay : IAgentDisplay
	{
		/// <summary>
		/// Return the user control to show
		/// </summary>
		UserControl GetUserControl();

		/// <summary>
		/// Return the anchor name on the help page or null if none exists
		/// </summary>
		string HelpAnchor { get; }
	}
}
