/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Kit.Bases
{
	/// <summary>
	/// Base class that can be used for streamer agents
	/// </summary>
	public abstract class StreamerAgentBase : IStreamerAgent
	{
		/// <summary>
		/// Holds the stream to stream from
		/// </summary>
		protected StreamingStream stream;

		private string title;
		private string author;
		private string[] comment;
		private Font commentFont;
		private string[] lyrics;
		private Font lyricsFont;
		private PictureInfo[] pictures;

		private readonly Dictionary<int, ModuleInfoChanged> changedModuleInfo = new Dictionary<int, ModuleInfoChanged>();

		#region IStreamerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Return an array of mime types that this agent can handle
		/// </summary>
		/********************************************************************/
		public abstract string[] PlayableMimeTypes { get; }



		/********************************************************************/
		/// <summary>
		/// Return some flags telling what the player supports
		/// </summary>
		/********************************************************************/
		public virtual StreamerSupportFlag SupportFlags => StreamerSupportFlag.None;



		/********************************************************************/
		/// <summary>
		/// Initializes the player
		/// </summary>
		/********************************************************************/
		public virtual bool InitPlayer(StreamingStream streamingStream, IMetadata metadata, out string errorMessage)
		{
			errorMessage = string.Empty;

			stream = streamingStream;

			title = metadata?.Title ?? string.Empty;
			author = metadata?.Author ?? string.Empty;
			comment = metadata?.Comment ?? [];
			commentFont = metadata?.CommentFont;
			lyrics = metadata?.Lyrics ?? [];
			lyricsFont = metadata?.LyricsFont;
			pictures = metadata?.Pictures;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the player
		/// </summary>
		/********************************************************************/
		public virtual void CleanupPlayer()
		{
			stream = null;
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the player to start the sample from start
		/// </summary>
		/********************************************************************/
		public virtual bool InitSound(out string errorMessage)
		{
			errorMessage = string.Empty;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the current song
		/// </summary>
		/********************************************************************/
		public virtual void CleanupSound()
		{
		}
		#endregion

		#region ISampleAgent implementation
		/********************************************************************/
		/// <summary>
		/// Will load and decode a data block and store it in the buffer
		/// given
		/// </summary>
		/********************************************************************/
		public abstract int LoadDataBlock(int[][] outputBuffer, int countInFrames);



		/********************************************************************/
		/// <summary>
		/// Return which speakers the player uses.
		/// 
		/// Note that the outputBuffer in LoadDataBlock match the defined
		/// order in SpeakerFlag enum
		/// </summary>
		/********************************************************************/
		public virtual SpeakerFlag SpeakerFlags => ChannelCount == 1 ? SpeakerFlag.FrontCenter : SpeakerFlag.FrontLeft | SpeakerFlag.FrontRight;



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the sample uses
		/// </summary>
		/********************************************************************/
		public abstract int ChannelCount { get; }



		/********************************************************************/
		/// <summary>
		/// Return the frequency the sample is stored with
		/// </summary>
		/********************************************************************/
		public abstract int Frequency { get; }
		#endregion

		#region IModuleInformation implementation
		/********************************************************************/
		/// <summary>
		/// Return the title
		/// </summary>
		/********************************************************************/
		public virtual string Title => title;



		/********************************************************************/
		/// <summary>
		/// Return the name of the author
		/// </summary>
		/********************************************************************/
		public virtual string Author => author;



		/********************************************************************/
		/// <summary>
		/// Return the comment separated in lines
		/// </summary>
		/********************************************************************/
		public virtual string[] Comment => comment;



		/********************************************************************/
		/// <summary>
		/// Return a specific font to be used for the comments
		/// </summary>
		/********************************************************************/
		public virtual Font CommentFont => commentFont;



		/********************************************************************/
		/// <summary>
		/// Return the lyrics separated in lines
		/// </summary>
		/********************************************************************/
		public virtual string[] Lyrics => lyrics;



		/********************************************************************/
		/// <summary>
		/// Return a specific font to be used for the lyrics
		/// </summary>
		/********************************************************************/
		public virtual Font LyricsFont => lyricsFont;



		/********************************************************************/
		/// <summary>
		/// Return all pictures available
		/// </summary>
		/********************************************************************/
		public virtual PictureInfo[] Pictures => pictures;



		/********************************************************************/
		/// <summary>
		/// Returns the description and value on the line given. If the line
		/// is out of range, false is returned
		/// </summary>
		/********************************************************************/
		public virtual bool GetInformationString(int line, out string description, out string value)
		{
			description = null;
			value = null;

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Return all module information changed since last call
		/// </summary>
		/********************************************************************/
		public virtual ModuleInfoChanged[] GetChangedInformation()
		{
			ModuleInfoChanged[] changedInfo = changedModuleInfo.Values.ToArray();
			changedModuleInfo.Clear();

			return changedInfo;
		}
		#endregion

		#region IEndDetection implementation
		/********************************************************************/
		/// <summary>
		/// This flag is set to true, when end is reached
		/// </summary>
		/********************************************************************/
		public virtual bool HasEndReached
		{
			get; set;
		}
		#endregion

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Call this when your player has reached the end of the module
		/// </summary>
		/********************************************************************/
		protected void OnEndReached()
		{
			// Set flag
			HasEndReached = true;
		}



		/********************************************************************/
		/// <summary>
		/// Call this every time your player change some information shown
		/// in the module information window
		/// </summary>
		/********************************************************************/
		protected void OnModuleInfoChanged(int line, string newValue)
		{
			changedModuleInfo[line] = new ModuleInfoChanged(line, newValue);
		}
		#endregion
	}
}
