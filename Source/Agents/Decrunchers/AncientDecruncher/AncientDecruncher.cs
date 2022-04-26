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
using Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Formats;
using Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Formats.Xpk;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

// This is needed to uniquely identify this agent
[assembly: Guid("AFDBEE3F-E5A6-4255-8A68-89D876BCE943")]

namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class AncientDecruncher : AgentBase
	{
		private static readonly Guid agent1Id = Guid.Parse("C7165383-9774-4297-8168-9FACE978EFA3");
		private static readonly Guid agent2Id = Guid.Parse("15166B32-0EFF-49F3-93CC-DCB8F6D9C23C");
		private static readonly Guid agent3Id = Guid.Parse("911FDA93-928F-4A93-9370-347C7C442717");
		private static readonly Guid agent4Id = Guid.Parse("3E4DCDC1-2A25-4D0C-A8C7-17F489F6DAD3");
		private static readonly Guid agent5Id = Guid.Parse("890D80F6-CFD5-4253-9557-751847AD8E3A");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_ANC_NAME;



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
					new AgentSupportInfo(Resources.IDS_ANC_NAME_AGENT1, Resources.IDS_ANC_DESCRIPTION_AGENT1, agent1Id),
					new AgentSupportInfo(Resources.IDS_ANC_NAME_AGENT2, Resources.IDS_ANC_DESCRIPTION_AGENT2, agent2Id),
					new AgentSupportInfo(Resources.IDS_ANC_NAME_AGENT3, Resources.IDS_ANC_DESCRIPTION_AGENT3, agent3Id),
					new AgentSupportInfo(Resources.IDS_ANC_NAME_AGENT4, Resources.IDS_ANC_DESCRIPTION_AGENT4, agent4Id),
					new AgentSupportInfo(Resources.IDS_ANC_NAME_AGENT5, Resources.IDS_ANC_DESCRIPTION_AGENT5, agent5Id)
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
			if (typeId == agent1Id)
				return new AncientDecruncherWorker_PowerPacker(Resources.IDS_ANC_NAME_AGENT1);

			if (typeId == agent2Id)
				return new AncientDecruncherWorker_Xpk_Sqsh(Resources.IDS_ANC_NAME_AGENT2);

			if (typeId == agent3Id)
				return new AncientDecruncherWorker_Mmcmp(Resources.IDS_ANC_NAME_AGENT3);

			if (typeId == agent4Id)
				return new AncientDecruncherWorker_Xpk_Bzp2(Resources.IDS_ANC_NAME_AGENT4);

			if (typeId == agent5Id)
				return new AncientDecruncherWorker_Xpk_Blzw(Resources.IDS_ANC_NAME_AGENT5);

			return null;
		}
		#endregion
	}
}
