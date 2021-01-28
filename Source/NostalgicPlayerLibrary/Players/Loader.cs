/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.IO;
using System.Linq;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;
using Polycode.NostalgicPlayer.PlayerLibrary.Containers;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Players
{
	/// <summary>
	/// Loader class that helps load a module
	/// </summary>
	public class Loader
	{
		private readonly Manager agentManager;

		private AgentInfo playerAgentInfo;
		private IPlayerAgent playerAgent;

		private IPlayer player;

		private long fileLength;
		private string moduleFormat;
		private string playerName;

		private ModuleStream stream;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Loader(Manager agentManager)
		{
			this.agentManager = agentManager;

			playerAgentInfo = null;
			playerAgent = null;
			stream = null;
		}



		/********************************************************************/
		/// <summary>
		/// Will try to find a player that understand the file and then load
		/// the file into memory.
		///
		/// This method takes ownership of the stream, so it will be closed
		/// when needed, even if no player could be found
		/// </summary>
		/********************************************************************/
		public bool Load(PlayerFileInfo fileInfo, out string errorMessage)
		{
			bool result = false;
			errorMessage = string.Empty;

			// Get the original length of the file
			fileLength = fileInfo.ModuleStream.Length;

			try
			{
				// Check to see if we can find a player via the file type
				if (!FindPlayerViaFileType(fileInfo))
				{
					// No player could be found via the file type

					// Try all the players to see if we can find one
					// that understand the file format
					if (FindPlayer(fileInfo))
						result = true;
				}
				else
					result = true;

				// Did we found a player?
				if (result)
				{
					// Yes, load the module if the player is a module player
					fileInfo.ModuleStream.Seek(0, SeekOrigin.Begin);

					if (playerAgent is IModulePlayerAgent modulePlayerAgent)
					{
						AgentResult agentResult = modulePlayerAgent.Load(fileInfo, out string playerError);

						if (agentResult != AgentResult.Ok)
						{
							// Well, something went wrong when loading the file
							//
							// Build the error string
							errorMessage = string.Format(Resources.IDS_ERR_LOAD_MODULE, fileInfo.FileName, playerAgentInfo.AgentName, playerError);

							playerAgentInfo = null;
							playerAgent = null;

							result = false;
						}
						else
						{
							// Get module information
							playerName = playerAgentInfo.AgentName;//XX

							moduleFormat = playerAgentInfo.AgentName;
						}
					}
				}
				else
				{
					// No, send an error back
					errorMessage = string.Format(Resources.IDS_ERR_UNKNOWN_MODULE, fileInfo.FileName);
				}
			}
			catch (Exception ex)
			{
				// Build an error message
				errorMessage = string.Format(Resources.IDS_ERR_FILE, fileInfo.FileName, ex.HResult.ToString("X8"), ex.Message);
				result = false;
			}

			// Close the files again if needed
			if (!result || playerAgent is IModulePlayerAgent)
				fileInfo.ModuleStream.Dispose();
			else
			{
				// Remember the stream, so it can be closed later on
				stream = fileInfo.ModuleStream;
			}

			if (result)
				player = playerAgent is IModulePlayerAgent ? new ModulePlayer() : null;

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Will unload the loaded file and free it from memory
		/// </summary>
		/********************************************************************/
		public void Unload()
		{
			stream?.Dispose();
			stream = null;

			playerAgentInfo = null;
			playerAgent = null;

			player = null;

			fileLength = 0;
			moduleFormat = string.Empty;
			playerName = string.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// Return the player instance
		/// </summary>
		/********************************************************************/
		public IPlayer Player
		{
			get
			{
				return player;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return information about the player agent
		/// </summary>
		/********************************************************************/
		public AgentInfo PlayerAgentInfo
		{
			get
			{
				return playerAgentInfo;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the agent player instance
		/// </summary>
		/********************************************************************/
		internal IPlayerAgent AgentPlayer
		{
			get
			{
				return playerAgent;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the module format of the module loaded
		/// </summary>
		/********************************************************************/
		internal string ModuleFormat => moduleFormat;



		/********************************************************************/
		/// <summary>
		/// Return the name of the player
		/// </summary>
		/********************************************************************/
		internal string PlayerName => playerName;



		/********************************************************************/
		/// <summary>
		/// Return the size of the module loaded
		/// </summary>
		/********************************************************************/
		internal long ModuleSize => fileLength;

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will get the file type and try to find a player that is
		/// associated with the type
		/// </summary>
		/********************************************************************/
		private bool FindPlayerViaFileType(PlayerFileInfo fileInfo)
		{
			// Get the file extension and pre-extension
			string fileName = Path.GetFileName(fileInfo.FileName);

			int lastIndex = fileName.LastIndexOf('.');
			if (lastIndex == -1)
				return false;

			string fileExtension = fileName.Substring(lastIndex + 1).ToLower();
			int index = fileName.IndexOf('.');
			string postExtension = index == lastIndex ? string.Empty : fileName.Substring(0, index).ToLower();

			foreach (AgentInfo agentInfo in agentManager.GetAllAgents(Manager.AgentType.Players))
			{
				// Check if the player is enabled
				if (agentInfo.Enabled)
				{
					// Create an instance of the player
					if (agentInfo.Agent.CreateInstance(agentInfo.TypeId) is IPlayerAgent player)
					{
						// Get the player type
						string[] playerTypes = player.FileExtensions;

						// Did we get a match
						if (playerTypes.Contains(fileExtension) || (!string.IsNullOrEmpty(postExtension) && playerTypes.Contains(postExtension)))
						{
							// Found the file type in a player. Now call the
							// players check routine, just to make sure it's
							// the right player
							AgentResult agentResult = player.Identify(fileInfo);
							if (agentResult == AgentResult.Ok)
							{
								// We found a player
								playerAgentInfo = agentInfo;
								playerAgent = player;

								return true;
							}

							if (agentResult != AgentResult.Unknown)
							{
								// Some error occurred
								throw new Exception();
							}
						}
					}
				}
			}

			// No player was found
			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Will call all the players identify method to see if some of them
		/// understand the file format
		/// </summary>
		/********************************************************************/
		private bool FindPlayer(PlayerFileInfo fileInfo)
		{
			foreach (AgentInfo agentInfo in agentManager.GetAllAgents(Manager.AgentType.Players))
			{
				// Is the player enabled?
				if (agentInfo.Enabled)
				{
					// Create an instance of the player
					if (agentInfo.Agent.CreateInstance(agentInfo.TypeId) is IPlayerAgent player)
					{
						// Check the file
						AgentResult agentResult = player.Identify(fileInfo);
						if (agentResult == AgentResult.Ok)
						{
							// We found the right player
							playerAgentInfo = agentInfo;
							playerAgent = player;

							return true;
						}

						if (agentResult != AgentResult.Unknown)
						{
							// Some error occurred
							throw new Exception();
						}
					}
				}
			}

			// No player was found
			return false;
		}
		#endregion
	}
}
