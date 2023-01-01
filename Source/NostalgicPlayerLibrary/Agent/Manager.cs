/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
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
			Visuals,

			/// <summary>
			/// Can decrunch a single file
			/// </summary>
			FileDecrunchers,

			/// <summary>
			/// Can decrunch archive files
			/// </summary>
			ArchiveDecrunchers
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

		private readonly object listLock = new object();
		private readonly Dictionary<Guid, AgentLoadInfo> agentsByAgentId = new Dictionary<Guid, AgentLoadInfo>();
		private readonly Dictionary<AgentType, List<AgentInfo>> agentsByAgentType = new Dictionary<AgentType, List<AgentInfo>>();

		private readonly Dictionary<Guid, AgentSettingsLoadInfo> settingAgents = new Dictionary<Guid, AgentSettingsLoadInfo>();

		private readonly List<IVisualAgent> registeredVisualAgents = new List<IVisualAgent>();

		/********************************************************************/
		/// <summary>
		/// Will load all available agents into memory
		/// </summary>
		/********************************************************************/
		public void LoadAllAgents()
		{
			// Initialize the agent type dictionary
			foreach (AgentType agentType in Enum.GetValues(typeof(AgentType)))
				agentsByAgentType[agentType] = new List<AgentInfo>();

			// First load all the agents
			List<IAgent> agentsNotInitializedYet = new List<IAgent>();
			LoadAllAgents(agentsNotInitializedYet);

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
			lock (listLock)
			{
				return agentsByAgentType[agentType].FirstOrDefault(a => a.TypeId == typeId);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return all agents loaded
		/// </summary>
		/********************************************************************/
		public IEnumerable<AgentInfo> GetAllAgents()
		{
			lock (listLock)
			{
				foreach (List<AgentInfo> agentList in agentsByAgentType.Values)
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
			lock (listLock)
			{
				return agentsByAgentType[agentType].ToArray();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return all types supported by the given agent
		/// </summary>
		/********************************************************************/
		public AgentInfo[] GetAllTypes(Guid agentId)
		{
			lock (listLock)
			{
				if (agentsByAgentId.TryGetValue(agentId, out AgentLoadInfo loadInfo))
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
			lock (listLock)
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
			lock (listLock)
			{
				if (agentsByAgentId.TryGetValue(agentId, out AgentLoadInfo loadInfo))
				{
					loadInfo.LoadContext = new AssemblyLoadContext(null, true);
					Assembly agentAssembly = loadInfo.LoadContext.LoadFromAssemblyPath(loadInfo.FileName);

					Type type = agentAssembly.GetTypes().FirstOrDefault(t => typeof(IAgent).IsAssignableFrom(t));
					if (type != null)
					{
						try
						{
							IAgent agent = (IAgent)Activator.CreateInstance(type);

							foreach (AgentInfo agentInfo in loadInfo.AgentInfo)
								agentInfo.Agent = agent;

							TellAgent(agent);
						}
						catch (Exception)
						{
							// If an exception is thrown, just ignore it and skip the file
						}
					}

					foreach (Type t in agentAssembly.GetTypes().Where(t => typeof(IAgentSettings).IsAssignableFrom(t)))
					{
						try
						{
							IAgentSettings agentSettings = (IAgentSettings)Activator.CreateInstance(t);

							if (settingAgents.TryGetValue(agentSettings.SettingAgentId, out AgentSettingsLoadInfo settingsLoadInfo))
							{
								settingsLoadInfo.LoadContext = loadInfo.LoadContext;
								settingsLoadInfo.Settings = agentSettings;

								TellSettingsAgent(settingsLoadInfo);
							}
						}
						catch (Exception)
						{
							// If an exception is thrown, just ignore it and skip the file
						}
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
			lock (listLock)
			{
				if (settingAgents.TryGetValue(agentId, out AgentSettingsLoadInfo settingsLoadInfo))
				{
					settingsLoadInfo.LoadContext = null;
					settingsLoadInfo.Settings = null;
				}

				if (agentsByAgentId.TryGetValue(agentId, out AgentLoadInfo loadInfo))
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
		/// Will load all available agents
		/// </summary>
		/********************************************************************/
		private void LoadAllAgents(List<IAgent> agentsNotInitializedYet)
		{
			// Build the search directory
			string searchDirectory = Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)));

			if (Directory.Exists(searchDirectory))
			{
				// Load the agents
				lock (listLock)
				{
					// Try to load all library files in the search directory and
					// find the agent interfaces
					foreach (string file in Directory.GetFiles(searchDirectory, "*.dll"))
					{
						if (!file.StartsWith("System.") && !file.StartsWith("Microsoft.") && !file.StartsWith("api-ms-"))
						{
							try
							{
								// Load the assembly into memory in its own context
								AssemblyLoadContext loadContext = new AssemblyLoadContext(null, true);
								Assembly agentAssembly = loadContext.LoadFromAssemblyPath(file);

								bool foundAnything = false;

								// Find all agent implementations
								Type type = agentAssembly.GetTypes().FirstOrDefault(t => typeof(IAgent).IsAssignableFrom(t));
								if (type != null)
								{
									try
									{
										IAgent agent = (IAgent)Activator.CreateInstance(type);

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

												// Find the type of agent and put it in the right list
												AgentType? agentType = FindAgentType(worker);
												if (agentType.HasValue)
												{
													List<AgentInfo> typesList = agentsByAgentType[agentType.Value];

													AgentInfo agentInfo = BuildAgentInfo(agent, worker, info);
													typesList.Add(agentInfo);
													typesInAgent.Add(agentInfo);
												}
											}
										}
										else
										{
											// If null is returned, it means it is not possible to tell about the types yet. This is used
											// e.g. by the sample player, because it need to know which sample converters are available first
											agentsNotInitializedYet.Add(agent);
										}

										agentsByAgentId[agent.AgentId] = new AgentLoadInfo { LoadContext = loadContext, FileName = file, AgentInfo = typesInAgent.ToArray() };
										foundAnything = true;
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
										foundAnything = true;
									}
									catch (Exception)
									{
										// If an exception is thrown, just ignore it and skip the file
									}
								}

								if (!foundAnything)
								{
									// Unload the assembly again
									loadContext.Unload();
								}
							}
							catch (Exception)
							{
								// If an exception is thrown, just ignore it and skip the file
							}
						}
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the agent type of the worker or null if not recognized
		/// </summary>
		/********************************************************************/
		private AgentType? FindAgentType(IAgentWorker worker)
		{
			if (worker is IOutputAgent)
				return AgentType.Output;

			if (worker is IPlayerAgent)
				return AgentType.Players;

			if ((worker is ISampleLoaderAgent) || (worker is ISampleSaverAgent))
				return AgentType.SampleConverters;

			if (worker is IModuleConverterAgent)
				return AgentType.ModuleConverters;

			if (worker is IVisualAgent)
				return AgentType.Visuals;

			if (worker is IFileDecruncherAgent)
				return AgentType.FileDecrunchers;

			if (worker is IArchiveDecruncherAgent)
				return AgentType.ArchiveDecrunchers;

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Build agent information structure
		/// </summary>
		/********************************************************************/
		private AgentInfo BuildAgentInfo(IAgent agent, IAgentWorker worker, AgentSupportInfo info)
		{
			return new AgentInfo(agent, info.Name, info.Description, agent.Version, info.TypeId, worker is IAgentSettingsRegistrar, worker is IAgentDisplay);
		}



		/********************************************************************/
		/// <summary>
		/// Check to see if any of the agents need to know which other agents
		/// that has been loaded
		/// </summary>
		/********************************************************************/
		private void TellAgentInfo(List<IAgent> agentsNotInitializedYet)
		{
			lock (listLock)
			{
				// First take any missing agents first
				foreach (IAgent agent in agentsNotInitializedYet)
				{
					TellAgent(agent);

					List<AgentInfo> typesInAgent = new List<AgentInfo>();

					AgentSupportInfo[] supportInfo = agent.AgentInformation;
					if (supportInfo != null)
					{
						foreach (AgentSupportInfo info in supportInfo)
						{
							IAgentWorker worker = agent.CreateInstance(info.TypeId);

							// Find the type of agent and put it in the right list
							AgentType? agentType = FindAgentType(worker);
							if (agentType.HasValue)
							{
								List<AgentInfo> typesList = agentsByAgentType[agentType.Value];

								AgentInfo agentInfo = BuildAgentInfo(agent, worker, info);
								typesList.Add(agentInfo);
								typesInAgent.Add(agentInfo);
							}
						}
					}

					agentsByAgentId[agent.AgentId].AgentInfo = typesInAgent.ToArray();
				}

				// Take all loaded agents and see if they need information for
				// any of the agents not initialized yet
				foreach (AgentInfo agentInfo in agentsByAgentId.Values.SelectMany(loadInfo => loadInfo.AgentInfo))
				{
					// Check if we have already taken this agent in the above loop
					if (!agentsNotInitializedYet.Contains(agentInfo.Agent))
						TellAgent(agentInfo.Agent);
				}

				foreach (AgentSettingsLoadInfo loadInfo in settingAgents.Values)
					TellSettingsAgent(loadInfo);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Tell agent about other agents
		/// </summary>
		/********************************************************************/
		private void TellAgent(IAgent agent)
		{
			if (agent is IWantOutputAgents wantOutputAgents)
				wantOutputAgents.SetOutputInfo(agentsByAgentType[AgentType.Output].ToArray());

			if (agent is IWantSampleConverterAgents wantSampleConverterAgents)
				wantSampleConverterAgents.SetSampleConverterInfo(agentsByAgentType[AgentType.SampleConverters].ToArray());
		}



		/********************************************************************/
		/// <summary>
		/// Tell settings agent about other agents
		/// </summary>
		/********************************************************************/
		private void TellSettingsAgent(AgentSettingsLoadInfo loadInfo)
		{
			if (loadInfo.Settings is IWantOutputAgents wantOutputAgents)
				wantOutputAgents.SetOutputInfo(agentsByAgentType[AgentType.Output].ToArray());

			if (loadInfo.Settings is IWantSampleConverterAgents wantSampleConverterAgents)
				wantSampleConverterAgents.SetSampleConverterInfo(agentsByAgentType[AgentType.SampleConverters].ToArray());
		}
		#endregion
	}
}
