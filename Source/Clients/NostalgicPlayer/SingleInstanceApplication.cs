/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;

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
		private SingleInstanceApplication()
		{
			IsSingleInstance = true;
		}



		/********************************************************************/
		/// <summary>
		/// Starts the application
		/// </summary>
		/********************************************************************/
		public static void Run(Form form, StartupNextInstanceEventHandler startupHandler)
		{
			SingleInstanceApplication app = new SingleInstanceApplication();
			app.MainForm = form;
			app.StartupNextInstance += startupHandler;

			app.Run(Environment.GetCommandLineArgs());
		}
	}
}
