/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;
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
		private DownMixer downMixer;
		private Visualizer currentVisualizer;
		private AmigaFilter amigaFilter;

		private readonly Lock mixerInfoLock = new Lock();
		private MixerInfo mixerInfo;			// The current mixer information

		private int mixerFrequency;				// The mixer frequency

		private int virtualChannelCount;		// The number of channels the module use
		private int mixerChannelCount;			// The number of channels to mix in
		private int outputChannelCount;			// The number of channels the output want

		private int[][] mixingBuffers;			// A buffer for each mixer channel to hold the mixed data
		private int bufferSizeInFrames;			// The maximum number of frames a buffer can be
		private int framesLeft;					// Number of frames left to call the player

		private int framesTakenSinceLastCall;	// Holds number of frames taken since the last call to Play(). Used by channel visualizers

		private Dictionary<int, int[][]> groupBuffers;
		private int[][][] mixerBufferMap;		// Maps each virtual channel to mixer buffers

		// Extra channels variables
		private IExtraChannels extraChannelsInstance;
		private int extraChannelsNumber;
		private MixerBase extraChannelsMixer;
		private IChannel[] extraChannelsChannels;

		private int[][] extraChannelsMixingBuffer;
		private int[][][] extraChannelsMixerBufferMap;
		private int[] extraChannelsTempBuffer;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Mixer()
		{
			// Initialize member variables
			playing = false;
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
				virtualChannelCount = bufferDirect ? 2 : currentPlayer.VirtualChannelCount;
				mixerChannelCount = 2;
				enableChannelVisualization = bufferMode && ((currentPlayer.SupportFlags & ModulePlayerSupportFlag.Visualize) != 0);
				enableChannelsSupport = !bufferDirect || ((currentPlayer.SupportFlags & ModulePlayerSupportFlag.EnableChannels) == 0);

				// Allocate the visual component
				currentVisualizer = new Visualizer();

				if (bufferDirect)
					currentMixer = new MixerBufferDirect();
				else
					currentMixer = new MixerNormal();

				// Initialize the mixer
				currentMixer.Initialize(virtualChannelCount, mixerChannelCount);

				// Allocate channel objects
				IChannel[] channels = new IChannel[virtualChannelCount];
				for (int i = 0; i < virtualChannelCount; i++)
					channels[i] = new ChannelParser();

				currentPlayer.VirtualChannels = channels;

				// Initialize the visualizer
				currentVisualizer.Initialize(agentManager);

				// Initialize extra channels
				InitializeExtraChannels(playerConfiguration.MixerConfiguration.ExtraChannels);

				// Initialize the mixers
				lock (mixerInfoLock)
				{
					mixerInfo = new MixerInfo();
					ChangeConfiguration(playerConfiguration.MixerConfiguration);
				}
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
			downMixer = null;

			// Deallocate the visualizer
			currentVisualizer?.Cleanup();
			currentVisualizer = null;

			// Deallocate mixer buffer
			mixingBuffers = null;
			groupBuffers = null;
			mixerBufferMap = null;

			// Deallocate Amiga filter
			amigaFilter = null;

			// Deallocate channel objects
			if (currentPlayer != null)
			{
				currentPlayer.VirtualChannels = null;
				currentPlayer = null;
			}

			lock (mixerInfoLock)
			{
				mixerInfo = null;
			}

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
			outputChannelCount = outputInformation.Channels;

			// Get the maximum number of frames the given destination
			// buffer from the output agent can be
			bufferSizeInFrames = outputInformation.BufferSizeInFrames;

			// Allocate mixer buffer. This buffer is used by the mixer
			// routines to store the mixed data
			mixingBuffers = CreateMixerBuffers();

			currentMixer.SetOutputFormat(outputInformation);
			currentVisualizer.SetOutputFormat(outputInformation);
			amigaFilter = new AmigaFilter(mixerFrequency, outputChannelCount);

			lock (currentPlayer)
			{
				if ((currentPlayer.SupportFlags & ModulePlayerSupportFlag.BufferMode) != 0)
					currentPlayer.SetOutputFormat((uint)mixerFrequency, outputInformation.Channels);

				downMixer = new DownMixer(currentPlayer.SpeakerFlags, outputInformation.AvailableSpeakers);
			}

			// Create pool of buffers needed for extra effects
			groupBuffers = new Dictionary<int, int[][]>();
			mixerBufferMap = new int[virtualChannelCount][][];

			// As default, map all channels to use the same output buffer
			for (int i = 0; i < virtualChannelCount; i++)
			{
				mixerBufferMap[i] = new int[mixerChannelCount][];

				for (int j = 0; j < mixerChannelCount; j++)
					mixerBufferMap[i][j] = mixingBuffers[j];
			}

			// Initialize extra channels
			if (extraChannelsMixer != null)
			{
				extraChannelsMixingBuffer = CreateMixerBuffers();
				extraChannelsMixer.SetOutputFormat(outputInformation);

				extraChannelsMixerBufferMap = new int[extraChannelsNumber][][];

				for (int i = 0; i < extraChannelsNumber; i++)
				{
					extraChannelsMixerBufferMap[i] = new int[mixerChannelCount][];

					for (int j = 0; j < mixerChannelCount; j++)
						extraChannelsMixerBufferMap[i][j] = extraChannelsMixingBuffer[j];
				}
			}
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

			lock (mixerInfoLock)
			{
				mixerInfo.EnableInterpolation = mixerConfiguration.EnableInterpolation;
				mixerInfo.EnableSurround = mixerConfiguration.EnableSurround;
				mixerInfo.SwapSpeakers = mixerConfiguration.SwapSpeakers;
				mixerInfo.EmulateFilter = mixerConfiguration.EnableAmigaFilter;

				Array.Copy(mixerConfiguration.ChannelsEnabled, mixerInfo.ChannelsEnabled, mixerConfiguration.ChannelsEnabled.Length);
			}

			lock (currentPlayer)
			{
				currentPlayer.ChangeMixerConfiguration(mixerInfo);
			}
		}



		/********************************************************************/
		/// <summary>
		/// This is the main mixer method. It's the method that is called
		/// from the MixerStream to read the next bunch of data.
		///
		/// The mixer can take any number of mixing channels and mix them to
		/// any number of output channels. This is done to support more
		/// channels than stereo.
		///
		/// A mixing channel indicates a virtual speaker channel that a
		/// player will use. Most module players will use 2 channels for
		/// stereo, but sample players can e.g. return 5 channels (2 front,
		/// 2 back and subwoofer).
		///
		/// The mixer will then mix these channels to the number of output
		/// channels you have setup in Windows. That way you can play a 5.1
		/// sample on a stereo setup (and hear all channels) or in a real
		/// surround setup.
		///
		/// Here is a diagram showing how it works:
		///
		/// Player is called which uses the virtual module channels
		///                         |
		/// Mix those channels into number of player channels
		///                         |
		/// Add DSP effects, such as echo
		///                         |
		/// Downmix/upmix to the number of output channels including swapping
		/// left and right channel if configured
		///                         |
		/// Add Amiga filter if enabled
		///                         |
		/// Return mixed data to output agent
		/// </summary>
		/********************************************************************/
		public int Mixing(Span<int> outputBuffer, int frameCount, out bool hasEndReached)
		{
			MixerInfo currentMixerInfo;

			lock (mixerInfoLock)
			{
				if (mixerInfo == null)
				{
					hasEndReached = true;
					return 0;
				}

				// Remember the mixer information in a local variable.
				// The reason to hold this in another variable, it when the
				// user change the mixing mode, it won't be changed in the
				// middle of a mixing, but used the next time this method is called
				currentMixerInfo = new MixerInfo(mixerInfo);
			}

			// Find out how many frames to process
			int framesToProcess = Math.Min(frameCount, bufferSizeInFrames);

			int totalFramesProcessed = DoMixing(currentMixerInfo, framesToProcess, out hasEndReached);
			if (totalFramesProcessed > 0)
			{
				// Now convert the mixed data to real 32 bit samples
				var buffersToConvert = mixerBufferMap.SelectMany(x => x).Union(mixingBuffers).Distinct();
				foreach (int[] buffer in buffersToConvert)
					currentMixer.ConvertMixedData(buffer, totalFramesProcessed);

				// Add extra effects if enabled and mix all buffers into mixingBuffers
				AddEffects(totalFramesProcessed);

				// Convert to output format
				downMixer.ConvertToOutputFormat(currentMixerInfo, mixingBuffers, outputBuffer, totalFramesProcessed);

				// Add Amiga low-pass filter if enabled
				if (currentMixerInfo.EmulateFilter && currentPlayer.AmigaFilter)
					amigaFilter.Apply(outputBuffer, totalFramesProcessed);
			}
			else
				outputBuffer.Slice(0, frameCount * outputChannelCount).Clear();

			// Mix extra channels into the output buffer
			int extraFramesProcessed = MixExtraChannels(currentMixerInfo, framesToProcess);
			if (extraFramesProcessed > 0)
				AddExtraChannelsIntoOutput(currentMixerInfo, outputBuffer, extraFramesProcessed);

			// Calculate total frames really written
			int totalFrames = Math.Max(totalFramesProcessed, extraFramesProcessed);

			// Tell visual agents about the mixed data
			currentVisualizer.TellAgentsAboutMixedData(outputBuffer, Math.Max(totalFrames, framesToProcess), downMixer.GetVisualizersChannelMapping(currentMixerInfo), outputChannelCount);

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
					extraChannelsMixer.Initialize(extraChannelsNumber, mixerChannelCount);

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
				extraChannelsMixingBuffer = null;
				extraChannelsMixerBufferMap = null;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Initialize mixer buffer info array
		/// </summary>
		/********************************************************************/
		private int[][] CreateMixerBuffers()
		{
			return ArrayHelper.Initialize2Arrays<int>(mixerChannelCount, bufferSizeInFrames);
		}



		/********************************************************************/
		/// <summary>
		/// This is the main mixer method. It will call the right mixer and
		/// mix the main module samples.
		/// </summary>
		/********************************************************************/
		private int DoMixing(MixerInfo currentMixerInfo, int framesToProcess, out bool hasEndReached)
		{
			hasEndReached = false;

			int totalFrames = 0;

			// Prepare the mixing buffers
			foreach (int[] buffer in mixingBuffers)
				Array.Clear(buffer, 0, buffer.Length);

			foreach (int[] buffer in groupBuffers.Values.SelectMany(x => x))
				Array.Clear(buffer, 0, buffer.Length);

			if (playing)
			{
				// This method is called right after the previous buffer has been player,
				// so at this point, add the number of samples played since last call
				DoTimedEvents();
				IncreaseCurrentTime(framesToProcess);

				int todoInFrames = framesToProcess;
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

							ChannelChanged[] channelChanges = new ChannelChanged[virtualChannelCount];

							for (int t = 0; t < virtualChannelCount; t++)
							{
								// virtualChannelNumber can be higher than ChannelsEnabled.Length for e.g. Impulse Tracker modules
								bool channelEnabled = t < currentMixerInfo.ChannelsEnabled.Length ? currentMixerInfo.ChannelsEnabled[t] : true;
								channelChanges[t] = ((ChannelParser)currentPlayer.VirtualChannels[t]).ParseInfo(voiceInfo[t], click, channelEnabled, bufferMode);
							}

							if (enableChannelVisualization)
								channelChanges = currentPlayer.VisualChannels;

							// If at least one channel has changed its information,
							// tell visual agents about it
							if ((channelChanges != null) && channelChanges.Any(x => x != null))
								currentVisualizer.QueueChannelChange(channelChanges, framesTakenSinceLastCall);

							// If any module information has been updated, queue those
							ModuleInfoChanged[] moduleInfoChanges = currentPlayer.GetChangedInformation();
							if (moduleInfoChanges != null)
							{
								foreach (ModuleInfoChanged moduleInfoChanged in moduleInfoChanges)
									timedEventHandler.AddEvent(new ModuleInfoChangedEvent(this, moduleInfoChanged), framesTakenSinceLastCall);
							}

							framesTakenSinceLastCall = 0;

							if (bufferDirect)
								framesLeft = (int)voiceInfo[0].SampleInfo.Sample.Length;
							else if (bufferMode)
								framesLeft = (int)(mixerFrequency * voiceInfo[0].SampleInfo.Sample.Length / voiceInfo[0].Frequency);
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
					currentVisualizer.TellAgentsAboutChannelChange(leftInFrames);

					// And mix it
					currentMixer.Mixing(currentMixerInfo, mixerBufferMap, totalFrames, leftInFrames);

					// Calculate new values for the counter variables
					framesTakenSinceLastCall += leftInFrames;
					framesLeft -= leftInFrames;
					todoInFrames -= leftInFrames;
					totalFrames += leftInFrames;

					if (enableChannelsSupport)
					{
						// Check all the channels to see if they are still active and
						// enable/disable the channels depending on the user settings
						for (int t = 0; t < virtualChannelCount; t++)
						{
							((ChannelParser)currentPlayer.VirtualChannels[t]).Active(currentMixer.IsActive(t));

							if (t < currentMixerInfo.ChannelsEnabled.Length)
								currentMixer.EnableChannel(t, currentMixerInfo.ChannelsEnabled[t]);
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
		private int MixExtraChannels(MixerInfo currentMixerInfo, int todoInFrames)
		{
			int totalFrames = 0;

			// Add extra channels if needed
			if ((extraChannelsInstance != null) && (extraChannelsNumber > 0))
			{
				if (extraChannelsInstance.PlayChannels(extraChannelsChannels))
				{
					foreach (int[] buffer in extraChannelsMixingBuffer)
						Array.Clear(buffer, 0, buffer.Length);

					// Get some mixer information we need to parse the data
					VoiceInfo[] voiceInfo = extraChannelsMixer.GetMixerChannels();
					int click = extraChannelsMixer.GetClickConstant();

					// Parse the channels
					for (int i = 0; i < extraChannelsNumber; i++)
						((ChannelParser)extraChannelsChannels[i]).ParseInfo(voiceInfo[i], click, true, false);

					// Mix the data
					extraChannelsMixer.Mixing(currentMixerInfo, extraChannelsMixerBufferMap, 0, todoInFrames);

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
		private void AddExtraChannelsIntoOutput(MixerInfo currentMixerInfo, Span<int> outputBuffer, int todoInFrames)
		{
			if ((extraChannelsTempBuffer == null) || (extraChannelsTempBuffer.Length < outputBuffer.Length))
				extraChannelsTempBuffer = new int[outputBuffer.Length];

			// Now convert the mixed data to real 32 bit samples
			foreach (int[] buffer in extraChannelsMixingBuffer)
				currentMixer.ConvertMixedData(buffer, todoInFrames);

			// Convert to output format
			downMixer.ConvertToOutputFormat(currentMixerInfo, extraChannelsMixingBuffer, extraChannelsTempBuffer, todoInFrames);

			// Mix extra output format into the main output format
			int todoInSamples = todoInFrames * outputChannelCount;

			for (int i = 0; i < todoInSamples; i++)
			{
				long val = (long)extraChannelsTempBuffer[i] + outputBuffer[i];
				val = (val >= int.MaxValue) ? int.MaxValue - 1 : (val < int.MinValue) ? int.MinValue : val;
				outputBuffer[i] = (int)val;
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

				for (int i = 0; i < virtualChannelCount; i++)
				{
					if ((groupMap != null) && groupMap.TryGetValue(i, out int group))
					{
						if (!groupBuffers.TryGetValue(group, out int[][] buffer))
						{
							// No buffer found, allocate a new one
							buffer = CreateMixerBuffers();
							groupBuffers[group] = buffer;
						}

						for (int j = 0; j < mixerChannelCount; j++)
							mixerBufferMap[i][j] = buffer[j];
					}
					else
					{
						// If channel is not mapped, use global buffer
						for (int j = 0; j < mixerChannelCount; j++)
							mixerBufferMap[i][j] = mixingBuffers[j];
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Adds extra DSP effects from e.g. the current player
		/// </summary>
		/********************************************************************/
		private void AddEffects(int todoInFrames)
		{
			// Add effect on each group and mix them together
			lock (currentPlayer)
			{
				IEffectMaster effectMaster = currentPlayer.EffectMaster;
				if (effectMaster != null)
				{
					foreach (KeyValuePair<int, int[][]> pair in groupBuffers)
					{
						effectMaster.AddChannelGroupEffects(pair.Key, pair.Value, todoInFrames, (uint)mixerFrequency);

						for (int i = 0; i < mixerChannelCount; i++)
						{
							int[] effectBuffer = pair.Value[i];
							int[] destBuffer = mixingBuffers[i];

							for (int j = 0; j < todoInFrames; j++)
							{
								long val = (long)destBuffer[j] + effectBuffer[j];
								val = (val >= int.MaxValue) ? int.MaxValue - 1 : (val < int.MinValue) ? int.MinValue : val;
								destBuffer[j] = (int)val;
							}
						}
					}

					effectMaster.AddGlobalEffects(mixingBuffers, todoInFrames, (uint)mixerFrequency);
				}
			}
		}
		#endregion
	}
}
