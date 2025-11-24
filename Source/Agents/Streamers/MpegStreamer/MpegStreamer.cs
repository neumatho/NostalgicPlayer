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
[assembly: Guid("C8565D11-58EE-4E77-B58D-9919787D9AB9")]

namespace Polycode.NostalgicPlayer.Agent.Streamer.MpegStreamer
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class MpegStreamer : AgentBase
	{
		internal static readonly Guid Agent1Id = Guid.Parse("3DC54EF4-0E34-40AE-9B8C-C3AB188D7DE8");

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
			new AgentSupportInfo(Resources.IDS_MPG_NAME_AGENT1, Resources.IDS_MPG_DESCRIPTION_AGENT1, Agent1Id),
		];



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance(Guid typeId)
		{
			return new MpegStreamerWorker();
		}
		#endregion
	}
}
