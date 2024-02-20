/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Exceptions;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;
using Polycode.NostalgicPlayer.PlayerLibrary.Players;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Loaders
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
			public long TotalLength;
			public bool HasSampleMarkings;
		}

		private readonly Manager agentManager;

		private ILoader currentLoader;
		private ConvertInfo convertInfo;
		private Stream loadStream;

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
		/// Will try to find a player that understand the file but will not
		/// load the file into memory. To load it, you can call the Load()
		/// override without filename.
		///
		/// For archive support, see the ArchiveFileDecruncher constructor
		/// description on how the path should look like
		/// </summary>
		/********************************************************************/
		public bool FindPlayer(string fileName, out string errorMessage)
		{
			CleanupLoadState();

			currentLoader = FindLoader(fileName);
			return FindPlayer(currentLoader, out errorMessage);
		}



		/********************************************************************/
		/// <summary>
		/// Will load the module recognized via FindPlayer() into memory
		/// </summary>
		/********************************************************************/
		public bool Load(out string errorMessage)
		{
			bool result = Load(currentLoader, out errorMessage);

			CleanupLoadState();

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Will try to find a player that understand the file and then load
		/// the file into memory.
		///
		/// For archive support, see the ArchiveFileDecruncher constructor
		/// description on how the path should look like
		/// </summary>
		/********************************************************************/
		public bool Load(string fileName, out string errorMessage)
		{
			using (ILoader loader = FindLoader(fileName))
			{
				return Load(loader, out errorMessage);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will try to find a player that understand the data in the loader
		/// given and then load it into memory.
		///
		/// The loading is implemented in a way, so if the module needs to be
		/// converted, the sample data is not copied over to the new stream,
		/// but is marked instead. When the player then loads the module, it
		/// will read the module structure from the converted stream and
		/// sample data from the original stream
		/// </summary>
		/********************************************************************/
		public bool Load(ILoader loader, out string errorMessage)
		{
			bool result = FindPlayer(loader, out errorMessage);
			if (result)
				result = LoadModule(loader, out errorMessage);

			CleanupLoadState();

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Will unload the loaded file and free it from memory
		/// </summary>
		/********************************************************************/
		public void Unload()
		{
			CleanupLoadState();

			Stream?.Dispose();
			Stream = null;

			PlayerAgentInfo = null;
			PlayerAgent = null;

			ConverterAgentInfo = null;

			Player = null;

			ModuleSize = 0;
			ModuleFormat = string.Empty;
			ModuleFormatDescription = string.Empty;
			PlayerName = string.Empty;
			PlayerDescription = string.Empty;
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
		/// Return the module format description
		/// </summary>
		/********************************************************************/
		internal string ModuleFormatDescription
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
		/// Return the description of the player
		/// </summary>
		/********************************************************************/
		internal string PlayerDescription
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
		/// Return the size of the module crunched. Is zero if not crunched.
		/// If -1, it means the crunched length is unknown
		/// </summary>
		/********************************************************************/
		internal long CrunchedSize
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
		/// Will open a ModuleStream based on the arguments given
		/// opened file. This method can open archive files
		/// </summary>
		/********************************************************************/
		private ModuleStream OpenModuleStream(ConvertInfo convertInfo, Stream stream)
		{
			return convertInfo != null ? new ModuleStream(convertInfo.ConvertedStream, convertInfo.SampleDataStream ?? stream, convertInfo.TotalLength, convertInfo.HasSampleMarkings) : new ModuleStream(stream, true);
		}



		/********************************************************************/
		/// <summary>
		/// Will parse the file name path and return the loader with the
		/// opened file. This method can open archive files
		/// </summary>
		/********************************************************************/
		private ILoader FindLoader(string fileName)
		{
			if (ArchivePath.IsArchivePath(fileName))
				return new ArchiveFileLoader(fileName, agentManager);

			return new NormalFileLoader(fileName, agentManager);
		}



		/********************************************************************/
		/// <summary>
		/// Will try to find a player that understand the data in the loader
		/// given
		/// </summary>
		/********************************************************************/
		private bool FindPlayer(ILoader loader, out string errorMessage)
		{
			bool result = false;
			errorMessage = string.Empty;

			try
			{
				loadStream = loader.OpenFile();

				bool foundPlayer;
				using (ModuleStream moduleStream = new ModuleStream(loadStream, true))
				{
					PlayerFileInfo fileInfo = new PlayerFileInfo(loader.FullPath, moduleStream, loader);

					// Check to see if we can find a player via the file type
					foundPlayer = FindPlayerViaFileType(fileInfo);
				}

				if (!foundPlayer)
				{
					// No player could be found via the file type.
					// Now try to convert the module
					convertInfo = ConvertModule(loadStream, loader, loader.FullPath, out string converterError);
					if (!string.IsNullOrEmpty(converterError))
					{
						// Something went wrong when converting the module
						//
						// Build the error string
						errorMessage = string.Format(Resources.IDS_ERR_CONVERT_MODULE, loader.FullPath, convertInfo.Agent.TypeName, converterError);
					}
					else
					{
						// Try all the players to see if we can find one
						// that understand the file format
						using (ModuleStream moduleStream = OpenModuleStream(convertInfo, loadStream))
						{
							PlayerFileInfo fileInfo = new PlayerFileInfo(loader.FullPath, moduleStream, loader);

							if (FindPlayer(fileInfo))
								result = true;
						}
					}
				}
				else
					result = true;
			}
			catch(DecruncherException ex)
			{
				// Build the error string
				errorMessage = string.Format(Resources.IDS_ERR_DECRUNCH_MODULE, Path.GetFileName(loader.FullPath), ex.AgentName, ex.Message);
				result = false;
			}
			catch(Exception ex)
			{
				// Build an error message
				errorMessage = string.Format(Resources.IDS_ERR_FILE, Path.GetFileName(loader.FullPath), ex.HResult.ToString("X8"), ex.Message);
				result = false;
			}

			if (!result)
			{
				// If a module has been converted, we don't need to converted stream anymore
				convertInfo?.ConvertedStream?.Dispose();
				convertInfo?.SampleDataStream?.Dispose();
				convertInfo = null;

				loadStream?.Dispose();
				loadStream = null;

				// Set error message if not already set
				if (string.IsNullOrEmpty(errorMessage))
					errorMessage = string.Format(Resources.IDS_ERR_UNKNOWN_MODULE, loader.FullPath);
			}

			return result;
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

			HashSet<Guid> agentsToSkip = new HashSet<Guid>();

			foreach (AgentInfo info in agentManager.GetAllAgents(Manager.AgentType.Players))
			{
				if (agentsToSkip.Contains(info.AgentId))
					continue;

				IPlayerAgent player = null;
				AgentInfo agentInfo = info;

				// Do the player implement multiple format identify method?
				if (agentInfo.Agent is IPlayerAgentMultipleFormatIdentify multipleFormatIdentify)
				{
					// Since this is a multi format agent, we don't want to call the agent for
					// each format and therefore we store the ID in this list
					agentsToSkip.Add(agentInfo.AgentId);

					// Get the extensions
					string[] playerExtensions = multipleFormatIdentify.FileExtensions;

					// Did we get a match
					if (playerExtensions.Contains(fileExtension) || (!string.IsNullOrEmpty(postExtension) && playerExtensions.Contains(postExtension)))
					{
						IdentifyFormatInfo identifyFormatInfo = multipleFormatIdentify.IdentifyFormat(fileInfo);
						if (identifyFormatInfo != null)
						{
							agentInfo = agentManager.GetAgent(Manager.AgentType.Players, identifyFormatInfo.TypeId);
							if (agentInfo.Enabled)
								player = identifyFormatInfo.Worker as IPlayerAgent;
						}
					}
				}
				else
				{
					// Check if the player is enabled
					if (agentInfo.Enabled)
					{
						// Create an instance of the player
						player = agentInfo.Agent.CreateInstance(agentInfo.TypeId) as IPlayerAgent;
						if (player != null)
						{
							// Get the extensions
							string[] playerExtensions = player.FileExtensions;

							// Did we get a match
							if (playerExtensions.Contains(fileExtension) || (!string.IsNullOrEmpty(postExtension) && playerExtensions.Contains(postExtension)))
							{
								// Found the file extension in a player. Now call the
								// players check routine, just to make sure it's
								// the right player
								AgentResult agentResult = player.Identify(fileInfo);

								if (agentResult == AgentResult.Error)
								{
									// Some error occurred
									throw new Exception($"Identify() on player {agentInfo.TypeName} returned an error");
								}

								if (agentResult == AgentResult.Unknown)
									player = null;
							}
							else
								player = null;
						}
					}
				}

				if (player != null)
				{
					// We found a player
					PlayerAgentInfo = agentInfo;
					PlayerAgent = player;

					return true;
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
			// Create a list with all the players and sort them by priority
			List<(IPlayerAgent player, AgentInfo agentInfo)> agents = new List<(IPlayerAgent, AgentInfo)>();

			foreach (AgentInfo agentInfo in agentManager.GetAllAgents(Manager.AgentType.Players))
			{
				// Is the player enabled?
				if (agentInfo.Enabled)
				{
					// Create an instance of the player
					if (agentInfo.Agent.CreateInstance(agentInfo.TypeId) is IPlayerAgent player)
						agents.Add((player, agentInfo));
				}
			}

			HashSet<Guid> agentsToSkip = new HashSet<Guid>();

			foreach ((IPlayerAgent player, AgentInfo agentInfo) agent in agents.OrderBy(a => a.player.IdentifyPriority))
			{
				AgentInfo agentInfo = agent.agentInfo;

				if (agentsToSkip.Contains(agentInfo.AgentId))
					continue;

				IPlayerAgent player = null;

				// Do the player implement multiple format identify method?
				if (agentInfo.Agent is IPlayerAgentMultipleFormatIdentify multipleFormatIdentify)
				{
					// Since this is a multi format agent, we don't want to call the agent for
					// each format and therefore we store the ID in this list
					agentsToSkip.Add(agentInfo.AgentId);

					IdentifyFormatInfo identifyFormatInfo = multipleFormatIdentify.IdentifyFormat(fileInfo);
					if (identifyFormatInfo != null)
					{
						agentInfo = agentManager.GetAgent(Manager.AgentType.Players, identifyFormatInfo.TypeId);
						if (agentInfo.Enabled)
							player = identifyFormatInfo.Worker as IPlayerAgent;
					}
				}
				else
				{
					// Check the file
					AgentResult agentResult = agent.player.Identify(fileInfo);

					if (agentResult == AgentResult.Error)
					{
						// Some error occurred
						throw new Exception($"Identify() on player {agent.agentInfo.TypeName} returned an error");
					}

					if (agentResult == AgentResult.Ok)
						player = agent.player;
				}

				if (player != null)
				{
					// We found the right player
					PlayerAgentInfo = agentInfo;
					PlayerAgent = player;

					return true;
				}
			}

			// No player was found
			return false;
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
									//
									// First we call the converter to see if it know the size of
									// the converted module. If not, we initialize it with an ok
									// buffer size, but not bigger than the original file, so it
									// won't be reallocated a lot
									int convertedLength = converter.ConvertedModuleLength(fileInfo);
									if (convertedLength > 0)
										convertedLength += 512;	// Add extra space for sample meta data
									else
										convertedLength = Math.Min(64 * 1024, (int)moduleStream.Length);

									MemoryStream ms = new MemoryStream(convertedLength);

									using (ConverterStream converterStream = new ConverterStream(ms, sampleInfo))
									{
										agentResult = converter.Convert(fileInfo, converterStream, out errorMessage);
										if (agentResult == AgentResult.Ok)
										{
											// Replace the module stream with the converted stream
											stream = ms;

											if (result == null)
												result = new ConvertInfo { Agent = agentInfo, OriginalFormat = converter.OriginalFormat, HasSampleMarkings = converterStream.HasSampleDataMarkers };

											if (!converterStream.HasSampleDataMarkers && (result.SampleDataStream == null))		// If we need to support multiple markings, it could be implemented by using a stack
											{
												byte[] buffer = ms.GetBuffer();
												result.SampleDataStream = new MemoryStream(buffer, 0, buffer.Length, false, true);
											}

											if (converterStream.HasSampleDataMarkers && (result.SampleDataStream is MemoryStream oldSampleDataStream))
											{
												result.SampleDataStream = new MemoryStream(oldSampleDataStream.GetBuffer());
												oldSampleDataStream.Dispose();
												result.HasSampleMarkings = true;
											}

											if (result.ConvertedStream != null)
												result.ConvertedStream.Dispose();

											result.ConvertedStream = ms;
											result.TotalLength = converterStream.ConvertedLength;

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



		/********************************************************************/
		/// <summary>
		/// Load the module into memory
		/// </summary>
		/********************************************************************/
		private bool LoadModule(ILoader loader, out string errorMessage)
		{
			bool result = true;
			errorMessage = string.Empty;

			try
			{
				// Yes, initialize the module stream with the right information
				using (ModuleStream moduleStream = OpenModuleStream(convertInfo, loadStream))
				{
					PlayerFileInfo fileInfo = new PlayerFileInfo(loader.FullPath, moduleStream, loader);

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
						errorMessage = string.Format(Resources.IDS_ERR_LOAD_MODULE, fileInfo.FileName, string.IsNullOrEmpty(PlayerAgentInfo.TypeName) ? PlayerAgentInfo.AgentName : PlayerAgentInfo.TypeName, playerError);

						PlayerAgentInfo = null;
						PlayerAgent = null;

						result = false;
					}
					else
					{
						// Get module information
						FileName = fileInfo.FileName;

						PlayerName = PlayerAgentInfo.AgentName;
						PlayerDescription = PlayerAgentInfo.AgentDescription;

						ModuleFormat = convertInfo != null ? string.IsNullOrEmpty(convertInfo.OriginalFormat) ? convertInfo.Agent.TypeName : convertInfo.OriginalFormat : string.IsNullOrEmpty(PlayerAgentInfo.TypeName) ? PlayerAgentInfo.AgentName : PlayerAgentInfo.TypeName;
						if (!string.IsNullOrEmpty(PlayerAgent.ExtraFormatInfo))
							ModuleFormat += $" ({PlayerAgent.ExtraFormatInfo})";

						ModuleFormatDescription = convertInfo != null ? convertInfo.Agent.TypeDescription : string.IsNullOrEmpty(PlayerAgentInfo.TypeName) ? PlayerAgentInfo.AgentDescription : PlayerAgentInfo.TypeDescription;

						ConverterAgentInfo = convertInfo?.Agent;
					}
				}
			}
			catch(Exception ex)
			{
				// Build an error message
				errorMessage = string.Format(Resources.IDS_ERR_FILE, Path.GetFileName(loader.FullPath), ex.HResult.ToString("X8"), ex.Message);
				result = false;
			}

			// If a module has been converted, we don't need to converted stream anymore
			convertInfo?.ConvertedStream?.Dispose();
			convertInfo?.SampleDataStream?.Dispose();

			// Close the files again if needed
			if (!result || PlayerAgent is IModulePlayerAgent)
				loadStream?.Dispose();
			else
			{
				// Remember the stream, so it can be closed later on
				Stream = new ModuleStream(loadStream, false);
			}

			if (result)
			{
				Player = PlayerAgent is IModulePlayerAgent ? new ModulePlayer(agentManager) : PlayerAgent is ISamplePlayerAgent ? new SamplePlayer(agentManager) : null;

				ModuleSize = loader.ModuleSize;
				CrunchedSize = loader.CrunchedSize;
			}

			CleanupLoadState();

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup load variables
		/// </summary>
		/********************************************************************/
		private void CleanupLoadState()
		{
			convertInfo = null;
			loadStream = null;

			currentLoader?.Dispose();
			currentLoader = null;
		}
		#endregion
	}
}
