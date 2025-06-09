/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Threading;
using Polycode.NostalgicPlayer.Audius.Interfaces;
using Polycode.NostalgicPlayer.Audius.Models.Tracks;
using RestSharp;

namespace Polycode.NostalgicPlayer.Audius.Clients
{
	/// <summary>
	/// Holds methods for interacting with tracks on Audius
	/// </summary>
	internal class TrackClient : ClientBase, ITrackClient
	{
		/********************************************************************/
		/// <summary>
		/// Gets the top 100 trending (most popular) tracks on Audius
		/// </summary>
		/********************************************************************/
		public TrackModel[] GetTrendingTracks(string genre, string time, CancellationToken cancellationToken)
		{
			RestRequest request = new RestRequest("v1/tracks/trending");
			request.AddQueryParameter("time", time);

			if (!string.IsNullOrEmpty(genre))
				request.AddQueryParameter("genre", genre);

			return DoRequest<TrackModel[]>(request, cancellationToken);
		}
	}
}
