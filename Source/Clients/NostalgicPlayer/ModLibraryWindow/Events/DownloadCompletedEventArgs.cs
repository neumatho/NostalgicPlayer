/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.ModLibraryWindow.Events
{
	/// <summary>
	/// Event args for download completion
	/// </summary>
	internal class DownloadCompletedEventArgs : EventArgs
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public DownloadCompletedEventArgs(TreeNode entry, string localPath, bool shouldPlayImmediately, bool success, string errorMessage = null)
		{
			Entry = entry;
			LocalPath = localPath;
			ShouldPlayImmediately = shouldPlayImmediately;
			Success = success;
			ErrorMessage = errorMessage;
		}

		/********************************************************************/
		/// <summary>
		/// The tree node entry that was downloaded
		/// </summary>
		/********************************************************************/
		public TreeNode Entry
		{
			get;
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
		/// Whether file should be played immediately
		/// </summary>
		/********************************************************************/
		public bool ShouldPlayImmediately
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

		/********************************************************************/
		/// <summary>
		/// Error message if download failed
		/// </summary>
		/********************************************************************/
		public string ErrorMessage
		{
			get;
		}
	}
}
