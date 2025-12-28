/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Threading;
using Polycode.NostalgicPlayer.RestClients.Audius.Interfaces;
using Polycode.NostalgicPlayer.RestClients.Audius.Models.Playlists;
using RestSharp;

namespace Polycode.NostalgicPlayer.RestClients.Audius.Clients
{
	/// <summary>
	/// Holds methods for interacting with playlists on Audius
	/// </summary>
	internal class PlaylistClient : ClientBase, IPlaylistClient
	{
		/********************************************************************/
		/// <summary>
		/// Gets the most popular playlists on Audius
		/// </summary>
		/********************************************************************/
		public TrendingPlaylistModel[] GetTrendingPlaylists(string time, CancellationToken cancellationToken)
		{
			RestRequest request = new RestRequest("v1/playlists/trending");
			request.AddQueryParameter("time", time);

			// For some reason, this call does not work on retrieved URLs (they got a
			// timeout), so we have to use the main Audius URL
			return DoRequestOnMainUrl<TrendingPlaylistModel[]>(request, cancellationToken);
		}



		/********************************************************************/
		/// <summary>
		/// Will use the query given to search after playlists
		/// </summary>
		/********************************************************************/
		public PlaylistModel[] Search(string query, CancellationToken cancellationToken)
		{
			RestRequest request = new RestRequest("v1/playlists/search");
			request.AddQueryParameter("query", query);

			return DoRequest<PlaylistModel[]>(request, cancellationToken) ?? [];
		}
	}
}
