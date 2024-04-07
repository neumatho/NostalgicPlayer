/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;
using Polycode.NostalgicPlayer.PlayerLibrary.Containers;
using Polycode.NostalgicPlayer.PlayerLibrary.Sound.Timer.Events;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Sound.Resampler
{
	/// <summary>
	/// This class read a block from a sample and resample it to the right output format
	/// </summary>
	internal class Resampler : SoundBase
	{
		private const int FracBits = 11;
		private const int FracMask = ((1 << FracBits) - 1);

		private const int ClickShift = 6;

		private const int BitShift16 = 16;

		private const int MasterVolume = 256;

		private ISamplePlayerAgent currentPlayer;

		private Visualizer currentVisualizer;

		private int inputFrequency;
		private int inputChannels;

		private int outputFrequency;
		private int outputChannels;
		private int mixerChannels;

		private int[] dataBuffer;
		private int samplesRead;

		private int[] sampleBuffer;
		private int bufferSizeInSamples;

		private int dataSize;
		private int currentIndex;
		private int increment;

		private int leftVolume;
		private int rightVolume;
		private int rampVolume;

		private bool interpolation;
		private bool swapSpeakers;
		private bool[] channelsEnabled;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Resampler()
		{
			swapSpeakers = false;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the resampler routines
		/// </summary>
		/********************************************************************/
		public override bool Initialize(Manager agentManager, PlayerConfiguration playerConfiguration, out string errorMessage)
		{
			bool retVal = true;

			try
			{
				if (!base.Initialize(agentManager, playerConfiguration, out errorMessage))
					return false;

				// Get the player instance
				currentPlayer = (ISamplePlayerAgent)playerConfiguration.Loader.PlayerAgent;

				inputFrequency = currentPlayer.Frequency;
				inputChannels = currentPlayer.ChannelCount;

				// Allocate the visual component
				currentVisualizer = new Visualizer();

				// Initialize the visualizer
				currentVisualizer.Initialize(agentManager);

				// Initialize the resampler
				ChangeConfiguration(playerConfiguration.MixerConfiguration);
			}
			catch (Exception ex)
			{
				Cleanup();

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
		public override void Cleanup()
		{
			// Deallocate the visualizer
			currentVisualizer?.Cleanup();
			currentVisualizer = null;

			base.Cleanup();
		}



		/********************************************************************/
		/// <summary>
		/// Pause the resampler routines
		/// </summary>
		/********************************************************************/
		public override void Pause()
		{
			base.Pause();

			currentVisualizer.TellAgentsAboutPauseState(true);
		}



		/********************************************************************/
		/// <summary>
		/// Resume the resampler routines
		/// </summary>
		/********************************************************************/
		public override void Resume()
		{
			base.Resume();

			currentVisualizer.TellAgentsAboutPauseState(false);
		}



		/********************************************************************/
		/// <summary>
		/// Set the output format
		/// </summary>
		/********************************************************************/
		public override void SetOutputFormat(OutputInfo outputInformation)
		{
			base.SetOutputFormat(outputInformation);

			outputFrequency = outputInformation.Frequency;
			outputChannels = outputInformation.Channels;
			mixerChannels = outputChannels >= 2 ? 2 : 1;

			// Initialize variables
			samplesRead = 0;

			dataSize = 0;
			currentIndex = 0;
			increment = (inputFrequency << FracBits) / outputFrequency;

			leftVolume = 0;
			rightVolume = 0;
			rampVolume = 0;

			// Get the maximum number of samples the given destination
			// buffer from the output agent can be
			bufferSizeInSamples = (outputInformation.BufferSizeInSamples / outputChannels) * mixerChannels;

			// Allocate the buffers
			int len = inputFrequency / 2 * inputChannels;		// Around half second buffer size
			if ((len % 2) != 0)
				len++;		// Need to make it even

			dataBuffer = new int[len];
			sampleBuffer = new int[bufferSizeInSamples];

			currentVisualizer.SetOutputFormat(outputInformation);
		}



		/********************************************************************/
		/// <summary>
		/// Will change the mixer configuration
		/// </summary>
		/********************************************************************/
		public override void ChangeConfiguration(MixerConfiguration mixerConfiguration)
		{
			base.ChangeConfiguration(mixerConfiguration);

			currentVisualizer.SetVisualsLatency(mixerConfiguration.VisualsLatency);

			interpolation = mixerConfiguration.EnableInterpolation;
			swapSpeakers = mixerConfiguration.SwapSpeakers;
			channelsEnabled = mixerConfiguration.ChannelsEnabled;
		}



		/********************************************************************/
		/// <summary>
		/// This is the main resampler method. It's the method that is called
		/// from the ResamplerStream to read the next bunch of data
		/// </summary>
		/********************************************************************/
		public int Resampling(byte[] buffer, int offsetInBytes, int countInSamples, out bool hasEndReached)
		{
			// Find the size of the buffer
			//
			// bufferSize = size of mixer buffer for either mono or stereo
			// count = size of output buffer for all channels the output need
			int outputCheckCount = (countInSamples / outputChannels) * mixerChannels;
			int bufSizeInSamples = Math.Min(bufferSizeInSamples, outputCheckCount);

			int totalSamples = ResampleSample(bufSizeInSamples, out hasEndReached);

			// And then convert the resampled sample to the output format
			bool isStereo = outputChannels >= 2;
			int samplesToSkip = isStereo ? outputChannels - 2 : 0;
			ResampleConvertTo32(MemoryMarshal.Cast<byte, int>(buffer), offsetInBytes / 4, sampleBuffer, totalSamples, samplesToSkip, isStereo, isStereo ? swapSpeakers : false);

			totalSamples = (totalSamples / mixerChannels) * outputChannels;

			// Tell visual agents about the mixed data
			currentVisualizer.TellAgentsAboutMixedData(buffer, offsetInBytes, totalSamples, outputChannels, swapSpeakers);

			return totalSamples;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will resample the sample and make sure to read new data when
		/// needed
		/// </summary>
		/********************************************************************/
		private int ResampleSample(int countInSamples, out bool hasEndReached)
		{
			hasEndReached = false;

			// And convert the number of samples to number of samples pair
			countInSamples = outputChannels >= 2 ? countInSamples >> 1 : countInSamples;

			// Get different parameters needed for the resampling
			bool doInterpolation = interpolation;

			int oldLeftVolume = leftVolume;
			int oldRightVolume = rightVolume;

			leftVolume = (channelsEnabled == null) || channelsEnabled[0] ? MasterVolume : 0;
			rightVolume = (inputChannels == 2) && ((channelsEnabled == null) || channelsEnabled[1]) ? MasterVolume : 0;

			// This method is called right after the previous buffer has been player,
			// so at this point, add the number of samples played since last call
			DoTimedEvents();
			IncreaseCurrentTime(countInSamples);

			int totalSamples = 0;

			while ((countInSamples > 0) && !hasEndReached)
			{
				if (currentIndex >= dataSize)
				{
					lock (currentPlayer)
					{
						samplesRead = currentPlayer.LoadDataBlock(dataBuffer, dataBuffer.Length);

						// If any module information has been updated, queue those
						ModuleInfoChanged[] moduleInfoChanges = currentPlayer.GetChangedInformation();
						if (moduleInfoChanges != null)
						{
							foreach (ModuleInfoChanged moduleInfoChanged in moduleInfoChanges)
								timedEventHandler.AddEvent(new ModuleInfoChangedEvent(this, moduleInfoChanged), 0);
						}

						if (currentPlayer.HasEndReached)
						{
							currentPlayer.HasEndReached = false;
							hasEndReached = true;

							if (currentPlayer is IDurationPlayer durationPlayer)
							{
								double restartTime = durationPlayer.GetRestartTime().TotalMilliseconds;
								RestartPosition(restartTime);
							}
							else
								RestartPosition(0);
						}

						if (samplesRead == 0)
							break;
					}

					currentIndex = 0;
					dataSize = samplesRead == 0 ? 0 : ((samplesRead / inputChannels) << FracBits) - 1;
				}

				int todoInSamples = Math.Min((dataSize - currentIndex) / increment + 1, Math.Min(samplesRead, countInSamples));
				if (todoInSamples > 0)
				{
					if (doInterpolation)
					{
						if (inputChannels == 1)
						{
							if (mixerChannels == 1)
								currentIndex = ResampleMonoToMonoInterpolation(dataBuffer, sampleBuffer, totalSamples, currentIndex, increment, todoInSamples, leftVolume, oldLeftVolume, ref rampVolume);
							else
								currentIndex = ResampleMonoToStereoInterpolation(dataBuffer, sampleBuffer, totalSamples, currentIndex, increment, todoInSamples, leftVolume, oldLeftVolume, ref rampVolume);
						}
						else if (inputChannels == 2)
						{
							if (mixerChannels == 1)
								currentIndex = ResampleStereoToMonoInterpolation(dataBuffer, sampleBuffer, totalSamples, currentIndex, increment, todoInSamples, leftVolume, rightVolume, oldLeftVolume, oldRightVolume, ref rampVolume);
							else
								currentIndex = ResampleStereoToStereoInterpolation(dataBuffer, sampleBuffer, totalSamples, currentIndex, increment, todoInSamples, leftVolume, rightVolume, oldLeftVolume, oldRightVolume, ref rampVolume);
						}
					}
					else
					{
						if (inputChannels == 1)
						{
							if (mixerChannels == 1)
								currentIndex = ResampleMonoToMonoNormal(dataBuffer, sampleBuffer, totalSamples, currentIndex, increment, todoInSamples, leftVolume);
							else
								currentIndex = ResampleMonoToStereoNormal(dataBuffer, sampleBuffer, totalSamples, currentIndex, increment, todoInSamples, leftVolume);
						}
						else if (inputChannels == 2)
						{
							if (mixerChannels == 1)
								currentIndex = ResampleStereoToMonoNormal(dataBuffer, sampleBuffer, totalSamples, currentIndex, increment, todoInSamples, leftVolume, rightVolume);
							else
								currentIndex = ResampleStereoToStereoNormal(dataBuffer, sampleBuffer, totalSamples, currentIndex, increment, todoInSamples, leftVolume, rightVolume);
						}
					}

					countInSamples -= todoInSamples;
					totalSamples += (todoInSamples * mixerChannels);
				}
			}

			return totalSamples;
		}

		#region Normal
		/********************************************************************/
		/// <summary>
		/// Resample a mono sample into a mono output buffer
		/// </summary>
		/********************************************************************/
		private int ResampleMonoToMonoNormal(int[] source, int[] dest, int offsetInSamples, int index, int incr, int todoInSamples, int volSel)
		{
			if (volSel == 256)
			{
				while (todoInSamples-- != 0)
				{
					int sample = source[index >> FracBits];
					index += incr;

					dest[offsetInSamples++] = sample;
				}
			}
			else
			{
				float volDiv = 256.0f / volSel;

				while (todoInSamples-- != 0)
				{
					int sample = (int)(source[index >> FracBits] / volDiv);
					index += incr;

					dest[offsetInSamples++] = sample;
				}
			}

			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Resample a mono sample into a stereo output buffer
		/// </summary>
		/********************************************************************/
		private int ResampleMonoToStereoNormal(int[] source, int[] dest, int offsetInSamples, int index, int incr, int todoInSamples, int volSel)
		{
			if (volSel == 256)
			{
				while (todoInSamples-- != 0)
				{
					int sample = source[index >> FracBits];
					index += incr;

					dest[offsetInSamples++] = sample;
					dest[offsetInSamples++] = sample;
				}
			}
			else
			{
				float volDiv = 256.0f / volSel;

				while (todoInSamples-- != 0)
				{
					int sample = (int)(source[index >> FracBits] / volDiv);
					index += incr;

					dest[offsetInSamples++] = sample;
					dest[offsetInSamples++] = sample;
				}
			}

			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Resample a stereo sample into a mono output buffer
		/// </summary>
		/********************************************************************/
		private int ResampleStereoToMonoNormal(int[] source, int[] dest, int offsetInSamples, int index, int incr, int todoInSamples, int lVolSel, int rVolSel)
		{
			if ((lVolSel == 256) && (rVolSel == 256))
			{
				while (todoInSamples-- != 0)
				{
					long sample1 = source[(index >> FracBits) * 2];
					long sample2 = source[(index >> FracBits) * 2 + 1];
					index += incr;

					dest[offsetInSamples++] = (int)((sample1 + sample2) / 2);
				}
			}
			else
			{
				float lVolDiv = lVolSel == 0 ? 0f : 256.0f / lVolSel;
				float rVolDiv = rVolSel == 0 ? 0f : 256.0f / rVolSel;

				while (todoInSamples-- != 0)
				{
					long sample1 = lVolSel == 0 ? 0 : (int)(source[(index >> FracBits) * 2] / lVolDiv);
					long sample2 = rVolSel == 0 ? 0 : (int)(source[(index >> FracBits) * 2 + 1] / rVolDiv);
					index += incr;

					dest[offsetInSamples++] = (int)((sample1 + sample2) / 2);
				}
			}

			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Resample a stereo sample into a stereo output buffer
		/// </summary>
		/********************************************************************/
		private int ResampleStereoToStereoNormal(int[] source, int[] dest, int offsetInSamples, int index, int incr, int todoInSamples, int lVolSel, int rVolSel)
		{
			if ((lVolSel == 256) && (rVolSel == 256))
			{
				while (todoInSamples-- != 0)
				{
					int sample1 = source[(index >> FracBits) * 2];
					int sample2 = source[(index >> FracBits) * 2 + 1];
					index += incr;

					dest[offsetInSamples++] = sample1;
					dest[offsetInSamples++] = sample2;
				}
			}
			else
			{
				float lVolDiv = lVolSel == 0 ? 0f : 256.0f / lVolSel;
				float rVolDiv = rVolSel == 0 ? 0f : 256.0f / rVolSel;

				while (todoInSamples-- != 0)
				{
					int sample1 = lVolSel == 0 ? 0 : (int)(source[(index >> FracBits) * 2] / lVolDiv);
					int sample2 = rVolSel == 0 ? 0 : (int)(source[(index >> FracBits) * 2 + 1] / rVolDiv);
					index += incr;

					dest[offsetInSamples++] = sample1;
					dest[offsetInSamples++] = sample2;
				}
			}

			return index;
		}
		#endregion

		#region Interpolation
		/********************************************************************/
		/// <summary>
		/// Resample a mono sample into a mono output buffer with
		/// interpolation
		/// </summary>
		/********************************************************************/
		private int ResampleMonoToMonoInterpolation(int[] source, int[] dest, int offsetInSamples, int index, int incr, int todoInSamples, int volSel, int oldVol, ref int rampVol)
		{
			int len = source.Length;

			float volDiv = volSel == 0 ? 0f : 256.0f / volSel;

			if (rampVol != 0)
			{
				oldVol -= volSel;

				while (todoInSamples-- != 0)
				{
					int idx = index >> FracBits;
					if (idx >= len)
						break;

					long a = source[idx];
					long b = idx + 1 >= len ? a : source[idx + 1];

					long sample = volSel == 0 ? 0 : (long)((a + ((b - a) * (index & FracMask) >> FracBits)) / volDiv);
					if (sample < -0x7fffffff)
						sample = -0x7fffffff;
					else if (sample > 0x7fffffff)
						sample = 0x7fffffff;

					index += incr;

					dest[offsetInSamples++] = (int)(((volSel << ClickShift) + oldVol * rampVol) * sample >> ClickShift);

					if (--rampVol == 0)
						break;
				}

				if (todoInSamples < 0)
					return index;
			}

			while (todoInSamples-- != 0)
			{
				int idx = index >> FracBits;
				if (idx >= len)
					break;

				long a = source[idx];
				long b = idx + 1 >= len ? a : source[idx + 1];

				long sample = volSel == 0 ? 0 : (long)((a + ((b - a) * (index & FracMask) >> FracBits)) / volDiv);
				if (sample < -0x7fffffff)
					sample = -0x7fffffff;
				else if (sample > 0x7fffffff)
					sample = 0x7fffffff;

				index += incr;

				dest[offsetInSamples++] = (int)sample;
			}

			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Resample a mono sample into a stereo output buffer with
		/// interpolation
		/// </summary>
		/********************************************************************/
		private int ResampleMonoToStereoInterpolation(int[] source, int[] dest, int offsetInSamples, int index, int incr, int todoInSamples, int volSel, int oldVol, ref int rampVol)
		{
			int len = source.Length;

			float volDiv = volSel == 0 ? 0f : 256.0f / volSel;

			if (rampVol != 0)
			{
				oldVol -= volSel;

				while (todoInSamples-- != 0)
				{
					int idx = index >> FracBits;
					if (idx >= len)
						break;

					long a = source[idx];
					long b = idx + 1 >= len ? a : source[idx + 1];

					long sample = volSel == 0 ? 0 : (long)((a + ((b - a) * (index & FracMask) >> FracBits)) / volDiv);
					if (sample < -0x7fffffff)
						sample = -0x7fffffff;
					else if (sample > 0x7fffffff)
						sample = 0x7fffffff;

					index += incr;

					sample = ((volSel << ClickShift) + oldVol * rampVol) * sample >> ClickShift;

					dest[offsetInSamples++] = (int)sample;
					dest[offsetInSamples++] = (int)sample;

					if (--rampVol == 0)
						break;
				}

				if (todoInSamples < 0)
					return index;
			}

			while (todoInSamples-- != 0)
			{
				int idx = index >> FracBits;
				if (idx >= len)
					break;

				long a = source[idx];
				long b = idx + 1 >= len ? a : source[idx + 1];

				long sample = volSel == 0 ? 0 : (long)((a + ((b - a) * (index & FracMask) >> FracBits)) / volDiv);
				if (sample < -0x7fffffff)
					sample = -0x7fffffff;
				else if (sample > 0x7fffffff)
					sample = 0x7fffffff;

				index += incr;

				dest[offsetInSamples++] = (int)sample;
				dest[offsetInSamples++] = (int)sample;
			}

			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Resample a stereo sample into a mono output buffer with
		/// interpolation
		/// </summary>
		/********************************************************************/
		private int ResampleStereoToMonoInterpolation(int[] source, int[] dest, int offsetInSamples, int index, int incr, int todoInSamples, int lVolSel, int rVolSel, int oldLVol, int oldRVol, ref int rampVol)
		{
			int len = source.Length;

			float lVolDiv = lVolSel == 0 ? 0f : 256.0f / lVolSel;
			float rVolDiv = rVolSel == 0 ? 0f : 256.0f / rVolSel;

			if (rampVol != 0)
			{
				oldLVol -= lVolSel;
				oldRVol -= rVolSel;

				while (todoInSamples-- != 0)
				{
					int idx = (index >> FracBits) * 2;
					if (idx >= len)
						break;

					long a = source[idx];
					long b = idx + 2 >= len ? a : source[idx + 2];

					long sample1 = lVolSel == 0 ? 0 : (long)((a + ((b - a) * (index & FracMask) >> FracBits)) / lVolDiv);
					if (sample1 < -0x7fffffff)
						sample1 = -0x7fffffff;
					else if (sample1 > 0x7fffffff)
						sample1 = 0x7fffffff;

					a = source[idx + 1];
					b = idx + 3 >= len ? a : source[idx + 3];

					long sample2 = rVolSel == 0 ? 0 : (long)((a + ((b - a) * (index & FracMask) >> FracBits)) / rVolDiv);
					if (sample2 < -0x7fffffff)
						sample2 = -0x7fffffff;
					else if (sample2 > 0x7fffffff)
						sample2 = 0x7fffffff;

					index += incr;

					dest[offsetInSamples++] = (int)(((((lVolSel << ClickShift) + oldLVol * rampVol) * sample1 >> ClickShift) + (((rVolSel << ClickShift) + oldRVol * rampVol) * sample2 >> ClickShift)) / 2);

					if (--rampVol == 0)
						break;
				}

				if (todoInSamples < 0)
					return index;
			}

			while (todoInSamples-- != 0)
			{
				int idx = (index >> FracBits) * 2;
				if (idx >= len)
					break;

				long a = source[idx];
				long b = idx + 2 >= len ? a : source[idx + 2];

				long sample1 = lVolSel == 0 ? 0 : (long)((a + ((b - a) * (index & FracMask) >> FracBits)) / lVolDiv);
				if (sample1 < -0x7fffffff)
					sample1 = -0x7fffffff;
				else if (sample1 > 0x7fffffff)
					sample1 = 0x7fffffff;

				a = source[idx + 1];
				b = idx + 3 >= len ? a : source[idx + 3];

				long sample2 = rVolSel == 0 ? 0 : (long)((a + ((b - a) * (index & FracMask) >> FracBits)) / rVolDiv);
				if (sample2 < -0x7fffffff)
					sample2 = -0x7fffffff;
				else if (sample2 > 0x7fffffff)
					sample2 = 0x7fffffff;

				index += incr;

				dest[offsetInSamples++] = (int)((sample1 + sample2) / 2);
			}

			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Resample a stereo sample into a stereo output buffer with
		/// interpolation
		/// </summary>
		/********************************************************************/
		private int ResampleStereoToStereoInterpolation(int[] source, int[] dest, int offsetInSamples, int index, int incr, int todoInSamples, int lVolSel, int rVolSel, int oldLVol, int oldRVol, ref int rampVol)
		{
			int len = source.Length;

			float lVolDiv = lVolSel == 0 ? 0f : 256.0f / lVolSel;
			float rVolDiv = rVolSel == 0 ? 0f : 256.0f / rVolSel;

			if (rampVol != 0)
			{
				oldLVol -= lVolSel;
				oldRVol -= rVolSel;

				while (todoInSamples-- != 0)
				{
					int idx = (index >> FracBits) * 2;
					if (idx >= len)
						break;

					long a = source[idx];
					long b = idx + 2 >= len ? a : source[idx + 2];

					long sample1 = lVolSel == 0 ? 0 : (long)((a + ((b - a) * (index & FracMask) >> FracBits)) / lVolDiv);
					if (sample1 < -0x7fffffff)
						sample1 = -0x7fffffff;
					else if (sample1 > 0x7fffffff)
						sample1 = 0x7fffffff;

					a = source[idx + 1];
					b = idx + 3 >= len ? a : source[idx + 3];

					long sample2 = rVolSel == 0 ? 0 : (long)((a + ((b - a) * (index & FracMask) >> FracBits)) / rVolDiv);
					if (sample2 < -0x7fffffff)
						sample2 = -0x7fffffff;
					else if (sample2 > 0x7fffffff)
						sample2 = 0x7fffffff;

					index += incr;

					dest[offsetInSamples++] = (int)(((lVolSel << ClickShift) + oldLVol * rampVol) * sample1 >> ClickShift);
					dest[offsetInSamples++] = (int)(((rVolSel << ClickShift) + oldRVol * rampVol) * sample2 >> ClickShift);

					if (--rampVol == 0)
						break;
				}

				if (todoInSamples < 0)
					return index;
			}

			while (todoInSamples-- != 0)
			{
				int idx = (index >> FracBits) * 2;
				if (idx >= len)
					break;

				long a = source[idx];
				long b = idx + 2 >= len ? a : source[idx + 2];

				long sample1 = lVolSel == 0 ? 0 : (long)((a + ((b - a) * (index & FracMask) >> FracBits)) / lVolDiv);
				if (sample1 < -0x7fffffff)
					sample1 = -0x7fffffff;
				else if (sample1 > 0x7fffffff)
					sample1 = 0x7fffffff;

				a = source[idx + 1];
				b = idx + 3 >= len ? a : source[idx + 3];

				long sample2 = rVolSel == 0 ? 0 : (long)((a + ((b - a) * (index & FracMask) >> FracBits)) / rVolDiv);
				if (sample2 < -0x7fffffff)
					sample2 = -0x7fffffff;
				else if (sample2 > 0x7fffffff)
					sample2 = 0x7fffffff;

				index += incr;

				dest[offsetInSamples++] = (int)sample1;
				dest[offsetInSamples++] = (int)sample2;
			}

			return index;
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Converts the resampled data to a 32 bit sample buffer
		/// </summary>
		/********************************************************************/
		private void ResampleConvertTo32(Span<int> dest, int offsetInSamples, int[] source, int countInSamples, int samplesToSkip, bool isStereo, bool swap)
		{
			int x1, x2, x3, x4;
			int remain;

			int sourceOffset = 0;

			if (swap)
			{
				if (samplesToSkip == 0)
				{
					remain = countInSamples & 3;

					for (countInSamples >>= 2; countInSamples != 0; countInSamples--)
					{
						x1 = source[sourceOffset++];
						x2 = source[sourceOffset++];
						x3 = source[sourceOffset++];
						x4 = source[sourceOffset++];

						dest[offsetInSamples++] = x2;
						dest[offsetInSamples++] = x1;
						dest[offsetInSamples++] = x4;
						dest[offsetInSamples++] = x3;
					}
				}
				else
				{
					remain = countInSamples & 1;

					for (countInSamples >>= 1; countInSamples != 0; countInSamples--)
					{
						x1 = source[sourceOffset++];
						x2 = source[sourceOffset++];

						dest[offsetInSamples++] = x2;
						dest[offsetInSamples++] = x1;

						for (int i = 0; i < samplesToSkip; i++)
							dest[offsetInSamples++] = 0;
					}
				}
			}
			else
			{
				if (isStereo)
				{
					if (samplesToSkip == 0)
					{
						remain = countInSamples & 3;

						for (countInSamples >>= 2; countInSamples != 0; countInSamples--)
						{
							x1 = source[sourceOffset++];
							x2 = source[sourceOffset++];
							x3 = source[sourceOffset++];
							x4 = source[sourceOffset++];

							dest[offsetInSamples++] = x1;
							dest[offsetInSamples++] = x2;
							dest[offsetInSamples++] = x3;
							dest[offsetInSamples++] = x4;
						}
					}
					else
					{
						remain = countInSamples & 1;

						for (countInSamples >>= 1; countInSamples != 0; countInSamples--)
						{
							x1 = source[sourceOffset++];
							x2 = source[sourceOffset++];

							dest[offsetInSamples++] = x1;
							dest[offsetInSamples++] = x2;

							for (int i = 0; i < samplesToSkip; i++)
								dest[offsetInSamples++] = 0;
						}
					}
				}
				else
				{
					remain = 0;

					for (; countInSamples != 0; countInSamples--)
						dest[offsetInSamples++] = source[sourceOffset++];
				}
			}

			while (remain-- != 0)
				dest[offsetInSamples++] = source[sourceOffset++];
		}
		#endregion
	}
}
