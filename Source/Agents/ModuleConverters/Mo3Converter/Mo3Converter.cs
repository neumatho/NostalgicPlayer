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
[assembly: Guid("DA9D821D-7510-470E-BF5B-5408671E98AD")]

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class Mo3Converter : AgentBase
	{
		private static readonly Guid agent1Id = Guid.Parse("42F7A186-1668-4F39-86FE-16E85BBC9004");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_MO3CONV_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description => Resources.IDS_MO3CONV_DESCRIPTION;



		/********************************************************************/
		/// <summary>
		/// Returns all the formats/types this agent supports
		/// </summary>
		/********************************************************************/
		public override AgentSupportInfo[] AgentInformation =>
		[
			new AgentSupportInfo(Resources.IDS_MO3CONV_NAME_AGENT1, Resources.IDS_MO3CONV_DESCRIPTION_AGENT1, agent1Id)
		];



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance(Guid typeId)
		{
			return new Mo3Format();
		}
		#endregion
	}
}
