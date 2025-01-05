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
[assembly: Guid("22BE6117-3809-44C0-988D-C10E6237577F")]

namespace Polycode.NostalgicPlayer.Agent.Player.MusicAssembler
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class MusicAssembler : AgentBase
	{
		private static readonly Guid Agent1Id = Guid.Parse("BC68CA87-B330-4A14-87A4-1D7AA5851095");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_MA_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description => Resources.IDS_MA_DESCRIPTION;



		/********************************************************************/
		/// <summary>
		/// Returns all the formats/types this agent supports
		/// </summary>
		/********************************************************************/
		public override AgentSupportInfo[] AgentInformation =>
		[
			new AgentSupportInfo(Resources.IDS_MA_NAME_AGENT1, Resources.IDS_MA_DESCRIPTION_AGENT1, Agent1Id)
		];



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance(Guid typeId)
		{
			return new MusicAssemblerWorker();
		}
		#endregion
	}
}
