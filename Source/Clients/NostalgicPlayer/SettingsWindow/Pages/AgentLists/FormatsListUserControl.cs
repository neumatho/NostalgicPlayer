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
	/// Handle the formats tab
	/// </summary>
	public class FormatsListUserControl : AgentsListUserControl
	{
		private readonly IAgentManager agentManager;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public FormatsListUserControl()
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
			foreach (AgentInfo agentInfo in agentManager.GetAllAgents(AgentType.Players).Where(agentInfo => !string.IsNullOrEmpty(agentInfo.TypeName)))
				yield return new AgentListInfo { Id = agentInfo.TypeId, Name = agentInfo.TypeName, Description = agentInfo.TypeDescription, AgentInfo = agentInfo };

			foreach (AgentInfo agentInfo in agentManager.GetAllAgents(AgentType.ModuleConverters).Where(agentInfo => !string.IsNullOrEmpty(agentInfo.TypeName)))
				yield return new AgentListInfo { Id = agentInfo.TypeId, Name = agentInfo.TypeName, Description = agentInfo.TypeDescription, AgentInfo = agentInfo };
		}



		/********************************************************************/
		/// <summary>
		/// Return the IDs of the agents in use if any
		/// </summary>
		/********************************************************************/
		protected override Guid[] GetAgentIdsInUse(IModuleHandlerService modHandler)
		{
			if (modHandler.StaticModuleInformation.ConverterAgentInfo != null)
				return [ modHandler.StaticModuleInformation.ConverterAgentInfo.TypeId ];

			return [ modHandler.StaticModuleInformation.PlayerAgentInfo.TypeId ];
		}
	}
}
