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
[assembly: Guid("D55107A1-E82B-47CD-813D-AD7206AB5A0B")]

namespace Polycode.NostalgicPlayer.Agent.Player.SonicArranger
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class SonicArranger : AgentBase
	{
		private static readonly Guid agent1Id = Guid.Parse("F9C6E671-AF32-425D-9EB7-E10938E287B5");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_SA_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description => Resources.IDS_SA_DESCRIPTION;



		/********************************************************************/
		/// <summary>
		/// Returns all the formats/types this agent supports
		/// </summary>
		/********************************************************************/
		public override AgentSupportInfo[] AgentInformation =>
		[
			new AgentSupportInfo(Resources.IDS_SA_NAME_AGENT1, Resources.IDS_SA_DESCRIPTION_AGENT1, agent1Id)
		];



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance(Guid typeId)
		{
			return new SonicArrangerWorker();
		}
		#endregion
	}
}
