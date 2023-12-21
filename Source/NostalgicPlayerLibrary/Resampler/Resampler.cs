/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Events;
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
		private const int FracMask = ((1 << FracBits) - 1);

		private const int ClickShift = 6;

		private const int BitShift16 = 16;

		private const int MasterVolume = 256;

		private ISamplePlayerAgent currentPlayer;

		private MixerVisualize currentVisualizer;

		private int inputFrequency;
		private int inputChannels;

		private int outputFrequency;
		private int outputChannels;
		private int mixerChannels;

		private int[] dataBuffer;
		private int samplesRead;

		private int[] sampleBuffer;
		private int bufferSize;

		private int dataSize;
		private int currentIndex;
		private int increment;

		private int leftVolume;
		private int rightVolume;
		private int rampVolume;

		private bool interpolation;
		private bool swapSpeakers;
		private bool[] channelsEnabled;

		private int samplesLeftToPositionChange;
		private int currentSongPosition;

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
				currentVisualizer.Initialize(agentManager);

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
		/// Starts the resampler routines
		/// </summary>
		/********************************************************************/
		public void StartResampler()
		{
			samplesLeftToPositionChange = 0;
			currentSongPosition = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Stops the resampler routines
		/// </summary>
		/********************************************************************/
		public void StopResampler()
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
			mixerChannels = outputChannels >= 2 ? 2 : 1;

			// Initialize variables
			samplesRead = 0;

			dataSize = 0;
			currentIndex = 0;
			increment = (inputFrequency << FracBits) / outputFrequency;

			leftVolume = 0;
			rightVolume = 0;
			rampVolume = 0;

			samplesLeftToPositionChange = CalculateSamplesPerPositionChange();

			// Get the maximum number of samples the given destination
			// buffer from the output agent can be
			bufferSize = (outputInformation.BufferSizeInSamples / outputChannels) * mixerChannels;

			// Allocate the buffers
			int len = inputFrequency / 2 * inputChannels;		// Around half second buffer size
			if ((len % 2) != 0)
				len++;		// Need to make it even

			dataBuffer = new int[len];
			sampleBuffer = new int[bufferSize];

			currentVisualizer.SetOutputFormat(outputInformation);
		}



		/********************************************************************/
		/// <summary>
		/// Will change the mixer configuration
		/// </summary>
		/********************************************************************/
		public void ChangeConfiguration(MixerConfiguration mixerConfiguration)
		{
			currentVisualizer.SetVisualsLatency(mixerConfiguration.VisualsLatency);

			interpolation = mixerConfiguration.EnableInterpolation;
			swapSpeakers = mixerConfiguration.SwapSpeakers;
			channelsEnabled = mixerConfiguration.ChannelsEnabled;
		}



		/********************************************************************/
		/// <summary>
		/// Get current song position
		/// </summary>
		/********************************************************************/
		public int SongPosition
		{
			get => currentSongPosition;

			set
			{
				currentSongPosition = value;
				samplesLeftToPositionChange = CalculateSamplesPerPositionChange();
			}
		}



		/********************************************************************/
		/// <summary>
		/// This is the main resampler method. It's the method that is called
		/// from the ResamplerStream to read the next bunch of data
		/// </summary>
		/********************************************************************/
		public int Resampling(byte[] buffer, int offset, int count, out bool hasEndReached)
		{
			// Find the size of the buffer
			//
			// bufferSize = size of mixer buffer for either mono or stereo
			// count = size of output buffer for all channels the output need
			int outputCheckCount = (count / outputChannels) * mixerChannels;
			int bufSize = Math.Min(bufferSize, outputCheckCount);

			int total = ResampleSample(bufSize, out hasEndReached);

			// And then convert the resampled sample to the output format
			bool isStereo = outputChannels >= 2;
			int samplesToSkip = isStereo ? outputChannels - 2 : 0;
			ResampleConvertTo32(MemoryMarshal.Cast<byte, int>(buffer), offset / 4, sampleBuffer, total, samplesToSkip, isStereo, isStereo ? swapSpeakers : false);

			total = (total / mixerChannels) * outputChannels;

			// Tell visual agents about the mixed data
			currentVisualizer.TellAgentsAboutMixedData(buffer, offset, total, outputChannels, swapSpeakers);

			// Update module information
			foreach (ModuleInfoChanged[] updatedModuleInfoChanges in currentVisualizer.GetModuleInfoChanges(total))
			{
				foreach (ModuleInfoChanged changes in updatedModuleInfoChanges)
					OnModuleInfoChanged(new ModuleInfoChangedEventArgs(changes.Line, changes.Value));
			}

			return total;
		}



		/********************************************************************/
		/// <summary>
		/// Event called when the position change
		/// </summary>
		/********************************************************************/
		public event EventHandler PositionChanged;



		/********************************************************************/
		/// <summary>
		/// Event called when the player update some module information
		/// </summary>
		/********************************************************************/
		public event ModuleInfoChangedEventHandler ModuleInfoChanged;

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Return the number of samples between each position change
		/// </summary>
		/********************************************************************/
		private int CalculateSamplesPerPositionChange()
		{
			return (int)(IDurationPlayer.NumberOfSecondsBetweenEachSnapshot * outputFrequency);
		}



		/********************************************************************/
		/// <summary>
		/// Will resample the sample and make sure to read new data when
		/// needed
		/// </summary>
		/********************************************************************/
		private int ResampleSample(int count, out bool hasEndReached)
		{
			hasEndReached = false;

			// And convert the number of samples to number of samples pair
			count = outputChannels >= 2 ? count >> 1 : count;

			// Get different parameters needed for the resampling
			bool doInterpolation = interpolation;

			int oldLeftVolume = leftVolume;
			int oldRightVolume = rightVolume;

			leftVolume = (channelsEnabled == null) || channelsEnabled[0] ? MasterVolume : 0;
			rightVolume = (inputChannels == 2) && ((channelsEnabled == null) || channelsEnabled[1]) ? MasterVolume : 0;

			int total = 0;

			while ((count > 0) && !hasEndReached)
			{
				if (currentIndex >= dataSize)
				{
					lock (currentPlayer)
					{
						while (samplesLeftToPositionChange <= 0)
						{
							currentSongPosition++;
							samplesLeftToPositionChange += CalculateSamplesPerPositionChange();

							OnPositionChanged();
						}

						samplesRead = currentPlayer.LoadDataBlock(dataBuffer, dataBuffer.Length);

						// If any module information has been updated, queue those
						ModuleInfoChanged[] moduleInfoChanges = currentPlayer.GetChangedInformation();
						if ((moduleInfoChanges != null) && (moduleInfoChanges.Length > 0))
							currentVisualizer.QueueModuleInfoChange(moduleInfoChanges, 0);

						if (currentPlayer.HasEndReached)
						{
							currentPlayer.HasEndReached = false;
							hasEndReached = true;

							if (currentPlayer is IDurationPlayer durationPlayer)
							{
								double restartTime = durationPlayer.GetRestartTime().TotalMilliseconds;
								int samplesPerPosition = CalculateSamplesPerPositionChange();

								currentSongPosition = (int)(restartTime / (IDurationPlayer.NumberOfSecondsBetweenEachSnapshot * 1000.0f));
								samplesLeftToPositionChange = (int)(samplesPerPosition - (((restartTime - (currentSongPosition * IDurationPlayer.NumberOfSecondsBetweenEachSnapshot * 1000.0f)) / 1000.0f) * outputFrequency));
							}
							else
							{
								samplesLeftToPositionChange = CalculateSamplesPerPositionChange();
								currentSongPosition = 0;
							}

							OnPositionChanged();
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
					if (doInterpolation)
					{
						if (inputChannels == 1)
						{
							if (mixerChannels == 1)
								currentIndex = ResampleMonoToMonoInterpolation(dataBuffer, sampleBuffer, total, currentIndex, increment, todo, leftVolume, oldLeftVolume, ref rampVolume);
							else
								currentIndex = ResampleMonoToStereoInterpolation(dataBuffer, sampleBuffer, total, currentIndex, increment, todo, leftVolume, oldLeftVolume, ref rampVolume);
						}
						else if (inputChannels == 2)
						{
							if (mixerChannels == 1)
								currentIndex = ResampleStereoToMonoInterpolation(dataBuffer, sampleBuffer, total, currentIndex, increment, todo, leftVolume, rightVolume, oldLeftVolume, oldRightVolume, ref rampVolume);
							else
								currentIndex = ResampleStereoToStereoInterpolation(dataBuffer, sampleBuffer, total, currentIndex, increment, todo, leftVolume, rightVolume, oldLeftVolume, oldRightVolume, ref rampVolume);
						}
					}
					else
					{
						if (inputChannels == 1)
						{
							if (mixerChannels == 1)
								currentIndex = ResampleMonoToMonoNormal(dataBuffer, sampleBuffer, total, currentIndex, increment, todo, leftVolume);
							else
								currentIndex = ResampleMonoToStereoNormal(dataBuffer, sampleBuffer, total, currentIndex, increment, todo, leftVolume);
						}
						else if (inputChannels == 2)
						{
							if (mixerChannels == 1)
								currentIndex = ResampleStereoToMonoNormal(dataBuffer, sampleBuffer, total, currentIndex, increment, todo, leftVolume, rightVolume);
							else
								currentIndex = ResampleStereoToStereoNormal(dataBuffer, sampleBuffer, total, currentIndex, increment, todo, leftVolume, rightVolume);
						}
					}

					samplesLeftToPositionChange -= todo;
					count -= todo;
					total += (todo * mixerChannels);
				}
			}

			return total;
		}

		#region Normal
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
				float rVolDiv = rVolSel == 0 ? 0f : 256.0f / rVolSel;

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
				float rVolDiv = rVolSel == 0 ? 0f : 256.0f / rVolSel;

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
		#endregion

		#region Interpolation
		/********************************************************************/
		/// <summary>
		/// Resample a mono sample into a mono output buffer with
		/// interpolation
		/// </summary>
		/********************************************************************/
		private int ResampleMonoToMonoInterpolation(int[] source, int[] dest, int offset, int index, int incr, int todo, int volSel, int oldVol, ref int rampVol)
		{
			int len = source.Length;

			float volDiv = volSel == 0 ? 0f : 256.0f / volSel;

			if (rampVol != 0)
			{
				oldVol -= volSel;

				while (todo-- != 0)
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

					dest[offset++] = (int)(((volSel << ClickShift) + oldVol * rampVol) * sample >> ClickShift);

					if (--rampVol == 0)
						break;
				}

				if (todo < 0)
					return index;
			}

			while (todo-- != 0)
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

				dest[offset++] = (int)sample;
			}

			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Resample a mono sample into a stereo output buffer with
		/// interpolation
		/// </summary>
		/********************************************************************/
		private int ResampleMonoToStereoInterpolation(int[] source, int[] dest, int offset, int index, int incr, int todo, int volSel, int oldVol, ref int rampVol)
		{
			int len = source.Length;

			float volDiv = volSel == 0 ? 0f : 256.0f / volSel;

			if (rampVol != 0)
			{
				oldVol -= volSel;

				while (todo-- != 0)
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

					dest[offset++] = (int)sample;
					dest[offset++] = (int)sample;

					if (--rampVol == 0)
						break;
				}

				if (todo < 0)
					return index;
			}

			while (todo-- != 0)
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

				dest[offset++] = (int)sample;
				dest[offset++] = (int)sample;
			}

			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Resample a stereo sample into a mono output buffer with
		/// interpolation
		/// </summary>
		/********************************************************************/
		private int ResampleStereoToMonoInterpolation(int[] source, int[] dest, int offset, int index, int incr, int todo, int lVolSel, int rVolSel, int oldLVol, int oldRVol, ref int rampVol)
		{
			int len = source.Length;

			float lVolDiv = lVolSel == 0 ? 0f : 256.0f / lVolSel;
			float rVolDiv = rVolSel == 0 ? 0f : 256.0f / rVolSel;

			if (rampVol != 0)
			{
				oldLVol -= lVolSel;
				oldRVol -= rVolSel;

				while (todo-- != 0)
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

					dest[offset++] = (int)(((((lVolSel << ClickShift) + oldLVol * rampVol) * sample1 >> ClickShift) + (((rVolSel << ClickShift) + oldRVol * rampVol) * sample2 >> ClickShift)) / 2);

					if (--rampVol == 0)
						break;
				}

				if (todo < 0)
					return index;
			}

			while (todo-- != 0)
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

				dest[offset++] = (int)((sample1 + sample2) / 2);
			}

			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Resample a stereo sample into a stereo output buffer with
		/// interpolation
		/// </summary>
		/********************************************************************/
		private int ResampleStereoToStereoInterpolation(int[] source, int[] dest, int offset, int index, int incr, int todo, int lVolSel, int rVolSel, int oldLVol, int oldRVol, ref int rampVol)
		{
			int len = source.Length;

			float lVolDiv = lVolSel == 0 ? 0f : 256.0f / lVolSel;
			float rVolDiv = rVolSel == 0 ? 0f : 256.0f / rVolSel;

			if (rampVol != 0)
			{
				oldLVol -= lVolSel;
				oldRVol -= rVolSel;

				while (todo-- != 0)
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

					dest[offset++] = (int)(((lVolSel << ClickShift) + oldLVol * rampVol) * sample1 >> ClickShift);
					dest[offset++] = (int)(((rVolSel << ClickShift) + oldRVol * rampVol) * sample2 >> ClickShift);

					if (--rampVol == 0)
						break;
				}

				if (todo < 0)
					return index;
			}

			while (todo-- != 0)
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

				dest[offset++] = (int)sample1;
				dest[offset++] = (int)sample2;
			}

			return index;
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Converts the resampled data to a 32 bit sample buffer
		/// </summary>
		/********************************************************************/
		private void ResampleConvertTo32(Span<int> dest, int offset, int[] source, int count, int samplesToSkip, bool isStereo, bool swap)
		{
			int x1, x2, x3, x4;
			int remain;

			int sourceOffset = 0;

			if (swap)
			{
				if (samplesToSkip == 0)
				{
					remain = count & 3;

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
				}
				else
				{
					remain = count & 1;

					for (count >>= 1; count != 0; count--)
					{
						x1 = source[sourceOffset++];
						x2 = source[sourceOffset++];

						dest[offset++] = x2;
						dest[offset++] = x1;

						for (int i = 0; i < samplesToSkip; i++)
							dest[offset++] = 0;
					}
				}
			}
			else
			{
				if (isStereo)
				{
					if (samplesToSkip == 0)
					{
						remain = count & 3;

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
					}
					else
					{
						remain = count & 1;

						for (count >>= 1; count != 0; count--)
						{
							x1 = source[sourceOffset++];
							x2 = source[sourceOffset++];

							dest[offset++] = x1;
							dest[offset++] = x2;

							for (int i = 0; i < samplesToSkip; i++)
								dest[offset++] = 0;
						}
					}
				}
				else
				{
					remain = 0;

					for (; count != 0; count--)
						dest[offset++] = source[sourceOffset++];
				}
			}

			while (remain-- != 0)
				dest[offset++] = source[sourceOffset++];
		}



		/********************************************************************/
		/// <summary>
		/// Send an event when the position change
		/// </summary>
		/********************************************************************/
		private void OnPositionChanged()
		{
			if (PositionChanged != null)
				PositionChanged(this, EventArgs.Empty);
		}



		/********************************************************************/
		/// <summary>
		/// Send an event when the module information change
		/// </summary>
		/********************************************************************/
		private void OnModuleInfoChanged(ModuleInfoChangedEventArgs e)
		{
			if (ModuleInfoChanged != null)
				ModuleInfoChanged(this, e);
		}
		#endregion
	}
}
