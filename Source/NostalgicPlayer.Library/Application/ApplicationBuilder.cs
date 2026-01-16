/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Composition;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Library.Application
{
	/// <summary>
	/// Use this to configure and start your application
	/// </summary>
	public class ApplicationBuilder
	{
		/// <summary>
		/// 
		/// </summary>
		public delegate void Configure_Handler(IApplicationContext context);

		/// <summary>
		/// 
		/// </summary>
		public delegate void Initialization_Handler();

		/// <summary>
		/// 
		/// </summary>
		public delegate IApplicationHost SetupHost_Handler(IApplicationContext context);

		private event Configure_Handler configure;
		private event Initialization_Handler initialize;
		private event SetupHost_Handler setupHost;

		private readonly string[] arguments;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ApplicationBuilder(string[] args)
		{
			arguments = args;
		}



		/********************************************************************/
		/// <summary>
		/// Register handler to do the container configuration
		/// </summary>
		/********************************************************************/
		public ApplicationBuilder ConfigureContainer(Configure_Handler handler)
		{
			configure += handler;

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Register handler to do the application initialization
		/// </summary>
		/********************************************************************/
		public ApplicationBuilder ConfigureInitialization(Initialization_Handler handler)
		{
			initialize += handler;

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Register host handler to run the application
		/// </summary>
		/********************************************************************/
		public ApplicationBuilder ConfigureHost(SetupHost_Handler handler)
		{
			if (setupHost != null)
				throw new InvalidOperationException("Host handler has already been setup");

			setupHost += handler;

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Build the application so it is ready to start
		/// </summary>
		/********************************************************************/
		public IApplicationHost Build()
		{
			if (setupHost == null)
				throw new InvalidOperationException("Application host has not been defined");

			ApplicationContext context = new ApplicationContext(arguments);

			context.Container.RegisterKit();

			if (configure != null)
				configure(context);

			context.Container.Verify();

			DependencyInjection.Container = context.Container;

			if (initialize != null)
				initialize();

			return setupHost(context);
		}
	}
}
