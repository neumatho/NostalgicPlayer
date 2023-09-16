/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;

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
			public bool Stereo;
			public bool SwapSpeakers;
		}

		private Manager manager;

		private int channelCount;
		private int mixerFrequency;
		private int bytesPerSample;

		private volatile ChannelChanged[] channelChanges;
		private volatile SampleDataInfo sampleDataInfo;

		private AutoResetEvent channelChangedEvent;
		private AutoResetEvent newSampleDataEvent;
		private ManualResetEvent exitEvent;

		private Thread thread;

		private int[] visualBuffer;			// Holding samples that visuals should show. It contain samples for 20 milliseconds
		private int visualBufferOffset;		// The position in the buffer above where to fill the next samples

		private int minimumLatency;
		private int currentLatency;

		private Queue<ChannelDataInfo> channelLatencyQueue;
		private int channelLatencySamplesLeft;

		private Queue<SampleDataInfo> bufferLatencyQueue;
		private int bufferLatencySamplesLeft;
		private bool bufferLatencyUseQueue;

		/********************************************************************/
		/// <summary>
		/// Will initialize itself
		/// </summary>
		/********************************************************************/
		public void Initialize(Manager agentManager, int numberOfChannels)
		{
			manager = agentManager;

			channelCount = numberOfChannels;

			channelLatencyQueue = new Queue<ChannelDataInfo>();

			bufferLatencyQueue = new Queue<SampleDataInfo>();
			bufferLatencyUseQueue = false;

			// Create the trigger events
			channelChangedEvent = new AutoResetEvent(false);
			newSampleDataEvent = new AutoResetEvent(false);

			// Create exit event used in the thread
			exitEvent = new ManualResetEvent(false);

			// Initialize the thread and start it
			thread = new Thread(VisualizeThread);
			thread.Name = "Visualizer distributor";
			thread.Start();
		}



		/********************************************************************/
		/// <summary>
		/// Will cleanup again
		/// </summary>
		/********************************************************************/
		public void Cleanup()
		{
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
			newSampleDataEvent?.Dispose();
			newSampleDataEvent = null;

			channelChangedEvent?.Dispose();
			channelChangedEvent = null;

			channelLatencyQueue = null;
			bufferLatencyQueue = null;
		}



		/********************************************************************/
		/// <summary>
		/// Set the output format
		/// </summary>
		/********************************************************************/
		public void SetOutputFormat(OutputInfo outputInformation)
		{
			mixerFrequency = outputInformation.Frequency;
			bytesPerSample = outputInformation.BytesPerSample;

			visualBufferOffset = 0;
			visualBuffer = new int[Math.Min(outputInformation.BufferSizeInSamples, mixerFrequency / (1000 / 20) * outputInformation.Channels)];

			minimumLatency = outputInformation.BufferSizeInSamples * 1000 / outputInformation.Frequency;

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
		}



		/********************************************************************/
		/// <summary>
		/// Will queue the given channel change information
		/// </summary>
		/********************************************************************/
		public void QueueChannelChange(ChannelChanged[] channelChanges, int samplesTakenSinceLastCall)
		{
			ChannelDataInfo channelDataInfo = new ChannelDataInfo
			{
				SamplesLeftBeforeTriggering = samplesTakenSinceLastCall,
				ChannelChanges = channelChanges
			};

			channelLatencyQueue.Enqueue(channelDataInfo);
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
		public void TellAgentsAboutMixedData(byte[] buffer, int offset, int count, bool stereo, bool swapSpeakers)
		{
			int[] bufferToSend = null;

			while (count > 0)
			{
				int todo = Math.Min(count, visualBuffer.Length - visualBufferOffset);

				if (bufferLatencySamplesLeft > 0)
					todo = Math.Min(todo, bufferLatencySamplesLeft);

				if (bytesPerSample == 4)
				{
					// Mixed output is already in 32-bit, so just copy the data
					Buffer.BlockCopy(buffer, offset, visualBuffer, visualBufferOffset * 4, todo * 4);

					visualBufferOffset += todo;
					offset += todo * 4;
				}
				else
				{
					// Mixed output is in 16-bit, so convert it to 32-bit
					Span<short> source = MemoryMarshal.Cast<byte, short>(buffer);

					for (int i = 0, cnt = todo; i < cnt; i++)
						visualBuffer[visualBufferOffset++] = source[offset++] << 16;
				}

				count -= todo;

				if (bufferLatencySamplesLeft > 0)
					bufferLatencySamplesLeft -= todo;

				if (visualBufferOffset == visualBuffer.Length)
				{
					bufferToSend = visualBuffer;

					visualBuffer = new int[visualBuffer.Length];
					visualBufferOffset = 0;
				}
			}

			if (bufferToSend != null)
			{
				SampleDataInfo info = new SampleDataInfo
				{
					Buffer = bufferToSend,
					Stereo = stereo,
					SwapSpeakers = swapSpeakers
				};

				bufferLatencyQueue.Enqueue(info);

				if (bufferLatencySamplesLeft == 0)
				{
					if (bufferLatencyUseQueue && (bufferLatencyQueue.Count > 0))
					{
						sampleDataInfo = bufferLatencyQueue.Dequeue();

						// Tell the thread about new data
						newSampleDataEvent.Set();
					}
					else
						bufferLatencyUseQueue = true;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the visuals latency variables
		/// </summary>
		/********************************************************************/
		private void InitializeVisualsLatency()
		{
			channelLatencySamplesLeft = (int)(((float)mixerFrequency / 1000) * currentLatency);
			bufferLatencySamplesLeft = (int)(((float)mixerFrequency / 1000) * (minimumLatency + currentLatency));

			bufferLatencyUseQueue = false;
			bufferLatencyQueue.Clear();
			channelLatencyQueue.Clear();
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
			WaitHandle[] waitArray = { exitEvent, channelChangedEvent, newSampleDataEvent };

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

					// newSampleDataEvent
					case 2:
					{
						SampleDataInfo info = sampleDataInfo;
						NewSampleData sampleData = new NewSampleData(info.Buffer, info.Stereo, info.SwapSpeakers);

						foreach (IVisualAgent visualAgent in manager.GetRegisteredVisualAgent())
						{
							if (visualAgent is ISampleDataVisualAgent sampleDataVisualAgent)
								sampleDataVisualAgent.SampleData(sampleData);
						}
						break;
					}
				}
			}
		}
	}
}
