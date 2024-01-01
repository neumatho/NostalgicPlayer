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
using Polycode.NostalgicPlayer.Kit.Containers.Events;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
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
		private bool enableChannelVisualization;
		private bool enableChannelsSupport;

		private bool playing;

		private MixerBase currentMixer;
		private MixerVisualize currentVisualizer;

		private MixerMode mixerMode;			// Which modes the mixer has to work in
		private MixerMode currentMode;			// Is the current mode the mixer uses
		private int mixerFrequency;				// The mixer frequency
		private int virtualChannelNumber;		// The number of channels the module use
		private int outputChannelNumber;		// The number of channels the output want
		private int mixerChannels;				// Number of channels mixed. Can either be 1 or 2

		private int[] mixBuffer;				// The buffer to hold the mixed data
		private int bufferSize;					// The maximum number of samples a buffer can be
		private int ticksLeft;					// Number of ticks left to call the player

		private int filterPrevLeft;				// The previous value for the left channel
		private int filterPrevRight;			// The previous value for the right channel

		private int samplesTakenSinceLastCall;	// Holds number of samples taken since the last call to Play(). Used by channel visualizers

		private int ticksLeftToPositionChange;	// Number of ticks left before sending a position change event
		private int currentSongPosition;

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
			virtualChannelNumber = bufferDirect ? 2 : currentPlayer.VirtualChannelCount;
			enableChannelVisualization = bufferMode && ((currentPlayer.SupportFlags & ModulePlayerSupportFlag.Visualize) != 0);
			enableChannelsSupport = !bufferDirect || ((currentPlayer.SupportFlags & ModulePlayerSupportFlag.EnableChannels) == 0);

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
			ticksLeftToPositionChange = 0;
			samplesTakenSinceLastCall = 0;
			currentSongPosition = 0;

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
			outputChannelNumber = outputInformation.Channels;
			ticksLeftToPositionChange = CalculateTicksPerPositionChange();

			mixerChannels = 1;

			if (outputChannelNumber >= 2)
			{
				mixerMode |= MixerMode.Stereo;
				mixerChannels = 2;
			}
			else
				mixerMode &= ~MixerMode.Stereo;

			// Get the maximum number of samples the given destination
			// buffer from the output agent can be
			bufferSize = (outputInformation.BufferSizeInSamples / outputChannelNumber) * mixerChannels;

			// Allocate mixer buffer. This buffer is used by the mixer
			// routines to store the mixed data
			mixBuffer = new int[bufferSize + MixerBufferPadSize];

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
		/// Get current song position
		/// </summary>
		/********************************************************************/
		public int SongPosition
		{
			get => currentSongPosition;

			set
			{
				currentSongPosition = value;
				ticksLeftToPositionChange = CalculateTicksPerPositionChange();
			}
		}



		/********************************************************************/
		/// <summary>
		/// This is the main mixer method. It's the method that is called
		/// from the MixerStream to read the next bunch of data
		/// </summary>
		/********************************************************************/
		public int Mixing(byte[] buffer, int offset, int count, out bool hasEndReached)
		{
			// Remember the mixing mode. The reason to hold this in another
			// variable, it when the user change the mixing mode, it won't
			// be changed in the middle of a mixing, but used the next
			// time this method is called
			currentMode = mixerMode;

			bool isStereo = (currentMode & MixerMode.Stereo) != 0;
			bool swap = isStereo ? swapSpeakers : false;

			// Find the size of the buffer
			//
			// bufferSize = size of mixer buffer for either mono or stereo
			// count = size of output buffer for all channels the output need
			int outputCheckCount = (count / outputChannelNumber) * mixerChannels;
			int bufSize = Math.Min(bufferSize, outputCheckCount);

			int total = DoMixing(bufSize, out hasEndReached);
			if (total == 0)
				Array.Clear(buffer);

			if (playing)
			{
				// Add extra effects if enabled
				AddEffects(mixBuffer, total);

				// Add Amiga low-pass filter if enabled
				AddAmigaFilter(mixBuffer, total);
			}

			// Now convert the mixed data to our output format
			int samplesToSkip = isStereo ? outputChannelNumber - 2 : 0;
			currentMixer.ConvertMixedData(buffer, offset, mixBuffer, total, samplesToSkip, isStereo, swap);

			// Mix extra channels into the output buffer
			int total2 = MixExtraChannels(bufSize);
			if (total2 > 0)
				AddExtraChannelsIntoOutput(buffer, offset, total2, samplesToSkip, isStereo, swap);

			// Calculate total bytes really written
			total = Math.Max(total, total2);
			total = (total / mixerChannels) * outputChannelNumber;

			// Tell visual agents about the mixed data
			currentVisualizer.TellAgentsAboutMixedData(buffer, offset, Math.Max(total, bufSize), outputChannelNumber, swap);

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
		/// Return the number of ticks between each position change
		/// </summary>
		/********************************************************************/
		private int CalculateTicksPerPositionChange()
		{
			return (int)(IDurationPlayer.NumberOfSecondsBetweenEachSnapshot * mixerFrequency);
		}



		/********************************************************************/
		/// <summary>
		/// This is the main mixer method. It will call the right mixer
		/// and mix the main module samples
		/// </summary>
		/********************************************************************/
		private int DoMixing(int todo, out bool hasEndReached)
		{
			hasEndReached = false;

			int total = 0;

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
							while (ticksLeftToPositionChange <= 0)
							{
								currentSongPosition++;
								ticksLeftToPositionChange += CalculateTicksPerPositionChange();

								OnPositionChanged();
							}

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
								currentVisualizer.QueueChannelChange(channelChanges, samplesTakenSinceLastCall);

							// If any module information has been updated, queue those
							ModuleInfoChanged[] moduleInfoChanges = currentPlayer.GetChangedInformation();
							if ((moduleInfoChanges != null) && (moduleInfoChanges.Length > 0))
								currentVisualizer.QueueModuleInfoChange(moduleInfoChanges, samplesTakenSinceLastCall);

							samplesTakenSinceLastCall = 0;

							if (bufferDirect)
								ticksLeft = (int)voiceInfo[0].Size;
							else if (bufferMode)
								ticksLeft = (int)(mixerFrequency * voiceInfo[0].Size / voiceInfo[0].Frequency);
							else
							{
								// Calculate the number of sample pair to mix before the
								// player need to be called again
								ticksLeft = (int)(mixerFrequency / currentPlayer.PlayingFrequency);
							}

							BuildChannelMap();

							if (currentPlayer.HasEndReached)
							{
								currentPlayer.HasEndReached = false;
								hasEndReached = true;

								if (currentPlayer is IDurationPlayer durationPlayer)
								{
									double restartTime = durationPlayer.GetRestartTime().TotalMilliseconds;
									int ticksPerPosition = CalculateTicksPerPositionChange();

									currentSongPosition = (int)(restartTime / (IDurationPlayer.NumberOfSecondsBetweenEachSnapshot * 1000.0f));
									ticksLeftToPositionChange = (int)(ticksPerPosition - (((restartTime - (currentSongPosition * IDurationPlayer.NumberOfSecondsBetweenEachSnapshot * 1000.0f)) / 1000.0f) * mixerFrequency));
								}
								else
								{
									ticksLeftToPositionChange = CalculateTicksPerPositionChange();
									currentSongPosition = 0;
								}

								OnPositionChanged();

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
					ticksLeftToPositionChange -= left;
					todo -= left;
					total += (currentMode & MixerMode.Stereo) != 0 ? left << 1 : left;

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

				// Update module information
				foreach (ModuleInfoChanged[] updatedModuleInfoChanges in currentVisualizer.GetModuleInfoChanges((currentMode & MixerMode.Stereo) != 0 ? total >> 1 : total))
				{
					foreach (ModuleInfoChanged changes in updatedModuleInfoChanges)
						OnModuleInfoChanged(new ModuleInfoChangedEventArgs(changes.Line, changes.Value));
				}
			}

			return total;
		}



		/********************************************************************/
		/// <summary>
		/// This will mix any extra samples playing
		/// </summary>
		/********************************************************************/
		private int MixExtraChannels(int todo)
		{
			int total = 0;

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
		/// Will mix the channel mixer and extra channel mixer data together
		/// </summary>
		/********************************************************************/
		private void AddExtraChannelsIntoOutput(byte[] buffer, int offset, int todo, int samplesToSkip, bool isStereo, bool swap)
		{
			if ((extraTempBuffer == null) || (extraTempBuffer.Length < buffer.Length))
				extraTempBuffer = new byte[buffer.Length];

			extraChannelsMixer.ConvertMixedData(extraTempBuffer, 0, extraChannelsMixBuffer, todo, samplesToSkip, isStereo, swap);

			Span<int> source = MemoryMarshal.Cast<byte, int>(extraTempBuffer);
			Span<int> dest = MemoryMarshal.Cast<byte, int>(buffer);
			offset /= 4;
			todo = (todo / mixerChannels) * outputChannelNumber;

			for (int i = 0; i < todo; i++)
			{
				long val = dest[offset] + source[i];
				val = (val >= 2147483647) ? 2147483647 - 1 : (val < -2147483647) ? -2147483647 : val;
				dest[offset++] = (int)val;
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
