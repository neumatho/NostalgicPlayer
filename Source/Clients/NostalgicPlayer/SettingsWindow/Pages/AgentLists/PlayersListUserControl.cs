/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Modules;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow.Pages.AgentLists
{
	/// <summary>
	/// Handle the players / converters tab
	/// </summary>
	public class PlayersListUserControl : AgentsListUserControl
	{
		/********************************************************************/
		/// <summary>
		/// Will return all agents of the main and extra types
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<AgentListInfo> GetAllAgents()
		{
			foreach (AgentInfo agentInfo in manager.GetAllAgents(Manager.AgentType.Players).GroupBy(a => a.AgentId).Select(g => g.First()))
				yield return new AgentListInfo { Id = agentInfo.AgentId, Name = agentInfo.AgentName, Description = agentInfo.AgentDescription, AgentInfo = agentInfo };

			foreach (AgentInfo agentInfo in manager.GetAllAgents(Manager.AgentType.ModuleConverters).GroupBy(a => a.AgentId).Select(g => g.First()))
				yield return new AgentListInfo { Id = agentInfo.AgentId, Name = agentInfo.AgentName, Description = agentInfo.AgentDescription, AgentInfo = agentInfo };
		}



		/********************************************************************/
		/// <summary>
		/// Return the IDs of the agents in use if any
		/// </summary>
		/********************************************************************/
		protected override Guid[] GetAgentIdsInUse(ModuleHandler handler)
		{
			if (handler.StaticModuleInformation.ConverterAgentInfo != null)
				return new [] { handler.StaticModuleInformation.PlayerAgentInfo.AgentId, handler.StaticModuleInformation.ConverterAgentInfo.AgentId };

			return new [] { handler.StaticModuleInformation.PlayerAgentInfo.AgentId };
		}



		/********************************************************************/
		/// <summary>
		/// Return the agents to enable/disable for the given agent
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<AgentInfo> GetAgentCollection(AgentListInfo agentListInfo)
		{
			return manager.GetAllTypes(agentListInfo.Id);
		}
	}
}
