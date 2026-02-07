/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
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

		private event Configure_Handler configure;
		private event Initialization_Handler initialize;

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
		/// Build the application so it is ready to start
		/// </summary>
		/********************************************************************/
		public IApplicationHost Build()
		{
			IApplicationContext context = InitializeApplicationContext();

			DependencyInjection.Container = context.Container;

			if (initialize != null)
				initialize();

			// Verify container after initialization to ensure Forms are not created
			// before SetCompatibleTextRenderingDefault is called
			VerifyContainer(context);

			return context.Container.GetInstance<IApplicationHost>();
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Setup the application context
		/// </summary>
		/********************************************************************/
		private IApplicationContext InitializeApplicationContext()
		{
			ApplicationContext context = new ApplicationContext(arguments);
			context.Container.RegisterInstance<IApplicationContext>(context);

			context.Container.RegisterKit();

			if (configure != null)
				configure(context);

			return context;
		}



		/********************************************************************/
		/// <summary>
		/// Verify the container after initialization is complete
		/// </summary>
		/********************************************************************/
		private void VerifyContainer(IApplicationContext context)
		{
			context.Container.Verify();
		}
		#endregion
	}
}
