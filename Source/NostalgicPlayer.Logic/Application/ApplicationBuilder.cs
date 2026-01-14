/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Logic.Application
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
		public delegate void Initialization_Handler(IApplicationContext context);

		private event Configure_Handler configure;
		private event Initialization_Handler initialize;
		private IApplicationHost applicationHost;

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
		public ApplicationBuilder ConfigureHost(IApplicationHost host)
		{
			applicationHost = host;

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Build the application so it is ready to start
		/// </summary>
		/********************************************************************/
		public IApplicationHost Build()
		{
			if (applicationHost == null)
				throw new InvalidOperationException("Application host has not been defined");

			ApplicationContext context = new ApplicationContext();

			if (configure != null)
				configure(context);

			context.Container.Verify();

			if (initialize != null)
				initialize(context);

			return applicationHost;
		}
	}
}
