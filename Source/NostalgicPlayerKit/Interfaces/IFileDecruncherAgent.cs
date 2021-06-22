/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// Agents of this type, can depack a single format
	/// </summary>
	public interface IFileDecruncherAgent : IAgentWorker
	{
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		AgentResult Identify(ModuleStream moduleStream);

		/// <summary>
		/// Return the size of the depacked data
		/// </summary>
		int GetDepackedLength(ModuleStream moduleStream);

		/// <summary>
		/// Depack the file and store the result in the buffer given
		/// </summary>
		AgentResult Depack(byte[] source, byte[] destination, int safetySize, out string errorMessage);
	}
}
