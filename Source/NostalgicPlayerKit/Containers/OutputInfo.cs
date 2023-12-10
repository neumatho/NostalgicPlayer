/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	/// This class holds information about how the output should
	/// be given to the output agent.
	///
	/// Note that the output should always be in 32-bit PCM and the
	/// number of channels, even if bigger than 2 channels
	/// </summary>
	public class OutputInfo
	{
		/// <summary>
		/// Output is always in 32-bit
		/// </summary>
		public const int BytesPerSample = 32 / 8;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public OutputInfo(int channels, int frequency, int bufferSizeInSamples)
		{
			Channels = channels;
			Frequency = frequency;
			BufferSizeInSamples = bufferSizeInSamples;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the number of channels
		/// </summary>
		/********************************************************************/
		public int Channels
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the output frequency
		/// </summary>
		/********************************************************************/
		public int Frequency
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the average buffer size in samples
		/// </summary>
		/********************************************************************/
		public int BufferSizeInSamples
		{
			get;
		}
	}
}
