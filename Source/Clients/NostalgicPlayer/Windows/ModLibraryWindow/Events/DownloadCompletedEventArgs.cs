/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Events
{
	/// <summary>
	/// Event args for download completion
	/// </summary>
	public class DownloadCompletedEventArgs : EventArgs
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public DownloadCompletedEventArgs(string localPath, PlayFlag playFlag, bool success)
		{
			LocalPath = localPath;
			PlayFlag = playFlag;
			Success = success;
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



		/********************************************************************/
		/// <summary>
		/// Indicate how to reach on the downloaded module
		/// </summary>
		/********************************************************************/
		public PlayFlag PlayFlag
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Whether download was successful
		/// </summary>
		/********************************************************************/
		public bool Success
		{
			get;
		}
	}
}
