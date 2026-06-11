/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// Helper for agents to create instances with dependency injections
	/// </summary>
	public interface IAgentWorkerFactory
	{
		/// <summary>
		/// Create a new worker instance
		/// </summary>
		T GetWorkerInstance<T>(Guid typeId, params object[] extraArguments) where T : IAgentWorker;
	}
}
