/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Events
{
	/// <summary>
	/// Event args for all downloads completion
	/// </summary>
	public class AllDownloadsCompletedEventArgs : EventArgs
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AllDownloadsCompletedEventArgs(int failedCount)
		{
			FailedCount = failedCount;
		}



		/********************************************************************/
		/// <summary>
		/// Number of modules that failed to be downloaded
		/// </summary>
		/********************************************************************/
		public int FailedCount
		{
			get;
		}
	}
}
