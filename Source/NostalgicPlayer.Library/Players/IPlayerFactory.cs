/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Library.Players
{
	/// <summary>
	/// Factory implementation to create new instances of the right player
	/// </summary>
	public interface IPlayerFactory
	{
		/// <summary>
		/// Return a new instance of the player to use based on the given
		/// agent
		/// </summary>
		IPlayer GetPlayer(IAgentWorker agent);
	}
}
