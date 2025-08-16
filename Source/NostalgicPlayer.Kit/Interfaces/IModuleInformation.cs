/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// Player can provide extra information about the module that is playing
	/// </summary>
	public interface IModuleInformation
	{
		/// <summary>
		/// Return the title
		/// </summary>
		string Title { get; }

		/// <summary>
		/// Return the name of the author
		/// </summary>
		string Author { get; }

		/// <summary>
		/// Return the comment separated in lines
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
		/// Returns the description and value on the line given. If the line
		/// is out of range, false is returned
		/// </summary>
		bool GetInformationString(int line, out string description, out string value);

		/// <summary>
		/// Return all module information changed since last call
		/// </summary>
		ModuleInfoChanged[] GetChangedInformation();
	}
}
