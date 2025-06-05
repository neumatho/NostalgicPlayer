/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.ModuleConverter.Containers;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.ModuleConverter.Formats.Archives;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.ModuleConverter.Utility;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ModuleConverter.Formats
{
	/// <summary>
	/// Multi modules SC68 are treated as archives
	/// </summary>
	internal class Sc68FormatArchive : ArchiveDecruncherAgentBase
	{
		private readonly string agentName;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Sc68FormatArchive(string agentName)
		{
			this.agentName = agentName;
		}

		#region IArchiveDecruncherAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		///
		/// Has to be in lowercase
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => [ "sc68" ];



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(ReaderStream archiveStream)
		{
			// First check the length
			long fileSize = archiveStream.Length;
			if (fileSize < 64)
				return AgentResult.Unknown;

			// Check ID
			archiveStream.Seek(0, SeekOrigin.Begin);

			if (archiveStream.ReadMark(56) != Sc68Helper.IdString)
				return AgentResult.Unknown;

			List<Sc68DataBlockInfo> dataBlocks = Sc68Helper.FindAllModules(archiveStream, out string _);
			if ((dataBlocks == null) || (dataBlocks.Count <= 1))
				return AgentResult.Unknown;

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Will open the archive and return it
		/// </summary>
		/********************************************************************/
		public override IArchive OpenArchive(string archiveFileName, ReaderStream archiveStream)
		{
			return new Sc68Archive(agentName, archiveFileName, archiveStream);
		}
		#endregion
	}
}
