/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Library.Containers;

namespace Polycode.NostalgicPlayer.Library.Agent
{
	/// <summary>
	/// Manage all available agents
	/// </summary>
	public interface IAgentManager
	{
		/// <summary>
		/// Callback used when loading agents to inform about the progress
		/// </summary>
		delegate void LoadAgentProgress(int numberLoaded, int totalNumbers);

		/// <summary>
		/// Will load all available agents into memory
		/// </summary>
		void LoadAllAgents();

		/// <summary>
		/// Will load all available agents into memory and call the callback
		/// for each agent loaded
		/// </summary>
		void LoadAllAgents(LoadAgentProgress callback);

		/// <summary>
		/// Will return information about the given agent type
		/// </summary>
		AgentInfo GetAgent(AgentType agentType, Guid typeId);

		/// <summary>
		/// Return all agents loaded
		/// </summary>
		IEnumerable<AgentInfo> GetAllAgents();

		/// <summary>
		/// Return all agents of the given type
		/// </summary>
		AgentInfo[] GetAllAgents(AgentType agentType);

		/// <summary>
		/// Return all types supported by the given agent
		/// </summary>
		AgentInfo[] GetAllTypes(Guid agentId);

		/// <summary>
		/// Get the setting agent with the ID given
		/// </summary>
		IAgentSettings GetSettingAgent(Guid settingAgentId);

		/// <summary>
		/// Will load the given agent into memory
		/// </summary>
		void LoadAgent(Guid agentId);

		/// <summary>
		/// Will flush the given agent from memory
		/// </summary>
		void UnloadAgent(Guid agentId);

		/// <summary>
		/// Register the visual so the rest of the system is aware that it
		/// is open and need updates
		/// </summary>
		void RegisterVisualAgent(IVisualAgent visualAgent);

		/// <summary>
		/// Unregister the visual, so it wont get updates anymore
		/// </summary>
		void UnregisterVisualAgent(IVisualAgent visualAgent);

		/// <summary>
		/// Return all registered visual agents
		/// </summary>
		IEnumerable<IVisualAgent> GetRegisteredVisualAgent();
	}
}
