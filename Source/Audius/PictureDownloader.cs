/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace Polycode.NostalgicPlayer.Audius
{
	/// <summary>
	/// Helper class to download pictures from Audius
	/// </summary>
	[SupportedOSPlatform("windows")]
	public class PictureDownloader : IDisposable
	{
		private const int DownloadPictureTimeout = 30000;

		private class DownloadWaitInfo
		{
			public ManualResetEvent WaitEvent;
			public int WaitingCount;
		}

		private readonly object downloadLock = new object();	// Cannot use Lock class here, because the way it is used
		private readonly Dictionary<string, Bitmap> downloadedPictures;
		private readonly Dictionary<string, DownloadWaitInfo> aboutToBeDownloaded;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public PictureDownloader()
		{
			downloadedPictures = new Dictionary<string, Bitmap>(StringComparer.OrdinalIgnoreCase);
			aboutToBeDownloaded = new Dictionary<string, DownloadWaitInfo>(StringComparer.OrdinalIgnoreCase);
		}



		/********************************************************************/
		/// <summary>
		/// Dispose all bitmaps downloaded
		/// </summary>
		/********************************************************************/
		public void Dispose()
		{
			lock (downloadLock)
			{
				foreach (Bitmap bitmap in downloadedPictures.Values)
					bitmap.Dispose();

				downloadedPictures.Clear();

				foreach (DownloadWaitInfo downloadWaitInfo in aboutToBeDownloaded.Values)
					downloadWaitInfo.WaitEvent.Dispose();

				aboutToBeDownloaded.Clear();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return a bitmap of the picture at the URL given.
		/// May not be called from the UI thread
		/// </summary>
		/********************************************************************/
		public async Task<Bitmap> GetPictureAsync(string url, CancellationToken cancellationToken)
		{
			Monitor.Enter(downloadLock);

			try
			{
				if (downloadedPictures.TryGetValue(url, out Bitmap picture))
					return picture;

				if (aboutToBeDownloaded.TryGetValue(url, out DownloadWaitInfo downloadWaitInfo))
				{
					// Some other thread has already started downloading this picture,
					// so we will wait for it to finish
					downloadWaitInfo.WaitingCount++;

					Monitor.Exit(downloadLock);

					int signaled = WaitHandle.WaitAny([cancellationToken.WaitHandle, downloadWaitInfo.WaitEvent]);

					Monitor.Enter(downloadLock);

					if (signaled == 0)
						throw new OperationCanceledException();

					downloadWaitInfo.WaitingCount--;
					if (downloadWaitInfo.WaitingCount == 0)
					{
						// This is the last waiting thread, dispose the wait event
						downloadWaitInfo.WaitEvent.Dispose();
					}

					// Return the bitmap or null if something went wrong
					return downloadedPictures.GetValueOrDefault(url);
				}
				else
				{
					aboutToBeDownloaded[url] = new DownloadWaitInfo
					{
						WaitEvent = new ManualResetEvent(false),
						WaitingCount = 0
					};
				}
			}
			finally
			{
				Monitor.Exit(downloadLock);
			}

			// Start the download
			Bitmap downloadedPicture = null;

			try
			{
				using (HttpClient httpClient = new HttpClient())
				{
					using (CancellationTokenSource timeoutToken = new CancellationTokenSource(DownloadPictureTimeout))
					{
						using (CancellationTokenSource linkedToken = CancellationTokenSource.CreateLinkedTokenSource(timeoutToken.Token, cancellationToken))
						{
							using (HttpResponseMessage response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, linkedToken.Token))
							{
								response.EnsureSuccessStatusCode();

								await using (Stream stream = await response.Content.ReadAsStreamAsync(linkedToken.Token))
								{
									downloadedPicture = new Bitmap(stream);
								}
							}
						}
					}
				}
			}
			finally
			{
				lock (downloadLock)
				{
					if (downloadedPicture != null)
						downloadedPictures[url] = downloadedPicture;

					if (aboutToBeDownloaded.Remove(url, out DownloadWaitInfo downloadWaitInfo))
					{
						// If nobody is waiting, dispose the wait event, else signal the waiting threads
						if (downloadWaitInfo.WaitingCount == 0)
							downloadWaitInfo.WaitEvent.Dispose();
						else
							downloadWaitInfo.WaitEvent.Set();
					}
				}
			}

			return downloadedPicture;
		}
	}
}
