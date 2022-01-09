/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;
using Polycode.NostalgicPlayer.PlayerLibrary.Containers;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Mixer
{
	/// <summary>
	/// This stream do the playing of the modules and mix the channels
	/// </summary>
	internal class MixerStream : SoundStream
	{
		private int bytesPerSampling;

		private Mixer mixer;
		private object mixerLock;

		/********************************************************************/
		/// <summary>
		/// Initialize the stream
		/// </summary>
		/********************************************************************/
		public bool Initialize(Manager agentManager, PlayerConfiguration playerConfiguration, out string errorMessage)
		{
			mixer = new Mixer();
			mixerLock = new object();

			return mixer.InitMixer(agentManager, playerConfiguration, out errorMessage);
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the stream
		/// </summary>
		/********************************************************************/
		public void Cleanup()
		{
			mixer?.CleanupMixer();
			mixer = null;
			mixerLock = null;
		}



		/********************************************************************/
		/// <summary>
		/// Will change the mixer configuration
		/// </summary>
		/********************************************************************/
		public void ChangeConfiguration(MixerConfiguration mixerConfiguration)
		{
			lock (mixerLock)
			{
				mixer.ChangeConfiguration(mixerConfiguration);
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
			lock (mixerLock)
			{
				bytesPerSampling = outputInformation.BytesPerSample;

				mixer.SetOutputFormat(outputInformation);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will set the master volume
		/// </summary>
		/********************************************************************/
		public override void SetMasterVolume(int volume)
		{
			lock (mixerLock)
			{
				mixer.SetMasterVolume(volume);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Start the playing
		/// </summary>
		/********************************************************************/
		public override void Start()
		{
			lock (mixerLock)
			{
				mixer.StartMixer();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Stop the playing
		/// </summary>
		/********************************************************************/
		public override void Stop()
		{
			lock (mixerLock)
			{
				mixer.StopMixer();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Pause the playing
		/// </summary>
		/********************************************************************/
		public override void Pause()
		{
			lock (mixerLock)
			{
				mixer.PauseMixer();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Resume the playing
		/// </summary>
		/********************************************************************/
		public override void Resume()
		{
			lock (mixerLock)
			{
				mixer.ResumeMixer();
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
				int mixedSamples;

				lock (mixerLock)
				{
					int samplesMixed = mixer.Mixing(buffer, offset, count / bytesPerSampling, out bool hasEndReached);
					HasEndReached = hasEndReached;

					mixedSamples = samplesMixed * bytesPerSampling;
				}

				if (HasEndReached)
					OnEndReached(this, EventArgs.Empty);

				return mixedSamples;
			}
			catch(Exception)
			{
				return 0;
			}
		}
		#endregion
	}
}
