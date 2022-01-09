/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Linq;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

// This is needed to uniquely identify this agent
[assembly: Guid("B3736A57-2C72-4756-A52A-9332BBDB1A17")]

namespace Polycode.NostalgicPlayer.Agent.Output.DiskSaver
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class DiskSaver : AgentBase, IWantOutputAgents, IWantSampleConverterAgents
	{
		internal static readonly Guid Agent1Id = Guid.Parse("53BE1DF8-83E1-4616-81DE-2ED537CF7D5A");

		private AgentInfo[] loadedOutputAgents;
		private AgentInfo[] loadedSampleConverterAgents;

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns all the formats/types this agent supports
		/// </summary>
		/********************************************************************/
		public override AgentSupportInfo[] AgentInformation
		{
			get
			{
				return new AgentSupportInfo[]
				{
					new AgentSupportInfo(Resources.IDS_NAME_AGENT1, Resources.IDS_DESCRIPTION_AGENT1, Agent1Id)
				};
			}
		}



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance(Guid typeId)
		{
			return new DiskSaverWorker(loadedOutputAgents, loadedSampleConverterAgents);
		}
		#endregion

		#region IWantOutputAgents implementation
		/********************************************************************/
		/// <summary>
		/// Gives a list with all loaded output agents
		/// </summary>
		/********************************************************************/
		public void SetOutputInfo(AgentInfo[] agents)
		{
			loadedOutputAgents = agents.Where(a => a.TypeId != Agent1Id).ToArray();
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
			loadedSampleConverterAgents = agents.Where(a => a.Agent.CreateInstance(a.TypeId) is ISampleSaverAgent).ToArray();
		}
		#endregion
	}
}
