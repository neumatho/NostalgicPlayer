/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Events;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Library.Agent;
using Polycode.NostalgicPlayer.Library.Containers;

namespace Polycode.NostalgicPlayer.Library.Sound.Resampler
{
	/// <summary>
	/// This stream do the playing of the samples and resample it to have the
	/// correct sample rate that the output agent want
	/// </summary>
	internal class ResamplerStream : SoundStream
	{
		private bool playing;

		private Resampler resampler;
		private Lock resamplerLock;

		private int maxBufferSizeInFrames;
		private int delayCount;
		private int outputChannelNumber;

		/********************************************************************/
		/// <summary>
		/// Initialize the stream
		/// </summary>
		/********************************************************************/
		public bool Initialize(Manager agentManager, PlayerConfiguration playerConfiguration, out string errorMessage)
		{
			playing = false;

			maxBufferSizeInFrames = 0;
			delayCount = 0;

			resampler = new Resampler();
			resamplerLock = new Lock();

			resampler.ClockUpdated += Resampler_ClockUpdated;
			resampler.PositionChanged += Resampler_PositionChanged;
			resampler.ModuleInfoChanged += Resampler_ModuleInfoChanged;

			return resampler.Initialize(agentManager, playerConfiguration, out errorMessage);
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
					resampler.Cleanup();

					resampler.ModuleInfoChanged -= Resampler_ModuleInfoChanged;
					resampler.PositionChanged -= Resampler_PositionChanged;
					resampler.ClockUpdated -= Resampler_ClockUpdated;

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
				outputChannelNumber = outputInformation.Channels;

				lock (resamplerLock)
				{
					resampler.SetOutputFormat(outputInformation);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return which speakers that are used to play the sound. Is first
		/// available after calling SetOutputFormat()
		/// </summary>
		/********************************************************************/
		public override SpeakerFlag VisualizerSpeakers
		{
			get
			{
				if (resamplerLock == null)
					return 0;

				lock (resamplerLock)
				{
					return resampler.VisualizerSpeakers;
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
				resampler.Start();
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
				resampler.Stop();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Pause the playing
		/// </summary>
		/********************************************************************/
		public override void Pause()
		{
			lock (resamplerLock)
			{
				playing = false;
				resampler.Pause();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Resume the playing
		/// </summary>
		/********************************************************************/
		public override void Resume()
		{
			lock (resamplerLock)
			{
				playing = true;
				resampler.Resume();
			}
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
		public override int Read(byte[] buffer, int offsetInBytes, int frameCount)
		{
			try
			{
				if (playing)
				{
					// To be sure that the whole sample has being played before trigger the EndReached
					// event, we want to play a full buffer of silence when the sample has ended
					//
					// We need to keep track of the maximum size the buffer could be
					maxBufferSizeInFrames = Math.Max(maxBufferSizeInFrames, frameCount);

					// Check if we need to delay the filling (wait for the latency buffer to be played)
					if (delayCount > 0)
					{
						int todoInFrames = Math.Min(delayCount, frameCount);

						delayCount -= todoInFrames;
						if (delayCount == 0)
						{
							// Now we're sure that the whole sampling has been played, so tell the client
							OnEndReached(this, EventArgs.Empty);
						}

						Array.Clear(buffer, offsetInBytes, todoInFrames * outputChannelNumber * OutputInfo.BytesPerSample);
						return todoInFrames;
					}

					Span<int> bufferSpan = MemoryMarshal.Cast<byte, int>(buffer.AsSpan(offsetInBytes));
					int framesTaken = resampler.Resampling(bufferSpan, frameCount, out bool hasEndReached);
					HasEndReached = hasEndReached;

					if (hasEndReached)
						delayCount = maxBufferSizeInFrames;		// Set the delay count to a whole buffer

					return framesTaken;
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
		/// Is called when the clock is updated in the resampler
		/// </summary>
		/********************************************************************/
		private void Resampler_ClockUpdated(object sender, ClockUpdatedEventArgs e)
		{
			OnClockUpdated(e);
		}



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
