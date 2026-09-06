/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Library.Agent;
using Polycode.NostalgicPlayer.Library.Containers;

namespace Polycode.NostalgicPlayer.Library.Test.Helpers
{
	/// <summary>
	/// Agent manager which only knows about the agents given to it. Only the
	/// methods the loader calls are implemented
	/// </summary>
	internal class TestAgentManager : IAgentManager
	{
		private readonly Dictionary<AgentType, AgentInfo[]> agents;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public TestAgentManager(AgentInfo[] moduleConverters, AgentInfo[] players)
		{
			agents = new Dictionary<AgentType, AgentInfo[]>
			{
				{ AgentType.ModuleConverters, moduleConverters },
				{ AgentType.Players, players }
			};
		}



		/********************************************************************/
		/// <summary>
		/// Will return information about the given agent type
		/// </summary>
		/********************************************************************/
		public AgentInfo GetAgent(AgentType agentType, Guid typeId)
		{
			return GetAllAgents(agentType).FirstOrDefault(x => x.TypeId == typeId);
		}



		/********************************************************************/
		/// <summary>
		/// Return all agents of the given type
		/// </summary>
		/********************************************************************/
		public AgentInfo[] GetAllAgents(AgentType agentType)
		{
			return agents.TryGetValue(agentType, out AgentInfo[] agentInfos) ? agentInfos : [];
		}



		/********************************************************************/
		/// <summary>
		/// Return all agents loaded
		/// </summary>
		/********************************************************************/
		public IEnumerable<AgentInfo> GetAllAgents()
		{
			return agents.Values.SelectMany(x => x);
		}

		#region Not used by the tests
		/********************************************************************/
		/// <summary>
		/// Will load all available agents into memory
		/// </summary>
		/********************************************************************/
		public void LoadAllAgents()
		{
			throw new NotSupportedException();
		}



		/********************************************************************/
		/// <summary>
		/// Will load all available agents into memory and call the callback
		/// for each agent loaded
		/// </summary>
		/********************************************************************/
		public void LoadAllAgents(IAgentManager.LoadAgentProgressHandler callback)
		{
			throw new NotSupportedException();
		}



		/********************************************************************/
		/// <summary>
		/// Return all types supported by the given agent
		/// </summary>
		/********************************************************************/
		public AgentInfo[] GetAllTypes(Guid agentId)
		{
			throw new NotSupportedException();
		}



		/********************************************************************/
		/// <summary>
		/// Get the setting agent with the ID given
		/// </summary>
		/********************************************************************/
		public IAgentSettings GetSettingAgent(Guid settingAgentId)
		{
			throw new NotSupportedException();
		}



		/********************************************************************/
		/// <summary>
		/// Will load the given agent into memory
		/// </summary>
		/********************************************************************/
		public void LoadAgent(Guid agentId)
		{
			throw new NotSupportedException();
		}



		/********************************************************************/
		/// <summary>
		/// Will flush the given agent from memory
		/// </summary>
		/********************************************************************/
		public void UnloadAgent(Guid agentId)
		{
			throw new NotSupportedException();
		}



		/********************************************************************/
		/// <summary>
		/// Register the visual so the rest of the system is aware that it
		/// is open and need updates
		/// </summary>
		/********************************************************************/
		public void RegisterVisualAgent(IVisualAgent visualAgent)
		{
			throw new NotSupportedException();
		}



		/********************************************************************/
		/// <summary>
		/// Unregister the visual, so it wont get updates anymore
		/// </summary>
		/********************************************************************/
		public void UnregisterVisualAgent(IVisualAgent visualAgent)
		{
			throw new NotSupportedException();
		}



		/********************************************************************/
		/// <summary>
		/// Return all registered visual agents
		/// </summary>
		/********************************************************************/
		public IEnumerable<IVisualAgent> GetRegisteredVisualAgent()
		{
			throw new NotSupportedException();
		}
		#endregion
	}
}
