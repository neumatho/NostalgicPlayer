/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.PlayerLibrary.Containers;
using Polycode.NostalgicPlayer.PlayerLibrary.Mixer.Containers;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Mixer
{
	/// <summary>
	/// This is the main mixer implementation
	/// </summary>
	internal class Mixer
	{
		private IModulePlayerAgent currentPlayer;

		private MixerBase currentMixer;

		private MixerMode mixerMode;		// Which modes the mixer has to work in
		private MixerMode currentMode;		// Is the current mode the mixer uses
		private int mixerFrequency;			// The mixer frequency
		private int moduleChannelNumber;	// The number of channels the module use

		private int[] mixBuffer;			// The buffer to hold the mixed data
		private int bufferSize;				// The maximum number of samples a buffer can be
		private int ticksLeft;				// Number of ticks left to call the player

		private int filterPrevLeft;			// The previous value for the left channel
		private int filterPrevRight;		// The previous value for the right channel

		private bool emulateFilter;

		private bool[] channelsEnabled;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Mixer()
		{
			// Initialize member variables
			mixerMode = MixerMode.None;
			emulateFilter = false;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the mixing routines
		/// </summary>
		/********************************************************************/
		public bool InitMixer(PlayerConfiguration playerConfiguration, out string errorMessage)
		{
			errorMessage = string.Empty;
			bool retVal = true;

			// Get the player instance
			currentPlayer = (IModulePlayerAgent)playerConfiguration.Loader.AgentPlayer;

			// Get player information
			moduleChannelNumber = currentPlayer.VirtualChannelCount;

			// Initialize other member variables
			filterPrevLeft = 0;
			filterPrevRight = 0;

			// Set flag for different mixer modes
			mixerMode = MixerMode.None;

			try
			{
				// Allocate mixer to use
				currentMixer = new MixerNormal();

				// Initialize the mixer
				currentMixer.Initialize(moduleChannelNumber);

				// Allocate channel objects
				ChannelParser[] channels = new ChannelParser[moduleChannelNumber];
				for (int i = 0; i < moduleChannelNumber; i++)
					channels[i] = new ChannelParser();

				currentPlayer.VirtualChannels = channels;

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
			// Deallocate mixer
			currentMixer?.Cleanup();
			currentMixer = null;

			// Deallocate mixer buffer
			mixBuffer = null;

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
		}



		/********************************************************************/
		/// <summary>
		/// Stops the mixing routines
		/// </summary>
		/********************************************************************/
		public void StopMixer()
		{
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
			mixBuffer = new int[bufferSize + 32];

			if (outputInformation.Channels == 2)
				mixerMode |= MixerMode.Stereo;
			else
				mixerMode &= ~MixerMode.Stereo;

			currentMixer.SetOutputFormat(outputInformation);
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

			if (mixerConfiguration.EnableInterpolation)
				mixerMode |= MixerMode.Interpolation;
			else
				mixerMode &= ~MixerMode.Interpolation;

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
			int retVal = DoMixing1(count, out hasEndReached);
			DoMixing2(buffer, offset, retVal);

			return retVal;
		}

		#region Private methods
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

			// Find the size of the buffer
			int bufSize = Math.Min(bufferSize, todo);

			// And convert the number of samples to number of samples pair
			todo = (currentMode & MixerMode.Stereo) != 0 ? bufSize >> 1 : bufSize;

			// Prepare the mixing buffer
			Array.Clear(mixBuffer, 0, mixBuffer.Length);

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

						for (int t = 0; t < moduleChannelNumber; t++)
							((ChannelParser)currentPlayer.VirtualChannels[t]).ParseInfo(ref voiceInfo[t], click);

						// Calculate the number of sample pair to mix before the
						// player need to be called again
						ticksLeft = (int)(mixerFrequency / currentPlayer.PlayingFrequency);

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

				// And mix it
				currentMixer.Mixing(mixBuffer, total, left, currentMode);

				// Calculate new values for the counter variables
				ticksLeft -= left;
				todo -= left;
				total += (currentMode & MixerMode.Stereo) != 0 ? left << 1 : left;

				// Check all the channels to see if they are still active and
				// enable/disable the channels depending on the user settings
				for (int t = 0; t < moduleChannelNumber; t++)
				{
					((ChannelParser)currentPlayer.VirtualChannels[t]).Active(currentMixer.IsActive(t));
					currentMixer.EnableChannel(t, (channelsEnabled == null) || channelsEnabled[t]);
				}
			}

			return total;
		}



		/********************************************************************/
		/// <summary>
		/// This is the secondary mixer method. It will call the right mixer
		/// and mix the extra samples and add effect to the mixed data and
		/// store it into the buffer given
		/// </summary>
		/********************************************************************/
		private void DoMixing2(byte[] buf, int offset, int todo)
		{
			// Find the size of the buffer
			int bufSize = Math.Min(bufferSize, todo);

			// Add Amiga low-pass filter if enabled
			AddAmigaFilter(mixBuffer, bufSize);

			// Now convert the mixed data to our output format
			currentMixer.ConvertMixedData(buf, offset, mixBuffer, bufSize);
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
				currentMixer.AddAmigaFilter((currentMode & MixerMode.Stereo) != 0, dest, todo, ref filterPrevLeft, ref filterPrevRight);
		}
		#endregion
	}
}
