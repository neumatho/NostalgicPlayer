/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Formats
{
	/// <summary>
	/// Base class for all XPK decrunchers
	/// </summary>
	internal abstract class AncientDecruncherWorker_Xpk : FileDecruncherAgentBase
	{
		protected readonly string agentName;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected AncientDecruncherWorker_Xpk(string agentName)
		{
			this.agentName = agentName;
		}

		#region IFileDecruncherAgent implementation
		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(Stream crunchedDataStream)
		{
			// Check the file size
			if (crunchedDataStream.Length < 44)
				return AgentResult.Unknown;

			using (ReaderStream readerStream = new ReaderStream(crunchedDataStream, true))
			{
				// Check the mark
				readerStream.Seek(0, SeekOrigin.Begin);

				if (readerStream.Read_B_UINT32() != 0x58504b46)		// XPKF
					return AgentResult.Unknown;

				readerStream.Seek(4, SeekOrigin.Current);
				if (readerStream.Read_B_UINT32() != CruncherId)
					return AgentResult.Unknown;
			}

			return AgentResult.Ok;
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Return the cruncher ID
		/// </summary>
		/********************************************************************/
		protected abstract uint CruncherId { get; }
	}
}
