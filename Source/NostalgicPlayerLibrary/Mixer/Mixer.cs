/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.NostalgicPlayerKit.Containers;
using Polycode.NostalgicPlayer.NostalgicPlayerKit.Interfaces;
using Polycode.NostalgicPlayer.NostalgicPlayerLibrary.Containers;
using Polycode.NostalgicPlayer.NostalgicPlayerLibrary.Mixer.Containers;

namespace Polycode.NostalgicPlayer.NostalgicPlayerLibrary.Mixer
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

//XX		private int filterPrevLeft;			// The previous value for the left channel
//XX		private int filterPrevRight;		// The previous value for the right channel

		private bool emulateFilter;

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
			//XXfilterPrevLeft = 0;
			//XXfilterPrevRight = 0;

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

				currentPlayer.Channels = channels;

				// Initialize the mixers
				ChangeConfiguration(playerConfiguration.MixerConfiguration);
			}
			catch (Exception ex)
			{
				CleanupMixer();

				errorMessage = string.Format(Properties.Resources.IDS_ERR_MIXER_INIT, ex.HResult, ex.Message);
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
				currentPlayer.Channels = null;

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
			emulateFilter = mixerConfiguration.EnableAmigaFilter;
			currentMixer?.SetStereoSeparation(mixerConfiguration.StereoSeparator);
		}



		/********************************************************************/
		/// <summary>
		/// This is the main mixer method. It's the method that is called
		/// from the MixerStream to read the next bunch of data
		/// </summary>
		/********************************************************************/
		public int Mixing(byte[] buffer, int offset, int count)
		{
			int retVal = DoMixing1(count);
			DoMixing2(buffer, offset, count);

			return retVal;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// This is the main mixer method. It will call the right mixer
		/// and mix the main module samples
		/// </summary>
		/********************************************************************/
		private int DoMixing1(int todo)
		{
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
							((ChannelParser)currentPlayer.Channels[t]).ParseInfo(ref voiceInfo[t], click);

						// Calculate the number of sample pair to mix before the
						// player need to be called again
						ticksLeft = (int)(mixerFrequency / currentPlayer.PlayingFrequency);
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

			// Now convert the mixed data to our output format
			currentMixer.ConvertMixedData(buf, offset, mixBuffer, bufSize);
		}
		#endregion
	}
}
