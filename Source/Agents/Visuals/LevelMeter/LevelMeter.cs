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
[assembly: Guid("799AA358-9F06-421D-8131-860D46ED4065")]

namespace Polycode.NostalgicPlayer.Agent.Visual.LevelMeter
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class LevelMeter : AgentBase
	{
		private static readonly Guid agent1Id = Guid.Parse("1E1894D8-076F-42FF-91DF-7F655BCCDC36");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_METER_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns all the formats/types this agent supports
		/// </summary>
		/********************************************************************/
		public override AgentSupportInfo[] AgentInformation =>
		[
			new AgentSupportInfo(Resources.IDS_METER_NAME_AGENT1, Resources.IDS_METER_DESCRIPTION_AGENT1, agent1Id)
		];



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance(Guid typeId)
		{
			return new LevelMeterWorker();
		}
		#endregion
	}
}
