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
using Polycode.NostalgicPlayer.Agent.Decruncher.FileDecruncher.Formats;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

// This is needed to uniquely identify this agent
[assembly: Guid("48DC4EA4-ABAB-4FDB-AADC-9137BBAAFC1E")]

namespace Polycode.NostalgicPlayer.Agent.Decruncher.FileDecruncher
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class FileDecruncher : AgentBase
	{
		private static readonly Guid agent1Id = Guid.Parse("615D0B1C-E86C-40D3-ACF8-5C4D1A88EEC6");
		private static readonly Guid agent2Id = Guid.Parse("CAC6F4B3-E037-4EC2-954F-08A44469EF1E");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_FILEDECR_NAME;



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
					new AgentSupportInfo(Resources.IDS_FILEDECR_NAME_AGENT1, Resources.IDS_FILEDECR_DESCRIPTION_AGENT1, agent1Id),
					new AgentSupportInfo(Resources.IDS_FILEDECR_NAME_AGENT2, Resources.IDS_FILEDECR_DESCRIPTION_AGENT2, agent2Id)
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
				return new FileDecruncherWorker_PowerPacker(Resources.IDS_FILEDECR_NAME_AGENT1);

			if (typeId == agent2Id)
				return new FileDecruncherWorker_Xpk_Sqsh(Resources.IDS_FILEDECR_NAME_AGENT2);

			return null;
		}
		#endregion
	}
}
