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
[assembly: Guid("A1B2C3D4-5E6F-7A8B-9C0D-1E2F3A4B5C6D")]

namespace Polycode.NostalgicPlayer.Agent.Visual.ChannelLevelMeter
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class ChannelLevelMeter : AgentBase
	{
		private static readonly Guid agent1Id = Guid.Parse("2F3A4B5C-6D7E-8F9A-0B1C-2D3E4F5A6B7C");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_CHANNELMETER_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns all the formats/types this agent supports
		/// </summary>
		/********************************************************************/
		public override AgentSupportInfo[] AgentInformation =>
		[
			new AgentSupportInfo(Resources.IDS_CHANNELMETER_NAME_AGENT1, Resources.IDS_CHANNELMETER_DESCRIPTION_AGENT1, agent1Id)
		];



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance(Guid typeId)
		{
			return new ChannelLevelMeterWorker();
		}
		#endregion
	}
}
