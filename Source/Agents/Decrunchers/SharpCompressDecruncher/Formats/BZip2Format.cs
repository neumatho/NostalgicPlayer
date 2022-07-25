/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Agent.Decruncher.SharpCompressDecruncher.Formats.Streams;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.SharpCompressDecruncher.Formats
{
	/// <summary>
	/// Can decrunch BZip2 files
	/// </summary>
	internal class BZip2Format : FileDecruncherAgentBase
	{
		private readonly string agentName;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public BZip2Format(string agentName)
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
			if (crunchedDataStream.Length < 10)
				return AgentResult.Unknown;

			using (ReaderStream readerStream = new ReaderStream(crunchedDataStream, true))
			{
				// Check the mark
				readerStream.Seek(0, SeekOrigin.Begin);

				if (readerStream.Read_B_UINT16() != 0x425a)			// BZ
					return AgentResult.Unknown;

				if (readerStream.Read_UINT8() != 0x68)				// h (Version)
					return AgentResult.Unknown;

				byte level = readerStream.Read_UINT8();
				if ((level < 0x30) || (level > 0x39))
					return AgentResult.Unknown;

				// Check block header magic bytes (its Pi)
				byte[] buf = new byte[6];
				readerStream.Read(buf, 0, 6);

				if ((buf[0] != 0x31) || (buf[1] != 0x41) || (buf[2] != 0x59) || (buf[3] != 0x26) || (buf[4] != 0x53) || (buf[5] != 0x59))
					return AgentResult.Unknown;
			}

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Return a stream holding the decrunched data
		/// </summary>
		/********************************************************************/
		public override DecruncherStream OpenStream(Stream crunchedDataStream)
		{
			return new BZip2Stream(agentName, crunchedDataStream);
		}
		#endregion
	}
}
