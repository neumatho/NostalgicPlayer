/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

// This is needed to uniquely identify this agent
[assembly: Guid("86281391-E959-4E49-8F9E-C5E757C0D3F0")]

namespace Polycode.NostalgicPlayer.Agent.Player.ArtOfNoise
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class ArtOfNoise : AgentBase
	{
		internal static readonly Guid Agent1Id = Guid.Parse("372D7341-0880-4C42-9D24-FDFCA2D4A395");
		internal static readonly Guid Agent2Id = Guid.Parse("409487B5-BC5E-45BE-AF67-9DA8D2BE2E5A");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_AON_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description => Resources.IDS_AON_DESCRIPTION;



		/********************************************************************/
		/// <summary>
		/// Returns all the formats/types this agent supports
		/// </summary>
		/********************************************************************/
		public override AgentSupportInfo[] AgentInformation =>
		[
			new AgentSupportInfo(Resources.IDS_AON_NAME_AGENT1, Resources.IDS_AON_DESCRIPTION_AGENT1, Agent1Id),
			new AgentSupportInfo(Resources.IDS_AON_NAME_AGENT2, Resources.IDS_AON_DESCRIPTION_AGENT2, Agent2Id)
		];



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance(Guid typeId)
		{
			return new ArtOfNoiseWorker(typeId);
		}
		#endregion
	}
}
