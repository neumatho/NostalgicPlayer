/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Composition;
using Polycode.NostalgicPlayer.Kit.Helpers;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Logic.Application;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer
{
	/// <summary>
	/// Main entry point
	/// </summary>
	static class Program
	{
		/********************************************************************/
		/// <summary>
		/// The main entry point for the application
		/// </summary>
		/********************************************************************/
		[STAThread]
		static void Main()
		{
			try
			{
				new ApplicationBuilder()
					.ConfigureContainer(context =>
					{
						CompositionRoot.Register(context.Container);
					})
					.ConfigureInitialization(context =>
					{
						// Some of the agents have their own settings. We use dependency injection
						// to add an implementation that read these settings
						DependencyInjection.Build(services =>
							{
								// We use the default NostalgicPlayer implementation
								services.AddTransient<ISettings, Settings>();
							}
						);

						Application.SetHighDpiMode(HighDpiMode.DpiUnaware);
						Application.EnableVisualStyles();
						Application.SetCompatibleTextRenderingDefault(false);
					})
					.ConfigureHost(new SingleInstanceApplication())
					.Build()
					.Run(Environment.GetCommandLineArgs());
			}
			catch (Exception ex)
			{
				MessageBox.Show(string.Format(Resources.IDS_ERR_EXCEPTION, ex.Message), Resources.IDS_MAIN_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
				Application.Exit();
			}
		}
	}
}
