/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Formats.Streams;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Formats
{
	/// <summary>
	/// Can depack PowerPacker data files
	/// </summary>
	internal class AncientDecruncherWorker_PowerPacker : FileDecruncherAgentBase
	{
		private readonly string agentName;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AncientDecruncherWorker_PowerPacker(string agentName)
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
			if (packedDataStream.Length < 16)
				return AgentResult.Unknown;

			using (ReaderStream readerStream = new ReaderStream(packedDataStream, true))
			{
				// Check the mark
				readerStream.Seek(0, SeekOrigin.Begin);

				if (readerStream.Read_B_UINT32() != 0x50503230)		// PP20
					return AgentResult.Unknown;

				// Check mode
				uint mode = readerStream.Read_B_UINT32();
				if ((mode != 0x09090909) && (mode != 0x090a0a0a) && (mode != 0x090a0b0b) && (mode != 0x090a0c0c) && (mode != 0x090a0c0d))
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
