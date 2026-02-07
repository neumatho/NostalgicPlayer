/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Services;
using Polycode.NostalgicPlayer.Client.GuiPlayer.SplashScreen;
using Polycode.NostalgicPlayer.External.Composition;
using Polycode.NostalgicPlayer.Library.Application;
using Polycode.NostalgicPlayer.Logic.Composition;
using Polycode.NostalgicPlayer.Platform.Composition;
using SimpleInjector;
using SimpleInjector.Diagnostics;

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

			RegisterServices(container);
			RegisterWindows(container);

			container.RegisterSingleton<IApplicationHost, SingleInstanceApplication>();
		}



		/********************************************************************/
		/// <summary>
		/// Register all services into the container
		/// </summary>
		/********************************************************************/
		private static void RegisterServices(Container container)
		{
			container.RegisterSingleton<FormCreatorService>();

			container.RegisterSingleton<IProgressCallbackFactory, ProgressCallbackFactory>();
			container.RegisterSingleton<SplashScreenService>();
		}



		/********************************************************************/
		/// <summary>
		/// Register all the different windows into the container
		/// </summary>
		/********************************************************************/
		private static void RegisterWindows(Container container)
		{
			RegisterSingleWindow<SplashScreenForm>(container);
			RegisterSingleWindow<MainWindowForm>(container);
		}



		/********************************************************************/
		/// <summary>
		/// Register a single window into the container
		/// </summary>
		/********************************************************************/
		private static void RegisterSingleWindow<T>(Container container) where T : Form
		{
			// Register windows as transient - Forms manage their own disposal lifecycle
			// We need to tell SimpleInjector not to dispose them automatically
			Registration registration = Lifestyle.Transient.CreateRegistration<T>(container);
			registration.SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent, "Forms manage their own disposal");
			container.AddRegistration(typeof(T), registration);
		}
	}
}
