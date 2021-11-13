/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;
using Polycode.NostalgicPlayer.PlayerLibrary.Containers;
using Polycode.NostalgicPlayer.PlayerLibrary.Mixer;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Resampler
{
	/// <summary>
	/// This class read a block from a sample and resample it to the right output format
	/// </summary>
	internal class Resampler
	{
		private const int FracBits = 11;
		private const int BitShift16 = 16;

		private ISamplePlayerAgent currentPlayer;

		private MixerVisualize currentVisualizer;

		private int inputFrequency;
		private int inputChannels;

		private int outputFrequency;
		private int outputChannels;
		private int outputBits;

		private int masterVolume;

		private int[] dataBuffer;
		private int samplesRead;

		private int[] sampleBuffer;
		private int bufferSize;

		private int dataSize;
		private int currentIndex;
		private int increment;

		private bool swapSpeakers;
		private bool[] channelsEnabled;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Resampler()
		{
			masterVolume = 256;

			swapSpeakers = false;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the resampler routines
		/// </summary>
		/********************************************************************/
		public bool InitResampler(Manager agentManager, PlayerConfiguration playerConfiguration, out string errorMessage)
		{
			errorMessage = string.Empty;
			bool retVal = true;

			try
			{
				// Get the player instance
				currentPlayer = (ISamplePlayerAgent)playerConfiguration.Loader.PlayerAgent;

				inputFrequency = currentPlayer.Frequency;
				inputChannels = currentPlayer.ChannelCount;

				// Allocate the visual component
				currentVisualizer = new MixerVisualize();

				// Initialize the visualizer
				currentVisualizer.Initialize(agentManager, 0, null);

				// Initialize the resampler
				ChangeConfiguration(playerConfiguration.MixerConfiguration);
			}
			catch (Exception ex)
			{
				CleanupResampler();

				errorMessage = string.Format(Resources.IDS_ERR_RESAMPLER_INIT, ex.HResult.ToString("X8"), ex.Message);
				retVal = false;
			}

			return retVal;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup resampler
		/// </summary>
		/********************************************************************/
		public void CleanupResampler()
		{
			// Deallocate the visualizer
			currentVisualizer?.Cleanup();
			currentVisualizer = null;
		}



		/********************************************************************/
		/// <summary>
		/// Set the output format
		/// </summary>
		/********************************************************************/
		public void SetOutputFormat(OutputInfo outputInformation)
		{
			outputFrequency = outputInformation.Frequency;
			outputChannels = outputInformation.Channels;
			outputBits = outputInformation.BytesPerSample * 8;

			// Initialize variables
			samplesRead = 0;

			dataSize = 0;
			currentIndex = 0;
			increment = (inputFrequency << FracBits) / outputFrequency;

			// Get the maximum number of samples the given destination
			// buffer from the output agent can be
			bufferSize = outputInformation.BufferSizeInSamples;

			// Allocate the buffers
			int len = inputFrequency / 2 * inputChannels;		// Around half second buffer size
			if ((len % 2) != 0)
				len++;		// Need to make it even

			dataBuffer = new int[len];
			sampleBuffer = new int[bufferSize];
		}



		/********************************************************************/
		/// <summary>
		/// Will set the master volume
		/// </summary>
		/********************************************************************/
		public void SetMasterVolume(int volume)
		{
			masterVolume = volume;
		}



		/********************************************************************/
		/// <summary>
		/// Will change the mixer configuration
		/// </summary>
		/********************************************************************/
		public void ChangeConfiguration(MixerConfiguration mixerConfiguration)
		{
			swapSpeakers = mixerConfiguration.SwapSpeakers;
			channelsEnabled = mixerConfiguration.ChannelsEnabled;
		}



		/********************************************************************/
		/// <summary>
		/// This is the main resampler method. It's the method that is called
		/// from the ResamplerStream to read the next bunch of data
		/// </summary>
		/********************************************************************/
		public int Resampling(byte[] buffer, int offset, int count, out bool hasEndReached)
		{
			int total = ResampleSample(count, out hasEndReached);

			// And then convert the resampled sample to the output format
			if (outputBits == 16)
				ResampleConvertTo16(MemoryMarshal.Cast<byte, short>(buffer), offset / 2, sampleBuffer, total, outputChannels == 2 ? swapSpeakers : false);
			else if (outputBits == 32)
				ResampleConvertTo32(MemoryMarshal.Cast<byte, int>(buffer), offset / 4, sampleBuffer, total, outputChannels == 2 ? swapSpeakers : false);

			// Tell visual agents about the mixed data
			currentVisualizer.TellAgentsAboutMixedData(buffer, total, outputBits / 8, outputChannels == 2, swapSpeakers);

			return total;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will resample the sample and make sure to read new data when
		/// needed
		/// </summary>
		/********************************************************************/
		private int ResampleSample(int count, out bool hasEndReached)
		{
			hasEndReached = false;

			// Find the size of the buffer
			int bufSize = Math.Min(bufferSize, count);

			// And convert the number of samples to number of samples pair
			count = outputChannels == 2 ? bufSize >> 1 : bufSize;

			int leftVolume = (channelsEnabled == null) || channelsEnabled[0] ? masterVolume : 0;
			int rightVolume = (inputChannels == 2) && ((channelsEnabled == null) || channelsEnabled[1]) ? masterVolume : 0;

			int total = 0;

			while ((count > 0) && !hasEndReached)
			{
				if (currentIndex >= dataSize)
				{
					lock (currentPlayer)
					{
						samplesRead = currentPlayer.LoadDataBlock(dataBuffer, dataBuffer.Length);

						if (currentPlayer.HasEndReached)
						{
							currentPlayer.HasEndReached = false;
							hasEndReached = true;
						}

						if (samplesRead == 0)
							break;
					}

					currentIndex = 0;
					dataSize = samplesRead == 0 ? 0 : ((samplesRead / inputChannels) << FracBits) - 1;
				}

				int todo = Math.Min((dataSize - currentIndex) / increment + 1, Math.Min(samplesRead, count));
				if (todo > 0)
				{
					if (masterVolume > 0)
					{
						if (inputChannels == 1)
						{
							if (outputChannels == 1)
								currentIndex = ResampleMonoToMonoNormal(dataBuffer, sampleBuffer, total, currentIndex, increment, todo, leftVolume);
							else
								currentIndex = ResampleMonoToStereoNormal(dataBuffer, sampleBuffer, total, currentIndex, increment, todo, leftVolume);
						}
						else if (inputChannels == 2)
						{
							if (outputChannels == 1)
								currentIndex = ResampleStereoToMonoNormal(dataBuffer, sampleBuffer, total, currentIndex, increment, todo, leftVolume, rightVolume);
							else
								currentIndex = ResampleStereoToStereoNormal(dataBuffer, sampleBuffer, total, currentIndex, increment, todo, leftVolume, rightVolume);
						}
					}
					else
					{
						Array.Clear(sampleBuffer, total, todo * outputChannels);
						currentIndex += todo * increment;
					}

					count -= todo;
					total += (todo * outputChannels);
				}
			}

			return total;
		}



		/********************************************************************/
		/// <summary>
		/// Resample a mono sample into a mono output buffer
		/// </summary>
		/********************************************************************/
		private int ResampleMonoToMonoNormal(int[] source, int[] dest, int offset, int index, int incr, int todo, int volSel)
		{
			if (volSel == 256)
			{
				while (todo-- != 0)
				{
					int sample = source[index >> FracBits];
					index += incr;

					dest[offset++] = sample;
				}
			}
			else
			{
				float volDiv = 256.0f / volSel;

				while (todo-- != 0)
				{
					int sample = (int)(source[index >> FracBits] / volDiv);
					index += incr;

					dest[offset++] = sample;
				}
			}

			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Resample a mono sample into a stereo output buffer
		/// </summary>
		/********************************************************************/
		private int ResampleMonoToStereoNormal(int[] source, int[] dest, int offset, int index, int incr, int todo, int volSel)
		{
			if (volSel == 256)
			{
				while (todo-- != 0)
				{
					int sample = source[index >> FracBits];
					index += incr;

					dest[offset++] = sample;
					dest[offset++] = sample;
				}
			}
			else
			{
				float volDiv = 256.0f / volSel;

				while (todo-- != 0)
				{
					int sample = (int)(source[index >> FracBits] / volDiv);
					index += incr;

					dest[offset++] = sample;
					dest[offset++] = sample;
				}
			}

			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Resample a stereo sample into a mono output buffer
		/// </summary>
		/********************************************************************/
		private int ResampleStereoToMonoNormal(int[] source, int[] dest, int offset, int index, int incr, int todo, int lVolSel, int rVolSel)
		{
			if ((lVolSel == 256) && (rVolSel == 256))
			{
				while (todo-- != 0)
				{
					long sample1 = source[(index >> FracBits) * 2];
					long sample2 = source[(index >> FracBits) * 2 + 1];
					index += incr;

					dest[offset++] = (int)((sample1 + sample2) / 2);
				}
			}
			else
			{
				float lVolDiv = lVolSel == 0 ? 0f : 256.0f / lVolSel;
				float rVolDiv = lVolSel == 0 ? 0f : 256.0f / rVolSel;

				while (todo-- != 0)
				{
					long sample1 = lVolSel == 0 ? 0 : (int)(source[(index >> FracBits) * 2] / lVolDiv);
					long sample2 = rVolSel == 0 ? 0 : (int)(source[(index >> FracBits) * 2 + 1] / rVolDiv);
					index += incr;

					dest[offset++] = (int)((sample1 + sample2) / 2);
				}
			}

			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Resample a stereo sample into a stereo output buffer
		/// </summary>
		/********************************************************************/
		private int ResampleStereoToStereoNormal(int[] source, int[] dest, int offset, int index, int incr, int todo, int lVolSel, int rVolSel)
		{
			if ((lVolSel == 256) && (rVolSel == 256))
			{
				while (todo-- != 0)
				{
					int sample1 = source[(index >> FracBits) * 2];
					int sample2 = source[(index >> FracBits) * 2 + 1];
					index += incr;

					dest[offset++] = sample1;
					dest[offset++] = sample2;
				}
			}
			else
			{
				float lVolDiv = lVolSel == 0 ? 0f : 256.0f / lVolSel;
				float rVolDiv = lVolSel == 0 ? 0f : 256.0f / rVolSel;

				while (todo-- != 0)
				{
					int sample1 = lVolSel == 0 ? 0 : (int)(source[(index >> FracBits) * 2] / lVolDiv);
					int sample2 = rVolSel == 0 ? 0 : (int)(source[(index >> FracBits) * 2 + 1] / rVolDiv);
					index += incr;

					dest[offset++] = sample1;
					dest[offset++] = sample2;
				}
			}

			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Converts the resampled data to a 16 bit sample buffer
		/// </summary>
		/********************************************************************/
		private void ResampleConvertTo16(Span<short> dest, int offset, int[] source, int count, bool swapSpeakers)
		{
			int x1, x2, x3, x4;

			int remain = count & 3;
			int sourceOffset = 0;

			if (swapSpeakers)
			{
				for (count >>= 2; count != 0; count--)
				{
					x1 = source[sourceOffset++] >> BitShift16;
					x2 = source[sourceOffset++] >> BitShift16;
					x3 = source[sourceOffset++] >> BitShift16;
					x4 = source[sourceOffset++] >> BitShift16;

					dest[offset++] = (short)x2;
					dest[offset++] = (short)x1;
					dest[offset++] = (short)x4;
					dest[offset++] = (short)x3;
				}

				// We know it is always stereo samples when coming here
				while (remain > 0)
				{
					x1 = source[sourceOffset++] >> BitShift16;
					x2 = source[sourceOffset++] >> BitShift16;

					dest[offset++] = (short)x2;
					dest[offset++] = (short)x1;

					remain -= 2;
				}
			}
			else
			{
				for (count >>= 2; count != 0; count--)
				{
					x1 = source[sourceOffset++] >> BitShift16;
					x2 = source[sourceOffset++] >> BitShift16;
					x3 = source[sourceOffset++] >> BitShift16;
					x4 = source[sourceOffset++] >> BitShift16;

					dest[offset++] = (short)x1;
					dest[offset++] = (short)x2;
					dest[offset++] = (short)x3;
					dest[offset++] = (short)x4;
				}

				while (remain-- != 0)
				{
					x1 = source[sourceOffset++] >> BitShift16;
					dest[offset++] = (short)x1;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Converts the resampled data to a 32 bit sample buffer
		/// </summary>
		/********************************************************************/
		private void ResampleConvertTo32(Span<int> dest, int offset, int[] source, int count, bool swapSpeakers)
		{
			int x1, x2, x3, x4;

			int remain = count & 3;
			int sourceOffset = 0;

			if (swapSpeakers)
			{
				for (count >>= 2; count != 0; count--)
				{
					x1 = source[sourceOffset++];
					x2 = source[sourceOffset++];
					x3 = source[sourceOffset++];
					x4 = source[sourceOffset++];

					dest[offset++] = x2;
					dest[offset++] = x1;
					dest[offset++] = x4;
					dest[offset++] = x3;
				}

				// We know it is always stereo samples when coming here
				while (remain > 0)
				{
					x1 = source[sourceOffset++];
					x2 = source[sourceOffset++];

					dest[offset++] = x2;
					dest[offset++] = x1;

					remain -= 2;
				}
			}
			else
			{
				for (count >>= 2; count != 0; count--)
				{
					x1 = source[sourceOffset++];
					x2 = source[sourceOffset++];
					x3 = source[sourceOffset++];
					x4 = source[sourceOffset++];

					dest[offset++] = x1;
					dest[offset++] = x2;
					dest[offset++] = x3;
					dest[offset++] = x4;
				}

				while (remain-- != 0)
				{
					x1 = source[sourceOffset++];
					dest[offset++] = x1;
				}
			}
		}
		#endregion
	}
}
