/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Events
{
	/// <summary>
	/// Event args for module download completion
	/// </summary>
	public class ModuleDownloadedEventArgs : EventArgs
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleDownloadedEventArgs(string localPath)
		{
			LocalPath = localPath;
		}



		/********************************************************************/
		/// <summary>
		/// Local path where file was saved
		/// </summary>
		/********************************************************************/
		public string LocalPath
		{
			get;
		}
	}
}
