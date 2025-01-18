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
[assembly: Guid("52013F5E-B82E-44D6-BE5C-53DB93CEFDBC")]

namespace Polycode.NostalgicPlayer.Agent.Player.PumaTracker
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class PumaTracker : AgentBase
	{
		private static readonly Guid Agent1Id = Guid.Parse("914741F8-625F-4E82-B925-AF85BB21E54E");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_PUMA_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description => Resources.IDS_PUMA_DESCRIPTION;



		/********************************************************************/
		/// <summary>
		/// Returns all the formats/types this agent supports
		/// </summary>
		/********************************************************************/
		public override AgentSupportInfo[] AgentInformation =>
		[
			new AgentSupportInfo(Resources.IDS_PUMA_NAME_AGENT1, Resources.IDS_PUMA_DESCRIPTION_AGENT1, Agent1Id)
		];



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance(Guid typeId)
		{
			return new PumaTrackerWorker();
		}
		#endregion
	}
}
