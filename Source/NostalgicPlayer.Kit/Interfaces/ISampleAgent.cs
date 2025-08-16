/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Containers.Flags;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// Agents that can play samples or stream samples must implement this interface
	/// </summary>
	public interface ISampleAgent : IModuleInformation, IEndDetection
	{
		/// <summary>
		/// Will load and decode a data block and store it in the buffer
		/// given
		/// </summary>
		int LoadDataBlock(int[][] outputBuffer, int countInFrames);

		/// <summary>
		/// Return which speakers the player uses.
		/// 
		/// Note that the outputBuffer in LoadDataBlock match the defined
		/// order in SpeakerFlag enum
		/// </summary>
		SpeakerFlag SpeakerFlags { get; }

		/// <summary>
		/// Return the number of channels the sample uses
		/// </summary>
		int ChannelCount { get; }

		/// <summary>
		/// Return the frequency the sample is stored with
		/// </summary>
		int Frequency { get; }
	}
}
