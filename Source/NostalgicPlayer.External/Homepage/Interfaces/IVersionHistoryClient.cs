/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.External.Homepage.Models.VersionHistory;

namespace Polycode.NostalgicPlayer.External.Homepage.Interfaces
{
	/// <summary>
	/// Interface to retrieve version history from NostalgicPlayer home page
	/// </summary>
	public interface IVersionHistoryClient
	{
		/// <summary>
		/// Return all histories between the two arguments
		/// </summary>
		HistoriesModel GetHistories(string fromVersion, string toVersion);
	}
}
