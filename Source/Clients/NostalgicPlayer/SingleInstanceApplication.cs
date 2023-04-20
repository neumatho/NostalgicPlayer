/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Linq;
using Microsoft.VisualBasic.ApplicationServices;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer
{
	/// <summary>
	/// Will make sure only one instance of the player is running
	/// </summary>
	public class SingleInstanceApplication : WindowsFormsApplicationBase
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SingleInstanceApplication()
		{
			IsSingleInstance = true;

			StartupNextInstance += StartupNextInstanceHandler;
		}



		/********************************************************************/
		/// <summary>
		/// Is called if the main form needs to be created
		/// </summary>
		/********************************************************************/
		protected override void OnCreateMainForm()
		{
			MainForm = new MainWindowForm();

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
