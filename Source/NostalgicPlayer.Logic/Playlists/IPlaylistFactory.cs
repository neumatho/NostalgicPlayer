/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;

namespace Polycode.NostalgicPlayer.Logic.Playlists
{
	/// <summary>
	/// 
	/// </summary>
	public interface IPlaylistFactory
	{
		/// <summary>
		/// Return all available extensions for list files
		/// </summary>
		string[] GetExtensions();

		/// <summary>
		/// Try to figure out which kind of list this is and return a loader
		/// if anyone could be found
		/// </summary>
		IPlaylist Create(Stream stream, string fileExtension);

		/// <summary>
		/// Create a playlist instance based on type
		/// </summary>
		IPlaylist Create(PlaylistType type);
	}
}
