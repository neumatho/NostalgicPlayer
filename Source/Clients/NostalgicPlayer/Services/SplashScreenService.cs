/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Factories;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.MainWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.SplashScreen;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Services
{
	/// <summary>
	/// Service that handles the creation of the main form with splash screen
	/// </summary>
	public class SplashScreenService : ISplashScreenService
	{
		private readonly IFormCreatorService formCreatorService;
		private readonly IProgressCallbackFactory progressCallbackFactory;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SplashScreenService(IFormCreatorService formCreatorService, IProgressCallbackFactory progressCallbackFactory)
		{
			this.formCreatorService = formCreatorService;
			this.progressCallbackFactory = progressCallbackFactory;
		}



		/********************************************************************/
		/// <summary>
		/// Create the main form with splash screen showing progress
		/// </summary>
		/********************************************************************/
		public Form CreateMainForm()
		{
			SplashScreenForm splash = formCreatorService.GetFormInstance<SplashScreenForm>();
			splash.Show();
			splash.Update();

			// Set the splash screen as the current progress callback
			// MainWindowForm.InitializeForm() will get it via the factory
			progressCallbackFactory.CurrentCallback = splash;

			// Create main form (heavy work happens here - InitializeForm will use the callback)
			MainWindowForm form = formCreatorService.GetFormInstance<MainWindowForm>();

			// Ensure UI refresh before closing splash
			Application.DoEvents();
			splash.Close();
			splash.Dispose();

			// Clear the callback
			progressCallbackFactory.CurrentCallback = null;

			return form;
		}
	}
}
