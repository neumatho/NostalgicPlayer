/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.SplashScreen;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Library.Application;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer
{
	/// <summary>
	/// Will make sure only one instance of the player is running
	/// </summary>
	public class SingleInstanceApplication : WindowsFormsApplicationBase, IApplicationHost
	{
		private readonly IApplicationContext context;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SingleInstanceApplication(IApplicationContext appContext)
		{
			context = appContext;

			IsSingleInstance = true;

			StartupNextInstance += StartupNextInstanceHandler;
		}



		/********************************************************************/
		/// <summary>
		/// Start the application
		/// </summary>
		/********************************************************************/
		public void Run()
		{
			base.Run(context.Arguments);
		}



		/********************************************************************/
		/// <summary>
		/// Is called if the main form needs to be created
		/// </summary>
		/********************************************************************/
		protected override void OnCreateMainForm()
		{
			SplashScreenForm splash = new SplashScreenForm();
			splash.Show();
			splash.Update();

			// Create main form (heavy work happens here)
			MainWindowForm form = new MainWindowForm();
			MainForm = form;
			form.InitializeForm(splash.UpdateProgress);

			// Ensure UI refresh before closing splash
			Application.DoEvents();
			splash.Close();
			splash.Dispose();

			base.OnCreateMainForm();
		}



		/********************************************************************/
		/// <summary>
		/// Is called if an instance is already called when starting the
		/// program
		/// </summary>
		/********************************************************************/
		private void StartupNextInstanceHandler(object sender, StartupNextInstanceEventArgs e)
		{
			// Skip the first argument, which is the name of our application
			((MainWindowForm)MainForm).StartupHandler(e.CommandLine.Skip(1).ToArray());
		}
	}
}
