﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Polycode.NostalgicPlayer.Controls.Theme;
using Polycode.NostalgicPlayer.Kit.Helpers;
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
		static void Main()
		{
			try
			{
				// Some of the agents have their own settings. We use dependency injection
				// to add an implementation that read these settings
				DependencyInjection.Build(services =>
					{
						// We use the default NostalgicPlayer implementation
						services.AddTransient<ISettings, Settings>();
						services.RegisterThemeManager();
					}
				);

				Application.SetHighDpiMode(HighDpiMode.DpiUnaware);
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);

				SingleInstanceApplication singleInstanceApplication = new SingleInstanceApplication();
				singleInstanceApplication.Run(Environment.GetCommandLineArgs());
			}
			catch (Exception ex)
			{
				MessageBox.Show(string.Format(Resources.IDS_ERR_EXCEPTION, ex.Message), Resources.IDS_MAIN_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
				Application.Exit();
			}
			finally
			{
				// Ensure DI root + singletons disposed
				DependencyInjection.Dispose();
			}
		}
	}
}
