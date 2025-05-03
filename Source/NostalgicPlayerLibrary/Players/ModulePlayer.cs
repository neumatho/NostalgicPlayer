/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Linq;
using System.Threading;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Events;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;
using Polycode.NostalgicPlayer.PlayerLibrary.Containers;
using Polycode.NostalgicPlayer.PlayerLibrary.Loaders;
using Polycode.NostalgicPlayer.PlayerLibrary.Sound.Mixer;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Players
{
	/// <summary>
	/// Main class to play modules
	/// </summary>
	internal class ModulePlayer : IModulePlayer
	{
		private readonly Manager agentManager;

		private readonly Lock playerLock = new Lock();
		private IModulePlayerAgent currentPlayer;
		private DurationInfo[] allSongsInfo;

		private IOutputAgent outputAgent;
		private MixerStream soundStream;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModulePlayer(Manager manager)
		{
			agentManager = manager;

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
		public string GetWarning(Loader loader)
		{
			lock (playerLock)
			{
				IModulePlayerAgent player = (IModulePlayerAgent)loader.PlayerAgent;

				lock (player)
				{
					return player.GetWarning();
				}
			}
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
				lock (playerLock)
				{
					outputAgent = playerConfiguration.OutputAgent;

					Loader loader = playerConfiguration.Loader;

					// Remember the player
					currentPlayer = (IModulePlayerAgent)loader.PlayerAgent;

					lock (currentPlayer)
					{
						// Initialize the player
						initOk = currentPlayer.InitPlayer(out errorMessage);

						if (initOk)
						{
							// Calculate the duration of all sub-songs
							CalculateDuration();

							// Subscribe the events
							currentPlayer.SubSongChanged += Player_SubSongChanged;

							// Initialize module information
							StaticModuleInformation = new ModuleInfoStatic(loader, currentPlayer);

							// Initialize the mixer
							soundStream = new MixerStream();

							// Subscribe the events
							soundStream.ClockUpdated += Stream_ClockUpdated;
							soundStream.PositionChanged += Stream_PositionChanged;
							soundStream.EndReached += Stream_EndReached;
							soundStream.ModuleInfoChanged += Stream_ModuleInfoChanged;

							initOk = soundStream.Initialize(agentManager, playerConfiguration, out errorMessage);

							if (!initOk)
								CleanupPlayer();
						}
						else
							CleanupPlayer();
					}
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
				lock (playerLock)
				{
					if (currentPlayer != null)
					{
						// End the mixer
						if (soundStream != null)
						{
							soundStream.ModuleInfoChanged -= Stream_ModuleInfoChanged;
							soundStream.EndReached -= Stream_EndReached;
							soundStream.PositionChanged -= Stream_PositionChanged;
							soundStream.ClockUpdated -= Stream_ClockUpdated;

							soundStream.Cleanup();
							soundStream.Dispose();
							soundStream = null;
						}

						// Shutdown the player
						currentPlayer.CleanupSound();
						currentPlayer.CleanupPlayer();

						// Unsubscribe the events
						currentPlayer.SubSongChanged -= Player_SubSongChanged;

						allSongsInfo = null;
						currentPlayer = null;

						// Clear player information
						StaticModuleInformation = new ModuleInfoStatic();
						PlayingModuleInformation = new ModuleInfoFloating();

						// Tell all visuals to stop
						foreach (IVisualAgent visualAgent in agentManager.GetRegisteredVisualAgent())
							visualAgent.CleanupVisual();
					}
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
		public bool StartPlaying(Loader loader, out string errorMessage, MixerConfiguration newMixerConfiguration)
		{
			lock (playerLock)
			{
				try
				{
					if (newMixerConfiguration != null)
						soundStream.ChangeConfiguration(newMixerConfiguration);

					soundStream.Start();

					if (outputAgent.SwitchStream(soundStream, loader.FileName, StaticModuleInformation.ModuleName, StaticModuleInformation.Author, out errorMessage) == AgentResult.Error)
						return false;

					StaticModuleInformation.PlayBackSpeakers = soundStream.VisualizerSpeakers;

					// Build note frequency table
					uint[][] noteFrequencies = null;
					if (StaticModuleInformation.Samples != null)
					{
						noteFrequencies = new uint[StaticModuleInformation.Samples.Length][];
						for (int i = 0; i < noteFrequencies.Length; i++)
							noteFrequencies[i] = StaticModuleInformation.Samples[i].NoteFrequencies;
					}

					// Tell all visuals to start
					bool channelChangeEnabled = ((currentPlayer.SupportFlags & ModulePlayerSupportFlag.BufferMode) == 0) || ((currentPlayer.SupportFlags & (ModulePlayerSupportFlag.BufferMode | ModulePlayerSupportFlag.Visualize)) == (ModulePlayerSupportFlag.BufferMode | ModulePlayerSupportFlag.Visualize));

					foreach (IVisualAgent visualAgent in agentManager.GetRegisteredVisualAgent())
					{
						visualAgent.CleanupVisual();

						if (!channelChangeEnabled && (visualAgent is IChannelChangeVisualAgent))
							continue;

						visualAgent.InitVisual(StaticModuleInformation.Channels, StaticModuleInformation.VirtualChannels, StaticModuleInformation.PlayBackSpeakers);

						if (visualAgent is IChannelChangeVisualAgent channelChangeVisualAgent)
							channelChangeVisualAgent.SetNoteFrequencies(noteFrequencies);
					}

					outputAgent.Play();
				}
				catch (Exception ex)
				{
					errorMessage = ex.Message;
					return false;
				}

				return true;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will stop the playing
		/// </summary>
		/********************************************************************/
		public void StopPlaying(bool stopOutputAgent)
		{
			lock (playerLock)
			{
				if (currentPlayer != null)
				{
					// Stop the mixer
					if (stopOutputAgent)
						outputAgent.Stop();

					soundStream.Stop();

					// Cleanup the player
					lock (currentPlayer)
					{
						currentPlayer.CleanupSound();
					}
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
			lock (playerLock)
			{
				if (currentPlayer != null)
				{
					outputAgent.Pause();
					soundStream.Pause();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will resume the playing
		/// </summary>
		/********************************************************************/
		public void ResumePlaying()
		{
			lock (playerLock)
			{
				if (currentPlayer != null)
				{
					soundStream.Resume();
					outputAgent.Play();
				}
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



		/********************************************************************/
		/// <summary>
		/// Event called when the player change sub-song
		/// </summary>
		/********************************************************************/
		public event SubSongChangedEventHandler SubSongChanged;
		#endregion

		#region IModulePlayer implementation
		/********************************************************************/
		/// <summary>
		/// Will select the song you want to play. If songNumber is -1, the
		/// default song will be selected
		/// </summary>
		/********************************************************************/
		public bool SelectSong(int songNumber, out string errorMessage)
		{
			errorMessage = string.Empty;

			lock (playerLock)
			{
				if (currentPlayer != null)
				{
					lock (currentPlayer)
					{
						try
						{
							// Get sub-song information
							SubSongInfo subSongs = currentPlayer.SubSongs;

							// Find the right song number
							int songNum = songNumber == -1 ? subSongs.DefaultStartSong : songNumber;

							// Initialize the player with the new song
							if (!currentPlayer.InitSound(songNum, out errorMessage))
							{
								CleanupPlayer();
								return false;
							}

							if (currentPlayer is IModuleDurationPlayer moduleDurationPlayer)
							{
								if (!moduleDurationPlayer.SetSubSong(songNum, out errorMessage))
								{
									CleanupPlayer();
									return false;
								}
							}

							// Initialize the module information
							PlayingModuleInformation = new ModuleInfoFloating(songNum, allSongsInfo?[songNum], PlayerHelper.GetModuleInformation(currentPlayer).ToArray());
						}
						catch (Exception ex)
						{
							CleanupPlayer();

							errorMessage = string.Format(Resources.IDS_ERR_PLAYER_SELECTSONG, ex.Message);
							return false;
						}
					}
				}

				return true;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will set a new song position
		/// </summary>
		/********************************************************************/
		public void SetSongPosition(int position)
		{
			lock (playerLock)
			{
				if (currentPlayer != null)
				{
					lock (currentPlayer)
					{
						if (currentPlayer is IDurationPlayer durationPlayer)
							durationPlayer.SetSongPosition(PlayingModuleInformation.DurationInfo?.PositionInfo[position]);

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
		/// Is called when the player change the sub-song
		/// </summary>
		/********************************************************************/
		private void Player_SubSongChanged(object sender, SubSongChangedEventArgs e)
		{
			if (currentPlayer != null)
			{
				// Update the sub-song
				PlayingModuleInformation.SetCurrentSong(e.SubSong, e.DurationInfo);

				// Just call the next event handler
				if (SubSongChanged != null)
					SubSongChanged(sender, e);
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
				// Update the position
				PlayingModuleInformation.SongPosition = soundStream.SongPosition;

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
			if (currentPlayer is IDurationPlayer durationPlayer)
			{
				SpeakerFlag speakerFlags = currentPlayer.SpeakerFlags;
				int mixerChannelCount = Enum.GetValues<SpeakerFlag>().Count(flag => (speakerFlags & flag) != 0);	// Count number of bits set
				int virtualChannelCount = (currentPlayer.SupportFlags & ModulePlayerSupportFlag.BufferDirect) != 0 ? mixerChannelCount : currentPlayer.VirtualChannelCount;

				currentPlayer.VirtualChannels = new IChannel[virtualChannelCount];

				for (int i = 0; i < currentPlayer.VirtualChannels.Length; i++)
					currentPlayer.VirtualChannels[i] = new DummyChannel();

				allSongsInfo = durationPlayer.CalculateDuration();
				if ((allSongsInfo != null) && (allSongsInfo.Length == 0))
					allSongsInfo = null;
			}
		}
		#endregion
	}
}
