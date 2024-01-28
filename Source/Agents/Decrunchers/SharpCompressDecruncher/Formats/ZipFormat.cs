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

namespace Polycode.NostalgicPlayer.Agent.Decruncher.SharpCompressDecruncher.Formats
{
	/// <summary>
	/// Can decrunch Zip archives
	/// </summary>
	internal class ZipFormat : ArchiveDecruncherAgentBase
	{
		private readonly string agentName;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ZipFormat(string agentName)
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
		public override string[] FileExtensions => new[] { "zip" };



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(Stream archiveStream)
		{
			archiveStream.Seek(0, SeekOrigin.Begin);

			if (SharpCompress.Archives.Zip.ZipArchive.IsZipFile(archiveStream))
				return AgentResult.Ok;

			return AgentResult.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Will open the archive and return it
		/// </summary>
		/********************************************************************/
		public override IArchive OpenArchive(Stream archiveStream)
		{
			return new ZipArchive(agentName, archiveStream);
		}
		#endregion
	}
}
