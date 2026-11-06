/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Sources;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Downloader
{
	/// <summary>
	/// Holds information about the module to download
	/// </summary>
	public class DownloadModuleItem
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public DownloadModuleItem(IModLibrarySource source, TreeNode entry)
		{
			Source = source;
			Entry = entry;
		}



		/********************************************************************/
		/// <summary>
		/// The source to use to download the entry
		/// </summary>
		/********************************************************************/
		public IModLibrarySource Source
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// The entry to download
		/// </summary>
		/********************************************************************/
		public TreeNode Entry
		{
			get;
		}
	}
}
