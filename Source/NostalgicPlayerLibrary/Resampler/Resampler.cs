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
using Polycode.NostalgicPlayer.PlayerLibrary.Containers;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Resampler
{
	/// <summary>
	/// This class read a block from a sample and resample it to the right output format
	/// </summary>
	internal class Resampler
	{
		private ISamplePlayerAgent currentPlayer;

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
		public bool InitResampler(PlayerConfiguration playerConfiguration, out string errorMessage)
		{
			errorMessage = string.Empty;
			bool retVal = true;

			try
			{
				// Get the player instance
				currentPlayer = (ISamplePlayerAgent)playerConfiguration.Loader.PlayerAgent;

				inputFrequency = currentPlayer.Frequency;
				inputChannels = currentPlayer.ChannelCount;

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

			currentIndex = 0;
			increment = (inputFrequency << Native.FRACBITS) / outputFrequency;

			// Get the maximum number of samples the given destination
			// buffer from the output agent can be
			bufferSize = outputInformation.BufferSizeInSamples;

			// Allocate the buffers
			dataBuffer = new int[inputFrequency / 2 * inputChannels];		// Around half second buffer size
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
				Native.ResampleConvertTo16(buffer, offset / 2, sampleBuffer, total, outputChannels == 2 ? swapSpeakers : false);
			else if (outputBits == 32)
				Native.ResampleConvertTo32(buffer, offset / 4, sampleBuffer, total, outputChannels == 2 ? swapSpeakers : false);

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
					}

					currentIndex = 0;
					dataSize = samplesRead == 0 ? 0 : ((samplesRead / inputChannels) << Native.FRACBITS) - 1;
				}

				int todo = Math.Min((dataSize - currentIndex) / increment + 1, Math.Min(samplesRead, count));
				if (todo > 0)
				{
					if (masterVolume > 0)
					{
						// Resample the input sample
						GCHandle pinnedBuf = GCHandle.Alloc(sampleBuffer, GCHandleType.Pinned);

						try
						{
							IntPtr bufAddr = pinnedBuf.AddrOfPinnedObject();

							if (inputChannels == 1)
							{
								if (outputChannels == 1)
									currentIndex = Native.ResampleMonoToMonoNormal(dataBuffer, bufAddr, total, currentIndex, increment, todo, (channelsEnabled == null) || channelsEnabled[0] ? masterVolume : 0);
								else
									currentIndex = Native.ResampleMonoToStereoNormal(dataBuffer, bufAddr, total, currentIndex, increment, todo, (channelsEnabled == null) || channelsEnabled[0] ? masterVolume : 0);
							}
							else if (inputChannels == 2)
							{
								if (outputChannels == 1)
									currentIndex = Native.ResampleStereoToMonoNormal(dataBuffer, bufAddr, total, currentIndex, increment, todo, (channelsEnabled == null) || channelsEnabled[0] ? masterVolume : 0, (channelsEnabled == null) || channelsEnabled[1] ? masterVolume : 0);
								else
									currentIndex = Native.ResampleStereoToStereoNormal(dataBuffer, bufAddr, total, currentIndex, increment, todo, (channelsEnabled == null) || channelsEnabled[0] ? masterVolume : 0, (channelsEnabled == null) || channelsEnabled[1] ? masterVolume : 0);
							}
						}
						finally
						{
							pinnedBuf.Free();
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
		#endregion
	}
}
