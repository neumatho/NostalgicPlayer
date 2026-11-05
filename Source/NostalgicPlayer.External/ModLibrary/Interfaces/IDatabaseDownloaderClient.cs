/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Threading;
using System.Threading.Tasks;

namespace Polycode.NostalgicPlayer.External.ModLibrary.Interfaces
{
	/// <summary>
	/// 
	/// </summary>
	public interface IDatabaseDownloaderClient
	{
		/// <summary>
		/// Download and parse the database
		/// </summary>
		Task DownloadDatabaseAsync(string cacheDir, CancellationToken cancellationToken);
	}
}
