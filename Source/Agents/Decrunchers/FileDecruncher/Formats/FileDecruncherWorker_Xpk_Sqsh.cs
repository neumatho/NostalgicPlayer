/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Agent.Decruncher.FileDecruncher.Formats.Streams;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.FileDecruncher.Formats
{
	/// <summary>
	/// Can depack XPK-SQSH files
	/// </summary>
	internal class FileDecruncherWorker_Xpk_Sqsh : FileDecruncherAgentBase
	{
		private readonly string agentName;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public FileDecruncherWorker_Xpk_Sqsh(string agentName)
		{
			this.agentName = agentName;
		}

		#region IFileDecruncherAgent implementation
		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(Stream packedDataStream)
		{
			// Check the file size
			if (packedDataStream.Length < 46)
				return AgentResult.Unknown;

			using (ReaderStream readerStream = new ReaderStream(packedDataStream, true))
			{
				// Check the mark
				readerStream.Seek(0, SeekOrigin.Begin);

				if (readerStream.Read_B_UINT32() != 0x58504b46)		// XPKF
					return AgentResult.Unknown;

				readerStream.Seek(4, SeekOrigin.Current);
				if (readerStream.Read_B_UINT32() != 0x53515348)		// SQSH
					return AgentResult.Unknown;
			}

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Return a stream holding the depacked data
		/// </summary>
		/********************************************************************/
		public override DepackerStream OpenStream(Stream packedDataStream)
		{
			return new Xpk_SqshStream(agentName, packedDataStream);
		}
		#endregion
	}
}
