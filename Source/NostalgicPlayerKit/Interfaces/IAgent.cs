/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// All agents implement this interface
	/// </summary>
	public interface IAgent
	{
		/// <summary>
		/// This is used to ensure that the agent is compatible with the
		/// current version of NostalgicPlayer
		/// </summary>
		public const int NostalgicPlayer_Current_Version = (3 << 16) + (0 << 8) + 0;

		/// <summary>
		/// Return the NostalgicPlayer_Current_Version constant defined above
		/// </summary>
		int NostalgicPlayerVersion { get; }

		/// <summary>
		/// Returns an unique ID for this agent
		/// </summary>
		Guid AgentId { get; }

		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Returns a description of this agent. Only needed for players
		/// and module converters
		/// </summary>
		string Description { get; }

		/// <summary>
		/// Returns the version of this agent
		/// </summary>
		Version Version { get; }

		/// <summary>
		/// Returns all the formats/types this agent supports
		/// </summary>
		AgentSupportInfo[] AgentInformation { get; }

		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		IAgentWorker CreateInstance(Guid typeId);
	}
}
