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

		/********************************************************************/
		/// <summary>
		/// Initialize the stream
		/// </summary>
		/********************************************************************/
		public bool Initialize(PlayerConfiguration playerConfiguration, out string errorMessage)
		{
			playing = false;

			resampler = new Resampler();
			return resampler.InitResampler(playerConfiguration, out errorMessage);
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
					int samplesTaken = resampler.Resampling(buffer, offset, count / bytesPerSampling, out bool hasEndReached);
					HasEndReached = hasEndReached;

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
