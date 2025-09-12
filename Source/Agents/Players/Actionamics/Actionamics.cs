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
[assembly: Guid("DDC8CC9E-A24D-4743-B54E-C168526BBC7B")]

namespace Polycode.NostalgicPlayer.Agent.Player.Actionamics
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class Actionamics : AgentBase
	{
		private static readonly Guid agent1Id = Guid.Parse("F25342AA-ADED-4AB6-92B5-E9220B0C76E1");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_AST_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description => Resources.IDS_AST_DESCRIPTION;



		/********************************************************************/
		/// <summary>
		/// Returns all the formats/types this agent supports
		/// </summary>
		/********************************************************************/
		public override AgentSupportInfo[] AgentInformation =>
		[
			new AgentSupportInfo(Resources.IDS_AST_NAME_AGENT1, Resources.IDS_AST_DESCRIPTION_AGENT1, agent1Id),
		];



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance(Guid typeId)
		{
			return new ActionamicsWorker();
		}
		#endregion
	}
}
