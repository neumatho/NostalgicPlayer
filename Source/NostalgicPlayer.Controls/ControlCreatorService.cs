/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Controls.Forms;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls
{
	/// <summary>
	/// Helper to instance Control objects
	/// </summary>
	internal class ControlCreatorService : IControlCreatorService
	{
		private readonly IControlInitializerService controlInitializerService;
		private readonly IThemeManager themeManager;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ControlCreatorService(IControlInitializerService controlInitializerService, IThemeManager themeManager)
		{
			this.controlInitializerService = controlInitializerService;
			this.themeManager = themeManager;
		}



		/********************************************************************/
		/// <summary>
		/// Create a new control instance. Will call the InitializeControl
		/// method if exists with dependency injections and setup themes
		/// </summary>
		/********************************************************************/
		public T GetInstance<T>(params object[] extraArguments) where T : Control, new()
		{
			T control = new T();

			controlInitializerService.InitializeSingleControl(control, extraArguments);
			themeManager.SetThemeOnControl(control);

			return control;
		}
	}
}
