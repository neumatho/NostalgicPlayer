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
[assembly: Guid("51D2C4C4-5669-460B-8B3D-ED031CFEF8C7")]

namespace Polycode.NostalgicPlayer.Agent.Streamer.OggStreamer
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class OggStreamer : AgentBase
	{
		private static readonly Guid agent1Id = Guid.Parse("9F71A133-3B6B-4F55-92F5-A379FC70C769");
		private static readonly Guid agent2Id = Guid.Parse("9295EFFE-A7C2-48E9-9D36-D895B9B4F642");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description => Resources.IDS_DESCRIPTION;



		/********************************************************************/
		/// <summary>
		/// Returns all the formats/types this agent supports
		/// </summary>
		/********************************************************************/
		public override AgentSupportInfo[] AgentInformation =>
		[
			new AgentSupportInfo(Resources.IDS_OGG_NAME_AGENT1, Resources.IDS_OGG_DESCRIPTION_AGENT1, agent1Id),
			new AgentSupportInfo(Resources.IDS_OGG_NAME_AGENT2, Resources.IDS_OGG_DESCRIPTION_AGENT2, agent2Id)
		];



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance(Guid typeId)
		{
			if (typeId == agent1Id)
				return new OggVorbisStreamerWorker();

			if (typeId == agent2Id)
				return new OpusStreamerWorker();

			return null;
		}
		#endregion
	}
}
