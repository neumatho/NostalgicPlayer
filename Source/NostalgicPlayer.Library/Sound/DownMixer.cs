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
using Polycode.NostalgicPlayer.Library.Sound.Mixer.Containers;

namespace Polycode.NostalgicPlayer.Library.Sound
{
	/// <summary>
	/// Can handler down-mixing mixer buffers to output buffers when needed
	/// </summary>
	internal class DownMixer
	{
		private readonly ReadOnlyDictionary<SpeakerFlag, int> playerSpeakerToChannelMap;
		private readonly ReadOnlyDictionary<SpeakerFlag, int> outputSpeakerToChannelMap;
		private readonly Dictionary<int, int> speakerSwappingMap;

		private readonly int realOutputChannelsCount;
		private readonly int outputChannelCountToUse;
		private readonly bool hasSubwoofer;
		private readonly ChannelFactorDictionary channelFactors;
		private readonly SpeakerFlag visualizerSpeakers;

		private readonly Dictionary<int, long[]> tempBuffers;
		private int tempBufferSize;

		private float adjustVolumeBy;

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
			hasSubwoofer = playerSpeakerToChannelMap.ContainsKey(SpeakerFlag.LowFrequency) && outputSpeakerToChannelMap.ContainsKey(SpeakerFlag.LowFrequency);
			channelFactors = FindChannelFactorDictionary();
			visualizerSpeakers = FindVisualizerSpeakers();

			tempBuffers = outputSpeakerToChannelMap
				.Where(x => x.Key != SpeakerFlag.LowFrequency)
				.ToDictionary(x => x.Value, x => (long[])null);
			tempBufferSize = 0;

			adjustVolumeBy = 1.0f;
		}



		/********************************************************************/
		/// <summary>
		/// Return the channel mapping for player speakers
		/// </summary>
		/********************************************************************/
		public ReadOnlyDictionary<SpeakerFlag, int> PlayerSpeakerToChannelMap => playerSpeakerToChannelMap;



		/********************************************************************/
		/// <summary>
		/// Convert the mixing buffers to output format
		/// </summary>
		/********************************************************************/
		public void ConvertToOutputFormat(MixerInfo currentMixerInfo, int[][] mixingBuffers, Span<int> outputBuffer, int todoInFrames)
		{
			// Allocate temporary mixing buffers
			if (tempBufferSize < todoInFrames)
			{
				foreach (KeyValuePair<int, long[]> pair in tempBuffers)
					tempBuffers[pair.Key] = new long[todoInFrames];

				tempBufferSize = todoInFrames;
			}
			else
			{
				// Clear the temp buffers
				foreach (long[] buffer in tempBuffers.Values)
					Array.Clear(buffer);
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
			HashSet<int> channelsToClear = outputSpeakerToChannelMap.Values.ToHashSet();

			foreach (int outputSpeakerIndex in outputSpeakerToChannelMap.Values)
			{
				if (tempBuffers.TryGetValue(outputSpeakerIndex, out long[] sourceBuffer))
				{
					int destIndex = outputSpeakerIndex;

					if (currentMixerInfo.SwapSpeakers && speakerSwappingMap.TryGetValue(outputSpeakerIndex, out int newChannel))
						destIndex = newChannel;

					for (int j = 0; j < todoInFrames; j++, destIndex += realOutputChannelsCount)
					{
						long val;

						for(;;)
						{
							val = (long)(sourceBuffer[j] * adjustVolumeBy);

							if ((val <= int.MaxValue) && (val >= int.MinValue))
								break;

							float newVal = (float)int.MaxValue / Math.Abs(sourceBuffer[j]);
							if (newVal == adjustVolumeBy)
								newVal -= 0.0001f;

							adjustVolumeBy = newVal;
						}

						outputBuffer[destIndex] = (int)val;
					}

					channelsToClear.Remove(outputSpeakerIndex);
				}
			}

			// If both input and output has subwoofer, copy the subwoofer channel to the output
			if (playerSpeakerToChannelMap.TryGetValue(SpeakerFlag.LowFrequency, out int playerSubwooferChannel) && outputSpeakerToChannelMap.TryGetValue(SpeakerFlag.LowFrequency, out int outputSubwooferChannel))
			{
				channelsToClear.Remove(outputSubwooferChannel);

				int[] sourceBuffer = mixingBuffers[playerSubwooferChannel];
				int destIndex = outputSubwooferChannel;

				for (int i = 0; i < todoInFrames; i++, destIndex += realOutputChannelsCount)
					outputBuffer[destIndex] = sourceBuffer[i];
			}

			// Make sure extra output channels are cleared
			foreach (int outputSpeakerIndex in channelsToClear)
			{
				int destIndex = outputSpeakerIndex;

				for (int j = 0; j < todoInFrames; j++, destIndex += realOutputChannelsCount)
					outputBuffer[destIndex] = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return which speakers that are used to play the sound
		/// </summary>
		/********************************************************************/
		public SpeakerFlag VisualizerSpeakers => visualizerSpeakers;



		/********************************************************************/
		/// <summary>
		/// Build channel mapping table for visualizers.
		/// </summary>
		/********************************************************************/
		public ReadOnlyDictionary<SpeakerFlag, int> GetVisualizersChannelMapping(MixerInfo currentMixerInfo)
		{
			Dictionary<SpeakerFlag, int> visualizerSpeakerToChannelMap = outputSpeakerToChannelMap.Where(x => visualizerSpeakers.HasFlag(x.Key)).ToDictionary();

			if (currentMixerInfo.SwapSpeakers)
			{
				Dictionary<SpeakerFlag, int> newVisualizerSpeakerToChannelMap = new();

				foreach (KeyValuePair<SpeakerFlag, int> speaker in visualizerSpeakerToChannelMap)
				{
					if (speakerSwappingMap.TryGetValue(speaker.Value, out int newChannel))
						newVisualizerSpeakerToChannelMap[speaker.Key] = newChannel;
					else
						newVisualizerSpeakerToChannelMap[speaker.Key] = speaker.Value;
				}

				visualizerSpeakerToChannelMap = newVisualizerSpeakerToChannelMap;
			}

			return new ReadOnlyDictionary<SpeakerFlag, int>(visualizerSpeakerToChannelMap);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Convert speaker flags into a channel mapping
		/// </summary>
		/********************************************************************/
		private ReadOnlyDictionary<SpeakerFlag, int> BuildChannelMap(SpeakerFlag speakers)
		{
			Dictionary<SpeakerFlag, int> map = new();

			int channel = 0;

			foreach (SpeakerFlag flag in Enum.GetValues<SpeakerFlag>())
			{
				if (speakers.HasFlag(flag))
					map.Add(flag, channel++);
			}

			return new ReadOnlyDictionary<SpeakerFlag, int>(map);
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



		/********************************************************************/
		/// <summary>
		/// Will return which speakers there actually will have output data
		/// </summary>
		/********************************************************************/
		private SpeakerFlag FindVisualizerSpeakers()
		{
			SpeakerFlag result = 0;

			foreach (SpeakerFlag playerSpeaker in playerSpeakerToChannelMap.Keys)
			{
				foreach (KeyValuePair<SpeakerFlag, Dictionary<SpeakerFlag, float>> pair in channelFactors)
				{
					if (pair.Value.TryGetValue(playerSpeaker, out float factor) && (factor != 0.0f))
						result |= pair.Key;
				}
			}

			if (hasSubwoofer)
				result |= SpeakerFlag.LowFrequency;

			return result;
		}
		#endregion
	}
}
