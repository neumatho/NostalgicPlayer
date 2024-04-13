/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;
using Polycode.NostalgicPlayer.PlayerLibrary.Containers;
using Polycode.NostalgicPlayer.PlayerLibrary.Interfaces;
using Polycode.NostalgicPlayer.PlayerLibrary.Sound.Mixer.Containers;
using Polycode.NostalgicPlayer.PlayerLibrary.Sound.Timer.Events;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Sound.Mixer
{
	/// <summary>
	/// This is the main mixer implementation
	///
	/// Here is a short description of how I see samples/frames etc.
	///
	/// You have a buffer which is 2000 bytes in size, which contains
	/// 16-bit samples in stereo
	///
	/// buffer have 2000 number of bytes
	/// buffer have 1000 number of samples
	/// buffer have 500 number of frames
	/// 
	/// </summary>
	internal class Mixer : SoundBase
	{
		private IModulePlayerAgent currentPlayer;
		private bool bufferMode;
		private bool bufferDirect;
		private bool enableChannelVisualization;
		private bool enableChannelsSupport;

		private bool playing;

		private MixerBase currentMixer;
		private Visualizer currentVisualizer;

		private MixerMode mixerMode;			// Which modes the mixer has to work in
		private MixerMode currentMode;			// Is the current mode the mixer uses
		private int mixerFrequency;				// The mixer frequency
		private int virtualChannelNumber;		// The number of channels the module use
		private int outputChannelNumber;		// The number of channels the output want
		private int mixerChannels;				// Number of channels mixed. Can either be 1 or 2

		private int[] mixBuffer;				// The buffer to hold the mixed data
		private int bufferSizeInFrames;			// The maximum number of frames a buffer can be
		private int framesLeft;					// Number of frames left to call the player

		private int filterPrevLeft;				// The previous value for the left channel
		private int filterPrevRight;			// The previous value for the right channel

		private int framesTakenSinceLastCall;	// Holds number of frames taken since the last call to Play(). Used by channel visualizers

		private int[] extraChannelsMixBuffer;
		private byte[] extraTempBuffer;

		private bool swapSpeakers;
		private bool emulateFilter;

		private bool[] channelsEnabled;

		private Dictionary<int, int[]> groupBuffers;
		private int[][] channelMap;

		// Extra channels variables
		private IExtraChannels extraChannelsInstance;
		private int extraChannelsNumber;
		private MixerBase extraChannelsMixer;
		private IChannel[] extraChannelsChannels;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Mixer()
		{
			// Initialize member variables
			playing = false;

			mixerMode = MixerMode.None;

			swapSpeakers = false;
			emulateFilter = false;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the mixing routines
		/// </summary>
		/********************************************************************/
		public override bool Initialize(Manager agentManager, PlayerConfiguration playerConfiguration, out string errorMessage)
		{
			bool retVal = true;

			try
			{
				if (!base.Initialize(agentManager, playerConfiguration, out errorMessage))
					return false;

				// Clear the playing flag
				playing = false;

				// Get the player instance
				currentPlayer = (IModulePlayerAgent)playerConfiguration.Loader.PlayerAgent;
				bufferMode = (currentPlayer.SupportFlags & ModulePlayerSupportFlag.BufferMode) != 0;
				bufferDirect = (currentPlayer.SupportFlags & ModulePlayerSupportFlag.BufferDirect) != 0;

				// Get player information
				virtualChannelNumber = bufferDirect ? 2 : currentPlayer.VirtualChannelCount;
				enableChannelVisualization = bufferMode && ((currentPlayer.SupportFlags & ModulePlayerSupportFlag.Visualize) != 0);
				enableChannelsSupport = !bufferDirect || ((currentPlayer.SupportFlags & ModulePlayerSupportFlag.EnableChannels) == 0);

				// Initialize other member variables
				filterPrevLeft = 0;
				filterPrevRight = 0;

				// Set flag for different mixer modes
				mixerMode = MixerMode.None;

				// Allocate the visual component
				currentVisualizer = new Visualizer();

				// Allocate mixer to use
				if (bufferDirect)
					currentMixer = new MixerBufferDirect();
				else
					currentMixer = new MixerNormal();

				// Initialize the mixer
				currentMixer.Initialize(virtualChannelNumber);

				// Allocate channel objects
				IChannel[] channels = new IChannel[virtualChannelNumber];
				for (int i = 0; i < virtualChannelNumber; i++)
					channels[i] = new ChannelParser();

				currentPlayer.VirtualChannels = channels;

				// Initialize the visualizer
				currentVisualizer.Initialize(agentManager);

				// Initialize extra channels
				InitializeExtraChannels(playerConfiguration.MixerConfiguration.ExtraChannels);

				// Initialize the mixers
				ChangeConfiguration(playerConfiguration.MixerConfiguration);
			}
			catch (Exception ex)
			{
				Cleanup();

				errorMessage = string.Format(Resources.IDS_ERR_MIXER_INIT, ex.HResult.ToString("X8"), ex.Message);
				retVal = false;
			}

			return retVal;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup mixer
		/// </summary>
		/********************************************************************/
		public override void Cleanup()
		{
			// Cleanup extra channels
			CleanupExtraChannels();

			// Deallocate mixer
			currentMixer?.Cleanup();
			currentMixer = null;

			// Deallocate the visualizer
			currentVisualizer?.Cleanup();
			currentVisualizer = null;

			// Deallocate mixer buffer
			mixBuffer = null;
			groupBuffers = null;
			channelMap = null;

			// Deallocate channel objects
			if (currentPlayer != null)
				currentPlayer.VirtualChannels = null;

			// Cleanup member variables
			currentPlayer = null;

			base.Cleanup();
		}



		/********************************************************************/
		/// <summary>
		/// Starts the mixing routines
		/// </summary>
		/********************************************************************/
		public override void Start()
		{
			base.Start();

			// Clear all the voices
			currentMixer.ClearVoices();

			// Initialize ticks left to call the player
			framesLeft = 0;
			framesTakenSinceLastCall = 0;

			// Ok to play
			playing = true;
		}



		/********************************************************************/
		/// <summary>
		/// Stops the mixing routines
		/// </summary>
		/********************************************************************/
		public override void Stop()
		{
			base.Stop();

			playing = false;
		}



		/********************************************************************/
		/// <summary>
		/// Pause the mixing routines
		/// </summary>
		/********************************************************************/
		public override void Pause()
		{
			base.Pause();

			playing = false;
			currentVisualizer.TellAgentsAboutPauseState(true);
		}



		/********************************************************************/
		/// <summary>
		/// Resume the mixing routines
		/// </summary>
		/********************************************************************/
		public override void Resume()
		{
			base.Resume();

			currentVisualizer.TellAgentsAboutPauseState(false);
			playing = true;
		}



		/********************************************************************/
		/// <summary>
		/// Set the output format
		/// </summary>
		/********************************************************************/
		public override void SetOutputFormat(OutputInfo outputInformation)
		{
			base.SetOutputFormat(outputInformation);

			mixerFrequency = outputInformation.Frequency;
			outputChannelNumber = outputInformation.Channels;

			mixerChannels = 1;

			if (outputChannelNumber >= 2)
			{
				mixerMode |= MixerMode.Stereo;
				mixerChannels = 2;
			}
			else
				mixerMode &= ~MixerMode.Stereo;

			// Get the maximum number of frames the given destination
			// buffer from the output agent can be
			bufferSizeInFrames = outputInformation.BufferSizeInFrames;

			// Allocate mixer buffer. This buffer is used by the mixer
			// routines to store the mixed data
			mixBuffer = new int[bufferSizeInFrames * mixerChannels];

			currentMixer.SetOutputFormat(outputInformation);
			currentVisualizer.SetOutputFormat(outputInformation);

			if (extraChannelsMixer != null)
			{
				extraChannelsMixBuffer = new int[mixBuffer.Length];

				extraChannelsMixer.SetOutputFormat(outputInformation);
			}

			lock (currentPlayer)
			{
				if ((currentPlayer.SupportFlags & ModulePlayerSupportFlag.BufferMode) != 0)
					currentPlayer.SetOutputFormat((uint)mixerFrequency, outputInformation.Channels);
			}

			// Create pool of buffers needed for extra effects
			groupBuffers = new Dictionary<int, int[]>();
			channelMap = new int[virtualChannelNumber][];

			// As default, map all channels to use the same output buffer
			for (int i = 0; i < virtualChannelNumber; i++)
				channelMap[i] = mixBuffer;
		}



		/********************************************************************/
		/// <summary>
		/// Will change the configuration
		/// </summary>
		/********************************************************************/
		public override void ChangeConfiguration(MixerConfiguration mixerConfiguration)
		{
			base.ChangeConfiguration(mixerConfiguration);

			currentMixer?.SetStereoSeparation(mixerConfiguration.StereoSeparator);
			currentVisualizer?.SetVisualsLatency(mixerConfiguration.VisualsLatency);

			if (mixerConfiguration.EnableInterpolation)
				mixerMode |= MixerMode.Interpolation;
			else
				mixerMode &= ~MixerMode.Interpolation;

			if (mixerConfiguration.EnableSurround)
				mixerMode |= MixerMode.Surround;
			else
				mixerMode &= ~MixerMode.Surround;

			swapSpeakers = mixerConfiguration.SwapSpeakers;
			emulateFilter = mixerConfiguration.EnableAmigaFilter;

			channelsEnabled = mixerConfiguration.ChannelsEnabled;

			lock (currentPlayer)
			{
				currentPlayer.ChangeMixerConfiguration(new MixerInfo
				{
					StereoSeparator = mixerConfiguration.StereoSeparator,
					EnableInterpolation = mixerConfiguration.EnableInterpolation,
					EnableSurround = mixerConfiguration.EnableSurround,
					ChannelsEnabled = mixerConfiguration.ChannelsEnabled
				});
			}
		}



		/********************************************************************/
		/// <summary>
		/// This is the main mixer method. It's the method that is called
		/// from the MixerStream to read the next bunch of data
		/// </summary>
		/********************************************************************/
		public int Mixing(byte[] buffer, int offsetInBytes, int frameCount, out bool hasEndReached)
		{
			// Remember the mixing mode. The reason to hold this in another
			// variable, it when the user change the mixing mode, it won't
			// be changed in the middle of a mixing, but used the next
			// time this method is called
			currentMode = mixerMode;

			bool isStereo = (currentMode & MixerMode.Stereo) != 0;
			bool swap = isStereo ? swapSpeakers : false;

			// Find out how many frames to process
			int framesToProcess = Math.Min(frameCount, bufferSizeInFrames);

			int totalFramesProcessed = DoMixing(framesToProcess, out hasEndReached);
			if (totalFramesProcessed == 0)
				Array.Clear(buffer);

			int totalSamplesProcessed = totalFramesProcessed * (isStereo ? 2 : 1);

			if (playing)
			{
				// Add extra effects if enabled
				AddEffects(mixBuffer, totalSamplesProcessed);

				// Add Amiga low-pass filter if enabled
				AddAmigaFilter(mixBuffer, totalSamplesProcessed);
			}

			// Now convert the mixed data to our output format
			int samplesToSkip = isStereo ? outputChannelNumber - 2 : 0;
			currentMixer.ConvertMixedData(buffer, offsetInBytes, mixBuffer, totalSamplesProcessed, samplesToSkip, isStereo, swap);

			// Mix extra channels into the output buffer
			int totalFrames = MixExtraChannels(framesToProcess);
			if (totalFrames > 0)
				AddExtraChannelsIntoOutput(buffer, offsetInBytes, totalFrames, samplesToSkip, isStereo, swap);

			// Calculate total frames really written
			totalFrames = Math.Max(totalFramesProcessed, totalFrames);

			// Tell visual agents about the mixed data
			currentVisualizer.TellAgentsAboutMixedData(buffer, offsetInBytes, Math.Max(totalFrames, framesToProcess) * outputChannelNumber, outputChannelNumber, swap);

			return totalFrames;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Initialize extra channels
		/// </summary>
		/********************************************************************/
		private void InitializeExtraChannels(IExtraChannels extraChannels)
		{
			if (extraChannels != null)
			{
				extraChannelsInstance = extraChannels;
				extraChannels.Initialize();

				extraChannelsNumber = extraChannels.ExtraChannels;

				if (extraChannelsNumber != 0)
				{
					// Initialize extra mixer
					extraChannelsMixer = new MixerNormal();
					extraChannelsMixer.Initialize(extraChannelsNumber);

					// Allocate channel objects
					extraChannelsChannels = new IChannel[extraChannelsNumber];
					for (int i = 0; i < extraChannelsNumber; i++)
						extraChannelsChannels[i] = new ChannelParser();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup extra channels
		/// </summary>
		/********************************************************************/
		private void CleanupExtraChannels()
		{
			if (extraChannelsInstance != null)
			{
				extraChannelsInstance.Cleanup();
				extraChannelsInstance = null;

				// Cleanup extra mixers
				extraChannelsMixer?.Cleanup();
				extraChannelsMixer = null;

				extraChannelsChannels = null;
				extraChannelsMixBuffer = null;
			}
		}



		/********************************************************************/
		/// <summary>
		/// This is the main mixer method. It will call the right mixer
		/// and mix the main module samples
		/// </summary>
		/********************************************************************/
		private int DoMixing(int todoInFrames, out bool hasEndReached)
		{
			hasEndReached = false;

			int totalFrames = 0;

			// Prepare the mixing buffers
			Array.Clear(mixBuffer, 0, mixBuffer.Length);

			foreach (int[] buffer in groupBuffers.Values)
				Array.Clear(buffer, 0, buffer.Length);

			if (playing)
			{
				// This method is called right after the previous buffer has been player,
				// so at this point, add the number of samples played since last call
				DoTimedEvents();
				IncreaseCurrentTime(todoInFrames);

				while (todoInFrames > 0)
				{
					if (framesLeft == 0)
					{
						// Call the player to play the next frame
						lock (currentPlayer)
						{
							currentPlayer.Play();

							// Get some mixer information we need to parse the data
							VoiceInfo[] voiceInfo = currentMixer.GetMixerChannels();
							int click = currentMixer.GetClickConstant();

							ChannelChanged[] channelChanges = new ChannelChanged[virtualChannelNumber];

							for (int t = 0; t < virtualChannelNumber; t++)
							{
								bool channelEnabled = (channelsEnabled != null) && (t < channelsEnabled.Length) ? channelsEnabled[t] : true;
								channelChanges[t] = ((ChannelParser)currentPlayer.VirtualChannels[t]).ParseInfo(ref voiceInfo[t], click, channelEnabled, bufferMode);
							}

							if (enableChannelVisualization)
								channelChanges = currentPlayer.VisualChannels;

							// If at least one channel has changed its information,
							// tell visual agents about it
							if ((channelChanges != null) && channelChanges.Any(x => x != null))
								currentVisualizer.QueueChannelChange(channelChanges, framesTakenSinceLastCall * mixerChannels);

							// If any module information has been updated, queue those
							ModuleInfoChanged[] moduleInfoChanges = currentPlayer.GetChangedInformation();
							if (moduleInfoChanges != null)
							{
								foreach (ModuleInfoChanged moduleInfoChanged in moduleInfoChanges)
									timedEventHandler.AddEvent(new ModuleInfoChangedEvent(this, moduleInfoChanged), framesTakenSinceLastCall);
							}

							framesTakenSinceLastCall = 0;

							if (bufferDirect)
								framesLeft = (int)voiceInfo[0].Size;
							else if (bufferMode)
								framesLeft = (int)(mixerFrequency * voiceInfo[0].Size / voiceInfo[0].Frequency);
							else
							{
								// Calculate the number of frames to mix before the
								// player need to be called again
								framesLeft = (int)(mixerFrequency / currentPlayer.PlayingFrequency);
							}

							BuildChannelMap();

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

								// Break out of the loop
								break;
							}
						}

						// If framesLeft is still 0, the player doesn't play
						// anything at all, so jump out of the loop
						if (framesLeft == 0)
							break;
					}

					// Find the number of frames to mix
					int leftInFrames = Math.Min(framesLeft, todoInFrames);

					// Call visualizers
					currentVisualizer.TellAgentsAboutChannelChange(leftInFrames * mixerChannels);

					// And mix it
					currentMixer.Mixing(channelMap, totalFrames, leftInFrames, currentMode);

					// Calculate new values for the counter variables
					framesTakenSinceLastCall += leftInFrames;
					framesLeft -= leftInFrames;
					todoInFrames -= leftInFrames;
					totalFrames += leftInFrames;

					if (enableChannelsSupport)
					{
						// Check all the channels to see if they are still active and
						// enable/disable the channels depending on the user settings
						for (int t = 0, cnt = channelsEnabled == null ? virtualChannelNumber : Math.Min(virtualChannelNumber, channelsEnabled.Length); t < cnt; t++)
						{
							((ChannelParser)currentPlayer.VirtualChannels[t]).Active(currentMixer.IsActive(t));
							currentMixer.EnableChannel(t, (channelsEnabled == null) || channelsEnabled[t]);
						}
					}
				}
			}

			return totalFrames;
		}



		/********************************************************************/
		/// <summary>
		/// This will mix any extra samples playing
		/// </summary>
		/********************************************************************/
		private int MixExtraChannels(int todoInFrames)
		{
			int totalFrames = 0;

			// Add extra channels if needed
			if ((extraChannelsInstance != null) && (extraChannelsNumber > 0))
			{
				if (extraChannelsInstance.PlayChannels(extraChannelsChannels))
				{
					Array.Clear(extraChannelsMixBuffer);

					// Get some mixer information we need to parse the data
					VoiceInfo[] voiceInfo = extraChannelsMixer.GetMixerChannels();
					int click = extraChannelsMixer.GetClickConstant();
					int[][] chanMap = new int[extraChannelsNumber][];

					// Parse the channels
					for (int t = 0; t < extraChannelsNumber; t++)
					{
						((ChannelParser)extraChannelsChannels[t]).ParseInfo(ref voiceInfo[t], click, true, false);
						chanMap[t] = extraChannelsMixBuffer;
					}

					// Mix the data
					extraChannelsMixer.Mixing(chanMap, 0, todoInFrames, currentMode);

					// Check all the channels to see if they are still active
					for (int t = 0; t < extraChannelsNumber; t++)
						((ChannelParser)extraChannelsChannels[t]).Active(extraChannelsMixer.IsActive(t));

					totalFrames = todoInFrames;
				}
			}

			return totalFrames;
		}



		/********************************************************************/
		/// <summary>
		/// Will mix the channel mixer and extra channel mixer data together
		/// </summary>
		/********************************************************************/
		private void AddExtraChannelsIntoOutput(byte[] buffer, int offsetInBytes, int todoInFrames, int samplesToSkip, bool isStereo, bool swap)
		{
			if ((extraTempBuffer == null) || (extraTempBuffer.Length < buffer.Length))
				extraTempBuffer = new byte[buffer.Length];

			extraChannelsMixer.ConvertMixedData(extraTempBuffer, 0, extraChannelsMixBuffer, todoInFrames * mixerChannels, samplesToSkip, isStereo, swap);

			Span<int> source = MemoryMarshal.Cast<byte, int>(extraTempBuffer);
			Span<int> dest = MemoryMarshal.Cast<byte, int>(buffer);
			int offsetInSamples = offsetInBytes / 4;
			int todoInSamples = todoInFrames * outputChannelNumber;

			for (int i = 0; i < todoInSamples; i++)
			{
				long val = dest[offsetInSamples] + source[i];
				val = (val >= 2147483647) ? 2147483647 - 1 : (val < -2147483647) ? -2147483647 : val;
				dest[offsetInSamples++] = (int)val;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Build a map of output buffers for each channel
		/// </summary>
		/********************************************************************/
		private void BuildChannelMap()
		{
			IEffectMaster effectMaster = currentPlayer.EffectMaster;
			if (effectMaster != null)
			{
				IReadOnlyDictionary<int, int> groupMap = effectMaster.GetChannelGroups();

				for (int i = 0; i < virtualChannelNumber; i++)
				{
					if ((groupMap != null) && groupMap.TryGetValue(i, out int group))
					{
						if (!groupBuffers.TryGetValue(group, out int[] buffer))
						{
							// No buffer found, allocate a new one
							buffer = new int[bufferSizeInFrames * mixerChannels];
							groupBuffers[group] = buffer;
						}

						channelMap[i] = buffer;
					}
					else
					{
						// If channel is not mapped, use final output buffer
						channelMap[i] = mixBuffer;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Adds extra DSP effects from e.g. the current player
		/// </summary>
		/********************************************************************/
		private void AddEffects(int[] dest, int todoInSamples)
		{
			// Add effect on each group and mix them together
			lock (currentPlayer)
			{
				IEffectMaster effectMaster = currentPlayer.EffectMaster;
				if (effectMaster != null)
				{
					bool isStereo = (currentMode & MixerMode.Stereo) != 0;

					foreach (KeyValuePair<int, int[]> pair in groupBuffers)
					{
						effectMaster.AddChannelGroupEffects(pair.Key, pair.Value, todoInSamples, (uint)mixerFrequency, isStereo);

						for (int i = 0; i < todoInSamples; i++)
							dest[i] += pair.Value[i];
					}

					effectMaster.AddGlobalEffects(dest, todoInSamples, (uint)mixerFrequency, isStereo);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Adds the Amiga LED filter if enabled
		/// </summary>
		/********************************************************************/
		private void AddAmigaFilter(int[] dest, int todoInSamples)
		{
			// Should we emulate the filter at all
			if (emulateFilter && currentPlayer.AmigaFilter)
			{
				int offset = 0;

				if ((currentMode & MixerMode.Stereo) != 0)
				{
					// Stereo buffer
					todoInSamples /= 2;

					while (todoInSamples-- != 0)
					{
						filterPrevLeft = (dest[offset] + filterPrevLeft * 3) >> 2;
						dest[offset++] = filterPrevLeft;

						filterPrevRight = (dest[offset] + filterPrevRight * 3) >> 2;
						dest[offset++] = filterPrevRight;
					}
				}
				else
				{
					// Mono buffer
					while (todoInSamples-- != 0)
					{
						filterPrevLeft = (dest[offset] + filterPrevLeft * 3) >> 2;
						dest[offset++] = filterPrevLeft;
					}
				}
			}
		}
		#endregion
	}
}
