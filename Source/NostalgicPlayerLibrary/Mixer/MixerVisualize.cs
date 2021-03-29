/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
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
		private Manager manager;

		private Channel[] channelInfo;
		private Channel.Flags[] channelFlags;

		private AutoResetEvent channelChangedEvent;
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

			// Create the trigger event
			channelChangedEvent = new AutoResetEvent(false);

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
		public void TellAgentsAboutMixedData(byte[] buffer, int offset, int size)
		{
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
						ChannelChanged channelChanged = new ChannelChanged(channelInfo, channelFlags);

						foreach (IVisualAgent visualAgent in manager.GetRegisteredVisualAgent())
						{
							if (visualAgent is IChannelChangeVisualAgent channelChangeVisualAgent)
								channelChangeVisualAgent.ChannelChange(channelChanged);
						}
						break;
					}
				}
			}
		}
	}
}
