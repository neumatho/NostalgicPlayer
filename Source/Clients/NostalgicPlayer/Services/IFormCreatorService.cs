/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Windows.Forms;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Services
{
	/// <summary>
	/// Helper to instance Windows Form objects
	/// </summary>
	public interface IFormCreatorService
	{
		/// <summary>
		/// Will create a new instance of the window type given and call
		/// InitializeForm() on it while resolving dependencies
		/// </summary>
		T GetFormInstance<T>() where T : Form, new();

		/// <summary>
		/// Initialize a single control with dependency injections
		/// </summary>
		void InitializeControl(Control control);
	}
}
