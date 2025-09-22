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
[assembly: Guid("840469A1-1A00-4901-87BD-1ABD73BC5A45")]

namespace Polycode.NostalgicPlayer.Agent.Player.SoundFactory
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class SoundFactory : AgentBase
	{
		private static readonly Guid agent1Id = Guid.Parse("2AA371A1-5509-4444-B0B8-6CB8EBB869CA");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_PSF_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description => Resources.IDS_PSF_DESCRIPTION;



		/********************************************************************/
		/// <summary>
		/// Returns all the formats/types this agent supports
		/// </summary>
		/********************************************************************/
		public override AgentSupportInfo[] AgentInformation =>
		[
			new AgentSupportInfo(Resources.IDS_PSF_NAME_AGENT1, Resources.IDS_PSF_DESCRIPTION_AGENT1, agent1Id),
		];



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance(Guid typeId)
		{
			return new SoundFactoryWorker();
		}
		#endregion
	}
}
