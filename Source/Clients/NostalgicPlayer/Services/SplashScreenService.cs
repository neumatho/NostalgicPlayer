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
	public class SplashScreenService
	{
		private readonly FormCreatorService _formCreatorService;
		private readonly IProgressCallbackFactory _progressCallbackFactory;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SplashScreenService(FormCreatorService formCreatorService, IProgressCallbackFactory progressCallbackFactory)
		{
			_formCreatorService = formCreatorService;
			_progressCallbackFactory = progressCallbackFactory;
		}



		/********************************************************************/
		/// <summary>
		/// Create the main form with splash screen showing progress
		/// </summary>
		/********************************************************************/
		public MainWindowForm CreateMainForm()
		{
			SplashScreenForm splash = _formCreatorService.GetFormInstance<SplashScreenForm>();
			splash.Show();
			splash.Update();

			// Set the splash screen as the current progress callback
			// MainWindowForm.InitializeForm() will get it via the factory
			_progressCallbackFactory.CurrentCallback = splash;

			// Create main form (heavy work happens here - InitializeForm will use the callback)
			MainWindowForm form = _formCreatorService.GetFormInstance<MainWindowForm>();

			// Ensure UI refresh before closing splash
			Application.DoEvents();
			splash.Close();
			splash.Dispose();

			// Clear the callback
			_progressCallbackFactory.CurrentCallback = null;

			return form;
		}
	}
}
