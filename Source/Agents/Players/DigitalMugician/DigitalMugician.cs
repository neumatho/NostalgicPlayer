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
[assembly: Guid("F9D4E6EF-F663-45BC-B3DF-D25108255DDE")]

namespace Polycode.NostalgicPlayer.Agent.Player.DigitalMugician
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class DigitalMugician : AgentBase
	{
		internal static readonly Guid Agent1Id = Guid.Parse("480D92D2-0C94-426A-BBE9-FA9F9AA90F84");
		internal static readonly Guid Agent2Id = Guid.Parse("4D133DBB-41C5-4751-AE8E-D2F4385A12E1");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_DMU_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description => Resources.IDS_DMU_DESCRIPTION;



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
					new AgentSupportInfo(Resources.IDS_DMU_NAME_AGENT1, Resources.IDS_DMU_DESCRIPTION_AGENT1, Agent1Id),
					new AgentSupportInfo(Resources.IDS_DMU_NAME_AGENT2, Resources.IDS_DMU_DESCRIPTION_AGENT2, Agent2Id)
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
			return new DigitalMugicianWorker(typeId);
		}
		#endregion
	}
}
