/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Threading;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Mixer;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Mixer
{
	/// <summary>
	/// Helper class to tell visual agents about the music
	/// </summary>
	internal class MixerVisualize
	{
		private class SampleDataInfo
		{
			public byte[] Buffer;
			public int NumberOfSamples;
			public int SampleSize;
			public bool Stereo;
			public bool SwapSpeakers;
		}

		private Manager manager;

		private Channel[] channelInfo;
		private Channel.Flags[] channelFlags;

		private volatile SampleDataInfo sampleDataInfo;

		private AutoResetEvent channelChangedEvent;
		private AutoResetEvent newSampleDataEvent;
		private ManualResetEvent exitEvent;

		private Thread thread;

		/********************************************************************/
		/// <summary>
		/// Will initialize itself
		/// </summary>
		/********************************************************************/
		public void Initialize(Manager agentManager, int numberOfChannels, Channel[] channels)
		{
			manager = agentManager;

			channelInfo = channels;
			channelFlags = new Channel.Flags[numberOfChannels];

			for (int i = 0; i < numberOfChannels; i++)
				channelFlags[i] = Channel.Flags.None;

			// Create the trigger events
			channelChangedEvent = new AutoResetEvent(false);
			newSampleDataEvent = new AutoResetEvent(false);

			// Create exit event used in the thread
			exitEvent = new ManualResetEvent(false);

			// Initialize the thread and start it
			thread = new Thread(VisualizeThread);
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
		}



		/********************************************************************/
		/// <summary>
		/// Will return the array holding all the channel flags
		/// </summary>
		/********************************************************************/
		public Channel.Flags[] GetFlagsArray()
		{
			return channelFlags;
		}



		/********************************************************************/
		/// <summary>
		/// Will call all visual agents and tell them about the new mixed
		/// data
		/// </summary>
		/********************************************************************/
		public void TellAgentsAboutMixedData(byte[] buffer, int numberOfSamples, int sampleSize, bool stereo, bool swapSpeakers)
		{
			// Create local object first, before store it in the object variable
			SampleDataInfo info = new SampleDataInfo
			{
				Buffer = buffer,
				NumberOfSamples = numberOfSamples,
				SampleSize = sampleSize,
				Stereo = stereo,
				SwapSpeakers = swapSpeakers
			};

			sampleDataInfo = info;

			// Tell the thread about new data
			newSampleDataEvent.Set();
		}



		/********************************************************************/
		/// <summary>
		/// Will call all visual agents and tell them about channel changes
		/// </summary>
		/********************************************************************/
		public void TellAgentsAboutChannelChange()
		{
			// Tell the thread that a channel has changed its status
			channelChangedEvent.Set();
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
						ChannelChanged channelChanged = new ChannelChanged(channelInfo, channelFlags);

						foreach (IVisualAgent visualAgent in manager.GetRegisteredVisualAgent())
						{
							if (visualAgent is IChannelChangeVisualAgent channelChangeVisualAgent)
								channelChangeVisualAgent.ChannelChange(channelChanged);
						}
						break;
					}

					// newSampleDataEvent
					case 2:
					{
						SampleDataInfo info = sampleDataInfo;
						NewSampleData sampleData = null;

						foreach (IVisualAgent visualAgent in manager.GetRegisteredVisualAgent())
						{
							if (visualAgent is ISampleDataVisualAgent sampleDataVisualAgent)
							{
								// Will first use CPU cycles to convert the buffer, if needed
								if (sampleData == null)
								{
									int[] buffer = new int[info.NumberOfSamples];

									if (info.SampleSize == 4)
									{
										// Mixed output is already in 32-bit, so just copy the data
										Buffer.BlockCopy(info.Buffer, 0, buffer, 0, info.NumberOfSamples * 4);
									}
									else
									{
										// Mixed output is in 16-bit, so convert it to 32-bit
										for (int i = 0, cnt = info.NumberOfSamples; i < cnt; i++)
											buffer[i] = BitConverter.ToInt16(info.Buffer, i * 2) << 16;
									}

									sampleData = new NewSampleData(buffer, info.Stereo);
								}

								sampleDataVisualAgent.SampleData(sampleData);
							}
						}
						break;
					}
				}
			}
		}
	}
}
