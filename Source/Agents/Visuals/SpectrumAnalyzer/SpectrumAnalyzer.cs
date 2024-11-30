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
[assembly: Guid("5C110162-3E0B-4AA9-AF62-9F933E83651F")]

namespace Polycode.NostalgicPlayer.Agent.Visual.SpectrumAnalyzer
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class SpectrumAnalyzer : AgentBase
	{
		private static readonly Guid agent1Id = Guid.Parse("F7C52106-40A5-46B3-ADF2-C5CA228D6DBE");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_SPEC_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns all the formats/types this agent supports
		/// </summary>
		/********************************************************************/
		public override AgentSupportInfo[] AgentInformation =>
		[
			new AgentSupportInfo(Resources.IDS_SPEC_NAME_AGENT1, Resources.IDS_SPEC_DESCRIPTION_AGENT1, agent1Id)
		];



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance(Guid typeId)
		{
			return new SpectrumAnalyzerWorker();
		}
		#endregion
	}
}
