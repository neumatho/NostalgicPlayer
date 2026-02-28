/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Polycode.NostalgicPlayer.External.Download
{
	/// <summary>
	/// Helper class to download pictures
	/// </summary>
	public interface IPictureDownloader : IDisposable
	{
		/// <summary>
		/// Change the max number of items in the cache
		/// </summary>
		void SetMaxNumberInCache(int maxNumberOfItemsInCache);

		/// <summary>
		/// Return a bitmap of the picture at the URL given.
		/// May not be called from the UI thread
		/// </summary>
		Task<Bitmap> GetPictureAsync(string url, CancellationToken cancellationToken);
	}
}
