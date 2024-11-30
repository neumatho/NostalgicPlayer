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
[assembly: Guid("A9A18A8E-C160-47EF-AD79-A903921C01E1")]

namespace Polycode.NostalgicPlayer.Agent.SampleConverter.AudioIff
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class AudioIff : AgentBase
	{
		private static readonly Guid agent1Id = Guid.Parse("BBAABC5F-AE88-42D1-8717-EAEC041AFE7D");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_AUDIOIFF_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns all the formats/types this agent supports
		/// </summary>
		/********************************************************************/
		public override AgentSupportInfo[] AgentInformation =>
		[
			new AgentSupportInfo(Resources.IDS_AUDIOIFF_NAME_AGENT1, Resources.IDS_AUDIOIFF_DESCRIPTION_AGENT1, agent1Id)
		];



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance(Guid typeId)
		{
			return new AudioIffWorker();
		}
		#endregion
	}
}
