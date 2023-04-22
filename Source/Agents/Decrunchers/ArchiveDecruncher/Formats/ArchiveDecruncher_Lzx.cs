/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Archive;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats
{
	/// <summary>
	/// Can decrunch Lzx archives
	/// </summary>
	internal class ArchiveDecruncher_Lzx : ArchiveDecruncherAgentBase
	{
		private readonly string agentName;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ArchiveDecruncher_Lzx(string agentName)
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
		public override string[] FileExtensions => new[] { "lzx" };



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(Stream archiveStream)
		{
			// Check the file size
			if (archiveStream.Length < 10)
				return AgentResult.Unknown;

			// Check the mark
			archiveStream.Seek(0, SeekOrigin.Begin);

			byte[] buf = new byte[4];

			archiveStream.Read(buf, 0, 4);

			if ((buf[0] != 0x4c) || (buf[1] != 0x5a) || (buf[2] != 0x58) || (buf[3]) != 0x00)	// LZX\0
				return AgentResult.Unknown;

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Will open the archive and return it
		/// </summary>
		/********************************************************************/
		public override IArchive OpenArchive(Stream archiveStream)
		{
			return new LzxArchive(agentName, archiveStream);
		}
		#endregion
	}
}
