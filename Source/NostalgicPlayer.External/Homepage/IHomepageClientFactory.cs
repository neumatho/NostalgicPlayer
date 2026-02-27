/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.External.Homepage.Interfaces;

namespace Polycode.NostalgicPlayer.External.Homepage
{
	/// <summary>
	/// Creates the different clients
	/// </summary>
	public interface IHomepageClientFactory
	{
		/// <summary>
		/// Returns the client for getting history
		/// </summary>
		IVersionHistoryClient GetVersionHistoryClient();
	}
}
