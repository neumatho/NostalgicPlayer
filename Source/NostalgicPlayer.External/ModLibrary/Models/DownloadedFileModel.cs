/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.External.ModLibrary.Models
{
	/// <summary>
	/// Holds information about a single downloaded file
	/// </summary>
	public class DownloadedFileModel
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		internal DownloadedFileModel(string relativeFileName, string fileName, byte[] content)
		{
			RelativeFileName = relativeFileName;
			FileName = fileName;
			Content = content;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the relative path and filename to the downloaded file
		/// </summary>
		/********************************************************************/
		public string RelativeFileName { get; }



		/********************************************************************/
		/// <summary>
		/// Holds the filename
		/// </summary>
		/********************************************************************/
		public string FileName { get; }



		/********************************************************************/
		/// <summary>
		/// Holds the content of the file
		/// </summary>
		/********************************************************************/
		public byte[] Content { get; }
	}
}
