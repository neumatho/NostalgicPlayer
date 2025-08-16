/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Library.Agent;
using Polycode.NostalgicPlayer.Library.Containers;
using Polycode.NostalgicPlayer.Library.Loaders;
using Polycode.NostalgicPlayer.Library.Players;

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
			if (args.Length != 1)
			{
				Console.WriteLine("Music player to play old-school Amiga and PC modules.");
				Console.WriteLine();
				Console.WriteLine("Syntax: NostalgicPlayerConsole <file> | <url>");
				return;
			}

			try
			{
				// Some of the agents have their own settings. We use dependency injection
				// to add an implementation that read these settings. You can implement your
				// own version by deriving from ISettings and register it
				DependencyInjection.Build(services =>
					{
						// We use the default NostalgicPlayer implementation,
						// which will read/write the settings in
						// %ProgramData%\Polycode\NostalgicPlayer folder
						//
						// It is important that the ISettings is added as transient
						services.AddTransient<ISettings, Settings>();
					}
				);

				// Load needed agents
				Manager agentManager = new Manager();
				agentManager.LoadAllAgents();

				LoaderBase loader;

				if (args[0].StartsWith("http://") || args[0].StartsWith("https://"))
					loader = LoadStream(args[0], agentManager);
				else
					loader = LoadFile(args[0], agentManager);

				if (loader == null)
					return;

				try
				{
					Play(agentManager, loader);
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
		/// Will load a file
		/// </summary>
		/********************************************************************/
		private static LoaderBase LoadFile(string fileName, Manager agentManager)
		{
			Loader loader = new Loader(agentManager);

			if (!loader.Load(fileName, out string errorMessage))
			{
				Console.WriteLine("Could not load the module. Failed with error:");
				Console.WriteLine(errorMessage);
				return null;
			}

			return loader;
		}



		/********************************************************************/
		/// <summary>
		/// Will load a stream
		/// </summary>
		/********************************************************************/
		private static LoaderBase LoadStream(string url, Manager agentManager)
		{
			StreamLoader streamLoader = new StreamLoader(agentManager);

			if (!streamLoader.Load(url, out string errorMessage))
			{
				Console.WriteLine("Could not start streaming. Failed with error:");
				Console.WriteLine(errorMessage);
				return null;
			}

			return streamLoader;
		}



		/********************************************************************/
		/// <summary>
		/// Will find the output agent to use
		/// </summary>
		/********************************************************************/
		private static IOutputAgent FindOutputAgent(Manager agentManager)
		{
			// The guid points to CoreAudio output agent
			AgentInfo agentInfo = agentManager.GetAgent(Manager.AgentType.Output, new Guid("b9cef7e4-c74c-4af0-b01d-802f0d1b4cc7"));
			if (agentInfo == null)
				return null;

			return (IOutputAgent)agentInfo.Agent.CreateInstance(agentInfo.TypeId);
		}



		/********************************************************************/
		/// <summary>
		/// Will start playing
		/// </summary>
		/********************************************************************/
		private static void Play(Manager agentManager, LoaderInfoBase loaderInfo)
		{
			string errorMessage;

			IPlayer player = loaderInfo.Player;

			string warningMessage = player.GetWarning(loaderInfo);
			if (!string.IsNullOrEmpty(warningMessage))
			{
				Console.WriteLine(warningMessage);
				Console.WriteLine();
			}

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
				if (!player.InitPlayer(new PlayerConfiguration(outputAgent, loaderInfo, SurroundMode.None, new MixerConfiguration
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
					{
						if (!modulePlayer.SelectSong(-1, out errorMessage))
						{
							Console.WriteLine("Cannot initialize player.");
							Console.WriteLine(errorMessage);
							return;
						}
					}

					if (!player.StartPlaying(loaderInfo, out errorMessage))
					{
						Console.WriteLine("Cannot start playing.");
						Console.WriteLine(errorMessage);
						return;
					}

					try
					{
						player.EndReached += (sender, e) =>
						{
							Console.WriteLine("End of module reached");
							Environment.Exit(0);
						};

						ModuleInfoStatic moduleInfoStatic = player.StaticModuleInformation;
						ModuleInfoFloating moduleInfoFloating = player.PlayingModuleInformation;
						string packedLength = moduleInfoStatic.CrunchedSize == -1 ? "unknown" : moduleInfoStatic.CrunchedSize.ToString("N0");

						Console.WriteLine("Playing module");
						Console.WriteLine();

						if (player is not IStreamingPlayer)
							Console.WriteLine("Module name/file name: " + (string.IsNullOrEmpty(moduleInfoStatic.Title) ? Path.GetFileName(loaderInfo.Source) : moduleInfoStatic.Title));

						Console.WriteLine("Author: " + (string.IsNullOrEmpty(moduleInfoStatic.Author) ? "Unknown" : moduleInfoStatic.Author));
						Console.WriteLine("Module format: " + moduleInfoStatic.Format);
						Console.WriteLine("Active player: " + moduleInfoStatic.PlayerName);
						Console.WriteLine("Used channels: " + moduleInfoStatic.Channels);
						Console.WriteLine("Total time: " + (moduleInfoFloating.DurationInfo == null ? "Unknown" : moduleInfoFloating.DurationInfo.TotalTime.ToString(@"m\:ss")));
						Console.WriteLine("Module size: " + moduleInfoStatic.ModuleSize + (moduleInfoStatic.CrunchedSize != 0 ? $@" (packed: {packedLength})" : string.Empty));

						if (player is IStreamingPlayer)
							Console.WriteLine("URL: " + loaderInfo.Source);
						else
							Console.WriteLine("File name: " + loaderInfo.Source);

						Console.WriteLine();

						// Output extra information
						if (moduleInfoFloating.ModuleInformation != null)
						{
							foreach (string info in moduleInfoFloating.ModuleInformation)
							{
								string[] parts = info.Split('\t');
								Console.WriteLine($"{parts[0]} {parts[1]}");
							}
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
	}
}
