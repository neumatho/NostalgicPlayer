/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// Agents of this type can play from a stream
	/// </summary>
	public interface IStreamerAgent : ISampleAgent, IAgentWorker
	{
		/// <summary>
		/// Return an array of mime types that this agent can handle
		/// </summary>
		string[] PlayableMimeTypes { get; }

		/// <summary>
		/// Return some flags telling what the player supports
		/// </summary>
		StreamerSupportFlag SupportFlags { get; }

		/// <summary>
		/// Initializes the player
		/// </summary>
		bool InitPlayer(StreamingStream streamingStream, IMetadata metadata, out string errorMessage);

		/// <summary>
		/// Cleanup the player
		/// </summary>
		void CleanupPlayer();

		/// <summary>
		/// Initializes the player to start the sample from start
		/// </summary>
		bool InitSound(out string errorMessage);

		/// <summary>
		/// Cleanup the current song
		/// </summary>
		void CleanupSound();
	}
}
