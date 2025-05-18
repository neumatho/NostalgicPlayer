/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
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
		private static readonly Guid agent3Id = Guid.Parse("53048FD0-E669-4B9B-AD17-0D19BF0A8ACD");

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
		public override AgentSupportInfo[] AgentInformation =>
		[
			new AgentSupportInfo(Resources.IDS_ARD_NAME_AGENT1, Resources.IDS_ARD_DESCRIPTION_AGENT1, agent1Id),
			new AgentSupportInfo(Resources.IDS_ARD_NAME_AGENT2, Resources.IDS_ARD_DESCRIPTION_AGENT2, agent2Id),
			new AgentSupportInfo(Resources.IDS_ARD_NAME_AGENT3, Resources.IDS_ARD_DESCRIPTION_AGENT3, agent3Id)
		];



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance(Guid typeId)
		{
			if (typeId == agent1Id)
				return new LzxFormat(Resources.IDS_ARD_NAME_AGENT1);

			if (typeId == agent2Id)
				return new LhaFormat(Resources.IDS_ARD_NAME_AGENT2);

			if (typeId == agent3Id)
				return new ArcFsFormat(Resources.IDS_ARD_NAME_AGENT3);

			return null;
		}
		#endregion
	}
}
