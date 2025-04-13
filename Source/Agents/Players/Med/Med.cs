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
[assembly: Guid("D64867F2-5618-4B82-ADFE-3AE6A32AB240")]

namespace Polycode.NostalgicPlayer.Agent.Player.Med
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class Med : AgentBase
	{
		internal static readonly Guid Agent1Id = Guid.Parse("F89F2816-7E45-4C62-A2BD-FC075F45DF59");
		internal static readonly Guid Agent2Id = Guid.Parse("4A5E1E78-B14D-4D04-8DC7-58C566B91F67");

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
		public override AgentSupportInfo[] AgentInformation =>
		[
			new AgentSupportInfo(Resources.IDS_MED_NAME_AGENT1, Resources.IDS_MED_DESCRIPTION_AGENT1, Agent1Id),
			new AgentSupportInfo(Resources.IDS_MED_NAME_AGENT2, Resources.IDS_MED_DESCRIPTION_AGENT2, Agent2Id)
		];



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance(Guid typeId)
		{
			return new MedWorker(typeId);
		}
		#endregion
	}
}
