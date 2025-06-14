/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Threading;
using Polycode.NostalgicPlayer.Audius.Models.Tracks;

namespace Polycode.NostalgicPlayer.Audius.Interfaces
{
	/// <summary>
	/// Interface for interacting of tracks with Audius
	/// </summary>
	public interface ITrackClient
	{
		/// <summary>
		/// Gets the top 100 trending (most popular) tracks on Audius
		/// </summary>
		TrackModel[] GetTrendingTracks(string genre, string time, CancellationToken cancellationToken);

		/// <summary>
		/// Gets the top 100 trending underground tracks on Audius
		/// </summary>
		TrackModel[] GetTrendingUndergroundTracks(CancellationToken cancellationToken);

		/// <summary>
		/// Return track information for the given track
		/// </summary>
		TrackModel GetTrackInfo(string trackId, CancellationToken cancellationToken);

		/// <summary>
		/// Will return the streaming URL for the given track ID
		/// </summary>
		Uri GetStreamingUrl(string trackId);
	}
}
