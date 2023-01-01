/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Windows.Forms;

namespace Polycode.NostalgicPlayer.GuiKit.Interfaces
{
	/// <summary>
	/// Implement this interface on your user control showing your settings
	/// </summary>
	public interface ISettingsControl
	{
		/// <summary>
		/// Return the user control holding the settings
		/// </summary>
		UserControl GetUserControl();

		/// <summary>
		/// Will make a backup of settings that can be changed in real-time
		/// </summary>
		void MakeBackup();

		/// <summary>
		/// Will read the settings and set all the controls
		/// </summary>
		void ReadSettings();

		/// <summary>
		/// Will read the data from the controls and store them in the
		/// settings
		/// </summary>
		void WriteSettings();

		/// <summary>
		/// Will restore real-time values
		/// </summary>
		void CancelSettings();
	}
}
