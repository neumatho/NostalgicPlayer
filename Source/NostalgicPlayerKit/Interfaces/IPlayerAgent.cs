/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Events;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// Agents of this type can play some kind of file
	/// </summary>
	public interface IPlayerAgent : IAgentWorker
	{
		/// <summary>
		/// Returns the file extensions that identify this player
		///
		/// Has to be in lowercase
		/// </summary>
		string[] FileExtensions { get; }

		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		AgentResult Identify(PlayerFileInfo fileInfo);

		/// <summary>
		/// Return some extra information about the format. If it returns
		/// null or an empty string, nothing extra is shown
		/// </summary>
		string ExtraFormatInfo { get; }

		/// <summary>
		/// Return the name of the module
		/// </summary>
		string ModuleName { get; }

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
		/// Returns the description and value on the line given. If the line
		/// is out of range, false is returned
		/// </summary>
		bool GetInformationString(int line, out string description, out string value);

		/// <summary>
		/// This flag is set to true, when end is reached
		/// </summary>
		bool HasEndReached { get; set; }

		/// <summary>
		/// Event called when the player update some module information
		/// </summary>
		event ModuleInfoChangedEventHandler ModuleInfoChanged;
	}
}
