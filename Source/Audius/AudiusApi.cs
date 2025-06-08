/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Audius.Clients;
using Polycode.NostalgicPlayer.Audius.Interfaces;

namespace Polycode.NostalgicPlayer.Audius
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
	}
}
