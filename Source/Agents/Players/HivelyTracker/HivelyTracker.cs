/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

// This is needed to uniquely identify this agent
[assembly: Guid("5520B860-5C83-4810-BC4D-9070D99A0189")]

namespace Polycode.NostalgicPlayer.Agent.Player.HivelyTracker
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class HivelyTracker : AgentBase
	{
		internal static readonly Guid Agent1Id = Guid.Parse("F219FA0A-7AD9-449F-B475-22E8F82E4BE4");
		internal static readonly Guid Agent2Id = Guid.Parse("BC6E1008-A72D-4F90-92D9-E0866E3C7AEF");
		internal static readonly Guid Agent3Id = Guid.Parse("C8DBD8E0-7B2B-4448-805C-F38702CC3FBB");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_HVL_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description => Resources.IDS_HVL_DESCRIPTION;



		/********************************************************************/
		/// <summary>
		/// Returns all the formats/types this agent supports
		/// </summary>
		/********************************************************************/
		public override AgentSupportInfo[] AgentInformation =>
		[
			new AgentSupportInfo(Resources.IDS_HVL_NAME_AGENT1, Resources.IDS_HVL_DESCRIPTION_AGENT1, Agent1Id),
			new AgentSupportInfo(Resources.IDS_HVL_NAME_AGENT2, Resources.IDS_HVL_DESCRIPTION_AGENT2, Agent2Id),
			new AgentSupportInfo(Resources.IDS_HVL_NAME_AGENT3, Resources.IDS_HVL_DESCRIPTION_AGENT3, Agent3Id)
		];



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance(Guid typeId)
		{
			return new HivelyTrackerWorker(typeId);
		}
		#endregion
	}
}
