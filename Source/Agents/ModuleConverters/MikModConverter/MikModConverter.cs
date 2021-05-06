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
using Polycode.NostalgicPlayer.Agent.ModuleConverter.MikModConverter.Formats;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

// This is needed to uniquely identify this agent
[assembly: Guid("ED2854DF-91A7-4174-B100-FB169AAE5AF6")]

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.MikModConverter
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class MikModConverter : AgentBase
	{
		private static readonly Guid agent1Id = Guid.Parse("DA884858-983E-45DB-B4FF-470DBB927239");
		private static readonly Guid agent2Id = Guid.Parse("216FCDE9-3E12-474A-A27D-28914C8C894D");
		private static readonly Guid agent15Id = Guid.Parse("E37DF813-DCBD-4A32-AA07-5EF1AF6DD037");
		private static readonly Guid agent16Id = Guid.Parse("1574A876-5F9D-4BAE-81AF-7DB01370ADDD");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_MIKCONV_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description => Resources.IDS_MIKCONV_DESCRIPTION;



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
					new AgentSupportInfo(Resources.IDS_MIKCONV_NAME_AGENT1, Resources.IDS_MIKCONV_DESCRIPTION_AGENT1, agent1Id),
					new AgentSupportInfo(Resources.IDS_MIKCONV_NAME_AGENT2, Resources.IDS_MIKCONV_DESCRIPTION_AGENT2, agent2Id),
					new AgentSupportInfo(Resources.IDS_MIKCONV_NAME_AGENT15, Resources.IDS_MIKCONV_DESCRIPTION_AGENT15, agent15Id),
					new AgentSupportInfo(Resources.IDS_MIKCONV_NAME_AGENT16, Resources.IDS_MIKCONV_DESCRIPTION_AGENT16, agent16Id)
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
				return new MikModConverterWorker_669(0x69, 0x66);

			if (typeId == agent2Id)
				return new MikModConverterWorker_669(0x4a, 0x4e);

			if (typeId == agent15Id)
				return new MikModConverterWorker_UniMod();

			if (typeId == agent16Id)
				return new MikModConverterWorker_Xm();

			return null;
		}
		#endregion
	}
}
