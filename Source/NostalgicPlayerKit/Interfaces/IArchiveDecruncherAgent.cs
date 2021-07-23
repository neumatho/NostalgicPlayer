/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// Agents of this type, can decrunch archives
	/// </summary>
	public interface IArchiveDecruncherAgent : IAgentWorker
	{
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		AgentResult Identify(Stream archiveStream);

		/// <summary>
		/// Will open the archive and return it
		/// </summary>
		IArchive OpenArchive(Stream archiveStream);
	}
}
