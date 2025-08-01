﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Drawing;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// Loaders which implement this interface can provide metadata which
	/// is given to the player
	/// </summary>
	public interface IMetadata
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
		/// Return a specific font to be used for the comments
		/// </summary>
		Font CommentFont { get; }

		/// <summary>
		/// Return the lyrics separated in lines
		/// </summary>
		string[] Lyrics { get; }

		/// <summary>
		/// Return a specific font to be used for the lyrics
		/// </summary>
		Font LyricsFont { get; }

		/// <summary>
		/// Return all pictures available
		/// </summary>
		PictureInfo[] Pictures { get; }

		/// <summary>
		/// Holds the duration of the module this item has
		/// </summary>
		public TimeSpan Duration { get; }
	}
}
