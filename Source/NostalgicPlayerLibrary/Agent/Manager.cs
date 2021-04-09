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
			SampleConverters,

			/// <summary>
			/// Converters that can convert from one module format to another
			/// </summary>
			ModuleConverters,

			/// <summary>
			/// Show what is playing in a window
			/// </summary>
			Visuals
		}

		private class AgentLoadInfo
		{
			public AssemblyLoadContext LoadContext;
			public string FileName;
			public AgentInfo[] AgentInfo;
		}

		private class AgentSettingsLoadInfo
		{
			public AssemblyLoadContext LoadContext;
			public string FileName;
			public IAgentSettings Settings;
		}

		private readonly object loadListLock = new object();
		private readonly Dictionary<Guid, AgentLoadInfo> loadedAgentsByAgentId = new Dictionary<Guid, AgentLoadInfo>();
		private readonly Dictionary<AgentType, AgentInfo[]> loadedAgentsByAgentType = new Dictionary<AgentType, AgentInfo[]>();

		private readonly Dictionary<Guid, AgentSettingsLoadInfo> settingAgents = new Dictionary<Guid, AgentSettingsLoadInfo>();

		private readonly List<IVisualAgent> registeredVisualAgents = new List<IVisualAgent>();

		/********************************************************************/
		/// <summary>
		/// Will load all available agents into memory
		/// </summary>
		/********************************************************************/
		public void LoadAllAgents()
		{
			Dictionary<AgentType, List<IAgent>> agentsNotInitializedYet = new Dictionary<AgentType, List<IAgent>>();

			// First load all the agents
			foreach (AgentType agentType in Enum.GetValues(typeof(AgentType)))
			{
				List<IAgent> notInitializedYet = new List<IAgent>();
				LoadSpecificAgents(agentType, notInitializedYet);

				agentsNotInitializedYet[agentType] = notInitializedYet;
			}

			// Now check to see if any of the agents need to know which other
			// agents that has been loaded
			TellAgentInfo(agentsNotInitializedYet);
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
		/// Get the setting agent with the ID given
		/// </summary>
		/********************************************************************/
		public IAgentSettings GetSettingAgent(Guid settingAgentId)
		{
			lock (loadListLock)
			{
				if (settingAgents.TryGetValue(settingAgentId, out AgentSettingsLoadInfo loadInfo))
					return loadInfo.Settings;
			}

			return null;
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



		/********************************************************************/
		/// <summary>
		/// Register the visual so the rest of the system is aware that it
		/// is open and need updates
		/// </summary>
		/********************************************************************/
		public void RegisterVisualAgent(IVisualAgent visualAgent)
		{
			lock (registeredVisualAgents)
			{
				registeredVisualAgents.Add(visualAgent);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Unregister the visual, so it wont get updates anymore
		/// </summary>
		/********************************************************************/
		public void UnregisterVisualAgent(IVisualAgent visualAgent)
		{
			lock (registeredVisualAgents)
			{
				registeredVisualAgents.Remove(visualAgent);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return all registered visual agents
		/// </summary>
		/********************************************************************/
		public IEnumerable<IVisualAgent> GetRegisteredVisualAgent()
		{
			lock (registeredVisualAgents)
			{
				foreach (IVisualAgent visualAgent in registeredVisualAgents)
					yield return visualAgent;
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will load all available agents of the specific type into memory
		/// </summary>
		/********************************************************************/
		private void LoadSpecificAgents(AgentType agentType, List<IAgent> agentsNotInitializedYet)
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

							loadContext.Resolving += LoadContext_Resolving;

							// Find all agent implementations
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
											typesInAgent.Add(BuildAgentInfo(agent, info));
									}
									else
									{
										// If null is returned, it means it is not possible to tell about the types yet. This is used
										// e.g. by the sample player, because it need to know which sample converters are available first
										agentsNotInitializedYet.Add(agent);
									}

									loadedAgentsByAgentId[agent.AgentId] = new AgentLoadInfo { LoadContext = loadContext, FileName = file, AgentInfo = typesInAgent.ToArray() };
									loadedAgents.AddRange(typesInAgent);
								}
								catch (Exception)
								{
									// If an exception is thrown, just ignore it and skip the file
								}
							}

							// Find all settings
							foreach (Type t in agentAssembly.GetTypes().Where(t => typeof(IAgentSettings).IsAssignableFrom(t)))
							{
								try
								{
									IAgentSettings agentSettings = (IAgentSettings)Activator.CreateInstance(t);

									// Check the NostalgicPlayer version the agent is compiled against
									if (agentSettings.NostalgicPlayerVersion != IAgent.NostalgicPlayer_Current_Version)
									{
										Console.WriteLine($"Agent {file} is not compiled against the current version of NostalgicPlayer and is therefore skipped");
										continue;
									}

									settingAgents[agentSettings.SettingAgentId] = new AgentSettingsLoadInfo { LoadContext = loadContext, FileName = file, Settings = agentSettings };
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
		/// Build agent information structure
		/// </summary>
		/********************************************************************/
		private AgentInfo BuildAgentInfo(IAgent agent, AgentSupportInfo info)
		{
			IAgentWorker worker = agent.CreateInstance(info.TypeId);

			return new AgentInfo(agent, agent.Name, info.Name, info.Description, agent.Version, info.TypeId, worker is IAgentSettingsRegistrar, worker is IAgentDisplay);
		}



		/********************************************************************/
		/// <summary>
		/// Check to see if any of the agents need to know which other agents
		/// that has been loaded
		/// </summary>
		/********************************************************************/
		private void TellAgentInfo(Dictionary<AgentType, List<IAgent>> agentsNotInitializedYet)
		{
			lock (loadListLock)
			{
				// First take any missing agents first
				foreach (KeyValuePair<AgentType, List<IAgent>> pair in agentsNotInitializedYet)
				{
					List<AgentInfo> loadedAgents = new List<AgentInfo>();

					for (int i = pair.Value.Count - 1; i >= 0; i--)
					{
						IAgent agent = pair.Value[i];

						if (agent is IWantOutputAgents wantOutputAgents)
							wantOutputAgents.SetOutputInfo(loadedAgentsByAgentType[AgentType.Output]);

						if (agent is IWantSampleConverterAgents wantSampleConverterAgents)
							wantSampleConverterAgents.SetSampleConverterInfo(loadedAgentsByAgentType[AgentType.SampleConverters]);

						List<AgentInfo> typesInAgent = new List<AgentInfo>();

						AgentSupportInfo[] supportInfo = agent.AgentInformation;
						if (supportInfo != null)
						{
							foreach (AgentSupportInfo info in supportInfo)
								typesInAgent.Add(BuildAgentInfo(agent, info));
						}

						loadedAgentsByAgentId[agent.AgentId].AgentInfo = typesInAgent.ToArray();
						loadedAgents.AddRange(typesInAgent);
					}

					if (loadedAgents.Count > 0)
						loadedAgentsByAgentType[pair.Key] = loadedAgentsByAgentType[pair.Key].Concat(loadedAgents).ToArray();
				}

				foreach (KeyValuePair<AgentType, AgentInfo[]> pair in loadedAgentsByAgentType)
				{
					List<IAgent> takenAgents = agentsNotInitializedYet[pair.Key];

					foreach (AgentInfo agentInfo in pair.Value)
					{
						// Check if we have already taken this agent in the above loop
						if (!takenAgents.Contains(agentInfo.Agent))
						{
							if (agentInfo.Agent is IWantOutputAgents wantOutputAgents)
								wantOutputAgents.SetOutputInfo(loadedAgentsByAgentType[AgentType.Output]);

							if (agentInfo.Agent is IWantSampleConverterAgents wantSampleConverterAgents)
								wantSampleConverterAgents.SetSampleConverterInfo(loadedAgentsByAgentType[AgentType.SampleConverters]);
						}
					}
				}

				foreach (AgentSettingsLoadInfo loadInfo in settingAgents.Values)
				{
					if (loadInfo.Settings is IWantOutputAgents wantOutputAgents)
						wantOutputAgents.SetOutputInfo(loadedAgentsByAgentType[AgentType.Output]);

					if (loadInfo.Settings is IWantSampleConverterAgents wantSampleConverterAgents)
						wantSampleConverterAgents.SetSampleConverterInfo(loadedAgentsByAgentType[AgentType.SampleConverters]);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called every time an agent want to load an external assembly.
		/// 
		/// This method will try to look at the agents local folder to find
		/// the needed assembly
		/// </summary>
		/********************************************************************/
		private Assembly LoadContext_Resolving(AssemblyLoadContext arg1, AssemblyName arg2)
		{
			Assembly ass = arg1.Assemblies.FirstOrDefault();
			if (ass != null)
			{
				string localPath = Path.GetDirectoryName(ass.Location);
				string assemblyPath = Path.Combine(localPath, arg2.Name + ".dll");

				if (File.Exists(assemblyPath))
					return arg1.LoadFromAssemblyPath(assemblyPath);
			}

			return null;
		}
		#endregion
	}
}
