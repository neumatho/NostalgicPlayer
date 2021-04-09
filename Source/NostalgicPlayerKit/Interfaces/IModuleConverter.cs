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
	/// Agents of this type, can convert one module format to another
	/// </summary>
	public interface IModuleConverter : IAgentWorker
	{
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		AgentResult Identify(PlayerFileInfo fileInfo);

		/// <summary>
		/// Convert the module and store the result in the stream given
		/// </summary>
		AgentResult Convert(PlayerFileInfo fileInfo, WriterStream writerStream, out string errorMessage);
	}
}
