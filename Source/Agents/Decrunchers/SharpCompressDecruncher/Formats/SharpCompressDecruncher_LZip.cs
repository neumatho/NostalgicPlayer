/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
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
	/// Can depack LZip files
	/// </summary>
	internal class SharpCompressDecruncher_LZip : FileDecruncherAgentBase
	{
		private readonly string agentName;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SharpCompressDecruncher_LZip(string agentName)
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
			packedDataStream.Seek(0, SeekOrigin.Begin);

			if (SharpCompress.Compressors.LZMA.LZipStream.IsLZipFile(packedDataStream))
				return AgentResult.Ok;

			return AgentResult.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Return a stream holding the depacked data
		/// </summary>
		/********************************************************************/
		public override DepackerStream OpenStream(Stream packedDataStream)
		{
			return new LZipStream(agentName, packedDataStream);
		}
		#endregion
	}
}
