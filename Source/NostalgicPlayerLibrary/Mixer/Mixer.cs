/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;
using Polycode.NostalgicPlayer.PlayerLibrary.Containers;
using Polycode.NostalgicPlayer.PlayerLibrary.Interfaces;
using Polycode.NostalgicPlayer.PlayerLibrary.Mixer.Containers;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Mixer
{
	/// <summary>
	/// This is the main mixer implementation
	/// </summary>
	internal class Mixer
	{
		private const int MixerBufferPadSize = 32;

		private IModulePlayerAgent currentPlayer;
		private bool bufferMode;
		private bool bufferDirect;

		private bool playing;

		private MixerBase currentMixer;
		private MixerVisualize currentVisualizer;

		private MixerMode mixerMode;		// Which modes the mixer has to work in
		private MixerMode currentMode;		// Is the current mode the mixer uses
		private int mixerFrequency;			// The mixer frequency
		private int moduleChannelNumber;	// The number of channels the module use

		private int[] mixBuffer;			// The buffer to hold the mixed data
		private int bufferSize;				// The maximum number of samples a buffer can be
		private int ticksLeft;				// Number of ticks left to call the player

		private int filterPrevLeft;			// The previous value for the left channel
		private int filterPrevRight;		// The previous value for the right channel

		private int samplesTakenSinceLastCall;	// Holds number of samples taken since the last call to Play(). Used by channel visualizers

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
		public bool InitMixer(Manager agentManager, PlayerConfiguration playerConfiguration, out string errorMessage)
		{
			errorMessage = string.Empty;
			bool retVal = true;

			// Clear the playing flag
			playing = false;

			// Get the player instance
			currentPlayer = (IModulePlayerAgent)playerConfiguration.Loader.PlayerAgent;
			bufferMode = (currentPlayer.SupportFlags & ModulePlayerSupportFlag.BufferMode) != 0;
			bufferDirect = (currentPlayer.SupportFlags & ModulePlayerSupportFlag.BufferDirect) != 0;

			// Get player information
			moduleChannelNumber = bufferDirect ? 2 : currentPlayer.VirtualChannelCount;

			// Initialize other member variables
			filterPrevLeft = 0;
			filterPrevRight = 0;

			// Set flag for different mixer modes
			mixerMode = MixerMode.None;

			// Allocate the visual component
			currentVisualizer = new MixerVisualize();

			try
			{
				// Allocate mixer to use
				if (bufferDirect)
					currentMixer = new MixerBufferDirect();
				else
					currentMixer = new MixerNormal();

				// Initialize the mixer
				currentMixer.Initialize(moduleChannelNumber);

				// Allocate channel objects
				IChannel[] channels = new IChannel[moduleChannelNumber];
				for (int i = 0; i < moduleChannelNumber; i++)
					channels[i] = new ChannelParser();

				currentPlayer.VirtualChannels = channels;

				// Initialize the visualizer
				currentVisualizer.Initialize(agentManager, moduleChannelNumber);

				// Initialize extra channels
				if (!bufferDirect)
					InitializeExtraChannels(playerConfiguration.MixerConfiguration.ExtraChannels);

				// Initialize the mixers
				ChangeConfiguration(playerConfiguration.MixerConfiguration);
			}
			catch (Exception ex)
			{
				CleanupMixer();

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
		public void CleanupMixer()
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
		}



		/********************************************************************/
		/// <summary>
		/// Starts the mixing routines
		/// </summary>
		/********************************************************************/
		public void StartMixer()
		{
			// Clear all the voices
			currentMixer.ClearVoices();

			// Initialize ticks left to call the player
			ticksLeft = 0;
			samplesTakenSinceLastCall = 0;

			// Ok to play
			playing = true;
		}



		/********************************************************************/
		/// <summary>
		/// Stops the mixing routines
		/// </summary>
		/********************************************************************/
		public void StopMixer()
		{
			playing = false;
		}



		/********************************************************************/
		/// <summary>
		/// Pause the mixing routines
		/// </summary>
		/********************************************************************/
		public void PauseMixer()
		{
			playing = false;
			currentVisualizer.TellAgentsAboutPauseState(true);
		}



		/********************************************************************/
		/// <summary>
		/// Resume the mixing routines
		/// </summary>
		/********************************************************************/
		public void ResumeMixer()
		{
			currentVisualizer.TellAgentsAboutPauseState(false);
			playing = true;
		}



		/********************************************************************/
		/// <summary>
		/// Set the output format
		/// </summary>
		/********************************************************************/
		public void SetOutputFormat(OutputInfo outputInformation)
		{
			mixerFrequency = outputInformation.Frequency;

			// Get the maximum number of samples the given destination
			// buffer from the output agent can be
			bufferSize = outputInformation.BufferSizeInSamples;

			// Allocate mixer buffer. This buffer is used by the mixer
			// routines to store the mixed data
			mixBuffer = new int[bufferSize + MixerBufferPadSize];

			if (outputInformation.Channels == 2)
				mixerMode |= MixerMode.Stereo;
			else
				mixerMode &= ~MixerMode.Stereo;

			currentMixer.SetOutputFormat(outputInformation);
			extraChannelsMixer?.SetOutputFormat(outputInformation);
			currentVisualizer.SetOutputFormat(outputInformation);

			lock (currentPlayer)
			{
				if ((currentPlayer.SupportFlags & ModulePlayerSupportFlag.BufferMode) != 0)
					currentPlayer.SetOutputFormat((uint)mixerFrequency, outputInformation.Channels);
			}

			// Create pool of buffers needed for extra effects
			groupBuffers = new Dictionary<int, int[]>();
			channelMap = new int[moduleChannelNumber][];

			// As default, map all channels to use the same output buffer
			for (int i = 0; i < moduleChannelNumber; i++)
				channelMap[i] = mixBuffer;
		}



		/********************************************************************/
		/// <summary>
		/// Will set the master volume
		/// </summary>
		/********************************************************************/
		public void SetMasterVolume(int volume)
		{
			currentMixer?.SetMasterVolume(volume);
		}



		/********************************************************************/
		/// <summary>
		/// Will change the mixer configuration
		/// </summary>
		/********************************************************************/
		public void ChangeConfiguration(MixerConfiguration mixerConfiguration)
		{
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
		}



		/********************************************************************/
		/// <summary>
		/// This is the main mixer method. It's the method that is called
		/// from the MixerStream to read the next bunch of data
		/// </summary>
		/********************************************************************/
		public int Mixing(byte[] buffer, int offset, int count, out bool hasEndReached)
		{
			// Find the size of the buffer
			int bufSize = Math.Min(bufferSize, count);

			int total = DoMixing1(bufSize, out hasEndReached);

			if (!bufferDirect)
			{
				int total2 = DoMixing2(bufSize);
				total = Math.Max(total, total2);
			}

			if (playing)
			{
				// Add extra effects if enabled
				AddEffects(mixBuffer, bufSize);

				// Add Amiga low-pass filter if enabled
				AddAmigaFilter(mixBuffer, bufSize);
			}

			bool isStereo = (currentMode & MixerMode.Stereo) != 0;

			// Now convert the mixed data to our output format
			currentMixer.ConvertMixedData(buffer, offset, mixBuffer, bufSize, isStereo ? swapSpeakers : false);

			// Tell visual agents about the mixed data
			currentVisualizer.TellAgentsAboutMixedData(buffer, offset, bufSize, isStereo, swapSpeakers);

			return total;
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
			}
		}



		/********************************************************************/
		/// <summary>
		/// This is the main mixer method. It will call the right mixer
		/// and mix the main module samples
		/// </summary>
		/********************************************************************/
		private int DoMixing1(int todo, out bool hasEndReached)
		{
			hasEndReached = false;

			int total = 0;

			// Remember the mixing mode. The reason to hold this in another
			// variable, it when the user change the mixing mode, it won't
			// be changed in the middle of a mixing, but used the next
			// time this method is called
			currentMode = mixerMode;

			// And convert the number of samples to number of samples pair
			todo = (currentMode & MixerMode.Stereo) != 0 ? todo >> 1 : todo;

			// Prepare the mixing buffers
			Array.Clear(mixBuffer, 0, mixBuffer.Length);

			foreach (int[] buffer in groupBuffers.Values)
				Array.Clear(buffer, 0, buffer.Length);

			if (playing)
			{
				while (todo > 0)
				{
					if (ticksLeft == 0)
					{
						// Call the player to play the next frame
						lock (currentPlayer)
						{
							currentPlayer.Play();

							// Get some mixer information we need to parse the data
							VoiceInfo[] voiceInfo = currentMixer.GetMixerChannels();
							int click = currentMixer.GetClickConstant();

							ChannelFlags[] flagArray = new ChannelFlags[moduleChannelNumber];
							ChannelFlags chanFlags = ChannelFlags.None;

							for (int t = 0; t < moduleChannelNumber; t++)
							{
								flagArray[t] = ((ChannelParser)currentPlayer.VirtualChannels[t]).ParseInfo(ref voiceInfo[t], click, bufferMode);
								chanFlags |= flagArray[t];
							}

							if (bufferDirect)
								ticksLeft = (int)voiceInfo[0].Size;
							else if (bufferMode)
								ticksLeft = (int)(mixerFrequency * voiceInfo[0].Size / voiceInfo[0].Frequency);
							else
							{
								// If at least one channel has changed its information,
								// tell visual agents about it
								if (chanFlags != ChannelFlags.None)
									currentVisualizer.QueueChannelChange(flagArray, currentPlayer.VirtualChannels, channelsEnabled, samplesTakenSinceLastCall);

								// Calculate the number of sample pair to mix before the
								// player need to be called again
								ticksLeft = (int)(mixerFrequency / currentPlayer.PlayingFrequency);
								samplesTakenSinceLastCall = 0;
							}

							BuildChannelMap();

							if (currentPlayer.HasEndReached)
							{
								currentPlayer.HasEndReached = false;
								hasEndReached = true;

								// Break out of the loop
								break;
							}
						}

						// If ticksLeft is still 0, the player doesn't play
						// anything at all, so jump out of the loop
						if (ticksLeft == 0)
							break;
					}

					// Find the number of samples pair to mix
					int left = Math.Min(ticksLeft, todo);

					// Call visualizers
					currentVisualizer.TellAgentsAboutChannelChange(left);

					// And mix it
					currentMixer.Mixing(channelMap, total, left, currentMode);

					// Calculate new values for the counter variables
					samplesTakenSinceLastCall += left;
					ticksLeft -= left;
					todo -= left;
					total += (currentMode & MixerMode.Stereo) != 0 ? left << 1 : left;

					// Check all the channels to see if they are still active and
					// enable/disable the channels depending on the user settings
					for (int t = 0, cnt = channelsEnabled == null ? moduleChannelNumber : Math.Min(moduleChannelNumber, channelsEnabled.Length); t < cnt; t++)
					{
						((ChannelParser)currentPlayer.VirtualChannels[t]).Active(currentMixer.IsActive(t));
						currentMixer.EnableChannel(t, (channelsEnabled == null) || channelsEnabled[t]);
					}
				}
			}

			return total;
		}



		/********************************************************************/
		/// <summary>
		/// This is the secondary mixer method. It will call the right mixer
		/// and mix the extra samples and add effect to the mixed data
		/// </summary>
		/********************************************************************/
		private int DoMixing2(int todo)
		{
			int total = 0;

			// Add extra channels if needed
			if ((extraChannelsInstance != null) && (extraChannelsNumber > 0))
			{
				if (extraChannelsInstance.PlayChannels(extraChannelsChannels))
				{
					// Get some mixer information we need to parse the data
					VoiceInfo[] voiceInfo = extraChannelsMixer.GetMixerChannels();
					int click = extraChannelsMixer.GetClickConstant();
					int[][] chanMap = new int[extraChannelsNumber][];

					// Parse the channels
					for (int t = 0; t < extraChannelsNumber; t++)
					{
						((ChannelParser)extraChannelsChannels[t]).ParseInfo(ref voiceInfo[t], click, bufferMode);
						chanMap[t] = mixBuffer;
					}

					// Mix the data
					extraChannelsMixer.Mixing(chanMap, 0, (currentMode & MixerMode.Stereo) != 0 ? todo >> 1 : todo, currentMode);

					// Check all the channels to see if they are still active
					for (int t = 0; t < extraChannelsNumber; t++)
						((ChannelParser)extraChannelsChannels[t]).Active(extraChannelsMixer.IsActive(t));

					total = todo;
				}
			}

			return total;
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

				for (int i = 0; i < moduleChannelNumber; i++)
				{
					if ((groupMap != null) && groupMap.TryGetValue(i, out int group))
					{
						if (!groupBuffers.TryGetValue(group, out int[] buffer))
						{
							// No buffer found, allocate a new one
							buffer = new int[bufferSize + MixerBufferPadSize];
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
		private void AddEffects(int[] dest, int todo)
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
						effectMaster.AddChannelGroupEffects(pair.Key, pair.Value, todo, (uint)mixerFrequency, isStereo);

						for (int i = 0; i < todo; i++)
							dest[i] += pair.Value[i];
					}

					effectMaster.AddGlobalEffects(dest, todo, (uint)mixerFrequency, isStereo);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Adds the Amiga LED filter if enabled
		/// </summary>
		/********************************************************************/
		private void AddAmigaFilter(int[] dest, int todo)
		{
			// Should we emulate the filter at all
			if (emulateFilter && currentPlayer.AmigaFilter)
			{
				int offset = 0;

				if ((currentMode & MixerMode.Stereo) != 0)
				{
					// Stereo buffer
					todo /= 2;

					while (todo-- != 0)
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
					while (todo-- != 0)
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
