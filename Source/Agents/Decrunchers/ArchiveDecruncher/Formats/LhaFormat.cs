/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Archives;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats
{
	/// <summary>
	/// Can decrunch Lha archives
	/// </summary>
	internal class LhaFormat : ArchiveDecruncherAgentBase
	{
		private readonly string agentName;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public LhaFormat(string agentName)
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
		public override string[] FileExtensions => [ "lha", "lzh", "lzs" ];



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(Stream archiveStream)
		{
			// Check the file size
			if (archiveStream.Length < 24)
				return AgentResult.Unknown;

			// Check the header
			archiveStream.Seek(0, SeekOrigin.Begin);

			byte[] buf = new byte[24];

			archiveStream.ReadExactly(buf, 0, 24);

			// Check header level
			if (buf[20] >= 3)
				return AgentResult.Unknown;

			// Check different crunching methods
			//
			// -lh0-, -lh1-, -lh2-, -lh3-, -lh4-, -lh5-, -lh6-, -lh7-
			// -lzs-, -lz5-, -lz4-, -lhd-
			if ((buf[2] != 0x2d) || (buf[3] != 0x6c) || (buf[6] != 0x2d))
				return AgentResult.Unknown;

			if ((buf[4] == 0x68) && (buf[5] >= 0x30) && (buf[5] <= 0x37))
				return AgentResult.Ok;

			if ((buf[4] == 0x7a) && ((buf[5] == 0x73) || (buf[5] == 0x35) || (buf[5] == 0x34)))
				return AgentResult.Ok;

			if ((buf[4] == 0x68) && (buf[5] == 0x64))
				return AgentResult.Ok;

			return AgentResult.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Will open the archive and return it
		/// </summary>
		/********************************************************************/
		public override IArchive OpenArchive(string archiveFileName, Stream archiveStream)
		{
			return new LhaArchive(agentName, archiveStream);
		}
		#endregion
	}
}
