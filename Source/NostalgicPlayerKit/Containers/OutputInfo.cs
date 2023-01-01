/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	/// This class holds information about how the output should
	/// be given to the output agent
	/// </summary>
	public class OutputInfo
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public OutputInfo(int channels, int frequency, int bufferSizeInSamples, int bytesPerSample)
		{
			Channels = channels;
			Frequency = frequency;
			BufferSizeInSamples = bufferSizeInSamples;
			BytesPerSample = bytesPerSample;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the number of channels. This can either be 1 for mono or
		/// 2 for stereo
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



		/********************************************************************/
		/// <summary>
		/// Holds the number of bytes each sample is using
		/// </summary>
		/********************************************************************/
		public int BytesPerSample
		{
			get;
		}
	}
}
