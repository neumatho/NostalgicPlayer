/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Threading;
using Polycode.NostalgicPlayer.Audius.Interfaces;
using Polycode.NostalgicPlayer.Audius.Models.Playlists;
using Polycode.NostalgicPlayer.Audius.Models.Tracks;
using Polycode.NostalgicPlayer.Audius.Models.Users;
using RestSharp;

namespace Polycode.NostalgicPlayer.Audius.Clients
{
	/// <summary>
	/// Holds methods for interacting with users on Audius
	/// </summary>
	internal class UserClient : ClientBase, IUserClient
	{
		/********************************************************************/
		/// <summary>
		/// Will use the query given to search after users
		/// </summary>
		/********************************************************************/
		public UserModel[] Search(string query, CancellationToken cancellationToken)
		{
			RestRequest request = new RestRequest("v1/users/search");
			request.AddQueryParameter("query", query);

			return DoRequest<UserModel[]>(request, cancellationToken) ?? [];
		}



		/********************************************************************/
		/// <summary>
		/// Retrieve all the tracks for the given user
		/// </summary>
		/********************************************************************/
		public TrackModel[] GetTracks(string userId, CancellationToken cancellationToken)
		{
			RestRequest request = new RestRequest("v1/users/{id}/tracks");
			request.AddUrlSegment("id", userId);

			return DoRequest<TrackModel[]>(request, cancellationToken) ?? [];
		}



		/********************************************************************/
		/// <summary>
		/// Retrieve all the playlists for the given user
		/// </summary>
		/********************************************************************/
		public PlaylistModel[] GetPlaylists(string userId, CancellationToken cancellationToken)
		{
			RestRequest request = new RestRequest("v1/users/{id}/playlists");
			request.AddUrlSegment("id", userId);

			return DoRequest<PlaylistModel[]>(request, cancellationToken) ?? [];
		}
	}
}
