/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Windows.Forms;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Services
{
	/// <summary>
	/// Initialize controls with dependency injection
	/// </summary>
	public interface IControlInitializerService
	{
		/// <summary>
		/// Initialize a collection of controls with dependency injections
		/// </summary>
		void InitializeControls(Control.ControlCollection controls);

		/// <summary>
		/// Initialize a single control and its child controls with
		/// dependency injections
		/// </summary>
		void InitializeSingleControl(Control control);
	}
}
