/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Events
{
	/// <summary>
	/// Event args for download progress
	/// </summary>
	internal class DownloadProgressEventArgs : EventArgs
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public DownloadProgressEventArgs(int remainingCount, TreeNode currentEntry)
		{
			RemainingCount = remainingCount;
			CurrentEntry = currentEntry;
		}



		/********************************************************************/
		/// <summary>
		/// Number of files remaining in queue
		/// </summary>
		/********************************************************************/
		public int RemainingCount
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Current entry being downloaded
		/// </summary>
		/********************************************************************/
		public TreeNode CurrentEntry
		{
			get;
		}
	}
}
