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
using System.IO;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;
using Polycode.NostalgicPlayer.NostalgicPlayer.MainWindow;
using Polycode.NostalgicPlayer.NostalgicPlayerKit.Containers;
using Polycode.NostalgicPlayer.NostalgicPlayerKit.Interfaces;
using Polycode.NostalgicPlayer.NostalgicPlayerKit.Streams;
using Polycode.NostalgicPlayer.NostalgicPlayerKit.Utility;
using Polycode.NostalgicPlayer.NostalgicPlayerLibrary.Agent;
using Polycode.NostalgicPlayer.NostalgicPlayerLibrary.Containers;
using Polycode.NostalgicPlayer.NostalgicPlayerLibrary.Players;

namespace Polycode.NostalgicPlayer.NostalgicPlayer.Modules
{
	/// <summary>
	/// This class handles all the loading and playing of modules
	/// </summary>
	public class ModuleHandler
	{
		private class ModuleItem
		{
			public ModuleListItem ListItem;
			public Loader Loader;
		}

		private MainWindowForm mainWindowForm;
		private Manager agentManager;
		private Settings settings;

		private IOutputAgent outputAgent;

		private List<ModuleItem> loadedFiles;

		private volatile bool isPlaying = false;

		/********************************************************************/
		/// <summary>
		/// Initialize and start the module handler thread
		/// </summary>
		/********************************************************************/
		public void Initialize(MainWindowForm mainWindow, Manager manager, Settings userSettings)
		{
			// Remember the arguments
			mainWindowForm = mainWindow;
			agentManager = manager;
			settings = userSettings;

			// Initialize the loader
			loadedFiles = new List<ModuleItem>();

			// Find the output agent to use
			FindOutputAgent();
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

			// Close down the output agent
			outputAgent?.Shutdown();
			outputAgent = null;

			settings = null;
			agentManager = null;
		}

		#region Properties
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
		public void LoadAndPlayModule(ModuleListItem listItem, int subSong = -1, int startPos = -1)
		{
			// Start to free all loaded modules
			FreeAllModules();

			// Now load and play the first module
			LoadAndInitModule(listItem, subSong, startPos);
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
				if (loadedFiles.Count > 0)
				{
					// Get the first one
					ModuleItem item = loadedFiles[0];

					if (IsPlaying)
					{
						// Stop the playing
						item.Loader.Player.StopPlaying();
						IsPlaying = false;
					}

					// Cleanup the player
					item.Loader.Player.CleanupPlayer();

					// Unload the module
					item.Loader.Unload();

					// Remove the file from our list
					loadedFiles.RemoveAt(0);
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
		/// Return the time on the position given
		/// </summary>
		/********************************************************************/
		public TimeSpan GetPositionTime(int position)
		{
			TimeSpan[] positionTimes = PlayingModuleInformation.PositionTimes;
			return positionTimes == null ? new TimeSpan(0) : positionTimes[position];
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
			mainWindowForm.Invoke(new Action(() =>
			{
				KryptonMessageBox.Show(message, Properties.Resources.IDS_MAIN_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}));
		}



		/********************************************************************/
		/// <summary>
		/// Will show an error message to the user with options
		/// </summary>
		/********************************************************************/
		private void ShowErrorMessage(string message)//XX
		{
			mainWindowForm.Invoke(new Action(() =>
			{
				KryptonMessageBox.Show(message, Properties.Resources.IDS_MAIN_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}));
		}



		/********************************************************************/
		/// <summary>
		/// Will find the configured or default output agent and initialize it
		/// </summary>
		/********************************************************************/
		private void FindOutputAgent()
		{
			string defaultAgentId = "b9cef7e4-c74c-4af0-b01d-802f0d1b4cc7";		// This is the ID of the CoreAudio output agent

			string agentId = settings.GetStringEntry("Mixer", "OutputAgent", defaultAgentId);
			IAgent agent = agentManager.GetAgent(Manager.AgentType.Output, new Guid(agentId));
			if (agent == null)
			{
				// Selected output agent could not be loaded, try with the default one if not already that one
				if (agentId != defaultAgentId)
				{
					agent = agentManager.GetAgent(Manager.AgentType.Output, new Guid(agentId));
					if (agent == null)
						ShowSimpleErrorMessage(Properties.Resources.IDS_ERR_NOOUTPUTAGENT);
				}
			}

			if (agent != null)
			{
				outputAgent = (IOutputAgent)agent.CreateInstance();
				if (outputAgent.Initialize(out string errorMessage) == AgentResult.Error)
					ShowSimpleErrorMessage(string.Format(Properties.Resources.IDS_ERR_INITOUTPUTAGENT, errorMessage));
			}
		}



		/********************************************************************/
		/// <summary>
		/// Load and/or initialize module
		/// </summary>
		/********************************************************************/
		private void LoadAndInitModule(ModuleListItem listItem, int? subSong = null, int? startPos = null, bool showError = true)
		{
			// Should we load a module?
			if (listItem != null)
			{
				string errorMessage;

				ModuleItem item = new ModuleItem();
				item.ListItem = listItem;

				// Get the file name
				string fileName = listItem.ShortText;

				// Create new loader
				item.Loader = new Loader(agentManager);

				// Load the module
				if (!item.Loader.Load(new PlayerFileInfo(fileName, listItem.OpenFile()), out errorMessage))
				{
					if (showError)
						ShowErrorMessage(string.Format(Properties.Resources.IDS_ERR_LOAD_FILE, errorMessage));

					return;
				}

				// Setup mixer settings
				MixerConfiguration mixerConfig = new MixerConfiguration();
				mixerConfig.EnableAmigaFilter = true;
				mixerConfig.StereoSeparator = 100;

				// Setup player settings
				PlayerConfiguration playerConfig = new PlayerConfiguration(outputAgent, item.Loader, mixerConfig);

				// Initialize the module
				if (!item.Loader.Player.InitPlayer(playerConfig, out errorMessage))
				{
					if (showError)
						ShowErrorMessage(string.Format(Properties.Resources.IDS_ERR_INIT_PLAYER, errorMessage));

					item.Loader.Unload();
					return;
				}

				lock (loadedFiles)
				{
					// Add the new module to the list
					loadedFiles.Add(item);
				}
			}

			// Should we start playing the module?
			if (subSong.HasValue)
			{
				if (!startPos.HasValue)
					startPos = -1;

				// Start to play the module
				StartPlaying(subSong.Value, startPos.Value);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will start to play the song number given
		/// </summary>
		/********************************************************************/
		private void StartPlaying(int subSong, int startPos)
		{
			ModuleItem modItem;

			lock (loadedFiles)
			{
				modItem = loadedFiles.Count > 0 ? loadedFiles[0] : null;
			}

			// Is there any modules loaded?
			if (modItem != null)
			{
				// Change the volume//XX
				;

				IPlayer player = modItem.Loader.Player;

				// Initialize the module
				if (player is IModulePlayer modulePlayer)
				{
					modulePlayer.SelectSong(subSong);

//XX					if (startPos != -1)
//						modulePlayer.SongPosition = startPos;	// Skal først bruges ved load og start ved opstart
				}

				// The module is playing
				IsPlaying = true;

				// Start playing the song
				player.StartPlaying();
			}
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
				return loadedFiles.Count > 0 ? loadedFiles[0].Loader.Player : null;
			}
		}
		#endregion
	}
}
