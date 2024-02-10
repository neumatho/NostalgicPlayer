/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Kit.Bases
{
	/// <summary>
	/// Base class that can be used for archive decruncher agents
	/// </summary>
	public abstract class ArchiveDecruncherAgentBase : IArchiveDecruncherAgent
	{
		#region IArchiveDecruncherAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		///
		/// Has to be in lowercase
		/// </summary>
		/********************************************************************/
		public abstract string[] FileExtensions { get; }



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public abstract AgentResult Identify(Stream archiveStream);



		/********************************************************************/
		/// <summary>
		/// Will open the archive and return it
		/// </summary>
		/********************************************************************/
		public abstract IArchive OpenArchive(string archiveFileName, Stream archiveStream);
		#endregion
	}
}
