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
using System.Linq;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;
using Polycode.NostalgicPlayer.PlayerLibrary.Containers;
using Polycode.NostalgicPlayer.PlayerLibrary.Loaders;
using Polycode.NostalgicPlayer.PlayerLibrary.Mixer;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Players
{
	/// <summary>
	/// Main class to play modules
	/// </summary>
	internal class ModulePlayer : IModulePlayer
	{
		private readonly Manager agentManager;

		private object playerLock = new object();
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
							currentPlayer.PositionChanged += Player_PositionChanged;
							currentPlayer.ModuleInfoChanged += Player_ModuleInfoChanged;

							// Initialize module information
							StaticModuleInformation = new ModuleInfoStatic(loader.PlayerAgentInfo, loader.ConverterAgentInfo, currentPlayer.ModuleName.Trim(), FindAuthor(), currentPlayer.Comment, currentPlayer.CommentFont, currentPlayer.Lyrics, currentPlayer.LyricsFont, loader.ModuleFormat, loader.PlayerName, currentPlayer.ModuleChannelCount, loader.CrunchedSize, loader.ModuleSize, currentPlayer.SupportFlags, currentPlayer.SubSongs.Number, currentPlayer.Instruments, currentPlayer.Samples);

							// Initialize the mixer
							soundStream = new MixerStream();
							soundStream.EndReached += Stream_EndReached;

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
							soundStream.EndReached -= Stream_EndReached;

							soundStream.Cleanup();
							soundStream.Dispose();
							soundStream = null;
						}

						// Shutdown the player
						currentPlayer.CleanupSound();
						currentPlayer.CleanupPlayer();

						// Unsubscribe the events
						currentPlayer.PositionChanged -= Player_PositionChanged;
						currentPlayer.ModuleInfoChanged -= Player_ModuleInfoChanged;

						allSongsInfo = null;
						currentPlayer = null;

						// Clear player information
						StaticModuleInformation = new ModuleInfoStatic();
						PlayingModuleInformation = new ModuleInfoFloating();
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
				if (newMixerConfiguration != null)
					soundStream.ChangeConfiguration(newMixerConfiguration);

				soundStream.Start();

				if (outputAgent.SwitchStream(soundStream, loader.FileName, StaticModuleInformation.ModuleName, StaticModuleInformation.Author, out errorMessage) == AgentResult.Error)
					return false;

				// Tell all visuals to start
				bool bufferMode = (currentPlayer.SupportFlags & ModulePlayerSupportFlag.BufferMode) != 0;

				foreach (IVisualAgent visualAgent in agentManager.GetRegisteredVisualAgent())
				{
					visualAgent.CleanupVisual();

					if (bufferMode && (visualAgent is IChannelChangeVisualAgent))
						continue;

					visualAgent.InitVisual(StaticModuleInformation.Channels);
				}

				outputAgent.Play();

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

				if (stopOutputAgent)
				{
					// Tell all visuals to stop
					foreach (IVisualAgent visualAgent in agentManager.GetRegisteredVisualAgent())
						visualAgent.CleanupVisual();
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
		/// Will set the master volume
		/// </summary>
		/********************************************************************/
		public void SetMasterVolume(int volume)
		{
			soundStream?.SetMasterVolume(volume);
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

		#region IModulePlayer implementation
		/********************************************************************/
		/// <summary>
		/// Will select the song you want to play
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

							// Get the position times for the current song
							DurationInfo durationInfo = allSongsInfo?[songNum];

							// Initialize the player with the new song
							if (!currentPlayer.InitSound(songNum, durationInfo, out errorMessage))
							{
								CleanupPlayer();
								return false;
							}

							// Find the length of the song
							int songLength = currentPlayer.SongLength;

							// Initialize the module information
							PlayingModuleInformation = new ModuleInfoFloating(songNum, durationInfo, currentPlayer.GetSongPosition(), songLength, PlayerHelper.GetModuleInformation(currentPlayer).ToArray());
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
						currentPlayer.SetSongPosition(position, PlayingModuleInformation.DurationInfo?.PositionInfo[position]);
					}

					PlayingModuleInformation.SongPosition = position;
				}
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
					PlayingModuleInformation.SongPosition = currentPlayer.GetSongPosition();
				}

				// Call the next event handler
				if (PositionChanged != null)
					PositionChanged(sender, e);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the player change some of the module information
		/// </summary>
		/********************************************************************/
		private void Player_ModuleInfoChanged(object sender, ModuleInfoChangedEventArgs e)
		{
			if (currentPlayer != null)
			{
				// Just call the next event handler
				if (ModuleInfoChanged != null)
					ModuleInfoChanged(sender, e);
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
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Calculates the duration of all sub-songs
		/// </summary>
		/********************************************************************/
		private void CalculateDuration()
		{
			currentPlayer.VirtualChannels = new IChannel[currentPlayer.VirtualChannelCount];

			for (int i = 0; i < currentPlayer.VirtualChannels.Length; i++)
				currentPlayer.VirtualChannels[i] = new DummyChannel();

			allSongsInfo = currentPlayer.CalculateDuration();
		}



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
				var nameList = currentPlayer.Instruments?.Select(instInfo => instInfo.Name);
				if (nameList != null)
					name = FindAuthorInList(nameList.ToList());

				if (string.IsNullOrEmpty(name))
				{
					// No author found in the instrument names, now try the samples
					nameList = currentPlayer.Samples?.Select(sampInfo => sampInfo.Name);
					if (nameList != null)
						name = FindAuthorInList(nameList.ToList());
				}
			}

			// Trim and return the name
			return name?.Trim();
		}



		/********************************************************************/
		/// <summary>
		/// Tries to find the author in a list of names
		/// </summary>
		/********************************************************************/
		private string FindAuthorInList(List<string> list)
		{
			int i, pos = -1, startPos = -1;
			string itemStr = string.Empty;
			string name = string.Empty;

			// First get the number of items in the list
			int count = list.Count;

			// Traverse all the names after the hash mark
			for (i = 0; i < count; i++)
			{
				// Get the string to search in
				itemStr = list[i];

				pos = itemStr.IndexOf('#');
				if (pos != -1)
				{
					if ((itemStr.Length >= (pos + 5)) && (itemStr.Substring(pos + 1, 4).ToLower() == "from"))
						startPos = pos + 5;
					else
					{
						startPos = pos + 1;

						// See if there is a "by" word after the mark
						pos = FindBy(itemStr.Substring(startPos));
						if (pos != -1)
							startPos = pos;
					}

					// Try to find the beginning of the author in the current string
					for (pos = startPos; pos < itemStr.Length; pos++)
					{
						if (char.IsLetterOrDigit(itemStr[pos]))
						{
							startPos = pos;
							break;
						}
					}

					// Find the author
					name = ClipOutAuthor(itemStr, startPos);

					// If the found name starts with a digit, ignore it.
					// Also ignore other common names, we know is not the author
					if (string.IsNullOrEmpty(name) || (char.IsDigit(name[0]) && !name.StartsWith("4-mat")) || name.ToLower().StartsWith("trax"))
					{
						startPos = -1;
						name = string.Empty;
					}
					break;
				}
			}

			if (startPos == -1)
			{
				// Traverse all the names
				for (i = 0; i < count; i++)
				{
					// Get the string to search in
					itemStr = list[i];

					// If the string is empty, we don't need to do a search :-)
					if (string.IsNullOrWhiteSpace(itemStr))
						continue;

					// Try to find a "by" word
					pos = FindBy(itemStr);
					if (pos != -1)
						break;
				}

				if (pos != -1)
				{
					// Now try to find the author, search through the current
					// string to the end of the list
					for (;;)
					{
						// Scan each character in the rest of the string
						for (; pos < itemStr.Length; pos++)
						{
							if (char.IsLetterOrDigit(itemStr[pos]))
							{
								startPos = pos;
								break;
							}
						}

						// Get a start position, break the loop
						if (startPos != -1)
							break;

						// Get next line
						i++;
						if (i == count)
							break;

						itemStr = list[i];
						pos = 0;
					}
				}
				else
				{
					// We didn't find a "by" word, try to find other author marks
					for (i = 0; i < count; i++)
					{
						// Get the string to search in
						itemStr = list[i];

						// If the string is empty, we don't need to do a search :-)
						if (string.IsNullOrWhiteSpace(itemStr))
							continue;

						// Is there a ">>>" mark?
						if (itemStr.StartsWith(">>>"))
						{
							startPos = 3;
							break;
						}

						// What about the ">>" mark?
						if (itemStr.StartsWith(">>"))
						{
							startPos = 2;
							break;
						}

						// Is there a "?>>>" mark?
						if ((itemStr.Length >= 4) && (itemStr.Substring(1, 3) == ">>>"))
						{
							startPos = 4;
							break;
						}

						// What about the "?>>" mark?
						if ((itemStr.Length >= 4) && (itemStr.Substring(1, 2) == ">>"))
						{
							startPos = 3;
							break;
						}
					}

					if (startPos != -1)
					{
						// See if there is a "by" word after the mark
						pos = FindBy(itemStr.Substring(startPos));
						if (pos != -1)
							startPos = pos;
					}
				}

				if (startPos != -1)
					name = ClipOutAuthor(itemStr, startPos);
			}

			return name;
		}



		/********************************************************************/
		/// <summary>
		/// Find the end of the given string and return what is found
		/// </summary>
		/********************************************************************/
		private string ClipOutAuthor(string itemStr, int startPos)
		{
			// Got the start position of the author, now find the end position
			string name = itemStr.Substring(startPos).Trim();

			int pos;
			for (pos = 0; pos < name.Length; pos++)
			{
				// Get the current character
				char chr = name[pos];

				// Check for legal characters
				if (((chr == ' ') || (chr == '!') || (chr == '\'') || (chr == '-') || (chr == '/') || char.IsDigit(chr)))
				{
					// It's legal, go to the next character
					continue;
				}

				// Check to see if the & character is the last one and if
				// not, it's legal
				if ((chr == '&') && ((pos + 1) < name.Length))
					continue;

				if (chr == '.')
				{
					// The point is the last character
					if ((pos + 1) == name.Length)
						break;

					// Are there a space or another point after the first one?
					if ((name[pos + 1] == ' ') || (name[pos + 1] == '.'))
						break;

					continue;
				}

				// Is the character a letter?
				if (!char.IsLetter(chr))
				{
					// No, stop the loop
					break;
				}

				// Stop if .... of
				if ((pos + 1) < name.Length)
				{
					if ((chr == 'o') && (name[pos + 1] == 'f') && ((pos + 2) == name.Length))
					{
						if ((pos > 0) && (name[pos - 1] == ' '))
							break;
					}
				}

				// Stop if .... from
				if ((pos + 3) < name.Length)
				{
					if ((chr == 'f') && (name[pos + 1] == 'r') && (name[pos + 2] == 'o') && (name[pos + 3] == 'm') && ((pos + 4) == name.Length))
					{
						if ((pos > 0) && (name[pos - 1] == ' '))
							break;
					}
				}

				// Stop if .... in
				if ((pos + 1) < name.Length)
				{
					if ((chr == 'i') && (name[pos + 1] == 'n') && ((pos + 2) == name.Length))
					{
						if ((pos > 0) && (name[pos - 1] == ' '))
							break;
					}
				}

				// Stop if .... and
				if ((pos + 2) < name.Length)
				{
					if ((chr == 'a') && (name[pos + 1] == 'n') && (name[pos + 2] == 'd') && ((pos + 3) == name.Length))
					{
						if ((pos > 0) && (name[pos - 1] == ' '))
							break;
					}
				}
			}

			// Clip out the author
			name = name.Substring(0, pos).TrimEnd();

			// Check for some special characters that needs to be removed
			if (!string.IsNullOrEmpty(name))
			{
				for (;;)
				{
					char chr = name[name.Length - 1];
					if (chr != '-')
						break;

					if (name[0] == chr)
						break;

					name = name.Substring(0, name.Length - 1);
				}
			}

			return name.TrimEnd('/').TrimEnd();
		}



		/********************************************************************/
		/// <summary>
		/// Will look in the string given after the "by" word and return the
		/// index where it found it
		/// </summary>
		/********************************************************************/
		private int FindBy(string str)
		{
			int index = 0;
			bool found = false;

			while (index < (str.Length - 1))
			{
				if (((str[index] == 'b') || (str[index] == 'B')) && ((str[index + 1] == 'y') || (str[index + 1] == 'Y')))
				{
					// Check to see if the character before "by" is a letter
					if ((index > 0) && char.IsLetter(str[index - 1]))
					{
						index++;
						continue;
					}

					// Check to see if the character after "by" is a letter
					if (((index + 2) < str.Length) && char.IsLetter(str[index + 2]))
					{
						index++;
						continue;
					}

					if ((index + 2) == str.Length)
					{
						// The last word in the string is "by", so we found it
						return index + 2;
					}

					index += 2;
					found = true;
					break;
				}

				// Go to the next character
				index++;
			}

			// Did we found the "by" word?
			if (found)
			{
				// Yep, check if it's "by" some known phrases we need to ignore
				if (((index + 5) <= str.Length) && (str.Substring(index + 1, 4) == "KIWI"))
					return -1;

				if (((index + 11) <= str.Length) && (str.Substring(index + 1, 10) == "the welder"))
					return -1;

				if (((index + 7) <= str.Length) && (str.Substring(index + 1, 6) == "e-mail"))
					return -1;

				if (((index + 7) <= str.Length) && (str.Substring(index + 1, 6) == "Gryzor"))
					return -1;

				if (((index + 6) <= str.Length) && (str.Substring(index + 1, 5) == ">Asle"))
					return -1;

				if (((index + 5) <= str.Length) && (str.Substring(index + 1, 4) == "Asle"))
					return -1;

				if (((index + 8) <= str.Length) && (str.Substring(index + 1, 7) == "Trilogy"))
					return -1;
			}
			else
			{
				// Okay, now try to find "(c)"
				index = 0;
				while (index < (str.Length - 2))
				{
					if ((str[index] == '(') && ((str[index + 1] == 'c') || (str[index + 1] == 'C')) && (str[index + 2] == ')'))
					{
						index += 3;
						found = true;
						break;
					}

					// Go to the next character
					index++;
				}
			}

			if (found)
			{
				// Find the first letter in author
				for (; index < str.Length; index++)
				{
					if (str[index] < '0')
						continue;

					if ((str[index] <= '9') || (str[index] >= 'A'))
						break;
				}

				return index;
			}

			return -1;
		}
		#endregion
	}
}
