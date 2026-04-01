/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.External.Audius;
using Polycode.NostalgicPlayer.Library.Agent;
using Polycode.NostalgicPlayer.Library.Players;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AudiusWindow.Loader
{
	/// <summary>
	/// Special loader factory for Audius loader
	/// </summary>
	public class AudiusLoaderFactory : IAudiusLoaderFactory
	{
		private readonly IAgentManager agentManager;
		private readonly IPlayerFactory playerFactory;
		private readonly IAudiusClientFactory clientFactory;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AudiusLoaderFactory(IAgentManager agentManager, IPlayerFactory playerFactory, IAudiusClientFactory audiusClientFactory)
		{
			this.agentManager = agentManager;
			this.playerFactory = playerFactory;
			clientFactory = audiusClientFactory;
		}



		/********************************************************************/
		/// <summary>
		/// Create specific loader for Audius
		/// </summary>
		/********************************************************************/
		public AudiusLoader CreateAudiusLoader()
		{
			return new AudiusLoader(agentManager, playerFactory, clientFactory);
		}
	}
}
