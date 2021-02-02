/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Agent
{
	/// <summary>
	/// This class manage all available agents
	/// </summary>
	public class Manager
	{
		/// <summary>
		/// The different types of agents
		/// </summary>
		public enum AgentType
		{
			/// <summary>
			/// Output agents plays the actual sound
			/// </summary>
			Output,

			/// <summary>
			/// Player agents can parse and play a specific file format
			/// </summary>
			Players,

			/// <summary>
			/// Converters that can read and/or write samples
			/// </summary>
			SampleConverters
		}

		private class AgentLoadInfo
		{
			public AssemblyLoadContext LoadContext;
			public string FileName;
			public AgentInfo[] AgentInfo;
		}

		private readonly object loadListLock = new object();
		private readonly Dictionary<Guid, AgentLoadInfo> loadedAgentsByAgentId = new Dictionary<Guid, AgentLoadInfo>();
		private readonly Dictionary<AgentType, AgentInfo[]> loadedAgentsByAgentType = new Dictionary<AgentType, AgentInfo[]>();

		/********************************************************************/
		/// <summary>
		/// Will load all available agents into memory
		/// </summary>
		/********************************************************************/
		public void LoadAllAgents()
		{
			// First load all the agents
			foreach (AgentType agentType in Enum.GetValues(typeof(AgentType)))
				LoadSpecificAgents(agentType);

			// Now check to see if any of the agents need to know which other
			// agents that has been loaded
			TellAgentInfo();
		}



		/********************************************************************/
		/// <summary>
		/// Will return information about the given agent type
		/// </summary>
		/********************************************************************/
		public AgentInfo GetAgent(AgentType agentType, Guid typeId)
		{
			lock (loadListLock)
			{
				if (loadedAgentsByAgentType.TryGetValue(agentType, out AgentInfo[] list))
					return list.FirstOrDefault(a => a.TypeId == typeId);
			}

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Return all agents loaded
		/// </summary>
		/********************************************************************/
		public IEnumerable<AgentInfo> GetAllAgents()
		{
			lock (loadListLock)
			{
				foreach (AgentInfo[] agentList in loadedAgentsByAgentType.Values)
				{
					foreach (AgentInfo agentInfo in agentList)
						yield return agentInfo;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return all agents of the given type
		/// </summary>
		/********************************************************************/
		public AgentInfo[] GetAllAgents(AgentType agentType)
		{
			lock (loadListLock)
			{
				if (loadedAgentsByAgentType.TryGetValue(agentType, out AgentInfo[] list))
					return list;
			}

			return new AgentInfo[0];
		}



		/********************************************************************/
		/// <summary>
		/// Return all types supported by the given agent
		/// </summary>
		/********************************************************************/
		public AgentInfo[] GetAllTypes(Guid agentId)
		{
			lock (loadListLock)
			{
				if (loadedAgentsByAgentId.TryGetValue(agentId, out AgentLoadInfo loadInfo))
					return loadInfo.AgentInfo;
			}

			return new AgentInfo[0];
		}



		/********************************************************************/
		/// <summary>
		/// Will load the given agent into memory
		/// </summary>
		/********************************************************************/
		public void LoadAgent(Guid agentId)
		{
			lock (loadListLock)
			{
				if (loadedAgentsByAgentId.TryGetValue(agentId, out AgentLoadInfo loadInfo))
				{
					loadInfo.LoadContext = new AssemblyLoadContext(null, true);
					Assembly agentAssembly = loadInfo.LoadContext.LoadFromAssemblyPath(loadInfo.FileName);

					int i = 0;
					foreach (Type t in agentAssembly.GetTypes().Where(t => typeof(IAgent).IsAssignableFrom(t)))
					{
						try
						{
							loadInfo.AgentInfo[i].Agent = (IAgent)Activator.CreateInstance(t);
						}
						catch (Exception)
						{
							// If an exception is thrown, just ignore it and skip the file
						}

						i++;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will flush the given agent from memory
		/// </summary>
		/********************************************************************/
		public void UnloadAgent(Guid agentId)
		{
			lock (loadListLock)
			{
				if (loadedAgentsByAgentId.TryGetValue(agentId, out AgentLoadInfo loadInfo))
				{
					foreach (AgentInfo agentInfo in loadInfo.AgentInfo)
					{
						agentInfo.Enabled = false;
						agentInfo.Agent = null;
					}

					loadInfo.LoadContext.Unload();
					loadInfo.LoadContext = null;
				}
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will load all available agents of the specific type into memory
		/// </summary>
		/********************************************************************/
		private void LoadSpecificAgents(AgentType agentType)
		{
			// Build the search directory
			string searchDirectory = Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Agents"), agentType.ToString());

			if (Directory.Exists(searchDirectory))
			{
				// Load the agents
				lock (loadListLock)
				{
					List<AgentInfo> loadedAgents = new List<AgentInfo>();

					// Try to load all library files in the search directory and
					// find the agent interfaces
					foreach (string file in Directory.GetFiles(searchDirectory, "*.dll"))
					{
						try
						{
							// Load the assembly into memory in its own context
							AssemblyLoadContext loadContext = new AssemblyLoadContext(null, true);
							Assembly agentAssembly = loadContext.LoadFromAssemblyPath(file);

							foreach (Type t in agentAssembly.GetTypes().Where(t => typeof(IAgent).IsAssignableFrom(t)))
							{
								try
								{
									IAgent agent = (IAgent)Activator.CreateInstance(t);

									// Check the NostalgicPlayer version the agent is compiled against
									if (agent.NostalgicPlayerVersion != IAgent.NostalgicPlayer_Current_Version)
									{
										Console.WriteLine($"Agent {file} is not compiled against the current version of NostalgicPlayer and is therefore skipped");
										continue;
									}

									List<AgentInfo> typesInAgent = new List<AgentInfo>();

									AgentSupportInfo[] supportInfo = agent.AgentInformation;
									if (supportInfo != null)
									{
										foreach (AgentSupportInfo info in supportInfo)
										{
											IAgentWorker worker = agent.CreateInstance(info.TypeId);

											typesInAgent.Add(new AgentInfo(agent, agent.Name, info.Name, info.Description, agent.Version, info.TypeId, worker is IAgentSettings));
										}
									}

									loadedAgentsByAgentId[agent.AgentId] = new AgentLoadInfo { LoadContext = loadContext, FileName = file, AgentInfo = typesInAgent.ToArray() };
									loadedAgents.AddRange(typesInAgent);
								}
								catch (Exception)
								{
									// If an exception is thrown, just ignore it and skip the file
								}
							}
						}
						catch (Exception)
						{
							// If an exception is thrown, just ignore it and skip the file
						}
					}

					loadedAgentsByAgentType[agentType] = loadedAgents.ToArray();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Check to see if any of the agents need to know which other agents
		/// that has been loaded
		/// </summary>
		/********************************************************************/
		private void TellAgentInfo()
		{
			lock (loadListLock)
			{
				foreach (AgentInfo[] agents in loadedAgentsByAgentType.Values)
				{
					foreach (AgentInfo agentInfo in agents)
					{
						if (agentInfo.Agent is IWantOutputAgents wantOutputAgents)
							wantOutputAgents.SetOutputInfo(loadedAgentsByAgentType[AgentType.Output]);

						if (agentInfo.Agent is IWantSampleConverterAgents wantSampleConverterAgents)
							wantSampleConverterAgents.SetSampleConverterInfo(loadedAgentsByAgentType[AgentType.SampleConverters]);
					}
				}
			}
		}
		#endregion
	}
}
