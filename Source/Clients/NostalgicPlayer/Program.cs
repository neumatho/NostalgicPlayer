/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Composition;
using Polycode.NostalgicPlayer.Library.Application;

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
				new ApplicationBuilder(Environment.GetCommandLineArgs())
					.ConfigureContainer(context =>
					{
						CompositionRoot.Register(context.Container);
					})
					.ConfigureInitialization(() =>
					{
						Application.SetHighDpiMode(HighDpiMode.DpiUnaware);
						Application.EnableVisualStyles();
						Application.SetCompatibleTextRenderingDefault(false);
					})
					.Build()
					.Run();
			}
			catch (Exception ex)
			{
				MessageBox.Show(string.Format(Resources.IDS_ERR_EXCEPTION, ex.Message), Resources.IDS_MAIN_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
				Application.Exit();
			}
		}
	}
}
