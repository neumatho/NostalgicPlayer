/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Events
{
	/// <summary>
	/// Event args for when a new download has started
	/// </summary>
	public class DownloadStartedEventArgs : EventArgs
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public DownloadStartedEventArgs(string moduleName)
		{
			ModuleName = moduleName;
		}



		/********************************************************************/
		/// <summary>
		/// The name of the module to download
		/// </summary>
		/********************************************************************/
		public string ModuleName
		{
			get;
		}
	}
}
