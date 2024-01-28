/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Agent.Decruncher.SharpCompressDecruncher.Formats.Archives;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.SharpCompressDecruncher.Formats
{
	/// <summary>
	/// Can parse Tar archives
	/// </summary>
	internal class TarFormat : ArchiveDecruncherAgentBase
	{
		private readonly string agentName;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public TarFormat(string agentName)
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
		public override string[] FileExtensions => new[] { "tar", "bz2", "gz" };



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(Stream archiveStream)
		{
			archiveStream.Seek(0, SeekOrigin.Begin);

			if (SharpCompress.Archives.Tar.TarArchive.IsTarFile(archiveStream))
			{
				// The implementation of the identify code in SharpCompress isn't
				// good enough, we make a double check here. However, that means
				// only new versions of tar files are supported
				using (ReaderStream readerStream = new ReaderStream(archiveStream, true))
				{
					readerStream.Seek(257, SeekOrigin.Begin);

					if ((readerStream.Read_B_UINT32() == 0x75737461) && (readerStream.Read_UINT8() == 0x72))		// ustar
						return AgentResult.Ok;
				}
			}

			return AgentResult.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Will open the archive and return it
		/// </summary>
		/********************************************************************/
		public override IArchive OpenArchive(Stream archiveStream)
		{
			return new TarArchive(agentName, archiveStream);
		}
		#endregion
	}
}
