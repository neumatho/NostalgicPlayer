/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.PlayerLibrary.Sound.Mixer.Containers;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Sound.Mixer
{
	/// <summary>
	/// Helper class to convert between mixer buffers to output buffers
	/// </summary>
	internal class MixerConverter
	{
		/********************************************************************/
		/// <summary>
		/// Build channel mapping table. Will return an array with indexes
		/// in the output buffer to take each channel from
		/// </summary>
		/********************************************************************/
		public int[] BuildChannelMapping(MixerInfo currentMixerInfo, int outputChannelCount)
		{
			if (outputChannelCount == 1)
				return [ 0 ];

			if (currentMixerInfo.SwapSpeakers)
				return [ 1, 0 ];

			return [ 0, 1 ];
		}



		/********************************************************************/
		/// <summary>
		/// Initialize extra channels
		/// </summary>
		/********************************************************************/
		public void ConvertToOutputFormat(MixerBufferInfo[] mixingBuffers, byte[] outputBuffer, int offsetInBytes, int[] channelMapping, int outputChannelCount, int todoInFrames)
		{
			Span<int> buffer = MemoryMarshal.Cast<byte, int>(outputBuffer.AsSpan(offsetInBytes));

			if (channelMapping.Length == 1)
			{
				// Mono
				int[] sourceBuffer1 = mixingBuffers[0].Buffer;
				int[] sourceBuffer2 = mixingBuffers[1].Buffer;

				for (int i = 0; i < todoInFrames; i++)
					buffer[i] = (int)(((long)sourceBuffer1[i] + sourceBuffer2[i]) * 0.707f);
			}
			else
			{
				// Stereo
				int[] sourceBuffer1 = mixingBuffers[channelMapping[0]].Buffer;
				int[] sourceBuffer2 = mixingBuffers[channelMapping[1]].Buffer;

				for (int i = 0, j = 0; i < todoInFrames; i++, j += outputChannelCount)
				{
					buffer[j] = sourceBuffer1[i];
					buffer[j + 1] = sourceBuffer2[i];
				}
			}
		}
	}
}
