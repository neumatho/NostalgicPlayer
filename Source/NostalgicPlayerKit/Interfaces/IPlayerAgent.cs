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
	/// Agents of this type can play some kind of file
	/// </summary>
	public interface IPlayerAgent : IModuleInformation, IEndDetection, IAgentWorker
	{
		/// <summary>
		/// Returns the file extensions that identify this player
		///
		/// Has to be in lowercase
		/// </summary>
		string[] FileExtensions { get; }

		/// <summary>
		/// Return the identity priority number. Players with the lowest
		/// numbers will be called first.
		///
		/// Normally, you should not change this, but make your Identify()
		/// method to be aware of similar formats
		/// </summary>
		int IdentifyPriority { get; }

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
		/// Return all pictures available
		/// </summary>
		PictureInfo[] Pictures { get; }
	}
}
