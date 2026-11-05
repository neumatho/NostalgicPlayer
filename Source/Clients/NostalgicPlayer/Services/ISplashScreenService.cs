/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Controls.Forms;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Services
{
	/// <summary>
	/// Service that handles the creation of the main form with splash screen
	/// </summary>
	public interface ISplashScreenService
	{
		/// <summary>
		/// Create the main form with splash screen showing progress
		/// </summary>
		NostalgicForm CreateMainForm();
	}
}
