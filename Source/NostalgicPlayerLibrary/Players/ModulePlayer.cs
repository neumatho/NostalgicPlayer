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
using Polycode.NostalgicPlayer.NostalgicPlayerKit.Containers;
using Polycode.NostalgicPlayer.NostalgicPlayerKit.Interfaces;
using Polycode.NostalgicPlayer.NostalgicPlayerLibrary.Containers;
using Polycode.NostalgicPlayer.NostalgicPlayerLibrary.Mixer;

namespace Polycode.NostalgicPlayer.NostalgicPlayerLibrary.Players
{
	/// <summary>
	/// Main class to play modules
	/// </summary>
	internal class ModulePlayer : IPlayer
	{
		private IModulePlayerAgent currentPlayer;

		private IOutputAgent outputAgent;
		private MixerStream soundStream;

		private SubSongInfo subSongs;
		private int? songNumber;
		private int songLength;

		private TimeSpan[] positionTimes;

		private List<string> moduleInfo;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModulePlayer()
		{
			// Initialize member variables
			songNumber = null;
			songLength = 0;
			ModuleSize = 0;
			moduleInfo = new List<string>();

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

				// Remember the module length
				ModuleSize = loader.ModuleSize;

				// Initialize other stuff
				ModuleFormat = loader.ModuleFormat;
				PlayerName = loader.PlayerName;

				// Initialize the player
				initOk = currentPlayer.InitPlayer();

				if (initOk)
				{
					// Add all extra files loaded file size to the module length
					ModuleSize += currentPlayer.GetExtraFilesSizes();

					// Fill out the sample list
					GetSamples();

					// Find the name of the module
					ModuleName = currentPlayer.GetModuleName().Trim();

					// Find the author
					Author = FindAuthor();

					// Find the number of channels used
					Channels = currentPlayer.GetModuleChannelCount();

					// Find number of sub-songs
					subSongs = currentPlayer.GetSubSongs();

					// Reset the song length
					songLength = 0;

					// Get the position times for the default song
					TotalTime = currentPlayer.GetPositionTimeTable(subSongs.DefaultStartSong, out positionTimes);

					// Initialize the mixer
					soundStream = new MixerStream();
					initOk = soundStream.Initialize(playerConfiguration, out errorMessage);

					if (!initOk)
						CleanupPlayer();
				}
				else
					CleanupPlayer();
			}
			catch (Exception)
			{
				errorMessage = Properties.Resources.IDS_ERR_PLAYER_INIT;
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

					// Free the sample list
					FreeSamples();
					currentPlayer = null;

					// Clear player information
					songNumber = null;
					songLength = 0;
					ModuleName = string.Empty;
					Author = string.Empty;
					Channels = 0;
					TotalTime = new TimeSpan(0);
					positionTimes = null;

					ModuleFormat = string.Empty;
					PlayerName = string.Empty;
					ModuleSize = 0;
					moduleInfo.Clear();
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
		/// Will select the song you want to play
		/// </summary>
		/********************************************************************/
		public void SelectSong(int song)
		{
			// Remember the song number
			songNumber = song == -1 ? subSongs.DefaultStartSong : song;

			lock (currentPlayer)
			{
				// Initialize the player with the new song
				currentPlayer.InitSound(songNumber.Value);

				// Find the length of the song
				songLength = currentPlayer.GetSongLength();

				// Get the position times for the current song
				TotalTime = currentPlayer.GetPositionTimeTable(subSongs.DefaultStartSong, out positionTimes);

				// Get module information
				moduleInfo.Clear();
				for (int i = 0; currentPlayer.GetInformationString(i, out string description, out string value); i++)
				{
					// Make sure we don't have any invalid characters
					description = description.Replace("\t", " ").Replace("\n", " ").Replace("\r", string.Empty);
					value = value.Replace("\t", " ").Replace("\n", " ").Replace("\r", string.Empty);

					// Build the information in the list
					moduleInfo.Add($"{description}\t{value}");
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will start playing the music
		/// </summary>
		/********************************************************************/
		public void StartPlaying()
		{
			if (!songNumber.HasValue)
				throw new Exception(Properties.Resources.IDS_ERR_SONG_NOT_SET);

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
		/// Returns the name of the module
		/// </summary>
		/********************************************************************/
		public string ModuleName
		{
			get; private set;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the name of the author
		/// </summary>
		/********************************************************************/
		public string Author
		{
			get; private set;
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the module use
		/// </summary>
		/********************************************************************/
		public int Channels
		{
			get; private set;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the total time of the current song
		/// </summary>
		/********************************************************************/
		public TimeSpan TotalTime
		{
			get; private set;
		}



		/********************************************************************/
		/// <summary>
		/// Return the format of the module
		/// </summary>
		/********************************************************************/
		public string ModuleFormat
		{
			get; private set;
		}



		/********************************************************************/
		/// <summary>
		/// Return the name of the player
		/// </summary>
		/********************************************************************/
		public string PlayerName
		{
			get; private set;
		}



		/********************************************************************/
		/// <summary>
		/// Return the size of the module
		/// </summary>
		/********************************************************************/
		public long ModuleSize
		{
			get; private set;
		}



		/********************************************************************/
		/// <summary>
		/// Return the module information list of the current song
		/// </summary>
		/********************************************************************/
		public string[] ModuleInformation
		{
			get
			{
				return moduleInfo.ToArray();
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
			string name = currentPlayer.GetAuthor();

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
