/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.MainWindow;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.ModLibraryWindow
{
	/// <summary>
	/// Handles integration with the main window playlist
	/// </summary>
	internal class ModLibraryPlaylistIntegration
	{
		private readonly IMainWindowApi mainWindow;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModLibraryPlaylistIntegration(IMainWindowApi mainWindow)
		{
			this.mainWindow = mainWindow;
		}


		/********************************************************************/
		/// <summary>
		/// Add module to playlist and optionally play it
		/// </summary>
		/********************************************************************/
		public void AddToPlaylist(string localPath, bool shouldPlayImmediately)
		{
			if (mainWindow.Form is MainWindowForm mainWindowForm)
			{
				mainWindowForm.Invoke((Action)(() =>
				{
					// Check if module already exists in playlist
					var existingItem = mainWindowForm.FindModuleInList(localPath);
					if (existingItem != null)
					{
						// Module already in list - select and play it if should play immediately
						if (shouldPlayImmediately)
						{
							mainWindowForm.SelectAndPlayModule(existingItem);
						}
					}
					else
					{
						// Add the file to the list and optionally play it immediately
						mainWindowForm.AddFilesToModuleList(new[] {localPath}, shouldPlayImmediately);
					}
				}));
			}
		}
	}
}
