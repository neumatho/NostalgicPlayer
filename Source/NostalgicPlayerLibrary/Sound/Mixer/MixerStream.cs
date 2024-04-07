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

namespace Polycode.NostalgicPlayer.PlayerLibrary.Sound.Mixer
{
	/// <summary>
	/// This stream do the playing of the modules and mix the channels
	/// </summary>
	internal class MixerStream : SoundStream
	{
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

			mixer.PositionChanged += Mixer_PositionChanged;
			mixer.ModuleInfoChanged += Mixer_ModuleInfoChanged;

			return mixer.Initialize(agentManager, playerConfiguration, out errorMessage);
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the stream
		/// </summary>
		/********************************************************************/
		public void Cleanup()
		{
			if (mixerLock != null)
			{
				lock (mixerLock)
				{
					mixer.Cleanup();

					mixer.PositionChanged -= Mixer_PositionChanged;

					mixer = null;
					mixerLock = null;
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
			if (mixerLock != null)
			{
				lock (mixerLock)
				{
					mixer.SetOutputFormat(outputInformation);
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
			lock (mixerLock)
			{
				mixer.Start();
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
				mixer.Stop();
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
				mixer.Pause();
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
				mixer.Resume();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Holds the current song position
		/// </summary>
		/********************************************************************/
		public override int SongPosition
		{
			get
			{
				lock (mixerLock)
				{
					return mixer.SongPosition;
				}
			}

			set
			{
				lock (mixerLock)
				{
					mixer.SongPosition = value;
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
				int mixedSamples;

				if (mixerLock == null)
					return 0;

				lock (mixerLock)
				{
					// In some rare cases, a cleanup is about to happen and the mixer has been cleared while waiting for the lock
					if (mixer == null)
						return 0;

					int samplesMixed = mixer.Mixing(buffer, offset, count / OutputInfo.BytesPerSample, out bool hasEndReached);
					HasEndReached = hasEndReached;

					mixedSamples = samplesMixed * OutputInfo.BytesPerSample;
				}

				if (HasEndReached)
					OnEndReached(this, EventArgs.Empty);

				return mixedSamples;
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
		/// Is called when the position changes in the mixer
		/// </summary>
		/********************************************************************/
		private void Mixer_PositionChanged(object sender, EventArgs e)
		{
			OnPositionChanged();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when some module information changes in the mixer
		/// </summary>
		/********************************************************************/
		private void Mixer_ModuleInfoChanged(object sender, ModuleInfoChangedEventArgs e)
		{
			OnModuleInfoChanged(e);
		}
		#endregion
	}
}
