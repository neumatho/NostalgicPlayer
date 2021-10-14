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
[assembly: Guid("0B121014-D09D-4FD7-A5A3-594261E8D692")]

namespace Polycode.NostalgicPlayer.Agent.Player.Mpg123
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class Mpg123 : AgentBase
	{
		internal static readonly Guid Agent1Id = Guid.Parse("87BBC337-81DA-4580-9D4E-B4CAFF366231");
		internal static readonly Guid Agent2Id = Guid.Parse("297FB32C-C784-4AEA-930B-7F479E873D72");
		internal static readonly Guid Agent3Id = Guid.Parse("D1E892D8-423B-4B4E-B0AA-4CA52FABD2AE");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description => Resources.IDS_DESCRIPTION;



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
					new AgentSupportInfo(Resources.IDS_MPG_NAME_AGENT1, Resources.IDS_MPG_DESCRIPTION_AGENT1, Agent1Id),
					new AgentSupportInfo(Resources.IDS_MPG_NAME_AGENT2, Resources.IDS_MPG_DESCRIPTION_AGENT2, Agent2Id),
					new AgentSupportInfo(Resources.IDS_MPG_NAME_AGENT3, Resources.IDS_MPG_DESCRIPTION_AGENT3, Agent3Id)
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
			return new Mpg123Worker(typeId);
		}
		#endregion
	}
}
