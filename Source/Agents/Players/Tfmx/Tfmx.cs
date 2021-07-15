/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

// This is needed to uniquely identify this agent
[assembly: Guid("C5673A24-1C37-49AB-B919-6EAFA9654EF8")]

namespace Polycode.NostalgicPlayer.Agent.Player.Tfmx
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class Tfmx : AgentBase
	{
		internal static readonly Guid Agent1Id = Guid.Parse("E41B630B-0C68-41D5-9D09-2771581A0D22");
		internal static readonly Guid Agent2Id = Guid.Parse("E9333E11-4CD4-4631-B758-507C4607AB8A");
		internal static readonly Guid Agent3Id = Guid.Parse("AFB99395-AA4B-4F35-BA5E-4B1513615B51");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_TFMX_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description => Resources.IDS_TFMX_DESCRIPTION;



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
					new AgentSupportInfo(Resources.IDS_TFMX_NAME_AGENT1, Resources.IDS_TFMX_DESCRIPTION_AGENT1, Agent1Id),
					new AgentSupportInfo(Resources.IDS_TFMX_NAME_AGENT2, Resources.IDS_TFMX_DESCRIPTION_AGENT2, Agent2Id),
					new AgentSupportInfo(Resources.IDS_TFMX_NAME_AGENT3, Resources.IDS_TFMX_DESCRIPTION_AGENT3, Agent3Id)
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
			return new TfmxWorker(typeId);
		}
		#endregion
	}
}
