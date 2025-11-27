/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.ModLibraryWindow
{
	/// <summary>
	/// Represents a single download item in the queue
	/// </summary>
	internal class DownloadQueueItem
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public DownloadQueueItem(TreeNode entry, bool shouldPlayImmediately)
		{
			Entry = entry;
			ShouldPlayImmediately = shouldPlayImmediately;
		}

		public TreeNode Entry
		{
			get;
		}

		public bool ShouldPlayImmediately
		{
			get;
		}
	}
}