/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Library.Players;

namespace Polycode.NostalgicPlayer.Library.Test.Helpers
{
	/// <summary>
	/// Player factory used by the tests. The loader only stores the returned
	/// player, so no real player is needed
	/// </summary>
	internal class TestPlayerFactory : IPlayerFactory
	{
		/********************************************************************/
		/// <summary>
		/// Return a new instance of the player to use based on the given
		/// agent
		/// </summary>
		/********************************************************************/
		public IPlayer GetPlayer(IAgentWorker agent)
		{
			return null;
		}
	}
}
