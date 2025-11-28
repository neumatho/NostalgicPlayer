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
		internal static readonly Guid Agent1Id = Guid.Parse("9F71A133-3B6B-4F55-92F5-A379FC70C769");

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
			new AgentSupportInfo(Resources.IDS_OGG_NAME_AGENT1, Resources.IDS_OGG_DESCRIPTION_AGENT1, Agent1Id),
		];



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance(Guid typeId)
		{
			return new OggVorbisStreamerWorker();
		}
		#endregion
	}
}
