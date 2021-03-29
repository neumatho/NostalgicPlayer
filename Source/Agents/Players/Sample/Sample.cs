/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Linq;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

// This is needed to uniquely identify this agent
[assembly: Guid("10C33C5C-78F6-4649-B59E-7971832EFB23")]

namespace Polycode.NostalgicPlayer.Agent.Player.Sample
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class Sample : AgentBase, IWantSampleConverterAgents
	{
		private AgentInfo[] loadedSampleConverterAgents;

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name
		{
			get
			{
				return Resources.IDS_SAMPLE_NAME;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Returns all the formats/types this agent supports
		/// </summary>
		/********************************************************************/
		public override AgentSupportInfo[] AgentInformation
		{
			get
			{
				if (loadedSampleConverterAgents == null)
					return null;

				return loadedSampleConverterAgents.Select(agentInfo => new AgentSupportInfo(agentInfo.TypeName, agentInfo.Description, agentInfo.TypeId)).ToArray();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance(Guid typeId)
		{
			return new SampleWorker(loadedSampleConverterAgents.First(agentInfo => agentInfo.TypeId == typeId));
		}
		#endregion

		#region IWantSampleConverterAgents implementation
		/********************************************************************/
		/// <summary>
		/// Gives a list with all loaded sample converter agents
		/// </summary>
		/********************************************************************/
		public void SetSampleConverterInfo(AgentInfo[] agents)
		{
			loadedSampleConverterAgents = agents.Where(a => a.Agent.CreateInstance(a.TypeId) is ISampleLoaderAgent).ToArray();
		}
		#endregion
	}
}
