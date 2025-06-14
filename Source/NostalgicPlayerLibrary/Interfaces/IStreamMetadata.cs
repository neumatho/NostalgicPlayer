/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Interfaces
{
	/// <summary>
	/// Loaders which implement this interface can provide metadata about the stream
	/// </summary>
	public interface IStreamMetadata
	{
		/// <summary>
		/// Return the title of the song
		/// </summary>
		string Title { get; }

		/// <summary>
		/// Return the name of the author
		/// </summary>
		string Author { get; }

		/// <summary>
		/// Return some comment about the song
		/// </summary>
		string[] Comment { get; }

		/// <summary>
		/// Holds the duration of the module this item has
		/// </summary>
		public TimeSpan Duration { get; }

		/// <summary>
		/// Return all pictures available
		/// </summary>
		PictureInfo[] Pictures { get; }
	}
}
