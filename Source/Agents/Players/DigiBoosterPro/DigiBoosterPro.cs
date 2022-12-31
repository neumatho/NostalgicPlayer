/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

// This is needed to uniquely identify this agent
[assembly: Guid("7C28DF33-0A14-4688-8C20-7633A7F08A42")]

namespace Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class DigiBoosterPro : AgentBase
	{
		internal static readonly Guid Agent1Id = Guid.Parse("18C1946D-1084-4968-8A07-EDA2DFA288FB");
		internal static readonly Guid Agent2Id = Guid.Parse("EFF8BEF7-3B37-48FD-9968-22AE3B066F33");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_DBM_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description => Resources.IDS_DBM_DESCRIPTION;



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
					new AgentSupportInfo(Resources.IDS_DBM_NAME_AGENT1, Resources.IDS_DBM_DESCRIPTION_AGENT1, Agent1Id),
					new AgentSupportInfo(Resources.IDS_DBM_NAME_AGENT2, Resources.IDS_DBM_DESCRIPTION_AGENT2, Agent2Id)
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
			return new DigiBoosterProWorker(typeId);
		}
		#endregion
	}
}
