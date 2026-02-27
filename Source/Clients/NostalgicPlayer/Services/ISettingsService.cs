/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Services
{
	/// <summary>
	/// Manage the current settings
	/// </summary>
	public interface ISettingsService
	{
		/// <summary>
		/// Save the settings
		/// </summary>
		void SaveSettings();

		/// <summary>
		/// Return the settings implementation
		/// </summary>
		ISettings Settings { get; }
	}
}
