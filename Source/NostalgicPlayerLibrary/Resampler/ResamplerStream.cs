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
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;
using Polycode.NostalgicPlayer.PlayerLibrary.Containers;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Resampler
{
	/// <summary>
	/// This stream do the playing of the samples and resample it to have the
	/// correct sample rate that the output agent want
	/// </summary>
	internal class ResamplerStream : SoundStream
	{
		private int bytesPerSampling;

		private bool playing;

		private Resampler resampler;

		private int maxBufferSize;
		private int delayCount;

		/********************************************************************/
		/// <summary>
		/// Initialize the stream
		/// </summary>
		/********************************************************************/
		public bool Initialize(Manager agentManager, PlayerConfiguration playerConfiguration, out string errorMessage)
		{
			playing = false;

			maxBufferSize = 0;
			delayCount = 0;

			resampler = new Resampler();
			return resampler.InitResampler(agentManager, playerConfiguration, out errorMessage);
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the stream
		/// </summary>
		/********************************************************************/
		public void Cleanup()
		{
			resampler?.CleanupResampler();
			resampler = null;
		}



		/********************************************************************/
		/// <summary>
		/// Will change the mixer configuration
		/// </summary>
		/********************************************************************/
		public void ChangeConfiguration(MixerConfiguration mixerConfiguration)
		{
			resampler.ChangeConfiguration(mixerConfiguration);
		}

		#region SoundStream implementation
		/********************************************************************/
		/// <summary>
		/// Set the output format
		/// </summary>
		/********************************************************************/
		public override void SetOutputFormat(OutputInfo outputInformation)
		{
			bytesPerSampling = outputInformation.BytesPerSample;

			resampler.SetOutputFormat(outputInformation);
		}



		/********************************************************************/
		/// <summary>
		/// Will set the master volume
		/// </summary>
		/********************************************************************/
		public override void SetMasterVolume(int volume)
		{
			resampler.SetMasterVolume(volume);
		}



		/********************************************************************/
		/// <summary>
		/// Start the playing
		/// </summary>
		/********************************************************************/
		public override void Start()
		{
			// Ok to play
			playing = true;
		}



		/********************************************************************/
		/// <summary>
		/// Stop the playing
		/// </summary>
		/********************************************************************/
		public override void Stop()
		{
			playing = false;
		}



		/********************************************************************/
		/// <summary>
		/// Pause the playing
		/// </summary>
		/********************************************************************/
		public override void Pause()
		{
			playing = false;
		}



		/********************************************************************/
		/// <summary>
		/// Resume the playing
		/// </summary>
		/********************************************************************/
		public override void Resume()
		{
			playing = true;
		}



		/********************************************************************/
		/// <summary>
		/// Read mixed data
		/// </summary>
		/********************************************************************/
		
		public override int Read(byte[] buffer, int offset, int count)
		{
			try
			{
				if (playing)
				{
					// To be sure that the whole sample has being played before trigger the EndReached
					// event, we want to play a full buffer of silence when the sample has ended
					//
					// We need to keep track of the maximum size the buffer could be
					maxBufferSize = Math.Max(maxBufferSize, count);

					// Check if we need to delay the filling (wait for the latency buffer to be played)
					if (delayCount > 0)
					{
						int todo = Math.Min(delayCount, count);

						delayCount -= todo;
						if (delayCount == 0)
						{
							// Now we're sure that the whole sampling has been played, so tell the client
							OnEndReached(this, EventArgs.Empty);
						}

						return 0;
					}

					int samplesTaken = resampler.Resampling(buffer, offset, count / bytesPerSampling, out bool hasEndReached);
					HasEndReached = hasEndReached;

					if (hasEndReached)
						delayCount = maxBufferSize;		// Set the delay count to a whole buffer

					return samplesTaken * bytesPerSampling;
				}

				return 0;
			}
			catch(Exception)
			{
				return 0;
			}
		}
		#endregion
	}
}
