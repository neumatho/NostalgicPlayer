/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Library.Players
{
	/// <summary>
	/// Factory implementation to create new instances of the right player
	/// </summary>
	internal class PlayerFactory : IPlayerFactory
	{
		private readonly IApplicationContext applicationContext;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public PlayerFactory(IApplicationContext applicationContext)
		{
			this.applicationContext = applicationContext;
		}



		/********************************************************************/
		/// <summary>
		/// Return a new instance of the player to use based on the given
		/// agent
		/// </summary>
		/********************************************************************/
		public IPlayer GetPlayer(IPlayerAgent playerAgent)
		{
			if (playerAgent is IModulePlayerAgent)
				return applicationContext.Container.GetInstance<IModulePlayer>();

			if (playerAgent is ISamplePlayerAgent)
				return applicationContext.Container.GetInstance<ISamplePlayer>();

			return null;
		}
	}
}
