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
[assembly: Guid("E42545C0-8A00-479A-8CBE-95667FF7F858")]

namespace Polycode.NostalgicPlayer.Agent.Player.ActivisionPro
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class ActivisionPro : AgentBase
	{
		private static readonly Guid agent1Id = Guid.Parse("7952CCE2-B49E-4824-8B6B-00515AC34E01");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_AVP_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description => Resources.IDS_AVP_DESCRIPTION;



		/********************************************************************/
		/// <summary>
		/// Returns all the formats/types this agent supports
		/// </summary>
		/********************************************************************/
		public override AgentSupportInfo[] AgentInformation =>
		[
			new AgentSupportInfo(Resources.IDS_AVP_NAME_AGENT1, Resources.IDS_AVP_DESCRIPTION_AGENT1, agent1Id),
		];



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance(Guid typeId)
		{
			return new ActivisionProWorker();
		}
		#endregion
	}
}
