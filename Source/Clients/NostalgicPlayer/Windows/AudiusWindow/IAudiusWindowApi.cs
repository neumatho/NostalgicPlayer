/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Windows.Forms;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AudiusWindow
{
	/// <summary>
	/// Defines the public API to the Audius window
	/// </summary>
	public interface IAudiusWindowApi
	{
		/// <summary>
		/// Return the form of the Audius window
		/// </summary>
		Form Form { get; }
	}
}
