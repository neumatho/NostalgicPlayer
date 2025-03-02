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
		public int[] BuildChannelMapping(MixerInfo currentMixerInfo, int mixerChannelCount, int outputChannelCount)
		{
			int[] mapping = new int[mixerChannelCount];

			// Currently, I know that the mixingChannelCount is always 2,
			// so I can optimize the code a bit. This will change when
			// supporting more than 2 channels from a player
			if (outputChannelCount == 1)
			{
				mapping[0] = 0;
				mapping[1] = 0;
			}
			else
			{
				if (currentMixerInfo.SwapSpeakers)
				{
					mapping[0] = 1;
					mapping[1] = 0;
				}
				else
				{
					mapping[0] = 0;
					mapping[1] = 1;
				}
			}

			return mapping;
		}



		/********************************************************************/
		/// <summary>
		/// Convert the mixing buffers to output format
		/// </summary>
		/********************************************************************/
		public void ConvertToOutputFormat(MixerBufferInfo[] mixingBuffers, byte[] outputBuffer, int offsetInBytes, int todoInFrames, int[] channelMapping, int outputChannelCount)
		{
			Span<int> buffer = MemoryMarshal.Cast<byte, int>(outputBuffer.AsSpan(offsetInBytes));

			if (outputChannelCount == 1)
			{
				// Mono
				int[] sourceBuffer1 = mixingBuffers[0].Buffer;
				int[] sourceBuffer2 = mixingBuffers[1].Buffer;

				for (int i = 0; i < todoInFrames; i++)
					buffer[i] = (int)(((long)sourceBuffer1[i] + sourceBuffer2[i]) * 0.707f);
			}
			else
			{
				// Stereo or above
				int[] sourceBuffer1 = mixingBuffers[channelMapping[0]].Buffer;
				int[] sourceBuffer2 = mixingBuffers[channelMapping[1]].Buffer;

				for (int i = 0, j = 0; i < todoInFrames; i++, j += outputChannelCount)
				{
					buffer[j] = sourceBuffer1[i];
					buffer[j + 1] = sourceBuffer2[i];

					for (int k = 2; k < outputChannelCount; k++)
						buffer[j + k] = 0;
				}
			}
		}
	}
}
