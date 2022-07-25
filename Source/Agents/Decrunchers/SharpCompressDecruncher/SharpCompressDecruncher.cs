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

		private static readonly Guid agent5Id = Guid.Parse("8E477E82-7CB8-448B-8DF6-F1F540858EBA");
		private static readonly Guid agent6Id = Guid.Parse("675E3CE2-84D9-4640-B6D4-648C742C1855");
		private static readonly Guid agent7Id = Guid.Parse("67D15ECE-6725-4EE8-88E0-44D000691C0C");
		private static readonly Guid agent8Id = Guid.Parse("67156766-FA54-4C79-A545-DF91DC588FB7");

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
					// File decrunchers
					new AgentSupportInfo(Resources.IDS_SCOM_NAME_AGENT1, Resources.IDS_SCOM_DESCRIPTION_AGENT1, agent1Id),
					new AgentSupportInfo(Resources.IDS_SCOM_NAME_AGENT2, Resources.IDS_SCOM_DESCRIPTION_AGENT2, agent2Id),
					new AgentSupportInfo(Resources.IDS_SCOM_NAME_AGENT3, Resources.IDS_SCOM_DESCRIPTION_AGENT3, agent3Id),
					new AgentSupportInfo(Resources.IDS_SCOM_NAME_AGENT4, Resources.IDS_SCOM_DESCRIPTION_AGENT4, agent4Id),

					// Archive decrunchers
					new AgentSupportInfo(Resources.IDS_SCOM_NAME_AGENT5, Resources.IDS_SCOM_DESCRIPTION_AGENT5, agent5Id),
					new AgentSupportInfo(Resources.IDS_SCOM_NAME_AGENT6, Resources.IDS_SCOM_DESCRIPTION_AGENT6, agent6Id),
					new AgentSupportInfo(Resources.IDS_SCOM_NAME_AGENT7, Resources.IDS_SCOM_DESCRIPTION_AGENT7, agent7Id),
					new AgentSupportInfo(Resources.IDS_SCOM_NAME_AGENT8, Resources.IDS_SCOM_DESCRIPTION_AGENT8, agent8Id)
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
				return new BZip2Format(Resources.IDS_SCOM_NAME_AGENT1);

			if (typeId == agent2Id)
				return new GZipFormat(Resources.IDS_SCOM_NAME_AGENT2);

			if (typeId == agent3Id)
				return new LZipFormat(Resources.IDS_SCOM_NAME_AGENT3);

			if (typeId == agent4Id)
				return new XzFormat(Resources.IDS_SCOM_NAME_AGENT4);

			if (typeId == agent5Id)
				return new ZipFormat(Resources.IDS_SCOM_NAME_AGENT5);

			if (typeId == agent6Id)
				return new TarFormat(Resources.IDS_SCOM_NAME_AGENT6);

			if (typeId == agent7Id)
				return new _7ZipFormat(Resources.IDS_SCOM_NAME_AGENT7);

			if (typeId == agent8Id)
				return new RarFormat(Resources.IDS_SCOM_NAME_AGENT8);

			return null;
		}
		#endregion
	}
}
