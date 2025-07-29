﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Audius;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.Pages
{
	/// <summary>
	/// All Audius pages must implement this interface
	/// </summary>
	public interface IAudiusPage
	{
		/// <summary>
		/// Will initialize the control
		/// </summary>
		void Initialize(IMainWindowApi mainWindow, IAudiusWindowApi audiusWindow, AudiusApi audiusApi);

		/// <summary>
		/// Will refresh the page with new data
		/// </summary>
		void RefreshPage();

		/// <summary>
		/// Cleanup used resources
		/// </summary>
		void CleanupPage();
	}
}
