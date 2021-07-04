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
using Polycode.NostalgicPlayer.Agent.Decruncher.SharpCompressDecruncher.Formats;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

// This is needed to uniquely identify this agent
[assembly: Guid("3D42A2DA-E3A0-4ADB-9760-CF9ED9EE955D")]

namespace Polycode.NostalgicPlayer.Agent.Decruncher.SharpCompressDecruncher
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class SharpCompressDecruncher : AgentBase
	{
		private static readonly Guid agent1Id = Guid.Parse("4A333599-C3BD-4A08-86E4-8698B3188829");
		private static readonly Guid agent2Id = Guid.Parse("16EB6389-565D-46AD-B8D2-8BB3E59161C0");
		private static readonly Guid agent3Id = Guid.Parse("71D5E132-FEBA-41D7-8B4A-36C0801644DC");
		private static readonly Guid agent4Id = Guid.Parse("E863B01C-E3E3-4524-BB93-EB5F7453EF11");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_SCOM_NAME;



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
					new AgentSupportInfo(Resources.IDS_SCOM_NAME_AGENT1, Resources.IDS_SCOM_DESCRIPTION_AGENT1, agent1Id),
					new AgentSupportInfo(Resources.IDS_SCOM_NAME_AGENT2, Resources.IDS_SCOM_DESCRIPTION_AGENT2, agent2Id),
					new AgentSupportInfo(Resources.IDS_SCOM_NAME_AGENT3, Resources.IDS_SCOM_DESCRIPTION_AGENT3, agent3Id),
					new AgentSupportInfo(Resources.IDS_SCOM_NAME_AGENT4, Resources.IDS_SCOM_DESCRIPTION_AGENT4, agent4Id)
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
				return new SharpCompressDecruncher_BZip2(Resources.IDS_SCOM_NAME_AGENT1);

			if (typeId == agent2Id)
				return new SharpCompressDecruncher_GZip(Resources.IDS_SCOM_NAME_AGENT2);

			if (typeId == agent3Id)
				return new SharpCompressDecruncher_LZip(Resources.IDS_SCOM_NAME_AGENT3);

			if (typeId == agent4Id)
				return new SharpCompressDecruncher_Xz(Resources.IDS_SCOM_NAME_AGENT4);

			return null;
		}
		#endregion
	}
}
