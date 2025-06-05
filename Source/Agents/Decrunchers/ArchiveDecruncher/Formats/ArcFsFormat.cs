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
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats
{
	/// <summary>
	/// Can decrunch ArcFs archives
	/// </summary>
	internal class ArcFsFormat : ArchiveDecruncherAgentBase
	{
		private readonly string agentName;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ArcFsFormat(string agentName)
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
		public override string[] FileExtensions => [ "arc", "arcfs", string.Empty ];



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(ReaderStream archiveStream)
		{
			// Check the file size
			if (archiveStream.Length < 96)
				return AgentResult.Unknown;

			// Check the header
			archiveStream.Seek(0, SeekOrigin.Begin);

			if (archiveStream.ReadMark(8, false) != "Archive\0")
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
			return new ArcFsArchive(agentName, archiveStream);
		}
		#endregion
	}
}
