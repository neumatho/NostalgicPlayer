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
using Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

// This is needed to uniquely identify this agent
[assembly: Guid("CB57AB91-0F2B-4A5E-B962-81897CF1FE52")]

namespace Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class ArchiveDecruncher : AgentBase
	{
		private static readonly Guid agent1Id = Guid.Parse("D791B0C0-C1D0-4CFD-9B58-67166BFBBC58");
		private static readonly Guid agent2Id = Guid.Parse("BC285D44-8412-47D9-89AC-D63ADB8EA006");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_ARD_NAME;



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
					new AgentSupportInfo(Resources.IDS_ARD_NAME_AGENT1, Resources.IDS_ARD_DESCRIPTION_AGENT1, agent1Id),
					new AgentSupportInfo(Resources.IDS_ARD_NAME_AGENT2, Resources.IDS_ARD_DESCRIPTION_AGENT2, agent2Id)
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
				return new ArchiveDecruncher_Lzx(Resources.IDS_ARD_NAME_AGENT1);

			if (typeId == agent2Id)
				return new ArchiveDecruncher_Lha(Resources.IDS_ARD_NAME_AGENT2);

			return null;
		}
		#endregion
	}
}
