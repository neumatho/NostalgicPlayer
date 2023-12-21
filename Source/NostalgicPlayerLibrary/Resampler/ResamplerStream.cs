/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Diagnostics;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Events;
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
		private bool playing;

		private Resampler resampler;
		private object resamplerLock;

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
			resamplerLock = new object();

			resampler.PositionChanged += Resampler_PositionChanged;
			resampler.ModuleInfoChanged += Resampler_ModuleInfoChanged;

			return resampler.InitResampler(agentManager, playerConfiguration, out errorMessage);
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the stream
		/// </summary>
		/********************************************************************/
		public void Cleanup()
		{
			if (resamplerLock != null)
			{
				lock (resamplerLock)
				{
					resampler.CleanupResampler();

					resampler.ModuleInfoChanged -= Resampler_ModuleInfoChanged;
					resampler.PositionChanged -= Resampler_PositionChanged;

					resampler = null;
					resamplerLock = null;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will change the mixer configuration
		/// </summary>
		/********************************************************************/
		public void ChangeConfiguration(MixerConfiguration mixerConfiguration)
		{
			lock (resamplerLock)
			{
				resampler.ChangeConfiguration(mixerConfiguration);
			}
		}

		#region SoundStream implementation
		/********************************************************************/
		/// <summary>
		/// Set the output format
		/// </summary>
		/********************************************************************/
		public override void SetOutputFormat(OutputInfo outputInformation)
		{
			if (resamplerLock != null)
			{
				lock (resamplerLock)
				{
					resampler.SetOutputFormat(outputInformation);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Start the playing
		/// </summary>
		/********************************************************************/
		public override void Start()
		{
			lock (resamplerLock)
			{
				resampler.StartResampler();
				playing = true;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Stop the playing
		/// </summary>
		/********************************************************************/
		public override void Stop()
		{
			lock (resamplerLock)
			{
				playing = false;
				resampler.StopResampler();
			}
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
		/// Set the stream to the given song position
		/// </summary>
		/********************************************************************/
		public override int SongPosition
		{
			get
			{
				lock (resamplerLock)
				{
					return resampler.SongPosition;
				}
			}

			set
			{
				lock (resamplerLock)
				{
					resampler.SongPosition = value;
				}
			}
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

						Array.Clear(buffer, offset, todo);
						return todo;
					}

					int samplesTaken = resampler.Resampling(buffer, offset, count / OutputInfo.BytesPerSample, out bool hasEndReached);
					HasEndReached = hasEndReached;

					if (hasEndReached)
						delayCount = maxBufferSize;		// Set the delay count to a whole buffer

					return samplesTaken * OutputInfo.BytesPerSample;
				}

				return 0;
			}
			catch(Exception ex)
			{
				Debug.WriteLine(ex);
				throw;
			}
		}
		#endregion

		#region Handler methods
		/********************************************************************/
		/// <summary>
		/// Is called when the position changes in the resampler
		/// </summary>
		/********************************************************************/
		private void Resampler_PositionChanged(object sender, EventArgs e)
		{
			OnPositionChanged();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when some module information changes in the mixer
		/// </summary>
		/********************************************************************/
		private void Resampler_ModuleInfoChanged(object sender, ModuleInfoChangedEventArgs e)
		{
			OnModuleInfoChanged(e);
		}
		#endregion
	}
}
