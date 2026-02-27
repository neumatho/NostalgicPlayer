/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.External.Audius.Interfaces;

namespace Polycode.NostalgicPlayer.External.Audius
{
	/// <summary>
	/// Creates the different clients
	/// </summary>
	public interface IAudiusClientFactory
	{
		/// <summary>
		/// Returns the client for interacting with tracks
		/// </summary>
		ITrackClient GetTrackClient();

		/// <summary>
		/// Returns the client for interacting with playlists
		/// </summary>
		IPlaylistClient GetPlaylistClient();

		/// <summary>
		/// Returns the client for interacting with users
		/// </summary>
		IUserClient GetUserClient();
	}
}
