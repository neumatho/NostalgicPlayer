/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

// This is needed to uniquely identify this agent
[assembly: Guid("465B504C-44CE-4ECE-BD75-A36F80FB7BBF")]

namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class OctaMed : AgentBase
	{
		internal static readonly Guid Agent1Id = Guid.Parse("0DCA54F5-C418-47CD-8FCB-1A3CAEA1DD94");
		internal static readonly Guid Agent2Id = Guid.Parse("669C0F1F-C2D0-41F2-9005-6BD64D02F92A");
		internal static readonly Guid Agent3Id = Guid.Parse("BDC291A4-5D79-4FA5-9050-16786DE0B6BB");
		internal static readonly Guid Agent4Id = Guid.Parse("55DC3437-30F8-4AD6-A70D-83EE29059F5B");
		internal static readonly Guid Agent5Id = Guid.Parse("1185BBC2-85D7-4AF7-9B55-3C90A778765C");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_MED_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description => Resources.IDS_MED_DESCRIPTION;



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
					new AgentSupportInfo(Resources.IDS_MED_NAME_AGENT1, Resources.IDS_MED_DESCRIPTION_AGENT1, Agent1Id),
					new AgentSupportInfo(Resources.IDS_MED_NAME_AGENT2, Resources.IDS_MED_DESCRIPTION_AGENT2, Agent2Id),
					new AgentSupportInfo(Resources.IDS_MED_NAME_AGENT3, Resources.IDS_MED_DESCRIPTION_AGENT3, Agent3Id),
					new AgentSupportInfo(Resources.IDS_MED_NAME_AGENT4, Resources.IDS_MED_DESCRIPTION_AGENT4, Agent4Id),
					new AgentSupportInfo(Resources.IDS_MED_NAME_AGENT5, Resources.IDS_MED_DESCRIPTION_AGENT5, Agent5Id)
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
			return new OctaMedWorker(typeId);
		}
		#endregion
	}
}
