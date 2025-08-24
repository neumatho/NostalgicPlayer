/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Windows.Forms;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Bases
{
	/// <summary>
	/// All window forms must implement this interface
	/// </summary>
	public interface IWindowForm
	{
		/// <summary>
		/// Will update the window settings
		/// </summary>
		void UpdateWindowSettings();

		/// <summary>
		/// Holds the current window state
		/// </summary>
		public FormWindowState WindowState { get; set; }
	}
}
