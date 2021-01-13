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
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.PlayerLibrary.Containers;
using Polycode.NostalgicPlayer.PlayerLibrary.Mixer;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Players
{
	/// <summary>
	/// Main class to play modules
	/// </summary>
	internal class ModulePlayer : IModulePlayer
	{
		private IModulePlayerAgent currentPlayer;

		private IOutputAgent outputAgent;
		private MixerStream soundStream;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModulePlayer()
		{
			// Initialize member variables
			StaticModuleInformation = new ModuleInfoStatic();
			PlayingModuleInformation = new ModuleInfoFloating();

			currentPlayer = null;
		}

		#region IPlayer implementation
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

				Loader loader = playerConfiguration.Loader;

				// Remember the player
				currentPlayer = (IModulePlayerAgent)loader.AgentPlayer;

				lock (currentPlayer)
				{
					// Initialize the player
					initOk = currentPlayer.InitPlayer();

					if (initOk)
					{
						// Subscribe the events
						currentPlayer.PositionChanged += Player_PositionChanged;
						currentPlayer.EndReached += Player_EndReached;

						// Initialize module information
						StaticModuleInformation = new ModuleInfoStatic(currentPlayer.ModuleName.Trim(), FindAuthor(), loader.ModuleFormat, loader.PlayerName, currentPlayer.ModuleChannelCount, loader.ModuleSize + currentPlayer.ExtraFilesSizes, currentPlayer.SupportFlags, currentPlayer.SubSongs.Number);

						// Fill out the sample list
						GetSamples();

						// Initialize the mixer
						soundStream = new MixerStream();
						initOk = soundStream.Initialize(playerConfiguration, out errorMessage);

						if (!initOk)
							CleanupPlayer();
					}
					else
						CleanupPlayer();
				}
			}
			catch (Exception)
			{
				errorMessage = Resources.IDS_ERR_PLAYER_INIT;
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
					soundStream?.Cleanup();
					soundStream?.Dispose();
					soundStream = null;

					// Shutdown the player
					currentPlayer.CleanupPlayer();

					// Unsubscribe the events
					currentPlayer.PositionChanged -= Player_PositionChanged;

					// Free the sample list
					FreeSamples();
					currentPlayer = null;

					// Clear player information
					StaticModuleInformation = new ModuleInfoStatic();
					PlayingModuleInformation = new ModuleInfoFloating();
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
		public void StartPlaying(MixerConfiguration newMixerConfiguration)
		{
			if (newMixerConfiguration != null)
				soundStream.ChangeConfiguration(newMixerConfiguration);

			soundStream.Start();

			outputAgent.SwitchStream(soundStream);
			outputAgent.Play();
		}



		/********************************************************************/
		/// <summary>
		/// Will stop the playing
		/// </summary>
		/********************************************************************/
		public void StopPlaying()
		{
			if (currentPlayer != null)
			{
				// Stop the mixer
				outputAgent.Stop();
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
				outputAgent.Pause();
		}



		/********************************************************************/
		/// <summary>
		/// Will resume the playing
		/// </summary>
		/********************************************************************/
		public void ResumePlaying()
		{
			if (currentPlayer != null)
				outputAgent.Play();
		}



		/********************************************************************/
		/// <summary>
		/// Will set the master volume
		/// </summary>
		/********************************************************************/
		public void SetMasterVolume(int volume)
		{
			soundStream?.SetMasterVolume(volume);
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
		/// Event called when the player reached the end
		/// </summary>
		/********************************************************************/
		public event EventHandler EndReached;
		#endregion

		#region IModulePlayer implementation
		/********************************************************************/
		/// <summary>
		/// Will select the song you want to play
		/// </summary>
		/********************************************************************/
		public void SelectSong(int songNumber)
		{
			if (currentPlayer != null)
			{
				lock (currentPlayer)
				{
					// Get sub-song information
					SubSongInfo subSongs = currentPlayer.SubSongs;

					// Find the right song number
					int songNum = songNumber == -1 ? subSongs.DefaultStartSong : songNumber;

					// Initialize the player with the new song
					currentPlayer.InitSound(songNum);

					// Find the length of the song
					int songLength = currentPlayer.SongLength;

					// Get the position times for the current song
					TimeSpan totalTime = currentPlayer.GetPositionTimeTable(subSongs.DefaultStartSong, out TimeSpan[] positionTimes);

					// Get module information
					List<string> moduleInfo = new List<string>();
					for (int i = 0; currentPlayer.GetInformationString(i, out string description, out string value); i++)
					{
						// Make sure we don't have any invalid characters
						description = description.Replace("\t", " ").Replace("\n", " ").Replace("\r", string.Empty);
						value = value.Replace("\t", " ").Replace("\n", " ").Replace("\r", string.Empty);

						// Build the information in the list
						moduleInfo.Add($"{description}\t{value}");
					}

					// Initialize the module information
					PlayingModuleInformation = new ModuleInfoFloating(songNum, totalTime, songLength, positionTimes, moduleInfo.ToArray());
				}
			}
		}



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
					currentPlayer.SongPosition = position;
				}

				PlayingModuleInformation.SongPosition = position;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Event called when the player change position
		/// </summary>
		/********************************************************************/
		public event EventHandler PositionChanged;
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// Is called every time the player changed position
		/// </summary>
		/********************************************************************/
		private void Player_PositionChanged(object sender, EventArgs e)
		{
			if (currentPlayer != null)
			{
				lock (currentPlayer)
				{
					// Update the position
					PlayingModuleInformation.SongPosition = currentPlayer.SongPosition;
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
		private void Player_EndReached(object sender, EventArgs e)
		{
			if (currentPlayer != null)
			{
				// Just call the next event handler
				if (EndReached != null)
					EndReached(sender, e);
			}
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Return the author of the module
		/// </summary>
		/********************************************************************/
		private string FindAuthor()
		{
			string name = currentPlayer.Author;

			if (string.IsNullOrWhiteSpace(name))
			{
				// We didn't get any author, now scan the instruments/samples
				// after an author
			}

			// Trim and return the name
			return name?.Trim();
		}



		/********************************************************************/
		/// <summary>
		/// Will find all the samples and fill out the sample list
		/// </summary>
		/********************************************************************/
		private void GetSamples()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will free all the items in the sample list
		/// </summary>
		/********************************************************************/
		private void FreeSamples()
		{
		}
		#endregion
	}
}
