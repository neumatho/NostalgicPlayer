/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of RetroPlayer is keep. See the LICENSE file for more information. */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / RetroPlayer team.                         */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Polycode.RetroPlayer.RetroPlayerKit.Interfaces;

namespace Polycode.RetroPlayer.RetroPlayerLibrary.Agent
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
			Players
		}

		private readonly Dictionary<AgentType, IAgent[]> loadedAgents = new Dictionary<AgentType, IAgent[]>();

		/********************************************************************/
		/// <summary>
		/// Will load all available agents into memory
		/// </summary>
		/********************************************************************/
		public void LoadAllAgents()
		{
			foreach (AgentType agentType in Enum.GetValues(typeof(AgentType)))
				LoadSpecificAgents(agentType);
		}



		/********************************************************************/
		/// <summary>
		/// Will load all available agents of the specific type into memory
		/// </summary>
		/********************************************************************/
		public void LoadSpecificAgents(AgentType agentType)
		{
			// Build the search directory
			string searchDirectory = Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Agents"), agentType.ToString());

			// Load the agents
			List<IAgent> agents = LoadAgents(searchDirectory);
			if (agents.Count > 0)
			{
				lock (loadedAgents)
				{
					loadedAgents[agentType] = agents.ToArray();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will create an instance of the given agent if found
		/// </summary>
		/********************************************************************/
		public IAgent GetAgent(AgentType agentType, Guid id)
		{
			lock (loadedAgents)
			{
				if (loadedAgents.TryGetValue(agentType, out IAgent[] list))
				{
					foreach (IAgent agent in list)
					{
						if (agent.Id == id)
						{
							// Found the agent, so return it
							return agent;
						}
					}
				}
			}

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Return all agents of the given type
		/// </summary>
		/********************************************************************/
		public IAgent[] GetAllAgents(AgentType agentType)
		{
			lock (loadedAgents)
			{
				if (loadedAgents.TryGetValue(agentType, out IAgent[] list))
					return list;
			}

			return new IAgent[0];
		}



		/********************************************************************/
		/// <summary>
		/// Will load all available agents of the specific type into memory
		/// </summary>
		/********************************************************************/
		private List<IAgent> LoadAgents(string searchDirectory)
		{
			List<IAgent> foundAgents = new List<IAgent>();

			// Try to load all library files in the search directory and
			// find the agent interfaces
			foreach (string file in Directory.GetFiles(searchDirectory, "*.dll"))
			{
				try
				{
					Assembly agentAssembly = Assembly.LoadFile(file);

					foreach (Type t in agentAssembly.GetTypes().Where(t => typeof(IAgent).IsAssignableFrom(t)))
						foundAgents.Add((IAgent)Activator.CreateInstance(t));
				}
				catch (Exception)
				{
					// If an exception is thrown, just ignore it and skip the file
				}
			}

			return foundAgents;
		}
	}
}
