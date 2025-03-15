/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.PlayerLibrary.Sound.Mixer.Containers;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Sound
{
	/// <summary>
	/// Can handler down-mixing mixer buffers to output buffers when needed
	/// </summary>
	internal class DownMixer
	{
		private readonly Dictionary<SpeakerFlag, int> playerSpeakerToChannelMap;
		private readonly Dictionary<SpeakerFlag, int> outputSpeakerToChannelMap;
		private readonly Dictionary<int, int> speakerSwappingMap;

		private readonly int realOutputChannelsCount;
		private readonly int outputChannelCountToUse;
		private readonly ChannelFactorDictionary channelFactors;
		private readonly long[][] tempBuffers;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public DownMixer(SpeakerFlag playerSpeakers, SpeakerFlag outputSpeakers)
		{
			playerSpeakerToChannelMap = BuildChannelMap(playerSpeakers);
			outputSpeakerToChannelMap = BuildChannelMap(outputSpeakers);
			speakerSwappingMap = BuildSpeakerSwappingMap();

			realOutputChannelsCount = outputSpeakerToChannelMap.Count;
			outputChannelCountToUse = FindNumberOfOutputChannelsToUse(realOutputChannelsCount);
			channelFactors = FindChannelFactorDictionary();
			tempBuffers = new long[outputChannelCountToUse][];
		}



		/********************************************************************/
		/// <summary>
		/// Convert the mixing buffers to output format
		/// </summary>
		/********************************************************************/
		public void ConvertToOutputFormat(MixerInfo currentMixerInfo, int[][] mixingBuffers, Span<int> outputBuffer, int todoInFrames)
		{
			// Allocate temporary mixing buffers
			if ((tempBuffers[0] == null) || (tempBuffers[0].Length < todoInFrames))
			{
				for (int i = 0; i < outputChannelCountToUse; i++)
					tempBuffers[i] = new long[todoInFrames];
			}
			else
			{
				// Clear the temp buffers
				for (int i = 0; i < outputChannelCountToUse; i++)
					Array.Clear(tempBuffers[i]);
			}

			// Down-mix each channel
			foreach (KeyValuePair<SpeakerFlag, int> outputSpeaker in outputSpeakerToChannelMap)
			{
				if (!channelFactors.TryGetValue(outputSpeaker.Key, out Dictionary<SpeakerFlag, float> factorLookup))
					continue;

				foreach (KeyValuePair<SpeakerFlag, int> inputSpeaker in playerSpeakerToChannelMap)
				{
					if (!factorLookup.TryGetValue(inputSpeaker.Key, out float factor))
						continue;

					if (factor == 0.0f)
						continue;

					int[] sourceBuffer = mixingBuffers[inputSpeaker.Value];
					long[] tempBuffer = tempBuffers[outputSpeaker.Value];

					if (factor == 1.0f)
					{
						for (int i = 0; i < todoInFrames; i++)
							tempBuffer[i] += sourceBuffer[i];
					}
					else
					{
						for (int i = 0; i < todoInFrames; i++)
							tempBuffer[i] += (long)(sourceBuffer[i] * factor);
					}
				}
			}

			// Convert temp buffers to output buffer
			for (int i = 0; i < outputChannelCountToUse; i++)
			{
				long[] sourceBuffer = tempBuffers[i];
				int destIndex = i;

				if (currentMixerInfo.SwapSpeakers && speakerSwappingMap.TryGetValue(i, out int newChannel))
					destIndex = newChannel;

				for (int j = 0; j < todoInFrames; j++, destIndex += realOutputChannelsCount)
				{
					long val = sourceBuffer[j];

					if (val > int.MaxValue)
						val = int.MaxValue;
					else if (val < int.MinValue)
						val = int.MinValue;

					outputBuffer[destIndex] = (int)val;
				}
			}

			// Make sure extra output channels are cleared
			for (int i = outputChannelCountToUse; i < realOutputChannelsCount; i++)
			{
				int destIndex = i;

				for (int j = 0; j < todoInFrames; j++, destIndex += realOutputChannelsCount)
					outputBuffer[destIndex] = 0;
			}

			// If both input and output has subwoofer, copy the subwoofer channel to the output
			if (playerSpeakerToChannelMap.TryGetValue(SpeakerFlag.LowFrequency, out int playerSubwooferChannel) && outputSpeakerToChannelMap.TryGetValue(SpeakerFlag.LowFrequency, out int outputSubwooferChannel))
			{
				int[] sourceBuffer = mixingBuffers[playerSubwooferChannel];
				int destIndex = outputSubwooferChannel;

				for (int i = 0; i < todoInFrames; i++, destIndex += realOutputChannelsCount)
					outputBuffer[destIndex] = sourceBuffer[i];
			}
		}



		/********************************************************************/
		/// <summary>
		/// Build channel mapping table for visualizers.
		/// </summary>
		/********************************************************************/
		public ReadOnlyDictionary<SpeakerFlag, int> GetVisualizersChannelMapping(MixerInfo currentMixerInfo)
		{
			Dictionary<SpeakerFlag, int> visualizerSpeakerToChannelMap = outputSpeakerToChannelMap;

			if (currentMixerInfo.SwapSpeakers)
			{
				visualizerSpeakerToChannelMap = new();

				foreach (KeyValuePair<SpeakerFlag, int> speaker in outputSpeakerToChannelMap)
				{
					if (speakerSwappingMap.TryGetValue(speaker.Value, out int newChannel))
						visualizerSpeakerToChannelMap[speaker.Key] = newChannel;
					else
						visualizerSpeakerToChannelMap[speaker.Key] = speaker.Value;
				}
			}

			return new ReadOnlyDictionary<SpeakerFlag, int>(visualizerSpeakerToChannelMap);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Convert speaker flags into a channel mapping
		/// </summary>
		/********************************************************************/
		private Dictionary<SpeakerFlag, int> BuildChannelMap(SpeakerFlag speakers)
		{
			Dictionary<SpeakerFlag, int> map = new();

			int channel = 0;

			foreach (SpeakerFlag flag in Enum.GetValues<SpeakerFlag>())
			{
				if (speakers.HasFlag(flag))
					map.Add(flag, channel++);
			}

			return map;
		}



		/********************************************************************/
		/// <summary>
		/// Build speaker swapping mapping
		/// </summary>
		/********************************************************************/
		private Dictionary<int, int> BuildSpeakerSwappingMap()
		{
			Dictionary<int, int> map = new();

			if (outputSpeakerToChannelMap.TryGetValue(SpeakerFlag.FrontLeft, out int leftChannel) && outputSpeakerToChannelMap.TryGetValue(SpeakerFlag.FrontRight, out int rightChannel))
			{
				map[leftChannel] = rightChannel;
				map[rightChannel] = leftChannel;
			}

			return map;
		}



		/********************************************************************/
		/// <summary>
		/// We only support up to 7.1 output, so find the real number of
		/// output channels to use
		/// </summary>
		/********************************************************************/
		private int FindNumberOfOutputChannelsToUse(int outputChannelCount)
		{
			if (outputSpeakerToChannelMap.ContainsKey(SpeakerFlag.LowFrequency))
				outputChannelCount--;

			if (outputChannelCount > 7)
				outputChannelCount = 7;

			return outputChannelCount;
		}



		/********************************************************************/
		/// <summary>
		/// Find the channel factor dictionary depending on the number of
		/// output channels. Then check if it matches the output speaker
		/// setup and if not, build a default stereo mapping
		/// </summary>
		/********************************************************************/
		private ChannelFactorDictionary FindChannelFactorDictionary()
		{
			ChannelFactorDictionary factorDictionary = DownMixerTable.ChannelFactors[outputChannelCountToUse - 1];
			SpeakerFlag[] outputSpeakers = outputSpeakerToChannelMap.Keys.Where(x => x != SpeakerFlag.LowFrequency).ToArray();

			foreach (SpeakerFlag outputSpeaker in outputSpeakers)
			{
				if (!factorDictionary.ContainsKey(outputSpeaker))
				{
					// The output speaker setup doesn't match the channel factor dictionary, so build a default stereo mapping
					factorDictionary = new ChannelFactorDictionary();

					if (outputSpeakers.Length == 1)
					{
						factorDictionary[outputSpeakers[0]] = new Dictionary<SpeakerFlag, float>
						{
							{ outputSpeakers[0], 1.0f },
						};
					}
					else
					{
						factorDictionary[outputSpeakers[0]] = new Dictionary<SpeakerFlag, float>
						{
							{ outputSpeakers[0], 1.0f },
							{ outputSpeakers[1], 0.0f }
						};

						factorDictionary[outputSpeakers[1]] = new Dictionary<SpeakerFlag, float>
						{
							{ outputSpeakers[0], 0.0f },
							{ outputSpeakers[1], 1.0f }
						};
					}
					break;
				}
			}

			return factorDictionary;
		}
		#endregion
	}
}
