/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Services;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Library.Agent;
using Polycode.NostalgicPlayer.Library.Containers;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow.Pages.AgentLists
{
	/// <summary>
	/// Handle the players / converters tab
	/// </summary>
	public class PlayersListUserControl : AgentsListUserControl
	{
		private readonly IAgentManager agentManager;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public PlayersListUserControl()
		{
			agentManager = DependencyInjection.Container?.GetInstance<IAgentManager>();
		}



		/********************************************************************/
		/// <summary>
		/// Will return all agents of the main and extra types
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<AgentListInfo> GetAllAgents()
		{
			foreach (AgentInfo agentInfo in agentManager.GetAllAgents(AgentType.Players).GroupBy(a => a.AgentId).Select(g => g.First()))
				yield return new AgentListInfo { Id = agentInfo.AgentId, Name = agentInfo.AgentName, Description = agentInfo.AgentDescription, AgentInfo = agentInfo };

			foreach (AgentInfo agentInfo in agentManager.GetAllAgents(AgentType.ModuleConverters).GroupBy(a => a.AgentId).Select(g => g.First()))
				yield return new AgentListInfo { Id = agentInfo.AgentId, Name = agentInfo.AgentName, Description = agentInfo.AgentDescription, AgentInfo = agentInfo };

			foreach (AgentInfo agentInfo in agentManager.GetAllAgents(AgentType.Streamers).GroupBy(a => a.AgentId).Select(g => g.First()))
				yield return new AgentListInfo { Id = agentInfo.AgentId, Name = agentInfo.AgentName, Description = agentInfo.AgentDescription, AgentInfo = agentInfo };
		}



		/********************************************************************/
		/// <summary>
		/// Return the IDs of the agents in use if any
		/// </summary>
		/********************************************************************/
		protected override Guid[] GetAgentIdsInUse(IModuleHandlerService modHandler)
		{
			if (modHandler.StaticModuleInformation.ConverterAgentInfo != null)
				return [ modHandler.StaticModuleInformation.PlayerAgentInfo.AgentId, modHandler.StaticModuleInformation.ConverterAgentInfo.AgentId ];

			return [ modHandler.StaticModuleInformation.PlayerAgentInfo.AgentId ];
		}



		/********************************************************************/
		/// <summary>
		/// Return the agents to enable/disable for the given agent
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<AgentInfo> GetAgentCollection(AgentListInfo agentListInfo)
		{
			return agentManager.GetAllTypes(agentListInfo.Id);
		}
	}
}
