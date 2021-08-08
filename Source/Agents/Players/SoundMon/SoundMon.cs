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
[assembly: Guid("18DAA6F3-AD3B-4C3D-9F77-245467B989DE")]

namespace Polycode.NostalgicPlayer.Agent.Player.SoundMon
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class SoundMon : AgentBase
	{
		internal static readonly Guid Agent1Id = Guid.Parse("90C4EC36-531B-46FF-9FD3-BC5745AD75BF");
		internal static readonly Guid Agent2Id = Guid.Parse("F47863B0-DB7E-4DAD-90E5-97EFF54F4455");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_BP_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description => Resources.IDS_BP_DESCRIPTION;



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
					new AgentSupportInfo(Resources.IDS_BP_NAME_AGENT1, Resources.IDS_BP_DESCRIPTION_AGENT1, Agent1Id),
					new AgentSupportInfo(Resources.IDS_BP_NAME_AGENT2, Resources.IDS_BP_DESCRIPTION_AGENT2, Agent2Id)
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
			return new SoundMonWorker(typeId);
		}
		#endregion
	}
}
