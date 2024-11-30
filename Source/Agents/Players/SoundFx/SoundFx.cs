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
[assembly: Guid("79F8365C-C73B-4D23-AAC6-6EFC661CC948")]

namespace Polycode.NostalgicPlayer.Agent.Player.SoundFx
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class SoundFx : AgentBase
	{
		private static readonly Guid agent1Id = Guid.Parse("4EDB2E11-1582-464C-A7B3-B34A70950398");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_SFX_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description => Resources.IDS_SFX_DESCRIPTION;



		/********************************************************************/
		/// <summary>
		/// Returns all the formats/types this agent supports
		/// </summary>
		/********************************************************************/
		public override AgentSupportInfo[] AgentInformation =>
		[
			new AgentSupportInfo(Resources.IDS_SFX_NAME_AGENT1, Resources.IDS_SFX_DESCRIPTION_AGENT1, agent1Id)
		];



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance(Guid typeId)
		{
			return new SoundFxWorker();
		}
		#endregion
	}
}
