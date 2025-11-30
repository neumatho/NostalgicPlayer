/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Polycode.NostalgicPlayer.Kit.Utility
{
	/// <summary>
	/// </summary>
	public delegate void RegisterServiceHandler(IServiceCollection services);

	/// <summary>
	/// Helper class to use dependency injection
	/// </summary>
	public static class DependencyInjection
	{
		private static IHost host;								// Root host (service provider owner)
		private static IServiceScope defaultScope;				// Default scope
		private static IServiceProvider serviceProvider;		// Root provider shortcut
		private static IServiceProvider defaultServiceProvider;	// Scope provider shortcut (backwards compatibility)

		/********************************************************************/
		/// <summary>
		/// Build all the services
		/// </summary>
		/********************************************************************/
		public static void Build(RegisterServiceHandler handler)
		{
			if (host != null)
				throw new Exception("Has already been initialized once");

			// Register and build all the services
			host = Host.CreateDefaultBuilder().ConfigureServices((_, services) => handler(services)).Build();
			serviceProvider = host.Services;

			// Create the default service scope
			defaultScope = serviceProvider.CreateScope();
			defaultServiceProvider = defaultScope.ServiceProvider;
		}



		/********************************************************************/
		/// <summary>
		/// Create a new scope holding registered objects
		/// </summary>
		/********************************************************************/
		public static IServiceProvider CreateScope()
		{
			if (serviceProvider == null)
				throw new Exception("Has not been built yet");

			return serviceProvider.CreateScope().ServiceProvider;
		}



		/********************************************************************/
		/// <summary>
		/// Return the default service provider
		/// </summary>
		/********************************************************************/
		public static IServiceProvider GetDefaultProvider()
		{
			if (defaultServiceProvider == null)
				throw new Exception("Has not been built yet");

			return defaultServiceProvider;
		}



		/********************************************************************/
		/// <summary>
		/// Dispose host and default scope so singletons get disposed.
		/// 
		/// Call this at application shutdown
		/// </summary>
		/********************************************************************/
		public static void Dispose()
		{
			// Ignore if not built
			if (host == null)
				return;

			// Dispose the default scope first (scoped + transient disposables)
			defaultScope?.Dispose();
			defaultScope = null;
			defaultServiceProvider = null;

			// Dispose root host (singleton disposables)
			host.Dispose();
			host = null;
			serviceProvider = null;
		}
	}
}
