/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Library.Loaders
{
	/// <summary>
	/// Helper to detect archive files and extract entries
	/// </summary>
	public interface IArchiveDetector
	{
		/// <summary>
		/// Return all available extensions for archive files
		/// </summary>
		string[] GetExtensions();

		/// <summary>
		/// Will check if the given file is an archive
		/// </summary>
		bool IsArchive(string fullPath);

		/// <summary>
		/// Will return a collection with all the entries in the given
		/// archive file
		/// </summary>
		IEnumerable<string> GetEntries(string fullPath);

		/// <summary>
		/// Will try to find the archive agent that can be used on the
		/// given stream and then open the archive and return a new stream
		/// </summary>
		IArchive OpenArchive(string archiveFileName, Stream stream, out Stream newStream, out List<string> decruncherAlgorithms);
	}
}
