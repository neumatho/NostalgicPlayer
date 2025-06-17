/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Threading;
using Polycode.NostalgicPlayer.Audius.Models.Playlists;

namespace Polycode.NostalgicPlayer.Audius.Interfaces
{
	/// <summary>
	/// Interface for interacting of playlists with Audius
	/// </summary>
	public interface IPlaylistClient
	{
		/// <summary>
		/// Gets the most popular playlists on Audius
		/// </summary>
		TrendingPlaylistModel[] GetTrendingPlaylists(string time, CancellationToken cancellationToken);
	}
}
