/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.External.Download;

namespace Polycode.NostalgicPlayer.Platform
{
	/// <summary>
	/// Create new instances of the picture downloader
	/// </summary>
	internal class PictureDownloaderFactory : IPictureDownloaderFactory
	{
		/********************************************************************/
		/// <summary>
		/// Create a new instance
		/// </summary>
		/********************************************************************/
		public IPictureDownloader Create()
		{
			if (OperatingSystem.IsWindows())
				return new PictureDownloader();

			throw new NotSupportedException("Not running on Windows");
		}
	}
}
