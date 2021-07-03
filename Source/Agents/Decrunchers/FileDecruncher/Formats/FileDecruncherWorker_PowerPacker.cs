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
	/// Can depack PowerPacker data files
	/// </summary>
	internal class FileDecruncherWorker_PowerPacker : FileDecruncherAgentBase
	{
		private readonly string agentName;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public FileDecruncherWorker_PowerPacker(string agentName)
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
			if (packedDataStream.Length < 12)
				return AgentResult.Unknown;

			using (ReaderStream readerStream = new ReaderStream(packedDataStream, true))
			{
				// Check the mark
				readerStream.Seek(0, SeekOrigin.Begin);

				if (readerStream.Read_B_UINT32() != 0x50503230)		// PP20
					return AgentResult.Unknown;

				// Check the offset sizes
				if ((readerStream.Read_UINT8() > 16) || (readerStream.Read_UINT8() > 16) || (readerStream.Read_UINT8() > 16) || (readerStream.Read_UINT8() > 16))
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
			return new PowerPackerStream(agentName, packedDataStream);
		}
		#endregion
	}
}
