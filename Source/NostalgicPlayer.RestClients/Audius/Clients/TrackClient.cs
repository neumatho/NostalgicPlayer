/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Polycode.NostalgicPlayer.RestClients.Audius.Interfaces;
using Polycode.NostalgicPlayer.RestClients.Audius.Models.Tracks;
using RestSharp;

namespace Polycode.NostalgicPlayer.RestClients.Audius.Clients
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



		/********************************************************************/
		/// <summary>
		/// Gets the top 100 trending underground tracks on Audius
		/// </summary>
		/********************************************************************/
		public TrackModel[] GetTrendingUndergroundTracks(CancellationToken cancellationToken)
		{
			RestRequest request = new RestRequest("v1/tracks/trending/underground");
			request.AddQueryParameter("limit", "100");

			return DoRequest<TrackModel[]>(request, cancellationToken);
		}



		/********************************************************************/
		/// <summary>
		/// Return track information for the given track
		/// </summary>
		/********************************************************************/
		public TrackModel GetTrackInfo(string trackId, CancellationToken cancellationToken)
		{
			RestRequest request = new RestRequest("v1/tracks/{id}");
			request.AddUrlSegment("id", trackId);

			return DoRequest<TrackModel>(request, cancellationToken);
		}



		/********************************************************************/
		/// <summary>
		/// Return track information for multiple tracks
		/// </summary>
		/********************************************************************/
		public TrackModel[] GetBulkTrackInfo(IEnumerable<string> trackIds, CancellationToken cancellationToken)
		{
			List<string> bulkIds = new List<string>(trackIds);
			List<TrackModel> result = new List<TrackModel>();

			while (bulkIds.Count > 0)
			{
				RestRequest request = new RestRequest("v1/tracks");

				foreach (string id in bulkIds.Take(200))
					request.AddQueryParameter("id", id);

				result.AddRange(DoRequest<TrackModel[]>(request, cancellationToken) ?? []);

				bulkIds.RemoveRange(0, Math.Min(bulkIds.Count, 200));
			}

			return result.ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Will return the streaming URL for the given track ID
		/// </summary>
		/********************************************************************/
		public Uri GetStreamingUrl(string trackId)
		{
			return BuildUrl($"/v1/tracks/{trackId}/stream");
		}



		/********************************************************************/
		/// <summary>
		/// Will use the query given to search after tracks
		/// </summary>
		/********************************************************************/
		public TrackModel[] Search(string query, CancellationToken cancellationToken)
		{
			RestRequest request = new RestRequest("v1/tracks/search");
			request.AddQueryParameter("query", query);

			return DoRequest<TrackModel[]>(request, cancellationToken) ?? [];
		}
	}
}
