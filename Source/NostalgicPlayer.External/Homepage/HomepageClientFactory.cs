/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.External.Homepage.Clients;
using Polycode.NostalgicPlayer.External.Homepage.Interfaces;

namespace Polycode.NostalgicPlayer.External.Homepage
{
	/// <summary>
	/// Creates the different clients
	/// </summary>
	internal class HomepageClientFactory : IHomepageClientFactory
	{
		/********************************************************************/
		/// <summary>
		/// Returns the client for getting history
		/// </summary>
		/********************************************************************/
		public IVersionHistoryClient GetVersionHistoryClient()
		{
			return new VersionHistoryClient();
		}
	}
}
