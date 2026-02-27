/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Factories;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Services;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.MainWindow;
using Polycode.NostalgicPlayer.External.Composition;
using Polycode.NostalgicPlayer.Library.Application;
using Polycode.NostalgicPlayer.Logic.Composition;
using Polycode.NostalgicPlayer.Platform.Composition;
using SimpleInjector;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Composition
{
	/// <summary>
	/// Register all classes/interfaces into the dependency injection container
	/// </summary>
	public static class CompositionRoot
	{
		/********************************************************************/
		/// <summary>
		/// Register all client specific classes into the container
		/// </summary>
		/********************************************************************/
		public static void Register(Container container)
		{
			container.RegisterLogic();
			container.RegisterExternal();
			container.RegisterPlatform();

			RegisterSettings(container);
			RegisterServices(container);
			RegisterAdapters(container);

			container.RegisterSingleton<IApplicationHost, SingleInstanceApplication>();
		}



		/********************************************************************/
		/// <summary>
		/// Register all the different settings into the container
		/// </summary>
		/********************************************************************/
		private static void RegisterSettings(Container container)
		{
			container.RegisterSingleton<SettingsService>();

			container.RegisterSingleton<ModuleSettings>();
			container.RegisterSingleton<OptionSettings>();
			container.RegisterSingleton<PathSettings>();
			container.RegisterSingleton<SoundSettings>();
		}



		/********************************************************************/
		/// <summary>
		/// Register all services into the container
		/// </summary>
		/********************************************************************/
		private static void RegisterServices(Container container)
		{
			container.RegisterSingleton<IProgressCallbackFactory, ProgressCallbackFactory>();
			container.RegisterSingleton<SplashScreenService>();

			container.RegisterSingleton<FormCreatorService>();
			container.RegisterSingleton<FileScannerService>();
			container.RegisterSingleton<ModuleHandlerService>();
		}



		/********************************************************************/
		/// <summary>
		/// Register all adapters into the container
		/// </summary>
		/********************************************************************/
		private static void RegisterAdapters(Container container)
		{
			container.RegisterSingleton<IMainWindowApi, MainWindowApiAdapter>();
			container.RegisterSingleton<MainWindowApiAdapter>();
		}
	}
}
