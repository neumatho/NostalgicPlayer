/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Library.Loaders;
using Polycode.NostalgicPlayer.Library.Loaders.FileDecrunchers;
using Polycode.NostalgicPlayer.Library.Loaders.FileLoaders;
using Polycode.NostalgicPlayer.Library.Players;
using Polycode.NostalgicPlayer.Library.Sound;
using Polycode.NostalgicPlayer.Library.Sound.Mixer;
using Polycode.NostalgicPlayer.Library.Sound.Resampler;
using SimpleInjector;

namespace Polycode.NostalgicPlayer.Library.Composition
{
	/// <summary>
	/// Register all classes/interfaces into the dependency injection container
	/// </summary>
	public static class CompositionRoot
	{
		/********************************************************************/
		/// <summary>
		/// Register all library specific classes into the container
		/// </summary>
		/********************************************************************/
		public static void RegisterLibrary(this Container container)
		{
			RegisterPlayers(container);
			RegisterLoaders(container);
			RegisterSoundStreams(container);
			RegisterSounds(container);
		}



		/********************************************************************/
		/// <summary>
		/// Register all players
		/// </summary>
		/********************************************************************/
		private static void RegisterPlayers(Container container)
		{
			container.RegisterSingleton<IPlayerFactory, PlayerFactory>();

			container.Register<IModulePlayer, ModulePlayer>();
			container.Register<ISamplePlayer, SamplePlayer>();
			container.Register<IStreamingPlayer, StreamingPlayer>();
		}



		/********************************************************************/
		/// <summary>
		/// Register all loaders
		/// </summary>
		/********************************************************************/
		private static void RegisterLoaders(Container container)
		{
			container.RegisterSingleton<ILoaderFactory, LoaderFactory>();
			container.RegisterSingleton<IFileLoaderFactory, FileLoaderFactory>();
			container.RegisterSingleton<IFileDecruncherFactory, FileDecruncherFactory>();
			container.RegisterSingleton<IArchiveDecruncherFactory, ArchiveDecruncherFactory>();

			container.Register<Loader>();
			container.Register<StreamLoader>();

			container.RegisterSingleton<IArchiveDetector, ArchiveDetector>();
		}



		/********************************************************************/
		/// <summary>
		/// Register all SoundStream implementations
		/// </summary>
		/********************************************************************/
		private static void RegisterSoundStreams(Container container)
		{
			container.RegisterSingleton<ISoundStreamFactory, SoundStreamFactory>();
		}



		/********************************************************************/
		/// <summary>
		/// Register all sound implementations
		/// </summary>
		/********************************************************************/
		private static void RegisterSounds(Container container)
		{
			container.RegisterSingleton<ISoundFactory, SoundFactory>();
			container.RegisterSingleton<IVisualizerFactory, VisualizerFactory>();

			container.Register<Mixer>();
			container.Register<Resampler>();

			container.Register<Visualizer>();
		}
	}
}
