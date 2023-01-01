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

namespace Polycode.NostalgicPlayer.Agent.Player.Ahx
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class Ahx : AgentBase
	{
		internal static readonly Guid Agent1Id = Guid.Parse("F219FA0A-7AD9-449F-B475-22E8F82E4BE4");
		internal static readonly Guid Agent2Id = Guid.Parse("BC6E1008-A72D-4F90-92D9-E0866E3C7AEF");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_AHX_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description => Resources.IDS_AHX_DESCRIPTION;



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
					new AgentSupportInfo(Resources.IDS_AHX_NAME_AGENT1, Resources.IDS_AHX_DESCRIPTION_AGENT1, Agent1Id),
					new AgentSupportInfo(Resources.IDS_AHX_NAME_AGENT2, Resources.IDS_AHX_DESCRIPTION_AGENT2, Agent2Id)
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
			return new AhxWorker(typeId);
		}
		#endregion
	}
}
