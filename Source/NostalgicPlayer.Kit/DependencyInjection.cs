/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Polycode.NostalgicPlayer.Kit
{
	/// <summary>
	/// </summary>
	public delegate void RegisterServiceHandler(IServiceCollection services);

	/// <summary>
	/// Helper class to use dependency injection
	/// </summary>
	public static class DependencyInjection
	{
		private static IServiceProvider serviceProvider;			// Used to create new scopes
		private static IServiceProvider defaultServiceProvider;		// Is the default scope services

		/********************************************************************/
		/// <summary>
		/// Build all the services
		/// </summary>
		/********************************************************************/
		public static void Build(RegisterServiceHandler handler)
		{
			if (serviceProvider != null)
				throw new Exception("Has already been initialized once");

			// Register and build all the services
			IHost host = Host.CreateDefaultBuilder().ConfigureServices((_, services) => handler(services)).Build();
			serviceProvider = host.Services;

			// Create the default service provider, which is used by the agents
			defaultServiceProvider = CreateScope();
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
			return defaultServiceProvider;
		}
	}
}
