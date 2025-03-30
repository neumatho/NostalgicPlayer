/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Threading;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;
using Polycode.NostalgicPlayer.PlayerLibrary.Containers;
using Polycode.NostalgicPlayer.PlayerLibrary.Sound.Mixer.Containers;
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

		private const int MasterVolume = 256;

		private ISamplePlayerAgent currentPlayer;

		private DownMixer downMixer;
		private Visualizer currentVisualizer;

		private int inputFrequency;
		private int inputChannelCount;

		private int outputFrequency;
		private int outputChannelCount;

		private int[][] dataBuffer;
		private int framesRead;

		private int[][] resampleBuffer;
		private int bufferSizeInFrames;

		private int dataSize;
		private int currentIndex;
		private int increment;

		private readonly Lock mixerInfoLock = new Lock();
		private MixerInfo mixerInfo;			// The current mixer information

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
				inputChannelCount = currentPlayer.ChannelCount;

				// Allocate the visual component
				currentVisualizer = new Visualizer();

				// Initialize the visualizer
				currentVisualizer.Initialize(agentManager);

				// Initialize the resampler
				lock (mixerInfoLock)
				{
					mixerInfo = new MixerInfo();
					ChangeConfiguration(playerConfiguration.MixerConfiguration);
				}
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

			downMixer = null;

			lock (mixerInfoLock)
			{
				mixerInfo = null;
			}

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
			outputChannelCount = outputInformation.Channels;

			// Initialize variables
			framesRead = 0;

			dataSize = 0;
			currentIndex = 0;
			increment = (inputFrequency << FracBits) / outputFrequency;

			// Get the maximum number of frames the given destination
			// buffer from the output agent can be
			bufferSizeInFrames = outputInformation.BufferSizeInFrames;

			// Allocate the buffers
			int len = inputFrequency / 2;		// Around half second buffer size
			if ((len % 2) != 0)
				len++;		// Need to make it even

			dataBuffer = ArrayHelper.Initialize2Arrays<int>(inputChannelCount, len);
			resampleBuffer = ArrayHelper.Initialize2Arrays<int>(inputChannelCount, bufferSizeInFrames);

			currentVisualizer.SetOutputFormat(outputInformation);

			lock (currentPlayer)
			{
				downMixer = new DownMixer(currentPlayer.SpeakerFlags, outputInformation.AvailableSpeakers);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return which speakers that are used to play the sound
		/// </summary>
		/********************************************************************/
		public SpeakerFlag VisualizerSpeakers => downMixer?.VisualizerSpeakers ?? 0;



		/********************************************************************/
		/// <summary>
		/// Will change the mixer configuration
		/// </summary>
		/********************************************************************/
		public override void ChangeConfiguration(MixerConfiguration mixerConfiguration)
		{
			base.ChangeConfiguration(mixerConfiguration);

			currentVisualizer.SetVisualsLatency(mixerConfiguration.VisualsLatency);

			lock (mixerInfoLock)
			{
				mixerInfo.EnableInterpolation = mixerConfiguration.EnableInterpolation;
				mixerInfo.EnableSurround = mixerConfiguration.EnableSurround;
				mixerInfo.SwapSpeakers = mixerConfiguration.SwapSpeakers;
				mixerInfo.EmulateFilter = mixerConfiguration.EnableAmigaFilter;

				Array.Copy(mixerConfiguration.ChannelsEnabled, mixerInfo.ChannelsEnabled, mixerConfiguration.ChannelsEnabled.Length);
			}
		}



		/********************************************************************/
		/// <summary>
		/// This is the main resampler method. It's the method that is called
		/// from the ResamplerStream to read the next bunch of data
		/// </summary>
		/********************************************************************/
		public int Resampling(Span<int> outputBuffer, int frameCount, out bool hasEndReached)
		{
			MixerInfo currentMixerInfo;

			lock (mixerInfoLock)
			{
				// Remember the mixer information in a local variable.
				// The reason to hold this in another variable, it when the
				// user change the mixing mode, it won't be changed in the
				// middle of a mixing, but used the next time this method is called
				currentMixerInfo = new MixerInfo(mixerInfo);
			}

			// Find out how many frames to process
			int framesToProcess = Math.Min(frameCount, bufferSizeInFrames);

			int totalFramesProcessed = ResampleSample(currentMixerInfo, framesToProcess, out hasEndReached);

			// And then convert the resampled sample to the output format
			downMixer.ConvertToOutputFormat(currentMixerInfo, resampleBuffer, outputBuffer, totalFramesProcessed);

			// Tell visual agents about the mixed data
			currentVisualizer.TellAgentsAboutMixedData(outputBuffer, totalFramesProcessed, downMixer.GetVisualizersChannelMapping(currentMixerInfo), outputChannelCount);

			return totalFramesProcessed;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will resample the sample and make sure to read new data when
		/// needed
		/// </summary>
		/********************************************************************/
		private int ResampleSample(MixerInfo currentMixerInfo, int countInFrames, out bool hasEndReached)
		{
			hasEndReached = false;

			// This method is called right after the previous buffer has been player,
			// so at this point, add the number of samples played since last call
			DoTimedEvents();
			IncreaseCurrentTime(countInFrames);

			int totalFrames = 0;

			while ((countInFrames > 0) && !hasEndReached)
			{
				if (currentIndex >= dataSize)
				{
					lock (currentPlayer)
					{
						framesRead = currentPlayer.LoadDataBlock(dataBuffer, dataBuffer[0].Length);

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

						if (framesRead == 0)
							break;
					}

					currentIndex = 0;
					dataSize = framesRead == 0 ? 0 : (framesRead << FracBits) - 1;
				}

				int todoInFrames = Math.Min((dataSize - currentIndex) / increment + 1, Math.Min(framesRead, countInFrames));
				if (todoInFrames > 0)
				{
					int newCurrent = currentIndex;

					if (currentMixerInfo.EnableInterpolation)
					{
						for (int i = 0; i < resampleBuffer.Length; i++)
						{
							int volume = i < currentMixerInfo.ChannelsEnabled.Length ? currentMixerInfo.ChannelsEnabled[i] ? MasterVolume : 0 : MasterVolume;
							newCurrent = ResampleInterpolation(dataBuffer[i], resampleBuffer[i], totalFrames, currentIndex, increment, todoInFrames, volume);
						}
					}
					else
					{
						for (int i = 0; i < resampleBuffer.Length; i++)
						{
							int volume = i < currentMixerInfo.ChannelsEnabled.Length ? currentMixerInfo.ChannelsEnabled[i] ? MasterVolume : 0 : MasterVolume;
							newCurrent = ResampleNormal(dataBuffer[i], resampleBuffer[i], totalFrames, currentIndex, increment, todoInFrames, volume);
						}
					}

					currentIndex = newCurrent;

					countInFrames -= todoInFrames;
					totalFrames += todoInFrames;
				}
			}

			return totalFrames;
		}



		/********************************************************************/
		/// <summary>
		/// Resample a sample into the output buffer
		/// </summary>
		/********************************************************************/
		private int ResampleNormal(int[] source, int[] dest, int offsetInFrames, int index, int incr, int todoInFrames, int volume)
		{
			int len = source.Length;

			if (volume == 256)
			{
				while (todoInFrames-- != 0)
				{
					int idx = index >> FracBits;
					if (idx >= len)
						break;

					int sample = source[idx];
					index += incr;

					dest[offsetInFrames++] = sample;
				}
			}
			else
			{
				float volDiv = 256.0f / volume;

				while (todoInFrames-- != 0)
				{
					int idx = index >> FracBits;
					if (idx >= len)
						break;

					int sample = (int)(source[idx] / volDiv);
					index += incr;

					dest[offsetInFrames++] = sample;
				}
			}

			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Resample a sample into the output buffer with interpolation
		/// </summary>
		/********************************************************************/
		private int ResampleInterpolation(int[] source, int[] dest, int offsetInSamples, int index, int incr, int todoInFrames, int volume)
		{
			int len = source.Length;

			float volDiv = volume == 0 ? 0f : 256.0f / volume;

			while (todoInFrames-- != 0)
			{
				int idx = index >> FracBits;
				if (idx >= len)
					break;

				long a = source[idx];
				long b = idx + 1 >= len ? a : source[idx + 1];

				long sample = volume == 0 ? 0 : (long)((a + ((b - a) * (index & FracMask) >> FracBits)) / volDiv);
				if (sample < -0x7fffffff)
					sample = -0x7fffffff;
				else if (sample > 0x7fffffff)
					sample = 0x7fffffff;

				index += incr;

				dest[offsetInSamples++] = (int)sample;
			}

			return index;
		}
		#endregion
	}
}
