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
using SharpCompress.Archives.GZip;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.SharpCompressDecruncher.Formats
{
	/// <summary>
	/// Can decrunch GZip files
	/// </summary>
	internal class SharpCompressDecruncher_GZip : FileDecruncherAgentBase
	{
		private readonly string agentName;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SharpCompressDecruncher_GZip(string agentName)
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
			crunchedDataStream.Seek(0, SeekOrigin.Begin);

			if (GZipArchive.IsGZipFile(crunchedDataStream))
				return AgentResult.Ok;

			return AgentResult.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Return a stream holding the decrunched data
		/// </summary>
		/********************************************************************/
		public override DecruncherStream OpenStream(Stream crunchedDataStream)
		{
			return new GZipStream(agentName, crunchedDataStream);
		}
		#endregion
	}
}
