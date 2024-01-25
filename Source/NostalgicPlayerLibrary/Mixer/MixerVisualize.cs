﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Threading;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;
using Timer = System.Timers.Timer;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Mixer
{
	/// <summary>
	/// Helper class to tell visual agents about the music
	/// </summary>
	internal class MixerVisualize
	{
		private class ChannelDataInfo
		{
			public int SamplesLeftBeforeTriggering;
			public ChannelChanged[] ChannelChanges;
		}

		private class SampleDataInfo
		{
			public int[] Buffer;
			public int NumberOfChannels;
			public bool SwapSpeakers;
			public long TimeWhenBufferWasRendered;
		}

		private class ModuleInfoChangeInfo
		{
			public int SamplesLeftBeforeTriggering;
			public ModuleInfoChanged[] ModuleInfoChanges;
		}

		private const int SampleBufferSizeInMs = 20;
		private const int MaxMilliSecondsInSampleBuffer = 300;

		private Manager manager;

		private int mixerFrequency;

		private volatile ChannelChanged[] channelChanges;

		private AutoResetEvent channelChangedEvent;
		private ManualResetEvent exitEvent;

		private Thread thread;
		private Timer timer;

		private int[] visualBuffer;			// Is the current buffer that is filled with samples that visuals should show. When filled, it will be stored in a list and a new buffer is allocated
		private int visualBufferOffset;		// The position in the buffer above where to fill the next samples

		private int currentLatency;

		private Queue<ChannelDataInfo> channelLatencyQueue;
		private int channelLatencySamplesLeft;

		private List<SampleDataInfo> sampleDataList;
		private long currentSampleDataTimeWhenFilling;
		private long currentSampleDataTimeForTimer;
		private int sampleDataLatencyLeft;

		private Queue<ModuleInfoChangeInfo> moduleInfoLatencyQueue;
		private int moduleInfoLatencySamples;
		private int moduleInfoLatencySamplesLeft;

		/********************************************************************/
		/// <summary>
		/// Will initialize itself
		/// </summary>
		/********************************************************************/
		public void Initialize(Manager agentManager)
		{
			manager = agentManager;

			channelLatencyQueue = new Queue<ChannelDataInfo>();

			sampleDataList = new List<SampleDataInfo>();
			currentSampleDataTimeWhenFilling = 0;
			currentSampleDataTimeForTimer = 0;

			moduleInfoLatencyQueue = new Queue<ModuleInfoChangeInfo>();

			// Create the trigger events
			channelChangedEvent = new AutoResetEvent(false);

			// Create exit event used in the thread
			exitEvent = new ManualResetEvent(false);

			// Initialize the thread and start it
			thread = new Thread(VisualizeThread);
			thread.Name = "Visualizer distributor";
			thread.Start();

			// Initialize timer
			timer = new Timer(SampleBufferSizeInMs);
			timer.AutoReset = true;
			timer.Elapsed += VisualizeTimer;
			timer.Start();
		}



		/********************************************************************/
		/// <summary>
		/// Will cleanup again
		/// </summary>
		/********************************************************************/
		public void Cleanup()
		{
			// Tell the timer to stop
			if (timer != null)
			{
				timer.Stop();
				timer.Dispose();

				timer = null;
			}

			// Tell the thread to exit
			if (exitEvent != null)
			{
				exitEvent.Set();
				thread.Join();

				exitEvent.Dispose();
				exitEvent = null;

				thread = null;
			}

			// Clear variables
			channelChangedEvent?.Dispose();
			channelChangedEvent = null;

			moduleInfoLatencyQueue = null;
			channelLatencyQueue = null;
			sampleDataList = null;
		}



		/********************************************************************/
		/// <summary>
		/// Set the output format
		/// </summary>
		/********************************************************************/
		public void SetOutputFormat(OutputInfo outputInformation)
		{
			mixerFrequency = outputInformation.Frequency;

			visualBuffer = new int[mixerFrequency / (1000 / SampleBufferSizeInMs) * outputInformation.Channels];
			visualBufferOffset = 0;

			InitializeVisualsLatency();
		}



		/********************************************************************/
		/// <summary>
		/// Set the visuals latency
		/// </summary>
		/********************************************************************/
		public void SetVisualsLatency(int latency)
		{
			currentLatency = latency;

			InitializeVisualsLatency();
		}



		/********************************************************************/
		/// <summary>
		/// Will call all visual agents and tell them about new pause state
		/// </summary>
		/********************************************************************/
		public void TellAgentsAboutPauseState(bool paused)
		{
			foreach (IVisualAgent visualAgent in manager.GetRegisteredVisualAgent())
				visualAgent.SetPauseState(paused);

			lock (sampleDataList)
			{
				sampleDataList.Clear();
				sampleDataLatencyLeft = currentLatency;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will queue the given channel change information
		/// </summary>
		/********************************************************************/
		public void QueueChannelChange(ChannelChanged[] changes, int samplesTakenSinceLastCall)
		{
			ChannelDataInfo channelDataInfo = new ChannelDataInfo
			{
				SamplesLeftBeforeTriggering = samplesTakenSinceLastCall,
				ChannelChanges = changes
			};

			channelLatencyQueue.Enqueue(channelDataInfo);
		}



		/********************************************************************/
		/// <summary>
		/// Will queue the given module information change information
		/// </summary>
		/********************************************************************/
		public void QueueModuleInfoChange(ModuleInfoChanged[] moduleInfoChanges, int samplesTakenSinceLastCall)
		{
			ModuleInfoChangeInfo moduleInfoChange = new ModuleInfoChangeInfo
			{
				SamplesLeftBeforeTriggering = samplesTakenSinceLastCall,
				ModuleInfoChanges = moduleInfoChanges
			};

			moduleInfoLatencyQueue.Enqueue(moduleInfoChange);
		}



		/********************************************************************/
		/// <summary>
		/// Will call all visual agents and tell them about channel changes
		/// </summary>
		/********************************************************************/
		public void TellAgentsAboutChannelChange(int samplesProcessed)
		{
			channelLatencySamplesLeft -= samplesProcessed;
			if (channelLatencySamplesLeft < 0)
			{
				samplesProcessed = -channelLatencySamplesLeft;
				channelLatencySamplesLeft = 0;
			}

			if (channelLatencySamplesLeft == 0)
			{
				while ((channelLatencyQueue.Count > 0) && (samplesProcessed > 0))
				{
					ChannelDataInfo peekInfo = channelLatencyQueue.Peek();
					int todo = Math.Min(samplesProcessed, peekInfo.SamplesLeftBeforeTriggering);

					peekInfo.SamplesLeftBeforeTriggering -= todo;
					if (peekInfo.SamplesLeftBeforeTriggering == 0)
					{
						channelChanges = channelLatencyQueue.Dequeue().ChannelChanges;

						// Tell the thread that a channel has changed its status
						channelChangedEvent.Set();
					}

					samplesProcessed -= todo;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will call all visual agents and tell them about the new mixed
		/// data
		/// </summary>
		/********************************************************************/
		public void TellAgentsAboutMixedData(byte[] buffer, int offset, int count, int numberOfChannels, bool swapSpeakers)
		{
			while (count > 0)
			{
				int todo = Math.Min(count, visualBuffer.Length - visualBufferOffset);

				// Mixed output is already in 32-bit, so just copy the data
				Buffer.BlockCopy(buffer, offset, visualBuffer, visualBufferOffset * 4, todo * 4);

				visualBufferOffset += todo;
				offset += todo * 4;

				count -= todo;

				if (visualBufferOffset == visualBuffer.Length)
				{
					SampleDataInfo info = new SampleDataInfo
					{
						Buffer = visualBuffer,
						NumberOfChannels = numberOfChannels,
						SwapSpeakers = swapSpeakers,
						TimeWhenBufferWasRendered = currentSampleDataTimeWhenFilling
					};

					lock (sampleDataList)
					{
						sampleDataList.Add(info);
					}

					currentSampleDataTimeWhenFilling += SampleBufferSizeInMs;

					visualBuffer = new int[visualBuffer.Length];
					visualBufferOffset = 0;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will tell if it's time to send module information changes
		/// </summary>
		/********************************************************************/
		public List<ModuleInfoChanged[]> GetModuleInfoChanges(int samplesProcessed)
		{
			List<ModuleInfoChanged[]> changes = new List<ModuleInfoChanged[]>();
			int samplesTaken = 0;

			while (samplesProcessed > 0)
			{
				int todo = Math.Min(samplesProcessed, moduleInfoLatencySamplesLeft);
				moduleInfoLatencySamplesLeft -= todo;
				samplesTaken += todo;

				if (moduleInfoLatencySamplesLeft == 0)
				{
					moduleInfoLatencySamplesLeft = moduleInfoLatencySamples;

					if (moduleInfoLatencyQueue.Count > 0)
					{
						ModuleInfoChangeInfo peekInfo = moduleInfoLatencyQueue.Peek();

						peekInfo.SamplesLeftBeforeTriggering -= samplesTaken;
						if ((peekInfo.SamplesLeftBeforeTriggering <= 0) || (todo == 0))
							changes.Add(moduleInfoLatencyQueue.Dequeue().ModuleInfoChanges);
					}
				}

				samplesProcessed -= todo;

				if (todo == 0)
					break;
			}

			return changes;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the visuals latency variables
		/// </summary>
		/********************************************************************/
		private void InitializeVisualsLatency()
		{
			channelLatencySamplesLeft = (int)(((float)mixerFrequency / 1000) * currentLatency);
			sampleDataLatencyLeft = currentLatency;
			moduleInfoLatencySamples = channelLatencySamplesLeft;
			moduleInfoLatencySamplesLeft = moduleInfoLatencySamples;

			channelLatencyQueue.Clear();

			lock (sampleDataList)
			{
				sampleDataList.Clear();
			}
		}



		/********************************************************************/
		/// <summary>
		/// This is the main visualize thread, that will communicate with the
		/// agents
		/// </summary>
		/********************************************************************/
		private void VisualizeThread()
		{
			// Initialize the synchronize objects to wait for
			WaitHandle[] waitArray = { exitEvent, channelChangedEvent };

			bool stillRunning = true;

			while (stillRunning)
			{
				// Wait for something to happen
				int waitResult = WaitHandle.WaitAny(waitArray);

				switch (waitResult)
				{
					// exitEvent
					case 0:
					{
						stillRunning = false;
						break;
					}

					// channelChangedEvent
					case 1:
					{
						ChannelChanged[] channelChanged = channelChanges;

						foreach (IVisualAgent visualAgent in manager.GetRegisteredVisualAgent())
						{
							if (visualAgent is IChannelChangeVisualAgent channelChangeVisualAgent)
								channelChangeVisualAgent.ChannelsChanged(channelChanged);
						}
						break;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called for every 20 ms and will give the next sample buffer
		/// to the visualizer agents
		/// </summary>
		/********************************************************************/
		private void VisualizeTimer(object sender, System.Timers.ElapsedEventArgs e)
		{
			SampleDataInfo sampleDataInfo = null;

			lock (sampleDataList)
			{
				if (sampleDataLatencyLeft > 0)
					sampleDataLatencyLeft -= SampleBufferSizeInMs;
				else
				{
					if (sampleDataList.Count > 0)
					{
						// Check if we're behind in time. This can happen if e.g. using the Disk Saver
						// output agent. If so, skip some of the buffers until we're in sync again
						long maxTime = currentSampleDataTimeForTimer + MaxMilliSecondsInSampleBuffer - currentLatency;
						long lastItemTime = sampleDataList[^1].TimeWhenBufferWasRendered;

						if (lastItemTime >= maxTime)
						{
							maxTime = lastItemTime - currentLatency;

							while (sampleDataList[0].TimeWhenBufferWasRendered < maxTime)
								sampleDataList.RemoveAt(0);

							currentSampleDataTimeForTimer = maxTime;
						}

						if (sampleDataList.Count > 0)
						{
							sampleDataInfo = sampleDataList[0];
							sampleDataList.RemoveAt(0);
						}
					}
				}
			}

			if (sampleDataInfo != null)
			{
				NewSampleData sampleData = new NewSampleData(sampleDataInfo.Buffer, sampleDataInfo.NumberOfChannels, sampleDataInfo.SwapSpeakers);

				foreach (IVisualAgent visualAgent in manager.GetRegisteredVisualAgent())
				{
					if (visualAgent is ISampleDataVisualAgent sampleDataVisualAgent)
						sampleDataVisualAgent.SampleData(sampleData);
				}
			}

			currentSampleDataTimeForTimer += SampleBufferSizeInMs;
		}
	}
}
