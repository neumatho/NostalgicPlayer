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
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;
using Polycode.NostalgicPlayer.PlayerLibrary.Containers;
using Polycode.NostalgicPlayer.PlayerLibrary.Players;

namespace Polycode.NostalgicPlayer.Client.ConsolePlayer
{
	/// <summary>
	/// NostalgicPlayer console player. Mainly used to easily test players
	/// </summary>
	public static class Program
	{
		/********************************************************************/
		/// <summary>
		/// Main entry point
		/// </summary>
		/********************************************************************/
		public static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine("Music player to play old-school Amiga and PC modules.");
				Console.WriteLine();
				Console.WriteLine("Syntax: NostalgicPlayerConsole file");
				return;
			}

			try
			{
				string errorMessage;

				string fileName = args[0];

				// Load needed agents
				Manager agentManager = new Manager();
				agentManager.LoadAllAgents();

				// First make sure that the file exists
				if (!File.Exists(fileName))
				{
					Console.WriteLine("File does not exists");
					return;
				}

				// Load the file
				Loader loader = new Loader(agentManager);
				if (!loader.Load(new PlayerFileInfo(fileName, new ModuleStream(new FileStream(fileName, FileMode.Open, FileAccess.Read))), out errorMessage))
				{
					Console.WriteLine("Could not load the module. Failed with error:");
					Console.WriteLine(errorMessage);
					return;
				}

				try
				{
					// Find the output agent to use
					IOutputAgent outputAgent = FindOutputAgent(agentManager);
					if (outputAgent == null)
					{
						Console.WriteLine("Could not find CoreAudio output agent");
						return;
					}

					// Initialize output agent so its ready for use
					if (outputAgent.Initialize(out errorMessage) == AgentResult.Error)
					{
						Console.WriteLine("Cannot initialize output device.");
						Console.WriteLine(errorMessage);
						return;
					}

					try
					{
						IPlayer player = loader.Player;

						if (!player.InitPlayer(new PlayerConfiguration(outputAgent, loader, new MixerConfiguration
							{
								EnableAmigaFilter = true,
								StereoSeparator = 100
							}), out errorMessage))
						{
							Console.WriteLine("Cannot initialize player.");
							Console.WriteLine(errorMessage);
							return;
						}

						try
						{
							// Start to play the music
							if (player is IModulePlayer modulePlayer)
								modulePlayer.SelectSong(0);

							player.StartPlaying(loader.FileName);

							try
							{
								ModuleInfoStatic moduleInfoStatic = player.StaticModuleInformation;
								ModuleInfoFloating moduleInfoFloating = player.PlayingModuleInformation;

								Console.WriteLine("Playing module");
								Console.WriteLine();
								Console.WriteLine("Module name/file name: " + (string.IsNullOrEmpty(moduleInfoStatic.ModuleName) ? Path.GetFileName(fileName) : moduleInfoStatic.ModuleName));
								Console.WriteLine("Author: " + (string.IsNullOrEmpty(moduleInfoStatic.Author) ? "Unknown" : moduleInfoStatic.Author));
								Console.WriteLine("Module format: " + moduleInfoStatic.ModuleFormat);
								Console.WriteLine("Active player: " + moduleInfoStatic.PlayerName);
								Console.WriteLine("Used channels: " + moduleInfoStatic.Channels);
								Console.WriteLine("Total time: " + moduleInfoFloating.TotalTime.ToString(@"m\:ss"));
								Console.WriteLine("Module size: " + moduleInfoStatic.ModuleSize);
								Console.WriteLine("File name: " + fileName);
								Console.WriteLine();

								// Output extra information
								foreach (string info in moduleInfoFloating.ModuleInformation)
								{
									string[] parts = info.Split('\t');
									Console.WriteLine(parts[0] + ": " + parts[1]);
								}

								Console.WriteLine();
								Console.WriteLine("Press enter to stop playing");
								Console.ReadLine();
							}
							finally
							{
								player.StopPlaying();
							}
						}
						finally
						{
							player.CleanupPlayer();
						}
					}
					finally
					{
						outputAgent.Shutdown();
					}
				}
				finally
				{
					loader.Unload();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Program failed with exception: " + ex);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will find the output agent to use
		/// </summary>
		/********************************************************************/
		private static IOutputAgent FindOutputAgent(Manager agentManager)
		{
			AgentInfo agentInfo = agentManager.GetAgent(Manager.AgentType.Output, new Guid("b9cef7e4-c74c-4af0-b01d-802f0d1b4cc7"));
			if (agentInfo == null)
				return null;

			return (IOutputAgent)agentInfo.Agent.CreateInstance(agentInfo.TypeId);
		}
	}
}
