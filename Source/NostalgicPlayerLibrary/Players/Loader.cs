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
using System.Linq;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Exceptions;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Players
{
	/// <summary>
	/// Loader class that helps load a module
	/// </summary>
	public class Loader
	{
		private class ConvertInfo
		{
			public AgentInfo Agent;
			public string OriginalFormat;
			public Stream SampleDataStream;
			public MemoryStream ConvertedStream;
		}

		private readonly Manager agentManager;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Loader(Manager agentManager)
		{
			this.agentManager = agentManager;

			PlayerAgentInfo = null;
			PlayerAgent = null;
			ConverterAgentInfo = null;
			Stream = null;
		}



		/********************************************************************/
		/// <summary>
		/// Will try to find a player that understand the file and then load
		/// the file into memory
		/// </summary>
		/********************************************************************/
		public bool Load(string fileName, ILoader loader, out string errorMessage)
		{
			return Load(OpenFile(loader), loader, fileName, out errorMessage);
		}



		/********************************************************************/
		/// <summary>
		/// Will try to find a player that understand the data in the stream
		/// given and then load it into memory.
		///
		/// This method takes ownership of the stream, so it will be closed
		/// when needed, even if no player could be found
		/// </summary>
		/********************************************************************/
		public bool Load(Stream stream, out string errorMessage)
		{
			return Load(stream, null, string.Empty, out errorMessage);
		}



		/********************************************************************/
		/// <summary>
		/// Will unload the loaded file and free it from memory
		/// </summary>
		/********************************************************************/
		public void Unload()
		{
			Stream?.Dispose();
			Stream = null;

			PlayerAgentInfo = null;
			PlayerAgent = null;

			ConverterAgentInfo = null;

			Player = null;

			ModuleSize = 0;
			ModuleFormat = string.Empty;
			PlayerName = string.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// Return the player instance
		/// </summary>
		/********************************************************************/
		public IPlayer Player
		{
			get; private set;
		}



		/********************************************************************/
		/// <summary>
		/// Return information about the player agent
		/// </summary>
		/********************************************************************/
		public AgentInfo PlayerAgentInfo
		{
			get; private set;
		}



		/********************************************************************/
		/// <summary>
		/// Return the agent player instance
		/// </summary>
		/********************************************************************/
		internal IPlayerAgent PlayerAgent
		{
			get; private set;
		}



		/********************************************************************/
		/// <summary>
		/// Return information about the converter agent
		/// </summary>
		/********************************************************************/
		public AgentInfo ConverterAgentInfo
		{
			get; private set;
		}



		/********************************************************************/
		/// <summary>
		/// Return the file name of the module loaded
		/// </summary>
		/********************************************************************/
		public string FileName
		{
			get; private set;
		}



		/********************************************************************/
		/// <summary>
		/// Return the module format of the module loaded
		/// </summary>
		/********************************************************************/
		internal string ModuleFormat
		{
			get; private set;
		}



		/********************************************************************/
		/// <summary>
		/// Return the name of the player
		/// </summary>
		/********************************************************************/
		internal string PlayerName
		{
			get; private set;
		}



		/********************************************************************/
		/// <summary>
		/// Return the size of the module loaded
		/// </summary>
		/********************************************************************/
		internal long ModuleSize
		{
			get; private set;
		}



		/********************************************************************/
		/// <summary>
		/// Return the stream to use when reading sample files
		/// </summary>
		/********************************************************************/
		internal ModuleStream Stream
		{
			get; private set;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will try to find a player that understand the data in the stream
		/// given and then load it into memory.
		///
		/// This method takes ownership of the stream, so it will be closed
		/// when needed, even if no player could be found.
		///
		/// The loading is implemented in a way, so if the module needs to be
		/// converted, the sample data is not copied over to the new stream,
		/// but is marked instead. When the player then loads the module, it
		/// will read the module structure from the converted stream and
		/// sample data from the original stream given as argument to this
		/// method
		/// </summary>
		/********************************************************************/
		private bool Load(Stream stream, ILoader loader, string fileName, out string errorMessage)
		{
			bool result = false;
			errorMessage = string.Empty;

			ConvertInfo convertInfo = null;

			// Get the original length of the file
			ModuleSize = stream.Length;

			try
			{
				// First try to depack the file if needed
				stream = DepackFileMultipleLevels(stream);

				bool foundPlayer;
				using (ModuleStream moduleStream = new ModuleStream(stream, true))
				{
					PlayerFileInfo fileInfo = new PlayerFileInfo(fileName, moduleStream, loader);

					// Check to see if we can find a player via the file type
					foundPlayer = FindPlayerViaFileType(fileInfo);
				}

				if (!foundPlayer)
				{
					// No player could be found via the file type.
					// Now try to convert the module
					convertInfo = ConvertModule(stream, loader, fileName, out string converterError);
					if (!string.IsNullOrEmpty(converterError))
					{
						// Something went wrong when converting the module
						//
						// Build the error string
						errorMessage = string.Format(Resources.IDS_ERR_CONVERT_MODULE, fileName, convertInfo.Agent.TypeName, converterError);
					}
					else
					{
						// Try all the players to see if we can find one
						// that understand the file format
						using (ModuleStream moduleStream = new ModuleStream(convertInfo != null ? convertInfo.ConvertedStream : stream, true))
						{
							PlayerFileInfo fileInfo = new PlayerFileInfo(fileName, moduleStream, loader);

							if (FindPlayer(fileInfo))
								result = true;
						}
					}
				}
				else
					result = true;

				// Did we found a player?
				if (result)
				{
					// Yes, initialize the module stream with the right information
					using (ModuleStream moduleStream = convertInfo != null ? new ModuleStream(convertInfo.ConvertedStream, convertInfo.SampleDataStream ?? stream) : new ModuleStream(stream, true))
					{
						PlayerFileInfo fileInfo = new PlayerFileInfo(fileName, moduleStream, loader);

						// Most players will start reading the structure from the beginning, so start there
						moduleStream.Seek(0, SeekOrigin.Begin);

						AgentResult agentResult = AgentResult.Unknown;
						string playerError = string.Empty;

						if (PlayerAgent is IModulePlayerAgent modulePlayerAgent)
						{
							// Load the module if the player is a module player
							agentResult = modulePlayerAgent.Load(fileInfo, out playerError);
						}
						else if (PlayerAgent is ISamplePlayerAgent samplePlayerAgent)
						{
							// Load header information if sample player
							agentResult = samplePlayerAgent.LoadHeaderInfo(fileInfo.ModuleStream, out playerError);
						}

						if (agentResult != AgentResult.Ok)
						{
							// Well, something went wrong when loading the file
							//
							// Build the error string
							errorMessage = string.Format(Resources.IDS_ERR_LOAD_MODULE, fileInfo.FileName, PlayerAgentInfo.TypeName, playerError);

							PlayerAgentInfo = null;
							PlayerAgent = null;

							result = false;
						}
						else
						{
							// Get module information
							FileName = fileInfo.FileName;

							PlayerName = PlayerAgentInfo.AgentName;
							ModuleFormat = convertInfo != null ? string.IsNullOrEmpty(convertInfo.OriginalFormat) ? convertInfo.Agent.TypeName : convertInfo.OriginalFormat : PlayerAgentInfo.TypeName;

							ConverterAgentInfo = convertInfo?.Agent;
						}
					}
				}
				else
				{
					if (string.IsNullOrEmpty(errorMessage))
					{
						// No, send an error back
						errorMessage = string.Format(Resources.IDS_ERR_UNKNOWN_MODULE, fileName);
					}
				}
			}
			catch(DepackerException ex)
			{
				// Build the error string
				errorMessage = string.Format(Resources.IDS_ERR_DEPACK_MODULE, fileName, ex.AgentName, errorMessage);
				result = false;
			}
			catch(Exception ex)
			{
				// Build an error message
				errorMessage = string.Format(Resources.IDS_ERR_FILE, fileName, ex.HResult.ToString("X8"), ex.Message);
				result = false;
			}

			// If a module has been converted, we don't need to converted stream anymore
			convertInfo?.ConvertedStream?.Dispose();
			convertInfo?.SampleDataStream?.Dispose();

			// Close the files again if needed
			if (!result || PlayerAgent is IModulePlayerAgent)
				stream.Dispose();
			else
			{
				// Remember the stream, so it can be closed later on
				Stream = new ModuleStream(stream, false);
			}

			if (result)
				Player = PlayerAgent is IModulePlayerAgent ? new ModulePlayer(agentManager) : PlayerAgent is ISamplePlayerAgent ? new SamplePlayer(agentManager) : null;

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Will try to open the file given
		/// </summary>
		/********************************************************************/
		private Stream OpenFile(ILoader loader)
		{
			return loader.OpenFile();
		}



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
								PlayerAgentInfo = agentInfo;
								PlayerAgent = player;

								return true;
							}

							if (agentResult != AgentResult.Unknown)
							{
								// Some error occurred
								throw new Exception($"Identify() on player {agentInfo.TypeName} returned an error");
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
							PlayerAgentInfo = agentInfo;
							PlayerAgent = player;

							return true;
						}

						if (agentResult != AgentResult.Unknown)
						{
							// Some error occurred
							throw new Exception($"Identify() on player {agentInfo.TypeName} returned an error");
						}
					}
				}
			}

			// No player was found
			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Will try to depack the file if needed. Will also check if the
		/// file has been packed multiple times
		/// </summary>
		/********************************************************************/
		private Stream DepackFileMultipleLevels(Stream stream)
		{
			for (;;)
			{
				DepackerStream depackerStream = DepackFile(stream);
				if (depackerStream == null)
					return stream;

				if (depackerStream.CanSeek)
					stream = depackerStream;
				else
					stream = new SeekableStream(depackerStream);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will try to depack the file if needed
		/// </summary>
		/********************************************************************/
		private DepackerStream DepackFile(Stream packedDataStream)
		{
			foreach (AgentInfo agentInfo in agentManager.GetAllAgents(Manager.AgentType.FileDecrunchers))
			{
				// Is the depacker enabled?
				if (agentInfo.Enabled)
				{
					// Create an instance of the depacker
					if (agentInfo.Agent.CreateInstance(agentInfo.TypeId) is IFileDecruncherAgent decruncher)
					{
						// Check the file
						AgentResult agentResult = decruncher.Identify(packedDataStream);
						if (agentResult == AgentResult.Ok)
							return decruncher.OpenStream(packedDataStream);

						if (agentResult != AgentResult.Unknown)
						{
							// Some error occurred
							throw new DepackerException(agentInfo.TypeName, "Identify() returned an error");
						}
					}
				}
			}

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Will try to convert the module to another format
		/// </summary>
		/********************************************************************/
		private ConvertInfo ConvertModule(Stream stream, ILoader loader, string fileName, out string errorMessage)
		{
			ConvertInfo result = null;
			bool takeAnotherRound;

			// This list is used to hold sample information needed
			Dictionary<int, ConvertSampleInfo> sampleInfo = new Dictionary<int, ConvertSampleInfo>();

			do
			{
				takeAnotherRound = false;

				using (ModuleStream moduleStream = new ModuleStream(stream, sampleInfo))
				{
					PlayerFileInfo fileInfo = new PlayerFileInfo(fileName, moduleStream, loader);

					foreach (AgentInfo agentInfo in agentManager.GetAllAgents(Manager.AgentType.ModuleConverters))
					{
						// Is the converter enabled?
						if (agentInfo.Enabled)
						{
							// Create an instance of the converter
							if (agentInfo.Agent.CreateInstance(agentInfo.TypeId) is IModuleConverterAgent converter)
							{
								// Check the file
								AgentResult agentResult = converter.Identify(fileInfo);
								if (agentResult == AgentResult.Ok)
								{
									// We found the right converter, so now convert it
									moduleStream.Seek(0, SeekOrigin.Begin);

									// Create new memory stream to store the converted module in.
									// We initialize it with an ok buffer size, but not bigger than
									// the original file, so it won't be reallocated a lot
									MemoryStream ms = new MemoryStream(Math.Min(64 * 1024, (int)moduleStream.Length));

									using (ConverterStream converterStream = new ConverterStream(ms, sampleInfo))
									{
										agentResult = converter.Convert(fileInfo, converterStream, out errorMessage);
										if (agentResult == AgentResult.Ok)
										{
											// Replace the module stream with the converted stream
											stream = ms;

											if (result == null)
												result = new ConvertInfo { Agent = agentInfo, OriginalFormat = converter.OriginalFormat };

											if (!converterStream.HasSampleDataMarkers && (result.SampleDataStream == null))		// If we need to support multiple markings, it could be implemented by using a stack
												result.SampleDataStream = new MemoryStream(ms.GetBuffer());

											if (result.ConvertedStream != null)
												result.ConvertedStream.Dispose();

											result.ConvertedStream = ms;

											// The module may need to be converted multiple times, so
											// we make a new check with the converted module
											takeAnotherRound = true;
											break;
										}

										// An error occurred, so return immediately. The error
										// is stored in the errorMessage out argument
										return new ConvertInfo { Agent = agentInfo };
									}
								}

								if (agentResult != AgentResult.Unknown)
								{
									// Some error occurred
									throw new Exception($"Identify() on module converter {agentInfo.TypeName} returned an error");
								}
							}
						}
					}
				}
			}
			while (takeAnotherRound);

			errorMessage = string.Empty;

			return result;
		}
		#endregion
	}
}
