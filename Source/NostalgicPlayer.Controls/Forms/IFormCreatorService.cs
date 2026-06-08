/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Controls.Dialogs;

namespace Polycode.NostalgicPlayer.Controls.Forms
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
		T GetFormInstance<T>(params object[] extraArguments) where T : Form, new();

		/// <summary>
		/// Will create a new instance of the message box
		/// </summary>
		CustomMessageBox GetMessageBox(string message, string title, CustomMessageBox.IconType icon);
	}
}
