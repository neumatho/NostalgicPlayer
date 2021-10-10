/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using NAudio.Wave;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Output.CoreAudio
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class CoreAudioWorker : OutputAgentBase, IAgentSettingsRegistrar, IAudioSessionEventsHandler, IMMNotificationClient
	{
		/// <summary>
		/// Playback State
		/// </summary>
		private enum PlaybackState
		{
			/// <summary>
			/// Just initialized
			/// </summary>
			Initialized,
			/// <summary>
			/// Stopped
			/// </summary>
			Stopped,
			/// <summary>
			/// Playing
			/// </summary>
			Playing,
			/// <summary>
			/// Paused
			/// </summary>
			Paused
		}

		private const int LatencyMilliseconds = 200;

		private object streamLock;
		private SoundStream stream;

		private MMDevice endpoint;
		private AudioClient audioClient;

		private WaveFormat outputFormat;

		private AudioRenderClient audioRenderClient;
		private int bufferFrameCount;
		private int bytesPerFrame;
		private byte[] readBuffer;

		private AudioSessionControl audioSessionControl;

		private AutoResetEvent shutdownEvent;
		private AutoResetEvent audioSamplesReadyEvent;
		private AutoResetEvent streamSwitchEvent;
		private ManualResetEvent streamSwitchCompletedEvent;
		private Thread renderThread;

		private AutoResetEvent flushBufferEvent;

		private object playingLock;
		private volatile PlaybackState playbackState;
		private bool inStreamSwitch;

		private CoreAudioSettings settings;
		private string currentEndpointId;

		#region IOutputAgent implementation
		/********************************************************************/
		/// <summary>
		/// Will initialize the output driver
		/// </summary>
		/********************************************************************/
		public override AgentResult Initialize(out string errorMessage)
		{
			errorMessage = string.Empty;

			try
			{
				settings = new CoreAudioSettings();

				stream = null;
				streamLock = new object();

				endpoint = FindEndpointToUse();
				audioClient = endpoint.AudioClient;

				// Create our shutdown and samples ready events - we want auto reset events that start in the not-signaled state
				shutdownEvent = new AutoResetEvent(false);
				audioSamplesReadyEvent = new AutoResetEvent(false);
				streamSwitchEvent = new AutoResetEvent(false);

				// Create stop/flush events
				flushBufferEvent = new AutoResetEvent(false);

				// Create lock used when playing
				playingLock = new object();

				// Initialize the audio engine
				InitializeAudioEngine();

				// Initialize stream switch support
				InitializeStreamSwitch();

				// Now create the thread which is going to drive the renderer
				renderThread = new Thread(DoRenderThread);
				renderThread.Start();

				return AgentResult.Ok;
			}
			catch (Exception ex)
			{
				Shutdown();

				errorMessage = string.Format(Resources.IDS_ERR_INITIALIZE, ex.HResult.ToString("X8"), ex.Message);
				return AgentResult.Error;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will shutdown the output driver
		/// </summary>
		/********************************************************************/
		public override void Shutdown()
		{
			// Tell the render thread to exit
			shutdownEvent?.Set();

			// Stop feeding the render thread
			if (audioClient != null)
			{
				if ((playbackState == PlaybackState.Playing) || (playbackState == PlaybackState.Stopped))
				{
					audioClient.Stop();
					audioClient.Reset();
				}
			}

			// Wait for the render thread to exit
			renderThread?.Join();

			// Cleanup stream switch
			TerminateStreamSwitch();

			// Clear all variables
			renderThread = null;

			audioClient?.Dispose();
			audioClient = null;

			streamSwitchEvent?.Dispose();
			streamSwitchEvent = null;

			audioSamplesReadyEvent?.Dispose();
			audioSamplesReadyEvent = null;

			shutdownEvent?.Dispose();
			shutdownEvent = null;

			endpoint?.Dispose();
			endpoint = null;

			flushBufferEvent?.Dispose();
			flushBufferEvent = null;

			playingLock = null;

			stream = null;
		}



		/********************************************************************/
		/// <summary>
		/// Tell the engine to begin playing
		/// </summary>
		/********************************************************************/
		public override void Play()
		{
			lock (playingLock)
			{
				switch (playbackState)
				{
					case PlaybackState.Initialized:
					case PlaybackState.Stopped:
					{
						// Fill a whole buffer
						FillBuffer(bufferFrameCount);

						// Begin to play sound
						audioClient.Start();
						playbackState = PlaybackState.Playing;
						break;
					}

					case PlaybackState.Paused:
					{
						// Just continue playing
						audioClient.Start();
						playbackState = PlaybackState.Playing;
						break;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Tell the engine to stop playing
		/// </summary>
		/********************************************************************/
		public override void Stop()
		{
			if ((playbackState == PlaybackState.Playing) || (playbackState == PlaybackState.Stopped))
			{
				// Stop the audio
				audioClient.Stop();

				lock (playingLock)
				{
					// Set state
					playbackState = PlaybackState.Stopped;
				}
			}

			// Tell the render thread to flush buffers
			flushBufferEvent.Set();
		}



		/********************************************************************/
		/// <summary>
		/// Tell the engine to pause playing
		/// </summary>
		/********************************************************************/
		public override void Pause()
		{
			// Uncommented, because the audio client should not be stopped,
			// else sample playing won't work when a module is paused
/*			if (playbackState == PlaybackState.Playing)
			{
				// Stop the audio
				audioClient.Stop();

				lock (playingLock)
				{
					// Set state
					playbackState = PlaybackState.Paused;
				}
			}
*/		}



		/********************************************************************/
		/// <summary>
		/// Will switch the stream to read the sound data from without
		/// interrupting the sound
		/// </summary>
		/********************************************************************/
		public override AgentResult SwitchStream(SoundStream soundStream, string fileName, string moduleName, string author, out string errorMessage)
		{
			errorMessage = string.Empty;

			lock (streamLock)
			{
				stream?.Dispose();

				int bytesPerSample = outputFormat.BitsPerSample / 8;
				soundStream.SetOutputFormat(new OutputInfo(outputFormat.Channels, outputFormat.SampleRate, (outputFormat.AverageBytesPerSecond / bytesPerSample) * LatencyMilliseconds / 1000, bytesPerSample));
				stream = soundStream;
			}

			return AgentResult.Ok;
		}
		#endregion

		#region IAgentSettingsRegistrar implementation
		/********************************************************************/
		/// <summary>
		/// Return the agent ID for the agent showing the settings
		/// </summary>
		/********************************************************************/
		public Guid GetSettingsAgentId()
		{
			return new Guid("EE538BB1-9CE3-472D-B030-D46B321A405A");
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will return the endpoint device to use based on the user settings
		/// </summary>
		/********************************************************************/
		private MMDevice FindEndpointToUse()
		{
			string device = settings.OutputDevice;

			using (MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator())
			{
				MMDevice foundEndpoint = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).FirstOrDefault(d => d.ID == device);
				if (foundEndpoint == null)
					foundEndpoint = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

				currentEndpointId = foundEndpoint.ID;

				return foundEndpoint;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Initialize for playing the specified wave stream
		/// </summary>
		/********************************************************************/
		private void InitializeAudioEngine()
		{
			long latencyRefTimes = LatencyMilliseconds * 10000;

			// Try these formats in this order:
			//
			// 32-bit, 2 channels
			// 16-bit, 2 channels
			// 32-bit, 1 channel
			// 16-bit, 1 channel
			outputFormat = new WaveFormat(audioClient.MixFormat.SampleRate, 32, 2);
			if (!audioClient.IsFormatSupported(AudioClientShareMode.Shared, outputFormat, out WaveFormatExtensible _))
			{
				outputFormat = new WaveFormat(audioClient.MixFormat.SampleRate, 16, 2);
				if (!audioClient.IsFormatSupported(AudioClientShareMode.Shared, outputFormat, out WaveFormatExtensible _))
				{
					outputFormat = new WaveFormat(audioClient.MixFormat.SampleRate, 32, 1);
					if (!audioClient.IsFormatSupported(AudioClientShareMode.Shared, outputFormat, out WaveFormatExtensible _))
					{
						outputFormat = new WaveFormat(audioClient.MixFormat.SampleRate, 16, 1);
						if (!audioClient.IsFormatSupported(AudioClientShareMode.Shared, outputFormat, out WaveFormatExtensible _))
							throw new Exception(Resources.IDS_ERR_NO_OUTPUT_DEVICE_FOUND);
					}
				}
			}

			// Normal setup for both sharedMode
			audioClient.Initialize(AudioClientShareMode.Shared, AudioClientStreamFlags.EventCallback | AudioClientStreamFlags.NoPersist, latencyRefTimes, 0, outputFormat, Guid.Empty);

			// Get the RenderClient
			audioRenderClient = audioClient.AudioRenderClient;

			// Set up the read buffer
			bufferFrameCount = audioClient.BufferSize;
			bytesPerFrame = outputFormat.Channels * outputFormat.BitsPerSample / 8;
			readBuffer = new byte[bufferFrameCount * bytesPerFrame];

			// Make sure that the render thread does not play anything
			audioClient.Stop();
			playbackState = PlaybackState.Initialized;

			// Set the event to use to trigger the render thread to fill out the buffer
			audioClient.SetEventHandle(audioSamplesReadyEvent.SafeWaitHandle.DangerousGetHandle());
		}



		/********************************************************************/
		/// <summary>
		/// Initialize stream switch support
		/// </summary>
		/********************************************************************/
		private void InitializeStreamSwitch()
		{
			inStreamSwitch = false;

			audioSessionControl = endpoint.AudioSessionManager.AudioSessionControl;

			// Create the stream complete event
			streamSwitchCompletedEvent = new ManualResetEvent(false);

			// Register for session and endpoint change notifications
			audioSessionControl.RegisterEventClient(this);

			using (MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator())
			{
				deviceEnumerator.RegisterEndpointNotificationCallback(this);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup stream switch
		/// </summary>
		/********************************************************************/
		private void TerminateStreamSwitch()
		{
			// Unregister change notifications
			try
			{
				audioSessionControl?.UnRegisterEventClient(this);
			}
			catch(Exception)
			{
			}

			try
			{
				using (MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator())
				{
					deviceEnumerator?.UnregisterEndpointNotificationCallback(this);
				}
			}
			catch(Exception)
			{
			}

			streamSwitchCompletedEvent?.Dispose();
			streamSwitchCompletedEvent = null;

			audioSessionControl?.Dispose();
			audioSessionControl = null;
		}



		/********************************************************************/
		/// <summary>
		/// Handles the stream switch
		/// </summary>
		/********************************************************************/
		private bool HandleSwitchEvent()
		{
			if (!inStreamSwitch)
				return true;

			try
			{
				lock (playingLock)
				{
					PlaybackState oldState = playbackState;

					// Step 1: Stop rendering
					audioClient.Stop();

					// Step 2: Release our resources
					audioSessionControl.UnRegisterEventClient(this);
					audioSessionControl.Dispose();
					audioSessionControl = null;

					audioRenderClient.Dispose();
					audioRenderClient = null;

					audioClient.Dispose();
					audioClient = null;

					endpoint.Dispose();
					endpoint = null;

					// Step 3: Wait for the default device to change.
					//
					// There is a race between the session disconnect arriving and the new default device
					// arriving (if applicable). Wait the shorter of 500 milliseconds or the arrival of the
					// new default device, then attempt to switch to the default device. In the case of a
					// format change (i.e. the default device does not change), we artificially generate a
					// new default device notification so the code will not needlessly wait 500ms before
					// re-opening on the new format
					if (!streamSwitchCompletedEvent.WaitOne(500))
						return false;

					// Step 4: If we can't get the new endpoint, we need to abort the stream switch.
					//         If there IS a new device, we should be able to retrieve it
					endpoint = FindEndpointToUse();

					// Step 5: Re-instantiate the audio client on the new endpoint
					audioClient = endpoint.AudioClient;

					// Step 6: Re-initialize the audio client
					InitializeAudioEngine();

					// Step 7: Re-register for session disconnect notifications
					audioSessionControl = endpoint.AudioSessionManager.AudioSessionControl;
					audioSessionControl.RegisterEventClient(this);

					// Step 8: Reset the stream switch completed event because it's a manual reset event
					streamSwitchCompletedEvent.Reset();

					// And we're done. Start rendering again if needed
					if (oldState == PlaybackState.Playing)
						audioClient.Start();

					// Tell the mixer about new sample rates etc.
					int bytesPerSample = outputFormat.BitsPerSample / 8;
					stream.SetOutputFormat(new OutputInfo(outputFormat.Channels, outputFormat.SampleRate, (outputFormat.AverageBytesPerSecond / bytesPerSample) * LatencyMilliseconds / 1000, bytesPerSample));

					playbackState = oldState;
				}

				return true;
			}
			catch (Exception)
			{
				return false;
			}
			finally
			{
				inStreamSwitch = false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will trigger a switch of the output device
		/// </summary>
		/********************************************************************/
		private void TriggerSwitch()
		{
			// If we're not in a stream switch already, we want to initiate a
			// stream switch event. We also want to set the default switch
			// completed event. That will signal the render thread that it's
			// ok to re-initialize the audio renderer
			if (!inStreamSwitch)
			{
				inStreamSwitch = true;
				streamSwitchEvent.Set();
			}

			streamSwitchCompletedEvent.Set();
		}



		/********************************************************************/
		/// <summary>
		/// Background thread that feeds to output device
		/// </summary>
		/********************************************************************/
		private void DoRenderThread()
		{
			try
			{
				// Create a shared event that will be triggered if the settings has changed
				using (EventWaitHandle settingsChangedEvent = new EventWaitHandle(false, EventResetMode.ManualReset, "NostalgicPlayer_CoreAudio"))
				{
					bool stillPlaying = true;
					WaitHandle[] waitArray = { shutdownEvent, streamSwitchEvent, audioSamplesReadyEvent, flushBufferEvent, settingsChangedEvent };

					while (stillPlaying)
					{
						int waitResult = WaitHandle.WaitAny(waitArray);
						switch (waitResult)
						{
							// ShutdownEvent
							case 0:
							{
								stillPlaying = false;
								break;
							}

							// StreamSwitchEvent
							case 1:
							{
								// We have received a stream switch request.
								//
								// We need to stop the renderer, tear down the audio client and render client objects
								// and recreate them on the new endpoint if possible. If this fails, abort the thread
								if (!HandleSwitchEvent())
									stillPlaying = false;

								break;
							}

							// AudioSamplesReadyEvent
							case 2:
							{
								lock (playingLock)
								{
									if (playbackState == PlaybackState.Playing)
									{
										int numFramesPadding = audioClient.CurrentPadding;
										int numFramesAvailable = bufferFrameCount - numFramesPadding;
										if (numFramesAvailable > 0)
											FillBuffer(numFramesAvailable);
									}
								}
								break;
							}

							// FlushBufferEvent
							case 3:
							{
								audioClient.Reset();
								break;
							}

							// SettingsChangedEvent
							case 4:
							{
								// We have received a setting update
								settingsChangedEvent.Reset();

								// Reload the settings
								settings = new CoreAudioSettings();

								// Switch the endpoint if needed
								TriggerSwitch();
								break;
							}
						}
					}
				}
			}
			catch (Exception)
			{
				// If an exception is thrown, abort the thread
			}
		}



		/********************************************************************/
		/// <summary>
		/// Fill the buffer with the next bunch of samples
		/// </summary>
		/********************************************************************/
		private void FillBuffer(int frameCount)
		{
			lock (streamLock)
			{
				IntPtr buffer = audioRenderClient.GetBuffer(frameCount);

				int readLength = frameCount * bytesPerFrame;
				int read = stream == null ? 0 : stream.Read(readBuffer, 0, readLength);
				if (read > 0)
					Marshal.Copy(readBuffer, 0, buffer, read);

				int actualFrameCount = read / bytesPerFrame;
				audioRenderClient.ReleaseBuffer(actualFrameCount, AudioClientBufferFlags.None);
			}
		}
		#endregion

		#region IAudioSessionEventsHandler implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void OnVolumeChanged(float volume, bool isMuted)
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void OnDisplayNameChanged(string displayName)
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void OnIconPathChanged(string iconPath)
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void OnChannelVolumeChanged(uint channelCount, IntPtr newVolumes, uint channelIndex)
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void OnGroupingParamChanged(ref Guid groupingId)
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void OnStateChanged(AudioSessionState state)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Called when an audio session is disconnected
		/// </summary>
		/********************************************************************/
		public void OnSessionDisconnected(AudioSessionDisconnectReason disconnectReason)
		{
			// When a session is disconnected because of a device removal or format change event,
			// we just want to let the renderer thread know that the session's gone away
			if (disconnectReason == AudioSessionDisconnectReason.DisconnectReasonDeviceRemoval)
			{
				// The stream was disconnected because the device we're rendering to was removed.
				//
				// We want to reset the stream switch complete event (so we'll block when the
				// HandleStreamSwitchEvent method waits until the default device changed event occurs
				//
				// Note that we _don't_ set the StreamSwitchCompletedEvent - that will be set
				// when the OnDefaultDeviceChanged event occurs
				inStreamSwitch = true;
				streamSwitchEvent.Set();
			}
			else if (disconnectReason == AudioSessionDisconnectReason.DisconnectReasonFormatChanged)
			{
				// The stream was disconnected because the format changed on our render device.
				//
				// We want to flag that we're in a stream switch and then set the stream switch event
				// (which breaks out of the renderer). We also want to set the StreamSwitchCompletedEvent
				// because we're not going to see a default change event after this
				inStreamSwitch = true;
				streamSwitchEvent.Set();
				streamSwitchCompletedEvent.Set();
			}
		}
		#endregion

		#region IMMNotificationClient implementation
		/********************************************************************/
		/// <summary>
		/// Called when the state changes, e.g. if a device is removed
		/// </summary>
		/********************************************************************/
		public void OnDeviceStateChanged(string deviceId, DeviceState newState)
		{
			if (newState == DeviceState.Active)
			{
				// Check if the new device is the one in the settings
				if (deviceId == settings.OutputDevice)
					TriggerSwitch();
			}
			else
			{
				if (deviceId == currentEndpointId)
					TriggerSwitch();
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void OnDeviceAdded(string pwstrDeviceId)
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void OnDeviceRemoved(string deviceId)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Called when the default render device changed
		/// </summary>
		/********************************************************************/
		public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
		{
			// We just want to set an event which lets the stream switch logic
			// know that it's ok to continue with the stream switch
			if ((flow == DataFlow.Render) && (role == Role.Multimedia))
			{
				// The default render device for our configured role was changed.
				TriggerSwitch();
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
		{
		}
		#endregion
	}
}
