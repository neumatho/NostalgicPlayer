/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.IO;

namespace Polycode.NostalgicPlayer.Logic.Playlists
{
	/// <summary>
	/// This interface is implemented by classes that can handle
	/// different kind of files that contains other files, e.g.
	/// module lists
	/// </summary>
	public interface IPlaylist
	{
		/// <summary>
		/// Returns the file extensions that identify this list
		///
		/// Has to be in lowercase
		/// </summary>
		string[] FileExtensions { get; }

		/// <summary>
		/// Will load a list from the given file
		/// </summary>
		IEnumerable<PlaylistFileInfo> LoadList(string directory, Stream stream, string fileExtension);

		/// <summary>
		/// Will save a list to the given file
		/// </summary>
		void SaveList(string fileName, IEnumerable<PlaylistFileInfo> list);
	}
}
