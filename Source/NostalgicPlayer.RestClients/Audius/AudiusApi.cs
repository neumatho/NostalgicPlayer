/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.RestClients.Audius.Clients;
using Polycode.NostalgicPlayer.RestClients.Audius.Interfaces;

namespace Polycode.NostalgicPlayer.RestClients.Audius
{
	/// <summary>
	/// Main class for the Audius API
	/// </summary>
	public class AudiusApi
	{
		/********************************************************************/
		/// <summary>
		/// Returns the client for interacting with tracks
		/// </summary>
		/********************************************************************/
		public ITrackClient GetTrackClient()
		{
			return new TrackClient();
		}



		/********************************************************************/
		/// <summary>
		/// Returns the client for interacting with playlists
		/// </summary>
		/********************************************************************/
		public IPlaylistClient GetPlaylistClient()
		{
			return new PlaylistClient();
		}



		/********************************************************************/
		/// <summary>
		/// Returns the client for interacting with users
		/// </summary>
		/********************************************************************/
		public IUserClient GetUserClient()
		{
			return new UserClient();
		}
	}
}
