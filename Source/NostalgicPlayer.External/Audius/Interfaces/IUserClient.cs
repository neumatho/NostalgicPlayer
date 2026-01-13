/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Threading;
using Polycode.NostalgicPlayer.External.Audius.Models.Playlists;
using Polycode.NostalgicPlayer.External.Audius.Models.Tracks;
using Polycode.NostalgicPlayer.External.Audius.Models.Users;

namespace Polycode.NostalgicPlayer.External.Audius.Interfaces
{
	/// <summary>
	/// Interface for interacting of users with Audius
	/// </summary>
	public interface IUserClient
	{
		/// <summary>
		/// Will use the query given to search after users
		/// </summary>
		UserModel[] Search(string query, CancellationToken cancellationToken);

		/// <summary>
		/// Retrieve all the tracks for the given user
		/// </summary>
		TrackModel[] GetTracks(string userId, CancellationToken cancellationToken);

		/// <summary>
		/// Retrieve all the playlists for the given user
		/// </summary>
		PlaylistModel[] GetPlaylists(string userId, CancellationToken cancellationToken);
	}
}
