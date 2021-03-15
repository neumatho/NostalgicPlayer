/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;
using Polycode.NostalgicPlayer.PlayerLibrary.Containers;
using Polycode.NostalgicPlayer.PlayerLibrary.Players;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Modules
{
	/// <summary>
	/// This class handles all the loading and playing of modules
	/// </summary>
	public class ModuleHandler
	{
		private class ModuleItem
		{
			public Loader Loader;
		}

		private MainWindowForm mainWindowForm;
		private Manager agentManager;
		private SoundSettings soundSettings;

		private IOutputAgent outputAgent;

		private List<ModuleItem> loadedFiles;

		private volatile bool isPlaying = false;
		private int currentMasterVolume;
		private bool isMuted = false;
		private MixerConfiguration mixerConfiguration;

		/********************************************************************/
		/// <summary>
		/// Event called when the player change position
		/// </summary>
		/********************************************************************/
		public event EventHandler PositionChanged;



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
		/// Initialize and start the module handler thread
		/// </summary>
		/********************************************************************/
		public void Initialize(MainWindowForm mainWindow, Manager manager, SoundSettings sndSettings, int startVolume)
		{
			// Remember the arguments
			mainWindowForm = mainWindow;
			agentManager = manager;
			soundSettings = sndSettings;

			currentMasterVolume = startVolume;

			mixerConfiguration = new MixerConfiguration
			{
				StereoSeparator = sndSettings.StereoSeparation,
				EnableInterpolation = sndSettings.Interpolation,
				EnableAmigaFilter = sndSettings.AmigaFilter,
				ChannelsEnabled = new bool[MixerConfiguration.MaxNumberOfChannels]
			};

			// Initialize the channels enable array
			Array.Fill(mixerConfiguration.ChannelsEnabled, true);

			// Initialize the loader
			loadedFiles = new List<ModuleItem>();
		}



		/********************************************************************/
		/// <summary>
		/// Shutdown and cleanup
		/// </summary>
		/********************************************************************/
		public void Shutdown()
		{
			isPlaying = false;
			loadedFiles = null;

			CloseOutputAgent();

			soundSettings = null;
			agentManager = null;
		}



		/********************************************************************/
		/// <summary>
		/// Close the currently active output agent
		/// </summary>
		/********************************************************************/
		public void CloseOutputAgent()
		{
			outputAgent?.Shutdown();
			outputAgent = null;
			OutputAgentInfo = null;
		}

		#region Properties
		/********************************************************************/
		/// <summary>
		/// Tells if the module has been loaded or not
		/// </summary>
		/********************************************************************/
		public bool IsModuleLoaded
		{
			get
			{
				lock (loadedFiles)
				{
					return loadedFiles.Count > 0;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Tells if the module is playing or not at the moment
		/// </summary>
		/********************************************************************/
		public bool IsPlaying
		{
			get
			{
				return isPlaying;
			}

			set
			{
				isPlaying = value;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the output agent in use if playing
		/// </summary>
		/********************************************************************/
		public AgentInfo OutputAgentInfo
		{
			get; private set;
		}



		/********************************************************************/
		/// <summary>
		/// Return all the static module information
		/// </summary>
		/********************************************************************/
		public ModuleInfoStatic StaticModuleInformation
		{
			get
			{
				return GetActivePlayer()?.StaticModuleInformation ?? new ModuleInfoStatic();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return all the information about the module which changes while
		/// playing
		/// </summary>
		/********************************************************************/
		public ModuleInfoFloating PlayingModuleInformation
		{
			get
			{
				return GetActivePlayer()?.PlayingModuleInformation ?? new ModuleInfoFloating();
			}
		}
		#endregion

		#region Public methods
		/********************************************************************/
		/// <summary>
		/// Will load and play the module at the index given
		/// </summary>
		/********************************************************************/
		public bool LoadAndPlayModule(ModuleListItem listItem, int subSong, int startPos)
		{
			// Start to free all loaded modules
			FreeAllModules();

			// Now load and play the first module
			return LoadAndInitModule(listItem, subSong, startPos);
		}



		/********************************************************************/
		/// <summary>
		/// Will stop and free the playing module if any
		/// </summary>
		/********************************************************************/
		public void StopAndFreeModule()
		{
			lock (loadedFiles)
			{
				// Did we have any modules loaded at all?
				if (IsModuleLoaded)
				{
					// Get the first one
					ModuleItem item = loadedFiles[0];

					IPlayer player = item.Loader.Player;

					// Stop the playing
					player.StopPlaying();

					if (player is IModulePlayer modulePlayer)
					{
						// Unsubscribe to position changes
						modulePlayer.PositionChanged -= Player_PositionChanged;
					}

					// Unsubscribe to event notifications
					player.EndReached -= Player_EndReached;
					player.ModuleInfoChanged -= Player_ModuleInfoChanged;

					IsPlaying = false;

					// Cleanup the player
					player.CleanupPlayer();

					// Tell all visuals to stop
					foreach (IVisualAgent visualAgent in agentManager.GetRegisteredVisualAgent())
						visualAgent.CleanupVisual();

					// Unload the module
					item.Loader.Unload();

					// Remove the file from our list
					loadedFiles.RemoveAt(0);

					// Flush the output agent if required
					if ((outputAgent.SupportFlags & OutputSupportFlag.FlushMe) != 0)
					{
						outputAgent.Shutdown();

						outputAgent = null;
						OutputAgentInfo = null;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will free all loaded modules
		/// </summary>
		/********************************************************************/
		public void FreeAllModules()
		{
			lock (loadedFiles)
			{
				foreach (ModuleItem item in loadedFiles)
				{
					// Cleanup the player
					item.Loader.Player.CleanupPlayer();

					// Unload the module
					item.Loader.Unload();
				}

				// Empty the list
				loadedFiles.Clear();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will start to play the song given
		/// </summary>
		/********************************************************************/
		public void StartSong(int newSong)
		{
			IPlayer player = GetActivePlayer();

			if (player != null)
			{
				// Stop the playing
				player.StopPlaying();

				// Switch song if possible
				if (player is IModulePlayer modulePlayer)
					modulePlayer.SelectSong(newSong);

				lock (loadedFiles)
				{
					// Did we have any modules loaded at all?
					if (IsModuleLoaded)
					{
						// And start playing again
						player.StartPlaying(loadedFiles[0].Loader.FileName, mixerConfiguration);
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will tell the player to change to the position given
		/// </summary>
		/********************************************************************/
		public void SetSongPosition(int newPosition)
		{
			IPlayer player = GetActivePlayer();

			if (player != null)
			{
				if (player is IModulePlayer modulePlayer)
					modulePlayer.SetSongPosition(newPosition);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will remember the mute status
		/// </summary>
		/********************************************************************/
		public void SetMuteStatus(bool muted)
		{
			isMuted = muted;
			SetVolume(currentMasterVolume);
		}



		/********************************************************************/
		/// <summary>
		/// Will tell the mixer to change the volume
		/// </summary>
		/********************************************************************/
		public void SetVolume(int newVolume)
		{
			IPlayer player = GetActivePlayer();

			if (player != null)
				player.SetMasterVolume(isMuted ? 0 : newVolume);

			currentMasterVolume = newVolume;
		}



		/********************************************************************/
		/// <summary>
		/// Will change the mixer settings that can be change real-time
		/// </summary>
		/********************************************************************/
		public void ChangeMixerSettings(MixerConfiguration newMixerConfiguration)
		{
			mixerConfiguration = newMixerConfiguration;

			IPlayer player = GetActivePlayer();

			if (player != null)
				player.ChangeMixerSettings(newMixerConfiguration);
		}



		/********************************************************************/
		/// <summary>
		/// Will pause the player
		/// </summary>
		/********************************************************************/
		public void PausePlaying()
		{
			IPlayer player = GetActivePlayer();

			if (player != null)
			{
				player.PausePlaying();
				IsPlaying = false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will resume the player
		/// </summary>
		/********************************************************************/
		public void ResumePlaying()
		{
			IPlayer player = GetActivePlayer();

			if (player != null)
			{
				player.ResumePlaying();
				IsPlaying = true;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the time on the position given
		/// </summary>
		/********************************************************************/
		public TimeSpan GetPositionTime(int position)
		{
			TimeSpan[] positionTimes = PlayingModuleInformation.PositionTimes;
			return positionTimes == null ? new TimeSpan(0) : positionTimes[position];
		}



		/********************************************************************/
		/// <summary>
		/// Return the enable status for all channels
		/// </summary>
		/********************************************************************/
		public bool[] GetEnabledChannels()
		{
			return mixerConfiguration.ChannelsEnabled;
		}



		/********************************************************************/
		/// <summary>
		/// Set the enable status for a given range of channels
		/// </summary>
		/********************************************************************/
		public void EnableChannels(bool enabled, int startChannel, int stopChannel = -1)
		{
			if (stopChannel == -1)
				mixerConfiguration.ChannelsEnabled[startChannel] = enabled;
			else
			{
				for (int i = startChannel; i <= stopChannel; i++)
					mixerConfiguration.ChannelsEnabled[i] = enabled;
			}
		}
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// Is called every time the player changed position
		/// </summary>
		/********************************************************************/
		private void Player_PositionChanged(object sender, EventArgs e)
		{
			// Just call the next event handler
			if (PositionChanged != null)
				PositionChanged(sender, e);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the player has reached the end
		/// </summary>
		/********************************************************************/
		private void Player_EndReached(object sender, EventArgs e)
		{
			// Just call the next event handler
			if (EndReached != null)
				EndReached(sender, e);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the player change some of the module information
		/// </summary>
		/********************************************************************/
		private void Player_ModuleInfoChanged(object sender, ModuleInfoChangedEventArgs e)
		{
			// Just call the next event handler
			if (ModuleInfoChanged != null)
				ModuleInfoChanged(sender, e);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will show an error message to the user
		/// </summary>
		/********************************************************************/
		private void ShowSimpleErrorMessage(string message)
		{
			mainWindowForm.BeginInvoke(new Action(() =>
			{
				mainWindowForm.ShowSimpleErrorMessage(message);
			}));
		}



		/********************************************************************/
		/// <summary>
		/// Will show an error message to the user with options
		/// </summary>
		/********************************************************************/
		private void ShowErrorMessage(string message, ModuleListItem listItem)
		{
			mainWindowForm.BeginInvoke(new Action(() =>
			{
				mainWindowForm.ShowErrorMessage(message, listItem);
			}));
		}



		/********************************************************************/
		/// <summary>
		/// Will find the configured or default output agent and initialize it
		/// </summary>
		/********************************************************************/
		private bool FindOutputAgent(bool showError)
		{
			if (outputAgent != null)
			{
				// Check if the current allocated is the same as in the settings
				if (!OutputAgentInfo.Enabled || (OutputAgentInfo.TypeId != soundSettings.OutputAgent))
				{
					// They are different, so stop the current output agent
					CloseOutputAgent();
				}
			}

			if (outputAgent == null)
			{
				Guid typeId = soundSettings.OutputAgent;

				AgentInfo agentInfo = agentManager.GetAgent(Manager.AgentType.Output, typeId);
				if ((agentInfo == null) || !agentInfo.Enabled)
				{
					// Selected output agent could not be loaded, try with the default one if not already that one
					if (typeId != soundSettings.DefaultOutputAgent)
						agentInfo = agentManager.GetAgent(Manager.AgentType.Output, soundSettings.DefaultOutputAgent);
				}

				if ((agentInfo?.Agent != null) && agentInfo.Enabled)
				{
					IOutputAgent agent = (IOutputAgent)agentInfo.Agent.CreateInstance(agentInfo.TypeId);

					if (agent.Initialize(out string errorMessage) == AgentResult.Error)
					{
						if (showError)
							ShowSimpleErrorMessage(string.Format(Resources.IDS_ERR_INITOUTPUTAGENT, errorMessage));

						return false;
					}

					OutputAgentInfo = agentInfo;
					outputAgent = agent;
				}
				else
				{
					if (showError)
						ShowSimpleErrorMessage(Resources.IDS_ERR_NOOUTPUTAGENT);

					return false;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load and/or initialize module
		/// </summary>
		/********************************************************************/
		private bool LoadAndInitModule(ModuleListItem listItem, int? subSong = null, int? startPos = null, bool showError = true)
		{
			// Should we load a module?
			if (listItem != null)
			{
				try
				{
					ModuleItem item = new ModuleItem();

					// Get the file name
					string fileName = listItem.ShortText;

					// Create new loader
					item.Loader = new Loader(agentManager);

					// Load the module
					if (!item.Loader.Load(fileName, listItem.GetLoader(), out string errorMessage))
					{
						if (showError)
							ShowErrorMessage(string.Format(Resources.IDS_ERR_LOAD_FILE, errorMessage), listItem);

						return false;
					}

					// Find the output agent to use
					if (!FindOutputAgent(showError))
					{
						item.Loader.Unload();
						return false;
					}

					// Setup player settings
					PlayerConfiguration playerConfig = new PlayerConfiguration(outputAgent, item.Loader, mixerConfiguration);

					// Initialize the module
					if (!item.Loader.Player.InitPlayer(playerConfig, out errorMessage))
					{
						if (showError)
							ShowErrorMessage(string.Format(Resources.IDS_ERR_INIT_PLAYER, errorMessage), listItem);

						item.Loader.Unload();
						return false;
					}

					lock (loadedFiles)
					{
						// Add the new module to the list
						loadedFiles.Add(item);
					}
				}
				catch (Exception ex)
				{
					if (showError)
						ShowErrorMessage(string.Format(Resources.IDS_ERR_LOAD_FILE, ex.Message), listItem);

					return false;
				}
			}

			// Should we start playing the module?
			if (subSong.HasValue)
			{
				if (!startPos.HasValue)
					startPos = -1;

				// Start to play the module
				if (!StartPlaying(subSong.Value, startPos.Value))
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Will start to play the song number given
		/// </summary>
		/********************************************************************/
		private bool StartPlaying(int subSong, int startPos)
		{
			IPlayer player = GetActivePlayer();

			// Is there any modules loaded?
			if (player != null)
			{
				// Change the volume
				SetVolume(currentMasterVolume);

				// Initialize the module
				if (player is IModulePlayer modulePlayer)
				{
					modulePlayer.SelectSong(subSong);

					if (startPos != -1)
						modulePlayer.SetSongPosition(startPos);

					// Subscribe to position changes
					modulePlayer.PositionChanged += Player_PositionChanged;
				}

				// Subscribe to event notifications
				player.EndReached += Player_EndReached;
				player.ModuleInfoChanged += Player_ModuleInfoChanged;

				// Tell all visuals to start
				foreach (IVisualAgent visualAgent in agentManager.GetRegisteredVisualAgent())
					visualAgent.InitVisual(StaticModuleInformation.Channels);

				lock (loadedFiles)
				{
					// Did we have any modules loaded at all?
					if (IsModuleLoaded)
					{
						// Start playing the song
						if (!player.StartPlaying(loadedFiles[0].Loader.FileName, mixerConfiguration))
							return false;
					}
				}

				// The module is playing
				IsPlaying = true;

				return true;
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Return current active player or null
		/// </summary>
		/********************************************************************/
		private IPlayer GetActivePlayer()
		{
			lock (loadedFiles)
			{
				return IsModuleLoaded ? loadedFiles[0].Loader.Player : null;
			}
		}
		#endregion
	}
}
