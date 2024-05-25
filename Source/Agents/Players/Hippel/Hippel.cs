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
[assembly: Guid("218C93B5-F7D9-44AE-BF82-586A4F479F66")]

namespace Polycode.NostalgicPlayer.Agent.Player.Hippel
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class Hippel : AgentBase
	{
		internal static readonly Guid Agent1Id = Guid.Parse("11ADC278-2497-4DF1-B159-989C18A42661");
		internal static readonly Guid Agent2Id = Guid.Parse("8C6B4030-91B9-4187-99E3-196BE2591232");
		internal static readonly Guid Agent3Id = Guid.Parse("590BA85B-B2A9-4D8F-B164-311BD2ADB68D");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_HIP_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description => Resources.IDS_HIP_DESCRIPTION;



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
					new AgentSupportInfo(Resources.IDS_HIP_NAME_AGENT1, Resources.IDS_HIP_DESCRIPTION_AGENT1, Agent1Id),
					new AgentSupportInfo(Resources.IDS_HIP_NAME_AGENT2, Resources.IDS_HIP_DESCRIPTION_AGENT2, Agent2Id),
					new AgentSupportInfo(Resources.IDS_HIP_NAME_AGENT3, Resources.IDS_HIP_DESCRIPTION_AGENT3, Agent3Id)
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
			return new HippelWorker(typeId);
		}
		#endregion
	}
}
