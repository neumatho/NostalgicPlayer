/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Linq;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Events;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Library.Agent;
using Polycode.NostalgicPlayer.Library.Containers;
using Polycode.NostalgicPlayer.Library.Loaders;
using Polycode.NostalgicPlayer.Library.Sound.Resampler;

namespace Polycode.NostalgicPlayer.Library.Players
{
	/// <summary>
	/// Main class to play samples
	/// </summary>
	internal class SamplePlayer : ISamplePlayer
	{
		private readonly IAgentManager agentManager;

		private ISamplePlayerAgent currentPlayer;
		private DurationInfo durationInfo;

		private IOutputAgent outputAgent;
		private ResamplerStream soundStream;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SamplePlayer()
		{
			agentManager = DependencyInjection.Container.GetInstance<IAgentManager>();

			// Initialize member variables
			StaticModuleInformation = new ModuleInfoStatic();
			PlayingModuleInformation = new ModuleInfoFloating();

			currentPlayer = null;
		}

		#region IPlayer implementation
		/********************************************************************/
		/// <summary>
		/// Return a string containing a warning string. If there is no
		/// warning, an empty string is returned
		/// </summary>
		/********************************************************************/
		public string GetWarning(LoaderInfoBase loaderInfo)
		{
			return string.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// Will initialize the player
		/// </summary>
		/********************************************************************/
		public bool InitPlayer(PlayerConfiguration playerConfiguration, out string errorMessage)
		{
			errorMessage = string.Empty;
			bool initOk;

			try
			{
				outputAgent = playerConfiguration.OutputAgent;

				Loader loader = playerConfiguration.LoaderInfo as Loader;
				if (loader == null)
					throw new ArgumentException("Invalid loader object");

				// Remember the player
				currentPlayer = (ISamplePlayerAgent)loader.WorkerAgent;

				lock (currentPlayer)
				{
					// Initialize the player
					initOk = currentPlayer.InitPlayer(loader.Stream, out errorMessage);

					if (initOk)
					{
						// Calculate the duration of the sample
						CalculateDuration();

						// Initialize module information
						StaticModuleInformation = new ModuleInfoStatic(loader, currentPlayer);

						// Initialize the mixer
						soundStream = new ResamplerStream();

						// Subscribe the events
						soundStream.ClockUpdated += Stream_ClockUpdated;
						soundStream.PositionChanged += Stream_PositionChanged;
						soundStream.EndReached += Stream_EndReached;
						soundStream.ModuleInfoChanged += Stream_ModuleInfoChanged;

						initOk = soundStream.Initialize(playerConfiguration, out errorMessage);

						if (!initOk)
							CleanupPlayer();
					}
					else
						CleanupPlayer();
				}
			}
			catch (Exception ex)
			{
				CleanupPlayer();

				errorMessage = string.Format(Resources.IDS_ERR_PLAYER_INIT, ex.Message);
				initOk = false;
			}

			return initOk;
		}



		/********************************************************************/
		/// <summary>
		/// Will cleanup the player
		/// </summary>
		/********************************************************************/
		public void CleanupPlayer()
		{
			try
			{
				if (currentPlayer != null)
				{
					// End the mixer
					if (soundStream != null)
					{
						soundStream.ModuleInfoChanged -= Stream_ModuleInfoChanged;
						soundStream.EndReached -= Stream_EndReached;
						soundStream.PositionChanged -= Stream_PositionChanged;
						soundStream.ClockUpdated += Stream_ClockUpdated;

						soundStream.Cleanup();
						soundStream.Dispose();
						soundStream = null;
					}

					// Shutdown the player
					currentPlayer.CleanupSound();
					currentPlayer.CleanupPlayer();

					durationInfo = null;
					currentPlayer = null;

					// Clear player information
					StaticModuleInformation = new ModuleInfoStatic();
					PlayingModuleInformation = new ModuleInfoFloating();

					// Tell all visuals to stop
					foreach (IVisualAgent visualAgent in agentManager.GetRegisteredVisualAgent())
						visualAgent.CleanupVisual();
				}
			}
			catch (Exception)
			{
				// Just ignore it
				;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will start playing the music
		/// </summary>
		/********************************************************************/
		public bool StartPlaying(LoaderInfoBase loaderInfo, out string errorMessage, MixerConfiguration newMixerConfiguration)
		{
			try
			{
				if (newMixerConfiguration != null)
					soundStream.ChangeConfiguration(newMixerConfiguration);

				lock (currentPlayer)
				{
					if (!currentPlayer.InitSound(out errorMessage))
						return false;

					// Initialize the module information
					PlayingModuleInformation = new ModuleInfoFloating(0, durationInfo, PlayerHelper.GetModuleInformation(currentPlayer).ToArray());
				}

				soundStream.Start();

				if (outputAgent.SwitchStream(soundStream, loaderInfo.Source, StaticModuleInformation.Title, StaticModuleInformation.Author, out errorMessage) == AgentResult.Error)
					return false;

				StaticModuleInformation.PlayBackSpeakers = soundStream.VisualizerSpeakers;

				// Tell all visuals to start
				foreach (IVisualAgent visualAgent in agentManager.GetRegisteredVisualAgent())
				{
					visualAgent.CleanupVisual();

					if (visualAgent is IChannelChangeVisualAgent)
						continue;

					visualAgent.InitVisual(StaticModuleInformation.Channels, StaticModuleInformation.VirtualChannels, StaticModuleInformation.PlayBackSpeakers);
				}

				outputAgent.Play();
			}
			catch (Exception ex)
			{
				try
				{
					outputAgent.Shutdown();
				}
				catch(Exception)
				{
					// Ignore
				}

				outputAgent = null;

				errorMessage = ex.Message;
				return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Will stop the playing
		/// </summary>
		/********************************************************************/
		public void StopPlaying(bool stopOutputAgent)
		{
			if (currentPlayer != null)
			{
				// Stop the mixer
				if (stopOutputAgent)
					outputAgent?.Stop();

				soundStream.Stop();

				// Cleanup the player
				lock (currentPlayer)
				{
					currentPlayer.CleanupSound();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will pause the playing
		/// </summary>
		/********************************************************************/
		public void PausePlaying()
		{
			if (currentPlayer != null)
			{
				outputAgent.Pause();
				soundStream.Pause();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will resume the playing
		/// </summary>
		/********************************************************************/
		public void ResumePlaying()
		{
			if (currentPlayer != null)
			{
				soundStream.Resume();
				outputAgent.Play();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will change the mixer settings that can be change real-time
		/// </summary>
		/********************************************************************/
		public void ChangeMixerSettings(MixerConfiguration mixerConfiguration)
		{
			soundStream?.ChangeConfiguration(mixerConfiguration);
		}



		/********************************************************************/
		/// <summary>
		/// Return all the static information about the module
		/// </summary>
		/********************************************************************/
		public ModuleInfoStatic StaticModuleInformation
		{
			get; private set;
		}



		/********************************************************************/
		/// <summary>
		/// Return all the information about the module which changes while
		/// playing
		/// </summary>
		/********************************************************************/
		public ModuleInfoFloating PlayingModuleInformation
		{
			get; private set;
		}



		/********************************************************************/
		/// <summary>
		/// Event called for each second the module has played
		/// </summary>
		/********************************************************************/
		public event ClockUpdatedEventHandler ClockUpdated;



		/********************************************************************/
		/// <summary>
		/// Event called when the player reached the end
		/// </summary>
		/********************************************************************/
		public event EventHandler EndReached;



		/********************************************************************/
		/// <summary>
		/// Event called when the player change some of the module information
		/// </summary>
		/********************************************************************/
		public event ModuleInfoChangedEventHandler ModuleInfoChanged;
		#endregion

		#region ISamplePlayer implementation
		/********************************************************************/
		/// <summary>
		/// Will set a new song position
		/// </summary>
		/********************************************************************/
		public void SetSongPosition(int position)
		{
			if (currentPlayer != null)
			{
				lock (currentPlayer)
				{
					if (currentPlayer is IDuration durationPlayer)
						durationPlayer.SetSongPosition(durationInfo?.PositionInfo[position]);

					ModuleInfoChanged[] moduleInfoChanges = currentPlayer.GetChangedInformation();
					if ((moduleInfoChanges != null) && (ModuleInfoChanged != null))
					{
						foreach (ModuleInfoChanged moduleInfoChanged in moduleInfoChanges)
							ModuleInfoChanged(this, new ModuleInfoChangedEventArgs(moduleInfoChanged.Line, moduleInfoChanged.Value));
					}
				}

				soundStream.SongPosition = position;
				PlayingModuleInformation.SongPosition = position;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Event called when the position is changed
		/// </summary>
		/********************************************************************/
		public event EventHandler PositionChanged;
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// Is called every time the clock is updated
		/// </summary>
		/********************************************************************/
		private void Stream_ClockUpdated(object sender, ClockUpdatedEventArgs e)
		{
			if (currentPlayer != null)
			{
				// Call the next event handler
				if (ClockUpdated != null)
					ClockUpdated(sender, e);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called every time the position is changed
		/// </summary>
		/********************************************************************/
		private void Stream_PositionChanged(object sender, EventArgs e)
		{
			if (currentPlayer != null)
			{
				lock (currentPlayer)
				{
					// Update the position
					PlayingModuleInformation.SongPosition = soundStream.SongPosition;
				}

				// Call the next event handler
				if (PositionChanged != null)
					PositionChanged(sender, e);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the player has reached the end
		/// </summary>
		/********************************************************************/
		private void Stream_EndReached(object sender, EventArgs e)
		{
			if (currentPlayer != null)
			{
				// Just call the next event handler
				if (EndReached != null)
					EndReached(sender, e);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the player change some of the module information
		/// </summary>
		/********************************************************************/
		private void Stream_ModuleInfoChanged(object sender, ModuleInfoChangedEventArgs e)
		{
			if (currentPlayer != null)
			{
				// Just call the next event handler
				if (ModuleInfoChanged != null)
					ModuleInfoChanged(sender, e);
			}
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Calculates the duration of all sub-songs
		/// </summary>
		/********************************************************************/
		private void CalculateDuration()
		{
			if (currentPlayer is IDuration durationPlayer)
			{
				DurationInfo[] allSongsInfo = durationPlayer.CalculateDuration();
				if ((allSongsInfo != null) && (allSongsInfo.Length > 0))
				{
					durationInfo = allSongsInfo[0];

					// Initialize the module information
					PlayingModuleInformation = new ModuleInfoFloating(0, durationInfo, null);
				}
			}
		}
		#endregion
	}
}
