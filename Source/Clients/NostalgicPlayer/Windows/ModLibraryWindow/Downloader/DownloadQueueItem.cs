/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Sources;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Downloader
{
	/// <summary>
	/// Information about a single queue item
	/// </summary>
	public class DownloadQueueItem
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public DownloadQueueItem(string sourcePath, string destinationPath, string fileName, IModLibraryDownloader downloader, PlayFlag playFlag)
		{
			SourcePath = sourcePath;
			DestinationPath = destinationPath;
			FileName = fileName;
			Downloader = downloader;
			PlayFlag = playFlag;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the relative path where to download from
		/// </summary>
		/********************************************************************/
		public string SourcePath
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the path where to save the file
		/// </summary>
		/********************************************************************/
		public string DestinationPath
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// The name of the file
		/// </summary>
		/********************************************************************/
		public string FileName
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// The downloader to use
		/// </summary>
		/********************************************************************/
		public IModLibraryDownloader Downloader
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
	}
}
