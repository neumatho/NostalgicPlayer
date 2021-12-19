/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic.ApplicationServices;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Utility;

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
		static void Main(string[] arguments)
		{
			try
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
				SingleInstanceApplication.Run(new MainWindowForm(), NewInstanceHandler);
			}
			catch (Exception ex)
			{
				MessageBox.Show(string.Format(Resources.IDS_ERR_EXCEPTION, ex.Message), Resources.IDS_MAIN_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
				Application.Exit();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called if an instance is already called when starting the
		/// program
		/// </summary>
		/********************************************************************/
		private static void NewInstanceHandler(object sender, StartupNextInstanceEventArgs e)
		{
			// Skip the first argument, which is the name of our application
			MainWindowForm.StartupHandler(e.CommandLine.Skip(1).ToArray());
		}
	}
}
