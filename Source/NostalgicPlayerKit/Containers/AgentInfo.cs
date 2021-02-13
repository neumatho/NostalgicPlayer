/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	/// Holds information about a single format/type in an agent
	/// </summary>
	public class AgentInfo
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AgentInfo(IAgent agent, string agentName, string typeName, string description, Version version, Guid typeId, bool hasSettings, bool hasDisplay)
		{
			Agent = agent;
			AgentId = agent.AgentId;

			AgentName = agentName;
			TypeName = typeName;
			Description = description;
			Version = version;
			TypeId = typeId;

			HasSettings = hasSettings;
			HasDisplay = hasDisplay;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the agent instance
		/// </summary>
		/********************************************************************/
		public IAgent Agent
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the ID of the agent
		/// </summary>
		/********************************************************************/
		public Guid AgentId
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the name of the agent
		/// </summary>
		/********************************************************************/
		public string AgentName
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the name of the format/type
		/// </summary>
		/********************************************************************/
		public string TypeName
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the description of the format/type
		/// </summary>
		/********************************************************************/
		public string Description
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the version of the agent
		/// </summary>
		/********************************************************************/
		public Version Version
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the ID of the format/type
		/// </summary>
		/********************************************************************/
		public Guid TypeId
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Indicate if the agent has its own settings
		/// </summary>
		/********************************************************************/
		public bool HasSettings
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Indicate if the agent has some display window
		/// </summary>
		/********************************************************************/
		public bool HasDisplay
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Indicate if this type is enabled or disabled
		/// </summary>
		/********************************************************************/
		public bool Enabled
		{
			get; set;
		} = true;
	}
}
